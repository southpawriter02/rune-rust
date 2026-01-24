namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the types of environmental hazards that can be detected using the Wasteland Survival skill.
/// </summary>
/// <remarks>
/// <para>
/// DetectableHazardType represents hazards that characters can proactively identify and avoid
/// through passive perception (WITS ÷ 2) or active investigation (Wasteland Survival skill check).
/// This is distinct from <see cref="HazardType"/> which defines combat zone hazards.
/// </para>
/// <para>
/// Detection DCs follow a progression based on how concealed or subtle the hazard is:
/// <list type="bullet">
///   <item><description><see cref="ObviousDanger"/> (DC 8): Easy to spot, causes minor damage</description></item>
///   <item><description><see cref="HiddenPit"/> (DC 12): Concealed drop, causes fall damage</description></item>
///   <item><description><see cref="ToxicZone"/> (DC 16): Invisible poison, causes status effect</description></item>
///   <item><description><see cref="AmbushSite"/> (DC 16): Signs of hostile presence</description></item>
///   <item><description><see cref="GlitchPocket"/> (DC 20): Reality distortion, random effects</description></item>
/// </list>
/// </para>
/// <para>
/// Passive detection uses WITS ÷ 2 compared against hazard DC to provide hints.
/// Active detection uses a full Wasteland Survival skill check for full identification.
/// </para>
/// </remarks>
public enum DetectableHazardType
{
    /// <summary>
    /// Obvious environmental danger such as unstable debris, exposed wiring, or crumbling structures.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Detection DC: 8 - Easy to spot but still causes minor damage if walked into.
    /// </para>
    /// <para>
    /// Consequence: 1d6 minor damage (cuts, burns, bruises).
    /// No lasting effects beyond immediate damage.
    /// </para>
    /// </remarks>
    ObviousDanger = 0,

    /// <summary>
    /// Concealed drop or false floor that causes fall damage.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Detection DC: 12 - Requires attention to notice the subtle signs.
    /// </para>
    /// <para>
    /// Consequence: 2d10 fall damage. May require DC 12 Acrobatics to halve damage.
    /// May require assistance to escape from the pit.
    /// </para>
    /// </remarks>
    HiddenPit = 1,

    /// <summary>
    /// Chemical spill, radiation zone, or biological hazard that poisons those who enter.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Detection DC: 16 - Often invisible or odorless, requires survival knowledge.
    /// </para>
    /// <para>
    /// Consequence: Apply [Poisoned] status for 3 rounds, dealing 1d6 damage per round.
    /// DC 14 ENDURANCE save at end of each turn to end early. Antidote cures immediately.
    /// </para>
    /// </remarks>
    ToxicZone = 2,

    /// <summary>
    /// Reality distortion zone where space and time behave unpredictably.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Detection DC: 20 - Extremely subtle, only visible to trained observers.
    /// </para>
    /// <para>
    /// Consequence: Roll on Glitch Effect Table (d6):
    /// <list type="bullet">
    ///   <item><description>1: Teleport 2d6 rooms in random direction</description></item>
    ///   <item><description>2: 2d10 psychic damage</description></item>
    ///   <item><description>3: Random equipment malfunction</description></item>
    ///   <item><description>4: Time skip (lose 1d4 hours)</description></item>
    ///   <item><description>5: Memory echo (narrative effect)</description></item>
    ///   <item><description>6: Reality anchor (cannot leave for 1d6 rounds)</description></item>
    /// </list>
    /// Also applies [Disoriented] status for 2 rounds.
    /// </para>
    /// </remarks>
    GlitchPocket = 3,

    /// <summary>
    /// Area showing signs of hostile presence or prepared ambush.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Detection DC: 16 - Requires reading environmental cues for danger signs.
    /// </para>
    /// <para>
    /// Consequence: Enemies gain surprise round. Player cannot act in first round of combat.
    /// Enemies attack with advantage during surprise round.
    /// </para>
    /// </remarks>
    AmbushSite = 4
}

