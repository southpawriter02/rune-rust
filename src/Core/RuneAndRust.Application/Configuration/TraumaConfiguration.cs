// ═══════════════════════════════════════════════════════════════════════════════
// TraumaConfiguration.cs
// Configuration records for deserializing trauma definitions from traumas.json.
// Version: 0.18.3e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Configuration;

/// <summary>
/// Root configuration structure for trauma definitions.
/// </summary>
/// <remarks>
/// <para>
/// Loaded from <c>config/traumas.json</c> at service initialization.
/// Contains version metadata and the full catalog of trauma definitions.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// {
///   "version": "1.0",
///   "traumas": [
///     { "id": "survivors-guilt", ... }
///   ]
/// }
/// </code>
/// </example>
public record TraumaConfiguration
{
    /// <summary>
    /// Gets the version of the configuration format.
    /// </summary>
    /// <remarks>
    /// Used for schema validation and migration support.
    /// Format: "Major.Minor" (e.g., "1.0").
    /// </remarks>
    public string Version { get; init; } = string.Empty;

    /// <summary>
    /// Gets the list of trauma definitions.
    /// </summary>
    /// <remarks>
    /// Contains all trauma templates that can be acquired by characters.
    /// Each entry defines the trauma's mechanical properties and effects.
    /// </remarks>
    public IReadOnlyList<TraumaConfigEntry> Traumas { get; init; } = [];
}

/// <summary>
/// Individual trauma definition from configuration.
/// </summary>
/// <remarks>
/// <para>
/// Represents a single trauma type that characters can acquire.
/// Maps directly to <see cref="Domain.Entities.TraumaDefinition"/> after parsing.
/// </para>
/// </remarks>
public record TraumaConfigEntry
{
    /// <summary>
    /// Gets the unique identifier for this trauma.
    /// </summary>
    /// <remarks>
    /// Lowercase kebab-case format (e.g., "survivors-guilt", "reality-doubt").
    /// Used as the primary key for lookups and persistence.
    /// </remarks>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the display name.
    /// </summary>
    /// <remarks>
    /// Human-readable name shown to players (e.g., "Survivor's Guilt").
    /// </remarks>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the trauma type category.
    /// </summary>
    /// <remarks>
    /// One of: Cognitive, Emotional, Physical, Social, Existential, Corruption.
    /// Determines which trauma checks can result in this trauma.
    /// </remarks>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Gets the mechanical description.
    /// </summary>
    /// <remarks>
    /// Clear explanation of what this trauma does mechanically.
    /// Used for tooltips and player information.
    /// </remarks>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets the narrative flavor text.
    /// </summary>
    /// <remarks>
    /// In-character description of the trauma experience.
    /// Often poetic or disturbing to emphasize psychological impact.
    /// </remarks>
    public string FlavorText { get; init; } = string.Empty;

    /// <summary>
    /// Gets whether this is a retirement trauma.
    /// </summary>
    /// <remarks>
    /// If true, acquiring this trauma may trigger character retirement.
    /// Check <see cref="RetirementCondition"/> for specific trigger rules.
    /// </remarks>
    public bool IsRetirementTrauma { get; init; }

    /// <summary>
    /// Gets the retirement condition description.
    /// </summary>
    /// <remarks>
    /// <para>Describes when retirement triggers:</para>
    /// <list type="bullet">
    ///   <item><description>"On acquisition" - Immediate retirement check</description></item>
    ///   <item><description>"5+" - After 5 stacks</description></item>
    ///   <item><description>"3+" - After 3 stacks</description></item>
    /// </list>
    /// <para>Null if <see cref="IsRetirementTrauma"/> is false.</para>
    /// </remarks>
    public string? RetirementCondition { get; init; }

    /// <summary>
    /// Gets whether this trauma can stack.
    /// </summary>
    /// <remarks>
    /// <para>If true, acquiring the trauma multiple times increments StackCount.</para>
    /// <para>If false, subsequent acquisitions are ignored.</para>
    /// </remarks>
    public bool IsStackable { get; init; }

