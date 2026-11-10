import Models.*;
import Enum.CommandType;

import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.StandardCopyOption;
import java.util.ArrayList;
import java.util.Comparator;
import java.util.Objects;
import java.util.stream.Stream;

public class CommandExecutor {
    static FileSystemController fsc = FileSystemController.fsc;
    public static ArrayList<String> errorTracker = new ArrayList<>();
    public static CommandResult result = new CommandResult();


    public static CommandResult ProcessCommand(ParsedCommand fullCommand) throws IOException {

        errorTracker = new ArrayList<>();
        result = new CommandResult();

        switch (fullCommand.type) {
            case CommandType.PWD:
                pwd();
                break;
            case CommandType.MKDIR:
                mkdir(fullCommand);
                break;
            case CommandType.WRITE:
                write(fullCommand);
                break;
            case CommandType.CREATE:
                create(fullCommand);
                break;
            case CommandType.READ:
                read(fullCommand);
                break;
            case CommandType.LS:
                ls(fullCommand);
                break;
            case CommandType.MV:
                mv(fullCommand);
                break;
            case CommandType.CD:
               cd(fullCommand);
               break;
            case CommandType.CP:
                cp(fullCommand);
                break;
            case CommandType.RM:
                rm(fullCommand);
                break;
            case CommandType.QUIT:
                result = new CommandResult(true);
                break;
            case CommandType.NULL:
                errorTracker.add("Command not Recognized");
                result = new CommandResult(relativizeAPath(fsc.currentPath).toString(), errorTracker);
                break;
        }

        return result;
    }

    // All Command Implementations --------------------------------------------------------------------------------------
    static void pwd()
    {
        Path currentDir = fsc.currentPath;

        Path relativized = relativizeAPath(fsc.currentPath);
        result = new CommandResult("~%s".formatted(relativized.toString()), errorTracker );
    }

    static void mkdir(ParsedCommand command) throws IOException {
        Path newFolderAddition = Path.of(command.listOfArgs[0]);

        Path resolvedPath = fsc.currentPath.resolve(newFolderAddition);

        if (confirmValidPath(resolvedPath))
        {
            Files.createDirectory(resolvedPath);
            fsc.currentPath = resolvedPath;
        }
        else
        {
            errorTracker.add("Path is not valid for new directory");
        }

        Path relativized = relativizeAPath(fsc.currentPath);
        result = new CommandResult("~%s".formatted(relativized.toString()), errorTracker );

    }

    static void create(ParsedCommand command) throws IOException {
        Path newFileAddition = Path.of(command.listOfArgs[0]);

        Path resolvedPath = fsc.currentPath.resolve(newFileAddition);

        if (confirmValidPath(resolvedPath)){
            Files.createFile(resolvedPath);
        }
        else
        {
            errorTracker.add("Path is not valid for new file!");
        }
        Path relativized = relativizeAPath(fsc.currentPath);

        result = new CommandResult("~%s".formatted(relativized.toString()), errorTracker );
    }

    static void write(ParsedCommand command) throws IOException {
        Path newFileAddition = Path.of(command.listOfArgs[0]);

        Path resolvedPath = fsc.currentPath.resolve(newFileAddition);

        if (confirmValidPath(resolvedPath)){
            Files.writeString(resolvedPath, command.listOfArgs[1]);
        }
        else
        {
            errorTracker.add("Path is not valid to write to a file");
        }

        Path relativized = relativizeAPath(fsc.currentPath);
        result = new CommandResult("~%s".formatted(relativized.toString()),  errorTracker );
    }

    static void read(ParsedCommand command) throws IOException {
        Path newFileAddition = Path.of(command.listOfArgs[0]);
        String pathContents = "";

        Path resolvedPath = fsc.currentPath.resolve(newFileAddition);
        if (confirmValidPath(resolvedPath))
        {
            pathContents = Files.readString(resolvedPath);
        }
        else
        {
            errorTracker.add("Path is not valid to read a file");
        }
        Path relativized = relativizeAPath(fsc.currentPath);
        result = new CommandResult("~%s".formatted(relativized.toString()), pathContents, errorTracker );
    }

