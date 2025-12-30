using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Models;

/// <summary>
/// Result of a reputation modification operation.
/// Contains before/after state for both reputation value and disposition tier.
/// </summary>
/// <remarks>See: v0.4.2a (The Repute) for Faction System implementation.</remarks>
public record ReputationChangeResult(
    bool Success,
    string Message,
    FactionType Faction,
    int OldValue,
    int NewValue,
    int Delta,
    Disposition OldDisposition,
    Disposition NewDisposition,
    bool DispositionChanged,
    string? Source = null)
{
    /// <summary>
    /// Creates a successful reputation change result.
    /// </summary>
    public static ReputationChangeResult Ok(
        FactionType faction,
        int oldVal,
        int newVal,
        Disposition oldDisp,
        Disposition newDisp,
        string? source = null)
    {
        var delta = newVal - oldVal;
        var direction = delta > 0 ? "increased" : "decreased";
        var message = $"Reputation with {faction} {direction} to {newVal}.";

        return new ReputationChangeResult(
            Success: true,
            Message: message,
            Faction: faction,
            OldValue: oldVal,
            NewValue: newVal,
            Delta: delta,
            OldDisposition: oldDisp,
            NewDisposition: newDisp,
            DispositionChanged: oldDisp != newDisp,
            Source: source);
    }

    /// <summary>
    /// Creates a no-change result (amount was 0 or already at boundary).
    /// </summary>
    public static ReputationChangeResult NoChange(
        FactionType faction,
        int value,
        Disposition disposition)
    {
        return new ReputationChangeResult(
            Success: true,
            Message: $"Reputation with {faction} unchanged at {value}.",
            Faction: faction,
            OldValue: value,
            NewValue: value,
            Delta: 0,
            OldDisposition: disposition,
            NewDisposition: disposition,
            DispositionChanged: false,
            Source: null);
    }

    /// <summary>
    /// Creates a failure result.
    /// </summary>
    public static ReputationChangeResult Failure(string reason, FactionType faction)
    {
        return new ReputationChangeResult(
            Success: false,
            Message: reason,
            Faction: faction,
            OldValue: 0,
            NewValue: 0,
            Delta: 0,
            OldDisposition: Disposition.Neutral,
            NewDisposition: Disposition.Neutral,
            DispositionChanged: false,
            Source: null);
    }

    /// <summary>
    /// Whether the disposition improved (moved toward Exalted).
    /// </summary>
    public bool DispositionImproved => DispositionChanged && NewDisposition > OldDisposition;

    /// <summary>
    /// Whether the disposition degraded (moved toward Hated).
    /// </summary>
    public bool DispositionDegraded => DispositionChanged && NewDisposition < OldDisposition;
}
