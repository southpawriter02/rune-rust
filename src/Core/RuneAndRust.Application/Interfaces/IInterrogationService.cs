// ------------------------------------------------------------------------------
// <copyright file="IInterrogationService.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Service interface for conducting multi-round interrogation sessions,
// including initialization, round execution, information extraction, and finalization.
// Part of v0.15.3f Interrogation System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service interface for conducting multi-round interrogation sessions.
/// </summary>
/// <remarks>
/// <para>
/// The interrogation service orchestrates the complete interrogation workflow:
/// </para>
/// <list type="bullet">
///   <item><description>Initialization: Creates a session with subject resistance based on WILL</description></item>
///   <item><description>Round execution: Processes method selection and skill checks</description></item>
///   <item><description>Resistance tracking: Depletes resistance on successful checks</description></item>
///   <item><description>Information extraction: Generates information with reliability ratings</description></item>
///   <item><description>Finalization: Compiles results with all costs and consequences</description></item>
/// </list>
/// <para>
/// The service supports five interrogation methods:
/// </para>
/// <list type="bullet">
///   <item><description>GoodCop: Empathetic approach, DC 14, 95% reliable, 30 min per round</description></item>
///   <item><description>BadCop: Aggressive approach, DC 12, 80% reliable, 15 min per round, -5 disposition</description></item>
///   <item><description>Deception: Trickery approach, DC 16, 70% reliable, 20 min per round</description></item>
///   <item><description>Bribery: Incentive approach, DC 10, 90% reliable, 10 min per round, costs gold</description></item>
///   <item><description>Torture: Extreme approach, DC = WILL×2, 50% reliable, 60 min per round, severe consequences</description></item>
/// </list>
/// <para>
/// WARNING: Using Torture at ANY point during an interrogation caps maximum
/// information reliability at 60%, regardless of other methods used.
/// </para>
/// </remarks>
public interface IInterrogationService
{
    /// <summary>
    /// Initializes a new interrogation session.
    /// </summary>
    /// <param name="interrogatorId">The unique identifier of the character conducting the interrogation.</param>
    /// <param name="subjectId">The unique identifier of the NPC being interrogated.</param>
    /// <param name="subjectWill">The subject's WILL attribute value (1-10+).</param>
    /// <param name="resistanceModifiers">
    /// Additional resistance modifiers from training, loyalty, or circumstances.
    /// Positive values increase resistance, negative values decrease it.
    /// </param>
    /// <returns>An initialized <see cref="InterrogationState"/> ready for rounds to begin.</returns>
    /// <remarks>
    /// <para>
    /// Initialization determines:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Resistance level (Minimal to Extreme) based on WILL</description></item>
    ///   <item><description>Initial resistance value (number of successful checks required)</description></item>
    ///   <item><description>Session ID for tracking</description></item>
    /// </list>
    /// <para>
    /// Resistance mapping by WILL:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>WILL 1-2: Minimal (1 check)</description></item>
    ///   <item><description>WILL 3-4: Low (2-3 checks)</description></item>
    ///   <item><description>WILL 5-6: Moderate (4-5 checks)</description></item>
    ///   <item><description>WILL 7-8: High (6-8 checks)</description></item>
    ///   <item><description>WILL 9+: Extreme (10+ checks)</description></item>
    /// </list>
    /// </remarks>
    InterrogationState InitializeInterrogation(
        string interrogatorId,
        string subjectId,
        int subjectWill,
        int resistanceModifiers = 0);

    /// <summary>
    /// Initializes a new interrogation session asynchronously.
    /// </summary>
    /// <param name="interrogatorId">The unique identifier of the character conducting the interrogation.</param>
    /// <param name="subjectId">The unique identifier of the NPC being interrogated.</param>
    /// <param name="subjectWill">The subject's WILL attribute value (1-10+).</param>
    /// <param name="resistanceModifiers">
    /// Additional resistance modifiers from training, loyalty, or circumstances.
    /// </param>
    /// <returns>A task that resolves to an initialized <see cref="InterrogationState"/>.</returns>
    /// <remarks>
    /// Async overload that allows for loading NPC data, checking prerequisites,
    /// and logging the session start.
    /// </remarks>
    Task<InterrogationState> InitializeInterrogationAsync(
        string interrogatorId,
        string subjectId,
        int subjectWill,
        int resistanceModifiers = 0);

