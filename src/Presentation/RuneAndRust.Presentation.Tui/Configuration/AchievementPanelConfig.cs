// ═══════════════════════════════════════════════════════════════════════════════
// AchievementPanelConfig.cs
// Configuration for the achievement panel display.
// Version: 0.13.4a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.Configuration;

/// <summary>
/// Configuration for the achievement panel display.
/// </summary>
/// <remarks>
/// <para>
/// This configuration controls the visual layout and behavior of the achievement panel:
/// </para>
/// <list type="bullet">
///   <item><description>Panel dimensions (width and height in characters)</description></item>
///   <item><description>Progress bar width for achievement completion display</description></item>
///   <item><description>Notification duration for achievement unlock popups</description></item>
///   <item><description>Secret achievement display behavior</description></item>
/// </list>
/// <para>
/// Default values can be overridden via <c>config/achievement-panel.json</c>.
/// </para>
/// </remarks>
public class AchievementPanelConfig
{
    /// <summary>
    /// Gets or sets the width of the panel in characters.
    /// </summary>
    /// <value>Default: 70 characters.</value>
    public int PanelWidth { get; set; } = 70;

    /// <summary>
    /// Gets or sets the height of the panel in characters.
    /// </summary>
    /// <value>Default: 40 characters.</value>
    public int PanelHeight { get; set; } = 40;

    /// <summary>
    /// Gets or sets the offset for the summary text from the left edge.
    /// </summary>
    /// <value>Default: 35 characters.</value>
    public int SummaryOffset { get; set; } = 35;

    /// <summary>
    /// Gets or sets the width of progress bars in characters.
    /// </summary>
    /// <value>Default: 40 characters.</value>
    public int ProgressBarWidth { get; set; } = 40;

    /// <summary>
    /// Gets or sets the duration for achievement notifications in seconds.
    /// </summary>
    /// <value>Default: 5 seconds.</value>
    public int NotificationDurationSeconds { get; set; } = 5;

    /// <summary>
    /// Gets or sets whether to show secret achievements as placeholders.
    /// </summary>
    /// <value>Default: true (show placeholders for locked secrets).</value>
    public bool ShowSecretPlaceholders { get; set; } = true;

    /// <summary>
    /// Gets or sets the width of achievement cards in characters.
    /// </summary>
    /// <value>Default: 65 characters.</value>
    public int CardWidth { get; set; } = 65;

    /// <summary>
    /// Gets or sets the width of unlock notification popups in characters.
    /// </summary>
    /// <value>Default: 55 characters.</value>
    public int NotificationWidth { get; set; } = 55;
}
