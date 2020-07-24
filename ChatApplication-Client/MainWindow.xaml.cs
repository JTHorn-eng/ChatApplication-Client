using Chat.client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChatApplication_Client
{
    class  MainWindow 
    {

        private ClientApplication client;
        public int SendTextBox { get; set; }


        public MainWindow()
        {
            client = new ClientApplication();
        }
        
        //SendEvent
        public void GetSendTextBoxString(object sender, RoutedEventArgs e) 
        {
            Console.WriteLine("Clicked sendL " + SendTextBox.ToString());
            
        }


        public static void Main(String[] args)
        {
            MainWindow mw = new MainWindow();
        }

    }
}
