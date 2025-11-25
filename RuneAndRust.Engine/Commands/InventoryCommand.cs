using System.Text;
using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine.Commands;

/// <summary>
/// v0.37.3: Inventory Command
/// Displays all carried items with capacity information.
/// Syntax: inventory (aliases: inv, i)
/// </summary>
public class InventoryCommand : ICommand
{
    private static readonly ILogger _log = Log.ForContext<InventoryCommand>();

    public CommandResult Execute(GameState state, string[] args)
    {
        if (state.Player == null)
        {
            _log.Warning("Inventory command failed: Player is null");
            return CommandResult.CreateFailure("Player not found.");
        }

        _log.Information(
            "Inventory command: CharacterId={CharacterId}",
            state.Player.CharacterID);

        var output = new StringBuilder();

        // Header
        output.AppendLine("╔════════════════════════════════════════╗");
        output.AppendLine("║ Your Inventory                         ║");

        // Capacity information
        int currentSize = state.Player.Inventory.Count;
        int maxSize = state.Player.MaxInventorySize;
        string capacityStatus = GetCapacityStatus(currentSize, maxSize);
        output.AppendLine($"║ Capacity: {currentSize}/{maxSize} ({capacityStatus})               ║");
        output.AppendLine("╠════════════════════════════════════════╣");

        // Check if inventory is empty
        if (state.Player.Inventory.Count == 0 &&
            state.Player.Consumables.Count == 0 &&
            state.Player.CraftingComponents.Count == 0)
        {
            output.AppendLine("║ (empty)                                ║");
            output.AppendLine("╚════════════════════════════════════════╝");

            _log.Information("Inventory displayed: Empty inventory");
            return CommandResult.CreateSuccess(output.ToString());
        }

        // Group inventory items by type
        var weapons = state.Player.Inventory.Where(e => e.Type == EquipmentType.Weapon).ToList();
        var armor = state.Player.Inventory.Where(e => e.Type == EquipmentType.Armor).ToList();
        var accessories = state.Player.Inventory.Where(e => e.Type == EquipmentType.Accessory).ToList();

        // Display weapons
        if (weapons.Any())
        {
            output.AppendLine("║ WEAPONS:                               ║");
            foreach (var weapon in weapons)
            {
                string weaponInfo = FormatWeaponInfo(weapon);
                output.AppendLine($"║  - {weaponInfo.PadRight(36)} ║");
            }
            output.AppendLine("║                                        ║");
        }

        // Display armor
        if (armor.Any())
        {
            output.AppendLine("║ ARMOR:                                 ║");
            foreach (var armorPiece in armor)
            {
                string armorInfo = FormatArmorInfo(armorPiece);
                output.AppendLine($"║  - {armorInfo.PadRight(36)} ║");
            }
            output.AppendLine("║                                        ║");
        }

        // Display accessories
        if (accessories.Any())
        {
            output.AppendLine("║ ACCESSORIES:                           ║");
            foreach (var accessory in accessories)
            {
                string accessoryInfo = $"{accessory.GetDisplayName()}";
                output.AppendLine($"║  - {accessoryInfo.PadRight(36)} ║");
            }
            output.AppendLine("║                                        ║");
        }

        // Display consumables
        if (state.Player.Consumables.Any())
        {
            output.AppendLine("║ CONSUMABLES:                           ║");

            // Group consumables by name to show quantity
            var consumableGroups = state.Player.Consumables
                .GroupBy(c => c.Name)
                .OrderBy(g => g.Key);

            foreach (var group in consumableGroups)
            {
                string consumableInfo = $"{group.Key} (x{group.Count()})";
                output.AppendLine($"║  - {consumableInfo.PadRight(36)} ║");
            }
            output.AppendLine("║                                        ║");
        }

        // Display crafting components and currency (MISC)
        if (state.Player.CraftingComponents.Any() || state.Player.Currency > 0)
        {
            output.AppendLine("║ MISC:                                  ║");

            // Show crafting components
            foreach (var component in state.Player.CraftingComponents.OrderBy(c => c.Key))
            {
                string componentInfo = $"{component.Key} ({component.Value})";
                output.AppendLine($"║  - {componentInfo.PadRight(36)} ║");
            }

            // Show currency (Scrap/Cogs)
            if (state.Player.Currency > 0)
            {
                string currencyInfo = $"Scrap ({state.Player.Currency})";
                output.AppendLine($"║  - {currencyInfo.PadRight(36)} ║");
            }

            output.AppendLine("║                                        ║");
        }

        output.AppendLine("╚════════════════════════════════════════╝");
        output.AppendLine();
        output.AppendLine("Commands: equip [item], drop [item], use [item]");

        _log.Information(
            "Inventory displayed: Items={ItemCount}, Consumables={ConsumableCount}, Components={ComponentCount}",
            state.Player.Inventory.Count,
            state.Player.Consumables.Count,
            state.Player.CraftingComponents.Count);

        return CommandResult.CreateSuccess(output.ToString());
    }

    /// <summary>
    /// Get capacity status description (Normal, Full, etc.)
    /// </summary>
    private string GetCapacityStatus(int current, int max)
    {
        if (current >= max)
            return "Full";
        else if (current >= max * 0.75)
            return "Heavy";
        else
            return "Normal";
    }

    /// <summary>
    /// Format weapon information for display
    /// </summary>
    private string FormatWeaponInfo(Equipment weapon)
    {
        string damageInfo = weapon.GetDamageDescription();
        string qualityTag = weapon.Quality >= QualityTier.ClanForged ? $"[{weapon.GetQualityName()}] " : "";

        return $"{qualityTag}{weapon.Name} ({damageInfo})";
    }

    /// <summary>
    /// Format armor information for display
    /// </summary>
    private string FormatArmorInfo(Equipment armor)
    {
        string qualityTag = armor.Quality >= QualityTier.ClanForged ? $"[{armor.GetQualityName()}] " : "";
        string defenseInfo = armor.DefenseBonus > 0 ? $"Def {armor.DefenseBonus}" : "";
        string hpInfo = armor.HPBonus > 0 ? $"+{armor.HPBonus} HP" : "";

        string stats = string.Join(", ", new[] { defenseInfo, hpInfo }.Where(s => !string.IsNullOrEmpty(s)));

        return $"{qualityTag}{armor.Name} ({stats})";
    }
}
