using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Threading;
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

        public MainWindow()
        {
            InitializeComponent();
        }


        public void Send_Message(object sender, RoutedEventArgs e) 
        {
            currentMessage = Send_Textbox.Text;
            ClientShareData.AddClientMessage(currentMessage);
            ClientShareData.SetSendButtonClicked(true);
            AddMessageToGUI(ClientShareData.GetUsername(), currentMessage);

            Console.WriteLine(Send_Textbox.Text);
        }

        //TODO: Format server response messages.
        public void UpdateTextBoxMessagesFromServer(string serverResponse)
        {
            ClientShareData.AddClientMessage(Send_Textbox.Text);
            Send_Textbox.Text = serverResponse;
        }

        //append message to scrollviwer, resize viewer if needed
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
            int rowChildNumber = 0;

            foreach (string user in users)
            {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(30);
                UserListGrid.RowDefinitions.Add(row);

                Border border = new Border();
                border.BorderThickness = new Thickness(0, 0, 0, 1);
                border.BorderBrush = Brushes.DarkGray;

                Button text = new Button();
                text.Padding = new Thickness(5, 5, 0, 0);
                text.Content = user;
                text.Click += (source, e) =>
                {
                    ClientShareData.setGUIRecipient(text.Content.ToString());
                };
                

                border.Child = text;
                UserListGrid.Children.Add(border);

                border.SetValue(Grid.RowProperty, rowChildNumber);

                rowChildNumber++;
            }
        }

        // On "set username" button click, we set the username and start the application
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            clientApp = new ClientApplication();
            ClientShareData.SetUsername(UsernameTextBox.Text);
            while (applicationRunning)
            {
                if (Client.initialised)
                {
                    UpdateUsersSidebar(Database.GetFriendsList());
                    break;
                }

                Thread.Sleep(200);
            }
        }

        // On "set DB path" button click, we set the local database path
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Database.MessagesDBLocation = @"URI=file:" + DBPathTextBox.Text;
        }

        // On recipient button click, we set the recipient in the Client class
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ClientShareData.setGUIRecipient(RecipientTextBox.Text);
        }

        //Send client message when Enter key is pressed
        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                currentMessage = Send_Textbox.Text;
                ClientShareData.AddClientMessage(currentMessage);
                ClientShareData.SetSendButtonClicked(true);
                AddMessageToGUI(ClientShareData.GetUsername(), currentMessage);

                Console.WriteLine(Send_Textbox.Text);
            }
        }
    }
}