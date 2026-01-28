// ═══════════════════════════════════════════════════════════════════════════════
// IProficiencyAcquisitionService.cs
// Interface defining the service for acquiring and advancing weapon proficiencies.
// Version: 0.16.1e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Defines the service for acquiring and advancing weapon proficiencies.
/// </summary>
/// <remarks>
/// <para>
/// The Proficiency Acquisition Service provides a centralized interface for all
/// methods of gaining and advancing weapon category proficiencies. It supports
/// five distinct acquisition methods:
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Method</term>
///     <description>Description</description>
///   </listheader>
///   <item>
///     <term><see cref="InitializeFromArchetypeAsync"/></term>
///     <description>Set starting proficiencies based on archetype selection</description>
///   </item>
///   <item>
///     <term><see cref="GrantFromSpecializationAsync"/></term>
///     <description>Grant proficiencies from specialization choice</description>
///   </item>
///   <item>
///     <term><see cref="RecordCombatExperienceAsync"/></term>
///     <description>Track weapon usage and trigger advancement when threshold is reached</description>
///   </item>
///   <item>
///     <term><see cref="PurchaseWithProgressionPointsAsync"/></term>
///     <description>Instant advancement using Progression Points currency</description>
///   </item>
///   <item>
///     <term><see cref="TrainWithNPCAsync"/></term>
///     <description>Advancement through NPC trainer (costs time and currency)</description>
///   </item>
/// </list>
/// <para>
/// Implementation responsibilities:
/// </para>
/// <list type="bullet">
///   <item><description>Validate prerequisites for each acquisition method</description></item>
///   <item><description>Manage costs and resource deduction</description></item>
///   <item><description>Update character proficiency state via <see cref="CharacterProficiencies"/></description></item>
///   <item><description>Return structured results for UI/logging consumption</description></item>
/// </list>
/// <para>
/// Logging requirements for implementations:
/// </para>
/// <list type="bullet">
///   <item><description>Information: Successful acquisitions with method and level change</description></item>
///   <item><description>Debug: Validation checks and cost calculations</description></item>
///   <item><description>Warning: Failed acquisitions with reason</description></item>
///   <item><description>Error: Unexpected failures during processing</description></item>
/// </list>
/// </remarks>
/// <seealso cref="AcquisitionMethod"/>
/// <seealso cref="ProficiencyGainResult"/>
/// <seealso cref="TrainingResult"/>
/// <seealso cref="AcquisitionCost"/>
/// <seealso cref="CharacterProficiencies"/>
public interface IProficiencyAcquisitionService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Archetype and Specialization Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a character's proficiencies based on their archetype.
    /// </summary>
    /// <param name="proficiencies">The character proficiencies to initialize.</param>
    /// <param name="archetypeId">The archetype identifier (e.g., "warrior", "mystic").</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A task containing a read-only list of <see cref="ProficiencyGainResult"/> for
    /// each weapon category, indicating which were set to Proficient and which remain
    /// NonProficient.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method should be called during character creation to set initial
    /// proficiency levels based on the selected archetype.
    /// </para>
    /// <para>
    /// Archetype proficiency counts:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Warrior: 11 categories (all)</description></item>
    ///   <item><description>Skirmisher: 5 categories</description></item>
    ///   <item><description>Adept: 4 categories</description></item>
    ///   <item><description>Mystic: 3 categories</description></item>
    /// </list>
    /// <para>
    /// Logging: Should log Information-level message with archetype ID and
    /// count of proficiencies granted.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var results = await _acquisitionService.InitializeFromArchetypeAsync(
    ///     character, "warrior", CancellationToken.None);
    /// 
    /// var proficientCount = results.Count(r => r.NewLevel == WeaponProficiencyLevel.Proficient);
    /// Console.WriteLine($"Granted {proficientCount} proficiencies");
    /// </code>
    /// </example>
    Task<IReadOnlyList<ProficiencyGainResult>> InitializeFromArchetypeAsync(
        CharacterProficiencies proficiencies,
        string archetypeId,
        CancellationToken ct = default);

    /// <summary>
    /// Grants proficiencies from a specialization choice.
    /// </summary>
    /// <param name="proficiencies">The character proficiencies receiving the proficiencies.</param>
    /// <param name="categories">The weapon categories to grant proficiency in.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A task containing a read-only list of <see cref="ProficiencyGainResult"/> for
    /// each granted category.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Specializations may grant new proficiencies or advance existing ones.
    /// Categories where the character already has equal or higher proficiency
    /// will result in a success with no level change.
    /// </para>
    /// <para>
    /// Logging: Should log Information-level message with specialization details
    /// and list of categories affected.
    /// </para>
    /// </remarks>
    Task<IReadOnlyList<ProficiencyGainResult>> GrantFromSpecializationAsync(
        CharacterProficiencies proficiencies,
        IEnumerable<WeaponCategory> categories,
        CancellationToken ct = default);

    // ═══════════════════════════════════════════════════════════════════════════
    // Combat Experience Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Records combat experience for a weapon category.
    /// </summary>
    /// <param name="proficiencies">The character proficiencies gaining experience.</param>
    /// <param name="category">The weapon category used in combat.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A task containing a <see cref="ProficiencyGainResult"/> indicating whether
    /// a level-up occurred.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Each call increments the combat experience counter for the specified category.
    /// When the experience threshold is reached, the proficiency level automatically
    /// advances and experience resets to 0.
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
    /// At Master level, experience is still tracked but no further advancement occurs.
    /// </para>
    /// <para>
    /// Logging:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Debug: Experience recorded with current progress</description></item>
    ///   <item><description>Information: Level-up when threshold is reached</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = await _acquisitionService.RecordCombatExperienceAsync(
    ///     character, WeaponCategory.Swords, CancellationToken.None);
    /// 
    /// if (result.LevelChanged)
    /// {
    ///     Console.WriteLine($"Advanced to {result.NewLevel}!");
    /// }
    /// </code>
    /// </example>
    Task<ProficiencyGainResult> RecordCombatExperienceAsync(
        CharacterProficiencies proficiencies,
        WeaponCategory category,
        CancellationToken ct = default);

    // ═══════════════════════════════════════════════════════════════════════════
    // Progression Point Purchase Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Purchases a proficiency advancement using Progression Points.
    /// </summary>
    /// <param name="proficiencies">The character proficiencies for the purchase.</param>
    /// <param name="category">The weapon category to advance.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A task containing a <see cref="ProficiencyGainResult"/> indicating the outcome.
    /// </returns>
    /// <remarks>
    /// <para>
    /// PP purchases provide instant advancement without time investment. The PP cost
    /// is configured per level transition (default: 2 PP per level).
    /// </para>
    /// <para>
    /// Prerequisites:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Character must have sufficient PP</description></item>
    ///   <item><description>Category must not already be at Master level</description></item>
    /// </list>
    /// <para>
    /// On success, PP is deducted from the character and proficiency is advanced.
    /// On failure, no PP is deducted and the result contains the failure reason.
    /// </para>
    /// <para>
    /// Logging:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Debug: PP balance check</description></item>
    ///   <item><description>Information: Successful purchase with PP spent</description></item>
    ///   <item><description>Warning: Failed purchase with reason</description></item>
    /// </list>
    /// </remarks>
    Task<ProficiencyGainResult> PurchaseWithProgressionPointsAsync(
        CharacterProficiencies proficiencies,
        WeaponCategory category,
        CancellationToken ct = default);

    /// <summary>
    /// Checks whether a character can afford a PP purchase for a category.
    /// </summary>
    /// <param name="proficiencies">The character proficiencies to check.</param>
    /// <param name="category">The weapon category to check.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A task containing <c>true</c> if the character has sufficient PP and the
    /// category is not at Master level; <c>false</c> otherwise.
    /// </returns>
    /// <remarks>
    /// This is a non-mutating check useful for UI display (e.g., enabling/disabling
    /// purchase buttons).
    /// </remarks>
    Task<bool> CanAffordPPPurchaseAsync(
        CharacterProficiencies proficiencies,
        WeaponCategory category,
        CancellationToken ct = default);

    // ═══════════════════════════════════════════════════════════════════════════
    // NPC Training Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Trains a proficiency with an NPC trainer.
    /// </summary>
    /// <param name="proficiencies">The character proficiencies undergoing training.</param>
    /// <param name="category">The weapon category to train.</param>
    /// <param name="trainerId">The unique identifier of the NPC trainer.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A task containing a <see cref="TrainingResult"/> with full training details.
    /// </returns>
    /// <remarks>
    /// <para>
    /// NPC training requires both currency (Pieces Silver) and time (weeks).
    /// Costs scale with the target proficiency level:
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
    /// Prerequisites:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Character must have sufficient currency</description></item>
    ///   <item><description>Category must not already be at Master level</description></item>
    ///   <item><description>Trainer must be valid and available</description></item>
    /// </list>
    /// <para>
    /// On success, currency is deducted, time passes, and proficiency advances.
    /// On failure, no resources are consumed.
    /// </para>
    /// <para>
    /// Logging:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Debug: Cost calculation and prerequisite checks</description></item>
    ///   <item><description>Information: Successful training with duration and cost</description></item>
    ///   <item><description>Warning: Failed training with reason</description></item>
    /// </list>
    /// </remarks>
    Task<TrainingResult> TrainWithNPCAsync(
        CharacterProficiencies proficiencies,
        WeaponCategory category,
        string trainerId,
        CancellationToken ct = default);

    /// <summary>
    /// Gets the training cost for advancing a proficiency with an NPC.
    /// </summary>
    /// <param name="proficiencies">The character proficiencies to check.</param>
    /// <param name="category">The weapon category to check.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A task containing an <see cref="AcquisitionCost"/> with the PS and time
    /// requirements. Returns <see cref="AcquisitionCost.None"/> if the category
    /// is already at Master level.
    /// </returns>
    /// <remarks>
    /// This is a non-mutating query useful for UI display (e.g., showing training
    /// costs before the player commits).
    /// </remarks>
    /// <example>
    /// <code>
    /// var cost = await _acquisitionService.GetTrainingCostAsync(
    ///     character, WeaponCategory.Swords, CancellationToken.None);
    /// 
    /// Console.WriteLine($"Training costs {cost.PiecesSilver} PS and takes {cost.TrainingWeeks} weeks");
    /// </code>
    /// </example>
    Task<AcquisitionCost> GetTrainingCostAsync(
        CharacterProficiencies proficiencies,
        WeaponCategory category,
        CancellationToken ct = default);

    /// <summary>
    /// Checks whether a character can afford NPC training for a category.
    /// </summary>
    /// <param name="proficiencies">The character proficiencies to check.</param>
    /// <param name="category">The weapon category to check.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A task containing <c>true</c> if the character has sufficient currency
    /// and the category is not at Master level; <c>false</c> otherwise.
    /// </returns>
    /// <remarks>
    /// This check does not validate trainer availability — only resource requirements.
    /// </remarks>
    Task<bool> CanAffordTrainingAsync(
        CharacterProficiencies proficiencies,
        WeaponCategory category,
        CancellationToken ct = default);
}