    /// <summary>
    /// Gets the acquisition sources/triggers.
    /// </summary>
    /// <remarks>
    /// Event types that can cause this trauma to be acquired
    /// (e.g., "AllyDeath", "CorruptionThreshold75").
    /// </remarks>
    public IReadOnlyList<string> AcquisitionSources { get; init; } = [];

    /// <summary>
    /// Gets the triggers for this trauma.
    /// </summary>
    /// <remarks>
    /// Events that activate the trauma's effects after acquisition.
    /// </remarks>
    public IReadOnlyList<TraumaConfigTrigger> Triggers { get; init; } = [];

    /// <summary>
    /// Gets the mechanical effects.
    /// </summary>
    /// <remarks>
    /// Penalties, disadvantages, and other mechanical impacts of this trauma.
    /// </remarks>
    public IReadOnlyList<TraumaConfigEffect> Effects { get; init; } = [];
}

/// <summary>
/// Effect definition from configuration.
/// </summary>
/// <remarks>
/// Represents a single mechanical effect applied by a trauma.
/// Maps to <see cref="Domain.ValueObjects.TraumaEffect"/>.
/// </remarks>
public record TraumaConfigEffect
{
    /// <summary>
    /// Gets the effect type.
    /// </summary>
    /// <remarks>
    /// Examples: "Penalty", "Disadvantage", "StressIncrease", "Bonus".
    /// </remarks>
    public string EffectType { get; init; } = string.Empty;

    /// <summary>
    /// Gets the effect target.
    /// </summary>
    /// <remarks>
    /// What the effect applies to (e.g., "initiative", "social-checks", "morale-checks").
    /// </remarks>
    public string Target { get; init; } = string.Empty;

    /// <summary>
    /// Gets the numeric value (if applicable).
    /// </summary>
    /// <remarks>
    /// The magnitude of the effect (e.g., -2 for a penalty, +5 for stress increase).
    /// Null for boolean effects like Disadvantage.
    /// </remarks>
    public int? Value { get; init; }

    /// <summary>
    /// Gets the activation condition (if any).
    /// </summary>
    /// <remarks>
    /// When the effect activates (e.g., "OnRest", "InCombat", "OnAllyTakeCriticalHit").
    /// Null if the effect is always active.
    /// </remarks>
    public string? Condition { get; init; }

    /// <summary>
    /// Gets the description.
    /// </summary>
    /// <remarks>
    /// Human-readable explanation of what this effect does.
    /// </remarks>
    public string Description { get; init; } = string.Empty;
}

/// <summary>
/// Trigger definition from configuration.
/// </summary>
/// <remarks>
/// Represents when a trauma's effects should activate.
/// Maps to <see cref="Domain.ValueObjects.TraumaTrigger"/>.
/// </remarks>
public record TraumaConfigTrigger
{
    /// <summary>
    /// Gets the trigger type.
    /// </summary>
    /// <remarks>
    /// Event type that activates this trigger (e.g., "OnRest", "InCombat", "OnMeetNewNPC").
    /// </remarks>
    public string TriggerType { get; init; } = string.Empty;

    /// <summary>
    /// Gets the specific condition (if any).
    /// </summary>
    /// <remarks>
    /// Additional constraint for trigger activation.
    /// Null if trigger activates without conditions.
    /// </remarks>
    public string? Condition { get; init; }

    /// <summary>
    /// Gets whether a check is required.
    /// </summary>
    /// <remarks>
    /// If true, character must make a check when trigger activates.
    /// </remarks>
    public bool CheckRequired { get; init; }

    /// <summary>
    /// Gets the check difficulty (if required).
    /// </summary>
    /// <remarks>
    /// Number of successes needed to pass the check (1-4 scale).
    /// Null if <see cref="CheckRequired"/> is false.
    /// </remarks>
    public int? CheckDifficulty { get; init; }
}
