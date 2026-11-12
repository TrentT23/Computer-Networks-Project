import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.PrintWriter;
import java.net.Socket;
import java.util.Objects;
import java.util.Scanner;

//TIP To <b>Run</b> code, press <shortcut actionId="Run"/> or
// click the <icon src="AllIcons.Actions.Execute"/> icon in the gutter.
public class ClientMain {
    private static final String END_MARKER = "<END_RESPONSE>";

    public static void main(String[] args) throws IOException {
        Scanner scanner = new Scanner(System.in);

        System.out.print("Enter an IP: ");
        String ip = scanner.nextLine();

        System.out.print("Enter a Port: ");
        int port = Integer.parseInt(scanner.nextLine());

        try (Socket socket = new Socket(ip, port)) {
            // create a socket to connect to the server running on localhost at port number 9090

            PrintWriter outputStream = new PrintWriter(socket.getOutputStream(), true);
            BufferedReader inputStream = new BufferedReader(new InputStreamReader(socket.getInputStream()));

            boolean isQuit = false;

            String response = readResponse(inputStream);

            System.out.print(response); // Connection Successful

            while (!isQuit) {

                String commandToSend = scanner.nextLine();

                outputStream.println(commandToSend);

                response = readResponse(inputStream);

                System.out.print(response); // First thing should be ~RootPath

                if (Objects.equals(commandToSend.toUpperCase(), "QUIT")) {
                    isQuit = true;
                }
            }
        }
        catch (Exception e){
            System.out.println("Server connection failed/shut down. Please try again");
        }
    }

    private static String readResponse(BufferedReader inputStream) throws IOException {
        StringBuilder response = new StringBuilder();
        String line;
        while ((line = inputStream.readLine()) != null && !line.equals(END_MARKER)) {
            if (response.length() > 0) response.append(System.lineSeparator());
            response.append(line);
        }
        return response.toString();
    }
}
