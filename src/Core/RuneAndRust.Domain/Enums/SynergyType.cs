using System.ComponentModel;

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Types of skill synergies for combined exploration checks.
/// </summary>
/// <remarks>
/// <para>
/// Each synergy type represents a multi-skill exploration action where a primary
/// Wasteland Survival check gates access to a secondary skill check:
/// <list type="bullet">
///   <item><description><see cref="FindHiddenPath"/>: Navigation → Acrobatics (traverse difficult terrain)</description></item>
///   <item><description><see cref="TrackToLair"/>: Tracking → System Bypass (enter secured den)</description></item>
///   <item><description><see cref="AvoidPatrol"/>: Hazard Detection → Acrobatics (stealth evasion)</description></item>
///   <item><description><see cref="FindAndLoot"/>: Foraging → System Bypass (open locked cache)</description></item>
/// </list>
/// </para>
/// <para>
/// The combined check system follows a primary-secondary pattern where:
/// <list type="number">
///   <item><description>Primary skill (Wasteland Survival subsystem) gates access to the situation</description></item>
///   <item><description>Secondary skill determines how well the player exploits or navigates the situation</description></item>
///   <item><description>Both checks must succeed for complete success</description></item>
/// </list>
/// </para>
/// </remarks>
/// <seealso cref="WastelandSurvivalCheckType"/>
/// <seealso cref="SecondaryCheckTiming"/>
public enum SynergyType
{
    /// <summary>
    /// Navigation + Acrobatics: Find and traverse a hidden path.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Primary check uses the Navigation subsystem of Wasteland Survival to locate
    /// a secret or difficult passage. Secondary check uses Acrobatics to physically
    /// traverse the challenging terrain (narrow ledges, collapsed tunnels, etc.).
    /// </para>
    /// <para>
    /// Outcome combinations:
    /// <list type="bullet">
    ///   <item><description>Both succeed: Path found and traversed, reach destination</description></item>
    ///   <item><description>Primary succeeds, secondary fails: Path found but cannot traverse</description></item>
    ///   <item><description>Primary fails: No hidden path found</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    [Description("Find Hidden Path")]
    FindHiddenPath = 0,

    /// <summary>
    /// Tracking + System Bypass: Follow trail and enter secured lair.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Primary check uses the Tracking subsystem of Wasteland Survival to follow
    /// prey to its den. Secondary check uses System Bypass to gain entry through
    /// secured, locked, or trapped entrances.
    /// </para>
    /// <para>
    /// Outcome combinations:
    /// <list type="bullet">
    ///   <item><description>Both succeed: Lair found and entered silently</description></item>
    ///   <item><description>Primary succeeds, secondary fails: Lair found but entry blocked</description></item>
    ///   <item><description>Primary fails: Trail lost, lair not found</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    [Description("Track to Lair")]
    TrackToLair = 1,

    /// <summary>
    /// Hazard Detection + Acrobatics: Spot and evade enemy patrol.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Primary check uses the Hazard Detection subsystem of Wasteland Survival to
    /// spot an approaching patrol before being detected. Secondary check uses
    /// Acrobatics (stealth mode) to evade the patrol undetected.
    /// </para>
    /// <para>
    /// Outcome combinations:
    /// <list type="bullet">
    ///   <item><description>Both succeed: Patrol spotted and evaded</description></item>
    ///   <item><description>Primary succeeds, secondary fails: Patrol spotted but detected during evasion</description></item>
    ///   <item><description>Primary fails: Patrol not spotted, surprise encounter</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    [Description("Avoid Patrol")]
    AvoidPatrol = 2,

    /// <summary>
    /// Foraging + System Bypass: Find and open locked cache.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Primary check uses the Foraging subsystem of Wasteland Survival to discover
    /// a hidden, locked cache. Secondary check uses System Bypass to bypass the
    /// lock mechanism and claim the contents.
    /// </para>
    /// <para>
    /// Outcome combinations:
    /// <list type="bullet">
    ///   <item><description>Both succeed: Cache found and opened, full loot</description></item>
    ///   <item><description>Primary succeeds, secondary fails: Cache found but locked</description></item>
    ///   <item><description>Primary fails: No cache found, standard foraging only</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    [Description("Find and Loot")]
    FindAndLoot = 3
}
