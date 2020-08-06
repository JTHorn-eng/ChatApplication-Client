﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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
            clientApp = new ClientApplication();
            while(applicationRunning)
            {
                if(Client.initialised)
                {
                    UpdateUsersSidebar(Database.GetFriendsList());
                    break;
                }

                Thread.Sleep(200);
            }
        }


        public void Send_Message(object sender, RoutedEventArgs e) 
        {
            currentMessage = Send_Textbox.Text;
            ClientShareData.AddClientMessage(currentMessage);
            ClientShareData.SetSendButtonClicked(true);
            updateMessageArea(currentMessage);

            Console.WriteLine(Send_Textbox.Text);
        }

        //TODO: Format server response messages.
        public void updateTextBoxMessagesFromServer(string serverResponse)
        {
            ClientShareData.AddClientMessage(Send_Textbox.Text);
            Send_Textbox.Text = serverResponse;
        }

        //append message to scrollviwer, resize viewer if needed
        public void updateMessageArea(string serverResponse)
        {
            Label message = new Label();
            message.Content = ClientShareData.GetUsername() + ": "+ serverResponse;
            Console.WriteLine("[INFO] Added new message");
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

                TextBlock text = new TextBlock();
                text.Padding = new Thickness(5, 5, 0, 0);
                text.Text = user;

                border.Child = text;
                UserListGrid.Children.Add(border);

                border.SetValue(Grid.RowProperty, rowChildNumber);

                rowChildNumber++;
            }
        }

    }
}