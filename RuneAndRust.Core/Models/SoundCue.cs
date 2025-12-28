using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Models;

/// <summary>
/// Represents an audio cue definition for playback (v0.3.19a).
/// Contains frequency and duration parameters for Console.Beep or future audio engines.
/// </summary>
/// <param name="Id">Unique identifier for the sound cue (e.g., "ui_click", "combat_hit").</param>
/// <param name="Category">The sound category for filtering and priority.</param>
/// <param name="Frequency">Frequency in Hertz (37-32767 for Console.Beep, typical range 100-2000).</param>
/// <param name="DurationMs">Duration in milliseconds (typical range 50-500).</param>
/// <param name="VolumeMultiplier">Relative volume multiplier (0.0-1.0, default 1.0).</param>
public record SoundCue(
    string Id,
    SoundCategory Category,
    int Frequency,
    int DurationMs,
    float VolumeMultiplier = 1.0f)
{
    /// <summary>
    /// Standard UI click sound for menu navigation.
    /// High pitch, very short duration for responsiveness.
    /// </summary>
    public static SoundCue UiClick => new("ui_click", SoundCategory.UI, 1000, 50);

    /// <summary>
    /// UI selection confirmation sound.
    /// Slightly higher pitch than navigation, medium duration.
    /// </summary>
    public static SoundCue UiSelect => new("ui_select", SoundCategory.UI, 1200, 100);

    /// <summary>
    /// Error or invalid action feedback.
    /// Low pitch, longer duration to indicate failure.
    /// </summary>
    public static SoundCue UiError => new("ui_error", SoundCategory.UI, 200, 300);

    /// <summary>
    /// Light combat hit sound for minor damage.
    /// Mid-range frequency, short duration.
    /// </summary>
    public static SoundCue CombatHitLight => new("combat_hit_light", SoundCategory.Combat, 400, 100);

    /// <summary>
    /// Heavy combat hit sound for significant damage.
    /// Lower frequency, longer duration for impact.
    /// </summary>
    public static SoundCue CombatHitHeavy => new("combat_hit_heavy", SoundCategory.Combat, 300, 250);

    /// <summary>
    /// Critical hit notification.
    /// High pitch, extended duration to stand out.
    /// </summary>
    public static SoundCue CombatCritical => new("combat_critical", SoundCategory.Combat, 2000, 400);

    /// <summary>
    /// Creates a custom UI sound cue.
    /// </summary>
    /// <param name="id">The cue identifier.</param>
    /// <param name="frequency">Frequency in Hz (recommended: 800-1500).</param>
    /// <param name="durationMs">Duration in ms (recommended: 50-150).</param>
    /// <returns>A UI category sound cue.</returns>
    public static SoundCue CreateUiCue(string id, int frequency, int durationMs)
    {
        return new SoundCue(id, SoundCategory.UI, frequency, durationMs);
    }

    /// <summary>
    /// Creates a custom combat sound cue.
    /// </summary>
    /// <param name="id">The cue identifier.</param>
    /// <param name="frequency">Frequency in Hz (recommended: 200-600).</param>
    /// <param name="durationMs">Duration in ms (recommended: 100-300).</param>
    /// <returns>A Combat category sound cue.</returns>
    public static SoundCue CreateCombatCue(string id, int frequency, int durationMs)
    {
        return new SoundCue(id, SoundCategory.Combat, frequency, durationMs);
    }
}
