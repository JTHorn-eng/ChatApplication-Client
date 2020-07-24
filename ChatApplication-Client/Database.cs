using System;
using System.Data.SQLite;

namespace Chat.client
{
    class Database
    {

        private string databaseDirectory = "Chat/dbfiles/messages.db";
        private SQLiteConnection connection;
        private int localID = 0;
        public Database()
        {
            try
            {
                //setup connection to database
                connection = new SQLiteConnection(databaseDirectory);
                connection.Open();
            } catch (Exception e)

            {
                Console.WriteLine("Error: " + e.Message);
            }
        }

        public void createMessageTable()
        {
            //check if table exists


            //create Message table
            SQLiteCommand cmd = new SQLiteCommand(connection);
            cmd.CommandText = "CREATE TABLE IF NOT EXISTS Messages(ID, Sender, Recipient, Content, Timestamp)";
            cmd.ExecuteNonQuery();
            Console.WriteLine(" \n Created new table \n ");
        }

        public void insertIntoMessageTable(string sender, string recipient, string content)
        {
            //retrieve timestamp
            string timeStamp = DateTime.Now.ToString();

            //form insert SQL statement
            SQLiteCommand cmd = new SQLiteCommand(connection);
            cmd = new SQLiteCommand(connection);
            cmd.CommandText = "INSERT INTO Messages(ID, Sender, Recipient, Content, Timestamp) VALUES(@ID, @Sender, @Recipient, @Content, @Timestamp)";
            cmd.Parameters.AddWithValue("@ID", localID);
            cmd.Parameters.AddWithValue("@Sender", sender);
            cmd.Parameters.AddWithValue("@Recipient", recipient);
            cmd.Parameters.AddWithValue("@Content", content);
            cmd.Parameters.AddWithValue("@timestamp", timeStamp);

            cmd.ExecuteNonQuery();
            Console.WriteLine(" \n Inserted Values To Message Table \n");
        }

        public void closeDatabase()
        {
            connection.Close();
        }
    }
}