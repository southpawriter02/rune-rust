// ═══════════════════════════════════════════════════════════════════════════════
// RUNE & RUST — v0.16.3c Weighted Item Selection
// ═══════════════════════════════════════════════════════════════════════════════
// File: WeightedItemPool.cs
// Purpose: Entity managing weighted item collection with O(log n) random selection
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Manages a collection of weighted items and provides efficient weighted random selection.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="WeightedItemPool"/> provides O(log n) weighted random selection using
/// cumulative weights and binary search. This is substantially more efficient than
/// linear scanning for large pools.
/// </para>
/// <para>
/// <b>Selection Algorithm:</b>
/// <list type="number">
/// <item><description>Build cumulative weight array (lazy, on modification)</description></item>
/// <item><description>Generate random value in [0, TotalWeight)</description></item>
/// <item><description>Binary search for first cumulative weight > random value</description></item>
/// <item><description>Return corresponding item</description></item>
/// </list>
/// </para>
/// <para>
/// Items with <see cref="WeightedItem.Weight"/> = 0 are stored but excluded from selection,
/// allowing items to be temporarily disabled without removal.
/// </para>
/// <para>
/// Empty pools or pools where all items have zero weight throw
/// <see cref="InvalidOperationException"/> on selection attempts.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var pool = WeightedItemPool.Create();
/// pool.Add(WeightedItem.CreateCommon(commonSword));   // Weight 100
/// pool.Add(WeightedItem.CreateRare(rareSword));       // Weight 25
/// 
/// // Select with weighted probability (Common ~80%, Rare ~20%)
/// var selected = pool.SelectRandom(new Random());
/// </code>
/// </example>
public class WeightedItemPool : IEntity
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Internal list of weighted items in the pool.
    /// </summary>
    private readonly List<WeightedItem> _items = [];

    /// <summary>
    /// Pre-computed cumulative weights for O(log n) binary search selection.
    /// Index i contains the sum of weights for items [0..i].
    /// Recalculated when <see cref="_isDirty"/> is true.
    /// </summary>
    private readonly List<int> _cumulativeWeights = [];

    /// <summary>
    /// Cached total weight of all selectable items (weight > 0).
    /// </summary>
    private int _totalWeight;

    /// <summary>
    /// Flag indicating cumulative weights need recalculation.
    /// Set to true when items are added, removed, or modified.
    /// </summary>
    private bool _isDirty;

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique identifier for this pool.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the items in this pool as a read-only list.
    /// </summary>
    /// <remarks>
    /// Includes all items regardless of weight (including zero-weight disabled items).
    /// </remarks>
    public IReadOnlyList<WeightedItem> Items => _items.AsReadOnly();

    /// <summary>
    /// Gets the total weight of all selectable items (weight greater than 0).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value is recalculated lazily when items change. Zero-weight items
    /// do not contribute to the total.
    /// </para>
    /// <para>
    /// When <see cref="TotalWeight"/> is 0, the pool has no selectable items
    /// and <see cref="SelectRandom(Random)"/> will throw.
    /// </para>
    /// </remarks>
    public int TotalWeight
    {
        get
        {
            EnsureCumulativeWeightsUpdated();
            return _totalWeight;
        }
    }

    /// <summary>
    /// Gets the total count of items in the pool (including disabled items).
    /// </summary>
    public int ItemCount => _items.Count;

    /// <summary>
    /// Gets the count of selectable items (weight greater than 0).
    /// </summary>
    /// <remarks>
    /// This count may differ from <see cref="ItemCount"/> if some items
    /// have been disabled by setting their weight to 0.
    /// </remarks>
    public int SelectableItemCount => _items.Count(i => i.IsSelectable);

    /// <summary>
    /// Gets a value indicating whether the pool has any selectable items.
    /// </summary>
    /// <remarks>
    /// Returns false if the pool is empty or all items have zero weight.
    /// </remarks>
    public bool HasSelectableItems => TotalWeight > 0;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Private constructor for creation through factory methods.
    /// </summary>
    private WeightedItemPool()
    {
        Id = Guid.NewGuid();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates an empty <see cref="WeightedItemPool"/>.
    /// </summary>
    /// <returns>A new empty pool ready for items to be added.</returns>
    public static WeightedItemPool Create() => new();

    /// <summary>
    /// Creates a <see cref="WeightedItemPool"/> pre-populated with the specified items.
    /// </summary>
    /// <param name="items">The weighted items to initialize the pool with.</param>
    /// <returns>A new pool containing all provided items.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="items"/> is null.</exception>
    public static WeightedItemPool CreateFrom(IEnumerable<WeightedItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var pool = new WeightedItemPool();
        pool.AddRange(items);
        return pool;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ITEM MANAGEMENT
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Adds an item to the pool with the specified weight and optional rarity label.
    /// </summary>
    /// <param name="item">The loot entry to add.</param>
    /// <param name="weight">Selection weight (>= 0). Zero disables selection.</param>
    /// <param name="rarity">Optional rarity label for display purposes.</param>
    /// <exception cref="ArgumentException">Thrown when item has an invalid ItemId.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when weight is negative.</exception>
    public void Add(LootEntry item, int weight, string rarity = "")
    {
        // Delegate validation to WeightedItem.Create
        _items.Add(WeightedItem.Create(item, weight, rarity));
        _isDirty = true;
    }

    /// <summary>
    /// Adds a pre-created <see cref="WeightedItem"/> to the pool.
    /// </summary>
    /// <param name="weightedItem">The weighted item to add.</param>
    public void Add(WeightedItem weightedItem)
    {
        _items.Add(weightedItem);
        _isDirty = true;
    }

    /// <summary>
    /// Adds multiple weighted items to the pool.
    /// </summary>
    /// <param name="items">The collection of weighted items to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="items"/> is null.</exception>
    public void AddRange(IEnumerable<WeightedItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        _items.AddRange(items);
        _isDirty = true;
    }

    /// <summary>
    /// Removes an item from the pool by its item ID.
    /// </summary>
    /// <param name="itemId">The ID of the item to remove.</param>
    /// <returns>True if the item was found and removed; otherwise, false.</returns>
    public bool Remove(string itemId)
    {
        var index = _items.FindIndex(i => i.ItemId == itemId);
        if (index < 0)
        {
            return false;
        }

        _items.RemoveAt(index);
        _isDirty = true;
        return true;
    }

    /// <summary>
    /// Checks whether an item exists in the pool.
    /// </summary>
    /// <param name="itemId">The ID of the item to check.</param>
    /// <returns>True if the item is in the pool (regardless of weight); otherwise, false.</returns>
    public bool Contains(string itemId) =>
        _items.Any(i => i.ItemId == itemId);

    /// <summary>
    /// Removes all items from the pool.
    /// </summary>
    public void Clear()
    {
        _items.Clear();
        _cumulativeWeights.Clear();
        _totalWeight = 0;
        _isDirty = false;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // WEIGHTED SELECTION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Selects a random item using weighted probability.
    /// </summary>
    /// <param name="random">The random number generator to use.</param>
    /// <returns>The selected <see cref="WeightedItem"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="random"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the pool is empty or has no selectable items (all weights are zero).
    /// </exception>
    /// <remarks>
    /// <para>
    /// Selection uses cumulative weights and binary search for O(log n) complexity.
    /// </para>
    /// <para>
    /// Items with higher weights are proportionally more likely to be selected.
    /// Zero-weight items are never selected.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Item A (weight 100) selected ~80% of the time
    /// // Item B (weight 25) selected ~20% of the time
    /// var selected = pool.SelectRandom(new Random());
    /// </code>
    /// </example>
    public WeightedItem SelectRandom(Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        EnsureCumulativeWeightsUpdated();

        // Validate pool has selectable items
        if (_items.Count == 0)
        {
            throw new InvalidOperationException("Cannot select from empty pool.");
        }

        if (_totalWeight == 0)
        {
            throw new InvalidOperationException("No selectable items (all weights are zero).");
        }

        // Generate random value in [0, TotalWeight)
        // Example: TotalWeight = 190, roll in range [0, 189]
        var roll = random.Next(0, _totalWeight);

        // Binary search for the item corresponding to this roll
        return FindItemByRoll(roll);
    }

    /// <summary>
    /// Selects a random item using a seed for deterministic results.
    /// </summary>
    /// <param name="seed">Random seed for reproducible selection.</param>
    /// <returns>The selected <see cref="WeightedItem"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the pool is empty or has no selectable items.
    /// </exception>
    /// <remarks>
    /// Primarily used for unit testing to verify selection behavior deterministically.
    /// The same seed with the same pool will always return the same item.
    /// </remarks>
    public WeightedItem SelectRandom(int seed) =>
        SelectRandom(new Random(seed));

    // ═══════════════════════════════════════════════════════════════════════════
    // PROBABILITY QUERIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the selection probability for a specific item.
    /// </summary>
    /// <param name="itemId">The ID of the item to check.</param>
    /// <returns>
    /// Probability as a value between 0.0 and 1.0. Returns 0 if the item
    /// is not found, has zero weight, or the pool has no selectable items.
    /// </returns>
    public double GetSelectionProbability(string itemId)
    {
        EnsureCumulativeWeightsUpdated();

        var item = _items.FirstOrDefault(i => i.ItemId == itemId);

        // Return 0 for not found (default struct has Weight=0), zero weight, or empty pool
        return item.Weight > 0 && _totalWeight > 0
            ? (double)item.Weight / _totalWeight
            : 0;
    }

    /// <summary>
    /// Gets selection probabilities for all items in the pool.
    /// </summary>
    /// <returns>
    /// A dictionary mapping item IDs to their selection probabilities (0.0 to 1.0).
    /// Zero-weight items will have probability 0.
    /// </returns>
    public IReadOnlyDictionary<string, double> GetAllProbabilities()
    {
        EnsureCumulativeWeightsUpdated();

        return _items.ToDictionary(
            i => i.ItemId,
            i => i.GetSelectionProbability(_totalWeight));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Recalculates cumulative weights if the pool has been modified.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method implements lazy recalculation - cumulative weights are only
    /// rebuilt when items have been added, removed, or the pool is marked dirty.
    /// </para>
    /// <para>
    /// Cumulative weight array example:
    /// <code>
    /// Items:       [Common(100), Uncommon(50), Rare(25)]
    /// Cumulative:  [100, 150, 175]
    /// </code>
    /// </para>
    /// </remarks>
    private void EnsureCumulativeWeightsUpdated()
    {
        if (!_isDirty)
        {
            return;
        }

        // Reset cumulative state
        _cumulativeWeights.Clear();
        _totalWeight = 0;

        // Build cumulative weight array
        // Each entry contains the sum of weights up to and including that index
        foreach (var item in _items)
        {
            // Only add weight if item is selectable (weight > 0)
            if (item.Weight > 0)
            {
                _totalWeight += item.Weight;
            }

            // Store cumulative weight for this position
            // (even for zero-weight items, to maintain index alignment)
            _cumulativeWeights.Add(_totalWeight);
        }

        _isDirty = false;
    }

    /// <summary>
    /// Finds the item corresponding to a roll value using binary search.
    /// </summary>
    /// <param name="roll">The random roll value in [0, TotalWeight).</param>
    /// <returns>The <see cref="WeightedItem"/> whose cumulative weight range contains the roll.</returns>
    /// <remarks>
    /// <para>
    /// Binary search finds the first index where cumulative weight > roll.
    /// </para>
    /// <para>
    /// Example with cumulative weights [100, 150, 175, 185, 190]:
    /// <code>
    /// Roll = 0   → Index 0 (0 &lt; 100)     → First item (Common)
    /// Roll = 99  → Index 0 (99 &lt; 100)    → First item (Common)
    /// Roll = 100 → Index 1 (100 = 100)   → Second item (Uncommon)  
    /// Roll = 150 → Index 2 (150 = 150)   → Third item (Rare)
    /// Roll = 189 → Index 4 (189 &lt; 190)   → Fifth item (Unique)
    /// </code>
    /// </para>
    /// </remarks>
    private WeightedItem FindItemByRoll(int roll)
    {
        // Binary search for the first cumulative weight greater than roll
        // This gives us the index of the selected item
        var low = 0;
        var high = _items.Count - 1;

        while (low < high)
        {
            var mid = (low + high) / 2;

            // If cumulative weight at mid is <= roll, the item is to the right
            if (_cumulativeWeights[mid] <= roll)
            {
                low = mid + 1;
            }
            else
            {
                // Otherwise, this index or an earlier one is the answer
                high = mid;
            }
        }

        return _items[low];
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY / DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation for debugging and logging.
    /// </summary>
    /// <returns>A formatted string showing item count and total weight.</returns>
    public override string ToString() =>
        $"WeightedItemPool: {ItemCount} items, TotalWeight={TotalWeight}";
}
