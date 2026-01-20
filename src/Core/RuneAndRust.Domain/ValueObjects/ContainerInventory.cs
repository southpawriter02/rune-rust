using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the inventory contents of a container object.
/// </summary>
/// <remarks>
/// <para>
/// ContainerInventory manages item storage for interactive objects like chests,
/// crates, and barrels. It enforces capacity limits and provides methods for
/// adding, removing, and searching for items.
/// </para>
/// <para>
/// This class is mutable to allow in-place item management. Use <see cref="Create"/>
/// to instantiate with a specified capacity.
/// </para>
/// </remarks>
public class ContainerInventory
{
    private readonly List<Item> _items = new();

    /// <summary>
    /// Gets the items stored in this container.
    /// </summary>
    public IReadOnlyList<Item> Items => _items;

    /// <summary>
    /// Gets the maximum number of items this container can hold.
    /// </summary>
    public int Capacity { get; private set; }

    /// <summary>
    /// Gets the current number of items in the container.
    /// </summary>
    public int Count => _items.Count;

    /// <summary>
    /// Gets whether the container is empty.
    /// </summary>
    public bool IsEmpty => _items.Count == 0;

    /// <summary>
    /// Gets whether the container is full.
    /// </summary>
    public bool IsFull => _items.Count >= Capacity;

    /// <summary>
    /// Gets the remaining capacity.
    /// </summary>
    public int RemainingCapacity => Math.Max(0, Capacity - _items.Count);

    /// <summary>
    /// Private constructor for factory pattern.
    /// </summary>
    private ContainerInventory() { }

    /// <summary>
    /// Creates a new container inventory with the specified capacity.
    /// </summary>
    /// <param name="capacity">The maximum number of items.</param>
    /// <returns>A new ContainerInventory.</returns>
    /// <exception cref="ArgumentException">Thrown when capacity is not positive.</exception>
    public static ContainerInventory Create(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentException("Capacity must be positive.", nameof(capacity));

        return new ContainerInventory { Capacity = capacity };
    }

    /// <summary>
    /// Creates an empty container with default capacity.
    /// </summary>
    /// <param name="capacity">The capacity (default 10).</param>
    /// <returns>A new empty ContainerInventory.</returns>
    public static ContainerInventory Empty(int capacity = 10) => Create(capacity);

    /// <summary>
    /// Attempts to add an item to the container.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <returns>True if the item was added, false if container is full or item is null.</returns>
    public bool TryAddItem(Item item)
    {
        if (IsFull) return false;
        if (item == null) return false;

        _items.Add(item);
        return true;
    }

    /// <summary>
    /// Removes an item from the container.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns>True if the item was removed.</returns>
    public bool RemoveItem(Item item)
    {
        return _items.Remove(item);
    }

    /// <summary>
    /// Finds an item by name (case-insensitive).
    /// </summary>
    /// <param name="name">The item name to search for.</param>
    /// <returns>The matching item, or null if not found.</returns>
    public Item? GetItemByName(string name)
    {
        return _items.FirstOrDefault(i =>
            i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Finds an item by partial name match (case-insensitive).
    /// </summary>
    /// <param name="partialName">The partial name to search for.</param>
    /// <returns>The first matching item, or null if not found.</returns>
    public Item? GetItemByPartialName(string partialName)
    {
        return _items.FirstOrDefault(i =>
            i.Name.Contains(partialName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Takes all items from the container, clearing it.
    /// </summary>
    /// <returns>All items that were in the container.</returns>
    public IEnumerable<Item> TakeAll()
    {
        var items = _items.ToList();
        _items.Clear();
        return items;
    }

    /// <summary>
    /// Populates the container with items (for initialization).
    /// </summary>
    /// <param name="items">The items to add.</param>
    /// <remarks>Stops adding if capacity is reached.</remarks>
    public void Populate(IEnumerable<Item> items)
    {
        foreach (var item in items)
        {
            if (!TryAddItem(item)) break;
        }
    }

    /// <summary>
    /// Gets a formatted list of contents for display.
    /// </summary>
    /// <returns>Formatted string of container contents.</returns>
    public string GetContentsDescription()
    {
        if (IsEmpty)
            return "The container is empty.";

        var lines = _items.Select(i => $"  - {i.Name}");
        return "Inside you find:\n" + string.Join("\n", lines);
    }
}
