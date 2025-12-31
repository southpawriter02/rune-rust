using System;

namespace RuneAndRust.Core.Events;

/// <summary>
/// Published when a Mystic's Aetheric Resonance reaches 100 (Overflow).
/// </summary>
public record OverflowTriggeredEvent(
    Guid CharacterId,
    string CharacterName,
    int OverflowCount,
    bool SoulFractureRisk)
{
    /// <summary>
    /// True if this is the first overflow for this character.
    /// </summary>
    public bool IsFirstOverflow => OverflowCount == 1;

    /// <summary>
    /// True if this overflow puts the character at risk of Soul Fracture (3+ overflows).
    /// </summary>
    public bool IsHighRisk => SoulFractureRisk;
}
