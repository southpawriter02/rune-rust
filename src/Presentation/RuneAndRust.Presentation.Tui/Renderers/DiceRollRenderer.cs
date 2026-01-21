// ═══════════════════════════════════════════════════════════════════════════════
// DiceRollRenderer.cs
// Renderer for formatting individual dice rolls and roll lists with critical highlighting.
// Version: 0.13.4d
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Records;
using RuneAndRust.Presentation.Tui.Configuration;

namespace RuneAndRust.Presentation.Tui.Renderers;

/// <summary>
/// Renderer for formatting individual dice rolls and roll lists
/// with appropriate highlighting for critical successes and failures.
/// </summary>
/// <remarks>
/// <para>This renderer provides formatting utilities for dice roll display:</para>
/// <list type="bullet">
///   <item><description>Individual roll formatting with "!" for criticals</description></item>
///   <item><description>Roll list formatting as comma-separated strings</description></item>
///   <item><description>Color determination based on roll value ranges</description></item>
/// </list>
/// <para>
/// Roll color mapping:
/// </para>
/// <list type="bullet">
///   <item><description>20: Yellow (critical success)</description></item>
///   <item><description>15-19: Green (good rolls)</description></item>
///   <item><description>11-14: White (average)</description></item>
///   <item><description>6-10: Gray (below average)</description></item>
///   <item><description>2-5: DarkYellow (poor rolls)</description></item>
///   <item><description>1: Red (critical failure)</description></item>
/// </list>
/// </remarks>
public class DiceRollRenderer
{
    private readonly DiceHistoryPanelConfig _config;
    private readonly ILogger<DiceRollRenderer>? _logger;

    /// <summary>
    /// Creates a new dice roll renderer with the specified configuration.
    /// </summary>
    /// <param name="config">Panel configuration settings.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when config is null.</exception>
    public DiceRollRenderer(
        DiceHistoryPanelConfig config,
        ILogger<DiceRollRenderer>? logger = null)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger;

        _logger?.LogDebug("DiceRollRenderer initialized");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Individual Roll Formatting
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats a single roll value with critical highlighting.
    /// </summary>
    /// <param name="rollValue">The roll result (1-20).</param>
    /// <param name="isCriticalSuccess">True if natural 20.</param>
    /// <param name="isCriticalFailure">True if natural 1.</param>
    /// <returns>Formatted roll string like "20!" or "7".</returns>
    /// <remarks>
    /// <para>Critical rolls are marked with an exclamation mark suffix.</para>
    /// </remarks>
    public string FormatRoll(int rollValue, bool isCriticalSuccess, bool isCriticalFailure)
    {
        _logger?.LogDebug(
            "Formatting roll: {Value}, CritSuccess={CritSuccess}, CritFail={CritFail}",
            rollValue, isCriticalSuccess, isCriticalFailure);

        // Critical rolls get the "!" suffix
        if (isCriticalSuccess || isCriticalFailure)
        {
            return $"{rollValue}!";
        }

        return rollValue.ToString();
    }

    /// <summary>
    /// Formats a single roll value with automatic critical detection.
    /// </summary>
    /// <param name="rollValue">The roll result (1-20).</param>
    /// <returns>Formatted roll string like "20!" or "7".</returns>
    public string FormatRoll(int rollValue)
    {
        var isCriticalSuccess = rollValue == 20;
        var isCriticalFailure = rollValue == 1;
        return FormatRoll(rollValue, isCriticalSuccess, isCriticalFailure);
    }

