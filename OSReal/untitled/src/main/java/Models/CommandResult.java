package Models;

import java.util.ArrayList;
import java.util.Objects;

public class CommandResult {
    public String newPathtoPrint;

    public String contentsInAFile;

    public Boolean isQuit = false;

    public ArrayList<String> errors = new ArrayList<>();

    public InDirectory directoryResults;

    //TODO: Make constructor that takes an absolute path for newPath, relativizes it, and ToStrings it, then remove all relativized code
    public CommandResult()
    {
        this.newPathtoPrint = null;
        this.errors = new ArrayList<>();
        this.contentsInAFile = "";
        this.directoryResults = null;
    }

    public CommandResult(String newPath,  InDirectory directory, String contentsInAFile, ArrayList<String> errors)
    {
        this.newPathtoPrint = newPath;
        this.errors = errors;
        this.contentsInAFile = contentsInAFile;
        this.directoryResults = directory;
    }
    public CommandResult(String newPath, String contentsInAFile, ArrayList<String> errors)
    {
        this.newPathtoPrint = newPath;
        this.errors = errors;
        this.contentsInAFile = contentsInAFile;
    }
    public CommandResult(String newPath, ArrayList<String> errors)
    {
        this.newPathtoPrint = newPath;
        this.errors = errors;
    }


    public CommandResult(Boolean isQuit)
    {
        this.isQuit = true;
    }





    public String ResultsToString()
    {
        if (errors.toArray().length == 0)
        {
            StringBuilder successMessage = new StringBuilder();

            if (this.newPathtoPrint != null)
            {
                successMessage.append("Result Message: %s\n".formatted(this.newPathtoPrint));
            }
            if (contentsInAFile != null)
            {
                successMessage.append(contentsInAFile);
            }


            int printCount = 1;

            if (directoryResults != null) {

                successMessage.append("Folders/Files in Directory:\n");

                if (!directoryResults.folders.isEmpty()) {
                    for (Folder folder : directoryResults.folders) {
                        successMessage.append("%d: %s\n".formatted(printCount, folder.nameOfFolder));
                        printCount++;
                    }
                }

                if (!directoryResults.files.isEmpty()) {
                    for (File file : directoryResults.files) {
                        successMessage.append("%d: %s\n".formatted(printCount, file.nameOfFile));
                        printCount++;
                    }
                }
            }

            // TODO: Add a check in here to loop through subfolders and files to print them out. You may need to make
            // TODO: cont... to make an interface that files and folders share to preserve ordering or smth.

            return successMessage.toString();
        }
        else
        {
            StringBuilder errorMessage = new StringBuilder("Error! See below:\n");
            for (int i = 1; i <= this.errors.toArray().length; i++ ) {
                String message = "%d: %s\n".formatted(i , this.errors.get(i - 1));
                errorMessage.append(message);
            }
            return errorMessage.toString();
        }
    }

    //TODO: Implement constructor(s) to take in subFiles and subFolders used for printing to client!
}
