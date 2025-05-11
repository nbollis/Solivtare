using Microsoft.Extensions.DependencyInjection;
using SolvitaireIO.Database.Repositories;
using SolvitaireIO.Database;
using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.EntityFrameworkCore;

namespace SolvitaireGUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();

            // Register the DbContext
            services.AddDbContext<SolvitaireDbContext>(options =>
                options.UseSqlite("Data Source=solvitaire.db"));

            // Register repositories
            services.AddScoped<GenerationLogRepository>();
            services.AddScoped<AgentLogRepository>();

            // Build the service provider
            ServiceProvider = services.BuildServiceProvider();

            base.OnStartup(e);
        }
    }

}
