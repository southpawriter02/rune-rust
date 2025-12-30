using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.ViewModels;

/// <summary>
/// Immutable view state for the Specialization UI screen.
/// Contains all data needed to render the "Tree of Runes" interface.
/// </summary>
/// <remarks>See: v0.4.1c (The Tree of Runes) for implementation.</remarks>
/// <param name="CharacterName">The player character's display name.</param>
/// <param name="AvailableProgressionPoints">Current PP available to spend.</param>
/// <param name="ViewMode">Which panel has focus (SpecList or TreeDetail).</param>
/// <param name="Specializations">List of specializations for the left panel.</param>
/// <param name="SelectedSpecIndex">Currently selected specialization index.</param>
/// <param name="CurrentTree">Tree view for the selected specialization (null if none).</param>
/// <param name="SelectedNodeIndex">Currently selected node index within the tree.</param>
/// <param name="StatusMessage">Optional feedback message for the user.</param>
public record SpecializationViewModel(
    string CharacterName,
    int AvailableProgressionPoints,
    SpecializationViewMode ViewMode,
    IReadOnlyList<SpecializationListItem> Specializations,
    int SelectedSpecIndex,
    SpecializationTreeView? CurrentTree,
    int SelectedNodeIndex,
    string? StatusMessage = null);

/// <summary>
/// Display item for the specialization list (left panel).
/// </summary>
/// <param name="Id">The specialization's unique identifier.</param>
/// <param name="Name">Display name for the specialization.</param>
/// <param name="Description">Brief description of the specialization.</param>
/// <param name="IsUnlocked">Whether the character has unlocked this specialization.</param>
/// <param name="CanUnlock">Whether the character can unlock this specialization (requirements met).</param>
/// <param name="UnlockCost">PP cost to unlock the specialization (10).</param>
/// <param name="NodesUnlocked">Number of nodes the character has unlocked in this tree.</param>
/// <param name="TotalNodes">Total number of nodes in this specialization tree.</param>
public record SpecializationListItem(
    Guid Id,
    string Name,
    string Description,
    bool IsUnlocked,
    bool CanUnlock,
    int UnlockCost,
    int NodesUnlocked,
    int TotalNodes);

/// <summary>
/// Display model for a specialization tree (right panel).
/// </summary>
/// <param name="SpecializationId">The specialization's unique identifier.</param>
/// <param name="SpecializationName">Display name for the specialization.</param>
/// <param name="Nodes">Ordered list of nodes in the tree.</param>
public record SpecializationTreeView(
    Guid SpecializationId,
    string SpecializationName,
    IReadOnlyList<NodeDisplayItem> Nodes);

/// <summary>
/// Display item for a single node in the tree.
/// </summary>
/// <param name="NodeId">The node's unique identifier.</param>
/// <param name="NodeName">Display name for the node.</param>
/// <param name="AbilityName">Name of the ability granted by this node.</param>
/// <param name="AbilityDescription">Description of the ability's effects.</param>
/// <param name="Tier">Node tier (1-4, where 4 is capstone).</param>
/// <param name="CostPP">PP cost to unlock this node.</param>
/// <param name="Status">Current unlock status of the node.</param>
/// <param name="PositionX">Horizontal position for layout.</param>
/// <param name="PositionY">Vertical position for layout.</param>
/// <param name="IsCapstone">Whether this is a capstone (Tier 4) node.</param>
/// <param name="ParentNodeIds">IDs of prerequisite nodes.</param>
public record NodeDisplayItem(
    Guid NodeId,
    string NodeName,
    string AbilityName,
    string AbilityDescription,
    int Tier,
    int CostPP,
    NodeStatus Status,
    int PositionX,
    int PositionY,
    bool IsCapstone,
    IReadOnlyList<Guid> ParentNodeIds);
