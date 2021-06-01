using Microsoft.Extensions.DependencyInjection;
using SCRI.Database;
using System;
using System.Data.Common;
using System.Windows;

namespace SCRI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider serviceProvider;

        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // Startup-Window
            services.AddTransient<DbConnectionWindow>();
            // Neo4J-Driver encapsulated and held as Singleton
            services.AddSingleton<IDisposable, GraphDbConnection>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var dbConnectionWindow = serviceProvider.GetService<DbConnectionWindow>();
            dbConnectionWindow.Show();
        }
    }
}
