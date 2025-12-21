using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Models;

/// <summary>
/// Template for creating dynamic hazards during dungeon generation (v0.3.3c).
/// Templates are seeded to the database; DynamicHazard instances are spawned from them.
/// </summary>
public class HazardTemplate
{
    /// <summary>
    /// Unique identifier for the template.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Display name for hazards created from this template.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// AAM-VOICE compliant description shown when the hazard triggers.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The thematic classification of this hazard.
    /// </summary>
    public HazardType HazardType { get; set; } = HazardType.Environmental;

    /// <summary>
    /// What condition activates hazards created from this template.
    /// </summary>
    public TriggerType Trigger { get; set; } = TriggerType.Movement;

    /// <summary>
    /// Effect script executed when the hazard triggers.
    /// Uses semicolon-delimited command format: "DAMAGE:Fire:2d6;STATUS:Burning:2".
    /// </summary>
    public string EffectScript { get; set; } = string.Empty;

    /// <summary>
    /// Number of turns required to recharge after activation.
    /// </summary>
    public int MaxCooldown { get; set; } = 2;

    /// <summary>
    /// If true, hazards created from this template are destroyed after a single use.
    /// </summary>
    public bool OneTimeUse { get; set; } = false;

    /// <summary>
    /// Biome types where hazards from this template can spawn (v0.3.3c).
    /// Empty list means hazard can spawn in any biome.
    /// </summary>
    public List<BiomeType> BiomeTags { get; set; } = new();
}
