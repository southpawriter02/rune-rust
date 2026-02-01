// ═══════════════════════════════════════════════════════════════════════════════
// CorruptionSourceDefinitionDto.cs
// DTO representing a single corruption-inducing event from JSON configuration.
// Each definition specifies a unique kebab-case ID, display name, and one of
// three corruption amount types: range-based (min/max), fixed amount, or
// per-HP-spent scaling. Loaded from config/corruption-sources.json for
// data-driven corruption application.
// Version: 0.18.1e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.DTOs;

using System.Text.Json.Serialization;

/// <summary>
/// DTO representing a single corruption-inducing event from configuration.
/// </summary>
/// <remarks>
/// <para>
/// Loaded from <c>config/corruption-sources.json</c> and used to apply consistent
/// corruption amounts across the game. Each corruption source belongs to one of four
/// categories: MysticMagic, HereticalAbility, Environmental, and Items.
/// </para>
/// <para>
/// <strong>Corruption Amount Types (mutually exclusive):</strong>
/// </para>
/// <list type="table">
/// <listheader>
/// <term>Type</term>
/// <description>Properties Used</description>
/// </listheader>
/// <item>
/// <term>Range</term>
/// <description><see cref="MinCorruption"/> and <see cref="MaxCorruption"/> — random corruption between min and max.</description>
/// </item>
/// <item>
/// <term>Fixed</term>
/// <description><see cref="FixedCorruption"/> — exact corruption amount every use.</description>
/// </item>
/// <item>
/// <term>Per-HP</term>
/// <description><see cref="CorruptionPerHp"/> — corruption scales with HP spent (sacrificial abilities).</description>
/// </item>
/// </list>
/// <para>
/// <strong>Configuration Example (range-based):</strong>
/// </para>
/// <code>
/// {
///   "id": "berserker-rage",
///   "name": "Berserker Rage Abilities",
///   "minCorruption": 2,
///   "maxCorruption": 5
/// }
/// </code>
/// <para>
/// <strong>Configuration Example (per-HP):</strong>
/// </para>
/// <code>
/// {
///   "id": "blot-priest-hp-cast",
///   "name": "Sacrificial Casting",
///   "corruptionPerHp": 1
/// }
/// </code>
/// <para>
/// <strong>Validation Constraints (enforced by JSON Schema):</strong>
/// </para>
/// <list type="bullet">
/// <item><description><c>id</c> — kebab-case pattern: <c>^[a-z][a-z0-9-]*$</c></description></item>
/// <item><description><c>name</c> — required display name string</description></item>
/// <item><description><c>minCorruption</c> — integer in range [0, 100]</description></item>
/// <item><description><c>maxCorruption</c> — integer in range [1, 100]</description></item>
/// <item><description><c>fixedCorruption</c> — integer in range [1, 100]</description></item>
/// <item><description><c>corruptionPerHp</c> — integer in range [1, 10]</description></item>
/// <item><description><c>perExposure</c> — optional boolean for environmental sources</description></item>
/// </list>
/// </remarks>
/// <seealso cref="CorruptionConfigurationDto"/>
/// <seealso cref="CorruptionSourceCategoriesDto"/>
public sealed record CorruptionSourceDefinitionDto
{
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique identifier for this corruption source.
    /// </summary>
    /// <remarks>
    /// Must be in kebab-case format (e.g., "berserker-rage", "blight-zone").
    /// Validated by JSON Schema pattern: <c>^[a-z][a-z0-9-]*$</c>.
    /// Used as a lookup key when applying corruption from game events.
    /// </remarks>
    /// <value>A kebab-case string uniquely identifying this corruption source within its category.</value>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// Gets the display name for this corruption source.
    /// </summary>
    /// <remarks>
    /// Used in combat log messages, UI tooltips, and corruption history display
    /// (e.g., "Berserker Rage Abilities", "Blight Zone Exposure").
    /// </remarks>
    /// <value>A human-readable display name string.</value>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Gets the minimum corruption for range-based sources.
    /// </summary>
    /// <remarks>
    /// Used with <see cref="MaxCorruption"/> to define a random corruption range.
    /// A value of 0 means the source may not cause any corruption on some uses
    /// (e.g., standard spells: 0-2 corruption). Valid range: 0-100.
    /// <c>null</c> if this source does not use range-based corruption.
    /// </remarks>
    /// <value>An integer between 0 and 100, or <c>null</c> if not range-based.</value>
    [JsonPropertyName("minCorruption")]
    public int? MinCorruption { get; init; }

    /// <summary>
    /// Gets the maximum corruption for range-based sources.
    /// </summary>
    /// <remarks>
    /// Used with <see cref="MinCorruption"/> to define a random corruption range.
    /// Must be greater than or equal to <see cref="MinCorruption"/>. Valid range: 1-100.
    /// <c>null</c> if this source does not use range-based corruption.
    /// </remarks>
    /// <value>An integer between 1 and 100, or <c>null</c> if not range-based.</value>
    [JsonPropertyName("maxCorruption")]
    public int? MaxCorruption { get; init; }

    /// <summary>
    /// Gets the fixed corruption amount for non-range sources.
    /// </summary>
    /// <remarks>
    /// Applied as an exact corruption amount every use (e.g., Life Siphon: fixed 1 corruption).
    /// Mutually exclusive with <see cref="MinCorruption"/>/<see cref="MaxCorruption"/> range.
    /// Valid range: 1-100. <c>null</c> if this source does not use fixed corruption.
    /// </remarks>
    /// <value>An integer between 1 and 100, or <c>null</c> if not fixed-amount.</value>
    [JsonPropertyName("fixedCorruption")]
    public int? FixedCorruption { get; init; }

    /// <summary>
    /// Gets the corruption gained per HP spent for sacrificial abilities.
    /// </summary>
    /// <remarks>
    /// Used by Blot-Priest sacrificial casting where corruption scales with HP investment.
    /// Total corruption = HP spent * <see cref="CorruptionPerHp"/>. Valid range: 1-10.
    /// <c>null</c> if this source does not use per-HP corruption.
    /// </remarks>
    /// <value>An integer between 1 and 10, or <c>null</c> if not per-HP scaling.</value>
    [JsonPropertyName("corruptionPerHp")]
    public int? CorruptionPerHp { get; init; }

    /// <summary>
    /// Gets whether corruption applies per discrete exposure event.
    /// </summary>
    /// <remarks>
    /// When <c>true</c>, corruption is applied each time the character is exposed to the source
    /// (e.g., each turn in a Blight zone). When <c>false</c> or not specified, corruption is
    /// applied as a one-time cost per use.
    /// </remarks>
    /// <value><c>true</c> for per-exposure environmental sources; <c>false</c> otherwise.</value>
    [JsonPropertyName("perExposure")]
    public bool PerExposure { get; init; }
}
