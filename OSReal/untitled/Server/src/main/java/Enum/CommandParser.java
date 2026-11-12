package Enum;

import Models.ParsedCommand;

import java.util.Objects;

public enum CommandParser {
    MKDIR(1),
    CREATE(1),
    WRITE(2),
    READ(1),
    LS(0) ,
    CD(1) ,
    PWD(0) ,
    CP(2) ,
    MV(2) ,
    RM(1),
    QUIT(0),
    NULL(0);

    final CommandType type;
    int requiredArgs;

    CommandParser(int i) {
        this.type = CommandType.valueOf(this.name());
        this.requiredArgs = i;
    }

    public ParsedCommand parseCommand(String[] args)
    {

        if (this.type == CommandType.WRITE)
        {
            args = redoWriteSplit(args); // Write Command should only do the split for the first two spaces, so the ending string doesn't get messed up
        }

        if (this.type == CommandType.RM)
        {
            if (args.length > 0)
            {
                catchRemoveFlag(args);
            }
        }

        if (this.type == CommandType.LS) // LS will most likely have 0, but can have 1 argument for a directory
        {
            if (args.length > 0)
            {
                this.requiredArgs = 1;
            }
        }

        if (args.length != requiredArgs)
        {
            return null; // Incorrect Format, return null
        }
        else
        {
            return new ParsedCommand(type, args);
        }
    }

    String[] redoWriteSplit(String[] args)
    {
        String allArgs = String.join(" ", args);

        return allArgs.split(" ", 2);
    }

    void catchRemoveFlag(String[] args)
    {
        if(Objects.equals(args[0], "-r")) // Catch the -r flag for recursive deleting in the FIRST slot. If not in first slot, then ignore
        {
            requiredArgs = 2;
        }
    }
}
