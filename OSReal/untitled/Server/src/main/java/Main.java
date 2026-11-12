import Enum.*;
import Handlers.CommandExecutor;
import Handlers.FileSystemController;
import Models.*;
import SharedModels.CommandResult;

import java.io.IOException;
import java.nio.file.Path;
import java.util.Arrays;
import java.util.Scanner;

public class Main {
    public static void main(String[] args) throws IOException {
        Scanner scanner = new Scanner(System.in);

        FileSystemController fsc = new FileSystemController();

        Boolean isQuit = false;

        while (!isQuit) {
            System.out.println("Enter your command: ");
            System.out.printf("~%s >> ".formatted(fsc.programPath.relativize(fsc.currentPath).toString()));
            String command = scanner.nextLine();

            String[] tokens = command.split(" ");

            if (tokens.length > 0) {
                String keyword = tokens[0];

                String[] arguments = Arrays.stream(tokens).skip(1).toArray(String[]::new);

                CommandParser parserEnum = CommandParser.valueOf("NULL");
                try {
                    parserEnum = CommandParser.valueOf(keyword.toUpperCase());
                } catch (IllegalArgumentException ignored) {
                    System.out.println("Error! Command does not exist!");
                    continue;
                }

                ParsedCommand fullCommandObject = parserEnum.parseCommand(arguments);

                CommandResult result = null;

                CommandExecutor executor = new CommandExecutor(fsc);

                if (fullCommandObject != null) {
                    result = executor.ProcessCommand(fullCommandObject);
                } else {
                    result = new CommandResult();
                    result.errors.add("Command does not exist.");
                }
                assert result != null;

                if (result.isQuit)
                {
                    isQuit = true;
                    continue;
                }

                System.out.println(result.ResultsToString());

            }
        }
    }
}