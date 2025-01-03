using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace vincenty
{
    internal class serverMain
    {
        static void Main(string[] args)
        {
            TcpListener theServer = serverUtils.startServer();
            Console.WriteLine("Waiting for connections...");

            while (true)
            {
                
                TcpClient client = theServer.AcceptTcpClient(); // Accept a client connection
                Console.WriteLine("Client connected.");

                // Handle the client connection in a new thread or task
                _ = Task.Run(() => HandleClient(client));
            }
        }

        private static void HandleClient(TcpClient client)
        {
            try
            {
                byte[] buffer = new byte[1024];

                using (NetworkStream stream = client.GetStream())
                {
                    while (true)
                    {
                        // Receive the request
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0) // Client disconnected
                        {
                            Console.WriteLine("Client disconnected.");
                            break;
                        }

                        string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Console.WriteLine($"Request received: {request}");

                        // Process the request
                        var theProblemNullable = serverUtils.parseRequest(request);
                        if (!theProblemNullable.HasValue)
                        {
                            Console.WriteLine("Failed to parse request");
                            string errorResponse = "Error: Invalid request";
                            serverUtils.sendResponse(stream, errorResponse);
                            continue;
                        }

                        var theProblem = theProblemNullable.Value;

                        if (theProblem.pType == constants.directProblem)
                        {
                            Console.WriteLine("Executing direct Vincenty problem");

                            coord startingPos = theProblem.Position;
                            var finalPosNullable = vincentyMain.directProblem(startingPos);

                            if (!finalPosNullable.HasValue)
                            {
                                Console.WriteLine("Failed to calculate the final position");
                                string failResponse = "Error: Calculation failed";
                                serverUtils.sendResponse(stream, failResponse);
                                continue;
                            }

                            coord finalPos = finalPosNullable.Value;
                            string response = string.Join(", ", theProblem.pType, finalPos.ToString());

                            // Send response to the client
                            serverUtils.sendResponse(stream, response);
                        }
                        else if (theProblem.pType == constants.inverseProblem)
                        {
                            Console.WriteLine("Inverse problem processing not implemented.");
                            string errorResponse = "Error: Inverse problem not implemented";
                            serverUtils.sendResponse(stream, errorResponse);
                        }
                        else
                        {
                            Console.WriteLine("Unknown problem type");
                            string errorResponse = "Error: Unknown problem type";
                            serverUtils.sendResponse(stream, errorResponse);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client: {ex.Message}");
            }
            finally
            {
                client.Close();
                Console.WriteLine("Connection closed.");
            }
        }


    }
}
