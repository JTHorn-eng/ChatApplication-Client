using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Windows;
using System.Linq;
using ChatServer;

namespace ChatClient
{

    // State object for receiving data from the server
    public class StateObject
    {
        // Client socket
        public Socket workSocket = null;
        // Size of receive buffer
        public const int BufferSize = 256;
        // Receive buffer
        public byte[] buffer = new byte[BufferSize];
        // Received data string
        public StringBuilder sb = new StringBuilder();
    }

    // Functions as an async socket client
    // Use with Client.Start()
    public static class Client
    {
        // Chat username. Should probably be relocated to another class
        public static string chatUsername;
        public static string chatRecipient = "alice_jones";
        public static string serverResponseMessages = "";
        public static string currentMessage = ""; 
        public static bool isConnected = false;
        public static bool nonSocketException = false;

        // MREs for signalling when threads may proceed
        private static ManualResetEvent connectionDone = new ManualResetEvent(false);
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);

        private const string ServerIP = "127.0.0.1";
        private const int ServerPort = 8182;

        // Socket to connect to the server
        private static Socket client;

        public static bool initialised = false;

        // Initialises connection to server then keeps listening for data from server and sending messages entered into the GUI
        public static void Start()
        {

            //Initialise security attributes, use chat username as keycontainer
            //TODO: Change container name to something more secure
            ClientSecurity.Init(chatUsername);

            // Get username as entered in the GUI
            chatUsername = ClientShareData.GetUsername();

            //Constantly attempt to connect to server recursively, with timeout
            AttemptConnection();
            
            //if the client is connected and the connection attempt hasn't returned a nonSocketException i.e. the program works
            //with no errors, then procede with client processes
            if (isConnected || !nonSocketException)
            {
                // Signal to the MainWindow thread that we're connected and messages have been updated so it can get the list of senders ("friends") to display in the GUI
                initialised = true;

                Console.WriteLine("[INFO] Running client processes");
                RunClientProcesses();
            }
            
        }

        //Attempt to connect to server
        public static void AttemptConnection()
        {
                try
                {
                    
                    // Initialises endpoint (IP + port) to connect to
                    IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse(ServerIP), ServerPort);

                    // Create a TCP/IP socket
                    client = new Socket(IPAddress.Parse(ServerIP).AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                    Console.WriteLine("[INFO] Connecting to server");

                    // Connect to the server
                    client.BeginConnect(remoteEndPoint, new AsyncCallback(ConnectCallback), client);

                    // Wait until connection is made
                    connectionDone.WaitOne();

                    Console.WriteLine("[INFO] Connected to server");

                // Perform handshake to get established with server
                if (isConnected)
                {
                    ChatHandshake(client);
                } 

                }
                catch (SocketException e)
                {
                    Console.WriteLine(e.ToString());
                    //timeout
                    Thread.Sleep(3000);
                    AttemptConnection();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    nonSocketException = true;
                }                
        }

