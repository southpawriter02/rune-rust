using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Repository for faction and character standing persistence.
/// </summary>
/// <remarks>See: v0.4.2a (The Repute) for Faction System implementation.</remarks>
public interface IFactionRepository
{
    // ═══════════════════════════════════════════════════════════════════════
    // Faction Queries
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a faction definition by type.
    /// </summary>
    Task<Faction?> GetFactionAsync(FactionType type);

    /// <summary>
    /// Gets all faction definitions.
    /// </summary>
    Task<IEnumerable<Faction>> GetAllFactionsAsync();

    // ═══════════════════════════════════════════════════════════════════════
    // Standing Queries
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a character's standing with a specific faction.
    /// Returns null if no standing exists.
    /// </summary>
    Task<CharacterFactionStanding?> GetStandingAsync(Guid characterId, FactionType faction);

    /// <summary>
    /// Gets all standings for a character.
    /// </summary>
    Task<IEnumerable<CharacterFactionStanding>> GetStandingsForCharacterAsync(Guid characterId);

    // ═══════════════════════════════════════════════════════════════════════
    // Standing Mutations
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Adds a new character faction standing.
    /// </summary>
    Task AddStandingAsync(CharacterFactionStanding standing);

    /// <summary>
    /// Updates an existing character faction standing.
    /// </summary>
    Task UpdateStandingAsync(CharacterFactionStanding standing);

    /// <summary>
    /// Persists all pending changes.
    /// </summary>
    Task SaveChangesAsync();
}
