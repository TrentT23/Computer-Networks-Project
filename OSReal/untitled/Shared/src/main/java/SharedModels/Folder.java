package SharedModels;

import java.nio.file.Path;

public class Folder {
    Path fullFolderPath;
    String nameOfFolder;

    public Folder(Path folderPath, String nameOfFolder ) {
        this.fullFolderPath = folderPath;
        this.nameOfFolder = nameOfFolder;
    }
}
