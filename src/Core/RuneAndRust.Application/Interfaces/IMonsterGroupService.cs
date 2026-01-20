using RuneAndRust.Application.Tracking;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for managing coordinated monster group behaviors at runtime (v0.10.4c).
/// </summary>
/// <remarks>
/// <para>
/// IMonsterGroupService is responsible for the complete lifecycle of monster groups:
/// </para>
/// <list type="bullet">
///   <item><description>Group registration with pre-spawned monsters</description></item>
///   <item><description>Tactical movement decisions (Flank, Swarm, Protect, etc.)</description></item>
///   <item><description>Coordinated target selection (FocusFire)</description></item>
///   <item><description>Synergy effect application and tracking</description></item>
///   <item><description>Member death handling and group cleanup</description></item>
/// </list>
/// <para>
/// The service separates runtime state (<see cref="MonsterGroupInstance"/>) from
/// static definition data (<see cref="MonsterGroupDefinition"/>), allowing dynamic
/// tracking of alive members, shared targets, and active synergies.
/// </para>
/// <para>
/// Unlike boss spawning, this service uses <see cref="RegisterGroup"/> to accept
/// pre-spawned monsters, allowing the spawn service to handle actual monster creation.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Register a group with pre-spawned monsters
/// var monsters = new[] { shaman, warrior1, warrior2, archer };
/// var instance = groupService.RegisterGroup("goblin-warband", monsters);
///
/// // Get tactical movement decision
/// var moveDecision = groupService.DetermineMove(warrior1);
/// if (moveDecision.ShouldMove)
/// {
///     MoveMonster(warrior1, moveDecision.TargetPosition!.Value);
/// }
///
/// // Get coordinated target
/// var target = groupService.DetermineTarget(warrior1, possibleTargets);
///
/// // Handle combat events
/// groupService.OnGroupMemberHit(warrior1, target);
/// groupService.OnGroupMemberDamaged(warrior1, 15);
/// </code>
/// </example>
public interface IMonsterGroupService
{
    // ═══════════════════════════════════════════════════════════════
    // GROUP REGISTRATION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Registers a monster group with pre-spawned monsters.
    /// </summary>
    /// <param name="groupId">The group definition ID (e.g., "goblin-warband").</param>
    /// <param name="monsters">The pre-spawned monsters to register as group members.</param>
    /// <returns>The created <see cref="MonsterGroupInstance"/> for tracking.</returns>
    /// <exception cref="ArgumentException">Thrown when group definition is not found.</exception>
    /// <exception cref="ArgumentNullException">Thrown when monsters is null.</exception>
    /// <exception cref="ArgumentException">Thrown when monsters collection is empty.</exception>
    /// <remarks>
    /// <para>
    /// This method creates a <see cref="MonsterGroupInstance"/> from the group definition,
    /// assigns roles to members based on their <see cref="Monster.MonsterDefinitionId"/>
    /// matching <see cref="GroupMember.MonsterDefinitionId"/> in the definition,
    /// applies initial synergies (with <see cref="SynergyTrigger.Always"/> trigger),
    /// and logs the group registration event.
    /// </para>
    /// <para>
    /// The service does not spawn monsters itself - use IMonsterSpawnService or similar
    /// to create the Monster entities before calling this method.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Spawn monsters first
    /// var shaman = spawnService.SpawnMonster("goblin-shaman", position);
    /// var warriors = Enumerable.Range(0, 3).Select(_ =>
    ///     spawnService.SpawnMonster("goblin-warrior", GetAdjacentPosition()));
    ///
    /// // Register as a group
    /// var monsters = new[] { shaman }.Concat(warriors).ToArray();
    /// var instance = groupService.RegisterGroup("goblin-warband", monsters);
    /// </code>
    /// </example>
    MonsterGroupInstance RegisterGroup(string groupId, IEnumerable<Monster> monsters);

