import socket
import struct
import asyncio
import websockets
import json
from concurrent.futures import ThreadPoolExecutor
from queue import Queue
from dataclasses import dataclass
from typing import Dict, Set

@dataclass
class RaceMessage:
    port: int
    data: str

class RaceServer:
    def __init__(self):
        self.race_clients: Dict[int, Set[websockets.WebSocketServerProtocol]] = {}
        self.message_queue = Queue()
        
    def get_available_ports(self):
        ports = []
        for _ in range(2):
            with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
                s.bind(('', 0))
                s.listen(5)
                ports.append(s.getsockname()[1])
        return ports

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

    def start_udp_server(self, udp_port):
        udp_server = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        server_address = ('0.0.0.0', udp_port)
        udp_server.bind(server_address)
        print(f'UDP Server listening on port {udp_port}')

        while True:
            try:
                data, _ = udp_server.recvfrom(4096)
                if len(data) == 12:
                    position = struct.unpack('fff', data)
                    message = json.dumps({
                        "x": position[0],
                        "y": position[1],
                        "z": position[2]
                    })
                    self.message_queue.put(RaceMessage(udp_port, message))
            except Exception as e:
                print(f"Error on UDP port {udp_port}: {str(e)}")

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
                await asyncio.sleep(0.001)  # Small delay to prevent CPU hogging
            except Exception as e:
                print(f"Error in broadcast worker: {str(e)}")

    async def main(self):
        udp_ports = self.get_available_ports()
        print("Available UDP ports:", udp_ports)

        # Start WebSocket server
        ws_server = await websockets.serve(self.websocket_handler, "0.0.0.0", 9090)
        print("WebSocket server started on port 9090")

        # Start UDP servers
        with ThreadPoolExecutor(max_workers=len(udp_ports)) as executor:
            executor.map(self.start_udp_server, udp_ports)

        # Start broadcast worker
        asyncio.create_task(self.broadcast_worker())
        
        await ws_server.wait_closed()

if __name__ == "__main__":
    print("Starting race server...")
    server = RaceServer()
    asyncio.run(server.main())