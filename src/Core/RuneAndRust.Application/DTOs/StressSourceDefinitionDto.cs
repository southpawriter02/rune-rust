// ═══════════════════════════════════════════════════════════════════════════════
// StressSourceDefinitionDto.cs
// DTO representing a single stress-inducing event from JSON configuration.
// Each definition specifies a unique kebab-case ID, base stress amount (1-100),
// WILL resistance DC (0-10), and optional narrative description.
// Loaded from config/stress-sources.json for data-driven stress application.
// Version: 0.18.0e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.DTOs;

using System.Text.Json.Serialization;

/// <summary>
/// DTO representing a single stress-inducing event from configuration.
/// </summary>
/// <remarks>
/// <para>
/// Loaded from <c>config/stress-sources.json</c> and used to apply consistent
/// stress amounts across the game. Each stress source belongs to one of six
/// categories matching the <c>StressSource</c> enum: Combat, Exploration,
/// Narrative, Heretical, Environmental, and Corruption.
/// </para>
/// <para>
/// <strong>Configuration Example:</strong>
/// </para>
/// <code>
/// {
///   "id": "enemy-fear-aura",
///   "baseStress": 15,
///   "resistDc": 2,
///   "description": "An enemy's terrifying presence radiates fear."
/// }
/// </code>
/// <para>
/// <strong>Validation Constraints (enforced by JSON Schema):</strong>
/// </para>
/// <list type="bullet">
/// <item><description><c>id</c> — kebab-case pattern: <c>^[a-z][a-z0-9-]*$</c></description></item>
/// <item><description><c>baseStress</c> — integer in range [1, 100]</description></item>
/// <item><description><c>resistDc</c> — integer in range [0, 10]; 0 = no resistance allowed</description></item>
/// <item><description><c>description</c> — optional narrative text</description></item>
/// </list>
/// </remarks>
/// <seealso cref="StressConfigurationDto"/>
/// <seealso cref="StressSourcesDto"/>
public sealed record StressSourceDefinitionDto
{
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique identifier for this stress source.
    /// </summary>
    /// <remarks>
    /// Must be in kebab-case format (e.g., "enemy-fear-aura", "corpse-discovery").
    /// Validated by JSON Schema pattern: <c>^[a-z][a-z0-9-]*$</c>.
    /// Used as a lookup key when applying stress from game events.
    /// </remarks>
    /// <value>A kebab-case string uniquely identifying this stress source within its category.</value>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// Gets the base stress amount before any resistance.
    /// </summary>
    /// <remarks>
    /// Valid range: 1-100. Higher values indicate more severe stressors.
    /// This is the raw stress applied before WILL-based resistance checks
    /// reduce it via the reduction table (0%/50%/75%/100%).
    /// <para>
    /// <strong>Typical ranges by severity:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Mild (5-10): Strange sounds, extreme weather</description></item>
    /// <item><description>Moderate (15-20): Fear auras, corpse discovery, eldritch symbols</description></item>
    /// <item><description>Severe (25-30): Near-death, forbidden knowledge, corruption touch</description></item>
    /// <item><description>Extreme (35-40): Reality tears, loved one death</description></item>
    /// </list>
    /// </remarks>
    /// <value>An integer between 1 and 100 representing raw stress before resistance.</value>
    [JsonPropertyName("baseStress")]
    public required int BaseStress { get; init; }

    /// <summary>
    /// Gets the difficulty class for WILL-based resistance checks.
    /// </summary>
    /// <remarks>
    /// Valid range: 0-10. A value of 0 means no resistance check is allowed —
    /// the full <see cref="BaseStress"/> is applied without any opportunity to resist.
    /// Higher DC values require more WILL successes to reduce the stress.
    /// <para>
    /// When <c>resistDc &gt; 0</c>, the character rolls a WILL d10 dice pool.
    /// Net successes map to the reduction table:
    /// </para>
    /// <list type="table">
    /// <listheader>
    /// <term>Successes</term>
    /// <description>Reduction</description>
    /// </listheader>
    /// <item><term>0</term><description>0% (full stress)</description></item>
    /// <item><term>1</term><description>50% (half stress)</description></item>
    /// <item><term>2-3</term><description>75% (quarter stress)</description></item>
    /// <item><term>4+</term><description>100% (no stress)</description></item>
    /// </list>
    /// </remarks>
    /// <value>An integer between 0 and 10 representing the resistance check difficulty.</value>
    [JsonPropertyName("resistDc")]
    public required int ResistDc { get; init; }

    /// <summary>
    /// Gets the optional narrative description of this stress source.
    /// </summary>
    /// <remarks>
    /// Used for combat log messages, UI tooltips, and narrative context when
    /// stress is applied. May be <see langword="null"/> if no description is provided.
    /// </remarks>
    /// <value>A narrative description string, or <see langword="null"/> if not specified.</value>
    [JsonPropertyName("description")]
    public string? Description { get; init; }
}
