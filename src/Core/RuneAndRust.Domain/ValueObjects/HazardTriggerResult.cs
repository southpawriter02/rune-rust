using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the result of triggering a hazard in the Wasteland Survival system.
/// </summary>
/// <remarks>
/// <para>
/// HazardTriggerResult captures all information about what happens when a player
/// triggers an undetected hazard:
/// <list type="bullet">
///   <item><description>The type of hazard that was triggered</description></item>
///   <item><description>Damage dealt (if any)</description></item>
///   <item><description>Status effects applied (if any)</description></item>
///   <item><description>Special effects or narrative consequences</description></item>
///   <item><description>Whether assistance is required to recover</description></item>
/// </list>
/// </para>
/// <para>
/// Hazard consequences vary by type:
/// <list type="bullet">
///   <item><description>ObviousDanger: 1d6 minor damage</description></item>
///   <item><description>HiddenPit: 2d10 fall damage, may require assistance</description></item>
///   <item><description>ToxicZone: [Poisoned] status for 3 rounds</description></item>
///   <item><description>GlitchPocket: Random effect from Glitch Effect Table + [Disoriented]</description></item>
///   <item><description>AmbushSite: Enemies gain surprise round</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="HazardType">The type of hazard that was triggered.</param>
/// <param name="DamageDealt">Amount of damage dealt (0 if no immediate damage).</param>
/// <param name="DamageType">Type of damage dealt (Physical, Psychic, etc.).</param>
/// <param name="StatusApplied">Status effect applied (null if none).</param>
/// <param name="StatusDuration">Duration of status effect in rounds (0 if no status).</param>
/// <param name="SpecialEffect">Description of any special effect (glitch, ambush, etc.).</param>
/// <param name="RequiresAssistance">Whether player needs help to recover from this hazard.</param>
/// <param name="TriggersSurpriseRound">Whether enemies gain a surprise round.</param>
/// <param name="GlitchEffectRoll">The d6 roll for Glitch Pocket effects (null if not applicable).</param>
/// <param name="RollDetails">Optional details about dice rolls for logging/display.</param>
public readonly record struct HazardTriggerResult(
    DetectableHazardType HazardType,
    int DamageDealt,
    string? DamageType,
    string? StatusApplied,
    int StatusDuration,
    string? SpecialEffect,
    bool RequiresAssistance,
    bool TriggersSurpriseRound = false,
    int? GlitchEffectRoll = null,
    string? RollDetails = null)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether the hazard dealt any immediate damage.
    /// </summary>
    public bool DealtDamage => DamageDealt > 0;

    /// <summary>
    /// Gets whether the hazard applied a status effect.
    /// </summary>
    public bool AppliedStatus => !string.IsNullOrEmpty(StatusApplied);

    /// <summary>
    /// Gets whether the hazard has a special effect beyond damage and status.
    /// </summary>
    public bool HasSpecialEffect => !string.IsNullOrEmpty(SpecialEffect);

    /// <summary>
    /// Gets whether this hazard triggers combat (ambush site only).
    /// </summary>
    public bool TriggersCombat => TriggersSurpriseRound;

    /// <summary>
    /// Gets whether this was a glitch pocket effect.
    /// </summary>
    public bool IsGlitchEffect => GlitchEffectRoll.HasValue;

    /// <summary>
    /// Gets the severity level based on consequences (1 = minor, 2 = moderate, 3 = severe).
    /// </summary>
    /// <remarks>
    /// Severity is determined by the worst consequence:
    /// <list type="bullet">
    ///   <item><description>Minor (1): Small damage or short status effect</description></item>
    ///   <item><description>Moderate (2): Significant damage, longer status, or assistance needed</description></item>
    ///   <item><description>Severe (3): Combat trigger, glitch effects, or major consequences</description></item>
    /// </list>
    /// </remarks>
    public int Severity
    {
        get
        {
            // Severe: combat trigger or glitch effects
            if (TriggersSurpriseRound || IsGlitchEffect)
                return 3;

            // Moderate: significant damage, assistance needed, or longer status
            if (DamageDealt >= 10 || RequiresAssistance || StatusDuration >= 3)
                return 2;

            // Minor: everything else
            return 1;
        }
    }

    /// <summary>
    /// Gets a severity label for display purposes.
    /// </summary>
    public string SeverityLabel => Severity switch
    {
        1 => "Minor",
        2 => "Moderate",
        3 => "Severe",
        _ => "Unknown"
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a result for triggering an Obvious Danger hazard.
    /// </summary>
    /// <param name="damageDealt">The 1d6 damage dealt.</param>
    /// <param name="rollDetails">Optional dice roll details.</param>
    /// <returns>A trigger result for obvious danger.</returns>
    /// <remarks>
    /// Obvious dangers cause 1d6 minor damage from debris, exposed wiring,
    /// or unstable structures. No status effects or lasting consequences.
    /// </remarks>
    public static HazardTriggerResult ObviousDanger(int damageDealt, string? rollDetails = null) =>
        new(
            HazardType: DetectableHazardType.ObviousDanger,
            DamageDealt: damageDealt,
            DamageType: "Physical",
            StatusApplied: null,
            StatusDuration: 0,
            SpecialEffect: null,
            RequiresAssistance: false,
            TriggersSurpriseRound: false,
            GlitchEffectRoll: null,
            RollDetails: rollDetails ?? $"Obvious danger dealt {damageDealt} damage");

    /// <summary>
    /// Creates a result for triggering a Hidden Pit hazard.
    /// </summary>
    /// <param name="damageDealt">The 2d10 fall damage dealt.</param>
    /// <param name="rollDetails">Optional dice roll details.</param>
    /// <returns>A trigger result for hidden pit.</returns>
    /// <remarks>
    /// Hidden pits cause 2d10 fall damage and may require assistance to escape.
    /// Player may attempt DC 12 Acrobatics to halve damage.
    /// </remarks>
    public static HazardTriggerResult HiddenPit(int damageDealt, string? rollDetails = null) =>
        new(
            HazardType: DetectableHazardType.HiddenPit,
            DamageDealt: damageDealt,
            DamageType: "Physical",
            StatusApplied: null,
            StatusDuration: 0,
            SpecialEffect: "You fall into a concealed pit! You may need assistance to climb out.",
            RequiresAssistance: true,
            TriggersSurpriseRound: false,
            GlitchEffectRoll: null,
            RollDetails: rollDetails ?? $"Hidden pit dealt {damageDealt} fall damage");

    /// <summary>
    /// Creates a result for triggering a Toxic Zone hazard.
    /// </summary>
    /// <param name="rollDetails">Optional details for logging.</param>
    /// <returns>A trigger result for toxic zone.</returns>
    /// <remarks>
    /// Toxic zones apply [Poisoned] status for 3 rounds.
    /// While poisoned, the character takes 1d6 damage per round.
    /// DC 14 ENDURANCE save at end of each turn to end early.
    /// Antidote consumable immediately removes the poison.
    /// </remarks>
    public static HazardTriggerResult ToxicZone(string? rollDetails = null) =>
        new(
            HazardType: DetectableHazardType.ToxicZone,
            DamageDealt: 0,
            DamageType: null,
            StatusApplied: "Poisoned",
            StatusDuration: 3,
            SpecialEffect: "You inhale toxic fumes! You take 1d6 damage at the start of each round. " +
                          "DC 14 ENDURANCE save at end of turn to end early. Antidote cures immediately.",
            RequiresAssistance: false,
            TriggersSurpriseRound: false,
            GlitchEffectRoll: null,
            RollDetails: rollDetails ?? "Toxic zone applied Poisoned status for 3 rounds");

    /// <summary>
    /// Creates a result for triggering a Glitch Pocket hazard.
    /// </summary>
    /// <param name="effectRoll">The d6 roll determining which glitch effect occurs.</param>
    /// <param name="effectDescription">Description of the specific glitch effect.</param>
    /// <param name="damageDealt">Damage dealt (if effect roll was 2 for psychic damage).</param>
    /// <param name="rollDetails">Optional dice roll details.</param>
    /// <returns>A trigger result for glitch pocket.</returns>
    /// <remarks>
    /// <para>
    /// Glitch pockets cause random effects based on d6 roll:
    /// <list type="bullet">
    ///   <item><description>1: Teleport 2d6 rooms in random direction</description></item>
    ///   <item><description>2: 2d10 psychic damage</description></item>
    ///   <item><description>3: Random equipment malfunction</description></item>
    ///   <item><description>4: Time skip (lose 1d4 hours)</description></item>
    ///   <item><description>5: Memory echo (narrative effect)</description></item>
    ///   <item><description>6: Reality anchor (cannot leave for 1d6 rounds)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// All glitch pocket effects also apply [Disoriented] status for 2 rounds.
    /// </para>
    /// </remarks>
    public static HazardTriggerResult GlitchPocket(
        int effectRoll,
        string effectDescription,
        int damageDealt = 0,
        string? rollDetails = null) =>
        new(
            HazardType: DetectableHazardType.GlitchPocket,
            DamageDealt: damageDealt,
            DamageType: damageDealt > 0 ? "Psychic" : null,
            StatusApplied: "Disoriented",
            StatusDuration: 2,
            SpecialEffect: effectDescription,
            RequiresAssistance: false,
            TriggersSurpriseRound: false,
            GlitchEffectRoll: effectRoll,
            RollDetails: rollDetails ?? $"Glitch pocket effect roll: {effectRoll}");

    /// <summary>
    /// Creates a result for triggering an Ambush Site hazard.
    /// </summary>
    /// <param name="rollDetails">Optional details for logging.</param>
    /// <returns>A trigger result for ambush site.</returns>
    /// <remarks>
    /// Ambush sites cause enemies to gain a surprise round.
    /// The player cannot act in the first round of combat.
    /// Enemies attack with advantage during the surprise round.
    /// </remarks>
    public static HazardTriggerResult AmbushSite(string? rollDetails = null) =>
        new(
            HazardType: DetectableHazardType.AmbushSite,
            DamageDealt: 0,
            DamageType: null,
            StatusApplied: null,
            StatusDuration: 0,
            SpecialEffect: "Enemies spring from concealment! They gain a surprise round—" +
                          "you cannot act in the first round of combat and enemies attack with advantage!",
            RequiresAssistance: false,
            TriggersSurpriseRound: true,
            GlitchEffectRoll: null,
            RollDetails: rollDetails ?? "Ambush triggered - enemies gain surprise round");

    /// <summary>
    /// Creates an empty result representing no hazard triggered.
    /// </summary>
    /// <returns>An empty trigger result.</returns>
    public static HazardTriggerResult Empty() =>
        new(
            HazardType: DetectableHazardType.ObviousDanger,
            DamageDealt: 0,
            DamageType: null,
            StatusApplied: null,
            StatusDuration: 0,
            SpecialEffect: null,
            RequiresAssistance: false,
            TriggersSurpriseRound: false,
            GlitchEffectRoll: null,
            RollDetails: "No hazard triggered");

    // ═══════════════════════════════════════════════════════════════════════════
    // GLITCH EFFECT HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the glitch effect description based on a d6 roll.
    /// </summary>
    /// <param name="roll">The d6 roll (1-6).</param>
    /// <returns>A description of the glitch effect.</returns>
    /// <remarks>
    /// This method is used when rolling on the Glitch Effect Table.
    /// Call this after rolling 1d6 to determine the effect description.
    /// </remarks>
    public static string GetGlitchEffectDescription(int roll) =>
        roll switch
        {
            1 => "Reality tears—you are teleported 2d6 rooms in a random direction!",
            2 => "Psychic feedback—you take 2d10 psychic damage!",
            3 => "Equipment malfunction—a random item is disabled until repaired!",
            4 => "Time skip—you lose 1d4 hours, disoriented and confused!",
            5 => "Memory echo—you experience a vivid flashback of past combat!",
            6 => "Reality anchor—you cannot leave this room for 1d6 rounds!",
            _ => "Strange glitch effect—reality shimmers around you."
        };

    /// <summary>
    /// Gets the name of a glitch effect based on a d6 roll.
    /// </summary>
    /// <param name="roll">The d6 roll (1-6).</param>
    /// <returns>The name of the glitch effect.</returns>
    public static string GetGlitchEffectName(int roll) =>
        roll switch
        {
            1 => "Teleport",
            2 => "Psychic Feedback",
            3 => "Equipment Malfunction",
            4 => "Time Skip",
            5 => "Memory Echo",
            6 => "Reality Anchor",
            _ => "Unknown Effect"
        };

    /// <summary>
    /// Determines whether a glitch effect roll causes damage.
    /// </summary>
    /// <param name="roll">The d6 roll (1-6).</param>
    /// <returns>True if the effect causes damage (roll of 2).</returns>
    public static bool GlitchEffectCausesDamage(int roll) => roll == 2;

    // ═══════════════════════════════════════════════════════════════════════════
    // STRING FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a display string describing what happened for the player.
    /// </summary>
    /// <returns>A formatted string suitable for display to the player.</returns>
    public string ToDisplayString()
    {
        var parts = new List<string>
        {
            $"You triggered a {HazardType.GetDisplayName()}!"
        };

        if (DealtDamage)
        {
            parts.Add($"You take {DamageDealt} {DamageType?.ToLowerInvariant() ?? "unknown"} damage.");
        }

        if (AppliedStatus)
        {
            parts.Add($"You are now [{StatusApplied}] for {StatusDuration} round{(StatusDuration != 1 ? "s" : "")}.");
        }

        if (HasSpecialEffect)
        {
            parts.Add(SpecialEffect!);
        }

        if (RequiresAssistance)
        {
            parts.Add("You may need assistance to recover from this.");
        }

        return string.Join("\n", parts);
    }

    /// <summary>
    /// Returns a detailed diagnostic string for logging.
    /// </summary>
    /// <returns>A multi-line string with complete result details.</returns>
    public string ToDetailedString()
    {
        var details = $"HazardTriggerResult\n" +
                      $"  Hazard Type: {HazardType.GetDisplayName()}\n" +
                      $"  Severity: {SeverityLabel} ({Severity})\n" +
                      $"  Damage Dealt: {DamageDealt}\n" +
                      $"  Damage Type: {DamageType ?? "None"}\n" +
                      $"  Status Applied: {StatusApplied ?? "None"}\n" +
                      $"  Status Duration: {StatusDuration} rounds\n" +
                      $"  Requires Assistance: {RequiresAssistance}\n" +
                      $"  Triggers Surprise Round: {TriggersSurpriseRound}\n";

        if (IsGlitchEffect)
        {
            details += $"  Glitch Effect Roll: {GlitchEffectRoll}\n" +
                      $"  Glitch Effect: {GetGlitchEffectName(GlitchEffectRoll!.Value)}\n";
        }

        if (HasSpecialEffect)
        {
            details += $"  Special Effect: {SpecialEffect}\n";
        }

        if (!string.IsNullOrEmpty(RollDetails))
        {
            details += $"  Roll Details: {RollDetails}\n";
        }

        return details;
    }

    /// <summary>
    /// Returns a human-readable summary of the trigger result.
    /// </summary>
    /// <returns>A concise string describing the outcome.</returns>
    public override string ToString()
    {
        var summary = $"{HazardType.GetDisplayName()} triggered";

        if (DealtDamage)
        {
            summary += $" ({DamageDealt} damage)";
        }

        if (AppliedStatus)
        {
            summary += $" [{StatusApplied}]";
        }

        if (TriggersSurpriseRound)
        {
            summary += " [AMBUSH]";
        }

        if (IsGlitchEffect)
        {
            summary += $" [GLITCH: {GetGlitchEffectName(GlitchEffectRoll!.Value)}]";
        }

        return summary;
    }
}
