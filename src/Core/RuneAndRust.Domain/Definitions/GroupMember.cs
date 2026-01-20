using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Defines a member slot within a monster group (v0.10.4c).
/// </summary>
/// <remarks>
/// <para>
/// GroupMember specifies which monsters make up a group and their tactical roles:
/// <list type="bullet">
///   <item><description><see cref="MonsterDefinitionId"/> - Reference to monster definition</description></item>
///   <item><description><see cref="Count"/> - Number of this monster type in the group</description></item>
///   <item><description><see cref="Role"/> - Tactical role (melee, ranged, leader, striker, caster)</description></item>
///   <item><description><see cref="PreferredPosition"/> - Optional spawn offset from group center</description></item>
/// </list>
/// </para>
/// <para>
/// Roles determine how the monster group service (IMonsterGroupService)
/// handles tactical decisions:
/// <list type="bullet">
///   <item><description><c>leader</c> - Group coordinator, protected by ProtectLeader tactic</description></item>
///   <item><description><c>melee</c> - Front-line fighters, prefer close combat</description></item>
///   <item><description><c>ranged</c> - Back-line attackers, prefer distance</description></item>
///   <item><description><c>caster</c> - Spellcasters, protected by ProtectCaster tactic</description></item>
///   <item><description><c>striker</c> - High-damage specialists, use hit-and-run</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create a goblin warband with different roles
/// var leaderMember = GroupMember.Create("goblin-shaman", 1, "leader")
///     .WithPreferredPosition(GridOffset.South(1)); // Leader stays behind
///
/// var warriorMembers = GroupMember.Create("goblin-warrior", 3, "melee")
///     .WithPreferredPosition(GridOffset.Zero); // Warriors at front
///
/// var archerMember = GroupMember.Create("goblin-archer", 2, "ranged")
///     .WithPreferredPosition(GridOffset.South(2)); // Archers in rear
/// </code>
/// </example>
public class GroupMember
{
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the monster definition ID for this group member.
    /// </summary>
    /// <remarks>
    /// <para>
    /// References a monster definition that defines the base stats, abilities,
    /// and AI behavior for monsters of this type. Resolved via
    /// <c>IMonsterProvider.GetMonster(MonsterDefinitionId)</c>.
    /// </para>
    /// </remarks>
    public string MonsterDefinitionId { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the number of monsters of this type in the group.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Defaults to 1. For groups with multiple instances of the same monster type
    /// (e.g., 3 goblin warriors), set this to the desired count.
    /// </para>
    /// </remarks>
    public int Count { get; private set; } = 1;

    /// <summary>
    /// Gets the tactical role for this member.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Common roles:
    /// <list type="bullet">
    ///   <item><description><c>leader</c> - Group coordinator, protected by other members</description></item>
    ///   <item><description><c>melee</c> - Close combat fighters</description></item>
    ///   <item><description><c>ranged</c> - Distance attackers</description></item>
    ///   <item><description><c>caster</c> - Spellcasters requiring protection</description></item>
    ///   <item><description><c>striker</c> - High-damage hit-and-run specialists</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Roles affect tactic evaluation (e.g., ProtectLeader skips the leader itself)
    /// and synergy targeting (e.g., synergies with SourceRole requirements).
    /// </para>
    /// </remarks>
    public string? Role { get; private set; }

    /// <summary>
    /// Gets the preferred spawn offset relative to the group center.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When null, the service uses default positioning based on role
    /// and spawn order. When specified, applies this offset to the
    /// group spawn center to calculate this member's initial position.
    /// </para>
    /// </remarks>
    public GridOffset? PreferredPosition { get; private set; }

    // ═══════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this member has a defined tactical role.
    /// </summary>
    public bool HasRole => !string.IsNullOrWhiteSpace(Role);

    /// <summary>
    /// Gets whether this member has a preferred spawn position.
    /// </summary>
    public bool HasPreferredPosition => PreferredPosition.HasValue;

    /// <summary>
    /// Gets whether this member is designated as the group leader.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Checks if <see cref="Role"/> equals "leader" (case-insensitive).
    /// </para>
    /// </remarks>
    public bool IsLeader => string.Equals(Role, "leader", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets whether this member is a caster.
    /// </summary>
    public bool IsCaster => string.Equals(Role, "caster", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets whether this member is a melee fighter.
    /// </summary>
    public bool IsMelee => string.Equals(Role, "melee", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets whether this member is a ranged attacker.
    /// </summary>
    public bool IsRanged => string.Equals(Role, "ranged", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets whether this member configuration is valid.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A valid member has a non-empty monster definition ID and positive count.
    /// </para>
    /// </remarks>
    public bool IsValid =>
        !string.IsNullOrWhiteSpace(MonsterDefinitionId) &&
        Count >= 1;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS & FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Private constructor for controlled creation.
    /// </summary>
    private GroupMember() { }

    /// <summary>
    /// Creates a new group member definition.
    /// </summary>
    /// <param name="monsterDefinitionId">The monster definition ID to reference.</param>
    /// <param name="count">Number of this monster type (default: 1).</param>
    /// <param name="role">Optional tactical role (leader, melee, ranged, caster, striker).</param>
    /// <returns>A new <see cref="GroupMember"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="monsterDefinitionId"/> is null or whitespace.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="count"/> is less than 1.
    /// </exception>
    /// <example>
    /// <code>
    /// var leader = GroupMember.Create("orc-chief", 1, "leader");
    /// var warriors = GroupMember.Create("orc-warrior", 4, "melee");
    /// </code>
    /// </example>
    public static GroupMember Create(string monsterDefinitionId, int count = 1, string? role = null)
    {
        if (string.IsNullOrWhiteSpace(monsterDefinitionId))
        {
            throw new ArgumentException("Monster definition ID cannot be null or empty.", nameof(monsterDefinitionId));
        }

        if (count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, "Count must be at least 1.");
        }

        return new GroupMember
        {
            MonsterDefinitionId = monsterDefinitionId.ToLowerInvariant(),
            Count = count,
            Role = string.IsNullOrWhiteSpace(role) ? null : role.ToLowerInvariant()
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // FLUENT CONFIGURATION METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Sets the preferred spawn position offset for this member.
    /// </summary>
    /// <param name="offset">The position offset relative to group center.</param>
    /// <returns>This instance for method chaining.</returns>
    /// <example>
    /// <code>
    /// var member = GroupMember.Create("archer", 2, "ranged")
    ///     .WithPreferredPosition(new GridOffset(0, 2)); // 2 cells behind center
    /// </code>
    /// </example>
    public GroupMember WithPreferredPosition(GridOffset offset)
    {
        PreferredPosition = offset;
        return this;
    }

    /// <summary>
    /// Sets the preferred spawn position using directional offset.
    /// </summary>
    /// <param name="deltaX">Horizontal offset (positive = East).</param>
    /// <param name="deltaY">Vertical offset (positive = South).</param>
    /// <returns>This instance for method chaining.</returns>
    public GroupMember WithPreferredPosition(int deltaX, int deltaY)
    {
        PreferredPosition = new GridOffset(deltaX, deltaY);
        return this;
    }

    /// <summary>
    /// Updates the tactical role for this member.
    /// </summary>
    /// <param name="role">The new role (leader, melee, ranged, caster, striker).</param>
    /// <returns>This instance for method chaining.</returns>
    public GroupMember WithRole(string role)
    {
        Role = string.IsNullOrWhiteSpace(role) ? null : role.ToLowerInvariant();
        return this;
    }

    // ═══════════════════════════════════════════════════════════════
    // UTILITY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public override string ToString()
    {
        var roleText = HasRole ? $" ({Role})" : "";
        var countText = Count > 1 ? $"{Count}x " : "";
        return $"{countText}{MonsterDefinitionId}{roleText}";
    }
}
