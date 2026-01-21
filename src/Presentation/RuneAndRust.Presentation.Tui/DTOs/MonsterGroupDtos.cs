namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// Data transfer object containing monster group display information.
/// </summary>
/// <param name="GroupId">The unique identifier for the group instance.</param>
/// <param name="GroupName">The display name of the group (e.g., "Goblin Warband").</param>
/// <param name="Members">The list of group member display data.</param>
/// <param name="CurrentTactic">The current tactic being used, null if no tactic.</param>
/// <param name="HasLeader">Whether the group has a living leader.</param>
/// <param name="LeaderRole">The role designation of the leader (e.g., "Chief").</param>
/// <param name="ActiveSynergies">The list of active synergy effects.</param>
/// <remarks>
/// <para>Used to transfer group state from the combat system to the
/// <see cref="UI.MonsterGroupView"/> for rendering.</para>
/// </remarks>
/// <example>
/// <code>
/// var dto = new MonsterGroupDisplayDto(
///     GroupId: Guid.NewGuid(),
///     GroupName: "Goblin Warband",
///     Members: memberDtos,
///     CurrentTactic: tacticDto,
///     HasLeader: true,
///     LeaderRole: "Chief",
///     ActiveSynergies: synergyDtos);
/// </code>
/// </example>
public record MonsterGroupDisplayDto(
    Guid GroupId,
    string GroupName,
    IReadOnlyList<GroupMemberDisplayDto> Members,
    TacticDisplayDto? CurrentTactic,
    bool HasLeader,
    string LeaderRole,
    IReadOnlyList<SynergyDisplayDto> ActiveSynergies);

/// <summary>
/// Data transfer object for a single group member display.
/// </summary>
/// <param name="MonsterId">The unique monster instance ID.</param>
/// <param name="MemberName">The display name of the monster.</param>
/// <param name="Role">The tactical role (melee, ranged, leader, caster, etc.).</param>
/// <param name="CurrentHealth">The current health value.</param>
/// <param name="MaxHealth">The maximum health value.</param>
/// <param name="HealthPercent">The calculated health percentage (0-100).</param>
/// <param name="IsLeader">Whether this member is the group leader.</param>
/// <param name="IsAlive">Whether this member is still alive.</param>
/// <remarks>
/// <para>Each member card displays:</para>
/// <list type="bullet">
///   <item><description>Leader badge (if applicable)</description></item>
///   <item><description>Monster name</description></item>
///   <item><description>Role designation</description></item>
///   <item><description>Health bar with current/max HP</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var dto = new GroupMemberDisplayDto(
///     MonsterId: monster.Id,
///     MemberName: "Goblin Chief",
///     Role: "leader",
///     CurrentHealth: 45,
///     MaxHealth: 45,
///     HealthPercent: 100,
///     IsLeader: true,
///     IsAlive: true);
/// </code>
/// </example>
public record GroupMemberDisplayDto(
    Guid MonsterId,
    string MemberName,
    string Role,
    int CurrentHealth,
    int MaxHealth,
    int HealthPercent,
    bool IsLeader,
    bool IsAlive);

/// <summary>
/// Data transfer object for tactic display information.
/// </summary>
/// <param name="TacticType">The tactic enum value as a string.</param>
/// <param name="TacticName">The display name of the tactic (e.g., "Flanking Assault").</param>
/// <param name="Description">A brief description of the tactic.</param>
/// <param name="RoleAssignments">The role-specific action descriptions.</param>
/// <remarks>
/// <para>The tactic indicator displays the group's coordinated behavior,
/// showing what each role does within the tactic.</para>
/// </remarks>
/// <example>
/// <code>
/// var dto = new TacticDisplayDto(
///     TacticType: "Flank",
///     TacticName: "Flanking Assault",
///     Description: "Surround target for flanking bonuses",
///     RoleAssignments: new List&lt;RoleAssignmentDto&gt;
///     {
///         new("Archer", "Attack from range"),
///         new("Chief", "Engage in melee")
///     });
/// </code>
/// </example>
public record TacticDisplayDto(
    string TacticType,
    string TacticName,
    string Description,
    IReadOnlyList<RoleAssignmentDto> RoleAssignments);

/// <summary>
/// Data transfer object for a role assignment within a tactic.
/// </summary>
/// <param name="RoleName">The role name (e.g., "Archer", "Chief", "Shaman").</param>
/// <param name="ActionDescription">What this role does in the tactic.</param>
/// <remarks>
/// <para>Role assignments are displayed in a tree format under the tactic:</para>
/// <example>
/// TACTIC: Flanking Assault
/// |-- Archers: Attack from range
/// |-- Chief: Engage in melee
/// +-- Shaman: Provide support
/// </example>
/// </remarks>
public record RoleAssignmentDto(
    string RoleName,
    string ActionDescription);

/// <summary>
/// Data transfer object for leader status display.
/// </summary>
/// <param name="LeaderName">The leader's display name.</param>
/// <param name="LeaderRole">The leader's role designation.</param>
/// <param name="IsAlive">Whether the leader is currently alive.</param>
/// <param name="IsDefeated">Whether the leader was recently defeated (triggers effect box).</param>
/// <param name="MoraleEffects">The morale effects applied to remaining members on defeat.</param>
/// <remarks>
/// <para>When the leader is defeated, an effect box displays showing:</para>
/// <list type="bullet">
///   <item><description>Leader name and defeat notification</description></item>
///   <item><description>Morale impact on remaining members</description></item>
///   <item><description>Negative effects applied (e.g., -25% attack)</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var dto = new LeaderStatusDto(
///     LeaderName: "Goblin Chief",
///     LeaderRole: "leader",
///     IsAlive: false,
///     IsDefeated: true,
///     MoraleEffects: new[] { "-25% attack for all remaining goblins", "50% chance to flee" });
/// </code>
/// </example>
public record LeaderStatusDto(
    string LeaderName,
    string LeaderRole,
    bool IsAlive,
    bool IsDefeated,
    IReadOnlyList<string> MoraleEffects);

/// <summary>
/// Data transfer object for synergy effect display.
/// </summary>
/// <param name="SynergyId">The synergy identifier.</param>
/// <param name="SynergyName">The display name of the synergy.</param>
/// <param name="EffectDescription">Description of the effect (e.g., "+1 attack to all allies").</param>
/// <param name="IsTriggered">Whether this synergy was just triggered (shows flash notification).</param>
/// <param name="SourceRole">The role that provides this synergy (e.g., "Shaman").</param>
/// <remarks>
/// <para>Synergies display in different formats based on trigger type:</para>
/// <list type="bullet">
///   <item><description>Always active: "Active: {Name} ({Effect})"</description></item>
///   <item><description>Just triggered: "SYNERGY: {Name} triggered! ({Effect})"</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var dto = new SynergyDisplayDto(
///     SynergyId: "shamans-blessing",
///     SynergyName: "Shaman's Blessing",
///     EffectDescription: "+1 attack to all allies",
///     IsTriggered: false,
///     SourceRole: "Shaman");
/// </code>
/// </example>
public record SynergyDisplayDto(
    string SynergyId,
    string SynergyName,
    string EffectDescription,
    bool IsTriggered,
    string? SourceRole);
