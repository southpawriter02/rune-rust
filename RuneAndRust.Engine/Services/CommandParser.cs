using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Represents the result of parsing a command, indicating if async follow-up is needed.
/// Uses boolean flags for deferred execution - the parser signals intent, the caller executes.
/// </summary>
/// <remarks>See: SPEC-INPUT-001, Section "ParseResult Class" for property inventory.</remarks>
public class ParseResult
{
    /// <summary>
    /// Gets or sets whether the command requires async navigation.
    /// </summary>
    public bool RequiresNavigation { get; set; }

    /// <summary>
    /// Gets or sets the direction for navigation commands.
    /// </summary>
    public Direction? NavigationDirection { get; set; }

    /// <summary>
    /// Gets or sets whether the look command was issued.
    /// </summary>
    public bool RequiresLook { get; set; }

    /// <summary>
    /// Gets or sets whether character creation should be triggered.
    /// </summary>
    public bool RequiresCharacterCreation { get; set; }

    /// <summary>
    /// Gets or sets whether an examine command was issued.
    /// </summary>
    public bool RequiresExamine { get; set; }

    /// <summary>
    /// Gets or sets the target of the examine command.
    /// </summary>
    public string? ExamineTarget { get; set; }

    /// <summary>
    /// Gets or sets whether an open command was issued.
    /// </summary>
    public bool RequiresOpen { get; set; }

    /// <summary>
    /// Gets or sets the target of the open command.
    /// </summary>
    public string? OpenTarget { get; set; }

    /// <summary>
    /// Gets or sets whether a close command was issued.
    /// </summary>
    public bool RequiresClose { get; set; }

    /// <summary>
    /// Gets or sets the target of the close command.
    /// </summary>
    public string? CloseTarget { get; set; }

    /// <summary>
    /// Gets or sets whether a search command was issued.
    /// </summary>
    public bool RequiresSearch { get; set; }

    /// <summary>
    /// Gets or sets whether a list objects command was issued.
    /// </summary>
    public bool RequiresListObjects { get; set; }

    /// <summary>
    /// Gets or sets whether the inventory display was requested.
    /// </summary>
    public bool RequiresInventory { get; set; }

    /// <summary>
    /// Gets or sets whether the equipment display was requested.
    /// </summary>
    public bool RequiresEquipment { get; set; }

    /// <summary>
    /// Gets or sets whether an equip command was issued.
    /// </summary>
    public bool RequiresEquip { get; set; }

    /// <summary>
    /// Gets or sets the target of the equip command.
    /// </summary>
    public string? EquipTarget { get; set; }

    /// <summary>
    /// Gets or sets whether an unequip command was issued.
    /// </summary>
    public bool RequiresUnequip { get; set; }

    /// <summary>
    /// Gets or sets the target of the unequip command (item name or slot).
    /// </summary>
    public string? UnequipTarget { get; set; }

    /// <summary>
    /// Gets or sets whether a drop command was issued.
    /// </summary>
    public bool RequiresDrop { get; set; }

    /// <summary>
    /// Gets or sets the target of the drop command.
    /// </summary>
    public string? DropTarget { get; set; }

    /// <summary>
    /// Gets or sets whether a take/get command was issued.
    /// </summary>
    public bool RequiresTake { get; set; }

    /// <summary>
    /// Gets or sets the target of the take command.
    /// </summary>
    public string? TakeTarget { get; set; }

    /// <summary>
    /// Gets or sets whether a loot command was issued (search container for loot).
    /// </summary>
    public bool RequiresLoot { get; set; }

    /// <summary>
    /// Gets or sets the target container for the loot command.
    /// </summary>
    public string? LootTarget { get; set; }

    /// <summary>
    /// Gets or sets whether a use command was issued.
    /// </summary>
    public bool RequiresUse { get; set; }

    /// <summary>
    /// Gets or sets the target of the use command.
    /// </summary>
    public string? UseTarget { get; set; }

    /// <summary>
    /// Gets or sets whether a craft command was issued.
    /// </summary>
    public bool RequiresCraft { get; set; }

    /// <summary>
    /// Gets or sets the target recipe for the craft command.
    /// </summary>
    public string? CraftTarget { get; set; }

    /// <summary>
    /// Gets or sets whether a repair command was issued.
    /// </summary>
    public bool RequiresRepair { get; set; }

    /// <summary>
    /// Gets or sets the target item for the repair command.
    /// </summary>
    public string? RepairTarget { get; set; }

    /// <summary>
    /// Gets or sets whether a salvage command was issued.
    /// </summary>
    public bool RequiresSalvage { get; set; }

    /// <summary>
    /// Gets or sets the target item for the salvage command.
    /// </summary>
    public string? SalvageTarget { get; set; }

    /// <summary>
    /// Gets or sets whether the full inventory screen should be opened (v0.3.7a).
    /// </summary>
    public bool RequiresInventoryScreen { get; set; }

    /// <summary>
    /// Gets or sets whether the full crafting screen should be opened (v0.3.7b).
    /// </summary>
    public bool RequiresCraftingScreen { get; set; }

    /// <summary>
    /// Gets or sets whether the full journal screen should be opened (v0.3.7c).
    /// </summary>
    public bool RequiresJournalScreen { get; set; }

    /// <summary>
    /// Gets or sets whether the options screen should be opened (v0.3.10b).
    /// </summary>
    public bool RequiresOptionsScreen { get; set; }

    #region Travel Commands (v0.3.20c)

    /// <summary>
    /// Gets or sets whether a travel command was issued.
    /// </summary>
    public bool RequiresTravel { get; set; }

    /// <summary>
    /// Gets or sets the destination target for the travel command.
    /// </summary>
    public string? TravelTarget { get; set; }

    #endregion

    #region Alchemy & Runeforging Commands (v0.3.1c)

    /// <summary>
    /// Gets or sets whether a brew command was issued (Alchemy).
    /// </summary>
    public bool RequiresBrew { get; set; }

    /// <summary>
    /// Gets or sets the target recipe for the brew command.
    /// </summary>
    public string? BrewTarget { get; set; }

    /// <summary>
    /// Gets or sets whether a forge command was issued (Runeforging).
    /// </summary>
    public bool RequiresForge { get; set; }

    /// <summary>
    /// Gets or sets the rune name for the forge command.
    /// </summary>
    public string? ForgeRune { get; set; }

    /// <summary>
    /// Gets or sets the target item for the forge command.
    /// </summary>
    public string? ForgeTarget { get; set; }

    #endregion

    /// <summary>
    /// Gets a default result with no async requirements.
    /// </summary>
    public static ParseResult None => new();
}