    /// <summary>
    /// Assesses a subject's resistance level based on WILL and modifiers.
    /// </summary>
    /// <param name="subjectWill">The subject's WILL attribute value.</param>
    /// <param name="resistanceModifiers">
    /// Additional resistance modifiers. These contribute at half value to
    /// effective WILL for resistance calculation.
    /// </param>
    /// <returns>The <see cref="SubjectResistance"/> assessment (Minimal to Extreme).</returns>
    /// <remarks>
    /// <para>
    /// This method calculates the resistance level without creating a full session.
    /// Useful for previewing interrogation difficulty before committing.
    /// </para>
    /// <para>
    /// The effective WILL used for resistance calculation is:
    /// <c>effectiveWill = subjectWill + (resistanceModifiers / 2)</c>
    /// </para>
    /// <para>
    /// Common resistance modifiers:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>+2: Trained to resist interrogation</description></item>
    ///   <item><description>+4: Fanatically loyal/religious</description></item>
    ///   <item><description>-2: Subject is injured or exhausted</description></item>
    ///   <item><description>-2: Previous interrogation has weakened resolve</description></item>
    /// </list>
    /// </remarks>
    SubjectResistance AssessResistance(int subjectWill, int resistanceModifiers = 0);

    /// <summary>
    /// Conducts a single round of interrogation.
    /// </summary>
    /// <param name="context">
    /// The <see cref="InterrogationContext"/> containing all information needed
    /// for this round, including method selection and modifiers.
    /// </param>
    /// <returns>
    /// An <see cref="InterrogationRound"/> containing the complete outcome
    /// of this round, including resistance change and side effects.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Round execution flow:
    /// </para>
    /// <list type="number">
    ///   <item><description>Calculate effective DC based on method and modifiers</description></item>
    ///   <item><description>Calculate dice pool (attribute + skill + bonuses)</description></item>
    ///   <item><description>Roll dice and determine outcome</description></item>
    ///   <item><description>Apply resistance change on success (-1)</description></item>
    ///   <item><description>Apply side effects (disposition, reputation, resources)</description></item>
    ///   <item><description>Handle fumble consequences if applicable</description></item>
    ///   <item><description>Update interrogation state</description></item>
    /// </list>
    /// <para>
    /// Success on a round reduces the subject's resistance by 1. When resistance
    /// reaches 0, the subject is broken and information can be extracted.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the interrogation is already in a terminal state.
    /// </exception>
    InterrogationRound ConductRound(InterrogationContext context);

    /// <summary>
    /// Conducts a single round of interrogation asynchronously.
    /// </summary>
    /// <param name="context">The interrogation context for this round.</param>
    /// <returns>A task that resolves to the <see cref="InterrogationRound"/> outcome.</returns>
    /// <remarks>
    /// Async overload that allows for delegating to underlying services
    /// (Persuasion, Deception, Intimidation) as needed.
    /// </remarks>
    Task<InterrogationRound> ConductRoundAsync(InterrogationContext context);

    /// <summary>
    /// Calculates the bribery cost for a round based on subject resistance.
    /// </summary>
    /// <param name="resistanceLevel">The subject's resistance level.</param>
    /// <returns>The gold cost for a bribery attempt.</returns>
    /// <remarks>
    /// <para>
    /// Base bribery costs by resistance level:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Minimal: 15-25 gold</description></item>
    ///   <item><description>Low: 35-50 gold</description></item>
    ///   <item><description>Moderate: 75-100 gold</description></item>
    ///   <item><description>High: 150-200 gold</description></item>
    ///   <item><description>Extreme: 300-500 gold</description></item>
    /// </list>
    /// <para>
    /// The actual cost includes ±20% variance for game variety.
    /// </para>
    /// </remarks>
    int CalculateBriberyCost(SubjectResistance resistanceLevel);

