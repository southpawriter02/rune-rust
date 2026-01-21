// ═══════════════════════════════════════════════════════════════════════════════
// StatisticDisplayDto.cs
// Data transfer object for displaying statistics with session and all-time values.
// Version: 0.13.4b
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// Data transfer object for displaying a statistic with session and all-time values.
/// </summary>
/// <remarks>
/// <para>
/// This DTO is used by the <see cref="UI.StatisticsDashboard"/> to display
/// statistics in a three-column comparison format showing the statistic name,
/// current session value, all-time cumulative value, and calculated delta.
/// </para>
/// <para>
/// For percentage-based statistics (like hit rates), the <see cref="IsPercentage"/>
/// property should be set to true. Percentage values are stored as integers
/// multiplied by 10 (e.g., 942 represents 94.2%) for precision without floating point.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var stat = new StatisticDisplayDto
/// {
///     Name = "Monsters Defeated",
///     SessionValue = 47,
///     AllTimeValue = 1284,
///     IsPercentage = false,
///     Category = StatisticCategory.Combat
/// };
/// </code>
/// </example>
public class StatisticDisplayDto
{
    /// <summary>
    /// Gets or sets the human-readable name of the statistic.
    /// </summary>
    /// <example>"Monsters Defeated", "Total Damage Dealt", "Critical Hit Rate"</example>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the value accumulated during the current session.
    /// </summary>
    /// <remarks>
    /// For percentage values, this is the percentage multiplied by 10
    /// (e.g., 942 for 94.2%).
    /// </remarks>
    public int SessionValue { get; set; }

    /// <summary>
    /// Gets or sets the cumulative all-time value across all sessions.
    /// </summary>
    /// <remarks>
    /// For percentage values, this is the percentage multiplied by 10
    /// (e.g., 917 for 91.7%).
    /// </remarks>
    public int AllTimeValue { get; set; }

    /// <summary>
    /// Gets the delta (change) value, which equals the session value.
    /// </summary>
    /// <remarks>
    /// The delta represents how much was added during this session.
    /// A positive value indicates an increase, displayed with a "+" prefix.
    /// </remarks>
    public int Delta => SessionValue;

    /// <summary>
    /// Gets or sets whether this statistic represents a percentage value.
    /// </summary>
    /// <remarks>
    /// When true, values are stored as int * 10 (e.g., 942 = 94.2%) and
    /// formatting will include the "%" suffix.
    /// </remarks>
    public bool IsPercentage { get; set; }

    /// <summary>
    /// Gets or sets the category this statistic belongs to.
    /// </summary>
    public StatisticCategory Category { get; set; }
}
