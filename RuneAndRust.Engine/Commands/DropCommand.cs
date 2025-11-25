using System.Text;
using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine.Commands;

/// <summary>
/// v0.37.3: Drop Command
/// Remove item from inventory and place in room.
/// Syntax: drop [item]
/// </summary>
public class DropCommand : ICommand
{
    private static readonly ILogger _log = Log.ForContext<DropCommand>();
    private readonly EquipmentService _equipmentService;

    public DropCommand(EquipmentService equipmentService)
    {
        _equipmentService = equipmentService;
    }

    public CommandResult Execute(GameState state, string[] args)
    {
        if (state.Player == null)
        {
            _log.Warning("Drop command failed: Player is null");
            return CommandResult.CreateFailure("Player not found.");
        }

        if (state.CurrentRoom == null)
        {
            _log.Warning("Drop command failed: CurrentRoom is null");
            return CommandResult.CreateFailure("You are not in a room.");
        }

        // Check for arguments
        if (args.Length == 0)
        {
            _log.Debug("Drop command: No item specified");
            return CommandResult.CreateFailure("Drop what? (Usage: drop [item name])");
        }

        // Join all arguments to handle multi-word item names
        string itemName = string.Join(" ", args);

        _log.Information(
            "Drop command: CharacterId={CharacterId}, Item={Item}, RoomId={RoomId}",
            state.Player.CharacterID,
            itemName,
            state.CurrentRoom.RoomId);

        // Find the item in inventory
        var item = _equipmentService.FindInInventory(state.Player, itemName);

        if (item == null)
        {
            _log.Warning(
                "Drop failed: Item not found in inventory: Item={Item}, CharacterId={CharacterId}",
                itemName,
                state.Player.CharacterID);

            // Show what's in inventory
            if (state.Player.Inventory.Any())
            {
                var inventoryItems = string.Join(", ", state.Player.Inventory.Select(i => i.Name));
                return CommandResult.CreateFailure(
                    $"You don't have a '{itemName}' in your inventory.\nYou have: {inventoryItems}");
            }

            return CommandResult.CreateFailure("Your inventory is empty.");
        }

        // Drop the item using EquipmentService
        bool success = _equipmentService.DropItem(state.Player, state.CurrentRoom, item);

        if (!success)
        {
            _log.Error(
                "Drop failed: Drop operation failed: Item={Item}, CharacterId={CharacterId}",
                item.Name,
                state.Player.CharacterID);

            return CommandResult.CreateFailure($"Failed to drop the {item.Name}.");
        }

        _log.Information(
            "Drop successful: Item={Item}, NewInventorySize={Size}",
            item.Name,
            state.Player.Inventory.Count);

        return CommandResult.CreateSuccess($"You drop the {item.Name}.");
    }
}
