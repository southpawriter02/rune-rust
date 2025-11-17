namespace RuneAndRust.Core.CombatFlavor;

/// <summary>
/// v0.38.6: Environmental combat modifier
/// Adds biome-specific atmosphere and hazard integration to combat
/// </summary>
public class EnvironmentalCombatModifier
{
    public int ModifierId { get; set; }
    public string BiomeName { get; set; } = string.Empty;
    public string ModifierType { get; set; } = string.Empty;
    public string DescriptorText { get; set; } = string.Empty;
    public float TriggerChance { get; set; } = 0.3f;
}

/// <summary>
/// Environmental modifier types
/// </summary>
public enum EnvironmentalModifierType
{
    Reaction,           // General atmospheric flavor
    HazardIntegration   // Tactical hazard elements
}
