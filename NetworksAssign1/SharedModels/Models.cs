namespace SharedModels;
using System.IO;
using System;

// --------- ENUMS -----------//
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
public enum STATUS
{
    USERNAME,
    INDIRECTORY
}
// -- FILE INTERFACE -- //
public interface IFile
{
    string ReturnFileName();
    int ReturnPlaceInParent();

}

// ------------- SHARED CLASSES FOR SERVER AND CLIENT FILES -----------//
public class ServerStatusResponse
{
    public STATUS currentStatus { get; set; }
    public string? message { get; set; }
    public string? output { get; set; }
    public DirectoryListing? currentFolder { get; set; }
    public bool waitingOnCommand {get;set;}
    public bool shouldClose { get; set; }
}

public class DirectoryListing // Specific Folder information needed for the client
{
    public string folderName { get; set; } = string.Empty;
    public string? parentFolderName { get; set; }
    public List<string> subFolders { get; set; } = new();
    public List<string> subFiles { get; set; } = new();
}

public class ServerFolderAndOutput
{
    SharedModels.Folder? currentFolder { get; set; }
    public string? output { get; set; }
    public string? message { get; set; }

}

public class Folder
{
    public Folder? parentFolder { get; set; }
    public int placeInParentFolder { get; set; }

    public string folderName { get; set; } = string.Empty;
    public string pathToFolder { get; set; } = string.Empty;
    public List<Folder> subFolders { get; set; } = new();
    public List<IFile> subFiles { get; set; } = new();



    // Will List subFolders and subFiles in a Folder. Used for 
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


        int count = 0; // Will not reset! Folders should be listed first, then files
        for (int i = 0; i < subFolders.Count(); i++) // Print all folders our first
        {
            Console.WriteLine(MethodsToShare.FormatListingRow(count.ToString(), "Folder", subFolders[i].folderName));
            subFolders[i].placeInParentFolder = count;
            count++;
        }
        for (int i = 0; i < subFiles.Count(); i++) // Print all files out next
        {
            Console.WriteLine(MethodsToShare.FormatListingRow(count.ToString(), "File", subFiles[i].ReturnFileName()));
            var tFile = (TextFile)subFiles[i];
            tFile.placeInParentFolder = count;
            count++;
        }

        if (count == 0) // No files/folders in currently directory
        {
            Console.WriteLine(MethodsToShare.FormatListingRow("-", "-", "(empty)"));
        }

    }

}
public static class MethodsToShare
{
    // Lists all the commands available for the Client!
    public static string GetCommandsText()
    {
        var lines = new List<string>
        {
            "Available Commands",
            new string('-', 78),
            FormatCommandRow("Category", "Command", "What It Does"),
            FormatCommandRow("--------", "-------", "------------"),
            FormatCommandRow("File", "READ <index>", "Show the contents of a file."),
            FormatCommandRow("File", "APPEND <index>", "Add text to the end of a file."),
            FormatCommandRow("File", "OVERWRITE <index>", "Replace all text inside a file."),
            FormatCommandRow("Create", "CREATE FILE <name>", "Create a new file, then enter its text."),
            FormatCommandRow("Create", "CREATE FOLDER <name>", "Create a new folder in the current folder."),
            FormatCommandRow("Delete", "DELETE <index>", "Delete the selected file or folder."),
            FormatCommandRow("Move", "MOVE FILE <item> <folder>", "Move a file into a folder."),
            FormatCommandRow("Move", "MOVE FOLDER <item> <folder>", "Move a folder into another folder."),
            FormatCommandRow("Folder", "ENTER <index>", "Open the selected folder."),
            FormatCommandRow("Folder", "EXIT", "Go to the parent folder."),
            FormatCommandRow("Help", "HELP", "Show the command list again."),
            FormatCommandRow("System", "QUIT", "Close the program."),
            string.Empty
        };

        return string.Join(Environment.NewLine, lines);
    }

    public static void ListCommands()
    {
        Console.WriteLine();
        Console.Write(GetCommandsText());
    }
    // Helper Method used to help format Terminal Strings nicely
    public static string FormatListingRow(string index, string type, string name)
    {
        return $"{index,-7} {type,-8} {name}";
    }
    // Helper Method used to help format Terminal Strings nicely
    public static string FormatCommandRow(string category, string command, string description)
    {
        return $"{category,-8} {command,-28} {description}";
    }
    // Centers text for Terminal Printing
    public static void WriteCenteredHeader(string text)
    {
        int windowWidth = GetSafeConsoleWidth();
        int stringLength = text.Length;
        int leftPadding = Math.Max(0, (windowWidth - stringLength) / 2);

        Console.WriteLine(text.PadLeft(leftPadding + stringLength));
    }
    // Method used when debugging, as debugging has weird terminal glitches when it comes to console width
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
// TextFile is the only file type this system supports
public class TextFile : IFile
{
    public string fileName { get; set; } = string.Empty;
    public string ReturnFileName() => fileName;
    public int placeInParentFolder { get; set; }
    public int ReturnPlaceInParent() => placeInParentFolder;
}
