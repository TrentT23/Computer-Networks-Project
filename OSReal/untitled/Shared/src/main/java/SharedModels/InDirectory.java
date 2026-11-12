package SharedModels;

import java.util.ArrayList;

public class InDirectory {
    public ArrayList<File> files;
    public ArrayList<Folder> folders;

    public InDirectory(ArrayList<File> files, ArrayList<Folder> folders ) {
        this.files = files;
        this.folders = folders;
    }
}
