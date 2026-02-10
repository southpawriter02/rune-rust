// ═══════════════════════════════════════════════════════════════════════════════
// CloneBehavior.cs
// Defines the behavior modes available to Myrk-gengr shadow clones.
// Version: 0.20.4b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the behavior pattern of a shadow clone created by the
/// Myrk-gengr Shadow Clone ability.
/// </summary>
/// <remarks>
/// <para>
/// Each behavior determines how the clone moves and interacts with the
/// battlefield after creation. Behavior is set at creation time and
/// cannot be changed once the clone is active.
/// </para>
/// <list type="bullet">
///   <item><description><b>Mirror:</b> Clone mirrors the caster's movements.</description></item>
///   <item><description><b>Independent:</b> Clone moves toward the nearest enemy.</description></item>
///   <item><description><b>Decoy:</b> Clone moves away from the caster to draw attention.</description></item>
///   <item><description><b>Static:</b> Clone remains at the creation position.</description></item>
/// </list>
/// </remarks>
/// <seealso cref="MyrkgengrAbilityId"/>
public enum CloneBehavior
{
    /// <summary>
    /// Clone mirrors the caster's movements in the opposite direction.
    /// Useful for creating confusion about the caster's true position.
    /// </summary>
    Mirror = 0,

    /// <summary>
    /// Clone moves toward the nearest enemy independently.
    /// Useful for drawing attacks and wasting enemy actions.
    /// </summary>
    Independent = 1,

    /// <summary>
    /// Clone actively moves away from the caster toward enemies.
    /// Best for drawing enemy attention away from the caster's approach.
    /// </summary>
    Decoy = 2,

    /// <summary>
    /// Clone remains at the creation position and does not move.
    /// Useful for blocking passages or holding a position.
    /// </summary>
    Static = 3
}
