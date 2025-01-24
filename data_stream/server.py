import socket
import secrets
import json
import struct

# Create a UDP socket
udp_server = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
salt = secrets.token_hex(8)

# Bind the socket to an IP address and port
server_address = ('0.0.0.0', 27015)
udp_server.bind(server_address)

print('UDP Server is listening at {}'.format(server_address))

# Define the StreamFormat
stream_format = [
    "ID",
    "Timestamp",
    "Position",
    "Attitude",
    "Velocity",
    "Gyro",
    "Input",
    "Battery",
    "MotorRPM"
]

# Helper function to decode telemetry data
def decode_telemetry(data, identifier):
    try:
        offset = 0
        decoded_data = {}

        decoded_data["ID"] = identifier

        # Unpack Timestamp (1 float)
        timestamp = struct.unpack_from('f', data, offset)[0]
        decoded_data["Timestamp"] = timestamp
        offset += 4

        # Unpack Position (3 floats)
        position = struct.unpack_from('fff', data, offset)
        decoded_data["Position"] = {"X": position[0], "Y": position[1], "Z": position[2]}
        offset += 12

        # Unpack Attitude (4 floats)
        attitude = struct.unpack_from('ffff', data, offset)
        decoded_data["Attitude"] = {
            "X": attitude[0], "Y": attitude[1], "Z": attitude[2], "W": attitude[3]
        }
        offset += 16

        # Unpack Velocity (3 floats)
        velocity = struct.unpack_from('fff', data, offset)
        decoded_data["Velocity"] = {"X": velocity[0], "Y": velocity[1], "Z": velocity[2]}
        offset += 12

        # Unpack Gyro (3 floats)
        gyro = struct.unpack_from('fff', data, offset)
        decoded_data["Gyro"] = {"Pitch": gyro[0], "Roll": gyro[1], "Yaw": gyro[2]}
        offset += 12

        # Unpack Input (4 floats)
        drone_input = struct.unpack_from('ffff', data, offset)
        decoded_data["Input"] = {
            "Throttle": drone_input[0], "Yaw": drone_input[1], 
            "Pitch": drone_input[2], "Roll": drone_input[3]
        }
        offset += 16

        # Unpack Battery (2 floats)
        battery = struct.unpack_from('ff', data, offset)
        decoded_data["Battery"] = {
            "Voltage": battery[0], "Percentage": battery[1]
        }
        offset += 8

        # Unpack MotorRPM (1 byte + floats)
        num_motors = struct.unpack_from('B', data, offset)[0]
        offset += 1
        motor_rpms = struct.unpack_from(f'{num_motors}f', data, offset)
        decoded_data["MotorRPM"] = {
            f"Motor{i+1}": rpm for i, rpm in enumerate(motor_rpms)
        }

        return decoded_data
    except Exception as e:
        print(f"Error decoding telemetry: {e}")
        return None

while True:
    try:
        print("Listening for data, port 27015...")
        data, client_address = udp_server.recvfrom(4096)
        print('Received {} bytes from {}'.format(len(data), client_address))
        ip_address, port = client_address
        ident = hash(str(ip_address) + ":" + str(port) + "-" + salt)
        telemetry = decode_telemetry(data, ident)

        if telemetry:
            print("Decoded Telemetry Data:")
            print(json.dumps(telemetry, indent=4))
    except KeyboardInterrupt:
        print("Server shutting down.")
        break

