namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Descriptive periods of the day for time-based gameplay.
/// </summary>
/// <remarks>
/// <para>
/// Used with <see cref="ValueObjects.GameTime"/> to determine:
/// <list type="bullet">
///   <item><description>Outdoor light levels (Dawn/Dusk → Dim, Night → Dark)</description></item>
///   <item><description>NPC schedules and availability</description></item>
///   <item><description>Time-sensitive event triggers</description></item>
/// </list>
/// </para>
/// </remarks>
public enum TimeOfDay
{
    /// <summary>5:00 - 7:00, transitional light (Dim).</summary>
    Dawn,

    /// <summary>7:00 - 12:00, full daylight (Bright).</summary>
    Morning,

    /// <summary>12:00 - 14:00, peak daylight (Bright).</summary>
    Noon,

    /// <summary>14:00 - 17:00, full daylight (Bright).</summary>
    Afternoon,

    /// <summary>17:00 - 20:00, transitional light (Dim).</summary>
    Dusk,

    /// <summary>20:00 - 24:00, night time (Dark).</summary>
    Evening,

    /// <summary>0:00 - 5:00, night time (Dark).</summary>
    Night
}
