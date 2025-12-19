using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Engine.Services;
using Serilog;
using Spectre.Console;

class Program
{
    static void Main(string[] args)
    {
        // 1. Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs/runeandrust.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            // 2. Build Host
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // Register Engine Services
                    services.AddSingleton<IGameService, GameService>();

                    // Future: Register Persistence Services here
                })
                .UseSerilog() // Wire Serilog into ILogger
                .Build();

            // 3. UI Handover
            AnsiConsole.MarkupLine("[green]Rune & Rust v0.0.1 Booting...[/]");

            // Resolve the entry point from DI
            var game = host.Services.GetRequiredService<IGameService>();
            game.Start();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "System Crash");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
