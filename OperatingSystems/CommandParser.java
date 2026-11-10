package OperatingSystems;

import OperatingSystems.Enums.*;
import OperatingSystems.Models.ParsedCommand;

public enum CommandParser {
    MKDIR(1),
    CREATE(1),
    WRITE(2),
    READ(1),
    LS(1) ,
    CD(1) ,
    PWD(0) ,
    CP(2) ,
    MV(2) ,
    RM(2),
    QUIT(0); 

    CommandType type;
    int requiredArgs;

    CommandParser(int i) {
        this.type = CommandType.valueOf(this.name());
        this.requiredArgs = i;
    }


    public ParsedCommand parseCommand(String[] args)
    {
        if (args.length != requiredArgs)
        {
            System.out.println("Incorrect Format for Command");
            return null;
        }
        else
        {
            return new ParsedCommand(type, args);
        }
    }

    public ParsedCommand parsedCommand(String arg)
    {
        if (1 != requiredArgs)
        {
            System.out.println("Incorrect Format for Command");
            return null;
        }
        else
        {
            return new ParsedCommand(type, arg);
        }
    }

}
