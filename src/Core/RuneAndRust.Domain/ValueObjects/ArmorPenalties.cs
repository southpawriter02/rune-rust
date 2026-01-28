// ═══════════════════════════════════════════════════════════════════════════════
// ArmorPenalties.cs
// Value object containing base penalties for an armor category.
// Version: 0.16.2b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Contains base penalties for an armor category.
/// </summary>
/// <remarks>
/// <para>
/// ArmorPenalties defines the agility dice reduction, stamina cost increase,
/// movement penalty, and stealth disadvantage for an armor category.
/// </para>
/// <para>
/// These base values are modified by proficiency level:
/// <list type="bullet">
///   <item><description>NonProficient: Penalties doubled (2.0x multiplier)</description></item>
///   <item><description>Proficient: Normal penalties (1.0x multiplier)</description></item>
///   <item><description>Expert/Master: May use lower tier penalties via tier reduction</description></item>
/// </list>
/// </para>
/// <para>
/// The penalty matrix for standard armor categories:
/// <list type="table">
///   <listheader>
///     <term>Category</term>
///     <description>Agility | Stamina | Movement | Stealth</description>
///   </listheader>
///   <item>
///     <term>Light</term>
///     <description>0 | 0 | 0 | No</description>
///   </item>
///   <item>
///     <term>Medium</term>
///     <description>-1d10 | +2 | -5 ft | Yes</description>
///   </item>
///   <item>
///     <term>Heavy</term>
///     <description>-2d10 | +5 | -10 ft | Yes</description>
///   </item>
///   <item>
///     <term>Shields</term>
///     <description>0 | +1 | 0 | No</description>
///   </item>
/// </list>
/// </para>
/// </remarks>
/// <param name="AgilityDicePenalty">
/// Dice removed from Agility-based rolls (0, -1, or -2).
/// Negative values indicate dice removed from the pool.
/// </param>
/// <param name="StaminaCostModifier">
/// Additional Stamina cost per combat action (0, +1, +2, or +5).
/// Positive values increase the cost of actions.
/// </param>
/// <param name="MovementPenalty">
/// Movement speed reduction in feet (0, -5, or -10).
/// Negative values reduce movement per round.
/// </param>
/// <param name="HasStealthDisadvantage">
/// Whether stealth checks have disadvantage when wearing this armor.
/// </param>
/// <seealso cref="Enums.ArmorCategory"/>
/// <seealso cref="ArmorCategoryDefinition"/>
public readonly record struct ArmorPenalties(
    int AgilityDicePenalty,
    int StaminaCostModifier,
    int MovementPenalty,
    bool HasStealthDisadvantage)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Static Instances
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets penalties representing no hindrance (Light armor).
    /// </summary>
    /// <value>
    /// An <see cref="ArmorPenalties"/> instance with all zero penalties
    /// and <see cref="HasStealthDisadvantage"/> set to <c>false</c>.
    /// </value>
    /// <remarks>
    /// Use this instance when creating Light armor definitions or when
    /// clearing penalties through tier reduction.
    /// </remarks>
    /// <example>
    /// <code>
    /// var lightArmorPenalties = ArmorPenalties.None;
    /// Console.WriteLine(lightArmorPenalties.HasAnyPenalty); // Output: False
    /// </code>
    /// </example>
    public static ArmorPenalties None => new(0, 0, 0, false);

    // ═══════════════════════════════════════════════════════════════════════════
    // Derived Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this has any agility penalty.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="AgilityDicePenalty"/> is less than 0; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// An agility penalty indicates dice are removed from Agility-based skill checks
    /// such as Acrobatics, Sleight of Hand, and Stealth (in addition to disadvantage).
    /// </remarks>
    public bool HasAgilityPenalty => AgilityDicePenalty < 0;

    /// <summary>
    /// Gets whether this has any stamina cost increase.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="StaminaCostModifier"/> is greater than 0; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// A stamina cost indicates additional stamina must be spent per combat action
    /// while wearing this armor category.
    /// </remarks>
    public bool HasStaminaCost => StaminaCostModifier > 0;

    /// <summary>
    /// Gets whether this has any movement penalty.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="MovementPenalty"/> is less than 0; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// A movement penalty reduces the character's base movement speed per round,
    /// affecting tactical positioning in combat.
    /// </remarks>
    public bool HasMovementPenalty => MovementPenalty < 0;

    /// <summary>
    /// Gets whether this category has any penalties at all.
    /// </summary>
    /// <value>
    /// <c>true</c> if any of <see cref="HasAgilityPenalty"/>, <see cref="HasStaminaCost"/>,
    /// <see cref="HasMovementPenalty"/>, or <see cref="HasStealthDisadvantage"/> is <c>true</c>;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Light armor typically returns <c>false</c> for this property.
    /// Useful for UI display decisions (whether to show penalty section).
    /// </remarks>
    /// <example>
    /// <code>
    /// var lightPenalties = ArmorPenalties.None;
    /// var heavyPenalties = ArmorPenalties.Create(-2, 5, -10, true);
    /// 
    /// Console.WriteLine(lightPenalties.HasAnyPenalty); // Output: False
    /// Console.WriteLine(heavyPenalties.HasAnyPenalty); // Output: True
    /// </code>
    /// </example>
    public bool HasAnyPenalty =>
        HasAgilityPenalty || HasStaminaCost || HasMovementPenalty || HasStealthDisadvantage;

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates ArmorPenalties with validation.
    /// </summary>
    /// <param name="agilityDicePenalty">
    /// Dice removed from Agility rolls. Must be between -10 and 0 (inclusive).
    /// </param>
    /// <param name="staminaCostModifier">
    /// Additional Stamina per action. Must be between 0 and 20 (inclusive).
    /// </param>
    /// <param name="movementPenalty">
    /// Movement speed reduction in feet. Must be between -30 and 0 (inclusive).
    /// </param>
    /// <param name="hasStealthDisadvantage">
    /// Whether stealth checks have disadvantage.
    /// </param>
    /// <returns>A new validated <see cref="ArmorPenalties"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when any numeric parameter is outside its valid range.
    /// </exception>
    /// <example>
    /// <code>
    /// // Create Heavy armor penalties
    /// var heavyPenalties = ArmorPenalties.Create(-2, 5, -10, true);
    /// 
    /// // Create Medium armor penalties
    /// var mediumPenalties = ArmorPenalties.Create(-1, 2, -5, true);
    /// 
    /// // Create Shield penalties
    /// var shieldPenalties = ArmorPenalties.Create(0, 1, 0, false);
    /// </code>
    /// </example>
    public static ArmorPenalties Create(
        int agilityDicePenalty,
        int staminaCostModifier,
        int movementPenalty,
        bool hasStealthDisadvantage)
    {
        // Validate agility penalty range (0 to -10 dice)
        ArgumentOutOfRangeException.ThrowIfGreaterThan(agilityDicePenalty, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(agilityDicePenalty, -10);

        // Validate stamina cost modifier range (0 to +20)
        ArgumentOutOfRangeException.ThrowIfNegative(staminaCostModifier);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(staminaCostModifier, 20);

        // Validate movement penalty range (0 to -30 feet)
        ArgumentOutOfRangeException.ThrowIfGreaterThan(movementPenalty, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(movementPenalty, -30);

        return new ArmorPenalties(
            agilityDicePenalty,
            staminaCostModifier,
            movementPenalty,
            hasStealthDisadvantage);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Utility Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns penalties with values multiplied by a factor (for non-proficiency).
    /// </summary>
    /// <param name="multiplier">
    /// The multiplier to apply (e.g., 2.0 for doubled penalties when non-proficient).
    /// </param>
    /// <returns>
    /// New <see cref="ArmorPenalties"/> with multiplied numerical values.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method multiplies the numerical penalties (agility, stamina, movement)
    /// but does NOT multiply the stealth disadvantage (it remains boolean).
    /// </para>
    /// <para>
    /// Used for calculating non-proficiency penalties where all base penalties
    /// are doubled (multiplier = 2.0).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var basePenalties = ArmorPenalties.Create(-2, 5, -10, true);
    /// var doubledPenalties = basePenalties.Multiply(2.0m);
    /// 
    /// Console.WriteLine(doubledPenalties.AgilityDicePenalty);  // Output: -4
    /// Console.WriteLine(doubledPenalties.StaminaCostModifier); // Output: 10
    /// Console.WriteLine(doubledPenalties.MovementPenalty);     // Output: -20
    /// Console.WriteLine(doubledPenalties.HasStealthDisadvantage); // Output: True (unchanged)
    /// </code>
    /// </example>
    public ArmorPenalties Multiply(decimal multiplier)
    {
        return new ArmorPenalties(
            (int)(AgilityDicePenalty * multiplier),
            (int)(StaminaCostModifier * multiplier),
            (int)(MovementPenalty * multiplier),
            HasStealthDisadvantage);  // Stealth disadvantage is not multiplied (boolean)
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Formatting Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats agility penalty for display.
    /// </summary>
    /// <returns>
    /// A formatted string showing the agility dice penalty (e.g., "-2d10") or "None".
    /// </returns>
    /// <remarks>
    /// Uses the d10 dice notation consistent with the game's dice pool system.
    /// </remarks>
    /// <example>
    /// <code>
    /// var heavyPenalties = ArmorPenalties.Create(-2, 5, -10, true);
    /// var lightPenalties = ArmorPenalties.None;
    /// 
    /// Console.WriteLine(heavyPenalties.FormatAgilityPenalty()); // Output: "-2d10"
    /// Console.WriteLine(lightPenalties.FormatAgilityPenalty()); // Output: "None"
    /// </code>
    /// </example>
    public string FormatAgilityPenalty() =>
        AgilityDicePenalty == 0 ? "None" : $"{AgilityDicePenalty}d10";

    /// <summary>
    /// Formats stamina cost for display.
    /// </summary>
    /// <returns>
    /// A formatted string showing the stamina cost modifier (e.g., "+5") or "None".
    /// </returns>
    /// <remarks>
    /// Uses a leading plus sign for positive values to indicate additional cost.
    /// </remarks>
    /// <example>
    /// <code>
    /// var heavyPenalties = ArmorPenalties.Create(-2, 5, -10, true);
    /// var lightPenalties = ArmorPenalties.None;
    /// 
    /// Console.WriteLine(heavyPenalties.FormatStaminaCost()); // Output: "+5"
    /// Console.WriteLine(lightPenalties.FormatStaminaCost()); // Output: "None"
    /// </code>
    /// </example>
    public string FormatStaminaCost() =>
        StaminaCostModifier == 0 ? "None" : $"+{StaminaCostModifier}";

    /// <summary>
    /// Formats movement penalty for display.
    /// </summary>
    /// <returns>
    /// A formatted string showing the movement reduction (e.g., "-10 ft") or "None".
    /// </returns>
    /// <remarks>
    /// Uses feet as the unit of measure consistent with the game's grid system.
    /// </remarks>
    /// <example>
    /// <code>
    /// var heavyPenalties = ArmorPenalties.Create(-2, 5, -10, true);
    /// var lightPenalties = ArmorPenalties.None;
    /// 
    /// Console.WriteLine(heavyPenalties.FormatMovementPenalty()); // Output: "-10 ft"
    /// Console.WriteLine(lightPenalties.FormatMovementPenalty()); // Output: "None"
    /// </code>
    /// </example>
    public string FormatMovementPenalty() =>
        MovementPenalty == 0 ? "None" : $"{MovementPenalty} ft";

    /// <summary>
    /// Creates a display string for debug/logging.
    /// </summary>
    /// <returns>
    /// A formatted string summarizing all penalty values.
    /// </returns>
    /// <remarks>
    /// Provides a compact representation suitable for logging and debugging.
    /// For structured logging, prefer individual property access.
    /// </remarks>
    /// <example>
    /// <code>
    /// var heavyPenalties = ArmorPenalties.Create(-2, 5, -10, true);
    /// Console.WriteLine(heavyPenalties.ToString());
    /// // Output: "Agility: -2d10, Stamina: +5, Movement: -10 ft, Stealth: Disadvantage"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"Agility: {FormatAgilityPenalty()}, Stamina: {FormatStaminaCost()}, " +
        $"Movement: {FormatMovementPenalty()}, Stealth: {(HasStealthDisadvantage ? "Disadvantage" : "Normal")}";

    /// <summary>
    /// Creates a detailed debug string with all property values.
    /// </summary>
    /// <returns>
    /// A verbose string representation suitable for detailed debugging.
    /// </returns>
    /// <example>
    /// <code>
    /// var heavyPenalties = ArmorPenalties.Create(-2, 5, -10, true);
    /// Console.WriteLine(heavyPenalties.ToDebugString());
    /// // Output: "ArmorPenalties { Agility: -2, Stamina: +5, Move: -10, Stealth: True }"
    /// </code>
    /// </example>
    public string ToDebugString() =>
        $"ArmorPenalties {{ Agility: {AgilityDicePenalty}, Stamina: +{StaminaCostModifier}, " +
        $"Move: {MovementPenalty}, Stealth: {HasStealthDisadvantage} }}";
}
