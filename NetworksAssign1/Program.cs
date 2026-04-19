using ServerFile;
using System.IO;
using System;
using System.Reflection.Metadata.Ecma335;
using System.IO.Enumeration;
class Program
{
    static void Main()
    {
        /*
        Console.WriteLine("Hello, World!");

        List<DirectoryRoot.Folder> testFolders = new List<DirectoryRoot.Folder>();
        List<DirectoryRoot.IFile> testFiles = new List<DirectoryRoot.IFile>();
        testFolders.Add(new DirectoryRoot.Folder() { folderName = "Sub1" });
        testFolders.Add(new DirectoryRoot.Folder() { folderName = "Sub2" });
        testFolders.Add(new DirectoryRoot.Folder() { folderName = "Sub3" });
        testFolders.Add(new DirectoryRoot.Folder() { folderName = "Sub4" });

        testFiles.Add(new DirectoryRoot.TextFile() { fileName = "File1" });
        testFiles.Add(new DirectoryRoot.TextFile() { fileName = "File2" });
        testFiles.Add(new DirectoryRoot.TextFile() { fileName = "File3" });
        testFiles.Add(new DirectoryRoot.TextFile() { fileName = "File4" });

        var newFolder = new DirectoryRoot.Folder() { folderName = "TrentsFolder", subFolders = testFolders, subFiles = testFiles};
        */
        Directory.CreateDirectory(DirectoryRoot.FileSystemConfig.Root);

        var folder = DirectoryRoot.InitializeFolder(null, DirectoryRoot.FileSystemConfig.Root, "ServerFiles");
        folder.ListFolderContents();


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

    }
    public class Folder
    {
        public Folder? parentFolder { get; set; }

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
            Console.WriteLine(underline.PadLeft((Console.WindowWidth - underline.Length) / 2 + underline.Length));


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
    static void MainLoop()
    {
        string DirectoryRoot = FileSystemConfig.Root;
        bool endLoop = false;

        while (endLoop == false)
        {
            Console.WriteLine("Please Select an Option (Enter an Integer 1-4): ");
            Console.WriteLine("(1) -- Read File");
            Console.WriteLine("(2) -- Write File");
            Console.WriteLine("(3) -- List Files");
            Console.WriteLine("(4) -- Create Folder");
            Console.WriteLine("(5) -- Exit Program");

            if (int.TryParse(Console.ReadLine(), out int option))
            {
                switch (option)
                {
                    case 1:
                        ReadFile();
                        break;
                    case 2:
                        WriteFile();
                        break;
                    case 3:
                        ListFiles();
                        break;
                    case 4: // END THE PROGRAM
                        CreateFolder();
                        break;
                    case 5: // END THE PROGRAM
                        endLoop = true;
                        break;
                    default:
                        break;

                }
            }


        }
    }

    static string SelectFile(string nameOfFile) // Returns a string holding the Path of the file needed
    {
        return "";
    }

    static void ListFiles()
    {

    }
    static void WriteFile()
    {

    }
    static void CreateFolder()
    {

    }
    static void ReadFile()
    {

    }

    static void DisplayFolderInfo()
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

        foreach (var file in rootFiles)
        {
            allFiles.Add(new TextFile()
            {
                fileName = Path.GetFileName(file)
            });
        }

        foreach (var folder in rootFolders)
        {
            allFolders.Add(new Folder()
            {
                folderName = Path.GetFileName(folder)
            });
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
        // 1. Calculate padding
        int windowWidth = Console.WindowWidth;
        int stringLength = text.Length;
        int leftPadding = (windowWidth - stringLength) / 2;

        Console.WriteLine(text.PadLeft(leftPadding + stringLength));
    }


}
