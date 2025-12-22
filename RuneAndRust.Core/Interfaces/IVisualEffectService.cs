using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Service contract for visual effects during gameplay (v0.3.9a).
/// Provides methods to trigger visual feedback like border flashes during combat events.
/// </summary>
public interface IVisualEffectService
{
    /// <summary>
    /// Triggers a visual effect asynchronously.
    /// The effect respects GameSettings.ReduceMotion for accessibility.
    /// </summary>
    /// <param name="effectType">The type of effect to trigger.</param>
    /// <param name="intensity">Effect intensity multiplier (1-3). Higher values increase duration.</param>
    /// <returns>A task that completes when the effect finishes.</returns>
    Task TriggerEffectAsync(VisualEffectType effectType, int intensity = 1);

    /// <summary>
    /// Sets a border color override for the combat grid panel.
    /// Used internally by effect triggers to flash the border.
    /// </summary>
    /// <param name="colorOverride">Spectre.Console color string (e.g., "red", "gold1").</param>
    void SetBorderOverride(string? colorOverride);

    /// <summary>
    /// Gets the current border color override, if any.
    /// </summary>
    /// <returns>The current color override, or null if none is active.</returns>
    string? GetBorderOverride();

    /// <summary>
    /// Clears any active border color override, returning to default styling.
    /// </summary>
    void ClearBorderOverride();
}
