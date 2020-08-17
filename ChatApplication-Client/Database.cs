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
        public static void UpdateMessageTable(string recipient, string serverMessageString)
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
                insertCommand.Parameters.AddWithValue("@recipient", recipient);
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
            string commandText = "SELECT Sender, Recipient FROM Messages;";
            SQLiteCommand selectQuery = new SQLiteCommand(commandText, connection);

            // Set up the reader we can use to pull sender names attached to message records in turn
            SQLiteDataReader reader = selectQuery.ExecuteReader();

            string thisUser = ClientShareData.GetUsername();

            if (reader.HasRows)
            {
                // Continually read sender names from the database and add new ones to the friends List
                while (reader.Read())
                {
                    string sender = reader.GetString(0);
                    if (!friends.Contains(sender) && sender != thisUser)
                    {
                        friends.Add(sender);
                    }
                    string recipient = reader.GetString(1);
                    if (!friends.Contains(recipient) && recipient != thisUser)
                    {
                        friends.Add(recipient);
                    }
                }
            }

            // Close up and return
            connection.Close();
            return friends;
        }

        // Returns all encrypted message object strings from the messages DB, where a given user is the sender or recipient
        // Message objects are separated by semicolons
        public static string RetrieveUserMessages(string username)
        {
            // Set up the connection
            SQLiteConnection connection = new SQLiteConnection(MessagesDBLocation);
            connection.Open();

            // Set up the query and insert the username value
            string commandText = "SELECT * FROM Messages WHERE Recipient=@username OR Sender=@username;";
            SQLiteCommand selectQuery = new SQLiteCommand(commandText, connection);
            selectQuery.Parameters.AddWithValue("@username", username);

            // Set up the reader we can use to pull message records in turn
            SQLiteDataReader reader = selectQuery.ExecuteReader();

            string messagesString = "";

            // Return a blank string if there are no messages for the user
            if (!reader.HasRows)
            {
                return "";
            }

            // Continually read message records from the database to compile all the message objects into a string
            while (reader.Read())
            {
                messagesString += reader.GetString(1) + ";" + reader.GetString(2) + ";" + reader.GetString(3) + "|";
            }

            // Close up and return
            connection.Close();
            return messagesString;
        }
    }
}