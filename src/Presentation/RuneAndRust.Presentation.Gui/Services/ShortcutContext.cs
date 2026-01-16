namespace RuneAndRust.Presentation.Gui.Services;

/// <summary>
/// Defines the contexts in which keyboard shortcuts operate.
/// </summary>
/// <remarks>
/// Global shortcuts are always checked first, regardless of current context.
/// Context-specific shortcuts only apply when their context is active.
/// </remarks>
public enum ShortcutContext
{
    /// <summary>Global shortcuts available everywhere.</summary>
    Global,

    /// <summary>Main menu shortcuts.</summary>
    MainMenu,

    /// <summary>General game/exploration shortcuts.</summary>
    Game,

    /// <summary>Combat mode shortcuts.</summary>
    Combat,

    /// <summary>Dialogue/conversation shortcuts.</summary>
    Dialogue,

    /// <summary>Puzzle interaction shortcuts.</summary>
    Puzzle
}
