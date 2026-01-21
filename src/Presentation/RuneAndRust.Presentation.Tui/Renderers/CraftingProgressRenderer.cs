// ═══════════════════════════════════════════════════════════════════════════════
// CraftingProgressRenderer.cs
// Renders crafting progress bars and completion feedback.
// Version: 0.13.3b
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;

namespace RuneAndRust.Presentation.Tui.Renderers;

/// <summary>
/// Renders crafting progress bars and completion feedback.
/// </summary>
/// <remarks>
/// <para>Displays progress during item crafting with a visual progress bar
/// and completion message. Supports animated progress transitions.</para>
/// <para>Progress bar format: [##########....................] 35%</para>
/// </remarks>
/// <example>
/// <code>
/// var renderer = new CraftingProgressRenderer(config, logger);
/// var progressLine = renderer.RenderProgress(0.5f, "Steel Blade");
/// // Output: "Crafting: Steel Blade\n[###############...............] 50%"
/// </code>
/// </example>
public class CraftingProgressRenderer
{
    private readonly CraftingStationConfig _config;
    private readonly ILogger<CraftingProgressRenderer> _logger;

    /// <summary>
    /// Creates a new instance of the CraftingProgressRenderer.
    /// </summary>
    /// <param name="config">Configuration for progress display settings.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when config is null.</exception>
    public CraftingProgressRenderer(
        CraftingStationConfig? config = null,
        ILogger<CraftingProgressRenderer>? logger = null)
    {
        _config = config ?? CraftingStationConfig.CreateDefault();
        _logger = logger ?? NullLogger<CraftingProgressRenderer>.Instance;

        _logger.LogDebug(
            "CraftingProgressRenderer initialized with {BarWidth} width bar",
            _config.ProgressBarWidth);
    }

    /// <summary>
    /// Renders the crafting progress bar with recipe name.
    /// </summary>
    /// <param name="progress">The progress value (0.0 to 1.0).</param>
    /// <param name="recipeName">The name of the recipe being crafted.</param>
    /// <returns>The formatted progress display with name and bar.</returns>
    /// <remarks>
    /// Returns a two-line string:
    /// <code>
    /// Crafting: Steel Blade
    /// [##########....................] 35%
    /// </code>
    /// </remarks>
    public string RenderProgress(float progress, string recipeName)
    {
        // Clamp progress to valid range
        var clampedProgress = Math.Clamp(progress, 0f, 1f);
        var percentage = (int)(clampedProgress * 100);

        // Build progress bar
        var progressBar = FormatProgressBar(clampedProgress);

        _logger.LogDebug(
            "Rendered progress for {RecipeName}: {Percentage}%",
            recipeName,
            percentage);

        return $"Crafting: {recipeName}\n{progressBar} {percentage}%";
    }

    /// <summary>
    /// Formats a progress bar string.
    /// </summary>
    /// <param name="progress">The progress value (0.0 to 1.0).</param>
    /// <returns>The formatted progress bar string (e.g., "[##########..........]").</returns>
    /// <remarks>
    /// <para>Uses configurable filled and empty characters.</para>
    /// <para>Progress is clamped to [0.0, 1.0] range.</para>
    /// </remarks>
    public string FormatProgressBar(float progress)
    {
        // Clamp to valid range
        var clampedProgress = Math.Clamp(progress, 0f, 1f);

        // Calculate bar segments
        var barWidth = _config.ProgressBarWidth;
        var filledWidth = (int)(barWidth * clampedProgress);
        var emptyWidth = barWidth - filledWidth;

        // Build bar string
        var filledBar = new string(_config.ProgressFilledChar, filledWidth);
        var emptyBar = new string(_config.ProgressEmptyChar, emptyWidth);

        return $"[{filledBar}{emptyBar}]";
    }

    /// <summary>
    /// Animates progress from one value to another.
    /// </summary>
    /// <param name="fromProgress">Starting progress value (0.0 to 1.0).</param>
    /// <param name="toProgress">Ending progress value (0.0 to 1.0).</param>
    /// <param name="recipeName">The name of the recipe.</param>
    /// <param name="terminalService">The terminal service for output.</param>
    /// <param name="x">X position for rendering.</param>
    /// <param name="y">Y position for rendering.</param>
    /// <remarks>
    /// <para>Animates the progress bar in steps, creating a smooth transition effect.</para>
    /// <para>Number of steps and delay are configurable.</para>
    /// </remarks>
    public void AnimateProgress(
        float fromProgress,
        float toProgress,
        string recipeName,
        ITerminalService terminalService,
        int x,
        int y)
    {
        ArgumentNullException.ThrowIfNull(terminalService);

        var steps = _config.ProgressAnimationSteps;
        var increment = (toProgress - fromProgress) / steps;

        _logger.LogDebug(
            "Animating progress from {From} to {To} in {Steps} steps",
            fromProgress,
            toProgress,
            steps);

        // Animate through each step
        for (var i = 0; i <= steps; i++)
        {
            var currentProgress = fromProgress + (increment * i);
            var progressLine = RenderProgress(currentProgress, recipeName);

            // Clear previous lines
            var clearLine = new string(' ', _config.ProgressBarWidth + 30);
            terminalService.WriteAt(x, y, clearLine);
            terminalService.WriteAt(x, y + 1, clearLine);

            // Write new progress
            var lines = progressLine.Split('\n');
            terminalService.WriteAt(x, y, lines[0]);
            terminalService.WriteColoredAt(x, y + 1, lines[1], _config.ProgressBarColor);

            // Delay between steps
            Thread.Sleep(_config.ProgressAnimationDelay);
        }

        _logger.LogDebug("Animation complete");
    }

    /// <summary>
    /// Shows the completion message.
    /// </summary>
    /// <param name="x">X position for rendering.</param>
    /// <param name="y">Y position for rendering.</param>
    /// <param name="terminalService">The terminal service for output.</param>
    /// <remarks>
    /// Displays "[x] Crafting Complete!" in the completion color.
    /// </remarks>
    public void ShowComplete(int x, int y, ITerminalService terminalService)
    {
        ArgumentNullException.ThrowIfNull(terminalService);

        var completeMessage = "[x] Crafting Complete!";
        terminalService.WriteColoredAt(x, y, completeMessage, _config.CompletionColor);

        _logger.LogInformation("Crafting complete displayed at ({X}, {Y})", x, y);
    }

    /// <summary>
    /// Shows the completion message with item name.
    /// </summary>
    /// <param name="x">X position for rendering.</param>
    /// <param name="y">Y position for rendering.</param>
    /// <param name="itemName">The name of the crafted item.</param>
    /// <param name="terminalService">The terminal service for output.</param>
    /// <remarks>
    /// Displays:
    /// <code>
    /// [x] Crafting Complete!
    /// Created: Steel Blade
    /// </code>
    /// </remarks>
    public void ShowCompleteWithItem(int x, int y, string itemName, ITerminalService terminalService)
    {
        ArgumentNullException.ThrowIfNull(terminalService);

        // Complete message
        var completeMessage = "[x] Crafting Complete!";
        terminalService.WriteColoredAt(x, y, completeMessage, _config.CompletionColor);

        // Item created message
        var itemMessage = $"Created: {itemName}";
        terminalService.WriteAt(x, y + 1, itemMessage);

        _logger.LogInformation(
            "Crafting complete: {ItemName}",
            itemName);
    }

    /// <summary>
    /// Gets the current configuration.
    /// </summary>
    /// <returns>The crafting station configuration.</returns>
    public CraftingStationConfig GetConfig() => _config;
}
