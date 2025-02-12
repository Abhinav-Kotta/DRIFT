import socket
import struct
import time
import math
import psycopg2
from dotenv import load_dotenv
import os
from datetime import datetime, timezone
from dataclasses import dataclass
from typing import List, Tuple

# Load environment variables
load_dotenv()

@dataclass
class DroneState:
    timestamp: float = 0.0
    position: Tuple[float, float, float] = (0.0, 0.0, 0.0)
    attitude: Tuple[float, float, float, float] = (0.0, 0.0, 0.0, 1.0)
    velocity: Tuple[float, float, float] = (0.0, 0.0, 0.0)
    gyro: Tuple[float, float, float] = (0.0, 0.0, 0.0)
    inputs: Tuple[float, float, float, float] = (0.0, 0.0, 0.0, 0.0)
    battery: Tuple[float, float] = (16.8, 100.0)
    motor_rpms: List[float] = None

    def __post_init__(self):
        if self.motor_rpms is None:
            self.motor_rpms = [0.0] * 4

class DroneSimulator:
    def __init__(self, ip="127.0.0.1", port=9001):
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        self.address = (ip, port)
        self.state = DroneState()
        self.start_time = time.time()
        
        # Database connection
        self.db_connection = psycopg2.connect(
            f"postgres://{os.getenv('DB_USER')}:{os.getenv('DB_PASSWORD')}@"
            f"{os.getenv('DB_HOST')}:{os.getenv('DB_PORT')}/{os.getenv('DB_NAME')}?sslmode=require"
        )
        
    def update_state(self):
        """Update drone state with simulated movement"""
        current_time = time.time() - self.start_time
        self.state.timestamp = current_time
        
        # Simulate a figure-8 pattern flight
        scale = 5.0
        frequency = 0.2
        
        # Position
        self.state.position = (
            scale * math.sin(2 * math.pi * frequency * current_time),
            2.0 + math.sin(2 * math.pi * frequency * 0.5 * current_time),
            scale * math.sin(4 * math.pi * frequency * current_time)
        )
        
        # Attitude (quaternion)
        yaw = math.pi * 0.25 * math.sin(2 * math.pi * frequency * current_time)
        pitch = math.pi * 0.15 * math.cos(2 * math.pi * frequency * current_time)
        roll = math.pi * 0.1 * math.sin(2 * math.pi * frequency * 2 * current_time)
        
        cy = math.cos(yaw * 0.5)
        sy = math.sin(yaw * 0.5)
        cp = math.cos(pitch * 0.5)
        sp = math.sin(pitch * 0.5)
        cr = math.cos(roll * 0.5)
        sr = math.sin(roll * 0.5)
        
        self.state.attitude = (
            sr * cp * cy - cr * sp * sy,
            cr * sp * cy + sr * cp * sy,
            cr * cp * sy - sr * sp * cy,
            cr * cp * cy + sr * sp * sy
        )
        
        # Velocity
        self.state.velocity = (
            scale * 2 * math.pi * frequency * math.cos(2 * math.pi * frequency * current_time),
            math.pi * frequency * math.cos(2 * math.pi * frequency * 0.5 * current_time),
            scale * 4 * math.pi * frequency * math.cos(4 * math.pi * frequency * current_time)
        )
        
        # Gyro (degrees/second)
        self.state.gyro = (
            math.degrees(pitch * 2 * math.pi * frequency * math.sin(2 * math.pi * frequency * current_time)),
            math.degrees(roll * 4 * math.pi * frequency * math.cos(2 * math.pi * frequency * 2 * current_time)),
            math.degrees(yaw * 2 * math.pi * frequency * math.cos(2 * math.pi * frequency * current_time))
        )
        
        # Inputs
        self.state.inputs = (
            0.5 + 0.2 * math.sin(2 * math.pi * frequency * current_time),
            0.2 * math.sin(2 * math.pi * frequency * current_time),
            0.15 * math.cos(2 * math.pi * frequency * current_time),
            0.1 * math.sin(4 * math.pi * frequency * current_time)
        )
        
        # Battery
        battery_drain_rate = 0.001
        initial_voltage = 16.8
        min_voltage = 14.4
        voltage = max(min_voltage, initial_voltage - battery_drain_rate * current_time)
        percentage = (voltage - min_voltage) / (initial_voltage - min_voltage) * 100
        self.state.battery = (voltage, percentage)
        
        # Motor RPMs
        base_rpm = 15000
        self.state.motor_rpms = [
            base_rpm + 1000 * math.sin(2 * math.pi * frequency * current_time),
            base_rpm + 1000 * math.sin(2 * math.pi * frequency * current_time + math.pi/2),
            base_rpm + 1000 * math.sin(2 * math.pi * frequency * current_time + math.pi),
            base_rpm + 1000 * math.sin(2 * math.pi * frequency * current_time + 3*math.pi/2)
        ]

    def save_to_database(self):
        """Save current state to TimescaleDB"""
        cursor = self.db_connection.cursor()
        
        cursor.execute("""
            INSERT INTO drone_telemetry (
                time, timestamp, 
                position_x, position_y, position_z,
                attitude_x, attitude_y, attitude_z, attitude_w,
                velocity_x, velocity_y, velocity_z,
                gyro_pitch, gyro_roll, gyro_yaw,
                input_throttle, input_yaw, input_pitch, input_roll,
                battery_voltage, battery_percentage,
                motor_rpm_1, motor_rpm_2, motor_rpm_3, motor_rpm_4
            ) VALUES (
                %s, %s, 
                %s, %s, %s,
                %s, %s, %s, %s,
                %s, %s, %s,
                %s, %s, %s,
                %s, %s, %s, %s,
                %s, %s,
                %s, %s, %s, %s
            )
        """, (
            datetime.now(timezone.utc),
            self.state.timestamp,
            *self.state.position,
            *self.state.attitude,
            *self.state.velocity,
            *self.state.gyro,
            *self.state.inputs,
            *self.state.battery,
            *self.state.motor_rpms
        ))
        
        self.db_connection.commit()
        cursor.close()

    def pack_data(self) -> bytes:
        """Pack drone state into binary format"""
        format_str = "f"  # Timestamp
        format_str += "fff"  # Position
        format_str += "ffff"  # Attitude
        format_str += "fff"  # Velocity
        format_str += "fff"  # Gyro
        format_str += "ffff"  # Inputs
        format_str += "ff"    # Battery
        format_str += "B"     # Motor count
        format_str += "f" * len(self.state.motor_rpms)
        
        return struct.pack(
            format_str,
            self.state.timestamp,
            *self.state.position,
            *self.state.attitude,
            *self.state.velocity,
            *self.state.gyro,
            *self.state.inputs,
            *self.state.battery,
            len(self.state.motor_rpms),
            *self.state.motor_rpms
        )

    def send_telemetry(self):
        """Update state, send UDP packet, and save to database"""
        self.update_state()
        packet = self.pack_data()
        self.sock.sendto(packet, self.address)
        self.save_to_database()

    def close(self):
        """Close database connection"""
        self.db_connection.close()

def main():
    # First create the table if it doesn't exist
    
    # Start the simulator
    simulator = DroneSimulator()
    print(f"Starting drone telemetry simulation...")
    print(f"Sending UDP packets to {simulator.address[0]}:{simulator.address[1]}")
    print(f"Saving data to TimescaleDB")
    print(f"Press Ctrl+C to stop")
    
    try:
        while True:
            simulator.send_telemetry()
            time.sleep(1/50)  # 50Hz update rate
    except KeyboardInterrupt:
        print("\nSimulation stopped")
        simulator.close()

if __name__ == "__main__":
    main()