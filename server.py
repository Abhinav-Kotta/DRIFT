from flask import Flask, jsonify
from flask_cors import CORS
from dotenv import load_dotenv
import psycopg2
import os
from datetime import datetime

app = Flask(__name__)
CORS(app, resources={
    r"/*": {
        "origins": "*",  # In production, you should specify exact origins
        "methods": ["GET", "POST", "OPTIONS"],
        "allow_headers": ["Content-Type", "Authorization"]
    }
})

# Load environment variables
load_dotenv()

app = Flask(__name__)

def get_db_connection():
    CONNECTION = f"postgres://{os.getenv('DB_USER')}:{os.getenv('DB_PASSWORD')}@{os.getenv('DB_HOST')}:{os.getenv('DB_PORT')}/{os.getenv('DB_NAME')}?sslmode=require"
    return psycopg2.connect(CONNECTION)

@app.route('/test', methods=['GET'])
def test_endpoint():
    return jsonify({"message": "Server is running!"})

@app.route('/telemetry', methods=['GET'])
def get_latest_telemetry():
    conn = get_db_connection()
    cursor = conn.cursor()
    
    try:
        # Get column names
        cursor.execute("""
            SELECT column_name 
            FROM information_schema.columns 
            WHERE table_name = 'drone_telemetry' 
            ORDER BY ordinal_position
        """)
        columns = [col[0] for col in cursor.fetchall()]
        
        # Get latest telemetry data
        cursor.execute("""
            SELECT * FROM drone_telemetry 
            ORDER BY time DESC 
            LIMIT 1
        """)
        
        result = cursor.fetchone()
        
        if result:
            # Convert to dictionary for JSON response
            data = dict(zip(columns, map(float_converter, result)))
            return jsonify({"status": "success", "data": data})
        else:
            return jsonify({"status": "error", "message": "No telemetry data found"})
            
    except Exception as e:
        return jsonify({"status": "error", "message": str(e)})
    finally:
        cursor.close()
        conn.close()

def float_converter(value):
    """Convert values to appropriate JSON format"""
    if isinstance(value, datetime):
        return value.isoformat()
    return value

@app.route('/health', methods=['GET'])
def health_check():
    return jsonify({"status": "healthy"})

if __name__ == '__main__':
    app.run(host='127.0.0.1', port=5000, debug=True)