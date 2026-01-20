using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Infrastructure;
using RuneAndRust.Presentation.Tui.Adapters;
using RuneAndRust.Presentation.Tui.Views;
using Serilog;
using Spectre.Console;

// Build configuration - use assembly location to find appsettings.json
var assemblyLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
    ?? Directory.GetCurrentDirectory();

var configuration = new ConfigurationBuilder()
    .SetBasePath(assemblyLocation)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

try
{
    Log.Information("Starting Rune & Rust TUI");

    // Build host with DI
    var host = Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureServices((context, services) =>
        {
            // Infrastructure
            var useInMemory = context.Configuration.GetValue<bool>("Game:UseInMemoryDatabase");
            services.AddInfrastructure(context.Configuration, useInMemory);

            // Application services
            services.AddApplicationServices();

            // Presentation
            services.AddSingleton<IGameRenderer, SpectreGameRenderer>();
            services.AddSingleton<IInputHandler, ConsoleInputHandler>();
            services.AddTransient<MainMenuView>();
            // Statistics view (v0.12.0c) - must be registered before GameView
            services.AddTransient<StatisticsView>();
            services.AddTransient<GameView>();
        })
        .Build();

    // Run the game
    await RunGameAsync(host.Services);
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    AnsiConsole.WriteException(ex);
}
finally
{
    Log.CloseAndFlush();
}

static async Task RunGameAsync(IServiceProvider services)
{
    var mainMenu = services.GetRequiredService<MainMenuView>();
    var gameService = services.GetRequiredService<GameSessionService>();
    var renderer = services.GetRequiredService<IGameRenderer>();
    var inputHandler = services.GetRequiredService<IInputHandler>();

    var running = true;

    while (running)
    {
        mainMenu.RenderTitle();

        var selection = mainMenu.GetMenuSelection();

        switch (selection)
        {
            case MainMenuOption.NewGame:
                await StartNewGameAsync(services, mainMenu, gameService, renderer, inputHandler);
                break;

            case MainMenuOption.LoadGame:
                await LoadGameAsync(services, gameService, renderer, inputHandler);
                break;

            case MainMenuOption.Quit:
                running = false;
                AnsiConsole.MarkupLine("[grey]Farewell, adventurer![/]");
                break;
        }
    }
}

static async Task StartNewGameAsync(
    IServiceProvider services,
    MainMenuView mainMenu,
    GameSessionService gameService,
    IGameRenderer renderer,
    IInputHandler inputHandler)
{
    var playerName = mainMenu.GetPlayerName();

    await AnsiConsole.Status()
        .StartAsync("Creating your adventure...", async ctx =>
        {
            await gameService.StartNewGameAsync(playerName);
            await Task.Delay(500); // Brief pause for effect
        });

    mainMenu.RenderWelcome(playerName);

    var gameView = services.GetRequiredService<GameView>();
    await gameView.RunGameLoopAsync();
}

static async Task LoadGameAsync(
    IServiceProvider services,
    GameSessionService gameService,
    IGameRenderer renderer,
    IInputHandler inputHandler)
{
    var savedGames = await gameService.GetSavedGamesAsync();

    if (savedGames.Count == 0)
    {
        await renderer.RenderMessageAsync("No saved games found.", MessageType.Warning);
        await inputHandler.GetTextInputAsync("Press Enter to continue...");
        return;
    }

    var selection = await inputHandler.GetSelectionAsync(
        "Select a saved game:",
        savedGames,
        s => $"{s.PlayerName} - Last played: {s.LastPlayedAt:g}");

    var state = await gameService.LoadGameAsync(selection.Id);

    if (state == null)
    {
        await renderer.RenderMessageAsync("Failed to load game.", MessageType.Error);
        await inputHandler.GetTextInputAsync("Press Enter to continue...");
        return;
    }

    await renderer.RenderMessageAsync($"Welcome back, {state.Player.Name}!", MessageType.Success);
    Console.WriteLine();

    var gameView = services.GetRequiredService<GameView>();
    await gameView.RunGameLoopAsync();
}
