// ------------------------------------------------------------------------------
// <copyright file="TrapDisarmState.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// State machine entity tracking the progress of a trap disarmament attempt
// through the three-step procedure.
// Part of v0.15.4d Trap Disarmament System implementation.
// </summary>
// ------------------------------------------------------------------------------

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// State machine entity tracking the progress of a trap disarmament attempt
/// through the three-step procedure.
/// </summary>
/// <remarks>
/// <para>
/// TrapDisarmState manages the full lifecycle of a disarmament attempt:
/// <list type="bullet">
///   <item><description>Detection → Detected (success) or Triggered (failure)</description></item>
///   <item><description>Detected → Analyzed (after analysis) or DisarmInProgress (skip)</description></item>
///   <item><description>DisarmInProgress → Disarmed (success) or Destroyed (fumble)</description></item>
/// </list>
/// </para>
/// <para>
/// The state tracks DC escalation from failed attempts and stores revealed
/// analysis information for hint bonuses.
/// </para>
/// </remarks>
public class TrapDisarmState
{
    // -------------------------------------------------------------------------
    // Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the unique identifier for this disarmament attempt.
    /// </summary>
    public string DisarmId { get; private init; } = string.Empty;

    /// <summary>
    /// Gets the ID of the character attempting disarmament.
    /// </summary>
    public string CharacterId { get; private init; } = string.Empty;

    /// <summary>
    /// Gets the type of trap being disarmed.
    /// </summary>
    public TrapType TrapType { get; private init; }

    /// <summary>
    /// Gets the current step in the disarmament procedure.
    /// </summary>
    public DisarmStep CurrentStep { get; private set; } = DisarmStep.Detection;

    /// <summary>
    /// Gets whether analysis has been completed.
    /// </summary>
    public bool AnalysisComplete { get; private set; }

    /// <summary>
    /// Gets the information revealed through analysis.
    /// </summary>
    public TrapAnalysisInfo RevealedInfo { get; private set; } = TrapAnalysisInfo.Empty;

    /// <summary>
    /// Gets the number of failed disarmament attempts.
    /// </summary>
    /// <remarks>
    /// Each failure increases the effective DC by +1.
    /// </remarks>
    public int FailedAttempts { get; private set; }

    /// <summary>
    /// Gets the current status of the disarmament attempt.
    /// </summary>
    public DisarmStatus Status { get; private set; } = DisarmStatus.Undetected;

    /// <summary>
    /// Gets the components salvaged from a critical success.
    /// </summary>
    public IReadOnlyList<string> SalvagedComponents { get; private set; } = Array.Empty<string>();

    // -------------------------------------------------------------------------
    // Computed Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the current effective disarm DC (base + failed attempts).
    /// </summary>
    /// <remarks>
    /// DC escalates by +1 for each failed disarmament attempt.
    /// </remarks>
    public int CurrentDisarmDc => GetBaseDisarmDc() + FailedAttempts;

    /// <summary>
    /// Gets whether the disarmament is in a terminal state.
    /// </summary>
    /// <remarks>
    /// Terminal states: Disarmed, Triggered, Destroyed.
    /// </remarks>
    public bool IsTerminal => Status is DisarmStatus.Disarmed
                                     or DisarmStatus.Triggered
                                     or DisarmStatus.Destroyed;

    /// <summary>
    /// Gets whether the trap was successfully neutralized.
    /// </summary>
    public bool WasSuccessful => Status == DisarmStatus.Disarmed;

    /// <summary>
    /// Gets whether the character has a hint bonus from analysis.
    /// </summary>
    public bool HasHintBonus => RevealedInfo.GrantsHintBonus;

    // -------------------------------------------------------------------------
    // Factory Method
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a new trap disarmament state for the specified character and trap.
    /// </summary>
    /// <param name="characterId">The ID of the character attempting disarmament.</param>
    /// <param name="trapType">The type of trap to disarm.</param>
    /// <returns>A new TrapDisarmState in the initial (Undetected) state.</returns>
    /// <exception cref="ArgumentException">Thrown when characterId is null or empty.</exception>
    public static TrapDisarmState Create(string characterId, TrapType trapType)
    {
        if (string.IsNullOrWhiteSpace(characterId))
        {
            throw new ArgumentException("Character ID cannot be null or empty.", nameof(characterId));
        }

        return new TrapDisarmState
        {
            DisarmId = Guid.NewGuid().ToString(),
            CharacterId = characterId,
            TrapType = trapType,
            CurrentStep = DisarmStep.Detection,
            Status = DisarmStatus.Undetected,
            AnalysisComplete = false,
            RevealedInfo = TrapAnalysisInfo.Empty,
            FailedAttempts = 0,
            SalvagedComponents = Array.Empty<string>()
        };
    }

    // -------------------------------------------------------------------------
    // State Transition Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Marks the trap as detected, advancing to the next step.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if not in Undetected state.</exception>
    public void MarkDetected()
    {
        if (Status != DisarmStatus.Undetected)
        {
            throw new InvalidOperationException(
                $"Cannot mark as detected from status {Status}. Must be Undetected.");
        }

        Status = DisarmStatus.Detected;
        CurrentStep = DisarmStep.Analysis; // Default to analysis, can skip
    }

