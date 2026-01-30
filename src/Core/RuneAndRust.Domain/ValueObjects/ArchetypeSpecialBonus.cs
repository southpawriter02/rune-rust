// ═══════════════════════════════════════════════════════════════════════════════
// ArchetypeSpecialBonus.cs
// Value object representing a unique special bonus provided by an archetype
// that cannot be expressed as a simple numeric bonus to resource pools. These
// effects modify game mechanics in unique ways, such as the Adept's +20%
// consumable effectiveness bonus.
// Version: 0.17.3b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using Microsoft.Extensions.Logging;

/// <summary>
/// Represents a unique special bonus provided by an archetype.
/// </summary>
/// <remarks>
/// <para>
/// ArchetypeSpecialBonus captures archetype-specific effects that cannot be
/// expressed as simple numeric bonuses to resource pools. These effects
/// modify game mechanics in unique ways that differentiate archetypes beyond
/// their standard resource bonuses (HP, Stamina, Aether Pool, Movement).
/// </para>
/// <para>
/// Currently implemented special bonus types:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <b>ConsumableEffectiveness</b> — A percentage multiplier applied to all
///       consumable item effects. Used exclusively by the Adept archetype at +20%
///       (BonusValue = 0.20, Multiplier = 1.20). This reflects the Adept's mastery
///       of mundane resources and preparation-focused playstyle.
///     </description>
///   </item>
/// </list>
/// <para>
/// The <see cref="BonusValue"/> is a float representing a percentage increment:
/// </para>
/// <list type="bullet">
///   <item><description>0.20 = +20% (multiply base value by 1.20)</description></item>
///   <item><description>0.50 = +50% (multiply base value by 1.50)</description></item>
/// </list>
/// <para>
/// Use <see cref="Multiplier"/> to get the full multiplier (1 + BonusValue) for
/// calculations, or <see cref="ApplyTo(int)"/> / <see cref="ApplyTo(float)"/> to
/// apply the bonus directly to a base value.
/// </para>
/// <para>
/// ArchetypeSpecialBonus instances are immutable value objects. Use the
/// <see cref="CreateConsumableEffectiveness"/> factory method for the standard
/// consumable bonus, or <see cref="Create"/> for validated construction from
/// arbitrary data (e.g., configuration loading).
/// </para>
/// </remarks>
/// <param name="BonusType">
/// The type identifier for this bonus (e.g., "ConsumableEffectiveness").
/// Used to determine how the bonus is applied during gameplay calculations.
/// </param>
/// <param name="BonusValue">
/// The numeric value of the bonus as a decimal fraction (e.g., 0.20 for +20%).
/// Combined with a base of 1.0 to produce the <see cref="Multiplier"/>.
/// </param>
/// <param name="Description">
/// Human-readable description for UI display (e.g., "+20% effectiveness from
/// all consumable items"). Shown during character creation and on the character sheet.
/// </param>
/// <seealso cref="ArchetypeResourceBonuses"/>
/// <seealso cref="RuneAndRust.Domain.Enums.Archetype"/>
/// <seealso cref="RuneAndRust.Domain.Entities.ArchetypeDefinition"/>
public readonly record struct ArchetypeSpecialBonus(
    string BonusType,
    float BonusValue,
    string Description)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Logger for detailed diagnostic output during special bonus creation
    /// and application.
    /// </summary>
    private static ILogger<ArchetypeSpecialBonus>? _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// The bonus type identifier for consumable effectiveness multiplier.
    /// </summary>
    /// <value>
    /// The string "ConsumableEffectiveness", used to identify bonuses that
    /// increase the effectiveness of all consumable items.
    /// </value>
    /// <remarks>
    /// Currently only the Adept archetype uses this bonus type. The bonus
    /// value is 0.20 (+20%), meaning all consumable effects are multiplied
    /// by 1.20 when applied to an Adept character.
    /// </remarks>
    public const string ConsumableEffectivenessType = "ConsumableEffectiveness";

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this is a consumable effectiveness bonus.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="BonusType"/> equals
    /// <see cref="ConsumableEffectivenessType"/> (case-insensitive);
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Case-insensitive comparison is used to handle potential configuration
    /// variations. The canonical form is "ConsumableEffectiveness" (PascalCase).
    /// </remarks>
    public bool IsConsumableEffectiveness =>
        BonusType.Equals(ConsumableEffectivenessType, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the multiplier to apply to base values (1 + BonusValue).
    /// </summary>
    /// <value>
    /// A float representing the full multiplier. For a +20% bonus
    /// (BonusValue = 0.20), returns 1.20.
    /// </value>
    /// <remarks>
    /// <para>
    /// The multiplier combines the base value (1.0) with the bonus increment.
    /// Apply this multiplier to a base value to get the modified result:
    /// </para>
    /// <para>
    /// <c>modifiedValue = baseValue × Multiplier</c>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var bonus = ArchetypeSpecialBonus.CreateConsumableEffectiveness(20);
    /// // bonus.Multiplier == 1.20f
    /// // A healing potion that restores 50 HP would restore 60 HP for an Adept
    /// </code>
    /// </example>
    public float Multiplier => 1f + BonusValue;

    /// <summary>
    /// Gets the bonus as a percentage integer for display.
    /// </summary>
    /// <value>
    /// An integer percentage value. For BonusValue = 0.20, returns 20.
    /// </value>
    /// <remarks>
    /// This property provides a human-friendly integer representation of the
    /// bonus percentage, suitable for UI display strings like "+20%".
    /// </remarks>
    public int PercentageValue => (int)(BonusValue * 100);

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a consumable effectiveness bonus with the specified percentage.
    /// </summary>
    /// <param name="percentageBonus">
    /// The percentage bonus as an integer (e.g., 20 for +20%).
    /// Converted to a float fraction (20 → 0.20) for internal storage.
    /// </param>
    /// <param name="logger">Optional logger for diagnostic output during creation.</param>
    /// <returns>
    /// A new <see cref="ArchetypeSpecialBonus"/> with BonusType set to
    /// <see cref="ConsumableEffectivenessType"/> and a generated description.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This factory method creates the standard consumable effectiveness bonus
    /// used by the Adept archetype. The percentage is converted to a decimal
    /// fraction for internal calculations (20 → 0.20).
    /// </para>
    /// <para>
    /// The generated description follows the format:
    /// "+{percentage}% effectiveness from all consumable items"
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create the Adept's +20% consumable effectiveness bonus
    /// var bonus = ArchetypeSpecialBonus.CreateConsumableEffectiveness(20);
    /// // bonus.BonusType == "ConsumableEffectiveness"
    /// // bonus.BonusValue == 0.20f
    /// // bonus.Multiplier == 1.20f
    /// // bonus.PercentageValue == 20
    /// // bonus.Description == "+20% effectiveness from all consumable items"
    /// </code>
    /// </example>
    public static ArchetypeSpecialBonus CreateConsumableEffectiveness(
        int percentageBonus,
        ILogger<ArchetypeSpecialBonus>? logger = null)
    {
        _logger = logger;

        _logger?.LogDebug(
            "Creating ConsumableEffectiveness special bonus with {PercentageBonus}% bonus",
            percentageBonus);

        var bonusValue = percentageBonus / 100f;

        var bonus = new ArchetypeSpecialBonus(
            ConsumableEffectivenessType,
            bonusValue,
            $"+{percentageBonus}% effectiveness from all consumable items");

        _logger?.LogInformation(
            "Created ConsumableEffectiveness special bonus. " +
            "PercentageBonus={PercentageBonus}%, BonusValue={BonusValue}, " +
            "Multiplier={Multiplier}, Description='{Description}'",
            percentageBonus,
            bonus.BonusValue,
            bonus.Multiplier,
            bonus.Description);

        return bonus;
    }

    /// <summary>
    /// Creates a special bonus with validation.
    /// </summary>
    /// <param name="bonusType">
    /// The bonus type identifier. Must not be null or whitespace.
    /// Identifies how the bonus is applied during gameplay.
    /// </param>
    /// <param name="bonusValue">
    /// The numeric value of the bonus as a decimal fraction.
    /// </param>
    /// <param name="description">
    /// Human-readable description for UI display. Must not be null or whitespace.
    /// </param>
    /// <param name="logger">Optional logger for diagnostic output during creation.</param>
    /// <returns>A new <see cref="ArchetypeSpecialBonus"/> instance with validated data.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="bonusType"/> or <paramref name="description"/>
    /// is null, empty, or whitespace.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This factory method validates all string parameters before construction.
    /// Use this method when loading bonus data from configuration files or
    /// other external sources to ensure data integrity.
    /// </para>
    /// <para>
    /// For creating the standard consumable effectiveness bonus, prefer using
    /// <see cref="CreateConsumableEffectiveness"/> which provides a more
    /// convenient API with automatic description generation.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create a custom special bonus from configuration data
    /// var bonus = ArchetypeSpecialBonus.Create(
    ///     "ConsumableEffectiveness",
    ///     0.20f,
    ///     "+20% effectiveness from all consumable items");
    /// </code>
    /// </example>
    public static ArchetypeSpecialBonus Create(
        string bonusType,
        float bonusValue,
        string description,
        ILogger<ArchetypeSpecialBonus>? logger = null)
    {
        _logger = logger;

        _logger?.LogDebug(
            "Creating ArchetypeSpecialBonus with BonusType='{BonusType}', " +
            "BonusValue={BonusValue}, Description='{Description}'",
            bonusType,
            bonusValue,
            description);

        // Validate bonus type is not null or whitespace
        ArgumentException.ThrowIfNullOrWhiteSpace(bonusType, nameof(bonusType));

        // Validate description is not null or whitespace
        ArgumentException.ThrowIfNullOrWhiteSpace(description, nameof(description));

        _logger?.LogDebug(
            "Validation passed for ArchetypeSpecialBonus. BonusType='{BonusType}', " +
            "BonusValue={BonusValue}",
            bonusType,
            bonusValue);

        var bonus = new ArchetypeSpecialBonus(bonusType, bonusValue, description);

        _logger?.LogInformation(
            "Created ArchetypeSpecialBonus. BonusType='{BonusType}', " +
            "BonusValue={BonusValue}, Multiplier={Multiplier}, " +
            "PercentageValue={PercentageValue}%, Description='{Description}'",
            bonus.BonusType,
            bonus.BonusValue,
            bonus.Multiplier,
            bonus.PercentageValue,
            bonus.Description);

        return bonus;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Applies this bonus to an integer base value and returns the modified value.
    /// </summary>
    /// <param name="baseValue">The original integer value to modify.</param>
    /// <returns>
    /// The modified value after applying the <see cref="Multiplier"/>,
    /// truncated to an integer.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The result is computed as <c>(int)(baseValue × Multiplier)</c>, which
    /// truncates any fractional result. For example, a +20% bonus applied to
    /// a base value of 100 returns 120.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var bonus = ArchetypeSpecialBonus.CreateConsumableEffectiveness(20);
    /// int healAmount = bonus.ApplyTo(50); // Returns 60 (50 × 1.20 = 60)
    /// </code>
    /// </example>
    public int ApplyTo(int baseValue) =>
        (int)(baseValue * Multiplier);

    /// <summary>
    /// Applies this bonus to a floating-point base value and returns the modified value.
    /// </summary>
    /// <param name="baseValue">The original float value to modify.</param>
    /// <returns>
    /// The modified value after applying the <see cref="Multiplier"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The result is computed as <c>baseValue × Multiplier</c>. For example,
    /// a +20% bonus applied to a base value of 50.0 returns 60.0.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var bonus = ArchetypeSpecialBonus.CreateConsumableEffectiveness(20);
    /// float healAmount = bonus.ApplyTo(50.5f); // Returns 60.6f (50.5 × 1.20)
    /// </code>
    /// </example>
    public float ApplyTo(float baseValue) =>
        baseValue * Multiplier;

    /// <summary>
    /// Gets a short display format for UI presentation.
    /// </summary>
    /// <returns>
    /// A compact string in the format "+{PercentageValue}% {BonusType}"
    /// (e.g., "+20% ConsumableEffectiveness").
    /// </returns>
    /// <remarks>
    /// This format is designed for compact UI elements like tooltips or
    /// summary panels where space is limited.
    /// </remarks>
    public string GetShortDisplay() =>
        $"+{PercentageValue}% {BonusType}";

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a formatted string representation of this special bonus.
    /// </summary>
    /// <returns>
    /// A string in the format "{BonusType}: +{PercentageValue}%"
    /// (e.g., "ConsumableEffectiveness: +20%").
    /// </returns>
    public override string ToString() =>
        $"{BonusType}: +{PercentageValue}%";
}
