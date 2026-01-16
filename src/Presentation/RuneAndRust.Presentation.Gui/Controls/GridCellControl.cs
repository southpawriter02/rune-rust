namespace RuneAndRust.Presentation.Gui.Controls;

using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Presentation.Gui.Enums;
using DomainHighlightType = RuneAndRust.Domain.Enums.HighlightType;

/// <summary>
/// Represents a single cell in the combat grid.
/// </summary>
public class GridCellControl : TemplatedControl
{
    /// <summary>
    /// Defines the CellType property.
    /// </summary>
    public static readonly StyledProperty<GridCellType> CellTypeProperty =
        AvaloniaProperty.Register<GridCellControl, GridCellType>(nameof(CellType));

    /// <summary>
    /// Defines the IsHighlighted property.
    /// </summary>
    public static readonly StyledProperty<bool> IsHighlightedProperty =
        AvaloniaProperty.Register<GridCellControl, bool>(nameof(IsHighlighted));

    /// <summary>
    /// Defines the HighlightType property.
    /// </summary>
    public static readonly StyledProperty<HighlightType> HighlightTypeProperty =
        AvaloniaProperty.Register<GridCellControl, HighlightType>(nameof(HighlightType));

    /// <summary>
    /// Defines the HasEntity property.
    /// </summary>
    public static readonly StyledProperty<bool> HasEntityProperty =
        AvaloniaProperty.Register<GridCellControl, bool>(nameof(HasEntity));

    /// <summary>
    /// Gets or sets the cell terrain type.
    /// </summary>
    public GridCellType CellType
    {
        get => GetValue(CellTypeProperty);
        set => SetValue(CellTypeProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the cell is highlighted.
    /// </summary>
    public bool IsHighlighted
    {
        get => GetValue(IsHighlightedProperty);
        set => SetValue(IsHighlightedProperty, value);
    }

    /// <summary>
    /// Gets or sets the highlight type.
    /// </summary>
    public HighlightType HighlightType
    {
        get => GetValue(HighlightTypeProperty);
        set => SetValue(HighlightTypeProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the cell contains an entity.
    /// </summary>
    public bool HasEntity
    {
        get => GetValue(HasEntityProperty);
        set => SetValue(HasEntityProperty, value);
    }

    /// <summary>
    /// Gets the background brush for this cell type.
    /// </summary>
    public IBrush CellBackground => CellType switch
    {
        GridCellType.Floor => Brushes.DimGray,
        GridCellType.Wall => Brushes.DarkSlateGray,
        GridCellType.Water => Brushes.DarkBlue,
        GridCellType.Hazard => Brushes.DarkRed,
        GridCellType.Cover => Brushes.SaddleBrown,
        _ => Brushes.Black
    };

    /// <summary>
    /// Gets the symbol for this cell type.
    /// </summary>
    public string CellSymbol => CellType switch
    {
        GridCellType.Floor => "░",
        GridCellType.Wall => "█",
        GridCellType.Water => "~",
        GridCellType.Hazard => "☼",
        GridCellType.Cover => "▓",
        _ => " "
    };

    /// <summary>
    /// Gets the highlight overlay color.
    /// </summary>
    public IBrush HighlightColor => HighlightType switch
    {
        DomainHighlightType.Movement => new SolidColorBrush(Color.FromArgb(128, 0, 128, 255)),
        DomainHighlightType.Attack => new SolidColorBrush(Color.FromArgb(128, 255, 0, 0)),
        DomainHighlightType.Ability => new SolidColorBrush(Color.FromArgb(128, 128, 0, 255)),
        DomainHighlightType.Selected => new SolidColorBrush(Color.FromArgb(64, 255, 255, 255)),
        _ => Brushes.Transparent
    };

    static GridCellControl()
    {
        CellTypeProperty.Changed.AddClassHandler<GridCellControl>((c, _) => c.OnCellTypeChanged());
        HighlightTypeProperty.Changed.AddClassHandler<GridCellControl>((c, _) => c.OnHighlightChanged());
    }

    private void OnCellTypeChanged()
    {
        Classes.Set("wall", CellType == GridCellType.Wall);
        Classes.Set("water", CellType == GridCellType.Water);
        Classes.Set("hazard", CellType == GridCellType.Hazard);
        Classes.Set("cover", CellType == GridCellType.Cover);
    }

    private void OnHighlightChanged()
    {
        Classes.Set("highlighted", IsHighlighted);
        Classes.Set("movement-highlight", HighlightType == DomainHighlightType.Movement);
        Classes.Set("attack-highlight", HighlightType == DomainHighlightType.Attack);
        Classes.Set("ability-highlight", HighlightType == DomainHighlightType.Ability);
    }
}
