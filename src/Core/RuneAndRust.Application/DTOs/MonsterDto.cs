namespace RuneAndRust.Application.DTOs;

public record MonsterDto(
    Guid Id,
    string Name,
    string Description,
    int Health,
    int MaxHealth,
    bool IsAlive
);
