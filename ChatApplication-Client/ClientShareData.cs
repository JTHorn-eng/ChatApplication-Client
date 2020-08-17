using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.RightsManagement;

namespace ChatClient
{
    public static class ClientShareData
    {
        //queue for reading and writing messages between GUI thread and clientSocket thread
        public static Queue<string> messageQueue = new Queue<string>();
        private static string username = "";
        private static string guiRecipient = "";
        private static bool sendButtonClicked = false;

        public static bool messageReceived = false;
      


        public static Queue<String> GetMessageQueue()
        {
            return messageQueue;
        }
        public static bool GetSendButtonClicked()
        {
            return sendButtonClicked;
        }

        public static string getGUIRecipient()
        {
            return guiRecipient;
        }

        public static void setGUIRecipient(string name) {
            guiRecipient = name;
            Client.SetRecipient(guiRecipient);
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

        public static string GetTimestamp()
        {
            DateTime utcTime = DateTime.UtcNow;
            return utcTime.ToString("HH-mm-ss-dd-MM-yyyy");
        }
    }
}