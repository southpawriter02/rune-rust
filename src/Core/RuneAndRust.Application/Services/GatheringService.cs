using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Events;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for gathering resources from harvestable features.
/// </summary>
/// <remarks>
/// <para>
/// The GatheringService coordinates the complete resource harvesting workflow:
/// validation of gathering attempts, dice rolling, quantity and quality determination,
/// feature depletion, and event publishing.
/// </para>
/// <para>
/// Gathering mechanics:
/// <list type="bullet">
///   <item><description>Uses Survival skill: d20 + Survival modifier vs feature DC</description></item>
///   <item><description>On success: random quantity within feature's min/max range</description></item>
///   <item><description>On success with margin >= 10: quality upgraded by one tier</description></item>
///   <item><description>Feature depleted when remaining quantity reaches 0</description></item>
///   <item><description>Replenishing features restore after specified turns</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var gatheringService = new GatheringService(
///     featureProvider,
///     resourceProvider,
///     skillService,
///     diceService,
///     gameEventLogger,
///     logger);
///
/// var features = gatheringService.GetHarvestableFeatures(room);
/// foreach (var feature in features)
/// {
///     Console.WriteLine($"- {feature.Name} (DC {gatheringService.GetGatherDC(feature)})");
/// }
///
/// var result = gatheringService.Gather(player, features.First());
/// Console.WriteLine(result.GetOutcomeDisplay());
/// </code>
/// </example>
public class GatheringService : IGatheringService
{
    // ═══════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// The margin required to upgrade resource quality by one tier.
    /// </summary>
    /// <remarks>
    /// When the gathering roll exceeds the DC by this amount or more,
    /// the gathered resource's quality is upgraded by one tier (capped at Legendary).
    /// </remarks>
    private const int QualityUpgradeMargin = 10;

    /// <summary>
    /// The skill ID used for gathering checks.
    /// </summary>
    /// <remarks>
    /// Gathering uses the Survival skill for the dice check.
    /// </remarks>
    private const string GatheringSkillId = "survival";

    // ═══════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════

    private readonly IHarvestableFeatureProvider _featureProvider;
    private readonly IResourceProvider _resourceProvider;
    private readonly ISkillService _skillService;
    private readonly IGameEventLogger _eventLogger;
    private readonly ILogger<GatheringService> _logger;
    private readonly Random _random;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new GatheringService instance.
    /// </summary>
    /// <param name="featureProvider">Provider for harvestable feature definitions.</param>
    /// <param name="resourceProvider">Provider for resource definitions.</param>
    /// <param name="skillService">Service for skill checks and modifiers.</param>
    /// <param name="eventLogger">Logger for game events.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <param name="random">Optional random generator for testing (defaults to Random.Shared).</param>
    /// <exception cref="ArgumentNullException">Thrown when any required parameter is null.</exception>
    public GatheringService(
        IHarvestableFeatureProvider featureProvider,
        IResourceProvider resourceProvider,
        ISkillService skillService,
        IGameEventLogger eventLogger,
        ILogger<GatheringService> logger,
        Random? random = null)
    {
        _featureProvider = featureProvider ?? throw new ArgumentNullException(nameof(featureProvider));
        _resourceProvider = resourceProvider ?? throw new ArgumentNullException(nameof(resourceProvider));
        _skillService = skillService ?? throw new ArgumentNullException(nameof(skillService));
        _eventLogger = eventLogger ?? throw new ArgumentNullException(nameof(eventLogger));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _random = random ?? Random.Shared;

        _logger.LogInformation(
            "GatheringService initialized with {FeatureCount} feature definitions and {ResourceCount} resource definitions",
            _featureProvider.Count,
            _resourceProvider.Count);
    }

    // ═══════════════════════════════════════════════════════════════
    // FEATURE ACCESS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public IReadOnlyList<HarvestableFeature> GetHarvestableFeatures(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);

        _logger.LogDebug(
            "Getting harvestable features from room {RoomId}",
            room.Id);

