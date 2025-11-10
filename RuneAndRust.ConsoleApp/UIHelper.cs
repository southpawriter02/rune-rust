using Spectre.Console;
using RuneAndRust.Core;
using RuneAndRust.Engine;

namespace RuneAndRust.ConsoleApp;

public static class UIHelper
{
    public static void DisplayCharacterSheet(PlayerCharacter character)
    {
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey);

        table.AddColumn(new TableColumn($"[bold yellow]{character.Name}[/] - [dim]{character.Class}[/]").Centered());

        // Resources
        var hpBar = CreateBar("HP", character.HP, character.MaxHP, Color.Red, Color.DarkRed);
        var staminaBar = CreateBar("Stamina", character.Stamina, character.MaxStamina, Color.Green, Color.DarkGreen);
        table.AddRow(hpBar);
        table.AddRow(staminaBar);
        table.AddRow($"[dim]AP:[/] {character.AP}");

        // Attributes
        table.AddRow(new Markup("[bold]ATTRIBUTES[/]"));
        table.AddRow($"MIGHT: {character.Attributes.Might} | FINESSE: {character.Attributes.Finesse} | WITS: {character.Attributes.Wits}");
        table.AddRow($"WILL: {character.Attributes.Will} | STURDINESS: {character.Attributes.Sturdiness}");

        // Weapon
        table.AddRow(new Markup("[bold]WEAPON[/]"));
        table.AddRow($"{character.WeaponName} ([yellow]{character.WeaponAttribute.ToUpper()}[/]-based, {character.BaseDamage}d6)");

        // Abilities
        table.AddRow(new Markup("[bold]ABILITIES[/]"));
        foreach (var ability in character.Abilities)
        {
            var costColor = character.Stamina >= ability.StaminaCost ? "green" : "red";
            table.AddRow($"[yellow]{ability.Name}[/] ([{costColor}]{ability.StaminaCost} Stamina[/])");
            table.AddRow($"[dim]{ability.Description}[/]");
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    public static void DisplayRoomDescription(Room room, List<string> availableDirections)
    {
        AnsiConsole.WriteLine();

        var panel = new Panel(new Markup($"[bold]{room.Name}[/]\n\n{room.Description}"))
        {
            Border = BoxBorder.Rounded,
            BorderColor = Color.Blue,
            Padding = new Padding(2, 1)
        };

        AnsiConsole.Write(panel);

        // Show exits
        if (availableDirections.Count > 0)
        {
            var exits = string.Join(", ", availableDirections.Select(d => $"[yellow]{d}[/]"));
            AnsiConsole.MarkupLine($"[dim]Exits: {exits}[/]");
        }

        // Show if puzzle is present
        if (room.HasPuzzle && !room.IsPuzzleSolved)
        {
            AnsiConsole.MarkupLine("[yellow]⚠ A puzzle blocks your progress. Use 'solve' to attempt it.[/]");
        }

        // Show if enemies are present (but don't spoil them)
        if (!room.HasBeenCleared && room.Enemies.Count > 0)
        {
            AnsiConsole.MarkupLine("[red]⚠ You sense danger ahead...[/]");
        }

        AnsiConsole.WriteLine();
    }

    public static void DisplayCombatStart(List<Enemy> enemies)
    {
        AnsiConsole.WriteLine();
        var rule = new Rule("[bold red]⚔ COMBAT INITIATED ⚔[/]")
        {
            Justification = Justify.Center
        };
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[red]Enemies detected:[/]");
        foreach (var enemy in enemies)
        {
            AnsiConsole.MarkupLine($"  • [bold]{enemy.Name}[/] ([dim]HP: {enemy.HP}/{enemy.MaxHP}[/])");
        }
        AnsiConsole.WriteLine();
    }

    public static void DisplayPuzzlePrompt(Room room)
    {
        AnsiConsole.WriteLine();
        var rule = new Rule("[bold yellow]⚙ PUZZLE CHAMBER ⚙[/]")
        {
            Justification = Justify.Center
        };
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        var panel = new Panel(new Markup(room.PuzzleDescription))
        {
            Border = BoxBorder.Double,
            BorderColor = Color.Yellow,
            Header = new PanelHeader("[bold]Environmental Puzzle[/]"),
            Padding = new Padding(2, 1)
        };

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[dim]This puzzle requires a [yellow]WITS check[/] ({room.PuzzleSuccessThreshold} successes needed).[/]");
        AnsiConsole.MarkupLine($"[dim]Failure will deal [red]{room.PuzzleFailureDamage}d6 damage[/].[/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[yellow]Type 'solve' to attempt the puzzle.[/]");
        AnsiConsole.WriteLine();
    }

    public static void DisplayDiceRoll(DiceResult result, string context = "")
    {
        var contextText = string.IsNullOrEmpty(context) ? "" : $"{context}: ";
        var rollsDisplay = string.Join(" ", result.Rolls.Select(r =>
            r >= 5 ? $"[green]{r}[/]" : $"[dim]{r}[/]"
        ));

        AnsiConsole.MarkupLine($"{contextText}Rolled [yellow]{result.DiceRolled}d6[/]: [{rollsDisplay}] = [bold]{result.Successes}[/] successes");
    }

    public static void DisplayVictory()
    {
        AnsiConsole.Clear();
        AnsiConsole.WriteLine();

        var figlet = new FigletText("VICTORY")
            .Centered()
            .Color(Color.Gold1);
        AnsiConsole.Write(figlet);

        AnsiConsole.WriteLine();
        var panel = new Panel(
            "[bold green]You have defeated the Ruin-Warden and survived the facility.[/]\n\n" +
            "The corrupted machines fall silent. The hum of residual energy fades.\n" +
            "You stand alone in the twilight ruins, victorious.\n\n" +
            "[dim]THE END[/]\n\n" +
            "[yellow]Thank you for playing Rune & Rust v0.1[/]"
        )
        {
            Border = BoxBorder.Double,
            BorderColor = Color.Gold1,
            Padding = new Padding(2, 1)
        };
        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    public static void DisplayGameOver()
    {
        AnsiConsole.Clear();
        AnsiConsole.WriteLine();

        var figlet = new FigletText("GAME OVER")
            .Centered()
            .Color(Color.Red);
        AnsiConsole.Write(figlet);

        AnsiConsole.WriteLine();
        var panel = new Panel(
            "[bold red]You have fallen in the depths of the facility.[/]\n\n" +
            "The corrupted machines stand over your broken form.\n" +
            "Your journey ends here, in the twilight ruins.\n\n" +
            "[dim]Perhaps another survivor will fare better...[/]"
        )
        {
            Border = BoxBorder.Double,
            BorderColor = Color.Red,
            Padding = new Padding(2, 1)
        };
        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    private static Markup CreateBar(string label, int current, int max, Color fillColor, Color emptyColor)
    {
        var percentage = max > 0 ? (double)current / max : 0;
        var barWidth = 20;
        var filledBlocks = (int)(percentage * barWidth);
        var emptyBlocks = barWidth - filledBlocks;

        var filled = new string('█', filledBlocks);
        var empty = new string('░', emptyBlocks);

        return new Markup($"[dim]{label}:[/] [{fillColor}]{filled}[/][{emptyColor}]{empty}[/] [bold]{current}/{max}[/]");
    }
}
