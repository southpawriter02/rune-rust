using Spectre.Console;
using RuneAndRust.Core;
using RuneAndRust.Core.Dialogue;
using RuneAndRust.Core.Population;
using RuneAndRust.Core.Quests;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using Serilog;
using Serilog.Events;

namespace RuneAndRust.ConsoleApp;

class Program
{
    private static GameState _gameState = new();
    private static DiceService _diceService = new();
    private static CommandParser _commandParser = new();
    private static SagaService _sagaService = new();
    private static LootService _lootService = new();
    private static EquipmentService _equipmentService = new();
    private static TraumaEconomyService _traumaService = new(); // [v0.6]
    private static HazardService _hazardService = new(_diceService, _traumaService); // [v0.6]
    private static CurrencyService _currencyService = new(); // [v0.9]
    private static MerchantService _merchantService = new(); // [v0.9]
    private static PricingService _pricingService = new(); // [v0.9]
    private static TransactionService _transactionService = new(_currencyService, _pricingService); // [v0.9]
    // [v0.21.3] Advanced Status Effect System
    private static StatusEffectRepository _statusEffectRepository = new();
    private static AdvancedStatusEffectService _statusEffectService = new(_statusEffectRepository, _traumaService, _diceService);
    private static CombatEngine _combatEngine = new(_diceService, _sagaService, _lootService, _equipmentService, _hazardService, _currencyService, _statusEffectService);
    private static EnemyAI _enemyAI = new(_diceService);
    private static AdvancedMovementService _advancedMovement = new(); // v0.20.4
    private static SaveRepository _saveRepository = new();
    // v0.13: Persistent World State System
    private static WorldStateRepository _worldStateRepository = new();
    private static DestructionService _destructionService = new(_worldStateRepository, _diceService);

    static void Main(string[] args)
    {
        // Configure Serilog (v0.8.1)
        Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()  // Verbose logging in development
#else
            .MinimumLevel.Information()  // Less verbose in release
#endif
            .Enrich.WithThreadId()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] ({ThreadId}) {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: "logs/runerust-.log",
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: LogEventLevel.Debug,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
                retainedFileCountLimit: 7)
            .CreateLogger();

