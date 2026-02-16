using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the Bone-Setter's Medical Supplies inventory.
/// An immutable collection of consumable medical items with a maximum carry capacity.
/// </summary>
/// <remarks>
/// <para>Unlike <see cref="RageResource"/> which uses mutable in-place updates for combat
/// performance, MedicalSuppliesResource is fully immutable — all modification operations
/// (<see cref="SpendSupply"/>, <see cref="AddSupply"/>) return new instances.
/// This ensures thread safety and simplifies state management.</para>
/// <para>Key characteristics:</para>
/// <list type="bullet">
/// <item>Default capacity: 10 individual items</item>
/// <item>Supplies are typed (<see cref="MedicalSupplyType"/>) and quality-rated (1–5)</item>
/// <item>No automatic regeneration — must be acquired via salvage, purchase, or crafting</item>
/// <item>Consumed by healing abilities: Field Dressing (1), Emergency Surgery (2), Miracle Worker (3)</item>
/// </list>
/// </remarks>
public sealed record MedicalSuppliesResource
{
    /// <summary>
    /// Default maximum number of supply items that can be carried.
    /// </summary>
    public const int DefaultMaxCapacity = 10;

    /// <summary>
    /// Default number of starting supply items for new Bone-Setter characters.
    /// </summary>
    public const int DefaultStartingCount = 5;

    /// <summary>
    /// Immutable collection of all Medical Supply items currently in inventory.
    /// </summary>
    public IReadOnlyList<MedicalSupplyItem> Supplies { get; init; } = [];

    /// <summary>
    /// Maximum number of individual supply items that can be carried.
    /// Default: <see cref="DefaultMaxCapacity"/> (10).
    /// </summary>
    public int MaxCarryCapacity { get; init; } = DefaultMaxCapacity;

    /// <summary>
    /// UTC timestamp of the last inventory modification.
    /// Used for audit trails and potential restock cooldown mechanics.
    /// </summary>
    public DateTime? LastModifiedAt { get; init; }

