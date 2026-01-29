using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Defines the contract for smart loot selection with class-appropriate bias.
/// </summary>
/// <remarks>
/// <para>
/// ISmartLootService implements the 60/40 smart loot algorithm that biases
/// item selection toward class-appropriate equipment for the player's archetype.
/// This creates a more satisfying loot experience where players receive
/// gear they can actually use more often than pure random chance would provide.
/// </para>
/// <para>
/// The algorithm flow:
/// 1. Roll 0-99 to determine selection path
/// 2. If roll &lt; BiasPercentage (default 60%), attempt class-appropriate selection
/// 3. Filter available items by archetype using IEquipmentClassMappingProvider
/// 4. If filtered pool is empty, fall back to random selection
/// 5. If roll &gt;= BiasPercentage, use random selection from full pool
/// </para>
/// <para>
/// Statistics are tracked internally and can be retrieved for debugging,
/// balancing analysis, and player feedback systems.
/// </para>
/// </remarks>
public interface ISmartLootService
{
    /// <summary>
    /// Selects an item from the available pool using the smart loot algorithm.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The selection process:
    /// 1. Validate context has available items
    /// 2. If no archetype, return random selection
    /// 3. Roll for bias (0-99)
    /// 4. If roll favors class-appropriate, filter and select from filtered pool
    /// 5. If filtered pool empty, fall back to random
    /// 6. If roll favors random, select from full pool
    /// </para>
    /// <para>
    /// The result includes metadata about the selection path for debugging
    /// and statistics tracking.
    /// </para>
    /// </remarks>
    /// <param name="context">The smart loot context containing archetype, tier, items, and bias settings.</param>
    /// <returns>A SmartLootResult containing the selected item and metadata.</returns>
    SmartLootResult SelectItem(SmartLootContext context);

    /// <summary>
    /// Filters items to only those appropriate for the specified archetype.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Uses IEquipmentClassMappingProvider to check each item's CategoryId
    /// against the archetype. Items without a CategoryId (consumables, etc.)
    /// are excluded from class-appropriate filtering.
    /// </para>
    /// <para>
    /// This method is exposed for testing and for scenarios where only
    /// the filtering logic is needed without full selection.
    /// </para>
    /// </remarks>
    /// <param name="items">The pool of items to filter.</param>
    /// <param name="archetypeId">The player's archetype ID (e.g., "warrior", "mystic").</param>
    /// <returns>A list of items appropriate for the archetype.</returns>
    IReadOnlyList<LootEntry> FilterClassAppropriateItems(
        IReadOnlyList<LootEntry> items,
        string archetypeId);

    /// <summary>
    /// Calculates whether a bias roll favors class-appropriate selection.
    /// </summary>
    /// <remarks>
    /// Returns true if roll is less than biasPercentage.
    /// For example, with default 60% bias, rolls 0-59 return true,
    /// rolls 60-99 return false.
    /// </remarks>
    /// <param name="roll">The random roll value (0-99).</param>
    /// <param name="biasPercentage">The bias percentage threshold (0-100).</param>
    /// <returns>True if the roll favors class-appropriate selection.</returns>
    bool CalculateBiasResult(int roll, int biasPercentage);

    /// <summary>
    /// Gets the current statistics for smart loot selections.
    /// </summary>
    /// <remarks>
    /// Statistics are accumulated across all SelectItem calls and can be
    /// used for balancing analysis, debugging, and player feedback.
    /// </remarks>
    /// <returns>Current statistics snapshot.</returns>
    SmartLootStatistics GetStatistics();

    /// <summary>
    /// Resets all selection statistics to zero.
    /// </summary>
    /// <remarks>
    /// Called at session boundaries or for testing isolation.
    /// </remarks>
    void ResetStatistics();
}

/// <summary>
/// Tracks aggregate statistics for smart loot selections.
/// </summary>
/// <remarks>
/// <para>
/// SmartLootStatistics provides insight into the distribution of loot
/// selections across different algorithm paths. This data helps with:
/// - Verifying the 60/40 bias is working as intended
/// - Identifying if fallbacks are occurring too frequently
/// - Balancing loot tables to ensure adequate class-appropriate options
/// </para>
/// </remarks>
/// <param name="TotalSelections">Total number of items selected.</param>
/// <param name="ClassAppropriateSelections">Count of items selected from class-appropriate pool.</param>
/// <param name="RandomSelections">Count of items selected via random path (40% or no archetype).</param>
/// <param name="FallbackSelections">Count of selections where bias succeeded but no class items were available.</param>
public readonly record struct SmartLootStatistics(
    int TotalSelections,
    int ClassAppropriateSelections,
    int RandomSelections,
    int FallbackSelections)
{
    /// <summary>
    /// Gets the percentage of selections that were class-appropriate.
    /// </summary>
    /// <remarks>
    /// With a 60% bias and adequate class-appropriate items, this should
    /// approach 60% over many selections. Lower values indicate either
    /// insufficient class-appropriate items or frequent fallbacks.
    /// </remarks>
    public double ClassAppropriatePercentage =>
        TotalSelections > 0
            ? (double)ClassAppropriateSelections / TotalSelections * 100
            : 0;

    /// <summary>
    /// Gets the percentage of selections that used the random path.
    /// </summary>
    /// <remarks>
    /// Should approach 40% when archetype is always provided and
    /// class-appropriate items are always available.
    /// </remarks>
    public double RandomPercentage =>
        TotalSelections > 0
            ? (double)RandomSelections / TotalSelections * 100
            : 0;

    /// <summary>
    /// Gets the percentage of selections that required fallback.
    /// </summary>
    /// <remarks>
    /// High fallback rates indicate loot tables need more class-appropriate
    /// items for the archetypes being played.
    /// </remarks>
    public double FallbackPercentage =>
        TotalSelections > 0
            ? (double)FallbackSelections / TotalSelections * 100
            : 0;

    /// <summary>
    /// Gets an empty statistics instance with all values at zero.
    /// </summary>
    public static SmartLootStatistics Empty => new(0, 0, 0, 0);

    /// <summary>
    /// Returns a string representation for debugging and logging.
    /// </summary>
    /// <returns>A formatted string showing statistics summary.</returns>
    public override string ToString() =>
        $"SmartLootStatistics[Total={TotalSelections}, " +
        $"Class={ClassAppropriateSelections} ({ClassAppropriatePercentage:F1}%), " +
        $"Random={RandomSelections} ({RandomPercentage:F1}%), " +
        $"Fallback={FallbackSelections} ({FallbackPercentage:F1}%)]";
}
