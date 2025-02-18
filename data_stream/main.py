from fastapi import FastAPI, HTTPException, Depends
import socket
import asyncio
import asyncpg
from pydantic import BaseModel
from typing import Dict, List
import uuid
from ws import race_server
from dotenv import load_dotenv
import os
import bcrypt
import uvicorn
import json

WS_PORT = 8765

# Load environment variables
load_dotenv()
CONNECTION = f"postgresql://{os.getenv('DB_USER')}:{os.getenv('DB_PASSWORD')}@{os.getenv('DB_HOST')}:{os.getenv('DB_PORT')}/{os.getenv('DB_NAME')}?sslmode=require"
app = FastAPI()

class RaceResponse(BaseModel):
    race_id: str
    udp_port: int
    ws_port: int
    status: str

class RaceListResponse(BaseModel):
    races: List[Dict[str, RaceResponse]]

class User(BaseModel):
    username: str
    password: str

class NewUser(BaseModel):
    username: str
    password: str
    security_question: str
    security_answer: str

class ResetPasswordRequest(BaseModel):
    username: str
    security_answer: str
    new_password: str

class SaveRaceRequest(BaseModel):
    race_name: str
    drift_map: str
    user_id: str

# Dependency for DB connection
async def get_db():
    conn = await asyncpg.connect(CONNECTION)
    try:
        yield conn
    finally:
        await conn.close()

def hash_password(password: str) -> str:
    salt = bcrypt.gensalt()
    return bcrypt.hashpw(password.encode(), salt).decode()

def verify_password(plain_password, hashed_password):
    return bcrypt.checkpw(plain_password.encode(), hashed_password.encode())

@app.post("/create_user")
async def create_user(user: NewUser, db=Depends(get_db)):
    hashed_password = hash_password(user.password)
    hashed_answer = hash_password(user.security_answer.lower())

    try:
        await db.execute(
            "INSERT INTO users (username, password, security_question, security_answer) VALUES ($1, $2, $3, $4)",
            user.username, hashed_password, user.security_question, hashed_answer
        )
        return {"status": "success", "message": "User created successfully"}
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Database error: {str(e)}")


@app.post("/login")
async def login(user: User, db=Depends(get_db)):
    try:
        result = await db.fetchrow("SELECT password FROM users WHERE username = $1", user.username)

        if not result:
            raise HTTPException(status_code=401, detail="Invalid username")

        stored_hashed_password = result["password"]

        if not verify_password(user.password, stored_hashed_password):
            raise HTTPException(status_code=401, detail="Invalid password")
        
        u_id = await db.fetchrow("SELECT id FROM users WHERE username = $1", user.username)
        return {"status": "success", "message": "Login successful", "user_id": u_id}
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Login failed: {str(e)}")

@app.post("/reset_password")
async def reset_password(request: ResetPasswordRequest, db=Depends(get_db)):
    try:
        result = await db.fetchrow("SELECT security_answer FROM users WHERE username = $1", request.username)

        if not result:
            raise HTTPException(status_code=404, detail="User not found")

        stored_hashed_answer = result["security_answer"]

        # Verify the security answer
        if not verify_password(request.security_answer.lower(), stored_hashed_answer):
            raise HTTPException(status_code=401, detail="Incorrect security answer")

        hashed_new_password = hash_password(request.new_password)
        await db.execute("UPDATE users SET password = $1 WHERE username = $2", hashed_new_password, request.username)

        return {"status": "success", "message": "Password reset successful"}
    
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Reset failed: {str(e)}")
    
@app.delete("/delete_user")
async def delete_user(username: str, db=Depends(get_db)):
    try:
        result = await db.execute("DELETE FROM users WHERE username = $1", username)
        print(result)
        # Check if a user was actually deleted
        if result == "DELETE 0":
            raise HTTPException(status_code=404, detail="User not found")

        return {"status": "success", "message": f"User '{username}' deleted successfully"}
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Database error: {str(e)}")
    
def get_available_port() -> int:
    """Get an available port for UDP server"""
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.bind(('', 0))
        s.listen(1)
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

@app.on_event("shutdown")
async def shutdown_event():
    print("Shutting down WebSocket and broadcast worker...")
    race_server.shutdown_event.set()  # SIGNAL broadcast_worker to stop
    await asyncio.sleep(0.1)  # Give time for cleanup
    tasks = [t for t in asyncio.all_tasks() if t is not asyncio.current_task()]
    for task in tasks:
        task.cancel()
    try:
        await asyncio.gather(*tasks, return_exceptions=True)
    except asyncio.CancelledError:
        print("CancelledError caught during shutdown.")  # Suppress error
    finally:
        print("Application shutdown complete.")
    
active_races: Dict[str, RaceResponse] = {}

@app.post("/create_race")
async def create_race() -> Dict[str, RaceResponse]:
    udp_port = get_available_port()
    race_id = str(uuid.uuid4())
    
    try:
        race_server.add_udp_server(udp_port)
        race_response = RaceResponse(
            race_id=race_id,
            udp_port=udp_port,
            ws_port=WS_PORT,
            status="started"
        )
        active_races[race_id] = race_response
        return {race_id: race_response}
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Failed to start race server: {str(e)}")
    
@app.get("/watch_race/{race_id}")
async def watch_race(race_id: str) -> RaceResponse:
    if race_id not in active_races:
        raise HTTPException(status_code=404, detail="Race not found")
    return active_races[race_id]

@app.get("/list_races")
async def list_races() -> RaceListResponse:
    race_list = [{k: v} for k, v in active_races.items()]
    return RaceListResponse(races=race_list)

@app.post("/save_race/{race_id}")
async def save_race(race_id: str, input: SaveRaceRequest, db=Depends(get_db)):
    try:
        # Get the cached race data
        race_data = race_server.race_cache

        if not race_data:
            raise HTTPException(status_code=404, detail="No race data available to save")

        # Convert the cached data to JSON string
        race_json = json.dumps(race_data)

        # Insert into the 'races' table
        await db.execute(
            "INSERT INTO races (race_id, race_name, drift_map, user_id, flight_packet) VALUES ($1, $2, $3, $4, $5)",
            race_id, input.race_name, input.drift_map, input.user_id, race_json  # Replace placeholders with actual data
        )

        # Clear the cache after saving
        race_server.race_cache.clear()

        return {"status": "success", "message": "Race data saved successfully"}

    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error saving race data: {str(e)}")

if __name__ == "__main__":
    uvicorn.run(
        "main:app",
        host="0.0.0.0",
        port=8000,
        log_level="info",
        reload=True,
        timeout_graceful_shutdown=5
    )