    /// <summary>
    /// Creates an empty Medical Supplies resource with default capacity.
    /// </summary>
    /// <returns>A new empty resource with capacity of <see cref="DefaultMaxCapacity"/>.</returns>
    public static MedicalSuppliesResource Create()
    {
        return new MedicalSuppliesResource
        {
            Supplies = [],
            MaxCarryCapacity = DefaultMaxCapacity,
            LastModifiedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a Medical Supplies resource with initial supplies and specified capacity.
    /// </summary>
    /// <param name="supplies">Initial supply items to include in inventory.</param>
    /// <param name="maxCapacity">Maximum carry capacity. Must be at least 1.</param>
    /// <returns>A new resource initialized with the provided supplies.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxCapacity"/> is less than 1.</exception>
    /// <exception cref="ArgumentException">Thrown when supply count exceeds <paramref name="maxCapacity"/>.</exception>
    public static MedicalSuppliesResource Create(IEnumerable<MedicalSupplyItem> supplies, int maxCapacity = DefaultMaxCapacity)
    {
        if (maxCapacity < 1)
            throw new ArgumentOutOfRangeException(nameof(maxCapacity), maxCapacity,
                "Max carry capacity must be at least 1.");

        var supplyList = supplies?.ToList() ?? [];

        if (supplyList.Count > maxCapacity)
            throw new ArgumentException(
                $"Supply count ({supplyList.Count}) exceeds max capacity ({maxCapacity}).",
                nameof(supplies));

        return new MedicalSuppliesResource
        {
            Supplies = supplyList.AsReadOnly(),
            MaxCarryCapacity = maxCapacity,
            LastModifiedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Gets the total count of all Medical Supply items in inventory.
    /// </summary>
    /// <returns>The number of supply items currently held.</returns>
    public int GetTotalSupplyCount() => Supplies.Count;

    /// <summary>
    /// Gets the count of supplies of a specific type.
    /// </summary>
    /// <param name="type">The supply type to count.</param>
    /// <returns>The number of items matching the specified type.</returns>
    public int GetCountByType(MedicalSupplyType type) =>
        Supplies.Count(s => s.SupplyType == type);

    /// <summary>
    /// Checks if at least one supply of the specified type exists in inventory.
    /// </summary>
    /// <param name="type">The supply type to check for.</param>
    /// <returns>True if at least one item of the specified type is available.</returns>
    public bool HasSupply(MedicalSupplyType type) =>
        Supplies.Any(s => s.SupplyType == type);

    /// <summary>
    /// Checks if inventory has room for additional items.
    /// </summary>
    /// <returns>True if current count is below <see cref="MaxCarryCapacity"/>.</returns>
    public bool CanCarryMore() =>
        Supplies.Count < MaxCarryCapacity;

    /// <summary>
    /// Gets the highest quality supply of a specific type.
    /// Returns null if no supplies of that type exist.
    /// </summary>
    /// <param name="type">The supply type to search for.</param>
    /// <returns>The highest quality <see cref="MedicalSupplyItem"/> of the specified type, or null if none available.</returns>
    public MedicalSupplyItem? GetHighestQualitySupply(MedicalSupplyType type) =>
        Supplies
            .Where(s => s.SupplyType == type)
            .OrderByDescending(s => s.Quality)
            .FirstOrDefault();

    /// <summary>
    /// Gets the highest quality supply of any type.
    /// Returns null if inventory is empty.
    /// </summary>
    /// <returns>The highest quality <see cref="MedicalSupplyItem"/> in inventory, or null if empty.</returns>
    public MedicalSupplyItem? GetHighestQualitySupply() =>
        Supplies
            .OrderByDescending(s => s.Quality)
            .FirstOrDefault();

    /// <summary>
    /// Spends one supply of the specified type, returning a new resource with the item removed.
    /// </summary>
    /// <param name="type">The type of supply to consume.</param>
    /// <returns>A new <see cref="MedicalSuppliesResource"/> with one item of the specified type removed.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no supply of the specified type is available.</exception>
    public MedicalSuppliesResource SpendSupply(MedicalSupplyType type)
    {
        var supplyToRemove = Supplies.FirstOrDefault(s => s.SupplyType == type);
        if (supplyToRemove == null)
            throw new InvalidOperationException(
                $"No {type} available to spend. Current inventory: {GetInventorySummary()}");

        var newSupplies = Supplies.Where(s => s.ItemId != supplyToRemove.ItemId).ToList();
        return new MedicalSuppliesResource
        {
            Supplies = newSupplies.AsReadOnly(),
            MaxCarryCapacity = MaxCarryCapacity,
            LastModifiedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Spends one supply of any available type (lowest quality first), returning a new resource.
    /// </summary>
    /// <returns>A tuple containing the new resource and the supply item that was consumed.</returns>
    /// <exception cref="InvalidOperationException">Thrown when inventory is empty.</exception>
    public (MedicalSuppliesResource Resource, MedicalSupplyItem SpentItem) SpendAnySupply()
    {
        if (Supplies.Count == 0)
            throw new InvalidOperationException(
                "No supplies available to spend. Inventory is empty.");

        var supplyToRemove = Supplies
            .OrderBy(s => s.Quality)
            .First();

        var newSupplies = Supplies.Where(s => s.ItemId != supplyToRemove.ItemId).ToList();
        var newResource = new MedicalSuppliesResource
        {
            Supplies = newSupplies.AsReadOnly(),
            MaxCarryCapacity = MaxCarryCapacity,
            LastModifiedAt = DateTime.UtcNow
        };

        return (newResource, supplyToRemove);
    }

    /// <summary>
    /// Adds a new supply item to inventory, returning a new resource with the item included.
    /// </summary>
    /// <param name="item">The supply item to add.</param>
    /// <returns>A new <see cref="MedicalSuppliesResource"/> with the item added.</returns>
    /// <exception cref="InvalidOperationException">Thrown when inventory is at maximum capacity.</exception>
    public MedicalSuppliesResource AddSupply(MedicalSupplyItem item)
    {
        if (!CanCarryMore())
            throw new InvalidOperationException(
                $"Cannot add supply: inventory is full ({Supplies.Count}/{MaxCarryCapacity}).");

        var newSupplies = new List<MedicalSupplyItem>(Supplies) { item };
        return new MedicalSuppliesResource
        {
            Supplies = newSupplies.AsReadOnly(),
            MaxCarryCapacity = MaxCarryCapacity,
            LastModifiedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Returns a human-readable summary of current inventory grouped by supply type.
    /// Example: "Bandage(3) + Salve(2) + Herbs(1) = 6/10"
    /// </summary>
    /// <returns>A formatted inventory summary string.</returns>
    public string GetInventorySummary()
    {
        var groupedByType = Supplies
            .GroupBy(s => s.SupplyType)
            .OrderBy(g => g.Key)
            .Select(g => $"{g.Key}({g.Count()})")
            .ToList();

        var itemsSummary = groupedByType.Count > 0
            ? string.Join(" + ", groupedByType)
            : "Empty";

        return $"{itemsSummary} = {Supplies.Count}/{MaxCarryCapacity}";
    }

    /// <summary>
    /// Gets a formatted resource value for UI status display.
    /// Example: "5/10"
    /// </summary>
    /// <returns>A string in the format "current/max".</returns>
    public string GetFormattedValue() =>
        $"{Supplies.Count}/{MaxCarryCapacity}";
}
