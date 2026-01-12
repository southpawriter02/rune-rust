using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a condition that can cause quest failure.
/// </summary>
public readonly record struct FailureCondition
{
    /// <summary>
    /// Gets the failure type.
    /// </summary>
    public FailureType Type { get; init; }

    /// <summary>
    /// Gets the target ID (NPC, item, area, faction).
    /// </summary>
    public string? TargetId { get; init; }

    /// <summary>
    /// Gets the threshold value (for reputation).
    /// </summary>
    public int? Threshold { get; init; }

    /// <summary>
    /// Gets the failure message.
    /// </summary>
    public string Message { get; init; }

    /// <summary>
    /// Creates a time expired failure condition.
    /// </summary>
    public static FailureCondition TimeExpired(string message = "Time has run out.") => new()
    {
        Type = FailureType.TimeExpired,
        Message = message
    };

    /// <summary>
    /// Creates an NPC died failure condition.
    /// </summary>
    public static FailureCondition NPCDied(string npcId, string? message = null) => new()
    {
        Type = FailureType.NPCDied,
        TargetId = npcId,
        Message = message ?? $"The protected NPC has died."
    };

    /// <summary>
    /// Creates an item lost failure condition.
    /// </summary>
    public static FailureCondition ItemLost(string itemId, string? message = null) => new()
    {
        Type = FailureType.ItemLost,
        TargetId = itemId,
        Message = message ?? $"The required item has been lost."
    };

    /// <summary>
    /// Creates a reputation dropped failure condition.
    /// </summary>
    public static FailureCondition ReputationDropped(string factionId, int threshold, string? message = null) => new()
    {
        Type = FailureType.ReputationDropped,
        TargetId = factionId,
        Threshold = threshold,
        Message = message ?? $"Your reputation has dropped too low."
    };

    /// <summary>
    /// Creates a left area failure condition.
    /// </summary>
    public static FailureCondition LeftArea(string areaId, string? message = null) => new()
    {
        Type = FailureType.LeftArea,
        TargetId = areaId,
        Message = message ?? $"You have left the quest area."
    };
}
