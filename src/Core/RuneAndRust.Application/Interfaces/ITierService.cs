using RuneAndRust.Domain.Definitions;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for managing monster tier definitions and tier selection.
/// </summary>
/// <remarks>
/// Tiers determine difficulty scaling (stat multipliers) and visual presentation.
/// Used during monster spawning to select and apply tier effects.
/// </remarks>
public interface ITierService
{
    /// <summary>
    /// Gets a tier definition by ID.
    /// </summary>
    /// <param name="tierId">The tier identifier.</param>
    /// <returns>The tier definition or null if not found.</returns>
    TierDefinition? GetTier(string tierId);

    /// <summary>
    /// Gets all available tier definitions.
    /// </summary>
    /// <returns>Read-only list of all tier definitions.</returns>
    IReadOnlyList<TierDefinition> GetAllTiers();

    /// <summary>
    /// Selects a random tier from the available tiers using weighted selection.
    /// </summary>
    /// <param name="possibleTierIds">The list of tier IDs to choose from.</param>
    /// <returns>The selected tier definition, or the default tier if none match.</returns>
    /// <remarks>
    /// Uses SpawnWeight for weighted random selection.
    /// Falls back to "common" tier if no valid tiers are found.
    /// </remarks>
    TierDefinition SelectRandomTier(IReadOnlyList<string> possibleTierIds);

    /// <summary>
    /// Gets the default (common) tier.
    /// </summary>
    /// <returns>The common tier definition.</returns>
    TierDefinition GetDefaultTier();
}
