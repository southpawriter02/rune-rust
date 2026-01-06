using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using Spectre.Console;

namespace RuneAndRust.Presentation.Tui.Adapters;

/// <summary>
/// Spectre.Console-based implementation of <see cref="IGameRenderer"/> for TUI output.
/// </summary>
/// <remarks>
/// SpectreGameRenderer uses Spectre.Console's rich formatting capabilities to render
/// colorful, styled game output including status bars, room descriptions, inventory
/// tables, and combat results.
/// </remarks>
public class SpectreGameRenderer : IGameRenderer
{
    /// <summary>
    /// Logger for rendering operations and diagnostics.
    /// </summary>
    private readonly ILogger<SpectreGameRenderer> _logger;

    /// <summary>
    /// Creates a new Spectre game renderer instance.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostics. If null, a no-op logger is used.</param>
    public SpectreGameRenderer(ILogger<SpectreGameRenderer>? logger = null)
    {
        _logger = logger ?? NullLogger<SpectreGameRenderer>.Instance;
        _logger.LogDebug("SpectreGameRenderer initialized");
    }

    /// <inheritdoc/>
    public Task RenderGameStateAsync(GameStateDto gameState, CancellationToken ct = default)
    {
        _logger.LogDebug(
            "Rendering game state - Player: {PlayerName}, Room: {RoomName}, State: {GameState}",
            gameState.Player.Name,
            gameState.CurrentRoom.Name,
            gameState.State);

        RenderStatusBar(gameState.Player);
        Console.WriteLine();
        RenderRoomInternal(gameState.CurrentRoom);

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task RenderMessageAsync(string message, MessageType type = MessageType.Info, CancellationToken ct = default)
    {
        _logger.LogDebug("Rendering message - Type: {MessageType}, Message: {Message}", type, message);

        var color = type switch
        {
            MessageType.Warning => "yellow",
            MessageType.Error => "red",
            MessageType.Success => "green",
            MessageType.Combat => "orange1",
            MessageType.Narrative => "italic grey",
            _ => "white"
        };

        AnsiConsole.MarkupLine($"[{color}]{Markup.Escape(message)}[/]");
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task RenderRoomAsync(RoomDto room, CancellationToken ct = default)
    {
        _logger.LogDebug(
            "Rendering room: {RoomName}, Items: {ItemCount}, Monsters: {MonsterCount}, Exits: {ExitCount}",
            room.Name,
            room.Items.Count,
            room.Monsters.Count,
            room.Exits.Count);

        RenderRoomInternal(room);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task RenderInventoryAsync(InventoryDto inventory, CancellationToken ct = default)
    {
        _logger.LogDebug("Rendering inventory: {ItemCount}/{Capacity} items", inventory.Count, inventory.Capacity);

        if (inventory.Items.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]Your inventory is empty.[/]");
            return Task.CompletedTask;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .Title("[yellow]Inventory[/]")
            .AddColumn(new TableColumn("[cyan]Item[/]"))
            .AddColumn(new TableColumn("[grey]Type[/]"))
            .AddColumn(new TableColumn("[grey]Description[/]"));

        foreach (var item in inventory.Items)
        {
            table.AddRow(
                $"[white]{Markup.Escape(item.Name)}[/]",
                $"[grey]{item.Type}[/]",
                $"[grey]{Markup.Escape(item.Description)}[/]"
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"[grey]Capacity: {inventory.Count}/{inventory.Capacity}[/]");

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task RenderCombatResultAsync(string combatDescription, CancellationToken ct = default)
    {
        _logger.LogDebug("Rendering combat result");

        var panel = new Panel(combatDescription)
            .Header("[red]Combat[/]")
            .Border(BoxBorder.Heavy)
            .BorderColor(Color.Red);

        AnsiConsole.Write(panel);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task ClearScreenAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("Clearing screen");
        AnsiConsole.Clear();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Renders the player status bar showing name, health, attack, and defense.
    /// </summary>
    /// <param name="player">The player DTO to render.</param>
    private static void RenderStatusBar(PlayerDto player)
    {
        var healthColor = player.Health > player.MaxHealth * 0.5 ? "green" :
                          player.Health > player.MaxHealth * 0.25 ? "yellow" : "red";

        var statusTable = new Table()
            .Border(TableBorder.None)
            .HideHeaders()
            .AddColumn("")
            .AddColumn("")
            .AddColumn("")
            .AddColumn("");

        statusTable.AddRow(
            $"[white]{Markup.Escape(player.Name)}[/]",
            $"[{healthColor}]HP: {player.Health}/{player.MaxHealth}[/]",
            $"[cyan]ATK: {player.Attack}[/]",
            $"[blue]DEF: {player.Defense}[/]"
        );

        AnsiConsole.Write(statusTable);
    }

    /// <summary>
    /// Renders room details including name, description, monsters, items, and exits.
    /// </summary>
    /// <param name="room">The room DTO to render.</param>
    private static void RenderRoomInternal(RoomDto room)
    {
        // Room title
        var rule = new Rule($"[yellow]{Markup.Escape(room.Name)}[/]")
        {
            Justification = Justify.Center,
            Style = Style.Parse("yellow")
        };
        AnsiConsole.Write(rule);

        // Room description
        AnsiConsole.MarkupLine($"[white]{Markup.Escape(room.Description)}[/]");
        Console.WriteLine();

        // Monsters
        var aliveMonsters = room.Monsters.Where(m => m.IsAlive).ToList();
        if (aliveMonsters.Count > 0)
        {
            AnsiConsole.MarkupLine("[red]Enemies here:[/]");
            foreach (var monster in aliveMonsters)
            {
                var healthPct = monster.MaxHealth > 0 ? (double)monster.Health / monster.MaxHealth : 0;
                var healthColor = healthPct > 0.5 ? "green" : healthPct > 0.25 ? "yellow" : "red";
                AnsiConsole.MarkupLine($"  [red]* {Markup.Escape(monster.Name)}[/] [{healthColor}](HP: {monster.Health}/{monster.MaxHealth})[/]");
            }
            Console.WriteLine();
        }

        // Items
        if (room.Items.Count > 0)
        {
            AnsiConsole.MarkupLine("[cyan]Items here:[/]");
            foreach (var item in room.Items)
            {
                AnsiConsole.MarkupLine($"  [cyan]* {Markup.Escape(item.Name)}[/]");
            }
            Console.WriteLine();
        }

        // Exits
        if (room.Exits.Count > 0)
        {
            var exits = string.Join(", ", room.Exits.Select(e => e.ToString().ToLower()));
            AnsiConsole.MarkupLine($"[grey]Exits: {exits}[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[grey]There are no visible exits.[/]");
        }
    }
}
