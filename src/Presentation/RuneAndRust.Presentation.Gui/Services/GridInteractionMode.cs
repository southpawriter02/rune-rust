namespace RuneAndRust.Presentation.Gui.Services;

/// <summary>
/// Interaction modes for the combat grid.
/// </summary>
public enum GridInteractionMode
{
    /// <summary>No active targeting.</summary>
    None,

    /// <summary>Selecting movement destination.</summary>
    Movement,

    /// <summary>Selecting attack target.</summary>
    Attack,

    /// <summary>Selecting ability target.</summary>
    Ability
}
