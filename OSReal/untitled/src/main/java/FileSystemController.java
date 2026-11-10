import Models.PathType;

import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;

import Enum.FileType;

public class FileSystemController {

    public Path programPath;
    public Path rootPath;
    public Path currentPath;

    public Path resolvedRoot;

    public static FileSystemController fsc; // Singleton for other classes

    static {
        try {
            fsc = new FileSystemController();
        } catch (IOException e) {
            throw new RuntimeException(e);
        }
    }

    public FileSystemController() throws IOException {
        Path programPath = Path.of("").toAbsolutePath(); // Get the path of the program

        Path dataPath = programPath.resolve("RootPath");
        Files.createDirectories(dataPath);

        this.programPath = programPath;

        this.rootPath = dataPath;
        this.currentPath = dataPath;

        this.resolvedRoot = this.programPath.resolve(this.rootPath);
    }

    public PathType NavigateToAnExistingPath(Path newPath)
    {
        Path pathCut = this.rootPath.relativize(newPath); // Cut off the new pathing for the
        Path fullPath = rootPath.resolve(pathCut).toAbsolutePath();

        if (Files.exists(fullPath) && Files.isRegularFile(fullPath)) {
            return new PathType(fullPath, FileType.FILE);
        }
        else if (Files.exists(fullPath) && Files.isDirectory(fullPath)) {
            return new PathType(fullPath, FileType.DIRECTORY);
        }
        else {
            System.out.println("[SERVER WARNING] Invalid Path: %s".formatted(this.programPath.relativize(fullPath).toString()));
            return null;
        }
        // Add the new path to the rootPath
        // (this may seem redundant, but it actually ensures security,
        // user cannot navigate outside of this root directory)
    }
    public void FileSemaphore()
    {

    }
    public void CreateAFolder(Path newPathForFolder)
    {

    }
    public void CreateAFile(Path newPathForFile)
    {

    }
}
