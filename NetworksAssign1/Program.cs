using ServerFile;
using System.IO;
using System;
using System.Reflection.Metadata.Ecma335;
using System.IO.Enumeration;
using System.Security.Cryptography.X509Certificates;
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
    static Folder currentFolderloaded = new Folder();
    public interface IFile
    {
        void ReadContents();
        void OverrwriteFile();
        void AppendFile();
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

        public void ReadContents()
        {

        }
        public void OverrwriteFile()
        {

        }
        public void AppendFile()
        {

        }

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
            Console.WriteLine("Please Select an Option (Enter an Integer 1-4): ");
            Console.WriteLine($"(1) -- Select Folder/File");
            Console.WriteLine($"(2) -- Create Folder under {currentFolderloaded.folderName}");
            Console.WriteLine($"(3) -- Create Text File under {currentFolderloaded.folderName}");
            Console.WriteLine($"(4) -- Return to Parent Folder");
            Console.WriteLine($"(5) -- Exit Program");

            if (int.TryParse(Console.ReadLine(), out int option))
            {
                switch (option)
                {
                    case 1:
                        HandleFileOrFolderSelection(currentFolderloaded);
                        break;
                    case 2:
                        CreateFolder(currentFolderloaded);
                        break;
                    case 3: // END THE PROGRAM
                        CreateFile(currentFolderloaded);
                        break;
                    case 4:
                        ReturnToParentFolder(currentFolderloaded);
                        break;
                    case 5: // END THE PROGRAM
                        endLoop = true;
                        break;
                    default:
                        Console.WriteLine("Invalid Option Selected\n");
                        break;

                }
            }


        }
    }

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
    static void CreateFile(Folder baseFolder)
    {

    }

    static void CreateFolder(Folder baseFolder)
    {


    }
    static void SelectFolder(Folder folder) // SELECT FOLDER PROCESS EXECUTED
    {

    }

    static void ReturnToParentFolder(Folder baseFolder)
    {

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


}
