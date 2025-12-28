namespace RuneAndRust.Core.Entities;

/// <summary>
/// Junction table tracking which specialization nodes a character has unlocked.
/// Uses a composite primary key (CharacterId, NodeId).
/// </summary>
/// <remarks>
/// See: SPEC-SPECIALIZATION-001 for design documentation.
/// See: v0.4.1a for implementation details.
///
/// This table enables:
/// - Tracking unlock timestamps for analytics
/// - Querying unlocked abilities per character
/// - EF Core navigation from Character to unlocked nodes
/// </remarks>
public class CharacterSpecializationProgress
{
    /// <summary>
    /// Foreign key to the Character who unlocked this node.
    /// Part of the composite primary key.
    /// </summary>
    public Guid CharacterId { get; set; }

    /// <summary>
    /// Navigation property to the Character.
    /// </summary>
    public Character Character { get; set; } = null!;

    /// <summary>
    /// Foreign key to the unlocked SpecializationNode.
    /// Part of the composite primary key.
    /// </summary>
    public Guid NodeId { get; set; }

    /// <summary>
    /// Navigation property to the unlocked node.
    /// </summary>
    public SpecializationNode Node { get; set; } = null!;

    /// <summary>
    /// Timestamp when this node was unlocked by the character.
    /// </summary>
    public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;
}
