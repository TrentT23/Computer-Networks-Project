# Trent Tiegs
# Computer Networks Programming Assignment
# CSC 474
# Dr. Jun Huang

# FILE DIRECTORY
All of the files are in the 'NetworksAssign1 Directory!

This ReadMe contains the base operations needed to run both the CLIENT and the SERVER FILES, along with a description of the project. 
There are many files in this project, but the ones of interest are: 

# FILES OF INTEREST ----------------------------------------------------------------
CLIENT/SERVER MODEL FILE
- SharedModels/Models.cs (CONTAINS CLASS DEFINTIONS FOR BOTH SERVER AND CLIENT)

CLIENT FILE
- clientFile.cs (CLIENT FILE)

SERVER FILES
- fileSystem.cs (CONTAINS CODE THAT RUNS FILE/DIRECTORY OPERATIONS)
- Program.cs (SERVER FILE)

These files contain all the code necessary for this project. Please note that while I designed most of the code functionality, I used Codex AI to help with learning socket/file IO functionality, fix errors in my code, and create descriptions and help with documentation. 

# How to Run Program: -------------------------------------------------------------

Things you will need to have installed: 
.NET 10 SDK

# Build Commands: 

- dotnet build .\ClientApp.csproj
- dotnet build .\NetworksAssign1.csproj

# RUN SERVER: 
** IMPORTANT NOTICE ** -- If you run the server file, you will be exposing your computer on port 8080! By default this will be LAN only, unless you are using Port Forwarding or some other tool to expose your computer on a broader scale. While the Server Program explicitly requires the client program to be interfaced with, this is still something to keep in mind!

After building the server, you can run it with: 

- dotnet run --project .\NetworksAssign1.csproj

The server will start by default on Port 8080. Any user that has the clientFile executable and is able to reach your computer via an IP can connect.
If you run this one yoour computer, it will create a new 'ServerFiles' directory in your bin folder. This is where your clients files will be stored. 
Any time after a client connects it will use the same 'ServerFiles' folder. 

# RUN CLIENT: 

After Building the client, you can run it with: 

- dotnet run --project .\ClientApp.csproj

The program will ask you to enter an IP in (if you leave blank, this will by default use my TailScale IP. To test it on your own, you will need to enter your own). 

Once a connection is established, you can began interfacing with the file system. 


# DESCRIPTION OF PROJECT  ----------------------------------------------------------------------------------------------------: 
This project contains programs that allow for a multi-user, TCP-based file system that allows clients to remotely browse, create, modify, move, and delete files and folders on a server. Each connected client receives its own isolated directory tree, ensuring users cannot interfere with each other’s data.

This project demonstrates:
- TCP socket programming
- Multi-threaded server design
- Custom text-based command protocol
- JSON-based server → client messaging
- Real filesystem manipulation
- Clean separation of concerns (Server, Client, Shared Models, FileSystem engine)

------------------------------------------------------------
## Features
------------------------------------------------------------
- Multi-User Support: Each client runs on its own thread with its own directory root.
- Per-User Root Directory: The server creates ServerFiles/<username>/ for each new client.
   * It's important to note that this will likely be in your directory's bin folder! * 
- Full File & Folder Operations: Read, append, overwrite, create, delete, move, enter, exit.
- JSON Communication: Server responses are serialized into ServerStatusResponse.
- Command-Driven Interface: Clean, structured command parsing using enums.

------------------------------------------------------------
## Project Structure
------------------------------------------------------------
/Server
    Program.cs
    FileSystem/
        DirectoryRoot.cs
    SharedModels/
        SharedModels.cs

/Client
    Program.cs
    SharedModels/
        SharedModels.cs

------------------------------------------------------------
## How the Server Works
------------------------------------------------------------
1. Starts a TCP listener on port 8080.
2. Accepts clients in a loop.
3. Spawns a new thread for each client.
4. Reads the username and creates a dedicated directory for that user.
5. Sends initial folder listing and command list.
6. Enters the main command loop until the client disconnects or sends QUIT.

Each client receives a UserSession containing:
- A username
- A unique DirectoryRoot instance

------------------------------------------------------------
## How the Client Works
------------------------------------------------------------
1. Prompts for a server IP (or uses default).
2. Connects to the server via TCP.
3. Sends a username.
4. Starts an async listener for server messages.
5. Reads user input and sends commands.
6. Prints folder listings and server messages in a clean format.

------------------------------------------------------------
## Command List
------------------------------------------------------------
READ <index>                Read a file
APPEND <index>              Append text to a file
OVERWRITE <index>           Replace file contents
CREATE FILE <name>          Create a new file
CREATE FOLDER <name>        Create a new folder
DELETE <index>              Delete file or folder
MOVE FILE <item> <folder>   Move file into folder
MOVE FOLDER <item> <folder> Move folder into folder
ENTER <index>               Enter a folder
EXIT                        Go to parent folder
HELP                        Show commands
QUIT                        Disconnect

------------------------------------------------------------
## Networking Notes
------------------------------------------------------------
- Server listens on TCP port 8080.
- Multiple clients can connect simultaneously.


