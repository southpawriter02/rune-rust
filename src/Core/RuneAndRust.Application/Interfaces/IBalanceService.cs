namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Defines operations for balance check mechanics.
/// </summary>
/// <remarks>
/// <para>
/// The balance service handles traversal of narrow or unstable surfaces,
/// calculating DCs based on width and stability, processing skill checks,
/// and coordinating with the fall damage system on failures.
/// </para>
/// <para>
/// <b>DC Calculation Flow:</b>
/// <list type="bullet">
///   <item><description>Base DC from width (2-5 successes)</description></item>
///   <item><description>+ Stability modifier (0-2)</description></item>
///   <item><description>+ Condition modifier (0-2)</description></item>
///   <item><description>+ Environmental modifiers (wind, encumbrance)</description></item>
///   <item><description>- Equipment bonuses (balance pole)</description></item>
/// </list>
/// </para>
/// <para>
/// <b>v0.15.2f:</b> Initial implementation of balance service contract.
/// </para>
/// </remarks>
public interface IBalanceService
{
    /// <summary>
    /// Attempts to cross a balanced surface.
    /// </summary>
    /// <param name="surface">The surface to cross.</param>
    /// <param name="dicePool">The dice pool size for the balance check.</param>
    /// <param name="windModifier">Optional wind condition modifier.</param>
    /// <param name="encumbranceModifier">Optional encumbrance modifier.</param>
    /// <param name="hasBalancePole">Whether character has a balance aid.</param>
    /// <returns>The result of the balance attempt.</returns>
    BalanceCheckResult AttemptBalance(
        BalanceSurface surface,
        int dicePool,
        int windModifier = 0,
        int encumbranceModifier = 0,
        bool hasBalancePole = false);

    /// <summary>
    /// Creates a balance context with all modifiers calculated.
    /// </summary>
    /// <param name="surface">The surface to analyze.</param>
    /// <param name="windModifier">Wind condition modifier.</param>
    /// <param name="encumbranceModifier">Encumbrance modifier.</param>
    /// <param name="hasBalancePole">Whether character has balance aid.</param>
    /// <returns>Complete balance context with DC calculation.</returns>
    BalanceContext CreateContext(
        BalanceSurface surface,
        int windModifier = 0,
        int encumbranceModifier = 0,
        bool hasBalancePole = false);

    /// <summary>
    /// Calculates the DC for a balance check given the surface and conditions.
    /// </summary>
    /// <param name="surface">The surface being balanced upon.</param>
    /// <param name="windModifier">Wind condition modifier.</param>
    /// <param name="encumbranceModifier">Character encumbrance modifier.</param>
    /// <param name="hasBalancePole">Whether character has balance aid.</param>
    /// <returns>The total DC in successes required.</returns>
    int CalculateDc(
        BalanceSurface surface,
        int windModifier = 0,
        int encumbranceModifier = 0,
        bool hasBalancePole = false);

    /// <summary>
    /// Determines if a surface requires balance checks based on width.
    /// </summary>
    /// <param name="widthInches">The surface width in inches.</param>
    /// <returns>True if balance check is required.</returns>
    bool RequiresBalanceCheck(int widthInches);

    /// <summary>
    /// Converts a width in inches to the appropriate BalanceWidth category.
    /// </summary>
    /// <param name="widthInches">The surface width in inches.</param>
    /// <returns>The corresponding balance width category.</returns>
    BalanceWidth GetWidthCategory(int widthInches);

    /// <summary>
    /// Gets the base DC for a given width category.
    /// </summary>
    /// <param name="width">The width category.</param>
    /// <returns>The base DC in successes.</returns>
    int GetBaseDc(BalanceWidth width);

    /// <summary>
    /// Gets the DC modifier for a given stability state.
    /// </summary>
    /// <param name="stability">The stability state.</param>
    /// <returns>The DC modifier.</returns>
    int GetStabilityModifier(SurfaceStability stability);

    /// <summary>
    /// Gets the DC modifier for a given surface condition.
    /// </summary>
    /// <param name="condition">The surface condition.</param>
    /// <returns>The DC modifier.</returns>
    int GetConditionModifier(SurfaceCondition condition);
}
