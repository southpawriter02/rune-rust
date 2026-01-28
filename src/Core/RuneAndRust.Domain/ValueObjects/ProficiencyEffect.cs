// ═══════════════════════════════════════════════════════════════════════════════
// ProficiencyEffect.cs
// Value object encapsulating combat effects for a weapon proficiency level.
// Version: 0.16.1a
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Contains combat effects for a weapon proficiency level.
/// </summary>
/// <remarks>
/// <para>
/// ProficiencyEffect is loaded from configuration (proficiency-effects.json)
/// rather than hardcoded, allowing balance adjustments without code changes.
/// </para>
/// <para>
/// This value object is immutable after creation. Use the factory method
/// <see cref="Create"/> to construct new instances with validation.
/// </para>
/// <para>
/// Proficiency effects determine:
/// </para>
/// <list type="bullet">
///   <item><description>Attack roll modifiers (-3 to +2)</description></item>
///   <item><description>Damage roll modifiers (-2 to +1)</description></item>
///   <item><description>Access to weapon special properties</description></item>
///   <item><description>Available combat technique tiers</description></item>
/// </list>
/// <para>
/// Default effect values per level:
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Level</term>
///     <description>Attack | Damage | Special | Techniques</description>
///   </listheader>
///   <item>
///     <term>NonProficient</term>
///     <description>-3 | -2 | No | None</description>
///   </item>
///   <item>
///     <term>Proficient</term>
///     <description>+0 | +0 | Yes | Basic</description>
///   </item>
///   <item>
///     <term>Expert</term>
///     <description>+1 | +0 | Yes | Advanced</description>
///   </item>
///   <item>
///     <term>Master</term>
///     <description>+2 | +1 | Yes | Signature</description>
///   </item>
/// </list>
/// </remarks>
/// <param name="Level">The proficiency level this effect describes.</param>
/// <param name="AttackModifier">Modifier applied to attack rolls (-3 to +2).</param>
/// <param name="DamageModifier">Modifier applied to damage rolls (-2 to +1).</param>
/// <param name="CanUseSpecialProperties">Whether special weapon properties are accessible.</param>
/// <param name="UnlockedTechniques">Highest tier of techniques available.</param>
/// <param name="DisplayName">Human-readable name for UI (e.g., "Non-Proficient").</param>
/// <param name="Description">Flavor text describing this proficiency level.</param>
/// <seealso cref="WeaponProficiencyLevel"/>
/// <seealso cref="TechniqueAccess"/>
public readonly record struct ProficiencyEffect(
    WeaponProficiencyLevel Level,
    int AttackModifier,
    int DamageModifier,
    bool CanUseSpecialProperties,
    TechniqueAccess UnlockedTechniques,
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
    /// Gets a value indicating whether this level has attack penalties.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="AttackModifier"/> is negative; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Only <see cref="WeaponProficiencyLevel.NonProficient"/> typically has
    /// attack penalties. This property is useful for UI display and warning systems.
    /// </remarks>
    public bool HasAttackPenalty => AttackModifier < 0;

    /// <summary>
    /// Gets a value indicating whether this level has attack bonuses.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="AttackModifier"/> is positive; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// <see cref="WeaponProficiencyLevel.Expert"/> and <see cref="WeaponProficiencyLevel.Master"/>
    /// have attack bonuses.
    /// </remarks>
    public bool HasAttackBonus => AttackModifier > 0;

    /// <summary>
    /// Gets a value indicating whether this level has damage penalties.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="DamageModifier"/> is negative; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Only <see cref="WeaponProficiencyLevel.NonProficient"/> typically has
    /// damage penalties.
    /// </remarks>
    public bool HasDamagePenalty => DamageModifier < 0;

    /// <summary>
    /// Gets a value indicating whether this level has damage bonuses.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="DamageModifier"/> is positive; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Only <see cref="WeaponProficiencyLevel.Master"/> typically has damage bonuses.
    /// </remarks>
    public bool HasDamageBonus => DamageModifier > 0;

    /// <summary>
    /// Gets a value indicating whether this is the lowest proficiency level.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="Level"/> equals <see cref="WeaponProficiencyLevel.NonProficient"/>;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Non-proficient characters face significant combat disadvantages and
    /// should be warned when equipping weapons outside their proficiency set.
    /// </remarks>
    public bool IsNonProficient => Level == WeaponProficiencyLevel.NonProficient;

    /// <summary>
    /// Gets a value indicating whether this is the highest proficiency level.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="Level"/> equals <see cref="WeaponProficiencyLevel.Master"/>;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Master proficiency represents peak combat effectiveness with a weapon
    /// category and unlocks signature techniques.
    /// </remarks>
    public bool IsMaster => Level == WeaponProficiencyLevel.Master;

    /// <summary>
    /// Gets a value indicating whether the character can use basic techniques.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="UnlockedTechniques"/> is at least <see cref="TechniqueAccess.Basic"/>;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Basic techniques are available to all proficient characters (Proficient, Expert, Master).
    /// </remarks>
    public bool CanUseBasicTechniques =>
        UnlockedTechniques >= TechniqueAccess.Basic;

    /// <summary>
    /// Gets a value indicating whether the character can use advanced techniques.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="UnlockedTechniques"/> is at least <see cref="TechniqueAccess.Advanced"/>;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Advanced techniques are available to Expert and Master characters.
    /// </remarks>
    public bool CanUseAdvancedTechniques =>
        UnlockedTechniques >= TechniqueAccess.Advanced;

    /// <summary>
    /// Gets a value indicating whether the character can use signature techniques.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="UnlockedTechniques"/> is at least <see cref="TechniqueAccess.Signature"/>;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Signature techniques are only available to Master characters.
    /// </remarks>
    public bool CanUseSignatureTechniques =>
        UnlockedTechniques >= TechniqueAccess.Signature;

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new <see cref="ProficiencyEffect"/> with validation.
    /// </summary>
    /// <param name="level">The proficiency level.</param>
    /// <param name="attackModifier">Attack roll modifier (-5 to +5).</param>
    /// <param name="damageModifier">Damage roll modifier (-5 to +5).</param>
    /// <param name="canUseSpecialProperties">Whether special properties can be activated.</param>
    /// <param name="unlockedTechniques">Highest technique tier accessible.</param>
    /// <param name="displayName">Display name for UI presentation.</param>
    /// <param name="description">Flavor description text.</param>
    /// <returns>A new validated <see cref="ProficiencyEffect"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="displayName"/> or <paramref name="description"/>
    /// is null or whitespace.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="attackModifier"/> or <paramref name="damageModifier"/>
    /// is outside the range -5 to +5.
    /// </exception>
    /// <example>
    /// <code>
    /// // Create effect for Master proficiency
    /// var masterEffect = ProficiencyEffect.Create(
    ///     WeaponProficiencyLevel.Master,
    ///     attackModifier: 2,
    ///     damageModifier: 1,
    ///     canUseSpecialProperties: true,
    ///     TechniqueAccess.Signature,
    ///     "Master",
    ///     "Complete mastery achieved. Peak combat performance.");
    /// </code>
    /// </example>
    public static ProficiencyEffect Create(
        WeaponProficiencyLevel level,
        int attackModifier,
        int damageModifier,
        bool canUseSpecialProperties,
        TechniqueAccess unlockedTechniques,
        string displayName,
        string description)
    {
        // Validate string parameters
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        // Validate modifier ranges (allow reasonable expansion room)
        ArgumentOutOfRangeException.ThrowIfLessThan(attackModifier, -5);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(attackModifier, 5);
        ArgumentOutOfRangeException.ThrowIfLessThan(damageModifier, -5);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(damageModifier, 5);

        return new ProficiencyEffect(
            level,
            attackModifier,
            damageModifier,
            canUseSpecialProperties,
            unlockedTechniques,
            displayName,
            description);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Default Effect Factory Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a default NonProficient effect.
    /// </summary>
    /// <returns>A <see cref="ProficiencyEffect"/> with NonProficient penalties.</returns>
    /// <remarks>
    /// Provides fallback values when configuration is unavailable.
    /// Default values: -3 attack, -2 damage, no special properties, no techniques.
    /// </remarks>
    public static ProficiencyEffect CreateNonProficient() => new(
        WeaponProficiencyLevel.NonProficient,
        -3,
        -2,
        false,
        TechniqueAccess.None,
        "Non-Proficient",
        "No training with this weapon type. Severe penalties apply and special properties cannot be activated.");

    /// <summary>
    /// Creates a default Proficient effect.
    /// </summary>
    /// <returns>A <see cref="ProficiencyEffect"/> with baseline values.</returns>
    /// <remarks>
    /// Provides fallback values when configuration is unavailable.
    /// Default values: +0 attack, +0 damage, special properties enabled, basic techniques.
    /// </remarks>
    public static ProficiencyEffect CreateProficient() => new(
        WeaponProficiencyLevel.Proficient,
        0,
        0,
        true,
        TechniqueAccess.Basic,
        "Proficient",
        "Basic training completed. Full access to weapon capabilities and basic combat techniques.");

    /// <summary>
    /// Creates a default Expert effect.
    /// </summary>
    /// <returns>A <see cref="ProficiencyEffect"/> with Expert bonuses.</returns>
    /// <remarks>
    /// Provides fallback values when configuration is unavailable.
    /// Default values: +1 attack, +0 damage, special properties enabled, advanced techniques.
    /// </remarks>
    public static ProficiencyEffect CreateExpert() => new(
        WeaponProficiencyLevel.Expert,
        1,
        0,
        true,
        TechniqueAccess.Advanced,
        "Expert",
        "Advanced training mastered. Enhanced attack accuracy and access to advanced combat techniques.");

    /// <summary>
    /// Creates a default Master effect.
    /// </summary>
    /// <returns>A <see cref="ProficiencyEffect"/> with Master bonuses.</returns>
    /// <remarks>
    /// Provides fallback values when configuration is unavailable.
    /// Default values: +2 attack, +1 damage, special properties enabled, signature techniques.
    /// </remarks>
    public static ProficiencyEffect CreateMaster() => new(
        WeaponProficiencyLevel.Master,
        2,
        1,
        true,
        TechniqueAccess.Signature,
        "Master",
        "Complete mastery achieved. Peak combat performance with signature techniques that define your fighting style.");

    // ═══════════════════════════════════════════════════════════════════════════
    // Formatting Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats the attack modifier for display.
    /// </summary>
    /// <returns>
    /// A formatted string with a leading sign (e.g., "+2", "-3", "+0").
    /// </returns>
    /// <remarks>
    /// Uses the standard D&amp;D/RPG convention of always showing a sign,
    /// even for zero values.
    /// </remarks>
    /// <example>
    /// <code>
    /// var effect = ProficiencyEffect.CreateMaster();
    /// Console.WriteLine(effect.FormatAttackModifier()); // Output: "+2"
    /// </code>
    /// </example>
    public string FormatAttackModifier() =>
        AttackModifier >= 0 ? $"+{AttackModifier}" : $"{AttackModifier}";

    /// <summary>
    /// Formats the damage modifier for display.
    /// </summary>
    /// <returns>
    /// A formatted string with a leading sign (e.g., "+1", "-2", "+0").
    /// </returns>
    /// <remarks>
    /// Uses the standard D&amp;D/RPG convention of always showing a sign,
    /// even for zero values.
    /// </remarks>
    /// <example>
    /// <code>
    /// var effect = ProficiencyEffect.CreateNonProficient();
    /// Console.WriteLine(effect.FormatDamageModifier()); // Output: "-2"
    /// </code>
    /// </example>
    public string FormatDamageModifier() =>
        DamageModifier >= 0 ? $"+{DamageModifier}" : $"{DamageModifier}";

    /// <summary>
    /// Creates a summary string for logging and debugging purposes.
    /// </summary>
    /// <returns>
    /// A compact string representation including all key effect values.
    /// </returns>
    /// <example>
    /// <code>
    /// var effect = ProficiencyEffect.CreateExpert();
    /// Console.WriteLine(effect.ToString());
    /// // Output: "Expert: Atk +1, Dmg +0, Special: True, Techniques: Advanced"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"{DisplayName}: Atk {FormatAttackModifier()}, Dmg {FormatDamageModifier()}, " +
        $"Special: {CanUseSpecialProperties}, Techniques: {UnlockedTechniques}";

    /// <summary>
    /// Creates a detailed debug string with all property values.
    /// </summary>
    /// <returns>
    /// A verbose string representation suitable for detailed debugging.
    /// </returns>
    /// <example>
    /// <code>
    /// var effect = ProficiencyEffect.CreateMaster();
    /// Console.WriteLine(effect.ToDebugString());
    /// // Output: "ProficiencyEffect { Level: Master (3), Atk: +2, Dmg: +1, SpecialProps: True, Techniques: Signature }"
    /// </code>
    /// </example>
    public string ToDebugString() =>
        $"ProficiencyEffect {{ Level: {Level} ({LevelValue}), Atk: {FormatAttackModifier()}, " +
        $"Dmg: {FormatDamageModifier()}, SpecialProps: {CanUseSpecialProperties}, Techniques: {UnlockedTechniques} }}";
}
