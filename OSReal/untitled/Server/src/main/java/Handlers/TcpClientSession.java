package Handlers;

import Models.ParsedCommand;
import SharedModels.CommandResult;

import Enum.*;

import java.io.*;
import java.net.Socket;
import java.util.Arrays;

public class TcpClientSession {
    private static final String END_MARKER = "<END_RESPONSE>";
    FileSystemController clientFSC;
    CommandExecutor clientExec;
    Socket clientSocket;
    BufferedReader inputStream;
    PrintWriter outputStream;


    public TcpClientSession(Socket clientSocket) throws IOException {
        this.clientFSC = new FileSystemController();
        this.clientExec = new CommandExecutor(clientFSC);
        this.clientSocket = clientSocket;
        this.inputStream = new BufferedReader(new InputStreamReader(clientSocket.getInputStream()));
        this.outputStream = new PrintWriter(clientSocket.getOutputStream(), true);
    }

    public void StartSession() throws IOException {

        try {
            boolean isQuit = false;
            outputStream.println("~%s >> ".formatted(clientFSC.programPath.relativize(clientFSC.rootPath)));
            outputStream.println(END_MARKER);

            while (!isQuit) {
                String command = inputStream.readLine(); // Will block until command is received
                if (command == null) break;
                System.out.printf("[CLIENT] Recieved Command: %s\n", command);

                String[] tokens = command.split(" ");

                if (tokens.length > 0) {
                    String keyword = tokens[0];

                    String[] arguments = Arrays.stream(tokens).skip(1).toArray(String[]::new);

                    CommandParser parserEnum = CommandParser.valueOf("NULL");
                    try {
                        parserEnum = CommandParser.valueOf(keyword.toUpperCase());
                    } catch (IllegalArgumentException ignored) {
                        outputStream.println("Error! Command does not exist!\n" + END_MARKER);
                        continue;
                    }

                    ParsedCommand fullCommandObject = parserEnum.parseCommand(arguments);

                    CommandResult result = null;

                    if (fullCommandObject != null) {
                        result = clientExec.ProcessCommand(fullCommandObject);
                    } else {
                        result = new CommandResult();
                        result.errors.add("Command does not exist.");
                    }

                    if (result.isQuit) {
                        isQuit = true;
                    }

                    if (result != null)
                    {
                        outputStream.println(FormatResponse(result));
                    }
                }
            }
        }
        catch (Exception e)
        {
            System.out.printf("[SERVER ERROR] %s%n", e.getMessage());
            outputStream.println("Something went wrong, shutting down connection.");
        }
        finally {
            System.out
            clientSocket.close();
        }
    }


    String FormatResponse(CommandResult result)
    {
        return "%s\n\n~%s >> \n%s".formatted(result.ResultsToString(), clientFSC.programPath.relativize(clientFSC.currentPath), END_MARKER);
    }

}
