// ═══════════════════════════════════════════════════════════════════════════════
// IArmorProficiencyAcquisitionService.cs
// Interface for armor proficiency training and acquisition services.
// Version: 0.16.2e
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service interface for acquiring and training armor proficiencies.
/// </summary>
/// <remarks>
/// <para>
/// This service provides the core functionality for characters to improve their
/// armor proficiency through NPC training. It handles:
/// </para>
/// <list type="bullet">
///   <item><description>Determining training requirements and costs</description></item>
///   <item><description>Validating training eligibility</description></item>
///   <item><description>Processing proficiency advancement</description></item>
///   <item><description>Managing resource deductions</description></item>
/// </list>
/// 
/// <para>
/// <strong>Training Costs by Level Transition:</strong>
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Transition</term>
///     <description>Requirements</description>
///   </listheader>
///   <item>
///     <term>NonProficient → Proficient</term>
///     <description>50 PS, 2 weeks, Level 1</description>
///   </item>
///   <item>
///     <term>Proficient → Expert</term>
///     <description>200 PS, 4 weeks, Level 5</description>
///   </item>
///   <item>
///     <term>Expert → Master</term>
///     <description>500 PS, 8 weeks, Level 10</description>
///   </item>
/// </list>
/// 
/// <para>
/// <strong>Usage Example:</strong>
/// </para>
/// <code>
/// // Check if player can train heavy armor
/// var canTrain = await acquisitionService.CanTrainAsync(
///     player, 
///     ArmorCategory.Heavy, 
///     ArmorProficiencyLevel.Proficient);
/// 
/// if (canTrain.IsEligible)
/// {
///     // Perform the training
///     var result = await acquisitionService.TrainArmorProficiencyAsync(
///         player, 
///         ArmorCategory.Heavy);
///     
///     if (result.Success)
///     {
///         // Player is now proficient with heavy armor
///         Console.WriteLine(result.Message);
///     }
/// }
/// </code>
/// </remarks>
/// <seealso cref="ArmorTrainingRequirement"/>
/// <seealso cref="ArmorTrainingResult"/>
/// <seealso cref="ArmorProficiencyLevel"/>
public interface IArmorProficiencyAcquisitionService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Training Operations
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Trains the player's armor proficiency to the next level.
    /// </summary>
    /// <param name="player">The player to train.</param>
    /// <param name="armorCategory">The armor category to train.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A <see cref="ArmorTrainingResult"/> indicating success or failure
    /// with details about resources consumed or failure reasons.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method performs the following steps:
    /// </para>
    /// <list type="number">
    ///   <item><description>Validates training eligibility</description></item>
    ///   <item><description>Calculates training requirements</description></item>
    ///   <item><description>Deducts currency cost</description></item>
    ///   <item><description>Advances proficiency level</description></item>
    ///   <item><description>Logs the training event</description></item>
    /// </list>
    /// <para>
    /// Training will fail if:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Insufficient currency</description></item>
    ///   <item><description>Character level too low</description></item>
    ///   <item><description>Already at maximum proficiency</description></item>
    ///   <item><description>No trainer available at current location</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = await service.TrainArmorProficiencyAsync(player, ArmorCategory.Heavy);
    /// if (result.Success)
    /// {
    ///     Console.WriteLine($"Trained to {result.NewLevel}! Cost: {result.ResourceSummary}");
    /// }
    /// else
    /// {
    ///     Console.WriteLine($"Training failed: {result.PrimaryFailureReason}");
    /// }
    /// </code>
    /// </example>
    Task<ArmorTrainingResult> TrainArmorProficiencyAsync(
        Player player,
        ArmorCategory armorCategory,
        CancellationToken ct = default);

    /// <summary>
    /// Trains the player's armor proficiency to a specific target level.
    /// </summary>
    /// <param name="player">The player to train.</param>
    /// <param name="armorCategory">The armor category to train.</param>
    /// <param name="targetLevel">The target proficiency level.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A <see cref="ArmorTrainingResult"/> indicating success or failure.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This overload allows specifying a target level explicitly.
    /// Training will fail if the player cannot progress to the target level
    /// in a single training session (e.g., cannot skip from NonProficient to Expert).
    /// </para>
    /// </remarks>
    Task<ArmorTrainingResult> TrainArmorProficiencyAsync(
        Player player,
        ArmorCategory armorCategory,
        ArmorProficiencyLevel targetLevel,
        CancellationToken ct = default);

    // ═══════════════════════════════════════════════════════════════════════════
    // Requirement Queries
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the training requirements for advancing to a specific proficiency level.
    /// </summary>
    /// <param name="armorCategory">The armor category.</param>
    /// <param name="targetLevel">The target proficiency level.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The <see cref="ArmorTrainingRequirement"/> for this level transition,
    /// or <c>null</c> if the level cannot be trained to (e.g., NonProficient).
    /// </returns>
    /// <remarks>
    /// <para>
    /// Requirements are determined from configuration and may vary by armor category.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var requirement = await service.GetTrainingRequirementsAsync(
    ///     ArmorCategory.Heavy, 
    ///     ArmorProficiencyLevel.Expert);
    /// 
    /// if (requirement.HasValue)
    /// {
    ///     Console.WriteLine($"Cost: {requirement.Value.FormatCostSummary()}");
    ///     Console.WriteLine($"Required Level: {requirement.Value.MinimumCharacterLevel}");
    /// }
    /// </code>
    /// </example>
    Task<ArmorTrainingRequirement?> GetTrainingRequirementsAsync(
        ArmorCategory armorCategory,
        ArmorProficiencyLevel targetLevel,
        CancellationToken ct = default);

    /// <summary>
    /// Gets all training requirements for an armor category.
    /// </summary>
    /// <param name="armorCategory">The armor category.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A dictionary mapping target levels to their training requirements.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Returns requirements for all trainable levels (Proficient, Expert, Master).
    /// </para>
    /// </remarks>
    Task<IReadOnlyDictionary<ArmorProficiencyLevel, ArmorTrainingRequirement>> 
        GetAllTrainingRequirementsAsync(
            ArmorCategory armorCategory,
            CancellationToken ct = default);

    /// <summary>
    /// Gets the next training requirement for a player's current proficiency.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="armorCategory">The armor category.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The requirement for the next level, or <c>null</c> if already at Master.
    /// </returns>
    /// <example>
    /// <code>
    /// var next = await service.GetNextTrainingRequirementAsync(player, ArmorCategory.Light);
    /// if (next.HasValue)
    /// {
    ///     Console.WriteLine($"Next: {next.Value.FormatDescription()}");
    /// }
    /// else
    /// {
    ///     Console.WriteLine("Already at maximum proficiency!");
    /// }
    /// </code>
    /// </example>
    Task<ArmorTrainingRequirement?> GetNextTrainingRequirementAsync(
        Player player,
        ArmorCategory armorCategory,
        CancellationToken ct = default);

    // ═══════════════════════════════════════════════════════════════════════════
    // Eligibility Checks
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if a player can train a specific armor proficiency.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="armorCategory">The armor category.</param>
    /// <param name="targetLevel">The target proficiency level.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A <see cref="TrainingEligibility"/> result indicating whether training
    /// is possible and any blocking reasons.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method performs a comprehensive eligibility check including:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Current proficiency level verification</description></item>
    ///   <item><description>Character level requirements</description></item>
    ///   <item><description>Currency availability</description></item>
    ///   <item><description>Trainer availability (if applicable)</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var eligibility = await service.CanTrainAsync(
    ///     player, 
    ///     ArmorCategory.Heavy, 
    ///     ArmorProficiencyLevel.Expert);
    /// 
    /// if (eligibility.IsEligible)
    /// {
    ///     Console.WriteLine("Ready to train!");
    /// }
    /// else
    /// {
    ///     foreach (var reason in eligibility.BlockingReasons)
    ///     {
    ///         Console.WriteLine($"• {reason}");
    ///     }
    /// }
    /// </code>
    /// </example>
    Task<TrainingEligibility> CanTrainAsync(
        Player player,
        ArmorCategory armorCategory,
        ArmorProficiencyLevel targetLevel,
        CancellationToken ct = default);

    /// <summary>
    /// Checks if a player can train to any next level for an armor category.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="armorCategory">The armor category.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A <see cref="TrainingEligibility"/> for the next available level.
    /// </returns>
    Task<TrainingEligibility> CanTrainNextLevelAsync(
        Player player,
        ArmorCategory armorCategory,
        CancellationToken ct = default);

    // ═══════════════════════════════════════════════════════════════════════════
    // Proficiency Queries
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a player's current proficiency level for an armor category.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="armorCategory">The armor category.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The player's current <see cref="ArmorProficiencyLevel"/> for this category.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method considers:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Archetype-granted proficiencies</description></item>
    ///   <item><description>Trained proficiencies</description></item>
    ///   <item><description>Quest/achievement-granted proficiencies</description></item>
    /// </list>
    /// <para>
    /// Returns the highest applicable proficiency from all sources.
    /// </para>
    /// </remarks>
    Task<ArmorProficiencyLevel> GetCurrentProficiencyAsync(
        Player player,
        ArmorCategory armorCategory,
        CancellationToken ct = default);

    /// <summary>
    /// Gets all armor proficiencies for a player.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A dictionary mapping armor categories to proficiency levels.
    /// </returns>
    Task<IReadOnlyDictionary<ArmorCategory, ArmorProficiencyLevel>> 
        GetAllProficienciesAsync(
            Player player,
            CancellationToken ct = default);
}

