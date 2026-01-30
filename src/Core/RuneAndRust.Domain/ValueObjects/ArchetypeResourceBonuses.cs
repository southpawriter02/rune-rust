// ═══════════════════════════════════════════════════════════════════════════════
// ArchetypeResourceBonuses.cs
// Value object encapsulating the resource pool bonuses provided by an archetype.
// Each archetype has a distinct bonus profile that modifies HP, Stamina, Aether
// Pool, Movement, and optionally grants a unique special bonus. These bonuses
// are applied during character creation and affect derived statistics.
// Version: 0.17.3b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using Microsoft.Extensions.Logging;

/// <summary>
/// Encapsulates the resource pool bonuses provided by an archetype.
/// </summary>
/// <remarks>
/// <para>
/// ArchetypeResourceBonuses is an immutable value object that defines how an
/// archetype modifies a character's resource pools. These bonuses are applied
/// during character creation and affect derived statistics calculated by the
/// DerivedStatCalculator service.
/// </para>
/// <para>
/// Four archetypes have distinct bonus profiles reflecting their combat roles:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <b>Warrior</b> — Highest HP (+49), moderate Stamina (+5). The Warrior's
///       massive HP bonus reflects their role as the primary tank archetype,
///       absorbing damage on the frontline.
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>Skirmisher</b> — Moderate HP (+30), Stamina (+5), Movement (+1).
///       The Skirmisher trades raw HP for mobility, supporting hit-and-run tactics
///       and evasive combat.
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>Mystic</b> — Lower HP (+20), highest Aether Pool (+20). The Mystic
///       sacrifices physical durability for Aether capacity, enabling sustained
///       magical combat from range.
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>Adept</b> — Moderate HP (+30), unique special bonus (+20% Consumables).
///       The Adept compensates for lower combat bonuses with enhanced consumable
///       effectiveness, reflecting their preparation-focused playstyle.
///     </description>
///   </item>
/// </list>
/// <para>
/// Use the static properties (<see cref="Warrior"/>, <see cref="Skirmisher"/>,
/// <see cref="Mystic"/>, <see cref="Adept"/>) for canonical bonus profiles,
/// the <see cref="Create"/> factory method for validated custom creation, or
/// <see cref="None"/> for an empty bonus set.
/// </para>
/// </remarks>
/// <param name="MaxHpBonus">
/// Bonus added to maximum HP calculation. Ranges from +20 (Mystic) to +49 (Warrior).
/// Applied additively to the base HP formula: (STURDINESS × 10) + 50 + ArchetypeBonus + LineageBonus.
/// </param>
/// <param name="MaxStaminaBonus">
/// Bonus added to maximum Stamina calculation. Either +5 (Warrior, Skirmisher) or 0 (Mystic, Adept).
/// Applied additively to the Stamina formula: (FINESSE × 5) + (MIGHT × 5) + 20 + ArchetypeBonus.
/// </param>
/// <param name="MaxAetherPoolBonus">
/// Bonus added to maximum Aether Pool calculation. Only Mystic receives +20.
/// Applied additively to the AP formula: (WILL × 10) + (WITS × 5) + ArchetypeBonus + LineageBonus.
/// </param>
/// <param name="MovementBonus">
/// Bonus added to base movement speed. Only Skirmisher receives +1.
/// Applied additively to base movement (5 tiles).
/// </param>
/// <param name="SpecialBonus">
/// Optional unique bonus for special effects that cannot be expressed as
/// simple numeric bonuses. Only Adept uses this (+20% ConsumableEffectiveness).
/// </param>
/// <seealso cref="ArchetypeSpecialBonus"/>
/// <seealso cref="RuneAndRust.Domain.Enums.Archetype"/>
/// <seealso cref="RuneAndRust.Domain.Entities.ArchetypeDefinition"/>
/// <seealso cref="DerivedStats"/>
public readonly record struct ArchetypeResourceBonuses(
    int MaxHpBonus,
    int MaxStaminaBonus,
    int MaxAetherPoolBonus,
    int MovementBonus,
    ArchetypeSpecialBonus? SpecialBonus = null)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Logger for detailed diagnostic output during resource bonus creation
    /// and access.
    /// </summary>
    private static ILogger<ArchetypeResourceBonuses>? _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this archetype has any HP bonus.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="MaxHpBonus"/> is greater than 0;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// All four archetypes have a positive HP bonus (Warrior: +49,
    /// Skirmisher: +30, Mystic: +20, Adept: +30). This property
    /// returns <c>false</c> only for <see cref="None"/> or custom
    /// configurations with zero HP bonus.
    /// </remarks>
    public bool HasHpBonus => MaxHpBonus > 0;

    /// <summary>
    /// Gets whether this archetype has any Stamina bonus.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="MaxStaminaBonus"/> is greater than 0;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Only Warrior (+5) and Skirmisher (+5) have Stamina bonuses.
    /// Mystic and Adept rely on other resource pools.
    /// </remarks>
    public bool HasStaminaBonus => MaxStaminaBonus > 0;

    /// <summary>
    /// Gets whether this archetype has any Aether Pool bonus.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="MaxAetherPoolBonus"/> is greater than 0;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Only Mystic (+20) has an Aether Pool bonus, reflecting their
    /// exclusive use of the AetherPool resource type for magical abilities.
    /// </remarks>
    public bool HasAetherPoolBonus => MaxAetherPoolBonus > 0;

    /// <summary>
    /// Gets whether this archetype has any Movement bonus.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="MovementBonus"/> is greater than 0;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Only Skirmisher (+1) has a Movement bonus, supporting their
    /// hit-and-run playstyle and tactical mobility advantage.
    /// </remarks>
    public bool HasMovementBonus => MovementBonus > 0;

    /// <summary>
    /// Gets whether this archetype has a special bonus.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="SpecialBonus"/> has a value;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Only Adept has a special bonus (+20% ConsumableEffectiveness).
    /// Warrior, Skirmisher, and Mystic have <c>null</c> special bonuses.
    /// </remarks>
    public bool HasSpecialBonus => SpecialBonus.HasValue;

    /// <summary>
    /// Gets the total resource points allocated (HP + Stamina + AP).
    /// </summary>
    /// <value>
    /// The sum of <see cref="MaxHpBonus"/>, <see cref="MaxStaminaBonus"/>,
    /// and <see cref="MaxAetherPoolBonus"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// Useful for balance comparison across archetypes. Movement and Special
    /// bonuses are not included as they represent different value types that
    /// cannot be directly compared to resource pool points.
    /// </para>
    /// <para>
    /// Total resource bonuses by archetype:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Warrior: 54 (49 + 5 + 0)</description></item>
    ///   <item><description>Skirmisher: 35 (30 + 5 + 0) — compensated by +1 Movement</description></item>
    ///   <item><description>Mystic: 40 (20 + 0 + 20)</description></item>
    ///   <item><description>Adept: 30 (30 + 0 + 0) — compensated by +20% Consumables</description></item>
    /// </list>
    /// </remarks>
    public int TotalResourceBonus => MaxHpBonus + MaxStaminaBonus + MaxAetherPoolBonus;

    // ═══════════════════════════════════════════════════════════════════════════
    // STATIC PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// An empty resource bonus with all values at zero.
    /// </summary>
    /// <value>
    /// An <see cref="ArchetypeResourceBonuses"/> with all numeric bonuses set to 0
    /// and no special bonus.
    /// </value>
    /// <remarks>
    /// Useful as a default value or for testing. All <c>Has*</c> properties
    /// return <c>false</c> and <see cref="TotalResourceBonus"/> returns 0.
    /// </remarks>
    public static ArchetypeResourceBonuses None => new(0, 0, 0, 0, null);

    /// <summary>
    /// Creates resource bonuses for the Warrior archetype.
    /// </summary>
    /// <value>
    /// MaxHpBonus: +49, MaxStaminaBonus: +5, MaxAetherPoolBonus: 0,
    /// MovementBonus: 0, SpecialBonus: null.
    /// </value>
    /// <remarks>
    /// The Warrior has the highest HP bonus (+49) in the game, reflecting their
    /// role as the primary tank. Combined with +5 Stamina, Warriors have the
    /// highest total resource bonus (54) but no Movement or Special bonuses.
    /// </remarks>
    public static ArchetypeResourceBonuses Warrior => new(49, 5, 0, 0, null);

    /// <summary>
    /// Creates resource bonuses for the Skirmisher archetype.
    /// </summary>
    /// <value>
    /// MaxHpBonus: +30, MaxStaminaBonus: +5, MaxAetherPoolBonus: 0,
    /// MovementBonus: +1, SpecialBonus: null.
    /// </value>
    /// <remarks>
    /// The Skirmisher trades Warrior-level HP for +1 Movement, enabling
    /// hit-and-run tactics. The Movement bonus stacks with the base movement
    /// of 5 tiles and any lineage bonuses (e.g., Vargr-Kin +1).
    /// </remarks>
    public static ArchetypeResourceBonuses Skirmisher => new(30, 5, 0, 1, null);

    /// <summary>
    /// Creates resource bonuses for the Mystic archetype.
    /// </summary>
    /// <value>
    /// MaxHpBonus: +20, MaxStaminaBonus: 0, MaxAetherPoolBonus: +20,
    /// MovementBonus: 0, SpecialBonus: null.
    /// </value>
    /// <remarks>
    /// The Mystic has the lowest HP bonus (+20) but the highest Aether Pool
    /// bonus (+20), reflecting their reliance on magical abilities over physical
    /// durability. They are the only archetype with an Aether Pool bonus.
    /// </remarks>
    public static ArchetypeResourceBonuses Mystic => new(20, 0, 20, 0, null);

    /// <summary>
    /// Creates resource bonuses for the Adept archetype.
    /// </summary>
    /// <value>
    /// MaxHpBonus: +30, MaxStaminaBonus: 0, MaxAetherPoolBonus: 0,
    /// MovementBonus: 0, SpecialBonus: +20% ConsumableEffectiveness.
    /// </value>
    /// <remarks>
    /// <para>
    /// The Adept has the lowest total resource bonus (30) but compensates with
    /// a unique +20% consumable effectiveness bonus. This means all consumable
    /// items (healing potions, buff elixirs, etc.) are 20% more effective when
    /// used by an Adept character.
    /// </para>
    /// <para>
    /// The special bonus is the only non-null <see cref="ArchetypeSpecialBonus"/>
    /// among the four archetypes.
    /// </para>
    /// </remarks>
    public static ArchetypeResourceBonuses Adept => new(
        30, 0, 0, 0,
        new ArchetypeSpecialBonus(
            ArchetypeSpecialBonus.ConsumableEffectivenessType,
            0.20f,
            "+20% effectiveness from all consumable items"));

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a custom resource bonus configuration with validation.
    /// </summary>
    /// <param name="maxHpBonus">
    /// HP bonus value. Must be non-negative. Typical range: 0-49.
    /// </param>
    /// <param name="maxStaminaBonus">
    /// Stamina bonus value. Must be non-negative. Typical range: 0-5.
    /// </param>
    /// <param name="maxAetherPoolBonus">
    /// Aether Pool bonus value. Must be non-negative. Typical range: 0-20.
    /// </param>
    /// <param name="movementBonus">
    /// Movement bonus value. Must be non-negative. Typical range: 0-1.
    /// </param>
    /// <param name="specialBonus">
    /// Optional special bonus. Pass <c>null</c> for archetypes without
    /// special effects.
    /// </param>
    /// <param name="logger">Optional logger for diagnostic output during creation.</param>
    /// <returns>A new <see cref="ArchetypeResourceBonuses"/> instance with validated data.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when any numeric bonus value is negative.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This factory method validates all numeric parameters to ensure they
    /// are non-negative. Negative bonuses are not supported — resource
    /// penalties are handled through separate game mechanics.
    /// </para>
    /// <para>
    /// For standard archetype bonuses, prefer using the static properties
    /// (<see cref="Warrior"/>, <see cref="Skirmisher"/>, <see cref="Mystic"/>,
    /// <see cref="Adept"/>) which return pre-validated canonical values.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create custom bonuses with validation
    /// var bonuses = ArchetypeResourceBonuses.Create(49, 5, 0, 0);
    ///
    /// // Create with special bonus
    /// var specialBonus = ArchetypeSpecialBonus.CreateConsumableEffectiveness(20);
    /// var adeptBonuses = ArchetypeResourceBonuses.Create(30, 0, 0, 0, specialBonus);
    ///
    /// // This throws ArgumentOutOfRangeException:
    /// // var invalid = ArchetypeResourceBonuses.Create(-10, 0, 0, 0);
    /// </code>
    /// </example>
    public static ArchetypeResourceBonuses Create(
        int maxHpBonus,
        int maxStaminaBonus,
        int maxAetherPoolBonus,
        int movementBonus,
        ArchetypeSpecialBonus? specialBonus = null,
        ILogger<ArchetypeResourceBonuses>? logger = null)
    {
        _logger = logger;

        _logger?.LogDebug(
            "Creating ArchetypeResourceBonuses. MaxHpBonus={MaxHpBonus}, " +
            "MaxStaminaBonus={MaxStaminaBonus}, MaxAetherPoolBonus={MaxAetherPoolBonus}, " +
            "MovementBonus={MovementBonus}, HasSpecialBonus={HasSpecialBonus}",
            maxHpBonus,
            maxStaminaBonus,
            maxAetherPoolBonus,
            movementBonus,
            specialBonus.HasValue);

        // Validate HP bonus is non-negative — resource bonuses cannot be penalties
        ArgumentOutOfRangeException.ThrowIfNegative(maxHpBonus);

        // Validate Stamina bonus is non-negative
        ArgumentOutOfRangeException.ThrowIfNegative(maxStaminaBonus);

        // Validate Aether Pool bonus is non-negative
        ArgumentOutOfRangeException.ThrowIfNegative(maxAetherPoolBonus);

        // Validate Movement bonus is non-negative
        ArgumentOutOfRangeException.ThrowIfNegative(movementBonus);

        _logger?.LogDebug(
            "Validation passed for ArchetypeResourceBonuses. MaxHpBonus={MaxHpBonus}, " +
            "MaxStaminaBonus={MaxStaminaBonus}, MaxAetherPoolBonus={MaxAetherPoolBonus}, " +
            "MovementBonus={MovementBonus}",
            maxHpBonus,
            maxStaminaBonus,
            maxAetherPoolBonus,
            movementBonus);

        var bonuses = new ArchetypeResourceBonuses(
            maxHpBonus,
            maxStaminaBonus,
            maxAetherPoolBonus,
            movementBonus,
            specialBonus);

        _logger?.LogInformation(
            "Created ArchetypeResourceBonuses. MaxHpBonus={MaxHpBonus}, " +
            "MaxStaminaBonus={MaxStaminaBonus}, MaxAetherPoolBonus={MaxAetherPoolBonus}, " +
            "MovementBonus={MovementBonus}, TotalResourceBonus={TotalResourceBonus}, " +
            "HasSpecialBonus={HasSpecialBonus}",
            bonuses.MaxHpBonus,
            bonuses.MaxStaminaBonus,
            bonuses.MaxAetherPoolBonus,
            bonuses.MovementBonus,
            bonuses.TotalResourceBonus,
            bonuses.HasSpecialBonus);

        return bonuses;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a formatted summary of the bonuses for display.
    /// </summary>
    /// <returns>
    /// A comma-separated string listing all active bonuses
    /// (e.g., "+49 HP, +5 Stamina" for Warrior, or "No bonuses" for <see cref="None"/>).
    /// </returns>
    /// <remarks>
    /// <para>
    /// Only non-zero bonuses are included in the summary. The special bonus
    /// description is included verbatim if present. This format is designed
    /// for the character creation UI archetype comparison panel.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var warrior = ArchetypeResourceBonuses.Warrior;
    /// warrior.GetDisplaySummary(); // "+49 HP, +5 Stamina"
    ///
    /// var adept = ArchetypeResourceBonuses.Adept;
    /// adept.GetDisplaySummary(); // "+30 HP, +20% effectiveness from all consumable items"
    ///
    /// var none = ArchetypeResourceBonuses.None;
    /// none.GetDisplaySummary(); // "No bonuses"
    /// </code>
    /// </example>
    public string GetDisplaySummary()
    {
        var parts = new List<string>();

        if (HasHpBonus)
            parts.Add($"+{MaxHpBonus} HP");
        if (HasStaminaBonus)
            parts.Add($"+{MaxStaminaBonus} Stamina");
        if (HasAetherPoolBonus)
            parts.Add($"+{MaxAetherPoolBonus} Aether Pool");
        if (HasMovementBonus)
            parts.Add($"+{MovementBonus} Movement");
        if (HasSpecialBonus)
            parts.Add(SpecialBonus!.Value.Description);

        return parts.Count > 0 ? string.Join(", ", parts) : "No bonuses";
    }

    /// <summary>
    /// Gets the special bonus description if present.
    /// </summary>
    /// <returns>
    /// The <see cref="ArchetypeSpecialBonus.Description"/> string if a special
    /// bonus exists, or <c>null</c> if no special bonus is defined.
    /// </returns>
    /// <remarks>
    /// Convenience method for UI elements that need to display only the special
    /// bonus information separately from the standard resource bonuses.
    /// </remarks>
    public string? GetSpecialBonusDescription() =>
        SpecialBonus?.Description;

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a formatted string representation of all resource bonuses.
    /// </summary>
    /// <returns>
    /// A string in the format "HP+{n}, Stam+{n}, AP+{n}, Mov+{n}" with an
    /// optional ", Special: {BonusType}" suffix if a special bonus is present
    /// (e.g., "HP+49, Stam+5, AP+0, Mov+0" for Warrior).
    /// </returns>
    public override string ToString() =>
        $"HP+{MaxHpBonus}, Stam+{MaxStaminaBonus}, AP+{MaxAetherPoolBonus}, Mov+{MovementBonus}" +
        (HasSpecialBonus ? $", Special: {SpecialBonus!.Value.BonusType}" : "");
}
