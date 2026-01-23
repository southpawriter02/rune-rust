// ------------------------------------------------------------------------------
// <copyright file="InterrogationState.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Tracks the complete state of an interrogation session including subject
// resistance, methods used, and information extracted.
// Part of v0.15.3f Interrogation System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Entities;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Tracks the complete state of an interrogation session including
/// subject resistance, methods used, and information extracted.
/// </summary>
/// <remarks>
/// <para>
/// An InterrogationState represents a single interrogation session between
/// an interrogator and a subject. It tracks:
/// <list type="bullet">
///   <item><description>Subject's remaining resistance (will to resist)</description></item>
///   <item><description>Methods used in each round</description></item>
///   <item><description>Cumulative effects (disposition, reputation, resources)</description></item>
///   <item><description>Information extracted upon breaking the subject</description></item>
/// </list>
/// </para>
/// <para>
/// The interrogation progresses through rounds until one of these conditions:
/// <list type="bullet">
///   <item><description>Resistance reaches 0: Subject is broken (success)</description></item>
///   <item><description>Max rounds reached: Subject resisted (failure)</description></item>
///   <item><description>Interrogator abandons: Session ended early (failure)</description></item>
///   <item><description>Subject dies/broken beyond recovery: Fumble during Torture</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class InterrogationState
{
    /// <summary>
    /// The history of all rounds conducted.
    /// </summary>
    private readonly List<InterrogationRound> _history = new();

    /// <summary>
    /// The information extracted from the subject.
    /// </summary>
    private readonly List<InformationGained> _informationGained = new();

    /// <summary>
    /// The methods used throughout the session (may contain duplicates).
    /// </summary>
    private readonly List<InterrogationMethod> _methodsUsed = new();

    /// <summary>
    /// Gets the unique identifier for this interrogation session.
    /// </summary>
    public required string InterrogationId { get; init; }

    /// <summary>
    /// Gets the ID of the character conducting the interrogation.
    /// </summary>
    public required string InterrogatorId { get; init; }

    /// <summary>
    /// Gets the ID of the NPC being interrogated.
    /// </summary>
    public required string SubjectId { get; init; }

    /// <summary>
    /// Gets the subject's WILL attribute value.
    /// </summary>
    /// <remarks>
    /// Used to calculate base resistance and Torture DC (WILL × 2).
    /// </remarks>
    public required int SubjectWill { get; init; }

    /// <summary>
    /// Gets the number of successful checks still required to break the subject.
    /// </summary>
    /// <remarks>
    /// Starts at a value determined by SubjectWill and modifiers.
    /// Reaches 0 when subject breaks.
    /// </remarks>
    public int ResistanceRemaining { get; private set; }

    /// <summary>
    /// Gets the initial resistance value before any checks.
    /// </summary>
    public required int InitialResistance { get; init; }

    /// <summary>
    /// Gets the resistance category of this subject.
    /// </summary>
    public required SubjectResistance ResistanceLevel { get; init; }

    /// <summary>
    /// Gets the current round number (1-based).
    /// </summary>
    /// <remarks>
    /// Incremented when a round is recorded. Round 0 means no rounds conducted yet.
    /// </remarks>
    public int RoundNumber { get; private set; }

    /// <summary>
    /// Gets the current status of the interrogation.
    /// </summary>
    public InterrogationStatus Status { get; private set; }

    /// <summary>
    /// Gets the cumulative disposition change from methods used.
    /// </summary>
    /// <remarks>
    /// Negative values indicate relationship damage with the subject.
    /// </remarks>
    public int TotalDispositionChange { get; private set; }

    /// <summary>
    /// Gets the cumulative reputation cost (primarily from Torture).
    /// </summary>
    public int TotalReputationCost { get; private set; }

    /// <summary>
    /// Gets the cumulative resource cost (from Bribery).
    /// </summary>
    public int TotalResourceCost { get; private set; }

    /// <summary>
    /// Gets a value indicating whether Torture has been used at any point.
    /// </summary>
    /// <remarks>
    /// This is significant because using Torture at ANY point caps the
    /// maximum information reliability at 60%, regardless of primary method.
    /// </remarks>
    public bool TortureUsed { get; private set; }

    /// <summary>
    /// Gets a read-only list of methods used (may contain duplicates).
    /// </summary>
    public IReadOnlyList<InterrogationMethod> MethodsUsed => _methodsUsed.AsReadOnly();

    /// <summary>
    /// Gets read-only access to the round history.
    /// </summary>
    public IReadOnlyList<InterrogationRound> History => _history.AsReadOnly();

    /// <summary>
    /// Gets read-only access to information gained.
    /// </summary>
    public IReadOnlyList<InformationGained> InformationGained => _informationGained.AsReadOnly();

    /// <summary>
    /// Initializes the interrogation state with starting values.
    /// </summary>
    /// <remarks>
    /// Must be called after creation to set up the initial state.
    /// Resets all cumulative values and clears history.
    /// </remarks>
    public void Initialize()
    {
        ResistanceRemaining = InitialResistance;
        RoundNumber = 0;
        Status = InterrogationStatus.NotStarted;
        TotalDispositionChange = 0;
        TotalReputationCost = 0;
        TotalResourceCost = 0;
        TortureUsed = false;
        _history.Clear();
        _informationGained.Clear();
        _methodsUsed.Clear();
    }

    /// <summary>
    /// Records a completed round and updates state accordingly.
    /// </summary>
    /// <param name="round">The round to record.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the interrogation has already ended.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method:
    /// <list type="bullet">
    ///   <item><description>Increments round number</description></item>
    ///   <item><description>Adds round to history</description></item>
    ///   <item><description>Applies resistance change</description></item>
    ///   <item><description>Accumulates disposition, reputation, and resource costs</description></item>
    ///   <item><description>Tracks Torture usage</description></item>
    ///   <item><description>Checks for subject broken condition</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public void RecordRound(InterrogationRound round)
    {
        if (Status.IsTerminal())
        {
            throw new InvalidOperationException(
                "Cannot record rounds on a terminal interrogation.");
        }

        // Increment round number
        RoundNumber++;

        // Add to history and methods used
        _history.Add(round);
        _methodsUsed.Add(round.MethodUsed);

        // Update status if this is the first round
        if (Status == InterrogationStatus.NotStarted)
        {
            Status = InterrogationStatus.InProgress;
        }

        // Apply resistance change
        ResistanceRemaining += round.ResistanceChange; // ResistanceChange is negative on success
        if (ResistanceRemaining < 0)
        {
            ResistanceRemaining = 0;
        }

        // Apply cumulative effects
        TotalDispositionChange += round.DispositionChange;
        TotalResourceCost += round.ResourceCost;

        // Track Torture usage
        if (round.MethodUsed == InterrogationMethod.Torture)
        {
            TortureUsed = true;
            TotalReputationCost += InterrogationMethod.Torture.GetReputationCost();
        }

        // Check for subject broken
        if (ResistanceRemaining == 0)
        {
            Status = InterrogationStatus.SubjectBroken;
        }
    }

    /// <summary>
    /// Adds extracted information to the session.
    /// </summary>
    /// <param name="info">The information to add.</param>
    /// <remarks>
    /// Information should only be added when the subject is broken.
    /// </remarks>
    public void AddInformation(InformationGained info)
    {
        _informationGained.Add(info);
    }

    /// <summary>
    /// Abandons the interrogation before completion.
    /// </summary>
    /// <remarks>
    /// Sets status to Abandoned. Any costs already incurred (reputation,
    /// resources, disposition) remain applied.
    /// </remarks>
    public void Abandon()
    {
        if (!Status.IsTerminal())
        {
            Status = InterrogationStatus.Abandoned;
        }
    }

    /// <summary>
    /// Marks the subject as successfully resisting (max rounds reached).
    /// </summary>
    /// <remarks>
    /// Called when the maximum allowed rounds have been conducted without
    /// breaking the subject's resistance.
    /// </remarks>
    public void MarkSubjectResisting()
    {
        if (!Status.IsTerminal())
        {
            Status = InterrogationStatus.SubjectResisting;
        }
    }

    /// <summary>
    /// Marks the subject as broken beyond recovery (Torture fumble).
    /// </summary>
    /// <param name="additionalReputationCost">Additional reputation loss.</param>
    /// <remarks>
    /// Called when a fumble occurs during Torture. The subject is permanently
    /// incapacitated (dead, insane, or catatonic) and no information can be extracted.
    /// </remarks>
    public void MarkSubjectBrokenBeyondRecovery(int additionalReputationCost = -20)
    {
        if (!Status.IsTerminal())
        {
            Status = InterrogationStatus.Abandoned; // Can't be SubjectBroken as no info extractable
            TotalReputationCost += additionalReputationCost;
        }
    }

    /// <summary>
    /// Gets the most frequently used method (primary method).
    /// </summary>
    /// <returns>The method used most often during the session.</returns>
    /// <remarks>
    /// Used to determine information reliability. If no methods have been used,
    /// returns GoodCop as the default.
    /// </remarks>
    public InterrogationMethod GetPrimaryMethod()
    {
        if (_methodsUsed.Count == 0)
        {
            return InterrogationMethod.GoodCop; // Default
        }

        return _methodsUsed
            .GroupBy(m => m)
            .OrderByDescending(g => g.Count())
            .First()
            .Key;
    }

    /// <summary>
    /// Calculates the reliability percentage based on methods used.
    /// </summary>
    /// <returns>The reliability percentage (0-100).</returns>
    /// <remarks>
    /// <para>
    /// Reliability is based on the primary method's reliability rating.
    /// However, if Torture was used at ANY point during the session,
    /// reliability is capped at 60%.
    /// </para>
    /// </remarks>
    public int CalculateReliability()
    {
        if (_methodsUsed.Count == 0)
        {
            return 0;
        }

        // If Torture was used at any point, cap reliability at 60%
        if (TortureUsed)
        {
            return Math.Min(60, GetPrimaryMethod().GetReliabilityPercent());
        }

        // Get primary method reliability
        var primaryMethod = GetPrimaryMethod();
        return primaryMethod.GetReliabilityPercent();
    }

    /// <summary>
    /// Gets a summary of the current interrogation state.
    /// </summary>
    /// <returns>A multi-line summary string.</returns>
    public string GetStateSummary()
    {
        var lines = new List<string>
        {
            $"Interrogation: {InterrogationId}",
            $"Status: {Status.GetDisplayName()}",
            $"Round: {RoundNumber}",
            $"Resistance: {ResistanceRemaining}/{InitialResistance} ({ResistanceLevel.GetDisplayName()})",
            $"Primary Method: {GetPrimaryMethod().GetDisplayName()}",
            $"Disposition Change: {TotalDispositionChange:+0;-0}",
            $"Reputation Cost: {TotalReputationCost}",
            $"Resource Cost: {TotalResourceCost} gold"
        };

        if (TortureUsed)
        {
            lines.Add("⚠️ Torture was used - reliability capped at 60%");
        }

        if (Status.IsSuccess())
        {
            lines.Add($"Information Reliability: {CalculateReliability()}%");
            lines.Add($"Information Pieces: {_informationGained.Count}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Gets a short progress display string.
    /// </summary>
    /// <returns>A compact progress string.</returns>
    public string ToProgressDisplay()
    {
        var statusIcon = Status.GetStatusIndicator();
        return $"{statusIcon} R{RoundNumber} | Res: {ResistanceRemaining}/{InitialResistance} | {Status.GetDisplayName()}";
    }

    /// <summary>
    /// Determines if the subject can be broken with current resistance.
    /// </summary>
    /// <param name="maxRounds">Maximum rounds allowed.</param>
    /// <returns>True if mathematically possible to break subject.</returns>
    /// <remarks>
    /// Returns false if remaining resistance exceeds remaining rounds,
    /// assuming average success rate.
    /// </remarks>
    public bool CanBreakSubject(int maxRounds)
    {
        if (Status.IsTerminal())
        {
            return Status.IsSuccess();
        }

        var roundsRemaining = maxRounds - RoundNumber;
        return roundsRemaining >= ResistanceRemaining;
    }
}