/// <summary>
/// Extension methods for <see cref="DetectableHazardType"/>.
/// </summary>
public static class DetectableHazardTypeExtensions
{
    // ═══════════════════════════════════════════════════════════════════════════
    // DETECTION DC METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the detection difficulty class for this hazard type.
    /// </summary>
    /// <param name="hazardType">The hazard type.</param>
    /// <returns>The DC required to detect this hazard.</returns>
    /// <remarks>
    /// Detection DCs:
    /// <list type="bullet">
    ///   <item><description>ObviousDanger: DC 8</description></item>
    ///   <item><description>HiddenPit: DC 12</description></item>
    ///   <item><description>ToxicZone: DC 16</description></item>
    ///   <item><description>AmbushSite: DC 16</description></item>
    ///   <item><description>GlitchPocket: DC 20</description></item>
    /// </list>
    /// </remarks>
    public static int GetDetectionDc(this DetectableHazardType hazardType)
    {
        return hazardType switch
        {
            DetectableHazardType.ObviousDanger => 8,
            DetectableHazardType.HiddenPit => 12,
            DetectableHazardType.ToxicZone => 16,
            DetectableHazardType.GlitchPocket => 20,
            DetectableHazardType.AmbushSite => 16,
            _ => 12
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the human-readable display name for this hazard type.
    /// </summary>
    /// <param name="hazardType">The hazard type.</param>
    /// <returns>A display name suitable for UI presentation.</returns>
    public static string GetDisplayName(this DetectableHazardType hazardType)
    {
        return hazardType switch
        {
            DetectableHazardType.ObviousDanger => "Obvious Danger",
            DetectableHazardType.HiddenPit => "Hidden Pit",
            DetectableHazardType.ToxicZone => "Toxic Zone",
            DetectableHazardType.GlitchPocket => "Glitch Pocket",
            DetectableHazardType.AmbushSite => "Ambush Site",
            _ => "Unknown Hazard"
        };
    }

    /// <summary>
    /// Gets a detailed description of the hazard type.
    /// </summary>
    /// <param name="hazardType">The hazard type.</param>
    /// <returns>A descriptive string explaining the hazard.</returns>
    public static string GetDescription(this DetectableHazardType hazardType)
    {
        return hazardType switch
        {
            DetectableHazardType.ObviousDanger =>
                "Unstable debris, exposed wiring, crumbling structures, or other visible hazards " +
                "that still cause harm if walked into carelessly.",

            DetectableHazardType.HiddenPit =>
                "Concealed drops, false floors, covered holes, or unstable ground that gives way underfoot.",

            DetectableHazardType.ToxicZone =>
                "Chemical spills, radiation pockets, biological contamination, or other environmental poisons " +
                "that harm those who enter.",

            DetectableHazardType.GlitchPocket =>
                "Reality distortion zones where space, time, and matter behave unpredictably " +
                "due to lingering Jötun corruption.",

            DetectableHazardType.AmbushSite =>
                "Signs of hostile presence—fresh tracks, disturbed debris, concealed positions, " +
                "or other indicators of a prepared ambush.",

            _ => "Unknown environmental hazard."
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSEQUENCE METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the consequence description for triggering this hazard.
    /// </summary>
    /// <param name="hazardType">The hazard type.</param>
    /// <returns>A string describing what happens when the hazard is triggered.</returns>
    public static string GetConsequenceDescription(this DetectableHazardType hazardType)
    {
        return hazardType switch
        {
            DetectableHazardType.ObviousDanger =>
                "1d6 minor damage from debris, wiring, or unstable structure",

            DetectableHazardType.HiddenPit =>
                "2d10 fall damage, may require assistance to escape",

            DetectableHazardType.ToxicZone =>
                "[Poisoned] for 3 rounds, taking 1d6 damage per round",

            DetectableHazardType.GlitchPocket =>
                "Random glitch effect (teleport, psychic damage, equipment malfunction, etc.)",

            DetectableHazardType.AmbushSite =>
                "Enemies gain surprise round, you cannot act in the first round",

            _ => "Unknown hazard consequence"
        };
    }

    /// <summary>
    /// Gets the damage dice expression for this hazard type, if any.
    /// </summary>
    /// <param name="hazardType">The hazard type.</param>
    /// <returns>A dice expression string, or null if the hazard doesn't deal direct damage.</returns>
    public static string? GetDamageDice(this DetectableHazardType hazardType)
    {
        return hazardType switch
        {
            DetectableHazardType.ObviousDanger => "1d6",
            DetectableHazardType.HiddenPit => "2d10",
            DetectableHazardType.ToxicZone => null, // Damage over time, not immediate
            DetectableHazardType.GlitchPocket => null, // Varies by glitch effect
            DetectableHazardType.AmbushSite => null, // No direct damage
            _ => null
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HAZARD CHARACTERISTIC METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Determines whether this hazard deals immediate damage when triggered.
    /// </summary>
    /// <param name="hazardType">The hazard type.</param>
    /// <returns>True if the hazard deals immediate damage.</returns>
    public static bool DealsImmediateDamage(this DetectableHazardType hazardType)
    {
        return hazardType is DetectableHazardType.ObviousDanger or DetectableHazardType.HiddenPit;
    }

    /// <summary>
    /// Determines whether this hazard applies a status effect.
    /// </summary>
    /// <param name="hazardType">The hazard type.</param>
    /// <returns>True if the hazard applies a status effect.</returns>
    public static bool AppliesStatusEffect(this DetectableHazardType hazardType)
    {
        return hazardType is DetectableHazardType.ToxicZone or DetectableHazardType.GlitchPocket;
    }

    /// <summary>
    /// Determines whether this hazard may trigger combat.
    /// </summary>
    /// <param name="hazardType">The hazard type.</param>
    /// <returns>True if the hazard may result in combat.</returns>
    public static bool MayTriggerCombat(this DetectableHazardType hazardType)
    {
        return hazardType == DetectableHazardType.AmbushSite;
    }

    /// <summary>
    /// Determines whether this hazard has unpredictable effects.
    /// </summary>
    /// <param name="hazardType">The hazard type.</param>
    /// <returns>True if the hazard has random or unpredictable effects.</returns>
    public static bool HasUnpredictableEffects(this DetectableHazardType hazardType)
    {
        return hazardType == DetectableHazardType.GlitchPocket;
    }

    /// <summary>
    /// Determines whether the player may need assistance to recover from this hazard.
    /// </summary>
    /// <param name="hazardType">The hazard type.</param>
    /// <returns>True if assistance may be required.</returns>
    public static bool MayRequireAssistance(this DetectableHazardType hazardType)
    {
        return hazardType == DetectableHazardType.HiddenPit;
    }

    /// <summary>
    /// Gets the status effect name applied by this hazard, if any.
    /// </summary>
    /// <param name="hazardType">The hazard type.</param>
    /// <returns>The status effect name, or null if no status effect is applied.</returns>
    public static string? GetStatusEffectName(this DetectableHazardType hazardType)
    {
        return hazardType switch
        {
            DetectableHazardType.ToxicZone => "Poisoned",
            DetectableHazardType.GlitchPocket => "Disoriented",
            _ => null
        };
    }

    /// <summary>
    /// Gets the duration in rounds for any status effect applied by this hazard.
    /// </summary>
    /// <param name="hazardType">The hazard type.</param>
    /// <returns>The status effect duration in rounds, or 0 if no status effect.</returns>
    public static int GetStatusEffectDuration(this DetectableHazardType hazardType)
    {
        return hazardType switch
        {
            DetectableHazardType.ToxicZone => 3,
            DetectableHazardType.GlitchPocket => 2,
            _ => 0
        };
    }
}
