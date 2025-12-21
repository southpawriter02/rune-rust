using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Entities;

/// <summary>
/// Represents an ambient environmental condition affecting a room (v0.3.3b).
/// Conditions apply persistent stat penalties (passive) and turn-based effects (active).
/// </summary>
public class AmbientCondition
{
    /// <summary>
    /// Unique identifier for the condition.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The condition type (used for UI and passive penalty lookup).
    /// </summary>
    public ConditionType Type { get; set; }

    /// <summary>
    /// Display name for the condition.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// AAM-VOICE compliant description shown on room entry.
    /// Should use Layer 2 diagnostic voice without precision measurements.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Spectre.Console color for UI rendering.
    /// </summary>
    public string Color { get; set; } = "grey";

    /// <summary>
    /// Effect script for turn-based damage/stress (e.g., "DAMAGE:Poison:1d4" or "STRESS:3").
    /// Empty string = no active effect.
    /// </summary>
    public string TickScript { get; set; } = string.Empty;

    /// <summary>
    /// Chance (0.0-1.0) that tick effect triggers each turn. Default 1.0 = always.
    /// </summary>
    public float TickChance { get; set; } = 1.0f;

    /// <summary>
    /// Biome types where this condition can appear (v0.3.3c).
    /// Empty list means condition can appear in any biome.
    /// </summary>
    public List<BiomeType> BiomeTags { get; set; } = new();
}
