namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the result of gaining experience points.
/// </summary>
/// <remarks>
/// <para>Contains information about the XP gained and the player's new totals.
/// Level-up information will be added in v0.0.8b.</para>
/// </remarks>
/// <param name="AmountGained">The amount of XP gained.</param>
/// <param name="NewTotal">The player's new total XP.</param>
/// <param name="PreviousTotal">The player's XP before this gain.</param>
/// <param name="Source">Description of the XP source (e.g., monster name).</param>
public readonly record struct ExperienceGainResult(
    int AmountGained,
    int NewTotal,
    int PreviousTotal,
    string Source)
{
    /// <summary>
    /// Gets whether any experience was actually gained.
    /// </summary>
    public bool DidGainExperience => AmountGained > 0;

    /// <summary>
    /// Creates an experience gain result from defeating a monster.
    /// </summary>
    /// <param name="monsterName">The name of the defeated monster.</param>
    /// <param name="xpValue">The XP value of the monster.</param>
    /// <param name="previousTotal">The player's XP before the gain.</param>
    /// <param name="newTotal">The player's XP after the gain.</param>
    /// <returns>An ExperienceGainResult for the monster defeat.</returns>
    public static ExperienceGainResult FromMonsterDefeat(
        string monsterName,
        int xpValue,
        int previousTotal,
        int newTotal) =>
        new(xpValue, newTotal, previousTotal, $"Defeated {monsterName}");

    /// <summary>
    /// Creates a result indicating no experience was gained.
    /// </summary>
    /// <param name="currentTotal">The player's current XP total.</param>
    /// <returns>An ExperienceGainResult with zero gain.</returns>
    public static ExperienceGainResult None(int currentTotal) =>
        new(0, currentTotal, currentTotal, string.Empty);
}
