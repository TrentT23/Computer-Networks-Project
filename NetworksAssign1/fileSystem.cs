using System.IO;
using System;
using SharedModels;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace FileSystem
{
    // Directory Root controls the processing of client commands and executes the actions!
    public class DirectoryRoot
    {

        // -- ENUMS are used to better organize commands and operations instead of just plain integers -- //
        public enum FileOperations
        {
            READ,
            APPEND,
            OVERWRITE,
        }
        public enum FolderOperations
        {
            ENTER,
            EXIT,
        }
        public enum SharedOperations
        {
            CREATE,
            DELETE,
            MOVE,
            HELP,
            QUIT
        }

        public enum DirectoryType
        {
            FILE,
            FOLDER
        }

        private Folder currentFolderloaded = new Folder(); // IMPORTANT! -- EACH client has their own instance of currentFolderLoaded since they each
        // ...have their own DirectoryRoot

        public Folder CurrentFolder => currentFolderloaded;

        public static class FileSystemConfig
        {
            public static string Root = Path.Combine(AppContext.BaseDirectory, "ServerFiles"); // When server starts, if a 'ServerFiles' folder doesn't....
            // exist, then one is created. This is in the bin folder. Everything the clients do is in this file
        }

        // Takes the username from the client and creates their own root directory for the server. 
        public void InitializeUserName(string username)
        {
            string rootPath = Path.Combine(FileSystemConfig.Root, username);
            Directory.CreateDirectory(rootPath);
            currentFolderloaded = InitializeFolder(null, rootPath, username); // The parent file is marked as null so they cannot go backwards
        }
        // --------------------------------PARSE COMMAND HELPER METHOD --------------------------------------//
        // Parse Command will take the full Command, Parse it into multiple arguments, process each one, perform the operations specified, and send response back to client.
        public bool ParseCommand(string fullCommand, TcpClient client)
        {
            try
            {
                string[] parts = fullCommand.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 0 || string.IsNullOrWhiteSpace(parts[0]))
                {
                    SendCommandResponse(client, "Please enter a valid command.");
                    return false;
                }

                int arg1 = -1; 
                if (parts.Length > 1)
                {
                    int.TryParse(parts[1], out arg1); // If the second part is an int, then the user is passing an index value
                }

                if (Enum.TryParse<FileOperations>(parts[0].ToUpper(), out FileOperations fileOp)) // If fileOp is intialized, then the command is a FILE command and is in this block
                {
                    var findFile = currentFolderloaded.subFiles.FirstOrDefault(x => x.ReturnPlaceInParent() == arg1); // Attempt to grab a file with arg1 if successfully intialized

                    if (findFile == null)
                    {
                        SendCommandResponse(client, "Cannot read a Folder!");
                        return false;
                    }

                    switch (fileOp) // These commands only have 2 arguments, the command and the index
                    {
                        case FileOperations.READ:
                            FileMethods.ReadFile(this, findFile, client); // Print the Contents of a file
                            break;
                        case FileOperations.APPEND:
                            FileMethods.AppendFile(this, currentFolderloaded, findFile, client); // Add onto the Contents of a file
                            break;
                        case FileOperations.OVERWRITE: // Overwrite the Contents of a file
                            FileMethods.OverwriteFile(this, currentFolderloaded, findFile, client);
                            break;
                    }

                    return false;
                }
                else if (Enum.TryParse<FolderOperations>(parts[0].ToUpper(), out FolderOperations folderOp)) // If folderOp is intialized, then the command is a folder command and is in this block
                {
                    switch (folderOp) // These commands only have 2 arguments, the command and the index
                    {
                        case FolderOperations.ENTER:
                            var folder = currentFolderloaded.subFolders.FirstOrDefault(x => x.placeInParentFolder == arg1);
                            if (folder != null)
                            {
                                FolderMethods.EnterFolder(this, folder, client);
                            }
                            else
                            {
                                SendCommandResponse(client, "Could not find Folder to Enter!");
                            }
                            break;
                        case FolderOperations.EXIT:
                            FolderMethods.ExitFolder(this, currentFolderloaded, client);
                            break;
                    }

                    return false;
                }
                else if (Enum.TryParse<SharedOperations>(parts[0].ToUpper(), out SharedOperations sharedOp)) // A sharedOp can be a Fileop or FolderOp. In this case, we treat Arg1 as a string and see 
                // ... if it's type File or Folder!
                {
                    switch (sharedOp)
                    {
                        case SharedOperations.CREATE:
                        {
                            if (parts.Length < 2)
                            {
                                SendCommandResponse(client, "Please specify FILE or FOLDER.");
                                break;
                            }

                            string sarg1 = parts[1];
                            string sarg2 = ExtractCreateName(fullCommand, sarg1); // Get the name of the file to make
                            if (Enum.TryParse<DirectoryType>(sarg1.ToUpper(), out DirectoryType nType)) // Determine if its a file or folder
                            {
                                sarg2 = !string.IsNullOrEmpty(sarg2) ? sarg2 : string.Empty;

                                if (nType == DirectoryType.FILE)
                                {
                                    FileMethods.CreateFile(this, currentFolderloaded, sarg2, client); // Create a File
                                }
                                else if (nType == DirectoryType.FOLDER)
                                {
                                    FolderMethods.CreateFolder(this, currentFolderloaded, sarg2, client); // Create a Folder
                                }
                            }
                            else
                            {
                                SendCommandResponse(client, "Please specify FILE or FOLDER.");
                            }
                            break;
                        }
                        case SharedOperations.DELETE: // Delete Operation
                        {
                            if (arg1 != -1)
                            {
                                var subFile = currentFolderloaded.subFiles.FirstOrDefault(x => x.ReturnPlaceInParent() == arg1);
                                var subFolder = currentFolderloaded.subFolders.FirstOrDefault(x => x.placeInParentFolder == arg1);

                                if (subFile != null)
                                {
                                    FileMethods.DeleteFile(this, currentFolderloaded, subFile, client);
                                }
                                else if (subFolder != null)
                                {
                                    FolderMethods.DeleteFolder(this, subFolder, client);
                                }
                                else
                                {
                                    SendCommandResponse(client, "Please select a valid file or folder.");
                                }
                            }
                            else
                            {
                                SendCommandResponse(client, "Please select a valid file or folder.");
                            }
                            break;
                        }
                        case SharedOperations.QUIT: // Quit Program
                            SendCommandResponse(client, "Closing session.", shouldClose: true);
                            return true;

                        case SharedOperations.HELP: // Return a List of Commands
                            SendCommandResponse(client, "Command list:", MethodsToShare.GetCommandsText());
                            break;

                        case SharedOperations.MOVE: // Move a File or Folder
                        {
                            if (parts.Length < 4) // Not enough arguments to perform operation
                            {
                                SendCommandResponse(client, "Use MOVE FILE <item> <folder> or MOVE FOLDER <item> <folder>.");
                                break;
                            }

                            string sarg1 = parts[1];

                            if (Enum.TryParse<DirectoryType>(sarg1.ToUpper(), out DirectoryType nType))
                            {
                                int arg2 = int.TryParse(parts[2], out int temp1) ? temp1 : -1;
                                int arg3 = int.TryParse(parts[3], out int temp2) ? temp2 : -1;

                                if (nType == DirectoryType.FILE) // Moving a File type
                                {
                                    var findFile = currentFolderloaded.subFiles.FirstOrDefault(x => x.ReturnPlaceInParent() == arg2); // Find File
                                    var folderToPut = currentFolderloaded.subFolders.FirstOrDefault(x => x.placeInParentFolder == arg3); // Find Directory to Move File to
                                    FileMethods.MoveFile(this, currentFolderloaded, findFile, folderToPut, client);
                                }
                                else if (nType == DirectoryType.FOLDER) // Moving a Folder Type
                                {
                                    var findFolder = currentFolderloaded.subFolders.FirstOrDefault(x => x.placeInParentFolder == arg2); // Find Folder
                                    var folderToPut = currentFolderloaded.subFolders.FirstOrDefault(x => x.placeInParentFolder == arg3); // Find Directory to Move Folder to
                                    FolderMethods.MoveFolder(this, findFolder, folderToPut, client);
                                }
                            }
                            else
                            {
                                SendCommandResponse(client, "Please specify FILE or FOLDER.");
                            }
                            break;
                        }
                    }
                }
                else
                {
                    SendCommandResponse(client, "Unknown command.");
                }

                return false;
            }
            catch (Exception ex)
            {
                SendCommandResponse(client, $"Command failed: {ex.Message}");
                return false;
            }
        }

        // This helper method is used to set up and refresh folder objects for the rest of the program
        // This will look in the server computer for the files in the file path, and take all that information to create the Folder Objects
        public static Folder InitializeFolder(Folder? parent, string pathToFolder, string baseName)
        {
            Folder baseFolder = new Folder() { parentFolder = parent, folderName = baseName, pathToFolder = pathToFolder };
            string root = pathToFolder; // specifies the root path of a folder

            var rootFiles = new List<string>();
            var rootFolders = new List<string>();

            List<Folder> allFolders = new List<Folder>();
            List<IFile> allFiles = new List<IFile>();

            rootFiles.AddRange(Directory.GetFiles(root));  // Get all Files in path, if they exist
            rootFolders.AddRange(Directory.GetDirectories(root)); // Get all Folders in path, if they exist

            int refCount = 0;
            for (int i = 0; i < rootFolders.Count(); i++) // For every Folder, get the name, list the path, list their index in their parent directory
            {
                allFolders.Add(new Folder()
                {
                    folderName = Path.GetFileName(rootFolders[i]),
                    pathToFolder = rootFolders[i],
                    parentFolder = baseFolder,
                    placeInParentFolder = refCount
                });
                refCount++;
            }
            for (int i = 0; i < rootFiles.Count(); i++) // Similar to Folders, store their name, and track their index in parent folder
            {
                allFiles.Add(new TextFile()
                {
                    fileName = Path.GetFileName(rootFiles[i]),
                    placeInParentFolder = refCount
                });
                refCount++;
            }

            baseFolder.subFolders = allFolders;
            baseFolder.subFiles = allFiles;
            return baseFolder; 
        }

        // Used to get the specific name for a File or Folder when using the Create Command
        private static string ExtractCreateName(string fullCommand, string directoryType)
        {
            string prefix = $"CREATE {directoryType}";
            if (!fullCommand.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            string remainder = fullCommand.Substring(prefix.Length).Trim(); // Grab the name seperate from arg0 and arg1
            return remainder.Trim().Trim('"'); // Trim white space
        }

        // PromptClientForInput sends a message for the client, as well as folder information needed for them
        public string? PromptClientForInput(TcpClient client, string message)
        {
            SendResponse(client, BuildResponse(message, null, false, false));
            return ReadClientLine(client);
        }


        /*
        Send Response is a helper method that will break a ServerResponse object into bytes to send BACK to the client, so the client can get responses. 
        The Client will Serialize and Process this object!
        */
        public static void SendResponse(TcpClient client, ServerStatusResponse response)
        {
            string json = JsonSerializer.Serialize(response) + Environment.NewLine;
            byte[] responseBytes = Encoding.UTF8.GetBytes(json);
            NetworkStream stream = client.GetStream();
            stream.Write(responseBytes, 0, responseBytes.Length);
        }

        /*
            ReadClient Line is a helper method that will block and wait for a client response. Could be for commands or entering a username
        */
        public static string? ReadClientLine(TcpClient client)
        {
            NetworkStream stream = client.GetStream(); // Blocks the thread
            List<byte> data = new List<byte>();
            byte[] buffer = new byte[1024];

            while (true) // Each iteration processes 1 byte at a time. Since We are sending small messages this is no problem, and allows to easily
            // check for newline. 
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead <= 0) // No Bytes recieved, terminate
                {
                    return data.Count == 0 ? null : Encoding.UTF8.GetString(data.ToArray()).TrimEnd('\r');
                }

                if (buffer[0] == (byte)'\n')  // Newline was read, terminate
                {
                    return Encoding.UTF8.GetString(data.ToArray()).TrimEnd('\r'); // Read bytes as a string
                }

                data.Add(buffer[0]); // Add each byte to the data byte list
            }
        }


        // Creates a DirectoryListing (small folder) just meant for the Client Reading
        public static DirectoryListing BuildListing(Folder folder)
        {
            return new DirectoryListing()
            {
                folderName = folder.folderName,
                parentFolderName = folder.parentFolder?.folderName,
                subFolders = folder.subFolders.Select(x => x.folderName).ToList(),
                subFiles = folder.subFiles.Select(x => x.ReturnFileName()).ToList()
            };
        }


        private void RefreshCurrentFolder()
        {
            if (string.IsNullOrWhiteSpace(currentFolderloaded.pathToFolder) || !Directory.Exists(currentFolderloaded.pathToFolder))
            {
                return;
            }
            // Purposely will update the currentFolderLoaded with itself to update for any file changes
            currentFolderloaded = InitializeFolder(
                currentFolderloaded.parentFolder,
                currentFolderloaded.pathToFolder,
                currentFolderloaded.folderName);
        }

        /*
        Build Reponse uses the ServerStatusResponse class to store all necessary information for the client!
        */
        private ServerStatusResponse BuildResponse(string message, string? output, bool waitingOnCommand, bool shouldClose)
        {
            RefreshCurrentFolder(); // Ensures the Folder is up-to-date with any changes just performed before

            return new ServerStatusResponse()
            {
                currentStatus = STATUS.INDIRECTORY,
                message = message,
                output = output,
                waitingOnCommand = waitingOnCommand,
                shouldClose = shouldClose,
                currentFolder = BuildListing(currentFolderloaded)
            };
        }

        // Sends a Response Back to the client. 
        private void SendCommandResponse(TcpClient client, string message, string? output = null, bool shouldClose = false)
        {
            SendResponse(client, BuildResponse(message, output, true, shouldClose));
        }


        // ------------------------- STATIC CLASSES CONTAINING FILE AND FOLDER OPERATION METHODS ---------------------------------- //
        // For all of these operations, they have a similar structure. Read command input, perform the operation, send back output, and return to the Main Loop. 
        // Some operations require more user input, for these the client is asked for more information (usually for creating or writing to files/folders).


        /*
            FileMethods Hold all of the methods used for File Operations. 
        */
        public static class FileMethods
        {
            public static void ReadFile(DirectoryRoot root, IFile file, TcpClient client)
            {
                var fullPath = Path.Combine(root.currentFolderloaded.pathToFolder, file.ReturnFileName());
                string fileContents = File.ReadAllText(fullPath);

                root.SendCommandResponse(client, $"Read {file.ReturnFileName()}.\n\nContents: ", fileContents);
            }

            public static void AppendFile(DirectoryRoot root, Folder baseFolder, IFile file, TcpClient client)
            {
                string path = Path.Combine(baseFolder.pathToFolder, file.ReturnFileName());

                if (File.Exists(path))
                {
                    string? input = root.PromptClientForInput(client, $"Please Enter Text you would like to append to {file.ReturnFileName()}: ");
                    File.AppendAllText(path, input ?? string.Empty);
                    root.SendCommandResponse(client, $"Appended text to {file.ReturnFileName()}.");
                }
                else
                {
                    root.SendCommandResponse(client, "File could not be found.");
                }
            }

            public static void OverwriteFile(DirectoryRoot root, Folder baseFolder, IFile file, TcpClient client)
            {
                string path = Path.Combine(baseFolder.pathToFolder, file.ReturnFileName());

                if (File.Exists(path))
                {
                    string? input = root.PromptClientForInput(client, $"Please Enter Text to Overwrite File {file.ReturnFileName()}: ");
                    File.WriteAllText(path, string.IsNullOrEmpty(input) ? "" : input + Environment.NewLine);
                    root.SendCommandResponse(client, $"Overwrote {file.ReturnFileName()}.");
                }
                else
                {
                    root.SendCommandResponse(client, "File could not be found.");
                }
            }

            public static void CreateFile(DirectoryRoot root, Folder baseFolder, string name, TcpClient client)
            {
                string fileName = !string.IsNullOrEmpty(name) ? name : $"newfile{baseFolder.subFiles.Count() + 1}";
                string path = Path.Combine(baseFolder.pathToFolder, fileName);

                if (File.Exists(path))
                {
                    root.SendCommandResponse(client, "File Already Exists in this Location!");
                }
                else
                {
                    string? input = root.PromptClientForInput(client, "Enter New File Text Input: ");
                    File.WriteAllText(path, string.IsNullOrEmpty(input) ? "" : input + Environment.NewLine);
                    root.SendCommandResponse(client, $"Created file {fileName}.");
                }
            }

            public static void MoveFile(DirectoryRoot root, Folder baseFolder, IFile? file, Folder? folderToMoveTo, TcpClient client)
            {
                if (file == null || folderToMoveTo == null)
                {
                    root.SendCommandResponse(client, "Please select a valid file and destination folder.");
                    return;
                }

                string filePath = Path.Combine(baseFolder.pathToFolder, file.ReturnFileName());
                string folderPath = Path.Combine(folderToMoveTo.pathToFolder, file.ReturnFileName());

                if (Directory.Exists(folderToMoveTo.pathToFolder))
                {
                    File.Move(filePath, folderPath);
                    root.SendCommandResponse(client, $"Moved file {file.ReturnFileName()} to {folderToMoveTo.folderName}.");
                }
                else
                {
                    root.SendCommandResponse(client, "Please select a valid destination folder.");
                }
            }

            public static void DeleteFile(DirectoryRoot root, Folder baseFolder, IFile fileToDelete, TcpClient client)
            {
                string path = Path.Combine(baseFolder.pathToFolder, fileToDelete.ReturnFileName());

                if (File.Exists(path))
                {
                    string? input = root.PromptClientForInput(client, $"Are you sure you want to delete {fileToDelete.ReturnFileName()}?\nYES or NO");

                    if (!string.IsNullOrEmpty(input) && input.ToUpper() == "YES")
                    {
                        File.Delete(path);
                        root.SendCommandResponse(client, $"Deleted file {fileToDelete.ReturnFileName()}.");
                    }
                    else
                    {
                        root.SendCommandResponse(client, $"Did not delete {fileToDelete.ReturnFileName()}.");
                    }
                }
                else
                {
                    root.SendCommandResponse(client, "File could not be found.");
                }
            }
        }

        /*
            Hold all of the methods for FolderOperations!
        */
        public static class FolderMethods 
        {
            public static void EnterFolder(DirectoryRoot root, Folder folderToEnter, TcpClient client)
            {
                var path = folderToEnter.pathToFolder;
                if (Directory.Exists(path))
                {
                    root.currentFolderloaded = InitializeFolder(folderToEnter.parentFolder, folderToEnter.pathToFolder, folderToEnter.folderName);
                    root.SendCommandResponse(client, $"Entered folder {folderToEnter.folderName}.");
                }
                else
                {
                    root.SendCommandResponse(client, "Could not find Folder to Enter!");
                }
            }

            public static void ExitFolder(DirectoryRoot root, Folder baseFolder, TcpClient client)
            {
                if (baseFolder.parentFolder == null)
                {
                    root.SendCommandResponse(client, "Already at the root folder.");
                    return;
                }

                var path = baseFolder.parentFolder.pathToFolder;

                if (Directory.Exists(path))
                {
                    root.currentFolderloaded = InitializeFolder(baseFolder.parentFolder.parentFolder, baseFolder.parentFolder.pathToFolder, baseFolder.parentFolder.folderName);
                    root.SendCommandResponse(client, $"Exited to folder {baseFolder.parentFolder.folderName}.");
                }
                else
                {
                    root.SendCommandResponse(client, "Could not exit to parent folder.");
                }
            }

            public static void DeleteFolder(DirectoryRoot root, Folder folderToDelete, TcpClient client)
            {
                string path = folderToDelete.pathToFolder;

                if (Directory.Exists(path))
                {
                    string? input = root.PromptClientForInput(client, $"Are you sure you want to delete {folderToDelete.folderName}? This includes all files in the folder!!\nYES or NO");

                    if (!string.IsNullOrEmpty(input) && input.ToUpper() == "YES")
                    {
                        Directory.Delete(path, true);
                        root.SendCommandResponse(client, $"Deleted folder {folderToDelete.folderName}.");
                    }
                    else
                    {
                        root.SendCommandResponse(client, $"Did not delete {folderToDelete.folderName}.");
                    }
                }
                else
                {
                    root.SendCommandResponse(client, "Folder could not be found.");
                }
            }

            public static void CreateFolder(DirectoryRoot root, Folder baseFolder, string nameOfFolder, TcpClient client)
            {
                string folderName = !string.IsNullOrEmpty(nameOfFolder) ? nameOfFolder : $"newfile{baseFolder.subFiles.Count() + 1}";
                string path = Path.Combine(baseFolder.pathToFolder, folderName);

                if (Directory.Exists(path))
                {
                    root.SendCommandResponse(client, "Folder Already Exists in this Folder!");
                }
                else
                {
                    Directory.CreateDirectory(path);
                    root.SendCommandResponse(client, $"Created folder {folderName}.");
                }
            }

            public static void MoveFolder(DirectoryRoot root, Folder? folderBeingMoved, Folder? folderToMoveTo, TcpClient client)
            {
                if (folderBeingMoved == null || folderToMoveTo == null)
                {
                    root.SendCommandResponse(client, "Please select a valid folder and destination folder.");
                    return;
                }

                string filePath = folderBeingMoved.pathToFolder;
                string folderPath = Path.Combine(folderToMoveTo.pathToFolder, folderBeingMoved.folderName);

                if (Directory.Exists(folderToMoveTo.pathToFolder))
                {
                    Directory.Move(filePath, folderPath);
                    root.SendCommandResponse(client, $"Moved folder {folderBeingMoved.folderName} to {folderToMoveTo.folderName}.");
                }
                else
                {
                    root.SendCommandResponse(client, "Please select a valid destination folder.");
                }
            }
        }
    }
}
