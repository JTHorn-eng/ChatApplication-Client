using System;
using System.Collections.Generic;

namespace ChatClient
{
    public static class ClientShareData
    {
        //queue for reading and writing messages between GUI thread and clientSocket thread
        private static Queue<string> messageQueue = new Queue<string>();
        private static string username = "";
        
        public static void AddClientMessage(string m)
        {
            messageQueue.Enqueue(m);
        }

        public static string GetUsername()
        {
            return username;
        }

        public static void SetUsername(string name)
        {
            username = name;
        }

        public static string ReadClientMessage()
        {
            if (messageQueue.Count > 0)
            {
                return messageQueue.Dequeue() + "<EOF>";
            }
            else
            {
                return "No new messages<EOF>";
            }
        }
    }
}