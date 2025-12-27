using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Service contract for visual effects during gameplay (v0.3.9a).
/// Provides methods to trigger visual feedback like border flashes during combat events.
/// </summary>
/// <remarks>
/// See: SPEC-RENDER-001 for Rendering Pipeline System design.
/// v0.3.23b: Added OnInvalidateVisuals event and CheckExpiredOverrides() for non-blocking loop support.
/// </remarks>
public interface IVisualEffectService
{
    /// <summary>
    /// Event raised when visual state changes and a redraw is required.
    /// v0.3.23b: Used by non-blocking game loop to trigger renders.
    /// </summary>
    event Action? OnInvalidateVisuals;

    /// <summary>
    /// Checks if any border override has expired and clears it.
    /// Called by the game loop each tick to manage time-based effects.
    /// </summary>
    /// <returns>True if an override was cleared (redraw needed).</returns>
    /// <remarks>v0.3.23b: Enables non-blocking VFX expiration.</remarks>
    bool CheckExpiredOverrides();

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
