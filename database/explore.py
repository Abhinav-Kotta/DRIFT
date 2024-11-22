from psycopg2 import sql
from datetime import datetime

class DroneDBExplorer:
    def __init__(self, connection_string):
        self.conn_string = connection_string
        self.conn = None
        self.cur = None

    def connect(self):
        try:
            self.conn = psycopg2.connect(self.conn_string)
            self.cur = self.conn.cursor()
            print("Successfully connected to the database")
        except Exception as e:
            print(f"Error connecting to database: {e}")

    def close(self):
        if self.cur:
            self.cur.close()
        if self.conn:
            self.conn.close()
            print("Database connection closed")

    def list_tables(self):
        """List all tables in the database"""
        try:
            self.cur.execute("""
                SELECT table_name 
                FROM information_schema.tables 
                WHERE table_schema = 'public'
            """)
            tables = self.cur.fetchall()
            print("\nAvailable tables:")
            for table in tables:
                print(f"- {table[0]}")
        except Exception as e:
            print(f"Error listing tables: {e}")

    def describe_table(self, table_name):
        """Show column information for a specific table"""
        try:
            self.cur.execute("""
                SELECT column_name, data_type, character_maximum_length
                FROM information_schema.columns
                WHERE table_name = %s
            """, (table_name,))
            columns = self.cur.fetchall()
            print(f"\nColumns in {table_name}:")
            for col in columns:
                print(f"- {col[0]}: {col[1]}" + 
                      (f" (max length: {col[2]})" if col[2] else ""))
        except Exception as e:
            print(f"Error describing table: {e}")

    def create_drone_tables(self):
        """Create tables for storing drone telemetry data"""
        try:
            # Create hypertable for drone telemetry
            self.cur.execute("""
                CREATE TABLE IF NOT EXISTS drone_telemetry (
                    time TIMESTAMPTZ NOT NULL,
                    position_x FLOAT,
                    position_y FLOAT,
                    position_z FLOAT,
                    quaternion_x FLOAT,
                    quaternion_y FLOAT,
                    quaternion_z FLOAT,
                    quaternion_w FLOAT,
                    gyro_x FLOAT,
                    gyro_y FLOAT,
                    gyro_z FLOAT,
                    input_throttle FLOAT,
                    input_yaw FLOAT,
                    input_pitch FLOAT,
                    input_roll FLOAT,
                    battery_voltage FLOAT,
                    battery_percentage FLOAT,
                    motor1_rpm FLOAT,
                    motor2_rpm FLOAT,
                    motor3_rpm FLOAT,
                    motor4_rpm FLOAT
                );
            """)
            
            # Convert to TimescaleDB hypertable
            self.cur.execute("""
                SELECT create_hypertable('drone_telemetry', 'time', 
                    if_not_exists => TRUE);
            """)
            
            self.conn.commit()
            print("Successfully created drone telemetry tables")
        except Exception as e:
            print(f"Error creating tables: {e}")

    def sample_query(self, table_name, limit=5):
        """Show sample data from a table"""
        try:
            self.cur.execute(
                sql.SQL("SELECT * FROM {} LIMIT %s").format(
                    sql.Identifier(table_name)
                ), (limit,)
            )
            rows = self.cur.fetchall()
            if rows:
                print(f"\nSample data from {table_name}:")
                for row in rows:
                    print(row)
            else:
                print(f"No data found in {table_name}")
        except Exception as e:
            print(f"Error querying table: {e}")

if __name__ == "__main__":
    # Your connection string
    CONNECTION_STRING = ""
    
    explorer = DroneDBExplorer(CONNECTION_STRING)
    try:
        explorer.connect()
        
        # Create tables if they don't exist
        explorer.create_drone_tables()
        
        # List all tables
        explorer.list_tables()
        
        # Describe the drone_telemetry table
        explorer.describe_table('drone_telemetry')
        
        # Show sample data
        explorer.sample_query('drone_telemetry')
        
    finally:
        explorer.close()