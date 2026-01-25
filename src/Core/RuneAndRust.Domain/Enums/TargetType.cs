using System.ComponentModel;

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Classification of tracking targets for specialization bonus evaluation.
/// </summary>
/// <remarks>
/// <para>
/// Target types are used to determine which specialization bonuses apply to
/// tracking checks. The Veioimaor's Beast Tracker ability, for example, only
/// grants its +2d10 bonus when tracking <see cref="LivingCreature"/> targets.
/// </para>
/// <para>
/// Target classification should be determined at the start of a tracking pursuit
/// based on available information about the quarry. If the target type is unknown,
/// use <see cref="Unknown"/> until more information is gathered.
/// </para>
/// </remarks>
/// <seealso cref="WastelandSurvivalSpecializationType"/>
/// <seealso cref="WastelandSurvivalCheckType"/>
public enum TargetType
{
    /// <summary>
    /// Target type is unknown or undetermined.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this value when:
    /// <list type="bullet">
    ///   <item><description>The tracker has not yet identified what they are tracking</description></item>
    ///   <item><description>The trail does not provide enough information to classify the target</description></item>
    ///   <item><description>The target's nature is deliberately obscured</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Tracking an unknown target does not trigger creature-specific bonuses
    /// like Beast Tracker until the target type is identified.
    /// </para>
    /// </remarks>
    [Description("Unknown")]
    Unknown = 0,

    /// <summary>
    /// A living creature such as a human, animal, or monster.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Living creatures leave biological traces that skilled trackers can follow:
    /// <list type="bullet">
    ///   <item><description>Footprints and disturbed vegetation</description></item>
    ///   <item><description>Scent trails</description></item>
    ///   <item><description>Body heat traces (in certain conditions)</description></item>
    ///   <item><description>Biological waste and shed materials</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The Veioimaor's Beast Tracker ability grants +2d10 to tracking checks
    /// when pursuing living creature targets.
    /// </para>
    /// </remarks>
    [Description("Living Creature")]
    LivingCreature = 1,

    /// <summary>
    /// A mechanical or magical construct.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Constructs leave different traces than living creatures:
    /// <list type="bullet">
    ///   <item><description>Mechanical wear marks on surfaces</description></item>
    ///   <item><description>Oil or lubricant residue</description></item>
    ///   <item><description>Distinctive tread or foot patterns</description></item>
    ///   <item><description>Magical energy signatures (for magical constructs)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Beast Tracker does not apply to constructs, as they are not living creatures.
    /// </para>
    /// </remarks>
    [Description("Construct")]
    Construct = 2,

    /// <summary>
    /// A vehicle or mounted conveyance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Vehicles leave distinctive traces that are often easier to track:
    /// <list type="bullet">
    ///   <item><description>Wheel ruts or track marks</description></item>
    ///   <item><description>Fuel or exhaust residue</description></item>
    ///   <item><description>Cargo spillage</description></item>
    ///   <item><description>Mount droppings (for animal-drawn vehicles)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Beast Tracker does not apply to vehicle tracking directly, but may apply
    /// if the vehicle is drawn by living creatures.
    /// </para>
    /// </remarks>
    [Description("Vehicle")]
    Vehicle = 3,

    /// <summary>
    /// A group of multiple targets traveling together.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Groups leave more obvious trails but may have mixed composition:
    /// <list type="bullet">
    ///   <item><description>Multiple overlapping tracks</description></item>
    ///   <item><description>Wider path of disturbed terrain</description></item>
    ///   <item><description>More frequent rest stops and camps</description></item>
    ///   <item><description>Greater noise and scent dispersal</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Beast Tracker applies to group targets if the group consists primarily
    /// of living creatures. The bonus applies to tracking the group as a whole.
    /// </para>
    /// </remarks>
    [Description("Group")]
    Group = 4
}
