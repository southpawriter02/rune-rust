using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Models;

/// <summary>
/// DTO containing a character's current standing with a faction.
/// Used for UI display and condition evaluation.
/// </summary>
/// <remarks>See: v0.4.2a (The Repute) for Faction System implementation.</remarks>
public record FactionStandingInfo(
    FactionType Faction,
    string FactionName,
    int Reputation,
    Disposition Disposition)
{
    /// <summary>
    /// Whether the faction is hostile (Hated or Hostile disposition).
    /// </summary>
    public bool IsHostile => Disposition <= Disposition.Hostile;

    /// <summary>
    /// Whether the faction is friendly (Friendly or Exalted disposition).
    /// </summary>
    public bool IsFriendly => Disposition >= Disposition.Friendly;

    /// <summary>
    /// Progress percentage toward the next positive tier (0-100).
    /// Returns 100 if already Exalted.
    /// </summary>
    public int ProgressToNextTier => Disposition switch
    {
        Disposition.Hated => Math.Max(0, (Reputation + 100) * 100 / 50),      // -100 to -50
        Disposition.Hostile => Math.Max(0, (Reputation + 49) * 100 / 40),     // -49 to -10
        Disposition.Neutral => Math.Max(0, (Reputation + 9) * 100 / 19),      // -9 to 9
        Disposition.Friendly => Math.Max(0, (Reputation - 10) * 100 / 40),    // 10 to 49
        Disposition.Exalted => 100,
        _ => 0
    };
}
