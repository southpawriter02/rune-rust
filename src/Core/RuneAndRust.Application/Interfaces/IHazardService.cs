using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for hazard management and triggering.
/// </summary>
public interface IHazardService
{
    /// <summary>
    /// Gets a hazard by its string ID.
    /// </summary>
    BiomeHazard? GetHazard(string hazardId);

    /// <summary>
    /// Gets all hazards that can spawn in the specified biome.
    /// </summary>
    IReadOnlyList<BiomeHazard> GetHazardsForBiome(string biomeId);

    /// <summary>
    /// Attempts to trigger a hazard, returning the result.
    /// </summary>
    HazardResult TriggerHazard(BiomeHazard hazard, string biomeId, int avoidanceRoll = 0);

    /// <summary>
    /// Attempts to disarm a hazard.
    /// </summary>
    DisarmResult TryDisarm(BiomeHazard hazard, int skillRoll);

    /// <summary>
    /// Gets the damage modifier for a biome.
    /// </summary>
    BiomeDamageModifier GetDamageModifier(string biomeId);

    /// <summary>
    /// Registers a hazard.
    /// </summary>
    void RegisterHazard(BiomeHazard hazard);

    /// <summary>
    /// Registers a damage modifier.
    /// </summary>
    void RegisterDamageModifier(BiomeDamageModifier modifier);
}

/// <summary>
/// Result of triggering a hazard.
/// </summary>
public record HazardResult(
    bool Triggered,
    bool Avoided,
    int DamageDealt,
    string DamageType,
    IReadOnlyList<string> StatusEffectsApplied);

/// <summary>
/// Result of attempting to disarm a hazard.
/// </summary>
public record DisarmResult(
    bool Success,
    bool CannotDisarm,
    bool CriticalFailure,
    HazardResult? TriggerResult);
