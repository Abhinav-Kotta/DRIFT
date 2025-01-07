import psycopg2
from dotenv import load_dotenv
import os

# Load environment variables from .env file
load_dotenv()

# Build connection string from environment variables
CONNECTION = f"postgres://{os.getenv('DB_USER')}:{os.getenv('DB_PASSWORD')}@{os.getenv('DB_HOST')}:{os.getenv('DB_PORT')}/{os.getenv('DB_NAME')}?sslmode=require"

def get_all_data():
    conn = psycopg2.connect(CONNECTION)
    cursor = conn.cursor()
    
    cursor.execute("""
        SELECT * FROM pg_stat_statements
    """)
    
    tables = cursor.fetchall()
    
    print("\nTables in database:")
    for table in tables:
        print(table[0])
        
    cursor.close()
    conn.close()

get_all_data()