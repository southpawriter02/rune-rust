using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Defines the contract for leaping system operations.
/// </summary>
/// <remarks>
/// <para>
/// The leaping service manages horizontal traversal attempts, including:
/// <list type="bullet">
///   <item><description>Calculating DCs based on distance and modifiers</description></item>
///   <item><description>Processing leap attempts with skill checks</description></item>
///   <item><description>Handling fumble consequences ([The Long Fall])</description></item>
///   <item><description>Calculating stamina costs based on outcome</description></item>
/// </list>
/// </para>
/// </remarks>
public interface ILeapingService
{
    /// <summary>
    /// Attempts a leap with the specified context.
    /// </summary>
    /// <param name="characterId">The ID of the character attempting the leap.</param>
    /// <param name="context">The leap context with all parameters.</param>
    /// <param name="baseDicePool">The character's base dice pool for Acrobatics.</param>
    /// <param name="additionalContext">Optional additional skill context modifiers.</param>
    /// <returns>The result of the leap attempt.</returns>
    /// <example>
    /// <code>
    /// var context = LeapContext.WithRunningStart(15, fallDepth: 30);
    /// var result = leapingService.AttemptLeap("player-1", context, baseDicePool: 5);
    /// if (result.Landed)
    /// {
    ///     // Apply stamina cost
    /// }
    /// else
    /// {
    ///     // Process fall damage
    /// }
    /// </code>
    /// </example>
    LeapResult AttemptLeap(
        string characterId,
        LeapContext context,
        int baseDicePool,
        SkillContext? additionalContext = null);

    /// <summary>
    /// Calculates the DC for a leap with the given parameters.
    /// </summary>
    /// <param name="distanceFeet">The distance to leap in feet.</param>
    /// <param name="hasRunningStart">Whether a running start is available.</param>
    /// <param name="landingType">The type of landing surface.</param>
    /// <param name="isEncumbered">Whether the character is encumbered.</param>
    /// <param name="hasLowGravity">Whether [Low Gravity] applies.</param>
    /// <returns>The final DC for the leap.</returns>
    int CalculateDc(
        int distanceFeet,
        bool hasRunningStart = false,
        LandingType landingType = LandingType.Normal,
        bool isEncumbered = false,
        bool hasLowGravity = false);

    /// <summary>
    /// Determines the leap distance category from feet.
    /// </summary>
    /// <param name="distanceFeet">The distance in feet.</param>
    /// <returns>The LeapDistance category.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if distance is less than 1 or exceeds 25 feet.
    /// </exception>
    LeapDistance GetDistanceCategory(int distanceFeet);

    /// <summary>
    /// Gets the base DC for a leap distance category.
    /// </summary>
    /// <param name="distance">The leap distance category.</param>
    /// <returns>The base DC in successes required.</returns>
    int GetBaseDc(LeapDistance distance);

    /// <summary>
    /// Gets the stamina cost for a leap distance.
    /// </summary>
    /// <param name="distance">The leap distance category.</param>
    /// <param name="outcome">The outcome to apply stamina modifiers.</param>
    /// <returns>The final stamina cost.</returns>
    /// <remarks>
    /// Stamina modifiers by outcome:
    /// <list type="bullet">
    ///   <item><description>Critical Success: Half stamina (round down)</description></item>
    ///   <item><description>Marginal Success: +1 stamina</description></item>
    ///   <item><description>Failure/Fumble: No stamina (fall damage instead)</description></item>
    /// </list>
    /// </remarks>
    int GetStaminaCost(LeapDistance distance, SkillOutcome outcome);

    /// <summary>
    /// Determines if a leap distance requires Master rank.
    /// </summary>
    /// <param name="distanceFeet">The distance in feet.</param>
    /// <returns>True if Heroic distance (21-25ft) requiring Master rank.</returns>
    bool RequiresMasterRank(int distanceFeet);
}
