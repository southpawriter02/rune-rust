using CommunityToolkit.Mvvm.ComponentModel;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Presentation.Shared.ViewModels;

/// <summary>
/// ViewModel representing a room in the dungeon for data binding in views.
/// </summary>
/// <remarks>
/// RoomViewModel provides observable properties for room contents (items, monsters)
/// and computed properties for display. It can be updated from RoomDto objects
/// received from the application layer.
/// </remarks>
public partial class RoomViewModel : ObservableObject
{
    /// <summary>
    /// The room's display name.
    /// </summary>
    [ObservableProperty]
    private string _name = string.Empty;

    /// <summary>
    /// The narrative description of the room.
    /// </summary>
    [ObservableProperty]
    private string _description = string.Empty;

    /// <summary>
    /// The available exit directions from this room.
    /// </summary>
    [ObservableProperty]
    private IReadOnlyList<Direction> _exits = [];

    /// <summary>
    /// The items present in this room.
    /// </summary>
    [ObservableProperty]
    private IReadOnlyList<ItemDto> _items = [];

    /// <summary>
    /// The monsters present in this room.
    /// </summary>
    [ObservableProperty]
    private IReadOnlyList<MonsterDto> _monsters = [];

    /// <summary>
    /// Gets a value indicating whether the room contains any items.
    /// </summary>
    public bool HasItems => Items.Count > 0;

    /// <summary>
    /// Gets a value indicating whether the room contains any alive monsters.
    /// </summary>
    public bool HasMonsters => Monsters.Any(m => m.IsAlive);

    /// <summary>
    /// Gets a value indicating whether the room has any exits.
    /// </summary>
    public bool HasExits => Exits.Count > 0;

    /// <summary>
    /// Gets a formatted string listing available exits, or "None" if none exist.
    /// </summary>
    public string ExitsDisplay => Exits.Count > 0
        ? string.Join(", ", Exits.Select(e => e.ToString()))
        : "None";

    /// <summary>
    /// Updates this view model from a room DTO.
    /// </summary>
    /// <param name="dto">The DTO containing the updated room state.</param>
    public void UpdateFrom(RoomDto dto)
    {
        Name = dto.Name;
        Description = dto.Description;
        Exits = dto.Exits;
        Items = dto.Items;
        Monsters = dto.Monsters;

        OnPropertyChanged(nameof(HasItems));
        OnPropertyChanged(nameof(HasMonsters));
        OnPropertyChanged(nameof(HasExits));
        OnPropertyChanged(nameof(ExitsDisplay));
    }
}
