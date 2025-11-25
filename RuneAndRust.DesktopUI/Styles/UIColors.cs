using Avalonia.Media;

namespace RuneAndRust.DesktopUI.Styles;

/// <summary>
/// v0.43.21: Standardized color palette for consistent UI styling.
/// All UI components should use these colors for a cohesive visual experience.
/// </summary>
public static class UIColors
{
    #region Primary Colors

    /// <summary>
    /// Gold color for rare items, important highlights, and positive effects.
    /// </summary>
    public static readonly Color Gold = Color.Parse("#FFD700");

    /// <summary>
    /// Dark red for damage, warnings, and negative effects.
    /// </summary>
    public static readonly Color DarkRed = Color.Parse("#DC143C");

    /// <summary>
    /// Blue for magic, mana, clan-forged items, and info elements.
    /// </summary>
    public static readonly Color Blue = Color.Parse("#4A90E2");

    /// <summary>
    /// Purple for rare items, optimized gear, and special abilities.
    /// </summary>
    public static readonly Color Purple = Color.Parse("#9400D3");

    /// <summary>
    /// Green for success, health, healing, and positive status.
    /// </summary>
    public static readonly Color Green = Color.Parse("#228B22");

    /// <summary>
    /// Orange for warnings, stamina, and medium-priority alerts.
    /// </summary>
    public static readonly Color Orange = Color.Parse("#FFA500");

    /// <summary>
    /// Cyan for ice, cold effects, and special UI elements.
    /// </summary>
    public static readonly Color Cyan = Color.Parse("#00CED1");

    #endregion

    #region Background Colors

    /// <summary>
    /// Darkest background for main window and primary containers.
    /// </summary>
    public static readonly Color BackgroundDark = Color.Parse("#1C1C1C");

    /// <summary>
    /// Medium background for panels and secondary containers.
    /// </summary>
    public static readonly Color BackgroundMedium = Color.Parse("#2C2C2C");

    /// <summary>
    /// Light background for interactive elements and tertiary containers.
    /// </summary>
    public static readonly Color BackgroundLight = Color.Parse("#3C3C3C");

    /// <summary>
    /// Hover state background.
    /// </summary>
    public static readonly Color BackgroundHover = Color.Parse("#4A4A4A");

    /// <summary>
    /// Selected/Active state background.
    /// </summary>
    public static readonly Color BackgroundSelected = Color.Parse("#5A5A5A");

    #endregion

    #region Text Colors

    /// <summary>
    /// Primary text color (white) for main content.
    /// </summary>
    public static readonly Color TextPrimary = Colors.White;

    /// <summary>
    /// Secondary text color for less important content.
    /// </summary>
    public static readonly Color TextSecondary = Color.Parse("#CCCCCC");

    /// <summary>
    /// Tertiary text color for hints and disabled content.
    /// </summary>
    public static readonly Color TextTertiary = Color.Parse("#888888");

    /// <summary>
    /// Disabled text color.
    /// </summary>
    public static readonly Color TextDisabled = Color.Parse("#666666");

    #endregion

    #region Status Colors

    /// <summary>
    /// Success color for positive outcomes.
    /// </summary>
    public static readonly Color Success = Color.Parse("#228B22");

    /// <summary>
    /// Warning color for caution states.
    /// </summary>
    public static readonly Color Warning = Color.Parse("#FFA500");

    /// <summary>
    /// Error color for failures and critical issues.
    /// </summary>
    public static readonly Color Error = Color.Parse("#DC143C");

    /// <summary>
    /// Info color for informational messages.
    /// </summary>
    public static readonly Color Info = Color.Parse("#4A90E2");

    #endregion

    #region Quality Tier Colors

    /// <summary>
    /// Jury-Rigged quality (gray).
    /// </summary>
    public static readonly Color QualityJuryRigged = Color.Parse("#808080");

    /// <summary>
    /// Scavenged quality (white).
    /// </summary>
    public static readonly Color QualityScavenged = Colors.White;

    /// <summary>
    /// Clan-Forged quality (blue).
    /// </summary>
    public static readonly Color QualityClanForged = Color.Parse("#4A90E2");

    /// <summary>
    /// Optimized quality (purple).
    /// </summary>
    public static readonly Color QualityOptimized = Color.Parse("#9400D3");

    /// <summary>
    /// Myth-Forged quality (gold).
    /// </summary>
    public static readonly Color QualityMythForged = Color.Parse("#FFD700");

    #endregion

    #region Combat Colors

    /// <summary>
    /// Player/friendly unit color.
    /// </summary>
    public static readonly Color PlayerColor = Color.Parse("#4A90E2");

    /// <summary>
    /// Enemy unit color.
    /// </summary>
    public static readonly Color EnemyColor = Color.Parse("#DC143C");

    /// <summary>
    /// Companion unit color.
    /// </summary>
    public static readonly Color CompanionColor = Color.Parse("#228B22");

    /// <summary>
    /// Boss enemy color.
    /// </summary>
    public static readonly Color BossColor = Color.Parse("#9400D3");

    /// <summary>
    /// Highlighted cell color.
    /// </summary>
    public static readonly Color HighlightColor = Color.Parse("#90EE9033");

    /// <summary>
    /// Selected cell color.
    /// </summary>
    public static readonly Color SelectedCellColor = Color.Parse("#FFD70066");

    #endregion

    #region Hazard Colors

    /// <summary>
    /// Fire hazard color.
    /// </summary>
    public static readonly Color HazardFire = Color.Parse("#FF4500");

    /// <summary>
    /// Ice hazard color.
    /// </summary>
    public static readonly Color HazardIce = Color.Parse("#00CED1");

    /// <summary>
    /// Poison hazard color.
    /// </summary>
    public static readonly Color HazardPoison = Color.Parse("#32CD32");

    /// <summary>
    /// Lightning hazard color.
    /// </summary>
    public static readonly Color HazardLightning = Color.Parse("#FFD700");

    /// <summary>
    /// Void/Dark hazard color.
    /// </summary>
    public static readonly Color HazardVoid = Color.Parse("#4B0082");

    #endregion

    #region Border Colors

    /// <summary>
    /// Default border color.
    /// </summary>
    public static readonly Color BorderDefault = Color.Parse("#555555");

    /// <summary>
    /// Focused border color.
    /// </summary>
    public static readonly Color BorderFocus = Color.Parse("#4A90E2");

    /// <summary>
    /// Error border color.
    /// </summary>
    public static readonly Color BorderError = Color.Parse("#DC143C");

    #endregion

    #region Helper Methods

    /// <summary>
    /// Gets a SolidColorBrush for the specified color.
    /// </summary>
    public static SolidColorBrush ToBrush(this Color color)
    {
        return new SolidColorBrush(color);
    }

    /// <summary>
    /// Creates a color with modified opacity.
    /// </summary>
    public static Color WithOpacity(this Color color, double opacity)
    {
        return Color.FromArgb((byte)(opacity * 255), color.R, color.G, color.B);
    }

    /// <summary>
    /// Gets the quality tier color for equipment.
    /// </summary>
    public static Color GetQualityColor(int qualityTier)
    {
        return qualityTier switch
        {
            0 => QualityJuryRigged,
            1 => QualityScavenged,
            2 => QualityClanForged,
            3 => QualityOptimized,
            4 => QualityMythForged,
            _ => QualityScavenged
        };
    }

    /// <summary>
    /// Gets the color for a health bar based on percentage.
    /// </summary>
    public static Color GetHealthBarColor(double percentage)
    {
        if (percentage > 0.5) return Green;
        if (percentage > 0.25) return Orange;
        return DarkRed;
    }

    #endregion
}
