using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a modifier based on the target of a skill check.
/// </summary>
/// <remarks>
/// <para>
/// Target modifiers include:
/// <list type="bullet">
///   <item><description>Disposition (friendly to hostile)</description></item>
///   <item><description>Suspicion level (for deception checks)</description></item>
///   <item><description>Resistance level (target's opposed ability)</description></item>
///   <item><description>Relative strength (for intimidation)</description></item>
/// </list>
/// </para>
/// <para>
/// Target modifiers are primarily used for social skill checks (Rhetoric)
/// but may also apply to some combat maneuvers.
/// </para>
/// </remarks>
/// <param name="ModifierId">Unique identifier for this modifier.</param>
/// <param name="Name">Display name for the modifier.</param>
/// <param name="DiceModifier">Bonus or penalty to dice pool.</param>
/// <param name="DcModifier">Bonus or penalty to difficulty class.</param>
/// <param name="TargetId">Optional ID of the target NPC or entity.</param>
/// <param name="Disposition">Target's disposition toward the actor.</param>
/// <param name="SuspicionLevel">Target's suspicion level (0-10).</param>
/// <param name="ResistanceLevel">Target's resistance rating.</param>
/// <param name="Description">Optional flavor text.</param>
public readonly record struct TargetModifier(
    string ModifierId,
    string Name,
    int DiceModifier,
    int DcModifier,
    string? TargetId = null,
    Disposition? Disposition = null,
    int? SuspicionLevel = null,
    int? ResistanceLevel = null,
    string? Description = null) : ISkillModifier
{
    /// <summary>
    /// Gets the modifier category.
    /// </summary>
    public ModifierCategory Category => ModifierCategory.Target;

    /// <summary>
    /// Creates a disposition modifier for persuasion and social checks.
    /// </summary>
    /// <param name="disposition">The target's disposition.</param>
    /// <param name="targetId">Optional target identifier.</param>
    /// <returns>A new target modifier with appropriate dice adjustment.</returns>
    public static TargetModifier FromDisposition(Disposition disposition, string? targetId = null)
    {
        var (dice, name) = disposition switch
        {
            Enums.Disposition.Friendly => (2, "Friendly Disposition"),
            Enums.Disposition.Neutral => (0, "Neutral Disposition"),
            Enums.Disposition.Suspicious => (-2, "Suspicious Disposition"),
            Enums.Disposition.Hostile => (-2, "Hostile Disposition"),
            _ => (0, "Unknown Disposition")
        };

        return new TargetModifier(
            $"disposition-{disposition.ToString().ToLowerInvariant()}",
            name,
            DiceModifier: dice,
            DcModifier: 0,
            TargetId: targetId,
            Disposition: disposition);
    }

    /// <summary>
    /// Creates a suspicion modifier for deception checks.
    /// </summary>
    /// <param name="level">Suspicion level (0-10, where 0 = trusting, 10 = extremely suspicious).</param>
    /// <param name="targetId">Optional target identifier.</param>
    /// <returns>A new target modifier with appropriate DC adjustment.</returns>
    public static TargetModifier FromSuspicion(int level, string? targetId = null)
    {
        var clampedLevel = Math.Clamp(level, 0, 10);

        // Suspicion adds to DC: level 0-3 = +0, 4-6 = +2, 7-9 = +4, 10 = +6
        var dcMod = clampedLevel switch
        {
            <= 3 => 0,
            <= 6 => 2,
            <= 9 => 4,
            _ => 6
        };

        var name = clampedLevel switch
        {
            <= 3 => "Trusting Target",
            <= 6 => "Wary Target",
            <= 9 => "Suspicious Target",
            _ => "Extremely Suspicious"
        };

        return new TargetModifier(
            $"suspicion-{clampedLevel}",
            name,
            DiceModifier: 0,
            DcModifier: dcMod,
            TargetId: targetId,
            SuspicionLevel: clampedLevel);
    }

    /// <summary>
    /// Creates a relative strength modifier for intimidation checks.
    /// </summary>
    /// <param name="targetIsStronger">Whether the target appears stronger than the actor.</param>
    /// <param name="targetId">Optional target identifier.</param>
    /// <returns>A new target modifier with appropriate dice adjustment.</returns>
    public static TargetModifier FromRelativeStrength(bool targetIsStronger, string? targetId = null)
    {
        return new TargetModifier(
            targetIsStronger ? "stronger-target" : "weaker-target",
            targetIsStronger ? "Stronger Target" : "Weaker Target",
            DiceModifier: targetIsStronger ? -1 : 1,
            DcModifier: 0,
            TargetId: targetId,
            Description: targetIsStronger ? "Target is more imposing" : "Target seems weaker");
    }

    /// <summary>
    /// Creates a resistance modifier based on target's opposed attribute.
    /// </summary>
    /// <param name="resistanceValue">Target's relevant attribute or resistance value.</param>
    /// <param name="targetId">Optional target identifier.</param>
    /// <returns>A new target modifier with appropriate DC adjustment.</returns>
    public static TargetModifier FromResistance(int resistanceValue, string? targetId = null)
    {
        // High resistance adds to DC
        var dcMod = resistanceValue switch
        {
            <= 1 => -1,   // Very low resistance
            <= 3 => 0,    // Normal resistance
            <= 5 => 1,    // Above average
            <= 7 => 2,    // High resistance
            _ => 3        // Very high resistance
        };

        return new TargetModifier(
            $"resistance-{resistanceValue}",
            $"Target Resistance ({resistanceValue})",
            DiceModifier: 0,
            DcModifier: dcMod,
            TargetId: targetId,
            ResistanceLevel: resistanceValue);
    }

    /// <summary>
    /// Returns a short description for UI display.
    /// </summary>
    public string ToShortDescription()
    {
        var parts = new List<string> { Name };

        if (DiceModifier != 0)
        {
            var diceStr = DiceModifier > 0 ? $"+{DiceModifier}d10" : $"{DiceModifier}d10";
            parts.Add($"({diceStr})");
        }

        if (DcModifier != 0)
        {
            var dcStr = DcModifier > 0 ? $"DC +{DcModifier}" : $"DC {DcModifier}";
            parts.Add($"({dcStr})");
        }

        return string.Join(" ", parts);
    }

    /// <inheritdoc/>
    public override string ToString() => ToShortDescription();
}
