using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TcpServerProject
{
    public class TcpServer
    {
        private TcpListener _server;
        private bool _isRunning;

        public TcpServer(string ipAddress, int port)
        {
            _server = new TcpListener(IPAddress.Parse(ipAddress), port);
        }

        public void Start()
        {
            _server.Start();
            _isRunning = true;
            Console.WriteLine("✅ Server started. Waiting for connections...");

            while (_isRunning)
            {
                TcpClient client = _server.AcceptTcpClient();
                Console.WriteLine($"🔗 Client connected: {client.Client.RemoteEndPoint}");

                // Handle multiple clients concurrently
                Thread clientThread = new Thread(() => HandleClient(client));
                clientThread.Start();
            }
        }

        private void HandleClient(TcpClient client)
        {
            using (client)
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead;
                string lastCommand = ""; // Stores the last valid command

                try
                {
                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        string request = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                        Console.WriteLine($"📩 Received: {request}");

                        string response = ProcessRequest(request, ref lastCommand);
                        SendMessage(response, stream);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Client Error: {ex.Message}");
                }
                finally
                {
                    Console.WriteLine("❌ Client disconnected.");
                }
            }
        }

        private string ProcessRequest(string request, ref string lastCommand)
        {
            string[] parts = request.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Step 1: Receive a valid command
            if (parts.Length == 1 && (parts[0] == "Random" || parts[0] == "Add" || parts[0] == "Subtract"))
            {
                lastCommand = parts[0]; // Store the last command
                return "2: Input numbers"; // Step 2 response
            }

            // Step 3: Receive numbers and process the command
            else if (parts.Length == 2 && int.TryParse(parts[0], out int num1) && int.TryParse(parts[1], out int num2))
            {
                if (string.IsNullOrEmpty(lastCommand))
                {
                    return "❌ Error: No command received before numbers.";
                }

                switch (lastCommand)
                {
                    case "Random":
                        if (num1 > num2) return "❌ Error: First number must be smaller for Random.";
                        Random rnd = new Random();
                        return $"4: {rnd.Next(num1, num2 + 1)}"; // Random number between num1 and num2
                    case "Add":
                        return $"4: {num1 + num2}"; // Addition result
                    case "Subtract":
                        return $"4: {num1 - num2}"; // Subtraction result
                }
            }

            return "❌ Invalid request"; // Error message if input is incorrect
        }

        private static void SendMessage(string message, NetworkStream stream)
        {
            byte[] responseBytes = Encoding.UTF8.GetBytes(message + "\n");
            stream.Write(responseBytes, 0, responseBytes.Length);
        }

        public static void Main()
        {
            TcpServer server = new TcpServer("127.0.0.1", 12345);
            server.Start();
        }
    }
}
