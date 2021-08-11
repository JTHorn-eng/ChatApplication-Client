# ChatApplication-Client
C# secure client chat application

# About
This project was an attempt at creating a chat application. This part of the application contains a simple GUI for the client to display sent/received messages
as well as sending them. Using a client-server-client model, the client uses AES to communicate messages to the server, 
where the public key is RSA encrypted. The client stores the machine key which 'hides' the private key used for encrypting the AES algorithms' public key
and also is responsible for sending a digital signature (not fully implemened). It is strongly advised not to use this program as it is only an attempt at 
creating a secure chat application and the system has not been verified by a security expert.

# Usage

It is strongly recommended not to use this system for messaging as it is only a demonstration, however examining the project is perfectly acceptable. Create a directory called ChatApplication-Server, use git clone, copy and paste files into directory and delete .git folder, then copy the files under the database directory to C:\ChatAppServer

