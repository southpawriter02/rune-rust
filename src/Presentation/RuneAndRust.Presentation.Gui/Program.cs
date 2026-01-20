using Avalonia;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;

namespace RuneAndRust.Presentation.Gui;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Configure Serilog from appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        try
        {
            Log.Information("Starting Rune & Rust GUI application");
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "GUI application terminated unexpectedly");
            throw;
        }
        finally
        {
            Log.Information("Rune & Rust GUI application shutting down");
            Log.CloseAndFlush();
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
