using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.Tracking;

/// <summary>
/// Tracks the runtime state of an active monster group in combat (v0.10.4c).
/// </summary>
/// <remarks>
/// <para>
/// MonsterGroupInstance separates runtime state from static definition data,
/// allowing the monster group system to track:
/// </para>
/// <list type="bullet">
///   <item><description><see cref="Definition"/> - Reference to the group definition</description></item>
///   <item><description><see cref="Members"/> - All monsters registered to this group</description></item>
///   <item><description><see cref="AliveMembers"/> - Currently alive group members</description></item>
///   <item><description><see cref="CurrentTarget"/> - Shared target for FocusFire tactic</description></item>
///   <item><description>Member roles tracked externally in <c>_memberRoles</c></description></item>
/// </list>
/// <para>
/// This class is mutable and updated throughout the encounter by the
/// <see cref="Interfaces.IMonsterGroupService"/> implementation.
/// </para>
/// <para>
/// Because the <see cref="Monster"/> entity does not have a GroupRole property,
/// roles are tracked externally in this instance via <see cref="GetMemberRole(Monster)"/>
/// and <see cref="SetMemberRole(Monster, string)"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Register a group with pre-spawned monsters
/// var instance = new MonsterGroupInstance(definition, monsters);
///
/// // Set member roles based on definition
/// instance.SetMemberRole(shaman, "leader");
/// instance.SetMemberRole(warrior1, "melee");
///
/// // Track shared target for FocusFire
/// instance.SetCurrentTarget(targetId);
///
/// // Query group state
/// Console.WriteLine($"Alive: {instance.AliveCount}/{instance.TotalCount}");
/// Console.WriteLine($"Has leader: {instance.HasAliveLeader}");
///
/// // Handle member death
/// instance.OnMemberDeath(deadMonster);
/// </code>
/// </example>
public class MonsterGroupInstance
{
    // ═══════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Backing list for <see cref="Members"/>.
    /// </summary>
    private readonly List<Monster> _members = [];

    /// <summary>
    /// External role tracking since Monster entity lacks GroupRole property.
    /// Maps monster ID to assigned role (leader, melee, ranged, caster, striker).
    /// </summary>
    private readonly Dictionary<Guid, string> _memberRoles = new();

    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique identifier for this group instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Generated when the group is registered. Used to identify this
    /// specific group instance in the monster-to-group mapping.
    /// </para>
    /// </remarks>
    public Guid Id { get; }

    /// <summary>
    /// Gets the group definition ID this instance was created from.
    /// </summary>
    /// <remarks>
    /// <para>
    /// References the <see cref="MonsterGroupDefinition"/> via
    /// <see cref="Interfaces.IMonsterGroupProvider.GetGroup(string)"/>.
    /// </para>
    /// </remarks>
    public string GroupId { get; }

    /// <summary>
    /// Gets the group definition this instance was created from.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Contains the static configuration including tactics, synergies,
    /// and member definitions.
    /// </para>
    /// </remarks>
    public MonsterGroupDefinition Definition { get; }

    /// <summary>
    /// Gets or sets the current shared target ID for FocusFire tactic.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When using the <see cref="Domain.Enums.GroupTactic.FocusFire"/> tactic,
    /// all group members focus on this target until it dies or becomes invalid.
    /// </para>
    /// <para>
    /// Set via <see cref="SetCurrentTarget(Guid)"/> and cleared via
    /// <see cref="ClearCurrentTarget"/>.
    /// </para>
    /// </remarks>
    public Guid? CurrentTarget { get; private set; }

    /// <summary>
    /// Gets the list of all monsters registered to this group.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Includes both alive and dead members. Use <see cref="AliveMembers"/>
    /// to get only currently alive members.
    /// </para>
    /// </remarks>
    public IReadOnlyList<Monster> Members => _members;

    // ═══════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the list of currently alive group members.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Filters <see cref="Members"/> to only include monsters where
    /// <see cref="Monster.IsAlive"/> is true.
    /// </para>
    /// </remarks>
    public IReadOnlyList<Monster> AliveMembers => _members.Where(m => m.IsAlive).ToList();

