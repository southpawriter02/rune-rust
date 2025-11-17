using System.Text;
using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine.Commands;

/// <summary>
/// v0.37.3: Equipment Command
/// Displays currently equipped gear and stats.
/// Syntax: equipment (aliases: eq)
/// </summary>
public class EquipmentCommand : ICommand
{
    private static readonly ILogger _log = Log.ForContext<EquipmentCommand>();
    private readonly EquipmentService _equipmentService;

    public EquipmentCommand(EquipmentService equipmentService)
    {
        _equipmentService = equipmentService;
    }

    public CommandResult Execute(GameState state, string[] args)
    {
        if (state.Player == null)
        {
            _log.Warning("Equipment command failed: Player is null");
            return CommandResult.Failure("Player not found.");
        }

        _log.Information(
            "Equipment command: CharacterId={CharacterId}",
            state.Player.CharacterID);

        var output = new StringBuilder();

        // Header
        output.AppendLine("╔══════════════════════════════════════╗");
        output.AppendLine("║ Your Loadout                         ║");
        output.AppendLine("╠══════════════════════════════════════╣");

        // Main Hand (Weapon)
        string mainHandInfo = FormatEquipmentSlot("MainHand", state.Player.EquippedWeapon);
        output.AppendLine($"║ {mainHandInfo.PadRight(36)} ║");

        // Off Hand (future implementation for dual-wield/shields)
        string offHandInfo = "OffHand:   (empty)";
        output.AppendLine($"║ {offHandInfo.PadRight(36)} ║");

        // Armor
        string armorInfo = FormatEquipmentSlot("Armor", state.Player.EquippedArmor);
        output.AppendLine($"║ {armorInfo.PadRight(36)} ║");

        output.AppendLine("║                                      ║");

        // Stats display
        output.AppendLine("║ STATS:                               ║");

        // HP and Stamina
        string hpLine = $" HP:      {state.Player.HP,3}/{state.Player.MaxHP,-3}";
        string staminaLine = $" Stamina:  {state.Player.Stamina,3}/{state.Player.MaxStamina,-3}";
        output.AppendLine($"║{hpLine.PadRight(38)}║");
        output.AppendLine($"║{staminaLine.PadRight(38)}║");

        // Defense (from armor + temp bonuses)
        int totalDefense = _equipmentService.GetTotalDefenseBonus(state.Player) + state.Player.DefenseBonus;
        string defenseLine = $" Defense:  {totalDefense}";
        output.AppendLine($"║{defenseLine.PadRight(38)}║");

        // Accuracy (from weapon)
        int accuracy = _equipmentService.GetTotalAccuracyBonus(state.Player);
        if (accuracy != 0)
        {
            string accuracyLine = $" Accuracy: {(accuracy > 0 ? "+" : "")}{accuracy}";
            output.AppendLine($"║{accuracyLine.PadRight(38)}║");
        }

        // Aether Pool (for Mystics)
        if (state.Player.Class == CharacterClass.Mystic && state.Player.MaxAP > 0)
        {
            string aetherLine = $" Aether:   {state.Player.AP,3}/{state.Player.MaxAP,-3}";
            output.AppendLine($"║{aetherLine.PadRight(38)}║");
        }

        output.AppendLine("╚══════════════════════════════════════╝");
        output.AppendLine();
        output.AppendLine("Commands: equip [item], unequip [weapon/armor]");

        _log.Information(
            "Equipment displayed: Weapon={Weapon}, Armor={Armor}",
            state.Player.EquippedWeapon?.Name ?? "None",
            state.Player.EquippedArmor?.Name ?? "None");

        return CommandResult.Success(output.ToString());
    }

    /// <summary>
    /// Format equipment slot information for display
    /// </summary>
    private string FormatEquipmentSlot(string slotName, Equipment? equipment)
    {
        if (equipment == null)
        {
            return $"{slotName}:  (empty)";
        }

        // Format quality tag for Clan-Forged and above
        string qualityTag = equipment.Quality >= QualityTier.ClanForged ? $"[{equipment.GetQualityName()}] " : "";

        // Format stats based on equipment type
        string stats = "";
        if (equipment.Type == EquipmentType.Weapon)
        {
            stats = $"({equipment.GetDamageDescription()})";
        }
        else if (equipment.Type == EquipmentType.Armor)
        {
            List<string> armorStats = new();
            if (equipment.DefenseBonus > 0)
                armorStats.Add($"Def {equipment.DefenseBonus}");
            if (equipment.HPBonus > 0)
                armorStats.Add($"+{equipment.HPBonus} HP");

            if (armorStats.Any())
                stats = $"({string.Join(", ", armorStats)})";
        }

        return $"{slotName}:  {qualityTag}{equipment.Name} {stats}".TrimEnd();
    }
}
