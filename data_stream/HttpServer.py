from flask import Flask, jsonify
#from multi_ports import ws_available_ports
from flask_cors import CORS

app = Flask(__name__)

CORS(app, resources={
    r"/*": {
        "origins": "*",  # In production, you should specify exact origins
        "methods": ["GET", "POST", "OPTIONS"],
        "allow_headers": ["Content-Type", "Authorization"]
    }
})

@app.route('/createrace', methods=['GET'])
def create_race():
    return jsonify({"message": "creating race"})

@app.route('/watchrace/<raceId>', methods=['GET'])
def watch_race(raceId):
    return jsonify({"message": "watching race"})



if __name__ == "__main__":
    app.run()
