using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Simplified check outcome for combined skill checks.
/// </summary>
/// <remarks>
/// Wrapper that captures the essential outcomes needed for combined check processing.
/// </remarks>
/// <param name="Success">Whether the check succeeded.</param>
/// <param name="Margin">The margin of success or failure.</param>
/// <param name="IsFumble">Whether the check was a fumble (critical failure).</param>
/// <param name="IsCriticalSuccess">Whether the check was a critical success.</param>
/// <param name="Message">Description of the check result.</param>
public readonly record struct SimpleCheckOutcome(
    bool Success,
    int Margin,
    bool IsFumble,
    bool IsCriticalSuccess,
    string Message)
{
    /// <summary>
    /// Gets whether the check succeeded (alias for Success).
    /// </summary>
    public bool IsSuccess => Success;

    /// <summary>
    /// Creates a successful outcome.
    /// </summary>
    /// <param name="margin">The success margin.</param>
    /// <param name="isCritical">Whether it was a critical success.</param>
    /// <param name="message">Result message.</param>
    /// <returns>A successful outcome.</returns>
    public static SimpleCheckOutcome Succeeded(int margin, bool isCritical = false, string message = "Success") =>
        new(true, margin, false, isCritical, message);

    /// <summary>
    /// Creates a failed outcome.
    /// </summary>
    /// <param name="margin">The failure margin (negative).</param>
    /// <param name="isFumble">Whether it was a fumble.</param>
    /// <param name="message">Result message.</param>
    /// <returns>A failed outcome.</returns>
    public static SimpleCheckOutcome Failed(int margin, bool isFumble = false, string message = "Failed") =>
        new(false, margin, isFumble, false, message);
}

