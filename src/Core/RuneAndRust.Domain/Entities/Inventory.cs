namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Represents a container for items with a fixed capacity.
/// </summary>
/// <remarks>
/// The Inventory class manages a collection of items that a player can carry.
/// It enforces a maximum capacity and provides methods for adding, removing,
/// and querying items.
/// </remarks>
public class Inventory
{
    /// <summary>
    /// The internal list of items in the inventory.
    /// </summary>
    private readonly List<Item> _items = [];

    /// <summary>
    /// Gets the maximum number of items this inventory can hold.
    /// </summary>
    public int Capacity { get; private set; }

    /// <summary>
    /// Gets a read-only list of items in the inventory.
    /// </summary>
    public IReadOnlyList<Item> Items => _items.AsReadOnly();

    /// <summary>
    /// Gets the current number of items in the inventory.
    /// </summary>
    public int Count => _items.Count;

    /// <summary>
    /// Gets a value indicating whether the inventory is at capacity.
    /// </summary>
    public bool IsFull => _items.Count >= Capacity;

    /// <summary>
    /// Gets a value indicating whether the inventory contains no items.
    /// </summary>
    public bool IsEmpty => _items.Count == 0;

    /// <summary>
    /// Creates a new inventory with the specified capacity.
    /// </summary>
    /// <param name="capacity">The maximum number of items (default is 20).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when capacity is less than 1.</exception>
    public Inventory(int capacity = 20)
    {
        if (capacity < 1)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be at least 1");
        Capacity = capacity;
    }

    /// <summary>
    /// Attempts to add an item to the inventory.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <returns><c>true</c> if the item was added; <c>false</c> if the inventory is full.</returns>
    /// <exception cref="ArgumentNullException">Thrown when item is null.</exception>
    public bool TryAdd(Item item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        if (IsFull)
            return false;

        _items.Add(item);
        return true;
    }

    /// <summary>
    /// Removes an item from the inventory.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns><c>true</c> if the item was found and removed; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when item is null.</exception>
    public bool Remove(Item item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        return _items.Remove(item);
    }

    /// <summary>
    /// Removes an item from the inventory by its ID.
    /// </summary>
    /// <param name="itemId">The ID of the item to remove.</param>
    /// <returns><c>true</c> if the item was found and removed; otherwise, <c>false</c>.</returns>
    public bool RemoveById(Guid itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        return item != null && _items.Remove(item);
    }

    /// <summary>
    /// Checks if the inventory contains the specified item.
    /// </summary>
    /// <param name="item">The item to check for.</param>
    /// <returns><c>true</c> if the item is in the inventory; otherwise, <c>false</c>.</returns>
    public bool Contains(Item item) => _items.Contains(item);

    /// <summary>
    /// Checks if the inventory contains an item with the specified ID.
    /// </summary>
    /// <param name="itemId">The ID of the item to check for.</param>
    /// <returns><c>true</c> if an item with the ID is in the inventory; otherwise, <c>false</c>.</returns>
    public bool ContainsById(Guid itemId) => _items.Any(i => i.Id == itemId);

    /// <summary>
    /// Gets an item from the inventory by its ID.
    /// </summary>
    /// <param name="itemId">The ID of the item to retrieve.</param>
    /// <returns>The item if found; otherwise, <c>null</c>.</returns>
    public Item? GetById(Guid itemId) => _items.FirstOrDefault(i => i.Id == itemId);

    /// <summary>
    /// Gets an item from the inventory by its name (case-insensitive).
    /// </summary>
    /// <param name="name">The name of the item to retrieve.</param>
    /// <returns>The item if found; otherwise, <c>null</c>.</returns>
    public Item? GetByName(string name) =>
        _items.FirstOrDefault(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Removes all items from the inventory.
    /// </summary>
    public void Clear() => _items.Clear();
}
