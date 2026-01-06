using CommunityToolkit.Mvvm.ComponentModel;
using RuneAndRust.Application.DTOs;

namespace RuneAndRust.Presentation.Shared.ViewModels;

/// <summary>
/// ViewModel representing a player character for data binding in views.
/// </summary>
/// <remarks>
/// PlayerViewModel provides observable properties for player stats and
/// computed properties for display formatting. It can be updated from
/// PlayerDto objects received from the application layer.
/// </remarks>
public partial class PlayerViewModel : ObservableObject
{
    /// <summary>
    /// The player's display name.
    /// </summary>
    [ObservableProperty]
    private string _name = string.Empty;

    /// <summary>
    /// The player's current health points.
    /// </summary>
    [ObservableProperty]
    private int _health;

    /// <summary>
    /// The player's maximum health points.
    /// </summary>
    [ObservableProperty]
    private int _maxHealth;

    /// <summary>
    /// The player's attack stat.
    /// </summary>
    [ObservableProperty]
    private int _attack;

    /// <summary>
    /// The player's defense stat.
    /// </summary>
    [ObservableProperty]
    private int _defense;

    /// <summary>
    /// The number of items in the player's inventory.
    /// </summary>
    [ObservableProperty]
    private int _inventoryCount;

    /// <summary>
    /// The maximum capacity of the player's inventory.
    /// </summary>
    [ObservableProperty]
    private int _inventoryCapacity;

    /// <summary>
    /// Gets a formatted string showing current/max health.
    /// </summary>
    public string HealthDisplay => $"{Health}/{MaxHealth}";

    /// <summary>
    /// Gets the health as a percentage (0.0 to 1.0) for progress bars.
    /// </summary>
    public double HealthPercentage => MaxHealth > 0 ? (double)Health / MaxHealth : 0;

    /// <summary>
    /// Gets a formatted string showing attack and defense stats.
    /// </summary>
    public string StatsDisplay => $"ATK: {Attack} | DEF: {Defense}";

    /// <summary>
    /// Gets a formatted string showing inventory count/capacity.
    /// </summary>
    public string InventoryDisplay => $"{InventoryCount}/{InventoryCapacity}";

    /// <summary>
    /// Updates this view model from a player DTO.
    /// </summary>
    /// <param name="dto">The DTO containing the updated player state.</param>
    public void UpdateFrom(PlayerDto dto)
    {
        Name = dto.Name;
        Health = dto.Health;
        MaxHealth = dto.MaxHealth;
        Attack = dto.Attack;
        Defense = dto.Defense;
        InventoryCount = dto.InventoryCount;
        InventoryCapacity = dto.InventoryCapacity;

        OnPropertyChanged(nameof(HealthDisplay));
        OnPropertyChanged(nameof(HealthPercentage));
        OnPropertyChanged(nameof(StatsDisplay));
        OnPropertyChanged(nameof(InventoryDisplay));
    }
}
