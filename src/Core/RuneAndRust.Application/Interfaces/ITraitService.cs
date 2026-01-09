using RuneAndRust.Domain.Definitions;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for managing monster trait definitions and trait selection.
/// </summary>
/// <remarks>
/// Traits provide special abilities and behaviors to monsters.
/// The number of traits assigned depends on the monster's tier.
/// </remarks>
public interface ITraitService
{
    /// <summary>
    /// Gets a trait definition by ID.
    /// </summary>
    /// <param name="traitId">The trait identifier.</param>
    /// <returns>The trait definition or null if not found.</returns>
    MonsterTrait? GetTrait(string traitId);

    /// <summary>
    /// Gets all available trait definitions.
    /// </summary>
    /// <returns>Read-only list of all trait definitions.</returns>
    IReadOnlyList<MonsterTrait> GetAllTraits();

    /// <summary>
    /// Gets multiple trait definitions by their IDs.
    /// </summary>
    /// <param name="traitIds">The trait identifiers to look up.</param>
    /// <returns>List of found trait definitions (skips unknown IDs).</returns>
    IReadOnlyList<MonsterTrait> GetTraits(IEnumerable<string> traitIds);

    /// <summary>
    /// Selects random traits from the possible trait pool based on tier.
    /// </summary>
    /// <param name="possibleTraitIds">The list of trait IDs available for this monster.</param>
    /// <param name="tier">The tier determining how many traits to select.</param>
    /// <returns>List of selected trait IDs.</returns>
    /// <remarks>
    /// Trait count by tier:
    /// - Common: 0 traits
    /// - Named: 1 trait
    /// - Elite: 2 traits
    /// - Boss: 3 traits
    /// </remarks>
    IReadOnlyList<string> SelectRandomTraits(IReadOnlyList<string> possibleTraitIds, TierDefinition tier);
}
