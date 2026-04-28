using System.Net;
using System.Net.Sockets;
using FileSystem;
using SharedModels;

class Program
{
    static void Main()
    {
        TcpListener server = new TcpListener(IPAddress.Any, 8080);
        server.Start();
        Console.WriteLine("Server Started!");

        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            UserSession session = new UserSession();

            Thread cThread = new Thread(() => StartUserFileChat(client, session));
            cThread.Start();
        }
    }

    static void StartUserFileChat(TcpClient client, UserSession session)
    {
        try
        {
            string? username = DirectoryRoot.ReadClientLine(client);
            if (string.IsNullOrWhiteSpace(username))
            {
                return;
            }

            Console.WriteLine($"Username Read: {username}");
            session.Username = username;
            session.DirectoryInstance.InitializeUserName(username);

            // CODEx change: send the initial folder state with the welcome response.
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

    static void MainLoop(UserSession session, TcpClient client)
    {
        bool endLoop = false;

        while (!endLoop && client.Connected)
        {
            string? commandRead = DirectoryRoot.ReadClientLine(client);
            if (commandRead == null)
            {
                break;
            }

            if (string.IsNullOrWhiteSpace(commandRead))
            {
                continue;
            }

            endLoop = session.DirectoryInstance.ParseCommand(commandRead, client);
        }
    }

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
