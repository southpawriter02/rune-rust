namespace RuneAndRust.Application.Configuration;

/// <summary>
/// Configuration for the theme system.
/// </summary>
public class ThemeConfiguration
{
    /// <summary>
    /// The currently active theme ID.
    /// </summary>
    public string ActiveTheme { get; set; } = "dark_fantasy";

    /// <summary>
    /// Available theme presets.
    /// </summary>
    public IReadOnlyDictionary<string, ThemePreset> Themes { get; init; } =
        new Dictionary<string, ThemePreset>();
}

/// <summary>
/// A theme preset with descriptor overrides and term emphasis.
/// </summary>
public class ThemePreset
{
    /// <summary>
    /// Theme identifier.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Display name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Description of the theme.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Descriptor pool overrides for this theme.
    /// Key is original pool path, value is override pool path.
    /// </summary>
    public IReadOnlyDictionary<string, string> DescriptorOverrides { get; init; } =
        new Dictionary<string, string>();

    /// <summary>
    /// Terms excluded from this theme.
    /// </summary>
    public IReadOnlyList<string> ExcludedTerms { get; init; } = [];

    /// <summary>
    /// Terms emphasized (weighted higher) in this theme.
    /// </summary>
    public IReadOnlyList<string> EmphasizedTerms { get; init; } = [];
}
