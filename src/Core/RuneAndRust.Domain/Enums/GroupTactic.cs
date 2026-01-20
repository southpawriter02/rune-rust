namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines tactical behaviors for coordinated monster groups (v0.10.4c).
/// </summary>
/// <remarks>
/// <para>
/// Group tactics determine how members of a <see cref="Definitions.MonsterGroupDefinition"/>
/// coordinate their movement and targeting during combat. Tactics are evaluated in
/// priority order defined by the group definition.
/// </para>
/// <para>
/// The monster group service (IMonsterGroupService) uses these tactics to:
/// <list type="bullet">
///   <item><description>Calculate optimal movement positions via <c>DetermineMove()</c></description></item>
///   <item><description>Select coordinated targets via <c>DetermineTarget()</c></description></item>
///   <item><description>Apply positioning bonuses (flanking, protection)</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // A goblin warband using multiple tactics in priority order
/// var tactics = new[] { GroupTactic.Flank, GroupTactic.FocusFire, GroupTactic.ProtectLeader };
///
/// // Evaluation order:
/// // 1. Try to get flanking positions around target
/// // 2. If no flanking available, focus fire on weakest target
/// // 3. If leader threatened, protect the leader
/// </code>
/// </example>
public enum GroupTactic
{
    /// <summary>
    /// Coordinate to attack the target from opposite sides for flanking bonuses.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Members attempt to position themselves on opposite sides of the target
    /// to gain flanking attack bonuses. Uses <c>IFlankingService.GetFlankingPositions()</c>
    /// to calculate optimal positions.
    /// </para>
    /// <para>
    /// Flanking bonuses are applied when at least two group members are positioned
    /// on opposite sides of the target.
    /// </para>
    /// </remarks>
    Flank = 0,

    /// <summary>
    /// All group members attack the same target, typically the weakest enemy.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The group selects a shared target (usually lowest HP) and all members
    /// focus their attacks on that target until it is defeated.
    /// </para>
    /// <para>
    /// The current target is tracked in <c>MonsterGroupInstance.CurrentTarget</c>
    /// and persists until the target dies or becomes invalid.
    /// </para>
    /// </remarks>
    FocusFire = 1,

    /// <summary>
    /// Position to shield and defend the group leader from attacks.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Non-leader members position themselves between the leader and enemies,
    /// forming a protective barrier. The leader role is defined by
    /// <see cref="Definitions.MonsterGroupDefinition.LeaderRole"/>.
    /// </para>
    /// <para>
    /// This tactic is skipped for the leader monster itself.
    /// </para>
    /// </remarks>
    ProtectLeader = 2,

    /// <summary>
    /// Position to shield and defend spellcasters in the group.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Similar to <see cref="ProtectLeader"/>, but protects members with
    /// the "caster" role instead of the designated leader.
    /// </para>
    /// <para>
    /// Useful for groups with valuable ranged attackers or healers that
    /// need protection from melee threats.
    /// </para>
    /// </remarks>
    ProtectCaster = 3,

    /// <summary>
    /// Cluster around the target from all available adjacent positions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Members move to surround the target, filling all available adjacent
    /// cells. Unlike <see cref="Flank"/>, Swarm does not require opposite
    /// positioning and simply maximizes adjacent presence.
    /// </para>
    /// <para>
    /// Effective for overwhelming a single target with numbers.
    /// </para>
    /// </remarks>
    Swarm = 4,

    /// <summary>
    /// Fall back and retreat when group health is low.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Members move away from threats when the group or individual health
    /// drops below a threshold. Typically combined with other tactics that
    /// activate when health is higher.
    /// </para>
    /// <para>
    /// Retreat positions are calculated based on threat direction.
    /// </para>
    /// </remarks>
    Retreat = 5,

    /// <summary>
    /// Wait in hiding until targets enter optimal attack range.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Members hold their positions until an enemy enters a trigger range,
    /// then spring the ambush. Returns <c>GroupMoveDecision.HoldPosition()</c>
    /// until conditions are met.
    /// </para>
    /// <para>
    /// Often paired with synergies that provide first-strike bonuses.
    /// </para>
    /// </remarks>
    Ambush = 6,

    /// <summary>
    /// Attack and immediately disengage to a safer position.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Members move in to attack, then retreat after their attack action
    /// to minimize counter-attack opportunities. Effective for ranged
    /// attackers or fast melee units.
    /// </para>
    /// <para>
    /// The disengage position is calculated to maximize distance from
    /// the nearest threat while staying within engagement range.
    /// </para>
    /// </remarks>
    HitAndRun = 7
}