    static void ls(ParsedCommand command) throws IOException {

        Path folderToSearch;
        if (command.listOfArgs.length > 0) {
            Path filePathToCheck = Path.of(command.listOfArgs[0]);

            Path resolvedPath = fsc.programPath.resolve(filePathToCheck);

            if (confirmValidPath(resolvedPath)) {
                folderToSearch = resolvedPath;
            } else {
                folderToSearch = null;
            }
        }
        else
        {
            folderToSearch = fsc.currentPath;
        }

        ArrayList<Folder> foldersList = new ArrayList<>();
        ArrayList<File> filesList = new ArrayList<>();

        try {
            assert folderToSearch != null;
            try (Stream<Path> pathStream = Files.walk(folderToSearch, 1)) {
                pathStream.filter(path -> !path.equals(folderToSearch)).forEach(path -> {
                    String name = path.getFileName().toString();
                    if (Files.isDirectory(path))
                    {
                        foldersList.add(new Folder(path, name));
                    }
                    else if (Files.isRegularFile(path))
                    {
                        filesList.add(new File(path, name));
                    }
                });
            }
        }
        catch (IOException e)
        {
            errorTracker.add("Failed to list sub-files and sub-folders of directory");
        }
        Path relativized = relativizeAPath(fsc.currentPath);
        result = new CommandResult("~%s".formatted(relativized.toString()), new InDirectory(filesList, foldersList),"", errorTracker );
    }

    static void mv(ParsedCommand command) throws IOException {
        Path source = Path.of(command.listOfArgs[0]);
        Path destination = Path.of(command.listOfArgs[1]);

        Path resolvedSource = fsc.currentPath.resolve(source);
        Path resolvedDestination = fsc.currentPath.resolve(destination);

        if (fsc.NavigateToAnExistingPath(resolvedSource) == null)
        {
            resolvedSource = fsc.programPath.resolve(source);

            if (fsc.NavigateToAnExistingPath(resolvedSource) == null)
            {
                errorTracker.add("Source fails to resolve!");
            }
        }

        if (fsc.NavigateToAnExistingPath(resolvedDestination) == null)
        {
            resolvedDestination = fsc.programPath.resolve(destination);

            if (fsc.NavigateToAnExistingPath(resolvedDestination) == null)
            {
                errorTracker.add("Destination fails to resolve");
            }
        }

        if (confirmValidPath(resolvedDestination) && confirmValidPath(resolvedSource))
        {
            if (fsc.currentPath.startsWith(resolvedSource))
            {
                errorTracker.add("Cannot move folder unless you leave the sub-directory of the folder");
            }
            else
            {
                Files.move(resolvedSource, resolvedDestination.resolve(resolvedSource.getFileName()));
                if (Files.isDirectory(resolvedDestination))
                {
                    fsc.currentPath = resolvedDestination;
                }
            }
        }
        Path relativized = relativizeAPath(fsc.currentPath);
        result = new CommandResult("~%s".formatted(relativized.toString()), errorTracker );

    }
    static void cp(ParsedCommand command) throws IOException {

        Path source = Path.of(command.listOfArgs[0]);
        Path destination = Path.of(command.listOfArgs[1]);

        Path resolvedSource = fsc.currentPath.resolve(source);
        Path resolvedDestination = fsc.currentPath.resolve(destination);

        if (fsc.NavigateToAnExistingPath(resolvedSource) == null)
        {
            resolvedSource = fsc.programPath.resolve(source);

            if (fsc.NavigateToAnExistingPath(resolvedSource) == null)
            {
                errorTracker.add("Source fails to resolve!");
            }
        }

        if (fsc.NavigateToAnExistingPath(resolvedDestination) == null)
        {
            resolvedDestination = fsc.programPath.resolve(destination);

            if (fsc.NavigateToAnExistingPath(resolvedDestination) == null)
            {
                errorTracker.add("Destination fails to resolve");
            }
        }

        if (confirmValidPath(resolvedDestination) && confirmValidPath(resolvedSource))
        {
            if (Files.isDirectory(resolvedSource) && Files.exists(resolvedDestination) && Files.isDirectory(resolvedDestination)) {
                resolvedDestination = resolvedDestination.resolve(resolvedSource.getFileName());
            }
                try (Stream<Path> stream = Files.walk(resolvedSource)) {
                    Path finalResolvedDestination = resolvedDestination;
                    Path finalResolvedSource = resolvedSource;

                    stream.forEach(path -> {
                        try {
                            Path targetPath = finalResolvedDestination.resolve(finalResolvedSource.relativize(path));

                            if (Files.isDirectory(path)) {
                                if (!Files.exists(targetPath)) {
                                    Files.createDirectories(targetPath);
                                }
                            } else {
                                Files.copy(path, targetPath.resolve(finalResolvedSource.getFileName()), StandardCopyOption.REPLACE_EXISTING);
                            }
                        } catch (IOException e) {
                            errorTracker.add(e.getMessage());
                        }
                    });

                }
            }

        Path relativized = relativizeAPath(fsc.currentPath);
        result = new CommandResult("~%s".formatted(relativized.toString()), errorTracker );
    }


