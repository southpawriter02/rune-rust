using System.Collections.Generic;
using System.Threading.Tasks;

namespace RuneAndRust.Core.AI;

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

    // v0.42.2: Archetype Configuration Methods

    /// <summary>
    /// Gets the archetype configuration for a specific AI archetype.
    /// Results are cached to avoid repeated database queries.
    /// </summary>
    /// <param name="archetype">The AI archetype.</param>
    /// <returns>The archetype configuration.</returns>
    Task<AIArchetypeConfiguration> GetArchetypeConfigurationAsync(AIArchetype archetype);

    /// <summary>
    /// Gets all archetype configurations.
    /// Used for caching on service startup.
    /// </summary>
    /// <returns>Dictionary of archetype to configuration.</returns>
    Task<Dictionary<AIArchetype, AIArchetypeConfiguration>> GetAllArchetypeConfigurationsAsync();

    /// <summary>
    /// Updates the archetype configuration.
    /// Used for balancing and tuning.
    /// </summary>
    /// <param name="config">The updated configuration.</param>
    Task UpdateArchetypeConfigurationAsync(AIArchetypeConfiguration config);

    /// <summary>
    /// Seeds the default archetype configurations into the database.
    /// Called during migration/initialization.
    /// </summary>
    Task SeedDefaultArchetypeConfigurationsAsync();

    // v0.42.3: Boss Configuration Methods

    /// <summary>
    /// Gets the boss configuration for a specific boss type.
    /// Results are cached to avoid repeated database queries.
    /// </summary>
    /// <param name="bossTypeId">The boss type ID.</param>
    /// <returns>The boss configuration, or null if not found.</returns>
    Task<BossConfiguration?> GetBossConfigurationAsync(int bossTypeId);

    /// <summary>
    /// Gets the phase transition configuration for a boss.
    /// </summary>
    /// <param name="bossTypeId">The boss type ID.</param>
    /// <param name="toPhase">The phase transitioning to.</param>
    /// <returns>The phase transition configuration, or null if not found.</returns>
    Task<BossPhaseTransition?> GetBossPhaseTransitionAsync(int bossTypeId, BossPhase toPhase);

    /// <summary>
    /// Gets the ability rotation for a boss phase.
    /// </summary>
    /// <param name="bossTypeId">The boss type ID.</param>
    /// <param name="phase">The phase to get rotation for.</param>
    /// <returns>The ability rotation, or null if not found.</returns>
    Task<AbilityRotation?> GetAbilityRotationAsync(int bossTypeId, BossPhase phase);

    /// <summary>
    /// Gets the add management configuration for a boss phase.
    /// </summary>
    /// <param name="bossTypeId">The boss type ID.</param>
    /// <param name="phase">The phase to get configuration for.</param>
    /// <returns>The add management configuration, or null if not found.</returns>
    Task<AddManagementConfig?> GetAddManagementConfigAsync(int bossTypeId, BossPhase phase);

    /// <summary>
    /// Seeds default boss configurations into the database.
    /// Called during migration/initialization.
    /// </summary>
    Task SeedDefaultBossConfigurationsAsync();
}
