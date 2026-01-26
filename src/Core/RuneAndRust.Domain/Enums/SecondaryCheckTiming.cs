using System.ComponentModel;

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines when the secondary check in a combined skill check should execute.
/// </summary>
/// <remarks>
/// <para>
/// In the Exploration Synergies system, combined checks consist of a primary
/// Wasteland Survival check followed by a secondary skill check. This enum
/// controls when the secondary check is triggered based on the primary result.
/// </para>
/// <para>
/// Timing modes:
/// <list type="bullet">
///   <item><description><see cref="OnPrimarySuccess"/>: Most common; secondary only executes if primary succeeds</description></item>
///   <item><description><see cref="OnPrimaryCritical"/>: Secondary only executes on exceptional primary success</description></item>
///   <item><description><see cref="Always"/>: Secondary always executes regardless of primary outcome</description></item>
/// </list>
/// </para>
/// <para>
/// The default timing for all v0.15.5i synergies is <see cref="OnPrimarySuccess"/>,
/// meaning players must first succeed at the Wasteland Survival check before
/// attempting the secondary skill.
/// </para>
/// </remarks>
/// <seealso cref="SynergyType"/>
public enum SecondaryCheckTiming
{
    /// <summary>
    /// Secondary check executes only when the primary check succeeds.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the standard timing for exploration synergies. The secondary
    /// check is gated behind primary success, creating a natural two-step
    /// exploration sequence.
    /// </para>
    /// <para>
    /// Examples:
    /// <list type="bullet">
    ///   <item><description>Find Hidden Path: Must find the path before attempting to traverse it</description></item>
    ///   <item><description>Track to Lair: Must track to the lair before attempting entry</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    [Description("On Primary Success")]
    OnPrimarySuccess = 0,

    /// <summary>
    /// Secondary check executes only when the primary check achieves critical success.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This timing mode reserves the secondary opportunity for exceptional
    /// primary performance. Used for rare synergies that require outstanding
    /// primary skill.
    /// </para>
    /// <para>
    /// Critical success in the success-counting system means achieving
    /// a margin of 5 or more (net successes - DC ≥ 5).
    /// </para>
    /// </remarks>
    [Description("On Primary Critical")]
    OnPrimaryCritical = 1,

    /// <summary>
    /// Secondary check always executes regardless of primary result.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This timing mode allows the secondary check to proceed even if the
    /// primary check fails. Useful for situations where both skills can
    /// be attempted independently.
    /// </para>
    /// <para>
    /// Note: This timing is not used by any v0.15.5i synergies but is
    /// provided for future expansion of the combined check system.
    /// </para>
    /// </remarks>
    [Description("Always")]
    Always = 2
}
