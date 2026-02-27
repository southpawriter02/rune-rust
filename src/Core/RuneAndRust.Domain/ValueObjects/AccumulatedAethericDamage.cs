namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Tracks accumulated Aetheric damage dealt by the Seiðkona for the Unraveling capstone.
/// Returns new instances for all modifications (immutable pattern).
/// </summary>
/// <remarks>
/// <para>Accumulated Aetheric Damage is a separate tracker from regular combat damage.
/// It records all Aetheric damage output (primarily from Seiðr Bolt) across an encounter
/// or session, serving as the damage pool for the Unraveling capstone ability (v0.20.8c).</para>
/// <list type="bullet">
/// <item>Built from: Seiðr Bolt damage rolls, future T2/T3 Aetheric abilities</item>
/// <item>NOT applied to enemies directly — stored until Unraveling is triggered</item>
/// <item>Reset by: Unraveling capstone (deals all accumulated as single burst) or character death</item>
/// <item>Tracked separately from Aether Resonance — Resonance measures attunement,
///   this measures raw destructive potential</item>
/// </list>
/// <para>Uses immutable <c>init</c> properties (like <see cref="MedicalSuppliesResource"/>)
/// because damage accumulation changes less frequently than Resonance and its primary
/// read point is the Unraveling capstone check. All modification methods return new instances.</para>
/// </remarks>
public sealed record AccumulatedAethericDamage
{
    /// <summary>
    /// Total accumulated Aetheric damage across all qualifying casts.
    /// This is the damage pool available to the Unraveling capstone.
    /// </summary>
    public int TotalDamage { get; init; }

    /// <summary>
    /// Number of casts that contributed to the accumulated damage total.
    /// Used for averaging and display purposes.
    /// </summary>
    public int CastCount { get; init; }

    /// <summary>
    /// UTC timestamp of the last cast that contributed to accumulation.
    /// Null if no damage has been accumulated yet.
    /// </summary>
    public DateTime? LastDamageAt { get; init; }

    /// <summary>
    /// Creates a new AccumulatedAethericDamage tracker with zero damage.
    /// Used when initializing a new Seiðkona character.
    /// </summary>
    /// <returns>A new tracker initialized to 0 damage, 0 casts.</returns>
    public static AccumulatedAethericDamage Create()
    {
        return new AccumulatedAethericDamage
        {
            TotalDamage = 0,
            CastCount = 0,
            LastDamageAt = null
        };
    }

    /// <summary>
    /// Adds damage from a single Aetheric cast (e.g., Seiðr Bolt), returning a new instance.
    /// </summary>
    /// <param name="damage">The damage dealt in this cast. Must be non-negative.</param>
    /// <returns>A new <see cref="AccumulatedAethericDamage"/> with the damage added.</returns>
    /// <remarks>
    /// Zero-damage casts are still counted (cast count increments) to track total attempts.
    /// Negative damage values are treated as zero to prevent data corruption.
    /// </remarks>
    public AccumulatedAethericDamage AddDamage(int damage)
    {
        var safeDamage = Math.Max(damage, 0);

        return new AccumulatedAethericDamage
        {
            TotalDamage = TotalDamage + safeDamage,
            CastCount = CastCount + 1,
            LastDamageAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Resets accumulated damage to zero, returning a new instance.
    /// Called by the Unraveling capstone after releasing all accumulated damage,
    /// or upon character death.
    /// </summary>
    /// <returns>A new tracker initialized to 0 damage, 0 casts.</returns>
    public AccumulatedAethericDamage Reset()
    {
        return Create();
    }

    /// <summary>
    /// Gets whether the Unraveling capstone can be used (requires at least 1 accumulated damage).
    /// </summary>
    public bool CanUnravel => TotalDamage > 0;

    /// <summary>
    /// Gets the average damage per cast for display purposes.
    /// Returns 0 if no casts have been recorded.
    /// </summary>
    /// <returns>The average damage per cast as a decimal value.</returns>
    public decimal GetAverageDamagePerCast()
    {
        if (CastCount == 0)
            return 0m;

        return (decimal)TotalDamage / CastCount;
    }

    /// <summary>
    /// Gets a formatted display string for the accumulated damage tracker.
    /// </summary>
    /// <returns>
    /// A string in the format "Accumulated: 42 Aetheric damage (6 casts, avg 7.0/cast)"
    /// or "Accumulated: 0 Aetheric damage (no casts)" if empty.
    /// </returns>
    public string GetFormattedValue()
    {
        if (CastCount == 0)
            return "Accumulated: 0 Aetheric damage (no casts)";

        return $"Accumulated: {TotalDamage} Aetheric damage " +
               $"({CastCount} casts, avg {GetAverageDamagePerCast():F1}/cast)";
    }
}
