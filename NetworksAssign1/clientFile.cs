using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using SharedModels;

class Program
{
    static async Task Main()
    {
        Console.WriteLine("Enter an IP (or nothing for default): ");
        string? ipRead = Console.ReadLine();
        string serverIp = string.IsNullOrEmpty(ipRead) ? "100.64.54.110" : ipRead; // IP Address by default is my TailScale IP, will only work if I (Trent Tiegs) am running the server
        int port = 8080;

        try
        {
            using TcpClient client = new TcpClient(); // TCP Client is used to establish connection to a server using a socket
            Console.WriteLine($"Attempting to connect to {serverIp}:{port}");

            await client.ConnectAsync(serverIp, port); // Attempt Connection to Server
            Console.WriteLine("Connected! Please Enter in a Username: ");

            CancellationTokenSource cts = new CancellationTokenSource(); // Cancellation Token is used to track if the connection is live or not. When
            //... the while loop ends, the cts will cancel. This tells the async recieveMessages task to quit running!
            Task receiveTask = Task.Run(() => ReceiveMessages(client, cts.Token));

            while (client.Connected)
            {
                string? message = Console.ReadLine();

                if (string.IsNullOrEmpty(message))
                {
                    continue;
                }

                await SendLineAsync(client, message);
            }

            cts.Cancel();
            await receiveTask;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error Thrown: {ex.Message}");
        }
    }

    static async Task ReceiveMessages(TcpClient client, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && client.Connected) // uses the token and connection to determine if it should still block for respones
        {
            string? json = await ReadLineAsync(client, cancellationToken); // BLOCKS FOR RESPONSE
            if (string.IsNullOrWhiteSpace(json))
            {
                continue;
            }

            ServerStatusResponse? response = JsonSerializer.Deserialize<ServerStatusResponse>(json); // Responses are built into the object the server and client uses to communciate with each other
            Console.WriteLine();
            if (!string.IsNullOrWhiteSpace(response?.message))
            {
                Console.WriteLine(response.message);
            }

            if (!string.IsNullOrWhiteSpace(response?.output))
            {
                Console.WriteLine(response.output);
            }

            if (response?.waitingOnCommand == true && response.currentFolder != null)
            {
                PrintFolder(response.currentFolder);
            }

            if (response?.shouldClose == true)
            {
                break;
            }
        }
    }

    // Takes the Directory Listing provided from the Server, and prints out the folder neatly for the client to see all files
    static void PrintFolder(DirectoryListing folder)
    {
        Console.WriteLine();
        Console.WriteLine($"Folder: {folder.folderName}");

        if (!string.IsNullOrWhiteSpace(folder.parentFolderName))
        {
            Console.WriteLine($"Parent Folder: {folder.parentFolderName}");
        }

        Console.WriteLine("Index   Type     Name");
        Console.WriteLine("-----   ------   ----");

        int count = 0;
        foreach (string subFolder in folder.subFolders)
        {
            Console.WriteLine($"{count,-7} {"Folder",-8} {subFolder}");
            count++;
        }

        foreach (string subFile in folder.subFiles)
        {
            Console.WriteLine($"{count,-7} {"File",-8} {subFile}");
            count++;
        }

        if (count == 0)
        {
            Console.WriteLine("-       -        (empty)");
        }

        Console.WriteLine();
    }

    // Sends a message to the server 
    static async Task SendLineAsync(TcpClient client, string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message + Environment.NewLine);
        NetworkStream stream = client.GetStream();
        await stream.WriteAsync(data, 0, data.Length);
    }

    // Reads a line from the server
    static async Task<string?> ReadLineAsync(TcpClient client, CancellationToken cancellationToken)
    {
        NetworkStream stream = client.GetStream();
        List<byte> data = new List<byte>();
        byte[] buffer = new byte[1];

        while (!cancellationToken.IsCancellationRequested) // Ensures connection is still active. Similar to server, will read 1 byte at a time to look for newline to terminate it
        {
            int bytesRead = await stream.ReadAsync(buffer, 0, 1, cancellationToken); // BLOCKS FOR RESPONSE
            if (bytesRead <= 0)
            {
                return data.Count == 0 ? null : Encoding.UTF8.GetString(data.ToArray()).TrimEnd('\r'); // No bytes read, return the data found
            }

            if (buffer[0] == (byte)'\n') // Newline read, return the data found
            {
                return Encoding.UTF8.GetString(data.ToArray()).TrimEnd('\r');
            }

            data.Add(buffer[0]); // Each byte read, add it to the list of bytes
        }

        return null; 
    }
}
