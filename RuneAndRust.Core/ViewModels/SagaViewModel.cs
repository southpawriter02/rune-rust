using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Core.ViewModels;

/// <summary>
/// Immutable snapshot of saga progression state for UI rendering (v0.4.0c).
/// Displays Legend progress, Progression Points, and attribute upgrade options.
/// </summary>
/// <remarks>See: v0.4.0c (The Shrine) for Saga UI implementation.</remarks>
/// <param name="CharacterName">The player character's display name.</param>
/// <param name="Level">Current character level (1-10).</param>
/// <param name="CurrentLegend">Current Legend (XP) total.</param>
/// <param name="LegendForNextLevel">Legend required to reach next level (-1 if at max).</param>
/// <param name="ProgressionPoints">Available PP for spending on upgrades.</param>
/// <param name="SelectedIndex">Currently selected attribute index (0-4, for navigation).</param>
/// <param name="Attributes">Ordered list of attribute rows for display.</param>
public record SagaViewModel(
    string CharacterName,
    int Level,
    int CurrentLegend,
    int LegendForNextLevel,
    int ProgressionPoints,
    int SelectedIndex,
    List<AttributeRowViewModel> Attributes
);

/// <summary>
/// Display-ready view of a single attribute row in the Saga UI.
/// </summary>
/// <param name="Type">The attribute type (MIGHT, FINESSE, etc.).</param>
/// <param name="CurrentValue">Current attribute value (1-10).</param>
/// <param name="UpgradeCost">PP cost to upgrade (1 normally, int.MaxValue if capped).</param>
/// <param name="Status">Display status for color-coding.</param>
public record AttributeRowViewModel(
    CharacterAttribute Type,
    int CurrentValue,
    int UpgradeCost,
    AttributeStatus Status
);

/// <summary>
/// Status of an attribute for UI display styling.
/// </summary>
public enum AttributeStatus
{
    /// <summary>
    /// Can afford upgrade and not at cap. Display in green.
    /// </summary>
    Upgrade,

    /// <summary>
    /// Cannot afford upgrade (insufficient PP). Display in red.
    /// </summary>
    Locked,

    /// <summary>
    /// At maximum value (10). Display in grey.
    /// </summary>
    Maxed
}
