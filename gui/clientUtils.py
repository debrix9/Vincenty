import socket
import threading
import time

"""
# Recive the response from the server
#
# client_socket -> The client socket
#
"""
def handle_server_response(client_socket, response_queue):
    while True:
        try:
            response = client_socket.recv(1024).decode('utf-8')
            if response:
                print(f"\nResponse from server: {response}")
                response_queue.put(response)
            else:
                print("\nServer closed the connection.")
                break
        except socket.error:
            print("\nConnection error while reading response.")
            break
        
"""
# Creates a socket for the client allwing to stay alive and reconnect
#
"""
def create_client_socket(response_queue):
     while True:
        try:
            client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            client_socket.connect(('127.0.0.1', 5000))  # Connect to the server
            print("Connected to server.")
            
            # Start a thread to handle the server's response
            response_thread = threading.Thread(target=handle_server_response, args=(client_socket, response_queue))
            response_thread.daemon = True
            response_thread.start()
            
            return client_socket
        except (socket.error, ConnectionRefusedError) as e:
            print(f"Connection failed. Retrying in 5 seconds... ({e})")
            time.sleep(5)

"""
# Send request to the server
#
# client_socket -> The client socket
# request -> The request
# Note it threads the response to wait for it separetly so the main keeps running
"""
def send_request_to_server(client_socket, request):
    try:
        client_socket.sendall(request.encode('utf-8'))
        print(f"Sent request to server: {request}")
        
        # Response handling is done by the `handle_server_response` thread
    except (socket.error, BrokenPipeError) as e:
        print(f"Error while sending request: {e}")
        return False
    return True
