using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Provides access to realm biome definitions loaded from configuration.
/// </summary>
/// <remarks>
/// <para>
/// IRealmBiomeProvider abstracts the loading and retrieval of realm biome data,
/// allowing the application to access realm definitions without knowledge of
/// the underlying storage mechanism.
/// </para>
/// <para>
/// This interface is separate from IBiomeService which handles the generic
/// dungeon biome system (cave, volcanic, forest).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var midgard = biomeProvider.GetBiome(RealmId.Midgard);
/// var zones = biomeProvider.GetZonesForRealm(RealmId.Muspelheim);
/// var heatCondition = biomeProvider.GetEnvironmentalCondition(EnvironmentalConditionType.IntenseHeat);
/// </code>
/// </example>
public interface IRealmBiomeProvider
{
    /// <summary>
    /// Gets all realm biome definitions.
    /// </summary>
    /// <returns>List of all loaded realm biome definitions.</returns>
    IReadOnlyList<RealmBiomeDefinition> GetAllBiomes();

    /// <summary>
    /// Gets a specific realm biome by realm ID.
    /// </summary>
    /// <param name="realmId">The realm identifier.</param>
    /// <returns>The realm biome definition, or null if not found.</returns>
    RealmBiomeDefinition? GetBiome(RealmId realmId);

    /// <summary>
    /// Gets all zones for a specific realm.
    /// </summary>
    /// <param name="realmId">The realm identifier.</param>
    /// <returns>List of zones within the realm, or empty list if realm not found.</returns>
    IReadOnlyList<RealmBiomeZone> GetZonesForRealm(RealmId realmId);

    /// <summary>
    /// Gets a specific zone by realm and zone ID.
    /// </summary>
    /// <param name="realmId">The realm identifier.</param>
    /// <param name="zoneId">The zone identifier within the realm.</param>
    /// <returns>The zone, or null if not found.</returns>
    RealmBiomeZone? GetZone(RealmId realmId, string zoneId);

    /// <summary>
    /// Gets the environmental condition definition for a type.
    /// </summary>
    /// <param name="type">The condition type.</param>
    /// <returns>The condition definition, or null if not found.</returns>
    EnvironmentalCondition? GetEnvironmentalCondition(EnvironmentalConditionType type);
}
