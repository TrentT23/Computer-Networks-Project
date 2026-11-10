package Models;

import java.nio.file.Path;

public class File {
    Path fullFilePath;
    String nameOfFile;

    public File(Path filePath, String nameOfFile) {
        this.fullFilePath = filePath;
        this.nameOfFile = nameOfFile;
    }
}
