namespace RuneAndRust.Core.Territory;

/// <summary>
/// v0.35.3: Represents modified NPC behavior based on territorial control
/// Applied to NPCs when their sector is controlled by a specific faction
/// </summary>
public class NPCBehaviorModifier
{
    /// <summary>
    /// Hostility level: "Friendly", "Neutral", "Suspicious", "Hostile"
    /// </summary>
    public string HostilityLevel { get; set; } = "Neutral";

    /// <summary>
    /// Price modifier for merchant NPCs (0.85 = 15% discount, 1.25 = 25% markup)
    /// </summary>
    public double PriceModifier { get; set; } = 1.0;

    /// <summary>
    /// Information willingness: "High", "Medium", "Low"
    /// Affects dialogue options and quest availability
    /// </summary>
    public string InformationWillingness { get; set; } = "Medium";

    /// <summary>
    /// Custom greeting dialogue based on faction relations
    /// </summary>
    public string? GreetingDialogue { get; set; }

    /// <summary>
    /// Disposition modifier applied to NPC's base disposition
    /// </summary>
    public int DispositionModifier { get; set; } = 0;
}
