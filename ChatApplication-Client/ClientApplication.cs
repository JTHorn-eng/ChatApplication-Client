using System.Threading;

class ClientApplication
{
    public ClientApplication()
    {
        //connect to server
        //download messages for user
        //decrypt messages using private key
        //add messages to database
        //server deletes downloaded messages
        //populate friends UI scrollviewer with friends list from database

        //WHILE APPLICATION IS RUNNING
        //Stay connected to server so it can listen to client + friends events

        Thread sockClientThread = new Thread(Client.Start);
        sockClientThread.Start();
    }
}