namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the result of a cascade check.
/// </summary>
/// <remarks>
/// <para>
/// Cascades occur when Arcanists cast at low coherence (Destabilized or Unstable),
/// causing magical backlash with various effects.
/// </para>
/// <para>
/// Cascade effects can include:
/// <list type="bullet">
/// <item>Coherence loss (always occurs on cascade)</item>
/// <item>Self-damage from magical backlash</item>
/// <item>Stress gain from reality instability</item>
/// <item>Corruption gain from wild magic exposure</item>
/// <item>Spell disruption (spell fails to complete)</item>
/// </list>
/// </para>
/// </remarks>
/// <param name="CascadeTriggered">Whether cascade actually occurred.</param>
/// <param name="CoherenceLost">Amount of coherence lost due to cascade.</param>
/// <param name="SelfDamage">Damage taken by Arcanist from magical backlash, if any.</param>
/// <param name="StressGained">Stress gained from cascade, if any.</param>
/// <param name="CorruptionGained">Corruption gained from wild magic exposure, if any.</param>
/// <param name="SpellDisrupted">Whether the spell was disrupted and failed to complete.</param>
/// <param name="CascadeEffectId">ID of cascade effect table entry for v0.19.x integration.</param>
public record CascadeResult(
    bool CascadeTriggered,
    int CoherenceLost,
    int? SelfDamage,
    int? StressGained,
    int? CorruptionGained,
    bool SpellDisrupted,
    string? CascadeEffectId)
{
    /// <summary>
    /// Gets a cascade result representing no cascade occurring.
    /// </summary>
    public static CascadeResult NoCascade => new(
        CascadeTriggered: false,
        CoherenceLost: 0,
        SelfDamage: null,
        StressGained: null,
        CorruptionGained: null,
        SpellDisrupted: false,
        CascadeEffectId: null);

    /// <summary>
    /// Gets whether cascade caused direct damage.
    /// </summary>
    public bool CausesDamage => SelfDamage.HasValue && SelfDamage > 0;

    /// <summary>
    /// Gets whether cascade caused stress gain.
    /// </summary>
    public bool CausesStress => StressGained.HasValue && StressGained > 0;

    /// <summary>
    /// Gets whether cascade caused corruption.
    /// </summary>
    public bool CausesCorruption => CorruptionGained.HasValue && CorruptionGained > 0;

    /// <summary>
    /// Gets the total number of negative effects from this cascade.
    /// </summary>
    public int TotalNegativeEffects
    {
        get
        {
            var count = 0;
            if (CoherenceLost > 0) count++;
            if (CausesDamage) count++;
            if (CausesStress) count++;
            if (CausesCorruption) count++;
            if (SpellDisrupted) count++;
            return count;
        }
    }

    /// <summary>
    /// Creates a display string for this cascade event.
    /// </summary>
    /// <returns>A human-readable description of the cascade result.</returns>
    public string ToDisplayString()
    {
        if (!CascadeTriggered)
        {
            return "No cascade.";
        }

        var effects = new List<string>();

        if (CoherenceLost > 0)
        {
            effects.Add($"Coherence -{CoherenceLost}");
        }

        if (SelfDamage.HasValue && SelfDamage > 0)
        {
            effects.Add($"DMG {SelfDamage}");
        }

        if (StressGained.HasValue && StressGained > 0)
        {
            effects.Add($"STRESS +{StressGained}");
        }

        if (CorruptionGained.HasValue && CorruptionGained > 0)
        {
            effects.Add($"CORRUPTION +{CorruptionGained}");
        }

        if (SpellDisrupted)
        {
            effects.Add("SPELL DISRUPTED");
        }

        return effects.Count > 0
            ? $"CASCADE: {string.Join(", ", effects)}"
            : "CASCADE: (no effects)";
    }
}
