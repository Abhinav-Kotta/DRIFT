from fastapi import FastAPI, HTTPException
import socket
import asyncio
import sys
from pydantic import BaseModel
from typing import Dict, Optional
import uuid

app = FastAPI()

class RaceResponse(BaseModel):
    race_id: str
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

active_races: Dict[str, RaceResponse] = {}
@app.post("/create_race")
async def create_race() -> RaceResponse:
    udp_port = get_available_port()
    race_id = str(uuid.uuid4())
    
    try:
        add_udp_server(udp_port)
        race_response = RaceResponse(
            race_id=race_id,
            udp_port=udp_port,
            ws_port=WS_PORT,  
            status="started"
        )
        active_races[race_id] = race_response
        return race_response
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Failed to start race server: {str(e)}")
    
@app.get("/watch_race/{race_id}")
async def watch_race(race_id: str) -> RaceResponse:
    if race_id not in active_races:
        raise HTTPException(status_code=404, detail="Race not found")
    
    race_info = active_races[race_id]
    return race_info

@app.on_event("shutdown")
async def shutdown_event():
    pass

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)