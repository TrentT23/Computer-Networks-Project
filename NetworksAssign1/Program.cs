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
                    switch (fileOp)
                    {
                        case FileOperations.READ:
                            FileMethods.ReadFile(currentFolderloaded, arg1);
                            break;
                        case FileOperations.APPEND:
                            break;
                        case FileOperations.OVERWRITE:
                            break;
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
    /*
    static void HandleFileOrFolderSelection(Folder baseFolder)
    {
        bool endLoop = false;
        while (endLoop == false)
        {

            baseFolder.ListFolderContents();
            WriteCenteredHeader("Please Enter the File/Folder Index you want to select: ");


            if (int.TryParse(Console.ReadLine(), out int selection))
            {
                var findFile = (object)baseFolder.subFolders.FirstOrDefault(x => x.placeInParentFolder == selection)
                ?? (object)baseFolder.subFiles.FirstOrDefault(x => x.ReturnPlaceInParent() == selection);

                if (findFile is Folder folder) // Match the pattern
                {
                    SelectFolder(folder);
                    endLoop = true;
                }
                else if (findFile is IFile file)
                {
                    SelectFile(file);
                    endLoop = true;
                }
                else
                {
                    WriteCenteredHeader("Please make a valid selection\n");
                }
            }
        }



    }
    */

  

    static string SelectFile(IFile file) // SELECT FILE PROCESS EXECUTED
    {
        bool endLoop = false;

        while (endLoop == false)
        {
            Console.WriteLine("Please Select File Option (Enter an Integer 1-4): ");
            Console.WriteLine($"(1) -- Read File Contents (READ)");
            Console.WriteLine($"(2) -- Append to File (WRITE)");
            Console.WriteLine($"(3) -- Overrwrite to File (WRITE)");
            Console.WriteLine($"(4) -- Exit Back to Directory");

            if (int.TryParse(Console.ReadLine(), out int selection))
            {
                switch (selection)
                {
                    case 1: // READ
                        var fullPath = Path.Combine(FileSystemConfig.Root, file.ReturnFileName());
                        string fileContents = File.ReadAllText(fullPath);

                        Console.WriteLine("File Contents:");
                        Console.WriteLine(fileContents);
                        break;
                    case 2: // APPEND

                        break;
                    case 3: // OVERWRITE

                        break;
                    case 4: // EXIT
                        endLoop = true;
                        break;
                    default:
                        break;
                }
            }

        }
        return "";
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
        public static void ReadFile(Folder baseFolder, int selection)
        {
            var findFile = (object)baseFolder.subFolders.FirstOrDefault(x => x.placeInParentFolder == selection)
                   ?? (object)baseFolder.subFiles.FirstOrDefault(x => x.ReturnPlaceInParent() == selection);

            if (findFile is Folder folder) // If Folder, then print error
            {
                WriteCenteredHeader("Cannot read a Folder!");
                return;
            }
            else if (findFile is IFile file)
            {
                var fullPath = Path.Combine(FileSystemConfig.Root, file.ReturnFileName());
                string fileContents = File.ReadAllText(fullPath);

                Console.WriteLine("File Contents:");
                Console.WriteLine(fileContents);
                return;
            }
        }
        public static void AppendFile(IFile file)
        {

        }
        public static void OverwriteFile(IFile file)
        {

        }
        public static void CreateFile(Folder baseFolder, string name)
        {

        }
        public static void DeleteFile(IFile fileToDelete)
        {

        }
    }

    public static class FolderMethods
    {
        public static void EnterFolder(Folder folderToEnter)
        {

        }
        public static void ExitFolder(Folder baseFolder)
        {

        }
        public static void DeleteFolder(Folder folderToDelete)
        {

        }
        public static void CreateFolder(Folder baseFolder, string nameOfFolder)
        {

        }
    }


}
