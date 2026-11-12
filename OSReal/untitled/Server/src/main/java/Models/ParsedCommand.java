package Models;
import Enum.*;;

public class ParsedCommand {
    public CommandType type;
    public String[] listOfArgs;

    public ParsedCommand(CommandType type, String[] args)
    {
        this.type = type;
        this.listOfArgs = args;
    }

    @Override
    public String toString(){
        String returnMessage = "Type: %s\n".formatted(type.toString());
        StringBuilder arguments = new StringBuilder();

        arguments.append(returnMessage);
        for (int i = 1; i <= listOfArgs.length; i++)
        {
            arguments.append("Argument %d: %s\n".formatted(i, listOfArgs[i - 1]));
        }

        return arguments.toString();
    }
}

