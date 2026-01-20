namespace RuneAndRust.Presentation.Gui.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Presentation.Gui.Enums;
using RuneAndRust.Presentation.Gui.Models;
using RuneAndRust.Presentation.Gui.Services;
using Serilog;
using System.Collections.ObjectModel;

/// <summary>
/// View model for the combat grid panel.
/// </summary>
/// <remarks>
/// Displays the tactical battlefield grid with cells, terrain, and entity tokens.
/// The grid appears when combat begins and hides when combat ends.
/// </remarks>
public partial class CombatGridPanelViewModel : ViewModelBase
{
    private const int CellSize = 40;

    /// <summary>
    /// Gets or sets the flattened cell collection for display.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<GridCellViewModel> _flatCells = [];

    /// <summary>
    /// Gets or sets the entity tokens on the grid.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<EntityTokenViewModel> _tokens = [];

    /// <summary>
    /// Gets or sets the grid width in cells.
    /// </summary>
    [ObservableProperty]
    private int _gridWidth = 8;

    /// <summary>
    /// Gets or sets the grid height in cells.
    /// </summary>
    [ObservableProperty]
    private int _gridHeight = 8;

    /// <summary>
    /// Gets or sets whether combat is active.
    /// </summary>
    [ObservableProperty]
    private bool _isInCombat;

    /// <summary>
    /// Gets or sets whether targeting is currently active.
    /// </summary>
    [ObservableProperty]
    private bool _isTargeting;

    /// <summary>
    /// Gets or sets the targeting hint message.
    /// </summary>
    [ObservableProperty]
    private string _targetingHint = string.Empty;

    /// <summary>
    /// Gets the interaction service for grid targeting.
    /// </summary>
    public IGridInteractionService? InteractionService { get; private set; }

    /// <summary>
    /// Row labels for coordinate display (A-H).
    /// </summary>
    public IReadOnlyList<string> RowLabels { get; private set; } = Array.Empty<string>();

    /// <summary>
    /// Column labels for coordinate display (1-8).
    /// </summary>
    public IReadOnlyList<int> ColumnLabels { get; private set; } = Array.Empty<int>();

    /// <summary>
    /// Legend items explaining grid symbols.
    /// </summary>
    public IReadOnlyList<GridLegendItem> LegendItems { get; } =
    [
        new("@", "You", "#32CD32"),
        new("░", "Floor", "#696969"),
        new("█", "Wall", "#2F4F4F"),
        new("~", "Water", "#00008B"),
        new("☼", "Hazard", "#8B0000"),
        new("▓", "Cover", "#8B4513")
    ];

    /// <summary>
    /// Creates a new CombatGridPanelViewModel (design-time).
    /// </summary>
    public CombatGridPanelViewModel()
    {
        IsInCombat = true;
        InitializeSampleGrid();
        Log.Debug("CombatGridPanelViewModel created (design-time)");
    }

    /// <summary>
    /// Initializes the grid from a CombatGrid.
    /// </summary>
    /// <param name="grid">The combat grid to display.</param>
    public void Initialize(CombatGrid grid)
    {
        GridWidth = grid.Width;
        GridHeight = grid.Height;

        // Generate coordinate labels
        RowLabels = Enumerable.Range(0, GridHeight)
            .Select(i => ((char)('A' + i)).ToString())
            .ToList();
        ColumnLabels = Enumerable.Range(1, GridWidth).ToList();

        OnPropertyChanged(nameof(RowLabels));
        OnPropertyChanged(nameof(ColumnLabels));

        // Create cell view models
        FlatCells.Clear();
        for (int row = 0; row < GridHeight; row++)
        {
            for (int col = 0; col < GridWidth; col++)
            {
                var position = new GridPosition(col, row);
                var cell = grid.GetCell(position);
                if (cell is not null)
                {
                    var cellVm = new GridCellViewModel(position, cell);
                    FlatCells.Add(cellVm);
                }
            }
        }

        Log.Information("Combat grid initialized: {Width}x{Height}", GridWidth, GridHeight);
    }

    /// <summary>
    /// Refreshes the grid cells from a CombatGrid.
    /// </summary>
    /// <param name="grid">The combat grid to refresh from.</param>
    public void RefreshGrid(CombatGrid grid)
    {
        foreach (var cellVm in FlatCells)
        {
            var cell = grid.GetCell(cellVm.Position);
            if (cell is not null)
            {
                cellVm.UpdateFromCell(cell);
            }
        }
        Log.Debug("Combat grid refreshed");
    }

    /// <summary>
    /// Refreshes the entity tokens.
    /// </summary>
    /// <param name="combatants">The combatants to display.</param>
    /// <param name="grid">The grid for positioning.</param>
    /// <param name="currentTurnId">The current turn combatant ID.</param>
    public void RefreshTokens(IEnumerable<Combatant> combatants, CombatGrid grid, Guid? currentTurnId)
    {
        Tokens.Clear();

        foreach (var combatant in combatants)
        {
            // Find the combatant's position from the grid
            var position = FindCombatantPosition(combatant, grid);
            if (position is null)
                continue;

            var token = new EntityTokenViewModel(combatant, position.Value.X, position.Value.Y, CellSize)
            {
                IsCurrentTurn = combatant.Id == currentTurnId
            };
            Tokens.Add(token);
        }

        Log.Debug("Tokens refreshed: {Count} combatants", Tokens.Count);
    }

