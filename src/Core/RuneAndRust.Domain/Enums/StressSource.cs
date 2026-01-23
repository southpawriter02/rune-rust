namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Identifies the source of psychic stress from a skill check.
/// </summary>
/// <remarks>
/// <para>
/// Stress sources determine how the trauma system processes and displays
/// stress accumulation. Different sources may have different recovery
/// mechanisms or consequences.
/// </para>
/// <para>
/// The trauma economy in Rune &amp; Rust links skill interactions with corrupted
/// objects and areas to psychic stress accumulation. Each source type has
/// distinct narrative implications and may trigger different UI feedback.
/// </para>
/// </remarks>
public enum StressSource
{
    /// <summary>
    /// No stress was incurred from this skill check.
    /// </summary>
    /// <remarks>
    /// Returned when the skill check occurred in a normal (uncorrupted) area
    /// and did not result in a fumble.
    /// </remarks>
    None = 0,

    /// <summary>
    /// Stress from exposure to a corrupted area during the skill check.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Scales with CorruptionTier:
    /// <list type="bullet">
    ///   <item>Glitched: 2 stress (minor psychic noise)</item>
    ///   <item>Blighted: 5 stress (significant corruption)</item>
    ///   <item>Resonance: 10 stress (direct Blight exposure)</item>
    /// </list>
    /// </para>
    /// </remarks>
    Corruption = 1,

    /// <summary>
    /// Stress from a fumble (catastrophic failure) during a skill check.
    /// </summary>
    /// <remarks>
    /// Combines corruption stress with fumble bonus stress. Fumbles in
    /// corrupted areas are particularly devastating, as the Blight seems
    /// to punish mistakes with amplified psychic backlash.
    /// </remarks>
    Fumble = 2,

    /// <summary>
    /// Stress from interacting with a corrupted object or artifact.
    /// </summary>
    /// <remarks>
    /// Used when manipulating [Glitched Artifacts] or corrupted terminals.
    /// The object's corruption tier determines the stress cost independently
    /// of the area's corruption level.
    /// </remarks>
    CorruptedObject = 3,

    /// <summary>
    /// Stress from prolonged exposure during extended skill checks.
    /// </summary>
    /// <remarks>
    /// Applies to multi-stage procedures in corrupted areas such as
    /// terminal hacking, complex lock mechanisms, or ritual completion.
    /// Stress accumulates with each step of the procedure.
    /// </remarks>
    ExtendedExposure = 4
}
