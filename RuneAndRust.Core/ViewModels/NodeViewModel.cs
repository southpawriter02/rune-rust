using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.ViewModels;

/// <summary>
/// View model for a single node in the specialization tree.
/// </summary>
/// <remarks>See: v0.4.1d (The Grid) for Specialization UI implementation.</remarks>
/// <param name="NodeId">The node's unique identifier.</param>
/// <param name="AbilityId">The ability ID granted by this node.</param>
/// <param name="Name">Display name for the node.</param>
/// <param name="Description">Description of the ability's effects.</param>
/// <param name="Tier">Node tier (1-4, where 4 is capstone).</param>
/// <param name="CostPP">PP cost to unlock this node.</param>
/// <param name="Status">Current unlock status of the node.</param>
/// <param name="PositionX">Horizontal position for layout.</param>
/// <param name="PositionY">Vertical position for layout.</param>
/// <param name="IsCapstone">Whether this is a capstone (Tier 4) node.</param>
/// <param name="ParentNodeIds">IDs of prerequisite nodes.</param>
public record NodeViewModel(
    Guid NodeId,
    Guid AbilityId,
    string Name,
    string Description,
    int Tier,
    int CostPP,
    NodeStatus Status,
    int PositionX,
    int PositionY,
    bool IsCapstone,
    IReadOnlyList<Guid> ParentNodeIds)
{
    /// <summary>
    /// Gets the display string for the cost column.
    /// </summary>
    public string CostDisplay => Status == NodeStatus.Unlocked ? "-" : $"{CostPP} PP";

    /// <summary>
    /// Gets the status display string with color markup.
    /// </summary>
    public string StatusMarkup => Status switch
    {
        NodeStatus.Unlocked => "[green](INSCRIBED)[/]",
        NodeStatus.Available => "[cyan](AVAILABLE)[/]",
        NodeStatus.Affordable => "[yellow](SAVE UP)[/]",
        NodeStatus.Locked => "[red](LOCKED)[/]",
        _ => "[grey](UNKNOWN)[/]"
    };
}
