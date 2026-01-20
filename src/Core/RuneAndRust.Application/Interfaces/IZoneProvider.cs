using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Provides zone definitions from configuration.
/// </summary>
/// <remarks>
/// <para>IZoneProvider is responsible for loading and providing access to zone definitions
/// that have been configured in zones.json. These definitions serve as templates for
/// creating active <see cref="RuneAndRust.Domain.Entities.ZoneEffect"/> instances.</para>
/// <para>Implementations should cache loaded definitions for performance.</para>
/// <para>Common usage patterns:</para>
/// <list type="bullet">
///   <item><description>Get a specific zone by ID when creating a zone effect</description></item>
///   <item><description>Query zones by effect type for UI display or AI decision-making</description></item>
///   <item><description>Get all zones for configuration validation</description></item>
/// </list>
/// </remarks>
public interface IZoneProvider
{
    /// <summary>
    /// Gets a zone definition by its unique identifier.
    /// </summary>
    /// <param name="zoneId">The zone definition ID (case-insensitive).</param>
    /// <returns>The zone definition, or null if not found.</returns>
    /// <example>
    /// <code>
    /// var wallOfFire = zoneProvider.GetZone("wall-of-fire");
    /// if (wallOfFire != null)
    /// {
    ///     var zone = zoneService.CreateZone(wallOfFire, position, caster);
    /// }
    /// </code>
    /// </example>
    ZoneDefinition? GetZone(string zoneId);

    /// <summary>
    /// Gets all configured zone definitions.
    /// </summary>
    /// <returns>A read-only list of all zone definitions.</returns>
    IReadOnlyList<ZoneDefinition> GetAllZones();

    /// <summary>
    /// Gets zone definitions filtered by effect type.
    /// </summary>
    /// <param name="effectType">The effect type to filter by.</param>
    /// <returns>Zone definitions matching the specified effect type.</returns>
    /// <example>
    /// <code>
    /// // Get all damage zones for AI targeting
    /// var damageZones = zoneProvider.GetZonesByType(ZoneEffectType.Damage);
    ///
    /// // Get all healing zones for healer AI
    /// var healingZones = zoneProvider.GetZonesByType(ZoneEffectType.Healing);
    /// </code>
    /// </example>
    IReadOnlyList<ZoneDefinition> GetZonesByType(ZoneEffectType effectType);

    /// <summary>
    /// Gets zone definitions filtered by shape.
    /// </summary>
    /// <param name="shape">The shape to filter by.</param>
    /// <returns>Zone definitions with the specified shape.</returns>
    IReadOnlyList<ZoneDefinition> GetZonesByShape(ZoneShape shape);

    /// <summary>
    /// Gets the total number of configured zones.
    /// </summary>
    /// <returns>The count of zone definitions.</returns>
    int GetZoneCount();

    /// <summary>
    /// Checks if a zone definition exists.
    /// </summary>
    /// <param name="zoneId">The zone definition ID to check.</param>
    /// <returns>True if the zone exists; otherwise, false.</returns>
    bool ZoneExists(string zoneId);
}
