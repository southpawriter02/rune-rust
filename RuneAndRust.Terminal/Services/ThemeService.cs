using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Settings;
using Spectre.Console;

namespace RuneAndRust.Terminal.Services;

/// <summary>
/// Provides themed color palettes for accessibility support (v0.3.9b).
/// Manages 5 color palettes: Standard, HighContrast, Protanopia, Deuteranopia, Tritanopia.
/// </summary>
public class ThemeService : IThemeService
{
    private readonly ILogger<ThemeService> _logger;
    private readonly Dictionary<ThemeType, Dictionary<string, string>> _palettes;

    /// <summary>
    /// Gets the current active theme from GameSettings.
    /// </summary>
    public ThemeType CurrentTheme => GameSettings.Theme;

    public ThemeService(ILogger<ThemeService> logger)
    {
        _logger = logger;
        _palettes = InitializePalettes();
        _logger.LogInformation("[Theme] Initialized with {Theme} theme", CurrentTheme);
    }

    /// <summary>
    /// Gets the color string for a semantic role in the current theme.
    /// </summary>
    public string GetColor(string colorRole)
    {
        if (_palettes.TryGetValue(CurrentTheme, out var palette) &&
            palette.TryGetValue(colorRole, out var color))
        {
            _logger.LogTrace("[Theme] Resolved {Role} to {Color}", colorRole, color);
            return color;
        }

        // Fallback to Standard palette
        if (_palettes.TryGetValue(ThemeType.Standard, out var standardPalette) &&
            standardPalette.TryGetValue(colorRole, out var standardColor))
        {
            _logger.LogTrace("[Theme] Role {Role} not in {Theme}, falling back to Standard: {Color}",
                colorRole, CurrentTheme, standardColor);
            return standardColor;
        }

        _logger.LogWarning("[Theme] Unknown role {Role}, falling back to grey", colorRole);
        return "grey";
    }

    /// <summary>
    /// Gets the Spectre.Console Color object for a semantic role.
    /// </summary>
    public Color GetColorObject(string colorRole)
    {
        var colorString = GetColor(colorRole);
        return ParseColor(colorString);
    }

    /// <summary>
    /// Changes the active theme and updates GameSettings.
    /// </summary>
    public void SetTheme(ThemeType theme)
    {
        var oldTheme = CurrentTheme;
        GameSettings.Theme = theme;
        _logger.LogInformation("[Theme] Changed from {OldTheme} to {NewTheme}", oldTheme, theme);
    }

    /// <summary>
    /// Parses a color string to a Spectre.Console Color object.
    /// Handles "bold" prefix by stripping it (boldness is a style, not a color).
    /// </summary>
    private static Color ParseColor(string colorString)
    {
        // Strip "bold " prefix if present - boldness is handled separately in markup
        var cleanColor = colorString.StartsWith("bold ", StringComparison.OrdinalIgnoreCase)
            ? colorString[5..]
            : colorString;

        return cleanColor.ToLowerInvariant() switch
        {
            "red" => Color.Red,
            "red1" => Color.Red1,
            "green" => Color.Green,
            "blue" => Color.Blue,
            "cyan" => Color.Cyan1,
            "cyan1" => Color.Cyan1,
            "yellow" => Color.Yellow,
            "orange1" => Color.Orange1,
            "magenta1" => Color.Magenta1,
            "purple" => Color.Purple,
            "gold1" => Color.Gold1,
            "white" => Color.White,
            "grey" => Color.Grey,
            "gray" => Color.Grey,
            _ => Color.Grey
        };
    }

