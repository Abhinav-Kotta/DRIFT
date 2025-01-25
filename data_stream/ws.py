import socket
import struct
import asyncio
import websockets
import json
import argparse
from concurrent.futures import ThreadPoolExecutor
from queue import Queue
from dataclasses import dataclass
from typing import Dict, Set

@dataclass
class RaceMessage:
    port: int
    data: str

class RaceServer:
    def __init__(self, udp_port: int):
        self.race_clients: Dict[int, Set[websockets.WebSocketServerProtocol]] = {}
        self.message_queue = Queue()
        self.udp_port = udp_port

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
        self.ws_server = await websockets.serve(self.websocket_handler, "0.0.0.0", 0)
        ws_port = self.ws_server.sockets[0].getsockname()[1]
        print(f"WebSocket server started on port {ws_port}", flush=True)
        return ws_port

    def start_udp_server(self):
        udp_server = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        server_address = ('0.0.0.0', self.udp_port)
        udp_server.bind(server_address)
        print(f'UDP Server listening on port {self.udp_port}')

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
                    self.message_queue.put(RaceMessage(self.udp_port, message))
            except Exception as e:
                print(f"Error on UDP port {self.udp_port}: {str(e)}")

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
        asyncio.get_event_loop().run_in_executor(None, self.start_udp_server)
        await self.ws_server.wait_closed()

if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument('--port', type=int, required=True)
    args = parser.parse_args()
    server = RaceServer(args.port)
    asyncio.run(server.main())