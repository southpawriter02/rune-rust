// ═══════════════════════════════════════════════════════════════════════════════
// LineState.cs
// Represents the visual state of a prerequisite line.
// Version: 0.13.2c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.Enums;

/// <summary>
/// Represents the visual state of a prerequisite line between ability tree nodes.
/// </summary>
/// <remarks>
/// <para>The line state determines visual styling:</para>
/// <list type="bullet">
///   <item><description><see cref="Satisfied"/>: Solid green line - prerequisite is met</description></item>
///   <item><description><see cref="Unsatisfied"/>: Dashed gray line - prerequisite not met</description></item>
/// </list>
/// <para>State is determined based on player's ability progress.</para>
/// </remarks>
/// <example>
/// <code>
/// var state = progress.HasUnlocked(prerequisiteNodeId) 
///     ? LineState.Satisfied 
///     : LineState.Unsatisfied;
/// </code>
/// </example>
public enum LineState
{
    /// <summary>
    /// Prerequisite is satisfied - displayed as solid line.
    /// </summary>
    /// <remarks>
    /// <para>Visual properties:</para>
    /// <list type="bullet">
    ///   <item><description>Color: Green (default)</description></item>
    ///   <item><description>Pattern: Continuous solid line</description></item>
    ///   <item><description>Character: ─</description></item>
    /// </list>
    /// </remarks>
    Satisfied,

    /// <summary>
    /// Prerequisite is unsatisfied - displayed as dashed line.
    /// </summary>
    /// <remarks>
    /// <para>Visual properties:</para>
    /// <list type="bullet">
    ///   <item><description>Color: DarkGray (default)</description></item>
    ///   <item><description>Pattern: Dashed (alternating character/space)</description></item>
    ///   <item><description>Character: ─ alternating with space</description></item>
    /// </list>
    /// </remarks>
    Unsatisfied
}
