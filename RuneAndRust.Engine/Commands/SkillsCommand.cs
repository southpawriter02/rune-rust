using System.Text;
using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine.Commands;

/// <summary>
/// v0.37.4: Skills Command
/// Displays skill sheet with abilities and proficiencies.
/// Syntax: skills
/// Note: Formal skill system not yet implemented; displays abilities for now
/// </summary>
public class SkillsCommand : ICommand
{
    private static readonly ILogger _log = Log.ForContext<SkillsCommand>();

    public CommandResult Execute(GameState state, string[] args)
    {
        if (state.Player == null)
        {
            _log.Warning("Skills command failed: Player is null");
            return CommandResult.Failure("Player not found.");
        }

        _log.Information(
            "Skills command: CharacterId={CharacterId}",
            state.Player.CharacterID);

        var output = new StringBuilder();

        // Header
        output.AppendLine("╔════════════════════════════════════════╗");
        output.AppendLine("║ Skills & Proficiencies                 ║");
        output.AppendLine("╠════════════════════════════════════════╣");

        // Class-based abilities (proxy for skills until formal skill system)
        var abilities = state.Player.Abilities;

        if (abilities.Any())
        {
            // Group abilities by type/category
            var combatAbilities = abilities.Where(a => IsCombatAbility(a)).ToList();
            var supportAbilities = abilities.Where(a => IsSupportAbility(a)).ToList();
            var passiveAbilities = abilities.Where(a => IsPassiveAbility(a)).ToList();

            // Combat Abilities
            if (combatAbilities.Any())
            {
                output.AppendLine("║ COMBAT ABILITIES:                      ║");
                foreach (var ability in combatAbilities.Take(3))
                {
                    string abilityLine = $" {ability.Name}";
                    if (abilityLine.Length > 38)
                        abilityLine = abilityLine.Substring(0, 38);

                    output.AppendLine($"║ {abilityLine.PadRight(38)} ║");
                }
                output.AppendLine("║                                        ║");
            }

            // Support Abilities
            if (supportAbilities.Any())
            {
                output.AppendLine("║ SUPPORT ABILITIES:                     ║");
                foreach (var ability in supportAbilities.Take(3))
                {
                    string abilityLine = $" {ability.Name}";
                    if (abilityLine.Length > 38)
                        abilityLine = abilityLine.Substring(0, 38);

                    output.AppendLine($"║ {abilityLine.PadRight(38)} ║");
                }
                output.AppendLine("║                                        ║");
            }

            // Passive Abilities
            if (passiveAbilities.Any())
            {
                output.AppendLine("║ PASSIVE ABILITIES:                     ║");
                foreach (var ability in passiveAbilities.Take(3))
                {
                    string abilityLine = $" {ability.Name}";
                    if (abilityLine.Length > 38)
                        abilityLine = abilityLine.Substring(0, 38);

                    output.AppendLine($"║ {abilityLine.PadRight(38)} ║");
                }
                output.AppendLine("║                                        ║");
            }

            // Total count
            string totalLine = $"Total Abilities: {abilities.Count}";
            output.AppendLine($"║ {totalLine.PadRight(38)} ║");
        }
        else
        {
            output.AppendLine("║ No abilities learned yet.              ║");
            output.AppendLine("║                                        ║");
            output.AppendLine("║ Gain abilities by:                     ║");
            output.AppendLine("║  - Advancing milestones                ║");
            output.AppendLine("║  - Choosing specializations            ║");
            output.AppendLine("║  - Completing quests                   ║");
        }

        output.AppendLine("╚════════════════════════════════════════╝");
        output.AppendLine();
        output.AppendLine("Note: Formal skill system pending. This displays abilities.");

        _log.Information(
            "Skills displayed: AbilityCount={AbilityCount}",
            abilities.Count);

        return CommandResult.Success(output.ToString());
    }

    /// <summary>
    /// Check if ability is a combat ability (offensive)
    /// </summary>
    private bool IsCombatAbility(Ability ability)
    {
        // Combat abilities typically have stamina/AP cost and target enemies
        return ability.StaminaCost > 0 || ability.APCost > 0;
    }

    /// <summary>
    /// Check if ability is a support ability (healing, buffs)
    /// </summary>
    private bool IsSupportAbility(Ability ability)
    {
        // Support abilities often have "heal", "shield", "buff" in name
        var supportKeywords = new[] { "heal", "shield", "buff", "restore", "protect", "guard" };
        return supportKeywords.Any(keyword =>
            ability.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Check if ability is passive (no resource cost)
    /// </summary>
    private bool IsPassiveAbility(Ability ability)
    {
        // Passive abilities have no cost and are always active
        return ability.StaminaCost == 0 && ability.APCost == 0 &&
               !IsSupportAbility(ability);
    }
}
