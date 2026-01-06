namespace RuneAndRust.Domain.Entities;

public class Inventory
{
    private readonly List<Item> _items = [];
    public int Capacity { get; private set; }

    public IReadOnlyList<Item> Items => _items.AsReadOnly();
    public int Count => _items.Count;
    public bool IsFull => _items.Count >= Capacity;
    public bool IsEmpty => _items.Count == 0;

    public Inventory(int capacity = 20)
    {
        if (capacity < 1)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be at least 1");
        Capacity = capacity;
    }

    public bool TryAdd(Item item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        if (IsFull)
            return false;

        _items.Add(item);
        return true;
    }

    public bool Remove(Item item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        return _items.Remove(item);
    }

    public bool RemoveById(Guid itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        return item != null && _items.Remove(item);
    }

    public bool Contains(Item item) => _items.Contains(item);

    public bool ContainsById(Guid itemId) => _items.Any(i => i.Id == itemId);

    public Item? GetById(Guid itemId) => _items.FirstOrDefault(i => i.Id == itemId);

    public Item? GetByName(string name) =>
        _items.FirstOrDefault(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    public void Clear() => _items.Clear();
}
