using RuneAndRust.Core;

namespace RuneAndRust.Core.Population;

/// <summary>
/// [Intense Heat] - Muspelheim biome ambient condition (v0.29.2)
/// Catastrophic thermal regulation failure requiring STURDINESS checks.
/// Significantly more dangerous than [Extreme Heat].
/// </summary>
public class IntenseHeatCondition : AmbientCondition
{
    public IntenseHeatCondition()
    {
        ConditionName = "[Intense Heat]";
        Description = "Catastrophic thermal load from containment system failure. Mandatory STURDINESS check each turn or suffer Fire damage.";
        Type = AmbientConditionType.IntenseHeat;
        StressPerTurn = 0; // No psychic stress, only physical damage
    }

    /// <summary>
    /// STURDINESS Resolve check DC (v2.0 canonical: 12)
    /// </summary>
    public int ResolveCheckDC { get; set; } = 12;

    /// <summary>
    /// Attribute used for Resolve check (STURDINESS)
    /// </summary>
    public string ResolveCheckAttribute { get; set; } = "STURDINESS";

    /// <summary>
    /// Damage dice count on check failure (v2.0 canonical: 2d6)
    /// </summary>
    public int DamageDiceCount { get; set; } = 2;

    /// <summary>
    /// Damage die size on check failure (v2.0 canonical: d6)
    /// </summary>
    public int DamageDieSize { get; set; } = 6;

    /// <summary>
    /// Damage type for resistance calculations (Fire)
    /// </summary>
    public string DamageType { get; set; } = "Fire";

    /// <summary>
    /// Whether this condition can be removed or suppressed
    /// [Intense Heat] is environmental and cannot be removed
    /// </summary>
    public bool IsRemovable { get; set; } = false;

    /// <summary>
    /// Whether Fire Resistance applies to damage
    /// </summary>
    public bool CanBeResisted { get; set; } = true;

    /// <summary>
    /// Biome ID this condition is associated with (4 = Muspelheim)
    /// </summary>
    public int BiomeId { get; set; } = 4;
}
