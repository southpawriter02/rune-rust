// ═══════════════════════════════════════════════════════════════════════════════
// SpecializationAbility.cs
// Value object representing an individual ability within a specialization tier.
// Each ability has mechanical properties including resource cost, cooldown,
// passive/active classification, and Corruption risk for Heretical paths.
// Version: 0.17.4c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using Microsoft.Extensions.Logging;

/// <summary>
/// Represents an individual ability within a specialization tier.
/// </summary>
/// <remarks>
/// <para>
/// SpecializationAbility defines the mechanical properties of abilities unique
/// to a specialization. These abilities are organized into tiers (1, 2, 3) and
/// unlocked via Progression Points. Each specialization has 7-12 abilities
/// distributed across three tiers.
/// </para>
/// <para>
/// Abilities can be:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <b>Active</b>: Requires manual activation. May have resource cost and cooldown.
///       Examples: Rage Strike (20 Rage, no cooldown), Berserker Charge (30 Rage, 4-turn CD).
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>Passive</b>: Always in effect once the tier is unlocked. No resource cost
///       or cooldown. Examples: Blood Frenzy (+10% damage below 50% HP), Pain is Power
///       (damage taken converts to Rage).
///     </description>
///   </item>
/// </list>
/// <para>
/// Heretical path abilities may have a <see cref="CorruptionRisk"/> value indicating
/// the chance or amount of Corruption gain when using the ability. Coherent path
/// abilities always have 0 Corruption risk.
/// </para>
/// <para>
/// Ability IDs use kebab-case format (e.g., "rage-strike", "blood-frenzy") and
/// are normalized to lowercase during creation for consistent lookups.
/// </para>
/// <para>
/// This is an immutable value object using the <c>readonly record struct</c> pattern.
/// All instances are created via the <see cref="Create"/> factory method or
/// convenience factories <see cref="CreateActive"/> and <see cref="CreatePassive"/>.
/// </para>
/// </remarks>
/// <seealso cref="SpecializationAbilityTier"/>
/// <seealso cref="SpecialResourceDefinition"/>
/// <seealso cref="RuneAndRust.Domain.Entities.SpecializationDefinition"/>
/// <seealso cref="RuneAndRust.Domain.Enums.SpecializationId"/>
public readonly record struct SpecializationAbility
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Logger for detailed diagnostic output during ability creation.
    /// </summary>
    private static ILogger<SpecializationAbility>? _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique identifier for this ability.
    /// </summary>
    /// <value>
    /// A kebab-case string identifier (e.g., "rage-strike", "blood-frenzy").
    /// Normalized to lowercase during creation.
    /// </value>
    /// <remarks>
    /// Must be unique within the specialization (across all tiers).
    /// Used for ability lookups, combat system references, and persistence.
    /// </remarks>
    public string AbilityId { get; init; }

    /// <summary>
    /// Gets the display name shown in UI.
    /// </summary>
    /// <value>A player-friendly ability name (e.g., "Rage Strike", "Blood Frenzy").</value>
    /// <example>"Rage Strike", "Blood Frenzy", "Intimidating Presence"</example>
    public string DisplayName { get; init; }

    /// <summary>
    /// Gets the description of the ability's effect.
    /// </summary>
    /// <value>A concise description of what the ability does mechanically.</value>
    /// <example>"Channel rage into a devastating blow. Damage scales with current Rage."</example>
    public string Description { get; init; }

    /// <summary>
    /// Gets whether this is a passive ability.
    /// </summary>
    /// <value>
    /// <c>true</c> for passive abilities (always in effect once unlocked);
    /// <c>false</c> for active abilities (require manual activation).
    /// </value>
    /// <remarks>
    /// Passive abilities cannot have a cooldown. Active abilities may or may not
    /// have a cooldown depending on their design.
    /// </remarks>
    public bool IsPassive { get; init; }

    /// <summary>
    /// Gets the resource cost to use this ability.
    /// </summary>
    /// <value>
    /// Non-negative integer. 0 for passive abilities or active abilities with
    /// no resource cost.
    /// </value>
    /// <remarks>
    /// The resource consumed is identified by <see cref="ResourceType"/>. For
    /// specialization-specific resources (e.g., Rage, Block Charges), this
    /// references the <see cref="SpecialResourceDefinition"/>. For standard
    /// resources (Stamina, Health), this references the base resource system.
    /// </remarks>
    public int ResourceCost { get; init; }

    /// <summary>
    /// Gets the resource type consumed by this ability.
    /// </summary>
    /// <value>
    /// A lowercase string identifying the resource type. Empty string for
    /// abilities with no resource cost.
    /// </value>
    /// <remarks>
    /// <para>Can be one of:</para>
    /// <list type="bullet">
    ///   <item><description>Special resource ID (e.g., "rage", "block-charges", "aether-resonance")</description></item>
    ///   <item><description>Standard resource (e.g., "stamina", "health")</description></item>
    ///   <item><description>Empty string for no-cost abilities (passive or free active)</description></item>
    /// </list>
    /// </remarks>
    public string ResourceType { get; init; }

    /// <summary>
    /// Gets the cooldown in turns between uses.
    /// </summary>
    /// <value>
    /// Non-negative integer. 0 for abilities with no cooldown.
    /// Passive abilities always have 0 cooldown.
    /// </value>
    /// <remarks>
    /// After using an ability with a cooldown, the player must wait this many
    /// turns before using it again. Cooldowns are tracked per-ability in combat.
    /// </remarks>
    public int Cooldown { get; init; }

    /// <summary>
    /// Gets the Corruption risk when using this ability.
    /// </summary>
    /// <value>
    /// Non-negative integer. 0 for Coherent path abilities or Heretical abilities
    /// with no Corruption risk on this specific ability.
    /// </value>
    /// <remarks>
    /// <para>Positive values indicate chance or amount of Corruption gain when using
    /// the ability. Heretical path abilities typically have values between 5-20.</para>
    /// <para>Corruption risk integrates with the Trauma Economy system. The exact
    /// mechanical application (flat amount, percentage chance, etc.) is determined
    /// by the combat system.</para>
    /// </remarks>
    public int CorruptionRisk { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this ability has a resource cost.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="ResourceCost"/> is greater than 0 and
    /// <see cref="ResourceType"/> is non-empty; <c>false</c> otherwise.
    /// </value>
    public bool HasResourceCost => ResourceCost > 0 && !string.IsNullOrEmpty(ResourceType);

    /// <summary>
    /// Gets whether this ability has a cooldown.
    /// </summary>
    /// <value><c>true</c> if <see cref="Cooldown"/> is greater than 0.</value>
    public bool HasCooldown => Cooldown > 0;

    /// <summary>
    /// Gets whether this ability risks Corruption on use.
    /// </summary>
    /// <value><c>true</c> if <see cref="CorruptionRisk"/> is greater than 0.</value>
    public bool RisksCorruption => CorruptionRisk > 0;

    /// <summary>
    /// Gets whether this is an active ability (requires manual activation).
    /// </summary>
    /// <value><c>true</c> if not passive; <c>false</c> for passive abilities.</value>
    public bool IsActive => !IsPassive;

    /// <summary>
    /// Gets a bracketed type display tag for UI formatting.
    /// </summary>
    /// <value>"[ACTIVE]" or "[PASSIVE]" depending on ability type.</value>
    public string TypeDisplay => IsPassive ? "[PASSIVE]" : "[ACTIVE]";

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new specialization ability with comprehensive validation.
    /// </summary>
    /// <param name="abilityId">Unique kebab-case identifier (e.g., "rage-strike"). Normalized to lowercase.</param>
    /// <param name="displayName">UI display name (e.g., "Rage Strike").</param>
    /// <param name="description">Effect description (e.g., "Channel rage into a devastating blow.").</param>
    /// <param name="isPassive"><c>true</c> for passive abilities, <c>false</c> for active.</param>
    /// <param name="resourceCost">Resource amount required to use (0 for no cost). Must be non-negative.</param>
    /// <param name="resourceType">Resource type ID consumed (empty for no cost). Normalized to lowercase.</param>
    /// <param name="cooldown">Turns between uses (0 for no cooldown). Must be non-negative. Must be 0 for passive abilities.</param>
    /// <param name="corruptionRisk">Corruption risk amount on use (0 for none). Must be non-negative.</param>
    /// <param name="logger">Optional logger for diagnostic output during creation.</param>
    /// <returns>A validated and normalized <see cref="SpecializationAbility"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="abilityId"/>, <paramref name="displayName"/>, or
    /// <paramref name="description"/> is null or whitespace, or when a passive ability
    /// has a non-zero cooldown.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="resourceCost"/>, <paramref name="cooldown"/>, or
    /// <paramref name="corruptionRisk"/> is negative.
    /// </exception>
    /// <example>
    /// <code>
    /// // Create an active ability with resource cost and corruption risk
    /// var rageStrike = SpecializationAbility.Create(
    ///     "rage-strike", "Rage Strike",
    ///     "Channel rage into a devastating blow.",
    ///     isPassive: false, resourceCost: 20, resourceType: "rage",
    ///     cooldown: 0, corruptionRisk: 5);
    ///
    /// // Create a passive ability
    /// var bloodFrenzy = SpecializationAbility.Create(
    ///     "blood-frenzy", "Blood Frenzy",
    ///     "When below 50% health, deal 10% increased damage.",
    ///     isPassive: true);
    /// </code>
    /// </example>
    public static SpecializationAbility Create(
        string abilityId,
        string displayName,
        string description,
        bool isPassive,
        int resourceCost = 0,
        string resourceType = "",
        int cooldown = 0,
        int corruptionRisk = 0,
        ILogger<SpecializationAbility>? logger = null)
    {
        // Store logger for this creation context
        _logger = logger;

        _logger?.LogDebug(
            "Creating SpecializationAbility with ID '{AbilityId}', display name '{DisplayName}', " +
            "isPassive: {IsPassive}, resourceCost: {ResourceCost}, resourceType: '{ResourceType}', " +
            "cooldown: {Cooldown}, corruptionRisk: {CorruptionRisk}",
            abilityId,
            displayName,
            isPassive,
            resourceCost,
            resourceType,
            cooldown,
            corruptionRisk);

        // Validate required string parameters
        ArgumentException.ThrowIfNullOrWhiteSpace(abilityId, nameof(abilityId));
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName, nameof(displayName));
        ArgumentException.ThrowIfNullOrWhiteSpace(description, nameof(description));

        // Validate non-negative numeric parameters
        ArgumentOutOfRangeException.ThrowIfNegative(resourceCost, nameof(resourceCost));
        ArgumentOutOfRangeException.ThrowIfNegative(cooldown, nameof(cooldown));
        ArgumentOutOfRangeException.ThrowIfNegative(corruptionRisk, nameof(corruptionRisk));

        // Passive abilities cannot have a cooldown
        if (isPassive && cooldown > 0)
        {
            _logger?.LogWarning(
                "Validation failed for ability '{AbilityId}': passive ability cannot have a cooldown " +
                "(cooldown={Cooldown})",
                abilityId,
                cooldown);

            throw new ArgumentException(
                "Passive abilities cannot have a cooldown",
                nameof(cooldown));
        }

        _logger?.LogDebug(
            "Validation passed for ability '{AbilityId}'. All string parameters are non-empty, " +
            "numeric parameters are non-negative, passive/cooldown constraint satisfied",
            abilityId);

        var ability = new SpecializationAbility
        {
            AbilityId = abilityId.ToLowerInvariant(),
            DisplayName = displayName.Trim(),
            Description = description.Trim(),
            IsPassive = isPassive,
            ResourceCost = resourceCost,
            ResourceType = resourceType?.ToLowerInvariant() ?? string.Empty,
            Cooldown = cooldown,
            CorruptionRisk = corruptionRisk
        };

        _logger?.LogInformation(
            "Created SpecializationAbility '{DisplayName}' (ID: {AbilityId}). " +
            "Type: {Type}, ResourceCost: {ResourceCost} {ResourceType}, " +
            "Cooldown: {Cooldown}, CorruptionRisk: {CorruptionRisk}, " +
            "HasResourceCost: {HasResourceCost}, HasCooldown: {HasCooldown}, " +
            "RisksCorruption: {RisksCorruption}",
            ability.DisplayName,
            ability.AbilityId,
            ability.IsPassive ? "Passive" : "Active",
            ability.ResourceCost,
            ability.ResourceType,
            ability.Cooldown,
            ability.CorruptionRisk,
            ability.HasResourceCost,
            ability.HasCooldown,
            ability.RisksCorruption);

        return ability;
    }

    /// <summary>
    /// Creates an active ability with standard parameters.
    /// </summary>
    /// <param name="abilityId">Unique kebab-case identifier. Normalized to lowercase.</param>
    /// <param name="displayName">UI display name.</param>
    /// <param name="description">Effect description.</param>
    /// <param name="resourceCost">Resource amount required to use.</param>
    /// <param name="resourceType">Resource type ID consumed.</param>
    /// <param name="cooldown">Turns between uses (0 for no cooldown).</param>
    /// <param name="corruptionRisk">Corruption risk amount on use (0 for none).</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <returns>A validated active <see cref="SpecializationAbility"/>.</returns>
    /// <example>
    /// <code>
    /// var rageStrike = SpecializationAbility.CreateActive(
    ///     "rage-strike", "Rage Strike",
    ///     "Channel rage into a devastating blow.",
    ///     resourceCost: 20, resourceType: "rage",
    ///     cooldown: 0, corruptionRisk: 5);
    /// </code>
    /// </example>
    public static SpecializationAbility CreateActive(
        string abilityId,
        string displayName,
        string description,
        int resourceCost,
        string resourceType,
        int cooldown = 0,
        int corruptionRisk = 0,
        ILogger<SpecializationAbility>? logger = null)
    {
        return Create(
            abilityId,
            displayName,
            description,
            isPassive: false,
            resourceCost,
            resourceType,
            cooldown,
            corruptionRisk,
            logger);
    }

    /// <summary>
    /// Creates a passive ability.
    /// </summary>
    /// <param name="abilityId">Unique kebab-case identifier. Normalized to lowercase.</param>
    /// <param name="displayName">UI display name.</param>
    /// <param name="description">Effect description.</param>
    /// <param name="corruptionRisk">Corruption risk amount (0 for none).</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <returns>A validated passive <see cref="SpecializationAbility"/>.</returns>
    /// <remarks>
    /// Passive abilities always have 0 resource cost, empty resource type, and 0 cooldown.
    /// </remarks>
    /// <example>
    /// <code>
    /// var bloodFrenzy = SpecializationAbility.CreatePassive(
    ///     "blood-frenzy", "Blood Frenzy",
    ///     "When below 50% health, deal 10% increased damage.");
    /// </code>
    /// </example>
    public static SpecializationAbility CreatePassive(
        string abilityId,
        string displayName,
        string description,
        int corruptionRisk = 0,
        ILogger<SpecializationAbility>? logger = null)
    {
        return Create(
            abilityId,
            displayName,
            description,
            isPassive: true,
            resourceCost: 0,
            resourceType: string.Empty,
            cooldown: 0,
            corruptionRisk,
            logger);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a short display string with ability name and type tag.
    /// </summary>
    /// <returns>A formatted string like "Rage Strike [ACTIVE]" or "Blood Frenzy [PASSIVE]".</returns>
    /// <example>
    /// <code>
    /// var ability = SpecializationAbility.CreateActive("rage-strike", "Rage Strike", "...", 20, "rage");
    /// ability.GetShortDisplay(); // "Rage Strike [ACTIVE]"
    /// </code>
    /// </example>
    public string GetShortDisplay() => $"{DisplayName} {TypeDisplay}";

    /// <summary>
    /// Gets a detailed multi-line display string with all ability properties.
    /// </summary>
    /// <returns>
    /// A formatted multi-line string with name, type, cost, cooldown, and description.
    /// </returns>
    /// <example>
    /// <code>
    /// var ability = SpecializationAbility.CreateActive("rage-strike", "Rage Strike", "A powerful blow.", 20, "rage", 0, 5);
    /// ability.GetDetailDisplay();
    /// // "Rage Strike [ACTIVE]
    /// //  Cost: 20 rage
    /// //  Corruption Risk: 5
    /// //  A powerful blow."
    /// </code>
    /// </example>
    public string GetDetailDisplay()
    {
        var lines = new List<string> { $"{DisplayName} {TypeDisplay}" };

        if (HasResourceCost)
        {
            lines.Add($"  Cost: {ResourceCost} {ResourceType}");
        }

        if (HasCooldown)
        {
            lines.Add($"  Cooldown: {Cooldown} turns");
        }

        if (RisksCorruption)
        {
            lines.Add($"  Corruption Risk: {CorruptionRisk}");
        }

        lines.Add($"  {Description}");

        return string.Join("\n", lines);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a compact string representation for debugging.
    /// </summary>
    /// <returns>
    /// A formatted string showing the ability name, type, cost, and cooldown.
    /// Format: "{DisplayName} [{type}{cost}{cd}]"
    /// </returns>
    /// <example>
    /// <code>
    /// // Active with cost and cooldown:  "Rage Strike [Active, 20 rage, CD: 3]"
    /// // Active with cost only:          "Rage Strike [Active, 20 rage]"
    /// // Passive:                        "Blood Frenzy [Passive]"
    /// </code>
    /// </example>
    public override string ToString()
    {
        var type = IsPassive ? "Passive" : "Active";
        var cost = HasResourceCost ? $", {ResourceCost} {ResourceType}" : "";
        var cd = HasCooldown ? $", CD: {Cooldown}" : "";
        return $"{DisplayName} [{type}{cost}{cd}]";
    }
}
