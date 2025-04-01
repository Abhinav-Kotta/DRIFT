import socket
import struct
import asyncio
import websockets
import json
import psutil
import os
import signal
from concurrent.futures import ThreadPoolExecutor
from queue import Queue
from dataclasses import dataclass
from typing import Dict, Set, List
from pympler import asizeof # remove this bitch later
import sys # remove this bitch later
import time

WS_PORT = 8765

@dataclass
class RaceMessage:
    port: int
    data: str

def cleanup_ws_port():
    """Cleanup any process using the WebSocket port"""
    for conn in psutil.net_connections():
        if conn.laddr.port == WS_PORT and conn.pid is not None:
            try:
                process = psutil.Process(conn.pid)
                if process.pid != os.getpid():  # Don't kill ourselves
                    print(f"Killing process using WebSocket port {WS_PORT}")
                    os.kill(process.pid, signal.SIGTERM)
            except (psutil.NoSuchProcess, psutil.AccessDenied):
                pass

def is_port_available(port: int) -> bool:
    """Check if a port is available"""
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        try:
            s.bind(('0.0.0.0', port))
            return True
        except socket.error:
            return False


class RaceServer:
    def __init__(self):
        self.race_clients: Dict[int, Set[websockets.WebSocketServerProtocol]] = {}
        self.race_queues: Dict[int, Queue] = {}  # Separate queue per race
        self.udp_servers: Dict[int, socket.socket] = {}
        self.shutdown_event = asyncio.Event()
        self.race_caches: Dict[int, List[str]] = {}  # Separate cache per race
        self.users_in_race: Dict[int, Set[int]] = {} # dict of users that entered a race
        self.race_id_to_port: Dict[str, int] = {}  # Map race_id to UDP port

    def get_user_race(self):
        return self.users_in_race

    def map_race_id_to_port(self, race_id: str, udp_port: int):
        """Stores a mapping of race_id (UUID) to udp_port and initializes a queue"""
        self.race_id_to_port[race_id] = udp_port
        self.race_caches[udp_port] = []  # Initialize an empty cache for this race
        self.race_queues[udp_port] = Queue()  # Create a dedicated queue per race

    async def websocket_handler(self, connection):
        """Handle WebSocket connections with a single connection parameter"""
        try:
            # Debug print to see what we're receiving
            print(f"Debug - Connection attributes: {connection.__dict__}")
            path = connection.request.path
            path_parts = path.strip('/').split('/')
            
            if len(path_parts) != 2 or path_parts[0] != 'race':
                await connection.close(1008, "Invalid path format")
                return
                
            race_port = int(path_parts[1])
            if race_port not in self.race_clients:
                self.race_clients[race_port] = set()
            
            self.race_clients[race_port].add(connection)
            print(f"Client connected to race on UDP port {race_port}")
            
            try:
                await connection.wait_closed()
            finally:
                if race_port in self.race_clients:
                    self.race_clients[race_port].remove(connection)
                    print(f"Current race cache length for UDP port {race_port}: {len(self.race_caches.get(race_port, []))}")
                    print(f"Race cache size in bytes for UDP port {race_port}: {sys.getsizeof(self.race_caches.get(race_port, []))}")
                    print(f"Race cache size in bytes (including contents) for UDP port {race_port}: {asizeof.asizeof(self.race_caches.get(race_port, []))}")
                    print(f"Client disconnected from race on UDP port {race_port}")
                    
        except (ValueError, IndexError) as e:
            print(f"Error processing WebSocket path: {e}")
            await connection.close(1008, "Invalid path format")
    
    async def start_ws_server(self):
        print("Starting start_ws_server method")
        
        if not is_port_available(WS_PORT):
            print(f"Port {WS_PORT} is in use. Attempting cleanup...")
            cleanup_ws_port()
            await asyncio.sleep(1)
            
            if not is_port_available(WS_PORT):
                raise RuntimeError(f"Could not secure port {WS_PORT} for WebSocket server")

        print("About to start WebSocket server")
        self.ws_server = await websockets.serve(self.websocket_handler, "0.0.0.0", WS_PORT)
        print(f"WebSocket server started on port {WS_PORT}", flush=True)
        
        return WS_PORT

    def start_udp_server(self, udp_port: int):
        """Start UDP server for receiving position data"""
        udp_server = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        server_address = ('0.0.0.0', udp_port)
        udp_server.bind(server_address)
        self.udp_servers[udp_port] = udp_server
        print(f'UDP Server listening on port {udp_port}')

        firstPacket = True
        while udp_port in self.udp_servers:
            try:
                data, addr = udp_server.recvfrom(4096)
                if len(data) >= 81:
                    try:
                        if (firstPacket):
                            startTime = time.time()
                            firstPacket = False
                    
                        unpacked_data = struct.unpack('<f 3f 4f 3f 3f 4f 2f B 4f', data)

                        timestamp = time.time() - startTime
                        position = unpacked_data[1:4]
                        attitude = unpacked_data[4:8]
                        velocity = unpacked_data[8:11]
                        gyro = unpacked_data[11:14]
                        inputs = unpacked_data[14:18]
                        battery = unpacked_data[18:20]
                        motor_count = unpacked_data[20]
                        motor_rpms = unpacked_data[21:]

                        drone_data = {
                            "drone_id": addr[0],
                            "timestamp": timestamp,
                            "position": {"x": position[0], "y": position[1], "z": position[2]},
                            "attitude": {"x": attitude[0], "y": attitude[1], "z": attitude[2], "w": attitude[3]},
                            "velocity": {"x": velocity[0], "y": velocity[1], "z": velocity[2]},
                            "gyro": {"pitch": gyro[0], "roll": gyro[1], "yaw": gyro[2]},
                            "inputs": {"throttle": inputs[0], "yaw": inputs[1], "pitch": inputs[2], "roll": inputs[3]},
                            "battery": {"percentage": battery[0], "voltage": battery[1]},
                            "motor_count": motor_count,
                            "motor_rpms": list(motor_rpms)
                        }

                        message = json.dumps(drone_data)
                        print(message)
                        # Add message to the correct queue
                        if udp_port in self.race_queues:
                            self.race_queues[udp_port].put(RaceMessage(udp_port, message))
                    except struct.error as e:
                        print(f"Error unpacking position data: {e}")
            except Exception as e:
                print(f"Error on UDP port {udp_port}: {str(e)}")

    def add_udp_server(self, udp_port: int):
        if udp_port not in self.race_queues:
            self.race_queues[udp_port] = Queue()
        asyncio.get_event_loop().run_in_executor(None, self.start_udp_server, udp_port)

    async def broadcast_worker(self):
        while not self.shutdown_event.is_set():
            try:
                for udp_port, queue in self.race_queues.items():
                    while not queue.empty():
                        msg = queue.get()

                        # Ensure cache exists for this race
                        if udp_port not in self.race_caches:
                            self.race_caches[udp_port] = []

                        # Store the message in the race-specific cache
                        self.race_caches[udp_port].append(msg.data)

                        # Send the message to all clients in the race
                        if udp_port in self.race_clients:
                            disconnected = set()
                            for client in self.race_clients[udp_port]:
                                try:
                                    await client.send(msg.data)
                                except websockets.exceptions.ConnectionClosed:
                                    disconnected.add(client)
                            self.race_clients[udp_port] -= disconnected
                await asyncio.sleep(0.001)
            except Exception as e:
                import traceback
                print(f"Error in broadcast worker: {str(e)}")
                print(traceback.format_exc())

        print("Broadcast worker stopped.")

    async def main(self):
        ws_port = await self.start_ws_server()
        asyncio.create_task(self.broadcast_worker())
        try:
            await self.ws_server.wait_closed()
        finally:
            for udp_socket in self.udp_servers.values():
                udp_socket.close()

race_server = RaceServer()

async def start_server():
    ws_port = await race_server.start_ws_server()
    asyncio.create_task(race_server.broadcast_worker())
    return ws_port

def add_udp_port(port: int):
    race_server.add_udp_server(port)

if __name__ == "__main__":
    try:
        asyncio.run(race_server.main())
    except KeyboardInterrupt:
        print("\nShutting down server...")
    finally:
        # Cleanup on exit
        for udp_socket in race_server.udp_servers.values():
            udp_socket.close()