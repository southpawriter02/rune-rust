using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Defines a synergy effect that provides bonuses to monster group members (v0.10.4c).
/// </summary>
/// <remarks>
/// <para>
/// GroupSynergy represents combat bonuses that activate under specific conditions:
/// <list type="bullet">
///   <item><description><see cref="SynergyTrigger.Always"/> - Permanent while source alive</description></item>
///   <item><description><see cref="SynergyTrigger.OnAllyHit"/> - When group member lands attack</description></item>
///   <item><description><see cref="SynergyTrigger.OnAllyDamaged"/> - When group member takes damage</description></item>
///   <item><description><see cref="SynergyTrigger.PerAdjacentAlly"/> - Scales with adjacent allies</description></item>
///   <item><description><see cref="SynergyTrigger.OnLeaderCommand"/> - When leader uses command ability</description></item>
/// </list>
/// </para>
/// <para>
/// Effects can include:
/// <list type="bullet">
///   <item><description>Attack bonus via <see cref="AttackBonus"/></description></item>
///   <item><description>Damage bonus via <see cref="DamageBonus"/></description></item>
///   <item><description>Status effect application via <see cref="StatusEffectId"/></description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Shaman's passive blessing
/// var blessing = GroupSynergy.Create("shamans-blessing", "Shaman's Blessing", SynergyTrigger.Always)
///     .WithDescription("The shaman's presence bolsters allies")
///     .WithSourceRole("caster")
///     .WithAttackBonus(1);
///
/// // Pack tactics on successful hit
/// var packTactics = GroupSynergy.Create("pack-tactics", "Pack Tactics", SynergyTrigger.OnAllyHit)
///     .WithDescription("Landing a blow emboldens the pack")
///     .WithAttackBonus(2)
///     .WithAppliesToAllMembers(false); // Excludes attacker
/// </code>
/// </example>
public class GroupSynergy
{
    // ═══════════════════════════════════════════════════════════════
    // IDENTITY
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique identifier for this synergy.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used for referencing the synergy in logs and events.
    /// Format: lowercase with hyphens (e.g., "shamans-blessing", "pack-tactics").
    /// </para>
    /// </remarks>
    public string SynergyId { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the display name for this synergy.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Shown in combat UI and logs when the synergy activates.
    /// Examples: "Shaman's Blessing", "Pack Tactics", "Bloodlust"
    /// </para>
    /// </remarks>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the description text explaining the synergy effect.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Flavor text describing why/how the synergy works.
    /// Example: "The shaman's presence bolsters allies with spiritual energy."
    /// </para>
    /// </remarks>
    public string Description { get; private set; } = string.Empty;

    // ═══════════════════════════════════════════════════════════════
    // TRIGGER
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the condition that activates this synergy.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Determines when the synergy effect is evaluated and applied.
    /// See <see cref="SynergyTrigger"/> for available trigger types.
    /// </para>
    /// </remarks>
    public SynergyTrigger Trigger { get; private set; }

    /// <summary>
    /// Gets the role required to activate this synergy.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If specified, only group members with this role can trigger the synergy.
    /// For example, a "caster" source role means only the caster can provide
    /// the synergy bonus.
    /// </para>
    /// <para>
    /// For <see cref="SynergyTrigger.Always"/>, the synergy remains active
    /// while any member with the source role is alive.
    /// </para>
    /// </remarks>
    public string? SourceRole { get; private set; }

    // ═══════════════════════════════════════════════════════════════
    // EFFECTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the status effect ID to apply when synergy activates.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Optional reference to a status effect definition that should be
    /// applied to affected members. Applied via <c>IBuffDebuffService</c>.
    /// </para>
    /// </remarks>
    public string? StatusEffectId { get; private set; }

    /// <summary>
    /// Gets the attack roll bonus provided by this synergy.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Added to attack rolls for affected members. For <see cref="SynergyTrigger.PerAdjacentAlly"/>,
    /// this value is multiplied by the number of adjacent allies.
    /// </para>
    /// </remarks>
    public int AttackBonus { get; private set; }

    /// <summary>
    /// Gets the damage bonus provided by this synergy.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Added to damage dealt by affected members. For <see cref="SynergyTrigger.PerAdjacentAlly"/>,
    /// this value is multiplied by the number of adjacent allies.
    /// </para>
    /// </remarks>
    public int DamageBonus { get; private set; }

    /// <summary>
    /// Gets whether the synergy effect applies to all group members including the source.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When true, the source member (e.g., the attacker for OnAllyHit) also receives
    /// the bonus. When false, only other group members receive the effect.
    /// </para>
    /// <para>
    /// Default is true for <see cref="SynergyTrigger.Always"/> synergies,
    /// false for reactive triggers like <see cref="SynergyTrigger.OnAllyHit"/>.
    /// </para>
    /// </remarks>
    public bool AppliesToAllMembers { get; private set; } = true;

    // ═══════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this synergy requires a specific source role.
    /// </summary>
    public bool RequiresSourceRole => !string.IsNullOrWhiteSpace(SourceRole);

    /// <summary>
    /// Gets whether this synergy applies a status effect.
    /// </summary>
    public bool HasStatusEffect => !string.IsNullOrWhiteSpace(StatusEffectId);

    /// <summary>
    /// Gets whether this synergy provides an attack bonus.
    /// </summary>
    public bool HasAttackBonus => AttackBonus != 0;

    /// <summary>
    /// Gets whether this synergy provides a damage bonus.
    /// </summary>
    public bool HasDamageBonus => DamageBonus != 0;

    /// <summary>
    /// Gets whether this synergy provides any combat bonuses.
    /// </summary>
    public bool HasCombatBonuses => HasAttackBonus || HasDamageBonus || HasStatusEffect;

    /// <summary>
    /// Gets whether this is an always-active (passive) synergy.
    /// </summary>
    public bool IsPassive => Trigger == SynergyTrigger.Always;

    /// <summary>
    /// Gets whether this synergy scales with adjacent allies.
    /// </summary>
    public bool IsAdjacencyBased => Trigger == SynergyTrigger.PerAdjacentAlly;

    /// <summary>
    /// Gets whether this synergy configuration is valid.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A valid synergy has a non-empty ID and name, and provides at least one effect.
    /// </para>
    /// </remarks>
    public bool IsValid =>
        !string.IsNullOrWhiteSpace(SynergyId) &&
        !string.IsNullOrWhiteSpace(Name) &&
        HasCombatBonuses;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS & FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Private constructor for controlled creation.
    /// </summary>
    private GroupSynergy() { }

    /// <summary>
    /// Creates a new group synergy definition.
    /// </summary>
    /// <param name="synergyId">Unique identifier for the synergy.</param>
    /// <param name="name">Display name for the synergy.</param>
    /// <param name="trigger">The condition that activates this synergy.</param>
    /// <returns>A new <see cref="GroupSynergy"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="synergyId"/> or <paramref name="name"/> is null or whitespace.
    /// </exception>
    /// <example>
    /// <code>
    /// var synergy = GroupSynergy.Create("pack-tactics", "Pack Tactics", SynergyTrigger.OnAllyHit)
    ///     .WithAttackBonus(2);
    /// </code>
    /// </example>
    public static GroupSynergy Create(string synergyId, string name, SynergyTrigger trigger)
    {
        if (string.IsNullOrWhiteSpace(synergyId))
        {
            throw new ArgumentException("Synergy ID cannot be null or empty.", nameof(synergyId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        }

        return new GroupSynergy
        {
            SynergyId = synergyId.ToLowerInvariant(),
            Name = name,
            Trigger = trigger
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // FLUENT CONFIGURATION METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Sets the description text for this synergy.
    /// </summary>
    /// <param name="description">The description text.</param>
    /// <returns>This instance for method chaining.</returns>
    public GroupSynergy WithDescription(string description)
    {
        Description = description ?? string.Empty;
        return this;
    }

    /// <summary>
    /// Sets the source role required to trigger this synergy.
    /// </summary>
    /// <param name="role">The role that must be present (leader, melee, ranged, caster, striker).</param>
    /// <returns>This instance for method chaining.</returns>
    /// <example>
    /// <code>
    /// // Only casters can provide this buff
    /// synergy.WithSourceRole("caster");
    /// </code>
    /// </example>
    public GroupSynergy WithSourceRole(string role)
    {
        SourceRole = string.IsNullOrWhiteSpace(role) ? null : role.ToLowerInvariant();
        return this;
    }

    /// <summary>
    /// Sets the status effect to apply when synergy activates.
    /// </summary>
    /// <param name="statusEffectId">The status effect definition ID.</param>
    /// <returns>This instance for method chaining.</returns>
    public GroupSynergy WithStatusEffect(string statusEffectId)
    {
        StatusEffectId = string.IsNullOrWhiteSpace(statusEffectId) ? null : statusEffectId.ToLowerInvariant();
        return this;
    }

    /// <summary>
    /// Sets the attack roll bonus for this synergy.
    /// </summary>
    /// <param name="bonus">The bonus to attack rolls.</param>
    /// <returns>This instance for method chaining.</returns>
    public GroupSynergy WithAttackBonus(int bonus)
    {
        AttackBonus = bonus;
        return this;
    }

    /// <summary>
    /// Sets the damage bonus for this synergy.
    /// </summary>
    /// <param name="bonus">The bonus to damage dealt.</param>
    /// <returns>This instance for method chaining.</returns>
    public GroupSynergy WithDamageBonus(int bonus)
    {
        DamageBonus = bonus;
        return this;
    }

    /// <summary>
    /// Sets both attack and damage bonuses for this synergy.
    /// </summary>
    /// <param name="attackBonus">The bonus to attack rolls.</param>
    /// <param name="damageBonus">The bonus to damage dealt.</param>
    /// <returns>This instance for method chaining.</returns>
    public GroupSynergy WithBonuses(int attackBonus, int damageBonus)
    {
        AttackBonus = attackBonus;
        DamageBonus = damageBonus;
        return this;
    }

    /// <summary>
    /// Sets whether the synergy applies to all members including the source.
    /// </summary>
    /// <param name="appliesTo">True to include source in effect; false to exclude.</param>
    /// <returns>This instance for method chaining.</returns>
    public GroupSynergy WithAppliesToAllMembers(bool appliesTo)
    {
        AppliesToAllMembers = appliesTo;
        return this;
    }

    // ═══════════════════════════════════════════════════════════════
    // UTILITY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public override string ToString()
    {
        var effects = new List<string>();
        if (HasAttackBonus) effects.Add($"+{AttackBonus} atk");
        if (HasDamageBonus) effects.Add($"+{DamageBonus} dmg");
        if (HasStatusEffect) effects.Add(StatusEffectId!);

        var effectText = effects.Count > 0 ? string.Join(", ", effects) : "no effect";
        return $"{Name} ({Trigger}): {effectText}";
    }
}
