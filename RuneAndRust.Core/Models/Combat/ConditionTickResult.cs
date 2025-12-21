namespace RuneAndRust.Core.Models.Combat;

/// <summary>
/// Result of processing an ambient condition tick effect on a combatant (v0.3.3b).
/// </summary>
/// <param name="WasApplied">Whether the tick effect was actually applied (can be false due to chance rolls).</param>
/// <param name="ConditionName">The name of the condition that triggered.</param>
/// <param name="Message">Display message describing what happened.</param>
/// <param name="DamageDealt">Amount of damage dealt by the tick, if any.</param>
/// <param name="StressApplied">Amount of stress applied by the tick, if any.</param>
/// <param name="CorruptionApplied">Amount of corruption applied by the tick, if any.</param>
public record ConditionTickResult(
    bool WasApplied,
    string ConditionName,
    string Message,
    int DamageDealt,
    int StressApplied,
    int CorruptionApplied
)
{
    /// <summary>
    /// Returns a result indicating no effect was applied.
    /// </summary>
    public static ConditionTickResult None => new(false, string.Empty, string.Empty, 0, 0, 0);
}
