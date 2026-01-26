namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the complete results of an investigation action,
/// including clues discovered, deductions made, and narrative output.
/// </summary>
public sealed record InvestigationResult
{
    /// <summary>
    /// The target that was investigated.
    /// </summary>
    public required string TargetId { get; init; }

    /// <summary>
    /// The type of target investigated.
    /// </summary>
    public required InvestigationTarget TargetType { get; init; }

    /// <summary>
    /// Whether the investigation check succeeded.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// The number of successes from the WITS + Investigation check.
    /// </summary>
    public required int CheckResult { get; init; }

    /// <summary>
    /// The difficulty class that was checked against.
    /// </summary>
    public required int DifficultyClass { get; init; }

    /// <summary>
    /// Collection of new clues discovered in this investigation.
    /// </summary>
    public required IReadOnlyList<Clue> CluesDiscovered { get; init; }

    /// <summary>
    /// Collection of deductions made from accumulated clues.
    /// </summary>
    public required IReadOnlyList<Deduction> DeductionsMade { get; init; }

    /// <summary>
    /// Narrative text describing the investigation findings.
    /// </summary>
    public required string NarrativeText { get; init; }

    /// <summary>
    /// Time spent on this investigation action, in minutes.
    /// </summary>
    public required int TimeSpent { get; init; }

    /// <summary>
    /// Whether this was an expert-level success (net successes >= 5).
    /// </summary>
    public bool IsExpertSuccess { get; init; }

    /// <summary>
    /// Optional: Hidden connections revealed on expert success.
    /// </summary>
    public string? HiddenConnectionsRevealed { get; init; }

    /// <summary>
    /// Gets the total number of discoveries (clues + deductions).
    /// </summary>
    public int TotalDiscoveries => CluesDiscovered.Count + DeductionsMade.Count;

    /// <summary>
    /// Gets whether any new information was discovered.
    /// </summary>
    public bool HasDiscoveries => TotalDiscoveries > 0;

    /// <summary>
    /// Gets whether any clues were discovered.
    /// </summary>
    public bool HasClues => CluesDiscovered.Count > 0;

    /// <summary>
    /// Gets whether any deductions were made.
    /// </summary>
    public bool HasDeductions => DeductionsMade.Count > 0;

    /// <summary>
    /// Gets whether hidden connections were revealed.
    /// </summary>
    public bool HasHiddenConnections => !string.IsNullOrEmpty(HiddenConnectionsRevealed);

    /// <summary>
    /// Creates a failed investigation result.
    /// </summary>
    public static InvestigationResult Failed(
        string targetId,
        InvestigationTarget targetType,
        int checkResult,
        int dc,
        int timeSpent,
        string narrative) =>
        new()
        {
            TargetId = targetId,
            TargetType = targetType,
            Success = false,
            CheckResult = checkResult,
            DifficultyClass = dc,
            CluesDiscovered = Array.Empty<Clue>(),
            DeductionsMade = Array.Empty<Deduction>(),
            NarrativeText = narrative,
            TimeSpent = timeSpent
        };
}
