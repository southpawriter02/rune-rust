using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using Spectre.Console;

namespace RuneAndRust.Presentation.Tui.Views;

/// <summary>
/// The main game view that runs the command loop for TUI gameplay.
/// </summary>
/// <remarks>
/// GameView coordinates user input, game logic, and rendering during active gameplay.
/// It processes commands from the input handler, calls the appropriate game service
/// methods, and renders results through the game renderer.
/// </remarks>
public class GameView
{
    /// <summary>
    /// The game session service for game logic operations.
    /// </summary>
    private readonly GameSessionService _gameService;

    /// <summary>
    /// The renderer for displaying game output.
    /// </summary>
    private readonly IGameRenderer _renderer;

    /// <summary>
    /// The input handler for reading user commands.
    /// </summary>
    private readonly IInputHandler _inputHandler;

    /// <summary>
    /// Logger for view operations and diagnostics.
    /// </summary>
    private readonly ILogger<GameView> _logger;

    /// <summary>
    /// Creates a new game view instance.
    /// </summary>
    /// <param name="gameService">The game session service.</param>
    /// <param name="renderer">The game renderer.</param>
    /// <param name="inputHandler">The input handler.</param>
    /// <param name="logger">The logger for diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public GameView(
        GameSessionService gameService,
        IGameRenderer renderer,
        IInputHandler inputHandler,
        ILogger<GameView> logger)
    {
        _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _inputHandler = inputHandler ?? throw new ArgumentNullException(nameof(inputHandler));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logger.LogDebug("GameView initialized");
    }

    /// <summary>
    /// Runs the main game loop, processing commands until the game ends or is cancelled.
    /// </summary>
    /// <param name="ct">Cancellation token to stop the game loop.</param>
    public async Task RunGameLoopAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Game loop started");

        var state = _gameService.CurrentState;
        if (state == null)
        {
            _logger.LogError("Game loop aborted: No active game session");
            await _renderer.RenderMessageAsync("Error: No active game session.", MessageType.Error, ct);
            return;
        }

        _logger.LogDebug(
            "Initial state - Session: {SessionId}, Player: {PlayerName}, Room: {RoomName}, State: {GameState}",
            state.SessionId,
            state.Player.Name,
            state.CurrentRoom.Name,
            state.State);

        await _renderer.RenderGameStateAsync(state, ct);
        Console.WriteLine();

        var commandCount = 0;
        while (!ct.IsCancellationRequested && _gameService.HasActiveSession)
        {
            var command = await _inputHandler.GetNextCommandAsync(ct);
            commandCount++;
            Console.WriteLine();

            _logger.LogDebug("Processing command #{CommandNumber}: {CommandType}", commandCount, command.GetType().Name);

            var shouldContinue = await ProcessCommandAsync(command, ct);

            if (!shouldContinue)
            {
                _logger.LogInformation("Game loop ended by user request after {CommandCount} commands", commandCount);
                break;
            }

            // Check for game over
            state = _gameService.CurrentState;
            if (state?.State == GameState.GameOver)
            {
                _logger.LogWarning("Game Over detected. Player: {PlayerName}, Commands processed: {CommandCount}",
                    state.Player.Name, commandCount);
                await _renderer.RenderMessageAsync("Game Over! You have fallen in battle.", MessageType.Error, ct);
                break;
            }
        }

        if (ct.IsCancellationRequested)
        {
            _logger.LogInformation("Game loop cancelled via CancellationToken after {CommandCount} commands", commandCount);
        }
    }

    private async Task<bool> ProcessCommandAsync(GameCommand command, CancellationToken ct)
    {
        switch (command)
        {
            case MoveCommand move:
                _logger.LogDebug("Handling move command: {Direction}", move.Direction);
                await HandleMoveAsync(move.Direction, ct);
                break;

            case LookCommand look:
                await HandleLookAsync(look.Target, ct);
                break;

            case InventoryCommand:
                _logger.LogDebug("Handling inventory command");
                await HandleInventoryAsync(ct);
                break;

            case TakeCommand take:
                _logger.LogDebug("Handling take command: {ItemName}", take.ItemName);
                await HandleTakeAsync(take.ItemName, ct);
                break;

            case DropCommand drop:
                _logger.LogDebug("Handling drop command: {ItemName}", drop.ItemName);
                await HandleDropAsync(drop.ItemName, ct);
                break;

            case UseCommand use:
                _logger.LogDebug("Handling use command: {ItemName}", use.ItemName);
                await HandleUseAsync(use.ItemName, ct);
                break;

            case ExamineCommand examine:
                _logger.LogDebug("Handling examine command: {Target}", examine.Target);
                await HandleExamineAsync(examine.Target, ct);
                break;

            case StatusCommand:
                _logger.LogDebug("Handling status command");
                await HandleStatusAsync(ct);
                break;

            case AbilitiesCommand:
                _logger.LogDebug("Handling abilities command");
                await HandleAbilitiesAsync(ct);
                break;

            case UseAbilityCommand useAbility:
                _logger.LogDebug("Handling use ability command: {AbilityName}", useAbility.AbilityName);
                await HandleUseAbilityAsync(useAbility.AbilityName, ct);
                break;

            case LoadCommand:
                _logger.LogDebug("Handling load command");
                await HandleLoadAsync(ct);
                break;

            case AttackCommand:
                _logger.LogDebug("Handling attack command");
                await HandleAttackAsync(ct);
                break;

            case SearchCommand search:
                await HandleSearchAsync(search.Target, ct);
                break;

            case InvestigateCommand investigate:
                await HandleInvestigateAsync(investigate.Target, ct);
                break;

            case ExamineCommand examine:
                await HandleExamineAsync(examine.Target, ct);
                break;

            case TravelCommand travel:
                await HandleTravelAsync(travel.Destination, ct);
                break;

            case EnterCommand enter:
                await HandleEnterAsync(enter.Location, ct);
                break;

            case ExitCommand exit:
                await HandleExitAsync(exit.Direction, ct);
                break;

            case SaveCommand:
                _logger.LogDebug("Handling save command");
                await HandleSaveAsync(ct);
                break;

            case HelpCommand:
                _logger.LogDebug("Handling help command");
                await HandleHelpAsync(ct);
                break;

            case QuitCommand:
                _logger.LogDebug("Handling quit command");
                return await HandleQuitAsync(ct);

            case UnknownCommand unknown:
                _logger.LogWarning("Unknown command received: {Input}", unknown.Input);
                await _renderer.RenderMessageAsync(
                    $"Unknown command: '{unknown.Input}'. Type 'help' for a list of commands.",
                    MessageType.Warning, ct);
                break;

            default:
                _logger.LogWarning("Unhandled command type: {CommandType}", command.GetType().Name);
                await _renderer.RenderMessageAsync(
                    "Command not implemented yet.",
                    MessageType.Warning, ct);
                break;
        }

        Console.WriteLine();
        return true;
    }

    private async Task HandleMoveAsync(Direction direction, CancellationToken ct)
    {
        var (success, message) = _gameService.TryMove(direction);

        if (success)
        {
            _logger.LogDebug("Move successful: {Direction}", direction);
            await _renderer.RenderMessageAsync(message, MessageType.Success, ct);
            Console.WriteLine();

            var room = _gameService.GetCurrentRoom();
            if (room != null)
            {
                await _renderer.RenderRoomAsync(room, ct);
            }
        }
        else
        {
            _logger.LogDebug("Move failed: {Direction} - {Message}", direction, message);
            await _renderer.RenderMessageAsync(message, MessageType.Warning, ct);
        }
    }

    private async Task HandleLookAsync(string? target, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(target))
        {
            // Look at room
            var room = _gameService.GetCurrentRoom();
            if (room != null)
            {
                await _renderer.RenderRoomAsync(room, ct);
            }
        }
        else
        {
            // Look at specific target
            var lookResult = _gameService.TryLookAtTarget(target);
            if (lookResult.Success)
            {
                await _renderer.RenderMessageAsync(lookResult.Description!, MessageType.Info, ct);
            }
            else
            {
                await _renderer.RenderMessageAsync(lookResult.ErrorMessage!, MessageType.Warning, ct);
            }
        }
        else
        {
            _logger.LogWarning("HandleLookAsync: No current room available");
        }
    }

    private async Task HandleInventoryAsync(CancellationToken ct)
    {
        var inventory = _gameService.GetInventory();
        if (inventory != null)
        {
            _logger.LogDebug("Displaying inventory: {ItemCount}/{Capacity} items", inventory.Count, inventory.Capacity);
            await _renderer.RenderInventoryAsync(inventory, ct);
        }
        else
        {
            _logger.LogWarning("HandleInventoryAsync: No inventory available");
        }
    }

    private async Task HandleTakeAsync(string itemName, CancellationToken ct)
    {
        var (success, message) = _gameService.TryPickUpItem(itemName);
        var messageType = success ? MessageType.Success : MessageType.Warning;
        _logger.LogDebug("Take item result: {ItemName}, Success: {Success}", itemName, success);
        await _renderer.RenderMessageAsync(message, messageType, ct);
    }

    private async Task HandleDropAsync(string itemName, CancellationToken ct)
    {
        var (success, message) = _gameService.TryDropItem(itemName);
        var messageType = success ? MessageType.Success : MessageType.Warning;
        _logger.LogDebug("Drop item result: {ItemName}, Success: {Success}", itemName, success);
        await _renderer.RenderMessageAsync(message, messageType, ct);
    }

    private async Task HandleUseAsync(string itemName, CancellationToken ct)
    {
        var (success, message) = _gameService.TryUseItem(itemName);
        var messageType = success ? MessageType.Success : MessageType.Warning;
        _logger.LogDebug("Use item result: {ItemName}, Success: {Success}", itemName, success);
        await _renderer.RenderMessageAsync(message, messageType, ct);
    }

    private async Task HandleExamineAsync(string target, CancellationToken ct)
    {
        var result = _gameService.GetExamineInfo(target);
        if (result == null)
        {
            _logger.LogDebug("Examine failed: target not found: {Target}", target);
            await _renderer.RenderMessageAsync($"You don't see '{target}' here.", MessageType.Warning, ct);
            return;
        }

        _logger.LogDebug("Examining: {Target} (Type: {TargetType})", result.Name, result.Type);
        await _renderer.RenderExamineResultAsync(result, ct);
    }

    private async Task HandleStatusAsync(CancellationToken ct)
    {
        var stats = _gameService.GetPlayerStats();
        if (stats == null)
        {
            _logger.LogWarning("Status failed: no active session");
            await _renderer.RenderMessageAsync("No active game session.", MessageType.Warning, ct);
            return;
        }

        _logger.LogDebug("Displaying player status for: {PlayerName}", stats.Name);
        await _renderer.RenderPlayerStatsAsync(stats, ct);
    }

    private async Task HandleLoadAsync(CancellationToken ct)
    {
        var savedGames = await _gameService.GetSavedGamesAsync(ct);
        if (savedGames.Count == 0)
        {
            _logger.LogInformation("No saved games found");
            await _renderer.RenderMessageAsync("No saved games found.", MessageType.Warning, ct);
            return;
        }

        _logger.LogDebug("Found {Count} saved games", savedGames.Count);
        var selected = await _inputHandler.GetSelectionAsync(
            "Select a saved game to load:",
            savedGames,
            g => $"{g.PlayerName} - {g.LastPlayedAt:g}",
            ct);

        _logger.LogInformation("Loading game session: {SessionId}", selected.Id);
        await _gameService.LoadGameAsync(selected.Id, ct);
        await _renderer.RenderMessageAsync($"Loaded game: {selected.PlayerName}", MessageType.Success, ct);
    }

    private async Task HandleAttackAsync(CancellationToken ct)
    {
        var (success, message, experienceGain, levelUp, lootDrop) = _gameService.TryAttack();

        if (success)
        {
            _logger.LogDebug("Attack executed successfully");
            await _renderer.RenderCombatResultAsync(message, ct);

            // Display experience gain if any (v0.0.8a)
            if (experienceGain != null)
            {
                await _renderer.RenderExperienceGainAsync(experienceGain, ct);
            }

            // Display level-up if any (v0.0.8b)
            if (levelUp != null)
            {
                await _renderer.RenderLevelUpAsync(levelUp, ct);
            }

            // Display loot drop if any (v0.0.9d)
            if (lootDrop != null && !lootDrop.IsEmpty)
            {
                await _renderer.RenderLootDropAsync(lootDrop, ct);
            }

            // Process turn-end effects (resource regen/decay, cooldown reduction)
            var turnEndResult = _gameService.ProcessTurnEnd();
            await _renderer.RenderTurnEndChangesAsync(turnEndResult, ct);

            // Refresh room display after combat
            var room = _gameService.GetCurrentRoom();
            if (room != null && !room.Monsters.Any(m => m.IsAlive))
            {
                _logger.LogInformation("All monsters defeated in room: {RoomName}", room.Name);
                Console.WriteLine();
                await _renderer.RenderMessageAsync("The area is now clear.", MessageType.Success, ct);
            }
        }
        else
        {
            _logger.LogDebug("Attack failed: {Message}", message);
            await _renderer.RenderMessageAsync(message, MessageType.Warning, ct);
        }
    }

    private async Task HandleSaveAsync(CancellationToken ct)
    {
        _logger.LogInformation("Saving game...");
        await _gameService.SaveCurrentGameAsync(ct);
        _logger.LogInformation("Game saved successfully");
        await _renderer.RenderMessageAsync("Game saved successfully!", MessageType.Success, ct);
    }

    private async Task HandleSearchAsync(string? target, CancellationToken ct)
    {
        var searchResult = _gameService.TrySearch(target);
        if (searchResult.Success)
        {
            await _renderer.RenderMessageAsync(searchResult.Message, MessageType.Success, ct);
        }
        else
        {
            await _renderer.RenderMessageAsync(searchResult.Message, MessageType.Warning, ct);
        }
    }

    private async Task HandleInvestigateAsync(string target, CancellationToken ct)
    {
        var investigateResult = _gameService.TryInvestigate(target);
        if (investigateResult.Success)
        {
            await _renderer.RenderMessageAsync(investigateResult.Message, MessageType.Success, ct);
        }
        else
        {
            await _renderer.RenderMessageAsync(investigateResult.Message, MessageType.Warning, ct);
        }
    }

    private async Task HandleExamineAsync(string target, CancellationToken ct)
    {
        var (success, result, errorMessage) = await _gameService.TryExamineAsync(target, ct);

        if (!success || result == null)
        {
            await _renderer.RenderMessageAsync(errorMessage ?? $"Cannot examine '{target}'.", MessageType.Warning, ct);
            return;
        }

        // Build the examination display
        var panel = new Panel(new Markup($"[white]{EscapeMarkup(result.CompositeDescription)}[/]"))
        {
            Header = new PanelHeader($"[yellow]{result.ObjectName.ToUpper()}[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Grey)
        };

        AnsiConsole.Write(panel);

        // Show WITS check result
        var layerName = result.HighestLayerUnlocked switch
        {
            Domain.Enums.ExaminationLayer.Cursory => "Cursory",
            Domain.Enums.ExaminationLayer.Detailed => "Detailed",
            Domain.Enums.ExaminationLayer.Expert => "Expert",
            _ => "Basic"
        };

        var checkColor = result.HighestLayerUnlocked switch
        {
            Domain.Enums.ExaminationLayer.Expert => "green",
            Domain.Enums.ExaminationLayer.Detailed => "yellow",
            _ => "grey"
        };

        await _renderer.RenderMessageAsync(
            $"[WITS Check: {result.WitsRoll} + {result.WitsTotal - result.WitsRoll} = {result.WitsTotal}] [{layerName} examination]",
            MessageType.Info, ct);

        // Show hint notification if applicable
        if (result.RevealedHint)
        {
            Console.WriteLine();
            await _renderer.RenderMessageAsync("[Hint revealed!]", MessageType.Success, ct);
        }

        // Show solution notification if applicable
        if (!string.IsNullOrWhiteSpace(result.RevealedSolutionId))
        {
            Console.WriteLine();
            await _renderer.RenderMessageAsync("[New puzzle solution discovered!]", MessageType.Success, ct);
        }
    }

    private static string EscapeMarkup(string text)
    {
        // Escape special Spectre.Console markup characters
        return text
            .Replace("[", "[[")
            .Replace("]", "]]");
    }

    private async Task HandleTravelAsync(string? destination, CancellationToken ct)
    {
        await _renderer.RenderMessageAsync(
            "Travel system is not yet implemented. This feature will be available in a future update.",
            MessageType.Info, ct);
    }

    private async Task HandleEnterAsync(string? location, CancellationToken ct)
    {
        await _renderer.RenderMessageAsync(
            "Enter command is not yet implemented. This feature will be available in a future update.",
            MessageType.Info, ct);
    }

    private async Task HandleExitAsync(string? direction, CancellationToken ct)
    {
        await _renderer.RenderMessageAsync(
            "Exit command is not yet implemented. Use movement commands (n/s/e/w) to leave the current area.",
            MessageType.Info, ct);
    }

    private Task HandleHelpAsync(CancellationToken ct)
    {
        _logger.LogDebug("Displaying help menu");

        var helpTable = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .Title("[yellow]Commands[/]")
            .AddColumn(new TableColumn("[cyan]Command[/]"))
            .AddColumn(new TableColumn("[white]Description[/]"));

        helpTable.AddRow("[cyan]n, north[/]", "Move north");
        helpTable.AddRow("[cyan]s, south[/]", "Move south");
        helpTable.AddRow("[cyan]e, east[/]", "Move east");
        helpTable.AddRow("[cyan]w, west[/]", "Move west");
        helpTable.AddRow("[cyan]u, up[/]", "Move up (stairs, ladders)");
        helpTable.AddRow("[cyan]d, down[/]", "Move down (stairs, ladders)");
        helpTable.AddRow("[cyan]ne, nw, se, sw[/]", "Move diagonally");
        helpTable.AddRow("[cyan]look, l [target][/]", "Look around or at target (brief)");
        helpTable.AddRow("[cyan]examine, x <target>[/]", "Examine with WITS check (detailed)");
        helpTable.AddRow("[cyan]search [container][/]", "Search for hidden elements");
        helpTable.AddRow("[cyan]investigate <target>[/]", "Investigate for secrets");
        helpTable.AddRow("[cyan]inventory, i[/]", "View your inventory");
        helpTable.AddRow("[cyan]take <item>[/]", "Pick up an item");
        helpTable.AddRow("[cyan]drop <item>[/]", "Drop an item");
        helpTable.AddRow("[cyan]use <item>[/]", "Use/consume an item");
        helpTable.AddRow("[cyan]examine <target>[/]", "Examine an item, monster, or room");
        helpTable.AddRow("[cyan]status[/]", "View character stats");
        helpTable.AddRow("[cyan]abilities, ab[/]", "View your abilities");
        helpTable.AddRow("[cyan]cast <ability>[/]", "Use an ability");
        helpTable.AddRow("[cyan]attack, a[/]", "Attack an enemy");
        helpTable.AddRow("[cyan]save[/]", "Save your game");
        helpTable.AddRow("[cyan]load[/]", "Load a saved game");
        helpTable.AddRow("[cyan]help, h, ?[/]", "Show this help");
        helpTable.AddRow("[cyan]quit, q[/]", "Quit the game");

        AnsiConsole.Write(helpTable);
        return Task.CompletedTask;
    }

    private async Task<bool> HandleQuitAsync(CancellationToken ct)
    {
        _logger.LogDebug("Quit requested, prompting for confirmation");

        var confirm = await _inputHandler.GetConfirmationAsync("Are you sure you want to quit?", ct);

        if (confirm)
        {
            _logger.LogInformation("User confirmed quit");

            var saveFirst = await _inputHandler.GetConfirmationAsync("Would you like to save before quitting?", ct);
            if (saveFirst)
            {
                _logger.LogDebug("User chose to save before quitting");
                await HandleSaveAsync(ct);
            }
            else
            {
                _logger.LogDebug("User chose not to save before quitting");
            }

            _gameService.EndSession();
            await _renderer.RenderMessageAsync("Thanks for playing Rune & Rust!", MessageType.Info, ct);
            return false;
        }

        _logger.LogDebug("User cancelled quit");
        return true;
    }
}
