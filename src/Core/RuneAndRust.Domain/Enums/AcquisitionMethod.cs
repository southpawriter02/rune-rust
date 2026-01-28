// ═══════════════════════════════════════════════════════════════════════════════
// AcquisitionMethod.cs
// Enum defining the methods for acquiring or advancing weapon proficiencies.
// Version: 0.16.1e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the methods by which a character can acquire or advance weapon proficiencies.
/// </summary>
/// <remarks>
/// <para>
/// The proficiency acquisition system supports five distinct methods, each with
/// different gameplay implications, costs, and availability:
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Method</term>
///     <description>Description and Cost</description>
///   </listheader>
///   <item>
///     <term>Archetype</term>
///     <description>Free starting proficiencies based on character archetype</description>
///   </item>
///   <item>
///     <term>Specialization</term>
///     <description>Free proficiencies granted by specialization choice</description>
///   </item>
///   <item>
///     <term>NpcTraining</term>
///     <description>Requires time (weeks) and currency (Pieces Silver)</description>
///   </item>
///   <item>
///     <term>CombatExperience</term>
///     <description>Free but requires extensive weapon usage (10-50 combats)</description>
///   </item>
///   <item>
///     <term>ProgressionPointPurchase</term>
///     <description>Instant advancement using Progression Points currency</description>
///   </item>
/// </list>
/// <para>
/// Values are explicitly assigned (0-4) to ensure stable serialization and
/// database storage. Do not change existing values.
/// </para>
/// </remarks>
/// <seealso cref="ValueObjects.ProficiencyGainResult"/>
/// <seealso cref="ValueObjects.TrainingResult"/>
/// <seealso cref="ValueObjects.AcquisitionCost"/>
public enum AcquisitionMethod
{
    /// <summary>
    /// Proficiency granted from character's starting archetype.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Archetype proficiencies are applied during character creation and represent
    /// the character's foundational weapon training. Each archetype has a predefined
    /// set of proficient categories:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Warrior: All 11 weapon categories</description></item>
    ///   <item><description>Skirmisher: 5 categories (Daggers, Swords, Axes, Bows, Crossbows)</description></item>
    ///   <item><description>Mystic: 3 categories (Daggers, Staves, ArcaneImplements)</description></item>
    ///   <item><description>Adept: 4 categories (Daggers, Staves, Hammers, Crossbows)</description></item>
    /// </list>
    /// <para>
    /// This method has no cost — proficiencies are granted automatically.
    /// </para>
    /// </remarks>
    Archetype = 0,

    /// <summary>
    /// Proficiency granted from a specialization choice.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Specializations may grant additional weapon proficiencies or advance existing
    /// ones. For example, a "Blademaster" specialization might grant Expert proficiency
    /// with Swords.
    /// </para>
    /// <para>
    /// This method has no direct cost — the specialization choice itself is the cost.
    /// </para>
    /// </remarks>
    Specialization = 1,

    /// <summary>
    /// Proficiency acquired through training with an NPC trainer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// NPC training requires both time and currency. Training costs scale with the
    /// target proficiency level:
    /// </para>
    /// <list type="table">
    ///   <listheader>
    ///     <term>Advancement</term>
    ///     <description>Time / Cost</description>
    ///   </listheader>
    ///   <item>
    ///     <term>NonProficient → Proficient</term>
    ///     <description>2 weeks / 50 PS</description>
    ///   </item>
    ///   <item>
    ///     <term>Proficient → Expert</term>
    ///     <description>4 weeks / 150 PS</description>
    ///   </item>
    ///   <item>
    ///     <term>Expert → Master</term>
    ///     <description>8 weeks / 400 PS</description>
    ///   </item>
    /// </list>
    /// <para>
    /// NPC trainers may have prerequisites (e.g., faction standing) and may not
    /// be available in all locations.
    /// </para>
    /// </remarks>
    NpcTraining = 2,

    /// <summary>
    /// Proficiency advanced through combat experience with the weapon category.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Characters gain combat experience by using weapons in actual combat. After
    /// reaching the experience threshold for their current level, they can advance
    /// to the next proficiency level.
    /// </para>
    /// <para>
    /// Default experience thresholds:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>NonProficient → Proficient: 10 combats</description></item>
    ///   <item><description>Proficient → Expert: 25 combats</description></item>
    ///   <item><description>Expert → Master: 50 combats</description></item>
    /// </list>
    /// <para>
    /// This method has no currency cost but requires significant gameplay investment.
    /// </para>
    /// </remarks>
    CombatExperience = 3,

    /// <summary>
    /// Proficiency purchased using Progression Points (PP).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Progression Points are a meta-currency earned through gameplay achievements.
    /// PP purchases provide instant proficiency advancement without time investment.
    /// </para>
    /// <para>
    /// Default PP costs:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>NonProficient → Proficient: 2 PP</description></item>
    ///   <item><description>Proficient → Expert: 2 PP</description></item>
    ///   <item><description>Expert → Master: 2 PP</description></item>
    /// </list>
    /// <para>
    /// PP is a valuable resource, so players should consider whether combat
    /// experience or NPC training might be more efficient.
    /// </para>
    /// </remarks>
    ProgressionPointPurchase = 4
}
