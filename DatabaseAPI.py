from flask import Flask, request, jsonify
from DatabaseConnectionExample import *

app = Flask(__name__)

@app.route('/getall', methods=['GET'])
def get_all():
    return jsonify(get_all_data())

if __name__ == '__main__':
    app.run(port=5000)