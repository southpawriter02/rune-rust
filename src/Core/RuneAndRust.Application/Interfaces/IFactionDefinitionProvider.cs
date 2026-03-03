using RuneAndRust.Application.DTOs;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Provider for faction definitions loaded from configuration (config/factions.json).
/// </summary>
/// <remarks>
/// <para>Follows the same provider pattern as <c>IQuestDefinitionProvider</c> and
/// <c>INpcDefinitionProvider</c>: read-only access to static configuration data
/// loaded at startup.</para>
///
/// <para>All lookups are case-insensitive on faction IDs.</para>
/// </remarks>
public interface IFactionDefinitionProvider
{
    /// <summary>
    /// Gets all faction definitions from the configuration.
    /// </summary>
    /// <returns>
    /// A read-only list of all faction definitions. Returns an empty list
    /// if the configuration file could not be loaded.
    /// </returns>
    IReadOnlyList<FactionDefinitionDto> GetAllFactions();

    /// <summary>
    /// Gets a specific faction definition by its ID.
    /// </summary>
    /// <param name="factionId">The faction ID to look up (case-insensitive).</param>
    /// <returns>
    /// The matching <see cref="FactionDefinitionDto"/>, or <c>null</c> if no faction
    /// with the given ID exists.
    /// </returns>
    FactionDefinitionDto? GetFaction(string factionId);

    /// <summary>
    /// Checks whether a faction with the given ID exists in the configuration.
    /// </summary>
    /// <param name="factionId">The faction ID to check (case-insensitive).</param>
    /// <returns><c>true</c> if the faction exists; <c>false</c> otherwise.</returns>
    bool FactionExists(string factionId);

    /// <summary>
    /// Gets the IDs of factions allied with the specified faction.
    /// </summary>
    /// <param name="factionId">The faction ID to query (case-insensitive).</param>
    /// <returns>
    /// A read-only list of allied faction IDs. Returns an empty list if the faction
    /// doesn't exist or has no allies.
    /// </returns>
    IReadOnlyList<string> GetAllies(string factionId);

    /// <summary>
    /// Gets the IDs of factions hostile to the specified faction.
    /// </summary>
    /// <param name="factionId">The faction ID to query (case-insensitive).</param>
    /// <returns>
    /// A read-only list of enemy faction IDs. Returns an empty list if the faction
    /// doesn't exist or has no enemies.
    /// </returns>
    IReadOnlyList<string> GetEnemies(string factionId);
}
