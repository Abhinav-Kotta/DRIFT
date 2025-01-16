import socket
import struct
import asyncio
import websockets
import json
from concurrent.futures import ThreadPoolExecutor
# Configuration for multiple races
RACE_CONFIGS = [
    {"udp_port": 27016, "ws_port": 9090},
    {"udp_port": 27017, "ws_port": 9091},
    {"udp_port": 27018, "ws_port": 9092},
]
# Store connected WebSocket clients for each race
connected_clients = {config["udp_port"]: set() for config in RACE_CONFIGS}
print("connected clients: ", connected_clients)

def ws_available_ports():
    ports = []
    for _ in range(2):
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
            s.bind(('', 0))
            s.listen(5)
            ws_port = s.getsockname()[1]
            udp_port = ws_port + 1
            #print(f"Listening on port {port}")
            config = {"udp_port" : udp_port, "ws_port" : ws_port}
            RACE_CONFIGS.append(config)
            ports.append({"udp_port" : udp_port, "ws_port" : ws_port})
    return ports

async def websocket_handler(websocket, path, udp_port):
    """Handle WebSocket connections for a specific race"""
    try:
        connected_clients[udp_port].add(websocket)
        print(f"Client connected to race on UDP port {udp_port}")

        # Keep the connection alive and wait for disconnection
        await websocket.wait_closed()
    finally:
        connected_clients[udp_port].remove(websocket)
        print(f"Client disconnected from race on UDP port {udp_port}")
def start_udp_server(udp_port):
    """Start a UDP server for a specific port"""
    udp_server = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    server_address = ('0.0.0.0', udp_port)
    udp_server.bind(server_address)
    print(f'UDP Server is listening at port {udp_port}')

    while True:
        print("after while true")
        try:
            data, client_address = udp_server.recvfrom(4096)
            print("data: ", data)
            if len(data) == 12:
                position = struct.unpack('fff', data)
                position_x, position_y, position_z = position

                # Create JSON message
                message = json.dumps({
                    "x": position_x,
                    "y": position_y,
                    "z": position_z
                })

                # Send to all connected WebSocket clients for this race
                asyncio.run(broadcast_position(message, udp_port))

                print(f"Port {udp_port} - Position: X={position_x:.2f}, Y={position_y:.2f}, Z={position_z:.2f}")
        except Exception as e:
            print(f"Error on port {udp_port}: {str(e)}")
async def broadcast_position(message, udp_port):
    """Broadcast position data to all connected clients for a specific race"""
    if udp_port in connected_clients:
        disconnected = set()
        for client in connected_clients[udp_port]:
            try:
                await client.send(message)
            except websockets.exceptions.ConnectionClosed:
                disconnected.add(client)

        # Remove disconnected clients
        connected_clients[udp_port] -= disconnected
async def main():
    """Start WebSocket servers and UDP listeners"""
    # Start WebSocket servers
    ws_available_ports()
    print(RACE_CONFIGS)
    websocket_servers = []
    for config in RACE_CONFIGS:
        ws_server = await websockets.serve(
            lambda ws, path, port=config["udp_port"]: websocket_handler(ws, path, port),
            "0.0.0.0", 
           config["ws_port"]
        )
        websocket_servers.append(ws_server)
    print(f"WebSocket server started on port {config['ws_port']}")

    # Start UDP servers in separate threads
    with ThreadPoolExecutor(max_workers=len(RACE_CONFIGS)) as executor:
        udp_tasks = [
            executor.submit(start_udp_server, config["udp_port"])
            for config in RACE_CONFIGS
        ]

    # Keep the program running
    await asyncio.gather(*(
        ws_server.wait_closed() 
        for ws_server in websocket_servers
    ))
if __name__ == "__main__":
    print("Starting multi-port race server...")
    asyncio.run(main())
