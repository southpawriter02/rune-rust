namespace RuneAndRust.Core.Enums;

/// <summary>
/// Represents the outcome of a Breaking Point resolve check.
/// Determines what happens when a character's stress reaches 100.
/// </summary>
public enum BreakingPointOutcome
{
    /// <summary>
    /// Character succeeded in the resolve check (3+ successes).
    /// Stress resets to 75. Character gains temporary Disoriented status.
    /// No permanent trauma acquired.
    /// </summary>
    Stabilized = 0,

    /// <summary>
    /// Character failed the resolve check (less than 3 successes, no fumble).
    /// Stress resets to 50. Character acquires a permanent Trauma.
    /// </summary>
    Trauma = 1,

    /// <summary>
    /// Character fumbled the resolve check (0 successes AND botches).
    /// Stress resets to 50. Character acquires a permanent Trauma AND Stunned status.
    /// Represents total psychological collapse.
    /// </summary>
    Catastrophe = 2
}
