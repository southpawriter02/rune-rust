namespace RuneAndRust.Core.Models;

/// <summary>
/// Result of determining a dialogue option's visibility and availability.
/// </summary>
/// <remarks>See: v0.4.2b (The Lexicon) for Dialogue System implementation.</remarks>
public record OptionVisibilityResult(
    Guid OptionId,
    bool IsVisible,
    bool IsAvailable,
    string? LockReason,
    string? LockHint,
    IReadOnlyList<ConditionResult>? ConditionResults = null)
{
    /// <summary>
    /// Creates a visible and available result.
    /// </summary>
    public static OptionVisibilityResult Available(Guid optionId) =>
        new(optionId, true, true, null, null);

    /// <summary>
    /// Creates a visible but locked result (shows requirement).
    /// </summary>
    public static OptionVisibilityResult Locked(
        Guid optionId,
        string reason,
        string hint,
        IReadOnlyList<ConditionResult> results) =>
        new(optionId, true, false, reason, hint, results);

    /// <summary>
    /// Creates a hidden result (conditions failed, hidden mode).
    /// </summary>
    public static OptionVisibilityResult Hidden(Guid optionId) =>
        new(optionId, false, false, null, null);
}
