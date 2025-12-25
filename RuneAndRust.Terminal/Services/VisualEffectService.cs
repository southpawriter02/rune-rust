using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Settings;

namespace RuneAndRust.Terminal.Services;

/// <summary>
/// Terminal implementation of visual effects service (v0.3.9a).
/// Provides border flash effects for combat feedback using Spectre.Console color overrides.
/// </summary>
/// <remarks>See: SPEC-RENDER-001 for Rendering Pipeline System design.</remarks>
public class VisualEffectService : IVisualEffectService
{
    private readonly ILogger<VisualEffectService> _logger;
    private string? _borderOverride;

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

        // Set border override (flash on)
        SetBorderOverride(color);

        // Wait for flash duration
        await Task.Delay(durationMs);

        // Clear override (flash off)
        ClearBorderOverride();

        _logger.LogTrace("[VFX] Effect {EffectType} completed", effectType);
    }

    /// <inheritdoc />
    public void SetBorderOverride(string? colorOverride)
    {
        _borderOverride = colorOverride;
        _logger.LogTrace("[VFX] Border override set to {Color}", colorOverride ?? "(null)");
    }

    /// <inheritdoc />
    public string? GetBorderOverride()
    {
        return _borderOverride;
    }

    /// <inheritdoc />
    public void ClearBorderOverride()
    {
        _borderOverride = null;
        _logger.LogTrace("[VFX] Border override cleared");
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