    /// <summary>
    /// Records the analysis result and advances status.
    /// </summary>
    /// <param name="analysisInfo">The information revealed through analysis.</param>
    /// <exception cref="InvalidOperationException">Thrown if not in Detected state.</exception>
    public void RecordAnalysis(TrapAnalysisInfo analysisInfo)
    {
        if (Status != DisarmStatus.Detected)
        {
            throw new InvalidOperationException(
                $"Cannot record analysis from status {Status}. Must be Detected.");
        }

        RevealedInfo = analysisInfo;
        AnalysisComplete = true;
        Status = DisarmStatus.Analyzed;
        CurrentStep = DisarmStep.Disarmament;
    }

    /// <summary>
    /// Skips analysis and proceeds directly to disarmament.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if not in Detected state.</exception>
    public void SkipAnalysis()
    {
        if (Status != DisarmStatus.Detected)
        {
            throw new InvalidOperationException(
                $"Cannot skip analysis from status {Status}. Must be Detected.");
        }

        Status = DisarmStatus.DisarmInProgress;
        CurrentStep = DisarmStep.Disarmament;
    }

    /// <summary>
    /// Records a failed disarmament attempt, escalating the DC.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if not in a valid disarming state.</exception>
    public void RecordFailedAttempt()
    {
        if (Status != DisarmStatus.Analyzed && Status != DisarmStatus.DisarmInProgress)
        {
            throw new InvalidOperationException(
                $"Cannot record failed attempt from status {Status}. " +
                "Must be Analyzed or DisarmInProgress.");
        }

        FailedAttempts++;
        Status = DisarmStatus.DisarmInProgress;
    }

    /// <summary>
    /// Marks the trap as successfully disarmed.
    /// </summary>
    /// <param name="salvage">Components salvaged (if critical success).</param>
    /// <exception cref="InvalidOperationException">Thrown if not in a valid disarming state.</exception>
    public void MarkDisarmed(IReadOnlyList<string> salvage)
    {
        if (Status != DisarmStatus.Analyzed && Status != DisarmStatus.DisarmInProgress)
        {
            throw new InvalidOperationException(
                $"Cannot mark as disarmed from status {Status}. " +
                "Must be Analyzed or DisarmInProgress.");
        }

        Status = DisarmStatus.Disarmed;
        SalvagedComponents = salvage ?? Array.Empty<string>();
    }

    /// <summary>
    /// Marks the trap as triggered (detection failure).
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if not in Undetected state.</exception>
    public void MarkTriggered()
    {
        if (Status != DisarmStatus.Undetected)
        {
            throw new InvalidOperationException(
                $"Cannot mark as triggered from status {Status}. Must be Undetected.");
        }

        Status = DisarmStatus.Triggered;
    }

    /// <summary>
    /// Marks the trap as destroyed (fumbled disarmament).
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if not in a valid disarming state.</exception>
    public void MarkDestroyed()
    {
        if (Status != DisarmStatus.Analyzed && Status != DisarmStatus.DisarmInProgress)
        {
            throw new InvalidOperationException(
                $"Cannot mark as destroyed from status {Status}. " +
                "Must be Analyzed or DisarmInProgress.");
        }

        Status = DisarmStatus.Destroyed;
    }

    // -------------------------------------------------------------------------
    // DC Calculation Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the detection DC for this trap type.
    /// </summary>
    /// <returns>The detection DC.</returns>
    public int GetDetectionDc()
    {
        return TrapType switch
        {
            TrapType.Tripwire => 8,
            TrapType.PressurePlate => 10,
            TrapType.Electrified => 14,
            TrapType.LaserGrid => 18,
            TrapType.JotunDefense => 22,
            _ => 12
        };
    }

    /// <summary>
    /// Gets the base disarm DC for this trap type (before escalation).
    /// </summary>
    /// <returns>The base disarm DC.</returns>
    private int GetBaseDisarmDc()
    {
        return TrapType switch
        {
            TrapType.Tripwire => 8,
            TrapType.PressurePlate => 12,
            TrapType.Electrified => 16,
            TrapType.LaserGrid => 20,
            TrapType.JotunDefense => 24,
            _ => 12
        };
    }

    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns a formatted display string for the state.
    /// </summary>
    /// <returns>A human-readable summary of the disarmament state.</returns>
    public string ToDisplayString()
    {
        var hintStr = HasHintBonus ? " [+1d10 hint]" : "";
        var failStr = FailedAttempts > 0 ? $" (+{FailedAttempts} DC escalation)" : "";

        return $"{TrapType}: {Status} - DC {CurrentDisarmDc}{failStr}{hintStr}";
    }

    /// <summary>
    /// Returns a compact string for logging purposes.
    /// </summary>
    /// <returns>A log-friendly string representation.</returns>
    public string ToLogString()
    {
        return $"TrapState[{DisarmId[..Math.Min(8, DisarmId.Length)]}] " +
               $"Type={TrapType} Status={Status} Step={CurrentStep} " +
               $"DC={CurrentDisarmDc} Failures={FailedAttempts} Hint={HasHintBonus}";
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return ToDisplayString();
    }

    // -------------------------------------------------------------------------
    // Private Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Private constructor - use Create() factory method.
    /// </summary>
    private TrapDisarmState()
    {
    }
}
