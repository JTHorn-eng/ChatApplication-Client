using System.Windows;

namespace ChatApplication_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ClientApplication client;

        public MainWindow()
        {
            InitializeComponent();
            client = new ClientApplication();
        }
    }
}