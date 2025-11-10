using Spectre.Console;
using RuneAndRust.Core;
using RuneAndRust.Engine;

namespace RuneAndRust.ConsoleApp;

class Program
{
    private static GameState _gameState = new();
    private static DiceService _diceService = new();
    private static CommandParser _commandParser = new();

    static void Main(string[] args)
    {
        DisplayWelcomeScreen();
        CharacterCreation();
        MainGameLoop();
    }

    static void DisplayWelcomeScreen()
    {
        AnsiConsole.Clear();

        var rule = new Rule("[bold yellow]RUNE & RUST[/]")
        {
            Justification = Justify.Center
        };
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        var panel = new Panel(
            "[dim]A text-based dungeon crawler set in the twilight of a broken world.\n" +
            "Corrupted machines guard ancient ruins. Only the bold survive.\n\n" +
            "[yellow]Vertical Slice v0.1[/] - A 30-minute adventure[/]"
        )
        {
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Grey),
            Padding = new Padding(2, 1)
        };

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to begin your journey...[/]");
        Console.ReadLine();
    }

    static void CharacterCreation()
    {
        AnsiConsole.Clear();
        AnsiConsole.WriteLine();

        var rule = new Rule("[bold cyan]CHARACTER CREATION[/]")
        {
            Justification = Justify.Center
        };
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[dim]Choose your class. Each class has unique strengths and abilities.[/]");
        AnsiConsole.WriteLine();

        // Display class options
        var classChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Select your class:[/]")
                .PageSize(10)
                .AddChoices(new[]
                {
                    "Warrior - High HP, Melee Focus",
                    "Scavenger - Balanced, Tactical",
                    "Mystic - Low HP, Ability Focus"
                })
        );

        CharacterClass selectedClass = classChoice switch
        {
            "Warrior - High HP, Melee Focus" => CharacterClass.Warrior,
            "Scavenger - Balanced, Tactical" => CharacterClass.Scavenger,
            "Mystic - Low HP, Ability Focus" => CharacterClass.Mystic,
            _ => CharacterClass.Warrior
        };

        // Show class details
        AnsiConsole.Clear();
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold yellow]You have chosen: {selectedClass}[/]");
        AnsiConsole.WriteLine();

        var description = CharacterFactory.GetClassDescription(selectedClass);
        var panel = new Panel(description)
        {
            Border = BoxBorder.Rounded,
            BorderColor = Color.Yellow,
            Padding = new Padding(2, 1)
        };
        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();

        // Optional: Ask for character name
        var name = AnsiConsole.Ask<string>("Enter your character's name (or press ENTER for 'Survivor'):", "Survivor");

        // Create character
        _gameState.Player = CharacterFactory.CreateCharacter(selectedClass, name);
        _gameState.CurrentPhase = GamePhase.Exploration;

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[green]✓[/] Character created successfully!");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to enter the ruins...[/]");
        Console.ReadLine();
    }

    static void MainGameLoop()
    {
        while (_gameState.CurrentPhase != GamePhase.GameOver &&
               _gameState.CurrentPhase != GamePhase.Victory)
        {
            // Check if player is dead
            if (!_gameState.Player.IsAlive)
            {
                _gameState.CurrentPhase = GamePhase.GameOver;
                UIHelper.DisplayGameOver();
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to exit...[/]");
                Console.ReadLine();
                return;
            }

            // Handle current phase
            switch (_gameState.CurrentPhase)
            {
                case GamePhase.Exploration:
                    ExplorationLoop();
                    break;
                case GamePhase.Combat:
                    // Combat will be implemented in Week 3
                    AnsiConsole.MarkupLine("[red]Combat not yet implemented![/]");
                    _gameState.CurrentPhase = GamePhase.GameOver;
                    break;
                case GamePhase.Puzzle:
                    PuzzleLoop();
                    break;
            }
        }
    }

    static void ExplorationLoop()
    {
        AnsiConsole.Clear();

        // Display room
        UIHelper.DisplayRoomDescription(_gameState.CurrentRoom, _gameState.GetAvailableDirections());

        // Check for automatic triggers
        if (_gameState.ShouldTriggerCombat())
        {
            // Show that combat would start
            UIHelper.DisplayCombatStart(_gameState.CurrentRoom.Enemies);
            AnsiConsole.MarkupLine("[yellow]Combat system will be implemented in Week 3.[/]");
            AnsiConsole.MarkupLine("[yellow]For now, clearing room automatically...[/]");
            _gameState.ClearCurrentRoom();
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
            Console.ReadLine();
            return;
        }

        if (_gameState.ShouldShowPuzzle())
        {
            _gameState.CurrentPhase = GamePhase.Puzzle;
            return;
        }

        // Check for victory condition (boss room cleared)
        if (_gameState.CurrentRoom.Name == "Boss Sanctum" && _gameState.CurrentRoom.HasBeenCleared)
        {
            _gameState.CurrentPhase = GamePhase.Victory;
            UIHelper.DisplayVictory();
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to exit...[/]");
            Console.ReadLine();
            return;
        }

        // Get player input
        var input = AnsiConsole.Prompt(
            new TextPrompt<string>("[grey]>[/] ")
                .AllowEmpty()
        );

        if (string.IsNullOrWhiteSpace(input))
        {
            return;
        }

        // Parse command
        var command = _commandParser.Parse(input);

        // Execute command
        ExecuteCommand(command);
    }

    static void ExecuteCommand(ParsedCommand command)
    {
        try
        {
            switch (command.Type)
            {
                case CommandType.Look:
                    // Just redisplay the room (loop will handle it)
                    break;

                case CommandType.Move:
                    HandleMove(command.Direction);
                    break;

                case CommandType.Stats:
                    HandleStats();
                    break;

                case CommandType.Help:
                    HandleHelp();
                    break;

                case CommandType.Quit:
                    AnsiConsole.MarkupLine("[yellow]Thanks for playing![/]");
                    _gameState.CurrentPhase = GamePhase.GameOver;
                    break;

                case CommandType.Inventory:
                    AnsiConsole.MarkupLine("[dim]Inventory system not implemented in v0.1[/]");
                    AnsiConsole.WriteLine();
                    AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
                    Console.ReadLine();
                    break;

                case CommandType.Unknown:
                default:
                    AnsiConsole.MarkupLine($"[red]Unknown command:[/] {command.RawInput.EscapeMarkup()}");
                    AnsiConsole.MarkupLine("[dim]Type 'help' for a list of commands.[/]");
                    AnsiConsole.WriteLine();
                    AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
                    Console.ReadLine();
                    break;
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message.EscapeMarkup()}");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
            Console.ReadLine();
        }
    }

    static void HandleMove(string direction)
    {
        if (string.IsNullOrEmpty(direction))
        {
            AnsiConsole.MarkupLine("[red]Move where?[/] Specify a direction (north, south, east, west)");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
            Console.ReadLine();
            return;
        }

        if (!_gameState.CanMove(direction))
        {
            AnsiConsole.MarkupLine($"[red]You cannot go {direction} from here.[/]");
            var exits = string.Join(", ", _gameState.GetAvailableDirections());
            AnsiConsole.MarkupLine($"[dim]Available exits: {exits}[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
            Console.ReadLine();
            return;
        }

        _gameState.MoveToRoom(direction);
        AnsiConsole.MarkupLine($"[dim]You move {direction}...[/]");
        AnsiConsole.WriteLine();
        System.Threading.Thread.Sleep(800); // Brief pause for atmosphere
    }

    static void HandleStats()
    {
        AnsiConsole.Clear();
        UIHelper.DisplayCharacterSheet(_gameState.Player);
        AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
        Console.ReadLine();
    }

    static void HandleHelp()
    {
        AnsiConsole.Clear();
        var helpText = CommandParser.GetHelpText();
        var panel = new Panel(helpText)
        {
            Border = BoxBorder.Rounded,
            BorderColor = Color.Cyan,
            Header = new PanelHeader("[bold]HELP[/]")
        };
        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
        Console.ReadLine();
    }

    static void PuzzleLoop()
    {
        AnsiConsole.Clear();
        UIHelper.DisplayPuzzlePrompt(_gameState.CurrentRoom);

        var input = AnsiConsole.Prompt(
            new TextPrompt<string>("[grey]>[/] ")
                .AllowEmpty()
        );

        if (string.IsNullOrWhiteSpace(input))
        {
            return;
        }

        var command = _commandParser.Parse(input);

        if (command.Type == CommandType.Solve)
        {
            AttemptPuzzle();
        }
        else if (command.Type == CommandType.Quit)
        {
            AnsiConsole.MarkupLine("[yellow]Thanks for playing![/]");
            _gameState.CurrentPhase = GamePhase.GameOver;
        }
        else if (command.Type == CommandType.Help)
        {
            HandleHelp();
        }
        else if (command.Type == CommandType.Stats)
        {
            HandleStats();
        }
        else
        {
            AnsiConsole.MarkupLine("[yellow]You need to solve the puzzle to proceed. Type 'solve' to attempt it.[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
            Console.ReadLine();
        }
    }

    static void AttemptPuzzle()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[yellow]You analyze the power conduits...[/]");
        AnsiConsole.WriteLine();
        System.Threading.Thread.Sleep(1000);

        // Roll WITS check
        var witsValue = _gameState.Player.Attributes.Wits;
        var result = _diceService.Roll(witsValue);

        UIHelper.DisplayDiceRoll(result, "WITS Check");
        AnsiConsole.WriteLine();

        if (result.Successes >= _gameState.CurrentRoom.PuzzleSuccessThreshold)
        {
            // Success!
            AnsiConsole.MarkupLine("[green]✓ Success![/] You route the power correctly.");
            AnsiConsole.MarkupLine("[green]The sealed door unlocks with a grinding hiss.[/]");
            _gameState.SolvePuzzle();
            _gameState.CurrentPhase = GamePhase.Exploration;
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
            Console.ReadLine();
        }
        else
        {
            // Failure - take damage
            var damage = _diceService.RollDamage(_gameState.CurrentRoom.PuzzleFailureDamage / 6);
            AnsiConsole.MarkupLine($"[red]✗ Failure![/] The conduits overload, shocking you for [red]{damage} damage[/]!");
            _gameState.Player.HP -= damage;

            if (_gameState.Player.HP < 0)
            {
                _gameState.Player.HP = 0;
            }

            AnsiConsole.MarkupLine($"[dim]HP: {_gameState.Player.HP}/{_gameState.Player.MaxHP}[/]");
            AnsiConsole.WriteLine();

            if (!_gameState.Player.IsAlive)
            {
                AnsiConsole.MarkupLine("[red]The shock was too much. You collapse...[/]");
                _gameState.CurrentPhase = GamePhase.GameOver;
            }
            else
            {
                AnsiConsole.MarkupLine("[yellow]You can try again, but be careful...[/]");
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
            Console.ReadLine();
        }
    }
}
