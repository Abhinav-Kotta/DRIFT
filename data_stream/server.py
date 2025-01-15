import socket

# Create a UDP socket
udp_server = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# Bind the socket to an IP address and port
server_address = ('0.0.0.0', 27015)
udp_server.bind(server_address)

print('UDP Server is listening at {}'.format(server_address))

while True:
    try:
        print("Listening for data, port 27015...")
        data, client_address = udp_server.recvfrom(4096)
        print('Received {} bytes from {}'.format(len(data), client_address))
        print('Data:', data)
    except KeyboardInterrupt:
        print("Server shutting down.")
        break
