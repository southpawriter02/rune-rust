namespace RuneAndRust.Presentation.Gui.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RuneAndRust.Domain.Enums;
using Serilog;
using System.Collections.ObjectModel;

/// <summary>
/// View model for the room display panel.
/// </summary>
/// <remarks>
/// Manages the display of room information including:
/// - Room name and description
/// - ASCII art with legend
/// - Available exits
/// - Visible monsters and items
/// </remarks>
public partial class RoomDisplayViewModel : ViewModelBase
{
    /// <summary>
    /// Gets or sets the room name.
    /// </summary>
    [ObservableProperty]
    private string _roomName = "Unknown Location";

    /// <summary>
    /// Gets or sets the room description.
    /// </summary>
    [ObservableProperty]
    private string _roomDescription = "You are in an unknown location.";

    /// <summary>
    /// Gets or sets the ASCII art lines.
    /// </summary>
    [ObservableProperty]
    private IReadOnlyList<string>? _asciiArt;

    /// <summary>
    /// Gets or sets whether ASCII art is available.
    /// </summary>
    [ObservableProperty]
    private bool _hasAsciiArt;

    /// <summary>
    /// Gets or sets the art legend items.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<LegendItem> _artLegendItems = [];

    /// <summary>
    /// Gets or sets whether an art legend is available.
    /// </summary>
    [ObservableProperty]
    private bool _hasArtLegend;

    /// <summary>
    /// Gets or sets the available exits.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ExitViewModel> _exits = [];

    /// <summary>
    /// Gets or sets the visible monsters.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<EntityViewModel> _visibleMonsters = [];

    /// <summary>
    /// Gets or sets the visible items.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ItemViewModel> _visibleItems = [];

    /// <summary>
    /// Gets or sets whether there are visible entities.
    /// </summary>
    [ObservableProperty]
    private bool _hasVisibleEntities;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoomDisplayViewModel"/> class
    /// with design-time sample data.
    /// </summary>
    public RoomDisplayViewModel()
    {
        // Design-time sample data
        RoomName = "Dark Cave";
        RoomDescription = "A dark, damp cave. Water drips from stalactites overhead. " +
                          "The air smells of mold and decay. A skeletal figure guards " +
                          "the passage to the north, its empty eye sockets watching your every move.";
        
        HasAsciiArt = true;
        AsciiArt =
        [
            "╔═══════════════════════════════════════╗",
            "║   ~~~   ▲   ~~~                       ║",
            "║   ~~~  ▲▲▲  ~~~    ☠                  ║",
            "║       ▲▲▲▲▲                           ║",
            "║  ░░     cave     ░░                   ║",
            "╚═══════════════════════════════════════╝"
        ];
        
        ArtLegendItems =
        [
            new LegendItem("~", "water"),
            new LegendItem("▲", "stalagmite"),
            new LegendItem("☠", "skeleton"),
            new LegendItem("░", "shadow")
        ];
        HasArtLegend = true;

        Exits =
        [
            new ExitViewModel(Direction.North, false),
            new ExitViewModel(Direction.East, false)
        ];

        VisibleMonsters =
        [
            new EntityViewModel("Skeleton", "☠", true, "A skeletal warrior")
        ];

        VisibleItems =
        [
            new ItemViewModel("Rusty Key", "🔑", false, "An old rusty key")
        ];

        HasVisibleEntities = true;
        
        Log.Debug("RoomDisplayViewModel initialized with sample data");
    }

    /// <summary>
    /// Moves the player in the specified direction.
    /// </summary>
    /// <param name="direction">The direction to move.</param>
    [RelayCommand]
    private void MoveToExit(Direction direction)
    {
        Log.Information("Player moving {Direction}", direction);
        // In future versions, this will call IGameSessionService.ExecuteCommand
        // For now, log the action
    }

    /// <summary>
    /// Updates the room display with new room data.
    /// </summary>
    /// <param name="roomName">The room name.</param>
    /// <param name="roomDescription">The room description.</param>
    /// <param name="asciiArt">Optional ASCII art lines.</param>
    /// <param name="legend">Optional art legend.</param>
    public void UpdateRoom(
        string roomName,
        string roomDescription,
        IReadOnlyList<string>? asciiArt = null,
        IReadOnlyDictionary<char, string>? legend = null)
    {
        Log.Debug("Room updated to {RoomName}", roomName);
        
        RoomName = roomName;
        RoomDescription = roomDescription;
        
        AsciiArt = asciiArt;
        HasAsciiArt = asciiArt is not null && asciiArt.Count > 0;
        
        ArtLegendItems.Clear();
        if (legend is not null)
        {
            foreach (var (symbol, description) in legend)
            {
                ArtLegendItems.Add(new LegendItem(symbol.ToString(), description));
            }
        }
        HasArtLegend = ArtLegendItems.Count > 0;
    }

    /// <summary>
    /// Updates the available exits.
    /// </summary>
    /// <param name="exits">The exit view models.</param>
    public void UpdateExits(IEnumerable<ExitViewModel> exits)
    {
        Exits.Clear();
        foreach (var exit in exits)
        {
            Exits.Add(exit);
        }
        Log.Debug("Exits updated: {Count} exits", Exits.Count);
    }

    /// <summary>
    /// Updates the visible entities.
    /// </summary>
    /// <param name="monsters">The visible monsters.</param>
    /// <param name="items">The visible items.</param>
    public void UpdateVisibleEntities(
        IEnumerable<EntityViewModel> monsters,
        IEnumerable<ItemViewModel> items)
    {
        VisibleMonsters.Clear();
        foreach (var monster in monsters)
        {
            VisibleMonsters.Add(monster);
        }

        VisibleItems.Clear();
        foreach (var item in items)
        {
            VisibleItems.Add(item);
        }

        HasVisibleEntities = VisibleMonsters.Count > 0 || VisibleItems.Count > 0;
        Log.Debug("Entities updated: {Monsters} monsters, {Items} items", 
            VisibleMonsters.Count, VisibleItems.Count);
    }

    /// <summary>
    /// Clears all displayed room information.
    /// </summary>
    public void Clear()
    {
        RoomName = "Unknown Location";
        RoomDescription = "You are in an unknown location.";
        AsciiArt = null;
        HasAsciiArt = false;
        ArtLegendItems.Clear();
        HasArtLegend = false;
        Exits.Clear();
        VisibleMonsters.Clear();
        VisibleItems.Clear();
        HasVisibleEntities = false;
        Log.Debug("Room display cleared");
    }
}

/// <summary>
/// Legend item for ASCII art symbols.
/// </summary>
/// <param name="Symbol">The symbol character.</param>
/// <param name="Description">The description text.</param>
public record LegendItem(string Symbol, string Description);
