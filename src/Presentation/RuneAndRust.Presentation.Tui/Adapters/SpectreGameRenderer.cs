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
    public Task RenderExamineResultAsync(ExamineResultDto result, CancellationToken ct = default)
    {
        _logger.LogDebug("Rendering examine result for: {Name} (Type: {Type})", result.Name, result.Type);

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Cyan1)
            .Title($"[cyan]{Markup.Escape(result.Name)}[/] [grey]({result.Type})[/]")
            .AddColumn(new TableColumn("[grey]Property[/]"))
            .AddColumn(new TableColumn("[white]Value[/]"));

        table.AddRow("[white]Description[/]", Markup.Escape(result.Description));

        foreach (var prop in result.Properties)
        {
            table.AddRow($"[grey]{Markup.Escape(prop.Key)}[/]", Markup.Escape(prop.Value));
        }

        AnsiConsole.Write(table);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task RenderPlayerStatsAsync(PlayerStatsDto stats, CancellationToken ct = default)
    {
        _logger.LogDebug("Rendering player stats for: {Name}", stats.Name);

        var healthColor = stats.HealthPercentage > 0.5 ? "green" :
                          stats.HealthPercentage > 0.25 ? "yellow" : "red";

        var barColor = stats.HealthPercentage > 0.5 ? Color.Green :
                       stats.HealthPercentage > 0.25 ? Color.Yellow : Color.Red;

        var healthBar = new BarChart()
            .Width(30)
            .AddItem("[white]HP[/]", (int)(stats.HealthPercentage * 100), barColor);

        var panel = new Panel(
            new Rows(
                new Markup($"[bold yellow]{Markup.Escape(stats.Name)}[/]"),
                new Text(""),
                new Markup($"[{healthColor}]Health: {stats.Health}/{stats.MaxHealth}[/]"),
                healthBar,
                new Text(""),
                new Markup($"[cyan]Attack: {stats.Attack}[/]"),
                new Markup($"[blue]Defense: {stats.Defense}[/]"),
                new Text(""),
                new Markup($"[grey]Location: {Markup.Escape(stats.CurrentRoomName)} ({stats.PositionX},{stats.PositionY})[/]"),
                new Markup($"[grey]Inventory: {stats.InventoryCount}/{stats.InventoryCapacity}[/]")
            ))
            .Header("[yellow]Character Status[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Yellow);

        AnsiConsole.Write(panel);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task RenderAbilitiesAsync(IReadOnlyList<PlayerAbilityDto> abilities, CancellationToken ct = default)
    {
        _logger.LogDebug("Rendering abilities: {AbilityCount} abilities", abilities.Count);

        if (abilities.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]You have no abilities.[/]");
            return Task.CompletedTask;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Purple)
            .Title("[purple]Abilities[/]")
            .AddColumn(new TableColumn("[cyan]Name[/]"))
            .AddColumn(new TableColumn("[grey]Cost[/]"))
            .AddColumn(new TableColumn("[grey]Cooldown[/]"))
            .AddColumn(new TableColumn("[grey]Status[/]"))
            .AddColumn(new TableColumn("[grey]Description[/]"));

        foreach (var ability in abilities)
        {
            var statusColor = ability.IsReady ? "green" :
                              ability.IsOnCooldown ? "yellow" :
                              !ability.CanAfford ? "red" :
                              !ability.IsUnlocked ? "grey" : "white";

            var status = ability.IsReady ? "Ready" :
                         ability.IsOnCooldown ? $"CD: {ability.CurrentCooldown}" :
                         !ability.CanAfford ? "No resource" :
                         !ability.IsUnlocked ? $"Lvl {ability.UnlockLevel}" : "Ready";

            var cost = ability.CostAmount > 0
                ? $"{ability.CostAmount} {ability.CostResourceTypeId}"
                : "Free";

            var cooldown = ability.Cooldown > 0 ? $"{ability.Cooldown} turns" : "None";

            table.AddRow(
                $"[white]{Markup.Escape(ability.Name)}[/]",
                $"[cyan]{Markup.Escape(cost)}[/]",
                $"[grey]{cooldown}[/]",
                $"[{statusColor}]{status}[/]",
                $"[grey]{Markup.Escape(ability.Description)}[/]"
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("[grey]Use 'cast <ability name>' to use an ability.[/]");

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
