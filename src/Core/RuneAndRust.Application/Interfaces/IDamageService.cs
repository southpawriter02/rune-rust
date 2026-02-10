using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Defines the contract for applying damage with protective ability integration.
/// Handles Unbreakable reduction and The Wall Lives lethal damage prevention.
/// </summary>
/// <remarks>
/// <para>Damage pipeline order:</para>
/// <list type="number">
/// <item>Apply Unbreakable reduction (-3, minimum 1 damage)</item>
/// <item>Check The Wall Lives protection (cap damage to preserve 1 HP)</item>
/// <item>Apply final damage to character HP</item>
/// </list>
/// </remarks>
public interface IDamageService
{
    /// <summary>
    /// Applies damage to a target player with all protective abilities integrated.
    /// </summary>
    /// <param name="target">The player receiving damage.</param>
    /// <param name="incomingDamage">Raw damage before any reductions.</param>
    /// <returns>The actual damage applied after all reductions.</returns>
    /// <remarks>
    /// Modifies the player's Health directly. The caller is responsible for persistence.
    /// </remarks>
    int ApplyDamage(Player target, int incomingDamage);

    /// <summary>
    /// Calculates the final damage after all reductions and protections,
    /// without actually applying it.
    /// </summary>
    /// <param name="target">The player to calculate damage for.</param>
    /// <param name="incomingDamage">Raw damage amount.</param>
    /// <returns>Effective damage after all reductions.</returns>
    int CalculateFinalDamage(Player target, int incomingDamage);

    /// <summary>
    /// Checks and applies lethal damage protection from The Wall Lives.
    /// </summary>
    /// <param name="player">The player with potential protection.</param>
    /// <param name="damageAmount">Damage amount to check.</param>
    /// <returns>
    /// Potentially reduced damage if The Wall Lives would prevent lethal damage;
    /// otherwise the original damage amount.
    /// </returns>
    int CheckLethalDamageProtection(Player player, int damageAmount);
}
