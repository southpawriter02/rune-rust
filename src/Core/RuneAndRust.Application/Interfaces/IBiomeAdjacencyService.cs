using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Validates and queries realm biome adjacency relationships.
/// </summary>
/// <remarks>
/// <para>
/// IBiomeAdjacencyService enforces rules about which realms can neighbor
/// each other in dungeon generation. It prevents invalid combinations
/// (like Fire/Ice) and determines transition requirements.
/// </para>
/// <para>
/// Critical Incompatibilities:
/// <list type="bullet">
/// <item>Muspelheim ↔ Niflheim (Fire/Ice)</item>
/// <item>Muspelheim ↔ Vanaheim (Fire/Bio)</item>
/// </list>
/// </para>
/// </remarks>
public interface IBiomeAdjacencyService
{
    /// <summary>
    /// Checks if two realms can be adjacent.
    /// </summary>
    /// <param name="realmA">First realm.</param>
    /// <param name="realmB">Second realm.</param>
    /// <returns>True if realms can neighbor (Compatible or RequiresTransition).</returns>
    bool CanBiomesNeighbor(RealmId realmA, RealmId realmB);

    /// <summary>
    /// Gets the compatibility classification for two realms.
    /// </summary>
    /// <param name="realmA">First realm.</param>
    /// <param name="realmB">Second realm.</param>
    /// <returns>The compatibility level between the realms.</returns>
    BiomeCompatibility GetCompatibility(RealmId realmA, RealmId realmB);

    /// <summary>
    /// Gets required transition room count for two realms.
    /// </summary>
    /// <param name="realmA">First realm.</param>
    /// <param name="realmB">Second realm.</param>
    /// <returns>Tuple of (minimum, maximum) transition rooms required.</returns>
    /// <exception cref="InvalidOperationException">If realms are incompatible.</exception>
    (int Min, int Max) GetTransitionRoomCount(RealmId realmA, RealmId realmB);

    /// <summary>
    /// Gets the transition theme description for two realms.
    /// </summary>
    /// <param name="fromRealm">Source realm.</param>
    /// <param name="toRealm">Destination realm.</param>
    /// <returns>Narrative description of the transition, or null.</returns>
    string? GetTransitionTheme(RealmId fromRealm, RealmId toRealm);

    /// <summary>
    /// Gets all realms that can be adjacent to the specified realm.
    /// </summary>
    /// <param name="realmId">The realm to query.</param>
    /// <returns>List of realms that can neighbor the specified realm.</returns>
    IReadOnlyList<RealmId> GetAdjacentRealms(RealmId realmId);

    /// <summary>
    /// Validates that a realm configuration doesn't violate adjacency rules.
    /// </summary>
    /// <param name="adjacentPairs">Collection of adjacent realm pairs.</param>
    /// <returns>True if all pairs are valid neighbors.</returns>
    bool ValidateRealmConfiguration(IEnumerable<(RealmId, RealmId)> adjacentPairs);
}
