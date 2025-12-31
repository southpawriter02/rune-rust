using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Models.Magic;

/// <summary>
/// Result of a backlash check during spellcasting.
/// Contains all information about whether backlash occurred and its effects.
/// </summary>
/// <remarks>
/// See: v0.4.3d (The Backlash) for implementation details.
/// Backlash occurs when casting spells while Flux exceeds the Critical threshold (50).
/// </remarks>
public sealed record BacklashResult
{
    /// <summary>
    /// Whether backlash was triggered.
    /// </summary>
    public bool Triggered { get; init; }

    /// <summary>
    /// Severity of the backlash (None if not triggered).
    /// </summary>
    public BacklashSeverity Severity { get; init; }

    /// <summary>
    /// Damage dealt to the caster by backlash.
    /// </summary>
    public int DamageDealt { get; init; }

    /// <summary>
    /// Duration of Aether Sickness applied (0 if none).
    /// Major backlash: 2 turns, Catastrophic: 5 turns.
    /// </summary>
    public int AetherSicknessDuration { get; init; }

    /// <summary>
    /// Corruption points added (0 or 1).
    /// Only Catastrophic backlash adds corruption.
    /// </summary>
    public int CorruptionAdded { get; init; }

    /// <summary>
    /// Narrative message describing the backlash.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// The calculated risk percentage (Flux - 50).
    /// </summary>
    public int RiskChance { get; init; }

    /// <summary>
    /// The d100 roll result.
    /// </summary>
    public int Roll { get; init; }

    /// <summary>
    /// Computed: How badly the roll failed (risk - roll).
    /// </summary>
    public int FailMargin => Triggered ? RiskChance - Roll : 0;

    /// <summary>
    /// Factory: No backlash occurred (safe cast or Flux below critical).
    /// </summary>
    /// <param name="riskChance">The calculated risk percentage.</param>
    /// <param name="roll">The d100 roll result (0 if no roll needed).</param>
    public static BacklashResult NoBacklash(int riskChance = 0, int roll = 0) => new()
    {
        Triggered = false,
        Severity = BacklashSeverity.None,
        DamageDealt = 0,
        AetherSicknessDuration = 0,
        CorruptionAdded = 0,
        Message = string.Empty,
        RiskChance = riskChance,
        Roll = roll
    };

    /// <summary>
    /// Factory: Backlash triggered with specified effects.
    /// </summary>
    /// <param name="severity">The backlash severity level.</param>
    /// <param name="damage">Damage dealt to the caster.</param>
    /// <param name="sicknessDuration">Duration of Aether Sickness (0 for Minor).</param>
    /// <param name="corruption">Corruption gained (0 or 1).</param>
    /// <param name="message">Narrative description of the backlash.</param>
    /// <param name="riskChance">The calculated risk percentage.</param>
    /// <param name="roll">The d100 roll result.</param>
    public static BacklashResult Backlash(
        BacklashSeverity severity,
        int damage,
        int sicknessDuration,
        int corruption,
        string message,
        int riskChance,
        int roll) => new()
    {
        Triggered = true,
        Severity = severity,
        DamageDealt = damage,
        AetherSicknessDuration = sicknessDuration,
        CorruptionAdded = corruption,
        Message = message,
        RiskChance = riskChance,
        Roll = roll
    };
}
