package Models;

import java.nio.file.Path;
import Enum.FileType;

public class PathType {
    public Path pathToFile;
    public FileType typeOfFile;

    public PathType(Path pathToFile, FileType typeOfFile)
    {
        this.pathToFile = pathToFile;
        this.typeOfFile = typeOfFile;
    }
}
