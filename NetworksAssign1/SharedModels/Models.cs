namespace SharedModels;

using System.IO;
using System;

// Contains shared models used by both client and server for processing!
public enum SessionStatus
{
    Username,
    InDirectory
}

public enum DirectoryEntryType
{
    File,
    Folder
}

public class ClientRequest
{
    public string Username { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
}

public class ServerResponse
{
    public bool Success { get; set; }
    public SessionStatus CurrentStatus { get; set; }
    public string Message { get; set; } = string.Empty;
    public Folder? Folder { get; set; }
}

// Folder and File System -- shared by client and server for sending and receiving data. 
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
        MethodsToShare.WriteCenteredHeader($"Folder: {folderName}");
        if (!string.IsNullOrEmpty(parentFolder?.folderName))
        {
            MethodsToShare.WriteCenteredHeader($"Parent Folder: {parentFolder.folderName}");
        }

        string underline = new string('-', 44);
        int consoleWidth = MethodsToShare.GetSafeConsoleWidth();
        int underlinePadding = Math.Max(0, (consoleWidth - underline.Length) / 2);
        Console.WriteLine(underline.PadLeft(underlinePadding + underline.Length));
        Console.WriteLine(MethodsToShare.FormatListingRow("Index", "Type", "Name"));
        Console.WriteLine(MethodsToShare.FormatListingRow("-----", "------", "----"));


        int count = 0;
        for (int i = 0; i < subFolders.Count(); i++)
        {
            Console.WriteLine(MethodsToShare.FormatListingRow(count.ToString(), "Folder", subFolders[i].folderName));
            subFolders[i].placeInParentFolder = count;
            count++;
        }
        for (int i = 0; i < subFiles.Count(); i++)
        {
            Console.WriteLine(MethodsToShare.FormatListingRow(count.ToString(), "File", subFiles[i].ReturnFileName()));
            var tFile = (TextFile)subFiles[i];
            tFile.placeInParentFolder = count;
            count++;
        }

        if (count == 0)
        {
            Console.WriteLine(MethodsToShare.FormatListingRow("-", "-", "(empty)"));
        }

    }

}
public static class MethodsToShare
{
    public static void ListCommands()
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
    public static string FormatListingRow(string index, string type, string name)
    {
        return $"{index,-7} {type,-8} {name}";
    }

    public static string FormatCommandRow(string category, string command, string description)
    {
        return $"{category,-8} {command,-28} {description}";
    }
    public static void WriteCenteredHeader(string text)
    {
        int windowWidth = GetSafeConsoleWidth();
        int stringLength = text.Length;
        int leftPadding = Math.Max(0, (windowWidth - stringLength) / 2);

        Console.WriteLine(text.PadLeft(leftPadding + stringLength));
    }
    public static int GetSafeConsoleWidth()
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
public class TextFile : IFile
{
    public string fileName { get; set; } = string.Empty;
    public string ReturnFileName() => fileName;
    public int placeInParentFolder { get; set; }
    public int ReturnPlaceInParent() => placeInParentFolder;
}


