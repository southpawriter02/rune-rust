using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Settings;

namespace RuneAndRust.Terminal.Services;

/// <summary>
/// Terminal implementation of visual effects service (v0.3.9a).
/// Provides border flash effects for combat feedback using Spectre.Console color overrides.
/// </summary>
/// <remarks>
/// See: SPEC-RENDER-001 for Rendering Pipeline System design.
/// v0.3.23b: Added OnInvalidateVisuals event and expiry tracking for non-blocking loop support.
/// </remarks>
public class VisualEffectService : IVisualEffectService
{
    private readonly ILogger<VisualEffectService> _logger;
    private string? _borderOverride;
    private DateTime? _borderOverrideExpiry;

    /// <inheritdoc />
    public event Action? OnInvalidateVisuals;

    /// <summary>
    /// Initializes a new instance of the <see cref="VisualEffectService"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    public VisualEffectService(ILogger<VisualEffectService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task TriggerEffectAsync(VisualEffectType effectType, int intensity = 1)
    {
        _logger.LogTrace("[VFX] TriggerEffectAsync called: EffectType={EffectType}, Intensity={Intensity}",
            effectType, intensity);

        if (GameSettings.ReduceMotion)
        {
            _logger.LogTrace("[VFX] ReduceMotion enabled, skipping effect");
            return;
        }

        if (effectType == VisualEffectType.None)
        {
            _logger.LogTrace("[VFX] EffectType is None, skipping");
            return;
        }

        var color = GetColorForEffect(effectType);
        if (color == null)
        {
            _logger.LogWarning("[VFX] No color mapping for EffectType={EffectType}", effectType);
            return;
        }

        // Clamp intensity to valid range (1-3)
        intensity = Math.Clamp(intensity, 1, 3);

        // Flash duration: 150ms base * intensity
        var durationMs = 150 * intensity;

        _logger.LogTrace("[VFX] Triggering {EffectType} with color {Color} for {Duration}ms",
            effectType, color, durationMs);

        // v0.3.23b: Set expiry time for non-blocking loop to clear
        _borderOverride = color;
        _borderOverrideExpiry = DateTime.UtcNow.AddMilliseconds(durationMs);
        OnInvalidateVisuals?.Invoke();

        // Legacy await for backward compatibility with callers expecting completion
        await Task.Delay(durationMs);

        _logger.LogTrace("[VFX] Effect {EffectType} completed", effectType);
    }

    /// <inheritdoc />
    /// <remarks>v0.3.23b: Manual override has no expiry and invokes OnInvalidateVisuals.</remarks>
    public void SetBorderOverride(string? colorOverride)
    {
        _borderOverride = colorOverride;
        _borderOverrideExpiry = null; // Manual override has no expiry
        _logger.LogTrace("[VFX] Border override set to {Color}", colorOverride ?? "(null)");
        OnInvalidateVisuals?.Invoke();
    }

    /// <inheritdoc />
    public string? GetBorderOverride()
    {
        return _borderOverride;
    }

    /// <inheritdoc />
    /// <remarks>v0.3.23b: Invokes OnInvalidateVisuals and clears expiry.</remarks>
    public void ClearBorderOverride()
    {
        _borderOverride = null;
        _borderOverrideExpiry = null;
        _logger.LogTrace("[VFX] Border override cleared");
        OnInvalidateVisuals?.Invoke();
    }

    /// <inheritdoc />
    /// <remarks>v0.3.23b: Called by game loop to manage time-based effects.</remarks>
    public bool CheckExpiredOverrides()
    {
        if (_borderOverride != null && _borderOverrideExpiry.HasValue && DateTime.UtcNow > _borderOverrideExpiry.Value)
        {
            _logger.LogTrace("[VFX] Border override expired, clearing");
            _borderOverride = null;
            _borderOverrideExpiry = null;
            OnInvalidateVisuals?.Invoke();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the Spectre.Console color string for an effect type.
    /// </summary>
    /// <param name="effectType">The effect type to get color for.</param>
    /// <returns>Color string or null if no mapping exists.</returns>
    private static string? GetColorForEffect(VisualEffectType effectType) => effectType switch
    {
        VisualEffectType.DamageFlash => "red",
        VisualEffectType.CriticalFlash => "gold1",
        VisualEffectType.HealFlash => "green",
        VisualEffectType.TraumaFlash => "magenta1",
        VisualEffectType.VictoryFlash => "bold gold1",
        _ => null
    };
}
