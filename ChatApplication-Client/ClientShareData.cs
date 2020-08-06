using System;
using System.Collections.Generic;
using System.Security.RightsManagement;

namespace ChatClient
{
    public static class ClientShareData
    {
        //queue for reading and writing messages between GUI thread and clientSocket thread
        public static Queue<string> messageQueue = new Queue<string>();
        private static string username = "";
        private static bool sendButtonClicked = false;


        public static Queue<String> GetMessageQueue()
        {
            return messageQueue;
        }
        public static bool GetSendButtonClicked()
        {
            return sendButtonClicked;
        }

        public static void SetSendButtonClicked(bool a)
        {
            sendButtonClicked = a;
        }

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
                return "";
            }
        }
    }
}