    /// <summary>
    /// Calculates the Torture DC based on subject WILL and modifiers.
    /// </summary>
    /// <param name="subjectWill">The subject's WILL attribute value.</param>
    /// <param name="modifiers">
    /// Additional DC modifiers (e.g., +4 for trained operative, +6 for fanatic).
    /// </param>
    /// <returns>The DC for a torture attempt.</returns>
    /// <remarks>
    /// <para>
    /// Torture DC calculation:
    /// <c>DC = (subjectWill × 2) + modifiers</c>
    /// </para>
    /// <para>
    /// This creates a unique mechanic where Torture is especially difficult
    /// against strong-willed subjects. A subject with WILL 8 has DC 16, while
    /// WILL 5 has DC 10.
    /// </para>
    /// <para>
    /// Common modifiers:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>+4: Trained operative (resistance training)</description></item>
    ///   <item><description>+6: Fanatical believer (welcomes martyrdom)</description></item>
    ///   <item><description>-2: Already tortured (diminishing returns)</description></item>
    ///   <item><description>-2: Subject is injured or exhausted</description></item>
    /// </list>
    /// </remarks>
    int CalculateTortureDc(int subjectWill, int modifiers = 0);

    /// <summary>
    /// Extracts information from a broken subject.
    /// </summary>
    /// <param name="state">
    /// The interrogation state (must have status <see cref="InterrogationStatus.SubjectBroken"/>).
    /// </param>
    /// <param name="topics">The topics to ask about (e.g., "location", "allies", "plans").</param>
    /// <returns>
    /// A read-only list of <see cref="InformationGained"/> containing extracted
    /// information with reliability ratings.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Information reliability is determined by the primary method used during
    /// the interrogation (the method used most frequently):
    /// </para>
    /// <list type="bullet">
    ///   <item><description>GoodCop: 95% reliable</description></item>
    ///   <item><description>Bribery: 90% reliable</description></item>
    ///   <item><description>BadCop: 80% reliable</description></item>
    ///   <item><description>Deception: 70% reliable</description></item>
    ///   <item><description>Torture: 50% reliable</description></item>
    /// </list>
    /// <para>
    /// WARNING: If Torture was used at ANY point during the interrogation,
    /// maximum reliability is capped at 60%, regardless of primary method.
    /// </para>
    /// <para>
    /// For each piece of information, a d100 is rolled against reliability.
    /// If the roll exceeds reliability, the information is false or misleading.
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when state is not in SubjectBroken status.
    /// </exception>
    IReadOnlyList<InformationGained> ExtractInformation(
        InterrogationState state,
        IReadOnlyList<string> topics);

    /// <summary>
    /// Extracts information from a broken subject asynchronously.
    /// </summary>
    /// <param name="state">The interrogation state (must be SubjectBroken).</param>
    /// <param name="topics">The topics to ask about.</param>
    /// <returns>
    /// A task that resolves to a read-only list of <see cref="InformationGained"/>.
    /// </returns>
    /// <remarks>
    /// Async overload that allows for looking up actual information from
    /// game state based on what the NPC knows.
    /// </remarks>
    Task<IReadOnlyList<InformationGained>> ExtractInformationAsync(
        InterrogationState state,
        IReadOnlyList<string> topics);

    /// <summary>
    /// Completes the interrogation and generates final results.
    /// </summary>
    /// <param name="state">The interrogation state to finalize.</param>
    /// <returns>
    /// The complete <see cref="InterrogationResult"/> containing all outcomes,
    /// costs, and extracted information.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Finalization aggregates:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Final status (SubjectBroken, SubjectResisting, Abandoned)</description></item>
    ///   <item><description>All information gained with reliability ratings</description></item>
    ///   <item><description>Total disposition change across all rounds</description></item>
    ///   <item><description>Total reputation cost (primarily from Torture)</description></item>
    ///   <item><description>Total resource cost (from Bribery)</description></item>
    ///   <item><description>Complete round history</description></item>
    ///   <item><description>Narrative summary</description></item>
    /// </list>
    /// <para>
    /// The result includes a flag indicating if Torture was used and whether
    /// the subject is now traumatized (permanent condition).
    /// </para>
    /// </remarks>
    InterrogationResult CompleteInterrogation(InterrogationState state);

