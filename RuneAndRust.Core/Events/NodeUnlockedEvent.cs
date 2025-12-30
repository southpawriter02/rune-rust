namespace RuneAndRust.Core.Events;

/// <summary>
/// Published when a character unlocks a specialization node.
/// Consumed by UI listeners to display node unlock notifications.
/// </summary>
/// <remarks>See: v0.4.1b (The Unlock) for implementation.</remarks>
/// <param name="CharacterId">The unique identifier of the character.</param>
/// <param name="CharacterName">The display name of the character.</param>
/// <param name="NodeId">The unique identifier of the unlocked node.</param>
/// <param name="NodeName">The display name of the unlocked node.</param>
/// <param name="AbilityId">The ability ID granted by this node.</param>
/// <param name="Tier">The tier level of the unlocked node (1-4).</param>
/// <param name="IsCapstone">Whether this is a Tier 4 capstone node.</param>
/// <param name="ProgressionPointsSpent">The PP spent to unlock this node.</param>
public record NodeUnlockedEvent(
    Guid CharacterId,
    string CharacterName,
    Guid NodeId,
    string NodeName,
    Guid AbilityId,
    int Tier,
    bool IsCapstone,
    int ProgressionPointsSpent);
