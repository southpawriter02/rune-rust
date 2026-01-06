namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Data transfer object representing a monster for display.
/// </summary>
/// <param name="Id">The unique identifier of the monster.</param>
/// <param name="Name">The monster's display name.</param>
/// <param name="Description">A description of the monster.</param>
/// <param name="Health">The monster's current health points.</param>
/// <param name="MaxHealth">The monster's maximum health points.</param>
/// <param name="IsAlive">True if the monster is still alive; otherwise, false.</param>
public record MonsterDto(
    Guid Id,
    string Name,
    string Description,
    int Health,
    int MaxHealth,
    bool IsAlive
);
