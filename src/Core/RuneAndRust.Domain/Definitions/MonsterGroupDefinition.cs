using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Defines a coordinated monster group with tactics, synergies, and member composition (v0.10.4c).
/// </summary>
/// <remarks>
/// <para>
/// MonsterGroupDefinition is the central entity for group encounters:
/// <list type="bullet">
///   <item><description><see cref="Members"/> - Monster composition with roles</description></item>
///   <item><description><see cref="Tactics"/> - Priority-ordered tactical behaviors</description></item>
///   <item><description><see cref="Synergies"/> - Combat bonuses under various conditions</description></item>
///   <item><description><see cref="LeaderRole"/> - Designation for group leadership</description></item>
/// </list>
/// </para>
/// <para>
/// The monster group service (IMonsterGroupService) uses these definitions to:
/// <list type="bullet">
///   <item><description>Register pre-spawned monsters as coordinated groups</description></item>
///   <item><description>Determine tactical movement via <c>DetermineMove()</c></description></item>
///   <item><description>Select coordinated targets via <c>DetermineTarget()</c></description></item>
///   <item><description>Apply synergy effects via <c>ApplySynergies()</c></description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var warband = MonsterGroupDefinition.Create(
///         "goblin-warband",
///         "Goblin Warband",
///         "A raiding party led by a shaman")
///     .WithMember(GroupMember.Create("goblin-shaman", 1, "leader"))
///     .WithMember(GroupMember.Create("goblin-warrior", 3, "melee"))
///     .WithTactics(GroupTactic.Flank, GroupTactic.FocusFire, GroupTactic.ProtectLeader)
///     .WithSynergy(GroupSynergy.Create("pack-tactics", "Pack Tactics", SynergyTrigger.OnAllyHit)
///         .WithAttackBonus(2))
///     .WithLeader("leader");
/// </code>
/// </example>
public class MonsterGroupDefinition
{
    // ═══════════════════════════════════════════════════════════════
    // IDENTITY
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique identifier for this group.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used for referencing the group in encounter definitions and spawning.
    /// Format: lowercase with hyphens (e.g., "goblin-warband", "skeleton-patrol").
    /// </para>
    /// </remarks>
    public string GroupId { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the display name for this group.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Shown in combat logs and encounter notifications.
    /// Examples: "Goblin Warband", "Skeleton Patrol", "Bandit Ambush"
    /// </para>
    /// </remarks>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the description text for this group.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Background lore and flavor text for encounter descriptions.
    /// </para>
    /// </remarks>
    public string Description { get; private set; } = string.Empty;

    // ═══════════════════════════════════════════════════════════════
    // MEMBERS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the member definitions for this group.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each <see cref="GroupMember"/> specifies a monster type, count, and role.
    /// The total group size is the sum of all member counts.
    /// </para>
    /// </remarks>
    public IReadOnlyList<GroupMember> Members => _members.AsReadOnly();
    private readonly List<GroupMember> _members = new();

    /// <summary>
    /// Gets the role designated as the group leader.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Members with this role are protected by <see cref="GroupTactic.ProtectLeader"/>.
    /// The leader is also the target of <see cref="SynergyTrigger.OnLeaderCommand"/> synergies.
    /// </para>
    /// <para>
    /// Default is "leader" if not explicitly set.
    /// </para>
    /// </remarks>
    public string? LeaderRole { get; private set; }

    // ═══════════════════════════════════════════════════════════════
    // TACTICS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the tactical behaviors for this group in priority order.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Tactics are evaluated from first to last. The first tactic that
    /// can be executed (e.g., has valid positions) is used.
    /// </para>
    /// <para>
    /// Example priority: Flank > FocusFire > ProtectLeader
    /// - First try to surround the target for flanking bonuses
    /// - If flanking not possible, focus on weakest target
    /// - If leader threatened, protect the leader
    /// </para>
    /// </remarks>
    public IReadOnlyList<GroupTactic> Tactics => _tactics.AsReadOnly();
    private readonly List<GroupTactic> _tactics = new();

    // ═══════════════════════════════════════════════════════════════
    // SYNERGIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the synergy effects for this group.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Synergies provide combat bonuses based on various triggers.
    /// See <see cref="GroupSynergy"/> for available configurations.
    /// </para>
    /// </remarks>
    public IReadOnlyList<GroupSynergy> Synergies => _synergies.AsReadOnly();
    private readonly List<GroupSynergy> _synergies = new();

    // ═══════════════════════════════════════════════════════════════
    // TAGS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets categorization tags for this group.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used for filtering and querying groups by category.
    /// Examples: "goblin", "undead", "humanoid", "patrol", "ambush"
    /// </para>
    /// </remarks>
    public IReadOnlyList<string> Tags => _tags.AsReadOnly();
    private readonly List<string> _tags = new();

    // ═══════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the total number of monsters in this group.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Sum of all <see cref="GroupMember.Count"/> values.
    /// </para>
    /// </remarks>
    public int TotalMemberCount => _members.Sum(m => m.Count);

    /// <summary>
    /// Gets the number of distinct monster types in this group.
    /// </summary>
    public int DistinctMemberTypes => _members.Count;

    /// <summary>
    /// Gets whether this group has multiple tactics defined.
    /// </summary>
    public bool HasMultipleTactics => _tactics.Count > 1;

    /// <summary>
    /// Gets whether this group has any synergies defined.
    /// </summary>
    public bool HasSynergies => _synergies.Count > 0;

    /// <summary>
    /// Gets whether this group has a designated leader role.
    /// </summary>
    public bool HasLeaderRole => !string.IsNullOrWhiteSpace(LeaderRole);

    /// <summary>
    /// Gets the "Always" trigger synergies for this group.
    /// </summary>
    /// <remarks>
    /// <para>
    /// These synergies are applied immediately when the group is registered.
    /// </para>
    /// </remarks>
    public IEnumerable<GroupSynergy> AlwaysSynergies =>
        _synergies.Where(s => s.Trigger == SynergyTrigger.Always);

    /// <summary>
    /// Gets whether this group has any passive (Always) synergies.
    /// </summary>
    public bool HasPassiveSynergies => AlwaysSynergies.Any();

    /// <summary>
    /// Gets whether this group definition is valid.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A valid group has a non-empty ID, name, at least one member,
    /// and at least one tactic.
    /// </para>
    /// </remarks>
    public bool IsValid =>
        !string.IsNullOrWhiteSpace(GroupId) &&
        !string.IsNullOrWhiteSpace(Name) &&
        _members.Count > 0 &&
        _members.All(m => m.IsValid) &&
        _tactics.Count > 0;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS & FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Private constructor for controlled creation.
    /// </summary>
    private MonsterGroupDefinition() { }

    /// <summary>
    /// Creates a new monster group definition.
    /// </summary>
    /// <param name="groupId">Unique identifier for the group.</param>
    /// <param name="name">Display name for the group.</param>
    /// <param name="description">Optional description text.</param>
    /// <returns>A new <see cref="MonsterGroupDefinition"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="groupId"/> or <paramref name="name"/> is null or whitespace.
    /// </exception>
    /// <example>
    /// <code>
    /// var group = MonsterGroupDefinition.Create(
    ///     "skeleton-patrol",
    ///     "Skeleton Patrol",
    ///     "Undead warriors guarding the crypt")
    ///     .WithMember(GroupMember.Create("skeleton-warrior", 2, "melee"))
    ///     .WithMember(GroupMember.Create("skeleton-archer", 2, "ranged"));
    /// </code>
    /// </example>
    public static MonsterGroupDefinition Create(string groupId, string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(groupId))
        {
            throw new ArgumentException("Group ID cannot be null or empty.", nameof(groupId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        }

        return new MonsterGroupDefinition
        {
            GroupId = groupId.ToLowerInvariant(),
            Name = name,
            Description = description ?? string.Empty
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // FLUENT CONFIGURATION - MEMBERS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Adds a member to this group.
    /// </summary>
    /// <param name="member">The member configuration to add.</param>
    /// <returns>This instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="member"/> is null.
    /// </exception>
    public MonsterGroupDefinition WithMember(GroupMember member)
    {
        ArgumentNullException.ThrowIfNull(member);
        _members.Add(member);
        return this;
    }

    /// <summary>
    /// Adds multiple members to this group.
    /// </summary>
    /// <param name="members">The member configurations to add.</param>
    /// <returns>This instance for method chaining.</returns>
    public MonsterGroupDefinition WithMembers(IEnumerable<GroupMember> members)
    {
        _members.AddRange(members.Where(m => m != null));
        return this;
    }

    /// <summary>
    /// Sets the role designated as the group leader.
    /// </summary>
    /// <param name="role">The role name (e.g., "leader", "chief").</param>
    /// <returns>This instance for method chaining.</returns>
    public MonsterGroupDefinition WithLeader(string role)
    {
        LeaderRole = string.IsNullOrWhiteSpace(role) ? null : role.ToLowerInvariant();
        return this;
    }

    // ═══════════════════════════════════════════════════════════════
    // FLUENT CONFIGURATION - TACTICS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Adds tactics to this group in priority order.
    /// </summary>
    /// <param name="tactics">The tactics to add (evaluated first to last).</param>
    /// <returns>This instance for method chaining.</returns>
    /// <example>
    /// <code>
    /// group.WithTactics(GroupTactic.Flank, GroupTactic.FocusFire, GroupTactic.ProtectLeader);
    /// </code>
    /// </example>
    public MonsterGroupDefinition WithTactics(params GroupTactic[] tactics)
    {
        _tactics.AddRange(tactics);
        return this;
    }

    /// <summary>
    /// Adds a single tactic to this group.
    /// </summary>
    /// <param name="tactic">The tactic to add.</param>
    /// <returns>This instance for method chaining.</returns>
    public MonsterGroupDefinition WithTactic(GroupTactic tactic)
    {
        _tactics.Add(tactic);
        return this;
    }

    // ═══════════════════════════════════════════════════════════════
    // FLUENT CONFIGURATION - SYNERGIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Adds a synergy effect to this group.
    /// </summary>
    /// <param name="synergy">The synergy configuration to add.</param>
    /// <returns>This instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="synergy"/> is null.
    /// </exception>
    public MonsterGroupDefinition WithSynergy(GroupSynergy synergy)
    {
        ArgumentNullException.ThrowIfNull(synergy);
        _synergies.Add(synergy);
        return this;
    }

    /// <summary>
    /// Adds multiple synergy effects to this group.
    /// </summary>
    /// <param name="synergies">The synergy configurations to add.</param>
    /// <returns>This instance for method chaining.</returns>
    public MonsterGroupDefinition WithSynergies(IEnumerable<GroupSynergy> synergies)
    {
        _synergies.AddRange(synergies.Where(s => s != null));
        return this;
    }

    // ═══════════════════════════════════════════════════════════════
    // FLUENT CONFIGURATION - TAGS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Adds tags to this group for categorization.
    /// </summary>
    /// <param name="tags">The tags to add.</param>
    /// <returns>This instance for method chaining.</returns>
    public MonsterGroupDefinition WithTags(params string[] tags)
    {
        _tags.AddRange(tags
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.ToLowerInvariant()));
        return this;
    }

    // ═══════════════════════════════════════════════════════════════
    // QUERY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks whether this group has the specified tactic.
    /// </summary>
    /// <param name="tactic">The tactic to check for.</param>
    /// <returns>True if the tactic is in the tactics list; otherwise, false.</returns>
    public bool HasTactic(GroupTactic tactic) => _tactics.Contains(tactic);

    /// <summary>
    /// Checks whether this group has the specified tag.
    /// </summary>
    /// <param name="tag">The tag to check for.</param>
    /// <returns>True if the tag is present; otherwise, false.</returns>
    public bool HasTag(string tag) =>
        !string.IsNullOrWhiteSpace(tag) &&
        _tags.Contains(tag.ToLowerInvariant());

    /// <summary>
    /// Gets synergies that match the specified trigger.
    /// </summary>
    /// <param name="trigger">The trigger type to filter by.</param>
    /// <returns>Synergies matching the trigger type.</returns>
    public IEnumerable<GroupSynergy> GetSynergiesByTrigger(SynergyTrigger trigger) =>
        _synergies.Where(s => s.Trigger == trigger);

    /// <summary>
    /// Gets members with the specified role.
    /// </summary>
    /// <param name="role">The role to filter by.</param>
    /// <returns>Members matching the specified role.</returns>
    public IEnumerable<GroupMember> GetMembersByRole(string role) =>
        string.IsNullOrWhiteSpace(role)
            ? Enumerable.Empty<GroupMember>()
            : _members.Where(m => string.Equals(m.Role, role, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Gets the member designated as the leader.
    /// </summary>
    /// <returns>The leader member, or null if no leader role is defined.</returns>
    public GroupMember? GetLeaderMember()
    {
        if (!HasLeaderRole) return null;
        return _members.FirstOrDefault(m => string.Equals(m.Role, LeaderRole, StringComparison.OrdinalIgnoreCase));
    }

    // ═══════════════════════════════════════════════════════════════
    // VALIDATION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Validates the group configuration and returns any issues found.
    /// </summary>
    /// <returns>A list of validation error messages, empty if valid.</returns>
    public IReadOnlyList<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(GroupId))
        {
            errors.Add("Group ID is required.");
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            errors.Add("Name is required.");
        }

        if (_members.Count == 0)
        {
            errors.Add("At least one member is required.");
        }

        if (_tactics.Count == 0)
        {
            errors.Add("At least one tactic is required.");
        }

        // Validate members
        for (var i = 0; i < _members.Count; i++)
        {
            if (!_members[i].IsValid)
            {
                errors.Add($"Member {i + 1} is invalid.");
            }
        }

        // Check for leader role if specified
        if (HasLeaderRole && !_members.Any(m => string.Equals(m.Role, LeaderRole, StringComparison.OrdinalIgnoreCase)))
        {
            errors.Add($"Leader role '{LeaderRole}' not found in members.");
        }

        // Check for duplicate synergy IDs
        var duplicateSynergyIds = _synergies
            .GroupBy(s => s.SynergyId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        foreach (var synId in duplicateSynergyIds)
        {
            errors.Add($"Duplicate synergy ID: {synId}");
        }

        return errors;
    }

    // ═══════════════════════════════════════════════════════════════
    // UTILITY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public override string ToString() =>
        $"{Name} ({GroupId}) - {TotalMemberCount} members, {_tactics.Count} tactic(s), {_synergies.Count} synergy(ies)";
}
