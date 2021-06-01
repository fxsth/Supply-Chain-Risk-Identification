using SCRI.Database;
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

namespace SCRI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void onClickConnectAsync(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtURL.Text) || string.IsNullOrEmpty(txtUsername.Text) || string.IsNullOrEmpty(txtPassword.Text))
            {
                lblStatus.Content = "Missing data to connect";
                return;
            }
            using (var con = new Neo4JConnection(txtURL.Text, txtUsername.Text, txtPassword.Text))
            {
                lblStatus.Content = con.connectionStatus;
                var connecting = con.checkConnectionStatus();
                lblStatus.Content = con.connectionStatus;
                lblStatus.Content = await connecting;
            }
        }
    }
}
