// ═══════════════════════════════════════════════════════════════════════════════
// AchievementOptions.cs
// Configuration options for the achievement system.
// Version: 0.12.1a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Infrastructure.Configuration;

/// <summary>
/// Configuration options for the achievement system.
/// </summary>
/// <remarks>
/// <para>
/// This class is used for configuration binding via the IOptions pattern.
/// It specifies where achievement definitions are loaded from.
/// </para>
/// <para>
/// Typical configuration in appsettings.json:
/// </para>
/// <code>
/// {
///   "Achievements": {
///     "ConfigurationPath": "config/achievements.json"
///   }
/// }
/// </code>
/// </remarks>
public class AchievementOptions
{
    /// <summary>
    /// The configuration section name used for binding.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this constant when configuring options in DI:
    /// <c>services.Configure&lt;AchievementOptions&gt;(configuration.GetSection(AchievementOptions.SectionName));</c>
    /// </para>
    /// </remarks>
    public const string SectionName = "Achievements";

    /// <summary>
    /// Gets or sets the path to the achievements configuration file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This path is relative to the application's working directory.
    /// The default value points to the standard config location.
    /// </para>
    /// <para>
    /// The file should be a JSON file following the achievements.schema.json format
    /// containing an array of achievement definitions with their conditions.
    /// </para>
    /// </remarks>
    public string ConfigurationPath { get; set; } = "config/achievements.json";
}
