namespace RuneAndRust.Presentation.Gui.Services;

using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Provides visual feedback animations for combat actions.
/// </summary>
/// <remarks>
/// <para>
/// This service orchestrates non-blocking visual effects during combat including:
/// <list type="bullet">
///   <item><description>Floating damage/healing numbers</description></item>
///   <item><description>Miss and block indicators</description></item>
///   <item><description>Turn start glow effects</description></item>
///   <item><description>Status effect notifications</description></item>
/// </list>
/// </para>
/// <para>
/// All methods are asynchronous and designed to complete without blocking
/// the combat flow. Animations run concurrently with gameplay.
/// </para>
/// </remarks>
public interface IAttackAnimationService
{
    /// <summary>
    /// Displays a floating damage number at the specified position.
    /// </summary>
    /// <param name="position">Grid position where the popup appears.</param>
    /// <param name="damage">Amount of damage dealt.</param>
    /// <param name="isCritical">True for critical hit styling (gold, larger).</param>
    /// <returns>A task that completes when the animation finishes (800ms).</returns>
    Task ShowDamagePopupAsync(GridPosition position, int damage, bool isCritical);

    /// <summary>
    /// Displays a floating healing number at the specified position.
    /// </summary>
    /// <param name="position">Grid position where the popup appears.</param>
    /// <param name="healing">Amount of healing received.</param>
    /// <returns>A task that completes when the animation finishes (800ms).</returns>
    Task ShowHealingPopupAsync(GridPosition position, int healing);

    /// <summary>
    /// Displays a "MISS" indicator at the specified position.
    /// </summary>
    /// <param name="position">Grid position where the popup appears.</param>
    /// <returns>A task that completes when the animation finishes (800ms).</returns>
    Task ShowMissPopupAsync(GridPosition position);

    /// <summary>
    /// Displays a "BLOCKED" indicator at the specified position.
    /// </summary>
    /// <param name="position">Grid position where the popup appears.</param>
    /// <returns>A task that completes when the animation finishes (800ms).</returns>
    Task ShowBlockPopupAsync(GridPosition position);

    /// <summary>
    /// Highlights the combatant whose turn is starting with a pulsing glow.
    /// </summary>
    /// <param name="combatant">The combatant whose turn is starting.</param>
    /// <returns>A task that completes when the glow animation finishes (3 pulses).</returns>
    Task HighlightTurnStartAsync(Combatant combatant);

    /// <summary>
    /// Shows a brief indicator when a status effect is applied.
    /// </summary>
    /// <param name="position">Grid position of the affected entity.</param>
    /// <param name="effect">The status effect that was applied.</param>
    /// <returns>A task that completes when the indicator fades (600ms).</returns>
    Task ShowStatusAppliedAsync(GridPosition position, StatusEffectDefinition effect);

    /// <summary>
    /// Shows a fade-out indicator when a status effect expires.
    /// </summary>
    /// <param name="position">Grid position of the affected entity.</param>
    /// <param name="effect">The status effect that expired.</param>
    /// <returns>A task that completes when the indicator fades (600ms).</returns>
    Task ShowStatusExpiredAsync(GridPosition position, StatusEffectDefinition effect);
}
