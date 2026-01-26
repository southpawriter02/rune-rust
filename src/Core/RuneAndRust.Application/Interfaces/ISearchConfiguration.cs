namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Configuration settings for search operations.
/// </summary>
/// <remarks>
/// Provides configurable values for search time costs, bonuses, and
/// cooldown periods. Default values aligned with v0.15.6f design spec.
/// </remarks>
public interface ISearchConfiguration
{
    /// <summary>
    /// The bonus applied to perception during active searching.
    /// </summary>
    /// <remarks>
    /// Default: +2 as per v0.15.6f design specification.
    /// </remarks>
    int ActiveSearchBonus { get; }

    /// <summary>
    /// Time in minutes to search a small room.
    /// </summary>
    /// <remarks>
    /// Default: 5 minutes (closet, alcove, small chamber).
    /// </remarks>
    int SmallRoomSearchTime { get; }

    /// <summary>
    /// Time in minutes to search a medium room.
    /// </summary>
    /// <remarks>
    /// Default: 10 minutes (standard living quarters, office).
    /// </remarks>
    int MediumRoomSearchTime { get; }

    /// <summary>
    /// Time in minutes to search a large room.
    /// </summary>
    /// <remarks>
    /// Default: 15 minutes (great hall, warehouse section).
    /// </remarks>
    int LargeRoomSearchTime { get; }

    /// <summary>
    /// Time in minutes to search an extra-large room.
    /// </summary>
    /// <remarks>
    /// Default: 20 minutes (hangar bay, cavern).
    /// </remarks>
    int XLargeRoomSearchTime { get; }

    /// <summary>
    /// Time in minutes to search a single container.
    /// </summary>
    /// <remarks>
    /// Default: 2 minutes.
    /// </remarks>
    int ContainerSearchTime { get; }

    /// <summary>
    /// Whether searching the same area again yields different results.
    /// </summary>
    bool AllowRepeatedSearches { get; }

    /// <summary>
    /// Cooldown in minutes before the same area can be searched again.
    /// </summary>
    int SearchCooldownMinutes { get; }
}
