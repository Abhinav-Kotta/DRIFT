import psycopg2
from psycopg2 import sql
from datetime import datetime
import pandas as pd
from dotenv import load_dotenv
import os
from collections import defaultdict
from sqlalchemy import create_engine

class DroneDBExplorer:
    def __init__(self):
        # Load environment variables
        load_dotenv()
        
        # Get database credentials from environment variables
        db_user = os.getenv('DB_USER')
        db_password = os.getenv('DB_PASSWORD')
        db_host = os.getenv('DB_HOST')
        db_port = os.getenv('DB_PORT')
        db_name = os.getenv('DB_NAME')
        
        # Create both psycopg2 and SQLAlchemy connections
        self.conn_string = f"postgresql://{db_user}:{db_password}@{db_host}:{db_port}/{db_name}"
        self.engine = create_engine(self.conn_string)
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
        try:
            if self.cur:
                self.cur.close()
            if self.conn:
                self.conn.close()
                print("Database connection closed")
        except Exception as e:
            print(f"Error closing connection: {e}")

    def get_schema_hierarchy(self):
        """Get database schema in a hierarchical structure"""
        try:
            # Get schemas
            self.cur.execute("""
                SELECT DISTINCT table_schema 
                FROM information_schema.tables 
                WHERE table_schema NOT IN ('pg_catalog', 'information_schema')
                ORDER BY table_schema;
            """)
            schemas = self.cur.fetchall()
            
            hierarchy = {}
            
            for schema in schemas:
                schema_name = schema[0]
                hierarchy[schema_name] = {}
                
                # Get tables in schema
                self.cur.execute("""
                    SELECT table_name 
                    FROM information_schema.tables 
                    WHERE table_schema = %s
                    ORDER BY table_name;
                """, (schema_name,))
                tables = self.cur.fetchall()
                
                for table in tables:
                    table_name = table[0]
                    hierarchy[schema_name][table_name] = {
                        'columns': {},
                        'indexes': [],
                        'dependencies': [],
                        'row_count': self.get_row_count(table_name)
                    }
                    
                    # Get columns and their details
                    self.cur.execute("""
                        SELECT 
                            column_name,
                            data_type,
                            is_nullable,
                            column_default,
                            character_maximum_length
                        FROM information_schema.columns
                        WHERE table_schema = %s AND table_name = %s
                        ORDER BY ordinal_position;
                    """, (schema_name, table_name))
                    
                    columns = self.cur.fetchall()
                    for col in columns:
                        hierarchy[schema_name][table_name]['columns'][col[0]] = {
                            'data_type': col[1],
                            'nullable': col[2],
                            'default': col[3],
                            'max_length': col[4]
                        }
                    
                    # Get indexes
                    self.cur.execute("""
                        SELECT indexname, indexdef
                        FROM pg_indexes
                        WHERE schemaname = %s AND tablename = %s;
                    """, (schema_name, table_name))
                    
                    indexes = self.cur.fetchall()
                    hierarchy[schema_name][table_name]['indexes'] = [
                        {'name': idx[0], 'definition': idx[1]} for idx in indexes
                    ]

            return hierarchy

        except Exception as e:
            print(f"Error getting schema hierarchy: {e}")
            self.conn.rollback()  # Rollback the transaction on error
            return None

    def get_row_count(self, table_name):
        """Get approximate row count for a table"""
        try:
            self.cur.execute(
                sql.SQL("SELECT count(*) FROM {}").format(sql.Identifier(table_name))
            )
            return self.cur.fetchone()[0]
        except Exception as e:
            self.conn.rollback()  # Rollback the transaction on error
            return 0

    def print_hierarchy(self, hierarchy=None, indent=0):
        """Print the database hierarchy in a tree-like structure"""
        if hierarchy is None:
            hierarchy = self.get_schema_hierarchy()
            
        if not hierarchy:
            return

        indent_str = "  " * indent
        
        for schema_name, schema_data in hierarchy.items():
            print(f"\n{indent_str}📁 Schema: {schema_name}")
            
            for table_name, table_data in schema_data.items():
                row_count = table_data['row_count']
                print(f"{indent_str}  📊 Table: {table_name} ({row_count} rows)")
                
                # Print columns
                print(f"{indent_str}    📋 Columns:")
                for col_name, col_data in table_data['columns'].items():
                    nullable = "NULL" if col_data['nullable'] == "YES" else "NOT NULL"
                    print(f"{indent_str}      ∟ {col_name}: {col_data['data_type']} {nullable}")
                
                # Print indexes
                if table_data['indexes']:
                    print(f"{indent_str}    🔍 Indexes:")
                    for idx in table_data['indexes']:
                        print(f"{indent_str}      ∟ {idx['name']}")

    def get_data_summary(self, table_name):
        """Get summary statistics for numerical columns"""
        try:
            query = f"""
            SELECT 
                count(*) as total_rows,
                count(distinct time::date) as unique_dates,
                min(time) as first_record,
                max(time) as last_record,
                avg(position_x) as avg_pos_x,
                avg(position_y) as avg_pos_y,
                avg(position_z) as avg_pos_z,
                avg(battery_percentage) as avg_battery,
                avg(motor1_rpm) as avg_motor1_rpm
            FROM {table_name}
            """
            
            # Use SQLAlchemy engine instead of psycopg2 connection
            df = pd.read_sql_query(query, self.engine)
            return df
            
        except Exception as e:
            print(f"Error getting data summary: {e}")
            return None

if __name__ == "__main__":
    explorer = DroneDBExplorer()
    try:
        explorer.connect()
        
        # Print full database hierarchy
        print("\n=== Database Structure ===")
        explorer.print_hierarchy()
        
        # Get summary of drone_telemetry table
        print("\n=== Data Summary for drone_telemetry ===")
        summary = explorer.get_data_summary('drone_telemetry')
        if summary is not None:
            print(summary)
        
    finally:
        explorer.close()