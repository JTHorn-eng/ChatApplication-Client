using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Schema;

namespace ChatApplication_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public class MyObject
    {
        public string Name { get; set; }
    }
    public partial class MainWindow : Window
    {
        private ClientApplication client;
        private ListBox lb;
        
        private string currentMessage = "";

        public MainWindow()
        {
            InitializeComponent();
            client = new ClientApplication();
            lb = new ListBox();
        }


        public void Send_Message(object sender, RoutedEventArgs e) 
        {
            currentMessage = Send_Textbox.Text;
            ClientShareData.AddClientMessage(currentMessage);
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
            message.Content = ClientShareData.getUsername() + ": "+ serverResponse;
            Console.WriteLine("Added new message");
            Message_Area.Children.Add(message);
           
        }

    }
}