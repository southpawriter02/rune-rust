namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Categorizes the types of obstacles that can appear during a chase sequence.
/// </summary>
/// <remarks>
/// <para>
/// Each obstacle type maps to a specific skill check. The environment and
/// context determine which obstacle types are available for generation.
/// Obstacle types integrate with existing acrobatics systems where applicable.
/// </para>
/// <para>
/// <b>v0.15.2e:</b> Initial implementation of chase obstacle types.
/// </para>
/// </remarks>
public enum ObstacleType
{
    /// <summary>
    /// A gap that must be leaped across. Uses Acrobatics (leap) check.
    /// </summary>
    /// <remarks>
    /// Examples: rooftop gaps, broken bridges, chasms.
    /// Integrates with ILeapService for check resolution.
    /// DC typically 2-4 successes based on gap width.
    /// </remarks>
    Gap = 0,

    /// <summary>
    /// A vertical surface that must be climbed. Uses Acrobatics (climb) check.
    /// </summary>
    /// <remarks>
    /// Examples: walls, fences, scaffolding.
    /// Integrates with IClimbingService for check resolution.
    /// DC typically 2-4 successes based on surface difficulty.
    /// </remarks>
    Climb = 1,

    /// <summary>
    /// Scattered debris that must be navigated. Uses Acrobatics (balance) check.
    /// </summary>
    /// <remarks>
    /// Examples: rubble, broken glass, fallen crates.
    /// Requires careful footing to traverse quickly.
    /// DC typically 2-3 successes.
    /// </remarks>
    Debris = 2,

    /// <summary>
    /// A crowd or group of obstacles to weave through. Uses Acrobatics (dodge) check.
    /// </summary>
    /// <remarks>
    /// Examples: marketplace crowds, fleeing civilians, patrol groups.
    /// Tests ability to maneuver through moving obstacles.
    /// DC typically 3-4 successes in dense crowds.
    /// </remarks>
    Crowd = 3,

    /// <summary>
    /// An environmental hazard to avoid. Uses Acrobatics (avoid) check.
    /// </summary>
    /// <remarks>
    /// Examples: steam vents, electrified floors, collapsing structures.
    /// Represents dangers that must be evaded rather than traversed.
    /// DC varies widely (2-5) based on hazard severity.
    /// </remarks>
    Hazard = 4
}
