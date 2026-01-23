using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for processing fall damage and Crash Landing attempts.
/// </summary>
/// <remarks>
/// <para>
/// The fall damage service handles the complete fall damage workflow:
/// <list type="bullet">
///   <item><description>Calculate damage from fall height (1d10 per 10ft, max 10d10)</description></item>
///   <item><description>Process Crash Landing attempts to reduce damage</description></item>
///   <item><description>Roll final damage dice</description></item>
///   <item><description>Aggregate results for display</description></item>
/// </list>
/// </para>
/// <para>
/// Crash Landing DC (success-counting):
/// <list type="bullet">
///   <item><description>DC = 2 + (Height / 10) successes needed</description></item>
///   <item><description>Each success above DC reduces damage by 1d10</description></item>
/// </list>
/// </para>
/// </remarks>
public interface IFallDamageService
{
    /// <summary>
    /// Calculates fall damage parameters from a height.
    /// </summary>
    /// <param name="heightFeet">The height fallen in feet.</param>
    /// <param name="source">The source of the fall.</param>
    /// <param name="bonusDice">Additional damage dice from fumble effects.</param>
    /// <returns>A FallDamage with calculated parameters.</returns>
    /// <example>
    /// <code>
    /// var fallDamage = service.CalculateFallDamage(30, FallSource.Climbing);
    /// // DamageDice = 3, CrashLandingDc = 5
    /// </code>
    /// </example>
    FallDamage CalculateFallDamage(
        int heightFeet,
        FallSource source = FallSource.Environmental,
        int bonusDice = 0);

    /// <summary>
    /// Calculates fall damage from an existing FallResult.
    /// </summary>
    /// <param name="fallResult">The fall result from climbing or leaping.</param>
    /// <returns>A FallDamage with parameters derived from the FallResult.</returns>
    FallDamage CalculateFallDamage(FallResult fallResult);

    /// <summary>
    /// Attempts a Crash Landing to reduce fall damage.
    /// </summary>
    /// <param name="characterId">The ID of the falling character.</param>
    /// <param name="fallDamage">The fall damage to potentially reduce.</param>
    /// <param name="baseDicePool">The character's base Acrobatics dice pool.</param>
    /// <param name="context">Optional skill context with modifiers.</param>
    /// <returns>A CrashLandingResult with the attempt outcome.</returns>
    /// <example>
    /// <code>
    /// var crashResult = service.AttemptCrashLanding("player-1", fallDamage, baseDicePool: 5);
    /// if (crashResult.ReducedDamage)
    /// {
    ///     Console.WriteLine($"Reduced by {crashResult.DiceReduced}d10!");
    /// }
    /// </code>
    /// </example>
    CrashLandingResult AttemptCrashLanding(
        string characterId,
        FallDamage fallDamage,
        int baseDicePool,
        SkillContext? context = null);

    /// <summary>
    /// Rolls the final damage dice.
    /// </summary>
    /// <param name="damageDice">The number of d10 to roll.</param>
    /// <returns>The total damage rolled (sum of all dice).</returns>
    int RollDamage(int damageDice);

    /// <summary>
    /// Processes a complete fall with optional Crash Landing.
    /// </summary>
    /// <param name="characterId">The ID of the falling character.</param>
    /// <param name="fallResult">The fall result from climbing or leaping.</param>
    /// <param name="baseDicePool">The character's base Acrobatics dice pool.</param>
    /// <param name="attemptCrashLanding">Whether to attempt Crash Landing.</param>
    /// <param name="context">Optional skill context with modifiers.</param>
    /// <returns>A FallDamageResult with complete fall outcome.</returns>
    /// <example>
    /// <code>
    /// var result = service.ProcessFall("player-1", fallResult, baseDicePool: 5);
    /// Console.WriteLine($"Took {result.DamageRolled} damage from {result.FallHeight}ft fall");
    /// </code>
    /// </example>
    FallDamageResult ProcessFall(
        string characterId,
        FallResult fallResult,
        int baseDicePool,
        bool attemptCrashLanding = true,
        SkillContext? context = null);

    /// <summary>
    /// Gets the Crash Landing DC for a given fall height.
    /// </summary>
    /// <param name="heightFeet">The fall height in feet.</param>
    /// <returns>The DC in successes needed for Crash Landing (0 if height below threshold).</returns>
    int GetCrashLandingDc(int heightFeet);

    /// <summary>
    /// Calculates the number of damage dice for a given fall height.
    /// </summary>
    /// <param name="heightFeet">The fall height in feet.</param>
    /// <returns>The number of d10 damage dice (0-10).</returns>
    int GetDamageDice(int heightFeet);
}
