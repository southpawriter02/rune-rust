using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Represents an environmental hazard that can appear in biomes.
/// </summary>
public class BiomeHazard : IEntity
{
    /// <summary>
    /// Gets the unique identifier.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the hazard string ID.
    /// </summary>
    public string HazardId { get; private set; }

    /// <summary>
    /// Gets the display name.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the description.
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Gets the damage type ID (e.g., "fire", "poison").
    /// </summary>
    public string DamageTypeId { get; private set; }

    /// <summary>
    /// Gets the base damage dealt.
    /// </summary>
    public int BaseDamage { get; private set; }

    /// <summary>
    /// Gets the trigger configuration.
    /// </summary>
    public HazardTrigger Trigger { get; private set; }

    /// <summary>
    /// Gets whether the hazard persists after triggering.
    /// </summary>
    public bool Persistent { get; private set; }

    /// <summary>
    /// Gets whether the hazard can be disarmed.
    /// </summary>
    public bool CanDisarm { get; private set; }

    /// <summary>
    /// Gets the DC required to disarm.
    /// </summary>
    public int DisarmDC { get; private set; }

    /// <summary>
    /// Gets the biome IDs where this hazard can spawn.
    /// </summary>
    public IReadOnlyList<string> BiomeIds { get; private set; }

    /// <summary>
    /// Gets status effect IDs applied on trigger.
    /// </summary>
    public IReadOnlyList<string> StatusEffects { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private BiomeHazard()
    {
        HazardId = null!;
        Name = null!;
        Description = string.Empty;
        DamageTypeId = "physical";
        BiomeIds = [];
        StatusEffects = [];
    }

    /// <summary>
    /// Creates a new biome hazard.
    /// </summary>
    public static BiomeHazard Create(
        string hazardId,
        string name,
        string description,
        string damageTypeId,
        int baseDamage,
        HazardTrigger trigger,
        bool persistent = true,
        bool canDisarm = false,
        int disarmDC = 0,
        IReadOnlyList<string>? biomeIds = null,
        IReadOnlyList<string>? statusEffects = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hazardId);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegative(baseDamage);

        return new BiomeHazard
        {
            Id = Guid.NewGuid(),
            HazardId = hazardId.ToLowerInvariant(),
            Name = name,
            Description = description ?? string.Empty,
            DamageTypeId = damageTypeId?.ToLowerInvariant() ?? "physical",
            BaseDamage = baseDamage,
            Trigger = trigger,
            Persistent = persistent,
            CanDisarm = canDisarm,
            DisarmDC = disarmDC,
            BiomeIds = biomeIds ?? [],
            StatusEffects = statusEffects ?? []
        };
    }

    /// <summary>
    /// Checks if this hazard can spawn in the specified biome.
    /// </summary>
    public bool CanSpawnIn(string biomeId) =>
        BiomeIds.Count == 0 || BiomeIds.Any(b => b.Equals(biomeId, StringComparison.OrdinalIgnoreCase));
}
