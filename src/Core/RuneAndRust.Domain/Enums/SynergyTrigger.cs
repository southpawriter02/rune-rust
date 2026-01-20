namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines when synergy effects activate within a monster group (v0.10.4c).
/// </summary>
/// <remarks>
/// <para>
/// Synergy triggers determine when <see cref="Definitions.GroupSynergy"/> effects
/// are applied to group members. Different triggers provide bonuses under
/// varying combat conditions.
/// </para>
/// <para>
/// Trigger evaluation occurs at these points:
/// <list type="bullet">
///   <item><description><see cref="Always"/> - On group registration/spawn</description></item>
///   <item><description><see cref="OnAllyHit"/> - When <c>OnGroupMemberHit()</c> is called</description></item>
///   <item><description><see cref="OnAllyDamaged"/> - When <c>OnGroupMemberDamaged()</c> is called</description></item>
///   <item><description><see cref="PerAdjacentAlly"/> - Per-attack calculation</description></item>
///   <item><description><see cref="OnLeaderCommand"/> - When leader uses command ability</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Define a synergy that provides a passive bonus
/// var passiveSynergy = GroupSynergy.Create("shamans-blessing", "Shaman's Blessing", SynergyTrigger.Always)
///     .WithSourceRole("caster")
///     .WithAttackBonus(1);
///
/// // Define a synergy that triggers when an ally lands a hit
/// var packTactics = GroupSynergy.Create("pack-tactics", "Pack Tactics", SynergyTrigger.OnAllyHit)
///     .WithAttackBonus(2);
/// </code>
/// </example>
public enum SynergyTrigger
{
    /// <summary>
    /// Synergy is always active while the source condition is met.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Applied immediately when the group is registered/spawned and remains
    /// active as long as the source role member is alive.
    /// </para>
    /// <para>
    /// Example: A shaman providing a constant +1 attack buff to all group members.
    /// </para>
    /// </remarks>
    Always = 0,

    /// <summary>
    /// Synergy activates when a group member lands a successful attack.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Triggered by <c>IMonsterGroupService.OnGroupMemberHit(attacker, target)</c>.
    /// Typically provides a temporary bonus (1 turn) to other group members.
    /// </para>
    /// <para>
    /// Example: "Pack Tactics" - When any goblin lands a hit, other goblins
    /// get +2 attack bonus for 1 turn.
    /// </para>
    /// </remarks>
    OnAllyHit = 1,

    /// <summary>
    /// Synergy activates when a group member takes damage.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Triggered by <c>IMonsterGroupService.OnGroupMemberDamaged(target, damage)</c>.
    /// Often used for defensive or retaliatory synergies.
    /// </para>
    /// <para>
    /// Example: "Rage" - When an orc takes damage, other orcs gain
    /// +3 damage bonus for 2 turns.
    /// </para>
    /// </remarks>
    OnAllyDamaged = 2,

    /// <summary>
    /// Synergy bonus scales with the number of adjacent group allies.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Calculated per-attack via <c>IMonsterGroupService.GetAdjacentAllyBonus()</c>.
    /// The bonus value in <see cref="Definitions.GroupSynergy"/> is multiplied
    /// by the count of adjacent allies.
    /// </para>
    /// <para>
    /// Example: "Pack Mentality" - +1 damage per adjacent ally.
    /// With 3 adjacent allies, the attacker deals +3 damage.
    /// </para>
    /// </remarks>
    PerAdjacentAlly = 3,

    /// <summary>
    /// Synergy activates when the group leader uses a command ability.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Triggered when the monster with the leader role uses a designated
    /// command ability. Provides a group-wide buff or tactical effect.
    /// </para>
    /// <para>
    /// Example: "Rally" - When the orc chief uses "War Cry", all orcs
    /// gain +2 attack and +2 damage for 3 turns.
    /// </para>
    /// </remarks>
    OnLeaderCommand = 4
}
