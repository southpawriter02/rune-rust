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
    public bool IsPassable { get; private set; } = true;

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
    /// Returns true if BlocksLOS is explicitly set or if the cell is impassable.
    /// Walls and obstacles block LOS, but occupied cells do not.
    /// </remarks>
    public bool EffectivelyBlocksLOS => BlocksLOS || !IsPassable;

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
    public void SetPassable(bool passable) => IsPassable = passable;

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
