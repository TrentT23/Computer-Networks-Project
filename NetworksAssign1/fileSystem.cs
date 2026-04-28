using ServerFile;
using System.IO;
using System;
using SharedModels;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace FileSystem
{
    public class DirectoryRoot
    {
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

        static Folder currentFolderloaded = new Folder();

        public static class FileSystemConfig
        {
            public static string Root = Path.Combine(AppContext.BaseDirectory, "ServerFiles");
        }

        public void InitializeUserName(string username)
        {
            string rootPath = Path.Combine(FileSystemConfig.Root, username);
            Directory.CreateDirectory(rootPath);
            currentFolderloaded = InitializeFolder(null, rootPath, username);
        }

        // CODEx change: kept your original bool-returning flow, but responses now go back to the client.
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
                    int.TryParse(parts[1], out arg1);
                }

                if (Enum.TryParse<FileOperations>(parts[0].ToUpper(), out FileOperations fileOp))
                {
                    var findFile = currentFolderloaded.subFiles.FirstOrDefault(x => x.ReturnPlaceInParent() == arg1);

                    if (findFile == null)
                    {
                        SendCommandResponse(client, "Cannot read a Folder!");
                        return false;
                    }

                    switch (fileOp)
                    {
                        case FileOperations.READ:
                            FileMethods.ReadFile(findFile, client);
                            break;
                        case FileOperations.APPEND:
                            FileMethods.AppendFile(currentFolderloaded, findFile, client);
                            break;
                        case FileOperations.OVERWRITE:
                            FileMethods.OverwriteFile(currentFolderloaded, findFile, client);
                            break;
                    }

                    return false;
                }
                else if (Enum.TryParse<FolderOperations>(parts[0].ToUpper(), out FolderOperations folderOp))
                {
                    switch (folderOp)
                    {
                        case FolderOperations.ENTER:
                            var folder = currentFolderloaded.subFolders.FirstOrDefault(x => x.placeInParentFolder == arg1);
                            if (folder != null)
                            {
                                FolderMethods.EnterFolder(folder, client);
                            }
                            else
                            {
                                SendCommandResponse(client, "Could not find Folder to Enter!");
                            }
                            break;
                        case FolderOperations.EXIT:
                            FolderMethods.ExitFolder(currentFolderloaded, client);
                            break;
                    }

                    return false;
                }
                else if (Enum.TryParse<SharedOperations>(parts[0].ToUpper(), out SharedOperations sharedOp))
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
                            string sarg2 = ExtractCreateName(fullCommand, sarg1);
                            if (Enum.TryParse<DirectoryType>(sarg1.ToUpper(), out DirectoryType nType))
                            {
                                sarg2 = !string.IsNullOrEmpty(sarg2) ? sarg2 : string.Empty;

                                if (nType == DirectoryType.FILE)
                                {
                                    FileMethods.CreateFile(currentFolderloaded, sarg2, client);
                                }
                                else if (nType == DirectoryType.FOLDER)
                                {
                                    FolderMethods.CreateFolder(currentFolderloaded, sarg2, client);
                                }
                            }
                            else
                            {
                                SendCommandResponse(client, "Please specify FILE or FOLDER.");
                            }
                            break;
                        }
                        case SharedOperations.DELETE:
                        {
                            if (arg1 != -1)
                            {
                                var subFile = currentFolderloaded.subFiles.FirstOrDefault(x => x.ReturnPlaceInParent() == arg1);
                                var subFolder = currentFolderloaded.subFolders.FirstOrDefault(x => x.placeInParentFolder == arg1);

                                if (subFile != null)
                                {
                                    FileMethods.DeleteFile(currentFolderloaded, subFile, client);
                                }
                                else if (subFolder != null)
                                {
                                    FolderMethods.DeleteFolder(subFolder, client);
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
                        case SharedOperations.QUIT:
                            SendCommandResponse(client, "Closing session.", shouldClose: true);
                            return true;

                        case SharedOperations.HELP:
                            SendCommandResponse(client, "Command list:", MethodsToShare.GetCommandsText());
                            break;

                        case SharedOperations.MOVE:
                        {
                            if (parts.Length < 4)
                            {
                                SendCommandResponse(client, "Use MOVE FILE <item> <folder> or MOVE FOLDER <item> <folder>.");
                                break;
                            }

                            string sarg1 = parts[1];

                            if (Enum.TryParse<DirectoryType>(sarg1.ToUpper(), out DirectoryType nType))
                            {
                                int arg2 = int.TryParse(parts[2], out int temp1) ? temp1 : -1;
                                int arg3 = int.TryParse(parts[3], out int temp2) ? temp2 : -1;

                                if (nType == DirectoryType.FILE)
                                {
                                    var findFile = currentFolderloaded.subFiles.FirstOrDefault(x => x.ReturnPlaceInParent() == arg2);
                                    var folderToPut = currentFolderloaded.subFolders.FirstOrDefault(x => x.placeInParentFolder == arg3);
                                    FileMethods.MoveFile(currentFolderloaded, findFile, folderToPut, client);
                                }
                                else if (nType == DirectoryType.FOLDER)
                                {
                                    var findFolder = currentFolderloaded.subFolders.FirstOrDefault(x => x.placeInParentFolder == arg2);
                                    var folderToPut = currentFolderloaded.subFolders.FirstOrDefault(x => x.placeInParentFolder == arg3);
                                    FolderMethods.MoveFolder(findFolder, folderToPut, client);
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

        public static Folder InitializeFolder(Folder? parent, string pathToFolder, string baseName)
        {
            Folder baseFolder = new Folder() { parentFolder = parent, folderName = baseName, pathToFolder = pathToFolder };
            string root = pathToFolder;

            var rootFiles = new List<string>();
            var rootFolders = new List<string>();

            List<Folder> allFolders = new List<Folder>();
            List<IFile> allFiles = new List<IFile>();

            rootFiles.AddRange(Directory.GetFiles(root));
            rootFolders.AddRange(Directory.GetDirectories(root));

            int refCount = 0;
            for (int i = 0; i < rootFolders.Count(); i++)
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
            for (int i = 0; i < rootFiles.Count(); i++)
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
            currentFolderloaded = baseFolder;
            return baseFolder;
        }

        private static string ExtractCreateName(string fullCommand, string directoryType)
        {
            string prefix = $"CREATE {directoryType}";
            if (!fullCommand.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            string remainder = fullCommand.Substring(prefix.Length).Trim();
            return remainder.Trim().Trim('"');
        }

        public static string? PromptClientForInput(TcpClient client, string message)
        {
            SendResponse(client, BuildResponse(message, null, false, false));
            return ReadClientLine(client);
        }

        public static void SendServerMessage(TcpClient client, string message, bool waitingOnCommand)
        {
            SendResponse(client, BuildResponse(message, null, waitingOnCommand, false));
        }

        public static void SendResponse(TcpClient client, ServerStatusResponse response)
        {
            string json = JsonSerializer.Serialize(response) + Environment.NewLine;
            byte[] responseBytes = Encoding.UTF8.GetBytes(json);
            NetworkStream stream = client.GetStream();
            stream.Write(responseBytes, 0, responseBytes.Length);
        }

        public static string? ReadClientLine(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            List<byte> data = new List<byte>();
            byte[] buffer = new byte[1];

            while (true)
            {
                int bytesRead = stream.Read(buffer, 0, 1);
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
        }

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

        private static void RefreshCurrentFolder()
        {
            if (string.IsNullOrWhiteSpace(currentFolderloaded.pathToFolder) || !Directory.Exists(currentFolderloaded.pathToFolder))
            {
                return;
            }

            currentFolderloaded = InitializeFolder(
                currentFolderloaded.parentFolder,
                currentFolderloaded.pathToFolder,
                currentFolderloaded.folderName);
        }

        private static ServerStatusResponse BuildResponse(string message, string? output, bool waitingOnCommand, bool shouldClose)
        {
            RefreshCurrentFolder();

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

        private static void SendCommandResponse(TcpClient client, string message, string? output = null, bool shouldClose = false)
        {
            SendResponse(client, BuildResponse(message, output, true, shouldClose));
        }

        public static class FileMethods
        {
            public static void ReadFile(IFile file, TcpClient client)
            {
                var fullPath = Path.Combine(currentFolderloaded.pathToFolder, file.ReturnFileName());
                string fileContents = File.ReadAllText(fullPath);

                SendCommandResponse(client, $"Read {file.ReturnFileName()}.", fileContents);
            }

            public static void AppendFile(Folder baseFolder, IFile file, TcpClient client)
            {
                string path = Path.Combine(baseFolder.pathToFolder, file.ReturnFileName());

                if (File.Exists(path))
                {
                    string? input = PromptClientForInput(client, $"Please Enter Text you would like to append to {file.ReturnFileName()}: ");
                    File.AppendAllText(path, input ?? string.Empty);
                    SendCommandResponse(client, $"Appended text to {file.ReturnFileName()}.");
                }
                else
                {
                    SendCommandResponse(client, "File could not be found.");
                }
            }

            public static void OverwriteFile(Folder baseFolder, IFile file, TcpClient client)
            {
                string path = Path.Combine(baseFolder.pathToFolder, file.ReturnFileName());

                if (File.Exists(path))
                {
                    string? input = PromptClientForInput(client, $"Please Enter Text to Overwrite File {file.ReturnFileName()}: ");
                    File.WriteAllText(path, string.IsNullOrEmpty(input) ? "" : input + Environment.NewLine);
                    SendCommandResponse(client, $"Overwrote {file.ReturnFileName()}.");
                }
                else
                {
                    SendCommandResponse(client, "File could not be found.");
                }
            }

            public static void CreateFile(Folder baseFolder, string name, TcpClient client)
            {
                string fileName = !string.IsNullOrEmpty(name) ? name : $"newfile{baseFolder.subFiles.Count() + 1}";
                string path = Path.Combine(baseFolder.pathToFolder, fileName);

                if (File.Exists(path))
                {
                    SendCommandResponse(client, "File Already Exists in this Location!");
                }
                else
                {
                    string? input = PromptClientForInput(client, "Enter New File Text Input: ");
                    File.WriteAllText(path, string.IsNullOrEmpty(input) ? "" : input + Environment.NewLine);
                    SendCommandResponse(client, $"Created file {fileName}.");
                }
            }

            public static void MoveFile(Folder baseFolder, IFile? file, Folder? folderToMoveTo, TcpClient client)
            {
                if (file == null || folderToMoveTo == null)
                {
                    SendCommandResponse(client, "Please select a valid file and destination folder.");
                    return;
                }

                string filePath = Path.Combine(baseFolder.pathToFolder, file.ReturnFileName());
                string folderPath = Path.Combine(folderToMoveTo.pathToFolder, file.ReturnFileName());

                if (Directory.Exists(folderToMoveTo.pathToFolder))
                {
                    File.Move(filePath, folderPath);
                    SendCommandResponse(client, $"Moved file {file.ReturnFileName()} to {folderToMoveTo.folderName}.");
                }
                else
                {
                    SendCommandResponse(client, "Please select a valid destination folder.");
                }
            }

            public static void DeleteFile(Folder baseFolder, IFile fileToDelete, TcpClient client)
            {
                string path = Path.Combine(baseFolder.pathToFolder, fileToDelete.ReturnFileName());

                if (File.Exists(path))
                {
                    string? input = PromptClientForInput(client, $"Are you sure you want to delete {fileToDelete.ReturnFileName()}?\nYES or NO");

                    if (!string.IsNullOrEmpty(input) && input.ToUpper() == "YES")
                    {
                        File.Delete(path);
                        SendCommandResponse(client, $"Deleted file {fileToDelete.ReturnFileName()}.");
                    }
                    else
                    {
                        SendCommandResponse(client, $"Did not delete {fileToDelete.ReturnFileName()}.");
                    }
                }
                else
                {
                    SendCommandResponse(client, "File could not be found.");
                }
            }
        }

        public static class FolderMethods
        {
            public static void EnterFolder(Folder folderToEnter, TcpClient client)
            {
                var path = folderToEnter.pathToFolder;
                if (Directory.Exists(path))
                {
                    currentFolderloaded = InitializeFolder(folderToEnter.parentFolder, folderToEnter.pathToFolder, folderToEnter.folderName);
                    SendCommandResponse(client, $"Entered folder {folderToEnter.folderName}.");
                }
                else
                {
                    SendCommandResponse(client, "Could not find Folder to Enter!");
                }
            }

            public static void ExitFolder(Folder baseFolder, TcpClient client)
            {
                if (baseFolder.parentFolder == null)
                {
                    SendCommandResponse(client, "Already at the root folder.");
                    return;
                }

                var path = baseFolder.parentFolder.pathToFolder;

                if (Directory.Exists(path))
                {
                    currentFolderloaded = InitializeFolder(baseFolder.parentFolder.parentFolder, baseFolder.parentFolder.pathToFolder, baseFolder.parentFolder.folderName);
                    SendCommandResponse(client, $"Exited to folder {baseFolder.parentFolder.folderName}.");
                }
                else
                {
                    SendCommandResponse(client, "Could not exit to parent folder.");
                }
            }

            public static void DeleteFolder(Folder folderToDelete, TcpClient client)
            {
                string path = folderToDelete.pathToFolder;

                if (Directory.Exists(path))
                {
                    string? input = PromptClientForInput(client, $"Are you sure you want to delete {folderToDelete.folderName}? This includes all files in the folder!!\nYES or NO");

                    if (!string.IsNullOrEmpty(input) && input.ToUpper() == "YES")
                    {
                        Directory.Delete(path, true);
                        SendCommandResponse(client, $"Deleted folder {folderToDelete.folderName}.");
                    }
                    else
                    {
                        SendCommandResponse(client, $"Did not delete {folderToDelete.folderName}.");
                    }
                }
                else
                {
                    SendCommandResponse(client, "Folder could not be found.");
                }
            }

            public static void CreateFolder(Folder baseFolder, string nameOfFolder, TcpClient client)
            {
                string folderName = !string.IsNullOrEmpty(nameOfFolder) ? nameOfFolder : $"newfile{baseFolder.subFiles.Count() + 1}";
                string path = Path.Combine(baseFolder.pathToFolder, folderName);

                if (Directory.Exists(path))
                {
                    SendCommandResponse(client, "Folder Already Exists in this Folder!");
                }
                else
                {
                    Directory.CreateDirectory(path);
                    SendCommandResponse(client, $"Created folder {folderName}.");
                }
            }

            public static void MoveFolder(Folder? folderBeingMoved, Folder? folderToMoveTo, TcpClient client)
            {
                if (folderBeingMoved == null || folderToMoveTo == null)
                {
                    SendCommandResponse(client, "Please select a valid folder and destination folder.");
                    return;
                }

                string filePath = folderBeingMoved.pathToFolder;
                string folderPath = Path.Combine(folderToMoveTo.pathToFolder, folderBeingMoved.folderName);

                if (Directory.Exists(folderToMoveTo.pathToFolder))
                {
                    Directory.Move(filePath, folderPath);
                    SendCommandResponse(client, $"Moved folder {folderBeingMoved.folderName} to {folderToMoveTo.folderName}.");
                }
                else
                {
                    SendCommandResponse(client, "Please select a valid destination folder.");
                }
            }
        }
    }
}
