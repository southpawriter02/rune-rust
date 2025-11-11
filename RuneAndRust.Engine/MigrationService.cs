using RuneAndRust.Core;
using RuneAndRust.Core.Archetypes;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.7.1: Migration service for updating existing characters to new archetype system
/// </summary>
public class MigrationService
{
    /// <summary>
    /// Migrate a Warrior character from v0.1-v0.6 to v0.7.1 archetype system
    /// </summary>
    public static bool MigrateWarriorToV071(PlayerCharacter character)
    {
        // Only migrate Warriors
        if (character.Class != CharacterClass.Warrior)
        {
            return false;
        }

        // Check if already migrated (has Strike ability)
        if (character.Abilities.Any(a => a.Name == "Strike"))
        {
            return false; // Already migrated
        }

        // Create archetype instance if not present
        if (character.Archetype == null)
        {
            character.Archetype = new WarriorArchetype();
        }

        // Initialize stance system if not present
        if (character.ActiveStance == null)
        {
            character.ActiveStance = Stance.CreateBalancedStance();
        }

        // Remove deprecated abilities (Power Strike, Shield Wall, Cleaving Strike, Battle Rage)
        var deprecatedAbilities = new[] { "Power Strike", "Shield Wall", "Cleaving Strike", "Battle Rage" };
        character.Abilities.RemoveAll(a => deprecatedAbilities.Contains(a.Name));

        // Grant new starting abilities
        var warriorArchetype = new WarriorArchetype();
        var startingAbilities = warriorArchetype.GetStartingAbilities();
        character.Abilities.AddRange(startingAbilities);

        // Recalculate stats to apply Warrior's Vigor
        var equipmentService = new EquipmentService();
        equipmentService.RecalculatePlayerStats(character);

        return true; // Migration successful
    }

    /// <summary>
    /// Check if a character needs migration
    /// </summary>
    public static bool NeedsMigration(PlayerCharacter character)
    {
        // Warriors without Strike ability need migration
        if (character.Class == CharacterClass.Warrior)
        {
            return !character.Abilities.Any(a => a.Name == "Strike");
        }

        return false;
    }

    /// <summary>
    /// Get migration report showing what will change
    /// </summary>
    public static string GetMigrationReport(PlayerCharacter character)
    {
        if (!NeedsMigration(character))
        {
            return "No migration needed.";
        }

        var report = "=== Warrior v0.7.1 Migration ===\n\n";
        report += "REMOVED Abilities:\n";

        var deprecatedAbilities = new[] { "Power Strike", "Shield Wall", "Cleaving Strike", "Battle Rage" };
        foreach (var abilityName in deprecatedAbilities)
        {
            if (character.Abilities.Any(a => a.Name == abilityName))
            {
                report += $"  - {abilityName}\n";
            }
        }

        report += "\nADDED Abilities:\n";
        report += "  + Strike (10 Stamina) - Standard weapon attack\n";
        report += "  + Defensive Stance (0 Stamina) - +3 Soak, -25% damage, stance\n";
        report += "  + Warrior's Vigor (Passive) - +10% Max HP\n";

        report += "\nOther Changes:\n";
        report += "  + Stance system initialized (Balanced Stance)\n";
        report += "  + Max HP recalculated with Warrior's Vigor bonus\n";

        return report;
    }
}
