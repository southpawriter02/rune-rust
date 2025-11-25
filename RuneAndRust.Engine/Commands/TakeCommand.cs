using System.Text;
using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine.Commands;

/// <summary>
/// v0.37.3: Take Command
/// Acquire items from the room ground.
/// Syntax: take [item] (aliases: get, pickup)
/// Future: take [item] from [container], take all from [container]
/// </summary>
public class TakeCommand : ICommand
{
    private static readonly ILogger _log = Log.ForContext<TakeCommand>();
    private readonly EquipmentService _equipmentService;

    public TakeCommand(EquipmentService equipmentService)
    {
        _equipmentService = equipmentService;
    }

    public CommandResult Execute(GameState state, string[] args)
    {
        if (state.Player == null)
        {
            _log.Warning("Take command failed: Player is null");
            return CommandResult.CreateFailure("Player not found.");
        }

        if (state.CurrentRoom == null)
        {
            _log.Warning("Take command failed: CurrentRoom is null");
            return CommandResult.CreateFailure("You are not in a room.");
        }

        // Check for arguments
        if (args.Length == 0)
        {
            _log.Debug("Take command: No item specified");
            return CommandResult.CreateFailure("Take what? (Usage: take [item name])");
        }

        // Join all arguments to handle multi-word item names
        string itemName = string.Join(" ", args);

        _log.Information(
            "Take command: CharacterId={CharacterId}, Item={Item}, RoomId={RoomId}",
            state.Player.CharacterID,
            itemName,
            state.CurrentRoom.RoomId);

        // Check inventory capacity
        if (state.Player.Inventory.Count >= state.Player.MaxInventorySize)
        {
            _log.Warning(
                "Take failed: Inventory full: CharacterId={CharacterId}, Current={Current}, Max={Max}",
                state.Player.CharacterID,
                state.Player.Inventory.Count,
                state.Player.MaxInventorySize);

            return CommandResult.CreateFailure(
                $"Your inventory is full ({state.Player.Inventory.Count}/{state.Player.MaxInventorySize}). " +
                "Drop something first or you'll be encumbered.");
        }

        // Find the item on the ground
        var item = _equipmentService.FindOnGround(state.CurrentRoom, itemName);

        if (item == null)
        {
            _log.Warning(
                "Take failed: Item not found: Item={Item}, RoomId={RoomId}",
                itemName,
                state.CurrentRoom.RoomId);

            // Show what's available
            if (state.CurrentRoom.ItemsOnGround.Any())
            {
                var availableItems = string.Join(", ", state.CurrentRoom.ItemsOnGround.Select(i => i.Name));
                return CommandResult.CreateFailure(
                    $"There is no '{itemName}' here.\nAvailable items: {availableItems}");
            }

            return CommandResult.CreateFailure($"There is no '{itemName}' here.");
        }

        // Pick up the item using EquipmentService
        bool success = _equipmentService.PickupItem(state.Player, state.CurrentRoom, item);

        if (!success)
        {
            _log.Error(
                "Take failed: Pickup failed: Item={Item}, CharacterId={CharacterId}",
                item.Name,
                state.Player.CharacterID);

            return CommandResult.CreateFailure($"Failed to pick up the {item.Name}.");
        }

        _log.Information(
            "Take successful: Item={Item}, NewInventorySize={Size}",
            item.Name,
            state.Player.Inventory.Count);

        return CommandResult.CreateSuccess($"You take the {item.Name}.");
    }
}