/// <summary>
/// Parses raw user input and executes commands based on the current game phase.
/// Implements phase-based routing with deferred execution via ParseResult flags.
/// </summary>
/// <remarks>See: SPEC-INPUT-001 for Input Handling System design.</remarks>
public class CommandParser
{
    private readonly ILogger<CommandParser> _logger;
    private readonly IInputHandler _inputHandler;
    private readonly IJournalService? _journalService;
    private readonly ICombatService? _combatService;
    private readonly IVictoryScreenRenderer? _victoryRenderer;
    private readonly IRestService? _restService;
    private readonly IRestScreenRenderer? _restRenderer;
    private readonly IRoomRepository? _roomRepository;
    private readonly IDebugConsoleRenderer? _debugConsoleRenderer;
    private readonly IMapExportService? _mapExportService;
    private readonly IAutoTravelService? _autoTravelService;
    private readonly GameState _gameState;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandParser"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    /// <param name="inputHandler">The input handler for displaying messages.</param>
    /// <param name="gameState">The game state for journal queries.</param>
    /// <param name="journalService">The optional journal service for codex display.</param>
    /// <param name="combatService">The optional combat service for debug combat initiation.</param>
    /// <param name="victoryRenderer">The optional victory screen renderer for combat rewards.</param>
    /// <param name="restService">The optional rest service for rest/camp commands (v0.3.2c).</param>
    /// <param name="restRenderer">The optional rest screen renderer for rest/camp display (v0.3.2c).</param>
    /// <param name="roomRepository">The optional room repository for rest location checks (v0.3.2c).</param>
    /// <param name="debugConsoleRenderer">The optional debug console renderer (v0.3.17a).</param>
    /// <param name="mapExportService">The optional map export service for atlas export (v0.3.20b).</param>
    /// <param name="autoTravelService">The optional auto-travel service for fast travel (v0.3.20c).</param>
    public CommandParser(
        ILogger<CommandParser> logger,
        IInputHandler inputHandler,
        GameState gameState,
        IJournalService? journalService = null,
        ICombatService? combatService = null,
        IVictoryScreenRenderer? victoryRenderer = null,
        IRestService? restService = null,
        IRestScreenRenderer? restRenderer = null,
        IRoomRepository? roomRepository = null,
        IDebugConsoleRenderer? debugConsoleRenderer = null,
        IMapExportService? mapExportService = null,
        IAutoTravelService? autoTravelService = null)
    {
        _logger = logger;
        _inputHandler = inputHandler;
        _gameState = gameState;
        _journalService = journalService;
        _combatService = combatService;
        _victoryRenderer = victoryRenderer;
        _restService = restService;
        _restRenderer = restRenderer;
        _roomRepository = roomRepository;
        _debugConsoleRenderer = debugConsoleRenderer;
        _mapExportService = mapExportService;
        _autoTravelService = autoTravelService;
    }

    /// <summary>
    /// Parses raw input and mutates GameState based on the current phase context.
    /// </summary>
    /// <param name="input">The raw user input string.</param>
    /// <param name="state">The current game state to potentially modify.</param>
    /// <returns>A ParseResult indicating any async operations needed.</returns>
    public async Task<ParseResult> ParseAndExecuteAsync(string input, GameState state)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            _logger.LogTrace("Empty input received, ignoring.");
            return ParseResult.None;
        }

        var command = input.Trim().ToLowerInvariant();
        _logger.LogDebug("Parsing command: '{Command}' in Phase: {Phase}", command, state.Phase);

#if DEBUG
        // v0.3.17a: Debug console toggle (works in any phase)
        // v0.3.24a: Wrapped in #if DEBUG to exclude from Release builds
        if (command == "~" || command == "debug")
        {
            if (_debugConsoleRenderer != null)
            {
                _logger.LogTrace("[DEBUG] Opening debug console");
                _debugConsoleRenderer.Run();
            }
            else
            {
                _inputHandler.DisplayError("Debug console not available.");
            }
            return ParseResult.None;
        }
