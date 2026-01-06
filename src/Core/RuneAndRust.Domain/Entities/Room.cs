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
    public Biome Biome { get; private set; }

    private readonly Dictionary<Direction, Guid> _exits = [];
    private readonly List<Item> _items = [];
    private readonly List<Monster> _monsters = [];
    private readonly List<HiddenElement> _hiddenElements = [];

    public IReadOnlyDictionary<Direction, Guid> Exits => _exits.AsReadOnly();
    public IReadOnlyList<Item> Items => _items.AsReadOnly();
    public IReadOnlyList<Monster> Monsters => _monsters.AsReadOnly();
    public IReadOnlyList<HiddenElement> HiddenElements => _hiddenElements.AsReadOnly();

    public bool HasMonsters => _monsters.Any(m => m.IsAlive);
    public bool HasItems => _items.Count > 0;
    public bool HasUnrevealedElements => _hiddenElements.Any(h => !h.IsRevealed);
    public bool HasRevealedElements => _hiddenElements.Any(h => h.IsRevealed);

    private Room()
    {
        Name = null!;
        Description = null!;
    } // For EF Core

    public Room(string name, string description, Position position, Biome biome = Biome.Citadel)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Position = position;
        Biome = biome;
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

    public void AddHiddenElement(HiddenElement element)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));
        _hiddenElements.Add(element);
    }

    public IEnumerable<HiddenElement> GetUnrevealedElements() =>
        _hiddenElements.Where(h => !h.IsRevealed);

    public IEnumerable<HiddenElement> GetRevealedElements() =>
        _hiddenElements.Where(h => h.IsRevealed);

    /// <summary>
    /// Checks passive perception against all unrevealed hidden elements
    /// and reveals any that the player can detect.
    /// </summary>
    /// <param name="passivePerception">The player's passive perception value.</param>
    /// <returns>List of newly revealed elements.</returns>
    public IReadOnlyList<HiddenElement> CheckPassivePerception(int passivePerception)
    {
        var revealed = new List<HiddenElement>();
        foreach (var element in _hiddenElements.Where(h => !h.IsRevealed))
        {
            if (element.CanBeDetectedBy(passivePerception))
            {
                element.Reveal();
                revealed.Add(element);
            }
        }
        return revealed;
    }

    /// <summary>
    /// Performs an active search with a WITS check result to reveal hidden elements.
    /// </summary>
    /// <param name="searchCheckResult">The result of the active WITS check.</param>
    /// <returns>List of newly revealed elements.</returns>
    public IReadOnlyList<HiddenElement> PerformActiveSearch(int searchCheckResult)
    {
        // Active search uses the full check result, not passive perception
        return CheckPassivePerception(searchCheckResult);
    }

    public string GetExitsDescription()
    {
        if (_exits.Count == 0)
            return "There are no visible exits.";

        var directions = _exits.Keys.Select(d => d.ToString().ToLower());
        return $"Exits: {string.Join(", ", directions)}";
    }

    public override string ToString() => Name;
}
