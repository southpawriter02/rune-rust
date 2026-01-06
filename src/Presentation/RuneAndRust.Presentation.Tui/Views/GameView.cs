using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using Spectre.Console;

namespace RuneAndRust.Presentation.Tui.Views;

public class GameView
{
    private readonly GameSessionService _gameService;
    private readonly IGameRenderer _renderer;
    private readonly IInputHandler _inputHandler;

    public GameView(
        GameSessionService gameService,
        IGameRenderer renderer,
        IInputHandler inputHandler)
    {
        _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _inputHandler = inputHandler ?? throw new ArgumentNullException(nameof(inputHandler));
    }

    public async Task RunGameLoopAsync(CancellationToken ct = default)
    {
        var state = _gameService.CurrentState;
        if (state == null)
        {
            await _renderer.RenderMessageAsync("Error: No active game session.", MessageType.Error, ct);
            return;
        }

        await _renderer.RenderGameStateAsync(state, ct);
        Console.WriteLine();

        while (!ct.IsCancellationRequested && _gameService.HasActiveSession)
        {
            var command = await _inputHandler.GetNextCommandAsync(ct);
            Console.WriteLine();

            var shouldContinue = await ProcessCommandAsync(command, ct);

            if (!shouldContinue)
                break;

            // Check for game over
            state = _gameService.CurrentState;
            if (state?.State == GameState.GameOver)
            {
                await _renderer.RenderMessageAsync("Game Over! You have fallen in battle.", MessageType.Error, ct);
                break;
            }
        }
    }

    private async Task<bool> ProcessCommandAsync(GameCommand command, CancellationToken ct)
    {
        switch (command)
        {
            case MoveCommand move:
                await HandleMoveAsync(move.Direction, ct);
                break;

            case LookCommand:
                await HandleLookAsync(ct);
                break;

            case InventoryCommand:
                await HandleInventoryAsync(ct);
                break;

            case TakeCommand take:
                await HandleTakeAsync(take.ItemName, ct);
                break;

            case AttackCommand:
                await HandleAttackAsync(ct);
                break;

            case SaveCommand:
                await HandleSaveAsync(ct);
                break;

            case HelpCommand:
                await HandleHelpAsync(ct);
                break;

            case QuitCommand:
                return await HandleQuitAsync(ct);

            case UnknownCommand unknown:
                await _renderer.RenderMessageAsync(
                    $"Unknown command: '{unknown.Input}'. Type 'help' for a list of commands.",
                    MessageType.Warning, ct);
                break;

            default:
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
            await _renderer.RenderMessageAsync(message, MessageType.Warning, ct);
        }
    }

    private async Task HandleLookAsync(CancellationToken ct)
    {
        var room = _gameService.GetCurrentRoom();
        if (room != null)
        {
            await _renderer.RenderRoomAsync(room, ct);
        }
    }

    private async Task HandleInventoryAsync(CancellationToken ct)
    {
        var inventory = _gameService.GetInventory();
        if (inventory != null)
        {
            await _renderer.RenderInventoryAsync(inventory, ct);
        }
    }

    private async Task HandleTakeAsync(string itemName, CancellationToken ct)
    {
        var (success, message) = _gameService.TryPickUpItem(itemName);
        var messageType = success ? MessageType.Success : MessageType.Warning;
        await _renderer.RenderMessageAsync(message, messageType, ct);
    }

    private async Task HandleAttackAsync(CancellationToken ct)
    {
        var (success, message) = _gameService.TryAttack();

        if (success)
        {
            await _renderer.RenderCombatResultAsync(message, ct);

            // Refresh room display after combat
            var room = _gameService.GetCurrentRoom();
            if (room != null && !room.Monsters.Any(m => m.IsAlive))
            {
                Console.WriteLine();
                await _renderer.RenderMessageAsync("The area is now clear.", MessageType.Success, ct);
            }
        }
        else
        {
            await _renderer.RenderMessageAsync(message, MessageType.Warning, ct);
        }
    }

    private async Task HandleSaveAsync(CancellationToken ct)
    {
        await _gameService.SaveCurrentGameAsync(ct);
        await _renderer.RenderMessageAsync("Game saved successfully!", MessageType.Success, ct);
    }

    private Task HandleHelpAsync(CancellationToken ct)
    {
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
        helpTable.AddRow("[cyan]look, l[/]", "Look around the current room");
        helpTable.AddRow("[cyan]inventory, i[/]", "View your inventory");
        helpTable.AddRow("[cyan]take <item>[/]", "Pick up an item");
        helpTable.AddRow("[cyan]attack, a[/]", "Attack an enemy");
        helpTable.AddRow("[cyan]save[/]", "Save your game");
        helpTable.AddRow("[cyan]help, h, ?[/]", "Show this help");
        helpTable.AddRow("[cyan]quit, q[/]", "Quit the game");

        AnsiConsole.Write(helpTable);
        return Task.CompletedTask;
    }

    private async Task<bool> HandleQuitAsync(CancellationToken ct)
    {
        var confirm = await _inputHandler.GetConfirmationAsync("Are you sure you want to quit?", ct);

        if (confirm)
        {
            var saveFirst = await _inputHandler.GetConfirmationAsync("Would you like to save before quitting?", ct);
            if (saveFirst)
            {
                await HandleSaveAsync(ct);
            }

            _gameService.EndSession();
            await _renderer.RenderMessageAsync("Thanks for playing Rune & Rust!", MessageType.Info, ct);
            return false;
        }

        return true;
    }
}
