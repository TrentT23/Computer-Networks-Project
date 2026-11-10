package OperatingSystems;
import OperatingSystems.Enums.*;
import OperatingSystems.Models.*;
public class Main {
    public static void main(String[] args)
    {
        System.out.println("Hello!");

        String command = "mkdir newFile";

        String[] tokens = command.split("");
        String keyword = tokens[0];
        String arg = tokens[1];

        CommandParser parserEnum = CommandParser.valueOf(keyword);

        ParsedCommand fullCommandObject = parserEnum.parsedCommand(arg);
        System.out.println(fullCommandObject);
    }
}