using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service interface for managing climbing operations.
/// </summary>
/// <remarks>
/// <para>
/// The climbing service orchestrates multi-stage climbing attempts, handling:
/// <list type="bullet">
///   <item><description>Climb initiation with context setup</description></item>
///   <item><description>Stage attempts with dice rolling and outcome determination</description></item>
///   <item><description>Fall processing when fumbles occur</description></item>
///   <item><description>Climb abandonment for safe exits</description></item>
/// </list>
/// </para>
/// <para>
/// Stage Mechanics:
/// <list type="bullet">
///   <item><description>Critical Success (margin ≥ 5): Advance 2 stages</description></item>
///   <item><description>Success (margin 0-4): Advance 1 stage</description></item>
///   <item><description>Failure (margin &lt; 0): Slip back 1 stage</description></item>
///   <item><description>Fumble (0 successes + botch): Fall and trigger [The Slip]</description></item>
/// </list>
/// </para>
/// </remarks>
public interface IClimbingService
{
    /// <summary>
    /// Starts a new climbing attempt for a character.
    /// </summary>
    /// <param name="characterId">The climbing character's identifier.</param>
    /// <param name="context">The climbing context with all parameters.</param>
    /// <returns>A new ClimbState in InProgress status.</returns>
    /// <exception cref="ArgumentException">Thrown if characterId is null or empty.</exception>
    /// <exception cref="ArgumentException">Thrown if context has invalid height.</exception>
    ClimbState StartClimb(string characterId, ClimbContext context);

    /// <summary>
    /// Attempts the next climbing stage.
    /// </summary>
    /// <param name="climbState">The current climb state.</param>
    /// <param name="baseDicePool">Base dice pool from character's skill rating.</param>
    /// <param name="additionalContext">Optional additional modifiers for this attempt.</param>
    /// <returns>The result of the stage attempt.</returns>
    /// <exception cref="InvalidOperationException">Thrown if climb is not in progress.</exception>
    /// <exception cref="InvalidOperationException">Thrown if no more stages to attempt.</exception>
    /// <remarks>
    /// The final dice pool is calculated as:
    /// baseDicePool + context modifiers + additional modifiers (minimum 1d10).
    /// </remarks>
    ClimbStageResult AttemptStage(
        ClimbState climbState,
        int baseDicePool,
        SkillContext? additionalContext = null);

    /// <summary>
    /// Processes a fall after a fumble.
    /// </summary>
    /// <param name="climbState">The climb state with Fallen status.</param>
    /// <returns>The fall result with damage parameters.</returns>
    /// <exception cref="InvalidOperationException">Thrown if climb status is not Fallen.</exception>
    /// <remarks>
    /// This creates a FallResult for use by the Fall Damage System (v0.15.2c).
    /// Actual damage calculation and Crash Landing attempts are handled there.
    /// </remarks>
    FallResult ProcessFall(ClimbState climbState);

    /// <summary>
    /// Safely abandons the current climb.
    /// </summary>
    /// <param name="climbState">The climb state to abandon.</param>
    /// <exception cref="InvalidOperationException">Thrown if climb is not in progress.</exception>
    /// <remarks>
    /// Abandoning a climb returns the character safely to ground level
    /// without fall damage. The climb status is set to Abandoned.
    /// </remarks>
    void AbandonClimb(ClimbState climbState);

    /// <summary>
    /// Calculates the number of stages required for a given height.
    /// </summary>
    /// <param name="heightFeet">The height to climb in feet.</param>
    /// <returns>The number of stages (0-3) required.</returns>
    /// <remarks>
    /// <list type="bullet">
    ///   <item><description>≤0ft: 0 stages</description></item>
    ///   <item><description>1-20ft: 1 stage</description></item>
    ///   <item><description>21-40ft: 2 stages</description></item>
    ///   <item><description>41+ft: 3 stages</description></item>
    /// </list>
    /// </remarks>
    int CalculateStagesRequired(int heightFeet);

    /// <summary>
    /// Gets the dice pool modifier for a surface type.
    /// </summary>
    /// <param name="surfaceType">The surface type.</param>
    /// <returns>The dice modifier (+1 to -3, or 0 for DC-modifying surfaces).</returns>
    int GetSurfaceDiceModifier(SurfaceType surfaceType);

    /// <summary>
    /// Gets the DC modifier for a surface type.
    /// </summary>
    /// <param name="surfaceType">The surface type.</param>
    /// <returns>The DC modifier (0 for most surfaces, +2 for Glitched).</returns>
    int GetSurfaceDcModifier(SurfaceType surfaceType);
}
