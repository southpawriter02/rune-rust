using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Repository interface for Specialization and SpecializationNode persistence operations.
/// </summary>
/// <remarks>
/// See: SPEC-SPECIALIZATION-001 for design documentation.
/// See: v0.4.1a for implementation details.
/// </remarks>
public interface ISpecializationRepository
{
    // ═══════════════════════════════════════════════════════════════════════
    // Specialization Queries
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets all available specializations with their nodes and abilities.
    /// </summary>
    /// <returns>All specializations ordered by type.</returns>
    Task<IEnumerable<Specialization>> GetAllAsync();

    /// <summary>
    /// Gets a specialization by its unique ID, including all nodes and abilities.
    /// </summary>
    /// <param name="id">The specialization ID.</param>
    /// <returns>The specialization if found; otherwise null.</returns>
    Task<Specialization?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets a specialization by its enum type, including all nodes and abilities.
    /// </summary>
    /// <param name="type">The specialization type enum.</param>
    /// <returns>The specialization if found; otherwise null.</returns>
    Task<Specialization?> GetByTypeAsync(SpecializationType type);

    /// <summary>
    /// Gets all specializations available to a given archetype.
    /// </summary>
    /// <param name="archetype">The archetype to filter by.</param>
    /// <returns>All specializations requiring that archetype.</returns>
    Task<IEnumerable<Specialization>> GetByArchetypeAsync(ArchetypeType archetype);

    // ═══════════════════════════════════════════════════════════════════════
    // Node Queries
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a specific node by its unique ID.
    /// </summary>
    /// <param name="nodeId">The node ID.</param>
    /// <returns>The node if found; otherwise null.</returns>
    Task<SpecializationNode?> GetNodeByIdAsync(Guid nodeId);

    /// <summary>
    /// Gets all nodes for a specialization, ordered by tier then position.
    /// </summary>
    /// <param name="specId">The specialization ID.</param>
    /// <returns>All nodes in the specialization tree.</returns>
    Task<IEnumerable<SpecializationNode>> GetNodesForSpecializationAsync(Guid specId);

    // ═══════════════════════════════════════════════════════════════════════
    // Character Progress Queries
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets all unlocked nodes for a specific character.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <returns>All nodes the character has unlocked.</returns>
    Task<IEnumerable<SpecializationNode>> GetUnlockedNodesAsync(Guid characterId);

    /// <summary>
    /// Records a node unlock for a character.
    /// Creates a new CharacterSpecializationProgress entry.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <param name="nodeId">The node ID being unlocked.</param>
    Task RecordNodeUnlockAsync(Guid characterId, Guid nodeId);

    // ═══════════════════════════════════════════════════════════════════════
    // Persistence
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Saves all pending changes to the database.
    /// </summary>
    Task SaveChangesAsync();
}
