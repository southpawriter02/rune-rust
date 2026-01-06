using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.DTOs;

public static class DtoMapper
{
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