    /// <summary>
    /// Gets the total number of members in this group.
    /// </summary>
    public int TotalCount => _members.Count;

    /// <summary>
    /// Gets the count of currently alive members.
    /// </summary>
    public int AliveCount => _members.Count(m => m.IsAlive);

    /// <summary>
    /// Gets whether any members are still alive.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When this becomes false, the group instance should be removed
    /// from tracking by the <see cref="Interfaces.IMonsterGroupService"/>.
    /// </para>
    /// </remarks>
    public bool HasAliveMembers => _members.Any(m => m.IsAlive);

    /// <summary>
    /// Gets whether the group has a leader that is still alive.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Checks if any alive member has the "leader" role assigned.
    /// Used by <see cref="Domain.Enums.GroupTactic.ProtectLeader"/> tactic.
    /// </para>
    /// </remarks>
    public bool HasAliveLeader => GetLeader() != null;

    /// <summary>
    /// Gets whether the group has any alive casters.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Checks if any alive member has the "caster" role assigned.
    /// Used by <see cref="Domain.Enums.GroupTactic.ProtectCaster"/> tactic.
    /// </para>
    /// </remarks>
    public bool HasAliveCaster => GetMembersByRole("caster").Any(m => m.IsAlive);

    /// <summary>
    /// Gets whether the current target is set.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note: This only indicates a target ID is set. The target may have
    /// died or become invalid. Use with <c>IsTargetValid()</c> from
    /// the service to verify the target is still attackable.
    /// </para>
    /// </remarks>
    public bool HasCurrentTarget => CurrentTarget.HasValue;

    /// <summary>
    /// Gets whether this instance is still active (has alive members).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Alias for <see cref="HasAliveMembers"/> for clarity in service code.
    /// </para>
    /// </remarks>
    public bool IsActive => HasAliveMembers;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new monster group instance from a definition and pre-spawned monsters.
    /// </summary>
    /// <param name="definition">The group definition to track.</param>
    /// <param name="monsters">The pre-spawned monsters to register as group members.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="definition"/> or <paramref name="monsters"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="monsters"/> is empty.
    /// </exception>
    /// <remarks>
    /// <para>
    /// The constructor registers all provided monsters as group members but does
    /// not assign roles. Use <see cref="SetMemberRole(Monster, string)"/> to
    /// assign tactical roles after construction.
    /// </para>
    /// <para>
    /// The group ID is taken from the definition. A new instance ID is generated
    /// to uniquely identify this runtime instance.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var definition = groupProvider.GetGroup("goblin-warband");
    /// var monsters = new[] { shaman, warrior1, warrior2, warrior3 };
    /// var instance = new MonsterGroupInstance(definition, monsters);
    /// </code>
    /// </example>
    public MonsterGroupInstance(MonsterGroupDefinition definition, IEnumerable<Monster> monsters)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(monsters);

        var monsterList = monsters.ToList();
        if (monsterList.Count == 0)
        {
            throw new ArgumentException("At least one monster must be provided.", nameof(monsters));
        }

        Id = Guid.NewGuid();
        GroupId = definition.GroupId;
        Definition = definition;

