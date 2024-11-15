import socket
import time
import struct
import math
import random
from dataclasses import dataclass
import threading

@dataclass
class DroneState:
    position: list[float]
    attitude: list[float]
    gyro: list[float]
    inputs: list[float]
    battery: list[float]
    motor_rpms: list[float]

class LiftoffSimulator:
    def __init__(self, target_ip='127.0.0.1', target_port=9001):
        self.socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        self.target = (target_ip, target_port)
        self.start_time = time.time()
        self.running = False
        self.drone_state = DroneState(
            position=[0.0, 0.0, 0.0],
            attitude=[0.0, 0.0, 0.0, 1.0],  # Quaternion (x,y,z,w)
            gyro=[0.0, 0.0, 0.0],
            inputs=[0.0, 0.0, 0.0, 0.0],
            battery=[12.6, 100.0],
            motor_rpms=[0.0, 0.0, 0.0, 0.0]
        )

    def update_simulation(self):
        t = time.time() - self.start_time
        
        # Simulate figure-8 pattern flight
        scale = 5.0
        self.drone_state.position[0] = scale * math.sin(t * 0.5)  # X
        self.drone_state.position[1] = scale * math.sin(t * 0.25) * math.cos(t * 0.5)  # Y
        self.drone_state.position[2] = -2.0 - math.sin(t * 0.1)  # Z with slight altitude change
        
        # Update attitude (quaternion)
        roll = 0.1 * math.sin(t * 0.5)
        pitch = 0.1 * math.cos(t * 0.25)
        yaw = 0.1 * math.sin(t * 0.25)
        
        # Convert Euler angles to quaternion (simplified)
        self.drone_state.attitude[0] = roll
        self.drone_state.attitude[1] = pitch
        self.drone_state.attitude[2] = yaw
        self.drone_state.attitude[3] = math.sqrt(1 - sum(x*x for x in self.drone_state.attitude[:3]))
        
        # Update gyro rates (degrees/second)
        self.drone_state.gyro = [
            45.0 * math.cos(t * 0.5),    # Pitch
            45.0 * math.sin(t * 0.25),   # Roll
            30.0 * math.cos(t * 0.25)    # Yaw
        ]
        
        # Update control inputs
        self.drone_state.inputs = [
            0.7 + 0.3 * math.sin(t * 0.5),  # Throttle
            0.5 * math.sin(t * 0.25),       # Yaw
            0.5 * math.cos(t * 0.5),        # Pitch
            0.5 * math.sin(t * 0.5)         # Roll
        ]
        
        # Update battery
        self.drone_state.battery[0] = max(10.0, 12.6 - (t * 0.01))  # Voltage
        self.drone_state.battery[1] = max(0.0, 100.0 - (t * 0.5))   # Percentage
        
        # Update motor RPMs
        base_rpm = 4000.0
        self.drone_state.motor_rpms = [
            base_rpm + 1000.0 * math.sin(t * 0.5),
            base_rpm + 1000.0 * math.cos(t * 0.5),
            base_rpm + 1000.0 * math.sin(t * 0.5),
            base_rpm + 1000.0 * math.cos(t * 0.5)
        ]

    def pack_telemetry(self):
        # Pack all data according to the specified format
        return struct.pack(
            '!f'    # Timestamp
            'fff'   # Position (x, y, z)
            'ffff'  # Attitude (quaternion)
            'fff'   # Gyro rates
            'ffff'  # Inputs
            'ff'    # Battery
            'B'     # Number of motors
            'ffff', # Motor RPMs
            time.time() - self.start_time,
            *self.drone_state.position,
            *self.drone_state.attitude,
            *self.drone_state.gyro,
            *self.drone_state.inputs,
            *self.drone_state.battery,
            4,  # Number of motors (quad)
            *self.drone_state.motor_rpms
        )

    def run(self):
        self.running = True
        while self.running:
            self.update_simulation()
            telemetry_data = self.pack_telemetry()
            self.socket.sendto(telemetry_data, self.target)
            time.sleep(1/60)  # 60Hz update rate

    def start(self):
        self.simulation_thread = threading.Thread(target=self.run)
        self.simulation_thread.start()

    def stop(self):
        self.running = False
        self.simulation_thread.join()

if __name__ == "__main__":
    simulator = LiftoffSimulator()
    try:
        simulator.start()
        print("Simulator running. Press Ctrl+C to stop...")
        while True:
            time.sleep(1)
    except KeyboardInterrupt:
        print("\nStopping simulator...")
        simulator.stop()