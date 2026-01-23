namespace RuneAndRust.Domain.Extensions;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Extension methods for <see cref="GantryRunnerAbility"/> enum.
/// </summary>
/// <remarks>
/// Provides display names, ability types, descriptions, and usage information
/// for all Gantry-Runner specialization abilities.
/// </remarks>
public static class GantryRunnerAbilityExtensions
{
    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the display name for this ability.
    /// </summary>
    /// <param name="ability">The ability.</param>
    /// <returns>Human-readable display name with brackets.</returns>
    public static string GetDisplayName(this GantryRunnerAbility ability)
    {
        return ability switch
        {
            GantryRunnerAbility.RoofRunner => "[Roof-Runner]",
            GantryRunnerAbility.DeathDefyingLeap => "[Death-Defying Leap]",
            GantryRunnerAbility.WallRun => "[Wall-Run]",
            GantryRunnerAbility.DoubleJump => "[Double Jump]",
            GantryRunnerAbility.Featherfall => "[Featherfall]",
            _ => "[Unknown Gantry-Runner Ability]"
        };
    }

    /// <summary>
    /// Gets the ability type for this ability.
    /// </summary>
    /// <param name="ability">The ability.</param>
    /// <returns>The ability activation type.</returns>
    public static SpecializationAbilityType GetAbilityType(this GantryRunnerAbility ability)
    {
        return ability switch
        {
            GantryRunnerAbility.RoofRunner => SpecializationAbilityType.Passive,
            GantryRunnerAbility.DeathDefyingLeap => SpecializationAbilityType.Passive,
            GantryRunnerAbility.WallRun => SpecializationAbilityType.Active,
            GantryRunnerAbility.DoubleJump => SpecializationAbilityType.Reactive,
            GantryRunnerAbility.Featherfall => SpecializationAbilityType.Passive,
            _ => SpecializationAbilityType.Passive
        };
    }

    /// <summary>
    /// Gets the description of this ability's effect.
    /// </summary>
    /// <param name="ability">The ability.</param>
    /// <returns>Description of the ability effect.</returns>
    public static string GetDescription(this GantryRunnerAbility ability)
    {
        return ability switch
        {
            GantryRunnerAbility.RoofRunner =>
                "Reduce climbing stages required by 1 (minimum 1 stage).",
            GantryRunnerAbility.DeathDefyingLeap =>
                "Add +10 ft to your maximum leap distance.",
            GantryRunnerAbility.WallRun =>
                "Spend 1 action to run vertically up a wall (DC 3 Acrobatics).",
            GantryRunnerAbility.DoubleJump =>
                "Once per day, reroll a failed leap check with +1d10 bonus.",
            GantryRunnerAbility.Featherfall =>
                "Auto-succeed Crash Landing checks with DC ≤ 3.",
            _ => "Unknown ability effect."
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // USAGE METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this ability has limited daily uses.
    /// </summary>
    /// <param name="ability">The ability.</param>
    /// <returns>True if the ability has daily use limits.</returns>
    public static bool HasDailyLimit(this GantryRunnerAbility ability)
    {
        return ability == GantryRunnerAbility.DoubleJump;
    }

    /// <summary>
    /// Gets the daily use limit for this ability.
    /// </summary>
    /// <param name="ability">The ability.</param>
    /// <returns>Number of daily uses, or -1 for unlimited.</returns>
    public static int GetDailyUses(this GantryRunnerAbility ability)
    {
        return ability switch
        {
            GantryRunnerAbility.DoubleJump => 1,
            _ => -1 // Unlimited
        };
    }

    /// <summary>
    /// Gets whether this ability requires an action to use.
    /// </summary>
    /// <param name="ability">The ability.</param>
    /// <returns>True if the ability costs an action.</returns>
    public static bool RequiresAction(this GantryRunnerAbility ability)
    {
        return ability == GantryRunnerAbility.WallRun;
    }

    /// <summary>
    /// Gets the string ID for configuration lookups.
    /// </summary>
    /// <param name="ability">The ability.</param>
    /// <returns>The kebab-case ability ID.</returns>
    public static string GetAbilityId(this GantryRunnerAbility ability)
    {
        return ability switch
        {
            GantryRunnerAbility.RoofRunner => "roof-runner",
            GantryRunnerAbility.DeathDefyingLeap => "death-defying-leap",
            GantryRunnerAbility.WallRun => "wall-run",
            GantryRunnerAbility.DoubleJump => "double-jump",
            GantryRunnerAbility.Featherfall => "featherfall",
            _ => "unknown"
        };
    }
}