    /// <summary>
    /// Completes the interrogation asynchronously.
    /// </summary>
    /// <param name="state">The interrogation state to finalize.</param>
    /// <returns>A task that resolves to the complete <see cref="InterrogationResult"/>.</returns>
    /// <remarks>
    /// Async overload that allows for persisting results, updating NPC state,
    /// and logging the session completion.
    /// </remarks>
    Task<InterrogationResult> CompleteInterrogationAsync(InterrogationState state);

    /// <summary>
    /// Abandons an in-progress interrogation.
    /// </summary>
    /// <param name="state">The interrogation state to abandon.</param>
    /// <remarks>
    /// <para>
    /// Abandoning an interrogation:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Sets status to Abandoned</description></item>
    ///   <item><description>Preserves all costs incurred (reputation, resources, disposition)</description></item>
    ///   <item><description>No information is extracted</description></item>
    /// </list>
    /// <para>
    /// Note: If Torture was used before abandonment, the subject still gains
    /// the [Traumatized] condition and reputation costs still apply.
    /// </para>
    /// </remarks>
    void AbandonInterrogation(InterrogationState state);

    /// <summary>
    /// Abandons an in-progress interrogation asynchronously.
    /// </summary>
    /// <param name="state">The interrogation state to abandon.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// Async overload that allows for logging and state persistence.
    /// </remarks>
    Task AbandonInterrogationAsync(InterrogationState state);

    /// <summary>
    /// Gets available methods and their current viability.
    /// </summary>
    /// <param name="state">The current interrogation state.</param>
    /// <param name="availableGold">Gold available for bribery.</param>
    /// <returns>
    /// A read-only dictionary mapping each <see cref="InterrogationMethod"/>
    /// to a boolean indicating whether it can be used.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Method availability:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>GoodCop: Always available</description></item>
    ///   <item><description>BadCop: Always available</description></item>
    ///   <item><description>Deception: Always available</description></item>
    ///   <item><description>Bribery: Requires sufficient gold (based on resistance level)</description></item>
    ///   <item><description>Torture: Always available (but with severe consequences)</description></item>
    /// </list>
    /// <para>
    /// All methods become unavailable once the interrogation reaches a terminal state.
    /// </para>
    /// </remarks>
    IReadOnlyDictionary<InterrogationMethod, bool> GetAvailableMethods(
        InterrogationState state,
        int availableGold);

    /// <summary>
    /// Builds an interrogation context for the next round.
    /// </summary>
    /// <param name="state">The current interrogation state.</param>
    /// <param name="selectedMethod">The method selected for the next round.</param>
    /// <param name="interrogatorAttribute">The interrogator's relevant attribute value.</param>
    /// <param name="interrogatorRhetoric">The interrogator's Rhetoric skill level.</param>
    /// <param name="subjectWits">The subject's WITS attribute (for Deception defense).</param>
    /// <param name="availableGold">Gold available for Bribery.</param>
    /// <param name="bonusDice">Any bonus dice from items, abilities, or circumstances.</param>
    /// <param name="dcModifier">DC modifiers from circumstances.</param>
    /// <param name="useMight">Whether to use MIGHT instead of WILL for BadCop/Torture.</param>
    /// <returns>A fully configured <see cref="InterrogationContext"/>.</returns>
    /// <remarks>
    /// <para>
    /// Context building includes:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Creating the base social context</description></item>
    ///   <item><description>Setting up method-specific parameters</description></item>
    ///   <item><description>Calculating effective DC and dice pool</description></item>
    ///   <item><description>Validating method availability</description></item>
    /// </list>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the selected method is not available (e.g., insufficient gold for Bribery).
    /// </exception>
    InterrogationContext BuildContext(
        InterrogationState state,
        InterrogationMethod selectedMethod,
        int interrogatorAttribute,
        int interrogatorRhetoric,
        int subjectWits,
        int availableGold = 0,
        int bonusDice = 0,
        int dcModifier = 0,
        bool useMight = false);

