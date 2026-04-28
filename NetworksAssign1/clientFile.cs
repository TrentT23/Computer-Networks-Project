using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using SharedModels;

class Program
{
    static async Task Main()
    {
        string serverIp = "192.168.1.136";
        int port = 8080;

        try
        {
            using TcpClient client = new TcpClient();
            Console.WriteLine($"Attempting to connect to {serverIp}:{port}");

            await client.ConnectAsync(serverIp, port);
            Console.WriteLine("Connected! Please Enter in a Username: ");

            CancellationTokenSource cts = new CancellationTokenSource();
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
        while (!cancellationToken.IsCancellationRequested && client.Connected)
        {
            string? json = await ReadLineAsync(client, cancellationToken);
            if (string.IsNullOrWhiteSpace(json))
            {
                continue;
            }

            ServerStatusResponse? response = JsonSerializer.Deserialize<ServerStatusResponse>(json);
            if (!string.IsNullOrWhiteSpace(response?.message))
            {
                Console.WriteLine(response.message);
            }

            if (!string.IsNullOrWhiteSpace(response?.output))
            {
                Console.WriteLine(response.output);
            }

            // CODEx change: render the server's current folder snapshot after each completed command.
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

    static async Task SendLineAsync(TcpClient client, string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message + Environment.NewLine);
        NetworkStream stream = client.GetStream();
        await stream.WriteAsync(data, 0, data.Length);
    }

    static async Task<string?> ReadLineAsync(TcpClient client, CancellationToken cancellationToken)
    {
        NetworkStream stream = client.GetStream();
        List<byte> data = new List<byte>();
        byte[] buffer = new byte[1];

        while (!cancellationToken.IsCancellationRequested)
        {
            int bytesRead = await stream.ReadAsync(buffer, 0, 1, cancellationToken);
            if (bytesRead <= 0)
            {
                return data.Count == 0 ? null : Encoding.UTF8.GetString(data.ToArray()).TrimEnd('\r');
            }

            if (buffer[0] == (byte)'\n')
            {
                return Encoding.UTF8.GetString(data.ToArray()).TrimEnd('\r');
            }

            data.Add(buffer[0]);
        }

        return null;
    }
}
