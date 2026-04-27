using ServerFile;
using System.IO;
using System;

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
            QUIT
        }

        public enum DirectoryType
        {
            FILE,
            FOLDER
        }
        static Folder currentFolderloaded = new Folder();
        public interface IFile
        {
            string ReturnFileName();
            int ReturnPlaceInParent();

        }
        public class Folder
        {
            public Folder? parentFolder { get; set; }
            public int placeInParentFolder { get; set; }

            public string folderName { get; set; } = string.Empty;
            public string pathToFolder { get; set; } = string.Empty;
            public List<Folder> subFolders { get; set; } = new();
            public List<IFile> subFiles { get; set; } = new();



            public void ListFolderContents()
            {
                WriteCenteredHeader($"Folder: {folderName}");
                if (!string.IsNullOrEmpty(parentFolder?.folderName))
                {
                    WriteCenteredHeader($"Parent Folder: {parentFolder.folderName}");
                }

                string underline = new string('-', 44);
                int consoleWidth = GetSafeConsoleWidth();
                int underlinePadding = Math.Max(0, (consoleWidth - underline.Length) / 2);
                Console.WriteLine(underline.PadLeft(underlinePadding + underline.Length));
                Console.WriteLine(FormatListingRow("Index", "Type", "Name"));
                Console.WriteLine(FormatListingRow("-----", "------", "----"));


                int count = 0;
                for (int i = 0; i < subFolders.Count(); i++)
                {
                    Console.WriteLine(FormatListingRow(count.ToString(), "Folder", subFolders[i].folderName));
                    subFolders[i].placeInParentFolder = count;
                    count++;
                }
                for (int i = 0; i < subFiles.Count(); i++)
                {
                    Console.WriteLine(FormatListingRow(count.ToString(), "File", subFiles[i].ReturnFileName()));
                    var tFile = (TextFile)subFiles[i];
                    tFile.placeInParentFolder = count;
                    count++;
                }

                if (count == 0)
                {
                    Console.WriteLine(FormatListingRow("-", "-", "(empty)"));
                }

            }


        }
        public class TextFile : IFile
        {
            public string fileName { get; set; } = string.Empty;
            public string ReturnFileName() => fileName;
            public int placeInParentFolder { get; set; }
            public int ReturnPlaceInParent() => placeInParentFolder;
        }
        static void ListCommands()
        {
            Console.WriteLine();
            WriteCenteredHeader("Available Commands");
            Console.WriteLine(new string('-', 78));
            Console.WriteLine(FormatCommandRow("Category", "Command", "What It Does"));
            Console.WriteLine(FormatCommandRow("--------", "-------", "------------"));

            Console.WriteLine(FormatCommandRow("File", "READ <index>", "Show the contents of a file."));
            Console.WriteLine(FormatCommandRow("File", "APPEND <index>", "Add text to the end of a file."));
            Console.WriteLine(FormatCommandRow("File", "OVERWRITE <index>", "Replace all text inside a file."));
            Console.WriteLine(FormatCommandRow("Create", "CREATE FILE <name>", "Create a new file, then enter its text."));
            Console.WriteLine(FormatCommandRow("Create", "CREATE FOLDER <name>", "Create a new folder in the current folder."));
            Console.WriteLine(FormatCommandRow("Delete", "DELETE <index>", "Delete the selected file or folder."));
            Console.WriteLine(FormatCommandRow("Move", "MOVE FILE <item> <folder>", "Move a file into a folder."));
            Console.WriteLine(FormatCommandRow("Move", "MOVE FOLDER <item> <folder>", "Move a folder into another folder."));
            Console.WriteLine(FormatCommandRow("Folder", "ENTER <index>", "Open the selected folder."));
            Console.WriteLine(FormatCommandRow("Folder", "EXIT", "Go to the parent folder."));
            Console.WriteLine(FormatCommandRow("System", "QUIT", "Close the program."));
            Console.WriteLine();
        }
        public static class FileSystemConfig
        {
            public static string Root = Path.Combine(AppContext.BaseDirectory, "ServerFiles");
        }
        public static void MainLoop(Folder baseFolder)
        {
            currentFolderloaded = baseFolder;
            bool endLoop = false;

            while (endLoop == false)
            {
                currentFolderloaded = InitializeFolder(currentFolderloaded.parentFolder, currentFolderloaded.pathToFolder, currentFolderloaded.folderName);
                currentFolderloaded.ListFolderContents();

                ListCommands();

                string? command = Console.ReadLine();
                Console.Clear();


                if (!string.IsNullOrEmpty(command))
                {
                    bool loop = ParseCommand(command);
                    endLoop = loop;
                }
            }
        }

        static bool ParseCommand(string fullCommand)
        {
            try
            {
                string[] parts = fullCommand.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length > 0 && !string.IsNullOrEmpty(parts[0]))
                {
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
                            WriteCenteredHeader("Cannot read a Folder!");
                        }
                        else
                        {
                            switch (fileOp)
                            {
                                case FileOperations.READ:
                                    FileMethods.ReadFile(findFile);
                                    break;
                                case FileOperations.APPEND:
                                    FileMethods.AppendFile(currentFolderloaded, findFile);
                                    break;
                                case FileOperations.OVERWRITE:
                                    FileMethods.OverwriteFile(currentFolderloaded, findFile);
                                    break;
                            }
                        }
                    }
                    else if (Enum.TryParse<FolderOperations>(parts[0].ToUpper(), out FolderOperations folderOp))
                    {
                        switch (folderOp)
                        {
                            case FolderOperations.ENTER:
                                var folder = currentFolderloaded.subFolders.FirstOrDefault(x => x.placeInParentFolder == arg1);
                                if (folder != null)
                                {
                                    FolderMethods.EnterFolder(folder);
                                    break;
                                }
                                Console.WriteLine("Could not find Folder to Enter!");
                                break;
                            case FolderOperations.EXIT:
                                FolderMethods.ExitFolder(currentFolderloaded);
                                break;
                        }
                    }
                    else if (Enum.TryParse<SharedOperations>(parts[0].ToUpper(), out SharedOperations sharedOp))
                    {
                        switch (sharedOp)
                        {
                            case SharedOperations.CREATE:
                                {
                                    if (parts.Length < 2)
                                    {
                                        Console.WriteLine("Please specify FILE or FOLDER.");
                                        break;
                                    }

                                    string sarg1 = parts[1];
                                    string sarg2 = ExtractCreateName(fullCommand, sarg1);
                                    if (Enum.TryParse<DirectoryType>(sarg1.ToUpper(), out DirectoryType nType))
                                    {
                                        sarg2 = !string.IsNullOrEmpty(sarg2) ? sarg2 : string.Empty;

                                        if (nType == DirectoryType.FILE)
                                        {
                                            FileMethods.CreateFile(currentFolderloaded, sarg2);
                                        }
                                        else if (nType == DirectoryType.FOLDER)
                                        {
                                            FolderMethods.CreateFolder(currentFolderloaded, sarg2);
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Please specify FILE or FOLDER.");
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
                                            FileMethods.DeleteFile(currentFolderloaded, subFile);

                                        }
                                        else if (subFolder != null)
                                        {
                                            FolderMethods.DeleteFolder(subFolder);

                                        }
                                    }
                                    break;
                                }
                            case SharedOperations.QUIT: // returns a true boolean to indicate program exit
                                return true;

                            case SharedOperations.MOVE:
                                {
                                    if (parts.Length < 4)
                                    {
                                        Console.WriteLine("Use MOVE FILE <item> <folder> or MOVE FOLDER <item> <folder>.");
                                        break;
                                    }

                                    string sarg1 = parts[1];

                                    if (Enum.TryParse<DirectoryType>(sarg1.ToUpper(), out DirectoryType nType))
                                    {
                                        int arg2 = int.TryParse(parts[2], out int temp1) ? temp1 : temp1 = -1;
                                        int arg3 = int.TryParse(parts[3], out int temp2) ? temp2 : temp2 = -1;

                                        if (nType == DirectoryType.FILE)
                                        {
                                            var findFile = currentFolderloaded.subFiles.FirstOrDefault(x => x.ReturnPlaceInParent() == arg2);
                                            var folderToPut = currentFolderloaded.subFolders.FirstOrDefault(x => x.placeInParentFolder == arg3);
                                            if (findFile != null && folderToPut != null)
                                            {
                                                FileMethods.MoveFile(currentFolderloaded, findFile, folderToPut);
                                            }
                                            else
                                            {
                                                Console.WriteLine("Please select a valid file and destination folder.");
                                            }
                                        }
                                        else if (nType == DirectoryType.FOLDER)
                                        {
                                            var findFolder = currentFolderloaded.subFolders.FirstOrDefault(x => x.placeInParentFolder == arg2);
                                            var folderToPut = currentFolderloaded.subFolders.FirstOrDefault(x => x.placeInParentFolder == arg3);
                                            if (findFolder != null && folderToPut != null)
                                            {
                                                FolderMethods.MoveFolder(findFolder, folderToPut);
                                            }
                                            else
                                            {
                                                Console.WriteLine("Please select a valid folder and destination folder.");
                                            }
                                        }
                                    }
                                    break;
                                }
                        }
                    }

                }
                return false;
            }
            catch (Exception)
            {
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

        public static void ReturnToParent()
        {
            Folder? pathParent = currentFolderloaded.parentFolder;

            if (pathParent != null)
            {
                InitializeFolder(pathParent.parentFolder, pathParent.pathToFolder, pathParent.folderName);
            }
        }

        public static void WriteCenteredHeader(string text)
        {
            int windowWidth = GetSafeConsoleWidth();
            int stringLength = text.Length;
            int leftPadding = Math.Max(0, (windowWidth - stringLength) / 2);

            Console.WriteLine(text.PadLeft(leftPadding + stringLength));
        }

        private static int GetSafeConsoleWidth()
        {
            try
            {
                return Console.WindowWidth > 0 ? Console.WindowWidth : 80;
            }
            catch (IOException)
            {
                return 80;
            }
            catch (InvalidOperationException)
            {
                return 80;
            }
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

        private static string FormatListingRow(string index, string type, string name)
        {
            return $"{index,-7} {type,-8} {name}";
        }

        private static string FormatCommandRow(string category, string command, string description)
        {
            return $"{category,-8} {command,-28} {description}";
        }

        public static class FileMethods
        {
            public static void ReadFile(IFile file)
            {
                var fullPath = Path.Combine(currentFolderloaded.pathToFolder, file.ReturnFileName());
                string fileContents = File.ReadAllText(fullPath);

                Console.WriteLine("File Contents:");
                Console.WriteLine(fileContents);
                return;
            }
            public static void AppendFile(Folder baseFolder, IFile file)
            {
                string path = Path.Combine(baseFolder.pathToFolder, file.ReturnFileName());

                if (File.Exists(path))
                {
                    Console.WriteLine($"Please Enter Text you would like to append to {file.ReturnFileName()}: ");
                    string? input = Console.ReadLine();

                    File.AppendAllText(path, input ?? string.Empty);
                }
            }
            public static void OverwriteFile(Folder baseFolder, IFile file)
            {
                string path = Path.Combine(baseFolder.pathToFolder, file.ReturnFileName());

                if (File.Exists(path))
                {
                    Console.WriteLine($"Please Enter Text to Overwrite File {file.ReturnFileName()}: ");
                    string? input = Console.ReadLine();

                    File.WriteAllText(path, string.IsNullOrEmpty(input) ? "" : input + Environment.NewLine);
                }
            }
            public static void CreateFile(Folder baseFolder, string name)
            {
                string path = Path.Combine(baseFolder.pathToFolder, !string.IsNullOrEmpty(name) ? name : $"newfile{baseFolder.subFiles.Count() + 1}");

                if (File.Exists(path))
                {
                    Console.WriteLine($"File Already Exists in this Location!");
                }
                else
                {
                    Console.WriteLine("Enter New File Text Input: ");
                    string? input = Console.ReadLine();

                    File.WriteAllText(path, string.IsNullOrEmpty(input) ? "" : input + Environment.NewLine);
                }
            }

            public static void MoveFile(Folder baseFolder, IFile file, Folder folderToMoveTo)
            {
                if (file == null || folderToMoveTo == null)
                {
                    Console.WriteLine("Please select a valid file and destination folder.");
                    return;
                }

                string filePath = Path.Combine(baseFolder.pathToFolder, file.ReturnFileName());
                string folderPath = Path.Combine(folderToMoveTo.pathToFolder, file.ReturnFileName());

                if (Directory.Exists(folderToMoveTo.pathToFolder))
                {
                    File.Move(filePath, folderPath);
                }
                else
                {
                    Console.WriteLine("Please select a valid destination folder.");
                }
            }
            public static void DeleteFile(Folder baseFolder, IFile fileToDelete)
            {
                string path = Path.Combine(baseFolder.pathToFolder, fileToDelete.ReturnFileName());

                if (File.Exists(path))
                {
                    Console.WriteLine($"Are you sure you want to delete {fileToDelete.ReturnFileName()}?\nYES or NO");
                    string? input = Console.ReadLine();

                    if (!string.IsNullOrEmpty(input) && input.ToUpper() == "YES")
                    {
                        File.Delete(path);
                    }
                }
            }
        }

        public static class FolderMethods
        {
            public static void EnterFolder(Folder folderToEnter)
            {
                var path = folderToEnter.pathToFolder;
                if (Directory.Exists(path))
                {
                    currentFolderloaded = InitializeFolder(folderToEnter.parentFolder, folderToEnter.pathToFolder, folderToEnter.folderName);
                }

            }
            public static void ExitFolder(Folder baseFolder)
            {
                if (baseFolder.parentFolder == null)
                {
                    return;
                }

                var path = baseFolder.parentFolder.pathToFolder;

                if (Directory.Exists(path))
                {
                    currentFolderloaded = InitializeFolder(baseFolder.parentFolder.parentFolder, baseFolder.parentFolder.pathToFolder, baseFolder.parentFolder.folderName);
                }
            }
            public static void DeleteFolder(Folder folderToDelete)
            {
                string path = folderToDelete.pathToFolder;

                if (Directory.Exists(path))
                {
                    Console.WriteLine($"Are you sure you want to delete {folderToDelete.folderName}? This includes all files in the folder!!\nYES or NO");
                    string? input = Console.ReadLine();

                    if (!string.IsNullOrEmpty(input) && input.ToUpper() == "YES")
                    {
                        Directory.Delete(path, true);
                    }
                }
            }
            public static void CreateFolder(Folder baseFolder, string nameOfFolder)
            {
                string path = Path.Combine(baseFolder.pathToFolder, !string.IsNullOrEmpty(nameOfFolder) ? nameOfFolder : $"newfile{baseFolder.subFiles.Count() + 1}");

                if (Directory.Exists(path))
                {
                    Console.WriteLine($"Folder Already Exists in this Folder!");
                }
                else
                {
                    Directory.CreateDirectory(path);
                }
            }
            public static void MoveFolder(Folder folderBeingMoved, Folder folderToMoveTo)
            {
                if (folderBeingMoved == null || folderToMoveTo == null)
                {
                    Console.WriteLine("Please select a valid folder and destination folder.");
                    return;
                }

                string filePath = folderBeingMoved.pathToFolder;
                string folderPath = Path.Combine(folderToMoveTo.pathToFolder, folderBeingMoved.folderName);

                if (Directory.Exists(folderToMoveTo.pathToFolder))
                {
                    Directory.Move(filePath, folderPath);
                }
                else
                {
                    Console.WriteLine("Please select a valid destination folder.");
                }
            }
        }
    }
}