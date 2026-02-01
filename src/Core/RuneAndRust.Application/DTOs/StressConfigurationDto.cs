// ═══════════════════════════════════════════════════════════════════════════════
// StressConfigurationDto.cs
// Root DTO for deserializing the complete stress configuration from
// config/stress-sources.json. Contains the top-level StressConfigurationDto,
// StressSourcesDto (six category arrays), and TraumaResetConfigDto
// (passed/failed reset values). Validated by stress-sources.schema.json.
// Version: 0.18.0e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.DTOs;

using System.Text.Json.Serialization;

/// <summary>
/// Root DTO for the complete stress configuration.
/// </summary>
/// <remarks>
/// <para>
/// Loaded from <c>config/stress-sources.json</c> and validated against
/// <c>config/schemas/stress-sources.schema.json</c> (JSON Schema Draft-07).
/// </para>
/// <para>
/// <strong>Top-Level Structure:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><see cref="Version"/> — Configuration version in MAJOR.MINOR format</description></item>
/// <item><description><see cref="StressSources"/> — 6 category arrays of <see cref="StressSourceDefinitionDto"/></description></item>
/// <item><description><see cref="RecoveryRates"/> — 4 rest type formulas via <see cref="StressRecoveryConfigDto"/></description></item>
/// <item><description><see cref="TraumaCheckReset"/> — Passed/failed reset values via <see cref="TraumaResetConfigDto"/></description></item>
/// </list>
/// <para>
/// <strong>Configuration Example:</strong>
/// </para>
/// <code>
/// {
///   "$schema": "./schemas/stress-sources.schema.json",
///   "version": "1.0",
///   "stressSources": {
///     "combat": [ { "id": "enemy-fear-aura", "baseStress": 15, "resistDc": 2 } ],
///     "exploration": [ ... ],
///     "narrative": [ ... ],
///     "heretical": [ ... ],
///     "environmental": [ ... ],
///     "corruption": [ ... ]
///   },
///   "recoveryRates": {
///     "shortRest": { "formula": "WILL × 2" },
///     "longRest": { "formula": "WILL × 5" },
///     "sanctuary": { "formula": "FULL_RESET" },
///     "milestone": { "formula": "25" }
///   },
///   "traumaCheckReset": { "passed": 75, "failed": 50 }
/// }
/// </code>
/// </remarks>
/// <seealso cref="StressSourcesDto"/>
/// <seealso cref="StressRecoveryConfigDto"/>
/// <seealso cref="TraumaResetConfigDto"/>
/// <seealso cref="StressSourceDefinitionDto"/>
public sealed record StressConfigurationDto
{
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the configuration version.
    /// </summary>
    /// <remarks>
    /// Must follow MAJOR.MINOR format (e.g., "1.0").
    /// Validated by JSON Schema pattern: <c>^\d+\.\d+$</c>.
    /// </remarks>
    /// <value>A version string in MAJOR.MINOR format.</value>
    [JsonPropertyName("version")]
    public required string Version { get; init; }

    /// <summary>
    /// Gets the stress sources organized by category.
    /// </summary>
    /// <remarks>
    /// Contains six arrays matching the <c>StressSource</c> enum values:
    /// Combat, Exploration, Narrative, Heretical, Environmental, and Corruption.
    /// Each array holds <see cref="StressSourceDefinitionDto"/> entries defining
    /// individual stress-inducing events.
    /// </remarks>
    /// <value>Categorized stress source definitions.</value>
    [JsonPropertyName("stressSources")]
    public required StressSourcesDto StressSources { get; init; }

    /// <summary>
    /// Gets the recovery rate configuration.
    /// </summary>
    /// <remarks>
    /// Defines formulas for stress recovery across all four <c>RestType</c> values:
    /// Short Rest (WILL × 2), Long Rest (WILL × 5), Sanctuary (FULL_RESET),
    /// and Milestone (fixed 25).
    /// </remarks>
    /// <value>Recovery rate formulas for each rest type.</value>
    [JsonPropertyName("recoveryRates")]
    public required StressRecoveryConfigDto RecoveryRates { get; init; }

    /// <summary>
    /// Gets the trauma check reset configuration.
    /// </summary>
    /// <remarks>
    /// Defines the stress value a character is reset to after a Trauma Check
    /// at 100 stress. Default: passed = 75, failed = 50.
    /// </remarks>
    /// <value>Trauma check reset values.</value>
    [JsonPropertyName("traumaCheckReset")]
    public required TraumaResetConfigDto TraumaCheckReset { get; init; }
}

