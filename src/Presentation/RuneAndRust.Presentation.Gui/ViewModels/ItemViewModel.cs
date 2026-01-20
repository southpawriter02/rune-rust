namespace RuneAndRust.Presentation.Gui.ViewModels;

/// <summary>
/// View model representing a visible item.
/// </summary>
/// <param name="Name">The item's display name.</param>
/// <param name="Symbol">The item's display symbol.</param>
/// <param name="IsQuestItem">Whether the item is a quest item.</param>
/// <param name="Description">A description of the item.</param>
public record ItemViewModel(string Name, string Symbol, bool IsQuestItem, string Description)
{
    /// <summary>
    /// Gets the display text with quest indicator.
    /// </summary>
    public string DisplayText => IsQuestItem ? $"{Name} (quest)" : Name;
}
