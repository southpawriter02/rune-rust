using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

public class Room : IEntity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Position Position { get; private set; }

    private readonly Dictionary<Direction, Guid> _exits = [];
    private readonly List<Item> _items = [];
    private readonly List<Monster> _monsters = [];

    public IReadOnlyDictionary<Direction, Guid> Exits => _exits.AsReadOnly();
    public IReadOnlyList<Item> Items => _items.AsReadOnly();
    public IReadOnlyList<Monster> Monsters => _monsters.AsReadOnly();

    public bool HasMonsters => _monsters.Any(m => m.IsAlive);
    public bool HasItems => _items.Count > 0;

    private Room() { } // For EF Core

    public Room(string name, string description, Position position)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Position = position;
    }

    public void AddExit(Direction direction, Guid roomId)
    {
        _exits[direction] = roomId;
    }

    public bool HasExit(Direction direction) => _exits.ContainsKey(direction);

    public Guid? GetExit(Direction direction) =>
        _exits.TryGetValue(direction, out var roomId) ? roomId : null;

    public void AddItem(Item item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
        _items.Add(item);
    }

    public bool RemoveItem(Item item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
        return _items.Remove(item);
    }

    public Item? GetItemByName(string name) =>
        _items.FirstOrDefault(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    public void AddMonster(Monster monster)
    {
        if (monster == null)
            throw new ArgumentNullException(nameof(monster));
        _monsters.Add(monster);
    }

    public IEnumerable<Monster> GetAliveMonsters() => _monsters.Where(m => m.IsAlive);

    public string GetExitsDescription()
    {
        if (_exits.Count == 0)
            return "There are no visible exits.";

        var directions = _exits.Keys.Select(d => d.ToString().ToLower());
        return $"Exits: {string.Join(", ", directions)}";
    }

    public override string ToString() => Name;
}
