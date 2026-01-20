using RuneAndRust.Application.DTOs;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for terrain calculations and management.
/// </summary>
/// <remarks>
/// <para>
/// Provides terrain-related operations for the combat grid including
/// movement cost calculations, passability checks, and hazard damage.
/// </para>
/// </remarks>
public interface ITerrainService
{
    /// <summary>
    /// Gets the total movement cost for moving to a position from a given direction.
    /// </summary>
    /// <param name="position">The target position.</param>
    /// <param name="direction">The movement direction.</param>
    /// <returns>The total movement point cost including terrain modifier.</returns>
    int GetMovementCost(GridPosition position, MovementDirection direction);

    /// <summary>
    /// Gets the movement cost multiplier for a position.
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <returns>The terrain's movement cost multiplier (1.0 for normal terrain).</returns>
    float GetMovementCostMultiplier(GridPosition position);

    /// <summary>
    /// Checks if a position is passable.
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <returns><c>true</c> if entities can enter the position; otherwise, <c>false</c>.</returns>
    bool IsPassable(GridPosition position);

    /// <summary>
    /// Checks if terrain at a position deals damage.
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <returns><c>true</c> if the terrain is hazardous; otherwise, <c>false</c>.</returns>
    bool DealsDamage(GridPosition position);

    /// <summary>
    /// Gets the potential terrain damage for a position without applying it.
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <returns>The damage result, or <c>null</c> if terrain doesn't deal damage.</returns>
    TerrainDamageResult? GetTerrainDamage(GridPosition position);

    /// <summary>
    /// Applies terrain damage to an entity at a position.
    /// </summary>
    /// <param name="entityId">The entity taking damage.</param>
    /// <param name="position">The terrain position.</param>
    /// <returns>The result of applying terrain damage.</returns>
    TerrainDamageResult ApplyTerrainDamage(Guid entityId, GridPosition position);

    /// <summary>
    /// Gets the terrain type at a position.
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <returns>The terrain type at the position.</returns>
    TerrainType GetTerrainType(GridPosition position);

    /// <summary>
    /// Gets the terrain definition at a position.
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <returns>The terrain definition, or <c>null</c> if using base terrain type.</returns>
    TerrainDefinition? GetTerrainDefinition(GridPosition position);

    /// <summary>
    /// Sets the terrain type at a position.
    /// </summary>
    /// <param name="position">The position to modify.</param>
    /// <param name="type">The terrain type to set.</param>
    void SetTerrain(GridPosition position, TerrainType type);

    /// <summary>
    /// Sets the terrain by definition ID at a position.
    /// </summary>
    /// <param name="position">The position to modify.</param>
    /// <param name="terrainDefinitionId">The terrain definition ID.</param>
    void SetTerrain(GridPosition position, string terrainDefinitionId);

    /// <summary>
    /// Gets all loaded terrain definitions.
    /// </summary>
    /// <returns>All terrain definitions from configuration.</returns>
    IEnumerable<TerrainDefinition> GetAllTerrainDefinitions();
}
