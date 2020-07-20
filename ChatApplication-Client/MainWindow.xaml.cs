using Chat.client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApplication_Client
{
    class  MainWindow 
    {

        private ClientApplication client;

        public MainWindow()
        {
            client = new ClientApplication();
        }

        public static void Main(String[] args)
        {
            MainWindow mw = new MainWindow();
        }

    }
}
