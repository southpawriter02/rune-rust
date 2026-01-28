// ═══════════════════════════════════════════════════════════════════════════════
// IProficiencyCheckService.cs
// Interface providing proficiency-based combat modifiers and experience recording.
// Version: 0.16.1f
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Provides proficiency-based combat modifiers and experience recording.
/// </summary>
/// <remarks>
/// <para>
/// This service is called by the combat system during attack resolution
/// to apply proficiency bonuses and penalties. It also handles recording
/// combat experience after each attack.
/// </para>
/// <para>
/// For combat performance, prefer using <see cref="GetCombatModifiers"/>
/// once per attack, then reuse the returned modifiers object for both
/// attack and damage calculations.
/// </para>
/// <para>
/// Implementation responsibilities:
/// </para>
/// <list type="bullet">
///   <item><description>Load proficiency effects from configuration via <see cref="IProficiencyEffectProvider"/></description></item>
///   <item><description>Query character proficiency levels from <see cref="CharacterProficiencies"/></description></item>
///   <item><description>Delegate experience recording to <see cref="IProficiencyAcquisitionService"/></description></item>
///   <item><description>Log modifier calculations at Debug level</description></item>
///   <item><description>Log level advancements at Information level</description></item>
/// </list>
/// <para>
/// Combat integration pattern:
/// </para>
/// <code>
/// // Get modifiers once per attack
/// var modifiers = _proficiencyCheckService.GetCombatModifiers(
///     character.Proficiencies, weapon.Category);
///
/// // Apply to attack roll
/// var attackBonus = baseAttack + modifiers.AttackModifier;
///
/// // Apply to damage roll
/// var damageBonus = baseDamage + modifiers.DamageModifier;
///
/// // Check special properties
/// if (weapon.HasSpecialProperty &amp;&amp; modifiers.CanUseSpecialProperties)
/// {
///     ApplySpecialProperty(weapon);
/// }
///
/// // Record combat usage
/// await _proficiencyCheckService.RecordCombatUsageAsync(
///     character.Proficiencies, weapon.Category);
/// </code>
/// </remarks>
/// <seealso cref="CombatProficiencyModifiers"/>
/// <seealso cref="IProficiencyEffectProvider"/>
/// <seealso cref="IProficiencyAcquisitionService"/>
/// <seealso cref="CharacterProficiencies"/>
public interface IProficiencyCheckService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Primary Combat Integration Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets all combat modifiers for a character using a weapon category.
    /// </summary>
    /// <param name="proficiencies">The character's proficiency tracking entity.</param>
    /// <param name="weaponCategory">The weapon's category.</param>
    /// <returns>Complete proficiency modifiers for combat.</returns>
    /// <remarks>
    /// <para>
    /// This is the primary method for combat integration. Call once per
    /// attack and reuse the returned modifiers for attack and damage calculations.
    /// </para>
    /// <para>
    /// The returned <see cref="CombatProficiencyModifiers"/> contains:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Attack modifier (-3 to +2)</description></item>
    ///   <item><description>Damage modifier (-2 to +1)</description></item>
    ///   <item><description>Special property access flag</description></item>
    ///   <item><description>Highest unlocked technique tier</description></item>
    /// </list>
    /// <para>
    /// Logging: Debug level with character proficiency level and calculated modifiers.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var modifiers = _proficiencyCheckService.GetCombatModifiers(
    ///     character.Proficiencies, WeaponCategory.Swords);
    ///
    /// var totalAttack = baseAttack + modifiers.AttackModifier;
    /// var totalDamage = baseDamage + modifiers.DamageModifier;
    /// </code>
    /// </example>
    CombatProficiencyModifiers GetCombatModifiers(
        CharacterProficiencies proficiencies,
        WeaponCategory weaponCategory);

    // ═══════════════════════════════════════════════════════════════════════════
    // Individual Modifier Accessors
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets only the attack modifier for a character and weapon category.
    /// </summary>
    /// <param name="proficiencies">The character's proficiency tracking entity.</param>
    /// <param name="weaponCategory">The weapon's category.</param>
    /// <returns>Attack modifier: -3 (NonProf), 0 (Prof), +1 (Expert), +2 (Master).</returns>
    /// <remarks>
    /// <para>
    /// Use this method when you only need the attack modifier without other
    /// proficiency effects. For full combat resolution, prefer <see cref="GetCombatModifiers"/>.
    /// </para>
    /// <para>
    /// Logging: Debug level with character proficiency level and attack modifier.
    /// </para>
    /// </remarks>
    int GetAttackModifier(CharacterProficiencies proficiencies, WeaponCategory weaponCategory);

    /// <summary>
    /// Gets only the damage modifier for a character and weapon category.
    /// </summary>
    /// <param name="proficiencies">The character's proficiency tracking entity.</param>
    /// <param name="weaponCategory">The weapon's category.</param>
    /// <returns>Damage modifier: -2 (NonProf), 0 (Prof/Expert), +1 (Master).</returns>
    /// <remarks>
    /// <para>
    /// Use this method when you only need the damage modifier without other
    /// proficiency effects. For full combat resolution, prefer <see cref="GetCombatModifiers"/>.
    /// </para>
    /// <para>
    /// Logging: Debug level with character proficiency level and damage modifier.
    /// </para>
    /// </remarks>
    int GetDamageModifier(CharacterProficiencies proficiencies, WeaponCategory weaponCategory);

    // ═══════════════════════════════════════════════════════════════════════════
    // Capability Check Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if a character can use weapon special properties.
    /// </summary>
    /// <param name="proficiencies">The character's proficiency tracking entity.</param>
    /// <param name="weaponCategory">The weapon's category.</param>
    /// <returns>True if Proficient or higher, false if NonProficient.</returns>
    /// <remarks>
    /// <para>
    /// NonProficient characters cannot activate weapon special properties
    /// like elemental damage, cleave effects, or magical abilities. The weapon
    /// functions as a basic weapon without its special features.
    /// </para>
    /// <para>
    /// Logging: Debug level with character proficiency level and result.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (_proficiencyCheckService.CanUseSpecialProperties(
    ///     character.Proficiencies, weapon.Category))
    /// {
    ///     ApplyLifestealEffect(weapon, damage);
    /// }
    /// </code>
    /// </example>
    bool CanUseSpecialProperties(CharacterProficiencies proficiencies, WeaponCategory weaponCategory);

    /// <summary>
    /// Checks if a character can use a specific technique level.
    /// </summary>
    /// <param name="proficiencies">The character's proficiency tracking entity.</param>
    /// <param name="weaponCategory">The weapon's category.</param>
    /// <param name="techniqueLevel">The required technique access level.</param>
    /// <returns>True if character's proficiency unlocks the technique level.</returns>
    /// <remarks>
    /// <para>
    /// Technique access levels map to proficiency levels:
    /// </para>
    /// <list type="table">
    ///   <listheader>
    ///     <term>Technique Level</term>
    ///     <description>Required Proficiency</description>
    ///   </listheader>
    ///   <item>
    ///     <term>None</term>
    ///     <description>Never available (placeholder)</description>
    ///   </item>
    ///   <item>
    ///     <term>Basic</term>
    ///     <description>Proficient or higher</description>
    ///   </item>
    ///   <item>
    ///     <term>Advanced</term>
    ///     <description>Expert or higher</description>
    ///   </item>
    ///   <item>
    ///     <term>Signature</term>
    ///     <description>Master only</description>
    ///   </item>
    /// </list>
    /// <para>
    /// Logging: Debug level with technique level check and result.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (_proficiencyCheckService.CanUseTechnique(
    ///     character.Proficiencies,
    ///     weapon.Category,
    ///     TechniqueAccess.Advanced))
    /// {
    ///     EnableAdvancedTechniqueButtons();
    /// }
    /// </code>
    /// </example>
    bool CanUseTechnique(
        CharacterProficiencies proficiencies,
        WeaponCategory weaponCategory,
        TechniqueAccess techniqueLevel);

    // ═══════════════════════════════════════════════════════════════════════════
    // Experience Recording Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Records combat usage for proficiency experience.
    /// </summary>
    /// <param name="proficiencies">The character's proficiency tracking entity.</param>
    /// <param name="weaponCategory">The weapon category used.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>Result indicating if proficiency level increased.</returns>
    /// <remarks>
    /// <para>
    /// Call this after each combat where the character attacked with a weapon.
    /// This delegates to <see cref="IProficiencyAcquisitionService.RecordCombatExperienceAsync"/>.
    /// </para>
    /// <para>
    /// Experience thresholds (configurable):
    /// </para>
    /// <list type="bullet">
    ///   <item><description>NonProficient → Proficient: 10 combats</description></item>
    ///   <item><description>Proficient → Expert: 25 combats</description></item>
    ///   <item><description>Expert → Master: 50 combats</description></item>
    /// </list>
    /// <para>
    /// Logging:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Debug: Experience recorded</description></item>
    ///   <item><description>Information: Level advancement when it occurs</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = await _proficiencyCheckService.RecordCombatUsageAsync(
    ///     character.Proficiencies, weapon.Category);
    ///
    /// if (result.LevelChanged)
    /// {
    ///     DisplayLevelUpNotification(weapon.Category, result.NewLevel);
    /// }
    /// </code>
    /// </example>
    Task<ProficiencyGainResult> RecordCombatUsageAsync(
        CharacterProficiencies proficiencies,
        WeaponCategory weaponCategory,
        CancellationToken ct = default);

    // ═══════════════════════════════════════════════════════════════════════════
    // Display Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a description of proficiency effects for display.
    /// </summary>
    /// <param name="level">The proficiency level.</param>
    /// <returns>Human-readable description of the level's combat effects.</returns>
    /// <remarks>
    /// <para>
    /// Returns a formatted string suitable for tooltips or UI display.
    /// The description includes attack/damage modifiers, special property
    /// access, and available technique tiers.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var description = _proficiencyCheckService.GetProficiencyDescription(
    ///     WeaponProficiencyLevel.Expert);
    /// // "Expert: +1 attack, +0 damage, advanced techniques"
    /// </code>
    /// </example>
    string GetProficiencyDescription(WeaponProficiencyLevel level);
}
