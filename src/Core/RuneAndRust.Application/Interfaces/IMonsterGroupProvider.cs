using RuneAndRust.Domain.Definitions;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Provides monster group definitions from configuration for the group encounter system (v0.10.4c).
/// </summary>
/// <remarks>
/// <para>
/// IMonsterGroupProvider is the primary interface for accessing group definitions loaded from configuration.
/// Implementations should load groups at startup and provide efficient lookup methods:
/// <list type="bullet">
///   <item><description><see cref="GetGroup"/> - Direct lookup by group ID</description></item>
///   <item><description><see cref="GetAllGroups"/> - Full list for UI display and encounter selection</description></item>
///   <item><description><see cref="GetGroupIds"/> - ID list for validation and quick existence checks</description></item>
///   <item><description><see cref="GroupExists"/> - Fast existence check without loading definition</description></item>
///   <item><description><see cref="GetGroupsByTag"/> - Filter groups by tags (e.g., "dungeon-level-1")</description></item>
/// </list>
/// </para>
/// <para>
/// Monster group definitions extend the monster system with coordinated group encounters.
/// Each group defines member compositions, tactical behaviors, and synergy effects
/// that enhance group combat effectiveness.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Get a specific group for an encounter
/// var goblinWarband = groupProvider.GetGroup("goblin-warband");
/// if (goblinWarband != null)
/// {
///     Console.WriteLine($"Spawning {goblinWarband.Name} with {goblinWarband.TotalMemberCount} members");
///     Console.WriteLine($"Tactics: {string.Join(", ", goblinWarband.Tactics)}");
/// }
///
/// // Get all groups tagged for a dungeon level
/// var level1Groups = groupProvider.GetGroupsByTag("dungeon-level-1");
/// var randomGroup = level1Groups[Random.Shared.Next(level1Groups.Count)];
/// </code>
/// </example>
public interface IMonsterGroupProvider
{
    /// <summary>
    /// Gets a monster group definition by its unique identifier.
    /// </summary>
    /// <param name="groupId">The group identifier to look up.</param>
    /// <returns>The group definition, or null if not found.</returns>
    /// <remarks>
    /// <para>Lookup is case-insensitive.</para>
    /// <para>Returns null for unknown group IDs rather than throwing.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var group = groupProvider.GetGroup("skeleton-patrol");
    /// if (group != null)
    /// {
    ///     var members = group.Members;
    ///     var tactics = group.Tactics;
    /// }
    /// </code>
    /// </example>
    MonsterGroupDefinition? GetGroup(string groupId);

    /// <summary>
    /// Gets all loaded monster group definitions.
    /// </summary>
    /// <returns>A read-only list of all group definitions.</returns>
    /// <remarks>
    /// <para>Returns an empty list if no groups are loaded.</para>
    /// <para>Results are not filtered by any criteria.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var allGroups = groupProvider.GetAllGroups();
    /// foreach (var group in allGroups)
    /// {
    ///     Console.WriteLine($"{group.Name}: {group.TotalMemberCount} members, {group.Tactics.Count} tactics");
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<MonsterGroupDefinition> GetAllGroups();

    /// <summary>
    /// Gets the IDs of all loaded groups.
    /// </summary>
    /// <returns>A read-only list of group IDs.</returns>
    /// <remarks>
    /// <para>Useful for validation, dropdown population, and quick enumeration.</para>
    /// <para>IDs are returned in lowercase format.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var groupIds = groupProvider.GetGroupIds();
    /// var randomGroupId = groupIds[Random.Shared.Next(groupIds.Count)];
    /// </code>
    /// </example>
    IReadOnlyList<string> GetGroupIds();

    /// <summary>
    /// Checks if a monster group with the specified ID exists.
    /// </summary>
    /// <param name="groupId">The group identifier to check.</param>
    /// <returns>True if the group exists, false otherwise.</returns>
    /// <remarks>
    /// <para>More efficient than <see cref="GetGroup"/> when you only need existence check.</para>
    /// <para>Lookup is case-insensitive.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (groupProvider.GroupExists(encounterId))
    /// {
    ///     RegisterGroupEncounter(encounterId);
    /// }
    /// </code>
    /// </example>
    bool GroupExists(string groupId);

    /// <summary>
    /// Gets groups filtered by a specific tag.
    /// </summary>
    /// <param name="tag">The tag to filter by (case-insensitive).</param>
    /// <returns>Groups that have the specified tag.</returns>
    /// <remarks>
    /// <para>
    /// Tags are used to categorize groups for spawn selection. Common tags include:
    /// <list type="bullet">
    ///   <item><description><c>dungeon-level-1</c>, <c>dungeon-level-2</c> - Dungeon difficulty</description></item>
    ///   <item><description><c>undead</c>, <c>goblinoid</c>, <c>beast</c> - Creature type</description></item>
    ///   <item><description><c>patrol</c>, <c>ambush</c>, <c>guard</c> - Encounter type</description></item>
    /// </list>
    /// </para>
    /// <para>Returns an empty list if no groups have the tag.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Get groups suitable for dungeon level 2
    /// var level2Groups = groupProvider.GetGroupsByTag("dungeon-level-2");
    ///
    /// // Get all undead groups
    /// var undeadGroups = groupProvider.GetGroupsByTag("undead");
    /// </code>
    /// </example>
    IReadOnlyList<MonsterGroupDefinition> GetGroupsByTag(string tag);

    /// <summary>
    /// Reloads group definitions from the configuration source.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used for hot-reloading group definitions during development or when
    /// configuration files change. Not typically called in production.
    /// </para>
    /// <para>
    /// After reload, all cached definitions are refreshed. Active group instances
    /// retain their original definitions until re-registered.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Reload after configuration change
    /// groupProvider.Reload();
    /// var updatedGroup = groupProvider.GetGroup("goblin-warband");
    /// </code>
    /// </example>
    void Reload();
}
