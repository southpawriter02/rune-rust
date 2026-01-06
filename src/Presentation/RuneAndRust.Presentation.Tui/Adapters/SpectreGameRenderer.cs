using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using Spectre.Console;

namespace RuneAndRust.Presentation.Tui.Adapters;

public class SpectreGameRenderer : IGameRenderer
{
    public Task RenderGameStateAsync(GameStateDto gameState, CancellationToken ct = default)
    {
        RenderStatusBar(gameState.Player);
        Console.WriteLine();
        RenderRoomInternal(gameState.CurrentRoom);

        return Task.CompletedTask;
    }

    public Task RenderMessageAsync(string message, MessageType type = MessageType.Info, CancellationToken ct = default)
    {
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

    public Task RenderRoomAsync(RoomDto room, CancellationToken ct = default)
    {
        RenderRoomInternal(room);
        return Task.CompletedTask;
    }

    public Task RenderInventoryAsync(InventoryDto inventory, CancellationToken ct = default)
    {
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

    public Task RenderCombatResultAsync(string combatDescription, CancellationToken ct = default)
    {
        var panel = new Panel(combatDescription)
            .Header("[red]Combat[/]")
            .Border(BoxBorder.Heavy)
            .BorderColor(Color.Red);

        AnsiConsole.Write(panel);
        return Task.CompletedTask;
    }

    public Task ClearScreenAsync(CancellationToken ct = default)
    {
        AnsiConsole.Clear();
        return Task.CompletedTask;
    }

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
