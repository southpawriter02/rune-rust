using RuneAndRust.Core.AI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RuneAndRust.Engine.AI;

/// <summary>
/// Repository for AI configuration data (threat weights, archetype assignments, etc.).
/// v0.42.1: Tactical Decision-Making & Target Selection
/// </summary>
public interface IAIConfigurationRepository
{
    /// <summary>
    /// Gets the threat weights for a specific AI archetype.
    /// Results are cached to avoid repeated database queries.
    /// </summary>
    /// <param name="archetype">The AI archetype.</param>
    /// <returns>The threat weights configuration.</returns>
    Task<AIThreatWeights> GetThreatWeightsAsync(AIArchetype archetype);

    /// <summary>
    /// Gets threat weights for all archetypes.
    /// Used for caching on service startup.
    /// </summary>
    /// <returns>Dictionary of archetype to threat weights.</returns>
    Task<Dictionary<AIArchetype, AIThreatWeights>> GetAllThreatWeightsAsync();

    /// <summary>
    /// Updates the threat weights for an archetype.
    /// Used for balancing and tuning.
    /// </summary>
    /// <param name="weights">The updated weights.</param>
    Task UpdateThreatWeightsAsync(AIThreatWeights weights);

    /// <summary>
    /// Seeds the default threat weights into the database.
    /// Called during migration/initialization.
    /// </summary>
    Task SeedDefaultThreatWeightsAsync();
}
