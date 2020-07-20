using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Chat.client;
namespace Chat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    


    public partial class MainWindow : Window
    {

        ClientApplication client;
        private string currentMessage = "";
        public MainWindow()
        {
            //run application
            InitializeComponent();
            client = new ClientApplication();

            Environment.Exit(0);

        }


        private void SendClick(object sender, RoutedEventArgs e) 
        {
            Console.WriteLine("Get message from textbox");
            currentMessage = Send_Textbox.Text;
            Console.WriteLine("Current Message: " + currentMessage);
        }
    }
}
