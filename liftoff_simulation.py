import socket
import struct
import time
import math
import numpy as np
from dataclasses import dataclass
from typing import List, Tuple

@dataclass
class DroneState:
    timestamp: float = 0.0
    position: Tuple[float, float, float] = (0.0, 0.0, 0.0)
    attitude: Tuple[float, float, float, float] = (0.0, 0.0, 0.0, 1.0)  # quaternion
    velocity: Tuple[float, float, float] = (0.0, 0.0, 0.0)
    gyro: Tuple[float, float, float] = (0.0, 0.0, 0.0)
    inputs: Tuple[float, float, float, float] = (0.0, 0.0, 0.0, 0.0)
    battery: Tuple[float, float] = (16.8, 100.0)  # voltage, percentage
    motor_rpms: List[float] = None

    def __post_init__(self):
        if self.motor_rpms is None:
            self.motor_rpms = [0.0] * 4  # 4 motors for a quadcopter

class DroneSimulator:
    def __init__(self, ip="127.0.0.1", port=9001):
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        self.address = (ip, port)
        self.state = DroneState()
        self.start_time = time.time()
        
    def update_state(self):
        """Update drone state with some simple simulated movement"""
        current_time = time.time() - self.start_time
        self.state.timestamp = current_time
        
        # Simulate a figure-8 pattern flight
        scale = 5.0  # meters
        frequency = 0.2  # Hz
        
        # Position
        self.state.position = (
            scale * math.sin(2 * math.pi * frequency * current_time),  # X
            2.0 + math.sin(2 * math.pi * frequency * 0.5 * current_time),  # Y (altitude)
            scale * math.sin(4 * math.pi * frequency * current_time),  # Z
        )
        
        # Attitude (quaternion)
        yaw = math.pi * 0.25 * math.sin(2 * math.pi * frequency * current_time)
        pitch = math.pi * 0.15 * math.cos(2 * math.pi * frequency * current_time)
        roll = math.pi * 0.1 * math.sin(2 * math.pi * frequency * 2 * current_time)
        
        # Convert Euler angles to quaternion
        cy = math.cos(yaw * 0.5)
        sy = math.sin(yaw * 0.5)
        cp = math.cos(pitch * 0.5)
        sp = math.sin(pitch * 0.5)
        cr = math.cos(roll * 0.5)
        sr = math.sin(roll * 0.5)
        
        self.state.attitude = (
            sr * cp * cy - cr * sp * sy,  # x
            cr * sp * cy + sr * cp * sy,  # y
            cr * cp * sy - sr * sp * cy,  # z
            cr * cp * cy + sr * sp * sy   # w
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
        
        # Simulate inputs
        self.state.inputs = (
            0.5 + 0.2 * math.sin(2 * math.pi * frequency * current_time),  # throttle
            0.2 * math.sin(2 * math.pi * frequency * current_time),        # yaw
            0.15 * math.cos(2 * math.pi * frequency * current_time),       # pitch
            0.1 * math.sin(4 * math.pi * frequency * current_time)         # roll
        )
        
        # Simulate battery drain
        battery_drain_rate = 0.001  # voltage per second
        initial_voltage = 16.8
        min_voltage = 14.4
        voltage = max(min_voltage, initial_voltage - battery_drain_rate * current_time)
        percentage = (voltage - min_voltage) / (initial_voltage - min_voltage) * 100
        self.state.battery = (voltage, percentage)
        
        # Simulate motor RPMs
        base_rpm = 15000
        self.state.motor_rpms = [
            base_rpm + 1000 * math.sin(2 * math.pi * frequency * current_time),
            base_rpm + 1000 * math.sin(2 * math.pi * frequency * current_time + math.pi/2),
            base_rpm + 1000 * math.sin(2 * math.pi * frequency * current_time + math.pi),
            base_rpm + 1000 * math.sin(2 * math.pi * frequency * current_time + 3*math.pi/2)
        ]

    def pack_data(self) -> bytes:
        """Pack drone state into binary format following Liftoff's protocol"""
        # Format: Timestamp, Position(XYZ), Attitude(XYZW), Velocity(XYZ), 
        # Gyro(Pitch,Roll,Yaw), Input(Throttle,Yaw,Pitch,Roll), 
        # Battery(Voltage,Percentage), MotorRPM(count + RPMs)
        
        format_str = "f" # Timestamp
        format_str += "fff"  # Position
        format_str += "ffff"  # Attitude quaternion
        format_str += "fff"  # Velocity
        format_str += "fff"  # Gyro
        format_str += "ffff"  # Inputs
        format_str += "ff"    # Battery
        format_str += "B"     # Motor count
        format_str += "f" * len(self.state.motor_rpms)  # Motor RPMs
        
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
        """Update drone state and send telemetry packet"""
        self.update_state()
        packet = self.pack_data()
        self.sock.sendto(packet, self.address)

def main():
    simulator = DroneSimulator()
    print(f"Starting drone telemetry simulation...")
    print(f"Sending UDP packets to {simulator.address[0]}:{simulator.address[1]}")
    print(f"Press Ctrl+C to stop")
    
    try:
        while True:
            simulator.send_telemetry()
            time.sleep(1/50)  # 50Hz update rate
    except KeyboardInterrupt:
        print("\nSimulation stopped")

if __name__ == "__main__":
    main()