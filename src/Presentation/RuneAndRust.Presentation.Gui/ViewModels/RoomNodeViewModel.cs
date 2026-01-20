namespace RuneAndRust.Presentation.Gui.ViewModels;

using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;

/// <summary>
/// ViewModel for a room node on the dungeon map.
/// </summary>
public partial class RoomNodeViewModel : ViewModelBase
{
    /// <summary>Gets the room ID.</summary>
    public Guid RoomId { get; }

    /// <summary>Gets the display name (room name or "???" if unexplored).</summary>
    [ObservableProperty] private string _displayName;

    /// <summary>Gets the X position on the map.</summary>
    [ObservableProperty] private double _x;

    /// <summary>Gets the Y position on the map.</summary>
    [ObservableProperty] private double _y;

    /// <summary>Gets whether the room is explored.</summary>
    [ObservableProperty] private bool _isExplored;

    /// <summary>Gets whether this is the current room.</summary>
    [ObservableProperty] private bool _isCurrent;

    /// <summary>Gets the room type icon.</summary>
    [ObservableProperty] private string _icon;

    /// <summary>Gets tooltip text for the room.</summary>
    public string TooltipText => IsExplored
        ? $"{DisplayName}\n{GetRoomTypeDescription()}"
        : "Unexplored";

    /// <summary>Gets the room type.</summary>
    public string RoomType { get; }

    /// <summary>Creates a room node ViewModel.</summary>
    public RoomNodeViewModel(Guid roomId, string name, string roomType, double x, double y, bool isExplored, bool isCurrent)
    {
        RoomId = roomId;
        RoomType = roomType;
        _displayName = isExplored ? name : "???";
        _x = x;
        _y = y;
        _isExplored = isExplored;
        _isCurrent = isCurrent;
        _icon = GetIconForType(roomType);
    }

    private static string GetIconForType(string roomType) => roomType switch
    {
        "Shop" => "💰",
        "Armory" => "🛡",
        "Boss" => "☠",
        "Exit" => "🚪",
        "Treasure" => "💎",
        "Danger" => "⚠",
        _ => ""
    };

    private string GetRoomTypeDescription() => RoomType switch
    {
        "Shop" => "Merchant available",
        "Armory" => "Equipment shop",
        "Boss" => "Dangerous enemy ahead",
        "Exit" => "Dungeon exit",
        "Treasure" => "Treasure room",
        "Danger" => "Hazardous area",
        _ => "Standard room"
    };
}
