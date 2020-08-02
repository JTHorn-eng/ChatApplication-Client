using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApplication_Client
{
    public static class ClientShareData
    {
        private static Queue<string> messageQueue = new Queue<string>();
        private static string username = "";
        public static void AddClientMessage(string m)
        {
            Console.WriteLine("New Message: " + m);
            messageQueue.Enqueue(m);
        }

        public static string getUsername()
        {
            return username;
        }

        public static void setUsername(string name)
        {
            username = name;
        }

        public static string readClientMessage()
        {
            if (messageQueue.Count > 0)
            {
                return messageQueue.Dequeue() + "<EOF>";
            } else
            {
                return "No new messages<EOF>";
            }
        }
       



    }
}
