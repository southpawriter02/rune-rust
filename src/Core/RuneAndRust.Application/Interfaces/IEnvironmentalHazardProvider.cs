using RuneAndRust.Application.DTOs;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Provides access to environmental hazard definitions loaded from configuration.
/// </summary>
/// <remarks>
/// <para>IEnvironmentalHazardProvider is responsible for loading and caching hazard definitions
/// from JSON configuration files.</para>
/// <para>Hazard definitions specify damage, status effects, and behavior for environmental
/// hazards used in tactical combat (lava, spikes, pits, acid pools, etc.).</para>
/// <para>Implementations should ensure thread-safe access to hazard definitions
/// and validate configuration on load.</para>
/// </remarks>
/// <seealso cref="EnvironmentalHazardDefinition"/>
/// <seealso cref="HazardType"/>
public interface IEnvironmentalHazardProvider
{
    /// <summary>
    /// Gets a hazard definition by its <see cref="HazardType"/> enum value.
    /// </summary>
    /// <param name="type">The hazard type to retrieve.</param>
    /// <returns>
    /// The matching <see cref="EnvironmentalHazardDefinition"/>, or null if not found.
    /// </returns>
    /// <remarks>
    /// Not all <see cref="HazardType"/> values may have environmental combat definitions.
    /// Some hazard types (e.g., Darkness) do not deal damage and may not be configured.
    /// </remarks>
    EnvironmentalHazardDefinition? GetHazard(HazardType type);

    /// <summary>
    /// Gets a hazard definition by its string identifier.
    /// </summary>
    /// <param name="hazardId">The unique identifier (e.g., "lava", "spikes", "pit").</param>
    /// <returns>
    /// The matching <see cref="EnvironmentalHazardDefinition"/>, or null if not found.
    /// </returns>
    /// <remarks>
    /// Lookup is case-insensitive. The identifier typically matches the HazardType name in lowercase.
    /// </remarks>
    EnvironmentalHazardDefinition? GetHazard(string hazardId);

    /// <summary>
    /// Gets all available hazard definitions.
    /// </summary>
    /// <returns>
    /// A read-only list of all configured <see cref="EnvironmentalHazardDefinition"/> instances.
    /// </returns>
    /// <remarks>
    /// Returns an empty list if no hazards are configured.
    /// </remarks>
    IReadOnlyList<EnvironmentalHazardDefinition> GetAllHazards();

    /// <summary>
    /// Gets all hazard definitions that deal damage (either on entry or per turn).
    /// </summary>
    /// <returns>
    /// A read-only list of hazard definitions where <see cref="EnvironmentalHazardDefinition.DealsDamage"/> is true.
    /// </returns>
    IReadOnlyList<EnvironmentalHazardDefinition> GetDamagingHazards();

    /// <summary>
    /// Gets all hazard definitions that deal per-turn tick damage.
    /// </summary>
    /// <returns>
    /// A read-only list of hazard definitions where <see cref="EnvironmentalHazardDefinition.DamagePerTurn"/> is true.
    /// </returns>
    /// <remarks>
    /// Used for processing hazard damage at the start of each combat round.
    /// </remarks>
    IReadOnlyList<EnvironmentalHazardDefinition> GetTickDamageHazards();

    /// <summary>
    /// Checks whether a hazard definition with the given type exists.
    /// </summary>
    /// <param name="type">The hazard type to check.</param>
    /// <returns>True if the hazard definition exists, false otherwise.</returns>
    bool HazardExists(HazardType type);

    /// <summary>
    /// Checks whether a hazard definition with the given identifier exists.
    /// </summary>
    /// <param name="hazardId">The hazard identifier to check.</param>
    /// <returns>True if the hazard definition exists, false otherwise.</returns>
    /// <remarks>
    /// Lookup is case-insensitive.
    /// </remarks>
    bool HazardExists(string hazardId);

    /// <summary>
    /// Gets the total number of configured hazards.
    /// </summary>
    /// <returns>The count of loaded hazard definitions.</returns>
    int Count { get; }
}
