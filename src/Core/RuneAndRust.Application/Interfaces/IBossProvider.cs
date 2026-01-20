using RuneAndRust.Domain.Definitions;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Provides boss definitions from configuration for the boss encounter system.
/// </summary>
/// <remarks>
/// <para>
/// IBossProvider is the primary interface for accessing boss definitions loaded from configuration.
/// Implementations should load bosses at startup and provide efficient lookup methods:
/// <list type="bullet">
///   <item><description><see cref="GetBoss"/> - Direct lookup by boss ID</description></item>
///   <item><description><see cref="GetAllBosses"/> - Full list for UI display and encounter selection</description></item>
///   <item><description><see cref="GetBossIds"/> - ID list for validation and quick existence checks</description></item>
///   <item><description><see cref="BossExists"/> - Fast existence check without loading definition</description></item>
///   <item><description><see cref="GetBossesByPhaseCount"/> - Filter bosses by complexity</description></item>
/// </list>
/// </para>
/// <para>
/// Boss definitions extend the monster system with multi-phase encounters.
/// Each boss references a <see cref="BossDefinition.BaseMonsterDefinitionId"/>
/// for base stats, with phase-specific modifications applied during combat.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Get a specific boss for an encounter
/// var skeletonKing = bossProvider.GetBoss("skeleton-king");
/// if (skeletonKing != null)
/// {
///     var startingPhase = skeletonKing.GetStartingPhase();
///     Console.WriteLine($"Starting {skeletonKing.Name} encounter in {startingPhase?.Name} phase");
/// }
///
/// // Get all multi-phase bosses for content filtering
/// var complexBosses = bossProvider.GetBossesByPhaseCount(minPhases: 3);
/// </code>
/// </example>
public interface IBossProvider
{
    /// <summary>
    /// Gets a boss definition by its unique identifier.
    /// </summary>
    /// <param name="bossId">The boss identifier to look up.</param>
    /// <returns>The boss definition, or null if not found.</returns>
    /// <remarks>
    /// <para>Lookup is case-insensitive.</para>
    /// <para>Returns null for unknown boss IDs rather than throwing.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var boss = bossProvider.GetBoss("volcanic-wyrm");
    /// if (boss != null)
    /// {
    ///     var phase = boss.GetPhaseForHealth(currentHealthPercent);
    /// }
    /// </code>
    /// </example>
    BossDefinition? GetBoss(string bossId);

    /// <summary>
    /// Gets all loaded boss definitions.
    /// </summary>
    /// <returns>A read-only list of all boss definitions.</returns>
    /// <remarks>
    /// <para>Returns an empty list if no bosses are loaded.</para>
    /// <para>Results are not filtered by any criteria.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var allBosses = bossProvider.GetAllBosses();
    /// foreach (var boss in allBosses)
    /// {
    ///     Console.WriteLine($"{boss.Name}: {boss.PhaseCount} phases");
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<BossDefinition> GetAllBosses();

    /// <summary>
    /// Gets the IDs of all loaded bosses.
    /// </summary>
    /// <returns>A read-only list of boss IDs.</returns>
    /// <remarks>
    /// <para>Useful for validation, dropdown population, and quick enumeration.</para>
    /// <para>IDs are returned in lowercase format.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var bossIds = bossProvider.GetBossIds();
    /// var randomBossId = bossIds[Random.Shared.Next(bossIds.Count)];
    /// </code>
    /// </example>
    IReadOnlyList<string> GetBossIds();

    /// <summary>
    /// Checks if a boss with the specified ID exists.
    /// </summary>
    /// <param name="bossId">The boss identifier to check.</param>
    /// <returns>True if the boss exists, false otherwise.</returns>
    /// <remarks>
    /// <para>More efficient than <see cref="GetBoss"/> when you only need existence check.</para>
    /// <para>Lookup is case-insensitive.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (bossProvider.BossExists(encounterId))
    /// {
    ///     StartBossEncounter(encounterId);
    /// }
    /// </code>
    /// </example>
    bool BossExists(string bossId);

    /// <summary>
    /// Gets bosses filtered by minimum phase count.
    /// </summary>
    /// <param name="minPhases">The minimum number of phases required.</param>
    /// <returns>Bosses with at least the specified number of phases.</returns>
    /// <remarks>
    /// <para>Useful for filtering by encounter complexity.</para>
    /// <para>A boss with exactly <paramref name="minPhases"/> phases is included.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Get only complex bosses with 3+ phases
    /// var hardBosses = bossProvider.GetBossesByPhaseCount(minPhases: 3);
    ///
    /// // Get all bosses (minimum 1 phase)
    /// var allBosses = bossProvider.GetBossesByPhaseCount(minPhases: 1);
    /// </code>
    /// </example>
    IReadOnlyList<BossDefinition> GetBossesByPhaseCount(int minPhases);
}
