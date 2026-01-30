// ═══════════════════════════════════════════════════════════════════════════════
// DerivedStats.cs
// Value object holding all calculated secondary statistics for a character.
// Derived stats are computed from core attributes, archetype bonuses, and
// lineage bonuses, representing the character's combat and resource capabilities.
// Version: 0.17.2d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using Microsoft.Extensions.Logging;

/// <summary>
/// Holds all derived (calculated) statistics for a character.
/// </summary>
/// <remarks>
/// <para>
/// DerivedStats is a value object that contains the seven secondary statistics
/// computed from a character's core attributes, archetype selection, and lineage.
/// These stats represent the character's combat effectiveness, resource pools,
/// and physical capabilities.
/// </para>
/// <para>
/// The seven derived stats are:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <b>MaxHp</b> — Maximum hit points, primarily scaled by Sturdiness with
///       archetype and lineage bonuses. Determines how much damage a character
///       can absorb before falling in combat.
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>MaxStamina</b> — Maximum stamina for physical actions, scaled by
///       Finesse and Might. Used by Warrior, Skirmisher, and Adept archetypes
///       to power physical abilities.
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>MaxAetherPool</b> — Maximum Aether capacity for magical abilities,
///       scaled by Will and Wits. Primary resource for Mystic archetype. Enhanced
///       by the Rune-Marked lineage's Aether-Tainted trait (×1.10 multiplier).
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>Initiative</b> — Combat turn order bonus, scaled by Finesse and Wits.
///       Higher initiative means acting earlier in combat rounds.
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>Soak</b> — Passive damage reduction, scaled by Sturdiness. Iron-Blooded
///       lineage gains +2 bonus from Hazard Acclimation trait.
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>MovementSpeed</b> — Base movement in tiles per turn. Vargr-Kin lineage
///       gains +1 from Primal Clarity trait.
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>CarryingCapacity</b> — Maximum weight that can be carried, scaled
///       by Might. Determines inventory limits.
///     </description>
///   </item>
/// </list>
/// <para>
/// DerivedStats instances are immutable value objects created through the
/// <see cref="Create"/> factory method, which validates all parameters to ensure
/// game-rule consistency. Use <see cref="GetSummary"/> for formatted display output.
/// </para>
/// </remarks>
/// <seealso cref="DerivedStatFormula"/>
/// <seealso cref="AttributeAllocationState"/>
/// <seealso cref="PointBuyConfiguration"/>
public readonly record struct DerivedStats
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Logger for detailed diagnostic output during derived stat creation and access.
    /// </summary>
    private static ILogger<DerivedStats>? _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the maximum hit points.
    /// </summary>
    /// <value>
    /// A positive integer representing the character's maximum HP.
    /// Calculated as: (STURDINESS × 10) + 50 + ArchetypeBonus + LineageBonus.
    /// </value>
    /// <remarks>
    /// <para>
    /// HP determines how much damage a character can sustain before falling.
    /// The formula provides a minimum baseline of 60 HP (base 50 + minimum
    /// Sturdiness of 1 × 10) before archetype and lineage bonuses.
    /// </para>
    /// <para>
    /// Archetype bonuses range from +20 (Mystic) to +49 (Warrior), reflecting
    /// the Warrior's role as the primary tank archetype.
    /// </para>
    /// </remarks>
    public int MaxHp { get; init; }

    /// <summary>
    /// Gets the maximum stamina for physical actions.
    /// </summary>
    /// <value>
    /// A non-negative integer representing the character's maximum Stamina pool.
    /// Calculated as: (FINESSE × 5) + (MIGHT × 5) + 20 + ArchetypeBonus.
    /// </value>
    /// <remarks>
    /// <para>
    /// Stamina powers physical abilities for Warrior, Skirmisher, and Adept
    /// archetypes. The dual-attribute scaling (Finesse + Might) means both
    /// agile and powerful characters benefit.
    /// </para>
    /// </remarks>
    public int MaxStamina { get; init; }

    /// <summary>
    /// Gets the maximum Aether pool for magical abilities.
    /// </summary>
    /// <value>
    /// A non-negative integer representing the character's maximum Aether capacity.
    /// Calculated as: (WILL × 10) + (WITS × 5) + ArchetypeBonus + LineageBonus,
    /// with an optional lineage multiplier (Rune-Marked: ×1.10).
    /// </value>
    /// <remarks>
    /// <para>
    /// The Aether Pool is the primary resource for Mystic archetype abilities.
    /// Will provides the strongest scaling (×10) while Wits provides secondary
    /// scaling (×5). The Rune-Marked lineage's Aether-Tainted trait applies a
    /// 10% multiplicative bonus after all additive bonuses.
    /// </para>
    /// </remarks>
    public int MaxAetherPool { get; init; }

    /// <summary>
    /// Gets the initiative bonus for combat turn order.
    /// </summary>
    /// <value>
    /// An integer representing the character's initiative modifier.
    /// Calculated as: FINESSE + (WITS ÷ 2) using integer division.
    /// </value>
    /// <remarks>
    /// <para>
    /// Initiative determines combat turn order. Higher values act first.
    /// Finesse provides the primary contribution while Wits provides a
    /// secondary bonus (halved, rounded down via integer division).
    /// </para>
    /// </remarks>
    public int Initiative { get; init; }

    /// <summary>
    /// Gets the soak value (passive damage reduction).
    /// </summary>
    /// <value>
    /// A non-negative integer representing flat damage reduction applied to
    /// incoming physical damage. Calculated as: (STURDINESS ÷ 2) + LineageBonus.
    /// </value>
    /// <remarks>
    /// <para>
    /// Soak reduces incoming damage by a flat amount. The Iron-Blooded lineage
    /// gains +2 Soak from the Hazard Acclimation trait, making them notably
    /// more resilient to physical damage.
    /// </para>
    /// </remarks>
    public int Soak { get; init; }

    /// <summary>
    /// Gets the base movement speed in tiles per turn.
    /// </summary>
    /// <value>
    /// A positive integer representing how many tiles the character can move
    /// per combat turn. Base value is 5, with the Vargr-Kin lineage gaining +1.
    /// </value>
    /// <remarks>
    /// <para>
    /// Movement speed determines tactical mobility in combat. The base of 5
    /// tiles ensures all characters have reasonable mobility, while the
    /// Vargr-Kin bonus reflects their primal connection and physical agility.
    /// </para>
    /// </remarks>
    public int MovementSpeed { get; init; }

    /// <summary>
    /// Gets the carrying capacity in weight units.
    /// </summary>
    /// <value>
    /// A non-negative integer representing maximum carryable weight.
    /// Calculated as: MIGHT × 10.
    /// </value>
    /// <remarks>
    /// <para>
    /// Carrying capacity determines how much equipment and loot a character
    /// can hold. It scales linearly with Might, making strength-focused
    /// characters better pack carriers.
    /// </para>
    /// </remarks>
    public int CarryingCapacity { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this stat block has non-zero combat stats (Initiative and Soak).
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="Initiative"/> is greater than 0 or
    /// <see cref="Soak"/> is greater than 0; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Combat stats affect turn order and damage mitigation. Most characters
    /// will have at least a positive Initiative from their Finesse attribute.
    /// </remarks>
    public bool HasCombatStats => Initiative > 0 || Soak > 0;

    /// <summary>
    /// Gets whether this stat block has non-zero resource pools (HP, Stamina, AP).
    /// </summary>
    /// <value>
    /// <c>true</c> if any of <see cref="MaxHp"/>, <see cref="MaxStamina"/>,
    /// or <see cref="MaxAetherPool"/> is greater than 0; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// All valid characters should have at least positive HP, so this property
    /// is primarily useful for validation checks.
    /// </remarks>
    public bool HasResourceStats => MaxHp > 0 || MaxStamina > 0 || MaxAetherPool > 0;

    /// <summary>
    /// Gets the total combined resource pool (HP + Stamina + Aether Pool).
    /// </summary>
    /// <value>
    /// The sum of <see cref="MaxHp"/>, <see cref="MaxStamina"/>,
    /// and <see cref="MaxAetherPool"/>.
    /// </value>
    /// <remarks>
    /// The total resource pool provides a rough measure of overall character
    /// endurance. Warriors tend to have the highest total due to their HP bonus,
    /// while Mystics compensate with higher Aether capacity.
    /// </remarks>
    public int TotalResourcePool => MaxHp + MaxStamina + MaxAetherPool;

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new <see cref="DerivedStats"/> with validation.
    /// </summary>
    /// <param name="maxHp">
    /// Maximum hit points. Must be greater than 0, since all characters
    /// must have at least some HP to exist in the game world.
    /// </param>
    /// <param name="maxStamina">
    /// Maximum stamina pool. Must be non-negative. Characters with no
    /// stamina-based abilities may have 0.
    /// </param>
    /// <param name="maxAetherPool">
    /// Maximum Aether capacity. Must be non-negative. Non-magical characters
    /// may have 0 Aether.
    /// </param>
    /// <param name="initiative">
    /// Initiative bonus for combat turn order. Can be any integer value,
    /// though negative values are unusual.
    /// </param>
    /// <param name="soak">
    /// Passive damage reduction. Must be non-negative.
    /// </param>
    /// <param name="movementSpeed">
    /// Base movement speed in tiles per turn. Must be greater than 0,
    /// since all characters must be able to move.
    /// </param>
    /// <param name="carryingCapacity">
    /// Maximum weight capacity. Must be non-negative.
    /// </param>
    /// <param name="logger">Optional logger for diagnostic output during creation.</param>
    /// <returns>A new <see cref="DerivedStats"/> instance with validated data.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="maxHp"/> or <paramref name="movementSpeed"/> is zero or negative,
    /// or when <paramref name="maxStamina"/>, <paramref name="maxAetherPool"/>,
    /// <paramref name="soak"/>, or <paramref name="carryingCapacity"/> is negative.
    /// </exception>
    /// <example>
    /// <code>
    /// // Create derived stats for a Warrior with standard build
    /// var stats = DerivedStats.Create(
    ///     maxHp: 139, maxStamina: 60, maxAetherPool: 30,
    ///     initiative: 4, soak: 2, movementSpeed: 5, carryingCapacity: 40);
    ///
    /// // stats.TotalResourcePool == 229 (139 + 60 + 30)
    /// // stats.HasCombatStats == true
    /// // stats.HasResourceStats == true
    /// </code>
    /// </example>
    /// <remarks>
    /// <para>
    /// This factory method validates all parameters before construction to ensure
    /// game-rule consistency. The validation rules enforce:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>HP must be positive (all characters need HP)</description></item>
    ///   <item><description>Movement speed must be positive (all characters can move)</description></item>
    ///   <item><description>Stamina, Aether, Soak, and Carrying Capacity must be non-negative</description></item>
    /// </list>
    /// </remarks>
    public static DerivedStats Create(
        int maxHp,
        int maxStamina,
        int maxAetherPool,
        int initiative,
        int soak,
        int movementSpeed,
        int carryingCapacity,
        ILogger<DerivedStats>? logger = null)
    {
        _logger = logger;

        _logger?.LogDebug(
            "Creating DerivedStats. MaxHp={MaxHp}, MaxStamina={MaxStamina}, " +
            "MaxAetherPool={MaxAetherPool}, Initiative={Initiative}, Soak={Soak}, " +
            "MovementSpeed={MovementSpeed}, CarryingCapacity={CarryingCapacity}",
            maxHp,
            maxStamina,
            maxAetherPool,
            initiative,
            soak,
            movementSpeed,
            carryingCapacity);

        // Validate HP must be positive — all characters need hit points
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxHp);

        // Validate Stamina must be non-negative — some builds may have 0
        ArgumentOutOfRangeException.ThrowIfNegative(maxStamina);

        // Validate Aether Pool must be non-negative — non-magical characters may have 0
        ArgumentOutOfRangeException.ThrowIfNegative(maxAetherPool);

        // Validate Soak must be non-negative — damage reduction cannot be negative
        ArgumentOutOfRangeException.ThrowIfNegative(soak);

        // Validate Movement Speed must be positive — all characters can move
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(movementSpeed);

        // Validate Carrying Capacity must be non-negative
        ArgumentOutOfRangeException.ThrowIfNegative(carryingCapacity);

        _logger?.LogDebug(
            "Validation passed for DerivedStats. MaxHp={MaxHp}, MaxStamina={MaxStamina}, " +
            "MaxAetherPool={MaxAetherPool}, Initiative={Initiative}, Soak={Soak}, " +
            "MovementSpeed={MovementSpeed}, CarryingCapacity={CarryingCapacity}",
            maxHp,
            maxStamina,
            maxAetherPool,
            initiative,
            soak,
            movementSpeed,
            carryingCapacity);

        var stats = new DerivedStats
        {
            MaxHp = maxHp,
            MaxStamina = maxStamina,
            MaxAetherPool = maxAetherPool,
            Initiative = initiative,
            Soak = soak,
            MovementSpeed = movementSpeed,
            CarryingCapacity = carryingCapacity
        };

        _logger?.LogInformation(
            "Created DerivedStats. MaxHp={MaxHp}, MaxStamina={MaxStamina}, " +
            "MaxAetherPool={MaxAetherPool}, Initiative={Initiative}, Soak={Soak}, " +
            "MovementSpeed={MovementSpeed}, CarryingCapacity={CarryingCapacity}, " +
            "TotalResourcePool={TotalResourcePool}",
            stats.MaxHp,
            stats.MaxStamina,
            stats.MaxAetherPool,
            stats.Initiative,
            stats.Soak,
            stats.MovementSpeed,
            stats.CarryingCapacity,
            stats.TotalResourcePool);

        return stats;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a formatted summary string for display in the UI.
    /// </summary>
    /// <returns>
    /// A single-line string with all derived stats separated by pipe characters,
    /// e.g., "HP: 139 | Stamina: 60 | AP: 30 | Init: 4 | Soak: 2 | Move: 5 | Carry: 40".
    /// </returns>
    /// <remarks>
    /// <para>
    /// This summary is designed for compact UI display during character creation,
    /// showing all seven stats in a single line with abbreviated labels.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var stats = DerivedStats.Create(139, 60, 30, 4, 2, 5, 40);
    /// var summary = stats.GetSummary();
    /// // "HP: 139 | Stamina: 60 | AP: 30 | Init: 4 | Soak: 2 | Move: 5 | Carry: 40"
    /// </code>
    /// </example>
    public string GetSummary() =>
        $"HP: {MaxHp} | Stamina: {MaxStamina} | AP: {MaxAetherPool} | " +
        $"Init: {Initiative} | Soak: {Soak} | Move: {MovementSpeed} | Carry: {CarryingCapacity}";

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a formatted string representation of all derived stats.
    /// </summary>
    /// <returns>
    /// A string in the format "DerivedStats [HP:139 ST:60 AP:30 Init:4 Soak:2 Move:5 Carry:40]"
    /// showing all seven stats with abbreviated labels for compact logging.
    /// </returns>
    public override string ToString() =>
        $"DerivedStats [HP:{MaxHp} ST:{MaxStamina} AP:{MaxAetherPool} " +
        $"Init:{Initiative} Soak:{Soak} Move:{MovementSpeed} Carry:{CarryingCapacity}]";
}
