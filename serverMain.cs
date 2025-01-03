
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace vincenty
{
    internal class serverMain
    {
        public static void serverRun()
        {
            TcpListener theServer = new TcpListener(IPAddress.Loopback, 5000);
            theServer.Start();
            Console.WriteLine("TCP Server up and alive! =)");
            Console.WriteLine("Vincenty calculator running");

            while (true)
            {
                using TcpClient client = theServer.AcceptTcpClient();
                using NetworkStream stream = client.GetStream();

                // Receive request from the client
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Received request: " + request);

                // Perform a calculation (example: square the number received)
                if (int.TryParse(request, out int number))
                {
                    int result = number * number; // Example calculation
                    string response = $"The square of {number} is {result}";

                    // Send the response back to the client
                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    stream.Write(responseBytes, 0, responseBytes.Length);
                    Console.WriteLine("Response sent.");
                }
                else
                {
                    string errorResponse = "Invalid input. Please send a number.";
                    byte[] responseBytes = Encoding.UTF8.GetBytes(errorResponse);
                    stream.Write(responseBytes, 0, responseBytes.Length);
                    Console.WriteLine("Error response sent.");
                }
            }
        }
    }
}
