using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chat.client {


    /**
     * INIT =================
     * create remote endpoint object
     * 
     */

    public class StateObject
    {
        public string clientName = "";
        public Socket workSocket = null;
        public const int BufferSize = 256;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();
    }
    public class Client
    {

        private IPAddress clientIP;
        private IPEndPoint ipe;
        private const int PORT_NUMBER = 25565;
        private static ManualResetEvent connectionAcceptThread;
        private static ManualResetEvent sendThread;
        private StateObject localState;

        private IPAddress ipAddress;
        private bool downloadAcknowledge = false;


        public Client()
        {

            clientIP = IPAddress.Parse("127.0.0.1");
           
            ipe = new IPEndPoint(clientIP.Address, PORT_NUMBER);
            connectionAcceptThread = new ManualResetEvent(false);
            sendThread = new ManualResetEvent(false);
            localState = new StateObject();
            Connect();


        }
            

     
        public void Connect()
        {
            //setup timeout
            Stopwatch localAppTime = new Stopwatch();
            localAppTime.Start();
           
            

            Socket client = new Socket(clientIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            //attempt to connect to server
            connectionAcceptThread.Reset();
            Console.WriteLine("Searching for server");
            int connectionStartTime = localAppTime.Elapsed.Milliseconds;


            
                try
                {
                    client.BeginConnect(ipe, new AsyncCallback(ConnectCallback), client);
                } catch(Exception e)
                {
                    Console.WriteLine("Error trying to connect: " + e.Message);
                }
            

                sendThread.Reset();
                Console.WriteLine("Attempting to send message request");
                SendMessagesDownloadRequest(client, "MESSAGE_REQUEST"); //attempt to download server messages from most recent timestamp
     
            connectionAcceptThread.WaitOne();

            //request server messagess 
            //SendInfo(client, getName());
            sendThread.WaitOne();

            try
            {
                Console.WriteLine("Ending connection to server");
                client.Shutdown(SocketShutdown.Both);
            } catch (Exception e)
            {
                Console.WriteLine("Error when sending message: " + e.Message);
            }
            localAppTime.Stop();
            client.Close();

            
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            connectionAcceptThread.Set();
            try
            {


                Socket client = (Socket)ar.AsyncState;
                //add timeout 
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}", client.RemoteEndPoint.ToString());

            }
            catch (SocketException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void SendInfo(Socket client, String data)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            if (client.Connected)
            {
                try
                {
                    client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private void SendMessagesDownloadRequest(Socket client, String data)
        {
 
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            if (client.Connected)
            {
                try
                {
                    Console.WriteLine("Trying to send data: " + data);

                    client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
                } catch (Exception e) {Console.WriteLine("Error in sending message request: " + e.Message);}
            }
        }
        

        private static void SendCallback(IAsyncResult ar)
        {
            sendThread.Set();

            try
            {
                Socket client = (Socket)ar.AsyncState;
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Send {0} bytes to server. ", bytesSent);

                

            } catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }   
    }
}