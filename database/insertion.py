import pandas as pd
import psycopg2
from sqlalchemy import create_engine
from datetime import datetime, timedelta
from simulator import LiftoffSimulator
import time
import os
from dotenv import load_dotenv

class DroneDBHelper:
    def __init__(self):
        load_dotenv()
        
        # Get database credentials from environment variables
        db_user = os.getenv('DB_USER')
        db_password = os.getenv('DB_PASSWORD')
        db_host = os.getenv('DB_HOST')
        db_port = os.getenv('DB_PORT')
        db_name = os.getenv('DB_NAME')
        
        # Construct connection string
        connection_string = f"postgresql://{db_user}:{db_password}@{db_host}:{db_port}/{db_name}"
        # Convert postgres:// to postgresql:// as required by SQLAlchemy
        if connection_string.startswith('postgres://'):
            connection_string = 'postgresql://' + connection_string[len('postgres://'):]
        
        self.engine = create_engine(connection_string)
        self.simulator = LiftoffSimulator()

    def collect_simulation_data(self, duration_seconds=10):
        """Collect simulation data for specified duration and return as DataFrame"""
        data_points = []
        start_time = time.time()
        end_time = start_time + duration_seconds

        while time.time() < end_time:
            self.simulator.update_simulation()
            
            # Create a data point
            data_point = {
                'time': datetime.now(),
                'position_x': self.simulator.drone_state.position[0],
                'position_y': self.simulator.drone_state.position[1],
                'position_z': self.simulator.drone_state.position[2],
                'quaternion_x': self.simulator.drone_state.attitude[0],
                'quaternion_y': self.simulator.drone_state.attitude[1],
                'quaternion_z': self.simulator.drone_state.attitude[2],
                'quaternion_w': self.simulator.drone_state.attitude[3],
                'gyro_x': self.simulator.drone_state.gyro[0],
                'gyro_y': self.simulator.drone_state.gyro[1],
                'gyro_z': self.simulator.drone_state.gyro[2],
                'input_throttle': self.simulator.drone_state.inputs[0],
                'input_yaw': self.simulator.drone_state.inputs[1],
                'input_pitch': self.simulator.drone_state.inputs[2],
                'input_roll': self.simulator.drone_state.inputs[3],
                'battery_voltage': self.simulator.drone_state.battery[0],
                'battery_percentage': self.simulator.drone_state.battery[1],
                'motor1_rpm': self.simulator.drone_state.motor_rpms[0],
                'motor2_rpm': self.simulator.drone_state.motor_rpms[1],
                'motor3_rpm': self.simulator.drone_state.motor_rpms[2],
                'motor4_rpm': self.simulator.drone_state.motor_rpms[3]
            }
            data_points.append(data_point)
            time.sleep(1/60)  # 60Hz update rate

        return pd.DataFrame(data_points)

    def save_data(self, df, table_name='drone_telemetry'):
        """Save DataFrame to database"""
        df.to_sql(table_name, self.engine, if_exists='append', index=False)
        print(f"Saved {len(df)} records to database")

    def get_latest_records(self, limit=5):
        """Get the most recent records"""
        query = f"""
        SELECT * FROM drone_telemetry 
        ORDER BY time DESC 
        LIMIT {limit}
        """
        return pd.read_sql(query, self.engine)

    def get_position_data(self, minutes=5):
        """Get position data for the last few minutes"""
        query = f"""
        SELECT 
            time, position_x, position_y, position_z 
        FROM drone_telemetry 
        WHERE time > NOW() - INTERVAL '{minutes} minutes'
        ORDER BY time DESC
        """
        return pd.read_sql(query, self.engine)

    def get_battery_stats(self, interval='1 minute'):
        """Get battery statistics over time"""
        query = f"""
        SELECT 
            time_bucket('{interval}', time) as time_period,
            AVG(battery_voltage) as avg_voltage,
            AVG(battery_percentage) as avg_percentage
        FROM drone_telemetry
        GROUP BY time_period
        ORDER BY time_period DESC
        """
        return pd.read_sql(query, self.engine)

    def get_motor_stats(self, interval='1 minute'):
        """Get motor RPM statistics"""
        query = f"""
        SELECT 
            time_bucket('{interval}', time) as time_period,
            AVG(motor1_rpm) as motor1_avg,
            AVG(motor2_rpm) as motor2_avg,
            AVG(motor3_rpm) as motor3_avg,
            AVG(motor4_rpm) as motor4_avg
        FROM drone_telemetry
        GROUP BY time_period
        ORDER BY time_period DESC
        """
        return pd.read_sql(query, self.engine)

# Example usage
if __name__ == "__main__":
    
    # Create helper instance
    db_helper = DroneDBHelper()
    
    # Collect and save some simulation data
    print("Collecting simulation data...")
    df = db_helper.collect_simulation_data(duration_seconds=10)
    db_helper.save_data(df)
    
    # Show some example queries
    print("\nLatest records:")
    print(db_helper.get_latest_records(3))
    
    print("\nRecent battery stats:")
    print(db_helper.get_battery_stats('1 minute'))
    
    print("\nMotor statistics:")
    print(db_helper.get_motor_stats('1 minute'))