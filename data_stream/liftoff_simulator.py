import socket
import struct
import time
import math
import argparse

def run_simulator(udp_port):
    """Run the simulator sending data to specified UDP port"""
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    server_address = ('34.68.252.128', udp_port)
    start_time = time.time()
    
    print(f"Starting simulation, sending data to {server_address[0]}:{server_address[1]}")
    print("Press Ctrl+C to stop")
    
    try:
        while True:
            current_time = time.time() - start_time
            # Generate figure-8 pattern
            scale = 5.0
            frequency = 0.2
            position = (
                scale * math.sin(2 * math.pi * frequency * current_time),
                2.0 + math.sin(2 * math.pi * frequency * 0.5 * current_time),
                scale * math.sin(4 * math.pi * frequency * current_time)
            )
            
            # Pack and send position data
            packet = struct.pack('fff', *position)
            sock.sendto(packet, server_address)
            
            # Print position being sent (for debugging)
            print(f"Sending position: x={position[0]:.2f}, y={position[1]:.2f}, z={position[2]:.2f}", end='\r')
            
            time.sleep(1/50)  # 50Hz update rate
            
    except KeyboardInterrupt:
        print("\nSimulation stopped")
    finally:
        sock.close()

def main():
    parser = argparse.ArgumentParser(description='Drone position simulator')
    parser.add_argument('port', type=int, help='UDP port number to send data to')
    args = parser.parse_args()
    
    run_simulator(args.port)

if __name__ == "__main__":
    main()