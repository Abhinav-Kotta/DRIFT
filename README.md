# DRIFT (Drone Racing Instant Flight Tracker)

A complete solution for streaming, storing, visualizing, and managing drone telemetry data in real-time with race management capabilities.

## Overview

This system handles real-time drone telemetry from LiftOff simulator (or compatible data streams) and provides comprehensive functionality including:

- Real-time telemetry streaming via UDP
- WebSocket-based real-time data distribution
- Time-series database storage with TimescaleDB
- Race creation and management
- User authentication and management
- Historical flight data and race playback

## System Architecture

The application consists of several interconnected components:

- **Data Stream Service**: Processes incoming UDP telemetry and distributes via WebSockets
- **FastAPI Backend**: Handles race management, user authentication, and data retrieval
- **TimescaleDB Database**: Stores time-series telemetry data and user/race information
- **Flask Server**: Alternative REST API for telemetry data access
- **Simulation Tools**: For testing and development

## Directory Structure

```
└── ./
    ├── data_stream/              # Core data streaming components
    │   ├── databaseconnection.py # Database connection utilities
    │   ├── liftoff_simulator.py  # Simulation data generator
    │   ├── main.py               # FastAPI application entry point
    │   ├── telemetry_parser.py   # Telemetry data structure parser
    │   └── ws.py                 # WebSocket server implementation
    ├── Frontend/                 # Frontend components 
    ├── DatabaseConnectionExample.py # Example for connecting to the database
    ├── liftoff_simulation.py     # Standalone simulation tool
    ├── server.py                 # Flask server for REST API
```

## Features

- **Real-time Telemetry Streaming**: Capture position, attitude, velocity, and more at 50Hz
- **Race Management**: Create, join, save, and replay races
- **Multi-user Support**: Authentication system with password reset capabilities
- **Time Series Database**: Efficient storage and retrieval of flight data
- **WebSocket Communication**: Low-latency data distribution
- **Flexible API**: Both REST and WebSocket endpoints available

## Data Structure

The telemetry data includes:

- Timestamp
- 3D Position (x, y, z)
- Attitude (quaternion)
- Velocity (3D vector)
- Gyro readings (pitch, roll, yaw)
- Control inputs (throttle, yaw, pitch, roll)
- Battery state (voltage, percentage)
- Motor RPMs

## Installation

### Prerequisites

- Python 3.8+
- PostgreSQL with TimescaleDB extension
- Node.js (for frontend)

### Environment Setup

1. Clone the repository
   ```bash
   git clone https://github.com/yourusername/drone-telemetry-system.git](https://github.com/Abhinav-Kotta/DRIFT.git
   cd DRIFT/data_strean
   ```

2. Create and activate a virtual environment
   ```bash
   python -m venv venv
   source venv/bin/activate  # On Windows: venv\Scripts\activate
   ```

3. Install dependencies
   ```bash
   pip install -r requirements.txt
   ```

4. Create a `.env` file in the project root with database credentials:
   ```
   DB_USER=your_db_user
   DB_PASSWORD=your_db_password
   DB_HOST=your_db_host
   DB_PORT=5432
   DB_NAME=your_db_name
   ```

### Database Setup

1. Create a PostgreSQL database and enable TimescaleDB extension
   ```sql
   CREATE DATABASE your_db_name;
   \c your_db_name
   CREATE EXTENSION IF NOT EXISTS timescaledb CASCADE;
   ```

2. Run the database initialization script
   ```bash
   python -m data_stream.databaseconnection
   ```

## Usage

### Running the Backend

Start the FastAPI server:
```bash
uvicorn data_stream.main:app --reload --host 0.0.0.0 --port 8000
```

### Simulation

For development and testing, you can run the drone simulator:
```bash
python liftoff_simulation.py
```

## API Endpoints

### FastAPI Endpoints

- `POST /create_user` - Register a new user
- `POST /login` - Authenticate user
- `POST /reset_password` - Reset user password
- `DELETE /delete_user` - Delete user account
- `GET /saved_races` - List all saved races
- `GET /replay_race/{race_id}` - Get data for a specific race
- `GET /user_races/{user_id}` - List races for a specific user
- `PUT /update_race/{race_id}` - Update race details
- `DELETE /delete_race/{race_id}/{user_id}` - Delete a race
- `POST /create_race` - Create a new race
- `GET /watch_race/{race_id}/{user_id}` - Join a race as viewer
- `GET /list_races` - List active races
- `POST /save_race/{race_id}` - Save race data
- `DELETE /end_race/{race_id}` - End a race

### WebSocket Endpoints

Connect to `ws://hostname:8765/race/{udp_port}` to receive real-time telemetry for a specific race.

## Development

## Architecture Diagram

```
┌───────────────┐     ┌───────────────┐
│  Drone/       │     │  Unity Client │
│  Simulator    │     │               │
└───────┬───────┘     └───────┬───────┘
        │                     │
        │ UDP                 │ WebSocket
        ▼                     │
┌───────────────┐             │
│ Data Stream   │◄────────────┘
│ Service       │
└───────┬───────┘
        │
        │ SQL
        ▼
┌───────────────┐     ┌───────────────┐
│ TimescaleDB   │◄────┤ FastAPI       │
│ Database      │     │ Backend       │
└───────────────┘     └───────────────┘
```

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.
