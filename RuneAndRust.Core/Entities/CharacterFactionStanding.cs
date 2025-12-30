using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Entities;

/// <summary>
/// Join entity tracking a character's reputation with a specific faction.
/// Composite key: (CharacterId, FactionType).
/// </summary>
/// <remarks>See: v0.4.2a (The Repute) for Faction System implementation.</remarks>
public class CharacterFactionStanding
{
    #region Composite Key

    /// <summary>
    /// Foreign key to the character.
    /// </summary>
    public Guid CharacterId { get; set; }

    /// <summary>
    /// The faction this standing tracks.
    /// </summary>
    public FactionType FactionType { get; set; }

    #endregion

    #region Properties

    /// <summary>
    /// Reputation value from -100 (Hated) to +100 (Exalted).
    /// </summary>
    public int Reputation { get; set; } = 0;

    /// <summary>
    /// When this standing was first established.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this standing was last modified.
    /// </summary>
    public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Navigation property to the character.
    /// </summary>
    public Character Character { get; set; } = null!;

    /// <summary>
    /// Navigation property to the faction definition.
    /// </summary>
    public Faction Faction { get; set; } = null!;

    #endregion
}
