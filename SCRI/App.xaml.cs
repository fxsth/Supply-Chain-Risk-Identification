﻿using Microsoft.Extensions.DependencyInjection;
using Neo4j.Driver;
using SCRI.Database;
using SCRI.Models;
using SCRI.Services;
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
