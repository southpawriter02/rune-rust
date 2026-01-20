namespace RuneAndRust.Presentation.Gui.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Presentation.Gui.Enums;
using HighlightType = RuneAndRust.Domain.Enums.HighlightType;

/// <summary>
/// View model for a combat grid cell.
/// </summary>
public partial class GridCellViewModel : ViewModelBase
{
    /// <summary>
    /// Gets the grid position of this cell.
    /// </summary>
    public GridPosition Position { get; }

    /// <summary>
    /// Gets or sets the cell terrain type.
    /// </summary>
    [ObservableProperty]
    private GridCellType _cellType;

    /// <summary>
    /// Gets or sets whether the cell is walkable.
    /// </summary>
    [ObservableProperty]
    private bool _isWalkable;

    /// <summary>
    /// Gets or sets whether the cell is highlighted.
    /// </summary>
    [ObservableProperty]
    private bool _isHighlighted;

    /// <summary>
    /// Gets or sets the highlight type.
    /// </summary>
    [ObservableProperty]
    private HighlightType _highlightType;

    /// <summary>
    /// Gets or sets whether the cell contains an entity.
    /// </summary>
    [ObservableProperty]
    private bool _hasEntity;

    /// <summary>
    /// Gets the coordinate label (e.g., "A1", "B3").
    /// </summary>
    public string CoordinateLabel => $"{(char)('A' + Position.Y)}{Position.X + 1}";

    /// <summary>
    /// Gets the tooltip text.
    /// </summary>
    public string Tooltip => $"{CoordinateLabel}: {CellType}";

    /// <summary>
    /// Creates a new GridCellViewModel from a position and cell.
    /// </summary>
    public GridCellViewModel(GridPosition position, GridCell cell)
    {
        Position = position;
        UpdateFromCell(cell);
    }

    /// <summary>
    /// Creates a new GridCellViewModel with explicit type (design-time).
    /// </summary>
    public GridCellViewModel(GridPosition position, GridCellType cellType)
    {
        Position = position;
        CellType = cellType;
        IsWalkable = cellType != GridCellType.Wall;
    }

    /// <summary>
    /// Updates this view model from a grid cell.
    /// </summary>
    public void UpdateFromCell(GridCell cell)
    {
        CellType = MapCellType(cell.TerrainType);
        IsWalkable = cell.IsPassable;
        HasEntity = cell.IsOccupied;
    }

    private static GridCellType MapCellType(TerrainType terrain)
    {
        return terrain switch
        {
            TerrainType.Normal => GridCellType.Floor,
            TerrainType.Difficult => GridCellType.Water, // Difficult terrain shown as water
            TerrainType.Hazardous => GridCellType.Hazard,
            TerrainType.Impassable => GridCellType.Wall,
            _ => GridCellType.Floor
        };
    }
}
