namespace RuneAndRust.Core.Models;

/// <summary>
/// Result of a specialization node unlock operation.
/// </summary>
/// <remarks>See: v0.4.1b (The Unlock) for implementation.</remarks>
public record NodeUnlockResult(
    bool Success,
    string Message,
    Guid? NodeId = null,
    string? NodeName = null,
    Guid? AbilityId = null,
    int Tier = 0,
    int PpSpent = 0)
{
    /// <summary>
    /// Creates a successful node unlock result.
    /// </summary>
    /// <param name="message">Success message to display.</param>
    /// <param name="nodeId">The unlocked node ID.</param>
    /// <param name="nodeName">The unlocked node display name.</param>
    /// <param name="abilityId">The ability ID granted by this node.</param>
    /// <param name="tier">The tier level of the unlocked node (1-4).</param>
    /// <param name="cost">The PP spent to unlock.</param>
    /// <returns>A successful NodeUnlockResult.</returns>
    public static NodeUnlockResult Ok(
        string message, Guid nodeId, string nodeName,
        Guid abilityId, int tier, int cost)
        => new(true, message, nodeId, nodeName, abilityId, tier, cost);

    /// <summary>
    /// Creates a failed node unlock result.
    /// </summary>
    /// <param name="reason">The reason for failure.</param>
    /// <returns>A failed NodeUnlockResult.</returns>
    public static NodeUnlockResult Failure(string reason)
        => new(false, reason);
}
