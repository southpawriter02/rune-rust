using CommunityToolkit.Mvvm.ComponentModel;
using RuneAndRust.Application.DTOs;

namespace RuneAndRust.Presentation.Shared.ViewModels;

public partial class PlayerViewModel : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private int _health;

    [ObservableProperty]
    private int _maxHealth;

    [ObservableProperty]
    private int _attack;

    [ObservableProperty]
    private int _defense;

    [ObservableProperty]
    private int _inventoryCount;

    [ObservableProperty]
    private int _inventoryCapacity;

    public string HealthDisplay => $"{Health}/{MaxHealth}";
    public double HealthPercentage => MaxHealth > 0 ? (double)Health / MaxHealth : 0;
    public string StatsDisplay => $"ATK: {Attack} | DEF: {Defense}";
    public string InventoryDisplay => $"{InventoryCount}/{InventoryCapacity}";

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
