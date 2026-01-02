using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DocsUnmessed.Core.Interfaces;
using DocsUnmessed.Services;
using DocsUnmessed.Data;
using Microsoft.EntityFrameworkCore;
using DocsUnmessed.GUI.ViewModels;

namespace DocsUnmessed.GUI;

public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Database
                services.AddDbContext<DocsUnmessedDbContext>(options =>
                    options.UseSqlite("Data Source=docsunmessed.db"));
                
                // Core services
                services.AddSingleton<IInventoryService, DatabaseInventoryService>();
                services.AddTransient<RulesEngine>();
                
                // Connectors (to be added when needed)
                // services.AddSingleton<IConnector, LocalFileSystemConnector>();
                services.AddSingleton<IConnector[]>(_ => Array.Empty<IConnector>());
                
                // ViewModels
                services.AddTransient<DashboardViewModel>();
                services.AddTransient<AssessViewModel>();
                services.AddTransient<MigrationViewModel>();
                services.AddTransient<SettingsViewModel>();
                
                // Windows
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        try
        {
            await _host.StartAsync();

            using (var scope = _host.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DocsUnmessedDbContext>();
                await dbContext.Database.EnsureCreatedAsync();
            }

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error starting application: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();
        base.OnExit(e);
    }

    public static T GetService<T>() where T : class
    {
        return ((App)Current)._host.Services.GetRequiredService<T>();
    }
}
