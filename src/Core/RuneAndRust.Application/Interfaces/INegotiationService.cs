// ------------------------------------------------------------------------------
// <copyright file="INegotiationService.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Service interface for managing multi-round negotiations, including
// initialization, round execution, concession handling, and finalization.
// Part of v0.15.3e Negotiation System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service interface for managing multi-round negotiations.
/// </summary>
/// <remarks>
/// <para>
/// The negotiation service orchestrates the complete negotiation workflow:
/// </para>
/// <list type="bullet">
///   <item><description>Initiation: Sets up the position track with starting positions</description></item>
///   <item><description>Round execution: Processes tactics and updates positions</description></item>
///   <item><description>Concession handling: Applies concessions and bonuses</description></item>
///   <item><description>Finalization: Generates the complete negotiation result</description></item>
/// </list>
/// <para>
/// The service delegates to underlying skill services (Persuasion, Deception,
/// Intimidation) based on the selected tactic each round.
/// </para>
/// </remarks>
public interface INegotiationService
{
    /// <summary>
    /// Initiates a new negotiation with an NPC.
    /// </summary>
    /// <param name="npcId">The unique identifier of the NPC to negotiate with.</param>
    /// <param name="requestComplexity">The complexity of what the player is asking for.</param>
    /// <returns>
    /// A task that resolves to the initialized <see cref="NegotiationPositionTrack"/>
    /// with starting positions set based on the request complexity and NPC disposition.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Initialization determines:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>PC starting position based on request complexity</description></item>
    ///   <item><description>NPC starting position based on disposition and complexity</description></item>
    ///   <item><description>Maximum rounds based on complexity</description></item>
    ///   <item><description>Initial gap between positions</description></item>
    /// </list>
    /// </remarks>
    Task<NegotiationPositionTrack> InitiateNegotiationAsync(
        string npcId,
        RequestComplexity requestComplexity);

    /// <summary>
    /// Executes a single round of negotiation.
    /// </summary>
    /// <param name="context">The negotiation context containing all information needed for this round.</param>
    /// <returns>
    /// A task that resolves to a <see cref="NegotiationRound"/> containing
    /// the outcome of this round, including position movements and costs.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Round execution based on tactic:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Persuade: Delegates to Persuasion service, no side effects</description></item>
    ///   <item><description>Deceive: Delegates to Deception service, incurs stress</description></item>
    ///   <item><description>Pressure: Delegates to Intimidation service, costs reputation</description></item>
    ///   <item><description>Concede: No check, voluntary position movement, grants bonus</description></item>
    /// </list>
    /// <para>
    /// After the round, the position track is updated with new positions based
    /// on the outcome, and the negotiation status may change.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the negotiation is already in a terminal state.
    /// </exception>
    Task<NegotiationRound> ExecuteRoundAsync(NegotiationContext context);

    /// <summary>
    /// Applies a concession and moves the player's position.
    /// </summary>
    /// <param name="positionTrack">The current negotiation position track.</param>
    /// <param name="concession">The concession being offered.</param>
    /// <returns>
    /// A task that resolves to the updated <see cref="NegotiationPositionTrack"/>
    /// with the concession applied.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Concession effects:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>PC position moves 1 step toward NPC (voluntary concession)</description></item>
    ///   <item><description>Concession bonus (+2d10, DC reduction) is stored for next check</description></item>
    ///   <item><description>Concession cost is applied (item consumed, debt created, etc.)</description></item>
    /// </list>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when positionTrack or concession is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the negotiation is in a terminal state.
    /// </exception>
    Task<NegotiationPositionTrack> ApplyConcessionAsync(
        NegotiationPositionTrack positionTrack,
        Concession concession);

    /// <summary>
    /// Checks if the negotiation has reached a terminal state.
    /// </summary>
    /// <param name="positionTrack">The position track to check.</param>
    /// <returns>
    /// True if the negotiation is complete (DealReached or Collapsed);
    /// otherwise, false.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Terminal conditions:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>DealReached: Gap = 0 (positions overlap)</description></item>
    ///   <item><description>Collapsed: Position reaches 8, fumble during crisis, or rounds exhausted</description></item>
    /// </list>
    /// </remarks>
    bool IsNegotiationComplete(NegotiationPositionTrack positionTrack);

    /// <summary>
    /// Finalizes a completed negotiation and returns the full result.
    /// </summary>
    /// <param name="positionTrack">The final position track.</param>
    /// <returns>
    /// A task that resolves to the complete <see cref="NegotiationResult"/>
    /// containing all outcomes, history, and consequences.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Finalization:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Generates deal terms (if successful)</description></item>
    ///   <item><description>Calculates final disposition change</description></item>
    ///   <item><description>Determines unlocked dialogue options</description></item>
    ///   <item><description>Aggregates total costs (stress, reputation)</description></item>
    ///   <item><description>Creates the complete result object</description></item>
    /// </list>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the negotiation is not yet complete.
    /// </exception>
    Task<NegotiationResult> FinalizeNegotiationAsync(NegotiationPositionTrack positionTrack);