        //Run any client methods when connected to server
        private static void RunClientProcesses()
        {
            while(true)
            {
                StateObject state = new StateObject();
                state.workSocket = client;

                // Set up a callback for when the client receives data from the server
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);

                receiveDone.Reset();
               
                //Check if recipient name has changed
                if (!(chatRecipient.Equals(ClientShareData.getGUIRecipient())))
                {
                    chatRecipient = ClientShareData.getGUIRecipient();
                }

                //TODO reload client-to-client messages from server and local database


                bool dataReceived = false;

                // Every 100 milliseconds, perform client processes
                while (!dataReceived)
                {
                    //If person has clicked send in the GUI, send the message.
                    if (ClientShareData.GetSendButtonClicked())
                    {
                        //handle all messages in the queue.
                        foreach (string message in ClientShareData.GetMessageQueue())
                        {
                            if (!message.Equals(""))
                            {
                                Console.WriteLine("New Message: " + message);
                                Send(client, "MESSAGES:<SOR>" + chatRecipient + "<EOR><SOT>" + Encoding.ASCII.GetString(ClientSecurity.Encrypt(message)) + "<EOT><EOF>");
                            }
                        }

                        for (int x = 0; x < ClientShareData.GetMessageQueue().Count; x++)
                        {
                            string messageRead = ClientShareData.ReadClientMessage();
                        }
                        ClientShareData.SetSendButtonClicked(false);
                    }

                    // Check whether we've received data from the client (but do not wait)
                    dataReceived = receiveDone.WaitOne(0);
            
                }
                //Split message into recipient and content
                string receivedMessage = state.sb.ToString().Replace("MESSAGES:","");
                string recipient = "";
                string content = "";
                for (int x = receivedMessage.IndexOf("<SOR>") + 4; x < receivedMessage.LastIndexOf("<EOR>") ; x++)
                {
                    recipient += receivedMessage[x];
                }
                for (int x = receivedMessage.IndexOf("<SOT>") + 4; x < receivedMessage.LastIndexOf("<EOT>"); x++)
                {
                    content += receivedMessage[x];
                }


                //Decrypt received message

                content = ClientSecurity.Decrypt(Encoding.ASCII.GetBytes(content));


                //Update local database table with received message
                Database.UpdateMessageTable(string.Join("", receivedMessage));

                // Reset the buffer so we can receive more data from it
                state.sb = new StringBuilder();
                state.buffer = new byte[StateObject.BufferSize];
            }
        }

        // Closes connection to server
        public static void Stop()
        {
            try
            {
                client.Shutdown(SocketShutdown.Both);
                client.Close();
                //Force close all running threads
                Environment.Exit(Environment.ExitCode);
            } catch (Exception e)
            {
                Console.WriteLine("Error when closing: " + e.Message);
                //Force close all running threads
                Environment.Exit(Environment.ExitCode); 
            }
        }

        // Handle connection to the server
        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection
                client.EndConnect(ar);

                // Signal that the connection has been made so the main thread can continue
                connectionDone.Set();

                //Set isConnected to true to prevent anymore connection attempts to server
                isConnected = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not complete connect callback" + e.ToString());
                //timeout
                Thread.Sleep(3000);
                AttemptConnection();
            }
        }

        // Run through a set process to get this client established after connection
        // Sends the client's chat username, its public key (if needed) and receives new messages for the client
        private static void ChatHandshake(Socket client)
        {
            

            Console.WriteLine("[INFO] Sending username to the server");

            // Send our chat username to the server
            Send(client, String.Format("IDENTIFICATION:{0}<EOF>", chatUsername));

            Console.WriteLine("[INFO] Receiving response from server");

            // Receive either messages or a key request from the server
            string serverResponse = Receive(client);

            // Server responds with either "KEY_GEN_REQUEST: <EOF>" or "MESSAGES: dummy_messages<EOF>"
            // We parse this string to find out which one
            string serverCommand = serverResponse.Split(":".ToCharArray())[0];

            if (serverCommand.Equals("KEY_GEN_REQUEST"))
            {
                // The server has requested a public key
                Console.WriteLine("[INFO] Key pair generation request received");
                Console.WriteLine("[INFO] Generating key pair and sending public key to server");

                //RSA public key already generated in Init method.
                // Send the pub key to the server
                Send(client, "PUBKEY:" + ClientSecurity.GetRSAPublicKey(chatUsername) + " <EOF>");

                // Recieve messages from the server
                serverResponse = Receive(client);
            }

            // We've got messages from the server
            Console.WriteLine("[INFO] Messages downloaded from server: " + serverResponse);

            //Add messages to the local database and display in GUI
            Database.UpdateMessageTable(serverResponse.Replace("<EOF>", "").Replace("MESSAGES:", ""));
            serverResponseMessages = serverResponse;
            isConnected = true;
        }

        //Get current server response messages
        public static string GetServerResponseMessages()
        {
            return serverResponseMessages;
        }

        // Receive data from the server. Blocks parent thread execution
        private static string Receive(Socket client)
        {
            try
            {
                // Create the state object
                StateObject state = new StateObject();
                state.workSocket = client;

                // Reset the MRE so we pause until the server responds
                receiveDone.Reset();

                // Set up a callback for when the server begins sending data
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);

                // Wait for the data to be received
                receiveDone.WaitOne();

                // Get the received data
                string data = state.sb.ToString();

                return data;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());

                return "";
            }
        }

        // Callback to handle receiving data from server
        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the server
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // Append the data we've just got to the string builder
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    // Check for end-of-file tag
                    if (state.sb.ToString().IndexOf("<EOF>") > -1)
                    {
                        // We've got all the data so let the parent thread proceed if it's waiting for the data
                        receiveDone.Set();

                        // The parent thread can get the data receieved with state.sb.ToString();
                    }
                    else
                    {
                        // Get some more data
                        client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        // Send data to the server. Blocks thread execution
        private static void Send(Socket client, String data)
        {
            // Convert the string to send into byte data using ASCII encoding
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Reset the MRE so we pause until sending is completed
            sendDone.Reset();

            // Begin sending the data to the server
            client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);

            // Wait for the data to be sent
            sendDone.WaitOne();
        }

        // Callback to handle sending data to the server
        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the client socket
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the server
                int bytesSent = client.EndSend(ar);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            // We've sent the data so let the parent thread proceed
            sendDone.Set();
        }
    }
}