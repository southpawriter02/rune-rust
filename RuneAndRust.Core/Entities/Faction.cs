using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Entities;

/// <summary>
/// Represents a faction in the world of Aethelgard.
/// Contains metadata for display and default disposition rules.
/// </summary>
/// <remarks>See: v0.4.2a (The Repute) for Faction System implementation.</remarks>
public class Faction
{
    /// <summary>
    /// The faction type identifier (used as primary key).
    /// </summary>
    public FactionType Type { get; set; }

    /// <summary>
    /// The display name of the faction.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Flavor text describing the faction's culture and role.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The default starting reputation for new characters.
    /// Most factions start at 0 (Neutral), but The Bound starts at -25 (Hostile).
    /// </summary>
    public int DefaultReputation { get; set; } = 0;

    /// <summary>
    /// When the faction record was seeded/created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property: All character standings with this faction.
    /// </summary>
    public ICollection<CharacterFactionStanding> CharacterStandings { get; set; }
        = new List<CharacterFactionStanding>();
}
