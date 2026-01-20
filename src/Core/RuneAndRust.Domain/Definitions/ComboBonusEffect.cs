using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Represents a bonus effect applied when a combo completes successfully.
/// </summary>
/// <remarks>
/// <para>ComboBonusEffect defines rewards granted to players for executing ability combos:</para>
/// <list type="bullet">
///   <item><description><see cref="EffectType"/> - The type of bonus (damage, heal, status, etc.)</description></item>
///   <item><description><see cref="Value"/> - The magnitude or identifier for the effect</description></item>
///   <item><description><see cref="DamageType"/> - Optional damage type for damage effects</description></item>
///   <item><description><see cref="StatusEffectId"/> - Optional status effect ID for ApplyStatus type</description></item>
///   <item><description><see cref="Target"/> - Who receives the bonus effect</description></item>
/// </list>
/// <para>
/// Multiple bonus effects can be attached to a single combo, allowing for complex rewards
/// like "deal extra damage AND apply a status effect AND heal the caster."
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create a damage multiplier bonus
/// var damageBonus = new ComboBonusEffect
/// {
///     EffectType = ComboBonusType.DamageMultiplier,
///     Value = "2.0",
///     Target = ComboBonusTarget.LastTarget
/// };
///
/// // Create a status effect bonus
/// var statusBonus = new ComboBonusEffect
/// {
///     EffectType = ComboBonusType.ApplyStatus,
///     StatusEffectId = "stunned",
///     Value = "",
///     Target = ComboBonusTarget.LastTarget
/// };
///
/// // Create a self-heal bonus
/// var healBonus = new ComboBonusEffect
/// {
///     EffectType = ComboBonusType.Heal,
///     Value = "2d8",
///     Target = ComboBonusTarget.Self
/// };
/// </code>
/// </example>
public class ComboBonusEffect
{
    // ===== Properties =====

    /// <summary>
    /// Gets or sets the type of bonus effect.
    /// </summary>
    /// <remarks>
    /// <para>Determines how <see cref="Value"/> is interpreted:</para>
    /// <list type="bullet">
    ///   <item><description>ExtraDamage: Dice notation (e.g., "4d6")</description></item>
    ///   <item><description>DamageMultiplier: Decimal (e.g., "2.0")</description></item>
    ///   <item><description>ApplyStatus: Empty (uses <see cref="StatusEffectId"/>)</description></item>
    ///   <item><description>Heal: Dice notation (e.g., "2d8")</description></item>
    ///   <item><description>ResetCooldown: Ability ID (e.g., "vanish")</description></item>
    ///   <item><description>RefundResource: Percentage or flat amount</description></item>
    ///   <item><description>AreaEffect: Radius in cells (e.g., "2")</description></item>
    /// </list>
    /// </remarks>
    public ComboBonusType EffectType { get; set; }

    /// <summary>
    /// Gets or sets the value for the effect.
    /// </summary>
    /// <remarks>
    /// <para>Interpretation depends on <see cref="EffectType"/>.</para>
    /// <para>For ApplyStatus, this is typically empty - use <see cref="StatusEffectId"/> instead.</para>
    /// </remarks>
    public string Value { get; set; } = null!;

    /// <summary>
    /// Gets or sets the damage type for damage effects.
    /// </summary>
    /// <remarks>
    /// <para>Only relevant for <see cref="ComboBonusType.ExtraDamage"/>.</para>
    /// <para>Common values: "physical", "fire", "ice", "lightning", "piercing", etc.</para>
    /// <para>If null, defaults to "physical" damage.</para>
    /// </remarks>
    public string? DamageType { get; set; }

    /// <summary>
    /// Gets or sets the status effect ID for ApplyStatus effects.
    /// </summary>
    /// <remarks>
    /// <para>Only relevant for <see cref="ComboBonusType.ApplyStatus"/>.</para>
    /// <para>Must match an existing status effect ID in the status effect system.</para>
    /// <para>Examples: "stunned", "bleeding", "poisoned", "elemental-overload".</para>
    /// </remarks>
    public string? StatusEffectId { get; set; }

    /// <summary>
    /// Gets or sets the target for this bonus effect.
    /// </summary>
    /// <remarks>
    /// <para>Defaults to <see cref="ComboBonusTarget.LastTarget"/>.</para>
    /// <para>Determines who receives the effect when the combo completes.</para>
    /// </remarks>
    public ComboBonusTarget Target { get; set; } = ComboBonusTarget.LastTarget;

    // ===== Methods =====

    /// <summary>
    /// Gets a human-readable description of this effect.
    /// </summary>
    /// <returns>A description string suitable for display to players.</returns>
    /// <remarks>
    /// <para>Generated descriptions are localized based on effect type:</para>
    /// <list type="bullet">
    ///   <item><description>ExtraDamage: "+4d6 piercing damage"</description></item>
    ///   <item><description>DamageMultiplier: "x2.0 damage"</description></item>
    ///   <item><description>ApplyStatus: "Apply stunned"</description></item>
    ///   <item><description>Heal: "Heal 2d8"</description></item>
    ///   <item><description>ResetCooldown: "Reset vanish cooldown"</description></item>
    ///   <item><description>RefundResource: "Refund 50 resource"</description></item>
    ///   <item><description>AreaEffect: "Expand to 2 cell radius"</description></item>
    /// </list>
    /// </remarks>
    public string GetDescription()
    {
        return EffectType switch
        {
            ComboBonusType.ExtraDamage => $"+{Value} {DamageType ?? "physical"} damage",
            ComboBonusType.DamageMultiplier => $"x{Value} damage",
            ComboBonusType.ApplyStatus => $"Apply {StatusEffectId}",
            ComboBonusType.Heal => $"Heal {Value}",
            ComboBonusType.ResetCooldown => $"Reset {Value} cooldown",
            ComboBonusType.RefundResource => $"Refund {Value} resource",
            ComboBonusType.AreaEffect => $"Expand to {Value} cell radius",
            _ => Value
        };
    }

    /// <summary>
    /// Returns a string representation of this bonus effect.
    /// </summary>
    /// <returns>The effect description.</returns>
    public override string ToString() => GetDescription();
}