#endif

        ParseResult result;
        switch (state.Phase)
        {
            case GamePhase.MainMenu:
                result = HandleMainMenu(command, state);
                break;
            case GamePhase.Exploration:
                result = await HandleExplorationAsync(command, input.Trim(), state);
                break;
            case GamePhase.Combat:
                result = HandleCombat(command, state);
                break;
            default:
                _logger.LogWarning("Input received in unhandled phase: {Phase}", state.Phase);
                result = ParseResult.None;
                break;
        }

        return result;
    }

    /// <summary>
    /// Handles commands in the MainMenu phase.
    /// </summary>
    private ParseResult HandleMainMenu(string command, GameState state)
    {
        switch (command)
        {
            case "new":
            case "create":
                _logger.LogInformation("Character creation requested from MainMenu.");
                return new ParseResult { RequiresCharacterCreation = true };

            case "start":
            case "play":
                state.Phase = GamePhase.Exploration;
                state.IsSessionActive = true;
                state.TurnCount = 0;
                _logger.LogInformation("Game Started. Transitioning to Exploration phase.");
                // Note: The actual room description will be shown by the game loop
                // after it initializes the dungeon and sets CurrentRoomId
                return new ParseResult { RequiresLook = true };

            case "load":
                state.PendingAction = PendingGameAction.Load;
                _logger.LogInformation("Load command received from MainMenu.");
                return ParseResult.None;

            case "quit":
            case "exit":
            case "q":
                state.Phase = GamePhase.Quit;
                _logger.LogInformation("Quit command received from MainMenu.");
                return ParseResult.None;

            case "help":
            case "?":
                DisplayMainMenuHelp();
                return ParseResult.None;

            default:
                _inputHandler.DisplayError($"Unknown command: '{command}'. Type 'help' for available commands.");
                _logger.LogDebug("Unknown MainMenu command: {Command}", command);
                return ParseResult.None;
        }
    }

    /// <summary>
    /// Handles commands in the Exploration phase.
    /// </summary>
    /// <param name="command">The lowercased command for matching.</param>
    /// <param name="originalInput">The original input with preserved casing (for note text, etc.).</param>
    /// <param name="state">The game state.</param>
    private async Task<ParseResult> HandleExplorationAsync(string command, string originalInput, GameState state)
    {
        // Check for direction aliases first
        var direction = ParseDirection(command);
        if (direction.HasValue)
        {
            _logger.LogDebug("Movement command: {Direction}", direction.Value);
            return new ParseResult
            {
                RequiresNavigation = true,
                NavigationDirection = direction.Value
            };
        }

        // Check for "go <direction>" format
        if (command.StartsWith("go "))
        {
            var dirString = command.Substring(3).Trim();
            direction = ParseDirection(dirString);
            if (direction.HasValue)
            {
                _logger.LogDebug("Go command: {Direction}", direction.Value);
                return new ParseResult
                {
                    RequiresNavigation = true,
                    NavigationDirection = direction.Value
                };
            }
            else
            {
                _inputHandler.DisplayError($"Unknown direction: '{dirString}'. Valid directions: north, south, east, west, up, down.");
                return ParseResult.None;
            }
        }

        // Check for examine command with target
        if (command.StartsWith("examine ") || command.StartsWith("x ") || command.StartsWith("look "))
        {
            var target = ExtractTarget(command, new[] { "examine ", "x ", "look " });
            if (!string.IsNullOrWhiteSpace(target))
            {
                state.TurnCount++;
                _logger.LogDebug("Examine command for target: {Target}", target);
                return new ParseResult
                {
                    RequiresExamine = true,
                    ExamineTarget = target
                };
            }
        }

        // Check for open command with target
        if (command.StartsWith("open "))
        {
            var target = command.Substring(5).Trim();
            if (!string.IsNullOrWhiteSpace(target))
            {
                state.TurnCount++;
                _logger.LogDebug("Open command for target: {Target}", target);
                return new ParseResult
                {
                    RequiresOpen = true,
                    OpenTarget = target
                };
            }
            else
            {
                _inputHandler.DisplayError("Open what? Specify a target.");
                return ParseResult.None;
            }
        }

        // Check for close command with target
        if (command.StartsWith("close "))
        {
            var target = command.Substring(6).Trim();
            if (!string.IsNullOrWhiteSpace(target))
            {
                state.TurnCount++;
                _logger.LogDebug("Close command for target: {Target}", target);
                return new ParseResult
                {
                    RequiresClose = true,
                    CloseTarget = target
                };
            }
            else
            {
                _inputHandler.DisplayError("Close what? Specify a target.");
                return ParseResult.None;
            }
        }

        // Check for equip command with target
        if (command.StartsWith("equip ") || command.StartsWith("bind "))
        {
            var target = ExtractTarget(command, new[] { "equip ", "bind " });
            if (!string.IsNullOrWhiteSpace(target))
            {
                _logger.LogDebug("Equip command for target: {Target}", target);
                return new ParseResult
                {
                    RequiresEquip = true,
                    EquipTarget = target
                };
            }
            else
            {
                _inputHandler.DisplayError("Equip what? Specify an item.");
                return ParseResult.None;
            }
        }

        // Check for unequip command with target
        if (command.StartsWith("unequip ") || command.StartsWith("unbind ") || command.StartsWith("remove "))
        {
            var target = ExtractTarget(command, new[] { "unequip ", "unbind ", "remove " });
            if (!string.IsNullOrWhiteSpace(target))
            {
                _logger.LogDebug("Unequip command for target: {Target}", target);
                return new ParseResult
                {
                    RequiresUnequip = true,
                    UnequipTarget = target
                };
            }
            else
            {
                _inputHandler.DisplayError("Unequip what? Specify an item or slot.");
                return ParseResult.None;
            }
        }

        // Check for drop command with target
        if (command.StartsWith("drop ") || command.StartsWith("discard "))
        {
            var target = ExtractTarget(command, new[] { "drop ", "discard " });
            if (!string.IsNullOrWhiteSpace(target))
            {
                _logger.LogDebug("Drop command for target: {Target}", target);
                return new ParseResult
                {
                    RequiresDrop = true,
                    DropTarget = target
                };
            }
            else
            {
                _inputHandler.DisplayError("Drop what? Specify an item.");
                return ParseResult.None;
            }
        }

        // Check for take/get command with target
        if (command.StartsWith("take ") || command.StartsWith("get ") || command.StartsWith("grab "))
        {
            var target = ExtractTarget(command, new[] { "take ", "get ", "grab " });
            if (!string.IsNullOrWhiteSpace(target))
            {
                state.TurnCount++;
                _logger.LogDebug("Take command for target: {Target}", target);
                return new ParseResult
                {
                    RequiresTake = true,
                    TakeTarget = target
                };
            }
            else
            {
                _inputHandler.DisplayError("Take what? Specify an item.");
                return ParseResult.None;
            }
        }

        // Check for loot command with target
        if (command.StartsWith("loot ") || command.StartsWith("search "))
        {
            var target = ExtractTarget(command, new[] { "loot ", "search " });
            if (!string.IsNullOrWhiteSpace(target))
            {
                state.TurnCount++;
                _logger.LogDebug("Loot command for target: {Target}", target);
                return new ParseResult
                {
                    RequiresLoot = true,
                    LootTarget = target
                };
            }
            // If no target for search, fall through to room search
        }

        // Check for use command with target
        if (command.StartsWith("use ") || command.StartsWith("consume ") || command.StartsWith("apply "))
        {
            var target = ExtractTarget(command, new[] { "use ", "consume ", "apply " });
            if (!string.IsNullOrWhiteSpace(target))
            {
                state.TurnCount++;
                _logger.LogDebug("Use command for target: {Target}", target);
                return new ParseResult
                {
                    RequiresUse = true,
                    UseTarget = target
                };
            }
            else
            {
                _inputHandler.DisplayError("Use what? Specify an item.");
                return ParseResult.None;
            }
        }

        // Check for craft command with target
        if (command.StartsWith("craft ") || command.StartsWith("make ") || command.StartsWith("create "))
        {
            var target = ExtractTarget(command, new[] { "craft ", "make ", "create " });
            if (!string.IsNullOrWhiteSpace(target))
            {
                state.TurnCount++;
                _logger.LogDebug("Craft command for target: {Target}", target);
                return new ParseResult
                {
                    RequiresCraft = true,
                    CraftTarget = target
                };
            }
            else
            {
                _inputHandler.DisplayError("Craft what? Specify a recipe name or ID.");
                return ParseResult.None;
            }
        }

        // Check for repair command with target
        if (command.StartsWith("repair ") || command.StartsWith("fix ") || command.StartsWith("mend "))
        {
            var target = ExtractTarget(command, new[] { "repair ", "fix ", "mend " });
            if (!string.IsNullOrWhiteSpace(target))
            {
                state.TurnCount++;
                _logger.LogDebug("Repair command for target: {Target}", target);
                return new ParseResult
                {
                    RequiresRepair = true,
                    RepairTarget = target
                };
            }
            else
            {
                _inputHandler.DisplayError("Repair what? Specify an equipment item.");
                return ParseResult.None;
            }
        }

        // Check for salvage command with target
        if (command.StartsWith("salvage ") || command.StartsWith("scrap ") || command.StartsWith("dismantle "))
        {
            var target = ExtractTarget(command, new[] { "salvage ", "scrap ", "dismantle " });
            if (!string.IsNullOrWhiteSpace(target))
            {
                state.TurnCount++;
                _logger.LogDebug("Salvage command for target: {Target}", target);
                return new ParseResult
                {
                    RequiresSalvage = true,
                    SalvageTarget = target
                };
            }
            else
            {
                _inputHandler.DisplayError("Salvage what? Specify an equipment item.");
                return ParseResult.None;
            }
        }

        // Check for brew command with target (Alchemy - v0.3.1c)
        if (command.StartsWith("brew ") || command.StartsWith("mix ") || command.StartsWith("concoct "))
        {
            var target = ExtractTarget(command, new[] { "brew ", "mix ", "concoct " });
            if (!string.IsNullOrWhiteSpace(target))
            {
                state.TurnCount++;
                _logger.LogDebug("Brew command for target: {Target}", target);
                return new ParseResult
                {
                    RequiresBrew = true,
                    BrewTarget = target
                };
            }
            else
            {
                _inputHandler.DisplayError("Brew what? Specify a potion recipe.");
                return ParseResult.None;
            }
        }

        // Check for forge command with target (Runeforging - v0.3.1c)
        // Format: "forge <rune> on <item>" or "inscribe <rune> on <item>" or "etch <rune> on <item>"
        if (command.StartsWith("forge ") || command.StartsWith("inscribe ") || command.StartsWith("etch "))
        {
            var result = ParseForgeCommand(command);
            if (result != null)
            {
                state.TurnCount++;
                return result;
            }
            else
            {
                _inputHandler.DisplayError("Forge what? Format: forge <rune> on <item>");
                return ParseResult.None;
            }
        }

        // Check for note command (v0.3.20a)
        // Note: Pass original input to preserve note text casing
        if (command == "note" || command.StartsWith("note "))
        {
            return HandleNoteCommand(originalInput, state);
        }

        // Check for map export command (v0.3.20b)
        if (command == "map export" || command == "atlas" || command == "export map")
        {
            return await HandleMapExportAsync(state);
        }

        // Check for travel/goto command (v0.3.20c)
        if (command.StartsWith("travel ") || command.StartsWith("goto "))
        {
            var target = ExtractTarget(command, new[] { "travel ", "goto " });
            if (!string.IsNullOrWhiteSpace(target))
            {
                return await HandleTravelCommandAsync(target, state);
            }
            else
            {
                _inputHandler.DisplayError("Travel where? Specify a destination (room name, 'home', 'anchor', or 'surface').");
                return ParseResult.None;
            }
        }

        // Check for codex command with target
        if (command.StartsWith("codex "))
        {
            var entryName = command.Substring(6).Trim();
            if (!string.IsNullOrWhiteSpace(entryName))
            {
                if (_journalService != null && _gameState.CurrentCharacter != null)
                {
                    _logger.LogDebug("Handling codex command for entry '{EntryTitle}'", entryName);
                    var codexOutput = await _journalService.FormatEntryDetailAsync(_gameState.CurrentCharacter.Id, entryName);
                    _inputHandler.DisplayMessage(codexOutput);
                }
                else
                {
                    _inputHandler.DisplayMessage("You must have an active character to view the codex.");
                }
                return ParseResult.None;
            }
            else
            {
                _inputHandler.DisplayMessage("[grey]Usage: codex <entry name>[/]");
                return ParseResult.None;
            }
        }

#if DEBUG
        // v0.3.24a: Debug combat command wrapped in #if DEBUG
        if (command == "debug-combat")
        {
            if (_combatService != null && _gameState.CurrentCharacter != null)
            {
                _logger.LogDebug("DEBUG: Initiating test combat encounter");
                var dummyEnemy = new Enemy
                {
                    Name = "Training Dummy",
                    MaxHp = 30,
                    CurrentHp = 30
                };
                _combatService.StartCombat(new List<Enemy> { dummyEnemy });
                _inputHandler.DisplayMessage("[yellow]DEBUG: Combat initiated with Training Dummy.[/]");
            }
            else
            {
                _inputHandler.DisplayMessage("[red]Cannot start combat: No active character or combat service unavailable.[/]");
            }
            return ParseResult.None;
        }
#endif

        switch (command)
        {
            case "quit":
            case "exit":
            case "q":
                state.Phase = GamePhase.Quit;
                _logger.LogInformation("Player quit from Exploration phase.");
                return ParseResult.None;

            case "menu":
            case "mainmenu":
                state.Phase = GamePhase.MainMenu;
                state.IsSessionActive = false;
                state.CurrentRoomId = null;
                _logger.LogInformation("Returning to MainMenu from Exploration.");
                _inputHandler.DisplayMessage("Returning to main menu...");
                return ParseResult.None;

            case "help":
            case "?":
                DisplayExplorationHelp();
                return ParseResult.None;

            case "look":
            case "l":
                state.TurnCount++;
                _logger.LogTrace("Look command executed. Turn: {TurnCount}", state.TurnCount);
                return new ParseResult { RequiresLook = true };

            case "exits":
                _logger.LogTrace("Exits command executed.");
                return new ParseResult { RequiresLook = true };

            case "status":
            case "stats":
                DisplayStatus(state);
                return ParseResult.None;

            case "save":
                state.PendingAction = PendingGameAction.Save;
                _logger.LogInformation("Save command received.");
                return ParseResult.None;

            case "load":
                state.PendingAction = PendingGameAction.Load;
                _logger.LogInformation("Load command received.");
                return ParseResult.None;

            case "search":
                state.TurnCount++;
                _logger.LogDebug("Search command executed.");
                return new ParseResult { RequiresSearch = true };

            case "objects":
            case "items":
                _logger.LogDebug("List objects command executed.");
                return new ParseResult { RequiresListObjects = true };

            case "inventory":
            case "i":
                _logger.LogDebug("Inventory text display command executed.");
                return new ParseResult { RequiresInventory = true };

            case "pack":
            case "p":
                _logger.LogDebug("Full inventory screen requested (v0.3.7a).");
                return new ParseResult { RequiresInventoryScreen = true };

            case "bench":
            case "workbench":
            case "craft-menu":
                _logger.LogDebug("Full crafting screen requested (v0.3.7b).");
                return new ParseResult { RequiresCraftingScreen = true };

            case "archive":
                _logger.LogDebug("Full journal screen requested (v0.3.7c).");
                return new ParseResult { RequiresJournalScreen = true };

            case "options":
            case "settings":
            case "config":
            case "o":
                _logger.LogDebug("Options screen requested (v0.3.10b).");
                return new ParseResult { RequiresOptionsScreen = true };

            case "equipment":
            case "gear":
            case "equipped":
                _logger.LogDebug("Equipment display command executed.");
                return new ParseResult { RequiresEquipment = true };

            case "journal":
            case "j":
                if (_journalService != null && _gameState.CurrentCharacter != null)
                {
                    _logger.LogDebug("Handling journal command for Character {CharacterId}", _gameState.CurrentCharacter.Id);
                    var journalOutput = await _journalService.FormatJournalListAsync(_gameState.CurrentCharacter.Id);
                    _inputHandler.DisplayMessage(journalOutput);
                }
                else
                {
                    _inputHandler.DisplayMessage("You must have an active character to view the journal.");
                }
                return ParseResult.None;

            case "fragments":
                if (_journalService != null && _gameState.CurrentCharacter != null)
                {
                    _logger.LogDebug("Handling fragments command for Character {CharacterId}", _gameState.CurrentCharacter.Id);
                    var fragmentsOutput = await _journalService.FormatUnassignedCapturesAsync(_gameState.CurrentCharacter.Id);
                    _inputHandler.DisplayMessage(fragmentsOutput);
                }
                else
                {
                    _inputHandler.DisplayMessage("You must have an active character to view fragments.");
                }
                return ParseResult.None;

            case "codex":
                _inputHandler.DisplayMessage("[grey]Usage: codex <entry name>[/]");
                return ParseResult.None;

            // REST & CAMP COMMANDS (v0.3.2c)
            case "rest":
                return await HandleRestCommandAsync(state, preferSanctuary: true);

            case "camp":
            case "sleep":
                return await HandleRestCommandAsync(state, preferSanctuary: false);

            default:
                _inputHandler.DisplayError($"Unknown command: '{command}'. Type 'help' for available commands.");
                _logger.LogDebug("Unknown Exploration command: {Command}", command);
                return ParseResult.None;
        }
    }

    /// <summary>
    /// Parses a direction string or alias into a Direction enum.
    /// </summary>
    /// <param name="input">The direction string to parse.</param>
    /// <returns>The parsed direction, or null if invalid.</returns>
    private Direction? ParseDirection(string input)
    {
        return input switch
        {
            "north" or "n" => Direction.North,
            "south" or "s" => Direction.South,
            "east" or "e" => Direction.East,
            "west" or "w" => Direction.West,
            "up" or "u" => Direction.Up,
            "down" or "d" => Direction.Down,
            _ => null
        };
    }

    /// <summary>
    /// Extracts the target from a command that starts with one of the given prefixes.
    /// </summary>
    /// <param name="command">The full command string.</param>
    /// <param name="prefixes">The possible command prefixes.</param>
    /// <returns>The target string, or null if not found.</returns>
    private static string? ExtractTarget(string command, string[] prefixes)
    {
        foreach (var prefix in prefixes)
        {
            if (command.StartsWith(prefix))
            {
                return command.Substring(prefix.Length).Trim();
            }
        }
        return null;
    }

    /// <summary>
    /// Parses a forge command in the format "forge <rune> on <item>".
    /// </summary>
    /// <param name="command">The full command string.</param>
    /// <returns>A ParseResult with forge properties, or null if invalid format.</returns>
    private ParseResult? ParseForgeCommand(string command)
    {
        // Remove the verb prefix
        var content = ExtractTarget(command, new[] { "forge ", "inscribe ", "etch " });
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        // Look for "on" separator (case insensitive)
        var onIndex = content.IndexOf(" on ", StringComparison.OrdinalIgnoreCase);
        if (onIndex < 0)
        {
            // No target item specified - this is valid for some runeforging recipes
            // that create new items rather than enchanting existing ones
            _logger.LogDebug("Forge command (no target): {Rune}", content);
            return new ParseResult
            {
                RequiresForge = true,
                ForgeRune = content.Trim(),
                ForgeTarget = null
            };
        }

        var runeName = content.Substring(0, onIndex).Trim();
        var targetItem = content.Substring(onIndex + 4).Trim();

        if (string.IsNullOrWhiteSpace(runeName))
        {
            return null;
        }

        _logger.LogDebug("Forge command: {Rune} on {Target}", runeName, targetItem);
        return new ParseResult
        {
            RequiresForge = true,
            ForgeRune = runeName,
            ForgeTarget = string.IsNullOrWhiteSpace(targetItem) ? null : targetItem
        };
    }

    /// <summary>
    /// Handles commands in the Combat phase.
    /// </summary>
    private ParseResult HandleCombat(string command, GameState state)
    {
        // Check for attack commands first (they have arguments)
        var attackTarget = ExtractTarget(command, new[] { "attack ", "hit ", "strike " });
        if (attackTarget != null)
        {
            return ExecuteCombatAttack(attackTarget, AttackType.Standard);
        }

        var lightTarget = ExtractTarget(command, new[] { "light ", "quick ", "fast " });
        if (lightTarget != null)
        {
            return ExecuteCombatAttack(lightTarget, AttackType.Light);
        }

        var heavyTarget = ExtractTarget(command, new[] { "heavy ", "power ", "strong " });
        if (heavyTarget != null)
        {
            return ExecuteCombatAttack(heavyTarget, AttackType.Heavy);
        }

        // Check for ability use commands (v0.2.3c)
        if (command.StartsWith("use "))
        {
            return ExecuteAbilityCommand(command.Substring(4).Trim());
        }

        // Check for numbered hotkey shortcuts (1, 2, 3)
        if (int.TryParse(command, out var hotkey) && hotkey >= 1 && hotkey <= 9)
        {
            return ExecuteAbilityByHotkey(hotkey);
        }

        switch (command)
        {
            case "quit":
            case "exit":
            case "q":
                state.Phase = GamePhase.Quit;
                _logger.LogInformation("Player quit from Combat phase.");
                return ParseResult.None;

            case "flee":
            case "run":
                _combatService?.EndCombat();
                _logger.LogInformation("Player fled combat. Returning to Exploration.");
                _inputHandler.DisplayMessage("You flee from the encounter!");
                return new ParseResult { RequiresLook = true };

            case "status":
            case "stats":
                DisplayCombatStatus();
                return ParseResult.None;

            case "end":
            case "pass":
            case "wait":
                _combatService?.NextTurn();
                _inputHandler.DisplayMessage("You end your turn.");
                DisplayCombatTurnInfo();
                return ParseResult.None;

            case "help":
            case "?":
                DisplayCombatHelp();
                return ParseResult.None;

            default:
                _inputHandler.DisplayError($"Unknown combat command: '{command}'. Type 'help' for available commands.");
                _logger.LogDebug("Unknown Combat command: {Command}", command);
                return ParseResult.None;
        }
    }

    /// <summary>
    /// Executes a combat attack and displays the result.
    /// </summary>
    private ParseResult ExecuteCombatAttack(string targetName, AttackType attackType)
    {
        if (_combatService == null)
        {
            _inputHandler.DisplayError("Combat system not available.");
            return ParseResult.None;
        }

        var result = _combatService.ExecutePlayerAttack(targetName, attackType);
        _inputHandler.DisplayMessage(result);

        // Check if combat ended (victory)
        if (_gameState.CombatState == null || _combatService.CheckVictoryCondition())
        {
            var combatResult = _combatService.EndCombat();

            // Display victory screen with loot/XP if victory and renderer available
            if (combatResult != null && combatResult.Victory && _victoryRenderer != null)
            {
                _victoryRenderer.Render(combatResult);
            }
            else
            {
                _inputHandler.DisplayMessage("");
                _inputHandler.DisplayMessage("Combat has ended. Returning to exploration.");
            }

            return new ParseResult { RequiresLook = true };
        }

        // Advance to next turn after attack
        _combatService.NextTurn();
        DisplayCombatTurnInfo();

        return ParseResult.None;
    }

    /// <summary>
    /// Executes an ability command, parsing the ability name and optional target.
    /// Format: "use <ability> [on <target>]" or "use <hotkey> [on <target>]"
    /// </summary>
    /// <param name="args">The arguments after "use ".</param>
    /// <returns>A ParseResult for the command.</returns>
    private ParseResult ExecuteAbilityCommand(string args)
    {
        if (_combatService == null)
        {
            _inputHandler.DisplayError("Combat system not available.");
            return ParseResult.None;
        }

        if (string.IsNullOrWhiteSpace(args))
        {
            _inputHandler.DisplayError("Use what? Specify an ability name or number.");
            return ParseResult.None;
        }

        // Check for target specifier (use <ability> on <target>)
        string? targetName = null;
        var abilityArg = args;

        var onIndex = args.IndexOf(" on ", StringComparison.OrdinalIgnoreCase);
        if (onIndex >= 0)
        {
            abilityArg = args.Substring(0, onIndex).Trim();
            targetName = args.Substring(onIndex + 4).Trim();
        }
        else
        {
            var atIndex = args.IndexOf(" at ", StringComparison.OrdinalIgnoreCase);
            if (atIndex >= 0)
            {
                abilityArg = args.Substring(0, atIndex).Trim();
                targetName = args.Substring(atIndex + 4).Trim();
            }
        }

        string result;

        // Check if ability argument is a number (hotkey)
        if (int.TryParse(abilityArg, out var hotkey))
        {
            result = _combatService.ExecutePlayerAbility(hotkey, targetName);
        }
        else
        {
            result = _combatService.ExecutePlayerAbility(abilityArg, targetName);
        }

        _inputHandler.DisplayMessage(result);

        // Check if combat ended (victory)
        if (_gameState.CombatState == null || _combatService.CheckVictoryCondition())
        {
            var combatResult = _combatService.EndCombat();

            if (combatResult != null && combatResult.Victory && _victoryRenderer != null)
            {
                _victoryRenderer.Render(combatResult);
            }
            else
            {
                _inputHandler.DisplayMessage("");
                _inputHandler.DisplayMessage("Combat has ended. Returning to exploration.");
            }

            return new ParseResult { RequiresLook = true };
        }

        // Advance to next turn after ability
        _combatService.NextTurn();
        DisplayCombatTurnInfo();

        return ParseResult.None;
    }

    /// <summary>
    /// Executes an ability by its hotkey number.
    /// </summary>
    /// <param name="hotkey">The 1-based hotkey index.</param>
    /// <returns>A ParseResult for the command.</returns>
    private ParseResult ExecuteAbilityByHotkey(int hotkey)
    {
        if (_combatService == null)
        {
            _inputHandler.DisplayError("Combat system not available.");
            return ParseResult.None;
        }

        var result = _combatService.ExecutePlayerAbility(hotkey);
        _inputHandler.DisplayMessage(result);

        // Check if combat ended (victory)
        if (_gameState.CombatState == null || _combatService.CheckVictoryCondition())
        {
            var combatResult = _combatService.EndCombat();

            if (combatResult != null && combatResult.Victory && _victoryRenderer != null)
            {
                _victoryRenderer.Render(combatResult);
            }
            else
            {
                _inputHandler.DisplayMessage("");
                _inputHandler.DisplayMessage("Combat has ended. Returning to exploration.");
            }

            return new ParseResult { RequiresLook = true };
        }

        // Advance to next turn after ability
        _combatService.NextTurn();
        DisplayCombatTurnInfo();

        return ParseResult.None;
    }

    /// <summary>
    /// Displays the current combat status.
    /// </summary>
    private void DisplayCombatStatus()
    {
        if (_combatService == null)
        {
            _inputHandler.DisplayError("Combat system not available.");
            return;
        }

        var status = _combatService.GetCombatStatus();
        _inputHandler.DisplayMessage(status);
    }

    /// <summary>
    /// Displays information about whose turn it is.
    /// </summary>
    private void DisplayCombatTurnInfo()
    {
        var combatState = _gameState.CombatState;
        if (combatState?.ActiveCombatant == null) return;

        var active = combatState.ActiveCombatant;
        if (active.IsPlayer)
        {
            _inputHandler.DisplayMessage($"");
            _inputHandler.DisplayMessage($">> Your turn. (Stamina: {active.CurrentStamina}/{active.MaxStamina})");
        }
        else
        {
            // For now, enemies just pass (AI will be added in v0.2.1)
            _inputHandler.DisplayMessage($"");
            _inputHandler.DisplayMessage($"{active.Name} prepares to act...");
            // TODO: Implement enemy AI in v0.2.1
            _combatService?.NextTurn();
            DisplayCombatTurnInfo(); // Recursively advance until player's turn
        }
    }

    /// <summary>
    /// Displays help for the MainMenu phase.
    /// </summary>
    private void DisplayMainMenuHelp()
    {
        _inputHandler.DisplayMessage("=== MAIN MENU ===");
        _inputHandler.DisplayMessage("  new, create      - Create a new character");
        _inputHandler.DisplayMessage("  start, play      - Start the game");
        _inputHandler.DisplayMessage("  load             - Load a saved game");
        _inputHandler.DisplayMessage("  help, ?          - Show this help");
        _inputHandler.DisplayMessage("  quit, exit, q    - Exit the game");
    }

    /// <summary>
    /// Displays help for the Exploration phase.
    /// </summary>
    private void DisplayExplorationHelp()
    {
        _inputHandler.DisplayMessage("=== EXPLORATION ===");
        _inputHandler.DisplayMessage("Movement:");
        _inputHandler.DisplayMessage("  n, s, e, w       - Move north, south, east, or west");
        _inputHandler.DisplayMessage("  u, d             - Move up or down");
        _inputHandler.DisplayMessage("  go <direction>   - Move in a direction");
        _inputHandler.DisplayMessage("");
        _inputHandler.DisplayMessage("Interaction:");
        _inputHandler.DisplayMessage("  examine <target> - Examine an object (uses WITS)");
        _inputHandler.DisplayMessage("  x <target>       - Shorthand for examine");
        _inputHandler.DisplayMessage("  look <target>    - Examine an object");
        _inputHandler.DisplayMessage("  search           - Search the room for objects (uses WITS)");
        _inputHandler.DisplayMessage("  open <target>    - Open a container");
        _inputHandler.DisplayMessage("  close <target>   - Close a container");
        _inputHandler.DisplayMessage("  objects          - List visible objects in the room");
        _inputHandler.DisplayMessage("");
        _inputHandler.DisplayMessage("Inventory:");
        _inputHandler.DisplayMessage("  inventory, i     - Display your inventory and burden");
        _inputHandler.DisplayMessage("  equipment, gear  - Display equipped items");
        _inputHandler.DisplayMessage("  equip <item>     - Equip an item from inventory");
        _inputHandler.DisplayMessage("  unequip <slot>   - Unequip item from slot (e.g., mainhand)");
        _inputHandler.DisplayMessage("  drop <item>      - Drop an item from inventory");
        _inputHandler.DisplayMessage("");
        _inputHandler.DisplayMessage("Loot:");
        _inputHandler.DisplayMessage("  loot <container> - Search an open container for items");
        _inputHandler.DisplayMessage("  take <item>      - Take an item from a container");
        _inputHandler.DisplayMessage("  get <item>       - Take an item from a container");
        _inputHandler.DisplayMessage("  use <item>       - Use a consumable item");
        _inputHandler.DisplayMessage("");
        _inputHandler.DisplayMessage("Crafting:");
        _inputHandler.DisplayMessage("  craft <recipe>   - Craft an item using a recipe (uses WITS)");
        _inputHandler.DisplayMessage("  make <recipe>    - Same as craft");
        _inputHandler.DisplayMessage("");
        _inputHandler.DisplayMessage("Bodging:");
        _inputHandler.DisplayMessage("  repair <item>    - Repair damaged equipment using Scrap (uses WITS)");
        _inputHandler.DisplayMessage("  fix <item>       - Same as repair");
        _inputHandler.DisplayMessage("  salvage <item>   - Destroy equipment to extract Scrap");
        _inputHandler.DisplayMessage("  scrap <item>     - Same as salvage");
        _inputHandler.DisplayMessage("");
        _inputHandler.DisplayMessage("Alchemy:");
        _inputHandler.DisplayMessage("  brew <recipe>    - Brew a potion or compound (uses WITS)");
        _inputHandler.DisplayMessage("  mix <recipe>     - Same as brew");
        _inputHandler.DisplayMessage("");
        _inputHandler.DisplayMessage("Runeforging:");
        _inputHandler.DisplayMessage("  forge <rune>     - Forge a runic item (uses WITS)");
        _inputHandler.DisplayMessage("  forge X on Y     - Inscribe rune X onto item Y");
        _inputHandler.DisplayMessage("  inscribe X on Y  - Same as forge");
        _inputHandler.DisplayMessage("");
        _inputHandler.DisplayMessage("Rest & Recovery:");
        _inputHandler.DisplayMessage("  rest             - Rest (Sanctuary at anchors, Wilderness elsewhere)");
        _inputHandler.DisplayMessage("  camp             - Set up camp in the wilderness (ambush risk)");
        _inputHandler.DisplayMessage("  sleep            - Same as camp");
        _inputHandler.DisplayMessage("");
        _inputHandler.DisplayMessage("Journal:");
        _inputHandler.DisplayMessage("  journal, j       - Open the Scavenger's Journal");
        _inputHandler.DisplayMessage("  codex <name>     - View a specific journal entry");
        _inputHandler.DisplayMessage("  fragments        - View unassigned knowledge fragments");
        _inputHandler.DisplayMessage("");
        _inputHandler.DisplayMessage("Notes:");
        _inputHandler.DisplayMessage("  note             - View note for current room");
        _inputHandler.DisplayMessage("  note <text>      - Set note for current room (appears as ! on map)");
        _inputHandler.DisplayMessage("  note clear       - Remove note from current room");
        _inputHandler.DisplayMessage("");
        _inputHandler.DisplayMessage("Fast Travel:");
        _inputHandler.DisplayMessage("  travel <name>    - Auto-walk to a visited room by name");
        _inputHandler.DisplayMessage("  travel home      - Auto-walk to nearest Runic Anchor");
        _inputHandler.DisplayMessage("  travel anchor    - Same as travel home");
        _inputHandler.DisplayMessage("  travel surface   - Auto-walk to nearest surface (Z=0) room");
        _inputHandler.DisplayMessage("  goto <name>      - Same as travel");
        _inputHandler.DisplayMessage("");
        _inputHandler.DisplayMessage("Map:");
        _inputHandler.DisplayMessage("  atlas            - Export explored map as ASCII text file");
        _inputHandler.DisplayMessage("  map export       - Same as atlas");
        _inputHandler.DisplayMessage("");
        _inputHandler.DisplayMessage("Actions:");
        _inputHandler.DisplayMessage("  look, l          - Examine your surroundings");
        _inputHandler.DisplayMessage("  exits            - Show available exits");
        _inputHandler.DisplayMessage("  status, stats    - View your current status");
        _inputHandler.DisplayMessage("  save             - Save your progress");
        _inputHandler.DisplayMessage("  load             - Load a saved game");
        _inputHandler.DisplayMessage("  menu, mainmenu   - Return to main menu");
        _inputHandler.DisplayMessage("  help, ?          - Show this help");
        _inputHandler.DisplayMessage("  quit, exit, q    - Exit the game");
#if DEBUG
        _inputHandler.DisplayMessage("");
        _inputHandler.DisplayMessage("Debug:");
        _inputHandler.DisplayMessage("  debug-combat     - Start a test combat encounter");
#endif
    }

    /// <summary>
    /// Displays help for the Combat phase.
    /// </summary>
    private void DisplayCombatHelp()
    {
        _inputHandler.DisplayMessage("=== COMBAT ===");
        _inputHandler.DisplayMessage("Attacks:");
        _inputHandler.DisplayMessage("  attack <target>  - Standard attack (25 stamina, d6 damage)");
        _inputHandler.DisplayMessage("  hit <target>     - Same as attack");
        _inputHandler.DisplayMessage("  light <target>   - Quick attack (15 stamina, d4 damage, +1 hit)");
        _inputHandler.DisplayMessage("  quick <target>   - Same as light");
        _inputHandler.DisplayMessage("  heavy <target>   - Power attack (40 stamina, d8 damage, -1 hit)");
        _inputHandler.DisplayMessage("  power <target>   - Same as heavy");
        _inputHandler.DisplayMessage("");
        _inputHandler.DisplayMessage("Abilities:");
        _inputHandler.DisplayMessage("  use <name>       - Use an ability by name");
        _inputHandler.DisplayMessage("  use 1, 2, 3...   - Use an ability by hotkey number");
        _inputHandler.DisplayMessage("  1, 2, 3...       - Shortcut for use <number>");
        _inputHandler.DisplayMessage("  use <name> on X  - Use ability targeting a specific enemy");
        _inputHandler.DisplayMessage("");
        _inputHandler.DisplayMessage("Actions:");
        _inputHandler.DisplayMessage("  status           - View HP/Stamina for all combatants");
        _inputHandler.DisplayMessage("  end, pass        - End your turn without attacking");
        _inputHandler.DisplayMessage("  flee, run        - Attempt to flee combat");
        _inputHandler.DisplayMessage("  help, ?          - Show this help");
        _inputHandler.DisplayMessage("  quit, exit, q    - Exit the game");
    }

    /// <summary>
    /// Displays the current game status.
    /// </summary>
    private void DisplayStatus(GameState state)
    {
        _inputHandler.DisplayMessage($"=== STATUS ===");
        _inputHandler.DisplayMessage($"  Phase: {state.Phase}");
        _inputHandler.DisplayMessage($"  Turn: {state.TurnCount}");
        _inputHandler.DisplayMessage($"  Session Active: {state.IsSessionActive}");

        if (state.CurrentCharacter != null)
        {
            _inputHandler.DisplayMessage($"  Character: {state.CurrentCharacter.Name}");
        }
        else
        {
            _inputHandler.DisplayMessage("  Character: None");
        }
    }

    #region Rest & Camp Commands (v0.3.2c)

    /// <summary>
    /// Handles the rest and camp commands.
    /// Rest prefers Sanctuary at RunicAnchor locations, camp always uses Wilderness.
    /// </summary>
    /// <param name="state">The current game state.</param>
    /// <param name="preferSanctuary">If true, use Sanctuary when at a RunicAnchor; otherwise always Wilderness.</param>
    /// <returns>A ParseResult indicating any follow-up actions needed.</returns>
    private async Task<ParseResult> HandleRestCommandAsync(GameState state, bool preferSanctuary)
    {
        // Validate dependencies
        if (_restService == null || _restRenderer == null || _roomRepository == null)
        {
            _inputHandler.DisplayError("Rest system not available.");
            _logger.LogWarning("Rest command attempted but required services are not configured.");
            return ParseResult.None;
        }

        // Validate character
        if (state.CurrentCharacter == null)
        {
            _inputHandler.DisplayError("No active character.");
            return ParseResult.None;
        }

        // Validate room
        if (state.CurrentRoomId == null)
        {
            _inputHandler.DisplayError("You are nowhere. This should not happen.");
            _logger.LogError("Rest command attempted with null CurrentRoomId.");
            return ParseResult.None;
        }

        // Get current room
        var room = await _roomRepository.GetByIdAsync(state.CurrentRoomId.Value);
        if (room == null)
        {
            _inputHandler.DisplayError("Cannot determine your location.");
            _logger.LogError("Room {RoomId} not found in database.", state.CurrentRoomId.Value);
            return ParseResult.None;
        }

        // Determine rest type based on location and preference
        RestType restType;
        bool hasAnchor = room.HasFeature(RoomFeature.RunicAnchor);

        if (preferSanctuary && hasAnchor)
        {
            restType = RestType.Sanctuary;
            _inputHandler.DisplayMessage("You rest at the Runic Anchor...");
            _logger.LogDebug("Rest type determined: {RestType} (HasAnchor: {HasAnchor})", restType, hasAnchor);
        }
        else
        {
            restType = RestType.Wilderness;
            _inputHandler.DisplayMessage("You set up camp in the wilderness...");
            _logger.LogDebug("Rest type determined: {RestType} (HasAnchor: {HasAnchor})", restType, hasAnchor);
        }

        _logger.LogInformation("Player executed {Command} in {Room}.", restType == RestType.Sanctuary ? "rest" : "camp", room.Name);

        // Perform rest with ambush check
        var result = await _restService.PerformRestAsync(state.CurrentCharacter, restType, room);

        // Handle ambush
        if (result.WasAmbushed && result.AmbushDetails != null)
        {
            _logger.LogWarning("Rest interrupted by ambush. Transitioning to Combat.");
            _restRenderer.RenderAmbushWarning(result.AmbushDetails);

            // Store encounter for GameService to handle
            state.PendingEncounter = result.AmbushDetails.Encounter;
            state.Phase = GamePhase.Combat;

            return ParseResult.None;
        }

        // Display rest results
        _restRenderer.Render(result);
        state.TurnCount++;

        _logger.LogInformation(
            "Rest complete. HP+{HP} Stamina+{Stamina} Stress-{Stress}.",
            result.HpRecovered, result.StaminaRecovered, result.StressRecovered);

        return ParseResult.None;
    }

    #endregion

    #region Note Command (v0.3.20a)

    /// <summary>
    /// Handles the note command for annotating rooms on the minimap.
    /// </summary>
    /// <param name="originalInput">The original (non-lowercased) command string to preserve note text casing.</param>
    /// <param name="state">The current game state.</param>
    /// <returns>A ParseResult (always None for note commands).</returns>
    private ParseResult HandleNoteCommand(string originalInput, GameState state)
    {
        // Validate room
        if (state.CurrentRoomId == null)
        {
            _inputHandler.DisplayError("You are nowhere. Cannot set a note.");
            _logger.LogWarning("Note command attempted with null CurrentRoomId.");
            return ParseResult.None;
        }

        var roomId = state.CurrentRoomId.Value;

        // Parse command: "note" (read), "note clear" (delete), "note <text>" (set)
        if (originalInput.Equals("note", StringComparison.OrdinalIgnoreCase))
        {
            // Read current note
            if (state.UserNotes.TryGetValue(roomId, out var existingNote))
            {
                _inputHandler.DisplayMessage($"[yellow]Note:[/] {existingNote}");
                _logger.LogDebug("Note read for room {RoomId}: {Note}", roomId, existingNote);
            }
            else
            {
                _inputHandler.DisplayMessage("[grey]No note for this room.[/]");
                _logger.LogDebug("No note exists for room {RoomId}.", roomId);
            }
            return ParseResult.None;
        }

        // Extract note text after "note " (case-insensitive prefix removal)
        var noteText = originalInput.Substring(5).Trim(); // Remove "note "

        if (noteText.Equals("clear", StringComparison.OrdinalIgnoreCase))
        {
            // Clear note
            if (state.UserNotes.Remove(roomId))
            {
                _inputHandler.DisplayMessage("[grey]Note cleared.[/]");
                _logger.LogDebug("Note cleared for room {RoomId}.", roomId);
            }
            else
            {
                _inputHandler.DisplayMessage("[grey]No note to clear.[/]");
            }
            return ParseResult.None;
        }

        // Validate note text
        if (string.IsNullOrWhiteSpace(noteText))
        {
            _inputHandler.DisplayMessage("[grey]Usage: note <text> | note clear | note (to view)[/]");
            return ParseResult.None;
        }

        // Clamp note length
        const int maxNoteLength = 100;
        if (noteText.Length > maxNoteLength)
        {
            noteText = noteText.Substring(0, maxNoteLength);
            _inputHandler.DisplayMessage($"[yellow]Note truncated to {maxNoteLength} characters.[/]");
        }

        // Set note
        state.UserNotes[roomId] = noteText;
        _inputHandler.DisplayMessage($"[green]Note set:[/] {noteText}");
        _logger.LogDebug("Note set for room {RoomId}: {Note}", roomId, noteText);

        return ParseResult.None;
    }

    /// <summary>
    /// Handles the map export command for generating ASCII atlas files (v0.3.20b).
    /// </summary>
    /// <param name="state">The current game state.</param>
    /// <returns>A ParseResult (always None for map export commands).</returns>
    private async Task<ParseResult> HandleMapExportAsync(GameState state)
    {
        _logger.LogDebug("[MapExport] Map export command received.");

        // Validate service availability
        if (_mapExportService == null)
        {
            _inputHandler.DisplayError("Map export service not available.");
            _logger.LogWarning("Map export command attempted but service is null.");
            return ParseResult.None;
        }

        // Validate character exists
        if (state.CurrentCharacter == null)
        {
            _inputHandler.DisplayError("No active character. Cannot export map.");
            _logger.LogWarning("Map export attempted with null CurrentCharacter.");
            return ParseResult.None;
        }

        // Validate room context
        if (state.CurrentRoomId == null)
        {
            _inputHandler.DisplayError("You are nowhere. Cannot export map.");
            _logger.LogWarning("Map export attempted with null CurrentRoomId.");
            return ParseResult.None;
        }

        // Validate visited rooms exist
        if (state.VisitedRoomIds.Count == 0)
        {
            _inputHandler.DisplayMessage("[grey]No rooms explored yet. Nothing to export.[/]");
            _logger.LogDebug("Map export attempted with empty VisitedRoomIds.");
            return ParseResult.None;
        }

        try
        {
            _inputHandler.DisplayMessage("[yellow]Generating atlas...[/]");

            // Get player position from room repository
            var currentRoom = _roomRepository != null
                ? await _roomRepository.GetByIdAsync(state.CurrentRoomId.Value)
                : null;

            var playerPosition = currentRoom?.Position ?? new Core.ValueObjects.Coordinate(0, 0, 0);

            // Export the map
            var filePath = await _mapExportService.ExportMapAsync(
                state.CurrentCharacter.Name,
                playerPosition,
                state.VisitedRoomIds,
                state.UserNotes);

            _inputHandler.DisplayMessage($"[green]Atlas exported successfully![/]");
            _inputHandler.DisplayMessage($"[grey]Saved to: {filePath}[/]");
            _logger.LogInformation("Map exported to {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _inputHandler.DisplayError($"Failed to export map: {ex.Message}");
            _logger.LogError(ex, "Map export failed.");
        }

        return ParseResult.None;
    }

    #endregion

    #region Travel Command (v0.3.20c)

    /// <summary>
    /// Handles the travel command for fast travel to visited locations.
    /// </summary>
    /// <param name="target">The destination target (room name, 'home', 'anchor', or 'surface').</param>
    /// <param name="state">The current game state.</param>
    /// <returns>A ParseResult (always None as travel executes inline).</returns>
    private async Task<ParseResult> HandleTravelCommandAsync(string target, GameState state)
    {
        _logger.LogDebug("[Travel] Travel command received with target: '{Target}'", target);

        // Validate service availability
        if (_autoTravelService == null)
        {
            _inputHandler.DisplayError("Travel service not available.");
            _logger.LogWarning("Travel command attempted but IAutoTravelService is null.");
            return ParseResult.None;
        }

        // Execute travel (the service handles all validation internally)
        // Note: ESC cancellation would require a CancellationTokenSource tied to Console.KeyAvailable polling
        // For now, travel runs to completion or until interrupted by combat
        var result = await _autoTravelService.TravelToAsync(target);

        // Display result
        if (result.Success)
        {
            _inputHandler.DisplayMessage($"[green]{result.Message}[/]");
        }
        else if (result.InterruptReason.HasValue && result.InterruptReason != TravelInterruptReason.None)
        {
            _inputHandler.DisplayMessage($"[yellow]{result.Message}[/]");
        }
        else
        {
            _inputHandler.DisplayError(result.Message);
        }

        // If travel completed or was interrupted, show current room
        if (result.RoomsTraveled > 0)
        {
            return new ParseResult { RequiresLook = true };
        }

        return ParseResult.None;
    }

    #endregion
}
