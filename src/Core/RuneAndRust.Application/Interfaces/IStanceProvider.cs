using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Provides access to combat stance definitions loaded from configuration.
/// </summary>
/// <remarks>
/// <para>IStanceProvider is responsible for loading and caching stance definitions
/// from JSON configuration files.</para>
/// <para>Implementations should ensure thread-safe access to stance definitions
/// and validate configuration on load.</para>
/// </remarks>
public interface IStanceProvider
{
    /// <summary>
    /// Gets a stance definition by its <see cref="CombatStance"/> enum value.
    /// </summary>
    /// <param name="stance">The combat stance to retrieve.</param>
    /// <returns>
    /// The matching <see cref="StanceDefinition"/>, or null if not found.
    /// </returns>
    /// <remarks>
    /// Converts the enum value to lowercase for lookup (e.g., Aggressive -> "aggressive").
    /// </remarks>
    StanceDefinition? GetStance(CombatStance stance);

    /// <summary>
    /// Gets a stance definition by its string identifier.
    /// </summary>
    /// <param name="stanceId">The unique identifier (e.g., "aggressive", "defensive").</param>
    /// <returns>
    /// The matching <see cref="StanceDefinition"/>, or null if not found.
    /// </returns>
    /// <remarks>
    /// Lookup is case-insensitive.
    /// </remarks>
    StanceDefinition? GetStance(string stanceId);

    /// <summary>
    /// Gets the default stance definition.
    /// </summary>
    /// <returns>
    /// The <see cref="StanceDefinition"/> marked as <see cref="StanceDefinition.IsDefault"/>,
    /// or the "balanced" stance if no default is configured.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no default stance is configured and no "balanced" stance exists.
    /// </exception>
    StanceDefinition GetDefaultStance();

    /// <summary>
    /// Gets all available stance definitions.
    /// </summary>
    /// <returns>
    /// A read-only list of all configured <see cref="StanceDefinition"/> instances.
    /// </returns>
    /// <remarks>
    /// Returns an empty list if no stances are configured (though this is an error state).
    /// </remarks>
    IReadOnlyList<StanceDefinition> GetAllStances();

    /// <summary>
    /// Checks whether a stance with the given identifier exists.
    /// </summary>
    /// <param name="stanceId">The stance identifier to check.</param>
    /// <returns>True if the stance exists, false otherwise.</returns>
    bool StanceExists(string stanceId);

    /// <summary>
    /// Gets the total number of configured stances.
    /// </summary>
    /// <returns>The count of loaded stance definitions.</returns>
    int Count { get; }
}
