namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Provides integration between the skill check system and the trauma economy.
/// </summary>
/// <remarks>
/// <para>
/// The Trauma Economy links skill interactions with corrupted objects and areas
/// to psychic stress accumulation. This service calculates the stress impact
/// of skill checks based on corruption exposure and fumble outcomes.
/// </para>
/// <para>
/// Stress costs scale with corruption tier:
/// <list type="bullet">
///   <item>Normal: 0 stress (no psychic interference)</item>
///   <item>Glitched: 2 stress (minor psychic noise)</item>
///   <item>Blighted: 5 stress (significant corruption)</item>
///   <item>Resonance: 10 stress (direct Blight exposure)</item>
/// </list>
/// </para>
/// <para>
/// Fumbles in corrupted areas add bonus stress, amplified by corruption tier.
/// </para>
/// </remarks>
public interface ITraumaIntegrationService
{
    /// <summary>
    /// Calculates the psychic stress result from a skill check.
    /// </summary>
    /// <param name="context">The skill context containing corruption tier information.</param>
    /// <param name="outcomeDetails">The outcome details including fumble status.</param>
    /// <returns>A <see cref="SkillStressResult"/> containing the calculated stress.</returns>
    SkillStressResult CalculateSkillStress(SkillContext context, OutcomeDetails outcomeDetails);

    /// <summary>
    /// Calculates stress from interacting with a corrupted object.
    /// </summary>
    /// <param name="objectCorruptionTier">The corruption tier of the object being manipulated.</param>
    /// <param name="isFumble">Whether the interaction resulted in a fumble.</param>
    /// <returns>A <see cref="SkillStressResult"/> containing the calculated stress.</returns>
    SkillStressResult CalculateObjectInteractionStress(
        CorruptionTier objectCorruptionTier,
        bool isFumble);

    /// <summary>
    /// Calculates cumulative stress from an extended skill check procedure.
    /// </summary>
    /// <param name="context">The skill context containing corruption tier information.</param>
    /// <param name="stepCount">The number of steps completed in the procedure.</param>
    /// <param name="fumbleCount">The number of fumbles that occurred during the procedure.</param>
    /// <returns>A <see cref="SkillStressResult"/> containing the accumulated stress.</returns>
    SkillStressResult CalculateExtendedCheckStress(
        SkillContext context,
        int stepCount,
        int fumbleCount);

    /// <summary>
    /// Gets the base stress cost for a corruption tier without fumble.
    /// </summary>
    /// <param name="tier">The corruption tier.</param>
    /// <returns>The base stress cost.</returns>
    int GetCorruptionStressCost(CorruptionTier tier);

    /// <summary>
    /// Gets the fumble bonus stress for a corruption tier.
    /// </summary>
    /// <param name="tier">The corruption tier where the fumble occurred.</param>
    /// <returns>The fumble bonus stress.</returns>
    int GetFumbleStressCost(CorruptionTier tier);
}