    /// <summary>
    /// Gets available concession options for the player.
    /// </summary>
    /// <returns>
    /// A task that resolves to a list of available <see cref="Concession"/> options
    /// that the player can choose from.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Available concessions depend on:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Inventory: Items available to offer</description></item>
    ///   <item><description>Player state: Whether they can take risks or stake reputation</description></item>
    ///   <item><description>Information known: Secrets that can be traded</description></item>
    /// </list>
    /// <para>
    /// Some concession types (PromiseFavor, TakeRisk) are always available.
    /// Others (OfferItem, StakeReputation) require specific conditions.
    /// </para>
    /// </remarks>
    Task<IReadOnlyList<Concession>> GetAvailableConcessionsAsync();

    /// <summary>
    /// Calculates the initial NPC position based on disposition and request.
    /// </summary>
    /// <param name="npcId">The unique identifier of the NPC.</param>
    /// <param name="requestComplexity">The complexity of the request.</param>
    /// <returns>
    /// A task that resolves to the NPC's starting position on the 0-8 scale.
    /// </returns>
    /// <remarks>
    /// <para>
    /// NPC starting position is influenced by:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Base position from request complexity (typically 6)</description></item>
    ///   <item><description>Disposition modifier (allies start closer to compromise)</description></item>
    ///   <item><description>NPC personality traits</description></item>
    /// </list>
    /// </remarks>
    Task<int> CalculateNpcStartPositionAsync(string npcId, RequestComplexity requestComplexity);

    /// <summary>
    /// Gets the NPC's flexibility level.
    /// </summary>
    /// <param name="npcId">The unique identifier of the NPC.</param>
    /// <returns>
    /// A task that resolves to the NPC's flexibility level (1-3).
    /// </returns>
    /// <remarks>
    /// <para>
    /// Flexibility levels:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>1: Inflexible - stubborn, hard to move</description></item>
    ///   <item><description>2: Average - standard negotiation</description></item>
    ///   <item><description>3: Flexible - willing to compromise quickly</description></item>
    /// </list>
    /// <para>
    /// Flexibility affects how far the NPC position moves on successful checks.
    /// </para>
    /// </remarks>
    Task<int> GetNpcFlexibilityAsync(string npcId);

    /// <summary>
    /// Builds a negotiation context for the next round.
    /// </summary>
    /// <param name="positionTrack">The current position track.</param>
    /// <param name="selectedTactic">The tactic selected for the next round.</param>
    /// <param name="concessionOffer">The concession being offered, if using Concede tactic.</param>
    /// <returns>
    /// A task that resolves to a <see cref="NegotiationContext"/> ready for round execution.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Context building includes:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Creating the base social context with current modifiers</description></item>
    ///   <item><description>Including active concession bonuses</description></item>
    ///   <item><description>Fetching NPC flexibility</description></item>
    ///   <item><description>Validating the selected tactic</description></item>
    /// </list>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when positionTrack is null.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when using Concede tactic without providing a concession offer.
    /// </exception>
    Task<NegotiationContext> BuildContextAsync(
        NegotiationPositionTrack positionTrack,
        NegotiationTactic selectedTactic,
        Concession? concessionOffer = null);

    /// <summary>
    /// Gets the current state summary for display.
    /// </summary>
    /// <param name="positionTrack">The current position track.</param>
    /// <returns>A formatted string summarizing the current negotiation state.</returns>
    string GetStateSummary(NegotiationPositionTrack positionTrack);

    /// <summary>
    /// Gets tactical advice for the current situation.
    /// </summary>
    /// <param name="positionTrack">The current position track.</param>
    /// <returns>
    /// A task that resolves to a list of tactical suggestions based on current state.
    /// </returns>
    /// <remarks>
    /// Provides context-aware advice such as:
    /// <list type="bullet">
    ///   <item><description>Warnings about risky tactics during crisis</description></item>
    ///   <item><description>Suggestions for when to use concessions</description></item>
    ///   <item><description>Information about rounds remaining</description></item>
    /// </list>
    /// </remarks>
    Task<IReadOnlyList<string>> GetTacticalAdviceAsync(NegotiationPositionTrack positionTrack);

    /// <summary>
    /// Calculates the position movement for a given outcome.
    /// </summary>
    /// <param name="outcome">The skill check outcome.</param>
    /// <param name="npcFlexibility">The NPC's flexibility level.</param>
    /// <returns>The number of steps to move (positive value).</returns>
    /// <remarks>
    /// <para>
    /// Movement calculation:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Base: 1 step for regular success/failure, 2 for critical</description></item>
    ///   <item><description>Flexibility modifier: May increase NPC movement on success</description></item>
    /// </list>
    /// </remarks>
    int CalculatePositionMovement(SkillOutcome outcome, int npcFlexibility);

    /// <summary>
    /// Retrieves an active negotiation by ID.
    /// </summary>
    /// <param name="negotiationId">The unique identifier of the negotiation.</param>
    /// <returns>
    /// A task that resolves to the <see cref="NegotiationPositionTrack"/> if found,
    /// or null if no active negotiation exists with that ID.
    /// </returns>
    Task<NegotiationPositionTrack?> GetActiveNegotiationAsync(string negotiationId);

    /// <summary>
    /// Abandons an active negotiation without resolution.
    /// </summary>
    /// <param name="negotiationId">The unique identifier of the negotiation to abandon.</param>
    /// <returns>A task that completes when the negotiation is abandoned.</returns>
    /// <remarks>
    /// Abandoning a negotiation may have negative consequences such as
    /// disposition penalties with the NPC.
    /// </remarks>
    Task AbandonNegotiationAsync(string negotiationId);
}
