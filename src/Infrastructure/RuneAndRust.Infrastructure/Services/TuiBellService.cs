namespace RuneAndRust.Infrastructure.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Enums;
using RuneAndRust.Application.Interfaces;

/// <summary>
/// Implementation of TUI bell service providing console beep notifications.
/// </summary>
/// <remarks>
/// <para>
/// Features:
/// <list type="bullet">
///   <item><description>8 predefined bell patterns for common events</description></item>
///   <item><description>Cross-platform support with fallback</description></item>
///   <item><description>Enable/disable toggle</description></item>
/// </list>
/// </para>
/// </remarks>
public class TuiBellService : ITuiBellService
{
    private readonly ILogger<TuiBellService> _logger;
    private readonly Dictionary<BellType, BellPattern> _patterns;
    private bool _isEnabled = true;

    /// <inheritdoc />
    public bool IsEnabled => _isEnabled;

    /// <summary>
    /// Creates a new TUI bell service.
    /// </summary>
    /// <param name="logger">Logger for bell operations.</param>
    public TuiBellService(ILogger<TuiBellService> logger)
    {
        _logger = logger;
        _patterns = InitializePatterns();
        _logger.LogDebug("TuiBellService initialized with {Count} patterns", _patterns.Count);
    }

    /// <inheritdoc />
    public void Bell(BellType type)
    {
        if (!_isEnabled)
        {
            _logger.LogDebug("Bell disabled, skipping: {Type}", type);
            return;
        }

        if (_patterns.TryGetValue(type, out var pattern))
        {
            ExecutePattern(pattern);
            _logger.LogDebug("Played bell: {Type}", type);
        }
        else
        {
            _logger.LogWarning("Unknown bell type: {Type}", type);
        }
    }

    /// <inheritdoc />
    public void BellCustom(int frequency, int durationMs)
    {
        if (!_isEnabled)
        {
            _logger.LogDebug("Bell disabled, skipping custom beep");
            return;
        }

        TryBeep(frequency, durationMs);
        _logger.LogDebug("Played custom bell: {Freq}Hz for {Duration}ms", frequency, durationMs);
    }

    /// <inheritdoc />
    public void SetEnabled(bool enabled)
    {
        _isEnabled = enabled;
        _logger.LogDebug("TUI bells enabled: {Enabled}", enabled);
    }

    /// <summary>
    /// Executes a bell pattern by playing each note in sequence.
    /// </summary>
    /// <param name="pattern">The pattern to execute.</param>
    private void ExecutePattern(BellPattern pattern)
    {
        foreach (var note in pattern.Notes)
        {
            TryBeep(note.Frequency, note.DurationMs);

            if (note.PauseAfterMs > 0)
            {
                Thread.Sleep(note.PauseAfterMs);
            }
        }
    }

    /// <summary>
    /// Attempts to play a beep with cross-platform fallback.
    /// </summary>
    /// <param name="frequency">Frequency in Hz.</param>
    /// <param name="durationMs">Duration in milliseconds.</param>
    private void TryBeep(int frequency, int durationMs)
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                // Windows: Native Console.Beep with frequency control
                Console.Beep(frequency, durationMs);
            }
            else
            {
                // Unix/Mac fallback: Terminal bell character
                // This produces a simple beep on most terminals
                Console.Write('\a');
            }
        }
        catch (Exception ex)
        {
            // Console.Beep may not be supported in all environments
            _logger.LogWarning(ex, "Console.Beep not supported on this platform");
        }
    }

    /// <summary>
    /// Initializes the predefined bell patterns.
    /// </summary>
    /// <returns>Dictionary mapping bell types to patterns.</returns>
    private static Dictionary<BellType, BellPattern> InitializePatterns() => new()
    {
        // Info: Single short beep (800Hz, 100ms)
        [BellType.Info] = new BellPattern(
            new BellNote(800, 100)),

        // Warning: Double beep (600Hz, 150ms × 2)
        [BellType.Warning] = new BellPattern(
            new BellNote(600, 150, 100),
            new BellNote(600, 150)),

        // Error: Long beep (400Hz, 500ms)
        [BellType.Error] = new BellPattern(
            new BellNote(400, 500)),

        // Combat: Quick triple (1000Hz, 80ms × 3)
        [BellType.Combat] = new BellPattern(
            new BellNote(1000, 80, 50),
            new BellNote(1000, 80, 50),
            new BellNote(1000, 80)),

        // LevelUp: Ascending tones (600→800→1000Hz)
        [BellType.LevelUp] = new BellPattern(
            new BellNote(600, 150, 50),
            new BellNote(800, 150, 50),
            new BellNote(1000, 200)),

        // QuestComplete: Celebratory pattern (800→1000→1200→1000Hz)
        [BellType.QuestComplete] = new BellPattern(
            new BellNote(800, 100, 50),
            new BellNote(1000, 100, 50),
            new BellNote(1200, 100, 50),
            new BellNote(1000, 200)),

        // LowHealth: Danger pattern (400Hz × 2)
        [BellType.LowHealth] = new BellPattern(
            new BellNote(400, 200, 150),
            new BellNote(400, 200)),

        // Notification: Simple beep (700Hz, 120ms)
        [BellType.Notification] = new BellPattern(
            new BellNote(700, 120))
    };
}

/// <summary>
/// A sequence of notes forming a bell pattern.
/// </summary>
/// <param name="Notes">The notes in the pattern.</param>
public record BellPattern(params BellNote[] Notes);

/// <summary>
/// A single note in a bell pattern.
/// </summary>
/// <param name="Frequency">Frequency in Hz (37-32767).</param>
/// <param name="DurationMs">Duration in milliseconds.</param>
/// <param name="PauseAfterMs">Pause after note in milliseconds (default 0).</param>
public record BellNote(int Frequency, int DurationMs, int PauseAfterMs = 0);
