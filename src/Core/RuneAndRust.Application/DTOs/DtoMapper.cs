using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Provides extension methods for converting domain entities to DTOs.
/// </summary>
/// <remarks>
/// The DtoMapper class centralizes all entity-to-DTO conversion logic,
/// ensuring consistent mapping behavior across the application layer.
/// </remarks>
public static class DtoMapper
{
    /// <summary>
    /// Converts a Player entity to a PlayerDto.
    /// </summary>
    /// <param name="player">The player entity to convert.</param>
    /// <returns>A new <see cref="PlayerDto"/> containing the player's state.</returns>
    /// <exception cref="ArgumentNullException">Thrown when player is null.</exception>
    public static PlayerDto ToDto(this Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        return new PlayerDto(
            player.Id,
            player.Name,
            player.Health,
            player.Stats.MaxHealth,
            player.Stats.Attack,
            player.Stats.Defense,
            player.Inventory.Count,
            player.Inventory.Capacity
        );
    }

    /// <summary>
    /// Converts an Item entity to an ItemDto.
    /// </summary>
    /// <param name="item">The item entity to convert.</param>
    /// <returns>A new <see cref="ItemDto"/> containing the item's state.</returns>
    /// <exception cref="ArgumentNullException">Thrown when item is null.</exception>
    public static ItemDto ToDto(this Item item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return new ItemDto(
            item.Id,
            item.Name,
            item.Description,
            item.Type,
            item.Value
        );
    }

    /// <summary>
    /// Converts a Monster entity to a MonsterDto.
    /// </summary>
    /// <param name="monster">The monster entity to convert.</param>
    /// <returns>A new <see cref="MonsterDto"/> containing the monster's state.</returns>
    /// <exception cref="ArgumentNullException">Thrown when monster is null.</exception>
    public static MonsterDto ToDto(this Monster monster)
    {
        ArgumentNullException.ThrowIfNull(monster);

        return new MonsterDto(
            monster.Id,
            monster.Name,
            monster.Description,
            monster.Health,
            monster.MaxHealth,
            monster.IsAlive
        );
    }

    /// <summary>
    /// Converts a Room entity to a RoomDto, including nested items and monsters.
    /// </summary>
    /// <param name="room">The room entity to convert.</param>
    /// <returns>A new <see cref="RoomDto"/> containing the room's state.</returns>
    /// <exception cref="ArgumentNullException">Thrown when room is null.</exception>
    public static RoomDto ToDto(this Room room)
    {
        ArgumentNullException.ThrowIfNull(room);

        return new RoomDto(
            room.Id,
            room.Name,
            room.Description,
            room.Exits.Keys.ToList(),
            room.Items.Select(i => i.ToDto()).ToList(),
            room.Monsters.Select(m => m.ToDto()).ToList()
        );
    }

    /// <summary>
    /// Converts an Inventory entity to an InventoryDto, including all contained items.
    /// </summary>
    /// <param name="inventory">The inventory entity to convert.</param>
    /// <returns>A new <see cref="InventoryDto"/> containing the inventory's state.</returns>
    /// <exception cref="ArgumentNullException">Thrown when inventory is null.</exception>
    public static InventoryDto ToDto(this Inventory inventory)
    {
        ArgumentNullException.ThrowIfNull(inventory);

        return new InventoryDto(
            inventory.Items.Select(i => i.ToDto()).ToList(),
            inventory.Capacity,
            inventory.Count,
            inventory.IsFull
        );
    }

    /// <summary>
    /// Converts a GameSession entity to a GameStateDto, including player and room state.
    /// </summary>
    /// <param name="session">The game session entity to convert.</param>
    /// <returns>A new <see cref="GameStateDto"/> containing the session's state.</returns>
    /// <exception cref="ArgumentNullException">Thrown when session is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the current room cannot be found.</exception>
    public static GameStateDto ToDto(this GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        var currentRoom = session.CurrentRoom
            ?? throw new InvalidOperationException("Current room not found");

        return new GameStateDto(
            session.Id,
            session.Player.ToDto(),
            currentRoom.ToDto(),
            session.State,
            session.LastPlayedAt
        );
    }
}
