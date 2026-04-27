using System.IO;
using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using SharedModels;
class Program
{
    static async Task Main()
    {
        string serverIp = "172.60.59.249";
        int port = 8081;

        try
        {
            using TcpClient client = new TcpClient(); // Initialize TcpClient
            Console.WriteLine($"Attempting to connect to {serverIp}:{port}");

            await client.ConnectAsync(serverIp, port); // Blocks until connection is established!
            NetworkStream stream = client.GetStream();
            Console.WriteLine("Connected! Please Enter in a Username: ");

            while (true)
            {
                string? message = Console.ReadLine();

                if (string.IsNullOrEmpty(message))
                {
                    continue; // Continue the while loop if the string is null or empty
                }
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);

            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error Thrown: {ex.Message}");
        }
    }
}
