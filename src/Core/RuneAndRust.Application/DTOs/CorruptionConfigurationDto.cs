// ═══════════════════════════════════════════════════════════════════════════════
// CorruptionConfigurationDto.cs
// Root DTO for deserializing the complete corruption configuration from
// config/corruption-sources.json. Contains the top-level CorruptionConfigurationDto,
// CorruptionSourceCategoriesDto (four category arrays), PenaltyFormulasDto,
// and FormulaDto. Validated by corruption-sources.schema.json.
// Version: 0.18.1e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.DTOs;

using System.Text.Json.Serialization;

/// <summary>
/// Root DTO for the complete corruption configuration.
/// </summary>
/// <remarks>
/// <para>
/// Loaded from <c>config/corruption-sources.json</c> and validated against
/// <c>config/schemas/corruption-sources.schema.json</c> (JSON Schema Draft-07).
/// </para>
/// <para>
/// <strong>Top-Level Structure:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><see cref="Version"/> — Configuration version in MAJOR.MINOR format</description></item>
/// <item><description><see cref="CorruptionSources"/> — 4 category arrays of <see cref="CorruptionSourceDefinitionDto"/></description></item>
/// <item><description><see cref="ThresholdEffects"/> — Effects triggered at 25/50/75/100 corruption thresholds</description></item>
/// <item><description><see cref="Penalties"/> — Penalty calculation formulas for HP/AP/Resolve</description></item>
/// </list>
/// <para>
/// <strong>Configuration Example:</strong>
/// </para>
/// <code>
/// {
///   "$schema": "schemas/corruption-sources.schema.json",
///   "version": "1.0",
///   "corruptionSources": {
///     "mysticMagic": [ { "id": "standard-spell", "name": "Standard Spell", "minCorruption": 0, "maxCorruption": 2 } ],
///     "hereticalAbility": [ ... ],
///     "environmental": [ ... ],
///     "items": [ ... ]
///   },
///   "thresholdEffects": {
///     "25": { "description": "You feel the Blight's touch...", "uiWarning": true },
///     "50": { "description": "Human faction reputation gains locked", "factionLock": true },
///     "75": { "description": "Acquire [MACHINE AFFINITY] trauma", "traumaId": "machine-affinity" },
///     "100": { "description": "Terminal Error - Character transformation", "terminalError": true }
///   },
///   "penalties": {
///     "maxHpPercent": { "formula": "floor(corruption / 10) * 5" },
///     "maxApPercent": { "formula": "floor(corruption / 10) * 5" },
///     "resolveDice": { "formula": "floor(corruption / 20)" }
///   }
/// }
/// </code>
/// </remarks>
/// <seealso cref="CorruptionSourceCategoriesDto"/>
/// <seealso cref="CorruptionSourceDefinitionDto"/>
/// <seealso cref="ThresholdEffectDto"/>
/// <seealso cref="PenaltyFormulasDto"/>
public sealed record CorruptionConfigurationDto
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
    /// Gets the corruption sources organized by category.
    /// </summary>
    /// <remarks>
    /// Contains four arrays matching the <c>CorruptionSource</c> enum categories:
    /// MysticMagic, HereticalAbility, Environmental, and Items.
    /// Each array holds <see cref="CorruptionSourceDefinitionDto"/> entries defining
    /// individual corruption-inducing events with their corruption amounts.
    /// </remarks>
    /// <value>Categorized corruption source definitions.</value>
    [JsonPropertyName("corruptionSources")]
    public required CorruptionSourceCategoriesDto CorruptionSources { get; init; }

    /// <summary>
    /// Gets the threshold effects configuration.
    /// </summary>
    /// <remarks>
    /// Maps threshold values (as string keys: "25", "50", "75", "100") to their
    /// corresponding <see cref="ThresholdEffectDto"/> definitions. Each threshold
    /// triggers once when corruption first crosses that milestone.
    /// </remarks>
    /// <value>Dictionary mapping threshold string keys to effect definitions.</value>
    [JsonPropertyName("thresholdEffects")]
    public required Dictionary<string, ThresholdEffectDto> ThresholdEffects { get; init; }

    /// <summary>
    /// Gets the penalty calculation formulas.
    /// </summary>
    /// <remarks>
    /// Defines formulas for three corruption-based penalties:
    /// Max HP penalty (floor(corruption/10) * 5%),
    /// Max AP penalty (floor(corruption/10) * 5%),
    /// and Resolve dice penalty (floor(corruption/20)).
    /// </remarks>
    /// <value>Penalty formula definitions for HP, AP, and Resolve.</value>
    [JsonPropertyName("penalties")]
    public required PenaltyFormulasDto Penalties { get; init; }
}

