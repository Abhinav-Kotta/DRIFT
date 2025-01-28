import socket
import asyncio
import websockets
import json
from queue import Queue
from dataclasses import dataclass
from typing import Dict, Set
from telemetry_parser import LiftoffParser, create_udp_server

@dataclass
class RaceMessage:
    port: int
    data: str

class RaceServer:
    def __init__(self):
        self.race_clients: Dict[int, Set[websockets.WebSocketServerProtocol]] = {}
        self.message_queue = Queue()
        self.udp_servers: Dict[int, socket.socket] = {}
        self.parsers: Dict[int, LiftoffParser] = {}

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

    def start_udp_server(self, udp_port: int):
        config = {
            "StreamFormat": [
                "Timestamp",
                "Position",
                "Attitude",
                "Velocity",
                "Gyro",
                "Input",
                "Battery",
                "MotorRPM"
            ]
        }

        udp_server, parser = create_udp_server('0.0.0.0', udp_port, config["StreamFormat"])
        self.udp_servers[udp_port] = udp_server
        self.parsers[udp_port] = parser
        print(f'UDP Server listening on port {udp_port}')

        while True:
            try:
                data, _ = udp_server.recvfrom(4096)
                telemetry = parser.parse_packet(data)
                self.message_queue.put(RaceMessage(udp_port, telemetry.to_json()))
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
        await self.ws_server.wait_closed()

# Global instance
race_server = RaceServer()

async def start_server():
    ws_port = await race_server.start_ws_server()
    asyncio.create_task(race_server.broadcast_worker())
    return ws_port

def add_udp_port(port: int):
    race_server.add_udp_server(port)

if __name__ == "__main__":
    asyncio.run(race_server.main())