    static void cd(ParsedCommand command) throws IOException {
        String argument = command.listOfArgs[0];

        // First case, going to ParentFolder
        if (Objects.equals(argument, ".."))
        {
            Path backPath = fsc.currentPath.getParent();

            boolean isValid = confirmValidPath(backPath);

            if (isValid)
            {
                Path relativized = relativizeAPath(backPath);
                fsc.currentPath = backPath;
                result = new CommandResult(("~%s".formatted(relativized.toString())), errorTracker);
                return;
            }
            else
            {
                errorTracker.add("Error! Cannot go back further than the RootPath directory!");
                return;
            }
        }

        // Second Case, Changing directory FORWARDS
        else
        {
            Path resolvedPath = resolveAPathFromCurrent(Path.of(argument));
            PathType goodPath = fsc.NavigateToAnExistingPath(resolvedPath);

            if (goodPath != null)
            {
                if (confirmValidPath(goodPath.pathToFile)) {
                    fsc.currentPath = goodPath.pathToFile;
                    
                    Path relativized = relativizeAPath(fsc.currentPath);
                    result = new CommandResult(("~%s".formatted(relativized.toString())), errorTracker);
                    return;
                }
                else
                {
                    errorTracker.add("Error! Invalid directory!");
                }
            }

            // Third Case, Give the full Path to change directory to
            resolvedPath = resolveAPathFromRoot(Path.of(argument));
            goodPath = fsc.NavigateToAnExistingPath(resolvedPath);

            if (goodPath != null)
            {
                if (!confirmValidPath(goodPath.pathToFile))
                {
                    errorTracker.add("Error! Invalid directory!");
                }
                fsc.currentPath = goodPath.pathToFile;
            }
            else
            {
                errorTracker.add("Error! Invalid directory!");
            }

            Path relativized = relativizeAPath(fsc.currentPath);
            result = new CommandResult(("~%s".formatted(relativized.toString())), errorTracker);
        }
    }

    static void rm(ParsedCommand command) throws IOException {

        if (Objects.equals(command.listOfArgs[0], "-r")) // Special case, delete a folder/file even if stuff is in it
        {
            Path pathToDelete = Path.of(command.listOfArgs[1]);

            Path resolvedPath = fsc.currentPath.resolve(pathToDelete);

            if (fsc.NavigateToAnExistingPath(resolvedPath) != null) {
                if (confirmValidPath(resolvedPath)) // TODO: Add full path option for this using Navigate
                {
                    try (Stream<Path> walk = Files.walk(resolvedPath)) {
                        walk.sorted(Comparator.reverseOrder())
                                .forEach(path -> {
                                    try {
                                        Files.delete(path);
                                    } catch (IOException e) {
                                        System.out.printf("[SERVER ERROR] Failed to delete %s: %s%n", fsc.programPath.relativize(path), e.getMessage());
                                    }
                                });
                    } catch (IOException e) {
                        errorTracker.add("Failed to finish Recursive Folder Deletion due to I/O error");
                        return;
                    }
                } else {
                    errorTracker.add("Invalid Path to Delete");
                }
            }
            else {
                errorTracker.add("Could not find path marked for deletion");
            }

            return;
        }

        // Normal case, delete an empty folder or file
        Path pathToDelete = Path.of(command.listOfArgs[0]);

        Path resolvedPath = fsc.currentPath.resolve(pathToDelete);

        if (confirmValidPath(resolvedPath))
        {
            if (Files.isDirectory(resolvedPath))
            {
                try (Stream<Path> stream = Files.list(resolvedPath))
                {
                    if(stream.findAny().isPresent()) {
                        errorTracker.add("Cannot delete a directory with files/folders without '-r' tag");
                        return;
                    }
                }
                catch (Exception e)
                {
                    errorTracker.add("Internal File I/O Error -- Failed to delete ");
                    return;
                }
            }

            if (!fsc.currentPath.startsWith(resolvedPath) || (fsc.currentPath.startsWith(resolvedPath) && !Files.isDirectory(resolvedPath))) {
                Files.deleteIfExists(resolvedPath);
            }
            else
            {
                errorTracker.add("Cannot delete directory you are currently in");
            }
        }

        Path relativized = relativizeAPath(fsc.currentPath);
        result = new CommandResult("~%s".formatted(relativized.toString()), errorTracker );
    }

    // All Helper Methods  --------------------------------------------------------------------------------------

    static boolean confirmValidPath(Path fullNewPath)
    {
        if (fullNewPath.startsWith(fsc.rootPath))
        {
            return true;
        }
        return false;
    }

    static Path relativizeAPath(Path fullNewPath) // When resolving a Path used for printing the resolved result
    {
        return fsc.programPath.relativize(fullNewPath);
    }

    static Path resolveAPathFromCurrent(Path newAddition) // when moving to a new directory/file that is immediately ahead
    {
        return fsc.currentPath.resolve(newAddition);
    }

    static Path resolveAPathFromRoot(Path newAdditionFromRoot) // when moving to a new directory/file that specifies a whole path
    {
        return fsc.programPath.resolve(newAdditionFromRoot);
    }
}
