using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.ViewModels;

/// <summary>
/// View model for the full specialization tree grid.
/// </summary>
/// <remarks>See: v0.4.1d (The Grid) for Specialization UI implementation.</remarks>
public class SpecializationGridViewModel
{
    /// <summary>
    /// Specialization ID being displayed.
    /// </summary>
    public Guid SpecializationId { get; init; }

    /// <summary>
    /// Specialization name for header.
    /// </summary>
    public string SpecializationName { get; init; } = string.Empty;

    /// <summary>
    /// Specialization description/flavor text.
    /// </summary>
    public string SpecializationDescription { get; init; } = string.Empty;

    /// <summary>
    /// Character's current Progression Points.
    /// </summary>
    public int ProgressionPoints { get; init; }

    /// <summary>
    /// Character name for header display.
    /// </summary>
    public string CharacterName { get; init; } = string.Empty;

    /// <summary>
    /// All nodes grouped by tier (1-4).
    /// </summary>
    public IReadOnlyDictionary<int, IReadOnlyList<NodeViewModel>> NodesByTier { get; init; }
        = new Dictionary<int, IReadOnlyList<NodeViewModel>>();

    /// <summary>
    /// Flat list of all nodes in display order.
    /// </summary>
    public IReadOnlyList<NodeViewModel> AllNodes { get; init; } = Array.Empty<NodeViewModel>();

    /// <summary>
    /// Currently selected node index (0-based in AllNodes).
    /// </summary>
    public int SelectedNodeIndex { get; set; }

    /// <summary>
    /// Currently selected node for display.
    /// </summary>
    public NodeViewModel? SelectedNode =>
        SelectedNodeIndex >= 0 && SelectedNodeIndex < AllNodes.Count
            ? AllNodes[SelectedNodeIndex]
            : null;

    /// <summary>
    /// Total nodes in the tree.
    /// </summary>
    public int TotalNodes => AllNodes.Count;

    /// <summary>
    /// Count of unlocked nodes.
    /// </summary>
    public int UnlockedCount => AllNodes.Count(n => n.Status == NodeStatus.Unlocked);

    /// <summary>
    /// Feedback message to display (purchase result, errors).
    /// </summary>
    public string? FeedbackMessage { get; set; }

    /// <summary>
    /// Whether feedback is a success (green) or error (red).
    /// </summary>
    public bool FeedbackIsSuccess { get; set; }

    /// <summary>
    /// Index of current specialization (for multi-spec tab switching).
    /// </summary>
    public int CurrentSpecIndex { get; set; }

    /// <summary>
    /// Total count of unlocked specializations (for tab indicator).
    /// </summary>
    public int TotalSpecCount { get; set; }
}
