using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Events;

/// <summary>
/// Event published when magical backlash occurs during spellcasting.
/// Consumed by UI and audio systems for feedback.
/// </summary>
/// <remarks>
/// See: v0.4.3d (The Backlash) for implementation details.
/// </remarks>
/// <param name="CasterId">The unique identifier of the caster.</param>
/// <param name="CasterName">The display name of the caster.</param>
/// <param name="Severity">The backlash severity level.</param>
/// <param name="DamageDealt">Damage dealt to the caster.</param>
/// <param name="AetherSicknessDuration">Duration of Aether Sickness applied (0 if none).</param>
/// <param name="CorruptionAdded">Corruption points gained (0 or 1).</param>
/// <param name="RiskChance">The calculated risk percentage (Flux - 50).</param>
/// <param name="Roll">The d100 roll result.</param>
/// <param name="CurrentFlux">The Flux level at time of cast.</param>
/// <param name="SpellAttempted">Name of the spell that was being cast (optional).</param>
public record BacklashEvent(
    Guid CasterId,
    string CasterName,
    BacklashSeverity Severity,
    int DamageDealt,
    int AetherSicknessDuration,
    int CorruptionAdded,
    int RiskChance,
    int Roll,
    int CurrentFlux,
    string? SpellAttempted = null)
{
    /// <summary>
    /// Computed: Whether corruption was gained.
    /// </summary>
    public bool GainedCorruption => CorruptionAdded > 0;

    /// <summary>
    /// Computed: Severity display string (uppercase).
    /// </summary>
    public string SeverityDisplay => Severity.ToString().ToUpperInvariant();

    /// <summary>
    /// Computed: How badly the roll failed (risk - roll).
    /// </summary>
    public int FailMargin => RiskChance - Roll;

    /// <summary>
    /// Computed: Whether the caster was knocked unconscious.
    /// </summary>
    public bool WasLethal { get; init; }
}
