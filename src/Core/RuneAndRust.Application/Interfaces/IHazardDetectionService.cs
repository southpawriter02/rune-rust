using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service interface for hazard detection operations in the Wasteland Survival system.
/// </summary>
/// <remarks>
/// <para>
/// Provides functionality for characters to detect and identify environmental hazards
/// using the Wasteland Survival skill. Detection can occur passively (automatic hints
/// for high-WITS characters) or actively (formal skill checks for identification).
/// </para>
/// <para>
/// Detection methods:
/// <list type="bullet">
///   <item><description>Passive: WITS ÷ 2 vs hazard DC (hint only, no type revealed)</description></item>
///   <item><description>Active: Full Wasteland Survival check (full identification)</description></item>
///   <item><description>Critical: Net successes ≥ 5 (additional context revealed)</description></item>
///   <item><description>Area Sweep: Multiple hazard detection in single location</description></item>
/// </list>
/// </para>
/// <para>
/// Hazard types and detection DCs:
/// <list type="bullet">
///   <item><description>ObviousDanger: DC 8 (unstable debris, exposed wiring)</description></item>
///   <item><description>HiddenPit: DC 12 (concealed drops, false floors)</description></item>
///   <item><description>ToxicZone: DC 16 (chemical spills, radiation)</description></item>
///   <item><description>AmbushSite: DC 16 (signs of hostile presence)</description></item>
///   <item><description>GlitchPocket: DC 20 (reality distortion zones)</description></item>
/// </list>
/// </para>
/// </remarks>
public interface IHazardDetectionService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PASSIVE DETECTION (Automatic on Area Entry)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks for passive detection of hazards when entering an area.
    /// </summary>
    /// <param name="player">The player entering the area.</param>
    /// <param name="hazards">The hazards present in the area.</param>
    /// <returns>
    /// A list of <see cref="HazardDetectionResult"/> for hazards that triggered
    /// passive awareness. Results will have <see cref="DetectionMethod.PassivePerception"/>
    /// and will NOT reveal the specific hazard type (hint only).
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="player"/> or <paramref name="hazards"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Passive detection occurs automatically when entering a new area.
    /// Compares the player's WITS ÷ 2 against each hazard's DC.
    /// </para>
    /// <para>
    /// If passive value &gt;= DC, the player receives a vague hint that something
    /// is wrong, but the hazard type is NOT revealed.
    /// </para>
    /// <para>
    /// Example:
    /// <code>
    /// var hazards = new[] { DetectableHazardType.HiddenPit, DetectableHazardType.ToxicZone };
    /// var results = hazardService.CheckPassiveDetection(player, hazards);
    /// foreach (var result in results)
    /// {
    ///     Console.WriteLine(result.ToDisplayString()); // "Something feels wrong here..."
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    IReadOnlyList<HazardDetectionResult> CheckPassiveDetection(
        Player player,
        IReadOnlyList<DetectableHazardType> hazards);

    /// <summary>
    /// Calculates the player's passive perception value.
    /// </summary>
    /// <param name="player">The player to calculate for.</param>
    /// <returns>The passive perception value (WITS ÷ 2).</returns>
    int GetPassivePerception(Player player);

    // ═══════════════════════════════════════════════════════════════════════════
    // ACTIVE DETECTION (Player-Initiated Investigation)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Actively investigates an area for all hazards present.
    /// </summary>
    /// <param name="player">The player investigating.</param>
    /// <param name="hazards">The hazards present in the area.</param>
    /// <returns>
    /// A list of <see cref="HazardDetectionResult"/> for each hazard in the area.
    /// Successful detections will include hazard type and avoidance options.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="player"/> or <paramref name="hazards"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Area sweep performs a separate Wasteland Survival check against each hazard.
    /// This is triggered by the "investigate area" or "sweep" commands.
    /// </para>
    /// <para>
    /// Example:
    /// <code>
    /// var hazards = new[] { DetectableHazardType.HiddenPit, DetectableHazardType.AmbushSite };
    /// var results = hazardService.InvestigateArea(player, hazards);
    /// foreach (var result in results.Where(r => r.HazardDetected))
    /// {
    ///     Console.WriteLine(result.ToDisplayString());
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    IReadOnlyList<HazardDetectionResult> InvestigateArea(
        Player player,
        IReadOnlyList<DetectableHazardType> hazards);

    /// <summary>
    /// Actively investigates for a specific hazard type.
    /// </summary>
    /// <param name="player">The player investigating.</param>
    /// <param name="hazardType">The type of hazard to investigate.</param>
    /// <returns>
    /// A <see cref="HazardDetectionResult"/> indicating whether the hazard was detected.
    /// On success, includes hazard type, avoidance options, and consequences.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="player"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Performs a single Wasteland Survival skill check against the hazard's DC.
    /// This is triggered by the "investigate" command.
    /// </para>
    /// <para>
    /// On success (net successes &gt;= DC): Reveals hazard type, avoidance options,
    /// and consequence description.
    /// </para>
    /// <para>
    /// On critical success (net successes &gt;= 5): Also provides additional context
    /// on how to disable or exploit the hazard.
    /// </para>
    /// <para>
    /// Example:
    /// <code>
    /// var result = hazardService.InvestigateSpecific(player, DetectableHazardType.GlitchPocket);
    /// if (result.FullyIdentified)
    /// {
    ///     Console.WriteLine($"Found: {result.HazardType}");
    ///     foreach (var option in result.AvoidanceOptions)
    ///     {
    ///         Console.WriteLine($"  - {option}");
    ///     }
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    HazardDetectionResult InvestigateSpecific(Player player, DetectableHazardType hazardType);

    // ═══════════════════════════════════════════════════════════════════════════
    // HAZARD INFORMATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the detection difficulty class for a hazard type.
    /// </summary>
    /// <param name="hazardType">The hazard type.</param>
    /// <returns>The detection DC for this hazard type.</returns>
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
    int GetDetectionDc(DetectableHazardType hazardType);

    /// <summary>
    /// Gets a human-readable description of a hazard type.
    /// </summary>
    /// <param name="hazardType">The hazard type.</param>
    /// <returns>A tuple of (DisplayName, Description) for the hazard.</returns>
    (string DisplayName, string Description) GetHazardDescription(DetectableHazardType hazardType);

    /// <summary>
    /// Gets the consequence description for triggering a hazard type.
    /// </summary>
    /// <param name="hazardType">The hazard type.</param>
    /// <returns>A description of what happens when the hazard is triggered.</returns>
    /// <remarks>
    /// Consequence descriptions include:
    /// <list type="bullet">
    ///   <item><description>ObviousDanger: "1d6 minor damage from debris, wiring, or unstable structure"</description></item>
    ///   <item><description>HiddenPit: "2d10 fall damage, may require assistance to escape"</description></item>
    ///   <item><description>ToxicZone: "[Poisoned] for 3 rounds, taking 1d6 damage per round"</description></item>
    ///   <item><description>GlitchPocket: "Random glitch effect (teleport, psychic damage, equipment malfunction, etc.)"</description></item>
    ///   <item><description>AmbushSite: "Enemies gain surprise round, you cannot act in the first round"</description></item>
    /// </list>
    /// </remarks>
    string GetHazardConsequence(DetectableHazardType hazardType);

    /// <summary>
    /// Gets the avoidance options for a detected hazard type.
    /// </summary>
    /// <param name="hazardType">The hazard type.</param>
    /// <returns>A list of ways to avoid or circumvent the hazard.</returns>
    IReadOnlyList<string> GetAvoidanceOptions(DetectableHazardType hazardType);

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSEQUENCE APPLICATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Applies the consequence of triggering an undetected hazard.
    /// </summary>
    /// <param name="player">The player triggering the hazard.</param>
    /// <param name="hazardType">The type of hazard triggered.</param>
    /// <returns>
    /// A <see cref="HazardTriggerResult"/> describing the consequence including
    /// damage dealt, status effects applied, and special effects.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="player"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method should be called when a player triggers an undetected hazard
    /// by proceeding through an area without detection or avoidance.
    /// </para>
    /// <para>
    /// Consequences by hazard type:
    /// <list type="bullet">
    ///   <item><description>ObviousDanger: Deals 1d6 physical damage</description></item>
    ///   <item><description>HiddenPit: Deals 2d10 fall damage, may require assistance</description></item>
    ///   <item><description>ToxicZone: Applies [Poisoned] status for 3 rounds</description></item>
    ///   <item><description>GlitchPocket: Rolls on Glitch Effect Table, applies [Disoriented]</description></item>
    ///   <item><description>AmbushSite: Triggers surprise round for enemies</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Example:
    /// <code>
    /// // Player proceeds without detecting the hidden pit
    /// var result = hazardService.ApplyHazardConsequence(player, DetectableHazardType.HiddenPit);
    /// Console.WriteLine(result.ToDisplayString()); // "You triggered a Hidden Pit! You take 14 physical damage..."
    /// </code>
    /// </para>
    /// </remarks>
    HazardTriggerResult ApplyHazardConsequence(Player player, DetectableHazardType hazardType);

    // ═══════════════════════════════════════════════════════════════════════════
    // GLITCH POCKET SPECIFICS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Rolls on the Glitch Effect Table to determine a random effect.
    /// </summary>
    /// <returns>
    /// A tuple of (roll, effectName, effectDescription) for the rolled effect.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Glitch Effect Table (d6):
    /// <list type="bullet">
    ///   <item><description>1: Teleport - Move 2d6 rooms in random direction</description></item>
    ///   <item><description>2: Psychic Feedback - Take 2d10 psychic damage</description></item>
    ///   <item><description>3: Equipment Malfunction - Random item disabled until repaired</description></item>
    ///   <item><description>4: Time Skip - Lose 1d4 hours</description></item>
    ///   <item><description>5: Memory Echo - Vivid flashback of past combat</description></item>
    ///   <item><description>6: Reality Anchor - Cannot leave room for 1d6 rounds</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    (int Roll, string EffectName, string EffectDescription) RollGlitchEffect();

    /// <summary>
    /// Gets the description for a specific glitch effect roll.
    /// </summary>
    /// <param name="roll">The d6 roll (1-6).</param>
    /// <returns>A tuple of (effectName, effectDescription) for the specified roll.</returns>
    (string EffectName, string EffectDescription) GetGlitchEffectDescription(int roll);

    // ═══════════════════════════════════════════════════════════════════════════
    // DETECTION CHECK PREREQUISITES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks whether the player can perform an investigation check.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player can investigate; otherwise, false.</returns>
    /// <remarks>
    /// Investigation may be blocked by:
    /// <list type="bullet">
    ///   <item><description>Active Disoriented status effect</description></item>
    ///   <item><description>Active Blinded status effect</description></item>
    ///   <item><description>Other incapacitating conditions</description></item>
    /// </list>
    /// </remarks>
    bool CanInvestigate(Player player);

    /// <summary>
    /// Gets the reason why investigation is blocked, if any.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>A human-readable reason why investigation is blocked, or null if allowed.</returns>
    string? GetInvestigationBlockedReason(Player player);
}
