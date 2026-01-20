namespace RuneAndRust.Presentation.Gui.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Presentation.Gui.Controls;
using RuneAndRust.Presentation.Gui.Enums;

/// <summary>
/// Provides visual feedback animations for combat actions.
/// </summary>
/// <remarks>
/// <para>
/// This service implements non-blocking combat animations including:
/// <list type="bullet">
///   <item><description>800ms damage/healing popup rise and fade</description></item>
///   <item><description>500ms × 3 glow pulse for turn start</description></item>
///   <item><description>600ms status effect indicators</description></item>
/// </list>
/// </para>
/// <para>
/// All animations are fire-and-forget to avoid blocking combat flow.
/// The service relies on an <see cref="ICombatGridOverlay"/> for popup placement.
/// </para>
/// </remarks>
public class AttackAnimationService : IAttackAnimationService
{
    private readonly ICombatGridOverlay? _overlay;
    private readonly ILogger<AttackAnimationService> _logger;

    /// <summary>
    /// Duration of damage/healing popup animation in milliseconds.
    /// </summary>
    private const int PopupDurationMs = 800;

    /// <summary>
    /// Duration of a single glow pulse in milliseconds.
    /// </summary>
    private const int GlowPulseDurationMs = 500;

    /// <summary>
    /// Number of glow pulses for turn start highlight.
    /// </summary>
    private const int GlowPulseCount = 3;

    /// <summary>
    /// Duration of status indicator display in milliseconds.
    /// </summary>
    private const int StatusIndicatorDurationMs = 600;

    /// <summary>
    /// Creates a new <see cref="AttackAnimationService"/>.
    /// </summary>
    /// <param name="logger">Logger for animation events.</param>
    /// <param name="overlay">Optional combat grid overlay for popup placement.</param>
    public AttackAnimationService(
        ILogger<AttackAnimationService> logger,
        ICombatGridOverlay? overlay = null)
    {
        _logger = logger;
        _overlay = overlay;

        _logger.LogDebug("AttackAnimationService initialized");
    }

    /// <inheritdoc />
    public async Task ShowDamagePopupAsync(GridPosition position, int damage, bool isCritical)
    {
        _logger.LogDebug(
            "Showing damage popup: {Damage}{Critical} at ({X}, {Y})",
            damage,
            isCritical ? " CRIT" : "",
            position.X,
            position.Y);

        var popup = new DamagePopupControl
        {
            Value = damage,
            PopupType = DamagePopupType.Damage,
            IsCritical = isCritical
        };

        await AnimatePopupAsync(popup, position);
    }

    /// <inheritdoc />
    public async Task ShowHealingPopupAsync(GridPosition position, int healing)
    {
        _logger.LogDebug(
            "Showing healing popup: +{Healing} at ({X}, {Y})",
            healing,
            position.X,
            position.Y);

        var popup = new DamagePopupControl
        {
            Value = healing,
            PopupType = DamagePopupType.Healing
        };

        await AnimatePopupAsync(popup, position);
    }

    /// <inheritdoc />
    public async Task ShowMissPopupAsync(GridPosition position)
    {
        _logger.LogDebug(
            "Showing miss popup at ({X}, {Y})",
            position.X,
            position.Y);

        var popup = new DamagePopupControl
        {
            PopupType = DamagePopupType.Miss
        };

        await AnimatePopupAsync(popup, position);
    }

    /// <inheritdoc />
    public async Task ShowBlockPopupAsync(GridPosition position)
    {
        _logger.LogDebug(
            "Showing block popup at ({X}, {Y})",
            position.X,
            position.Y);

        var popup = new DamagePopupControl
        {
            PopupType = DamagePopupType.Block
        };

        await AnimatePopupAsync(popup, position);
    }

    /// <inheritdoc />
    public async Task HighlightTurnStartAsync(Combatant combatant)
    {
        ArgumentNullException.ThrowIfNull(combatant);

        _logger.LogDebug("Highlighting turn start for {Combatant}", combatant.DisplayName);

        var token = _overlay?.GetToken(combatant.Id);
        if (token is null)
        {
            _logger.LogDebug("No token found for combatant {Id}, skipping highlight", combatant.Id);
            return;
        }

        // Pulse glow effect 3 times
        token.ShowGlowEffect = true;

        for (var i = 0; i < GlowPulseCount; i++)
        {
            token.GlowOpacity = 1.0;
            await Task.Delay(GlowPulseDurationMs / 2);
            token.GlowOpacity = 0.4;
            await Task.Delay(GlowPulseDurationMs / 2);
        }

        token.ShowGlowEffect = false;
        token.GlowOpacity = 1.0; // Reset for next time

        _logger.LogDebug("Turn highlight complete for {Combatant}", combatant.DisplayName);
    }

    /// <inheritdoc />
    public async Task ShowStatusAppliedAsync(GridPosition position, StatusEffectDefinition effect)
    {
        ArgumentNullException.ThrowIfNull(effect);

        _logger.LogDebug(
            "Showing status applied: {Effect} at ({X}, {Y})",
            effect.Name,
            position.X,
            position.Y);

        _overlay?.ShowStatusIndicator(position, effect.IconId ?? "⚡", $"+{effect.Name}");
        await Task.Delay(StatusIndicatorDurationMs);
        _overlay?.HideStatusIndicator(position);

        _logger.LogDebug("Status applied indicator complete for {Effect}", effect.Name);
    }

    /// <inheritdoc />
    public async Task ShowStatusExpiredAsync(GridPosition position, StatusEffectDefinition effect)
    {
        ArgumentNullException.ThrowIfNull(effect);

        _logger.LogDebug(
            "Showing status expired: {Effect} at ({X}, {Y})",
            effect.Name,
            position.X,
            position.Y);

        _overlay?.ShowStatusIndicator(position, effect.IconId ?? "⚡", $"-{effect.Name}", fadeOut: true);
        await Task.Delay(StatusIndicatorDurationMs);
        _overlay?.HideStatusIndicator(position);

        _logger.LogDebug("Status expired indicator complete for {Effect}", effect.Name);
    }

    /// <summary>
    /// Animates a popup rising and fading over the configured duration.
    /// </summary>
    /// <param name="popup">The popup control to animate.</param>
    /// <param name="position">Grid position where the popup appears.</param>
    private async Task AnimatePopupAsync(DamagePopupControl popup, GridPosition position)
    {
        if (_overlay is null)
        {
            _logger.LogDebug("No overlay available, waiting popup duration only");
            await Task.Delay(PopupDurationMs);
            return;
        }

        var screenPos = _overlay.GridToScreen(position);
        _overlay.AddPopup(popup, screenPos);

        // Simple timing animation: rise up and fade out over 800ms
        // In a real implementation, this would use Avalonia's animation system
        var startTime = DateTime.UtcNow;
        while ((DateTime.UtcNow - startTime).TotalMilliseconds < PopupDurationMs)
        {
            var t = (DateTime.UtcNow - startTime).TotalMilliseconds / PopupDurationMs;

            // Rise: move up as t increases
            // Fade: start fading at 50%, fully gone at 100%
            popup.Opacity = t < 0.5 ? 1.0 : 1.0 - (t - 0.5) * 2;

            await Task.Delay(16); // ~60fps update rate
        }

        _overlay.RemovePopup(popup);
    }
}