    // ═══════════════════════════════════════════════════════════════
    // TACTICAL MOVEMENT
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Determines the tactical movement for a group member.
    /// </summary>
    /// <param name="monster">The monster to get a move decision for.</param>
    /// <returns>
    /// A <see cref="GroupMoveDecision"/> indicating the recommended movement action.
    /// Returns <see cref="GroupMoveDecision.NoGroup"/> if the monster is not in a group.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method evaluates the group's tactics in priority order and returns
    /// the first applicable movement decision. Tactic evaluation includes:
    /// </para>
    /// <list type="bullet">
    ///   <item><description><see cref="GroupTactic.Flank"/> - Position for flanking bonus</description></item>
    ///   <item><description><see cref="GroupTactic.FocusFire"/> - Move toward shared target</description></item>
    ///   <item><description><see cref="GroupTactic.ProtectLeader"/> - Position to shield leader</description></item>
    ///   <item><description><see cref="GroupTactic.ProtectCaster"/> - Position to shield casters</description></item>
    ///   <item><description><see cref="GroupTactic.Swarm"/> - Cluster around target</description></item>
    ///   <item><description><see cref="GroupTactic.Retreat"/> - Fall back when health low</description></item>
    ///   <item><description><see cref="GroupTactic.Ambush"/> - Hold position until trigger</description></item>
    ///   <item><description><see cref="GroupTactic.HitAndRun"/> - Attack and disengage</description></item>
    /// </list>
    /// <para>
    /// The returned decision includes the target position and the tactic that generated it.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var decision = groupService.DetermineMove(monster);
    /// switch (decision.Type)
    /// {
    ///     case GroupMoveDecisionType.MoveTo:
    ///         MoveMonster(monster, decision.TargetPosition!.Value);
    ///         break;
    ///     case GroupMoveDecisionType.HoldPosition:
    ///         // Stay in place (e.g., Ambush tactic)
    ///         break;
    ///     default:
    ///         // Use default AI movement
    ///         break;
    /// }
    /// </code>
    /// </example>
    GroupMoveDecision DetermineMove(Monster monster);

    // ═══════════════════════════════════════════════════════════════
    // TARGET SELECTION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Determines the coordinated target for a group member.
    /// </summary>
    /// <param name="monster">The monster selecting a target.</param>
    /// <param name="possibleTargets">The list of potential targets (typically players).</param>
    /// <returns>
    /// The selected target, or null if no valid target is available.
    /// Returns null if the monster is not in a group.
    /// </returns>
    /// <remarks>
    /// <para>
    /// For groups with <see cref="GroupTactic.FocusFire"/>:
    /// <list type="bullet">
    ///   <item><description>If a valid shared target exists, returns that target</description></item>
    ///   <item><description>Otherwise, selects the lowest HP target and sets as shared target</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// For groups without FocusFire, returns null to indicate the monster should
    /// use its default target selection logic.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var players = combatService.GetAlivePlayers();
    /// var target = groupService.DetermineTarget(monster, players);
    /// if (target != null)
    /// {
    ///     // Attack coordinated target
    ///     AttackTarget(monster, target);
    /// }
    /// else
    /// {
    ///     // Use default AI targeting
    ///     target = SelectDefaultTarget(monster, players);
    /// }
    /// </code>
    /// </example>
    Combatant? DetermineTarget(Monster monster, IEnumerable<Combatant> possibleTargets);

    // ═══════════════════════════════════════════════════════════════
    // SYNERGY MANAGEMENT
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Applies synergies to all members of a group.
    /// </summary>
    /// <param name="group">The group instance to apply synergies to.</param>
    /// <remarks>
    /// <para>
    /// Applies all <see cref="SynergyTrigger.Always"/> synergies from the group definition
    /// to alive members via <see cref="IBuffDebuffService"/>.
    /// </para>
    /// <para>
    /// This is typically called during group registration and when synergy conditions change.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Re-apply synergies after a member death might change conditions
    /// groupService.ApplySynergies(groupInstance);
    /// </code>
    /// </example>
    void ApplySynergies(MonsterGroupInstance group);

    /// <summary>
    /// Handles synergy triggers when a group member lands an attack.
    /// </summary>
    /// <param name="attacker">The monster that hit a target.</param>
    /// <param name="target">The target that was hit.</param>
    /// <remarks>
    /// <para>
    /// Triggers <see cref="SynergyTrigger.OnAllyHit"/> synergies for the attacker's group.
    /// These typically provide temporary attack or damage bonuses to other group members.
    /// </para>
    /// <para>
    /// Call this from the combat service after a successful hit is confirmed.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // After confirming a hit
    /// if (attackResult.IsHit)
    /// {
    ///     groupService.OnGroupMemberHit(attacker, target);
    /// }
    /// </code>
    /// </example>
    void OnGroupMemberHit(Monster attacker, Combatant target);

    /// <summary>
    /// Handles synergy triggers when a group member takes damage.
    /// </summary>
    /// <param name="target">The monster that was damaged.</param>
    /// <param name="damage">The amount of damage taken.</param>
    /// <remarks>
    /// <para>
    /// Triggers <see cref="SynergyTrigger.OnAllyDamaged"/> synergies for the target's group.
    /// These may provide defensive buffs or healing to group members.
    /// </para>
    /// <para>
    /// Call this from the combat service after damage is applied.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // After applying damage
    /// var actualDamage = monster.TakeDamage(calculatedDamage);
    /// groupService.OnGroupMemberDamaged(monster, actualDamage);
    /// </code>
    /// </example>
    void OnGroupMemberDamaged(Monster target, int damage);

