namespace RuneAndRust.Core.Entities;

/// <summary>
/// A single node in a specialization's ability tree.
/// Represents an ability that can be unlocked by spending Progression Points (PP).
/// </summary>
/// <remarks>
/// See: SPEC-SPECIALIZATION-001 for design documentation.
/// See: v0.4.1a for implementation details.
///
/// Tree structure:
/// - Tier 1: Root nodes with no prerequisites (3 nodes typically)
/// - Tier 2: Requires at least one Tier 1 parent unlocked
/// - Tier 3: Requires specific Tier 2 parents unlocked
/// - Tier 4: Capstone node requiring all Tier 3 nodes
///
/// Position (X, Y) is used for UI grid rendering:
/// - Y typically matches (Tier - 1)
/// - X allows horizontal spacing within a tier
/// </remarks>
public class SpecializationNode
{
    /// <summary>
    /// Unique identifier for this node.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Foreign key to the parent Specialization.
    /// </summary>
    public Guid SpecializationId { get; set; }

    /// <summary>
    /// Navigation property to the parent Specialization.
    /// </summary>
    public Specialization Specialization { get; set; } = null!;

    /// <summary>
    /// Foreign key to the ability this node unlocks.
    /// References an existing ActiveAbility.Id.
    /// </summary>
    public Guid AbilityId { get; set; }

    /// <summary>
    /// Navigation property to the unlocked ability.
    /// </summary>
    public ActiveAbility Ability { get; set; } = null!;

    /// <summary>
    /// Tier level (1-4). Determines position in the tree and prerequisites.
    /// Tier 4 is reserved for Capstone abilities.
    /// </summary>
    public int Tier { get; set; } = 1;

    /// <summary>
    /// IDs of parent nodes that must be unlocked before this node becomes available.
    /// Empty list for Tier 1 (root) nodes.
    /// Stored as JSONB in PostgreSQL.
    /// </summary>
    public List<Guid> ParentNodeIds { get; set; } = new();

    /// <summary>
    /// Progression Point cost to unlock this node.
    /// Typical costs: Tier 1 = 1 PP, Tier 2 = 2 PP, Tier 3 = 3 PP, Capstone = 5 PP.
    /// </summary>
    public int CostPP { get; set; } = 1;

    /// <summary>
    /// Horizontal position in the UI grid (0-indexed from left).
    /// Used for visual layout of the ability tree.
    /// </summary>
    public int PositionX { get; set; } = 0;

    /// <summary>
    /// Vertical position in the UI grid (0-indexed from top).
    /// Typically matches (Tier - 1) for consistent vertical alignment.
    /// </summary>
    public int PositionY { get; set; } = 0;

    /// <summary>
    /// Optional display name override for the UI.
    /// If null, the Ability.Name is used instead.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Indicates if this is a Capstone (Tier 4) node.
    /// Capstones are the ultimate abilities in a specialization tree.
    /// </summary>
    public bool IsCapstone => Tier == 4;

    /// <summary>
    /// Gets the effective display name for UI rendering.
    /// Returns DisplayName if set, otherwise returns Ability.Name.
    /// </summary>
    public string GetDisplayName() => DisplayName ?? Ability?.Name ?? "Unknown";
}
