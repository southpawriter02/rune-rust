namespace RuneAndRust.Domain.ValueObjects;

using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Immutable value object representing a special effect on a Myth-Forged item.
/// Effects are defined in configuration and referenced by UniqueItem.SpecialEffectIds.
/// </summary>
/// <remarks>
/// <para>
/// Special effects are the defining features of Myth-Forged (Tier 4) items,
/// providing powerful unique modifiers such as armor penetration, life steal,
/// elemental damage, and passive detection abilities.
/// </para>
/// <para>
/// Each effect has:
/// <list type="bullet">
///   <item><description><see cref="EffectId"/>: Configuration identifier (e.g., "life-steal")</description></item>
///   <item><description><see cref="EffectType"/>: The type of special effect from <see cref="SpecialEffectType"/></description></item>
///   <item><description><see cref="TriggerType"/>: When the effect activates from <see cref="EffectTriggerType"/></description></item>
///   <item><description><see cref="Magnitude"/>: Effect strength (interpretation varies by type)</description></item>
///   <item><description><see cref="DamageTypeId"/>: Optional damage type for elemental effects</description></item>
///   <item><description><see cref="Description"/>: Player-facing description of the effect</description></item>
/// </list>
/// </para>
/// <para>
/// The <see cref="Create"/> factory method validates:
/// <list type="bullet">
///   <item><description>Effect ID is not null or whitespace</description></item>
///   <item><description>Effect type is not <see cref="SpecialEffectType.None"/></description></item>
///   <item><description>Magnitude is appropriate for the effect type</description></item>
///   <item><description>Elemental effects have a damage type ID</description></item>
///   <item><description>Trigger type matches the expected type for the effect</description></item>
/// </list>
/// </para>
/// </remarks>
/// <seealso cref="SpecialEffectType"/>
/// <seealso cref="EffectTriggerType"/>
public readonly record struct SpecialEffect
{
    /// <summary>
    /// Configuration identifier (e.g., "life-steal").
    /// </summary>
    /// <remarks>
    /// Effect IDs are normalized to lowercase during creation.
    /// IDs should use kebab-case format (e.g., "life-steal", "fire-damage").
    /// </remarks>
    public string EffectId { get; }

    /// <summary>
    /// The type of special effect.
    /// </summary>
    /// <remarks>
    /// Determines the behavior and expected <see cref="TriggerType"/> of the effect.
    /// </remarks>
    public SpecialEffectType EffectType { get; }

    /// <summary>
    /// When this effect triggers.
    /// </summary>
    /// <remarks>
    /// Must match the expected trigger type for the <see cref="EffectType"/>.
    /// Use <see cref="GetExpectedTriggerType"/> to get the expected trigger for an effect type.
    /// </remarks>
    public EffectTriggerType TriggerType { get; }

    /// <summary>
    /// Effect strength/magnitude. Interpretation varies by effect type.
    /// </summary>
    /// <remarks>
    /// <para>Magnitude interpretation by effect category:</para>
    /// <list type="bullet">
    ///   <item><description><b>Percentage effects</b> (<see cref="SpecialEffectType.LifeSteal"/>, 
    ///     <see cref="SpecialEffectType.Reflect"/>, <see cref="SpecialEffectType.CriticalBonus"/>): 
    ///     0.0-1.0 (e.g., 0.15 = 15%)</description></item>
    ///   <item><description><b>Flat damage</b> (Elemental effects): Absolute damage value 
    ///     (e.g., 10 = 10 bonus damage)</description></item>
    ///   <item><description><b>Duration</b> (<see cref="SpecialEffectType.Slow"/>): 
    ///     Seconds or turns</description></item>
    ///   <item><description><b>Binary effects</b> (<see cref="SpecialEffectType.IgnoreArmor"/>, 
    ///     <see cref="SpecialEffectType.Cleave"/>, etc.): Typically 1.0</description></item>
    /// </list>
    /// </remarks>
    public decimal Magnitude { get; }

    /// <summary>
    /// Optional damage type for elemental effects (e.g., "fire", "ice", "shadow").
    /// </summary>
    /// <remarks>
    /// Required for <see cref="SpecialEffectType.FireDamage"/>, <see cref="SpecialEffectType.IceDamage"/>,
    /// and <see cref="SpecialEffectType.LightningDamage"/> effects. Null for non-elemental effects.
    /// </remarks>
    public string? DamageTypeId { get; }

    /// <summary>
    /// Player-facing description of the effect.
    /// </summary>
    /// <remarks>
    /// If not provided during creation, a default description is generated based on the effect type.
    /// </remarks>
    public string Description { get; }

    /// <summary>
    /// Empty/null effect.
    /// </summary>
    /// <remarks>
    /// Returns a default effect with <see cref="SpecialEffectType.None"/> and no magnitude.
    /// Use for representing the absence of an effect.
    /// </remarks>
    public static SpecialEffect None => new(
        "none",
        SpecialEffectType.None,
        EffectTriggerType.Passive,
        0m,
        null,
        "No effect");

    /// <summary>
    /// Private constructor for creating SpecialEffect instances.
    /// </summary>
    /// <param name="effectId">The effect identifier.</param>
    /// <param name="effectType">The type of effect.</param>
    /// <param name="triggerType">When the effect triggers.</param>
    /// <param name="magnitude">The effect strength.</param>
    /// <param name="damageTypeId">Optional damage type for elemental effects.</param>
    /// <param name="description">Player-facing description.</param>
    private SpecialEffect(
        string effectId,
        SpecialEffectType effectType,
        EffectTriggerType triggerType,
        decimal magnitude,
        string? damageTypeId,
        string description)
    {
        EffectId = effectId;
        EffectType = effectType;
        TriggerType = triggerType;
        Magnitude = magnitude;
        DamageTypeId = damageTypeId;
        Description = description;
    }

    /// <summary>
    /// Creates a new SpecialEffect with full validation.
    /// </summary>
    /// <param name="effectId">The unique effect identifier (will be normalized to lowercase).</param>
    /// <param name="effectType">The type of special effect.</param>
    /// <param name="triggerType">When the effect triggers (must match expected for effect type).</param>
    /// <param name="magnitude">Effect strength/magnitude.</param>
    /// <param name="damageTypeId">Optional damage type for elemental effects (required for elemental).</param>
    /// <param name="description">Optional player-facing description (auto-generated if not provided).</param>
    /// <param name="logger">Optional logger for logging effect creation.</param>
    /// <returns>A validated SpecialEffect instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when effectId is null or whitespace, effectType is None, trigger type doesn't match,
    /// or elemental effect is missing damage type.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when magnitude is out of valid range for the effect type.
    /// </exception>
    /// <example>
    /// <code>
    /// // Create a life steal effect
    /// var lifeSteal = SpecialEffect.Create(
    ///     "life-steal",
    ///     SpecialEffectType.LifeSteal,
    ///     EffectTriggerType.OnDamageDealt,
    ///     0.15m,
    ///     description: "Heals 15% of damage dealt");
    ///
    /// // Create an elemental damage effect
    /// var fireDamage = SpecialEffect.Create(
    ///     "fire-damage",
    ///     SpecialEffectType.FireDamage,
    ///     EffectTriggerType.OnHit,
    ///     10m,
    ///     damageTypeId: "fire");
    /// </code>
    /// </example>
    public static SpecialEffect Create(
        string effectId,
        SpecialEffectType effectType,
        EffectTriggerType triggerType,
        decimal magnitude,
        string? damageTypeId = null,
        string? description = null,
        ILogger? logger = null)
    {
        // Validate effect ID
        ArgumentException.ThrowIfNullOrWhiteSpace(effectId, nameof(effectId));

        // Validate effect type is not None
        if (effectType == SpecialEffectType.None)
        {
            logger?.LogWarning(
                "SpecialEffect validation failed for {EffectId}: Cannot create effect with None type",
                effectId);

            throw new ArgumentException(
                "Cannot create effect with None type.",
                nameof(effectType));
        }

        // Validate magnitude based on effect type
        ValidateMagnitude(effectType, magnitude, effectId, logger);

        // Validate damage type for elemental effects
        ValidateDamageType(effectType, damageTypeId, effectId, logger);

        // Validate trigger type matches expected for effect type
        ValidateTriggerType(effectType, triggerType, effectId, logger);

        var normalizedEffectId = effectId.ToLowerInvariant();
        var normalizedDamageTypeId = damageTypeId?.ToLowerInvariant();
        var effectDescription = description ?? GenerateDefaultDescription(effectType, magnitude, damageTypeId);

        logger?.LogDebug(
            "Created SpecialEffect {EffectId}: {EffectType} @ {TriggerType}, Magnitude: {Magnitude}",
            normalizedEffectId,
            effectType,
            triggerType,
            magnitude);

        return new SpecialEffect(
            normalizedEffectId,
            effectType,
            triggerType,
            magnitude,
            normalizedDamageTypeId,
            effectDescription);
    }

    /// <summary>
    /// Validates magnitude is appropriate for the effect type.
    /// </summary>
    /// <param name="effectType">The effect type to validate for.</param>
    /// <param name="magnitude">The magnitude value to validate.</param>
    /// <param name="effectId">The effect ID for logging purposes.</param>
    /// <param name="logger">Optional logger.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when magnitude is out of valid range for the effect type.
    /// </exception>
    private static void ValidateMagnitude(
        SpecialEffectType effectType,
        decimal magnitude,
        string effectId,
        ILogger? logger)
    {
        switch (effectType)
        {
            // Percentage effects must be 0.0-1.0
            case SpecialEffectType.LifeSteal:
            case SpecialEffectType.Reflect:
            case SpecialEffectType.CriticalBonus:
                if (magnitude < 0m)
                {
                    logger?.LogWarning(
                        "Invalid magnitude {Magnitude} for effect type {EffectType}: must be non-negative",
                        magnitude,
                        effectType);

                    ArgumentOutOfRangeException.ThrowIfNegative(magnitude, nameof(magnitude));
                }

                if (magnitude > 1.0m)
                {
                    logger?.LogWarning(
                        "Invalid magnitude {Magnitude} for effect type {EffectType}: must not exceed 1.0",
                        magnitude,
                        effectType);

                    ArgumentOutOfRangeException.ThrowIfGreaterThan(magnitude, 1.0m, nameof(magnitude));
                }

                break;

            // Flat values must be non-negative
            case SpecialEffectType.FireDamage:
            case SpecialEffectType.IceDamage:
            case SpecialEffectType.LightningDamage:
            case SpecialEffectType.DamageReduction:
            case SpecialEffectType.Slow:
                if (magnitude < 0m)
                {
                    logger?.LogWarning(
                        "Invalid magnitude {Magnitude} for effect type {EffectType}: must be non-negative",
                        magnitude,
                        effectType);

                    ArgumentOutOfRangeException.ThrowIfNegative(magnitude, nameof(magnitude));
                }

                break;

            // Binary effects (magnitude ignored but no validation needed)
            case SpecialEffectType.IgnoreArmor:
            case SpecialEffectType.Cleave:
            case SpecialEffectType.Phase:
            case SpecialEffectType.AutoHide:
            case SpecialEffectType.Detection:
            case SpecialEffectType.FearAura:
                // No validation needed - magnitude is informational
                break;
        }
    }

    /// <summary>
    /// Validates damage type is provided for elemental effects.
    /// </summary>
    /// <param name="effectType">The effect type to validate for.</param>
    /// <param name="damageTypeId">The damage type ID to validate.</param>
    /// <param name="effectId">The effect ID for logging purposes.</param>
    /// <param name="logger">Optional logger.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when an elemental effect is missing a damage type.
    /// </exception>
    private static void ValidateDamageType(
        SpecialEffectType effectType,
        string? damageTypeId,
        string effectId,
        ILogger? logger)
    {
        var isElemental = effectType is
            SpecialEffectType.FireDamage or
            SpecialEffectType.IceDamage or
            SpecialEffectType.LightningDamage;

        if (isElemental && string.IsNullOrWhiteSpace(damageTypeId))
        {
            logger?.LogWarning(
                "Missing damageTypeId for elemental effect {EffectId}: {EffectType} requires a damage type",
                effectId,
                effectType);

            throw new ArgumentException(
                $"DamageTypeId is required for {effectType} effects.",
                nameof(damageTypeId));
        }
    }

    /// <summary>
    /// Validates trigger type is appropriate for the effect type.
    /// </summary>
    /// <param name="effectType">The effect type to validate for.</param>
    /// <param name="triggerType">The trigger type to validate.</param>
    /// <param name="effectId">The effect ID for logging purposes.</param>
    /// <param name="logger">Optional logger.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when trigger type doesn't match the expected type for the effect.
    /// </exception>
    private static void ValidateTriggerType(
        SpecialEffectType effectType,
        EffectTriggerType triggerType,
        string effectId,
        ILogger? logger)
    {
        var expectedTrigger = GetExpectedTriggerType(effectType);

        if (triggerType != expectedTrigger)
        {
            logger?.LogWarning(
                "Trigger type mismatch for {EffectId}: expected {Expected}, got {Actual}",
                effectId,
                expectedTrigger,
                triggerType);

            throw new ArgumentException(
                $"Effect {effectType} expects trigger {expectedTrigger}, got {triggerType}.",
                nameof(triggerType));
        }
    }

    /// <summary>
    /// Gets the expected trigger type for an effect type.
    /// </summary>
    /// <param name="effectType">The effect type to get the expected trigger for.</param>
    /// <returns>The expected <see cref="EffectTriggerType"/> for the given effect type.</returns>
    /// <remarks>
    /// This method defines the canonical mapping between effect types and their triggers:
    /// <list type="bullet">
    ///   <item><description>OnAttack: IgnoreArmor, Cleave, Phase</description></item>
    ///   <item><description>OnHit: FireDamage, IceDamage, LightningDamage, Slow</description></item>
    ///   <item><description>OnDamageDealt: LifeSteal</description></item>
    ///   <item><description>OnDamageTaken: Reflect</description></item>
    ///   <item><description>OnKill: AutoHide</description></item>
    ///   <item><description>Passive: Detection, CriticalBonus, DamageReduction, FearAura</description></item>
    /// </list>
    /// </remarks>
    public static EffectTriggerType GetExpectedTriggerType(SpecialEffectType effectType) =>
        effectType switch
        {
            // OnAttack effects
            SpecialEffectType.IgnoreArmor => EffectTriggerType.OnAttack,
            SpecialEffectType.Cleave => EffectTriggerType.OnAttack,
            SpecialEffectType.Phase => EffectTriggerType.OnAttack,

            // OnHit effects
            SpecialEffectType.FireDamage => EffectTriggerType.OnHit,
            SpecialEffectType.IceDamage => EffectTriggerType.OnHit,
            SpecialEffectType.LightningDamage => EffectTriggerType.OnHit,
            SpecialEffectType.Slow => EffectTriggerType.OnHit,

            // OnDamageDealt effects
            SpecialEffectType.LifeSteal => EffectTriggerType.OnDamageDealt,

            // OnDamageTaken effects
            SpecialEffectType.Reflect => EffectTriggerType.OnDamageTaken,

            // OnKill effects
            SpecialEffectType.AutoHide => EffectTriggerType.OnKill,

            // Passive effects
            SpecialEffectType.Detection => EffectTriggerType.Passive,
            SpecialEffectType.CriticalBonus => EffectTriggerType.Passive,
            SpecialEffectType.DamageReduction => EffectTriggerType.Passive,
            SpecialEffectType.FearAura => EffectTriggerType.Passive,

            // Default to Passive for unknown/None types
            _ => EffectTriggerType.Passive
        };

    /// <summary>
    /// Generates a default description for an effect.
    /// </summary>
    /// <param name="effectType">The effect type.</param>
    /// <param name="magnitude">The effect magnitude.</param>
    /// <param name="damageTypeId">Optional damage type for elemental effects.</param>
    /// <returns>A player-facing description string.</returns>
    private static string GenerateDefaultDescription(
        SpecialEffectType effectType,
        decimal magnitude,
        string? damageTypeId) =>
        effectType switch
        {
            SpecialEffectType.IgnoreArmor => "Attacks ignore target's defense",
            SpecialEffectType.LifeSteal => $"Heals {magnitude:P0} of damage dealt",
            SpecialEffectType.Cleave => "Attacks hit adjacent enemies",
            SpecialEffectType.Phase => "Attacks cannot be blocked",
            SpecialEffectType.Reflect => $"Returns {magnitude:P0} of damage taken to attacker",
            SpecialEffectType.FireDamage => $"Deals {magnitude} fire damage on hit",
            SpecialEffectType.IceDamage => $"Deals {magnitude} ice damage on hit",
            SpecialEffectType.LightningDamage => $"Deals {magnitude} lightning damage on hit",
            SpecialEffectType.Slow => $"Slows target for {magnitude} seconds",
            SpecialEffectType.AutoHide => "Enter hidden state after killing an enemy",
            SpecialEffectType.Detection => "Reveals hidden enemies and traps",
            SpecialEffectType.CriticalBonus => $"+{magnitude:P0} critical hit chance",
            SpecialEffectType.DamageReduction => $"Reduces incoming damage by {magnitude}",
            SpecialEffectType.FearAura => "Intimidates nearby enemies",
            _ => "Unknown effect"
        };

    /// <summary>
    /// Checks if this effect has the specified trigger type.
    /// </summary>
    /// <param name="trigger">The trigger type to check.</param>
    /// <returns>True if this effect has the specified trigger type; otherwise, false.</returns>
    public bool HasTrigger(EffectTriggerType trigger) => TriggerType == trigger;

    /// <summary>
    /// Checks if this is a passive effect.
    /// </summary>
    /// <remarks>
    /// Passive effects are always active while the item is equipped.
    /// </remarks>
    public bool IsPassive => TriggerType == EffectTriggerType.Passive;

    /// <summary>
    /// Checks if this is an elemental damage effect.
    /// </summary>
    /// <remarks>
    /// Elemental effects include <see cref="SpecialEffectType.FireDamage"/>,
    /// <see cref="SpecialEffectType.IceDamage"/>, and <see cref="SpecialEffectType.LightningDamage"/>.
    /// </remarks>
    public bool IsElemental => EffectType is
        SpecialEffectType.FireDamage or
        SpecialEffectType.IceDamage or
        SpecialEffectType.LightningDamage;

    /// <summary>
    /// Gets a string representation of this special effect.
    /// </summary>
    /// <returns>A string containing the effect ID, type, trigger, and magnitude.</returns>
    public override string ToString() =>
        $"SpecialEffect[{EffectId}: {EffectType} @ {TriggerType}, Mag:{Magnitude}]";
}
