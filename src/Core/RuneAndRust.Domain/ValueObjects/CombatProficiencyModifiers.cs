// ═══════════════════════════════════════════════════════════════════════════════
// CombatProficiencyModifiers.cs
// Value object encapsulating all proficiency-based combat modifiers for an attack.
// Version: 0.16.1f
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents all proficiency-based combat modifiers for an attack.
/// </summary>
/// <remarks>
/// <para>
/// CombatProficiencyModifiers is a lightweight value object designed for use during
/// combat resolution. It encapsulates all proficiency-related effects in a single
/// object that can be retrieved once and reused for both attack and damage calculations.
/// </para>
/// <para>
/// This value object is created by <c>IProficiencyCheckService.GetCombatModifiers</c>
/// and should be retrieved once per attack, then reused for all modifier applications.
/// </para>
/// <para>
/// Modifier values by proficiency level:
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Level</term>
///     <description>Attack | Damage | Special Props | Techniques</description>
///   </listheader>
///   <item>
///     <term>NonProficient</term>
///     <description>-3 | -2 | Blocked | None</description>
///   </item>
///   <item>
///     <term>Proficient</term>
///     <description>+0 | +0 | Allowed | Basic</description>
///   </item>
///   <item>
///     <term>Expert</term>
///     <description>+1 | +0 | Allowed | Advanced</description>
///   </item>
///   <item>
///     <term>Master</term>
///     <description>+2 | +1 | Allowed | Signature</description>
///   </item>
/// </list>
/// </remarks>
/// <param name="ProficiencyLevel">Current proficiency level with the weapon category.</param>
/// <param name="AttackModifier">Bonus/penalty to attack rolls (-3 to +2).</param>
/// <param name="DamageModifier">Bonus/penalty to damage rolls (-2 to +1).</param>
/// <param name="CanUseSpecialProperties">Whether weapon special properties can be activated.</param>
/// <param name="UnlockedTechniqueLevel">Highest technique level available.</param>
/// <seealso cref="WeaponProficiencyLevel"/>
/// <seealso cref="TechniqueAccess"/>
/// <seealso cref="ProficiencyEffect"/>
public readonly record struct CombatProficiencyModifiers(
    WeaponProficiencyLevel ProficiencyLevel,
    int AttackModifier,
    int DamageModifier,
    bool CanUseSpecialProperties,
    TechniqueAccess UnlockedTechniqueLevel)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Convenience Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether the character is non-proficient (has penalties).
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="ProficiencyLevel"/> equals
    /// <see cref="WeaponProficiencyLevel.NonProficient"/>; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Non-proficient characters suffer attack and damage penalties and cannot
    /// use weapon special properties or combat techniques.
    /// </remarks>
    public bool IsNonProficient => ProficiencyLevel == WeaponProficiencyLevel.NonProficient;

    /// <summary>
    /// Gets whether there are any penalties (negative modifiers).
    /// </summary>
    /// <value>
    /// <c>true</c> if either <see cref="AttackModifier"/> or <see cref="DamageModifier"/>
    /// is negative; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Useful for UI display to highlight when a character is suffering penalties
    /// from using an unfamiliar weapon.
    /// </remarks>
    public bool HasPenalty => AttackModifier < 0 || DamageModifier < 0;

    /// <summary>
    /// Gets whether there are any bonuses (positive modifiers).
    /// </summary>
    /// <value>
    /// <c>true</c> if either <see cref="AttackModifier"/> or <see cref="DamageModifier"/>
    /// is positive; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Useful for UI display to highlight when a character gains bonuses from
    /// advanced weapon proficiency (Expert or Master).
    /// </remarks>
    public bool HasBonus => AttackModifier > 0 || DamageModifier > 0;

    /// <summary>
    /// Gets whether the character is at Expert level or higher.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="ProficiencyLevel"/> is
    /// <see cref="WeaponProficiencyLevel.Expert"/> or <see cref="WeaponProficiencyLevel.Master"/>;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Expert and Master levels provide attack bonuses and access to advanced
    /// or signature techniques respectively.
    /// </remarks>
    public bool IsExpertOrHigher => ProficiencyLevel >= WeaponProficiencyLevel.Expert;

    /// <summary>
    /// Gets whether the character has mastered this weapon category.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="ProficiencyLevel"/> equals
    /// <see cref="WeaponProficiencyLevel.Master"/>; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Master proficiency represents peak combat effectiveness with both
    /// attack and damage bonuses, plus access to signature techniques.
    /// </remarks>
    public bool IsMaster => ProficiencyLevel == WeaponProficiencyLevel.Master;

    // ═══════════════════════════════════════════════════════════════════════════
    // Description Property
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a formatted description of the modifiers for display.
    /// </summary>
    /// <value>
    /// A human-readable string describing the proficiency level and its effects.
    /// </value>
    /// <remarks>
    /// <para>
    /// Format varies by proficiency state:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>NonProficient: "NonProficient (-3 attack, -2 damage, special properties blocked)"</description></item>
    ///   <item><description>With bonuses: "Master (+2 attack, +1 damage)"</description></item>
    ///   <item><description>Baseline: "Proficient"</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var modifiers = CombatProficiencyModifiers.Master;
    /// Console.WriteLine(modifiers.Description);
    /// // Output: "Master (+2 attack, +1 damage)"
    /// </code>
    /// </example>
    public string Description
    {
        get
        {
            if (IsNonProficient)
            {
                return $"NonProficient ({AttackModifier:+0;-#} attack, {DamageModifier:+0;-#} damage, special properties blocked)";
            }

            if (HasBonus)
            {
                return $"{ProficiencyLevel} ({AttackModifier:+0;-#} attack, {DamageModifier:+0;-#} damage)";
            }

            return ProficiencyLevel.ToString();
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates modifiers from a <see cref="ProficiencyEffect"/>.
    /// </summary>
    /// <param name="level">The proficiency level.</param>
    /// <param name="effect">The effect configuration for this level.</param>
    /// <returns>Combat modifiers based on the effect.</returns>
    /// <remarks>
    /// <para>
    /// This factory method is the primary way to create CombatProficiencyModifiers
    /// from configuration-loaded effect data. It extracts all relevant combat
    /// modifiers from the <see cref="ProficiencyEffect"/> value object.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var effect = _effectProvider.GetEffect(WeaponProficiencyLevel.Expert);
    /// var modifiers = CombatProficiencyModifiers.FromEffect(
    ///     WeaponProficiencyLevel.Expert, effect);
    /// </code>
    /// </example>
    public static CombatProficiencyModifiers FromEffect(
        WeaponProficiencyLevel level,
        ProficiencyEffect effect)
    {
        return new CombatProficiencyModifiers(
            ProficiencyLevel: level,
            AttackModifier: effect.AttackModifier,
            DamageModifier: effect.DamageModifier,
            CanUseSpecialProperties: effect.CanUseSpecialProperties,
            UnlockedTechniqueLevel: effect.UnlockedTechniques);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Static Factory Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates default modifiers for NonProficient level.
    /// </summary>
    /// <value>
    /// A <see cref="CombatProficiencyModifiers"/> with NonProficient penalties:
    /// -3 attack, -2 damage, special properties blocked, no techniques.
    /// </value>
    /// <remarks>
    /// Use this property for quick access to default NonProficient modifiers
    /// when configuration data is not needed.
    /// </remarks>
    public static CombatProficiencyModifiers NonProficient =>
        new(
            ProficiencyLevel: WeaponProficiencyLevel.NonProficient,
            AttackModifier: -3,
            DamageModifier: -2,
            CanUseSpecialProperties: false,
            UnlockedTechniqueLevel: TechniqueAccess.None);

    /// <summary>
    /// Creates default modifiers for Proficient level.
    /// </summary>
    /// <value>
    /// A <see cref="CombatProficiencyModifiers"/> with baseline values:
    /// +0 attack, +0 damage, special properties allowed, basic techniques.
    /// </value>
    /// <remarks>
    /// Proficient is the baseline level with no bonuses or penalties.
    /// Characters can use all weapon features normally.
    /// </remarks>
    public static CombatProficiencyModifiers Proficient =>
        new(
            ProficiencyLevel: WeaponProficiencyLevel.Proficient,
            AttackModifier: 0,
            DamageModifier: 0,
            CanUseSpecialProperties: true,
            UnlockedTechniqueLevel: TechniqueAccess.Basic);

    /// <summary>
    /// Creates default modifiers for Expert level.
    /// </summary>
    /// <value>
    /// A <see cref="CombatProficiencyModifiers"/> with Expert bonuses:
    /// +1 attack, +0 damage, special properties allowed, advanced techniques.
    /// </value>
    /// <remarks>
    /// Expert level provides improved attack accuracy and unlocks
    /// advanced combat techniques for the weapon category.
    /// </remarks>
    public static CombatProficiencyModifiers Expert =>
        new(
            ProficiencyLevel: WeaponProficiencyLevel.Expert,
            AttackModifier: +1,
            DamageModifier: 0,
            CanUseSpecialProperties: true,
            UnlockedTechniqueLevel: TechniqueAccess.Advanced);

    /// <summary>
    /// Creates default modifiers for Master level.
    /// </summary>
    /// <value>
    /// A <see cref="CombatProficiencyModifiers"/> with Master bonuses:
    /// +2 attack, +1 damage, special properties allowed, signature techniques.
    /// </value>
    /// <remarks>
    /// Master level represents peak combat effectiveness with this weapon
    /// category, providing both attack and damage bonuses plus access to
    /// signature techniques that define the character's fighting style.
    /// </remarks>
    public static CombatProficiencyModifiers Master =>
        new(
            ProficiencyLevel: WeaponProficiencyLevel.Master,
            AttackModifier: +2,
            DamageModifier: +1,
            CanUseSpecialProperties: true,
            UnlockedTechniqueLevel: TechniqueAccess.Signature);

    // ═══════════════════════════════════════════════════════════════════════════
    // Formatting Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a display string for debug/logging purposes.
    /// </summary>
    /// <returns>The formatted <see cref="Description"/> property.</returns>
    public override string ToString() => Description;
}
