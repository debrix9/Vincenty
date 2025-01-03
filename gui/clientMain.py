import clientUtils
import threading
import time
import queue

## Note: Top level code ##
response_queue = queue.Queue()

"""
Main function to be called to start the client alongside the GUI
Debug functions to skip GUI inside and testing
"""
def clientMain():
    
    global client_socket
    # Thread the client stuff
    connection_thread = threading.Thread(target=connect_to_server)
    connection_thread.daemon = True  # Be an evil folk and die with your main host
    connection_thread.start()
    
    while client_socket is None:
        time.sleep(0.1)  # Wait until the client_socket is initialized
    
    while True:
        # Main loop for handling user inputs
        problem_type = input("Do you want to solve a 'direct' or 'inverse' problem? ").strip().lower()

        if problem_type == "direct":
            lat = float(input("Enter latitude: "))
            lon = float(input("Enter longitude: "))
            bearing = float(input("Enter bearing: "))
            range_value = float(input("Enter range: "))
            request = f"dp, {lat}, {lon}, {0}, {0}, {bearing}, {range_value}, {0}"
        elif problem_type == "inverse":
            # Handle inverse problem input (similar to direct)
            print("Inverse problem input handling here.")
            continue
        else:
            print("Invalid input, try again.")
            continue

        if client_socket:
            # Send request to the server and wait for response
            if not clientUtils.send_request_to_server(client_socket, request):
                print("Connection lost, reconnecting...")
                break  # Exit and reconnect if the server is lost
        else:
            print("Not connected to server.")
            break
        if not response_queue.empty():
            response = response_queue.get()  # Get the response from the queue
            print(f"Received response: {response}")
            
        cont = input("Do you want to continue? (y/n): ").strip().lower()
        if cont != 'y':
            break

"""
Connect to server on a different Thread
Attempt a connection every 5 seconds
If server disconnects retry connection again
"""
def connect_to_server():
    global client_socket
    while True:
        try:
            # Try connecting to the server, attempt every 5 seconds
            client_socket = clientUtils.create_client_socket(response_queue)
            print("Connected to server.")

            # Case of connection loss
            while True:
                if not client_socket:
                    print("Connection lost, reconnecting...")
                    break
                # Attempts to reconnect after 1 second
                time.sleep(1)
            
            client_socket.close()
            print("Disconnected from server.")
        except Exception as e:
            print(f"Error in server connection: {e}")
            time.sleep(5)  
