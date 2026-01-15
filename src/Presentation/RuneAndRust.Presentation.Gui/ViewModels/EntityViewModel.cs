namespace RuneAndRust.Presentation.Gui.ViewModels;

/// <summary>
/// View model representing a visible entity (monster).
/// </summary>
/// <param name="Name">The entity's display name.</param>
/// <param name="Symbol">The entity's display symbol.</param>
/// <param name="IsHostile">Whether the entity is hostile to the player.</param>
/// <param name="Description">A description of the entity.</param>
public record EntityViewModel(string Name, string Symbol, bool IsHostile, string Description)
{
    /// <summary>
    /// Gets the display text with hostility indicator.
    /// </summary>
    public string DisplayText => IsHostile ? $"{Name} (hostile)" : Name;
}
