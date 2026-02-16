namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the outcome of a Triage passive ability evaluation.
/// Records the most wounded target identification and bonus healing calculation.
/// </summary>
/// <remarks>
/// <para>Triage is the Bone-Setter's Tier 2 passive ability:</para>
/// <list type="bullet">
/// <item>Always active when unlocked (no AP or supply cost)</item>
/// <item>Evaluates all allies within 5-space radius</item>
/// <item>Identifies the most wounded ally by HP percentage</item>
/// <item>Grants +50% healing bonus (×1.5 multiplier) when healing that target</item>
/// <item>Corruption: None (Coherent path)</item>
/// </list>
/// <para>This result is used by other healing abilities (Field Dressing, Emergency Surgery)
/// to determine if additional Triage healing should be applied to their target.</para>
/// </remarks>
public sealed record TriageResult
{
    /// <summary>
    /// Unique identifier of the most wounded ally within Triage radius.
    /// </summary>
    public Guid MostWoundedTargetId { get; init; }

    /// <summary>
    /// Display name of the most wounded ally.
    /// </summary>
    public string MostWoundedTargetName { get; init; } = string.Empty;

    /// <summary>
    /// HP percentage of the most wounded ally (0.0 to 1.0).
    /// The ally with the lowest HP percentage receives the Triage bonus.
    /// </summary>
    public float MostWoundedHpPercentage { get; init; }

    /// <summary>
    /// Total number of allied targets evaluated within the 5-space Triage radius.
    /// </summary>
    public int TargetsInRadius { get; init; }

    /// <summary>
    /// The base healing amount before Triage bonus is applied.
    /// This is the healing total from the executing ability (e.g., Field Dressing or Emergency Surgery).
    /// </summary>
    public int BaseHealing { get; init; }

    /// <summary>
    /// Bonus healing amount from Triage passive = <c>(int)(BaseHealing * 0.5f)</c>.
    /// Represents the additional 50% healing granted by Triage.
    /// </summary>
    public int BonusHealing => (int)(BaseHealing * 0.5f);

    /// <summary>
    /// Total healing after Triage bonus = BaseHealing + BonusHealing.
    /// </summary>
    public int TotalHealing => BaseHealing + BonusHealing;

    /// <summary>
    /// Gets a formatted summary of the Triage bonus application for combat log display.
    /// </summary>
    /// <returns>
    /// A formatted string such as "Ranger receives Triage bonus: 9 × 1.5 = 13 HP (+4)"
    /// </returns>
    public string GetBonusSummary() =>
        $"{MostWoundedTargetName} receives Triage bonus: " +
        $"{BaseHealing} × 1.5 = {TotalHealing} HP (+{BonusHealing})";
}
