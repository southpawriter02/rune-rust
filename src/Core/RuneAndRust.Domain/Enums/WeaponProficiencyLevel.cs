// ═══════════════════════════════════════════════════════════════════════════════
// WeaponProficiencyLevel.cs
// Enum defining the four proficiency levels for weapon usage.
// Version: 0.16.1a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the proficiency levels for weapon usage.
/// </summary>
/// <remarks>
/// <para>
/// Weapon proficiency levels determine how effectively a character can
/// wield weapons of a specific category. Higher proficiency provides
/// combat bonuses and unlocks advanced techniques.
/// </para>
/// <para>
/// The four levels represent a clear progression:
/// </para>
/// <list type="bullet">
///   <item><description><see cref="NonProficient"/>: Severely hampered, cannot use special properties</description></item>
///   <item><description><see cref="Proficient"/>: Baseline competency, full weapon functionality</description></item>
///   <item><description><see cref="Expert"/>: Enhanced attack bonus, advanced techniques available</description></item>
///   <item><description><see cref="Master"/>: Peak performance, signature techniques unlocked</description></item>
/// </list>
/// <para>
/// Level values are explicitly assigned (0-3) to ensure stable
/// serialization and database storage.
/// </para>
/// <para>
/// Combat modifiers per level:
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Level</term>
///     <description>Attack/Damage Modifiers</description>
///   </listheader>
///   <item>
///     <term>NonProficient</term>
///     <description>-3 attack, -2 damage, no special properties</description>
///   </item>
///   <item>
///     <term>Proficient</term>
///     <description>+0 attack, +0 damage (baseline)</description>
///   </item>
///   <item>
///     <term>Expert</term>
///     <description>+1 attack, +0 damage</description>
///   </item>
///   <item>
///     <term>Master</term>
///     <description>+2 attack, +1 damage</description>
///   </item>
/// </list>
/// </remarks>
/// <seealso cref="TechniqueAccess"/>
/// <seealso cref="ValueObjects.ProficiencyEffect"/>
public enum WeaponProficiencyLevel
{
    /// <summary>
    /// No training with this weapon category.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Non-proficient characters suffer severe combat penalties:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>-3 attack modifier</description></item>
    ///   <item><description>-2 damage modifier</description></item>
    ///   <item><description>Cannot activate weapon special properties (lifesteal, cleave, etc.)</description></item>
    ///   <item><description>No access to combat techniques requiring training</description></item>
    /// </list>
    /// <para>
    /// Characters using weapons outside their proficiency set should expect
    /// significantly reduced combat effectiveness. Consider training with
    /// NPCs, gaining combat experience, or spending Progression Points to
    /// acquire proficiency.
    /// </para>
    /// </remarks>
    NonProficient = 0,

    /// <summary>
    /// Basic training with this weapon category.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Proficient characters have completed basic training and can use
    /// weapons at full effectiveness:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>No attack or damage penalties</description></item>
    ///   <item><description>Full access to weapon special properties</description></item>
    ///   <item><description>Basic combat techniques available</description></item>
    /// </list>
    /// <para>
    /// This is the baseline proficiency level for most archetype weapon
    /// categories. Warriors start proficient with all weapon categories,
    /// while other archetypes have more limited starting proficiencies.
    /// </para>
    /// </remarks>
    Proficient = 1,

    /// <summary>
    /// Advanced training with this weapon category.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Expert characters have mastered advanced techniques and gain
    /// improved combat performance:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>+1 attack modifier</description></item>
    ///   <item><description>Full access to weapon special properties</description></item>
    ///   <item><description>Advanced combat techniques unlocked</description></item>
    /// </list>
    /// <para>
    /// Expert proficiency typically requires significant combat experience
    /// (25+ combats with the weapon category) or specialized training from
    /// master weapon trainers.
    /// </para>
    /// </remarks>
    Expert = 2,

    /// <summary>
    /// Mastery of this weapon category.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Master characters have achieved peak proficiency with this weapon
    /// category and gain significant combat bonuses:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>+2 attack modifier</description></item>
    ///   <item><description>+1 damage modifier</description></item>
    ///   <item><description>Full access to weapon special properties</description></item>
    ///   <item><description>Signature techniques that define combat style</description></item>
    /// </list>
    /// <para>
    /// Master proficiency represents the pinnacle of weapon skill. It
    /// requires extensive combat experience (50+ combats) or training
    /// with legendary weapon masters.
    /// </para>
    /// </remarks>
    Master = 3
}
