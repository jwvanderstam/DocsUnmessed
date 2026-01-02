namespace DocsUnmessed.GUI;

using DocsUnmessed.GUI.ViewModels;
using DocsUnmessed.GUI.Views;
using DocsUnmessed.Core.Interfaces;
using DocsUnmessed.Services;
using DocsUnmessed.Connectors;
using DocsUnmessed.Data;
using DocsUnmessed.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                // Using default fonts
            });

        // Configure DbContext
        var connectionString = "Data Source=docsunmessed.db";
        builder.Services.AddDbContext<DocsUnmessedDbContext>(options =>
            options.UseSqlite(connectionString));

        // Register services
        builder.Services.AddSingleton<IHashService, HashService>();
        builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
        builder.Services.AddTransient<IInventoryService, DatabaseInventoryService>();
        
        // Register connectors as array for IConnector[]
        builder.Services.AddSingleton<FileSystemConnector>();
        builder.Services.AddSingleton<IConnector[]>(sp =>
        {
            var fileSystemConnector = sp.GetRequiredService<FileSystemConnector>();
            return new IConnector[] { fileSystemConnector };
        });

        // Register ViewModels
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<AssessViewModel>();
        builder.Services.AddTransient<MigrationViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();

        // Register Views
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<AssessPage>();
        builder.Services.AddTransient<MigrationPage>();
        builder.Services.AddTransient<SettingsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
