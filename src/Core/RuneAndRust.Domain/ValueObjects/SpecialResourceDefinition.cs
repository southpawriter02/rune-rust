// ═══════════════════════════════════════════════════════════════════════════════
// SpecialResourceDefinition.cs
// Value object defining a special resource unique to a specialization. Special
// resources provide unique gameplay mechanics for certain specializations,
// unlike standard resources (Health, Stamina, Aether Pool). Each resource has
// defined min/max ranges, starting values, and passive regen/decay behavior.
// Version: 0.17.4b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using Microsoft.Extensions.Logging;

/// <summary>
/// Defines a special resource unique to a specialization.
/// </summary>
/// <remarks>
/// <para>
/// Special resources provide unique gameplay mechanics for certain specializations.
/// Unlike standard resources (Health, Stamina, Aether Pool), these are tied to
/// specific specialization abilities and mechanics.
/// </para>
/// <para>
/// Five specializations have unique special resources:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <b>Berserkr</b>: Rage (0–100) — builds during combat via dealing and
///       receiving damage, decays 5 per turn between engagements. Powers
///       devastating rage-fueled abilities.
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>Skjaldmaer</b>: Block Charges (0–3) — starts at maximum, regenerates
///       1 per turn. Consumed by powerful defensive abilities like Shield Wall
///       and Iron Curtain.
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>Iron-Bane</b>: Righteous Fervor (0–50) — builds when engaging
///       Blighted and other abominations. No passive regen or decay.
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>Seiðkona</b>: Aether Resonance (0–10) — accumulates with
///       spellcasting, decays 1 per turn. Enables spell combinations and
///       resonance effects.
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>Echo-Caller</b>: Echoes (0–5) — lingering connections to departed
///       spirits gained from fallen enemies. No passive regen or decay.
///     </description>
///   </item>
/// </list>
/// <para>
/// Use <see cref="Create"/> for validated construction, or <see cref="None"/>
/// for specializations without a unique resource. Check <see cref="HasResource"/>
/// before accessing resource properties.
/// </para>
/// </remarks>
/// <seealso cref="RuneAndRust.Domain.Enums.SpecializationId"/>
/// <seealso cref="RuneAndRust.Domain.Enums.SpecializationPathType"/>
public readonly record struct SpecialResourceDefinition
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Logger for detailed diagnostic output during resource definition creation.
    /// </summary>
    private static ILogger<SpecialResourceDefinition>? _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique identifier for this resource type.
    /// </summary>
    /// <value>
    /// A kebab-case string identifier (e.g., "rage", "block-charges",
    /// "aether-resonance"). Empty string for <see cref="None"/>.
    /// </value>
    /// <remarks>
    /// Uses kebab-case format for consistency with other configuration
    /// identifiers. Normalized to lowercase during creation via
    /// <see cref="Create"/>.
    /// </remarks>
    public string ResourceId { get; init; }

    /// <summary>
    /// Gets the display name shown in UI.
    /// </summary>
    /// <value>
    /// A human-readable name such as "Rage", "Block Charges", or
    /// "Aether Resonance". Empty string for <see cref="None"/>.
    /// </value>
    /// <example>"Rage", "Block Charges", "Righteous Fervor", "Aether Resonance", "Echoes"</example>
    public string DisplayName { get; init; }

    /// <summary>
    /// Gets the minimum value for this resource.
    /// </summary>
    /// <value>
    /// The floor value the resource cannot drop below. Typically 0,
    /// but could be negative for debt-based resources.
    /// </value>
    /// <remarks>
    /// All current special resources have a minimum of 0.
    /// </remarks>
    public int MinValue { get; init; }

    /// <summary>
    /// Gets the maximum value for this resource.
    /// </summary>
    /// <value>
    /// The ceiling value the resource cannot exceed.
    /// </value>
    /// <remarks>
    /// Maximum values vary significantly by resource type:
    /// <list type="bullet">
    ///   <item><description>Rage: 100 (large pool, builds gradually)</description></item>
    ///   <item><description>Righteous Fervor: 50 (moderate pool)</description></item>
    ///   <item><description>Aether Resonance: 10 (small, precise pool)</description></item>
    ///   <item><description>Echoes: 5 (very limited, high-impact)</description></item>
    ///   <item><description>Block Charges: 3 (consumable charges)</description></item>
    /// </list>
    /// </remarks>
    public int MaxValue { get; init; }

    /// <summary>
    /// Gets the starting value when combat begins.
    /// </summary>
    /// <value>
    /// The initial resource value at the start of each combat encounter.
    /// Must be within <see cref="MinValue"/> and <see cref="MaxValue"/> range.
    /// </value>
    /// <remarks>
    /// <para>
    /// Some resources start empty and build during combat (Rage starts at 0,
    /// builds with each strike). Others start full and are consumed
    /// (Block Charges start at 3, spent on defensive abilities).
    /// </para>
    /// <para>
    /// Starting values by resource:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Rage: 0 (builds from empty)</description></item>
    ///   <item><description>Righteous Fervor: 0 (builds from empty)</description></item>
    ///   <item><description>Aether Resonance: 0 (builds from empty)</description></item>
    ///   <item><description>Echoes: 0 (builds from empty)</description></item>
    ///   <item><description>Block Charges: 3 (starts full)</description></item>
    /// </list>
    /// </remarks>
    public int StartsAt { get; init; }

    /// <summary>
    /// Gets the amount regenerated per turn (passive).
    /// </summary>
    /// <value>
    /// Non-negative integer. 0 means no passive regeneration.
    /// </value>
    /// <remarks>
    /// Only Block Charges (1/turn) has passive regeneration among current
    /// special resources. Other resources are generated through specific
    /// gameplay actions (dealing damage, killing enemies, casting spells).
    /// </remarks>
    public int RegenPerTurn { get; init; }

    /// <summary>
    /// Gets the amount that decays per turn (passive).
    /// </summary>
    /// <value>
    /// Non-negative integer. 0 means no passive decay.
    /// </value>
    /// <remarks>
    /// Decay represents the natural loss of accumulated resource over time:
    /// <list type="bullet">
    ///   <item><description>Rage: 5/turn decay (fury subsides without combat)</description></item>
    ///   <item><description>Aether Resonance: 1/turn decay (harmonic attunement fades)</description></item>
    ///   <item><description>All others: 0 (no passive decay)</description></item>
    /// </list>
    /// </remarks>
    public int DecayPerTurn { get; init; }

    /// <summary>
    /// Gets the flavor description for this resource.
    /// </summary>
    /// <value>
    /// A thematic description of the resource's nature and purpose.
    /// Empty string for <see cref="None"/>.
    /// </value>
    public string Description { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this represents a valid special resource.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="ResourceId"/> is non-null and non-empty;
    /// <c>false</c> for <see cref="None"/> or uninitialized instances.
    /// </value>
    /// <remarks>
    /// Check this property before accessing resource properties to distinguish
    /// between specializations with and without special resources. Only 5 of
    /// 17 specializations have special resources.
    /// </remarks>
    public bool HasResource => !string.IsNullOrEmpty(ResourceId);

    // ═══════════════════════════════════════════════════════════════════════════
    // STATIC PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Represents a specialization with no special resource.
    /// </summary>
    /// <value>
    /// A <see cref="SpecialResourceDefinition"/> with empty strings and zero
    /// values. <see cref="HasResource"/> returns <c>false</c>.
    /// </value>
    /// <remarks>
    /// Used as the default value for specializations that do not have unique
    /// resource mechanics (12 of 17 specializations). Prefer this over null
    /// to avoid null reference checks throughout the codebase.
    /// </remarks>
    public static SpecialResourceDefinition None => new()
    {
        ResourceId = string.Empty,
        DisplayName = string.Empty,
        MinValue = 0,
        MaxValue = 0,
        StartsAt = 0,
        RegenPerTurn = 0,
        DecayPerTurn = 0,
        Description = string.Empty
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new special resource definition with validation.
    /// </summary>
    /// <param name="resourceId">
    /// Unique identifier in kebab-case format (e.g., "rage", "block-charges").
    /// Will be normalized to lowercase.
    /// </param>
    /// <param name="displayName">
    /// UI display name (e.g., "Rage", "Block Charges").
    /// </param>
    /// <param name="minValue">
    /// Minimum resource value. Typically 0.
    /// </param>
    /// <param name="maxValue">
    /// Maximum resource value. Must be greater than or equal to
    /// <paramref name="minValue"/>.
    /// </param>
    /// <param name="startsAt">
    /// Starting value at combat start. Must be within
    /// [<paramref name="minValue"/>, <paramref name="maxValue"/>] range.
    /// </param>
    /// <param name="regenPerTurn">
    /// Passive regeneration per turn. Must be non-negative.
    /// </param>
    /// <param name="decayPerTurn">
    /// Passive decay per turn. Must be non-negative.
    /// </param>
    /// <param name="description">
    /// Flavor description of the resource.
    /// </param>
    /// <param name="logger">
    /// Optional logger for diagnostic output during creation.
    /// </param>
    /// <returns>A validated <see cref="SpecialResourceDefinition"/>.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="resourceId"/> or <paramref name="displayName"/>
    /// is null or whitespace.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="minValue"/> exceeds <paramref name="maxValue"/>,
    /// <paramref name="startsAt"/> is outside the valid range, or
    /// <paramref name="regenPerTurn"/>/<paramref name="decayPerTurn"/> is negative.
    /// </exception>
    /// <example>
    /// <code>
    /// // Create Rage resource for Berserkr
    /// var rage = SpecialResourceDefinition.Create(
    ///     resourceId: "rage",
    ///     displayName: "Rage",
    ///     minValue: 0,
    ///     maxValue: 100,
    ///     startsAt: 0,
    ///     regenPerTurn: 0,
    ///     decayPerTurn: 5,
    ///     description: "Fury that builds with each strike and received blow");
    ///
    /// // Create Block Charges for Skjaldmaer
    /// var charges = SpecialResourceDefinition.Create(
    ///     resourceId: "block-charges",
    ///     displayName: "Block Charges",
    ///     minValue: 0,
    ///     maxValue: 3,
    ///     startsAt: 3,
    ///     regenPerTurn: 1,
    ///     decayPerTurn: 0,
    ///     description: "Stored defensive reactions ready to deflect incoming attacks");
    /// </code>
    /// </example>
    public static SpecialResourceDefinition Create(
        string resourceId,
        string displayName,
        int minValue,
        int maxValue,
        int startsAt,
        int regenPerTurn,
        int decayPerTurn,
        string description,
        ILogger<SpecialResourceDefinition>? logger = null)
    {
        // Store logger for this creation context
        _logger = logger;

        _logger?.LogDebug(
            "Creating SpecialResourceDefinition. ResourceId='{ResourceId}', " +
            "DisplayName='{DisplayName}', Range={MinValue}-{MaxValue}, " +
            "StartsAt={StartsAt}, RegenPerTurn={RegenPerTurn}, DecayPerTurn={DecayPerTurn}",
            resourceId,
            displayName,
            minValue,
            maxValue,
            startsAt,
            regenPerTurn,
            decayPerTurn);

        // Validate required string parameters
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceId, nameof(resourceId));
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName, nameof(displayName));

        // Validate numeric ranges — min must not exceed max
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minValue, maxValue, nameof(minValue));

        // Validate starting value is within valid range
        ArgumentOutOfRangeException.ThrowIfLessThan(startsAt, minValue, nameof(startsAt));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(startsAt, maxValue, nameof(startsAt));

        // Validate passive rates are non-negative
        ArgumentOutOfRangeException.ThrowIfNegative(regenPerTurn, nameof(regenPerTurn));
        ArgumentOutOfRangeException.ThrowIfNegative(decayPerTurn, nameof(decayPerTurn));

        _logger?.LogDebug(
            "Validation passed for SpecialResourceDefinition '{ResourceId}'. " +
            "Range: {MinValue}-{MaxValue}, StartsAt: {StartsAt}, " +
            "Regen: {RegenPerTurn}/turn, Decay: {DecayPerTurn}/turn",
            resourceId,
            minValue,
            maxValue,
            startsAt,
            regenPerTurn,
            decayPerTurn);

        var resource = new SpecialResourceDefinition
        {
            ResourceId = resourceId.ToLowerInvariant(),
            DisplayName = displayName,
            MinValue = minValue,
            MaxValue = maxValue,
            StartsAt = startsAt,
            RegenPerTurn = regenPerTurn,
            DecayPerTurn = decayPerTurn,
            Description = description ?? string.Empty
        };

        _logger?.LogInformation(
            "Created SpecialResourceDefinition '{DisplayName}' (ID: '{ResourceId}'). " +
            "Range: {MinValue}-{MaxValue}, StartsAt: {StartsAt}, " +
            "RegenPerTurn: {RegenPerTurn}, DecayPerTurn: {DecayPerTurn}",
            resource.DisplayName,
            resource.ResourceId,
            resource.MinValue,
            resource.MaxValue,
            resource.StartsAt,
            resource.RegenPerTurn,
            resource.DecayPerTurn);

        return resource;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a formatted string representation of this resource definition.
    /// </summary>
    /// <returns>
    /// A string in the format "{DisplayName} ({MinValue}-{MaxValue})" for
    /// valid resources, or "None" for empty resource definitions.
    /// </returns>
    /// <example>
    /// <code>
    /// var rage = SpecialResourceDefinition.Create("rage", "Rage", 0, 100, 0, 0, 5, "Fury");
    /// rage.ToString(); // "Rage (0-100)"
    ///
    /// var none = SpecialResourceDefinition.None;
    /// none.ToString(); // "None"
    /// </code>
    /// </example>
    public override string ToString()
    {
        return HasResource
            ? $"{DisplayName} ({MinValue}-{MaxValue})"
            : "None";
    }
}
