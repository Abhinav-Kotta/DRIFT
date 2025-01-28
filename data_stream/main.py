from fastapi import FastAPI, HTTPException
import socket
import asyncio
import sys
from pydantic import BaseModel
from typing import Dict, Optional

app = FastAPI()

class RaceResponse(BaseModel):
    udp_port: int
    ws_port: int
    status: str

WS_PORT = 8765

def get_available_port():
    """Get an available port for UDP"""
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.bind(('', 0))
        s.listen(5)
        return s.getsockname()[1]

async def ensure_ws_server():
    """Start the WebSocket server if it's not running"""
    try:
        from ws import start_server, cleanup_ws_port
        
        sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        result = sock.connect_ex(('127.0.0.1', WS_PORT))
        sock.close()
        
        if result == 0:
            print("WebSocket server already running")
        else:
            print("Starting WebSocket server")
            cleanup_ws_port()
            await start_server()
            
    except Exception as e:
        print(f"Error ensuring WebSocket server: {str(e)}")
        raise

def add_udp_server(udp_port: int):
    from ws import add_udp_port
    add_udp_port(udp_port)

@app.on_event("startup")
async def startup_event():
    await ensure_ws_server()

@app.post("/create_race")
async def create_race() -> RaceResponse:
    udp_port = get_available_port()
    
    try:
        add_udp_server(udp_port)
        return RaceResponse(
            udp_port=udp_port,
            ws_port=WS_PORT,  
            status="started"
        )
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Failed to start race server: {str(e)}")

@app.on_event("shutdown")
async def shutdown_event():
    pass