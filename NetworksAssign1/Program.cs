using System.IO;
using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using FileSystem;
using System.Text.Json;
class Program
{
    static void Main()
    {
        TcpListener server = new TcpListener(IPAddress.Any, 8081); // opens the server
        server.Start();
        Console.WriteLine("Sever Started!");

        while (true)
        {
            TcpClient client = server.AcceptTcpClient(); // Accepts a client

            Thread cThread = new Thread(() => StartUserFileChat(client));
            cThread.Start();
        }
    }

    static void StartUserFileChat(TcpClient client)
    {
        using (NetworkStream stream = client.GetStream())
        {
            byte[] buffer = new byte[1024]; // Holds the bytes sent from the client
            int dataRead;

            try
            {
                while((dataRead = stream.Read(buffer, 0 , buffer.Length)) > 0)
                {
                    string stringRead = Encoding.UTF8.GetString(buffer, 0, dataRead);
                    Console.WriteLine($"Username Read: {stringRead}");

                    var responseObject = new ServerStatusResponse() {currentStatus = STATUS.USERNAME, message = $"Successfully created User {stringRead}"};
                    string json = JsonSerializer.Serialize(responseObject);

                    byte[] response = Encoding.UTF8.GetBytes(json);
                    stream.Write(response, 0, response.Length);
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

    enum STATUS
    {
        USERNAME, 
        INDIRECTORY
    }

    class ServerStatusResponse
    {
        public STATUS currentStatus{get; set;}
        public string? message {get; set;}
    }

    class ServerFolderAndOutput
    {
        SharedModels.Folder? currentFolder {get; set;}
        public string? output {get; set;}
        public string? message {get; set;}

    }
}
