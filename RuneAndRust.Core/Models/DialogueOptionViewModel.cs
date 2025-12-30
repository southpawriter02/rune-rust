using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Models;

/// <summary>
/// View model for a dialogue option, including visibility state.
/// Used by the UI to render available options.
/// </summary>
/// <remarks>See: v0.4.2c (The Voice) for DialogueService implementation.</remarks>
public class DialogueOptionViewModel
{
    /// <summary>
    /// The option's unique identifier within the node.
    /// </summary>
    public required string OptionId { get; set; }

    /// <summary>
    /// The display text for this option.
    /// </summary>
    public required string Text { get; set; }

    /// <summary>
    /// Whether this option is currently available to select.
    /// </summary>
    public bool IsAvailable { get; set; }

    /// <summary>
    /// Whether this option should be displayed (even if locked).
    /// Based on OptionVisibility setting.
    /// </summary>
    public bool IsVisible { get; set; }

    /// <summary>
    /// Optional reason why this option is locked.
    /// Only populated when IsAvailable is false and IsVisible is true.
    /// </summary>
    public string? LockedReason { get; set; }

    /// <summary>
    /// The visibility mode for this option.
    /// </summary>
    public OptionVisibility VisibilityMode { get; set; }

    /// <summary>
    /// Optional tooltip text for the option.
    /// </summary>
    public string? Tooltip { get; set; }

    /// <summary>
    /// Order index for display sorting.
    /// </summary>
    public int DisplayOrder { get; set; }
}
