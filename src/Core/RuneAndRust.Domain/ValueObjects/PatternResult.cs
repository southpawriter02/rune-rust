// ------------------------------------------------------------------------------
// <copyright file="PatternResult.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// The result of the Pattern Recognition step in the jury-rigging procedure.
// Part of v0.15.4e Jury-Rigging System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// The result of the Pattern Recognition step in the jury-rigging procedure.
/// </summary>
/// <remarks>
/// <para>
/// The Pattern Recognition step is an optional WITS DC 12 check that attempts
/// to recognize the mechanism type from past experience:
/// <list type="bullet">
///   <item><description>Success + Familiar: +2d10 bonus dice on experiment</description></item>
///   <item><description>Success + Unfamiliar: Mark mechanism type as "seen" for future</description></item>
///   <item><description>Failure: No recognition, may proceed without bonus</description></item>
///   <item><description>Skipped: Character proceeds without attempting recognition</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="Success">Whether pattern recognition succeeded.</param>
/// <param name="WasSkipped">Whether the character skipped this step.</param>
/// <param name="NetSuccesses">The net successes from the WITS check.</param>
/// <param name="EffectiveDc">The DC for pattern recognition (typically 12).</param>
/// <param name="IsFamiliar">Whether the character is familiar with this mechanism type.</param>
/// <param name="MechanismType">The mechanism type that was recognized.</param>
/// <param name="BonusDice">The number of bonus dice gained (0 or 2).</param>
/// <param name="NarrativeText">Flavor text describing the result.</param>
public readonly record struct PatternResult(
    bool Success,
    bool WasSkipped,
    int NetSuccesses,
    int EffectiveDc,
    bool IsFamiliar,
    string? MechanismType,
    int BonusDice,
    string NarrativeText)
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    /// <summary>
    /// The standard DC for pattern recognition attempts.
    /// </summary>
    public const int StandardDc = 12;

    /// <summary>
    /// The number of bonus dice granted when familiar with mechanism type.
    /// </summary>
    public const int FamiliarityBonusDice = 2;

    // -------------------------------------------------------------------------
    // Computed Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets a value indicating whether the step was attempted.
    /// </summary>
    public bool WasAttempted => !WasSkipped;

    /// <summary>
    /// Gets a value indicating whether bonus dice were earned.
    /// </summary>
    /// <remarks>
    /// Bonus dice are only earned when:
    /// 1. Pattern recognition succeeded, AND
    /// 2. The character is familiar with this mechanism type
    /// </remarks>
    public bool HasBonusDice => BonusDice > 0;

    /// <summary>
    /// Gets a value indicating whether the mechanism type was recognized.
    /// </summary>
    /// <remarks>
    /// Recognition occurs on successful pattern matching, regardless
    /// of whether the character was previously familiar with the type.
    /// </remarks>
    public bool TypeRecognized => Success && !string.IsNullOrEmpty(MechanismType);

    /// <summary>
    /// Gets a value indicating whether this is a new mechanism type for the character.
    /// </summary>
    /// <remarks>
    /// If the character recognizes a type they haven't seen before,
    /// it should be marked as "seen" for future encounters.
    /// </remarks>
    public bool IsNewType => Success && !IsFamiliar;

    /// <summary>
    /// Gets a value indicating whether Memorized Sequence is now available.
    /// </summary>
    /// <remarks>
    /// Memorized Sequence (-2 DC) requires familiarity with the mechanism type.
    /// This is true when the character was already familiar OR this isn't
    /// their first successful bypass of this type.
    /// </remarks>
    public bool MemorizedSequenceAvailable => IsFamiliar;

    // -------------------------------------------------------------------------
    // Factory Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a successful pattern recognition result with familiarity.
    /// </summary>
    /// <param name="netSuccesses">The net successes from the WITS check.</param>
    /// <param name="mechanismType">The recognized mechanism type.</param>
    /// <param name="narrative">Optional narrative text.</param>
    /// <returns>A PatternResult for successful recognition with familiarity bonus.</returns>
    /// <remarks>
    /// When familiar with a mechanism type, the character gains +2d10 bonus dice
    /// on the experiment roll from muscle memory and past experience.
    /// </remarks>
    public static PatternResult SuccessWithFamiliarity(
        int netSuccesses,
        string mechanismType,
        string? narrative = null)
    {
        return new PatternResult(
            Success: true,
            WasSkipped: false,
            NetSuccesses: netSuccesses,
            EffectiveDc: StandardDc,
            IsFamiliar: true,
            MechanismType: mechanismType,
            BonusDice: FamiliarityBonusDice,
            NarrativeText: narrative ?? $"You recognize this {mechanismType}—you've bypassed " +
                                        $"its kind before. Muscle memory guides your hands. [+{FamiliarityBonusDice}d10]");
    }

    /// <summary>
    /// Creates a successful pattern recognition result without prior familiarity.
    /// </summary>
    /// <param name="netSuccesses">The net successes from the WITS check.</param>
    /// <param name="mechanismType">The recognized mechanism type.</param>
    /// <param name="narrative">Optional narrative text.</param>
    /// <returns>A PatternResult for successful recognition of a new type.</returns>
    /// <remarks>
    /// When recognizing a new type, no bonus dice are gained, but the type
    /// is marked as "seen" for future encounters. Successfully bypassing
    /// this mechanism will grant familiarity for next time.
    /// </remarks>
    public static PatternResult SuccessNewType(
        int netSuccesses,
        string mechanismType,
        string? narrative = null)
    {
        return new PatternResult(
            Success: true,
            WasSkipped: false,
            NetSuccesses: netSuccesses,
            EffectiveDc: StandardDc,
            IsFamiliar: false,
            MechanismType: mechanismType,
            BonusDice: 0,
            NarrativeText: narrative ?? $"You recognize this as a {mechanismType}, though you've " +
                                        "never worked on one before. Learn from this, and next time " +
                                        "will be easier.");
    }

    /// <summary>
    /// Creates a failed pattern recognition result.
    /// </summary>
    /// <param name="netSuccesses">The net successes from the WITS check.</param>
    /// <param name="narrative">Optional narrative text.</param>
    /// <returns>A PatternResult for failed recognition.</returns>
    /// <remarks>
    /// Failed recognition means the character cannot identify the mechanism
    /// type, denying both familiarity bonus and type marking.
    /// </remarks>
    public static PatternResult Failure(
        int netSuccesses,
        string? narrative = null)
    {
        return new PatternResult(
            Success: false,
            WasSkipped: false,
            NetSuccesses: netSuccesses,
            EffectiveDc: StandardDc,
            IsFamiliar: false,
            MechanismType: null,
            BonusDice: 0,
            NarrativeText: narrative ?? "You rack your brain, but this mechanism's design is " +
                                        "foreign to your experience. You'll have to proceed " +
                                        "without the advantage of recognition.");
    }

    /// <summary>
    /// Creates a skipped pattern recognition result.
    /// </summary>
    /// <param name="narrative">Optional narrative text.</param>
    /// <returns>A PatternResult for skipped recognition.</returns>
    /// <remarks>
    /// Skipping pattern recognition saves time but forfeits any potential
    /// familiarity bonus and the chance to learn a new mechanism type.
    /// </remarks>
    public static PatternResult Skipped(string? narrative = null)
    {
        return new PatternResult(
            Success: false,
            WasSkipped: true,
            NetSuccesses: 0,
            EffectiveDc: 0,
            IsFamiliar: false,
            MechanismType: null,
            BonusDice: 0,
            NarrativeText: narrative ?? "No time for reminiscing—you dive straight into " +
                                        "the mechanism's guts.");
    }

    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns a formatted display string for the result.
    /// </summary>
    /// <returns>A human-readable summary of the pattern recognition result.</returns>
    public string ToDisplayString()
    {
        if (WasSkipped)
        {
            return "Pattern Recognition: SKIPPED";
        }

        var statusStr = Success ? "SUCCESS" : "FAILURE";
        var typeStr = TypeRecognized ? $" ({MechanismType})" : "";
        var bonusStr = HasBonusDice ? $" [+{BonusDice}d10 bonus]" : "";
        var newTypeStr = IsNewType ? " [NEW TYPE]" : "";

        return $"Pattern Recognition: {statusStr} " +
               $"(Roll {NetSuccesses} vs DC {EffectiveDc}){typeStr}{bonusStr}{newTypeStr}";
    }

    /// <summary>
    /// Returns a compact string for logging purposes.
    /// </summary>
    /// <returns>A log-friendly string representation.</returns>
    public string ToLogString()
    {
        return $"PatternResult[Success={Success} Skipped={WasSkipped} " +
               $"Type={MechanismType ?? "unknown"} Familiar={IsFamiliar} Bonus={BonusDice}]";
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return ToDisplayString();
    }
}
