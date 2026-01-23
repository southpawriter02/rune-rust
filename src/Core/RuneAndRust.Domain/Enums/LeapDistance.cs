namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Categorizes leap distances with associated difficulty classes.
/// </summary>
/// <remarks>
/// <para>
/// Each leap distance category represents a range of horizontal distances
/// with an associated base DC for the Acrobatics skill check.
/// </para>
/// <para>
/// DCs are expressed in success-counting terms (number of net successes required):
/// <list type="bullet">
///   <item><description>Short (5ft): DC 1 - Easy for trained characters</description></item>
///   <item><description>Medium (10ft): DC 2 - Standard athletic feat</description></item>
///   <item><description>Long (15ft): DC 3 - Challenging, requires good Acrobatics</description></item>
///   <item><description>Extreme (20ft): DC 4 - Very difficult, approaching limits</description></item>
///   <item><description>Heroic (25ft): DC 5 - Near-impossible, Master rank recommended</description></item>
/// </list>
/// </para>
/// </remarks>
public enum LeapDistance
{
    /// <summary>
    /// Short leap of up to 5 feet. DC 1.
    /// </summary>
    /// <remarks>
    /// Trivial for most characters. Useful for small gaps, stepping stones.
    /// </remarks>
    Short = 0,

    /// <summary>
    /// Medium leap of 6-10 feet. DC 2.
    /// </summary>
    /// <remarks>
    /// Standard athletic jump. Most gaps in urban environments.
    /// </remarks>
    Medium = 1,

    /// <summary>
    /// Long leap of 11-15 feet. DC 3.
    /// </summary>
    /// <remarks>
    /// Challenging jump requiring commitment. Rooftop gaps, wide chasms.
    /// </remarks>
    Long = 2,

    /// <summary>
    /// Extreme leap of 16-20 feet. DC 4.
    /// </summary>
    /// <remarks>
    /// Pushing physical limits. Running start highly recommended.
    /// </remarks>
    Extreme = 3,

    /// <summary>
    /// Heroic leap of 21-25 feet. DC 5.
    /// </summary>
    /// <remarks>
    /// Near-impossible without Master rank or specialization abilities.
    /// Gantry-Runner [Death-Defying Leap] extends this range.
    /// </remarks>
    Heroic = 4
}