    // ═══════════════════════════════════════════════════════════════
    // MEMBER DEATH HANDLING
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Handles the death of a group member.
    /// </summary>
    /// <param name="monster">The monster that died.</param>
    /// <remarks>
    /// <para>
    /// This method:
    /// <list type="bullet">
    ///   <item><description>Updates the group instance via <see cref="MonsterGroupInstance.OnMemberDeath"/></description></item>
    ///   <item><description>Clears shared target if the dead monster was the target</description></item>
    ///   <item><description>Removes the group instance if all members are dead</description></item>
    ///   <item><description>Logs the member death event</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Call this from the combat service when a monster is killed.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // When a monster dies in combat
    /// if (monster.IsDefeated)
    /// {
    ///     groupService.OnGroupMemberDeath(monster);
    /// }
    /// </code>
    /// </example>
    void OnGroupMemberDeath(Monster monster);

    // ═══════════════════════════════════════════════════════════════
    // POSITIONING HELPERS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets optimal flanking positions around a target for a group.
    /// </summary>
    /// <param name="target">The target to flank.</param>
    /// <param name="groupSize">Number of positions needed.</param>
    /// <returns>
    /// A list of grid positions that would provide flanking bonuses.
    /// May return fewer positions than requested if space is limited.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Uses <see cref="IFlankingService.GetFlankingPositions"/> to calculate
    /// positions that would grant flanking attack bonuses when multiple
    /// group members are positioned.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var positions = groupService.GetFlankingPositions(target, aliveMembers.Count);
    /// // Assign positions to group members for Flank tactic
    /// </code>
    /// </example>
    IReadOnlyList<GridPosition> GetFlankingPositions(Combatant target, int groupSize);

    /// <summary>
    /// Gets the adjacency-based bonus for a monster.
    /// </summary>
    /// <param name="monster">The monster to check.</param>
    /// <param name="bonusType">The type of bonus ("attack" or "damage").</param>
    /// <returns>
    /// The total bonus from <see cref="SynergyTrigger.PerAdjacentAlly"/> synergies
    /// multiplied by the number of adjacent allies. Returns 0 if not in a group.
    /// </returns>
    /// <remarks>
    /// <para>
    /// For groups with <see cref="SynergyTrigger.PerAdjacentAlly"/> synergies,
    /// calculates the bonus based on how many group members are adjacent
    /// to the specified monster.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var attackBonus = groupService.GetAdjacentAllyBonus(monster, "attack");
    /// var damageBonus = groupService.GetAdjacentAllyBonus(monster, "damage");
    /// var totalAttack = monster.Stats.Attack + attackBonus;
    /// </code>
    /// </example>
    int GetAdjacentAllyBonus(Monster monster, string bonusType);

    // ═══════════════════════════════════════════════════════════════
    // GROUP QUERIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if a monster is part of any registered group.
    /// </summary>
    /// <param name="monster">The monster to check.</param>
    /// <returns>True if the monster is registered with a group; false otherwise.</returns>
    /// <remarks>
    /// <para>
    /// Use this to determine if group-specific logic should be applied to a monster.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (groupService.IsInGroup(monster))
    /// {
    ///     var decision = groupService.DetermineMove(monster);
    ///     // Apply group tactical movement
    /// }
    /// else
    /// {
    ///     // Use default AI movement
    /// }
    /// </code>
    /// </example>
    bool IsInGroup(Monster monster);

    /// <summary>
    /// Gets the group instance for a specific monster.
    /// </summary>
    /// <param name="monster">The monster to look up.</param>
    /// <returns>
    /// The <see cref="MonsterGroupInstance"/> the monster belongs to, or null if not in a group.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Provides access to the group's runtime state including alive members,
    /// current target, and definition.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var group = groupService.GetGroupForMonster(monster);
    /// if (group != null)
    /// {
    ///     Console.WriteLine($"Part of {group.Definition.Name}: {group.AliveCount} alive");
    /// }
    /// </code>
    /// </example>
    MonsterGroupInstance? GetGroupForMonster(Monster monster);

    /// <summary>
    /// Gets all currently active monster groups.
    /// </summary>
    /// <returns>A read-only list of all active group instances.</returns>
    /// <remarks>
    /// <para>
    /// Returns groups that have at least one alive member.
    /// Groups are removed from tracking when all members die.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var activeGroups = groupService.GetActiveGroups();
    /// Console.WriteLine($"Managing {activeGroups.Count} active monster groups");
    /// foreach (var group in activeGroups)
    /// {
    ///     Console.WriteLine($"  {group.Definition.Name}: {group.AliveCount} alive");
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<MonsterGroupInstance> GetActiveGroups();
}
