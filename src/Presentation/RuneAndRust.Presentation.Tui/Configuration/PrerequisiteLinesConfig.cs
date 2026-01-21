// ═══════════════════════════════════════════════════════════════════════════════
// PrerequisiteLinesConfig.cs
// Configuration for prerequisite line display settings.
// Version: 0.13.2c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.Configuration;

/// <summary>
/// Configuration for prerequisite line display settings.
/// </summary>
/// <remarks>
/// <para>Loaded from <c>config/prerequisite-lines.json</c>.</para>
/// <para>Settings include:</para>
/// <list type="bullet">
///   <item><description>Line characters (horizontal, vertical, corners)</description></item>
///   <item><description>Colors for satisfied/unsatisfied states</description></item>
///   <item><description>Default node dimensions for edge calculation</description></item>
/// </list>
/// </remarks>
public class PrerequisiteLinesConfig
{
    // ═══════════════════════════════════════════════════════════════
    // NODE DIMENSIONS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Default node width for edge calculations when bounds unavailable.
    /// </summary>
    /// <value>Default: 11 characters.</value>
    public int DefaultNodeWidth { get; set; } = 11;

    /// <summary>
    /// Default node height for edge calculations when bounds unavailable.
    /// </summary>
    /// <value>Default: 3 rows.</value>
    public int DefaultNodeHeight { get; set; } = 3;

    // ═══════════════════════════════════════════════════════════════
    // LINE CHARACTERS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Character for horizontal line segments.
    /// </summary>
    /// <value>Default: ─ (Box Drawings Light Horizontal).</value>
    public char HorizontalLineCharacter { get; set; } = '─';

    /// <summary>
    /// Character for vertical line segments.
    /// </summary>
    /// <value>Default: │ (Box Drawings Light Vertical).</value>
    public char VerticalLineCharacter { get; set; } = '│';

    /// <summary>
    /// Character for top-right corner (down and left).
    /// </summary>
    /// <value>Default: ┐.</value>
    public char CornerTopRightCharacter { get; set; } = '┐';

    /// <summary>
    /// Character for bottom-right corner (up and left).
    /// </summary>
    /// <value>Default: ┘.</value>
    public char CornerBottomRightCharacter { get; set; } = '┘';

    /// <summary>
    /// Character for top-left corner (down and right).
    /// </summary>
    /// <value>Default: ┌.</value>
    public char CornerTopLeftCharacter { get; set; } = '┌';

    /// <summary>
    /// Character for bottom-left corner (up and right).
    /// </summary>
    /// <value>Default: └.</value>
    public char CornerBottomLeftCharacter { get; set; } = '└';

    /// <summary>
    /// Character for connection points at node edges.
    /// </summary>
    /// <value>Default: ─ (same as horizontal).</value>
    public char ConnectionPointCharacter { get; set; } = '─';

    /// <summary>
    /// Character for satisfied (solid) lines.
    /// </summary>
    /// <value>Default: ─.</value>
    public char SatisfiedLineCharacter { get; set; } = '─';

    /// <summary>
    /// Character for unsatisfied (dashed) lines.
    /// </summary>
    /// <value>Default: ─ (rendered with gaps for dashed effect).</value>
    public char UnsatisfiedLineCharacter { get; set; } = '─';

    // ═══════════════════════════════════════════════════════════════
    // COLORS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Color for satisfied lines.
    /// </summary>
    /// <value>Default: Green.</value>
    public ConsoleColor SatisfiedLineColor { get; set; } = ConsoleColor.Green;

    /// <summary>
    /// Color for unsatisfied lines.
    /// </summary>
    /// <value>Default: DarkGray.</value>
    public ConsoleColor UnsatisfiedLineColor { get; set; } = ConsoleColor.DarkGray;

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a default configuration instance.
    /// </summary>
    /// <returns>A new PrerequisiteLinesConfig with default values.</returns>
    public static PrerequisiteLinesConfig CreateDefault() => new();

    /// <summary>
    /// Creates a configuration with ASCII-only characters (no unicode).
    /// </summary>
    /// <returns>A configuration using only ASCII characters.</returns>
    /// <remarks>
    /// Useful for terminals without Unicode support.
    /// </remarks>
    public static PrerequisiteLinesConfig CreateAsciiOnly()
    {
        return new PrerequisiteLinesConfig
        {
            HorizontalLineCharacter = '-',
            VerticalLineCharacter = '|',
            CornerTopRightCharacter = '+',
            CornerBottomRightCharacter = '+',
            CornerTopLeftCharacter = '+',
            CornerBottomLeftCharacter = '+',
            ConnectionPointCharacter = '-',
            SatisfiedLineCharacter = '-',
            UnsatisfiedLineCharacter = '-'
        };
    }
}
