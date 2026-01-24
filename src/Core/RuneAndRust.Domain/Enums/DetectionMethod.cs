namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines how a hazard was detected in the Wasteland Survival system.
/// </summary>
/// <remarks>
/// <para>
/// Detection methods determine what information is revealed to the player:
/// <list type="bullet">
///   <item><description><see cref="None"/>: Detection not attempted or failed</description></item>
///   <item><description><see cref="PassivePerception"/>: Automatic awareness based on WITS</description></item>
///   <item><description><see cref="ActiveInvestigation"/>: Full skill check with investigate command</description></item>
///   <item><description><see cref="AreaSweep"/>: Multiple hazard check in single location</description></item>
/// </list>
/// </para>
/// <para>
/// Passive detection provides hints but not identification.
/// Active detection reveals full hazard information on success.
/// Critical success (net ≥ 5) provides additional context.
/// </para>
/// </remarks>
public enum DetectionMethod
{
    /// <summary>
    /// No detection attempted or detection failed.
    /// </summary>
    /// <remarks>
    /// Used when the player did not attempt to detect hazards or when
    /// an active investigation attempt failed. No information is provided
    /// about hazards in the area.
    /// </remarks>
    None = 0,

    /// <summary>
    /// Automatic detection based on WITS ÷ 2 vs hazard DC.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Passive detection occurs automatically when entering a new area.
    /// The system compares the player's WITS ÷ 2 against each hazard's DC.
    /// </para>
    /// <para>
    /// If passive value >= DC: Player receives a vague hint that something
    /// is wrong, but the hazard type is NOT revealed.
    /// </para>
    /// <para>
    /// Example hint: "Something feels wrong here. You sense danger,
    /// but can't identify its nature."
    /// </para>
    /// </remarks>
    PassivePerception = 1,

    /// <summary>
    /// Full skill check via investigate command.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Active investigation is triggered by the player using the `investigate`
    /// command. Performs a full Wasteland Survival skill check against the
    /// hazard's detection DC.
    /// </para>
    /// <para>
    /// On success: Reveals hazard type, avoidance options, and consequences.
    /// On critical (net ≥ 5): Also reveals additional context (how to disable/exploit).
    /// On failure: No information provided about the specific hazard.
    /// </para>
    /// </remarks>
    ActiveInvestigation = 2,

    /// <summary>
    /// Area sweep checking for multiple hazards.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Area sweep is a variant of active investigation that checks for all
    /// hazards in the current location with a single command.
    /// </para>
    /// <para>
    /// Triggered by `investigate area` or `sweep` commands.
    /// Each hazard requires its own skill check, but they are all performed
    /// as part of a single action.
    /// </para>
    /// </remarks>
    AreaSweep = 3
}

/// <summary>
/// Extension methods for <see cref="DetectionMethod"/>.
/// </summary>
public static class DetectionMethodExtensions
{
    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the human-readable display name for this detection method.
    /// </summary>
    /// <param name="method">The detection method.</param>
    /// <returns>A display name suitable for UI presentation.</returns>
    public static string GetDisplayName(this DetectionMethod method)
    {
        return method switch
        {
            DetectionMethod.None => "Not Detected",
            DetectionMethod.PassivePerception => "Passive Perception",
            DetectionMethod.ActiveInvestigation => "Active Investigation",
            DetectionMethod.AreaSweep => "Area Sweep",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Gets a description of the detection method.
    /// </summary>
    /// <param name="method">The detection method.</param>
    /// <returns>A descriptive string explaining how detection works.</returns>
    public static string GetDescription(this DetectionMethod method)
    {
        return method switch
        {
            DetectionMethod.None =>
                "No detection was attempted or the detection attempt failed.",

            DetectionMethod.PassivePerception =>
                "Automatic awareness based on WITS ÷ 2. Provides hints but not full identification.",

            DetectionMethod.ActiveInvestigation =>
                "Full skill check using investigate command. Reveals hazard type and avoidance options on success.",

            DetectionMethod.AreaSweep =>
                "Multiple hazard detection in a single location. Each hazard is checked separately.",

            _ => "Unknown detection method."
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CHARACTERISTIC METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Determines whether this method requires a dice roll.
    /// </summary>
    /// <param name="method">The detection method.</param>
    /// <returns>True if dice roll is required.</returns>
    public static bool RequiresDiceRoll(this DetectionMethod method)
    {
        return method is DetectionMethod.ActiveInvestigation or DetectionMethod.AreaSweep;
    }

    /// <summary>
    /// Determines whether this method is automatic (no player action required).
    /// </summary>
    /// <param name="method">The detection method.</param>
    /// <returns>True if detection happens automatically.</returns>
    public static bool IsAutomatic(this DetectionMethod method)
    {
        return method == DetectionMethod.PassivePerception;
    }

    /// <summary>
    /// Determines whether this method fully identifies the hazard on success.
    /// </summary>
    /// <param name="method">The detection method.</param>
    /// <returns>True if hazard type is revealed on success.</returns>
    public static bool RevealsHazardType(this DetectionMethod method)
    {
        return method is DetectionMethod.ActiveInvestigation or DetectionMethod.AreaSweep;
    }

    /// <summary>
    /// Determines whether this method provides only hints (not full information).
    /// </summary>
    /// <param name="method">The detection method.</param>
    /// <returns>True if only vague hints are provided.</returns>
    public static bool ProvidesHintOnly(this DetectionMethod method)
    {
        return method == DetectionMethod.PassivePerception;
    }

    /// <summary>
    /// Determines whether this method can detect multiple hazards at once.
    /// </summary>
    /// <param name="method">The detection method.</param>
    /// <returns>True if multiple hazards can be detected in one action.</returns>
    public static bool CanDetectMultipleHazards(this DetectionMethod method)
    {
        // All methods can detect multiple hazards, but AreaSweep explicitly
        // checks all hazards in a location with a single player action
        return method == DetectionMethod.AreaSweep;
    }

    /// <summary>
    /// Determines whether this method was successful in detecting something.
    /// </summary>
    /// <param name="method">The detection method.</param>
    /// <returns>True if this method indicates some level of detection success.</returns>
    public static bool WasSuccessful(this DetectionMethod method)
    {
        return method != DetectionMethod.None;
    }
}
