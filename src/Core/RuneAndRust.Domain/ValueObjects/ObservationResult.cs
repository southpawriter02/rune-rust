// ------------------------------------------------------------------------------
// <copyright file="ObservationResult.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// The result of the Observe step in the jury-rigging procedure.
// Part of v0.15.4e Jury-Rigging System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// The result of the Observe step in the jury-rigging procedure.
/// </summary>
/// <remarks>
/// <para>
/// The Observe step is an optional WITS DC 10 check that allows the character
/// to study the mechanism visually before attempting to bypass it.
/// <list type="bullet">
///   <item><description>Success: Learn mechanism type and reveal potential bypass methods</description></item>
///   <item><description>Failure: No information gained, may still proceed</description></item>
///   <item><description>Skipped: Character proceeds blind without observation</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="Success">Whether the observation succeeded.</param>
/// <param name="WasSkipped">Whether the character skipped the observation step.</param>
/// <param name="NetSuccesses">The net successes from the WITS check.</param>
/// <param name="EffectiveDc">The DC for the observation (typically 10).</param>
/// <param name="MechanismType">The identified mechanism type, if successful.</param>
/// <param name="Hints">Hints about the mechanism revealed by observation.</param>
/// <param name="NarrativeText">Flavor text describing the observation result.</param>
public readonly record struct ObservationResult(
    bool Success,
    bool WasSkipped,
    int NetSuccesses,
    int EffectiveDc,
    string? MechanismType,
    IReadOnlyList<string> Hints,
    string NarrativeText)
{
    // -------------------------------------------------------------------------
    // Computed Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets a value indicating whether the mechanism type was identified.
    /// </summary>
    /// <remarks>
    /// Mechanism type is identified on successful observation.
    /// Knowing the type enables familiarity bonuses if the character has
    /// previously bypassed this type.
    /// </remarks>
    public bool TypeIdentified => !string.IsNullOrEmpty(MechanismType);

    /// <summary>
    /// Gets a value indicating whether any hints were revealed.
    /// </summary>
    /// <remarks>
    /// Hints provide information about available bypass methods,
    /// risks, and the mechanism's current state.
    /// </remarks>
    public bool HasHints => Hints.Count > 0;

    /// <summary>
    /// Gets a value indicating whether the step was attempted.
    /// </summary>
    /// <remarks>
    /// Returns false if the character chose to skip observation.
    /// </remarks>
    public bool WasAttempted => !WasSkipped;

    // -------------------------------------------------------------------------
    // Factory Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a successful observation result.
    /// </summary>
    /// <param name="netSuccesses">The net successes from the WITS check.</param>
    /// <param name="dc">The observation DC (typically 10).</param>
    /// <param name="mechanismType">The identified mechanism type.</param>
    /// <param name="hints">Hints revealed about the mechanism.</param>
    /// <param name="narrative">Optional narrative text.</param>
    /// <returns>An ObservationResult for successful observation.</returns>
    /// <remarks>
    /// On successful observation, the character identifies the mechanism type
    /// and gains hints about potential bypass approaches.
    /// </remarks>
    public static ObservationResult Succeeded(
        int netSuccesses,
        int dc,
        string mechanismType,
        IReadOnlyList<string> hints,
        string? narrative = null)
    {
        return new ObservationResult(
            Success: true,
            WasSkipped: false,
            NetSuccesses: netSuccesses,
            EffectiveDc: dc,
            MechanismType: mechanismType,
            Hints: hints,
            NarrativeText: narrative ?? $"You recognize this as a {mechanismType}. " +
                                        "Its design reveals potential approaches.");
    }

    /// <summary>
    /// Creates a failed observation result.
    /// </summary>
    /// <param name="netSuccesses">The net successes from the WITS check.</param>
    /// <param name="dc">The observation DC (typically 10).</param>
    /// <param name="narrative">Optional narrative text.</param>
    /// <returns>An ObservationResult for failed observation.</returns>
    /// <remarks>
    /// On failed observation, the mechanism remains mysterious.
    /// The character may still attempt to bypass it, but without the
    /// benefit of knowing its type.
    /// </remarks>
    public static ObservationResult Failure(
        int netSuccesses,
        int dc,
        string? narrative = null)
    {
        return new ObservationResult(
            Success: false,
            WasSkipped: false,
            NetSuccesses: netSuccesses,
            EffectiveDc: dc,
            MechanismType: null,
            Hints: Array.Empty<string>(),
            NarrativeText: narrative ?? "The mechanism's purpose eludes you. " +
                                        "Its Old World design is unfamiliar.");
    }

    /// <summary>
    /// Creates a skipped observation result.
    /// </summary>
    /// <param name="narrative">Optional narrative text.</param>
    /// <returns>An ObservationResult for skipped observation.</returns>
    /// <remarks>
    /// When observation is skipped, the character proceeds directly to
    /// the Probe step without attempting to identify the mechanism.
    /// </remarks>
    public static ObservationResult Skipped(string? narrative = null)
    {
        return new ObservationResult(
            Success: false,
            WasSkipped: true,
            NetSuccesses: 0,
            EffectiveDc: 0,
            MechanismType: null,
            Hints: Array.Empty<string>(),
            NarrativeText: narrative ?? "You decide to proceed without careful observation, " +
                                        "trusting your instincts over study.");
    }

    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns a formatted display string for the result.
    /// </summary>
    /// <returns>A human-readable summary of the observation result.</returns>
    public string ToDisplayString()
    {
        if (WasSkipped)
        {
            return "Observation: SKIPPED";
        }

        var statusStr = Success ? "SUCCESS" : "FAILURE";
        var typeStr = TypeIdentified ? $" ({MechanismType})" : "";
        var hintCount = HasHints ? $" [{Hints.Count} hints]" : "";

        return $"Observation: {statusStr} (Roll {NetSuccesses} vs DC {EffectiveDc}){typeStr}{hintCount}";
    }

    /// <summary>
    /// Returns a compact string for logging purposes.
    /// </summary>
    /// <returns>A log-friendly string representation.</returns>
    public string ToLogString()
    {
        return $"ObservationResult[Success={Success} Skipped={WasSkipped} " +
               $"Type={MechanismType ?? "unknown"} Hints={Hints.Count}]";
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return ToDisplayString();
    }
}
