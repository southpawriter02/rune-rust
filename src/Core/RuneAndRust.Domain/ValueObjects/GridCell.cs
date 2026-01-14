using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a single cell on the combat grid.
/// </summary>
/// <remarks>
/// <para>
/// Each cell tracks its position, occupancy state, and terrain passability.
/// A cell can hold at most one entity (player or monster) at a time.
/// </para>
/// <para>
/// Display characters:
/// <list type="bullet">
/// <item><description><c>.</c> - Empty passable cell</description></item>
/// <item><description><c>@</c> - Player position</description></item>
/// <item><description><c>M</c> - Monster position</description></item>
/// <item><description><c>#</c> - Impassable terrain</description></item>
/// </list>
/// </para>
/// </remarks>
public class GridCell
{
    /// <summary>
    /// Gets the position of this cell on the grid.
    /// </summary>
    public GridPosition Position { get; }

    /// <summary>
    /// Gets a value indicating whether this cell is occupied by an entity.
    /// </summary>
    public bool IsOccupied => OccupantId.HasValue;

    /// <summary>
    /// Gets the unique identifier of the occupying entity, if any.
    /// </summary>
    public Guid? OccupantId { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the occupant is the player.
    /// </summary>
    public bool IsPlayerOccupied { get; private set; }

    /// <summary>
    /// Gets a value indicating whether entities can move through this cell.
    /// </summary>
    /// <remarks>
    /// Considers both explicit passability setting and terrain type.
    /// Impassable terrain (walls, pits) always returns <c>false</c>.
    /// </remarks>
    public bool IsPassable => _isPassable && TerrainType != TerrainType.Impassable;

    /// <summary>
    /// Backing field for passability.
    /// </summary>
    private bool _isPassable = true;

    // ===== Line of Sight Properties (v0.5.1c) =====

    /// <summary>
    /// Gets whether this cell explicitly blocks line of sight.
    /// </summary>
    /// <remarks>
    /// Set to true for pillars and obstacles that block LOS.
    /// Walls (impassable cells) automatically block LOS via EffectivelyBlocksLOS.
    /// </remarks>
    public bool BlocksLOS { get; private set; }

    /// <summary>
    /// Gets whether this cell blocks line of sight (computed).
    /// </summary>
    /// <remarks>
    /// Returns true if BlocksLOS is explicitly set, if the cell is impassable,
    /// or if the terrain type is Impassable.
    /// Walls and obstacles block LOS, but occupied cells do not.
    /// </remarks>
    public bool EffectivelyBlocksLOS => BlocksLOS || !IsPassable || TerrainType == TerrainType.Impassable;

    // ===== Terrain Properties (v0.5.2a) =====

    /// <summary>
    /// Gets the terrain type of this cell.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="Enums.TerrainType.Normal"/>.
    /// Affects movement costs and passability.
    /// </remarks>
    public TerrainType TerrainType { get; private set; } = TerrainType.Normal;

    /// <summary>
    /// Gets the terrain definition ID for custom terrain configuration.
    /// </summary>
    /// <remarks>
    /// References a terrain definition from <c>config/terrain.json</c>.
    /// When set, the terrain service uses the definition's properties.
    /// </remarks>
    public string? TerrainDefinitionId { get; private set; }

    /// <summary>
    /// Sets the terrain type for this cell.
    /// </summary>
    /// <param name="type">The terrain type to set.</param>
    /// <remarks>
    /// Clears any custom terrain definition ID when setting base type.
    /// </remarks>
    public void SetTerrain(TerrainType type)
    {
        TerrainType = type;
        TerrainDefinitionId = null;
    }

    /// <summary>
    /// Sets the terrain by definition ID.
    /// </summary>
    /// <param name="definitionId">The terrain definition ID (normalized to lowercase).</param>
    /// <remarks>
    /// The terrain service will look up the definition to get properties.
    /// </remarks>
    public void SetTerrainDefinition(string definitionId)
    {
        TerrainDefinitionId = definitionId?.ToLowerInvariant();
    }

    /// <summary>
    /// Gets the movement cost multiplier based on terrain type.
    /// </summary>
    /// <returns>
    /// 1.0 for Normal/Hazardous, 2.0 for Difficult, <see cref="float.MaxValue"/> for Impassable.
    /// </returns>
    /// <remarks>
    /// For cells with a <see cref="TerrainDefinitionId"/>, the terrain service
    /// should use the definition's multiplier instead of this base value.
    /// </remarks>
    public float GetMovementCostMultiplier() => TerrainType switch
    {
        TerrainType.Normal => 1.0f,
        TerrainType.Difficult => 2.0f,
        TerrainType.Hazardous => 1.0f,
        TerrainType.Impassable => float.MaxValue,
        _ => 1.0f
    };

    /// <summary>
    /// Sets whether this cell blocks line of sight.
    /// </summary>
    /// <param name="blocksLOS">Whether this cell should block LOS.</param>
    public void SetBlocksLOS(bool blocksLOS) => BlocksLOS = blocksLOS;

    /// <summary>
    /// Private constructor for factory pattern.
    /// </summary>
    private GridCell(GridPosition position)
    {
        Position = position;
    }

    /// <summary>
    /// Creates a new empty grid cell at the specified position.
    /// </summary>
    /// <param name="position">The position for this cell.</param>
    /// <returns>A new empty, passable GridCell.</returns>
    public static GridCell Create(GridPosition position) => new(position);

    /// <summary>
    /// Creates a new empty grid cell at the specified coordinates.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <returns>A new empty, passable GridCell.</returns>
    public static GridCell Create(int x, int y) => new(new GridPosition(x, y));

    /// <summary>
    /// Attempts to place an entity in this cell.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity.</param>
    /// <param name="isPlayer">Whether the entity is the player.</param>
    /// <returns>
    /// <c>true</c> if the entity was placed successfully;
    /// <c>false</c> if the cell is already occupied or impassable.
    /// </returns>
    public bool PlaceEntity(Guid entityId, bool isPlayer)
    {
        if (IsOccupied || !IsPassable)
            return false;

        OccupantId = entityId;
        IsPlayerOccupied = isPlayer;
        return true;
    }

    /// <summary>
    /// Removes the entity from this cell.
    /// </summary>
    /// <returns>
    /// <c>true</c> if an entity was removed;
    /// <c>false</c> if the cell was already empty.
    /// </returns>
    public bool RemoveEntity()
    {
        if (!IsOccupied)
            return false;

        OccupantId = null;
        IsPlayerOccupied = false;
        return true;
    }

    /// <summary>
    /// Sets whether this cell is passable for movement.
    /// </summary>
    /// <param name="passable">Whether entities can enter this cell.</param>
    public void SetPassable(bool passable) => _isPassable = passable;

    /// <summary>
    /// Gets the ASCII character for displaying this cell.
    /// </summary>
    /// <returns>
    /// <c>#</c> for impassable, <c>@</c> for player, <c>M</c> for monster,
    /// or <c>.</c> for empty passable cells.
    /// </returns>
    public char GetDisplayChar()
    {
        if (!IsPassable)
            return '#';
        if (IsPlayerOccupied)
            return '@';
        if (IsOccupied)
            return 'M';
        return '.';
    }
}
