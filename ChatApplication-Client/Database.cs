using System;
using System.Data.SQLite;

namespace ChatClient
{
    public static class Database
    {

        private static string message_objects_location = @"URI=file:C:\Users\laure\Documents\Repos\TornadoClient\ChatApplication-Client\ChatApplication-Client\database\messages.db";
        private static Int32 localID = 0;

        private static void GetUpdatedID()
        {
            SQLiteConnection connection = new SQLiteConnection(message_objects_location);
            connection.Open();
            string commandText = "SELECT rowid FROM Messages WHERE rowid = (SELECT MAX(rowid) FROM Messages) ";
            SQLiteCommand cmd = new SQLiteCommand(commandText, connection);
            SQLiteDataReader r = cmd.ExecuteReader();
            if (r.Read())
                localID = (Int32)r.GetInt64(0);
            connection.Close();
        }
        public static String GetTimestamp(this DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssfff");
        }
        public static void UpdateMessageTable(string sender, string serverResponse)
        {
            string[] messageObject = serverResponse.Split(':');
            SQLiteConnection connection = new SQLiteConnection(message_objects_location);
            connection.Open();
            //retrieve timestamp
            string currentTimestamp = GetTimestamp(new DateTime());

            //find most recent ID
            GetUpdatedID();

            //form insert SQL statement
            string commandText = "INSERT INTO Messages(rowid, Sender, Recipient, Content, Timestamp) VALUES ('" + ((Int32) (localID + 1)).ToString() + "','" + messageObject[2] +"','" + messageObject[3] + "','" + messageObject[4] + "','" + messageObject[5] + "');";
            SQLiteCommand cmd = new SQLiteCommand(commandText, connection);
            cmd.ExecuteNonQuery();
            Console.WriteLine("Inserted Values To Message Table");
            connection.Close();
        }
    }
}