// ═══════════════════════════════════════════════════════════════════════════════
// LineageTrait.cs
// Value object representing a unique trait granted by a lineage.
// Version: 0.17.0c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents a unique trait granted by a lineage.
/// </summary>
/// <remarks>
/// <para>
/// LineageTrait defines a signature ability unique to each bloodline. Unlike
/// passive bonuses which are always active, traits have conditions that
/// determine when they apply.
/// </para>
/// <para>
/// Traits use one of four effect types:
/// <list type="bullet">
///   <item><description>BonusDiceToSkill: Adds dice to skill checks (e.g., Rhetoric)</description></item>
///   <item><description>PercentageModifier: Scales values (e.g., Stress reduction)</description></item>
///   <item><description>BonusDiceToResolve: Adds dice to resolve checks (e.g., vs hazards)</description></item>
///   <item><description>PassiveAuraBonus: Percentage increase to pools (e.g., Max AP)</description></item>
/// </list>
/// </para>
/// <para>
/// Each lineage has exactly one unique trait that provides a signature ability:
/// <list type="bullet">
///   <item><description>Clan-Born: [Survivor's Resolve] - +1d10 to Rhetoric with Clan-Born NPCs</description></item>
///   <item><description>Rune-Marked: [Aether-Tainted] - +10% Maximum Aether Pool</description></item>
///   <item><description>Iron-Blooded: [Hazard Acclimation] - +1d10 vs environmental hazards</description></item>
///   <item><description>Vargr-Kin: [Primal Clarity] - -10% Psychic Stress from all sources</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="TraitId">Kebab-case unique identifier (e.g., "survivors_resolve").</param>
/// <param name="TraitName">Display name with brackets (e.g., "[Survivor's Resolve]").</param>
/// <param name="Description">Player-facing explanation of the trait effect.</param>
/// <param name="EffectType">The type of effect this trait applies.</param>
/// <param name="TriggerCondition">Condition string for when trait activates (e.g., "rhetoric_check_initiated").</param>
/// <param name="BonusDice">Number of bonus dice (for dice-based effects).</param>
/// <param name="PercentModifier">Percentage modifier (for percentage effects, e.g., 0.10 for +10%).</param>
/// <param name="TargetCheck">Skill or resolve check type affected (e.g., "rhetoric", "sturdiness").</param>
/// <param name="TargetCondition">Condition for valid targets (e.g., "npc.lineage == ClanBorn").</param>
/// <seealso cref="LineageTraitEffectType"/>
/// <seealso cref="Entities.LineageDefinition"/>
public readonly record struct LineageTrait(
    string TraitId,
    string TraitName,
    string Description,
    LineageTraitEffectType EffectType,
    string? TriggerCondition,
    int? BonusDice,
    float? PercentModifier,
    string? TargetCheck,
    string? TargetCondition)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Logger for detailed diagnostic output during trait creation.
    /// </summary>
    private static ILogger<LineageTrait>? _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this trait uses bonus dice.
    /// </summary>
    /// <value>
    /// <c>true</c> if the effect type is <see cref="LineageTraitEffectType.BonusDiceToSkill"/>
    /// or <see cref="LineageTraitEffectType.BonusDiceToResolve"/>; otherwise, <c>false</c>.
    /// </value>
    public bool UsesBonusDice =>
        EffectType is LineageTraitEffectType.BonusDiceToSkill
                   or LineageTraitEffectType.BonusDiceToResolve;

    /// <summary>
    /// Gets whether this trait uses percentage modification.
    /// </summary>
    /// <value>
    /// <c>true</c> if the effect type is <see cref="LineageTraitEffectType.PercentageModifier"/>
    /// or <see cref="LineageTraitEffectType.PassiveAuraBonus"/>; otherwise, <c>false</c>.
    /// </value>
    public bool UsesPercentModifier =>
        EffectType is LineageTraitEffectType.PercentageModifier
                   or LineageTraitEffectType.PassiveAuraBonus;

    /// <summary>
    /// Gets whether this trait has a target condition.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="TargetCondition"/> is set and not empty; otherwise, <c>false</c>.
    /// </value>
    public bool HasTargetCondition => !string.IsNullOrWhiteSpace(TargetCondition);

    /// <summary>
    /// Gets whether this trait has a trigger condition.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="TriggerCondition"/> is set and not empty; otherwise, <c>false</c>.
    /// </value>
    public bool HasTriggerCondition => !string.IsNullOrWhiteSpace(TriggerCondition);

    /// <summary>
    /// Gets the effective bonus dice count.
    /// </summary>
    /// <value>
    /// The <see cref="BonusDice"/> value if set; otherwise, 0.
    /// </value>
    public int EffectiveBonusDice => BonusDice ?? 0;

    /// <summary>
    /// Gets the effective percent modifier.
    /// </summary>
    /// <value>
    /// The <see cref="PercentModifier"/> value if set; otherwise, 0.
    /// </value>
    public float EffectivePercentModifier => PercentModifier ?? 0f;

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHOD
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new <see cref="LineageTrait"/> with validation.
    /// </summary>
    /// <param name="traitId">Kebab-case unique identifier.</param>
    /// <param name="traitName">Display name with brackets.</param>
    /// <param name="description">Player-facing effect description.</param>
    /// <param name="effectType">The type of effect this trait applies.</param>
    /// <param name="triggerCondition">Optional condition string for when trait activates.</param>
    /// <param name="bonusDice">Optional number of bonus dice (required for dice effects).</param>
    /// <param name="percentModifier">Optional percentage modifier (required for percentage effects).</param>
    /// <param name="targetCheck">Optional skill or resolve check type affected.</param>
    /// <param name="targetCondition">Optional condition for valid targets.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <returns>A new validated <see cref="LineageTrait"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="traitId"/>, <paramref name="traitName"/>, or
    /// <paramref name="description"/> is null or whitespace.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="effectType"/> requires BonusDice but it is not provided
    /// or is not positive.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="effectType"/> requires PercentModifier but it is not provided.
    /// </exception>
    /// <example>
    /// <code>
    /// var trait = LineageTrait.Create(
    ///     traitId: "survivors_resolve",
    ///     traitName: "[Survivor's Resolve]",
    ///     description: "Gain +1d10 to Rhetoric checks when interacting with Clan-Born NPCs.",
    ///     effectType: LineageTraitEffectType.BonusDiceToSkill,
    ///     triggerCondition: "rhetoric_check_initiated",
    ///     bonusDice: 1,
    ///     targetCheck: "rhetoric",
    ///     targetCondition: "npc.lineage == ClanBorn"
    /// );
    /// </code>
    /// </example>
    public static LineageTrait Create(
        string traitId,
        string traitName,
        string description,
        LineageTraitEffectType effectType,
        string? triggerCondition = null,
        int? bonusDice = null,
        float? percentModifier = null,
        string? targetCheck = null,
        string? targetCondition = null,
        ILogger<LineageTrait>? logger = null)
    {
        // Store logger for this creation context
        _logger = logger;

        _logger?.LogDebug(
            "Creating LineageTrait '{TraitId}' with effect type {EffectType}",
            traitId,
            effectType);

        // Validate required string parameters
        ArgumentException.ThrowIfNullOrWhiteSpace(traitId, nameof(traitId));
        ArgumentException.ThrowIfNullOrWhiteSpace(traitName, nameof(traitName));
        ArgumentException.ThrowIfNullOrWhiteSpace(description, nameof(description));

        // Validate effect type has required properties
        if (effectType is LineageTraitEffectType.BonusDiceToSkill
                       or LineageTraitEffectType.BonusDiceToResolve)
        {
            if (!bonusDice.HasValue || bonusDice.Value <= 0)
            {
                _logger?.LogWarning(
                    "Trait creation failed: Effect type {EffectType} requires a positive BonusDice value. " +
                    "TraitId={TraitId}, BonusDice={BonusDice}",
                    effectType,
                    traitId,
                    bonusDice);

                throw new ArgumentException(
                    $"Effect type {effectType} requires a positive BonusDice value.",
                    nameof(bonusDice));
            }

            _logger?.LogDebug(
                "Validated BonusDice effect: TraitId={TraitId}, BonusDice={BonusDice}",
                traitId,
                bonusDice.Value);
        }

        if (effectType is LineageTraitEffectType.PercentageModifier
                       or LineageTraitEffectType.PassiveAuraBonus)
        {
            if (!percentModifier.HasValue)
            {
                _logger?.LogWarning(
                    "Trait creation failed: Effect type {EffectType} requires a PercentModifier value. " +
                    "TraitId={TraitId}",
                    effectType,
                    traitId);

                throw new ArgumentException(
                    $"Effect type {effectType} requires a PercentModifier value.",
                    nameof(percentModifier));
            }

            _logger?.LogDebug(
                "Validated PercentModifier effect: TraitId={TraitId}, PercentModifier={PercentModifier:P1}",
                traitId,
                percentModifier.Value);
        }

        var trait = new LineageTrait(
            traitId.ToLowerInvariant(),
            traitName,
            description,
            effectType,
            triggerCondition,
            bonusDice,
            percentModifier,
            targetCheck?.ToLowerInvariant(),
            targetCondition);

        _logger?.LogInformation(
            "Created LineageTrait '{TraitName}' (ID: {TraitId}). EffectType={EffectType}, " +
            "UsesBonusDice={UsesBonusDice}, UsesPercentModifier={UsesPercentModifier}, " +
            "HasTargetCondition={HasTargetCondition}",
            trait.TraitName,
            trait.TraitId,
            trait.EffectType,
            trait.UsesBonusDice,
            trait.UsesPercentModifier,
            trait.HasTargetCondition);

        return trait;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STATIC TRAIT PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the default trait for Clan-Born lineage: Survivor's Resolve.
    /// </summary>
    /// <value>
    /// A trait that grants +1d10 to Rhetoric checks when interacting with Clan-Born NPCs.
    /// </value>
    /// <remarks>
    /// Effect Type: <see cref="LineageTraitEffectType.BonusDiceToSkill"/>
    /// </remarks>
    public static LineageTrait SurvivorsResolve => Create(
        traitId: "survivors_resolve",
        traitName: "[Survivor's Resolve]",
        description: "Gain +1d10 to Rhetoric checks when interacting with Clan-Born NPCs.",
        effectType: LineageTraitEffectType.BonusDiceToSkill,
        triggerCondition: "rhetoric_check_initiated",
        bonusDice: 1,
        targetCheck: "rhetoric",
        targetCondition: "npc.lineage == ClanBorn");

    /// <summary>
    /// Gets the default trait for Rune-Marked lineage: Aether-Tainted.
    /// </summary>
    /// <value>
    /// A trait that increases Maximum Aether Pool by 10%.
    /// </value>
    /// <remarks>
    /// Effect Type: <see cref="LineageTraitEffectType.PassiveAuraBonus"/>
    /// </remarks>
    public static LineageTrait AetherTainted => Create(
        traitId: "aether_tainted",
        traitName: "[Aether-Tainted]",
        description: "Your Maximum Aether Pool is increased by 10%.",
        effectType: LineageTraitEffectType.PassiveAuraBonus,
        triggerCondition: "max_ap_calculation",
        percentModifier: 0.10f);

    /// <summary>
    /// Gets the default trait for Iron-Blooded lineage: Hazard Acclimation.
    /// </summary>
    /// <value>
    /// A trait that grants +1d10 to Sturdiness Resolve checks versus environmental hazards.
    /// </value>
    /// <remarks>
    /// Effect Type: <see cref="LineageTraitEffectType.BonusDiceToResolve"/>
    /// </remarks>
    public static LineageTrait HazardAcclimation => Create(
        traitId: "hazard_acclimation",
        traitName: "[Hazard Acclimation]",
        description: "Gain +1d10 to Sturdiness Resolve checks versus environmental hazards.",
        effectType: LineageTraitEffectType.BonusDiceToResolve,
        triggerCondition: "sturdiness_resolve_check",
        bonusDice: 1,
        targetCheck: "sturdiness",
        targetCondition: "source.type == environmental_hazard");

    /// <summary>
    /// Gets the default trait for Vargr-Kin lineage: Primal Clarity.
    /// </summary>
    /// <value>
    /// A trait that reduces Psychic Stress from all sources by 10%.
    /// </value>
    /// <remarks>
    /// Effect Type: <see cref="LineageTraitEffectType.PercentageModifier"/>
    /// </remarks>
    public static LineageTrait PrimalClarity => Create(
        traitId: "primal_clarity",
        traitName: "[Primal Clarity]",
        description: "Take 10% less Psychic Stress from all sources.",
        effectType: LineageTraitEffectType.PercentageModifier,
        triggerCondition: "psychic_stress_gain",
        percentModifier: -0.10f);

    /// <summary>
    /// Gets a default empty trait representing no unique ability.
    /// </summary>
    /// <value>
    /// A trait with empty values, useful for placeholder or uninitialized states.
    /// </value>
    public static LineageTrait None => new(
        TraitId: string.Empty,
        TraitName: string.Empty,
        Description: string.Empty,
        EffectType: LineageTraitEffectType.BonusDiceToSkill,
        TriggerCondition: null,
        BonusDice: null,
        PercentModifier: null,
        TargetCheck: null,
        TargetCondition: null);

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation of this lineage trait.
    /// </summary>
    /// <returns>
    /// A formatted string containing the trait name and description.
    /// </returns>
    public override string ToString() =>
        string.IsNullOrEmpty(TraitName)
            ? "No unique trait"
            : $"{TraitName}: {Description}";
}
