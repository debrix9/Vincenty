import socket

# Create a TCP client socket
client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
client_socket.connect(('127.0.0.1', 5000))  # Connect to the server

try:
    while True:
        # Prompt the user for a command and input
        command = input("Enter command (A or B): ").strip().upper()
        if command not in ['A', 'B']:
            print("Invalid command. Use A or B.")
            continue

        input_value = input("Enter a number: ").strip()
        if not input_value.isdigit():
            print("Invalid number. Please enter a valid integer.")
            continue

        # Construct the request and send it to the server
        request = f"{command},{input_value}"
        client_socket.sendall(request.encode('utf-8'))

        # Receive the response from the server
        response = client_socket.recv(1024).decode('utf-8')
        print(f"Server response: {response}")

        # Option to exit the loop
        cont = input("Do you want to continue? (y/n): ").strip().lower()
        if cont != 'y':
            break
finally:
    # Close the connection
    client_socket.close()