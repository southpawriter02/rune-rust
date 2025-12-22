using RuneAndRust.Core.Entities;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Repository interface for RoomTemplate entities with specialized query methods.
/// Part of the Dynamic Room Engine (v0.4.0).
/// </summary>
public interface IRoomTemplateRepository : IRepository<RoomTemplate>
{
    /// <summary>
    /// Retrieves all room templates for a specific biome.
    /// </summary>
    /// <param name="biomeId">The biome ID (e.g., "the_roots").</param>
    /// <returns>Collection of room templates for the specified biome.</returns>
    Task<IEnumerable<RoomTemplate>> GetByBiomeIdAsync(string biomeId);

    /// <summary>
    /// Retrieves all room templates matching a specific archetype.
    /// </summary>
    /// <param name="archetype">The archetype (e.g., "EntryHall", "Corridor", "BossArena").</param>
    /// <returns>Collection of room templates with the specified archetype.</returns>
    Task<IEnumerable<RoomTemplate>> GetByArchetypeAsync(string archetype);

    /// <summary>
    /// Retrieves a single room template by its unique template ID.
    /// </summary>
    /// <param name="templateId">The template ID (e.g., "reactor_core").</param>
    /// <returns>The room template if found; otherwise, null.</returns>
    Task<RoomTemplate?> GetByTemplateIdAsync(string templateId);

    /// <summary>
    /// Upserts a room template (inserts if new, updates if existing based on TemplateId).
    /// </summary>
    /// <param name="template">The room template to upsert.</param>
    Task UpsertAsync(RoomTemplate template);
}
