// ═══════════════════════════════════════════════════════════════════════════════
// ISkjaldmaerAbilityService.cs
// Interface for Skjaldmær-specific ability operations.
// Version: 0.20.1a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Defines operations for Skjaldmær specialization abilities.
/// </summary>
/// <remarks>
/// <para>
/// This interface defines the contract for Shield Wall, Intercept, and Bulwark
/// ability execution. Implementation is deferred until combat system hooks
/// and character entity integration are available.
/// </para>
/// </remarks>
public interface ISkjaldmaerAbilityService
{
    /// <summary>
    /// Activates Shield Wall stance at the specified position.
    /// </summary>
    /// <param name="position">Grid position for stance activation.</param>
    /// <returns>The activated ShieldWallState.</returns>
    ShieldWallState ActivateShieldWall((int X, int Y) position);

    /// <summary>
    /// Deactivates an active Shield Wall stance.
    /// </summary>
    /// <returns>An inactive ShieldWallState.</returns>
    ShieldWallState DeactivateShieldWall();

    /// <summary>
    /// Calculates total defense bonus for a character with active Shield Wall.
    /// </summary>
    /// <param name="shieldWallState">Current Shield Wall state.</param>
    /// <returns>Defense bonus (0 if inactive).</returns>
    int CalculateDefenseBonus(ShieldWallState? shieldWallState);

    /// <summary>
    /// Calculates Bulwark Max HP bonus based on current charges.
    /// </summary>
    /// <param name="blockCharges">Current Block Charge resource.</param>
    /// <returns>Max HP bonus from Bulwark passive.</returns>
    int CalculateBulwarkHpBonus(BlockChargeResource? blockCharges);
}
