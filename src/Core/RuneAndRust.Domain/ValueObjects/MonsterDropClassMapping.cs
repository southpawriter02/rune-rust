using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Maps a monster type to its drop class and drop count.
/// </summary>
/// <remarks>
/// <para>
/// MonsterDropClassMapping is an immutable value object that defines how a
/// specific monster type relates to the loot drop system. The drop class
/// determines the probability distribution of quality tiers, while drop count
/// specifies how many items the monster drops.
/// </para>
/// <para>
/// For unknown monster types, use <see cref="Default"/> which provides
/// standard drop behavior (Standard class, 1 item).
/// </para>
/// </remarks>
public readonly record struct MonsterDropClassMapping
{
    /// <summary>
    /// Gets the monster type identifier.
    /// </summary>
    /// <remarks>
    /// Monster type IDs are normalized to lowercase for consistent comparison.
    /// Examples: "goblin", "dragon-boss", "skeleton-warrior"
    /// </remarks>
    public string MonsterTypeId { get; init; }

    /// <summary>
    /// Gets the enemy drop class for this monster type.
    /// </summary>
    /// <remarks>
    /// Drop class determines tier probability distribution:
    /// <list type="bullet">
    ///   <item><description>Trash - Primarily Tier 0-1, 10% no-drop chance</description></item>
    ///   <item><description>Standard - Guaranteed drops, up to Tier 3</description></item>
    ///   <item><description>Elite - Guaranteed drops, small Tier 4 chance</description></item>
    ///   <item><description>MiniBoss - Tier 2+ guaranteed, 30% Tier 4 chance</description></item>
    ///   <item><description>Boss - Always Tier 3-4, 70% Tier 4 chance</description></item>
    /// </list>
    /// </remarks>
    public EnemyDropClass DropClass { get; init; }

    /// <summary>
    /// Gets the number of items this monster drops.
    /// </summary>
    /// <remarks>
    /// Most monsters drop 1 item. Bosses and elite enemies may drop multiple.
    /// Minimum value is 0 (for trash enemies that can drop nothing).
    /// </remarks>
    public int DropCount { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this monster drops multiple items.
    /// </summary>
    public bool DropsMultipleItems => DropCount > 1;

    /// <summary>
    /// Gets whether this is a boss-tier monster.
    /// </summary>
    public bool IsBoss => DropClass == EnemyDropClass.Boss;

    /// <summary>
    /// Gets whether this is a mini-boss monster.
    /// </summary>
    public bool IsMiniBoss => DropClass == EnemyDropClass.MiniBoss;

    /// <summary>
    /// Gets whether this is an elite monster.
    /// </summary>
    public bool IsElite => DropClass == EnemyDropClass.Elite;

    /// <summary>
    /// Gets whether this monster can potentially drop nothing.
    /// </summary>
    /// <remarks>
    /// Only Trash enemies have a chance to drop nothing.
    /// </remarks>
    public bool CanDropNothing => DropClass == EnemyDropClass.Trash;

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the default mapping for unknown monster types.
    /// </summary>
    /// <remarks>
    /// Returns Standard drop class with 1 item drop.
    /// </remarks>
    public static MonsterDropClassMapping Default => new()
    {
        MonsterTypeId = "unknown",
        DropClass = EnemyDropClass.Standard,
        DropCount = 1
    };

    /// <summary>
    /// Creates a monster drop class mapping with validation.
    /// </summary>
    /// <param name="monsterTypeId">The monster type identifier (will be normalized to lowercase).</param>
    /// <param name="dropClass">The enemy drop class.</param>
    /// <param name="dropCount">The number of items to drop (default: 1).</param>
    /// <returns>A new MonsterDropClassMapping instance.</returns>
    /// <exception cref="ArgumentException">Thrown when monsterTypeId is null or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when dropCount is negative.</exception>
    public static MonsterDropClassMapping Create(
        string monsterTypeId,
        EnemyDropClass dropClass,
        int dropCount = 1)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(monsterTypeId, nameof(monsterTypeId));
        ArgumentOutOfRangeException.ThrowIfNegative(dropCount, nameof(dropCount));

        return new MonsterDropClassMapping
        {
            MonsterTypeId = monsterTypeId.ToLowerInvariant(),
            DropClass = dropClass,
            DropCount = dropCount
        };
    }

    /// <summary>
    /// Creates a boss mapping with appropriate defaults.
    /// </summary>
    /// <param name="monsterTypeId">The monster type identifier.</param>
    /// <param name="dropCount">The number of items to drop (default: 3).</param>
    /// <returns>A new MonsterDropClassMapping configured for boss drops.</returns>
    public static MonsterDropClassMapping CreateBoss(string monsterTypeId, int dropCount = 3)
    {
        return Create(monsterTypeId, EnemyDropClass.Boss, dropCount);
    }

    /// <summary>
    /// Creates a mini-boss mapping with appropriate defaults.
    /// </summary>
    /// <param name="monsterTypeId">The monster type identifier.</param>
    /// <param name="dropCount">The number of items to drop (default: 2).</param>
    /// <returns>A new MonsterDropClassMapping configured for mini-boss drops.</returns>
    public static MonsterDropClassMapping CreateMiniBoss(string monsterTypeId, int dropCount = 2)
    {
        return Create(monsterTypeId, EnemyDropClass.MiniBoss, dropCount);
    }

    /// <summary>
    /// Creates an elite mapping with appropriate defaults.
    /// </summary>
    /// <param name="monsterTypeId">The monster type identifier.</param>
    /// <param name="dropCount">The number of items to drop (default: 2).</param>
    /// <returns>A new MonsterDropClassMapping configured for elite drops.</returns>
    public static MonsterDropClassMapping CreateElite(string monsterTypeId, int dropCount = 2)
    {
        return Create(monsterTypeId, EnemyDropClass.Elite, dropCount);
    }

    /// <summary>
    /// Returns a string representation for logging.
    /// </summary>
    public override string ToString() =>
        $"[MonsterDropClassMapping: {MonsterTypeId} -> {DropClass}, drops={DropCount}]";
}
