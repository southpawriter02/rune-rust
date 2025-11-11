using Spectre.Console;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;

namespace RuneAndRust.ConsoleApp;

class Program
{
    private static GameState _gameState = new();
    private static DiceService _diceService = new();
    private static CommandParser _commandParser = new();
    private static SagaService _sagaService = new();
    private static LootService _lootService = new();
    private static EquipmentService _equipmentService = new();
    private static CombatEngine _combatEngine = new(_diceService, _sagaService, _lootService, _equipmentService);
    private static EnemyAI _enemyAI = new(_diceService);
    private static SaveRepository _saveRepository = new();

    static void Main(string[] args)
    {
        bool playAgain = true;

        while (playAgain)
        {
            // Reset game state for new game
            _gameState = new GameState();

            DisplayWelcomeScreen();

            // Show start menu (New Game or Load Game)
            var startChoice = ShowStartMenu();

            if (startChoice == "new")
            {
                CharacterCreation();
            }
            else if (startChoice == "load")
            {
                if (!LoadGame())
                {
                    // Load failed, return to start
                    continue;
                }
            }
            else
            {
                // User chose to exit
                break;
            }

            MainGameLoop();

            // Ask if player wants to play again
            playAgain = PromptPlayAgain();
        }

        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[yellow]Thanks for playing Rune & Rust![/]");
        AnsiConsole.WriteLine();
    }

    static bool PromptPlayAgain()
    {
        AnsiConsole.WriteLine();
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Play again?[/]")
                .AddChoices(new[] { "Yes - New Game", "No - Exit" })
        );