    /// <summary>
    /// Builds an interrogation context asynchronously.
    /// </summary>
    /// <param name="state">The current interrogation state.</param>
    /// <param name="selectedMethod">The method selected for the next round.</param>
    /// <param name="interrogatorId">The ID of the interrogator (to fetch attributes).</param>
    /// <param name="bonusDice">Any bonus dice.</param>
    /// <param name="dcModifier">DC modifiers.</param>
    /// <param name="useMight">Whether to use MIGHT instead of WILL.</param>
    /// <returns>A task that resolves to a fully configured <see cref="InterrogationContext"/>.</returns>
    /// <remarks>
    /// Async overload that fetches character attributes and gold automatically.
    /// </remarks>
    Task<InterrogationContext> BuildContextAsync(
        InterrogationState state,
        InterrogationMethod selectedMethod,
        string interrogatorId,
        int bonusDice = 0,
        int dcModifier = 0,
        bool useMight = false);

    /// <summary>
    /// Gets the current state summary for display.
    /// </summary>
    /// <param name="state">The current interrogation state.</param>
    /// <returns>A formatted string summarizing the current interrogation state.</returns>
    /// <remarks>
    /// The summary includes:
    /// <list type="bullet">
    ///   <item><description>Current round number</description></item>
    ///   <item><description>Remaining resistance</description></item>
    ///   <item><description>Primary method used</description></item>
    ///   <item><description>Cumulative costs (disposition, reputation, resources)</description></item>
    ///   <item><description>Current status</description></item>
    /// </list>
    /// </remarks>
    string GetStateSummary(InterrogationState state);

    /// <summary>
    /// Gets tactical advice for the current situation.
    /// </summary>
    /// <param name="state">The current interrogation state.</param>
    /// <param name="availableGold">Gold available for bribery.</param>
    /// <returns>
    /// A read-only list of tactical suggestions based on current state.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Provides context-aware advice such as:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Method recommendations based on subject resistance</description></item>
    ///   <item><description>Warnings about Torture consequences</description></item>
    ///   <item><description>Suggestions for mixing methods</description></item>
    ///   <item><description>Resource management for Bribery</description></item>
    /// </list>
    /// </remarks>
    IReadOnlyList<string> GetTacticalAdvice(InterrogationState state, int availableGold);

    /// <summary>
    /// Retrieves an active interrogation by ID.
    /// </summary>
    /// <param name="interrogationId">The unique identifier of the interrogation.</param>
    /// <returns>
    /// A task that resolves to the <see cref="InterrogationState"/> if found,
    /// or null if no active interrogation exists with that ID.
    /// </returns>
    Task<InterrogationState?> GetActiveInterrogationAsync(string interrogationId);

    /// <summary>
    /// Gets the maximum rounds allowed for a given resistance level.
    /// </summary>
    /// <param name="resistanceLevel">The subject's resistance level.</param>
    /// <returns>The maximum number of rounds before the subject resists.</returns>
    /// <remarks>
    /// <para>
    /// Maximum rounds by resistance level:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Minimal: 3 rounds</description></item>
    ///   <item><description>Low: 6 rounds</description></item>
    ///   <item><description>Moderate: 10 rounds</description></item>
    ///   <item><description>High: 15 rounds</description></item>
    ///   <item><description>Extreme: 20 rounds</description></item>
    /// </list>
    /// <para>
    /// If the maximum rounds are reached without breaking the subject,
    /// the interrogation ends with status SubjectResisting.
    /// </para>
    /// </remarks>
    int GetMaxRounds(SubjectResistance resistanceLevel);

    /// <summary>
    /// Handles the [Subject Broken] fumble consequence during Torture.
    /// </summary>
    /// <param name="state">The current interrogation state.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The state will be
    /// updated to reflect the fumble consequence.
    /// </returns>
    /// <remarks>
    /// <para>
    /// [Subject Broken] fumble consequence:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Subject dies, goes insane, or becomes catatonic</description></item>
    ///   <item><description>No information can be extracted</description></item>
    ///   <item><description>Additional -20 reputation penalty</description></item>
    ///   <item><description>Interrogation ends immediately</description></item>
    /// </list>
    /// <para>
    /// This is the most severe consequence in the interrogation system and
    /// represents the ultimate failure of the Torture method.
    /// </para>
    /// </remarks>
    Task HandleSubjectBrokenFumbleAsync(InterrogationState state);
}
