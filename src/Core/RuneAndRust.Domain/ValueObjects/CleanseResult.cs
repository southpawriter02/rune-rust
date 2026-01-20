namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Result of a cleanse operation.
/// </summary>
/// <param name="Success">Whether any effects were removed.</param>
/// <param name="RemovedEffects">Effect IDs that were removed.</param>
/// <param name="Message">Display message for the result.</param>
public readonly record struct CleanseResult(
    bool Success,
    IReadOnlyList<string> RemovedEffects,
    string Message)
{
    /// <summary>Creates a successful cleanse result.</summary>
    public static CleanseResult Succeeded(IReadOnlyList<string> removed) =>
        new(true, removed, BuildMessage(removed));

    /// <summary>Creates a failed cleanse result (no matching effects).</summary>
    public static CleanseResult NoEffectsRemoved() =>
        new(false, Array.Empty<string>(), "No effects to cleanse.");

    private static string BuildMessage(IReadOnlyList<string> removed)
    {
        if (removed.Count == 0)
            return "No effects to cleanse.";
        if (removed.Count == 1)
            return $"Cleansed {removed[0]}.";
        return $"Cleansed {removed.Count} effects.";
    }
}
