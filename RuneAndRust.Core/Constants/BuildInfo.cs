namespace RuneAndRust.Core.Constants;

/// <summary>
/// Compile-time build information for version tracking and display.
/// Updated each release cycle by The Architect.
/// </summary>
/// <remarks>
/// v0.3.24b: Initial implementation for alpha packaging.
/// See: SPEC-BUILD-001 for Release Packaging design.
/// </remarks>
public static class BuildInfo
{
    /// <summary>
    /// Semantic version string (Major.Minor.Patch[-prerelease]).
    /// </summary>
    public const string Version = "0.4.3e";

    /// <summary>
    /// Human-readable release codename.
    /// </summary>
    public const string Codename = "The Resonance";

    /// <summary>
    /// Build timestamp (ISO 8601 format).
    /// Updated by CI/CD pipeline or manually before release.
    /// </summary>
    public const string BuildDate = "2026-01-01";

    /// <summary>
    /// Target .NET runtime version.
    /// </summary>
    public const string RuntimeVersion = "net9.0";

    /// <summary>
    /// Full version string for display (e.g., "v0.4.0-alpha \"The Saga\"").
    /// </summary>
    public static string FullVersionString => $"v{Version} \"{Codename}\"";

    /// <summary>
    /// Short version string for compact display (e.g., "v0.4.0-alpha").
    /// </summary>
    public static string ShortVersionString => $"v{Version}";

    /// <summary>
    /// Whether this is a pre-release build.
    /// </summary>
    public static bool IsPreRelease => Version.Contains('-');

    /// <summary>
    /// Whether this is a debug build.
    /// </summary>
#if DEBUG
    public static bool IsDebugBuild => true;
#else
    public static bool IsDebugBuild => false;
#endif

    /// <summary>
    /// Build configuration string ("Debug" or "Release").
    /// </summary>
#if DEBUG
    public const string Configuration = "Debug";
#else
    public const string Configuration = "Release";
#endif
}
