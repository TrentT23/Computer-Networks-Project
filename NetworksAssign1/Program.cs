using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    static void Main()
    {
        TcpListener server = new TcpListener(IPAddress.Any, 8080);
        server.Start();

        while (true)
        {
            TcpClient client = server.AcceptTcpClient();

            Thread cThread = new Thread(() => StartUserFileChat(client));
            cThread.Start();
        }
    }

    static void StartUserFileChat(TcpClient client)
    {
        using (NetworkStream stream = client.GetStream())
        {
            byte[] buffer = new byte[1024];
            int dataRead;

            try
            {
                while ((dataRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    string stringRead = Encoding.UTF8.GetString(buffer, 0, dataRead);
                    Console.WriteLine($"Data Sent: {stringRead}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error! {ex.Message}");
            }
            finally
            {
                client.Close();
                Console.WriteLine("Client Disconnected!");
            }
        }
    }
}