        // Return only non-depleted features that are not awaiting replenishment
        var features = room.HarvestableFeatures
            .Where(f => f.CanHarvest)
            .ToList()
            .AsReadOnly();

        _logger.LogDebug(
            "Found {AvailableCount} available harvestable features in room {RoomId} (total: {TotalCount})",
            features.Count,
            room.Id,
            room.HarvestableFeatures.Count);

        return features;
    }

    // ═══════════════════════════════════════════════════════════════
    // VALIDATION
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public GatherValidation CanGather(Player player, HarvestableFeature feature)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(feature);

        _logger.LogDebug(
            "Validating gather attempt: Player {PlayerId} -> Feature {FeatureId} ({FeatureName})",
            player.Id,
            feature.Id,
            feature.Name);

        // Check if feature is depleted
        if (feature.IsDepleted)
        {
            _logger.LogDebug(
                "Validation failed: Feature {FeatureId} is depleted",
                feature.Id);
            return GatherValidation.Failed("This resource has been depleted.");
        }

        // Check if feature is awaiting replenishment
        if (feature.IsAwaitingReplenishment)
        {
            _logger.LogDebug(
                "Validation failed: Feature {FeatureId} is awaiting replenishment at turn {Turn}",
                feature.Id,
                feature.ReplenishAtTurn);
            return GatherValidation.Failed("This resource is not yet available.");
        }

        // Get feature definition
        var definition = _featureProvider.GetFeature(feature.DefinitionId);
        if (definition is null)
        {
            _logger.LogWarning(
                "Validation failed: Feature definition '{DefinitionId}' not found for feature {FeatureId}",
                feature.DefinitionId,
                feature.Id);
            return GatherValidation.Failed("Unknown harvestable feature type.");
        }

        // Check tool requirement
        if (definition.RequiresTool)
        {
            var hasTool = PlayerHasTool(player, definition.RequiredToolId!);
            if (!hasTool)
            {
                _logger.LogDebug(
                    "Validation failed: Player {PlayerId} lacks required tool '{ToolId}' for feature {FeatureId}",
                    player.Id,
                    definition.RequiredToolId,
                    feature.Id);
                return GatherValidation.Failed($"You need a {definition.RequiredToolId} to gather from this.");
            }

            _logger.LogDebug(
                "Player {PlayerId} has required tool '{ToolId}' for gathering",
                player.Id,
                definition.RequiredToolId);
        }

        _logger.LogDebug(
            "Validation passed: Player {PlayerId} can gather from feature {FeatureId} (DC: {DC})",
            player.Id,
            feature.Id,
            definition.DifficultyClass);

        return GatherValidation.Success(definition.DifficultyClass);
    }

    // ═══════════════════════════════════════════════════════════════
    // GATHERING EXECUTION
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public GatherResult Gather(Player player, HarvestableFeature feature)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(feature);

        _logger.LogInformation(
            "Player {PlayerId} ({PlayerName}) attempting to gather from {FeatureName}",
            player.Id,
            player.Name,
            feature.Name);

        // Step 1: Validate the gathering attempt
        var validation = CanGather(player, feature);
        if (!validation.IsValid)
        {
            _logger.LogInformation(
                "Gathering validation failed for player {PlayerId}: {Reason}",
                player.Id,
                validation.FailureReason);
            return GatherResult.ValidationFailed(validation.FailureReason!);
        }

        // Step 2: Get definitions for dice roll and resource info
        var featureDefinition = _featureProvider.GetFeature(feature.DefinitionId)!;
        var resourceDefinition = _resourceProvider.GetResource(featureDefinition.ResourceId);
        if (resourceDefinition is null)
        {
            _logger.LogError(
                "Resource definition '{ResourceId}' not found for feature '{FeatureId}'",
                featureDefinition.ResourceId,
                feature.DefinitionId);
            return GatherResult.ValidationFailed("Resource type not configured properly.");
        }

        // Step 3: Perform the skill check (uses game's standard 2d6 + skill bonus vs DC)
        var dc = validation.DifficultyClass!.Value;
        var skillCheckResult = _skillService.PerformSkillCheck(player, GatheringSkillId, dc);
        var roll = skillCheckResult.Roll;
        var modifier = skillCheckResult.SkillBonus;
        var total = skillCheckResult.Total;
        var success = skillCheckResult.Success;
        var margin = skillCheckResult.Margin;

        _logger.LogInformation(
            "Gathering check: 2d6({Roll}) + {Modifier} = {Total} vs DC {DC} [{Result}]",
            roll,
            modifier,
            total,
            dc,
            success ? "SUCCESS" : "FAILURE");

        // Step 4: Publish GatherAttemptedEvent (always published after dice roll)
        PublishGatherAttempted(player, feature, roll, modifier, total, dc, success);

        // Step 5: Handle failure
        if (!success)
        {
            _logger.LogDebug(
                "Player {PlayerId} failed gathering check by {Margin}",
                player.Id,
                Math.Abs(margin));

            return GatherResult.Failed(roll, modifier, total, dc);
        }

        // Step 6: Determine quantity (random within range, limited by remaining)
        var quantity = DetermineQuantity(featureDefinition, feature.RemainingQuantity);

        // Step 7: Determine quality (upgrade if margin >= 10)
        var quality = DetermineQuality(resourceDefinition, margin);

        _logger.LogInformation(
            "Gathering success: {Quantity}x {ResourceName} ({Quality}) from {FeatureName} (margin: +{Margin})",
            quantity,
            resourceDefinition.Name,
            quality,
            feature.Name,
            margin);

        // Step 8: Deplete feature
        feature.Harvest(quantity);
        var featureDepleted = feature.IsDepleted;

        if (featureDepleted)
        {
            _logger.LogInformation(
                "Feature {FeatureId} ({FeatureName}) has been depleted",
                feature.Id,
                feature.Name);

            // Set replenishment timer if feature replenishes
            if (featureDefinition.Replenishes)
            {
                // Note: We'd need the current turn from game state
                // For now, the caller should handle this via ProcessReplenishment
                _logger.LogDebug(
                    "Feature {FeatureId} will replenish after {Turns} turns",
                    feature.Id,
                    featureDefinition.ReplenishTurns);
            }
        }

        // Step 9: Publish ResourceGatheredEvent
        PublishResourceGathered(player, feature, resourceDefinition, quantity, quality, featureDepleted);

        // Step 10: Log to game event logger
        _eventLogger.LogInteraction(
            "ResourceGathered",
            $"{player.Name} gathered {quantity}x {resourceDefinition.Name} ({quality}) from {feature.Name}",
            new Dictionary<string, object>
            {
                ["playerId"] = player.Id,
                ["resourceId"] = resourceDefinition.ResourceId,
                ["quantity"] = quantity,
                ["quality"] = quality.ToString(),
                ["featureId"] = feature.Id.ToString(),
                ["featureDepleted"] = featureDepleted
            });

        return GatherResult.Success(
            roll: roll,
            modifier: modifier,
            total: total,
            dc: dc,
            resourceId: resourceDefinition.ResourceId,
            resourceName: resourceDefinition.Name,
            quantity: quantity,
            quality: quality,
            featureDepleted: featureDepleted);
    }

    // ═══════════════════════════════════════════════════════════════
    // GATHERING INFORMATION
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public int GetGatherDC(HarvestableFeature feature)
    {
        ArgumentNullException.ThrowIfNull(feature);

        var definition = _featureProvider.GetFeature(feature.DefinitionId);
        if (definition is null)
        {
            _logger.LogWarning(
                "Cannot get DC for feature {FeatureId}: Definition '{DefinitionId}' not found",
                feature.Id,
                feature.DefinitionId);
            return 0;
        }

        return definition.DifficultyClass;
    }

    /// <inheritdoc/>
    public int GetGatherModifier(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Gathering uses the Survival skill
        var modifier = _skillService.GetSkillBonus(player, GatheringSkillId);

        _logger.LogDebug(
            "Player {PlayerId} gathering modifier (Survival): {Modifier}",
            player.Id,
            modifier);

        return modifier;
    }

    // ═══════════════════════════════════════════════════════════════
    // REPLENISHMENT
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public IReadOnlyList<HarvestableFeature> ProcessReplenishment(Room room, int currentTurn)
    {
        ArgumentNullException.ThrowIfNull(room);

        _logger.LogDebug(
            "Processing feature replenishment in room {RoomId} at turn {Turn}",
            room.Id,
            currentTurn);

        var replenished = new List<HarvestableFeature>();

        foreach (var feature in room.HarvestableFeatures)
        {
            if (feature.ShouldReplenish(currentTurn))
            {
                // Get definition to determine new random quantity
                var definition = _featureProvider.GetFeature(feature.DefinitionId);
                if (definition is not null)
                {
                    var newQuantity = _random.Next(definition.MinQuantity, definition.MaxQuantity + 1);
                    feature.Replenish(newQuantity);

                    _logger.LogInformation(
                        "Feature {FeatureId} ({FeatureName}) replenished with {Quantity} resources at turn {Turn}",
                        feature.Id,
                        feature.Name,
                        newQuantity,
                        currentTurn);

                    // Publish replenishment event
                    PublishFeatureReplenished(feature, room, newQuantity);

                    replenished.Add(feature);
                }
                else
                {
                    _logger.LogWarning(
                        "Cannot replenish feature {FeatureId}: Definition '{DefinitionId}' not found",
                        feature.Id,
                        feature.DefinitionId);

                    // Replenish with initial quantity as fallback
                    feature.Replenish();
                    replenished.Add(feature);
                }
            }
        }

        if (replenished.Count > 0)
        {
            _logger.LogInformation(
                "Replenished {Count} features in room {RoomId}",
                replenished.Count,
                room.Id);
        }

        return replenished.AsReadOnly();
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if a player has the required tool for gathering.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="toolId">The required tool ID.</param>
    /// <returns>True if the player has the tool, false otherwise.</returns>
    /// <remarks>
    /// Currently returns true as a placeholder. This should be implemented
    /// to check the player's inventory for the specific tool item.
    /// </remarks>
    private bool PlayerHasTool(Player player, string toolId)
    {
        // TODO: Implement actual inventory check
        // For now, assume player always has the required tool
        // This will be implemented when inventory system is integrated
        _logger.LogDebug(
            "Tool check for '{ToolId}' - returning true (placeholder implementation)",
            toolId);
        return true;
    }

    /// <summary>
    /// Determines the quantity of resources to gather.
    /// </summary>
    /// <param name="definition">The feature definition with min/max range.</param>
    /// <param name="remainingQuantity">The remaining quantity in the feature.</param>
    /// <returns>The quantity to gather (clamped to remaining).</returns>
    private int DetermineQuantity(HarvestableFeatureDefinition definition, int remainingQuantity)
    {
        // Roll random quantity within definition's range
        var quantity = _random.Next(definition.MinQuantity, definition.MaxQuantity + 1);

        // Clamp to remaining quantity
        quantity = Math.Min(quantity, remainingQuantity);

        _logger.LogDebug(
            "Determined gather quantity: {Quantity} (range: {Min}-{Max}, remaining: {Remaining})",
            quantity,
            definition.MinQuantity,
            definition.MaxQuantity,
            remainingQuantity);

        return quantity;
    }

    /// <summary>
    /// Determines the quality of gathered resources.
    /// </summary>
    /// <param name="resourceDefinition">The resource definition with base quality.</param>
    /// <param name="margin">The margin by which the roll exceeded the DC.</param>
    /// <returns>The final quality (possibly upgraded if margin >= 10).</returns>
    private ResourceQuality DetermineQuality(ResourceDefinition resourceDefinition, int margin)
    {
        var baseQuality = resourceDefinition.Quality;

        // Upgrade quality if margin is >= 10 (capped at Legendary)
        if (margin >= QualityUpgradeMargin && baseQuality < ResourceQuality.Legendary)
        {
            var upgradedQuality = baseQuality + 1;

            _logger.LogDebug(
                "Quality upgraded from {BaseQuality} to {UpgradedQuality} (margin: +{Margin} >= {Required})",
                baseQuality,
                upgradedQuality,
                margin,
                QualityUpgradeMargin);

            return upgradedQuality;
        }

        return baseQuality;
    }

    /// <summary>
    /// Publishes a GatherAttemptedEvent.
    /// </summary>
    private void PublishGatherAttempted(
        Player player,
        HarvestableFeature feature,
        int roll,
        int modifier,
        int total,
        int dc,
        bool success)
    {
        var gatherEvent = new GatherAttemptedEvent(
            PlayerId: player.Id,
            FeatureId: feature.Id,
            FeatureDefinitionId: feature.DefinitionId,
            Roll: roll,
            Modifier: modifier,
            Total: total,
            DifficultyClass: dc,
            Success: success);

        _logger.LogDebug(
            "Publishing GatherAttemptedEvent: Player {PlayerId}, Feature {FeatureId}, Success: {Success}",
            player.Id,
            feature.Id,
            success);

        // Log to game event logger
        _eventLogger.LogInteraction(
            "GatherAttempted",
            $"{player.Name} attempted to gather from {feature.Name}: {(success ? "Success" : "Failed")} (rolled {total} vs DC {dc})",
            new Dictionary<string, object>
            {
                ["playerId"] = player.Id,
                ["featureId"] = feature.Id.ToString(),
                ["roll"] = roll,
                ["modifier"] = modifier,
                ["total"] = total,
                ["dc"] = dc,
                ["success"] = success,
                ["margin"] = total - dc
            });

        // TODO: Publish to event bus when available
        // _eventBus.Publish(gatherEvent);
    }

    /// <summary>
    /// Publishes a ResourceGatheredEvent.
    /// </summary>
    private void PublishResourceGathered(
        Player player,
        HarvestableFeature feature,
        ResourceDefinition resource,
        int quantity,
        ResourceQuality quality,
        bool featureDepleted)
    {
        var resourceEvent = new ResourceGatheredEvent(
            PlayerId: player.Id,
            ResourceId: resource.ResourceId,
            ResourceName: resource.Name,
            Quantity: quantity,
            Quality: quality,
            FeatureId: feature.Id,
            FeatureDepleted: featureDepleted);

        _logger.LogDebug(
            "Publishing ResourceGatheredEvent: Player {PlayerId}, Resource {ResourceId}, Quantity: {Quantity}",
            player.Id,
            resource.ResourceId,
            quantity);

        // TODO: Publish to event bus when available
        // _eventBus.Publish(resourceEvent);
    }

    /// <summary>
    /// Publishes a FeatureReplenishedEvent.
    /// </summary>
    private void PublishFeatureReplenished(HarvestableFeature feature, Room room, int newQuantity)
    {
        var replenishEvent = new FeatureReplenishedEvent(
            FeatureId: feature.Id,
            FeatureDefinitionId: feature.DefinitionId,
            RoomId: room.Id,
            NewQuantity: newQuantity);

        _logger.LogDebug(
            "Publishing FeatureReplenishedEvent: Feature {FeatureId}, Room {RoomId}, Quantity: {Quantity}",
            feature.Id,
            room.Id,
            newQuantity);

        // Log to game event logger
        _eventLogger.LogInteraction(
            "FeatureReplenished",
            $"{feature.Name} has replenished with {newQuantity} resources",
            new Dictionary<string, object>
            {
                ["featureId"] = feature.Id.ToString(),
                ["featureDefinitionId"] = feature.DefinitionId,
                ["roomId"] = room.Id.ToString(),
                ["newQuantity"] = newQuantity
            });

        // TODO: Publish to event bus when available
        // _eventBus.Publish(replenishEvent);
    }
}
