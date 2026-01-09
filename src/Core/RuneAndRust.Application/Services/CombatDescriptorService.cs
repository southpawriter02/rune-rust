using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Damage severity tiers for descriptor selection.
/// </summary>
public enum DamageTier
{
    /// <summary>0-15% of max health - barely scratches.</summary>
    Glancing,

    /// <summary>15-30% of max health - finds purchase.</summary>
    Light,

    /// <summary>30-50% of max health - strikes true.</summary>
    Solid,

    /// <summary>50-75% of max health - crashes into.</summary>
    Heavy,

    /// <summary>75-90% of max health - devastates.</summary>
    Devastating,

    /// <summary>90-100%+ of max health - lethal blow.</summary>
    Lethal
}

/// <summary>
/// Context information for combat descriptor selection.
/// </summary>
/// <remarks>
/// Provides all the information needed to select appropriate combat descriptors,
/// including weapon type, damage dealt, target characteristics, and environment.
/// </remarks>
public record CombatDescriptorContext
{
    /// <summary>
    /// The weapon type used for the attack (e.g., "sword", "axe", "bow").
    /// </summary>
    public string? WeaponType { get; init; }

    /// <summary>
    /// The damage type inflicted (e.g., "physical", "fire", "ice").
    /// </summary>
    public string? DamageType { get; init; }

    /// <summary>
    /// The damage dealt as a percentage of target's max health (0.0 to 1.0+).
    /// </summary>
    public double DamagePercent { get; init; }

    /// <summary>
    /// Whether this attack was a critical hit.
    /// </summary>
    public bool IsCritical { get; init; }

    /// <summary>
    /// Whether this attack was a fumble/critical miss.
    /// </summary>
    public bool IsFumble { get; init; }

    /// <summary>
    /// The attacker's name for description generation.
    /// </summary>
    public string AttackerName { get; init; } = string.Empty;

    /// <summary>
    /// The target's name for description generation.
    /// </summary>
    public string TargetName { get; init; } = string.Empty;

    /// <summary>
    /// Category tags for the target (e.g., "humanoid", "undead", "beast").
    /// </summary>
    public IReadOnlyList<string> TargetTags { get; init; } = [];

    /// <summary>
    /// The environment context for biome-aware descriptors.
    /// </summary>
    public EnvironmentContext? Environment { get; init; }

    /// <summary>
    /// Gets the damage tier based on damage percentage.
    /// </summary>
    public DamageTier GetDamageTier()
    {
        return DamagePercent switch
        {
            <= 0.15 => DamageTier.Glancing,
            <= 0.30 => DamageTier.Light,
            <= 0.50 => DamageTier.Solid,
            <= 0.75 => DamageTier.Heavy,
            <= 0.90 => DamageTier.Devastating,
            _ => DamageTier.Lethal
        };
    }
}

/// <summary>
/// Context information for death descriptor selection.
/// </summary>
public record DeathDescriptorContext
{
    /// <summary>
    /// The creature that died.
    /// </summary>
    public string CreatureName { get; init; } = string.Empty;

    /// <summary>
    /// Category tags for the creature (e.g., "humanoid", "undead").
    /// </summary>
    public IReadOnlyList<string> CreatureTags { get; init; } = [];

    /// <summary>
    /// Whether the kill was from a critical hit.
    /// </summary>
    public bool WasCriticalKill { get; init; }

    /// <summary>
    /// The damage type that killed the creature.
    /// </summary>
    public string? KillingDamageType { get; init; }

    /// <summary>
    /// The weapon type that killed the creature.
    /// </summary>
    public string? KillingWeaponType { get; init; }

    /// <summary>
    /// Whether this is a player death (for special handling).
    /// </summary>
    public bool IsPlayerDeath { get; init; }

