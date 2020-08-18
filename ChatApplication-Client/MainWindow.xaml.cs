using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace ChatClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 



    public partial class MainWindow : Window
    {




        private ClientApplication clientApp;
        private string currentMessage = "";
        private bool applicationRunning = true;
        public static List<string> RecipientsInGUI = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            Client.TaskFactoryGUI = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void Send_Message(object sender, RoutedEventArgs e) 
        {
            if(!ClientShareData.GetGUIRecipient().Equals("") && Client.Initialised)
            {
                currentMessage = Send_Textbox.Text;

                //Send data from textbox to Client class for processing
                ClientShareData.AddClientMessage(currentMessage);
                ClientShareData.sendButtonClicked = true;
                AddMessageToGUI(ClientShareData.username, currentMessage);
            }
        }

        //TODO: Format server response messages.
        public void UpdateTextBoxMessagesFromServer(string serverResponse)
        {
            ClientShareData.AddClientMessage(Send_Textbox.Text);
            Send_Textbox.Text = serverResponse;
        }

        //Append message to scrollviwer, resize viewer if needed
        public void AddMessageToGUI(string username, string messageText)
        {
            Label message = new Label();
            Style st = FindResource("StyleA") as Style;
            message.Style = st;
            message.Content = username + ": "+ messageText;
            Console.WriteLine("[INFO] Added new message to GUI");
            Message_Area.Children.Add(message);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            applicationRunning = false;
            clientApp.Close();
        }

        // Accepts a list of usernames and adds a clickable TextBlock in a Border to the users sidebar of the GUI for each username
        public void UpdateUsersSidebar(List<String> users)
        {
            int rowChildNumber = UserListGrid.RowDefinitions.Count;

            foreach (string user in users)
            {
                RecipientsInGUI.Add(user);

                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(30);
                UserListGrid.RowDefinitions.Add(row);

                Border border = new Border();
                border.BorderThickness = new Thickness(0, 0, 0, 1);
                border.BorderBrush = Brushes.DarkGray;

                TextBlock textBlock = new TextBlock();
                textBlock.Padding = new Thickness(5, 5, 0, 0);
                textBlock.Text = user;
                textBlock.PreviewMouseDown += (source, e) =>
                {
                    ClientShareData.SetGUIRecipient(textBlock.Text);
                    Message_Area.Children.Clear();
                    string messages = Database.RetrieveUserMessages(textBlock.Text);
                    string[] messagesSplit = messages.Split('|');

                    foreach (string message in messagesSplit)
                    {

                        if (message != "")
                        {
                            string[] secondSplit = message.Split(';');


                            AddMessageToGUI(secondSplit[0], secondSplit[2]);
                        }
                    }
                };

                border.Child = textBlock;
                UserListGrid.Children.Add(border);

                border.SetValue(Grid.RowProperty, rowChildNumber);

                rowChildNumber++;
            }
        }

        //Set the password for the client
        private void Password_Set(object sender, RoutedEventArgs e)
        {
             
        }

        // On "set username" button click, we set the username and start the application
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            clientApp = new ClientApplication();
            ClientShareData.SetUsername(UsernameTextBox.Text);
            if (Client.Initialised) { 
                while (applicationRunning)
                {
                    UpdateUsersSidebar(Database.GetFriendsList());
                    break;
                    //if (Client.receivedMessage != "")
                    //{
                    //    Client.receivedMessage = "";
                    //    string[] splitMessage = Client.receivedMessage.Split(';');
                    //    AddMessageToGUI(splitMessage[0], splitMessage[1]);
                    //}
                }
            }
        }

        //public void NewMessageChecker(Object source, ElapsedEventArgs e)
        //{
        //    while (applicationRunning)
        //    {
        //        if (Client.receivedMessage != "")
        //        {
        //            string[] splitMessage = Client.receivedMessage.Split(';');
        //            AddMessageToGUI(splitMessage[0], splitMessage[1]);
        //            Client.receivedMessage = "";
        //        }

        //        Thread.Sleep(200);
        //    }
        //}

        // On "set DB path" button click, we set the local database path
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Database.MessagesDBLocation = @"URI=file:" + DBPathTextBox.Text;
        }

        // On recipient button click, we set the recipient in the Client class
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //ClientShareData.setGUIRecipient(RecipientTextBox.Text);
            List<string> list = new List<string>();
            list.Add(RecipientTextBox.Text);
            UpdateUsersSidebar(list);
        }

        //TODO Send client message when Enter key is pressed
        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                currentMessage = Send_Textbox.Text;
                ClientShareData.AddClientMessage(currentMessage);
                ClientShareData.SetSendButtonClicked(true);
                AddMessageToGUI(ClientShareData.GetUsername(), currentMessage);
            }
        }
    }
}