        try
        {
            Log.Information("Rune & Rust starting up...");

            bool playAgain = true;

            while (playAgain)
            {
                // Reset game state for new game
                _gameState = new GameState();

                // v0.8: Load NPC, Dialogue, and Quest databases
                InitializeV08Systems();

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

            Log.Information("Rune & Rust shutting down normally");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Unhandled exception in main game loop. Application terminating.");
            AnsiConsole.WriteException(ex);
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
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

    /// <summary>
    /// v0.8: Initialize NPC & Dialogue System
    /// Load databases and place NPCs in rooms
    /// </summary>
    static void InitializeV08Systems()
    {
        try
        {
            Log.Information("Loading v0.8 NPC & Dialogue systems and v0.9 Economy system...");

            // v0.9: Initialize Currency Service for Quest rewards
            _gameState.SetCurrencyService(_currencyService);

            // Load NPC, Dialogue, and Quest databases
            _gameState.NPCService.LoadNPCDatabase();
            _gameState.DialogueService.LoadDialogueDatabase();
            _gameState.QuestService.LoadQuestDatabase();

            // Place NPCs in designated rooms
            PlaceNPCsInWorld();

            Log.Information("v0.8 and v0.9 systems loaded successfully");
        }
        catch (Exception ex)
        {
            // Non-fatal error - game can continue without NPCs
            Log.Warning(ex, "Failed to load NPC system - game will continue without NPCs");
            Console.WriteLine($"Warning: Failed to load NPC system: {ex.Message}");
        }
    }

    /// <summary>
    /// v0.8, v0.9: Place NPCs and Merchants in their designated rooms
    /// </summary>
    static void PlaceNPCsInWorld()
    {
        var npcPlacements = new Dictionary<int, string>
        {
            // Regular NPCs
            { 2, "sigrun_scavenger" },
            { 5, "kjartan_smith" },
            { 8, "bjorn_exile" },
            { 10, "astrid_reader" },
            { 12, "thorvald_guard" },
            { 15, "gunnar_raider" },
            { 18, "rolf_hermit" },
            { 22, "eydis_survivor" },

            // v0.9: Merchants
            { 7, "kjartan_merchant" },    // General Merchant (MidgardCombine)
            { 14, "ragnhild_apothecary" }, // Apothecary (Independents)
            { 19, "ulf_scrapper" }          // Scrap Trader (RustClans)
        };

        foreach (var (roomId, npcId) in npcPlacements)
        {
            var npc = _gameState.NPCService.CreateNPCInstance(npcId);
            if (npc != null)
            {
                var room = _gameState.World.GetRoom(roomId);
                if (room != null)
                {
                    room.NPCs.Add(npc);

                    // v0.9: Initialize merchant inventories
                    if (npc is Merchant merchant)
                    {
                        _merchantService.InitializeCoreStock(merchant);
                        _merchantService.RestockInventory(merchant, DateTime.Now);
                        Log.Information("Initialized merchant: {MerchantName} ({MerchantType}) in Room {RoomId}",
                            merchant.Name, merchant.Type, roomId);
                    }
                }
            }
        }
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

        // Load the game (including equipment, room items, and NPC states - v0.3, v0.8)
        var (loadedPlayer, loadedWorldState, roomItemsJson, npcStatesJson, dungeonSeed, biomeId) = _saveRepository.LoadGame(selectedSave.CharacterName);

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
            _saveRepository.RestoreRoomItems(_gameState.World.Rooms, roomItemsJson);
        }

        // Restore NPC states (v0.8)
        if (!string.IsNullOrEmpty(npcStatesJson))
        {
            _saveRepository.RestoreNPCStates(_gameState.World.Rooms, npcStatesJson);
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

        // [v0.4] Restore puzzle state (deprecated - multiple puzzle rooms now)
        // Old v0.3 saves may have PuzzleSolved flag, but v0.4 has multiple puzzles
        // For backwards compatibility, we'll just ignore this for now
        if (worldState.PuzzleSolved)
        {
            // TODO: Migrate old saves to new puzzle system
            // For now, players will need to re-solve puzzles in v0.4
        }

        // v0.13: Apply persistent world state changes to all rooms
        var saveId = _worldStateRepository.GetSaveIdForCharacter(_gameState.Player.Name);
        if (saveId.HasValue)
        {
            Log.Information("Applying persistent world state changes for save ID: {SaveId}", saveId.Value);

            foreach (var room in _gameState.World.Rooms.Values)
            {
                var sectorSeed = ExtractSectorSeed(room.RoomId);
                var changes = _worldStateRepository.GetChangesForRoom(saveId.Value, sectorSeed, room.RoomId);

                if (changes.Count > 0)
                {
                    _destructionService.ApplyWorldStateChanges(room, changes, saveId.Value);
                    _destructionService.InitializeRoomElements(room);

                    Log.Debug("Applied {ChangeCount} world state changes to room: {RoomId}",
                        changes.Count, room.RoomId);
                }
            }

            Log.Information("World state restoration complete");
        }
        else
        {
            Log.Warning("No save ID found for character, skipping world state restoration");
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
                    "Mystic - Low HP, Ability Focus",
                    "Adept - WITS-based, Specialist (v0.7)" // NEW: v0.7 archetype
                })
        );

        CharacterClass selectedClass = classChoice switch
        {
            "Warrior - High HP, Melee Focus" => CharacterClass.Warrior,
            "Scavenger - Balanced, Tactical" => CharacterClass.Scavenger,
            "Mystic - Low HP, Ability Focus" => CharacterClass.Mystic,
            "Adept - WITS-based, Specialist (v0.7)" => CharacterClass.Adept, // NEW: v0.7
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

            // Initialize combat (v0.4: pass room for environmental hazards)
            var canFlee = !_gameState.CurrentRoom.IsBossRoom;
            _gameState.Combat = _combatEngine.InitializeCombat(
                _gameState.Player,
                _gameState.CurrentRoom.Enemies,
                _gameState.CurrentRoom,
                canFlee);
            _gameState.CurrentPhase = GamePhase.Combat;
            return;
        }

        if (_gameState.ShouldShowPuzzle())
        {
            _gameState.CurrentPhase = GamePhase.Puzzle;
            return;
        }

        // [v0.4] Check for talkable NPC encounter
        if (_gameState.CurrentRoom.HasTalkableNPC &&
            !_gameState.CurrentRoom.HasTalkedToNPC &&
            !_gameState.CurrentRoom.HasBeenCleared &&
            _gameState.CurrentRoom.Enemies.Count > 0)
        {
            var npcPanel = new Panel(
                "[yellow]⚠ SPECIAL ENCOUNTER[/]\n\n" +
                "The Forlorn Scholar regards you with hollow eyes. Its form flickers between solid and ethereal.\n\n" +
                "You sense it may be reasoned with... or you could attack first.\n\n" +
                "[cyan]• Type 'talk' or 'negotiate' to attempt peaceful resolution (WILL check)[/]\n" +
                "[cyan]• Type 'attack' to engage in combat immediately[/]"
            )
            {
                Border = BoxBorder.Rounded,
                    Padding = new Padding(1, 0)
            };
            AnsiConsole.Write(npcPanel);
            AnsiConsole.WriteLine();
        }

        // [v0.4] Check for victory condition (either boss room cleared)
        if ((_gameState.CurrentRoom.Name == "Arsenal Vault" || _gameState.CurrentRoom.Name == "Energy Core")
            && _gameState.CurrentRoom.HasBeenCleared)
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

                case CommandType.Rest:
                    HandleRest();
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

                case CommandType.Talk:
                    HandleTalk();
                    break;

                case CommandType.Quests:
                    HandleQuests();
                    break;

                case CommandType.Quest:
                    HandleQuestDetails(command.Target);
                    break;

                case CommandType.Reputation:
                    HandleReputation();
                    break;

                case CommandType.Shop:
                    HandleShop();
                    break;

                case CommandType.Buy:
                    HandleBuy(command.Arguments);
                    break;

                case CommandType.Sell:
                    HandleSell(command.Arguments);
                    break;

                // v0.13: Persistent World State System
                case CommandType.Destroy:
                    HandleDestroy(command.Target);
                    break;

                case CommandType.History:
                    HandleHistory();
                    break;

                case CommandType.Attack:
                    // [v0.4] Allow attacking Forlorn Scholar to skip negotiation
                    if (_gameState.CurrentRoom.HasTalkableNPC &&
                        !_gameState.CurrentRoom.HasTalkedToNPC &&
                        !_gameState.CurrentRoom.HasBeenCleared)
                    {
                        _gameState.CurrentRoom.HasTalkedToNPC = true; // Mark as resolved
                        AnsiConsole.MarkupLine("[red]You decide to attack first![/]");
                        AnsiConsole.WriteLine();
                        AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to begin combat...[/]");
                        Console.ReadLine();

                        var canFlee = !_gameState.CurrentRoom.IsBossRoom;
                        _gameState.Combat = _combatEngine.InitializeCombat(
                            _gameState.Player,
                            _gameState.CurrentRoom.Enemies,
                            _gameState.CurrentRoom,
                            canFlee);
                        _gameState.CurrentPhase = GamePhase.Combat;
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[red]There's nothing to attack here. (Use this command in combat)[/]");
                        AnsiConsole.WriteLine();
                        AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
                        Console.ReadLine();
                    }
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

        // [v0.6] Process check-based hazards on room entry (e.g., Unstable Flooring)
        if (_gameState.CurrentRoom.HasEnvironmentalHazard &&
            _gameState.CurrentRoom.IsHazardActive &&
            _gameState.CurrentRoom.HazardRequiresCheck)
        {
            var (success, damage, logMessage) = _hazardService.ProcessCheckBasedHazard(_gameState.CurrentRoom, _gameState.Player);

            AnsiConsole.MarkupLine("[yellow]" + logMessage.Replace("\n", "\n") + "[/]");
            AnsiConsole.WriteLine();

            if (!_gameState.Player.IsAlive)
            {
                AnsiConsole.MarkupLine("[red bold]You have fallen![/]");
                AnsiConsole.WriteLine();
                _gameState.CurrentPhase = GamePhase.GameOver;
                return;
            }

            System.Threading.Thread.Sleep(1000); // Pause to let player read hazard result
        }

        // [v0.4] Add loot to secret room when first discovered
        if (_gameState.CurrentRoom.Name == "Supply Cache" && _gameState.CurrentRoom.ItemsOnGround.Count == 0)
        {
            _gameState.World.AddSecretRoomLoot(_gameState.Player);
            AnsiConsole.MarkupLine("[yellow]You've discovered a pristine supply cache! Legendary equipment awaits...[/]");
            AnsiConsole.WriteLine();
        }

        // Auto-save on room transition
        try
        {
            _gameState.UpdateWorldState();
            _saveRepository.SaveGame(_gameState.Player, _gameState.WorldState, _gameState.World.IsProcedurallyGenerated);
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
            _saveRepository.SaveGame(_gameState.Player, _gameState.WorldState, _gameState.World.IsProcedurallyGenerated);

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

    static void HandleRest()
    {
        AnsiConsole.Clear();
        AnsiConsole.WriteLine();

        var restRule = new Rule("[bold cyan]REST[/]")
        {
            Justification = Justify.Center
        };
        AnsiConsole.Write(restRule);
        AnsiConsole.WriteLine();

        // Check if current room is a Sanctuary
        if (!_gameState.CurrentRoom.IsSanctuary)
        {
            AnsiConsole.MarkupLine("[yellow]⚠️ This location is not safe enough to rest.[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]You must find a Sanctuary to recover from trauma.[/]");
            AnsiConsole.MarkupLine("[dim]Sanctuaries: Entrance, Operations Center[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
            Console.ReadLine();
            return;
        }

        // Apply rest recovery
        var player = _gameState.Player;
        var traumaService = new TraumaEconomyService();

        int oldHP = player.HP;
        int oldStamina = player.Stamina;
        int oldStress = player.PsychicStress;

        player.HP = player.MaxHP;
        player.Stamina = player.MaxStamina;
        traumaService.ClearStress(player);
        player.RoomsExploredSinceRest = 0;

        AnsiConsole.MarkupLine("[green]You rest in the relative safety of the Sanctuary...[/]");
        AnsiConsole.MarkupLine("[dim]The screaming in your mind fades to a dull whisper...[/]");
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine($"[green]✅ HP:[/]             {oldHP}/{player.MaxHP} → {player.HP}/{player.MaxHP}");
        AnsiConsole.MarkupLine($"[green]✅ Stamina:[/]        {oldStamina}/{player.MaxStamina} → {player.Stamina}/{player.MaxStamina}");
        AnsiConsole.MarkupLine($"[green]✅ Psychic Stress:[/] {oldStress}/100 → {player.PsychicStress}/100");
        AnsiConsole.MarkupLine($"[yellow]⚠️ Corruption:[/]     {player.Corruption}/100 (unchanged - permanent)");
        AnsiConsole.WriteLine();

        // v0.7: Offer crafting option for Bone-Setters
        if (player.Specialization == Specialization.BoneSetter)
        {
            AnsiConsole.MarkupLine("[cyan]💊 Would you like to craft Field Medicine items?[/]");
            AnsiConsole.WriteLine();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[cyan]Rest Menu:[/]")
                    .AddChoices(new[] { "Craft Items", "Continue" })
            );

            if (choice == "Craft Items")
            {
                HandleCrafting();
            }
        }
        else
        {
            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
            Console.ReadLine();
        }
    }

    static void HandleCrafting()
    {
        var player = _gameState.Player;
        var craftingService = new CraftingService();

        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.WriteLine();

            var craftingRule = new Rule("[bold cyan]FIELD MEDICINE CRAFTING[/]")
            {
                Justification = Justify.Center
            };
            AnsiConsole.Write(craftingRule);
            AnsiConsole.WriteLine();

            // Display current components
            AnsiConsole.MarkupLine("[cyan]Your Crafting Components:[/]");
            if (player.CraftingComponents.Count == 0)
            {
                AnsiConsole.MarkupLine("[dim]  No components available[/]");
            }
            else
            {
                foreach (var component in player.CraftingComponents.OrderBy(c => c.Key))
                {
                    var componentInfo = CraftingComponent.Create(component.Key);
                    AnsiConsole.MarkupLine($"  [yellow]{componentInfo.Name}:[/] {component.Value}x");
                }
            }
            AnsiConsole.WriteLine();

            // Display current consumables
            AnsiConsole.MarkupLine($"[cyan]Current Consumables:[/] [yellow]{player.Consumables.Count}/{player.MaxConsumables}[/]");
            if (player.Consumables.Count > 0)
            {
                foreach (var item in player.Consumables.GroupBy(c => c.Name))
                {
                    int count = item.Count();
                    AnsiConsole.MarkupLine($"  [green]{item.Key}[/] x{count}");
                }
            }
            else
            {
                AnsiConsole.MarkupLine("[dim]  No consumables in inventory[/]");
            }
            AnsiConsole.WriteLine();

            // Get available recipes
            var recipes = craftingService.GetAvailableRecipes(player);
            if (recipes.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No recipes available. (Requires Bone-Setter specialization)[/]");
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
                Console.ReadLine();
                return;
            }

            // Build recipe choices with availability indicators
            var choices = new List<string>();
            foreach (var recipe in recipes)
            {
                bool hasComponents = recipe.HasRequiredComponents(player.CraftingComponents);
                string availability = hasComponents ? "[green]✓[/]" : "[red]✗[/]";
                choices.Add($"{availability} {recipe.Name} (DC {recipe.SkillCheckDC})");
            }
            choices.Add("[dim]Back to Rest[/]");

            // Show recipe selection menu
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[cyan]Select a recipe to craft:[/]")
                    .AddChoices(choices)
                    .HighlightStyle(new Style(Color.Cyan1))
            );

            if (choice.Contains("Back to Rest"))
            {
                return;
            }

            // Extract recipe name from choice (remove availability indicator and DC)
            string recipeName = choice.Substring(choice.IndexOf(']') + 2);
            recipeName = recipeName.Substring(0, recipeName.IndexOf(" (DC"));

            var selectedRecipe = craftingService.GetRecipeByName(recipeName);
            if (selectedRecipe == null)
                continue;

            // Show recipe details and confirm
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[yellow]Recipe:[/] {selectedRecipe.Name}");
            AnsiConsole.MarkupLine($"[dim]{selectedRecipe.Description}[/]");
            AnsiConsole.MarkupLine($"[yellow]Required Components:[/] {selectedRecipe.GetRequirementsDescription()}");
            AnsiConsole.MarkupLine($"[yellow]Difficulty Check:[/] DC {selectedRecipe.SkillCheckDC} ({selectedRecipe.SkillAttribute})");
            AnsiConsole.WriteLine();

            // Check if player has components
            if (!selectedRecipe.HasRequiredComponents(player.CraftingComponents))
            {
                var missing = selectedRecipe.GetMissingComponents(player.CraftingComponents);
                AnsiConsole.MarkupLine("[red]⚠️ Missing components:[/]");
                foreach (var item in missing)
                {
                    AnsiConsole.MarkupLine($"  [red]{item}[/]");
                }
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
                Console.ReadLine();
                continue;
            }

            // Check if inventory is full
            if (player.Consumables.Count >= player.MaxConsumables)
            {
                AnsiConsole.MarkupLine($"[red]⚠️ Consumables inventory full! ({player.Consumables.Count}/{player.MaxConsumables})[/]");
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
                Console.ReadLine();
                continue;
            }

            // Confirm crafting
            bool confirm = AnsiConsole.Confirm("[cyan]Craft this item?[/]");
            if (!confirm)
                continue;

            // Attempt crafting
            var result = craftingService.CraftItem(player, selectedRecipe);

            AnsiConsole.WriteLine();
            if (result.Success && result.CraftedItem != null)
            {
                // Success - add item and consume components
                player.Consumables.Add(result.CraftedItem);

                // Consume components
                foreach (var requirement in selectedRecipe.RequiredComponents)
                {
                    player.CraftingComponents[requirement.Key] -= requirement.Value;
                    if (player.CraftingComponents[requirement.Key] <= 0)
                    {
                        player.CraftingComponents.Remove(requirement.Key);
                    }
                }

                AnsiConsole.MarkupLine($"[green]✅ {result.Message}[/]");
                AnsiConsole.MarkupLine($"[green]Created:[/] {result.CraftedItem.GetDisplayName()}");
                AnsiConsole.MarkupLine($"[dim]{result.CraftedItem.GetEffectsDescription()}[/]");
            }
            else
            {
                // Failure - still consume components (wasted in failed attempt)
                foreach (var requirement in selectedRecipe.RequiredComponents)
                {
                    player.CraftingComponents[requirement.Key] -= requirement.Value;
                    if (player.CraftingComponents[requirement.Key] <= 0)
                    {
                        player.CraftingComponents.Remove(requirement.Key);
                    }
                }

                AnsiConsole.MarkupLine($"[red]❌ {result.Message}[/]");
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
            Console.ReadLine();
        }
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
            AnsiConsole.MarkupLine("[green]✓ Success![/] You solve the puzzle.");

            // Room-specific success messages
            if (_gameState.CurrentRoom.HasEnvironmentalHazard)
            {
                AnsiConsole.MarkupLine("[green]The unstable reactors power down safely.[/]");
            }
            else if (_gameState.CurrentRoom.Name == "Vault Corridor")
            {
                AnsiConsole.MarkupLine("[green]You notice a hidden door in the wall![/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[green]The sealed terminal unlocks with a soft chime.[/]");
            }

            _gameState.SolvePuzzle();

            // Add puzzle reward (v0.4: pass room name for context-specific rewards)
            _gameState.World.AddPuzzleReward(_gameState.CurrentRoom.Name, _gameState.Player);
            AnsiConsole.MarkupLine("[yellow]You find something valuable![/]");

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

    /// <summary>
    /// v0.8: Handle talking to NPCs with dialogue system
    /// Supports both v0.4 legacy encounters and v0.8 dialogue trees
    /// </summary>
    static void HandleTalk()
    {
        // v0.4: Handle legacy Forlorn Scholar encounter first
        if (_gameState.CurrentRoom.HasTalkableNPC &&
            !_gameState.CurrentRoom.HasTalkedToNPC &&
            !_gameState.CurrentRoom.HasBeenCleared &&
            _gameState.CurrentRoom.Enemies.Count > 0)
        {
            HandleForlornScholar();
            return;
        }

        // v0.8: Check for NPCs in room
        if (_gameState.CurrentRoom.NPCs.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]There's no one here to talk to.[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
            Console.ReadLine();
            return;
        }

        // Select NPC to talk to
        NPC? selectedNPC = null;
        var aliveNPCs = _gameState.CurrentRoom.NPCs.Where(n => n.IsAlive).ToList();

        if (aliveNPCs.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]There's no one alive here to talk to.[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
            Console.ReadLine();
            return;
        }
        else if (aliveNPCs.Count == 1)
        {
            selectedNPC = aliveNPCs[0];
        }
        else
        {
            var npcNames = aliveNPCs.Select(n => n.Name).ToList();
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Who do you want to talk to?[/]")
                    .AddChoices(npcNames)
            );
            selectedNPC = aliveNPCs.First(n => n.Name == choice);
        }

