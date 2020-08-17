using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace ChatClient
{

    // Handles all I/O with the local database, which stores messages
    public static class Database
    {
        // Location on disk of the database
        public static string MessagesDBLocation = @"URI=file:C:\ChatAppClient\messages.db";

        //Add a message from the server to the local DB
        public static void UpdateMessageTable(string recepient, string serverMessageString)
        {
            // Split the response from the server into an array containing the sender, content and timestamp
            string[] messageObject = serverMessageString.Split(';');

            // Set up the connection
            SQLiteConnection connection = new SQLiteConnection(MessagesDBLocation);
            connection.Open();

            if (!serverMessageString.Equals(""))
            {
                // Set up the insert command
                string commandText = "INSERT INTO Messages(Sender, Recipient, Content, Timestamp) VALUES (@sender, @recipient,@content,@timestamp);";
                SQLiteCommand insertCommand = new SQLiteCommand(commandText, connection);

                // Add the sender, recipient, content and timestamp values into the command
                insertCommand.Parameters.AddWithValue("@sender", messageObject[0]);
                insertCommand.Parameters.AddWithValue("@recipient", recepient);
                insertCommand.Parameters.AddWithValue("@content", messageObject[1]);
                insertCommand.Parameters.AddWithValue("@timestamp", messageObject[2]);

                // Execute the command and close up
                insertCommand.ExecuteNonQuery();
                Console.WriteLine("[INFO] Message added to the database");
                connection.Close();
            } else
            {
                Console.WriteLine("[INFO] No message to add to the database");
            }
        }


        // Get a complete list of people the user has chatted with, based on the senders in the local message DB
        public static List<String> GetFriendsList()
        {
            // List to store usernames of everyone the user has chatted with
            List<String> friends = new List<String>();

            // Set up the connection
            SQLiteConnection connection = new SQLiteConnection(MessagesDBLocation);
            connection.Open();

            // Set up the query and add the recipient value
            string commandText = "SELECT Sender FROM Messages WHERE Recipient=@recipient;";
            SQLiteCommand selectQuery = new SQLiteCommand(commandText, connection);
            selectQuery.Parameters.AddWithValue("@recipient", ClientShareData.GetUsername());

            // Set up the reader we can use to pull sender names attached to message records in turn
            SQLiteDataReader reader = selectQuery.ExecuteReader();

            if (reader.HasRows)
            {
                // Continually read sender names from the database and add new ones to the friends List
                while (reader.Read())
                {
                    string sender = reader.GetString(0);
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