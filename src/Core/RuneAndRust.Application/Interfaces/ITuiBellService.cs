namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Application.Enums;

/// <summary>
/// Provides console beep notifications for TUI mode.
/// </summary>
/// <remarks>
/// <para>
/// Provides audio feedback in terminal mode through console beeps:
/// <list type="bullet">
///   <item><description>Predefined patterns for common events</description></item>
///   <item><description>Custom frequency/duration support</description></item>
///   <item><description>Enable/disable toggle</description></item>
/// </list>
/// </para>
/// </remarks>
public interface ITuiBellService
{
    /// <summary>
    /// Plays a bell pattern for the specified type.
    /// </summary>
    /// <param name="type">The type of bell pattern to play.</param>
    void Bell(BellType type);

    /// <summary>
    /// Plays a custom bell with specific frequency and duration.
    /// </summary>
    /// <param name="frequency">Frequency in Hz (37-32767).</param>
    /// <param name="durationMs">Duration in milliseconds.</param>
    void BellCustom(int frequency, int durationMs);

    /// <summary>
    /// Gets whether bells are enabled.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Enables or disables bell sounds.
    /// </summary>
    /// <param name="enabled">True to enable, false to disable.</param>
    void SetEnabled(bool enabled);
}