        if (selectedNPC == null)
        {
            return;
        }

        // Update NPC disposition based on reputation
        _gameState.NPCService.UpdateDisposition(selectedNPC, _gameState.Player);

        // Check if NPC is hostile
        if (_gameState.NPCService.IsHostile(selectedNPC))
        {
            AnsiConsole.MarkupLine($"[red]{selectedNPC.Name} is hostile and refuses to talk![/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
            Console.ReadLine();
            return;
        }

        // First encounter greeting
        if (!selectedNPC.HasBeenMet)
        {
            _gameState.NPCService.MarkAsMet(selectedNPC);
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[cyan]{selectedNPC.Name}:[/] \"{selectedNPC.InitialGreeting.EscapeMarkup()}\"");
            AnsiConsole.WriteLine();
            System.Threading.Thread.Sleep(1000);
        }

        // Start dialogue
        var dialogueNode = _gameState.DialogueService.StartConversation(selectedNPC, _gameState.Player);
        if (dialogueNode == null)
        {
            AnsiConsole.MarkupLine($"[red]{selectedNPC.Name} has nothing more to say.[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
            Console.ReadLine();
            return;
        }

        // Update quest objectives for talking to NPC
        var talkMessages = _gameState.QuestService.OnNPCTalk(selectedNPC.Id, _gameState.Player);
        foreach (var msg in talkMessages)
        {
            AnsiConsole.MarkupLine($"[yellow]{msg.EscapeMarkup()}[/]");
        }
        if (talkMessages.Count > 0)
        {
            AnsiConsole.WriteLine();
        }

        // Dialogue loop
        while (dialogueNode != null && !dialogueNode.EndsConversation)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[cyan]{selectedNPC.Name}:[/] \"{dialogueNode.Text.EscapeMarkup()}\"");
            AnsiConsole.WriteLine();

