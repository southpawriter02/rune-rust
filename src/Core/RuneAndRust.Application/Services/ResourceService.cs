using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Manages resource types and player resource pools.
/// </summary>
public class ResourceService
{
    private readonly IGameConfigurationProvider _configProvider;
    private readonly ILogger<ResourceService> _logger;

    public ResourceService(
        IGameConfigurationProvider configProvider,
        ILogger<ResourceService> logger)
    {
        _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogInformation(
            "ResourceService initialized with {ResourceTypeCount} resource types",
            _configProvider.GetResourceTypes().Count);
    }

    /// <summary>
    /// Gets a resource type by ID.
    /// </summary>
    public ResourceTypeDefinition? GetResourceType(string id)
    {
        _logger.LogDebug("GetResourceType called for: {ResourceTypeId}", id);
        return _configProvider.GetResourceTypeById(id);
    }

    /// <summary>
    /// Gets all resource types.
    /// </summary>
    public IReadOnlyList<ResourceTypeDto> GetAllResourceTypes()
    {
        _logger.LogDebug("GetAllResourceTypes called");
        return _configProvider.GetResourceTypes()
            .Select(r => new ResourceTypeDto(
                r.Id, r.DisplayName, r.Abbreviation, r.Color, r.Description,
                r.DefaultMax, r.RegenPerTurn, r.DecayPerTurn,
                r.IsUniversal, r.StartsAtZero))
            .ToList();
    }

    /// <summary>
    /// Initializes a player's resource pools based on their class.
    /// </summary>
    public void InitializePlayerResources(Player player, ClassDefinition classDef)
    {
        _logger.LogDebug(
            "InitializePlayerResources for Player: {PlayerName}, Class: {ClassId}",
            player.Name, classDef.Id);

        var resourceTypes = _configProvider.GetResourceTypes();

        // Initialize universal resources (Health)
        foreach (var resourceType in resourceTypes.Where(r => r.IsUniversal))
        {
            player.InitializeResource(
                resourceType.Id,
                resourceType.DefaultMax,
                resourceType.StartsAtZero);

            _logger.LogDebug(
                "Initialized universal resource {ResourceId} for {PlayerName}: {Max}",
                resourceType.Id, player.Name, resourceType.DefaultMax);
        }

        // Initialize class-specific primary resource
        var primaryResource = GetResourceType(classDef.PrimaryResourceId);
        if (primaryResource != null && !primaryResource.IsUniversal)
        {
            player.InitializeResource(
                primaryResource.Id,
                primaryResource.DefaultMax,
                primaryResource.StartsAtZero);

            _logger.LogInformation(
                "Initialized primary resource {ResourceName} for {PlayerName}: {Current}/{Max}",
                primaryResource.DisplayName, player.Name,
                primaryResource.StartsAtZero ? 0 : primaryResource.DefaultMax,
                primaryResource.DefaultMax);
        }
        else if (primaryResource == null)
        {
            _logger.LogWarning(
                "Primary resource type not found for class {ClassId}: {ResourceId}",
                classDef.Id, classDef.PrimaryResourceId);
        }
    }

    /// <summary>
    /// Gets all resource pools for a player as DTOs.
    /// </summary>
    public IReadOnlyList<ResourcePoolDto> GetPlayerResources(Player player)
    {
        _logger.LogDebug("GetPlayerResources for: {PlayerName}", player.Name);

        return player.Resources.Values
            .Select(pool =>
            {
                var resourceType = GetResourceType(pool.ResourceTypeId);
                return new ResourcePoolDto(
                    pool.ResourceTypeId,
                    resourceType?.DisplayName ?? pool.ResourceTypeId,
                    resourceType?.Abbreviation ?? "??",
                    resourceType?.Color ?? "#FFFFFF",
                    pool.Current,
                    pool.Maximum,
                    pool.Percentage);
            })
            .ToList();
    }

    /// <summary>
    /// Attempts to spend a resource.
    /// </summary>
    public bool SpendResource(Player player, string resourceTypeId, int amount)
    {
        _logger.LogDebug(
            "SpendResource: {PlayerName}, {ResourceId}, Amount: {Amount}",
            player.Name, resourceTypeId, amount);

        var pool = player.GetResource(resourceTypeId);
        if (pool == null)
        {
            _logger.LogWarning(
                "Player {PlayerName} does not have resource: {ResourceId}",
                player.Name, resourceTypeId);
            return false;
        }

        var previousValue = pool.Current;
        if (!pool.Spend(amount))
        {
            _logger.LogDebug(
                "Insufficient {ResourceId}: Has {Current}, needs {Amount}",
                resourceTypeId, pool.Current, amount);
            return false;
        }

        _logger.LogInformation(
            "Resource spent: {PlayerName} {ResourceId} {Previous} -> {Current} (-{Amount})",
            player.Name, resourceTypeId, previousValue, pool.Current, amount);

        return true;
    }

