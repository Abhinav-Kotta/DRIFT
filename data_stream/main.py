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

ws_port: Optional[int] = None
ws_process = None

def get_available_port():
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.bind(('', 0))
        s.listen(5)
        return s.getsockname()[1]

async def ensure_ws_server():
    global ws_port, ws_process
    
    if ws_port is None:
        from ws import start_server
        ws_port = await start_server()

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
            ws_port=ws_port,
            status="started"
        )
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Failed to start race server: {str(e)}")