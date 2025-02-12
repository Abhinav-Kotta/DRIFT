import psycopg2
from dotenv import load_dotenv
import os

# Load environment variables
load_dotenv()
CONNECTION = f"postgres://{os.getenv('DB_USER')}:{os.getenv('DB_PASSWORD')}@{os.getenv('DB_HOST')}:{os.getenv('DB_PORT')}/{os.getenv('DB_NAME')}?sslmode=require"


def create_telemetry_table():
    
    conn = psycopg2.connect(CONNECTION)
    cursor = conn.cursor()
    
    # Create a hypertable for time-series data
    cursor.execute("""
        CREATE TABLE IF NOT EXISTS drone_telemetry (
            time TIMESTAMPTZ NOT NULL,
            timestamp FLOAT,
            position_x FLOAT,
            position_y FLOAT,
            position_z FLOAT,
            attitude_x FLOAT,
            attitude_y FLOAT,
            attitude_z FLOAT,
            attitude_w FLOAT,
            velocity_x FLOAT,
            velocity_y FLOAT,
            velocity_z FLOAT,
            gyro_pitch FLOAT,
            gyro_roll FLOAT,
            gyro_yaw FLOAT,
            input_throttle FLOAT,
            input_yaw FLOAT,
            input_pitch FLOAT,
            input_roll FLOAT,
            battery_voltage FLOAT,
            battery_percentage FLOAT,
            motor_rpm_1 FLOAT,
            motor_rpm_2 FLOAT,
            motor_rpm_3 FLOAT,
            motor_rpm_4 FLOAT
        );
    """)
    
    # Convert to hypertable
    cursor.execute("""
        SELECT create_hypertable('drone_telemetry', 'time', if_not_exists => TRUE);
    """)
    
    conn.commit()
    cursor.close()
    conn.close()

def query_latest_telemetry():
    conn = psycopg2.connect(CONNECTION)
    cursor = conn.cursor()
    
    # First get column names
    cursor.execute("""
        SELECT column_name 
        FROM information_schema.columns 
        WHERE table_name = 'drone_telemetry' 
        ORDER BY ordinal_position
    """)
    columns = [col[0] for col in cursor.fetchall()]
    
    # Then get the latest data
    cursor.execute("""
        SELECT * FROM drone_telemetry 
        ORDER BY time DESC 
        LIMIT 1
    """)
    
    result = cursor.fetchone()
    
    # Print in a readable format
    print("\nLatest Telemetry Data:")
    print("----------------------")
    for col, val in zip(columns, result):
        # Format floating point numbers to 3 decimal places
        if isinstance(val, float):
            print(f"{col}: {val:.3f}")
        else:
            print(f"{col}: {val}")
    
    cursor.close()
    conn.close()
    return result

# Call the function
query_latest_telemetry()