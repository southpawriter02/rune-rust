namespace RuneAndRust.Presentation.Gui.ViewModels;

using RuneAndRust.Domain.Enums;

/// <summary>
/// View model representing a room exit.
/// </summary>
/// <param name="Direction">The direction of the exit.</param>
/// <param name="IsLocked">Whether the exit is locked.</param>
/// <remarks>
/// Provides display properties for exit buttons including direction symbols,
/// labels, and tooltips.
/// </remarks>
public record ExitViewModel(Direction Direction, bool IsLocked)
{
    /// <summary>
    /// Gets the directional arrow symbol.
    /// </summary>
    public string Symbol => Direction switch
    {
        Direction.North => "↑",
        Direction.South => "↓",
        Direction.East => "→",
        Direction.West => "←",
        Direction.Up => "⬆",
        Direction.Down => "⬇",
        _ => "?"
    };

    /// <summary>
    /// Gets the button label text.
    /// </summary>
    public string Label => $"{Symbol} {Direction}";

    /// <summary>
    /// Gets the tooltip text.
    /// </summary>
    public string Tooltip => IsLocked ? $"{Direction} (Locked)" : Direction.ToString();
}
