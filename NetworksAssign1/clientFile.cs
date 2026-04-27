using System.IO;
using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
class Program
{
    static void Main()
    {
        string serverIp = "192.168.1.136";
        int port = 8080;

        try
        {
            using TcpClient client = new TcpClient(serverIp, port); // Initialize TcpClient
            NetworkStream stream = client.GetStream();

            while (true)
            {
                Console.WriteLine("Message to Send: ");
                var message = Console.ReadLine();

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