    /// <summary>
    /// Gains a resource amount.
    /// </summary>
    public int GainResource(Player player, string resourceTypeId, int amount)
    {
        _logger.LogDebug(
            "GainResource: {PlayerName}, {ResourceId}, Amount: {Amount}",
            player.Name, resourceTypeId, amount);

        var pool = player.GetResource(resourceTypeId);
        if (pool == null)
        {
            _logger.LogWarning(
                "Player {PlayerName} does not have resource: {ResourceId}",
                player.Name, resourceTypeId);
            return 0;
        }

        var previousValue = pool.Current;
        var actualGain = pool.Gain(amount);

        if (actualGain > 0)
        {
            _logger.LogInformation(
                "Resource gained: {PlayerName} {ResourceId} {Previous} -> {Current} (+{Actual})",
                player.Name, resourceTypeId, previousValue, pool.Current, actualGain);
        }

        return actualGain;
    }

    /// <summary>
    /// Processes end-of-turn resource regeneration and decay.
    /// </summary>
    public ResourceChangeResult ProcessTurnEnd(Player player, bool inCombat = false)
    {
        _logger.LogDebug(
            "ProcessTurnEnd for {PlayerName}, InCombat: {InCombat}",
            player.Name, inCombat);

        var changes = new List<ResourceChange>();

        foreach (var pool in player.Resources.Values)
        {
            var resourceType = GetResourceType(pool.ResourceTypeId);
            if (resourceType == null) continue;

            // Apply regeneration
            if (resourceType.RegenPerTurn > 0 && !pool.IsFull)
            {
                var previousValue = pool.Current;
                var actualGain = pool.Gain(resourceType.RegenPerTurn);

                if (actualGain > 0)
                {
                    changes.Add(new ResourceChange(
                        pool.ResourceTypeId, previousValue, pool.Current,
                        ResourceChangeType.Regeneration));

                    _logger.LogDebug(
                        "Regeneration: {ResourceId} {Previous} -> {Current} (+{Regen})",
                        pool.ResourceTypeId, previousValue, pool.Current, actualGain);
                }
            }

            // Apply decay (only out of combat if configured)
            if (resourceType.DecayPerTurn > 0 && !pool.IsEmpty)
            {
                var shouldDecay = !resourceType.DecayOnlyOutOfCombat || !inCombat;

                if (shouldDecay)
                {
                    var previousValue = pool.Current;
                    var actualLoss = pool.Lose(resourceType.DecayPerTurn);

                    if (actualLoss > 0)
                    {
                        changes.Add(new ResourceChange(
                            pool.ResourceTypeId, previousValue, pool.Current,
                            ResourceChangeType.Decay));

                        _logger.LogDebug(
                            "Decay: {ResourceId} {Previous} -> {Current} (-{Decay})",
                            pool.ResourceTypeId, previousValue, pool.Current, actualLoss);
                    }
                }
            }
        }

        if (changes.Count > 0)
        {
            _logger.LogInformation(
                "Turn end processed for {PlayerName}: {ChangeCount} resource changes",
                player.Name, changes.Count);
        }

        return new ResourceChangeResult(changes);
    }

    /// <summary>
    /// Processes combat hit for build-on-hit resources.
    /// </summary>
    public ResourceChangeResult ProcessCombatHit(Player player, int damageDealt, int damageTaken)
    {
        _logger.LogDebug(
            "ProcessCombatHit: {PlayerName}, Dealt: {Dealt}, Taken: {Taken}",
            player.Name, damageDealt, damageTaken);

        var changes = new List<ResourceChange>();

        foreach (var pool in player.Resources.Values)
        {
            var resourceType = GetResourceType(pool.ResourceTypeId);
            if (resourceType == null) continue;

            // Build on damage dealt
            if (resourceType.BuildOnDamageDealt > 0 && damageDealt > 0)
            {
                var previousValue = pool.Current;
                var actualGain = pool.Gain(resourceType.BuildOnDamageDealt);

                if (actualGain > 0)
                {
                    changes.Add(new ResourceChange(
                        pool.ResourceTypeId, previousValue, pool.Current,
                        ResourceChangeType.BuildOnDamageDealt));
                }
            }

            // Build on damage taken
            if (resourceType.BuildOnDamageTaken > 0 && damageTaken > 0)
            {
                var previousValue = pool.Current;
                var actualGain = pool.Gain(resourceType.BuildOnDamageTaken);

                if (actualGain > 0)
                {
                    changes.Add(new ResourceChange(
                        pool.ResourceTypeId, previousValue, pool.Current,
                        ResourceChangeType.BuildOnDamageTaken));
                }
            }
        }

        return new ResourceChangeResult(changes);
    }

    /// <summary>
    /// Processes support action for build-on-heal resources.
    /// </summary>
    public ResourceChangeResult ProcessSupportAction(Player player, int healAmount)
    {
        _logger.LogDebug(
            "ProcessSupportAction: {PlayerName}, HealAmount: {HealAmount}",
            player.Name, healAmount);

        var changes = new List<ResourceChange>();

        foreach (var pool in player.Resources.Values)
        {
            var resourceType = GetResourceType(pool.ResourceTypeId);
            if (resourceType == null) continue;

            if (resourceType.BuildOnHeal > 0 && healAmount > 0)
            {
                var previousValue = pool.Current;
                var actualGain = pool.Gain(resourceType.BuildOnHeal);

                if (actualGain > 0)
                {
                    changes.Add(new ResourceChange(
                        pool.ResourceTypeId, previousValue, pool.Current,
                        ResourceChangeType.BuildOnHeal));
                }
            }
        }

        return new ResourceChangeResult(changes);
    }
}
