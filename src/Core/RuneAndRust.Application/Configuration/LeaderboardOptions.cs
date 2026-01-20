// ═══════════════════════════════════════════════════════════════════════════════
// LeaderboardOptions.cs
// Configuration options for the leaderboard system.
// Version: 0.12.1c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Configuration;

/// <summary>
/// Configuration options for the leaderboard system.
/// </summary>
/// <remarks>
/// <para>
/// This class is used for configuration binding via the IOptions pattern.
/// It specifies where leaderboard data is persisted and limits.
/// </para>
/// <para>
/// Typical configuration in appsettings.json:
/// </para>
/// <code>
/// {
///   "Leaderboard": {
///     "DataFilePath": "data/leaderboard.json",
///     "MaxEntriesPerCategory": 100
///   }
/// }
/// </code>
/// </remarks>
public class LeaderboardOptions
{
    /// <summary>
    /// The configuration section name used for binding.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this constant when configuring options in DI:
    /// <c>services.Configure&lt;LeaderboardOptions&gt;(configuration.GetSection(LeaderboardOptions.SectionName));</c>
    /// </para>
    /// </remarks>
    public const string SectionName = "Leaderboard";

    /// <summary>
    /// Gets or sets the maximum number of entries per category.
    /// Default is 100.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When the number of entries in a category exceeds this limit,
    /// the lowest-scoring entries are removed (or highest for Speedrun).
    /// </para>
    /// </remarks>
    public int MaxEntriesPerCategory { get; set; } = 100;

    /// <summary>
    /// Gets or sets the path to the leaderboard data file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This path is relative to the application's working directory.
    /// The default value points to the standard data location.
    /// </para>
    /// <para>
    /// The file stores leaderboard entries in JSON format and is
    /// automatically created if it does not exist.
    /// </para>
    /// </remarks>
    public string DataFilePath { get; set; } = "data/leaderboard.json";
}