/// <summary>
/// Represents the result of a training eligibility check.
/// </summary>
/// <remarks>
/// <para>
/// Contains detailed information about whether training is possible
/// and what conditions are blocking it if not.
/// </para>
/// </remarks>
public readonly record struct TrainingEligibility
{
    /// <summary>
    /// Gets a value indicating whether the player is eligible to train.
    /// </summary>
    public bool IsEligible { get; init; }

    /// <summary>
    /// Gets the target proficiency level being checked.
    /// </summary>
    public ArmorProficiencyLevel TargetLevel { get; init; }

    /// <summary>
    /// Gets the list of reasons blocking training.
    /// </summary>
    /// <remarks>
    /// Empty if <see cref="IsEligible"/> is true.
    /// </remarks>
    public IReadOnlyList<string> BlockingReasons { get; init; }

    /// <summary>
    /// Gets a value indicating whether the player has a blocking reason.
    /// </summary>
    public bool HasBlockingReasons => BlockingReasons.Count > 0;

    /// <summary>
    /// Gets the primary blocking reason.
    /// </summary>
    public string PrimaryBlockingReason =>
        HasBlockingReasons ? BlockingReasons[0] : string.Empty;

    /// <summary>
    /// Creates an eligible result.
    /// </summary>
    /// <param name="targetLevel">The target proficiency level.</param>
    /// <returns>An eligible training result.</returns>
    public static TrainingEligibility Eligible(ArmorProficiencyLevel targetLevel)
    {
        return new TrainingEligibility
        {
            IsEligible = true,
            TargetLevel = targetLevel,
            BlockingReasons = Array.Empty<string>()
        };
    }

    /// <summary>
    /// Creates an ineligible result.
    /// </summary>
    /// <param name="targetLevel">The target proficiency level.</param>
    /// <param name="blockingReasons">The reasons training is blocked.</param>
    /// <returns>An ineligible training result.</returns>
    public static TrainingEligibility Ineligible(
        ArmorProficiencyLevel targetLevel,
        IReadOnlyList<string> blockingReasons)
    {
        return new TrainingEligibility
        {
            IsEligible = false,
            TargetLevel = targetLevel,
            BlockingReasons = blockingReasons
        };
    }

    /// <summary>
    /// Creates an ineligible result with a single reason.
    /// </summary>
    /// <param name="targetLevel">The target proficiency level.</param>
    /// <param name="reason">The blocking reason.</param>
    /// <returns>An ineligible training result.</returns>
    public static TrainingEligibility Ineligible(
        ArmorProficiencyLevel targetLevel,
        string reason)
    {
        return Ineligible(targetLevel, new[] { reason });
    }
}
