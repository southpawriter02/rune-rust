namespace RuneAndRust.Presentation.Gui.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using RuneAndRust.Domain.Entities;

/// <summary>
/// View model for an entity token on the combat grid.
/// </summary>
public partial class EntityTokenViewModel : ViewModelBase
{
    private readonly int _cellSize;
    private readonly int _x;
    private readonly int _y;
    private readonly string _designSymbol;
    private readonly string _designColor;

    /// <summary>
    /// Gets the combatant this token represents.
    /// </summary>
    public Combatant? Combatant { get; }

    /// <summary>
    /// Gets or sets whether this token is selected.
    /// </summary>
    [ObservableProperty]
    private bool _isSelected;

    /// <summary>
    /// Gets or sets whether it's this entity's turn.
    /// </summary>
    [ObservableProperty]
    private bool _isCurrentTurn;

    /// <summary>
    /// Gets the token symbol.
    /// </summary>
    public string Symbol => Combatant?.IsPlayer == true 
        ? "@" 
        : Combatant?.DisplayName.FirstOrDefault().ToString() ?? _designSymbol;

    /// <summary>
    /// Gets the token color based on faction.
    /// </summary>
    /// <remarks>
    /// Players are green, enemies are orange-red, allies would be cyan.
    /// </remarks>
    public string Color => Combatant?.IsPlayer == true
        ? "#32CD32" // LimeGreen
        : Combatant?.IsMonster == true
            ? "#FF4500" // OrangeRed
            : _designColor;

    /// <summary>
    /// Gets the X position on the canvas.
    /// </summary>
    public double CanvasX => _x * _cellSize;

    /// <summary>
    /// Gets the Y position on the canvas.
    /// </summary>
    public double CanvasY => _y * _cellSize;

    /// <summary>
    /// Gets the entity name for display.
    /// </summary>
    public string Name => Combatant?.DisplayName ?? _designSymbol;

    /// <summary>
    /// Gets the tooltip text.
    /// </summary>
    public string Tooltip => Combatant is not null
        ? $"{Combatant.DisplayName}\nHP: {Combatant.CurrentHealth}/{Combatant.MaxHealth}"
        : Name;

    /// <summary>
    /// Creates a new EntityTokenViewModel from a combatant.
    /// </summary>
    /// <param name="combatant">The combatant entity.</param>
    /// <param name="x">X grid position.</param>
    /// <param name="y">Y grid position.</param>
    /// <param name="cellSize">The cell size in pixels.</param>
    public EntityTokenViewModel(Combatant combatant, int x, int y, int cellSize = 40)
    {
        Combatant = combatant;
        _x = x;
        _y = y;
        _cellSize = cellSize;
        _designSymbol = "@";
        _designColor = "#32CD32";
    }

    /// <summary>
    /// Design-time constructor.
    /// </summary>
    public EntityTokenViewModel(string name, string symbol, string color, int x, int y, int cellSize)
    {
        Combatant = null;
        _x = x;
        _y = y;
        _cellSize = cellSize;
        _designSymbol = symbol;
        _designColor = color;
    }

    /// <summary>
    /// Parameterless design-time constructor.
    /// </summary>
    public EntityTokenViewModel()
    {
        _x = 0;
        _y = 0;
        _cellSize = 40;
        _designSymbol = "@";
        _designColor = "#32CD32";
    }
}
