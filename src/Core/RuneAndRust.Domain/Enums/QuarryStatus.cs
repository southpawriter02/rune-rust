namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Status states for a Quarry Mark placed by the Veiðimaðr (Hunter) specialization.
/// Tracks the lifecycle of individual marks from creation through resolution.
/// </summary>
/// <remarks>
/// <para>Quarry Marks transition through states as follows:</para>
/// <list type="bullet">
/// <item><see cref="Active"/> — Mark is providing hit bonuses to the hunter</item>
/// <item><see cref="Defeated"/> — Target was killed or incapacitated; mark auto-clears</item>
/// <item><see cref="Escaped"/> — Target fled beyond tracking range; mark auto-clears</item>
/// <item><see cref="Expired"/> — Mark duration ended (Tier 2+ feature) or encounter ended</item>
/// <item><see cref="Unmarked"/> — Target has no active mark (default/cleared state)</item>
/// </list>
/// <para>Introduced in v0.20.7a as part of the Veiðimaðr specialization framework.</para>
/// </remarks>
public enum QuarryStatus
{
    /// <summary>Mark is active and providing +2 hit bonus to the hunter against this target.</summary>
    Active = 0,

    /// <summary>Marked target has been defeated (killed or incapacitated); mark is no longer providing bonuses.</summary>
    Defeated = 1,

    /// <summary>Marked target has escaped beyond tracking range; mark is no longer providing bonuses.</summary>
    Escaped = 2,

    /// <summary>Mark has expired due to turn count limit (Tier 2+ feature) or encounter end.</summary>
    Expired = 3,

    /// <summary>Target is not currently marked. Default state before marking or after mark removal.</summary>
    Unmarked = 4
}
