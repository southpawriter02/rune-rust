namespace RuneAndRust.Presentation.Configuration;

/// <summary>
/// Represents a color threshold for a bar type.
/// </summary>
/// <param name="Percent">Percentage at or below which this color applies.</param>
/// <param name="Color">ConsoleColor for this threshold.</param>
public record ColorThreshold(int Percent, ConsoleColor Color);

/// <summary>
/// Characters used for bar rendering.
/// </summary>
/// <param name="Filled">Filled portion character (Unicode).</param>
/// <param name="Empty">Empty portion character (Unicode).</param>
/// <param name="FilledFallback">Filled portion fallback (ASCII).</param>
/// <param name="EmptyFallback">Empty portion fallback (ASCII).</param>
public record BarCharacters(
    char Filled = '█',
    char Empty = '░',
    char FilledFallback = '#',
    char EmptyFallback = '-');
