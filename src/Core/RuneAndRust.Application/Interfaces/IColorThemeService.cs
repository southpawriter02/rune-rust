namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Manages color themes for styled console output.
/// </summary>
/// <remarks>
/// Provides methods to:
/// <list type="bullet">
/// <item><description>Switch between available themes</description></item>
/// <item><description>Get colors for specific message types</description></item>
/// <item><description>Write colored text to the terminal</description></item>
/// </list>
/// </remarks>
public interface IColorThemeService
{
    /// <summary>
    /// Gets the current theme name.
    /// </summary>
    string CurrentTheme { get; }
    
    /// <summary>
    /// Sets the active theme.
    /// </summary>
    /// <param name="themeName">Theme name to activate (case-insensitive).</param>
    /// <exception cref="ArgumentException">Thrown when theme is not found.</exception>
    void SetTheme(string themeName);
    
    /// <summary>
    /// Gets the color for a message type in the current theme.
    /// </summary>
    /// <param name="type">The message type.</param>
    /// <returns>ConsoleColor for the type.</returns>
    ConsoleColor GetColor(MessageType type);
    
    /// <summary>
    /// Gets available theme names.
    /// </summary>
    IReadOnlyList<string> AvailableThemes { get; }
    
    /// <summary>
    /// Writes colored text to the terminal at current cursor position.
    /// </summary>
    /// <param name="text">Text to write.</param>
    /// <param name="type">Message type for color lookup.</param>
    void WriteColored(string text, MessageType type);
    
    /// <summary>
    /// Writes colored text to a specific position.
    /// </summary>
    /// <param name="x">Column position (0-indexed).</param>
    /// <param name="y">Row position (0-indexed).</param>
    /// <param name="text">Text to write.</param>
    /// <param name="type">Message type for color lookup.</param>
    void WriteColoredAt(int x, int y, string text, MessageType type);
}
