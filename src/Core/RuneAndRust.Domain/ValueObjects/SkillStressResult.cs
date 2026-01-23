namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the psychic stress result from a skill check, calculated based on
/// corruption exposure and fumble severity.
/// </summary>
/// <remarks>
/// <para>
/// The Trauma Economy in Rune &amp; Rust links skill interactions with corrupted
/// objects and areas to psychic stress accumulation. This value object captures
/// the stress calculation result for consumption by the trauma system.
/// </para>
/// <para>
/// Stress accumulates from two sources:
/// <list type="bullet">
///   <item>Corruption exposure during the skill check (based on CorruptionTier)</item>
///   <item>Fumble bonus when a catastrophic failure occurs in a corrupted area</item>
/// </list>
/// </para>
/// <para>
/// When total stress exceeds the breaking point threshold (8), the trauma system
/// may trigger a breaking point check, risking permanent mental trauma.
/// </para>
/// </remarks>
/// <param name="TotalStress">The total psychic stress inflicted by this skill check.</param>
/// <param name="CorruptionStress">Stress from corruption tier exposure (0-10).</param>
/// <param name="FumbleStress">Additional stress from fumbling in corrupted area (0-8).</param>
/// <param name="Source">The primary source of stress for this check.</param>
/// <param name="TriggersBreakingPoint">Whether this stress level may trigger a breaking point check.</param>
/// <param name="CorruptionTier">The corruption tier that was active during the check.</param>
public readonly record struct SkillStressResult(
    int TotalStress,
    int CorruptionStress,
    int FumbleStress,
    StressSource Source,
    bool TriggersBreakingPoint,
    CorruptionTier CorruptionTier)
{
    /// <summary>
    /// The threshold at which a breaking point check may be triggered.
    /// </summary>
    public const int BreakingPointThreshold = 8;

    /// <summary>
    /// Gets a value indicating whether any stress was incurred from this check.
    /// </summary>
    public bool HasStress => TotalStress > 0;

    /// <summary>
    /// Gets a value indicating whether the check occurred in a corrupted area.
    /// </summary>
    public bool InCorruptedArea => CorruptionTier != CorruptionTier.Normal;

    /// <summary>
    /// Gets a value indicating whether fumble stress was applied.
    /// </summary>
    public bool HadFumble => FumbleStress > 0;

    /// <summary>
    /// Creates a SkillStressResult with no stress (normal area, no fumble).
    /// </summary>
    /// <returns>A zero-stress result.</returns>
    public static SkillStressResult None()
    {
        return new SkillStressResult(
            TotalStress: 0,
            CorruptionStress: 0,
            FumbleStress: 0,
            Source: StressSource.None,
            TriggersBreakingPoint: false,
            CorruptionTier: CorruptionTier.Normal);
    }

    /// <summary>
    /// Creates a SkillStressResult from corruption exposure without a fumble.
    /// </summary>
    /// <param name="tier">The corruption tier of the area.</param>
    /// <returns>A stress result based on corruption exposure.</returns>
    public static SkillStressResult FromCorruption(CorruptionTier tier)
    {
        var stress = GetCorruptionStress(tier);
        return new SkillStressResult(
            TotalStress: stress,
            CorruptionStress: stress,
            FumbleStress: 0,
            Source: stress > 0 ? StressSource.Corruption : StressSource.None,
            TriggersBreakingPoint: stress >= BreakingPointThreshold,
            CorruptionTier: tier);
    }

    /// <summary>
    /// Creates a SkillStressResult from both corruption exposure and a fumble.
    /// </summary>
    /// <param name="tier">The corruption tier of the area.</param>
    /// <returns>A stress result with corruption and fumble stress combined.</returns>
    public static SkillStressResult FromFumble(CorruptionTier tier)
    {
        var corruptionStress = GetCorruptionStress(tier);
        var fumbleStress = GetFumbleStress(tier);
        var totalStress = corruptionStress + fumbleStress;

        return new SkillStressResult(
            TotalStress: totalStress,
            CorruptionStress: corruptionStress,
            FumbleStress: fumbleStress,
            Source: StressSource.Fumble,
            TriggersBreakingPoint: totalStress >= BreakingPointThreshold,
            CorruptionTier: tier);
    }

    /// <summary>
    /// Gets the base stress cost for a corruption tier.
    /// </summary>
    /// <param name="tier">The corruption tier.</param>
    /// <returns>The stress cost (0-10).</returns>
    public static int GetCorruptionStress(CorruptionTier tier)
    {
        return tier switch
        {
            CorruptionTier.Normal => 0,
            CorruptionTier.Glitched => 2,
            CorruptionTier.Blighted => 5,
            CorruptionTier.Resonance => 10,
            _ => 0
        };
    }

    /// <summary>
    /// Gets the fumble bonus stress for a corruption tier.
    /// </summary>
    /// <param name="tier">The corruption tier where the fumble occurred.</param>
    /// <returns>The bonus stress from fumbling (1-8).</returns>
    public static int GetFumbleStress(CorruptionTier tier)
    {
        return tier switch
        {
            CorruptionTier.Normal => 1,     // Baseline fumble stress
            CorruptionTier.Glitched => 2,   // Corruption amplifies failure
            CorruptionTier.Blighted => 4,   // Blight punishes mistakes
            CorruptionTier.Resonance => 8,  // Catastrophic psychic backlash
            _ => 1
        };
    }

    /// <summary>
    /// Returns a human-readable description of the stress result.
    /// </summary>
    /// <returns>A formatted string describing the stress incurred.</returns>
    public string ToDescription()
    {
        if (!HasStress)
            return "No psychic stress incurred.";

        var parts = new List<string>();

        if (CorruptionStress > 0)
            parts.Add($"{CorruptionStress} from {CorruptionTier} corruption");

        if (FumbleStress > 0)
            parts.Add($"{FumbleStress} from fumble");

        var description = $"Psychic Stress: {TotalStress} ({string.Join(", ", parts)})";

        if (TriggersBreakingPoint)
            description += " [Breaking Point Risk]";

        return description;
    }
}
