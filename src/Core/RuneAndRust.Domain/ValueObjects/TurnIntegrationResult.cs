// ═══════════════════════════════════════════════════════════════════════════════
// TurnIntegrationResult.cs
// Immutable result object capturing all effects applied during turn processing.
// Provides complete tracking of resource decay, Apotheosis mechanics, panic
// effects, CPS stage effects, environmental stress, and trauma triggers.
// Version: 0.18.5d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Immutable result capturing all effects applied during a turn processing phase.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Overview:</strong>
/// TurnIntegrationResult encapsulates all outcomes from processing either the
/// start or end of a character's turn. It aggregates results from multiple
/// trauma economy subsystems into a single, coherent snapshot.
/// </para>
/// <para>
/// <strong>Turn Start Effects:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>Resource decay (Rage 10/turn, Momentum 15/turn out of combat)</description></item>
///   <item><description>Apotheosis stress cost (10/turn)</description></item>
///   <item><description>Apotheosis auto-exit (stress >= 100)</description></item>
/// </list>
/// <para>
/// <strong>Turn End Effects:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>Panic table check (RuinMadness stage triggers d10 roll)</description></item>
///   <item><description>CPS stage effects (UI distortions, unreliable perception)</description></item>
///   <item><description>Environmental stress (0-5 based on environment)</description></item>
///   <item><description>Trauma trigger checks</description></item>
/// </list>
/// <para>
/// <strong>Usage Pattern:</strong>
/// Created by <c>UnifiedTurnHandler.ProcessTurnStart</c> and
/// <c>UnifiedTurnHandler.ProcessTurnEnd</c>. Consumers should check relevant
/// properties based on the <see cref="Phase"/> to determine what actions occurred.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // At turn start
/// var startResult = turnHandler.ProcessTurnStart(characterId, isInCombat: false);
/// if (startResult.AutoExitedApotheosis)
///     ShowApotheosisExitNotification(startResult.ApotheosisExitReason);
///
/// // At turn end
/// var endResult = turnHandler.ProcessTurnEnd(characterId, environmentalStress: 3);
/// if (endResult.PanicEffectApplied.HasValue)
///     ApplyPanicEffect(endResult.PanicEffectApplied.Value);
/// if (endResult.TraumaCheckTriggered)
///     InitiateTraumaCheck(characterId);
/// </code>
/// </example>
/// <seealso cref="TurnPhase"/>
/// <seealso cref="RageDecayResult"/>
/// <seealso cref="MomentumDecayResult"/>
/// <seealso cref="PanicEffect"/>
/// <seealso cref="CpsStage"/>
public readonly record struct TurnIntegrationResult
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CORE IDENTIFICATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// The character whose turn was processed.
    /// </summary>
    public Guid CharacterId { get; init; }

    /// <summary>
    /// The phase of turn processing (Start or End).
    /// </summary>
    public TurnPhase Phase { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // RESOURCE DECAY (Turn Start)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Result of Rage decay for Berserker characters.
    /// </summary>
    /// <remarks>
    /// <c>null</c> if character is not a Berserker, is in combat, or no decay occurred.
    /// Rage decays 10 per turn when out of combat.
    /// </remarks>
    public RageDecayResult? RageDecay { get; init; }

    /// <summary>
    /// Result of Momentum decay for Storm Blade characters.
    /// </summary>
    /// <remarks>
    /// <c>null</c> if character is not a Storm Blade, is in combat, or no decay occurred.
    /// Momentum decays 15 per turn when out of combat and idle.
    /// </remarks>
    public MomentumDecayResult? MomentumDecay { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // APOTHEOSIS MECHANICS (Turn Start)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Whether the character is currently in Apotheosis state.
    /// </summary>
    public bool ApotheosisActive { get; init; }

    /// <summary>
    /// Stress cost applied for maintaining Apotheosis this turn.
    /// </summary>
    /// <remarks>
    /// Always 10 when in Apotheosis, 0 otherwise.
    /// </remarks>
    public int ApotheosisStressCost { get; init; }

    /// <summary>
    /// Whether Apotheosis was forcibly exited this turn.
    /// </summary>
    /// <remarks>
    /// <c>true</c> if stress reached 100 and Apotheosis auto-exited.
    /// </remarks>
    public bool AutoExitedApotheosis { get; init; }

    /// <summary>
    /// Reason for Apotheosis exit, if auto-exited.
    /// </summary>
    /// <remarks>
    /// <c>null</c> if Apotheosis did not exit this turn.
    /// </remarks>
    public string? ApotheosisExitReason { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // PANIC TABLE (Turn End)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Whether a Panic Table check was performed this turn.
    /// </summary>
    /// <remarks>
    /// <c>true</c> if character was in RuinMadness CPS stage at turn end.
    /// </remarks>
    public bool PanicCheckPerformed { get; init; }

    /// <summary>
    /// The panic effect applied, if any.
    /// </summary>
    /// <remarks>
    /// <c>null</c> if no panic check was performed or if the roll resulted
    /// in <see cref="PanicEffect.None"/> (rolled 10).
    /// </remarks>
    public PanicEffect? PanicEffectApplied { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // CPS EFFECTS (Turn End)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// The character's CPS stage at the time of processing.
    /// </summary>
    public CpsStage? CpsStage { get; init; }

    /// <summary>
    /// List of CPS effect descriptions applied this turn.
    /// </summary>
    /// <remarks>
    /// May include: "UI distortions active", "Unreliable perception",
    /// "Reality bleed visible", etc. Empty list if no CPS effects.
    /// </remarks>
    public IReadOnlyList<string> CpsEffectsApplied { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // ENVIRONMENTAL STRESS (Turn End)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Amount of environmental stress applied this turn.
    /// </summary>
    /// <remarks>
    /// Range 0-5 based on environment factors. 0 if turn start phase.
    /// </remarks>
    public int EnvironmentalStressApplied { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // TRAUMA TRIGGERS (Turn End)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Whether a Trauma Check was triggered this turn.
    /// </summary>
    /// <remarks>
    /// <c>true</c> if stress reached 100 during turn processing.
    /// </remarks>
    public bool TraumaCheckTriggered { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether any resource decay occurred this turn.
    /// </summary>
    public bool HadResourceDecay => RageDecay is not null || MomentumDecay is not null;

    /// <summary>
    /// Gets whether any panic or CPS effects were applied.
    /// </summary>
    public bool HadPsychologicalEffects =>
        PanicEffectApplied is not null ||
        (CpsEffectsApplied?.Count ?? 0) > 0;

    /// <summary>
    /// Gets whether this result contains any significant effects.
    /// </summary>
    public bool HasEffects =>
        HadResourceDecay ||
        ApotheosisStressCost > 0 ||
        AutoExitedApotheosis ||
        PanicCheckPerformed ||
        EnvironmentalStressApplied > 0 ||
        TraumaCheckTriggered;

    /// <summary>
    /// Gets whether this is a valid result object.
    /// </summary>
    public bool IsValid => CharacterId != Guid.Empty;

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a result for turn start processing.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <param name="rageDecay">Rage decay result, if applicable.</param>
    /// <param name="momentumDecay">Momentum decay result, if applicable.</param>
    /// <param name="apotheosisActive">Whether character is in Apotheosis.</param>
    /// <param name="apotheosisStressCost">Stress cost for Apotheosis maintenance.</param>
    /// <param name="autoExitedApotheosis">Whether Apotheosis was force-exited.</param>
    /// <param name="apotheosisExitReason">Reason for force-exit.</param>
    /// <returns>A new TurnIntegrationResult for turn start.</returns>
    public static TurnIntegrationResult CreateTurnStart(
        Guid characterId,
        RageDecayResult? rageDecay = null,
        MomentumDecayResult? momentumDecay = null,
        bool apotheosisActive = false,
        int apotheosisStressCost = 0,
        bool autoExitedApotheosis = false,
        string? apotheosisExitReason = null)
    {
        return new TurnIntegrationResult
        {
            CharacterId = characterId,
            Phase = TurnPhase.Start,
            RageDecay = rageDecay,
            MomentumDecay = momentumDecay,
            ApotheosisActive = apotheosisActive,
            ApotheosisStressCost = apotheosisStressCost,
            AutoExitedApotheosis = autoExitedApotheosis,
            ApotheosisExitReason = apotheosisExitReason,
            PanicCheckPerformed = false,
            PanicEffectApplied = null,
            CpsStage = null,
            CpsEffectsApplied = Array.Empty<string>(),
            EnvironmentalStressApplied = 0,
            TraumaCheckTriggered = false
        };
    }

    /// <summary>
    /// Creates a result for turn end processing.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <param name="panicCheckPerformed">Whether panic check was performed.</param>
    /// <param name="panicEffectApplied">Panic effect applied, if any.</param>
    /// <param name="cpsStage">Current CPS stage.</param>
    /// <param name="cpsEffectsApplied">CPS effects applied.</param>
    /// <param name="environmentalStressApplied">Environmental stress applied.</param>
    /// <param name="traumaCheckTriggered">Whether trauma check was triggered.</param>
    /// <returns>A new TurnIntegrationResult for turn end.</returns>
    public static TurnIntegrationResult CreateTurnEnd(
        Guid characterId,
        bool panicCheckPerformed = false,
        PanicEffect? panicEffectApplied = null,
        CpsStage? cpsStage = null,
        IReadOnlyList<string>? cpsEffectsApplied = null,
        int environmentalStressApplied = 0,
        bool traumaCheckTriggered = false)
    {
        return new TurnIntegrationResult
        {
            CharacterId = characterId,
            Phase = TurnPhase.End,
            RageDecay = null,
            MomentumDecay = null,
            ApotheosisActive = false,
            ApotheosisStressCost = 0,
            AutoExitedApotheosis = false,
            ApotheosisExitReason = null,
            PanicCheckPerformed = panicCheckPerformed,
            PanicEffectApplied = panicEffectApplied,
            CpsStage = cpsStage,
            CpsEffectsApplied = cpsEffectsApplied ?? Array.Empty<string>(),
            EnvironmentalStressApplied = environmentalStressApplied,
            TraumaCheckTriggered = traumaCheckTriggered
        };
    }

    /// <summary>
    /// Creates an empty result with no effects.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <param name="phase">The turn phase.</param>
    /// <returns>An empty TurnIntegrationResult.</returns>
    public static TurnIntegrationResult Empty(Guid characterId, TurnPhase phase)
    {
        return new TurnIntegrationResult
        {
            CharacterId = characterId,
            Phase = phase,
            RageDecay = null,
            MomentumDecay = null,
            ApotheosisActive = false,
            ApotheosisStressCost = 0,
            AutoExitedApotheosis = false,
            ApotheosisExitReason = null,
            PanicCheckPerformed = false,
            PanicEffectApplied = null,
            CpsStage = null,
            CpsEffectsApplied = Array.Empty<string>(),
            EnvironmentalStressApplied = 0,
            TraumaCheckTriggered = false
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STRING REPRESENTATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation for debugging and logging.
    /// </summary>
    /// <returns>A formatted string describing the turn result.</returns>
    public override string ToString()
    {
        var parts = new List<string>
        {
            $"Turn[{Phase}]: Character={CharacterId:N}"
        };

        if (RageDecay is { } rageDecay)
            parts.Add($"RageDecay={rageDecay.AmountDecayed}");

        if (MomentumDecay is { } momentumDecay)
            parts.Add($"MomentumDecay={momentumDecay.AmountDecayed}");

        if (ApotheosisStressCost > 0)
            parts.Add($"ApotheosisStress={ApotheosisStressCost}");

        if (AutoExitedApotheosis)
            parts.Add("[APOTHEOSIS EXIT]");

        if (PanicEffectApplied is not null)
            parts.Add($"Panic={PanicEffectApplied}");

        if (EnvironmentalStressApplied > 0)
            parts.Add($"EnvStress={EnvironmentalStressApplied}");

        if (TraumaCheckTriggered)
            parts.Add("[TRAUMA CHECK]");

        return string.Join(" | ", parts);
    }
}
