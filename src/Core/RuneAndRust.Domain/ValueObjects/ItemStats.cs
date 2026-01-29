namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Immutable value object representing stat bonuses provided by an item.
/// </summary>
/// <remarks>
/// <para>
/// ItemStats encapsulates all stat modifications that equipment can provide.
/// Both positive (bonuses) and negative (cursed items) values are supported.
/// </para>
/// <para>
/// Primary stats (Might, Agility, Will, Fortitude, Arcana) affect attribute
/// checks and derived values. Bonus stats (Health, Damage, Defense) provide
/// flat modifiers to combat calculations.
/// </para>
/// <para>
/// This value object is used primarily by <see cref="Entities.UniqueItem"/>
/// to define the fixed stat bonuses for Myth-Forged equipment.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create stats for a weapon with Might and damage bonuses
/// var stats = ItemStats.Create(might: 5, bonusDamage: 10);
/// 
/// // Create empty stats for items with no stat bonuses
/// var empty = ItemStats.Empty;
/// 
/// // Check if stats provide any bonuses
/// if (!stats.IsEmpty)
/// {
///     Console.WriteLine($"Item provides: {stats}");
/// }
/// </code>
/// </example>
public readonly record struct ItemStats
{
    /// <summary>
    /// Bonus to Might stat (affects melee damage and strength checks).
    /// </summary>
    public int Might { get; }

    /// <summary>
    /// Bonus to Agility stat (affects accuracy and evasion).
    /// </summary>
    public int Agility { get; }

    /// <summary>
    /// Bonus to Will stat (affects magic power and resistance).
    /// </summary>
    public int Will { get; }

    /// <summary>
    /// Bonus to Fortitude stat (affects health and physical resistance).
    /// </summary>
    public int Fortitude { get; }

    /// <summary>
    /// Bonus to Arcana stat (affects magical abilities and spell slots).
    /// </summary>
    public int Arcana { get; }

    /// <summary>
    /// Flat bonus to maximum health pool.
    /// </summary>
    public int BonusHealth { get; }

    /// <summary>
    /// Flat bonus to damage dealt by attacks.
    /// </summary>
    public int BonusDamage { get; }

    /// <summary>
    /// Flat bonus to defense rating.
    /// </summary>
    public int BonusDefense { get; }

    /// <summary>
    /// Gets an empty ItemStats instance with all values at zero.
    /// </summary>
    /// <value>A static instance representing no stat bonuses.</value>
    public static ItemStats Empty => new(0, 0, 0, 0, 0, 0, 0, 0);

    /// <summary>
    /// Private constructor for creating ItemStats instances.
    /// </summary>
    /// <param name="might">Bonus to Might stat.</param>
    /// <param name="agility">Bonus to Agility stat.</param>
    /// <param name="will">Bonus to Will stat.</param>
    /// <param name="fortitude">Bonus to Fortitude stat.</param>
    /// <param name="arcana">Bonus to Arcana stat.</param>
    /// <param name="bonusHealth">Flat bonus to maximum health.</param>
    /// <param name="bonusDamage">Flat bonus to damage dealt.</param>
    /// <param name="bonusDefense">Flat bonus to defense rating.</param>
    private ItemStats(
        int might,
        int agility,
        int will,
        int fortitude,
        int arcana,
        int bonusHealth,
        int bonusDamage,
        int bonusDefense)
    {
        Might = might;
        Agility = agility;
        Will = will;
        Fortitude = fortitude;
        Arcana = arcana;
        BonusHealth = bonusHealth;
        BonusDamage = bonusDamage;
        BonusDefense = bonusDefense;
    }

    /// <summary>
    /// Creates a new ItemStats instance with the specified stat bonuses.
    /// </summary>
    /// <param name="might">Bonus to Might stat. Default is 0.</param>
    /// <param name="agility">Bonus to Agility stat. Default is 0.</param>
    /// <param name="will">Bonus to Will stat. Default is 0.</param>
    /// <param name="fortitude">Bonus to Fortitude stat. Default is 0.</param>
    /// <param name="arcana">Bonus to Arcana stat. Default is 0.</param>
    /// <param name="bonusHealth">Flat bonus to maximum health. Default is 0.</param>
    /// <param name="bonusDamage">Flat bonus to damage dealt. Default is 0.</param>
    /// <param name="bonusDefense">Flat bonus to defense rating. Default is 0.</param>
    /// <returns>A new ItemStats instance with the specified values.</returns>
    /// <remarks>
    /// All stats can be negative to represent cursed items or debuff effects.
    /// No upper bound validation is performed - configuration controls limits.
    /// </remarks>
    public static ItemStats Create(
        int might = 0,
        int agility = 0,
        int will = 0,
        int fortitude = 0,
        int arcana = 0,
        int bonusHealth = 0,
        int bonusDamage = 0,
        int bonusDefense = 0)
    {
        return new ItemStats(
            might,
            agility,
            will,
            fortitude,
            arcana,
            bonusHealth,
            bonusDamage,
            bonusDefense);
    }

    /// <summary>
    /// Gets a value indicating whether all stats are zero.
    /// </summary>
    /// <value><c>true</c> if all stat values are zero; otherwise, <c>false</c>.</value>
    public bool IsEmpty =>
        Might == 0 &&
        Agility == 0 &&
        Will == 0 &&
        Fortitude == 0 &&
        Arcana == 0 &&
        BonusHealth == 0 &&
        BonusDamage == 0 &&
        BonusDefense == 0;

    /// <summary>
    /// Gets the count of non-zero stat values.
    /// </summary>
    /// <value>The number of stats with non-zero values.</value>
    public int NonZeroStatCount
    {
        get
        {
            var count = 0;
            if (Might != 0) count++;
            if (Agility != 0) count++;
            if (Will != 0) count++;
            if (Fortitude != 0) count++;
            if (Arcana != 0) count++;
            if (BonusHealth != 0) count++;
            if (BonusDamage != 0) count++;
            if (BonusDefense != 0) count++;
            return count;
        }
    }

    /// <summary>
    /// Returns a formatted string representation of the stat bonuses.
    /// </summary>
    /// <returns>A string showing all stat values in a compact format.</returns>
    public override string ToString() =>
        $"Stats[M:{Might} A:{Agility} W:{Will} F:{Fortitude} Arc:{Arcana} " +
        $"HP:{BonusHealth} DMG:{BonusDamage} DEF:{BonusDefense}]";
}
