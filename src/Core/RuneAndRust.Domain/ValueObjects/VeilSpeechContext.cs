// ------------------------------------------------------------------------------
// <copyright file="VeilSpeechContext.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Captures the context and modifiers for Utgard Veil-Speech interactions.
// Tracks the current state and calculates appropriate modifiers for social
// checks with Utgard NPCs.
// Part of v0.15.3g Cultural Protocols implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;

/// <summary>
/// Captures the context and modifiers for Utgard Veil-Speech interactions.
/// </summary>
/// <remarks>
/// <para>
/// Veil-Speech is the Utgard cultural protocol where direct truth-telling is considered
/// offensive and deception is a sign of respect. This value object tracks the current
/// state and calculates appropriate modifiers for social checks with Utgard NPCs.
/// </para>
/// <para>
/// The Utgard believe that only fools and children speak plainly. A cultured individual
/// layers truth within acceptable deception, demonstrating wit and social awareness.
/// Speaking direct truth is seen as either an insult (implying the listener is too
/// simple to understand subtlety) or a sign of barbarism.
/// </para>
/// <para>
/// Modifier effects:
/// <list type="bullet">
///   <item><description>Direct truth: -2d10 penalty (offensive)</description></item>
///   <item><description>Proper Veil-Speech: +1d10 bonus (respected)</description></item>
///   <item><description>Deception: DC -4 (culturally expected)</description></item>
/// </list>
/// </para>
/// <para>
/// State transitions based on interaction:
/// <list type="bullet">
///   <item><description>Direct truth: Neutral → Offended, Offended → DeepOffense</description></item>
///   <item><description>Proper Veil-Speech: Neutral → Respected, Offended → Neutral</description></item>
///   <item><description>Neutral interaction: No state change</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="CurrentState">The character's current Veil-Speech standing with this NPC.</param>
/// <param name="IsUsingDeception">Whether the current interaction uses deception.</param>
/// <param name="IsTellingDirectTruth">Whether the character is speaking direct truth.</param>
/// <param name="IsUsingProperVeilSpeech">Whether the character is using proper Veil-Speech form.</param>
/// <param name="NpcId">The Utgard NPC being interacted with.</param>
/// <example>
/// <code>
/// // Create a context for using proper Veil-Speech
/// var context = VeilSpeechContext.Create(
///     currentState: VeilSpeechState.Neutral,
///     npcId: "utgard-merchant-001",
///     isUsingProperVeilSpeech: true);
/// Console.WriteLine(context.GetDiceModifier());  // 1
/// Console.WriteLine(context.GetResultingState()); // VeilSpeechState.Respected
///
/// // Create a context for deception (culturally respected)
/// var deceptiveContext = VeilSpeechContext.ForDeception(
///     VeilSpeechState.Neutral, "utgard-noble-003");
/// Console.WriteLine(deceptiveContext.GetDcAdjustment());  // -4
/// </code>
/// </example>
public readonly record struct VeilSpeechContext(
    VeilSpeechState CurrentState,
    bool IsUsingDeception,
    bool IsTellingDirectTruth,
    bool IsUsingProperVeilSpeech,
    string NpcId)
{
    /// <summary>
    /// The DC reduction applied when using deception with Utgard NPCs.
    /// </summary>
    /// <remarks>
    /// Deception is culturally respected by Utgard, making Deception checks
    /// 4 points easier than with other cultures.
    /// </remarks>
    public const int DeceptionDcReduction = 4;

    /// <summary>
    /// The dice bonus for using proper Veil-Speech.
    /// </summary>
    /// <remarks>
    /// Using proper Veil-Speech (layered communication) demonstrates social
    /// sophistication and grants a +1d10 bonus on the interaction.
    /// </remarks>
    public const int ProperVeilSpeechBonus = 1;

    /// <summary>
    /// The dice penalty for telling direct truth to Utgard.
    /// </summary>
    /// <remarks>
    /// Direct truth-telling is offensive to Utgard, resulting in a -2d10 penalty
    /// and potential state degradation.
    /// </remarks>
    public const int DirectTruthPenalty = 2;

    /// <summary>
    /// Creates a VeilSpeechContext with the specified parameters.
    /// </summary>
    /// <param name="currentState">The current Veil-Speech standing.</param>
    /// <param name="npcId">The Utgard NPC identifier.</param>
    /// <param name="isUsingDeception">Whether using deception.</param>
    /// <param name="isTellingDirectTruth">Whether telling direct truth.</param>
    /// <param name="isUsingProperVeilSpeech">Whether using proper Veil-Speech.</param>
    /// <returns>A new <see cref="VeilSpeechContext"/>.</returns>
    /// <remarks>
    /// <para>
    /// Only one of the boolean flags should typically be true:
    /// <list type="bullet">
    ///   <item><description><paramref name="isUsingDeception"/>: Active deception attempt</description></item>
    ///   <item><description><paramref name="isTellingDirectTruth"/>: Blunt, unvarnished truth</description></item>
    ///   <item><description><paramref name="isUsingProperVeilSpeech"/>: Truth layered in acceptable deception</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// If none are true, the interaction is considered neutral (no specific Veil-Speech
    /// handling, using the current state's base modifiers).
    /// </para>
    /// </remarks>
    public static VeilSpeechContext Create(
        VeilSpeechState currentState,
        string npcId,
        bool isUsingDeception = false,
        bool isTellingDirectTruth = false,
        bool isUsingProperVeilSpeech = false) =>
        new(currentState, isUsingDeception, isTellingDirectTruth, isUsingProperVeilSpeech, npcId);

    /// <summary>
    /// Creates a context for a deception interaction with an Utgard NPC.
    /// </summary>
    /// <param name="currentState">The current Veil-Speech standing.</param>
    /// <param name="npcId">The Utgard NPC identifier.</param>
    /// <returns>A <see cref="VeilSpeechContext"/> configured for deception.</returns>
    /// <remarks>
    /// Deception is culturally respected by Utgard, granting a DC -4 bonus.
    /// </remarks>
    public static VeilSpeechContext ForDeception(VeilSpeechState currentState, string npcId) =>
        new(currentState, IsUsingDeception: true, IsTellingDirectTruth: false,
            IsUsingProperVeilSpeech: false, NpcId: npcId);

    /// <summary>
    /// Creates a context for telling direct truth to an Utgard NPC.
    /// </summary>
    /// <param name="currentState">The current Veil-Speech standing.</param>
    /// <param name="npcId">The Utgard NPC identifier.</param>
    /// <returns>A <see cref="VeilSpeechContext"/> configured for direct truth.</returns>
    /// <remarks>
    /// <para>
    /// <b>Warning:</b> Direct truth is offensive to Utgard. This will apply a -2d10
    /// penalty and worsen the character's Veil-Speech state.
    /// </para>
    /// </remarks>
    public static VeilSpeechContext ForDirectTruth(VeilSpeechState currentState, string npcId) =>
        new(currentState, IsUsingDeception: false, IsTellingDirectTruth: true,
            IsUsingProperVeilSpeech: false, NpcId: npcId);

    /// <summary>
    /// Creates a context for using proper Veil-Speech with an Utgard NPC.
    /// </summary>
    /// <param name="currentState">The current Veil-Speech standing.</param>
    /// <param name="npcId">The Utgard NPC identifier.</param>
    /// <returns>A <see cref="VeilSpeechContext"/> configured for proper Veil-Speech.</returns>
    /// <remarks>
    /// Proper Veil-Speech (truth layered in acceptable deception) is the preferred
    /// communication method and grants a +1d10 bonus.
    /// </remarks>
    public static VeilSpeechContext ForProperVeilSpeech(VeilSpeechState currentState, string npcId) =>
        new(currentState, IsUsingDeception: false, IsTellingDirectTruth: false,
            IsUsingProperVeilSpeech: true, NpcId: npcId);

    /// <summary>
    /// Creates a neutral context for non-Utgard interactions.
    /// </summary>
    /// <returns>A <see cref="VeilSpeechContext"/> with no Veil-Speech modifiers.</returns>
    /// <remarks>
    /// Use this factory method when creating a context for NPCs that are not Utgard,
    /// or when Veil-Speech mechanics do not apply to the interaction.
    /// </remarks>
    public static VeilSpeechContext NotApplicable() =>
        new(VeilSpeechState.Neutral, false, false, false, string.Empty);

    /// <summary>
    /// Creates an initial context for a new interaction with an Utgard NPC.
    /// </summary>
    /// <param name="npcId">The Utgard NPC identifier.</param>
    /// <returns>A <see cref="VeilSpeechContext"/> in neutral starting state.</returns>
    /// <remarks>
    /// This creates a context with no special behavior flags set, suitable for
    /// evaluating what the character chooses to do in the interaction.
    /// </remarks>
    public static VeilSpeechContext Initial(string npcId) =>
        new(VeilSpeechState.Neutral, false, false, false, npcId);

    /// <summary>
    /// Gets a value indicating whether this context applies Veil-Speech mechanics.
    /// </summary>
    /// <value>
    /// <c>true</c> if this context has a valid NPC ID; otherwise, <c>false</c>.
    /// </value>
    public bool IsApplicable => !string.IsNullOrEmpty(NpcId);

    /// <summary>
    /// Gets a value indicating whether this context allows social interaction.
    /// </summary>
    /// <value>
    /// <c>true</c> if interaction is possible; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Interaction is blocked when in <see cref="VeilSpeechState.DeepOffense"/> state.
    /// </remarks>
    public bool AllowsInteraction => CurrentState.AllowsInteraction();

    /// <summary>
    /// Gets a value indicating whether this interaction will offend the NPC.
    /// </summary>
    /// <value>
    /// <c>true</c> if telling direct truth; otherwise, <c>false</c>.
    /// </value>
    public bool WillCauseOffense => IsTellingDirectTruth;

    /// <summary>
    /// Gets a value indicating whether this interaction will improve standing.
    /// </summary>
    /// <value>
    /// <c>true</c> if using proper Veil-Speech; otherwise, <c>false</c>.
    /// </value>
    public bool WillImproveStanding => IsUsingProperVeilSpeech;

    /// <summary>
    /// Calculates the DC adjustment for this Veil-Speech context.
    /// </summary>
    /// <returns>
    /// The DC adjustment. Negative values make the check easier.
    /// Returns -4 when using deception (culturally respected), 0 otherwise.
    /// </returns>
    /// <remarks>
    /// The DC reduction only applies to active Deception checks. Other interaction
    /// types are not affected by this modifier.
    /// </remarks>
    public int GetDcAdjustment() => IsUsingDeception ? -DeceptionDcReduction : 0;

    /// <summary>
    /// Calculates the dice pool modifier for this Veil-Speech context.
    /// </summary>
    /// <returns>
    /// The number of dice to add (positive) or remove (negative) from the pool.
    /// <list type="bullet">
    ///   <item><description>Direct truth: -2d10</description></item>
    ///   <item><description>Proper Veil-Speech: +1d10</description></item>
    ///   <item><description>Otherwise: Based on current state</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// The modifier calculation priority is:
    /// <list type="number">
    ///   <item><description>Direct truth penalty (highest priority, always applied if true)</description></item>
    ///   <item><description>Proper Veil-Speech bonus</description></item>
    ///   <item><description>Current state modifier (fallback)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public int GetDiceModifier()
    {
        // Priority: Direct truth penalty > Proper Veil-Speech bonus > State modifier
        if (IsTellingDirectTruth)
            return -DirectTruthPenalty;
        if (IsUsingProperVeilSpeech)
            return ProperVeilSpeechBonus;
        return CurrentState.GetDiceModifier();
    }

    /// <summary>
    /// Determines the resulting Veil-Speech state after this interaction.
    /// </summary>
    /// <returns>The state that will result from this interaction.</returns>
    /// <remarks>
    /// <para>
    /// State transitions:
    /// <list type="bullet">
    ///   <item><description>Direct truth: State degrades (Neutral → Offended, Offended → DeepOffense)</description></item>
    ///   <item><description>Proper Veil-Speech: State improves (Neutral → Respected, Offended → Neutral)</description></item>
    ///   <item><description>Neutral interaction: No change</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public VeilSpeechState GetResultingState()
    {
        if (IsTellingDirectTruth)
            return CurrentState.GetStateAfterDirectTruth();
        if (IsUsingProperVeilSpeech)
            return CurrentState.GetStateAfterProperVeilSpeech();
        return CurrentState;
    }

    /// <summary>
    /// Gets a description of the current interaction approach.
    /// </summary>
    /// <returns>A human-readable description of what the character is doing.</returns>
    public string GetApproachDescription() => (IsTellingDirectTruth, IsUsingProperVeilSpeech, IsUsingDeception) switch
    {
        (true, _, _) => "Speaking direct truth (offensive to Utgard)",
        (_, true, _) => "Using proper Veil-Speech (respected)",
        (_, _, true) => "Using deception (culturally expected)",
        _ => "Neutral communication"
    };

    /// <summary>
    /// Gets a description suitable for modifier display.
    /// </summary>
    /// <returns>A formatted string showing the modifier effect.</returns>
    private string GetModifierDescription() => (IsTellingDirectTruth, IsUsingProperVeilSpeech, IsUsingDeception) switch
    {
        (true, _, _) => $"Direct truth offends (-{DirectTruthPenalty}d10)",
        (_, true, _) => $"Proper Veil-Speech (+{ProperVeilSpeechBonus}d10)",
        (_, _, true) => $"Deception respected (DC -{DeceptionDcReduction})",
        _ => CurrentState.GetDescription()
    };

    /// <summary>
    /// Converts this context to a <see cref="SocialModifier"/> for integration with the social context.
    /// </summary>
    /// <returns>A <see cref="SocialModifier"/> representing this Veil-Speech context.</returns>
    /// <remarks>
    /// <para>
    /// This method enables seamless integration with the existing <see cref="SocialModifier"/> system
    /// from v0.15.3a. The returned modifier includes both dice and DC adjustments.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var context = VeilSpeechContext.ForDeception(VeilSpeechState.Neutral, "npc-001");
    /// var modifier = context.ToSocialModifier();
    /// // modifier.DcModifier == -4
    /// // modifier.ModifierType == SocialModifierType.Cultural
    /// </code>
    /// </example>
    public SocialModifier ToSocialModifier() => new(
        Source: "Veil-Speech",
        Description: GetModifierDescription(),
        DiceModifier: GetDiceModifier(),
        DcModifier: GetDcAdjustment(),
        ModifierType: SocialModifierType.Cultural,
        AppliesToInteractionTypes: null); // Applies to all interaction types

    /// <summary>
    /// Creates a copy of this context with the state updated to the resulting state.
    /// </summary>
    /// <returns>A new <see cref="VeilSpeechContext"/> with the updated state.</returns>
    /// <remarks>
    /// This is useful for carrying the context forward after an interaction completes.
    /// </remarks>
    public VeilSpeechContext WithResultingState() =>
        this with { CurrentState = GetResultingState() };

    /// <summary>
    /// Creates a copy with a different approach (resets interaction flags).
    /// </summary>
    /// <param name="isUsingDeception">Whether using deception.</param>
    /// <param name="isTellingDirectTruth">Whether telling direct truth.</param>
    /// <param name="isUsingProperVeilSpeech">Whether using proper Veil-Speech.</param>
    /// <returns>A new <see cref="VeilSpeechContext"/> with updated approach.</returns>
    public VeilSpeechContext WithApproach(
        bool isUsingDeception = false,
        bool isTellingDirectTruth = false,
        bool isUsingProperVeilSpeech = false) =>
        this with
        {
            IsUsingDeception = isUsingDeception,
            IsTellingDirectTruth = isTellingDirectTruth,
            IsUsingProperVeilSpeech = isUsingProperVeilSpeech
        };

    /// <summary>
    /// Gets a formatted display string for UI presentation.
    /// </summary>
    /// <returns>A formatted string showing the context details.</returns>
    public string ToDisplayString()
    {
        var parts = new List<string> { $"State: {CurrentState.GetDisplayName()}" };

        if (IsUsingDeception)
            parts.Add($"DC {GetDcAdjustment():+0;-0}");

        var diceModifier = GetDiceModifier();
        if (diceModifier != 0)
            parts.Add($"{diceModifier:+0;-0}d10");

        return string.Join(", ", parts);
    }

    /// <summary>
    /// Gets a summary suitable for logging.
    /// </summary>
    /// <returns>A detailed string representation for debugging.</returns>
    public string GetSummary() =>
        $"VeilSpeechContext[NPC={NpcId}, State={CurrentState}, " +
        $"Deception={IsUsingDeception}, DirectTruth={IsTellingDirectTruth}, " +
        $"ProperVeil={IsUsingProperVeilSpeech}, Dice={GetDiceModifier():+0;-0}, DC={GetDcAdjustment():+0;-0}]";

    /// <inheritdoc/>
    public override string ToString() => ToDisplayString();
}
