namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Types of defensive combat actions available during combat.
/// </summary>
/// <remarks>
/// <para>Defense actions allow combatants to react to incoming attacks:</para>
/// <list type="bullet">
///   <item><description><see cref="Block"/> - Reduces damage using a shield</description></item>
///   <item><description><see cref="Dodge"/> - Attempts to avoid the attack entirely</description></item>
///   <item><description><see cref="Parry"/> - Deflects the attack and counter-attacks</description></item>
/// </list>
/// <para>
/// <see cref="Dodge"/> and <see cref="Parry"/> consume the combatant's reaction for the round,
/// while <see cref="Block"/> can be used as either an action or reaction.
/// </para>
/// </remarks>
public enum DefenseActionType
{
    /// <summary>
    /// Block incoming damage with a shield.
    /// </summary>
    /// <remarks>
    /// <para>Requirements: Shield equipped</para>
    /// <para>Effect: Reduces incoming damage by 50% plus shield defense bonus</para>
    /// <para>Reaction cost: None (can be used as action or reaction)</para>
    /// <para>Cooldown: None</para>
    /// </remarks>
    Block,

    /// <summary>
    /// Attempt to dodge an attack entirely.
    /// </summary>
    /// <remarks>
    /// <para>Requirements: Reaction available, not wearing heavy armor</para>
    /// <para>Effect: DEX-based roll vs attack roll; avoids attack on success</para>
    /// <para>Reaction cost: 1</para>
    /// <para>Cooldown: Once per round</para>
    /// </remarks>
    Dodge,

    /// <summary>
    /// Deflect an attack and counter-attack.
    /// </summary>
    /// <remarks>
    /// <para>Requirements: Reaction available, melee weapon equipped</para>
    /// <para>Effect: DEX-based roll vs attack roll + 2; deflects and counter-attacks on success</para>
    /// <para>Reaction cost: 1</para>
    /// <para>Cooldown: Once per round</para>
    /// </remarks>
    Parry
}
