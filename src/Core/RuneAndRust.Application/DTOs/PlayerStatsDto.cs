namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Comprehensive player statistics for status display.
/// </summary>
public record PlayerStatsDto(
    string Name,
    int Health,
    int MaxHealth,
    int Attack,
    int Defense,
    int PositionX,
    int PositionY,
    string CurrentRoomName,
    int InventoryCount,
    int InventoryCapacity
)
{
    /// <summary>
    /// Gets the player's health as a percentage (0.0 to 1.0).
    /// </summary>
    public double HealthPercentage => MaxHealth > 0 ? (double)Health / MaxHealth : 0;
}
