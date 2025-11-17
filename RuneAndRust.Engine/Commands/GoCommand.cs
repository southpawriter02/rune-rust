using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine.Commands;

/// <summary>
/// v0.37.1: Go Command
/// Navigate between rooms in the game world.
/// Syntax: go [direction] or go [room_name]
/// Aliases: g, move, n, s, e, w, u, d
/// </summary>
public class GoCommand : ICommand
{
    private static readonly ILogger _log = Log.ForContext<GoCommand>();

    public CommandResult Execute(GameState state, string[] args)
    {
        if (args.Length == 0)
        {
            _log.Debug("Go command called with no direction");
            var availableExits = GetAvailableExitsMessage(state.CurrentRoom);
            return CommandResult.Failure($"Go where? {availableExits}");
        }

        var direction = args[0].ToLower();

        _log.Information(
            "Go command executed: CharacterID={CharacterID}, Direction={Direction}, CurrentRoom={RoomId}",
            state.Player?.CharacterID ?? 0,
            direction,
            state.CurrentRoom?.RoomId ?? "unknown");

        try
        {
            // Validation 1: Cannot move during combat (must flee)
            if (state.CurrentPhase == GamePhase.Combat)
            {
                _log.Debug("Movement blocked: Player is in combat");
                return CommandResult.Failure("You cannot leave during combat. Use 'flee' to escape.");
            }

            // Validation 2: Exit must exist
            if (!state.CurrentRoom.Exits.ContainsKey(direction))
            {
                _log.Debug("Invalid direction: {Direction}", direction);
                var availableExits = GetAvailableExitsMessage(state.CurrentRoom);
                return CommandResult.Failure($"There is no exit to the {direction}. {availableExits}");
            }

            // TODO: Validation 3: Exit must not be locked (implement in future)
            // This will require a LockedExits system on Room

            // Validation 4: Character must have movement capability (future: status effects)
            // For now, assume player can always move

            // Execute movement
            var previousRoom = state.CurrentRoom.Name;
            state.MoveToRoom(direction);
            var newRoom = state.CurrentRoom.Name;

            _log.Information(
                "Player moved: From={From} To={To} Direction={Direction}",
                previousRoom,
                newRoom,
                direction);

            // Update world state
            state.UpdateWorldState();

            // Return success with new room description
            var lookCommand = new LookCommand();
            var roomDescription = lookCommand.Execute(state, Array.Empty<string>());

            return CommandResult.Success(roomDescription.Message, redrawRoom: true);
        }
        catch (InvalidOperationException ex)
        {
            _log.Warning(ex, "Movement failed: {Message}", ex.Message);
            return CommandResult.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _log.Error(ex,
                "Go command failed: CharacterID={CharacterID}, Error={ErrorType}",
                state.Player?.CharacterID ?? 0,
                ex.GetType().Name);
            return CommandResult.Failure("An error occurred while moving.");
        }
    }

    /// <summary>
    /// Generate helpful message showing available exits
    /// </summary>
    private string GetAvailableExitsMessage(Room room)
    {
        if (room?.Exits == null || !room.Exits.Any())
        {
            return "There are no exits from here.";
        }

        var exitList = string.Join(", ", room.Exits.Keys);
        return $"Valid exits: {exitList}";
    }
}
