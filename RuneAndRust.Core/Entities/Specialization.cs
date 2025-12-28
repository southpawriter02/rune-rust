using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Entities;

/// <summary>
/// Represents a character specialization path (e.g., Berserkr, Skald).
/// Specializations are distinct from Archetypes and provide access to
/// a tree of unlockable abilities.
/// </summary>
/// <remarks>
/// See: SPEC-SPECIALIZATION-001 for design documentation.
/// See: v0.4.1a for implementation details.
///
/// Key concepts:
/// - Each Specialization is tied to a specific Archetype
/// - Contains a tree of SpecializationNodes representing unlockable abilities
/// - Characters must meet level requirements to unlock a specialization
/// </remarks>
public class Specialization
{
    /// <summary>
    /// Unique identifier for this specialization.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Machine-readable type for enum-based lookups.
    /// </summary>
    public SpecializationType Type { get; set; }

    /// <summary>
    /// Display name shown in UI (e.g., "Berserkr", "Skald").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// AAM-VOICE compliant flavor description for the specialization.
    /// Describes the thematic focus and playstyle.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The archetype required to unlock this specialization.
    /// A Warrior can only access Warrior specializations (Berserkr, Guardian).
    /// </summary>
    public ArchetypeType RequiredArchetype { get; set; }

    /// <summary>
    /// Minimum character level required to unlock this specialization.
    /// Default is 1, meaning available from the start.
    /// </summary>
    public int RequiredLevel { get; set; } = 1;

    /// <summary>
    /// Navigation property: All nodes in this specialization's ability tree.
    /// Nodes are organized by Tier (1-4) with prerequisites.
    /// </summary>
    public List<SpecializationNode> Nodes { get; set; } = new();

    /// <summary>
    /// Timestamp when this specialization record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
