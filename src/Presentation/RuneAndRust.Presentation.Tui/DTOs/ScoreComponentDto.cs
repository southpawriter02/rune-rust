// ═══════════════════════════════════════════════════════════════════════════════
// ScoreComponentDto.cs
// Data transfer object for score breakdown components.
// Version: 0.13.4c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// Data transfer object for a score breakdown component.
/// </summary>
/// <remarks>
/// <para>
/// Represents a single component of the score calculation, such as
/// monsters killed, rooms discovered, level multiplier, or death penalty.
/// </para>
/// <para>Component types:</para>
/// <list type="bullet">
///   <item><description>Base: Normal point-earning components (monsters, rooms, gold)</description></item>
///   <item><description>Multiplier: Scaling modifiers (level bonus)</description></item>
///   <item><description>Penalty: Negative modifiers (death penalty)</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Base component with count and per-unit points
/// var monstersComponent = new ScoreComponentDto
/// {
///     Name = "Monsters Killed",
///     Count = 127,
///     PointsEach = 10,
///     TotalPoints = 1270
/// };
/// 
/// // Multiplier component (TotalPoints stores multiplier * 100)
/// var levelMultiplier = new ScoreComponentDto
/// {
///     Name = "Level Multiplier",
///     TotalPoints = 180,  // 1.8x
///     IsMultiplier = true
/// };
/// 
/// // Penalty component
/// var deathPenalty = new ScoreComponentDto
/// {
///     Name = "Death Penalty",
///     TotalPoints = -300,
///     IsPenalty = true
/// };
/// </code>
/// </example>
public class ScoreComponentDto
{
    /// <summary>
    /// Gets or sets the component name (e.g., "Monsters Killed").
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the count of items (e.g., number of monsters killed).
    /// </summary>
    /// <remarks>
    /// Only relevant for base components that have a per-unit value.
    /// Set to 0 for flat values, multipliers, and penalties.
    /// </remarks>
    public int Count { get; set; }

    /// <summary>
    /// Gets or sets the points earned per unit.
    /// </summary>
    /// <remarks>
    /// For example, 10 points per monster killed.
    /// Set to 0 for flat values, multipliers, and penalties.
    /// </remarks>
    public int PointsEach { get; set; }

    /// <summary>
    /// Gets or sets the total points for this component.
    /// </summary>
    /// <remarks>
    /// For base components: <c>Count × PointsEach</c>.
    /// For multipliers: The multiplier × 100 (e.g., 180 for 1.8x).
    /// For penalties: Negative value (e.g., -300).
    /// </remarks>
    public int TotalPoints { get; set; }

    /// <summary>
    /// Gets or sets whether this is a multiplier component.
    /// </summary>
    public bool IsMultiplier { get; set; }

    /// <summary>
    /// Gets or sets whether this is a penalty (negative points).
    /// </summary>
    public bool IsPenalty { get; set; }
}
