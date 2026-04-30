using System.Net;
using System.Net.Sockets;
using FileSystem;
using SharedModels;

/*

Trent Tiegs 
CSC 474 -- Computer Networks 
Dr. Huang
- This file controls the SERVER of the File System! Initializes a server, and accepts client connections.
- Each Client is given their own thread, and DirectoryRoot Object so multiple users are handled at once
- Uses SharedModels and FileSystem to parse client input and perform specified operations

Dependancies: SharedModels.cs, fileSystem.cs
*/
class Program
{
    static void Main()
    {
        TcpListener server = new TcpListener(IPAddress.Any, 8080); // TCP Socket Initialization
        server.Start(); // Start the server and listen for ANY IP Address attempting connection to this computer on port 8080
        Console.WriteLine("Server Started!");

        while (true) 
        {
            TcpClient client = server.AcceptTcpClient(); // Constantly block for new clients
            UserSession session = new UserSession(); // Session Object gives each client their own custom File Directory so users don't overwrite each others stuff

            Thread cThread = new Thread(() => StartUserFileChat(client, session)); // Every new client is given their own thread! This allows for multiple users!
            cThread.Start();
        }
    }

    static void StartUserFileChat(TcpClient client, UserSession session)
    {
        try
        {
            string? username = DirectoryRoot.ReadClientLine(client); // Read in a username
            if (string.IsNullOrWhiteSpace(username))
            {
                return;
            }

            Console.WriteLine($"Username Read: {username}");
            session.Username = username;
            session.DirectoryInstance.InitializeUserName(username); // Create a root directory specifically for the username

            DirectoryRoot.SendResponse(client, new ServerStatusResponse()
            {
                currentStatus = STATUS.INDIRECTORY,
                message = $"Successfully created User {username}",
                output = MethodsToShare.GetCommandsText(),
                waitingOnCommand = true,
                currentFolder = DirectoryRoot.BuildListing(session.DirectoryInstance.CurrentFolder)
            });

            MainLoop(session, client);
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

    /*
        Main Loop basically continously loops the program until the client quits for connection is interrupted. 
        Each client will loop through MainLoop on a seperate thread with their own unique session
    */
    static void MainLoop(UserSession session, TcpClient client)
    {
        bool endLoop = false;

        while (!endLoop && client.Connected) // Continously Loop Until connection fails or user types 'QUIT'
        {
            string? commandRead = DirectoryRoot.ReadClientLine(client); // Read for a Command
            if (commandRead == null)
            {
                break;
            }

            if (string.IsNullOrWhiteSpace(commandRead))
            {
                continue;
            }

            endLoop = session.DirectoryInstance.ParseCommand(commandRead, client); // Parse Command Reieved, false is returned if user quits
        }
    }

    // Class controls a clients session, so every client connected has their own unique directory interface. 
    class UserSession
    {
        public string Username { get; set; } = string.Empty;
        public DirectoryRoot DirectoryInstance { get; set; }

        public UserSession()
        {
            DirectoryInstance = new DirectoryRoot();
        }
    }
}