/// <summary>
/// DTO for corruption sources organized by category.
/// </summary>
/// <remarks>
/// <para>
/// Each property corresponds to one of the four corruption source categories
/// in the configuration file. Categories that are not present in the configuration
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
/// <item><term>MysticMagic</term><description>Standard spells, powerful spells, overcasting</description></item>
/// <item><term>HereticalAbility</term><description>Berserker rage, Blot-Priest sacrificial casting, Seidkona divination</description></item>
/// <item><term>Environmental</term><description>Blight zone exposure, Forlorn contact</description></item>
/// <item><term>Items</term><description>Glitched artifacts, corrupted consumables</description></item>
/// </list>
/// </remarks>
/// <seealso cref="CorruptionSourceDefinitionDto"/>
/// <seealso cref="CorruptionConfigurationDto"/>
public sealed record CorruptionSourceCategoriesDto
{
    /// <summary>Mystic magic corruption sources.</summary>
    /// <value>Array of mystic magic corruption source definitions, or empty if none configured.</value>
    [JsonPropertyName("mysticMagic")]
    public CorruptionSourceDefinitionDto[] MysticMagic { get; init; } = [];

    /// <summary>Heretical ability corruption sources.</summary>
    /// <value>Array of heretical ability corruption source definitions, or empty if none configured.</value>
    [JsonPropertyName("hereticalAbility")]
    public CorruptionSourceDefinitionDto[] HereticalAbility { get; init; } = [];

    /// <summary>Environmental corruption sources.</summary>
    /// <value>Array of environmental corruption source definitions, or empty if none configured.</value>
    [JsonPropertyName("environmental")]
    public CorruptionSourceDefinitionDto[] Environmental { get; init; } = [];

    /// <summary>Item-based corruption sources.</summary>
    /// <value>Array of item corruption source definitions, or empty if none configured.</value>
    [JsonPropertyName("items")]
    public CorruptionSourceDefinitionDto[] Items { get; init; } = [];
}

/// <summary>
/// DTO for corruption penalty calculation formulas.
/// </summary>
/// <remarks>
/// <para>
/// Defines the three penalty formulas applied based on current corruption level.
/// All formulas use 'corruption' as the variable name. These formulas are stored
/// as human-readable strings; actual calculation is performed by the
/// <c>CorruptionTracker</c> entity using hardcoded implementations.
/// </para>
/// <para>
/// <strong>Default Formulas:</strong>
/// </para>
/// <list type="table">
/// <listheader>
/// <term>Penalty</term>
/// <description>Formula</description>
/// </listheader>
/// <item><term>Max HP %</term><description><c>floor(corruption / 10) * 5</c> — 0% to 50%</description></item>
/// <item><term>Max AP %</term><description><c>floor(corruption / 10) * 5</c> — 0% to 50%</description></item>
/// <item><term>Resolve Dice</term><description><c>floor(corruption / 20)</c> — 0 to 5 dice penalty</description></item>
/// </list>
/// </remarks>
/// <seealso cref="CorruptionConfigurationDto"/>
/// <seealso cref="FormulaDto"/>
public sealed record PenaltyFormulasDto
{
    /// <summary>
    /// Gets the Max HP penalty formula.
    /// </summary>
    /// <remarks>
    /// Default: <c>floor(corruption / 10) * 5</c> — yields 0% at corruption 0,
    /// 25% at corruption 50, and 50% at corruption 100.
    /// </remarks>
    /// <value>Formula definition for Max HP percentage penalty.</value>
    [JsonPropertyName("maxHpPercent")]
    public required FormulaDto MaxHpPercent { get; init; }

    /// <summary>
    /// Gets the Max AP penalty formula.
    /// </summary>
    /// <remarks>
    /// Default: <c>floor(corruption / 10) * 5</c> — identical to HP penalty formula.
    /// </remarks>
    /// <value>Formula definition for Max AP percentage penalty.</value>
    [JsonPropertyName("maxApPercent")]
    public required FormulaDto MaxApPercent { get; init; }

    /// <summary>
    /// Gets the Resolve dice penalty formula.
    /// </summary>
    /// <remarks>
    /// Default: <c>floor(corruption / 20)</c> — yields 0 at corruption 0,
    /// 2 at corruption 45, and 5 at corruption 100.
    /// </remarks>
    /// <value>Formula definition for Resolve dice penalty.</value>
    [JsonPropertyName("resolveDice")]
    public required FormulaDto ResolveDice { get; init; }
}

/// <summary>
/// DTO for a single formula definition.
/// </summary>
/// <remarks>
/// Contains a human-readable formula string using 'corruption' as the variable name.
/// These formulas serve as documentation and configuration reference — actual penalty
/// calculations are implemented in <c>CorruptionTracker</c> computed properties.
/// </remarks>
/// <seealso cref="PenaltyFormulasDto"/>
public sealed record FormulaDto
{
    /// <summary>
    /// Gets the formula expression string.
    /// </summary>
    /// <remarks>
    /// Uses 'corruption' as the variable name. Examples:
    /// <c>"floor(corruption / 10) * 5"</c> for percentage penalties,
    /// <c>"floor(corruption / 20)"</c> for dice penalties.
    /// </remarks>
    /// <value>A formula expression string.</value>
    [JsonPropertyName("formula")]
    public required string Formula { get; init; }
}
