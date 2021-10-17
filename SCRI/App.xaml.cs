using Microsoft.Extensions.DependencyInjection;
using SCRI.Database;
using SCRI.Services;
using System.Windows;
using Microsoft.Extensions.Configuration;

namespace SCRI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ServiceProvider serviceProvider;

        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            services.AddSingleton<IConfiguration>(configuration);
            // DriverFactory as Singleton
            // easy driver replacement with different credentials / settings
            // credentials / settings stored in factory
            services.AddSingleton<IDriverFactory, DriverFactory>();
            services.AddSingleton<IGraphStore,GraphStore>();
            services.AddScoped<IGraphService, GraphService>();
            services.AddTransient<DbConnectionWindow>();
            services.AddTransient<MainWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {

            var dbConnectionWindow = serviceProvider.GetService<DbConnectionWindow>();
            dbConnectionWindow.Show();
        }
    }
}
