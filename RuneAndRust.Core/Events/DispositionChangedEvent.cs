using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Events;

/// <summary>
/// Published when a character's disposition tier with a faction changes.
/// Only fires on threshold crossings (e.g., Neutral → Friendly).
/// Consumed by dialogue system for option unlocking and combat AI for aggression changes.
/// </summary>
/// <remarks>See: v0.4.2a (The Repute) for Faction System implementation.</remarks>
/// <param name="CharacterId">The character whose disposition changed.</param>
/// <param name="CharacterName">The character's display name.</param>
/// <param name="Faction">The faction affected.</param>
/// <param name="OldDisposition">Previous disposition tier.</param>
/// <param name="NewDisposition">New disposition tier.</param>
/// <param name="Direction">Whether the change was an improvement or degradation.</param>
public record DispositionChangedEvent(
    Guid CharacterId,
    string CharacterName,
    FactionType Faction,
    Disposition OldDisposition,
    Disposition NewDisposition,
    DispositionChangeDirection Direction)
{
    /// <summary>
    /// Whether the disposition improved (moved toward Exalted).
    /// </summary>
    public bool IsImprovement => Direction == DispositionChangeDirection.Improved;

    /// <summary>
    /// Whether the disposition degraded (moved toward Hated).
    /// </summary>
    public bool IsDegradation => Direction == DispositionChangeDirection.Degraded;
}

/// <summary>
/// Direction of a disposition tier change.
/// </summary>
public enum DispositionChangeDirection
{
    /// <summary>Disposition moved toward Exalted (reputation increased).</summary>
    Improved = 1,

    /// <summary>Disposition moved toward Hated (reputation decreased).</summary>
    Degraded = -1
}
