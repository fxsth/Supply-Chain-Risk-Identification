using Microsoft.Extensions.DependencyInjection;
using Neo4j.Driver;
using SCRI.Database;
using SCRI.Services;
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
        private IDriverFactory _driverFactory;
        //private MainWindow _mainWindow;
        private readonly IServiceProvider _serviceProvider;

        // Simple Injection to be able to change IDrivers default config
        public DbConnectionWindow(IDriverFactory driverFactory, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _driverFactory = driverFactory;
            _serviceProvider = serviceProvider;
        }
         
        private async void onClickConnectAsync(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtURL.Text) || string.IsNullOrEmpty(txtUsername.Text))
            {
                TextBlockStatus.Text = "Missing data to connect";
                return;
            }
            _driverFactory.URI = txtURL.Text;
            if (string.IsNullOrEmpty(txtPassword.Text))
            {
                _driverFactory.AuthToken = AuthTokens.None;
            }
            else
            {
                _driverFactory.AuthToken = AuthTokens.Basic(txtUsername.Text, txtPassword.Text);
            }
            try
            {
                using (IDriver driver = _driverFactory.CreateDriver())
                {
                    var verifyCon = driver.VerifyConnectivityAsync();
                    TextBlockStatus.Text = verifyCon.Status.ToString();
                    await verifyCon;
                    if (verifyCon.IsCompletedSuccessfully)
                    {
                        TextBlockStatus.Text = "Connected";
                        MainWindow mainWindow = _serviceProvider.GetService<MainWindow>();
                        mainWindow.Show();
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                TextBlockStatus.Text = ex.Message;
            }
        }
    }
}