/// <summary>
/// DTO for stress sources organized by category.
/// </summary>
/// <remarks>
/// <para>
/// Each property corresponds to one of the six <c>StressSource</c> enum values
/// defined in v0.18.0a. Categories that are not present in the configuration
/// file default to empty arrays.
/// </para>
/// <para>
/// <strong>Categories:</strong>
/// </para>
/// <list type="table">
/// <listheader>
/// <term>Category</term>
/// <description>Description</description>
/// </listheader>
/// <item><term>Combat</term><description>Fear auras, overwhelming odds, ally casualties, near-death</description></item>
/// <item><term>Exploration</term><description>Darkness, strange sounds, corpse discovery, impossible geometry</description></item>
/// <item><term>Narrative</term><description>Betrayal, loss, moral dilemmas, dark secrets</description></item>
/// <item><term>Heretical</term><description>Forbidden knowledge, eldritch symbols, void whispers, reality tears</description></item>
/// <item><term>Environmental</term><description>Extreme weather, toxic air, unstable ground, storms</description></item>
/// <item><term>Corruption</term><description>Blight exposure, corruption touch, mutations, resonance</description></item>
/// </list>
/// </remarks>
/// <seealso cref="StressSourceDefinitionDto"/>
/// <seealso cref="StressConfigurationDto"/>
public sealed record StressSourcesDto
{
    /// <summary>Combat-related stress sources.</summary>
    /// <value>Array of combat stress source definitions, or empty if none configured.</value>
    [JsonPropertyName("combat")]
    public StressSourceDefinitionDto[] Combat { get; init; } = [];

    /// <summary>Exploration-related stress sources.</summary>
    /// <value>Array of exploration stress source definitions, or empty if none configured.</value>
    [JsonPropertyName("exploration")]
    public StressSourceDefinitionDto[] Exploration { get; init; } = [];

    /// <summary>Narrative/story-related stress sources.</summary>
    /// <value>Array of narrative stress source definitions, or empty if none configured.</value>
    [JsonPropertyName("narrative")]
    public StressSourceDefinitionDto[] Narrative { get; init; } = [];

    /// <summary>Heretical/forbidden knowledge stress sources.</summary>
    /// <value>Array of heretical stress source definitions, or empty if none configured.</value>
    [JsonPropertyName("heretical")]
    public StressSourceDefinitionDto[] Heretical { get; init; } = [];

    /// <summary>Environmental hazard stress sources.</summary>
    /// <value>Array of environmental stress source definitions, or empty if none configured.</value>
    [JsonPropertyName("environmental")]
    public StressSourceDefinitionDto[] Environmental { get; init; } = [];

    /// <summary>Corruption/Blight stress sources.</summary>
    /// <value>Array of corruption stress source definitions, or empty if none configured.</value>
    [JsonPropertyName("corruption")]
    public StressSourceDefinitionDto[] Corruption { get; init; } = [];
}

/// <summary>
/// DTO for trauma check reset values.
/// </summary>
/// <remarks>
/// <para>
/// When a character's stress reaches 100, a Trauma Check is triggered.
/// After resolution, the character's stress is reset to one of these values:
/// </para>
/// <list type="table">
/// <listheader>
/// <term>Outcome</term>
/// <description>Reset Value</description>
/// </listheader>
/// <item><term>Passed</term><description>75 — character narrowly avoids permanent trauma but remains highly stressed</description></item>
/// <item><term>Failed</term><description>50 — character gains a permanent trauma but stress drops further as a concession</description></item>
/// </list>
/// <para>
/// Both values must be integers in the range [0, 100], validated by JSON Schema.
/// </para>
/// </remarks>
/// <seealso cref="StressConfigurationDto"/>
public sealed record TraumaResetConfigDto
{
    /// <summary>
    /// Gets the stress value after passing a Trauma Check.
    /// </summary>
    /// <remarks>
    /// Default: 75. Character avoids permanent trauma but remains in the
    /// Breaking threshold (80-99 → Defense -4, skill disadvantage) or
    /// Panicked threshold (60-79 → Defense -3).
    /// </remarks>
    /// <value>An integer between 0 and 100.</value>
    [JsonPropertyName("passed")]
    public required int Passed { get; init; }

    /// <summary>
    /// Gets the stress value after failing a Trauma Check.
    /// </summary>
    /// <remarks>
    /// Default: 50. Character gains a permanent trauma but stress drops to
    /// the Anxious threshold (40-59 → Defense -2), providing some relief
    /// as a counterbalance to the permanent consequence.
    /// </remarks>
    /// <value>An integer between 0 and 100.</value>
    [JsonPropertyName("failed")]
    public required int Failed { get; init; }
}