            var options = _gameState.DialogueService.GetAvailableOptions(dialogueNode, _gameState.Player);
            if (options.Count == 0)
            {
                AnsiConsole.MarkupLine("[dim](End of conversation)[/]");
                break;
            }

            var optionTexts = new List<string>();
            for (int i = 0; i < options.Count; i++)
            {
                var opt = options[i];
                var skillTag = opt.SkillCheck != null ?
                    _gameState.DialogueService.FormatSkillCheckTag(opt.SkillCheck) + " " : "";
                optionTexts.Add($"{skillTag}{opt.Text}");
            }

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Choose your response:[/]")
                    .AddChoices(optionTexts)
            );

            int choiceIndex = optionTexts.IndexOf(choice);
            var selectedOption = options[choiceIndex];
            var (nextNode, outcome) = _gameState.DialogueService.SelectOption(selectedOption, _gameState.Player);

            // Process outcome
            if (outcome != null)
            {
                var outcomeMessages = _gameState.DialogueService.ProcessOutcome(outcome, _gameState.Player, selectedNPC);
                if (outcomeMessages.Count > 0)
                {
                    AnsiConsole.WriteLine();
                    foreach (var msg in outcomeMessages)
                    {
                        AnsiConsole.MarkupLine($"[yellow]{msg.EscapeMarkup()}[/]");
                    }
                }

                // Handle quest-related outcomes
                if (outcome.Type == OutcomeType.QuestGiven && !string.IsNullOrEmpty(outcome.Data))
                {
                    if (_gameState.QuestService.AcceptQuest(outcome.Data, _gameState.Player))
                    {
                        var quest = _gameState.QuestService.GetQuest(outcome.Data);
                        AnsiConsole.MarkupLine($"[green]Quest accepted: {quest?.Title.EscapeMarkup() ?? outcome.Data.EscapeMarkup()}[/]");
                    }
                }
                else if (outcome.Type == OutcomeType.QuestComplete && !string.IsNullOrEmpty(outcome.Data))
                {
                    var completionMessages = _gameState.QuestService.CompleteQuest(outcome.Data, _gameState.Player);
                    foreach (var msg in completionMessages)
                    {
                        AnsiConsole.MarkupLine($"[green]{msg.EscapeMarkup()}[/]");
                    }
                }
            }

            dialogueNode = nextNode;
        }

        _gameState.DialogueService.EndConversation();
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
        Console.ReadLine();
    }

    /// <summary>
    /// v0.4: Handle legacy Forlorn Scholar encounter
    /// </summary>
    static void HandleForlornScholar()
    {
        _gameState.CurrentRoom.HasTalkedToNPC = true;

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[yellow]You attempt to reason with the Forlorn Scholar...[/]");
        AnsiConsole.MarkupLine("[dim]The data-ghost tilts its head, listening. Your words must reach its fragmented consciousness.[/]");
        AnsiConsole.WriteLine();
        System.Threading.Thread.Sleep(1500);

        var willValue = _gameState.Player.Attributes.Will;
        var result = _diceService.Roll(willValue);

        UIHelper.DisplayDiceRoll(result, "WILL Check");
        AnsiConsole.WriteLine();

        if (result.Successes >= 4)
        {
            AnsiConsole.MarkupLine("[green]✓ Success![/] The Forlorn Scholar recognizes you as an ally.");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[cyan]The data-ghost speaks in a crackling, distorted voice:[/]");
            AnsiConsole.MarkupLine("[italic]\"Survivor... I remember now. We were... researchers. The Blight came. " +
                "Everything fell. Beware the vault ahead—the Warden still guards it. Or... perhaps the Energy Core, " +
                "where the aberration dwells. Choose wisely.\"[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]The Scholar fades, leaving behind a token of knowledge.[/]");

            _gameState.ClearCurrentRoom();

            var legendGain = _gameState.CurrentRoom.Enemies[0].BaseLegendValue;
            _gameState.Player.CurrentLegend += legendGain;
            AnsiConsole.MarkupLine($"[yellow]+ {legendGain} Legend[/] (Peaceful Resolution)");

            var reward = _lootService.CreatePuzzleReward(_gameState.Player.Class);
            if (reward != null)
            {
                _gameState.CurrentRoom.ItemsOnGround.Add(reward);
                AnsiConsole.MarkupLine($"[yellow]The Scholar left behind: {reward.GetDisplayName()}[/]");
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
            Console.ReadLine();
        }
        else
        {
            AnsiConsole.MarkupLine("[red]✗ Failure![/] The Forlorn Scholar does not understand. It turns hostile!");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Communication has failed. Combat is inevitable.[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to begin combat...[/]");
            Console.ReadLine();

            var canFlee = !_gameState.CurrentRoom.IsBossRoom;
            _gameState.Combat = _combatEngine.InitializeCombat(
                _gameState.Player,
                _gameState.CurrentRoom.Enemies,
                _gameState.CurrentRoom,
                canFlee);
            _gameState.CurrentPhase = GamePhase.Combat;
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
            // v0.7: Check if player is [Seized] (complete action lockdown)
            if (combat.Player.SeizedTurnsRemaining > 0)
            {
                UIHelper.DisplayCombatState(combat);
                AnsiConsole.MarkupLine($"[red]⛓️ [[SEIZED]] You are completely immobilized and cannot act![/]");
                AnsiConsole.MarkupLine($"[dim]({combat.Player.SeizedTurnsRemaining} rounds remaining)[/]");
                combat.AddLogEntry($"{combat.Player.Name} is [[Seized]] and cannot act!");

                AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to skip turn...[/]");
                Console.ReadLine();
                return; // End turn immediately
            }

            // v0.7: Check if player is performing (action restrictions)
            if (combat.Player.IsPerforming)
            {
                // Display combat state with performance status
                UIHelper.DisplayCombatState(combat);

                AnsiConsole.MarkupLine($"[magenta]🎵 Performing: {combat.Player.CurrentPerformance} ({combat.Player.PerformingTurnsRemaining} rounds remaining)[/]");
                AnsiConsole.MarkupLine("[dim]While performing, you cannot take other actions.[/]");

                var performanceChoice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[yellow]Performance Options:[/]")
                        .AddChoices(new[] { "Continue Performance", "End Performance Early", "Stats - View character sheet" })
                );

                if (performanceChoice == "Continue Performance")
                {
                    combat.AddLogEntry($"{combat.Player.Name} continues the performance...");
                    turnComplete = true;
                }
                else if (performanceChoice == "End Performance Early")
                {
                    var performanceService = new PerformanceService();
                    var endMessage = performanceService.EndPerformance(combat.Player, forced: false);
                    combat.AddLogEntry(endMessage);
                    turnComplete = true;
                }
                else if (performanceChoice.StartsWith("Stats"))
                {
                    HandleStats();
                }
                continue; // Skip normal action processing
            }

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

                case "move": // v0.20.4
                    turnComplete = HandlePlayerMovement(combat);
                    break;

                case "stance": // v0.21.1
                    turnComplete = HandlePlayerStanceChange(combat);
                    break;

                case "item":
                    turnComplete = HandlePlayerUseConsumable(combat);
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

    /// <summary>
    /// v0.21.1: Handle player changing combat stance.
    /// Stance changes are FREE ACTIONS (don't consume turn) - 1 free shift per turn.
    /// </summary>
    static bool HandlePlayerStanceChange(CombatState combat)
    {
        var stanceChoice = UIHelper.PromptStanceChoice(combat.Player);

        if (stanceChoice == "cancel")
        {
            return false; // Didn't use turn, go back to action menu
        }

        // Map choice to StanceType
        StanceType newStanceType = stanceChoice switch
        {
            "calculated" => StanceType.Calculated,
            "aggressive" => StanceType.Aggressive,
            "defensive" => StanceType.Defensive,
            "evasive" => StanceType.Evasive,
            _ => StanceType.Calculated
        };

        var stanceService = new StanceService();
        var success = stanceService.SwitchStance(combat.Player, newStanceType, combat);

        if (!success)
        {
            // Already in that stance or no shifts remaining - didn't consume turn
            return false;
        }

        // v0.21.1 SPEC: Stance changes are FREE ACTIONS (don't consume turn)
        // Player can continue to take their turn after changing stance
        return false;
    }

    static bool HandlePlayerUseConsumable(CombatState combat)
    {
        var player = combat.Player;

        if (player.Consumables.Count == 0)
        {
            combat.AddLogEntry("No consumables available!");
            return false;
        }

        // Group consumables by name for display
        var groupedConsumables = player.Consumables
            .GroupBy(c => c.Name)
            .Select(g => new { Name = g.First().GetDisplayName(), Count = g.Count(), Item = g.First() })
            .ToList();

        var choices = groupedConsumables
            .Select(g => $"{g.Name} x{g.Count} - {g.Item.GetEffectsDescription()}")
            .ToList();
        choices.Add("Cancel");

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Select a consumable to use:[/]")
                .AddChoices(choices)
                .HighlightStyle(new Style(Color.Cyan1))
        );

        if (choice == "Cancel")
        {
            return false;
        }

        // Extract consumable name from choice (remove count and effects)
        string consumableName = choice.Split(" x")[0].Trim();
        if (consumableName.EndsWith(" ⭐"))
        {
            consumableName = consumableName.Substring(0, consumableName.Length - 2).Trim();
        }

        // Find the consumable
        var consumable = player.Consumables.FirstOrDefault(c =>
            c.Name.Equals(consumableName, StringComparison.OrdinalIgnoreCase) ||
            c.GetDisplayName().Equals(consumableName, StringComparison.OrdinalIgnoreCase));

        if (consumable == null)
        {
            combat.AddLogEntry("Consumable not found!");
            return false;
        }

        return _combatEngine.PlayerUseConsumable(combat, consumable);
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

    /// <summary>
    /// v0.20.4: Handle player advanced movement action
    /// </summary>
    static bool HandlePlayerMovement(CombatState combat)
    {
        var player = combat.Player;

        // Ensure grid and position are valid
        if (combat.Grid == null || player.Position == null)
        {
            combat.AddLogEntry("Movement not available in this combat!");
            return false;
        }

        // Prompt for movement type
        var movementType = UIHelper.PromptMovementChoice(player, combat.Grid);

        if (movementType == "cancel")
        {
            return false;
        }

        // Prompt for target position
        var targetPosition = UIHelper.PromptMovementTarget(combat.Grid, player.Position.Value);

        if (targetPosition == null)
        {
            return false;
        }

        // Execute movement based on type
        AdvancedMovementResult result;

        switch (movementType)
        {
            case "leap":
                result = _advancedMovement.Leap(player, targetPosition.Value, combat.Grid);
                break;

            case "dash":
                result = _advancedMovement.Dash(player, targetPosition.Value, combat.Grid);
                break;

            case "blink":
                result = _advancedMovement.Blink(player, targetPosition.Value, combat.Grid);
                break;

            case "climb":
                result = _advancedMovement.Climb(player, targetPosition.Value, combat.Grid);
                break;

            case "safestep":
                result = _advancedMovement.SafeStep(player, targetPosition.Value, combat.Grid);
                break;

            default:
                combat.AddLogEntry("Invalid movement type!");
                return false;
        }

        // Display result
        combat.AddLogEntry($"{player.Name} attempts {movementType}!");
        combat.AddLogEntry($"  {result.Message}");

        if (result.AlternatePosition != null)
        {
            combat.AddLogEntry($"  Landed at: {result.AlternatePosition.Value}");
        }

        return result.Success;
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

            // v0.8: Update quest objectives for defeated enemies
            foreach (var enemy in combat.Enemies)
            {
                var questMessages = _gameState.QuestService.OnEnemyKilled(enemy.Id, combat.Player);
                foreach (var msg in questMessages)
                {
                    AnsiConsole.MarkupLine($"[yellow]{msg.EscapeMarkup()}[/]");
                }
            }

            // v0.13: Record defeated enemies to persistent world state
            var saveId = _worldStateRepository.GetSaveIdForCharacter(combat.Player.Name);
            if (saveId.HasValue && combat.CurrentRoom != null)
            {
                foreach (var enemy in combat.Enemies)
                {
                    _destructionService.RecordEnemyDefeat(
                        combat.CurrentRoom,
                        enemy,
                        saveId.Value,
                        combat.CurrentTurnIndex,
                        droppedLoot: true); // Loot generation happens next

                    Log.Debug("Recorded enemy defeat to world state: {EnemyName} in {RoomId}",
                        enemy.Name, combat.CurrentRoom.RoomId);
                }
            }

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

            // v0.7: Check for specialization unlock
            if (player.ProgressionPoints >= 3 &&
                player.Specialization == Specialization.None &&
                SpecializationFactory.GetAvailableSpecializations(player.Class).Count > 0)
            {
                choices.Add("⭐ Unlock Specialization (3 PP)");
            }

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
            else if (choice.StartsWith("⭐ Unlock Specialization"))
            {
                HandleSpecializationSelection(player);
                // Break after specialization selection since it's a major choice
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

    static void HandleSpecializationSelection(PlayerCharacter player)
    {
        AnsiConsole.Clear();
        AnsiConsole.WriteLine();

        var specializationRule = new Rule("[bold cyan]⭐ SPECIALIZATION UNLOCK ⭐[/]")
        {
            Justification = Justify.Center
        };
        AnsiConsole.Write(specializationRule);
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[yellow]You have reached a pivotal moment in your journey![/]");
        AnsiConsole.MarkupLine("[dim]Choose a specialization to define your role and unlock powerful abilities.[/]");
        AnsiConsole.WriteLine();

        // Get available specializations for player's class
        var availableSpecs = SpecializationFactory.GetAvailableSpecializations(player.Class);

        if (availableSpecs.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No specializations available for your class.[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
            Console.ReadLine();
            return;
        }

        // Build choice list with specialization names
        var choices = availableSpecs.Select(s => s.ToString()).ToList();
        choices.Add("Cancel");

        // Display specialization options
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[yellow]Available Specializations for {player.Class}:[/]")
                .AddChoices(choices)
                .HighlightStyle(new Style(Color.Cyan1))
        );

        if (choice == "Cancel")
        {
            return;
        }

        // Parse selected specialization
        if (!Enum.TryParse<Specialization>(choice, out var selectedSpec))
        {
            AnsiConsole.MarkupLine("[red]Invalid specialization selection.[/]");
            System.Threading.Thread.Sleep(1000);
            return;
        }

        // Show detailed description
        AnsiConsole.Clear();
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold yellow]{selectedSpec}[/]");
        AnsiConsole.WriteLine();

        var description = SpecializationFactory.GetSpecializationDescription(selectedSpec);
        var panel = new Panel(description)
        {
            Border = BoxBorder.Rounded,
            Padding = new Padding(2, 1)
        };
        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();

        // Confirm selection
        AnsiConsole.MarkupLine($"[yellow]Cost: 3 PP (Current: {player.ProgressionPoints} PP)[/]");
        AnsiConsole.MarkupLine("[dim]This is a permanent choice and cannot be changed![/]");
        AnsiConsole.WriteLine();

        bool confirm = AnsiConsole.Confirm($"[cyan]Unlock {selectedSpec} specialization?[/]");

        if (!confirm)
        {
            AnsiConsole.MarkupLine("[yellow]Specialization selection cancelled.[/]");
            System.Threading.Thread.Sleep(1000);
            return;
        }

        // Apply specialization
        bool success = _sagaService.UnlockSpecialization(player, selectedSpec);

        if (success)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[green]✅ Specialization unlocked: {selectedSpec}![/]");
            AnsiConsole.MarkupLine($"[yellow]PP Remaining: {player.ProgressionPoints}[/]");
            AnsiConsole.WriteLine();

            // Show granted abilities
            AnsiConsole.MarkupLine("[cyan]New Abilities Granted:[/]");
            var newAbilities = player.Abilities.TakeLast(3).ToList(); // Assuming 3 tier 1 abilities
            foreach (var ability in newAbilities)
            {
                AnsiConsole.MarkupLine($"  [green]• {ability.Name}[/] - {ability.Description}");
            }

            // Special message for Bone-Setter
            if (selectedSpec == Specialization.BoneSetter)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[cyan]💊 Field Medicine crafting unlocked![/]");
                AnsiConsole.MarkupLine("[dim]You can craft healing items during rest at Sanctuaries.[/]");
                AnsiConsole.MarkupLine("[dim]Starting supplies granted: 3x Healing Poultice + components[/]");
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
            Console.ReadLine();
        }
        else
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[red]❌ Failed to unlock specialization (insufficient PP or already have one)[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
            Console.ReadLine();
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

            // v0.8: Update quest objectives for collected items
            var questMessages = _gameState.QuestService.OnItemCollected(item.Name, _gameState.Player);
            foreach (var msg in questMessages)
            {
                AnsiConsole.MarkupLine($"[yellow]{msg.EscapeMarkup()}[/]");
            }
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

    /// <summary>
    /// v0.8: Handle quests command - display quest log
    /// </summary>
    static void HandleQuests()
    {
        AnsiConsole.Clear();
        AnsiConsole.WriteLine();

        var rule = new Rule("[bold yellow]QUEST LOG[/]")
        {
            Justification = Justify.Center
        };
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        if (_gameState.Player.ActiveQuests.Count == 0 && _gameState.Player.CompletedQuests.Count == 0)
        {
            AnsiConsole.MarkupLine("[dim]You have no quests.[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
            Console.ReadLine();
            return;
        }

        if (_gameState.Player.ActiveQuests.Count > 0)
        {
            AnsiConsole.MarkupLine("[bold cyan]=== ACTIVE QUESTS ===[/]\n");
            for (int i = 0; i < _gameState.Player.ActiveQuests.Count; i++)
            {
                var quest = _gameState.Player.ActiveQuests[i];
                AnsiConsole.MarkupLine($"[yellow][{i + 1}] {quest.Title.EscapeMarkup()}[/]");
                AnsiConsole.MarkupLine($"[dim]{quest.Description.EscapeMarkup()}[/]");

                foreach (var obj in quest.Objectives)
                {
                    var check = obj.IsComplete ? "[green]✓[/]" : "[yellow]○[/]";
                    AnsiConsole.MarkupLine($"  {check} {obj.Description.EscapeMarkup()}: [cyan]{obj.GetProgress()}[/]");
                }

                if (quest.Reward != null)
                {
                    var rewardParts = new List<string>();
                    if (quest.Reward.Experience > 0)
                        rewardParts.Add($"{quest.Reward.Experience} Legend");
                    if (quest.Reward.ItemIds.Count > 0)
                        rewardParts.Add($"{quest.Reward.ItemIds.Count} items");
                    if (quest.Reward.ReputationChange > 0 && quest.Reward.Faction.HasValue)
                        rewardParts.Add($"+{quest.Reward.ReputationChange} {quest.Reward.Faction}");

                    AnsiConsole.MarkupLine($"[dim]  Reward: {string.Join(", ", rewardParts)}[/]");
                }

                AnsiConsole.WriteLine();
            }
        }

        if (_gameState.Player.CompletedQuests.Count > 0)
        {
            AnsiConsole.MarkupLine("\n[bold green]=== COMPLETED QUESTS ===[/]\n");
            foreach (var quest in _gameState.Player.CompletedQuests)
            {
                AnsiConsole.MarkupLine($"[green]✓ {quest.Title.EscapeMarkup()}[/]");
            }
            AnsiConsole.WriteLine();
        }

        AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
        Console.ReadLine();
    }

    /// <summary>
    /// v0.8: Handle quest details command
    /// </summary>
    static void HandleQuestDetails(string questName)
    {
        if (string.IsNullOrEmpty(questName))
        {
            AnsiConsole.MarkupLine("[red]Specify a quest name or number.[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
            Console.ReadLine();
            return;
        }

        // Try to find quest by index or name
        Quest? quest = null;
        if (int.TryParse(questName, out int index) &&
            index > 0 &&
            index <= _gameState.Player.ActiveQuests.Count)
        {
            quest = _gameState.Player.ActiveQuests[index - 1];
        }
        else
        {
            quest = _gameState.Player.ActiveQuests
                .FirstOrDefault(q => q.Title.Contains(questName, StringComparison.OrdinalIgnoreCase));
        }

        if (quest == null)
        {
            AnsiConsole.MarkupLine($"[red]Quest '{questName.EscapeMarkup()}' not found.[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
            Console.ReadLine();
            return;
        }

        AnsiConsole.Clear();
        AnsiConsole.WriteLine();

        var panel = new Panel($"[bold]{quest.Title.EscapeMarkup()}[/]\n\n{quest.Description.EscapeMarkup()}")
        {
            Border = BoxBorder.Rounded,
            Padding = new Padding(1, 0)
        };
        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[bold cyan]Objectives:[/]");
        foreach (var obj in quest.Objectives)
        {
            var status = obj.IsComplete ? "[green]COMPLETE[/]" : "[yellow]IN PROGRESS[/]";
            AnsiConsole.MarkupLine($"  • {obj.Description.EscapeMarkup()} - {status} ({obj.GetProgress()})");
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
        Console.ReadLine();
    }

    /// <summary>
    /// v0.8: Handle reputation command - display faction standing
    /// </summary>
    static void HandleReputation()
    {
        AnsiConsole.Clear();
        AnsiConsole.WriteLine();

        var rule = new Rule("[bold yellow]FACTION REPUTATION[/]")
        {
            Justification = Justify.Center
        };
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        foreach (FactionType faction in Enum.GetValues(typeof(FactionType)))
        {
            int rep = _gameState.Player.FactionReputations.GetReputation(faction);
            var tier = _gameState.Player.FactionReputations.GetReputationTier(faction);

            // Generate reputation bar
            string repBar = GenerateReputationBar(rep);

            // Color code based on tier
            var tierColor = tier switch
            {
                ReputationTier.Revered or ReputationTier.Honored => "green",
                ReputationTier.Friendly or ReputationTier.Liked => "cyan",
                ReputationTier.Neutral => "yellow",
                ReputationTier.Disliked or ReputationTier.Hated => "red",
                ReputationTier.Despised => "darkred",
                _ => "white"
            };

            AnsiConsole.MarkupLine($"[bold]{faction}:[/] [{tierColor}]{tier}[/] ([dim]{rep:+0;-#}[/])");
            AnsiConsole.MarkupLine($"[dim]{repBar}[/]");
            AnsiConsole.WriteLine();
        }

        AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
        Console.ReadLine();
    }

    /// <summary>
    /// v0.9: Handle shop command - browse merchant inventory
    /// </summary>
    static void HandleShop()
    {
        var merchant = _transactionService.FindMerchantInRoom(_gameState.CurrentRoom);

        if (merchant == null)
        {
            AnsiConsole.MarkupLine("[yellow]There is no merchant here.[/]");
            return;
        }

        AnsiConsole.Clear();
        AnsiConsole.WriteLine();

        var rule = new Rule($"[bold cyan]{merchant.Name}'s Shop[/]")
        {
            Justification = Justify.Center
        };
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        // Show merchant greeting and disposition
        _gameState.NPCService.UpdateDisposition(merchant, _gameState.Player);
        var disposition = merchant.GetDispositionTier();
        var dispColor = disposition switch
        {
            "Friendly" => "green",
            "Neutral-Positive" or "Neutral" => "yellow",
            _ => "red"
        };
        AnsiConsole.MarkupLine($"[dim]{merchant.InitialGreeting}[/]");
        AnsiConsole.MarkupLine($"[dim]Disposition: [{dispColor}]{disposition}[/][/]");
        AnsiConsole.WriteLine();

        // Check and restock if needed
        _merchantService.CheckAndRestock(merchant, DateTime.Now);

        // Display inventory with prices
        var shopListing = _merchantService.GetShopListingWithPrices(merchant, _gameState.Player, _pricingService);

        if (shopListing.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]The merchant has nothing to sell right now.[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[bold]Available Items:[/]");
            AnsiConsole.WriteLine();
            foreach (var item in shopListing)
            {
                AnsiConsole.MarkupLine(item);
            }
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Use 'buy <item name>' to purchase an item[/]");
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[dim]Your currency: [green]{_gameState.Player.Currency} Cogs ⚙[/][/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
        Console.ReadLine();
    }

    /// <summary>
    /// v0.9: Handle buy command - purchase item from merchant
    /// </summary>
    static void HandleBuy(List<string> arguments)
    {
        var merchant = _transactionService.FindMerchantInRoom(_gameState.CurrentRoom);

        if (merchant == null)
        {
            AnsiConsole.MarkupLine("[yellow]There is no merchant here.[/]");
            return;
        }

        if (arguments.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]What do you want to buy? Usage: buy <item name>[/]");
            return;
        }

        // Join arguments to form item name
        string itemName = string.Join(" ", arguments).ToLower().Trim();

        // Find item in merchant inventory (case-insensitive partial match)
        var shopItem = merchant.Inventory.Items.FirstOrDefault(i =>
            i.ItemId.ToLower().Contains(itemName) || itemName.Contains(i.ItemId.ToLower()));

        if (shopItem == null)
        {
            AnsiConsole.MarkupLine($"[yellow]{merchant.Name} doesn't have '{itemName}' in stock.[/]");
            return;
        }

        // Process transaction
        var result = _transactionService.BuyItem(merchant, shopItem, _gameState.Player);

        if (result.Success)
        {
            AnsiConsole.MarkupLine($"[green]✓ {result.Message}[/]");

            // Add purchased item to player inventory based on type
            if (result.ItemType == "Equipment")
            {
                var equipment = EquipmentDatabase.GetByName(result.ItemId);
                if (equipment != null)
                {
                    _gameState.Player.Inventory.Add(equipment);
                }
            }
            else if (result.ItemType == "Component")
            {
                // Parse ComponentType from ItemId
                if (Enum.TryParse<ComponentType>(result.ItemId, out var componentType))
                {
                    if (_gameState.Player.CraftingComponents.ContainsKey(componentType))
                        _gameState.Player.CraftingComponents[componentType] += result.Quantity;
                    else
                        _gameState.Player.CraftingComponents[componentType] = result.Quantity;
                }
            }
            // TODO: Handle Consumable items when implemented
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]✗ {result.Message}[/]");
        }
    }

    /// <summary>
    /// v0.9: Handle sell command - sell item to merchant
    /// </summary>
    static void HandleSell(List<string> arguments)
    {
        var merchant = _transactionService.FindMerchantInRoom(_gameState.CurrentRoom);

        if (merchant == null)
        {
            AnsiConsole.MarkupLine("[yellow]There is no merchant here.[/]");
            return;
        }

        if (arguments.Count == 0)
        {
            // Show sellable items
            AnsiConsole.Clear();
            AnsiConsole.WriteLine();

            var rule = new Rule($"[bold cyan]Sell to {merchant.Name}[/]")
            {
                Justification = Justify.Center
            };
            AnsiConsole.Write(rule);
            AnsiConsole.WriteLine();

            var sellListing = _merchantService.GetSellListingForPlayerItems(merchant, _gameState.Player, _pricingService);

            if (sellListing.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]You have nothing to sell.[/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[bold]Items you can sell:[/]");
                AnsiConsole.WriteLine();
                foreach (var item in sellListing)
                {
                    AnsiConsole.MarkupLine(item);
                }
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[dim]Use 'sell <item name>' to sell an item[/]");
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
            Console.ReadLine();
            return;
        }

        // Join arguments to form item name
        string itemName = string.Join(" ", arguments).ToLower().Trim();

        // Try to find equipment first
        var equipment = _gameState.Player.Inventory.FirstOrDefault(e =>
            e.Name.ToLower().Contains(itemName) || itemName.Contains(e.Name.ToLower()));

        if (equipment != null)
        {
            var result = _transactionService.SellEquipment(merchant, equipment, _gameState.Player);
            if (result.Success)
            {
                AnsiConsole.MarkupLine($"[green]✓ {result.Message}[/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]✗ {result.Message}[/]");
            }
            return;
        }

        // Try to find component
        foreach (var (componentType, quantity) in _gameState.Player.CraftingComponents)
        {
            var component = CraftingComponent.Create(componentType);
            if (component.Name.ToLower().Contains(itemName) || itemName.Contains(component.Name.ToLower()))
            {
                // Ask how many to sell
                int sellQuantity = 1;
                if (quantity > 1)
                {
                    AnsiConsole.MarkupLine($"[dim]You have {quantity} {component.Name}. How many do you want to sell?[/]");
                    var input = AnsiConsole.Ask<int>("[cyan]Quantity:[/]", 1);
                    sellQuantity = Math.Clamp(input, 1, quantity);
                }

                var result = _transactionService.SellComponents(merchant, componentType, sellQuantity, _gameState.Player);
                if (result.Success)
                {
                    AnsiConsole.MarkupLine($"[green]✓ {result.Message}[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]✗ {result.Message}[/]");
                }
                return;
            }
        }

        AnsiConsole.MarkupLine($"[yellow]You don't have '{itemName}' to sell.[/]");
    }

    // v0.13: Persistent World State System - Destruction Handler
    static void HandleDestroy(string target)
    {
        if (string.IsNullOrWhiteSpace(target))
        {
            AnsiConsole.MarkupLine("[yellow]What do you want to destroy? (e.g., 'destroy pillar', 'smash grating')[/]");
            return;
        }

        var currentRoom = _gameState.CurrentRoom;
        if (currentRoom == null)
        {
            AnsiConsole.MarkupLine("[red]You are not in a valid location.[/]");
            return;
        }

        // Get save ID for recording changes
        var saveId = _worldStateRepository.GetSaveIdForCharacter(_gameState.Player.Name);
        if (saveId == null)
        {
            Log.Warning("No save ID found for character: {CharacterName}", _gameState.Player.Name);
            AnsiConsole.MarkupLine("[yellow]Unable to save world changes. Please save your game first.[/]");
            return;
        }

        var turnNumber = _gameState.TurnNumber;

        // Try to find matching static terrain
        var terrain = FindTerrainByName(currentRoom, target);
        if (terrain != null)
        {
            var result = _destructionService.AttemptDestroyTerrain(
                currentRoom,
                terrain,
                _gameState.Player,
                saveId.Value,
                turnNumber);

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[cyan]{result.Message}[/]");
            AnsiConsole.WriteLine();

            if (result.WasDestroyed)
            {
                // Remove from room
                currentRoom.StaticTerrain.Remove(terrain);

                // Add rubble if applicable
                if (result.SpawnRubble)
                {
                    currentRoom.StaticTerrain.Add(new RuneAndRust.Core.Population.RubblePile
                    {
                        TerrainId = $"rubble_from_{terrain.TerrainId}",
                        Description = $"Debris from the destroyed {terrain.Name.ToLower()}.",
                        IsDestructible = true,
                        HP = 15
                    });

                    AnsiConsole.MarkupLine("[dim]A pile of rubble now occupies the space.[/]");
                    AnsiConsole.WriteLine();
                }

                // Increment turn counter
                _gameState.TurnNumber++;
            }

            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
            Console.ReadLine();
            return;
        }

        // Try to find matching dynamic hazard
        var hazard = FindHazardByName(currentRoom, target);
        if (hazard != null)
        {
            var result = _destructionService.AttemptDestroyHazard(
                currentRoom,
                hazard,
                _gameState.Player,
                saveId.Value,
                turnNumber);

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[cyan]{result.Message}[/]");
            AnsiConsole.WriteLine();

            if (result.WasDestroyed)
            {
                // Remove from room
                currentRoom.DynamicHazards.Remove(hazard);

                // Handle secondary effects
                if (result.CausedSecondaryEffect)
                {
                    AnsiConsole.MarkupLine("[red]The destruction causes a violent secondary reaction![/]");

                    // Apply damage if it's an explosive hazard
                    if (hazard.Type == DynamicHazardType.PressurizedPipe)
                    {
                        var damage = _diceService.RollDamage(2);
                        _gameState.Player.HP -= damage;
                        AnsiConsole.MarkupLine($"[red]You take {damage} damage from the explosion![/]");

                        // Check if player died
                        if (_gameState.Player.HP <= 0)
                        {
                            AnsiConsole.WriteLine();
                            AnsiConsole.MarkupLine("[red bold]The explosion was fatal. You have died.[/]");
                            AnsiConsole.WriteLine();
                            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
                            Console.ReadLine();
                            Environment.Exit(0);
                        }
                    }
                    AnsiConsole.WriteLine();
                }

                // Increment turn counter
                _gameState.TurnNumber++;
            }

            AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
            Console.ReadLine();
            return;
        }

        // No matching element found
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[yellow]You cannot find '{target}' to destroy.[/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
        Console.ReadLine();
    }

    // v0.13: Persistent World State System - History Handler
    static void HandleHistory()
    {
        var currentRoom = _gameState.CurrentRoom;
        if (currentRoom == null)
        {
            AnsiConsole.MarkupLine("[red]You are not in a valid location.[/]");
            return;
        }

        var saveId = _worldStateRepository.GetSaveIdForCharacter(_gameState.Player.Name);
        if (saveId == null)
        {
            AnsiConsole.MarkupLine("[yellow]No save history available.[/]");
            return;
        }

        var sectorSeed = ExtractSectorSeed(currentRoom.RoomId);
        var changes = _worldStateRepository.GetChangesForRoom(saveId.Value, sectorSeed, currentRoom.RoomId);

        AnsiConsole.Clear();
        AnsiConsole.WriteLine();

        var rule = new Rule($"[bold cyan]Room History: {currentRoom.Name}[/]")
        {
            Justification = Justify.Center
        };
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        if (changes.Count == 0)
        {
            AnsiConsole.MarkupLine("[dim]This room is unchanged from its original state.[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"[bold]Total modifications:[/] {changes.Count}");
            AnsiConsole.WriteLine();

            foreach (var change in changes.OrderBy(c => c.Timestamp))
            {
                var timeAgo = FormatTimeAgo(change.Timestamp);
                var description = GetChangeDescription(change);
                var icon = GetChangeIcon(change.ChangeType);

                AnsiConsole.MarkupLine($"{icon} [dim]{description}[/] [yellow]({timeAgo})[/]");
            }
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to continue...[/]");
        Console.ReadLine();
    }

    // v0.13: Helper methods for destruction system
    private static RuneAndRust.Core.Population.StaticTerrain? FindTerrainByName(Room room, string target)
    {
        var normalized = target.ToLower().Trim();

        return room.StaticTerrain.FirstOrDefault(t =>
            t.Name.ToLower().Contains(normalized) ||
            t.Type.ToString().ToLower().Contains(normalized) ||
            normalized.Contains(t.Type.ToString().ToLower()));
    }

    private static RuneAndRust.Core.Population.DynamicHazard? FindHazardByName(Room room, string target)
    {
        var normalized = target.ToLower().Trim();

        return room.DynamicHazards.FirstOrDefault(h =>
            h.HazardName.ToLower().Contains(normalized) ||
            h.Type.ToString().ToLower().Contains(normalized) ||
            normalized.Contains(h.Type.ToString().ToLower()));
    }

    private static string ExtractSectorSeed(string roomId)
    {
        // room_d1_n5 -> "d1"
        if (roomId.StartsWith("room_d"))
        {
            var parts = roomId.Split('_');
            return parts.Length >= 2 ? parts[1] : roomId;
        }
        return roomId;
    }

    private static string GetChangeDescription(WorldStateChange change)
    {
        return change.ChangeType switch
        {
            WorldStateChangeType.TerrainDestroyed => $"Destroyed {GetElementName(change.TargetId)}",
            WorldStateChangeType.HazardDestroyed => $"Disabled {GetElementName(change.TargetId)}",
            WorldStateChangeType.EnemyDefeated => $"Defeated {change.TargetId}",
            WorldStateChangeType.LootCollected => $"Collected loot from {GetElementName(change.TargetId)}",
            _ => $"Modified {change.TargetId}"
        };
    }

    private static string GetElementName(string targetId)
    {
        // Convert "pillar_1" to "Pillar"
        var parts = targetId.Split('_');
        if (parts.Length > 0)
        {
            var name = parts[0];
            return char.ToUpper(name[0]) + name.Substring(1);
        }
        return targetId;
    }

    private static string GetChangeIcon(WorldStateChangeType changeType)
    {
        return changeType switch
        {
            WorldStateChangeType.TerrainDestroyed => "[red]💥[/]",
            WorldStateChangeType.HazardDestroyed => "[yellow]⚠️[/]",
            WorldStateChangeType.EnemyDefeated => "[green]⚔️[/]",
            WorldStateChangeType.LootCollected => "[cyan]💎[/]",
            _ => "[dim]•[/]"
        };
    }

    private static string FormatTimeAgo(DateTime timestamp)
    {
        var elapsed = DateTime.UtcNow - timestamp;

        if (elapsed.TotalMinutes < 1)
            return "just now";
        if (elapsed.TotalMinutes < 60)
            return $"{(int)elapsed.TotalMinutes} min ago";
        if (elapsed.TotalHours < 24)
            return $"{(int)elapsed.TotalHours} hr ago";
        return $"{(int)elapsed.TotalDays} days ago";
    }

    /// <summary>
    /// v0.8: Generate a visual reputation bar
    /// </summary>
    static string GenerateReputationBar(int reputation)
    {
        // Map -100 to +100 onto a 21-character bar
        int pos = (reputation + 100) / 10; // 0-20
        pos = Math.Clamp(pos, 0, 20);

        var bar = new char[21];
        for (int i = 0; i < 21; i++)
        {
            if (i == pos)
                bar[i] = '█';
            else if (i == 10)
                bar[i] = '|'; // Neutral marker
            else
                bar[i] = '▁';
        }
        return new string(bar);
    }
}
