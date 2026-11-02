package OperatingSystems;

public enum CommandList {
    MKDIR("mkdir", 1) {

    },
    CREATE("create", 1) {
        
    },
    WRITE("write", 2) {

    },
    READ("read", 1) {

    },
    LS("ls", 1) {

    },
    CD("cd", 1) {

    },
    PWD("pwd"){

    },






    String keyword;
    int requiredArgs;

    CommandList(String string, int i) {
        this.keyword = string;
        this.requiredArgs = i;
    }
    CommandList(String string) {
        this.keyword = string;
        this.requiredArgs = 0;
    }
}
