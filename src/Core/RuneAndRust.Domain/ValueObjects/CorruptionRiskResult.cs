// ═══════════════════════════════════════════════════════════════════════════════
// CorruptionRiskResult.cs
// Immutable value object encapsulating the outcome of a corruption risk
// evaluation for Myrk-gengr (Heretical path) abilities.
// Version: 0.20.4a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Encapsulates the outcome of a corruption risk evaluation for a
/// Myrk-gengr ability activation.
/// </summary>
/// <remarks>
/// <para>
/// Corruption risk is evaluated whenever a Heretical ability is used. The
/// result tracks whether corruption was triggered, the amount gained, and
/// the environmental context that caused the evaluation.
/// </para>
/// <para>
/// Common triggers for the Myrk-gengr specialization:
/// </para>
/// <list type="bullet">
///   <item><description>Using Shadow Step from bright light (+1 Corruption)</description></item>
///   <item><description>Maintaining Cloak of Night in bright light (+1/turn)</description></item>
///   <item><description>Targeting Coherent creatures with shadow abilities (+1)</description></item>
/// </list>
/// <example>
/// <code>
/// var riskResult = CorruptionRiskResult.CreateTriggered(
///     corruptionGained: 1,
///     reason: "Shadow Step used in bright light",
///     abilityUsed: "shadow-step",
///     lightCondition: LightLevelType.BrightLight);
///
/// if (riskResult.RiskTriggered)
///     Console.WriteLine(riskResult.GetDescriptionForPlayer());
/// </code>
/// </example>
/// </remarks>
/// <seealso cref="MyrkgengrAbilityId"/>
/// <seealso cref="LightLevelType"/>
public sealed record CorruptionRiskResult
{
    // ─────────────────────────────────────────────────────────────────────────
    // Properties
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Whether the corruption risk was triggered.
    /// </summary>
    public bool RiskTriggered { get; private init; }

    /// <summary>
    /// Amount of corruption gained (0 if not triggered).
    /// </summary>
    public int CorruptionGained { get; private init; }

    /// <summary>
    /// Human-readable reason for the corruption evaluation outcome.
    /// </summary>
    public string Reason { get; private init; } = string.Empty;

    /// <summary>
    /// The kebab-case ability ID that caused this evaluation.
    /// </summary>
    public string AbilityUsed { get; private init; } = string.Empty;

    /// <summary>
    /// The light condition at the time of evaluation.
    /// </summary>
    public LightLevelType LightCondition { get; private init; }

    /// <summary>
    /// Whether the target of the ability was a Coherent creature.
    /// </summary>
    public bool TargetIsCoherent { get; private init; }

    /// <summary>
    /// When this evaluation occurred.
    /// </summary>
    public DateTime EvaluatedAt { get; private init; }

    // ─────────────────────────────────────────────────────────────────────────
    // Factory Methods
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a result indicating corruption was triggered.
    /// </summary>
    /// <param name="corruptionGained">Amount of corruption gained. Must be positive.</param>
    /// <param name="reason">Description of why corruption was triggered.</param>
    /// <param name="abilityUsed">The ability ID that triggered corruption.</param>
    /// <param name="lightCondition">Light conditions during the evaluation.</param>
    /// <param name="targetIsCoherent">Whether the target was Coherent.</param>
    /// <returns>A new triggered corruption result.</returns>
    public static CorruptionRiskResult CreateTriggered(
        int corruptionGained,
        string reason,
        string abilityUsed,
        LightLevelType lightCondition,
        bool targetIsCoherent = false)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(corruptionGained, 1);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        ArgumentException.ThrowIfNullOrWhiteSpace(abilityUsed);

        return new CorruptionRiskResult
        {
            RiskTriggered = true,
            CorruptionGained = corruptionGained,
            Reason = reason,
            AbilityUsed = abilityUsed,
            LightCondition = lightCondition,
            TargetIsCoherent = targetIsCoherent,
            EvaluatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a result indicating no corruption was triggered (safe usage).
    /// </summary>
    /// <param name="abilityUsed">The ability ID that was evaluated.</param>
    /// <param name="lightCondition">Light conditions during the evaluation.</param>
    /// <returns>A new safe (no corruption) result.</returns>
    public static CorruptionRiskResult CreateSafe(
        string abilityUsed,
        LightLevelType lightCondition)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(abilityUsed);

        return new CorruptionRiskResult
        {
            RiskTriggered = false,
            CorruptionGained = 0,
            Reason = "No corruption risk in current conditions",
            AbilityUsed = abilityUsed,
            LightCondition = lightCondition,
            TargetIsCoherent = false,
            EvaluatedAt = DateTime.UtcNow
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Display
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Formats a player-facing description of the corruption outcome.
    /// </summary>
    /// <returns>A formatted string suitable for game UI display.</returns>
    public string GetDescriptionForPlayer()
    {
        if (!RiskTriggered)
            return "The shadows accept your presence without consequence.";

        return CorruptionGained switch
        {
            1 => "A whisper of corruption seeps into your being from the shadows.",
            2 => "Dark tendrils of corruption coil around your spirit.",
            >= 3 => "A surge of corruption floods through you, the shadows demanding their price.",
            _ => "The shadows stir uneasily."
        };
    }

    /// <summary>
    /// Returns a diagnostic representation of this result.
    /// </summary>
    public override string ToString() =>
        $"CorruptionRisk(Triggered={RiskTriggered}, Amount={CorruptionGained}, " +
        $"Ability={AbilityUsed}, Light={LightCondition})";
}