        _members.AddRange(monsterList);
    }

    // ═══════════════════════════════════════════════════════════════
    // ROLE MANAGEMENT METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the tactical role assigned to a monster in this group.
    /// </summary>
    /// <param name="monster">The monster to get the role for.</param>
    /// <returns>
    /// The assigned role (leader, melee, ranged, caster, striker), or null if no role assigned.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Roles are tracked externally because the <see cref="Monster"/> entity
    /// does not have a GroupRole property. Roles are assigned during group
    /// registration based on the <see cref="GroupMember.Role"/> in the definition.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var role = instance.GetMemberRole(monster);
    /// if (role == "leader")
    /// {
    ///     // Apply leader-specific logic
    /// }
    /// </code>
    /// </example>
    public string? GetMemberRole(Monster monster)
    {
        ArgumentNullException.ThrowIfNull(monster);
        return _memberRoles.TryGetValue(monster.Id, out var role) ? role : null;
    }

    /// <summary>
    /// Assigns a tactical role to a monster in this group.
    /// </summary>
    /// <param name="monster">The monster to assign the role to.</param>
    /// <param name="role">The role to assign (leader, melee, ranged, caster, striker).</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="monster"/> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the monster is not a member of this group.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Roles affect tactic evaluation (e.g., <see cref="Domain.Enums.GroupTactic.ProtectLeader"/>
    /// skips the leader) and synergy targeting (e.g., synergies with SourceRole requirements).
    /// </para>
    /// <para>
    /// If role is null or whitespace, the role is removed from the monster.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// instance.SetMemberRole(shaman, "leader");
    /// instance.SetMemberRole(warrior, "melee");
    /// instance.SetMemberRole(archer, "ranged");
    /// </code>
    /// </example>
    public void SetMemberRole(Monster monster, string? role)
    {
        ArgumentNullException.ThrowIfNull(monster);

        if (!_members.Contains(monster))
        {
            throw new InvalidOperationException(
                $"Monster '{monster.Name}' (ID: {monster.Id}) is not a member of this group.");
        }

        if (string.IsNullOrWhiteSpace(role))
        {
            _memberRoles.Remove(monster.Id);
        }
        else
        {
            _memberRoles[monster.Id] = role.ToLowerInvariant();
        }
    }

    /// <summary>
    /// Checks if a monster has a specific role in this group.
    /// </summary>
    /// <param name="monster">The monster to check.</param>
    /// <param name="role">The role to check for (case-insensitive).</param>
    /// <returns>True if the monster has the specified role; false otherwise.</returns>
    public bool HasRole(Monster monster, string role)
    {
        ArgumentNullException.ThrowIfNull(monster);
        if (string.IsNullOrWhiteSpace(role)) return false;

        var memberRole = GetMemberRole(monster);
        return string.Equals(memberRole, role, StringComparison.OrdinalIgnoreCase);
    }

    // ═══════════════════════════════════════════════════════════════
    // MEMBER QUERY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the leader of this group if alive.
    /// </summary>
    /// <returns>The alive leader monster, or null if no leader or leader is dead.</returns>
    /// <remarks>
    /// <para>
    /// Searches for a member with the "leader" role that is still alive.
    /// Used by <see cref="Domain.Enums.GroupTactic.ProtectLeader"/> tactic.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var leader = instance.GetLeader();
    /// if (leader != null)
    /// {
    ///     // Position other members to protect the leader
    /// }
    /// </code>
    /// </example>
    public Monster? GetLeader()
    {
        return _members.FirstOrDefault(m =>
            m.IsAlive && HasRole(m, "leader"));
    }

    /// <summary>
    /// Gets all members with a specific role.
    /// </summary>
    /// <param name="role">The role to filter by (case-insensitive).</param>
    /// <returns>A list of monsters with the specified role (including dead members).</returns>
    /// <remarks>
    /// <para>
    /// Returns all members regardless of alive status. Use <see cref="AliveMembers"/>
    /// or filter results with <see cref="Monster.IsAlive"/> if needed.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var meleeMembers = instance.GetMembersByRole("melee");
    /// var aliveMelee = meleeMembers.Where(m => m.IsAlive);
    /// </code>
    /// </example>
    public IReadOnlyList<Monster> GetMembersByRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return [];
        }

        return _members.Where(m => HasRole(m, role)).ToList();
    }

    /// <summary>
    /// Gets all alive members with a specific role.
    /// </summary>
    /// <param name="role">The role to filter by (case-insensitive).</param>
    /// <returns>A list of alive monsters with the specified role.</returns>
    /// <example>
    /// <code>
    /// var aliveCasters = instance.GetAliveMembersByRole("caster");
    /// foreach (var caster in aliveCasters)
    /// {
    ///     // Apply caster protection logic
    /// }
    /// </code>
    /// </example>
    public IReadOnlyList<Monster> GetAliveMembersByRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return [];
        }

        return _members.Where(m => m.IsAlive && HasRole(m, role)).ToList();
    }

    /// <summary>
    /// Checks if a monster is a member of this group.
    /// </summary>
    /// <param name="monster">The monster to check.</param>
    /// <returns>True if the monster is a member of this group; false otherwise.</returns>
    public bool IsMember(Monster monster)
    {
        ArgumentNullException.ThrowIfNull(monster);
        return _members.Contains(monster);
    }

    /// <summary>
    /// Gets a member by their entity ID.
    /// </summary>
    /// <param name="monsterId">The monster's entity ID.</param>
    /// <returns>The monster if found; null otherwise.</returns>
    public Monster? GetMemberById(Guid monsterId)
    {
        return _members.FirstOrDefault(m => m.Id == monsterId);
    }

    // ═══════════════════════════════════════════════════════════════
    // TARGET MANAGEMENT METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Sets the current shared target for the FocusFire tactic.
    /// </summary>
    /// <param name="targetId">The target's entity ID.</param>
    /// <remarks>
    /// <para>
    /// When using <see cref="Domain.Enums.GroupTactic.FocusFire"/>, all group
    /// members will attack this target until it dies or becomes invalid.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Select lowest HP enemy as focus target
    /// var weakestEnemy = enemies.OrderBy(e => e.Health).First();
    /// instance.SetCurrentTarget(weakestEnemy.Id);
    /// </code>
    /// </example>
    public void SetCurrentTarget(Guid targetId)
    {
        CurrentTarget = targetId;
    }

    /// <summary>
    /// Clears the current shared target.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Called when the current target dies or becomes invalid.
    /// A new target will be selected on the next targeting decision.
    /// </para>
    /// </remarks>
    public void ClearCurrentTarget()
    {
        CurrentTarget = null;
    }

    // ═══════════════════════════════════════════════════════════════
    // LIFECYCLE METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Handles the death of a group member.
    /// </summary>
    /// <param name="monster">The monster that died.</param>
    /// <remarks>
    /// <para>
    /// This method does not remove the monster from <see cref="Members"/>
    /// (the monster remains for historical tracking). The monster's
    /// <see cref="Monster.IsAlive"/> property naturally filters it out
    /// of <see cref="AliveMembers"/>.
    /// </para>
    /// <para>
    /// If the dead monster was the current target, the target is cleared
    /// automatically.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // When a monster in the group dies
    /// instance.OnMemberDeath(deadMonster);
    ///
    /// // Check if group should be removed
    /// if (!instance.HasAliveMembers)
    /// {
    ///     // Remove group from tracking
    /// }
    /// </code>
    /// </example>
    public void OnMemberDeath(Monster monster)
    {
        ArgumentNullException.ThrowIfNull(monster);

        // Clear target if the dead monster was our focus target
        if (CurrentTarget.HasValue && CurrentTarget.Value == monster.Id)
        {
            ClearCurrentTarget();
        }

        // Note: We don't remove the monster from _members
        // The Monster.IsAlive property naturally excludes it from AliveMembers
    }

    /// <summary>
    /// Removes a monster from the group entirely (e.g., if despawned).
    /// </summary>
    /// <param name="monster">The monster to remove.</param>
    /// <returns>True if the monster was removed; false if it wasn't a member.</returns>
    /// <remarks>
    /// <para>
    /// Unlike <see cref="OnMemberDeath(Monster)"/>, this completely removes
    /// the monster from the group. Use for despawn scenarios, not deaths.
    /// </para>
    /// </remarks>
    public bool RemoveMember(Monster monster)
    {
        ArgumentNullException.ThrowIfNull(monster);

        var removed = _members.Remove(monster);
        if (removed)
        {
            _memberRoles.Remove(monster.Id);

            // Clear target if removed monster was our focus target
            if (CurrentTarget.HasValue && CurrentTarget.Value == monster.Id)
            {
                ClearCurrentTarget();
            }
        }

        return removed;
    }

    // ═══════════════════════════════════════════════════════════════
    // UTILITY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation of this group instance.
    /// </summary>
    /// <returns>A string showing the group ID and member count.</returns>
    public override string ToString()
    {
        var leaderText = HasAliveLeader ? ", leader alive" : "";
        var targetText = HasCurrentTarget ? $", targeting {CurrentTarget}" : "";
        return $"Group[{GroupId}]: {AliveCount}/{TotalCount} alive{leaderText}{targetText}";
    }
}
