// ═══════════════════════════════════════════════════════════════════════════════
// ArmorProficiencyEffect.cs
// Value object encapsulating effects for an armor proficiency level.
// Version: 0.16.2a
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Contains effects for an armor proficiency level.
/// </summary>
/// <remarks>
/// <para>
/// ArmorProficiencyEffect is loaded from configuration (armor-proficiency-effects.json)
/// rather than hardcoded, allowing balance adjustments without code changes.
/// </para>
/// <para>
/// This value object is immutable after creation. Use the factory method
/// <see cref="Create"/> to construct new instances with validation.
/// </para>
/// <para>
/// Armor proficiency effects determine:
/// </para>
/// <list type="bullet">
///   <item><description>Penalty multiplier (1.0x normal, 2.0x for NonProficient)</description></item>
///   <item><description>Attack roll modifiers (-2 to 0)</description></item>
///   <item><description>Defense roll modifiers (0 to +1)</description></item>
///   <item><description>Tier reduction (0 to 1, for treating Heavy as Medium)</description></item>
///   <item><description>Access to armor special properties</description></item>
/// </list>
/// <para>
/// Default effect values per level:
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Level</term>
///     <description>Penalty Mult | Attack | Defense | Tier Reduce | Special</description>
///   </listheader>
///   <item>
///     <term>NonProficient</term>
///     <description>2.0x | -2 | +0 | 0 | No</description>
///   </item>
///   <item>
///     <term>Proficient</term>
///     <description>1.0x | +0 | +0 | 0 | Yes</description>
///   </item>
///   <item>
///     <term>Expert</term>
///     <description>1.0x | +0 | +0 | 1 | Yes</description>
///   </item>
///   <item>
///     <term>Master</term>
///     <description>1.0x | +0 | +1 | 1 | Yes</description>
///   </item>
/// </list>
/// </remarks>
/// <param name="Level">The proficiency level this effect describes.</param>
/// <param name="PenaltyMultiplier">Multiplier for armor penalties (1.0 or 2.0).</param>
/// <param name="AttackModifier">Modifier applied to attack rolls (-2 to 0).</param>
/// <param name="DefenseModifier">Modifier applied to defense (0 to +1).</param>
/// <param name="TierReduction">Number of tiers to reduce for penalty calculation.</param>
/// <param name="CanUseSpecialProperties">Whether special armor properties are accessible.</param>
/// <param name="DisplayName">Human-readable name for UI (e.g., "Non-Proficient").</param>
/// <param name="Description">Flavor text describing this proficiency level.</param>
/// <seealso cref="ArmorProficiencyLevel"/>
public readonly record struct ArmorProficiencyEffect(
    ArmorProficiencyLevel Level,
    decimal PenaltyMultiplier,
    int AttackModifier,
    int DefenseModifier,
    int TierReduction,
    bool CanUseSpecialProperties,
    string DisplayName,
    string Description)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Derived Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the level as an integer value.
    /// </summary>
    /// <value>
    /// The integer representation of <see cref="Level"/> (0-3).
    /// </value>
    /// <remarks>
    /// Useful for serialization, comparison, and display purposes.
    /// </remarks>
    public int LevelValue => (int)Level;

    /// <summary>
    /// Gets a value indicating whether penalties are doubled at this level.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="PenaltyMultiplier"/> is at least 2.0; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Only <see cref="ArmorProficiencyLevel.NonProficient"/> typically has
    /// doubled penalties. This property is useful for UI display and warning systems.
    /// </remarks>
    public bool HasDoubledPenalties => PenaltyMultiplier >= 2.0m;

    /// <summary>
    /// Gets a value indicating whether this level has attack penalties.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="AttackModifier"/> is negative; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Only <see cref="ArmorProficiencyLevel.NonProficient"/> typically has attack penalties.
    /// </remarks>
    public bool HasAttackPenalty => AttackModifier < 0;

    /// <summary>
    /// Gets a value indicating whether this level has defense bonuses.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="DefenseModifier"/> is positive; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Only <see cref="ArmorProficiencyLevel.Master"/> typically has defense bonuses.
    /// </remarks>
    public bool HasDefenseBonus => DefenseModifier > 0;

    /// <summary>
    /// Gets a value indicating whether this level provides tier reduction.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="TierReduction"/> is greater than 0; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// <see cref="ArmorProficiencyLevel.Expert"/> and <see cref="ArmorProficiencyLevel.Master"/>
    /// have tier reduction, causing Heavy armor to apply Medium-tier penalties.
    /// </remarks>
    public bool HasTierReduction => TierReduction > 0;

    /// <summary>
    /// Gets a value indicating whether this is the lowest proficiency level.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="Level"/> equals <see cref="ArmorProficiencyLevel.NonProficient"/>;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Non-proficient characters face significant armor disadvantages and
    /// should be warned when equipping armor outside their proficiency set.
    /// </remarks>
    public bool IsNonProficient => Level == ArmorProficiencyLevel.NonProficient;

    /// <summary>
    /// Gets a value indicating whether this is the highest proficiency level.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="Level"/> equals <see cref="ArmorProficiencyLevel.Master"/>;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Master proficiency represents peak armor effectiveness with a category
    /// and unlocks defensive bonuses.
    /// </remarks>
    public bool IsMaster => Level == ArmorProficiencyLevel.Master;

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new <see cref="ArmorProficiencyEffect"/> with validation.
    /// </summary>
    /// <param name="level">The proficiency level.</param>
    /// <param name="penaltyMultiplier">Penalty multiplier (1.0 to 3.0).</param>
    /// <param name="attackModifier">Attack roll modifier (-5 to 0).</param>
    /// <param name="defenseModifier">Defense modifier (0 to 3).</param>
    /// <param name="tierReduction">Tier reduction (0 to 2).</param>
    /// <param name="canUseSpecialProperties">Whether special properties can be activated.</param>
    /// <param name="displayName">Display name for UI presentation.</param>
    /// <param name="description">Flavor description text.</param>
    /// <returns>A new validated <see cref="ArmorProficiencyEffect"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="displayName"/> or <paramref name="description"/>
    /// is null or whitespace.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when any numeric parameter is outside its valid range.
    /// </exception>
    /// <example>
    /// <code>
    /// // Create effect for Master proficiency
    /// var masterEffect = ArmorProficiencyEffect.Create(
    ///     ArmorProficiencyLevel.Master,
    ///     penaltyMultiplier: 1.0m,
    ///     attackModifier: 0,
    ///     defenseModifier: 1,
    ///     tierReduction: 1,
    ///     canUseSpecialProperties: true,
    ///     "Master",
    ///     "Peak armor proficiency achieved.");
    /// </code>
    /// </example>
    public static ArmorProficiencyEffect Create(
        ArmorProficiencyLevel level,
        decimal penaltyMultiplier,
        int attackModifier,
        int defenseModifier,
        int tierReduction,
        bool canUseSpecialProperties,
        string displayName,
        string description)
    {
        // Validate string parameters
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        // Validate penalty multiplier range (1.0x to 3.0x)
        ArgumentOutOfRangeException.ThrowIfLessThan(penaltyMultiplier, 1.0m);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(penaltyMultiplier, 3.0m);

        // Validate attack modifier range (-5 to 0)
        ArgumentOutOfRangeException.ThrowIfLessThan(attackModifier, -5);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(attackModifier, 0);

        // Validate defense modifier range (0 to 3)
        ArgumentOutOfRangeException.ThrowIfNegative(defenseModifier);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(defenseModifier, 3);

        // Validate tier reduction range (0 to 2)
        ArgumentOutOfRangeException.ThrowIfNegative(tierReduction);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(tierReduction, 2);

        return new ArmorProficiencyEffect(
            level,
            penaltyMultiplier,
            attackModifier,
            defenseModifier,
            tierReduction,
            canUseSpecialProperties,
            displayName,
            description);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Default Effect Factory Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a default NonProficient effect.
    /// </summary>
    /// <returns>An <see cref="ArmorProficiencyEffect"/> with NonProficient penalties.</returns>
    /// <remarks>
    /// Provides fallback values when configuration is unavailable.
    /// Default values: 2.0x penalty, -2 attack, no special properties.
    /// </remarks>
    public static ArmorProficiencyEffect CreateNonProficient() => new(
        ArmorProficiencyLevel.NonProficient,
        2.0m,
        -2,
        0,
        0,
        false,
        "Non-Proficient",
        "All armor penalties are doubled. -2 attack penalty. Cannot use special armor properties.");

    /// <summary>
    /// Creates a default Proficient effect.
    /// </summary>
    /// <returns>An <see cref="ArmorProficiencyEffect"/> with baseline values.</returns>
    /// <remarks>
    /// Provides fallback values when configuration is unavailable.
    /// Default values: 1.0x penalty, standard armor function.
    /// </remarks>
    public static ArmorProficiencyEffect CreateProficient() => new(
        ArmorProficiencyLevel.Proficient,
        1.0m,
        0,
        0,
        0,
        true,
        "Proficient",
        "Standard armor penalties apply. Full access to armor functions and special properties.");

    /// <summary>
    /// Creates a default Expert effect.
    /// </summary>
    /// <returns>An <see cref="ArmorProficiencyEffect"/> with Expert benefits.</returns>
    /// <remarks>
    /// Provides fallback values when configuration is unavailable.
    /// Default values: 1.0x penalty, tier reduction (Heavy → Medium).
    /// </remarks>
    public static ArmorProficiencyEffect CreateExpert() => new(
        ArmorProficiencyLevel.Expert,
        1.0m,
        0,
        0,
        1,
        true,
        "Expert",
        "Armor is treated as one tier lighter for penalty calculations. Heavy armor uses Medium penalties.");

    /// <summary>
    /// Creates a default Master effect.
    /// </summary>
    /// <returns>An <see cref="ArmorProficiencyEffect"/> with Master bonuses.</returns>
    /// <remarks>
    /// Provides fallback values when configuration is unavailable.
    /// Default values: 1.0x penalty, tier reduction, +1 Defense.
    /// </remarks>
    public static ArmorProficiencyEffect CreateMaster() => new(
        ArmorProficiencyLevel.Master,
        1.0m,
        0,
        1,
        1,
        true,
        "Master",
        "Tier reduction like Expert, plus +1 Defense bonus and halved armor maintenance costs.");

    // ═══════════════════════════════════════════════════════════════════════════
    // Formatting Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats the penalty multiplier for display.
    /// </summary>
    /// <returns>
    /// A formatted string showing the multiplier (e.g., "1.0x", "2.0x").
    /// </returns>
    /// <example>
    /// <code>
    /// var effect = ArmorProficiencyEffect.CreateNonProficient();
    /// Console.WriteLine(effect.FormatPenaltyMultiplier()); // Output: "2.0x"
    /// </code>
    /// </example>
    public string FormatPenaltyMultiplier() => $"{PenaltyMultiplier:F1}x";

    /// <summary>
    /// Formats the attack modifier for display.
    /// </summary>
    /// <returns>
    /// A formatted string with a leading sign (e.g., "+0", "-2").
    /// </returns>
    /// <remarks>
    /// Uses the standard RPG convention of always showing a sign.
    /// </remarks>
    /// <example>
    /// <code>
    /// var effect = ArmorProficiencyEffect.CreateNonProficient();
    /// Console.WriteLine(effect.FormatAttackModifier()); // Output: "-2"
    /// </code>
    /// </example>
    public string FormatAttackModifier() =>
        AttackModifier >= 0 ? $"+{AttackModifier}" : $"{AttackModifier}";

    /// <summary>
    /// Formats the defense modifier for display.
    /// </summary>
    /// <returns>
    /// A formatted string with a leading sign (e.g., "+0", "+1").
    /// </returns>
    /// <remarks>
    /// Uses the standard RPG convention of always showing a sign.
    /// </remarks>
    /// <example>
    /// <code>
    /// var effect = ArmorProficiencyEffect.CreateMaster();
    /// Console.WriteLine(effect.FormatDefenseModifier()); // Output: "+1"
    /// </code>
    /// </example>
    public string FormatDefenseModifier() =>
        DefenseModifier >= 0 ? $"+{DefenseModifier}" : $"{DefenseModifier}";

    /// <summary>
    /// Creates a summary string for logging and debugging purposes.
    /// </summary>
    /// <returns>
    /// A compact string representation including all key effect values.
    /// </returns>
    /// <example>
    /// <code>
    /// var effect = ArmorProficiencyEffect.CreateExpert();
    /// Console.WriteLine(effect.ToString());
    /// // Output: "Expert: Penalty 1.0x, Atk +0, Def +0, TierReduce 1"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"{DisplayName}: Penalty {FormatPenaltyMultiplier()}, Atk {FormatAttackModifier()}, " +
        $"Def {FormatDefenseModifier()}, TierReduce {TierReduction}";

    /// <summary>
    /// Creates a detailed debug string with all property values.
    /// </summary>
    /// <returns>
    /// A verbose string representation suitable for detailed debugging.
    /// </returns>
    /// <example>
    /// <code>
    /// var effect = ArmorProficiencyEffect.CreateMaster();
    /// Console.WriteLine(effect.ToDebugString());
    /// // Output: "ArmorProficiencyEffect { Level: Master (3), Penalty: 1.0x, Atk: +0, Def: +1, TierReduce: 1, SpecialProps: True }"
    /// </code>
    /// </example>
    public string ToDebugString() =>
        $"ArmorProficiencyEffect {{ Level: {Level} ({LevelValue}), Penalty: {FormatPenaltyMultiplier()}, " +
        $"Atk: {FormatAttackModifier()}, Def: {FormatDefenseModifier()}, TierReduce: {TierReduction}, SpecialProps: {CanUseSpecialProperties} }}";
}
