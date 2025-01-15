
import socket
import struct
import time

# Replace with your VM's external IP address
VM_IP = "34.68.252.128"
VM_PORT = 27016


# Create a UDP socket
udp_client = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# Define dummy positional data (X, Y, Z)
dummy_positions = [
    (1.0, 2.0, 3.0),
    (4.5, 5.5, 6.5),
    (7.0, 8.0, 9.0),
    (0.0, 0.0, 0.0)  # Example of resetting position
]

try:
    for position in dummy_positions:
        # Pack the data as three floats (12 bytes total)
        packed_data = struct.pack('fff', *position)

        # Send the data to the VM's UDP server
        udp_client.sendto(packed_data, (VM_IP, VM_PORT))
        print(f"Sent position: {position}")

        # Wait for 1 second before sending the next packet
        time.sleep(1)

except KeyboardInterrupt:
    print("Stopped sending dummy data.")
finally:
    udp_client.close()
