using CommunityToolkit.Mvvm.ComponentModel;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Presentation.Shared.ViewModels;

public partial class RoomViewModel : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private IReadOnlyList<Direction> _exits = [];

    [ObservableProperty]
    private IReadOnlyList<ItemDto> _items = [];

    [ObservableProperty]
    private IReadOnlyList<MonsterDto> _monsters = [];

    public bool HasItems => Items.Count > 0;
    public bool HasMonsters => Monsters.Any(m => m.IsAlive);
    public bool HasExits => Exits.Count > 0;

    public string ExitsDisplay => Exits.Count > 0
        ? string.Join(", ", Exits.Select(e => e.ToString()))
        : "None";

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
