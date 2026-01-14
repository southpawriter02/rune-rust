namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a single frame in an animation sequence.
/// </summary>
/// <param name="TextTemplate">Text to display with template variables.</param>
/// <param name="DurationMs">How long to display this frame.</param>
/// <param name="Color">Optional foreground color.</param>
/// <param name="PositionX">Optional X offset from animation area.</param>
/// <param name="PositionY">Optional Y offset from animation area.</param>
/// <param name="Center">Whether to center the text horizontally.</param>
/// <param name="ClearPrevious">Whether to clear before next frame.</param>
public record AnimationFrame(
    string TextTemplate,
    int DurationMs,
    ConsoleColor? Color = null,
    int? PositionX = null,
    int? PositionY = null,
    bool Center = false,
    bool ClearPrevious = true);
