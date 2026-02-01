// ═══════════════════════════════════════════════════════════════════════════════
// StressRecoveryConfigDto.cs
// DTOs for deserializing recovery rate configuration from stress-sources.json.
// Contains StressRecoveryConfigDto (four rest type formulas) and RecoveryRateDto
// (individual formula string). Supports WILL multipliers, FULL_RESET, and fixed
// integer recovery formulas.
// Version: 0.18.0e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.DTOs;

using System.Text.Json.Serialization;

/// <summary>
/// DTO representing recovery rate configuration for all rest types.
/// </summary>
/// <remarks>
/// <para>
/// Loaded from the <c>recoveryRates</c> section of <c>config/stress-sources.json</c>.
/// Defines the formula used to calculate stress recovery for each of the four
/// <c>RestType</c> values: Short Rest, Long Rest, Sanctuary, and Milestone.
/// </para>
/// <para>
/// <strong>Configuration Example:</strong>
/// </para>
/// <code>
/// "recoveryRates": {
///   "shortRest": { "formula": "WILL × 2" },
///   "longRest": { "formula": "WILL × 5" },
///   "sanctuary": { "formula": "FULL_RESET" },
///   "milestone": { "formula": "25" }
/// }
/// </code>
/// <para>
/// <strong>Recovery Formulas:</strong>
/// </para>
/// <list type="table">
/// <listheader>
/// <term>Rest Type</term>
/// <description>Formula / Example (WILL=4)</description>
/// </listheader>
/// <item><term>Short Rest</term><description>WILL × 2 → 8 stress recovered</description></item>
/// <item><term>Long Rest</term><description>WILL × 5 → 20 stress recovered</description></item>
/// <item><term>Sanctuary</term><description>FULL_RESET → all stress removed</description></item>
/// <item><term>Milestone</term><description>25 → fixed 25 stress recovered</description></item>
/// </list>
/// </remarks>
/// <seealso cref="RecoveryRateDto"/>
/// <seealso cref="StressConfigurationDto"/>
public sealed record StressRecoveryConfigDto
{
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the recovery configuration for Short Rest.
    /// </summary>
    /// <remarks>
    /// Default formula: <c>WILL × 2</c>.
    /// Quick tactical recovery during exploration pauses.
    /// </remarks>
    /// <value>Recovery rate configuration for Short Rest.</value>
    [JsonPropertyName("shortRest")]
    public required RecoveryRateDto ShortRest { get; init; }

    /// <summary>
    /// Gets the recovery configuration for Long Rest.
    /// </summary>
    /// <remarks>
    /// Default formula: <c>WILL × 5</c>.
    /// Significant but not full recovery during extended rest periods.
    /// </remarks>
    /// <value>Recovery rate configuration for Long Rest.</value>
    [JsonPropertyName("longRest")]
    public required RecoveryRateDto LongRest { get; init; }

    /// <summary>
    /// Gets the recovery configuration for Sanctuary Rest.
    /// </summary>
    /// <remarks>
    /// Default formula: <c>FULL_RESET</c>.
    /// Complete stress removal at a safe haven. Sanctuary rest is the only
    /// guaranteed way to fully clear psychic stress.
    /// </remarks>
    /// <value>Recovery rate configuration for Sanctuary Rest.</value>
    [JsonPropertyName("sanctuary")]
    public required RecoveryRateDto Sanctuary { get; init; }

    /// <summary>
    /// Gets the recovery configuration for Milestone Recovery.
    /// </summary>
    /// <remarks>
    /// Default formula: <c>25</c> (fixed amount).
    /// Achievement-based reward that does not scale with WILL attribute.
    /// </remarks>
    /// <value>Recovery rate configuration for Milestone Recovery.</value>
    [JsonPropertyName("milestone")]
    public required RecoveryRateDto Milestone { get; init; }
}

/// <summary>
/// DTO representing a single recovery rate formula.
/// </summary>
/// <remarks>
/// <para>
/// Each recovery rate is defined by a formula string that describes how
/// stress recovery is calculated. The formula is stored as a string and
/// interpreted by the <c>StressService</c> at runtime.
/// </para>
/// <para>
/// <strong>Supported Formula Formats:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><c>"WILL × N"</c> — WILL attribute multiplied by N (e.g., "WILL × 2" for Short Rest)</description></item>
/// <item><description><c>"FULL_RESET"</c> — Reset stress to 0 (used for Sanctuary rest)</description></item>
/// <item><description><c>"N"</c> — Fixed integer amount as string (e.g., "25" for Milestone recovery)</description></item>
/// </list>
/// </remarks>
/// <seealso cref="StressRecoveryConfigDto"/>
public sealed record RecoveryRateDto
{
    /// <summary>
    /// Gets the recovery formula string.
    /// </summary>
    /// <remarks>
    /// Describes the calculation for stress recovery. See class-level remarks
    /// for the list of supported formula formats.
    /// </remarks>
    /// <value>A formula string such as "WILL × 2", "FULL_RESET", or "25".</value>
    [JsonPropertyName("formula")]
    public required string Formula { get; init; }
}
