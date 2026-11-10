package OperatingSystems.Models;
import OperatingSystems.Enums.*;;

public class ParsedCommand {
    CommandType type;
    String[] listOfArgs;

    public ParsedCommand(CommandType type, String[] args)
    {
        this.type = type;
        this.listOfArgs = args;
    }

    public ParsedCommand(CommandType type, String arg)
    {
        this.type = type;
        this.listOfArgs[0] = arg;
    }
}

