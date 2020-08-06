using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows.Documents;

namespace ChatClient
{
    public static class Database
    {

        private const string MessagesDBLocation = @"URI=file:C:\ChatAppClient\messages.db";
        private static Int32 localID = 0; //stores the most recent id for a message in the Messages table

        //Retrieve the most recent ID value from the Messages table
        private static void GetUpdatedID()
        {
            SQLiteConnection connection = new SQLiteConnection(MessagesDBLocation);
            connection.Open();
            string commandText = "SELECT rowid FROM Messages WHERE rowid = (SELECT MAX(rowid) FROM Messages) ";
            SQLiteCommand cmd = new SQLiteCommand(commandText, connection);
            SQLiteDataReader r = cmd.ExecuteReader();
            if (r.Read())
                localID = (Int32)r.GetInt64(0);
            connection.Close();
        }

        //Get current timestamp for message objects
        public static String GetTimestamp(this DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssfff");
        }

        //Add messages from the server to the client's Messages table
        public static void UpdateMessageTable(string sender, string serverResponse)
        {
            string[] messageObject = serverResponse.Split(':');
            SQLiteConnection connection = new SQLiteConnection(MessagesDBLocation);
            connection.Open();

            if (!serverResponse.Equals(""))
            {

                //retrieve timestamp
                string currentTimestamp = GetTimestamp(new DateTime());

                //find most recent ID
                GetUpdatedID();

                //form insert SQL statement
                string commandText = "INSERT INTO Messages(rowid, Sender, Recipient, Content, Timestamp) VALUES ('" + ((Int32)(localID + 1)).ToString() + "','" + messageObject[2] + "','" + messageObject[3] + "','" + messageObject[4] + "','" + messageObject[5] + "');";
                SQLiteCommand cmd = new SQLiteCommand(commandText, connection);
                cmd.ExecuteNonQuery();
                Console.WriteLine("Inserted Values To Message Table");
                connection.Close();
            } else
            {
                Console.WriteLine("[INFO] All messages up-to-date");
            }
        }


        // Get a complete list of people the user has chatted with, based on the senders in the local message DB
        public static List<String> GetFriendsList()
        {
            // List to store usernames of everyone the user has chatted with
            List<String> friends = new List<String>();

            // Set up the connection and query
            SQLiteConnection connection = new SQLiteConnection(MessagesDBLocation);
            connection.Open();
            string commandText = "SELECT Sender FROM Messages WHERE Recipient='" + ClientShareData.GetUsername() + "';";
            SQLiteCommand select = new SQLiteCommand(commandText, connection);

            // Set up the reader we can use to pull sender names attached to message records in turn
            SQLiteDataReader rdr = select.ExecuteReader();

            if (rdr.HasRows)
            {
                // Continually read sender names from the database and add new ones to the friends List
                while (rdr.Read())
                {
                    string sender = rdr.GetString(0);
                    if (!friends.Contains(sender))
                    {
                        friends.Add(sender);
                    }
                }
            }

            // Close up and return
            connection.Close();
            return friends;
        }
    }
}