    private static GridPosition? FindCombatantPosition(Combatant combatant, CombatGrid grid)
    {
        // Search the grid for this combatant
        for (int y = 0; y < grid.Height; y++)
        {
            for (int x = 0; x < grid.Width; x++)
            {
                var pos = new GridPosition(x, y);
                var cell = grid.GetCell(pos);
                if (cell?.OccupantId == combatant.Id)
                {
                    return pos;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Updates the current turn indicator.
    /// </summary>
    /// <param name="currentTurnId">The current turn combatant ID.</param>
    public void UpdateCurrentTurn(Guid currentTurnId)
    {
        foreach (var token in Tokens)
        {
            token.IsCurrentTurn = token.Combatant?.Id == currentTurnId;
        }
        Log.Debug("Current turn updated");
    }

    /// <summary>
    /// Sets combat state.
    /// </summary>
    /// <param name="inCombat">Whether combat is active.</param>
    public void SetCombatState(bool inCombat)
    {
        IsInCombat = inCombat;

        if (!inCombat)
        {
            FlatCells.Clear();
            Tokens.Clear();
            InteractionService?.CancelTargeting();
            IsTargeting = false;
            TargetingHint = string.Empty;
            Log.Information("Combat ended, grid cleared");
        }
    }

    /// <summary>
    /// Sets the interaction service and subscribes to events.
    /// </summary>
    /// <param name="service">The interaction service.</param>
    public void SetInteractionService(IGridInteractionService service)
    {
        // Unsubscribe from previous service
        if (InteractionService is not null)
        {
            InteractionService.OnHighlightsChanged -= RefreshHighlights;
            InteractionService.OnTargetingComplete -= HandleTargetingComplete;
        }

        InteractionService = service;
        InteractionService.OnHighlightsChanged += RefreshHighlights;
        InteractionService.OnTargetingComplete += HandleTargetingComplete;
        Log.Debug("Interaction service set");
    }

    /// <summary>
    /// Refreshes cell highlights from the interaction service.
    /// </summary>
    public void RefreshHighlights()
    {
        if (InteractionService is null)
            return;

        // Clear all highlights
        foreach (var cell in FlatCells)
        {
            cell.IsHighlighted = false;
            cell.HighlightType = HighlightType.Movement;
        }

        // Apply current highlights
        var highlights = InteractionService.GetHighlightedCells();
        foreach (var highlight in highlights)
        {
            var cell = GetCellAt(highlight.Position);
            if (cell is not null)
            {
                cell.IsHighlighted = true;
                cell.HighlightType = highlight.Type;
            }
        }

        // Update targeting state
        IsTargeting = InteractionService.IsTargeting;
        TargetingHint = InteractionService.CurrentMode switch
        {
            GridInteractionMode.Movement => "Click to move • Right-click to cancel",
            GridInteractionMode.Attack => "Click enemy to attack • Right-click to cancel",
            GridInteractionMode.Ability => "Click target • Right-click to cancel",
            _ => string.Empty
        };
    }

    /// <summary>
    /// Gets the cell ViewModel at the specified position.
    /// </summary>
    /// <param name="position">The grid position.</param>
    /// <returns>The cell ViewModel, or null if not found.</returns>
    public GridCellViewModel? GetCellAt(GridPosition position)
    {
        var index = position.Y * GridWidth + position.X;
        return index >= 0 && index < FlatCells.Count ? FlatCells[index] : null;
    }

    private void HandleTargetingComplete(TargetingResult result)
    {
        Log.Debug("Targeting complete: {Mode} at {Position}, Success: {Success}",
            result.Mode, result.Target, result.Success);
        RefreshHighlights();
    }

    private void InitializeSampleGrid()
    {
        GridWidth = 8;
        GridHeight = 8;

        RowLabels = ["A", "B", "C", "D", "E", "F", "G", "H"];
        ColumnLabels = [1, 2, 3, 4, 5, 6, 7, 8];

        for (int row = 0; row < GridHeight; row++)
        {
            for (int col = 0; col < GridWidth; col++)
            {
                var position = new GridPosition(col, row);
                var cellType = GetSampleCellType(col, row);
                var cellVm = new GridCellViewModel(position, cellType);
                FlatCells.Add(cellVm);
            }
        }

        // Sample tokens
        Tokens.Add(new EntityTokenViewModel("Hero", "@", "#32CD32", 4, 3, CellSize) { IsCurrentTurn = true });
        Tokens.Add(new EntityTokenViewModel("Skeleton", "S", "#FF4500", 2, 2, CellSize));
        Tokens.Add(new EntityTokenViewModel("Goblin", "G", "#FF4500", 5, 5, CellSize));
    }

    private static GridCellType GetSampleCellType(int col, int row)
    {
        // Walls at row 0, columns 2-3
        if (row == 0 && col is 2 or 3)
            return GridCellType.Wall;

        // Water at row 4, columns 1-3 and row 5, column 2
        if ((row == 4 && col is >= 1 and <= 3) || (row == 5 && col == 2))
            return GridCellType.Water;

        return GridCellType.Floor;
    }
}

/// <summary>
/// Legend item for combat grid symbols.
/// </summary>
/// <param name="Symbol">The symbol character.</param>
/// <param name="Label">The label text.</param>
/// <param name="Color">The color hex code.</param>
public record GridLegendItem(string Symbol, string Label, string Color);
