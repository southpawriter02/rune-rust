using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the outcome of a scavenger sign interpretation attempt.
/// </summary>
/// <remarks>
/// <para>
/// SignInterpretationResult captures all information about an interpretation attempt:
/// <list type="bullet">
///   <item><description>Interpreted - Whether the sign was successfully interpreted</description></item>
///   <item><description>SignType - The type of sign (if identified)</description></item>
///   <item><description>Meaning - The understood meaning of the sign</description></item>
///   <item><description>FactionName - The faction that left the sign (if identified)</description></item>
///   <item><description>AdditionalContext - Extra information from critical success</description></item>
///   <item><description>IsMisinterpretation - Whether this is a fumble with false information</description></item>
/// </list>
/// </para>
/// <para>
/// Interpretation outcomes:
/// <list type="bullet">
///   <item><description>Success: Net successes >= DC - Sign type and meaning revealed</description></item>
///   <item><description>Critical Success: Net successes >= 5 - Additional context provided</description></item>
///   <item><description>Failure: Net successes &lt; DC - No information gained</description></item>
///   <item><description>Fumble: 0 successes + botch - Misinterpretation with false info</description></item>
/// </list>
/// </para>
/// <para>
/// Misinterpretations are particularly dangerous—the player receives false information
/// and believes it to be true, potentially leading them into danger or conflict.
/// </para>
/// </remarks>
/// <param name="Interpreted">Whether the sign was successfully interpreted.</param>
/// <param name="SignType">The type of sign, if identified.</param>
/// <param name="Meaning">The understood meaning of the sign.</param>
/// <param name="FactionName">The display name of the faction that left the sign.</param>
/// <param name="FactionKnown">Whether the faction was previously known to the player.</param>
/// <param name="AdditionalContext">Extra information from critical success.</param>
/// <param name="IsMisinterpretation">Whether this is a fumble misinterpretation.</param>
/// <param name="NetSuccesses">Number of net successes on the check.</param>
/// <param name="TargetDc">The target DC for the interpretation attempt.</param>
/// <param name="RollDetails">Optional details about the dice roll for logging/display.</param>
public readonly record struct SignInterpretationResult(
    bool Interpreted,
    ScavengerSignType? SignType,
    string Meaning,
    string? FactionName,
    bool FactionKnown,
    string? AdditionalContext,
    bool IsMisinterpretation,
    int NetSuccesses,
    int TargetDc = 0,
    string? RollDetails = null)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this was a critical success (additional context available).
    /// </summary>
    /// <remarks>
    /// Critical success occurs when net successes >= 5 on a successful interpretation.
    /// Critical success provides deeper insight including sign age, faction behavior patterns,
    /// time-based restrictions, or hidden nuances in the marking.
    /// </remarks>
    public bool IsCritical => Interpreted && !IsMisinterpretation && NetSuccesses >= CriticalSuccessThreshold;

    /// <summary>
    /// Gets whether interpretation failed (no information gained).
    /// </summary>
    public bool IsFailed => !Interpreted && !IsMisinterpretation;

    /// <summary>
    /// Gets the margin by which the interpretation succeeded or failed.
    /// </summary>
    /// <remarks>
    /// Positive values indicate success margin (how much above DC).
    /// Negative values indicate failure margin (how much below DC).
    /// Only meaningful for non-fumble results.
    /// </remarks>
    public int Margin => NetSuccesses - TargetDc;

    /// <summary>
    /// Gets whether the player gained any information (true or false).
    /// </summary>
    /// <remarks>
    /// Returns true for both successful interpretations AND misinterpretations,
    /// since in both cases the player believes they have learned something.
    /// </remarks>
    public bool GainedInformation => Interpreted;

    /// <summary>
    /// Gets whether the information gained is accurate.
    /// </summary>
    /// <remarks>
    /// Returns true only for successful interpretations that are not misinterpretations.
    /// Use this to determine if the player's understanding is correct.
    /// </remarks>
    public bool IsAccurate => Interpreted && !IsMisinterpretation;

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a result for a successfully interpreted sign.
    /// </summary>
    /// <param name="signType">The type of sign identified.</param>
    /// <param name="meaning">The meaning of the sign.</param>
    /// <param name="factionName">The display name of the faction.</param>
    /// <param name="factionKnown">Whether the faction was known to the player.</param>
    /// <param name="netSuccesses">Net successes from the skill check.</param>
    /// <param name="targetDc">The target DC for the interpretation.</param>
    /// <param name="rollDetails">Optional roll details for logging.</param>
    /// <returns>A successful interpretation result.</returns>
    public static SignInterpretationResult Success(
        ScavengerSignType signType,
        string meaning,
        string? factionName,
        bool factionKnown,
        int netSuccesses,
        int targetDc = 0,
        string? rollDetails = null)
    {
        return new SignInterpretationResult(
            Interpreted: true,
            SignType: signType,
            Meaning: meaning,
            FactionName: factionName,
            FactionKnown: factionKnown,
            AdditionalContext: null,
            IsMisinterpretation: false,
            NetSuccesses: netSuccesses,
            TargetDc: targetDc,
            RollDetails: rollDetails);
    }

    /// <summary>
    /// Creates a result for a critical success with additional context.
    /// </summary>
    /// <param name="signType">The type of sign identified.</param>
    /// <param name="meaning">The meaning of the sign.</param>
    /// <param name="factionName">The display name of the faction.</param>
    /// <param name="factionKnown">Whether the faction was known to the player.</param>
    /// <param name="additionalContext">Additional context from critical success.</param>
    /// <param name="netSuccesses">Net successes from the skill check.</param>
    /// <param name="targetDc">The target DC for the interpretation.</param>
    /// <param name="rollDetails">Optional roll details for logging.</param>
    /// <returns>A critical success interpretation result.</returns>
    public static SignInterpretationResult CriticalSuccess(
        ScavengerSignType signType,
        string meaning,
        string? factionName,
        bool factionKnown,
        string additionalContext,
        int netSuccesses,
        int targetDc = 0,
        string? rollDetails = null)
    {
        return new SignInterpretationResult(
            Interpreted: true,
            SignType: signType,
            Meaning: meaning,
            FactionName: factionName,
            FactionKnown: factionKnown,
            AdditionalContext: additionalContext,
            IsMisinterpretation: false,
            NetSuccesses: netSuccesses,
            TargetDc: targetDc,
            RollDetails: rollDetails);
    }

    /// <summary>
    /// Creates a result for a failed interpretation.
    /// </summary>
    /// <param name="netSuccesses">Net successes from the failed check.</param>
    /// <param name="targetDc">The target DC that was not met.</param>
    /// <param name="rollDetails">Optional roll details for logging.</param>
    /// <returns>A failed interpretation result.</returns>
    public static SignInterpretationResult Failure(
        int netSuccesses = 0,
        int targetDc = 0,
        string? rollDetails = null)
    {
        return new SignInterpretationResult(
            Interpreted: false,
            SignType: null,
            Meaning: "The markings are incomprehensible to you.",
            FactionName: null,
            FactionKnown: false,
            AdditionalContext: null,
            IsMisinterpretation: false,
            NetSuccesses: netSuccesses,
            TargetDc: targetDc,
            RollDetails: rollDetails);
    }

    /// <summary>
    /// Creates a result for a fumble misinterpretation.
    /// </summary>
    /// <param name="signType">The actual sign type (player believes they identified it).</param>
    /// <param name="falseMeaning">The incorrect meaning the player believes.</param>
    /// <param name="falseFactionName">The faction the player incorrectly attributes the sign to.</param>
    /// <param name="targetDc">The target DC for the interpretation.</param>
    /// <param name="rollDetails">Optional roll details for logging.</param>
    /// <returns>A misinterpretation result from a fumble.</returns>
    /// <remarks>
    /// The player believes they have successfully interpreted the sign, but the information
    /// is incorrect. This can lead to dangerous situations:
    /// <list type="bullet">
    ///   <item><description>Territory markers misread as safe havens → faction hostility</description></item>
    ///   <item><description>Warning signs misread as salvage → walking into danger</description></item>
    ///   <item><description>Taboo signs misread as treasure → triggering supernatural effects</description></item>
    /// </list>
    /// </remarks>
    public static SignInterpretationResult Misinterpretation(
        ScavengerSignType signType,
        string falseMeaning,
        string? falseFactionName,
        int targetDc = 0,
        string? rollDetails = null)
    {
        return new SignInterpretationResult(
            Interpreted: true, // Player thinks they understood it
            SignType: signType,
            Meaning: falseMeaning,
            FactionName: falseFactionName,
            FactionKnown: true, // Player thinks they know the faction
            AdditionalContext: null,
            IsMisinterpretation: true,
            NetSuccesses: 0,
            TargetDc: targetDc,
            RollDetails: rollDetails ?? "Fumble - misinterpretation");
    }

    /// <summary>
    /// Creates an empty result representing no interpretation attempted.
    /// </summary>
    /// <returns>An empty interpretation result.</returns>
    public static SignInterpretationResult Empty()
    {
        return new SignInterpretationResult(
            Interpreted: false,
            SignType: null,
            Meaning: string.Empty,
            FactionName: null,
            FactionKnown: false,
            AdditionalContext: null,
            IsMisinterpretation: false,
            NetSuccesses: 0,
            TargetDc: 0,
            RollDetails: "No interpretation attempted");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STRING FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a display string describing the interpretation result for the player.
    /// </summary>
    /// <returns>A formatted string suitable for display to the player.</returns>
    /// <remarks>
    /// Note: For misinterpretations, this returns the false information as if it were true,
    /// since the player believes they have correctly interpreted the sign.
    /// </remarks>
    public string ToDisplayString()
    {
        // Failed interpretation
        if (!Interpreted)
        {
            return "You study the markings carefully, but their meaning eludes you. " +
                   "Perhaps with more time or knowledge, you could decipher them.";
        }

        // Build result string
        var result = $"You recognize this as a {SignType?.GetDisplayName() ?? "scavenger sign"}";

        // Add faction information
        if (!string.IsNullOrEmpty(FactionName))
        {
            result += FactionKnown
                ? $" left by the {FactionName}.\n"
                : $" left by an unfamiliar faction (possibly the {FactionName}).\n";
        }
        else
        {
            result += " of unknown origin.\n";
        }

        // Add meaning
        result += $"\nMeaning: {Meaning}";

        // Add critical success context
        if (IsCritical && !string.IsNullOrEmpty(AdditionalContext))
        {
            result += $"\n\n[CRITICAL] Additional insight: {AdditionalContext}";
        }

        return result;
    }

    /// <summary>
    /// Returns a detailed diagnostic string for logging.
    /// </summary>
    /// <returns>A multi-line string with complete result details.</returns>
    public string ToDetailedString()
    {
        var details = $"SignInterpretationResult\n" +
                      $"  Interpreted: {Interpreted}\n" +
                      $"  Sign Type: {SignType?.GetDisplayName() ?? "Unknown/Not Identified"}\n" +
                      $"  Faction: {FactionName ?? "Unknown"} (Known: {FactionKnown})\n" +
                      $"  Net Successes: {NetSuccesses}\n" +
                      $"  Target DC: {TargetDc}\n" +
                      $"  Margin: {Margin:+0;-0;0}\n" +
                      $"  Is Critical: {IsCritical}\n" +
                      $"  Is Misinterpretation: {IsMisinterpretation}\n" +
                      $"  Meaning: {Meaning}\n";

        if (!string.IsNullOrEmpty(AdditionalContext))
        {
            details += $"  Additional Context: {AdditionalContext}\n";
        }

        if (!string.IsNullOrEmpty(RollDetails))
        {
            details += $"  Roll Details: {RollDetails}\n";
        }

        return details;
    }

    /// <summary>
    /// Returns a human-readable summary of the interpretation result.
    /// </summary>
    /// <returns>A formatted string describing the outcome.</returns>
    public override string ToString()
    {
        if (!Interpreted)
        {
            return $"Failed to interpret sign (DC {TargetDc})";
        }

        if (IsMisinterpretation)
        {
            return $"[FUMBLE] Misinterpreted {SignType?.GetDisplayName() ?? "sign"} as {Meaning}";
        }

        var criticalStr = IsCritical ? " [CRITICAL]" : "";
        return $"Interpreted: {SignType?.GetDisplayName() ?? "Unknown"} " +
               $"({NetSuccesses} vs DC {TargetDc}){criticalStr}";
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// The minimum net successes required for a critical success.
    /// </summary>
    public const int CriticalSuccessThreshold = 5;
}
