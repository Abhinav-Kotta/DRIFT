import socket
import struct
import json
from dataclasses import dataclass
from typing import List, Dict, Any, Tuple

@dataclass
class TelemetryData:
    timestamp: float = 0.0
    position: Tuple[float, float, float] = (0.0, 0.0, 0.0)
    attitude: Tuple[float, float, float, float] = (0.0, 0.0, 0.0, 1.0)
    velocity: Tuple[float, float, float] = (0.0, 0.0, 0.0)
    gyro: Tuple[float, float, float] = (0.0, 0.0, 0.0)
    input: Tuple[float, float, float, float] = (0.0, 0.0, 0.0, 0.0)
    battery: Tuple[float, float] = (0.0, 0.0)
    motor_rpm: List[float] = None

    def to_json(self) -> str:
        data = {
            "timestamp": self.timestamp,
            "position": {
                "x": self.position[0],
                "y": self.position[1],
                "z": self.position[2]
            },
            "attitude": {
                "x": self.attitude[0],
                "y": self.attitude[1],
                "z": self.attitude[2],
                "w": self.attitude[3]
            },
            "velocity": {
                "x": self.velocity[0],
                "y": self.velocity[1],
                "z": self.velocity[2]
            },
            "gyro": {
                "pitch": self.gyro[0],
                "roll": self.gyro[1],
                "yaw": self.gyro[2]
            },
            "input": {
                "throttle": self.input[0],
                "yaw": self.input[1],
                "pitch": self.input[2],
                "roll": self.input[3]
            },
            "battery": {
                "voltage": self.battery[0],
                "percentage": self.battery[1]
            },
            "motor_rpm": self.motor_rpm
        }
        return json.dumps(data)

class LiftoffParser:
    def __init__(self, stream_format: List[str]):
        """Initialize parser with stream format configuration."""
        self.stream_format = stream_format
        self._format_sizes = {
            "Timestamp": (4, "f"),           # 1 float
            "Position": (12, "fff"),         # 3 floats
            "Attitude": (16, "ffff"),        # 4 floats
            "Velocity": (12, "fff"),         # 3 floats
            "Gyro": (12, "fff"),            # 3 floats
            "Input": (16, "ffff"),          # 4 floats
            "Battery": (8, "ff"),           # 2 floats
            "MotorRPM": None                # Variable size (1 byte + N floats)
        }

    def _parse_motor_rpm(self, data: bytes, offset: int) -> Tuple[List[float], int]:
        """Parse motor RPM data with variable length."""
        motor_count = data[offset]
        format_str = f"{'f' * motor_count}"
        size = 1 + (4 * motor_count)  # 1 byte count + 4 bytes per float
        values = list(struct.unpack(format_str, data[offset+1:offset+size]))
        return values, size

    def parse_packet(self, data: bytes) -> TelemetryData:
        """Parse a UDP packet according to stream format."""
        telemetry = TelemetryData()
        offset = 0

        for format_type in self.stream_format:
            if format_type == "MotorRPM":
                rpm_values, size = self._parse_motor_rpm(data, offset)
                telemetry.motor_rpm = rpm_values
                offset += size
                continue

            size, format_str = self._format_sizes[format_type]
            values = struct.unpack(format_str, data[offset:offset+size])
            
            # Assign values to appropriate fields
            if format_type == "Timestamp":
                telemetry.timestamp = values[0]
            elif format_type == "Position":
                telemetry.position = values
            elif format_type == "Attitude":
                telemetry.attitude = values
            elif format_type == "Velocity":
                telemetry.velocity = values
            elif format_type == "Gyro":
                telemetry.gyro = values
            elif format_type == "Input":
                telemetry.input = values
            elif format_type == "Battery":
                telemetry.battery = values

            offset += size

        return telemetry

def create_udp_server(host: str, port: int, stream_format: List[str]) -> Tuple[socket.socket, LiftoffParser]:
    """Create and configure UDP server with parser."""
    server = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    server.bind((host, port))
    parser = LiftoffParser(stream_format)
    return server, parser
