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
from typing import Dict, Set
from telemetry_parser import create_udp_server

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
        self.message_queue = Queue()
        self.udp_servers: Dict[int, socket.socket] = {}

    async def websocket_handler(self, websocket, path):
        race_port = int(path.split('/')[-1])
        try:
            if race_port not in self.race_clients:
                self.race_clients[race_port] = set()
            self.race_clients[race_port].add(websocket)
            print(f"Client connected to race on UDP port {race_port}")
            await websocket.wait_closed()
        finally:
            self.race_clients[race_port].remove(websocket)
            print(f"Client disconnected from race on UDP port {race_port}")
    
    async def start_ws_server(self):
        # Check if port is in use and clean up if necessary
        if not is_port_available(WS_PORT):
            print(f"Port {WS_PORT} is in use. Attempting cleanup...")
            cleanup_ws_port()
            await asyncio.sleep(1)  # Give time for cleanup
            
            if not is_port_available(WS_PORT):
                raise RuntimeError(f"Could not secure port {WS_PORT} for WebSocket server")

        self.ws_server = await websockets.serve(self.websocket_handler, "0.0.0.0", WS_PORT)
        print(f"WebSocket server started on port {WS_PORT}", flush=True)
        return WS_PORT

    def start_udp_server(self, udp_port: int):
        stream_format = ["Timestamp", "Position", "Attitude", "Velocity", "Gyro", "Input", "Battery", "MotorRPM"]
        udp_server, parser = create_udp_server('0.0.0.0', udp_port, stream_format)
        self.udp_servers[udp_port] = udp_server
        print(f'UDP Server listening on port {udp_port}')

        while True:
            try:
                data, _ = udp_server.recvfrom(4096)
                telemetry = parser.parse_packet(data)
                message = telemetry.to_json()
                self.message_queue.put(RaceMessage(udp_port, message))
            except Exception as e:
                print(f"Error on UDP port {udp_port}: {str(e)}")

    def add_udp_server(self, udp_port: int):
        asyncio.get_event_loop().run_in_executor(None, self.start_udp_server, udp_port)

    async def broadcast_worker(self):
        while True:
            try:
                while not self.message_queue.empty():
                    msg = self.message_queue.get()
                    if msg.port in self.race_clients:
                        disconnected = set()
                        for client in self.race_clients[msg.port]:
                            try:
                                await client.send(msg.data)
                            except websockets.exceptions.ConnectionClosed:
                                disconnected.add(client)
                        self.race_clients[msg.port] -= disconnected
                await asyncio.sleep(0.001)
            except Exception as e:
                print(f"Error in broadcast worker: {str(e)}")

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