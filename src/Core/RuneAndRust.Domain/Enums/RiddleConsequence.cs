namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Consequence for failing a riddle NPC encounter.
/// </summary>
/// <remarks>
/// When a player reaches the maximum wrong answer count for a riddle NPC,
/// the NPC applies this consequence.
/// </remarks>
public enum RiddleConsequence
{
    /// <summary>
    /// NPC becomes hostile and attacks the player.
    /// </summary>
    BecomeHostile,

    /// <summary>
    /// NPC disappears from the room.
    /// </summary>
    Disappear,

    /// <summary>
    /// NPC applies a penalty to the player (damage, status effect, etc.).
    /// </summary>
    ApplyPenalty,

    /// <summary>
    /// Riddle resets (can try again later).
    /// </summary>
    Reset
}
