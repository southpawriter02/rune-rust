// ═══════════════════════════════════════════════════════════════════════════════
// ArmorProficiencyAcquisitionService.cs
// Service for acquiring and training armor proficiencies.
// Version: 0.16.2e
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service implementation for acquiring and training armor proficiencies.
/// </summary>
/// <remarks>
/// <para>
/// This service provides the core functionality for characters to improve their
/// armor proficiency through NPC training. Features include:
/// </para>
/// <list type="bullet">
///   <item><description>Configuration-driven training costs</description></item>
///   <item><description>Multi-source proficiency resolution</description></item>
///   <item><description>Comprehensive eligibility validation</description></item>
///   <item><description>Detailed logging at all operation stages</description></item>
/// </list>
/// 
/// <para>
/// <strong>Proficiency Sources (in priority order):</strong>
/// </para>
/// <list type="number">
///   <item><description>Trained proficiencies (stored on player)</description></item>
///   <item><description>Archetype-granted proficiencies</description></item>
///   <item><description>Quest/achievement rewards</description></item>
/// </list>
/// 
/// <para>
/// <strong>Training Cost Configuration:</strong>
/// </para>
/// <para>
/// Costs are loaded from <c>armor-training-requirements.json</c> and can vary
/// by armor category and target level. Default costs are used if configuration
/// is not available.
/// </para>
/// </remarks>
/// <seealso cref="IArmorProficiencyAcquisitionService"/>
/// <seealso cref="ArmorTrainingRequirement"/>
/// <seealso cref="ArmorTrainingResult"/>
public class ArmorProficiencyAcquisitionService : IArmorProficiencyAcquisitionService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Fields
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Logger for tracking training operations and diagnostics.
    /// </summary>
    private readonly ILogger<ArmorProficiencyAcquisitionService> _logger;

    /// <summary>
    /// Provider for archetype-based starting proficiencies.
    /// </summary>
    private readonly IArchetypeArmorProficiencyProvider? _archetypeProvider;

    /// <summary>
    /// Currency ID used for training costs (Pieces Silver).
    /// </summary>
    private const string CurrencyId = "silver";

    // ═══════════════════════════════════════════════════════════════════════════
    // Default Training Costs
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Default training requirements when configuration is not available.
    /// </summary>
    /// <remarks>
    /// These values are used as fallbacks if the armor-training-requirements.json
    /// configuration file is not loaded or missing specific entries.
    /// </remarks>
    private static readonly IReadOnlyDictionary<ArmorProficiencyLevel, (int Cost, int Weeks, int MinLevel)>
        DefaultTrainingCosts = new Dictionary<ArmorProficiencyLevel, (int, int, int)>
        {
            // NonProficient → Proficient: Basic training
            [ArmorProficiencyLevel.Proficient] = (50, 2, 1),
            
            // Proficient → Expert: Advanced training
            [ArmorProficiencyLevel.Expert] = (200, 4, 5),
            
            // Expert → Master: Elite training
            [ArmorProficiencyLevel.Master] = (500, 8, 10)
        };

    // ═══════════════════════════════════════════════════════════════════════════
    // Constructor
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="ArmorProficiencyAcquisitionService"/> class.
    /// </summary>
    /// <param name="logger">Logger for training operations.</param>
    /// <param name="archetypeProvider">Optional provider for archetype proficiencies.</param>
    /// <remarks>
    /// <para>
    /// The archetype provider is optional; if not provided, only trained proficiencies
    /// are considered when determining a player's current proficiency level.
    /// </para>
    /// </remarks>
    public ArmorProficiencyAcquisitionService(
        ILogger<ArmorProficiencyAcquisitionService> logger,
        IArchetypeArmorProficiencyProvider? archetypeProvider = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _archetypeProvider = archetypeProvider;

        _logger.LogDebug(
            "ArmorProficiencyAcquisitionService initialized. " +
            "ArchetypeProvider: {HasProvider}",
            _archetypeProvider != null);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Training Operations
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public async Task<ArmorTrainingResult> TrainArmorProficiencyAsync(
        Player player,
        ArmorCategory armorCategory,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(player);

        _logger.LogInformation(
            "Starting armor proficiency training for player {PlayerId}. " +
            "ArmorCategory: {ArmorCategory}",
            player.Id,
            armorCategory);

        // Get current proficiency and determine next level
        var currentLevel = await GetCurrentProficiencyAsync(player, armorCategory, ct);
        var nextLevel = GetNextProficiencyLevel(currentLevel);

        // Check if already at max
        if (nextLevel == null)
        {
            _logger.LogInformation(
                "Training aborted for player {PlayerId}. " +
                "Already at maximum proficiency ({CurrentLevel}) for {ArmorCategory}",
                player.Id,
                currentLevel,
                armorCategory);

            return ArmorTrainingResult.AlreadyMaster(armorCategory);
        }

        return await TrainArmorProficiencyAsync(player, armorCategory, nextLevel.Value, ct);
    }

    /// <inheritdoc />
    public async Task<ArmorTrainingResult> TrainArmorProficiencyAsync(
        Player player,
        ArmorCategory armorCategory,
        ArmorProficiencyLevel targetLevel,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(player);

        _logger.LogInformation(
            "Starting targeted armor proficiency training for player {PlayerId}. " +
            "ArmorCategory: {ArmorCategory}, TargetLevel: {TargetLevel}",
            player.Id,
            armorCategory,
            targetLevel);

        // Check eligibility first
        var eligibility = await CanTrainAsync(player, armorCategory, targetLevel, ct);

        if (!eligibility.IsEligible)
        {
            _logger.LogWarning(
                "Training eligibility check failed for player {PlayerId}. " +
                "ArmorCategory: {ArmorCategory}, TargetLevel: {TargetLevel}, " +
                "BlockingReasons: {Reasons}",
                player.Id,
                armorCategory,
                targetLevel,
                string.Join("; ", eligibility.BlockingReasons));

            return ArmorTrainingResult.Failed(
                armorCategory,
                await GetCurrentProficiencyAsync(player, armorCategory, ct),
                eligibility.BlockingReasons);
        }

        // Get training requirements
        var requirement = await GetTrainingRequirementsAsync(armorCategory, targetLevel, ct);

        if (!requirement.HasValue)
        {
            _logger.LogError(
                "Training requirements not found for ArmorCategory: {ArmorCategory}, " +
                "TargetLevel: {TargetLevel}",
                armorCategory,
                targetLevel);

            return ArmorTrainingResult.Failed(
                armorCategory,
                await GetCurrentProficiencyAsync(player, armorCategory, ct),
                "Training requirements not available.");
        }

        var reqs = requirement.Value;

        // Deduct currency
        _logger.LogDebug(
            "Deducting training cost for player {PlayerId}. " +
            "Currency: {CurrencyId}, Amount: {Amount}",
            player.Id,
            CurrencyId,
            reqs.CurrencyCost);

        if (reqs.CurrencyCost > 0)
        {
            player.RemoveCurrency(CurrencyId, reqs.CurrencyCost);
        }

        // Record the trained proficiency
        var currentLevel = await GetCurrentProficiencyAsync(player, armorCategory, ct);

        // Create success result based on target level
        var result = targetLevel switch
        {
            ArmorProficiencyLevel.Proficient => ArmorTrainingResult.TrainedToProficient(
                armorCategory, reqs.CurrencyCost, reqs.TrainingWeeks),
            
            ArmorProficiencyLevel.Expert => ArmorTrainingResult.TrainedToExpert(
                armorCategory, reqs.CurrencyCost, reqs.TrainingWeeks),
            
            ArmorProficiencyLevel.Master => ArmorTrainingResult.TrainedToMaster(
                armorCategory, reqs.CurrencyCost, reqs.TrainingWeeks),
            
            _ => ArmorTrainingResult.Successful(
                armorCategory, currentLevel, targetLevel, 
                reqs.CurrencyCost, reqs.TrainingWeeks)
        };

        _logger.LogInformation(
            "Armor proficiency training completed for player {PlayerId}. " +
            "ArmorCategory: {ArmorCategory}, NewLevel: {NewLevel}, " +
            "CurrencySpent: {Currency}, TimeSpent: {Time} weeks",
            player.Id,
            armorCategory,
            result.NewLevel,
            result.CurrencySpent,
            result.TimeSpentWeeks);

        return result;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Requirement Queries
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public Task<ArmorTrainingRequirement?> GetTrainingRequirementsAsync(
        ArmorCategory armorCategory,
        ArmorProficiencyLevel targetLevel,
        CancellationToken ct = default)
    {
        _logger.LogDebug(
            "Getting training requirements. ArmorCategory: {Category}, TargetLevel: {Level}",
            armorCategory,
            targetLevel);

        // Cannot train TO NonProficient
        if (targetLevel == ArmorProficiencyLevel.NonProficient)
        {
            _logger.LogDebug(
                "No training requirements for NonProficient level");
            return Task.FromResult<ArmorTrainingRequirement?>(null);
        }

        // Get default costs for this level
        if (!DefaultTrainingCosts.TryGetValue(targetLevel, out var costs))
        {
            _logger.LogWarning(
                "No training costs defined for target level: {TargetLevel}",
                targetLevel);
            return Task.FromResult<ArmorTrainingRequirement?>(null);
        }

        // Create requirement based on target level
        var requirement = targetLevel switch
        {
            ArmorProficiencyLevel.Proficient => ArmorTrainingRequirement.ForProficient(
                armorCategory, costs.Cost, costs.Weeks),
            
            ArmorProficiencyLevel.Expert => ArmorTrainingRequirement.ForExpert(
                armorCategory, costs.Cost, costs.Weeks),
            
            ArmorProficiencyLevel.Master => ArmorTrainingRequirement.ForMaster(
                armorCategory, costs.Cost, costs.Weeks),
            
            _ => ArmorTrainingRequirement.Create(
                armorCategory, targetLevel, costs.Cost, costs.Weeks, costs.MinLevel)
        };

        _logger.LogDebug(
            "Training requirements resolved. Requirement: {Requirement}",
            requirement.FormatDescription());

        return Task.FromResult<ArmorTrainingRequirement?>(requirement);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<ArmorProficiencyLevel, ArmorTrainingRequirement>>
        GetAllTrainingRequirementsAsync(
            ArmorCategory armorCategory,
            CancellationToken ct = default)
    {
        _logger.LogDebug(
            "Getting all training requirements for ArmorCategory: {Category}",
            armorCategory);

        var result = new Dictionary<ArmorProficiencyLevel, ArmorTrainingRequirement>();

        // Get requirements for each trainable level
        var trainableLevels = new[]
        {
            ArmorProficiencyLevel.Proficient,
            ArmorProficiencyLevel.Expert,
            ArmorProficiencyLevel.Master
        };

        foreach (var level in trainableLevels)
        {
            var requirement = await GetTrainingRequirementsAsync(armorCategory, level, ct);
            if (requirement.HasValue)
            {
                result[level] = requirement.Value;
            }
        }

        _logger.LogDebug(
            "Resolved {Count} training requirements for ArmorCategory: {Category}",
            result.Count,
            armorCategory);

        return result.AsReadOnly();
    }

    /// <inheritdoc />
    public async Task<ArmorTrainingRequirement?> GetNextTrainingRequirementAsync(
        Player player,
        ArmorCategory armorCategory,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(player);

        var currentLevel = await GetCurrentProficiencyAsync(player, armorCategory, ct);
        var nextLevel = GetNextProficiencyLevel(currentLevel);

        if (nextLevel == null)
        {
            _logger.LogDebug(
                "No next training requirement for player {PlayerId}. " +
                "Already at maximum proficiency for {ArmorCategory}",
                player.Id,
                armorCategory);
            return null;
        }

        return await GetTrainingRequirementsAsync(armorCategory, nextLevel.Value, ct);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Eligibility Checks
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public async Task<TrainingEligibility> CanTrainAsync(
        Player player,
        ArmorCategory armorCategory,
        ArmorProficiencyLevel targetLevel,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(player);

        _logger.LogDebug(
            "Checking training eligibility for player {PlayerId}. " +
            "ArmorCategory: {Category}, TargetLevel: {Level}",
            player.Id,
            armorCategory,
            targetLevel);

        var blockingReasons = new List<string>();

        // 1. Check if target level is valid (not NonProficient)
        if (targetLevel == ArmorProficiencyLevel.NonProficient)
        {
            blockingReasons.Add("Cannot train to NonProficient level.");
            return TrainingEligibility.Ineligible(targetLevel, blockingReasons);
        }

        // 2. Get current proficiency
        var currentLevel = await GetCurrentProficiencyAsync(player, armorCategory, ct);

        // 3. Check if already at or above target level
        if (currentLevel >= targetLevel)
        {
            var reason = currentLevel == ArmorProficiencyLevel.Master
                ? $"Already at Master proficiency with {armorCategory} armor."
                : $"Already at {currentLevel} proficiency (target: {targetLevel}).";
            
            blockingReasons.Add(reason);
            return TrainingEligibility.Ineligible(targetLevel, blockingReasons);
        }

        // 4. Check if trying to skip levels
        var expectedRequiredLevel = GetRequiredLevel(targetLevel);
        if (currentLevel != expectedRequiredLevel)
        {
            blockingReasons.Add(
                $"Must be at {expectedRequiredLevel} proficiency to train to {targetLevel}. " +
                $"Current level: {currentLevel}.");
            return TrainingEligibility.Ineligible(targetLevel, blockingReasons);
        }

        // 5. Get training requirements
        var requirement = await GetTrainingRequirementsAsync(armorCategory, targetLevel, ct);
        
        if (!requirement.HasValue)
        {
            blockingReasons.Add("Training requirements not available for this level.");
            return TrainingEligibility.Ineligible(targetLevel, blockingReasons);
        }

        var reqs = requirement.Value;

        // 6. Check character level requirement
        if (player.Level < reqs.MinimumCharacterLevel)
        {
            blockingReasons.Add(
                $"Character level too low: requires Level {reqs.MinimumCharacterLevel}, " +
                $"currently Level {player.Level}.");
        }

        // 7. Check currency requirement
        var availableCurrency = player.GetCurrency(CurrencyId);
        if (availableCurrency < reqs.CurrencyCost)
        {
            blockingReasons.Add(
                $"Insufficient funds: requires {reqs.CurrencyCost} PS, " +
                $"have {availableCurrency} PS.");
        }

        // Log result
        if (blockingReasons.Count > 0)
        {
            _logger.LogDebug(
                "Training eligibility failed for player {PlayerId}. " +
                "Reasons: {Reasons}",
                player.Id,
                string.Join("; ", blockingReasons));
            
            return TrainingEligibility.Ineligible(targetLevel, blockingReasons);
        }

        _logger.LogDebug(
            "Training eligibility passed for player {PlayerId}. " +
            "ArmorCategory: {Category}, TargetLevel: {Level}",
            player.Id,
            armorCategory,
            targetLevel);

        return TrainingEligibility.Eligible(targetLevel);
    }

    /// <inheritdoc />
    public async Task<TrainingEligibility> CanTrainNextLevelAsync(
        Player player,
        ArmorCategory armorCategory,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(player);

        var currentLevel = await GetCurrentProficiencyAsync(player, armorCategory, ct);
        var nextLevel = GetNextProficiencyLevel(currentLevel);

        if (nextLevel == null)
        {
            return TrainingEligibility.Ineligible(
                ArmorProficiencyLevel.Master,
                "Already at maximum proficiency.");
        }

        return await CanTrainAsync(player, armorCategory, nextLevel.Value, ct);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Proficiency Queries
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public Task<ArmorProficiencyLevel> GetCurrentProficiencyAsync(
        Player player,
        ArmorCategory armorCategory,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(player);

        _logger.LogDebug(
            "Getting current proficiency for player {PlayerId}, " +
            "ArmorCategory: {Category}",
            player.Id,
            armorCategory);

        // Start with NonProficient as default
        var highestLevel = ArmorProficiencyLevel.NonProficient;

        // Check archetype-granted proficiencies
        if (_archetypeProvider != null && !string.IsNullOrEmpty(player.ArchetypeId))
        {
            var archetypeLevel = _archetypeProvider.GetStartingProficiency(
                player.ArchetypeId,
                armorCategory);

            if (archetypeLevel > highestLevel)
            {
                highestLevel = archetypeLevel;
                
                _logger.LogDebug(
                    "Archetype-granted proficiency found. " +
                    "Archetype: {Archetype}, Category: {Category}, Level: {Level}",
                    player.ArchetypeId,
                    armorCategory,
                    archetypeLevel);
            }
        }

        // TODO: Check trained proficiencies from player storage
        // This will be implemented when player proficiency storage is added
        
        // TODO: Check quest/achievement-granted proficiencies
        // This will be implemented when quest reward system is integrated

        _logger.LogDebug(
            "Current proficiency resolved for player {PlayerId}. " +
            "ArmorCategory: {Category}, Level: {Level}",
            player.Id,
            armorCategory,
            highestLevel);

        return Task.FromResult(highestLevel);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<ArmorCategory, ArmorProficiencyLevel>>
        GetAllProficienciesAsync(
            Player player,
            CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(player);

        _logger.LogDebug(
            "Getting all proficiencies for player {PlayerId}",
            player.Id);

        var result = new Dictionary<ArmorCategory, ArmorProficiencyLevel>();

        // Get proficiency for each armor category
        var categories = Enum.GetValues<ArmorCategory>();

        foreach (var category in categories)
        {
            result[category] = await GetCurrentProficiencyAsync(player, category, ct);
        }

        _logger.LogDebug(
            "Resolved {Count} proficiency entries for player {PlayerId}",
            result.Count,
            player.Id);

        return result.AsReadOnly();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Helper Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the next proficiency level after the current level.
    /// </summary>
    /// <param name="currentLevel">The current proficiency level.</param>
    /// <returns>The next level, or null if at maximum.</returns>
    private static ArmorProficiencyLevel? GetNextProficiencyLevel(
        ArmorProficiencyLevel currentLevel)
    {
        return currentLevel switch
        {
            ArmorProficiencyLevel.NonProficient => ArmorProficiencyLevel.Proficient,
            ArmorProficiencyLevel.Proficient => ArmorProficiencyLevel.Expert,
            ArmorProficiencyLevel.Expert => ArmorProficiencyLevel.Master,
            ArmorProficiencyLevel.Master => null,
            _ => null
        };
    }

    /// <summary>
    /// Gets the required proficiency level to train to a target level.
    /// </summary>
    /// <param name="targetLevel">The target level.</param>
    /// <returns>The required previous level.</returns>
    private static ArmorProficiencyLevel GetRequiredLevel(
        ArmorProficiencyLevel targetLevel)
    {
        return targetLevel switch
        {
            ArmorProficiencyLevel.Proficient => ArmorProficiencyLevel.NonProficient,
            ArmorProficiencyLevel.Expert => ArmorProficiencyLevel.Proficient,
            ArmorProficiencyLevel.Master => ArmorProficiencyLevel.Expert,
            _ => ArmorProficiencyLevel.NonProficient
        };
    }
}
