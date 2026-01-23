using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.Extensions;

/// <summary>
/// Extension methods for <see cref="LeapDistance"/> enum.
/// </summary>
public static class LeapDistanceExtensions
{
    /// <summary>
    /// Gets the base DC (in successes required) for this leap distance.
    /// </summary>
    /// <param name="distance">The leap distance category.</param>
    /// <returns>The number of net successes required.</returns>
    /// <example>
    /// <code>
    /// var dc = LeapDistance.Long.GetBaseDc(); // Returns 3
    /// </code>
    /// </example>
    public static int GetBaseDc(this LeapDistance distance)
    {
        return distance switch
        {
            LeapDistance.Short => 1,
            LeapDistance.Medium => 2,
            LeapDistance.Long => 3,
            LeapDistance.Extreme => 4,
            LeapDistance.Heroic => 5,
            _ => 2
        };
    }

    /// <summary>
    /// Gets the maximum distance in feet for this category.
    /// </summary>
    /// <param name="distance">The leap distance category.</param>
    /// <returns>Maximum feet for this category.</returns>
    /// <example>
    /// <code>
    /// var maxFeet = LeapDistance.Medium.GetMaxFeet(); // Returns 10
    /// </code>
    /// </example>
    public static int GetMaxFeet(this LeapDistance distance)
    {
        return distance switch
        {
            LeapDistance.Short => 5,
            LeapDistance.Medium => 10,
            LeapDistance.Long => 15,
            LeapDistance.Extreme => 20,
            LeapDistance.Heroic => 25,
            _ => 10
        };
    }

    /// <summary>
    /// Gets the minimum distance in feet for this category.
    /// </summary>
    /// <param name="distance">The leap distance category.</param>
    /// <returns>Minimum feet for this category.</returns>
    /// <example>
    /// <code>
    /// var minFeet = LeapDistance.Long.GetMinFeet(); // Returns 11
    /// </code>
    /// </example>
    public static int GetMinFeet(this LeapDistance distance)
    {
        return distance switch
        {
            LeapDistance.Short => 1,
            LeapDistance.Medium => 6,
            LeapDistance.Long => 11,
            LeapDistance.Extreme => 16,
            LeapDistance.Heroic => 21,
            _ => 1
        };
    }

    /// <summary>
    /// Gets the base stamina cost for this leap distance.
    /// </summary>
    /// <param name="distance">The leap distance category.</param>
    /// <returns>Stamina points consumed by the leap.</returns>
    /// <remarks>
    /// Formula: Distance / 10 (round up).
    /// <list type="bullet">
    ///   <item><description>Short/Medium: 1 stamina</description></item>
    ///   <item><description>Long/Extreme: 2 stamina</description></item>
    ///   <item><description>Heroic: 3 stamina</description></item>
    /// </list>
    /// </remarks>
    public static int GetBaseStaminaCost(this LeapDistance distance)
    {
        return distance switch
        {
            LeapDistance.Short => 1,
            LeapDistance.Medium => 1,
            LeapDistance.Long => 2,
            LeapDistance.Extreme => 2,
            LeapDistance.Heroic => 3,
            _ => 1
        };
    }

    /// <summary>
    /// Gets a human-readable description of the leap distance.
    /// </summary>
    /// <param name="distance">The leap distance category.</param>
    /// <returns>A descriptive string for UI display.</returns>
    /// <example>
    /// <code>
    /// var desc = LeapDistance.Long.GetDescription(); // "Long (11-15ft) - DC 3"
    /// </code>
    /// </example>
    public static string GetDescription(this LeapDistance distance)
    {
        return distance switch
        {
            LeapDistance.Short => "Short (up to 5ft) - DC 1",
            LeapDistance.Medium => "Medium (6-10ft) - DC 2",
            LeapDistance.Long => "Long (11-15ft) - DC 3",
            LeapDistance.Extreme => "Extreme (16-20ft) - DC 4",
            LeapDistance.Heroic => "Heroic (21-25ft) - DC 5",
            _ => "Unknown distance"
        };
    }

    /// <summary>
    /// Gets a short display name for the distance category.
    /// </summary>
    /// <param name="distance">The leap distance category.</param>
    /// <returns>The category name.</returns>
    public static string GetDisplayName(this LeapDistance distance)
    {
        return distance switch
        {
            LeapDistance.Short => "Short",
            LeapDistance.Medium => "Medium",
            LeapDistance.Long => "Long",
            LeapDistance.Extreme => "Extreme",
            LeapDistance.Heroic => "Heroic",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Determines the leap distance category from a distance in feet.
    /// </summary>
    /// <param name="distanceFeet">The distance to leap in feet.</param>
    /// <returns>The appropriate LeapDistance category.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when distance is less than 1 or exceeds 25 feet (Heroic maximum).
    /// </exception>
    /// <example>
    /// <code>
    /// var category = LeapDistanceExtensions.FromFeet(12); // Returns LeapDistance.Long
    /// </code>
    /// </example>
    public static LeapDistance FromFeet(int distanceFeet)
    {
        return distanceFeet switch
        {
            <= 0 => throw new ArgumentOutOfRangeException(
                nameof(distanceFeet), "Distance must be positive."),
            <= 5 => LeapDistance.Short,
            <= 10 => LeapDistance.Medium,
            <= 15 => LeapDistance.Long,
            <= 20 => LeapDistance.Extreme,
            <= 25 => LeapDistance.Heroic,
            _ => throw new ArgumentOutOfRangeException(
                nameof(distanceFeet),
                $"Distance {distanceFeet}ft exceeds maximum leap distance of 25ft.")
        };
    }

    /// <summary>
    /// Gets whether this distance category requires Master rank.
    /// </summary>
    /// <param name="distance">The leap distance category.</param>
    /// <returns>True if Master rank is recommended.</returns>
    public static bool RequiresMasterRank(this LeapDistance distance)
    {
        return distance == LeapDistance.Heroic;
    }
}
