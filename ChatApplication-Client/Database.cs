using System;
using System.Data.SQLite;

namespace Chat.client
{
    public static class Database
    {

        private static string message_objects_location = "Chat/dbfiles/messages.db";
        private static long localID = 0;

        private static void GetUpdatedID()
        {
            SQLiteConnection connection = new SQLiteConnection(message_objects_location);
            connection.Open();
            SQLiteTransaction transaction = null;
            transaction = connection.BeginTransaction();
            localID = connection.LastInsertRowId;
            transaction.Commit();
            connection.Close();
        }
        public static String GetTimestamp(this DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssfff");
        }
        public static void UpdateMessageTable(string sender, string recipient, string content)
        {

            SQLiteConnection connection = new SQLiteConnection(message_objects_location);
            connection.Open();
            //retrieve timestamp
            string currentTimestamp = GetTimestamp(new DateTime());

            //form insert SQL statement
            SQLiteCommand cmd = new SQLiteCommand(connection);
            string commandText = "INSERT INTO Messages(ID, Sender, Recipient, Content, Timestamp) VALUES(" +(long) (localID + 1) + ","+sender +", " +recipient + ", " + content + ", " + currentTimestamp + ");";
            cmd = new SQLiteCommand(commandText, connection);
            cmd.ExecuteNonQuery();
            Console.WriteLine("Inserted Values To Message Table");
            connection.Close();
        }

        
    }
}