using ReactiveUI;
using RuneAndRust.Core;
using RuneAndRust.DesktopUI.Services;
using SkiaSharp;
using System.Collections.Generic;
using System.Reactive;

namespace RuneAndRust.DesktopUI.ViewModels;

/// <summary>
/// View model for the combat view.
/// Manages grid state, unit positions, and combat interactions.
/// </summary>
public class CombatViewModel : ViewModelBase
{
    private readonly ISpriteService _spriteService;
    private int _columns = 3;
    private double _cellSize = 80.0;
    private GridPosition? _selectedPosition;
    private GridPosition? _hoveredPosition;
    private HashSet<GridPosition> _highlightedPositions = new();
    private Dictionary<GridPosition, SKBitmap> _unitSprites = new();
    private string _statusMessage = "Select a unit to begin combat";

    /// <summary>
    /// Gets the view title.
    /// </summary>
    public string Title => "Tactical Combat";

    /// <summary>
    /// Gets or sets the number of columns in the grid.
    /// </summary>
    public int Columns
    {
        get => _columns;
        set => this.RaiseAndSetIfChanged(ref _columns, value);
    }

    /// <summary>
    /// Gets or sets the cell size in pixels.
    /// </summary>
    public double CellSize
    {
        get => _cellSize;
        set => this.RaiseAndSetIfChanged(ref _cellSize, value);
    }

    /// <summary>
    /// Gets or sets the currently selected grid position.
    /// </summary>
    public GridPosition? SelectedPosition
    {
        get => _selectedPosition;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedPosition, value);
            UpdateHighlightedPositions();
            UpdateStatusMessage();
        }
    }

    /// <summary>
    /// Gets or sets the currently hovered grid position.
    /// </summary>
    public GridPosition? HoveredPosition
    {
        get => _hoveredPosition;
        set => this.RaiseAndSetIfChanged(ref _hoveredPosition, value);
    }

    /// <summary>
    /// Gets the collection of highlighted grid positions (e.g., valid move/attack targets).
    /// </summary>
    public IReadOnlyCollection<GridPosition> HighlightedPositions => _highlightedPositions;

    /// <summary>
    /// Gets the dictionary of unit sprites at grid positions.
    /// </summary>
    public Dictionary<GridPosition, SKBitmap> UnitSprites
    {
        get => _unitSprites;
        private set => this.RaiseAndSetIfChanged(ref _unitSprites, value);
    }

    /// <summary>
    /// Gets the current status message.
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        private set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    /// <summary>
    /// Command to handle cell clicks.
    /// </summary>
    public ReactiveCommand<GridPosition, Unit> CellClickedCommand { get; }

    /// <summary>
    /// Command to handle cell hover.
    /// </summary>
    public ReactiveCommand<GridPosition, Unit> CellHoveredCommand { get; }

    public CombatViewModel(ISpriteService spriteService)
    {
        _spriteService = spriteService;

        // Initialize commands
        CellClickedCommand = ReactiveCommand.Create<GridPosition>(OnCellClicked);
        CellHoveredCommand = ReactiveCommand.Create<GridPosition>(OnCellHovered);

        // Initialize demo combat scenario
        InitializeDemoScenario();
    }

    private void OnCellClicked(GridPosition position)
    {
        SelectedPosition = position;
    }

    private void OnCellHovered(GridPosition position)
    {
        HoveredPosition = position;
    }

    private void UpdateHighlightedPositions()
    {
        _highlightedPositions.Clear();

        if (SelectedPosition.HasValue)
        {
            // Demo: Highlight adjacent positions as valid move targets
            var pos = SelectedPosition.Value;

            // Same zone, adjacent columns
            for (int colOffset = -1; colOffset <= 1; colOffset++)
            {
                var targetCol = pos.Column + colOffset;
                if (targetCol >= 0 && targetCol < Columns && colOffset != 0)
                {
                    _highlightedPositions.Add(new GridPosition(pos.Zone, pos.Row, targetCol));
                }
            }

            // Same column, opposite row in same zone
            var oppositeRow = pos.Row == Row.Front ? Row.Back : Row.Front;
            _highlightedPositions.Add(new GridPosition(pos.Zone, oppositeRow, pos.Column));

            // Front row can attack enemy front row at same column
            if (pos.Row == Row.Front)
            {
                var oppositeZone = pos.Zone == Zone.Player ? Zone.Enemy : Zone.Player;
                _highlightedPositions.Add(new GridPosition(oppositeZone, Row.Front, pos.Column));
            }
        }

        this.RaisePropertyChanged(nameof(HighlightedPositions));
    }

    private void UpdateStatusMessage()
    {
        if (SelectedPosition.HasValue)
        {
            var pos = SelectedPosition.Value;
            var hasUnit = _unitSprites.ContainsKey(pos);

            if (hasUnit)
            {
                StatusMessage = $"Selected: {pos} - Green cells show valid actions";
            }
            else
            {
                StatusMessage = $"Selected empty cell: {pos}";
            }
        }
        else
        {
            StatusMessage = "Select a unit to begin combat";
        }
    }

    private void InitializeDemoScenario()
    {
        // Create demo combat scenario with player and enemy units
        var demoSprites = new Dictionary<GridPosition, string>
        {
            // Player units
            { new GridPosition(Zone.Player, Row.Front, 0), "warrior" },
            { new GridPosition(Zone.Player, Row.Front, 2), "warrior" },
            { new GridPosition(Zone.Player, Row.Back, 1), "blessed" },

            // Enemy units
            { new GridPosition(Zone.Enemy, Row.Front, 0), "goblin" },
            { new GridPosition(Zone.Enemy, Row.Front, 1), "goblin" },
            { new GridPosition(Zone.Enemy, Row.Back, 2), "goblin" }
        };

        // Load sprite bitmaps
        _unitSprites.Clear();
        foreach (var (pos, spriteName) in demoSprites)
        {
            var bitmap = _spriteService.GetSpriteBitmap(spriteName, scale: 3);
            if (bitmap != null)
            {
                _unitSprites[pos] = bitmap;
            }
        }

        this.RaisePropertyChanged(nameof(UnitSprites));
    }
}