/// <summary>
/// Result of executing a combined skill check.
/// </summary>
/// <remarks>
/// <para>
/// CombinedCheckResult captures the complete outcome of a multi-skill exploration
/// action, including:
/// <list type="bullet">
///   <item><description>The primary skill check result (Wasteland Survival subsystem)</description></item>
///   <item><description>The secondary skill check result (Acrobatics or System Bypass), if executed</description></item>
///   <item><description>Overall success determination (both checks must succeed)</description></item>
///   <item><description>Narrative description for player feedback</description></item>
/// </list>
/// </para>
/// <para>
/// Possible outcome states:
/// <list type="bullet">
///   <item><description><b>Full Success</b>: Both primary and secondary checks succeeded</description></item>
///   <item><description><b>Partial Success</b>: Primary succeeded but secondary failed</description></item>
///   <item><description><b>Primary Failed</b>: Primary check failed, secondary not attempted</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="SynergyType">The synergy type that was executed.</param>
/// <param name="PrimaryResult">Result of the primary skill check.</param>
/// <param name="SecondaryResult">Result of the secondary skill check (null if not executed).</param>
/// <param name="OverallSuccess">Whether both checks succeeded.</param>
/// <param name="NarrativeDescription">Player-facing description of the outcome.</param>
/// <seealso cref="SynergyType"/>
/// <seealso cref="CombinedCheckContext"/>
public readonly record struct CombinedCheckResult(
    SynergyType SynergyType,
    SimpleCheckOutcome PrimaryResult,
    SimpleCheckOutcome? SecondaryResult,
    bool OverallSuccess,
    string NarrativeDescription)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // DERIVED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether the primary check succeeded.
    /// </summary>
    public bool PrimarySucceeded => PrimaryResult.IsSuccess;

    /// <summary>
    /// Gets whether the secondary check was executed.
    /// </summary>
    public bool SecondaryExecuted => SecondaryResult.HasValue;

    /// <summary>
    /// Gets whether the secondary check succeeded (false if not executed).
    /// </summary>
    public bool SecondarySucceeded => SecondaryResult?.IsSuccess ?? false;

    /// <summary>
    /// Gets whether the primary check was a fumble.
    /// </summary>
    public bool PrimaryFumbled => PrimaryResult.IsFumble;

    /// <summary>
    /// Gets whether the secondary check was a fumble.
    /// </summary>
    public bool SecondaryFumbled => SecondaryResult?.IsFumble ?? false;

    /// <summary>
    /// Gets whether the primary check was a critical success.
    /// </summary>
    public bool PrimaryCritical => PrimaryResult.IsCriticalSuccess;

    /// <summary>
    /// Gets whether the secondary check was a critical success.
    /// </summary>
    public bool SecondaryCritical => SecondaryResult?.IsCriticalSuccess ?? false;

    /// <summary>
    /// Gets whether this is a partial success (primary succeeded, overall failed).
    /// </summary>
    public bool IsPartialSuccess => PrimarySucceeded && !OverallSuccess;

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a result for primary failure (no secondary executed).
    /// </summary>
    /// <param name="synergyType">The synergy type that was attempted.</param>
    /// <param name="primaryResult">The failed primary check result.</param>
    /// <param name="narrative">Player-facing description of the failure.</param>
    /// <returns>A combined result indicating primary failure.</returns>
    /// <remarks>
    /// Use this factory when the primary Wasteland Survival check fails,
    /// preventing the secondary check from being attempted.
    /// </remarks>
    public static CombinedCheckResult PrimaryFailed(
        SynergyType synergyType,
        SimpleCheckOutcome primaryResult,
        string narrative) =>
        new(synergyType, primaryResult, null, false, narrative);

    /// <summary>
    /// Creates a result for complete success (both checks passed).
    /// </summary>
    /// <param name="synergyType">The synergy type that was executed.</param>
    /// <param name="primaryResult">The successful primary check result.</param>
    /// <param name="secondaryResult">The successful secondary check result.</param>
    /// <param name="narrative">Player-facing description of the success.</param>
    /// <returns>A combined result indicating full success.</returns>
    /// <remarks>
    /// Use this factory when both the primary Wasteland Survival check and
    /// the secondary skill check succeed.
    /// </remarks>
    public static CombinedCheckResult FullSuccess(
        SynergyType synergyType,
        SimpleCheckOutcome primaryResult,
        SimpleCheckOutcome secondaryResult,
        string narrative) =>
        new(synergyType, primaryResult, secondaryResult, true, narrative);

    /// <summary>
    /// Creates a result for partial success (primary passed, secondary failed).
    /// </summary>
    /// <param name="synergyType">The synergy type that was executed.</param>
    /// <param name="primaryResult">The successful primary check result.</param>
    /// <param name="secondaryResult">The failed secondary check result.</param>
    /// <param name="narrative">Player-facing description of the partial success.</param>
    /// <returns>A combined result indicating partial success.</returns>
    /// <remarks>
    /// Use this factory when the primary Wasteland Survival check succeeds
    /// but the secondary skill check fails. The player has discovered the
    /// opportunity but couldn't exploit it fully.
    /// </remarks>
    public static CombinedCheckResult PartialSuccess(
        SynergyType synergyType,
        SimpleCheckOutcome primaryResult,
        SimpleCheckOutcome secondaryResult,
        string narrative) =>
        new(synergyType, primaryResult, secondaryResult, false, narrative);

    // ═══════════════════════════════════════════════════════════════════════════
    // STRING FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a display string for this result.
    /// </summary>
    /// <returns>A formatted string showing the combined check outcome.</returns>
    public string ToDisplayString()
    {
        var status = OverallSuccess ? "SUCCESS" :
                     PrimarySucceeded ? "PARTIAL" : "FAILED";

        return $"[{status}] {SynergyType}: {NarrativeDescription}";
    }

    /// <summary>
    /// Returns a detailed string representation of this result.
    /// </summary>
    /// <returns>A string with full check details.</returns>
    public override string ToString()
    {
        var secondary = SecondaryExecuted
            ? $", Secondary: {(SecondarySucceeded ? "Success" : "Failed")}"
            : "";

        return $"CombinedCheckResult {{ Synergy: {SynergyType}, " +
               $"Primary: {(PrimarySucceeded ? "Success" : "Failed")}{secondary}, " +
               $"Overall: {(OverallSuccess ? "Success" : "Failed")} }}";
    }
}
