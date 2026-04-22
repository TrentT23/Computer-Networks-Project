using ServerFile;
using System.IO;
using System;
using System.Reflection.Metadata.Ecma335;
using System.IO.Enumeration;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
class Program
{
    static void Main()
    {

        Directory.CreateDirectory(DirectoryRoot.FileSystemConfig.Root);

        var folder = DirectoryRoot.InitializeFolder(null, DirectoryRoot.FileSystemConfig.Root, "ServerFiles");

        DirectoryRoot.MainLoop(folder);

    }
}

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

        public string? folderName { get; set; }
        public string? pathToFolder { get; set; }
        public List<Folder> subFolders { get; set; } = new();
        public List<IFile> subFiles { get; set; } = new();



        public void ListFolderContents()
        {
            WriteCenteredHeader($"Folder: {folderName}");
            if (!string.IsNullOrEmpty(parentFolder?.folderName))
            {
                WriteCenteredHeader($"Parent Folder: {parentFolder.folderName}");
            }

            string underline = "------------------------";
            int consoleWidth = GetSafeConsoleWidth();
            int underlinePadding = Math.Max(0, (consoleWidth - underline.Length) / 2);
            Console.WriteLine(underline.PadLeft(underlinePadding + underline.Length));


            int count = 0;
            for (int i = 0; i < subFolders.Count(); i++)
            {
                WriteCenteredHeader($"({count} - Folder): {subFolders[i].folderName}");
                count++;
            }
            for (int i = 0; i < subFiles.Count(); i++)
            {
                WriteCenteredHeader($"({count} - File): {subFiles[i].ReturnFileName()}");
                count++;
            }

        }


    }
    public class TextFile : IFile
    {
        public string? fileName { get; set; }
        public string ReturnFileName() => fileName ?? "";
        public int placeInParentFolder { get; set; }
        public int ReturnPlaceInParent() => placeInParentFolder;
    }
    static void ListCommands()
    {
        WriteCenteredHeader("File Operations: ");
        Console.WriteLine($"READ (index)");
        Console.WriteLine($"APPEND (index)");
        Console.WriteLine($"OVERWRITE (index)");
        Console.WriteLine($"CREATE FILE \"(New File Name)\"");
        Console.WriteLine($"DELETE FILE (index)");
        Console.WriteLine();
        WriteCenteredHeader("Folder Operations: ");
        Console.WriteLine("ENTER (index)");
        Console.WriteLine("EXIT (index)");
        Console.WriteLine("CREATE FOLDER \"(New Folder Name)\"");
        Console.WriteLine("DELETE FOLDER (index)");
        Console.WriteLine();
        Console.WriteLine("To Exit Program, type \"(EXIT)\"");
    }
    public static class FileSystemConfig
    {
        public static string Root = Path.Combine(AppContext.BaseDirectory, "ServerFiles");
    }
    public static void MainLoop(Folder baseFolder)
    {
        currentFolderloaded = baseFolder;

        currentFolderloaded.ListFolderContents();

        string DirectoryRoot = FileSystemConfig.Root;
        bool endLoop = false;

        while (endLoop == false)
        {
            ListCommands();

            string? command = Console.ReadLine();

            if (!string.IsNullOrEmpty(command))
            {
                ParseCommand(command);
            }
        }
    }

    static void ParseCommand(string fullCommand)
    {
        try
        {
            string[] parts = fullCommand.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (!string.IsNullOrEmpty(parts[0]))
            {
                int arg1 = -1;
                int.TryParse(parts[1], out arg1);

                if (Enum.TryParse<FileOperations>(parts[0].ToUpper(), out FileOperations fileOp))
                {
                    var findFile = currentFolderloaded.subFiles.FirstOrDefault(x => x.ReturnPlaceInParent() == arg1);

                    if (findFile == null || findFile is Folder folder) // If Folder, then print error
                    {
                        WriteCenteredHeader("Cannot read a Folder!");
                        return;
                    }
                    else
                    {
                        switch (fileOp)
                        {
                            case FileOperations.READ:
                                FileMethods.ReadFile(currentFolderloaded, findFile);
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
                            break;
                        case FolderOperations.EXIT:
                            break;
                    }
                }
                else if (Enum.TryParse<SharedOperations>(parts[0].ToUpper(), out SharedOperations sharedOp))
                {
                    switch (sharedOp)
                    {
                        case SharedOperations.CREATE:
                            string sarg1 = parts[1]; // Create is the only case were the second argument is a string

                            if (Enum.TryParse<DirectoryType>(sarg1.ToUpper(), out DirectoryType nType))
                            {

                            }


                            break;
                        case SharedOperations.DELETE:
                            break;
                        case SharedOperations.QUIT:
                            break;
                    }
                }

            }
        }
        catch (Exception)
        {

        }
    }

    public static Folder InitializeFolder(Folder parent, string pathToFolder, string baseName)
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
                folderName = Path.GetFileName(rootFolders[refCount]),
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
        Folder pathParent = currentFolderloaded.parentFolder;

        if (pathParent != null)
        {
            InitializeFolder(pathParent?.parentFolder, pathParent?.pathToFolder, pathParent?.folderName);
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

    public static class FileMethods
    {
        public static void ReadFile(Folder baseFolder, IFile file)
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
                string input = Console.ReadLine();

                File.AppendAllText(path, input);
            }
        }
        public static void OverwriteFile(Folder baseFolder, IFile file)
        {
            string path = Path.Combine(baseFolder.pathToFolder, file.ReturnFileName());

            if (File.Exists(path))
            {
                Console.WriteLine($"Please Enter Text to Overwrite File {file.ReturnFileName()}: ");
                string input = Console.ReadLine();

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
                using (File.Create(path))
                {
                    Console.WriteLine("Enter New File Text Input: ");
                    string? input = Console.ReadLine();

                    File.WriteAllText(path, string.IsNullOrEmpty(input) ? "" : input + Environment.NewLine);
                }
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
            if (File.Exists(path))
            {
                currentFolderloaded = folderToEnter;
                currentFolderloaded.ListFolderContents();
            }

        }
        public static void ExitFolder(Folder baseFolder)
        {
            var path = baseFolder.parentFolder.pathToFolder;

            if (File.Exists(path))
            {
                currentFolderloaded = baseFolder.parentFolder;
                currentFolderloaded.ListFolderContents();
            }
        }
        public static void DeleteFolder(Folder folderToDelete)
        {

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
    }


}