    /// <summary>
    /// Highlights a critical roll with special formatting.
    /// </summary>
    /// <param name="rollValue">The roll result (1 or 20).</param>
    /// <returns>Highlighted string with "!" suffix.</returns>
    /// <remarks>
    /// <para>Only critical values (1 and 20) receive the suffix.</para>
    /// </remarks>
    public string HighlightCritical(int rollValue)
    {
        if (rollValue == 20 || rollValue == 1)
        {
            return $"{rollValue}!";
        }
        return rollValue.ToString();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Roll List Formatting
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats a list of recent rolls as a comma-separated string.
    /// </summary>
    /// <param name="rolls">Collection of dice roll records to format.</param>
    /// <returns>Formatted string like "18, 14, 12, 3, 20!, 8, 15".</returns>
    /// <remarks>
    /// <para>Rolls are displayed in the order provided. Critical rolls are highlighted with "!".</para>
    /// </remarks>
    public string FormatRollList(IEnumerable<DiceRollRecord> rolls)
    {
        ArgumentNullException.ThrowIfNull(rolls, nameof(rolls));

        var rollList = rolls.ToList();
        _logger?.LogDebug("Formatting roll list with {Count} rolls", rollList.Count);

        if (rollList.Count == 0)
        {
            return "No rolls recorded";
        }

        var formattedRolls = rollList.Select(roll =>
        {
            // Use the first individual roll value for d20 rolls
            var value = roll.IndividualRolls.Length > 0 ? roll.IndividualRolls[0] : roll.Result;
            return FormatRoll(value, roll.HasNatural20, roll.HasNatural1);
        });

        return string.Join(", ", formattedRolls);
    }

    /// <summary>
    /// Formats a list of raw roll values as a comma-separated string.
    /// </summary>
    /// <param name="rollValues">Collection of roll values to format.</param>
    /// <returns>Formatted string like "18, 14, 12, 3, 20!, 8, 15".</returns>
    /// <remarks>
    /// <para>This overload is used when only the roll values are available (not full records).</para>
    /// </remarks>
    public string FormatRollList(IEnumerable<int> rollValues)
    {
        ArgumentNullException.ThrowIfNull(rollValues, nameof(rollValues));

        var values = rollValues.ToList();
        _logger?.LogDebug("Formatting roll value list with {Count} values", values.Count);

        if (values.Count == 0)
        {
            return "No rolls recorded";
        }

        var formattedRolls = values.Select(FormatRoll);
        return string.Join(", ", formattedRolls);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Color Determination
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the appropriate color for a roll value.
    /// </summary>
    /// <param name="rollValue">The roll result (1-20).</param>
    /// <returns>Console color for the roll.</returns>
    /// <remarks>
    /// <para>Color mapping based on roll value ranges:</para>
    /// <list type="bullet">
    ///   <item><description>20: Yellow (critical success)</description></item>
    ///   <item><description>15-19: Green (good rolls)</description></item>
    ///   <item><description>11-14: White (average)</description></item>
    ///   <item><description>6-10: Gray (below average)</description></item>
    ///   <item><description>2-5: DarkYellow (poor rolls)</description></item>
    ///   <item><description>1: Red (critical failure)</description></item>
    /// </list>
    /// </remarks>
    public ConsoleColor GetRollColor(int rollValue)
    {
        return rollValue switch
        {
            20 => _config.CriticalSuccessColor,    // Critical success: Yellow
            >= 15 => ConsoleColor.Green,            // Good rolls: Green
            >= 11 => ConsoleColor.White,            // Average: White
            >= 6 => ConsoleColor.Gray,              // Below average: Gray
            >= 2 => ConsoleColor.DarkYellow,        // Poor rolls: DarkYellow
            1 => _config.CriticalFailureColor,      // Critical failure: Red
            _ => ConsoleColor.White                 // Default fallback
        };
    }

    /// <summary>
    /// Determines if a roll value is a critical (natural 20 or natural 1).
    /// </summary>
    /// <param name="rollValue">The roll result.</param>
    /// <returns>True if the roll is a natural 20 or natural 1.</returns>
    public static bool IsCritical(int rollValue)
    {
        return rollValue == 20 || rollValue == 1;
    }

    /// <summary>
    /// Determines if a roll value is a critical success (natural 20).
    /// </summary>
    /// <param name="rollValue">The roll result.</param>
    /// <returns>True if the roll is a natural 20.</returns>
    public static bool IsCriticalSuccess(int rollValue)
    {
        return rollValue == 20;
    }

    /// <summary>
    /// Determines if a roll value is a critical failure (natural 1).
    /// </summary>
    /// <param name="rollValue">The roll result.</param>
    /// <returns>True if the roll is a natural 1.</returns>
    public static bool IsCriticalFailure(int rollValue)
    {
        return rollValue == 1;
    }
}
