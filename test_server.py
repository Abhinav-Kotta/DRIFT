import requests

def test_server():
    # Test basic connection
    print("Testing server connection...")
    try:
        response = requests.get('http://127.0.0.1:5000/test')
        print("Test endpoint response:", response.json())
    except requests.exceptions.ConnectionError:
        print("Failed to connect to server!")
        return

    # Test telemetry endpoint
    print("\nTesting telemetry endpoint...")
    try:
        response = requests.get('http://127.0.0.1:5000/telemetry')
        print("Telemetry response:", response.json())
    except requests.exceptions.ConnectionError:
        print("Failed to get telemetry data!")

if __name__ == "__main__":
    test_server()
