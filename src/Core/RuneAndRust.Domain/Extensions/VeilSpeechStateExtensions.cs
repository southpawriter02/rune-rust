// ------------------------------------------------------------------------------
// <copyright file="VeilSpeechStateExtensions.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Extension methods for the VeilSpeechState enum providing dice modifiers, DC
// adjustments, display names, and state transition information.
// Part of v0.15.3g Cultural Protocols implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Extensions;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Extension methods for the <see cref="VeilSpeechState"/> enum providing dice modifiers,
/// DC adjustments, display names, and state transition information.
/// </summary>
public static class VeilSpeechStateExtensions
{
    /// <summary>
    /// The DC reduction applied when using deception with Utgard NPCs.
    /// </summary>
    /// <remarks>
    /// Deception is culturally respected by Utgard, making it easier to deceive them
    /// as long as the deception follows proper Veil-Speech form.
    /// </remarks>
    public const int DeceptionDcReduction = 4;

    /// <summary>
    /// The dice bonus for using proper Veil-Speech.
    /// </summary>
    public const int ProperVeilSpeechBonus = 1;

    /// <summary>
    /// The dice penalty for telling direct truth to Utgard.
    /// </summary>
    public const int DirectTruthPenalty = 2;

    /// <summary>
    /// Gets the dice pool modifier associated with this Veil-Speech state.
    /// </summary>
    /// <param name="state">The Veil-Speech state.</param>
    /// <returns>
    /// The number of dice to add (positive) or remove (negative) from the pool.
    /// <list type="bullet">
    ///   <item><description><see cref="VeilSpeechState.Neutral"/>: 0</description></item>
    ///   <item><description><see cref="VeilSpeechState.Respected"/>: +1</description></item>
    ///   <item><description><see cref="VeilSpeechState.Offended"/>: -2</description></item>
    ///   <item><description><see cref="VeilSpeechState.DeepOffense"/>: -4</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown state is provided.
    /// </exception>
    public static int GetDiceModifier(this VeilSpeechState state)
    {
        return state switch
        {
            VeilSpeechState.Neutral => 0,
            VeilSpeechState.Respected => 1,
            VeilSpeechState.Offended => -2,
            VeilSpeechState.DeepOffense => -4,
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "Unknown Veil-Speech state")
        };
    }

    /// <summary>
    /// Gets the DC modifier for deception attempts based on current Veil-Speech state.
    /// </summary>
    /// <param name="state">The Veil-Speech state.</param>
    /// <returns>
    /// The DC adjustment (negative values make deception easier).
    /// Deception is always easier with Utgard (-4 DC) unless deeply offended.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This modifier applies specifically to Deception interaction type with Utgard NPCs.
    /// The Utgard respect deception as a form of social sophistication.
    /// </para>
    /// <para>
    /// When in DeepOffense state, this bonus is lost as the NPC refuses to engage.
    /// </para>
    /// </remarks>
    public static int GetDcModifier(this VeilSpeechState state)
    {
        return state switch
        {
            VeilSpeechState.DeepOffense => 0, // No deception bonus when deeply offended
            _ => -DeceptionDcReduction // Deception is respected (-4 DC)
        };
    }

    /// <summary>
    /// Gets the display name for UI presentation.
    /// </summary>
    /// <param name="state">The Veil-Speech state.</param>
    /// <returns>Human-readable state name.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown state is provided.
    /// </exception>
    public static string GetDisplayName(this VeilSpeechState state)
    {
        return state switch
        {
            VeilSpeechState.Neutral => "Neutral",
            VeilSpeechState.Respected => "Respected",
            VeilSpeechState.Offended => "Offended",
            VeilSpeechState.DeepOffense => "Deeply Offended",
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "Unknown Veil-Speech state")
        };
    }

    /// <summary>
    /// Gets a human-readable description of this state including the modifier.
    /// </summary>
    /// <param name="state">The Veil-Speech state.</param>
    /// <returns>A descriptive string suitable for display.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown state is provided.
    /// </exception>
    public static string GetDescription(this VeilSpeechState state)
    {
        return state switch
        {
            VeilSpeechState.Neutral => "Neutral",
            VeilSpeechState.Respected => "Respected (+1d10)",
            VeilSpeechState.Offended => "Offended (-2d10)",
            VeilSpeechState.DeepOffense => "Deeply Offended (interaction blocked)",
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "Unknown Veil-Speech state")
        };
    }

    /// <summary>
    /// Gets a narrative description explaining the character's standing.
    /// </summary>
    /// <param name="state">The Veil-Speech state.</param>
    /// <returns>Narrative text describing the social situation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown state is provided.
    /// </exception>
    public static string GetNarrativeDescription(this VeilSpeechState state)
    {
        return state switch
        {
            VeilSpeechState.Neutral =>
                "The Utgard regards you with measured interest, evaluating your " +
                "social sophistication through your speech.",
            VeilSpeechState.Respected =>
                "Your skillful use of Veil-Speech has earned the Utgard's respect. " +
                "They view you as a cultured individual worthy of serious engagement.",
            VeilSpeechState.Offended =>
                "Your direct speech has offended the Utgard's sensibilities. They " +
                "view you as either barbaric or deliberately insulting.",
            VeilSpeechState.DeepOffense =>
                "Your repeated affronts have deeply insulted the Utgard. They may " +
                "refuse further interaction until significant amends are made.",
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "Unknown Veil-Speech state")
        };
    }

    /// <summary>
    /// Determines if this state represents an offense condition.
    /// </summary>
    /// <param name="state">The Veil-Speech state.</param>
    /// <returns>
    /// <c>true</c> if the character has offended the NPC;
    /// <c>false</c> otherwise.
    /// </returns>
    public static bool IsOffended(this VeilSpeechState state)
    {
        return state is VeilSpeechState.Offended or VeilSpeechState.DeepOffense;
    }

    /// <summary>
    /// Determines if this state represents the character being in good standing.
    /// </summary>
    /// <param name="state">The Veil-Speech state.</param>
    /// <returns>
    /// <c>true</c> if the character is respected or neutral;
    /// <c>false</c> if offended.
    /// </returns>
    public static bool IsRespected(this VeilSpeechState state)
    {
        return state == VeilSpeechState.Respected;
    }

    /// <summary>
    /// Determines if this state allows normal social interaction.
    /// </summary>
    /// <param name="state">The Veil-Speech state.</param>
    /// <returns>
    /// <c>true</c> if interaction is possible;
    /// <c>false</c> if the NPC refuses to engage.
    /// </returns>
    /// <remarks>
    /// Only <see cref="VeilSpeechState.DeepOffense"/> blocks interaction entirely.
    /// Other states allow interaction with appropriate modifiers.
    /// </remarks>
    public static bool AllowsInteraction(this VeilSpeechState state)
    {
        return state != VeilSpeechState.DeepOffense;
    }

    /// <summary>
    /// Determines if this state is recoverable through normal means.
    /// </summary>
    /// <param name="state">The Veil-Speech state.</param>
    /// <returns>
    /// <c>true</c> if recovery through apology or time is possible;
    /// <c>false</c> if extraordinary measures are required.
    /// </returns>
    /// <remarks>
    /// <see cref="VeilSpeechState.Offended"/> can be recovered through apology or time.
    /// <see cref="VeilSpeechState.DeepOffense"/> requires significant effort: gifts,
    /// intermediaries, or quest completion.
    /// </remarks>
    public static bool IsEasilyRecoverable(this VeilSpeechState state)
    {
        return state is VeilSpeechState.Neutral or VeilSpeechState.Respected
               or VeilSpeechState.Offended;
    }

    /// <summary>
    /// Gets the color hint for UI presentation of this state.
    /// </summary>
    /// <param name="state">The Veil-Speech state.</param>
    /// <returns>A suggested color name for visual indication.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown state is provided.
    /// </exception>
    public static string GetColorHint(this VeilSpeechState state)
    {
        return state switch
        {
            VeilSpeechState.Neutral => "White",
            VeilSpeechState.Respected => "Green",
            VeilSpeechState.Offended => "Orange",
            VeilSpeechState.DeepOffense => "Red",
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "Unknown Veil-Speech state")
        };
    }

    /// <summary>
    /// Gets the next state if direct truth is spoken.
    /// </summary>
    /// <param name="currentState">The current Veil-Speech state.</param>
    /// <returns>The resulting state after telling direct truth.</returns>
    /// <remarks>
    /// Speaking direct truth always worsens the relationship:
    /// <list type="bullet">
    ///   <item><description>Neutral/Respected → Offended</description></item>
    ///   <item><description>Offended → DeepOffense</description></item>
    ///   <item><description>DeepOffense → DeepOffense (cannot get worse)</description></item>
    /// </list>
    /// </remarks>
    public static VeilSpeechState GetStateAfterDirectTruth(this VeilSpeechState currentState)
    {
        return currentState switch
        {
            VeilSpeechState.Neutral => VeilSpeechState.Offended,
            VeilSpeechState.Respected => VeilSpeechState.Offended,
            VeilSpeechState.Offended => VeilSpeechState.DeepOffense,
            VeilSpeechState.DeepOffense => VeilSpeechState.DeepOffense,
            _ => VeilSpeechState.Offended
        };
    }

    /// <summary>
    /// Gets the next state if proper Veil-Speech is used.
    /// </summary>
    /// <param name="currentState">The current Veil-Speech state.</param>
    /// <returns>The resulting state after using proper Veil-Speech.</returns>
    /// <remarks>
    /// Using proper Veil-Speech improves or maintains the relationship:
    /// <list type="bullet">
    ///   <item><description>Neutral → Respected</description></item>
    ///   <item><description>Respected → Respected (maintained)</description></item>
    ///   <item><description>Offended → Neutral (apology accepted)</description></item>
    ///   <item><description>DeepOffense → Offended (partial recovery)</description></item>
    /// </list>
    /// </remarks>
    public static VeilSpeechState GetStateAfterProperVeilSpeech(this VeilSpeechState currentState)
    {
        return currentState switch
        {
            VeilSpeechState.Neutral => VeilSpeechState.Respected,
            VeilSpeechState.Respected => VeilSpeechState.Respected,
            VeilSpeechState.Offended => VeilSpeechState.Neutral,
            VeilSpeechState.DeepOffense => VeilSpeechState.Offended,
            _ => VeilSpeechState.Neutral
        };
    }
}
