import Handlers.TcpClientSession;

import java.io.IOException;
import java.net.ServerSocket;
import java.net.Socket;

public class ServerMain {
    public static void main(String[] args) throws IOException {

        try (ServerSocket serverSocket = new ServerSocket(5000)) {
            System.out.println("Server is Waiting for Clients!");

            while(true) {
                Socket clientSocket = serverSocket.accept();
                AcceptAConnection(clientSocket);
            }
        }
        catch (Exception e)
        {
            System.out.printf("Error occurred: %s%n", e.getMessage());
            System.out.println("Server Shutting Down.");
        }
    }

    static void AcceptAConnection(Socket clientSocket) {
        new Thread(() -> {
            try (Socket socket = clientSocket) {
                System.out.println("Client Connected -- Started New Thread");
                new TcpClientSession(socket).StartSession();
            } catch (IOException e) {
                System.out.printf("Error occurred: %s%n", e.getMessage());
            }
        }).start();
    }
}