    /// <summary>
    /// Gets the primary creature category for descriptor selection.
    /// </summary>
    public string GetPrimaryCategory()
    {
        // Priority order for category matching
        string[] categories = ["undead", "beast", "construct", "elemental", "demon", "humanoid"];
        return CreatureTags.FirstOrDefault(t => categories.Contains(t.ToLowerInvariant())) ?? "humanoid";
    }
}

/// <summary>
/// Generates descriptive combat narratives based on context.
/// </summary>
/// <remarks>
/// This service extends the base DescriptorService with combat-specific
/// logic for weapon types, damage tiers, and creature categories.
/// </remarks>
public class CombatDescriptorService
{
    private readonly DescriptorService _descriptorService;
    private readonly ILogger<CombatDescriptorService> _logger;

    public CombatDescriptorService(
        DescriptorService descriptorService,
        ILogger<CombatDescriptorService> logger)
    {
        _descriptorService = descriptorService ?? throw new ArgumentNullException(nameof(descriptorService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug("CombatDescriptorService initialized");
    }

    /// <summary>
    /// Generates a complete hit description for an attack.
    /// </summary>
    /// <param name="context">The combat context.</param>
    /// <returns>A narrative description of the attack.</returns>
    public string GetHitDescription(CombatDescriptorContext context)
    {
        _logger.LogDebug(
            "GetHitDescription: Weapon={Weapon}, DamagePercent={Percent:P}, Critical={Critical}",
            context.WeaponType, context.DamagePercent, context.IsCritical);

        var poolPath = GetHitPoolPath(context);
        var descriptorContext = BuildDescriptorContext(context);

        var action = _descriptorService.GetDescriptor(poolPath, context.TargetTags, descriptorContext);

        // Fall back to generic pool if weapon-specific pool is empty
        if (string.IsNullOrEmpty(action) && !string.IsNullOrEmpty(context.WeaponType))
        {
            var fallbackPath = context.IsCritical ? "combat.critical_hit" : "combat.hit_descriptions";
            action = _descriptorService.GetDescriptor(fallbackPath, context.TargetTags, descriptorContext);
        }

        var severity = GetDamageTierDescription(context);

        if (string.IsNullOrEmpty(action))
        {
            action = "strikes";
        }

        var description = FormatHitDescription(context, action, severity);

        _logger.LogDebug("Generated hit description: {Description}", description);
        return description;
    }

    /// <summary>
    /// Generates a miss description for a failed attack.
    /// </summary>
    /// <param name="context">The combat context.</param>
    /// <returns>A narrative description of the miss.</returns>
    public string GetMissDescription(CombatDescriptorContext context)
    {
        _logger.LogDebug(
            "GetMissDescription: Weapon={Weapon}, Fumble={Fumble}",
            context.WeaponType, context.IsFumble);

        var poolPath = context.IsFumble
            ? "combat.fumble_descriptions"
            : "combat.miss_descriptions";

        var descriptorContext = BuildDescriptorContext(context);
        var miss = _descriptorService.GetDescriptor(poolPath, context.TargetTags, descriptorContext);

        if (string.IsNullOrEmpty(miss))
        {
            miss = context.IsFumble ? "stumbles badly" : "misses";
        }

        return $"{context.AttackerName}'s attack {miss}!";
    }

    /// <summary>
    /// Generates a death description for a defeated creature.
    /// </summary>
    /// <param name="context">The death context.</param>
    /// <returns>A narrative description of the death.</returns>
    public string GetDeathDescription(DeathDescriptorContext context)
    {
        _logger.LogDebug(
            "GetDeathDescription: Creature={Creature}, Category={Category}",
            context.CreatureName, context.GetPrimaryCategory());

        var category = context.GetPrimaryCategory();
        var poolPath = $"combat.death_{category}";

        // Fall back to generic pool if category-specific doesn't exist
        // Note: We don't filter by tags here since we're already using category-specific pools
        var description = _descriptorService.GetDescriptor(poolPath);

        if (string.IsNullOrEmpty(description))
        {
            description = _descriptorService.GetDescriptor("combat.death_descriptions");
        }

        if (string.IsNullOrEmpty(description))
        {
            description = "falls lifeless";
        }

        // Add critical kill flair
        if (context.WasCriticalKill)
        {
            var criticalFlair = _descriptorService.GetDescriptor("combat.critical_kill");
            if (!string.IsNullOrEmpty(criticalFlair))
            {
                return $"{criticalFlair} {context.CreatureName} {description}";
            }
        }

        return $"The {context.CreatureName} {description}.";
    }

    /// <summary>
    /// Generates a player death description.
    /// </summary>
    /// <param name="playerName">The player's name.</param>
    /// <param name="killerName">The name of what killed the player.</param>
    /// <returns>A narrative description of the player's death.</returns>
    public string GetPlayerDeathDescription(string playerName, string killerName)
    {
        var description = _descriptorService.GetDescriptor("combat.player_death");

        if (string.IsNullOrEmpty(description))
        {
            return $"{playerName} has fallen in battle against the {killerName}...";
        }

        return description
            .Replace("{player}", playerName)
            .Replace("{killer}", killerName);
    }

    /// <summary>
    /// Generates a near-death warning description.
    /// </summary>
    /// <param name="healthPercent">Current health as percentage (0.0 to 1.0).</param>
    /// <returns>A warning description based on health level, or null if health is normal.</returns>
    public string? GetNearDeathDescription(double healthPercent)
    {
        if (healthPercent > 0.25) return null;

        var poolPath = healthPercent <= 0.10
            ? "combat.near_death_critical"
            : "combat.near_death_warning";

        return _descriptorService.GetDescriptor(poolPath);
    }

    private string GetHitPoolPath(CombatDescriptorContext context)
    {
        if (context.IsCritical)
        {
            // Try weapon-specific critical pool first
            if (!string.IsNullOrEmpty(context.WeaponType))
            {
                return $"combat.critical_{context.WeaponType}";
            }
            return "combat.critical_hit";
        }

        // Try weapon-specific pool
        if (!string.IsNullOrEmpty(context.WeaponType))
        {
            return $"combat.hit_{context.WeaponType}";
        }

        return "combat.hit_descriptions";
    }

    private string GetDamageTierDescription(CombatDescriptorContext context)
    {
        var tier = context.GetDamageTier();
        var poolPath = $"combat.damage_tier_{tier.ToString().ToLowerInvariant()}";

        var description = _descriptorService.GetDescriptor(poolPath);

        // Fall back to severity pool with damage percent filtering
        if (string.IsNullOrEmpty(description))
        {
            var descriptorContext = new DescriptorContext { DamagePercent = context.DamagePercent };
            description = _descriptorService.GetDescriptor("combat.damage_severity", context: descriptorContext);
        }

        return description ?? string.Empty;
    }

    private DescriptorContext BuildDescriptorContext(CombatDescriptorContext context)
    {
        return new DescriptorContext
        {
            DamagePercent = context.DamagePercent,
            Tags = context.TargetTags.ToList(),
            Environment = context.Environment
        };
    }

    private string FormatHitDescription(CombatDescriptorContext context, string action, string severity)
    {
        var parts = new List<string>
        {
            // Add attacker
            context.AttackerName,
            // Add action
            action,
            // Add target
            $"the {context.TargetName}"
        };

        var description = string.Join(" ", parts);

        // Add severity if present
        if (!string.IsNullOrEmpty(severity))
        {
            description = $"{description}, {severity}";
        }

        // Add damage type flavor if applicable
        if (!string.IsNullOrEmpty(context.DamageType) && context.DamageType != "physical")
        {
            var damageTypeFlavor = _descriptorService.GetDescriptor(
                $"combat.damage_type_{context.DamageType}");
            if (!string.IsNullOrEmpty(damageTypeFlavor))
            {
                description = $"{description}. {damageTypeFlavor}";
            }
        }

        return $"{description}!";
    }
}
