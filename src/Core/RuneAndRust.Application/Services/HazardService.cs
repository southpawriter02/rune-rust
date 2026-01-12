using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for hazard management and triggering.
/// </summary>
public class HazardService : IHazardService
{
    private readonly Dictionary<string, BiomeHazard> _hazards = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, BiomeDamageModifier> _modifiers = new(StringComparer.OrdinalIgnoreCase);
    private readonly ISeededRandomService _random;
    private readonly ILogger<HazardService> _logger;

    public HazardService(
        ISeededRandomService random,
        ILogger<HazardService>? logger = null)
    {
        _random = random ?? throw new ArgumentNullException(nameof(random));
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<HazardService>.Instance;

        RegisterDefaultHazards();
        RegisterDefaultModifiers();
        _logger.LogDebug("HazardService initialized with {HazardCount} hazards, {ModifierCount} modifiers",
            _hazards.Count, _modifiers.Count);
    }

    /// <inheritdoc/>
    public BiomeHazard? GetHazard(string hazardId) =>
        _hazards.TryGetValue(hazardId, out var hazard) ? hazard : null;

    /// <inheritdoc/>
    public IReadOnlyList<BiomeHazard> GetHazardsForBiome(string biomeId) =>
        _hazards.Values.Where(h => h.CanSpawnIn(biomeId)).ToList();

    /// <inheritdoc/>
    public HazardResult TriggerHazard(BiomeHazard hazard, string biomeId, int avoidanceRoll = 0)
    {
        // Check avoidance
        if (hazard.Trigger.CanAvoid && avoidanceRoll >= hazard.Trigger.AvoidanceDC)
        {
            _logger.LogInformation("Hazard {HazardId} avoided with roll {Roll} vs DC {DC}",
                hazard.HazardId, avoidanceRoll, hazard.Trigger.AvoidanceDC);
            return new HazardResult(false, true, 0, hazard.DamageTypeId, []);
        }

        // Apply damage with biome modifiers
        var modifier = GetDamageModifier(biomeId);
        var damage = modifier.ApplyModifier(hazard.DamageTypeId, hazard.BaseDamage);

        _logger.LogInformation("Hazard {HazardId} triggered: {Damage} {DamageType} damage, effects: [{Effects}]",
            hazard.HazardId, damage, hazard.DamageTypeId, string.Join(", ", hazard.StatusEffects));

        return new HazardResult(true, false, damage, hazard.DamageTypeId, hazard.StatusEffects);
    }

    /// <inheritdoc/>
    public DisarmResult TryDisarm(BiomeHazard hazard, int skillRoll)
    {
        if (!hazard.CanDisarm)
        {
            _logger.LogDebug("Hazard {HazardId} cannot be disarmed", hazard.HazardId);
            return new DisarmResult(false, true, false, null);
        }

        // Critical failure on natural 1 (roll of 1)
        if (skillRoll == 1)
        {
            _logger.LogWarning("Critical failure disarming {HazardId}", hazard.HazardId);
            var triggerResult = TriggerHazard(hazard, "unknown", 0);
            return new DisarmResult(false, false, true, triggerResult);
        }

        if (skillRoll >= hazard.DisarmDC)
        {
            _logger.LogInformation("Hazard {HazardId} successfully disarmed", hazard.HazardId);
            return new DisarmResult(true, false, false, null);
        }

        _logger.LogDebug("Failed to disarm {HazardId}: roll {Roll} vs DC {DC}",
            hazard.HazardId, skillRoll, hazard.DisarmDC);
        return new DisarmResult(false, false, false, null);
    }

    /// <inheritdoc/>
    public BiomeDamageModifier GetDamageModifier(string biomeId) =>
        _modifiers.TryGetValue(biomeId, out var modifier) ? modifier : BiomeDamageModifier.Default;

    /// <inheritdoc/>
    public void RegisterHazard(BiomeHazard hazard)
    {
        ArgumentNullException.ThrowIfNull(hazard);
        _hazards[hazard.HazardId] = hazard;
        _logger.LogDebug("Registered hazard: {HazardId}", hazard.HazardId);
    }

    /// <inheritdoc/>
    public void RegisterDamageModifier(BiomeDamageModifier modifier)
    {
        ArgumentNullException.ThrowIfNull(modifier);
        _modifiers[modifier.BiomeId] = modifier;
        _logger.LogDebug("Registered damage modifier: {BiomeId}", modifier.BiomeId);
    }

    private void RegisterDefaultHazards()
    {
        // Lava Pool - volcanic caverns
        RegisterHazard(BiomeHazard.Create(
            "lava-pool", "Lava Pool",
            "A bubbling pool of molten rock.",
            "fire", 15,
            HazardTrigger.OnEnter(avoidanceStat: "athletics", avoidanceDC: 12),
            persistent: true,
            biomeIds: ["volcanic-caverns"],
            statusEffects: ["burning"]));

        // Poison Gas Cloud - fungal caverns
        RegisterHazard(BiomeHazard.Create(
            "poison-gas-cloud", "Poison Gas Cloud",
            "A thick cloud of toxic spores.",
            "poison", 8,
            HazardTrigger.PerTurn(0.5f, "fortitude", 10),
            persistent: true,
            biomeIds: ["fungal-caverns"],
            statusEffects: ["poisoned"]));

        // Ice Patch - flooded depths
        RegisterHazard(BiomeHazard.Create(
            "ice-patch", "Ice Patch",
            "A slick patch of frozen water.",
            "ice", 5,
            HazardTrigger.OnEnter(avoidanceStat: "athletics", avoidanceDC: 10),
            persistent: true,
            canDisarm: false,
            biomeIds: ["flooded-depths"]));

        // Bone Trap - stone corridors
        RegisterHazard(BiomeHazard.Create(
            "bone-trap", "Bone Trap",
            "A hidden trap made of sharpened bones.",
            "physical", 10,
            HazardTrigger.OnEnter(avoidanceStat: "perception", avoidanceDC: 14),
            persistent: false,
            canDisarm: true,
            disarmDC: 12,
            biomeIds: ["stone-corridors"]));
    }

    private void RegisterDefaultModifiers()
    {
        RegisterDamageModifier(BiomeDamageModifier.Volcanic);
        RegisterDamageModifier(BiomeDamageModifier.Flooded);
        RegisterDamageModifier(BiomeDamageModifier.Fungal);
    }
}