        return choice.StartsWith("Yes");
    }

    static string ShowStartMenu()
    {
        AnsiConsole.Clear();
        AnsiConsole.WriteLine();

        var rule = new Rule("[bold yellow]MAIN MENU[/]")
        {
            Justification = Justify.Center
        };
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        var choices = new List<string> { "New Game" };

        // Check if there are any saved games
        var saves = _saveRepository.ListSaves();
        if (saves.Count > 0)
        {
            choices.Add("Load Game");
        }

        choices.Add("Exit");

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]What would you like to do?[/]")
                .AddChoices(choices)
        );

        return choice switch
        {
            "New Game" => "new",
            "Load Game" => "load",
            _ => "exit"
        };
    }

    static bool LoadGame()
    {
        AnsiConsole.Clear();
        AnsiConsole.WriteLine();

        var rule = new Rule("[bold cyan]LOAD GAME[/]")
        {
            Justification = Justify.Center
        };
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        var saves = _saveRepository.ListSaves();

        if (saves.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No saved games found.[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
            Console.ReadLine();
            return false;
        }

        // Build save list with details
        var saveChoices = new List<string>();
        foreach (var save in saves)
        {
            var status = save.BossDefeated ? "COMPLETED" : "IN PROGRESS";
            var saveText = $"{save.CharacterName} - M{save.CurrentMilestone} {save.Class} - {status}";
            saveChoices.Add(saveText);
        }
        saveChoices.Add("Cancel");

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Select a save to load:[/]")
                .PageSize(10)
                .AddChoices(saveChoices)
        );

        if (choice == "Cancel")
        {
            return false;
        }

        // Extract character name from choice
        var selectedIndex = saveChoices.IndexOf(choice);
        var selectedSave = saves[selectedIndex];

        // Load the game (including equipment and room items - v0.3)
        var (loadedPlayer, loadedWorldState, roomItemsJson) = _saveRepository.LoadGame(selectedSave.CharacterName);

        if (loadedPlayer == null || loadedWorldState == null)
        {
            AnsiConsole.MarkupLine("[red]Failed to load game![/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
            Console.ReadLine();
            return false;
        }

        // Restore game state
        _gameState.Player = loadedPlayer;
        _gameState.WorldState = loadedWorldState;

        // Restore world state
        RestoreWorldState(loadedWorldState);

        // Restore room items (v0.3)
        if (!string.IsNullOrEmpty(roomItemsJson))
        {
            _saveRepository.RestoreRoomItems(_gameState.World, roomItemsJson);
        }

        // Set current room
        foreach (var room in _gameState.World.Rooms.Values)
        {
            if (room.Id == loadedWorldState.CurrentRoomId)
            {
                _gameState.CurrentRoom = room;
                break;
            }
        }

        _gameState.CurrentPhase = GamePhase.Exploration;

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[green]✓[/] Game loaded: {loadedPlayer.Name} - Milestone {loadedPlayer.CurrentMilestone} {loadedPlayer.Class}");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
        Console.ReadLine();

        return true;
    }

    static void RestoreWorldState(WorldState worldState)
    {
        // Mark cleared rooms
        foreach (var roomId in worldState.ClearedRoomIds)
        {
            foreach (var room in _gameState.World.Rooms.Values)
            {
                if (room.Id == roomId)
                {
                    room.HasBeenCleared = true;
                }
            }
        }

        // Restore puzzle state
        if (worldState.PuzzleSolved)
        {
            var puzzleRoom = _gameState.World.GetRoom("Puzzle Chamber");
            puzzleRoom.IsPuzzleSolved = true;
            _gameState.World.UnlockPuzzleDoor();
        }
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
            "[yellow]Version 0.3[/] - Equipment & Loot Systems\n" +
            "Now with 36 unique items, quality tiers, loot drops, and inventory management![/]"
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

        // Add starting loot to world (v0.3)
        _gameState.World.AddStartingLoot(_gameState.Player);

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
                return;
            }

            // Handle current phase
            switch (_gameState.CurrentPhase)
            {
                case GamePhase.Exploration:
                    ExplorationLoop();
                    break;
                case GamePhase.Combat:
                    CombatLoop();
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

        // Show tutorial hints in the entrance
        if (_gameState.CurrentRoom.IsStartRoom)
        {
            var tutorialPanel = new Panel(
                "[yellow]TUTORIAL HINTS[/]\n\n" +
                "• Type [cyan]north/south/east/west[/] (or [cyan]n/s/e/w[/]) to move\n" +
                "• Type [cyan]look[/] (or [cyan]l[/]) to examine your surroundings\n" +
                "• Type [cyan]stats[/] to view your character sheet\n" +
                "• Type [cyan]help[/] (or [cyan]h[/]) for a full command list\n" +
                "• In combat, use [cyan]attack[/], [cyan]defend[/], or [cyan]ability[/]\n" +
                "• Manage your stamina wisely - abilities are powerful but costly!"
            )
            {
                Border = BoxBorder.Rounded,
                BorderColor = Color.Yellow,
                Padding = new Padding(1, 0)
            };
            AnsiConsole.Write(tutorialPanel);
            AnsiConsole.WriteLine();
        }

        // Check for automatic triggers
        if (_gameState.ShouldTriggerCombat())
        {
            UIHelper.DisplayCombatStart(_gameState.CurrentRoom.Enemies);
            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to begin combat...[/]");
            Console.ReadLine();

            // Initialize combat
            var canFlee = !_gameState.CurrentRoom.IsBossRoom;
            _gameState.Combat = _combatEngine.InitializeCombat(_gameState.Player, _gameState.CurrentRoom.Enemies, canFlee);
            _gameState.CurrentPhase = GamePhase.Combat;
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
            UIHelper.DisplayVictory(_gameState.Player);
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

                case CommandType.Legend:
                case CommandType.Saga:
                    HandleLegend();
                    break;

                case CommandType.Milestone:
                    HandleMilestone(_gameState.Player);
                    break;

                case CommandType.Spend:
                case CommandType.PP:
                    HandlePPSpending(_gameState.Player);
                    break;

                case CommandType.Save:
                    HandleSave();
                    break;

                case CommandType.Help:
                    HandleHelp();
                    break;

                case CommandType.Quit:
                    AnsiConsole.MarkupLine("[yellow]Thanks for playing![/]");
                    _gameState.CurrentPhase = GamePhase.GameOver;
                    break;

                case CommandType.Inventory:
                    HandleInventory();
                    break;

                case CommandType.Equip:
                    HandleEquip(command.Target);
                    break;

                case CommandType.Unequip:
                    HandleUnequip(command.Target);
                    break;

                case CommandType.Pickup:
                    HandlePickup(command.Target);
                    break;

                case CommandType.Drop:
                    HandleDrop(command.Target);
                    break;

                case CommandType.Compare:
                    HandleCompare(command.Target);
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

        // Auto-save on room transition
        try
        {
            _gameState.UpdateWorldState();
            _saveRepository.SaveGame(_gameState.Player, _gameState.WorldState, _gameState.World);
            AnsiConsole.MarkupLine("[dim]Game auto-saved.[/]");
        }
        catch
        {
            // Silently fail auto-save - don't interrupt gameplay
        }
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

    static void HandleLegend()
    {
        AnsiConsole.Clear();
        var player = _gameState.Player;

        AnsiConsole.WriteLine();
        var legendRule = new Rule("[bold cyan]LEGEND & SAGA[/]")
        {
            Justification = Justify.Center
        };
        AnsiConsole.Write(legendRule);
        AnsiConsole.WriteLine();

        var legendToNext = _sagaService.CalculateLegendToNextMilestone(player.CurrentMilestone) - player.CurrentLegend;
        var legendText = player.CurrentMilestone >= 3
            ? $"[yellow]Current Milestone:[/] {player.CurrentMilestone} (MAX)\n" +
              $"[yellow]Total Legend:[/] {player.CurrentLegend}"
            : $"[yellow]Current Milestone:[/] {player.CurrentMilestone}\n" +
              $"[yellow]Current Legend:[/] {player.CurrentLegend}\n" +
              $"[yellow]Legend to Next Milestone:[/] {legendToNext}\n" +
              $"[yellow]Next Milestone At:[/] {player.LegendToNextMilestone} Legend";

        var panel = new Panel(legendText)
        {
            Border = BoxBorder.Rounded,
            BorderColor = Color.Yellow,
            Padding = new Padding(2, 1)
        };
        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
        Console.ReadLine();
    }


    static void HandleSave()
    {
        AnsiConsole.Clear();
        AnsiConsole.WriteLine();

        var saveRule = new Rule("[bold cyan]SAVE GAME[/]")
        {
            Justification = Justify.Center
        };
        AnsiConsole.Write(saveRule);
        AnsiConsole.WriteLine();

        try
        {
            // Update world state before saving
            _gameState.UpdateWorldState();

            // Save the game (including equipment and room items - v0.3)
            _saveRepository.SaveGame(_gameState.Player, _gameState.WorldState, _gameState.World);

            AnsiConsole.MarkupLine($"[green]✓ Game saved successfully![/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[dim]Character: {_gameState.Player.Name}[/]");
            AnsiConsole.MarkupLine($"[dim]Milestone: {_gameState.Player.CurrentMilestone}[/]");
            AnsiConsole.MarkupLine($"[dim]Location: {_gameState.CurrentRoom.Name}[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to save game:[/] {ex.Message.EscapeMarkup()}");
        }

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

            // Add puzzle reward (v0.3)
            _gameState.World.AddPuzzleReward(_gameState.Player);
            AnsiConsole.MarkupLine("[yellow]You find something valuable in the newly unlocked chamber![/]");

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

    static void CombatLoop()
    {
        var combat = _gameState.Combat;
        if (combat == null) return;

        while (combat.IsActive)
        {
            // Check for combat end
            if (_combatEngine.IsCombatOver(combat))
            {
                HandleCombatEnd(combat);
                return;
            }

            // Get current participant
            var currentParticipant = combat.CurrentParticipant;

            if (currentParticipant.IsPlayer)
            {
                // Player turn
                HandlePlayerTurn(combat);
            }
            else
            {
                // Enemy turn
                var enemy = (Enemy)currentParticipant.Character!;
                HandleEnemyTurn(combat, enemy);
            }

            // Check for combat end after turn
            if (_combatEngine.IsCombatOver(combat))
            {
                HandleCombatEnd(combat);
                return;
            }

            // Advance to next turn
            _combatEngine.NextTurn(combat);
        }
    }

    static void HandlePlayerTurn(CombatState combat)
    {
        bool turnComplete = false;

        while (!turnComplete)
        {
            // Display combat state
            UIHelper.DisplayCombatState(combat);

            // Show recent combat log
            if (combat.CombatLog.Count > 0)
            {
                UIHelper.DisplayCombatLog(combat.CombatLog, 8);
            }

            // Get player action
            var action = UIHelper.PromptCombatAction(combat);

            switch (action)
            {
                case "attack":
                    turnComplete = HandlePlayerAttack(combat);
                    break;

                case "defend":
                    HandlePlayerDefend(combat);
                    turnComplete = true;
                    break;

                case "ability":
                    turnComplete = HandlePlayerAbility(combat);
                    break;

                case "flee":
                    if (_combatEngine.PlayerFlee(combat))
                    {
                        combat.IsActive = false;
                        _gameState.CurrentPhase = GamePhase.Exploration;
                        AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
                        Console.ReadLine();
                        return;
                    }
                    else
                    {
                        turnComplete = true;
                    }
                    break;

                case "stats":
                    HandleStats();
                    break;

                default:
                    AnsiConsole.MarkupLine("[red]Unknown action![/]");
                    break;
            }
        }

        // Show action result
        AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
        Console.ReadLine();
    }

    static bool HandlePlayerAttack(CombatState combat)
    {
        var targetIndex = UIHelper.PromptEnemyTarget(combat);
        var target = _combatEngine.GetEnemyByIndex(combat, targetIndex);

        if (target == null)
        {
            combat.AddLogEntry("Invalid target!");
            return false;
        }

        _combatEngine.PlayerAttack(combat, target);
        return true;
    }

    static void HandlePlayerDefend(CombatState combat)
    {
        _combatEngine.PlayerDefend(combat);
    }

    static bool HandlePlayerAbility(CombatState combat)
    {
        var abilityName = UIHelper.PromptAbilityChoice(combat.Player);

        if (abilityName == "cancel")
        {
            return false;
        }

        var ability = combat.Player.Abilities.FirstOrDefault(a =>
            a.Name.Equals(abilityName, StringComparison.OrdinalIgnoreCase));

        if (ability == null)
        {
            combat.AddLogEntry("Ability not found!");
            return false;
        }

        // Check if ability needs a target
        Enemy? target = null;
        if (ability.Type == AbilityType.Attack || ability.Type == AbilityType.Control)
        {
            var targetIndex = UIHelper.PromptEnemyTarget(combat);
            target = _combatEngine.GetEnemyByIndex(combat, targetIndex);

            if (target == null)
            {
                combat.AddLogEntry("Invalid target!");
                return false;
            }
        }

        return _combatEngine.PlayerUseAbility(combat, ability, target);
    }

    static void HandleEnemyTurn(CombatState combat, Enemy enemy)
    {
        // Display combat state
        UIHelper.DisplayCombatState(combat);
        UIHelper.DisplayCombatLog(combat.CombatLog, 8);

        AnsiConsole.MarkupLine($"[red]{enemy.Name} is preparing to act...[/]");
        System.Threading.Thread.Sleep(1000);

        // Determine and execute action
        var action = _enemyAI.DetermineAction(enemy);
        _enemyAI.ExecuteAction(enemy, action, combat.Player, combat);

        AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
        Console.ReadLine();
    }

    static void HandleCombatEnd(CombatState combat)
    {
        UIHelper.DisplayCombatState(combat);
        UIHelper.DisplayCombatLog(combat.CombatLog, 15);

        if (!combat.Player.IsAlive)
        {
            // Player defeated
            AnsiConsole.MarkupLine("[red]You have been defeated...[/]");
            _gameState.CurrentPhase = GamePhase.GameOver;
        }
        else
        {
            // Victory!
            AnsiConsole.MarkupLine("[green]✓ Combat victory![/]");
            _gameState.ClearCurrentRoom();

            // Generate loot (v0.3)
            _combatEngine.GenerateLoot(combat, _gameState.CurrentRoom);
            UIHelper.DisplayCombatLog(combat.CombatLog, 20);

            // Check for milestone
            while (_sagaService.CanReachMilestone(combat.Player))
            {
                AnsiConsole.WriteLine();
                HandleMilestone(combat.Player);
            }

            _gameState.CurrentPhase = GamePhase.Exploration;
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
        Console.ReadLine();
    }

    static void HandleMilestone(PlayerCharacter player)
    {
        AnsiConsole.Clear();
        AnsiConsole.WriteLine();

        // Display milestone banner
        var milestoneRule = new Rule("[bold yellow]⚡ MILESTONE REACHED! ⚡[/]")
        {
            Justification = Justify.Center,
            Style = new Style(Color.Yellow)
        };
        AnsiConsole.Write(milestoneRule);
        AnsiConsole.WriteLine();

        var oldMilestone = player.CurrentMilestone;
        var newMilestone = oldMilestone + 1;

        AnsiConsole.MarkupLine($"[green]Congratulations! You have reached Milestone {newMilestone}![/]");
        AnsiConsole.WriteLine();

        // Show rewards
        var rewardsPanel = new Panel(
            "[yellow]Milestone Rewards:[/]\n\n" +
            "• [green]+10 Max HP[/]\n" +
            "• [green]+5 Max Stamina[/]\n" +
            "• [green]+1 Progression Point (PP)[/]\n" +
            "• [green]Full HP and Stamina Restored[/]"
        )
        {
            Border = BoxBorder.Rounded,
            BorderColor = Color.Green,
            Padding = new Padding(2, 1)
        };
        AnsiConsole.Write(rewardsPanel);
        AnsiConsole.WriteLine();

        // Perform milestone advancement
        _sagaService.ReachMilestone(player);

        AnsiConsole.MarkupLine($"[green]You are now at Milestone {newMilestone}![/]");
        AnsiConsole.MarkupLine($"[yellow]Unspent PP: {player.ProgressionPoints}[/]");
        AnsiConsole.MarkupLine($"[dim]Legend: {player.CurrentLegend} / {player.LegendToNextMilestone} to next milestone[/]");
        AnsiConsole.WriteLine();

        // Prompt to spend PP
        var spendChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]You have PP to spend. What would you like to do?[/]")
                .AddChoices(new[] { "Spend PP Now", "Save for Later" })
        );

        if (spendChoice == "Spend PP Now")
        {
            HandlePPSpending(player);
        }
        else
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]You can spend PP later using the 'spend' or 'pp' command.[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
            Console.ReadLine();
        }
    }

    static void HandlePPSpending(PlayerCharacter player)
    {
        while (player.ProgressionPoints > 0)
        {
            AnsiConsole.Clear();
            AnsiConsole.WriteLine();

            var ppRule = new Rule($"[bold cyan]PROGRESSION POINTS: {player.ProgressionPoints} Unspent[/]")
            {
                Justification = Justify.Center
            };
            AnsiConsole.Write(ppRule);
            AnsiConsole.WriteLine();

            var choices = new List<string>();
            if (player.ProgressionPoints >= 1)
            {
                choices.Add("Increase Attribute (1 PP)");
            }
            if (player.ProgressionPoints >= 5)
            {
                choices.Add("Advance Ability Rank (5 PP)");
            }
            choices.Add("Save PP for Later");

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]How would you like to spend your PP?[/]")
                    .AddChoices(choices)
            );

            if (choice == "Save PP for Later")
            {
                break;
            }
            else if (choice.StartsWith("Increase Attribute"))
            {
                SpendPPOnAttributes(player);
            }
            else if (choice.StartsWith("Advance Ability"))
            {
                SpendPPOnAbilities(player);
            }
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[green]✓ PP spending complete![/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
        Console.ReadLine();
    }

    static void SpendPPOnAttributes(PlayerCharacter player)
    {
        AnsiConsole.Clear();
        AnsiConsole.WriteLine();

        // Show current attributes
        AnsiConsole.MarkupLine($"[yellow]Current Attributes (PP Available: {player.ProgressionPoints}):[/]");
        AnsiConsole.MarkupLine($"  MIGHT:       {player.Attributes.Might} {(player.Attributes.Might >= 6 ? "[dim](MAX)[/]" : "")}");
        AnsiConsole.MarkupLine($"  FINESSE:     {player.Attributes.Finesse} {(player.Attributes.Finesse >= 6 ? "[dim](MAX)[/]" : "")}");
        AnsiConsole.MarkupLine($"  WITS:        {player.Attributes.Wits} {(player.Attributes.Wits >= 6 ? "[dim](MAX)[/]" : "")}");
        AnsiConsole.MarkupLine($"  WILL:        {player.Attributes.Will} {(player.Attributes.Will >= 6 ? "[dim](MAX)[/]" : "")}");
        AnsiConsole.MarkupLine($"  STURDINESS:  {player.Attributes.Sturdiness} {(player.Attributes.Sturdiness >= 6 ? "[dim](MAX)[/]" : "")}");
        AnsiConsole.WriteLine();

        // Build list of attributes that can be increased
        var availableAttributes = new List<string>();
        if (player.Attributes.Might < 6) availableAttributes.Add("MIGHT");
        if (player.Attributes.Finesse < 6) availableAttributes.Add("FINESSE");
        if (player.Attributes.Wits < 6) availableAttributes.Add("WITS");
        if (player.Attributes.Will < 6) availableAttributes.Add("WILL");
        if (player.Attributes.Sturdiness < 6) availableAttributes.Add("STURDINESS");
        availableAttributes.Add("Cancel");

        var attributeChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Choose an attribute to increase (1 PP):[/]")
                .AddChoices(availableAttributes)
        );

        if (attributeChoice != "Cancel")
        {
            if (_sagaService.SpendPPOnAttribute(player, attributeChoice))
            {
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine($"[green]✓ {attributeChoice} increased to {player.GetAttributeValue(attributeChoice)}![/]");
                AnsiConsole.MarkupLine($"[yellow]PP Remaining: {player.ProgressionPoints}[/]");
                AnsiConsole.WriteLine();
                System.Threading.Thread.Sleep(1000);
            }
        }
    }

    static void SpendPPOnAbilities(PlayerCharacter player)
    {
        AnsiConsole.Clear();
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine($"[yellow]Advance Ability Rank (PP Available: {player.ProgressionPoints}):[/]");
        AnsiConsole.WriteLine();

        // Show abilities with current ranks
        var abilityChoices = new List<string>();
        foreach (var ability in player.Abilities)
        {
            var rankInfo = ability.CurrentRank >= 2 ? " [dim](MAX for v0.2)[/]" : $" [green](Rank {ability.CurrentRank} → {ability.CurrentRank + 1})[/]";
            var costInfo = ability.CurrentRank < 2 ? $" - {ability.CostToRank2} PP" : "";
            abilityChoices.Add($"{ability.Name}{rankInfo}{costInfo}");
        }
        abilityChoices.Add("Cancel");

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Choose an ability to advance:[/]")
                .AddChoices(abilityChoices)
        );

        if (choice != "Cancel")
        {
            var abilityName = choice.Split(" [")[0]; // Extract ability name
            var ability = player.Abilities.FirstOrDefault(a => a.Name == abilityName);

            if (ability != null && _sagaService.AdvanceAbilityRank(player, ability))
            {
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine($"[green]✓ {ability.Name} advanced to Rank {ability.CurrentRank}![/]");
                AnsiConsole.MarkupLine($"[yellow]PP Remaining: {player.ProgressionPoints}[/]");
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[cyan]Rank 2 improvements applied![/]");
                AnsiConsole.WriteLine();
                System.Threading.Thread.Sleep(1500);
            }
            else
            {
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[red]Cannot advance this ability (not enough PP or already at max rank)[/]");
                System.Threading.Thread.Sleep(1000);
            }
        }
    }

    // ============================================================
    // EQUIPMENT & INVENTORY HANDLERS (v0.3)
    // ============================================================

    static void HandleInventory()
    {
        AnsiConsole.Clear();
        UIHelper.DisplayInventory(_gameState.Player);
        AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
        Console.ReadLine();
    }

    static void HandleEquip(string itemName)
    {
        if (string.IsNullOrWhiteSpace(itemName))
        {
            AnsiConsole.MarkupLine("[red]Equip what?[/] Specify an item name.");
            AnsiConsole.MarkupLine("[dim]Example: equip scavenged axe[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
            Console.ReadLine();
            return;
        }

        var item = _equipmentService.FindInInventory(_gameState.Player, itemName);
        if (item == null)
        {
            AnsiConsole.MarkupLine($"[red]You don't have '{itemName.EscapeMarkup()}' in your inventory.[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
            Console.ReadLine();
            return;
        }

        // Equip the item
        if (item.Type == EquipmentType.Weapon)
        {
            var oldWeapon = _gameState.Player.EquippedWeapon;
            _equipmentService.EquipWeapon(_gameState.Player, item);
            AnsiConsole.MarkupLine($"[green]✓[/] Equipped: [bold]{item.GetDisplayName()}[/]");
            if (oldWeapon != null)
            {
                AnsiConsole.MarkupLine($"[dim]Unequipped: {oldWeapon.GetDisplayName()}[/]");
            }
        }
        else if (item.Type == EquipmentType.Armor)
        {
            var oldArmor = _gameState.Player.EquippedArmor;
            _equipmentService.EquipArmor(_gameState.Player, item);
            AnsiConsole.MarkupLine($"[green]✓[/] Equipped: [bold]{item.GetDisplayName()}[/]");
            if (oldArmor != null)
            {
                AnsiConsole.MarkupLine($"[dim]Unequipped: {oldArmor.GetDisplayName()}[/]");
            }
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
        Console.ReadLine();
    }

    static void HandleUnequip(string slot)
    {
        if (string.IsNullOrWhiteSpace(slot))
        {
            AnsiConsole.MarkupLine("[red]Unequip what?[/] Specify 'weapon' or 'armor'.");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
            Console.ReadLine();
            return;
        }

        slot = slot.ToLower();
        if (slot.Contains("weapon"))
        {
            if (_gameState.Player.EquippedWeapon == null)
            {
                AnsiConsole.MarkupLine("[yellow]You don't have a weapon equipped.[/]");
            }
            else if (_equipmentService.UnequipWeapon(_gameState.Player))
            {
                AnsiConsole.MarkupLine($"[green]✓[/] Unequipped weapon.");
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Inventory is full! Drop something first.[/]");
            }
        }
        else if (slot.Contains("armor"))
        {
            if (_gameState.Player.EquippedArmor == null)
            {
                AnsiConsole.MarkupLine("[yellow]You don't have armor equipped.[/]");
            }
            else if (_equipmentService.UnequipArmor(_gameState.Player))
            {
                AnsiConsole.MarkupLine($"[green]✓[/] Unequipped armor.");
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Inventory is full! Drop something first.[/]");
            }
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Invalid slot.[/] Specify 'weapon' or 'armor'.");
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
        Console.ReadLine();
    }

    static void HandlePickup(string itemName)
    {
        if (string.IsNullOrWhiteSpace(itemName))
        {
            AnsiConsole.MarkupLine("[red]Pick up what?[/] Specify an item name.");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
            Console.ReadLine();
            return;
        }

        var item = _equipmentService.FindOnGround(_gameState.CurrentRoom, itemName);
        if (item == null)
        {
            AnsiConsole.MarkupLine($"[red]There is no '{itemName.EscapeMarkup()}' on the ground here.[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
            Console.ReadLine();
            return;
        }

        if (_equipmentService.PickupItem(_gameState.Player, _gameState.CurrentRoom, item))
        {
            AnsiConsole.MarkupLine($"[green]✓[/] Picked up: [bold]{item.GetDisplayName()}[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Your inventory is full![/] Drop something first.");
            AnsiConsole.MarkupLine($"[dim]Inventory: {_gameState.Player.Inventory.Count}/{_gameState.Player.MaxInventorySize}[/]");
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
        Console.ReadLine();
    }

    static void HandleDrop(string itemName)
    {
        if (string.IsNullOrWhiteSpace(itemName))
        {
            AnsiConsole.MarkupLine("[red]Drop what?[/] Specify an item name.");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
            Console.ReadLine();
            return;
        }

        var item = _equipmentService.FindInInventory(_gameState.Player, itemName);
        if (item == null)
        {
            AnsiConsole.MarkupLine($"[red]You don't have '{itemName.EscapeMarkup()}' in your inventory.[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
            Console.ReadLine();
            return;
        }

        if (_equipmentService.DropItem(_gameState.Player, _gameState.CurrentRoom, item))
        {
            AnsiConsole.MarkupLine($"[green]✓[/] Dropped: [bold]{item.GetDisplayName()}[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Failed to drop item.[/]");
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
        Console.ReadLine();
    }

    static void HandleCompare(string itemName)
    {
        if (string.IsNullOrWhiteSpace(itemName))
        {
            AnsiConsole.MarkupLine("[red]Compare what?[/] Specify an item name.");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
            Console.ReadLine();
            return;
        }

        // Try to find in inventory first
        var item = _equipmentService.FindInInventory(_gameState.Player, itemName);

        // If not in inventory, try ground
        if (item == null)
        {
            item = _equipmentService.FindOnGround(_gameState.CurrentRoom, itemName);
        }

        if (item == null)
        {
            AnsiConsole.MarkupLine($"[red]Cannot find '{itemName.EscapeMarkup()}' in your inventory or on the ground.[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
            Console.ReadLine();
            return;
        }

        // Determine what to compare against
        Equipment? currentItem = null;
        if (item.Type == EquipmentType.Weapon)
        {
            currentItem = _gameState.Player.EquippedWeapon;
        }
        else if (item.Type == EquipmentType.Armor)
        {
            currentItem = _gameState.Player.EquippedArmor;
        }

        var comparison = _equipmentService.CompareEquipment(currentItem, item);
        UIHelper.DisplayEquipmentComparison(comparison);

        AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
        Console.ReadLine();
    }
}