    /// <summary>
    /// Initializes all 5 color palettes with semantic role mappings.
    /// </summary>
    private static Dictionary<ThemeType, Dictionary<string, string>> InitializePalettes()
    {
        return new Dictionary<ThemeType, Dictionary<string, string>>
        {
            [ThemeType.Standard] = new()
            {
                // Entity Colors
                ["PlayerColor"] = "cyan",
                ["EnemyColor"] = "red",
                ["AllyColor"] = "green",
                ["NeutralColor"] = "grey",

                // Health States
                ["HealthFull"] = "green",
                ["HealthHigh"] = "yellow",
                ["HealthLow"] = "orange1",
                ["HealthCritical"] = "red",

                // Resource Colors
                ["StaminaColor"] = "cyan1",
                ["StressLow"] = "grey",
                ["StressMid"] = "yellow",
                ["StressHigh"] = "magenta1",

                // Quality Tiers
                ["QualityJunk"] = "grey",
                ["QualityCommon"] = "white",
                ["QualityUncommon"] = "green",
                ["QualityRare"] = "blue",
                ["QualityLegendary"] = "gold1",

                // Status Colors
                ["SuccessColor"] = "green",
                ["WarningColor"] = "yellow",
                ["ErrorColor"] = "red",
                ["InfoColor"] = "cyan",

                // UI Elements
                ["HeaderColor"] = "gold1",
                ["BorderColor"] = "grey",
                ["ActiveColor"] = "yellow",
                ["InactiveColor"] = "grey",

                // Danger Levels
                ["DangerSafe"] = "grey",
                ["DangerUnstable"] = "yellow",
                ["DangerHostile"] = "orange1",
                ["DangerLethal"] = "red",

                // Biome Colors (v0.3.14a)
                ["BiomeRuin"] = "grey",
                ["BiomeIndustrial"] = "orange1",
                ["BiomeOrganic"] = "green",
                ["BiomeVoid"] = "purple",

                // UI Structural Colors (v0.3.14a)
                ["DimColor"] = "grey",
                ["SeparatorColor"] = "grey",
                ["LabelColor"] = "grey",
                ["InputColor"] = "cyan",
                ["BorderActive"] = "yellow",
                ["BorderInactive"] = "grey",
                ["NarrativeColor"] = "grey",
                ["TabActive"] = "gold1"
            },

            [ThemeType.HighContrast] = new()
            {
                // Entity Colors - Maximum contrast
                ["PlayerColor"] = "white",
                ["EnemyColor"] = "yellow",
                ["AllyColor"] = "bold green",
                ["NeutralColor"] = "white",

                // Health States - Bold for visibility
                ["HealthFull"] = "bold green",
                ["HealthHigh"] = "bold yellow",
                ["HealthLow"] = "bold orange1",
                ["HealthCritical"] = "bold red",

                // Resource Colors
                ["StaminaColor"] = "bold cyan",
                ["StressLow"] = "grey",
                ["StressMid"] = "bold yellow",
                ["StressHigh"] = "bold magenta1",

                // Quality Tiers - Bold for readability
                ["QualityJunk"] = "grey",
                ["QualityCommon"] = "white",
                ["QualityUncommon"] = "bold green",
                ["QualityRare"] = "bold blue",
                ["QualityLegendary"] = "bold gold1",

                // Status Colors
                ["SuccessColor"] = "bold green",
                ["WarningColor"] = "bold yellow",
                ["ErrorColor"] = "bold red",
                ["InfoColor"] = "bold cyan",

                // UI Elements
                ["HeaderColor"] = "bold gold1",
                ["BorderColor"] = "white",
                ["ActiveColor"] = "bold yellow",
                ["InactiveColor"] = "grey",

                // Danger Levels
                ["DangerSafe"] = "white",
                ["DangerUnstable"] = "bold yellow",
                ["DangerHostile"] = "bold orange1",
                ["DangerLethal"] = "bold red",

                // Biome Colors (v0.3.14a) - High visibility
                ["BiomeRuin"] = "white",
                ["BiomeIndustrial"] = "bold yellow",
                ["BiomeOrganic"] = "bold green",
                ["BiomeVoid"] = "bold purple",

                // UI Structural Colors (v0.3.14a)
                ["DimColor"] = "grey",
                ["SeparatorColor"] = "white",
                ["LabelColor"] = "white",
                ["InputColor"] = "bold cyan",
                ["BorderActive"] = "bold yellow",
                ["BorderInactive"] = "grey",
                ["NarrativeColor"] = "white",
                ["TabActive"] = "bold gold1"
            },

            [ThemeType.Protanopia] = new()
            {
                // Red-Green Colorblind - Replace red/green with blue/orange
                ["PlayerColor"] = "blue",
                ["EnemyColor"] = "orange1",
                ["AllyColor"] = "cyan",
                ["NeutralColor"] = "grey",

                // Health States - Blue/Orange/Yellow spectrum
                ["HealthFull"] = "blue",
                ["HealthHigh"] = "cyan",
                ["HealthLow"] = "yellow",
                ["HealthCritical"] = "orange1",

                // Resource Colors
                ["StaminaColor"] = "blue",
                ["StressLow"] = "grey",
                ["StressMid"] = "cyan",
                ["StressHigh"] = "blue",

                // Quality Tiers - Avoid red/green
                ["QualityJunk"] = "grey",
                ["QualityCommon"] = "white",
                ["QualityUncommon"] = "blue",
                ["QualityRare"] = "cyan",
                ["QualityLegendary"] = "yellow",

                // Status Colors
                ["SuccessColor"] = "blue",
                ["WarningColor"] = "cyan",
                ["ErrorColor"] = "orange1",
                ["InfoColor"] = "blue",

                // UI Elements
                ["HeaderColor"] = "yellow",
                ["BorderColor"] = "grey",
                ["ActiveColor"] = "cyan",
                ["InactiveColor"] = "grey",

                // Danger Levels
                ["DangerSafe"] = "grey",
                ["DangerUnstable"] = "cyan",
                ["DangerHostile"] = "yellow",
                ["DangerLethal"] = "orange1",

                // Biome Colors (v0.3.14a) - Avoid red/green confusion
                ["BiomeRuin"] = "grey",
                ["BiomeIndustrial"] = "orange1",
                ["BiomeOrganic"] = "cyan",
                ["BiomeVoid"] = "blue",

                // UI Structural Colors (v0.3.14a)
                ["DimColor"] = "grey",
                ["SeparatorColor"] = "grey",
                ["LabelColor"] = "grey",
                ["InputColor"] = "blue",
                ["BorderActive"] = "cyan",
                ["BorderInactive"] = "grey",
                ["NarrativeColor"] = "grey",
                ["TabActive"] = "yellow"
            },

            [ThemeType.Deuteranopia] = new()
            {
                // Green-Red Colorblind - Similar to Protanopia
                ["PlayerColor"] = "blue",
                ["EnemyColor"] = "orange1",
                ["AllyColor"] = "cyan",
                ["NeutralColor"] = "grey",

                // Health States
                ["HealthFull"] = "blue",
                ["HealthHigh"] = "cyan",
                ["HealthLow"] = "yellow",
                ["HealthCritical"] = "orange1",

                // Resource Colors
                ["StaminaColor"] = "blue",
                ["StressLow"] = "grey",
                ["StressMid"] = "cyan",
                ["StressHigh"] = "blue",

                // Quality Tiers
                ["QualityJunk"] = "grey",
                ["QualityCommon"] = "white",
                ["QualityUncommon"] = "blue",
                ["QualityRare"] = "cyan",
                ["QualityLegendary"] = "yellow",

                // Status Colors
                ["SuccessColor"] = "blue",
                ["WarningColor"] = "cyan",
                ["ErrorColor"] = "orange1",
                ["InfoColor"] = "blue",

                // UI Elements
                ["HeaderColor"] = "yellow",
                ["BorderColor"] = "grey",
                ["ActiveColor"] = "cyan",
                ["InactiveColor"] = "grey",

                // Danger Levels
                ["DangerSafe"] = "grey",
                ["DangerUnstable"] = "cyan",
                ["DangerHostile"] = "yellow",
                ["DangerLethal"] = "orange1",

                // Biome Colors (v0.3.14a) - Avoid red/green confusion
                ["BiomeRuin"] = "grey",
                ["BiomeIndustrial"] = "orange1",
                ["BiomeOrganic"] = "cyan",
                ["BiomeVoid"] = "blue",

                // UI Structural Colors (v0.3.14a)
                ["DimColor"] = "grey",
                ["SeparatorColor"] = "grey",
                ["LabelColor"] = "grey",
                ["InputColor"] = "blue",
                ["BorderActive"] = "cyan",
                ["BorderInactive"] = "grey",
                ["NarrativeColor"] = "grey",
                ["TabActive"] = "yellow"
            },

            [ThemeType.Tritanopia] = new()
            {
                // Blue-Yellow Colorblind - Replace blue/yellow with magenta/cyan
                ["PlayerColor"] = "cyan",
                ["EnemyColor"] = "magenta1",
                ["AllyColor"] = "green",
                ["NeutralColor"] = "grey",

                // Health States
                ["HealthFull"] = "green",
                ["HealthHigh"] = "cyan",
                ["HealthLow"] = "orange1",
                ["HealthCritical"] = "magenta1",

                // Resource Colors
                ["StaminaColor"] = "cyan",
                ["StressLow"] = "grey",
                ["StressMid"] = "cyan",
                ["StressHigh"] = "magenta1",

                // Quality Tiers - Avoid blue/yellow confusion
                ["QualityJunk"] = "grey",
                ["QualityCommon"] = "white",
                ["QualityUncommon"] = "green",
                ["QualityRare"] = "cyan",
                ["QualityLegendary"] = "gold1",

                // Status Colors
                ["SuccessColor"] = "green",
                ["WarningColor"] = "cyan",
                ["ErrorColor"] = "magenta1",
                ["InfoColor"] = "cyan",

                // UI Elements
                ["HeaderColor"] = "gold1",
                ["BorderColor"] = "grey",
                ["ActiveColor"] = "cyan",
                ["InactiveColor"] = "grey",

                // Danger Levels
                ["DangerSafe"] = "grey",
                ["DangerUnstable"] = "cyan",
                ["DangerHostile"] = "orange1",
                ["DangerLethal"] = "magenta1",

                // Biome Colors (v0.3.14a) - Avoid blue/yellow confusion
                ["BiomeRuin"] = "grey",
                ["BiomeIndustrial"] = "orange1",
                ["BiomeOrganic"] = "green",
                ["BiomeVoid"] = "magenta1",

                // UI Structural Colors (v0.3.14a)
                ["DimColor"] = "grey",
                ["SeparatorColor"] = "grey",
                ["LabelColor"] = "grey",
                ["InputColor"] = "cyan",
                ["BorderActive"] = "cyan",
                ["BorderInactive"] = "grey",
                ["NarrativeColor"] = "grey",
                ["TabActive"] = "gold1"
            }
        };
    }
}
