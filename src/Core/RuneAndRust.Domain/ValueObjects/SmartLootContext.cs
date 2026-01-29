using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Encapsulates all inputs for smart loot selection.
/// </summary>
/// <remarks>
/// <para>
/// SmartLootContext provides the complete context needed for the smart loot
/// algorithm to select an appropriate item. It includes the player's archetype
/// for class-appropriate filtering, the quality tier, available items, and
/// the configurable bias percentage.
/// </para>
/// <para>
/// The bias percentage (default 60%) determines how often the algorithm
/// favors class-appropriate items over random selection. A bias of 60 means
/// 60% of selections will attempt to use class-appropriate items if available.
/// </para>
/// <para>
/// For deterministic testing, an optional random seed can be provided to
/// ensure reproducible selection behavior.
/// </para>
/// </remarks>
/// <param name="PlayerArchetypeId">The player's archetype ID (e.g., "warrior", "mystic"). Null for random-only selection.</param>
/// <param name="QualityTier">The quality tier for this loot drop.</param>
/// <param name="AvailableItems">The pool of items available for selection.</param>
/// <param name="BiasPercentage">The percentage (0-100) favoring class-appropriate items. Default is 60.</param>
/// <param name="RandomSeed">Optional seed for deterministic random selection (testing).</param>
public readonly record struct SmartLootContext(
    string? PlayerArchetypeId,
    QualityTier QualityTier,
    IReadOnlyList<LootEntry> AvailableItems,
    int BiasPercentage = 60,
    int? RandomSeed = null)
{
    /// <summary>
    /// The default bias percentage (60%) favoring class-appropriate items.
    /// </summary>
    /// <remarks>
    /// With a 60% bias, roughly 6 out of 10 selections will attempt to select
    /// from the class-appropriate pool when archetypeId is provided.
    /// </remarks>
    public const int DefaultBiasPercentage = 60;

    /// <summary>
    /// Gets a value indicating whether this context has a player archetype specified.
    /// </summary>
    /// <remarks>
    /// When false, the smart loot algorithm falls back to purely random selection
    /// since no class-appropriate filtering is possible.
    /// </remarks>
    public bool HasPlayerArchetype => !string.IsNullOrWhiteSpace(PlayerArchetypeId);

    /// <summary>
    /// Gets a value indicating whether there are items available for selection.
    /// </summary>
    /// <remarks>
    /// When false, the selection should return SmartLootResult.Empty.
    /// </remarks>
    public bool HasAvailableItems => AvailableItems?.Count > 0;

    /// <summary>
    /// Gets the normalized (lowercase) archetype ID for consistent lookups.
    /// </summary>
    /// <remarks>
    /// EquipmentClassMapping uses lowercase archetype IDs, so normalization
    /// ensures case-insensitive matching.
    /// </remarks>
    public string? NormalizedArchetypeId => PlayerArchetypeId?.ToLowerInvariant();

    /// <summary>
    /// Gets the count of available items for logging and statistics.
    /// </summary>
    public int AvailableItemCount => AvailableItems?.Count ?? 0;

    /// <summary>
    /// Creates a validated SmartLootContext with explicit parameters.
    /// </summary>
    /// <param name="playerArchetypeId">The player's archetype ID (e.g., "warrior", "mystic"). Null for random-only selection.</param>
    /// <param name="qualityTier">The quality tier for this loot drop.</param>
    /// <param name="availableItems">The pool of items available for selection. Cannot be null.</param>
    /// <param name="biasPercentage">The percentage (0-100) favoring class-appropriate items. Default is 60.</param>
    /// <param name="randomSeed">Optional seed for deterministic random selection (testing).</param>
    /// <returns>A validated SmartLootContext instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when availableItems is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when biasPercentage is negative or greater than 100.</exception>
    public static SmartLootContext Create(
        string? playerArchetypeId,
        QualityTier qualityTier,
        IReadOnlyList<LootEntry> availableItems,
        int biasPercentage = DefaultBiasPercentage,
        int? randomSeed = null)
    {
        // Validate required parameters
        ArgumentNullException.ThrowIfNull(availableItems);
        ArgumentOutOfRangeException.ThrowIfNegative(biasPercentage);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(biasPercentage, 100);

        return new SmartLootContext(
            playerArchetypeId,
            qualityTier,
            availableItems,
            biasPercentage,
            randomSeed);
    }

    /// <summary>
    /// Creates a context for random-only selection (no class-appropriate bias).
    /// </summary>
    /// <remarks>
    /// Used for container loot, treasure drops, or other scenarios where
    /// player class should not influence item selection. Sets bias to 0%.
    /// </remarks>
    /// <param name="qualityTier">The quality tier for this loot drop.</param>
    /// <param name="availableItems">The pool of items available for selection.</param>
    /// <param name="randomSeed">Optional seed for deterministic random selection (testing).</param>
    /// <returns>A SmartLootContext with no archetype and zero bias.</returns>
    /// <exception cref="ArgumentNullException">Thrown when availableItems is null.</exception>
    public static SmartLootContext CreateRandomOnly(
        QualityTier qualityTier,
        IReadOnlyList<LootEntry> availableItems,
        int? randomSeed = null)
    {
        ArgumentNullException.ThrowIfNull(availableItems);

        // Zero bias means all selections are random
        return new SmartLootContext(
            PlayerArchetypeId: null,
            QualityTier: qualityTier,
            AvailableItems: availableItems,
            BiasPercentage: 0,
            RandomSeed: randomSeed);
    }

    /// <summary>
    /// Creates a context with a fixed random seed for deterministic testing.
    /// </summary>
    /// <remarks>
    /// Enables unit tests to verify selection behavior with predictable
    /// random number sequences.
    /// </remarks>
    /// <param name="playerArchetypeId">The player's archetype ID.</param>
    /// <param name="qualityTier">The quality tier for this loot drop.</param>
    /// <param name="availableItems">The pool of items available for selection.</param>
    /// <param name="seed">The random seed for deterministic behavior.</param>
    /// <param name="biasPercentage">The percentage (0-100) favoring class-appropriate items. Default is 60.</param>
    /// <returns>A SmartLootContext with the specified seed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when availableItems is null.</exception>
    public static SmartLootContext CreateForTesting(
        string? playerArchetypeId,
        QualityTier qualityTier,
        IReadOnlyList<LootEntry> availableItems,
        int seed,
        int biasPercentage = DefaultBiasPercentage)
    {
        ArgumentNullException.ThrowIfNull(availableItems);
        ArgumentOutOfRangeException.ThrowIfNegative(biasPercentage);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(biasPercentage, 100);

        return new SmartLootContext(
            playerArchetypeId,
            qualityTier,
            availableItems,
            biasPercentage,
            RandomSeed: seed);
    }

    /// <summary>
    /// Returns a string representation for debugging and logging.
    /// </summary>
    /// <returns>A formatted string showing context details.</returns>
    public override string ToString() =>
        $"SmartLootContext[Archetype={PlayerArchetypeId ?? "none"}, " +
        $"Tier={QualityTier}, Items={AvailableItemCount}, Bias={BiasPercentage}%]";
}
