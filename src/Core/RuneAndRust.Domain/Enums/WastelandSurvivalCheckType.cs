using System.ComponentModel;

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Types of Wasteland Survival skill checks that can receive specialization bonuses.
/// </summary>
/// <remarks>
/// <para>
/// Each check type corresponds to a specific subsystem within the Wasteland Survival skill:
/// <list type="bullet">
///   <item><description><see cref="General"/>: Default category for miscellaneous survival checks</description></item>
///   <item><description><see cref="Tracking"/>: Extended Tracking System (v0.15.5a)</description></item>
///   <item><description><see cref="Foraging"/>: Foraging System (v0.15.5c)</description></item>
///   <item><description><see cref="Navigation"/>: Navigation System (v0.15.5d)</description></item>
///   <item><description><see cref="HazardDetection"/>: Hazard Detection System (v0.15.5e)</description></item>
///   <item><description><see cref="SignReading"/>: Scavenger Sign Reading (v0.15.5f)</description></item>
///   <item><description><see cref="Scouting"/>: Scout Action System (v0.15.5g)</description></item>
/// </list>
/// </para>
/// <para>
/// Specialization abilities may apply to specific check types or to all check types
/// depending on their trigger conditions. For example, the Beast Tracker ability
/// only applies to <see cref="Tracking"/> checks, while Hunting Grounds applies
/// to all check types within a marked area.
/// </para>
/// </remarks>
/// <seealso cref="WastelandSurvivalSpecializationType"/>
public enum WastelandSurvivalCheckType
{
    /// <summary>
    /// General survival check not fitting into a specific category.
    /// </summary>
    /// <remarks>
    /// Use this for miscellaneous Wasteland Survival checks that don't fall
    /// into one of the specialized subsystems, such as basic camping setup,
    /// weather prediction, or general environmental awareness.
    /// </remarks>
    [Description("General")]
    General = 0,

    /// <summary>
    /// Tracking check for following trails and pursuing targets.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Tracking checks are used in the Extended Tracking System (v0.15.5a) for:
    /// <list type="bullet">
    ///   <item><description>Initial trail acquisition</description></item>
    ///   <item><description>Pursuit maintenance</description></item>
    ///   <item><description>Trail recovery after losing the target</description></item>
    ///   <item><description>Closing in on the target</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The Veioimaor specialization grants +2d10 to tracking checks when
    /// pursuing living creatures through the Beast Tracker ability.
    /// </para>
    /// </remarks>
    [Description("Tracking")]
    Tracking = 1,

    /// <summary>
    /// Foraging check for finding food, water, and useful resources.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Foraging checks are used in the Foraging System (v0.15.5c) for:
    /// <list type="bullet">
    ///   <item><description>Finding edible plants and fungi</description></item>
    ///   <item><description>Locating water sources</description></item>
    ///   <item><description>Scavenging useful materials</description></item>
    ///   <item><description>Identifying medicinal herbs</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The Gantry-Runner specialization grants +1d10 to foraging checks
    /// in ruins terrain through the Scrap Familiar ability.
    /// </para>
    /// </remarks>
    [Description("Foraging")]
    Foraging = 2,

    /// <summary>
    /// Navigation check for finding paths and avoiding getting lost.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Navigation checks are used in the Navigation System (v0.15.5d) for:
    /// <list type="bullet">
    ///   <item><description>Pathfinding through difficult terrain</description></item>
    ///   <item><description>Avoiding hazards during travel</description></item>
    ///   <item><description>Determining optimal routes</description></item>
    ///   <item><description>Maintaining course in poor visibility</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Both Myr-Stalker (+1d10 in swamp/marsh) and Gantry-Runner (+1d10 in ruins)
    /// provide navigation bonuses through their Navigator abilities.
    /// </para>
    /// </remarks>
    [Description("Navigation")]
    Navigation = 3,

    /// <summary>
    /// Hazard detection check for identifying environmental dangers.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Hazard detection checks are used in the Hazard Detection System (v0.15.5e) for:
    /// <list type="bullet">
    ///   <item><description>Spotting hidden traps</description></item>
    ///   <item><description>Detecting poisonous gases or radiation</description></item>
    ///   <item><description>Identifying unstable structures</description></item>
    ///   <item><description>Recognizing dangerous wildlife signs</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The Myr-Stalker specialization grants advantage on saves against poison
    /// gas hazards through the Toxin Resistance ability.
    /// </para>
    /// </remarks>
    [Description("Hazard Detection")]
    HazardDetection = 4,

    /// <summary>
    /// Sign reading check for interpreting scavenger markers and faction signs.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Sign reading checks are used in the Scavenger Sign System (v0.15.5f) for:
    /// <list type="bullet">
    ///   <item><description>Identifying faction territory markers</description></item>
    ///   <item><description>Interpreting warning signs left by scavengers</description></item>
    ///   <item><description>Reading trail blazes and directional markers</description></item>
    ///   <item><description>Understanding coded messages between survivors</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    [Description("Sign Reading")]
    SignReading = 5,

    /// <summary>
    /// Scouting check for reconnaissance and area assessment.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Scouting checks are used in the Scout Action System (v0.15.5g) for:
    /// <list type="bullet">
    ///   <item><description>Surveying an area for threats and resources</description></item>
    ///   <item><description>Determining enemy positions and numbers</description></item>
    ///   <item><description>Assessing environmental conditions</description></item>
    ///   <item><description>Planning approach routes</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    [Description("Scouting")]
    Scouting = 6
}
