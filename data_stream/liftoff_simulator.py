import socket
import struct
import time
import math
from dataclasses import dataclass
from typing import List, Tuple

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
    def __init__(self, ip="127.0.0.1", port=27016):  # Using first port from server config
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        self.address = (ip, port)
        self.state = DroneState()
        self.start_time = time.time()

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
        
        # Send only position data since that's what the server expects
        return struct.pack('fff', *self.state.position)

    def send_telemetry(self):
        """Update state and send UDP packet"""
        packet = self.update_state()
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
