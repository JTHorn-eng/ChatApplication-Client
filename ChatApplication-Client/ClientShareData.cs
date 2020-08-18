using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.RightsManagement;

namespace ChatClient
{
    public static class ClientShareData
    {
        //queue for reading and writing messages between GUI thread and clientSocket thread
        public static Queue<string> _MessageQueue = new Queue<string>();
        public static Queue<string> MessageQueue {get => _MessageQueue;}
        public static bool sendButtonClicked { get; set; } = false;
        public static string username { get; set; } = "";
        public static string password { get; set; } = "";

        private static string guiRecipient = "";
        public static bool MessageReceived = false;
    
        public static string GetGUIRecipient()
        {
            return guiRecipient;
        }

        public static void SetGUIRecipient(string name) {
            guiRecipient = name;
            Client.SetRecipient(guiRecipient);
        }

        public static void AddClientMessage(string m)
        {
            MessageQueue.Enqueue(m);
        }

        public static string ReadClientMessage()
        {
            return (MessageQueue.Count > 0) ? MessageQueue.Dequeue() + "<EOF>" : "";
        }

        public static string GetTimestamp()
        {
            DateTime utcTime = DateTime.UtcNow;
            return utcTime.ToString("HH-mm-ss-dd-MM-yyyy");
        }
    }
}