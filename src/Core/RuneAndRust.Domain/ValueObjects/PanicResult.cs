// ═══════════════════════════════════════════════════════════════════════════════
// PanicResult.cs
// Represents the complete result of a Panic Table roll during RuinMadness stage.
// The Panic Table is triggered when a character in RuinMadness experiences a
// stress-inducing event. This record contains all data needed to apply the
// mechanical effects and display the outcome to players.
// Version: 0.18.2b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the complete result of a Panic Table roll.
/// </summary>
/// <remarks>
/// <para>
/// The Panic Table is triggered when a character in RuinMadness stage
/// experiences a stress-inducing event. This record contains all data
/// needed to apply the mechanical effects and display the outcome.
/// </para>
/// <para>
/// Panic Table (d10):
/// <list type="bullet">
/// <item>1: Frozen — Cannot act for 1 round</item>
/// <item>2: Scream — Alerts enemies</item>
/// <item>3: Flee — Must flee combat</item>
/// <item>4: Fetal — Prone + disadvantage</item>
/// <item>5: Blackout — Unconscious 1d4 rounds</item>
/// <item>6: Denial — Cannot perceive threat</item>
/// <item>7: Violence — Attack nearest creature</item>
/// <item>8: Catatonia — Stunned until hit</item>
/// <item>9: Dissociation — Random action</item>
/// <item>10: None — Lucky break</item>
/// </list>
/// </para>
/// </remarks>
/// <param name="DieRoll">The d10 roll result (1-10).</param>
/// <param name="Effect">The panic effect triggered.</param>
/// <param name="EffectName">Display name of the effect.</param>
/// <param name="Description">Narrative description of the effect.</param>
/// <param name="DurationTurns">Duration in turns, if applicable.</param>
/// <param name="SelfDamage">Self-damage dealt, if applicable.</param>
/// <param name="StatusEffects">Status effects to apply.</param>
/// <param name="ForcesAction">Whether this effect forces a specific action.</param>
/// <param name="ForcedActionType">Type of forced action, if applicable.</param>
/// <seealso cref="PanicEffect"/>
/// <seealso cref="CpsState"/>
/// <seealso cref="CpsStage"/>
public readonly record struct PanicResult(
    int DieRoll,
    PanicEffect Effect,
    string EffectName,
    string Description,
    int? DurationTurns,
    int? SelfDamage,
    IReadOnlyList<string> StatusEffects,
    bool ForcesAction,
    string? ForcedActionType)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // ARROW-EXPRESSION PROPERTIES — Computed Indicators
    // ═══════════════════════════════════════════════════════════════════════════

    #region Arrow-Expression Properties

    /// <summary>
    /// Gets whether this is a "lucky break" result with no effect.
    /// </summary>
    /// <value>
    /// <c>true</c> if Effect is None (rolled 10); otherwise, <c>false</c>.
    /// </value>
    public bool IsLuckyBreak => Effect == PanicEffect.None;

    /// <summary>
    /// Gets whether this effect causes the character to lose their turn.
    /// </summary>
    /// <value>
    /// <c>true</c> if Effect is Frozen, Blackout, or Catatonia; otherwise, <c>false</c>.
    /// </value>
    public bool LosesTurn => Effect is PanicEffect.Frozen or PanicEffect.Blackout or PanicEffect.Catatonia;

    /// <summary>
    /// Gets whether this effect affects nearby allies.
    /// </summary>
    /// <value>
    /// <c>true</c> if Effect is Scream or Violence; otherwise, <c>false</c>.
    /// </value>
    public bool AffectsAllies => Effect is PanicEffect.Scream or PanicEffect.Violence;

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS — Panic Effect Creation
    // ═══════════════════════════════════════════════════════════════════════════

    #region Factory Methods

    /// <summary>
    /// Creates a "lucky break" result for a roll of 10.
    /// </summary>
    /// <returns>A PanicResult with no effect.</returns>
    /// <remarks>
    /// Rolled 10 on the Panic Table — the character's mind holds together.
    /// No status effects, no forced actions.
    /// </remarks>
    public static PanicResult LuckyBreak() => new(
        DieRoll: 10,
        Effect: PanicEffect.None,
        EffectName: "Lucky Break",
        Description: "Your mind holds together... for now.",
        DurationTurns: null,
        SelfDamage: null,
        StatusEffects: Array.Empty<string>(),
        ForcesAction: false,
        ForcedActionType: null);

    /// <summary>
    /// Creates a Frozen result for a roll of 1.
    /// </summary>
    /// <returns>A PanicResult representing the Logic Lock effect.</returns>
    /// <remarks>
    /// The character's mind freezes, unable to process the paradox.
    /// They are Stunned for 1 round.
    /// </remarks>
    public static PanicResult Frozen() => new(
        DieRoll: 1,
        Effect: PanicEffect.Frozen,
        EffectName: "Logic Lock",
        Description: "Your mind freezes, unable to process the paradox.",
        DurationTurns: 1,
        SelfDamage: null,
        StatusEffects: new[] { "Stunned" },
        ForcesAction: false,
        ForcedActionType: null);

    /// <summary>
    /// Creates a Scream result for a roll of 2.
    /// </summary>
    /// <returns>A PanicResult representing the involuntary scream effect.</returns>
    /// <remarks>
    /// The character screams involuntarily, alerting all nearby enemies.
    /// May trigger additional encounters in exploration mode.
    /// </remarks>
    public static PanicResult Scream() => new(
        DieRoll: 2,
        Effect: PanicEffect.Scream,
        EffectName: "Primal Scream",
        Description: "An involuntary scream tears from your throat, alerting everything nearby.",
        DurationTurns: null,
        SelfDamage: null,
        StatusEffects: Array.Empty<string>(),
        ForcesAction: false,
        ForcedActionType: null);

    /// <summary>
    /// Creates a Flee result for a roll of 3.
    /// </summary>
    /// <returns>A PanicResult representing the forced flight response.</returns>
    /// <remarks>
    /// Survival instincts override all else. The character must flee.
    /// Forces a FleeFromSource action.
    /// </remarks>
    public static PanicResult Flee() => new(
        DieRoll: 3,
        Effect: PanicEffect.Flee,
        EffectName: "Evacuation Protocol",
        Description: "Your survival instincts override all else. RUN.",
        DurationTurns: null,
        SelfDamage: null,
        StatusEffects: Array.Empty<string>(),
        ForcesAction: true,
        ForcedActionType: "FleeFromSource");

    /// <summary>
    /// Creates a Fetal result for a roll of 4.
    /// </summary>
    /// <returns>A PanicResult representing the fetal position response.</returns>
    /// <remarks>
    /// The character drops and curls into a ball, Prone with disadvantage
    /// until they use an action to recover.
    /// </remarks>
    public static PanicResult Fetal() => new(
        DieRoll: 4,
        Effect: PanicEffect.Fetal,
        EffectName: "Protective Curl",
        Description: "Your body curls into a protective ball as your mind retreats.",
        DurationTurns: null,
        SelfDamage: null,
        StatusEffects: new[] { "Prone", "Disadvantage" },
        ForcesAction: false,
        ForcedActionType: null);

    /// <summary>
    /// Creates a Blackout result for a roll of 5.
    /// </summary>
    /// <param name="durationRoll">The 1d4 roll determining unconscious duration.</param>
    /// <returns>A PanicResult representing the blackout effect.</returns>
    /// <remarks>
    /// The mind shuts down temporarily. Character falls unconscious for
    /// 1d4 rounds and cannot be woken by normal means.
    /// </remarks>
    public static PanicResult Blackout(int durationRoll) => new(
        DieRoll: 5,
        Effect: PanicEffect.Blackout,
        EffectName: "System Shutdown",
        Description: "Everything goes dark as your mind shuts down.",
        DurationTurns: durationRoll,
        SelfDamage: null,
        StatusEffects: new[] { "Unconscious" },
        ForcesAction: false,
        ForcedActionType: null);

    /// <summary>
    /// Creates a Denial result for a roll of 6.
    /// </summary>
    /// <param name="durationRoll">The 1d4 roll determining denial duration.</param>
    /// <returns>A PanicResult representing the perceptual denial effect.</returns>
    /// <remarks>
    /// The mind refuses to acknowledge the threat. Character cannot perceive
    /// or target the triggering entity for 1d4 rounds.
    /// </remarks>
    public static PanicResult Denial(int durationRoll) => new(
        DieRoll: 6,
        Effect: PanicEffect.Denial,
        EffectName: "Selective Blindness",
        Description: "Your mind refuses to acknowledge what it cannot accept.",
        DurationTurns: durationRoll,
        SelfDamage: null,
        StatusEffects: new[] { "CannotPerceiveThreat" },
        ForcesAction: false,
        ForcedActionType: null);

    /// <summary>
    /// Creates a Violence result for a roll of 7.
    /// </summary>
    /// <returns>A PanicResult representing the rage response.</returns>
    /// <remarks>
    /// Rage fills the void where reason should be. The character must attack
    /// the nearest creature, friend or foe.
    /// </remarks>
    public static PanicResult Violence() => new(
        DieRoll: 7,
        Effect: PanicEffect.Violence,
        EffectName: "Paradox Fury",
        Description: "Rage fills the void where reason should be. ATTACK.",
        DurationTurns: 1,
        SelfDamage: null,
        StatusEffects: Array.Empty<string>(),
        ForcesAction: true,
        ForcedActionType: "AttackNearest");

    /// <summary>
    /// Creates a Catatonia result for a roll of 8.
    /// </summary>
    /// <param name="durationRoll">The 1d4 roll determining catatonic duration.</param>
    /// <returns>A PanicResult representing the catatonic state.</returns>
    /// <remarks>
    /// Complete mental shutdown. Character is Prone and Stunned,
    /// remaining so until they take any damage.
    /// </remarks>
    public static PanicResult Catatonia(int durationRoll) => new(
        DieRoll: 8,
        Effect: PanicEffect.Catatonia,
        EffectName: "System Crash",
        Description: "Your mind shuts down completely.",
        DurationTurns: durationRoll,
        SelfDamage: null,
        StatusEffects: new[] { "Prone", "Stunned" },
        ForcesAction: false,
        ForcedActionType: null);

    /// <summary>
    /// Creates a Dissociation result for a roll of 9.
    /// </summary>
    /// <returns>A PanicResult representing the dissociative state.</returns>
    /// <remarks>
    /// Mind and body disconnect. The character takes a random action
    /// determined by the game system.
    /// </remarks>
    public static PanicResult Dissociation() => new(
        DieRoll: 9,
        Effect: PanicEffect.Dissociation,
        EffectName: "Reality Slip",
        Description: "Your connection to your own body severs. You watch yourself act without control.",
        DurationTurns: 1,
        SelfDamage: null,
        StatusEffects: new[] { "Dissociated" },
        ForcesAction: true,
        ForcedActionType: "RandomAction");

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY — String Representation
    // ═══════════════════════════════════════════════════════════════════════════

    #region Display

    /// <summary>
    /// Returns a string representation of the panic result for debugging and logging.
    /// </summary>
    /// <returns>
    /// A formatted string in the format:
    /// <c>"Panic[d10={DieRoll}]: {EffectName} - {Description}"</c>,
    /// or <c>"Panic[d10={DieRoll}]: LUCKY BREAK - No effect"</c> for None.
    /// </returns>
    /// <example>
    /// <code>
    /// var frozen = PanicResult.Frozen();
    /// // Returns "Panic[d10=1]: Logic Lock - Your mind freezes, unable to process the paradox."
    ///
    /// var lucky = PanicResult.LuckyBreak();
    /// // Returns "Panic[d10=10]: LUCKY BREAK - No effect"
    /// </code>
    /// </example>
    public override string ToString() =>
        IsLuckyBreak
            ? $"Panic[d10={DieRoll}]: LUCKY BREAK - No effect"
            : $"Panic[d10={DieRoll}]: {EffectName} - {Description}";

    #endregion
}
