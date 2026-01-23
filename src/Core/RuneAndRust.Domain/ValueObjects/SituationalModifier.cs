using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a modifier from temporary or situational conditions.
/// </summary>
/// <remarks>
/// <para>
/// Situational modifiers include:
/// <list type="bullet">
///   <item><description>Time pressure (rushed vs. taking extra time)</description></item>
///   <item><description>Familiarity with the task</description></item>
///   <item><description>Distraction from combat or danger</description></item>
///   <item><description>Assistance from other characters</description></item>
///   <item><description>Fatigue or injury penalties</description></item>
/// </list>
/// </para>
/// <para>
/// Situational modifiers have a duration and may or may not stack with
/// other modifiers of the same type.
/// </para>
/// </remarks>
/// <param name="ModifierId">Unique identifier for this modifier type.</param>
/// <param name="Name">Display name for the modifier.</param>
/// <param name="DiceModifier">Bonus or penalty to dice pool.</param>
/// <param name="DcModifier">Bonus or penalty to difficulty class.</param>
/// <param name="Source">What caused this modifier (for logging/display).</param>
/// <param name="Duration">How long this modifier persists.</param>
/// <param name="IsStackable">Whether multiple instances can apply.</param>
/// <param name="Description">Optional flavor text.</param>
public readonly record struct SituationalModifier(
    string ModifierId,
    string Name,
    int DiceModifier,
    int DcModifier,
    string Source,
    ModifierDuration Duration = ModifierDuration.Instant,
    bool IsStackable = false,
    string? Description = null) : ISkillModifier
{
    /// <summary>
    /// Gets the modifier category.
    /// </summary>
    public ModifierCategory Category => ModifierCategory.Situational;

    /// <summary>
    /// Creates a time pressure modifier (rushed attempt).
    /// </summary>
    /// <param name="source">Source of the time pressure.</param>
    /// <returns>A new situational modifier with -1d10.</returns>
    public static SituationalModifier TimePressure(string source = "Combat nearby")
    {
        return new SituationalModifier(
            "time-pressure",
            "Time Pressure",
            DiceModifier: -1,
            DcModifier: 0,
            source,
            ModifierDuration.Scene);
    }

    /// <summary>
    /// Creates a taking-time modifier (extra care).
    /// </summary>
    /// <param name="source">Description of extra time taken.</param>
    /// <returns>A new situational modifier with +1d10.</returns>
    public static SituationalModifier TakingTime(string source = "Taking extra time")
    {
        return new SituationalModifier(
            "taking-time",
            "Taking Time",
            DiceModifier: 1,
            DcModifier: 0,
            source,
            ModifierDuration.Instant);
    }

    /// <summary>
    /// Creates a familiarity modifier.
    /// </summary>
    /// <param name="taskDescription">Description of the familiar task.</param>
    /// <returns>A new situational modifier with +1d10.</returns>
    public static SituationalModifier Familiarity(string taskDescription)
    {
        return new SituationalModifier(
            "familiarity",
            "Familiarity",
            DiceModifier: 1,
            DcModifier: 0,
            $"Familiar with {taskDescription}",
            ModifierDuration.Persistent);
    }

    /// <summary>
    /// Creates a distraction modifier.
    /// </summary>
    /// <param name="distractionSource">What is causing the distraction.</param>
    /// <returns>A new situational modifier with -1d10.</returns>
    public static SituationalModifier Distracted(string distractionSource)
    {
        return new SituationalModifier(
            "distracted",
            "Distracted",
            DiceModifier: -1,
            DcModifier: 0,
            distractionSource,
            ModifierDuration.Scene);
    }

    /// <summary>
    /// Creates an assistance modifier from helpers.
    /// </summary>
    /// <param name="helperCount">Number of characters assisting.</param>
    /// <param name="helperName">Name of primary helper (for description).</param>
    /// <returns>A new situational modifier with bonus dice (capped at +3d10).</returns>
    public static SituationalModifier Assisted(int helperCount, string helperName)
    {
        var diceBonus = Math.Min(helperCount, 3); // Cap at +3d10 from assistance
        return new SituationalModifier(
            "assisted",
            "Assisted",
            DiceModifier: diceBonus,
            DcModifier: 0,
            $"Assisted by {helperName}{(helperCount > 1 ? $" (+{helperCount - 1} others)" : "")}",
            ModifierDuration.Instant,
            IsStackable: false);
    }

    /// <summary>
    /// Creates a fatigue modifier.
    /// </summary>
    /// <returns>A new situational modifier with -2d10.</returns>
    public static SituationalModifier Fatigued()
    {
        return new SituationalModifier(
            "fatigued",
            "Fatigued",
            DiceModifier: -2,
            DcModifier: 0,
            "Exhaustion",
            ModifierDuration.Persistent);
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
