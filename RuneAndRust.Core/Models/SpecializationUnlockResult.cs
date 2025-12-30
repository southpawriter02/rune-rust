namespace RuneAndRust.Core.Models;

/// <summary>
/// Result of a specialization unlock operation.
/// </summary>
/// <remarks>See: v0.4.1b (The Unlock) for implementation.</remarks>
public record SpecializationUnlockResult(
    bool Success,
    string Message,
    Guid? SpecializationId = null,
    string? SpecializationName = null,
    int PpSpent = 0)
{
    /// <summary>
    /// Creates a successful unlock result.
    /// </summary>
    /// <param name="message">Success message to display.</param>
    /// <param name="specId">The unlocked specialization ID.</param>
    /// <param name="specName">The unlocked specialization name.</param>
    /// <param name="cost">The PP spent to unlock.</param>
    /// <returns>A successful SpecializationUnlockResult.</returns>
    public static SpecializationUnlockResult Ok(
        string message, Guid specId, string specName, int cost)
        => new(true, message, specId, specName, cost);

    /// <summary>
    /// Creates a failed unlock result.
    /// </summary>
    /// <param name="reason">The reason for failure.</param>
    /// <returns>A failed SpecializationUnlockResult.</returns>
    public static SpecializationUnlockResult Failure(string reason)
        => new(false, reason);
}
