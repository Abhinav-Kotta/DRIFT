from fastapi import FastAPI, HTTPException
import socket
import subprocess
import sys
import time
from pydantic import BaseModel
from typing import Dict

app = FastAPI()

class RaceResponse(BaseModel):
    udp_port: int
    ws_port: int
    status: str

def get_available_port():
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.bind(('', 0))
        s.listen(5)
        return s.getsockname()[1]

def start_race_server(udp_port: int):
    try:
        process = subprocess.Popen([
            sys.executable, 
            'ws.py',
            '--port', 
            str(udp_port)
        ], stdout=subprocess.PIPE)
        
        # Add timeout
        line = process.stdout.readline().decode().strip()
        ws_port = int(line.split()[-1])
        return True, ws_port

    except Exception as e:
        print(f"Error starting race server: {e}")
        return False, 0

@app.post("/create_race")
async def create_race() -> RaceResponse:
    udp_port = get_available_port()
    success, ws_port = start_race_server(udp_port)
    
    if success:
        return RaceResponse(
            udp_port=udp_port,
            ws_port=ws_port,
            status="started"
        )
    
    raise HTTPException(status_code=500, detail="Failed to start race server")