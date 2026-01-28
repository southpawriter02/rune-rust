// ═══════════════════════════════════════════════════════════════════════════════
// ArmorProficiencyLevel.cs
// Enum defining the four proficiency levels for armor usage.
// Version: 0.16.2a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the proficiency levels for armor usage.
/// </summary>
/// <remarks>
/// <para>
/// Armor proficiency levels determine how effectively a character can
/// wear armor of a specific category. Higher proficiency provides
/// penalty reductions and defensive bonuses.
/// </para>
/// <para>
/// The four levels represent a clear progression:
/// </para>
/// <list type="bullet">
///   <item><description><see cref="NonProficient"/>: Penalties doubled, -2 attack, no special properties</description></item>
///   <item><description><see cref="Proficient"/>: Baseline competency, normal penalties</description></item>
///   <item><description><see cref="Expert"/>: Tier reduction (Heavy → Medium penalties)</description></item>
///   <item><description><see cref="Master"/>: Tier reduction, +1 Defense, halved maintenance</description></item>
/// </list>
/// <para>
/// Level values are explicitly assigned (0-3) to ensure stable
/// serialization and database storage.
/// </para>
/// <para>
/// Armor penalty modifiers per level:
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Level</term>
///     <description>Effect Summary</description>
///   </listheader>
///   <item>
///     <term>NonProficient</term>
///     <description>2.0x penalty multiplier, -2 attack, no special properties</description>
///   </item>
///   <item>
///     <term>Proficient</term>
///     <description>1.0x penalty multiplier, baseline function</description>
///   </item>
///   <item>
///     <term>Expert</term>
///     <description>1.0x penalty multiplier, armor treated as one tier lighter</description>
///   </item>
///   <item>
///     <term>Master</term>
///     <description>1.0x penalty multiplier, tier reduction, +1 Defense</description>
///   </item>
/// </list>
/// </remarks>
/// <seealso cref="ValueObjects.ArmorProficiencyEffect"/>
public enum ArmorProficiencyLevel
{
    /// <summary>
    /// No training with this armor category.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Non-proficient characters suffer severely when wearing armor:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>All armor penalties doubled (2.0x multiplier)</description></item>
    ///   <item><description>-2 attack modifier</description></item>
    ///   <item><description>Cannot activate armor special properties (fire resistance, etc.)</description></item>
    ///   <item><description>No access to armor-based defensive techniques</description></item>
    /// </list>
    /// <para>
    /// Characters wearing armor outside their proficiency set should expect
    /// significantly reduced effectiveness. Consider training with NPCs,
    /// gaining combat experience (15 combats), or spending Progression Points
    /// (3 PP) to acquire proficiency.
    /// </para>
    /// </remarks>
    NonProficient = 0,

    /// <summary>
    /// Basic training with this armor category.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Proficient characters have completed basic armor training and can wear
    /// armor at full effectiveness:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Standard armor penalties apply (1.0x multiplier)</description></item>
    ///   <item><description>Full access to armor special properties</description></item>
    ///   <item><description>Basic defensive techniques available</description></item>
    /// </list>
    /// <para>
    /// This is the baseline proficiency level for most archetype armor
    /// categories. Warriors start proficient with all armor, while other
    /// archetypes have more limited starting proficiencies (Mystic: Light only).
    /// </para>
    /// </remarks>
    Proficient = 1,

    /// <summary>
    /// Advanced training with this armor category.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Expert characters have mastered advanced armor techniques and gain
    /// improved performance:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Armor treated as one tier lighter for penalty calculations</description></item>
    ///   <item><description>Heavy armor uses Medium-tier penalties</description></item>
    ///   <item><description>Full access to armor special properties</description></item>
    ///   <item><description>Advanced defensive techniques available</description></item>
    /// </list>
    /// <para>
    /// Expert proficiency typically requires significant combat experience
    /// (25+ combats wearing the armor category) or specialized training from
    /// master armorers.
    /// </para>
    /// </remarks>
    Expert = 2,

    /// <summary>
    /// Mastery of this armor category.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Master characters have achieved peak proficiency with this armor
    /// category and gain significant defensive bonuses:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Tier reduction like Expert (Heavy → Medium penalties)</description></item>
    ///   <item><description>+1 Defense bonus</description></item>
    ///   <item><description>Halved armor maintenance costs</description></item>
    ///   <item><description>Full access to armor special properties</description></item>
    ///   <item><description>Signature defensive techniques unlocked</description></item>
    /// </list>
    /// <para>
    /// Master proficiency represents the pinnacle of armor skill. It requires
    /// extensive combat experience (50+ combats) or training with legendary
    /// armor masters.
    /// </para>
    /// </remarks>
    Master = 3
}
