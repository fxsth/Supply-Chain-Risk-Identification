using Microsoft.Extensions.DependencyInjection;
using Neo4j.Driver;
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
    public partial class DbConnectionWindow : Window
    {
        private readonly GraphDbConnection _connection;

        public DbConnectionWindow(IDisposable connection)
        {
            InitializeComponent();
            if (connection is GraphDbConnection graphDbConnection)
                _connection = graphDbConnection;
        }

        private async void onClickConnectAsync(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtURL.Text) || string.IsNullOrEmpty(txtUsername.Text) || string.IsNullOrEmpty(txtPassword.Text))
            {
                lblStatus.Content = "Missing data to connect";
                return;
            }

            if (_connection.SetUpDriver(txtURL.Text, txtUsername.Text, txtPassword.Text))
            {
                lblStatus.Content = _connection.connectionStatus;
                var connecting = _connection.checkConnectionStatus();
                lblStatus.Content = _connection.connectionStatus;
                lblStatus.Content = await connecting;
                if (_connection.connectionStatus == "Connected")
                {
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                }
            }
        }
    }
}
