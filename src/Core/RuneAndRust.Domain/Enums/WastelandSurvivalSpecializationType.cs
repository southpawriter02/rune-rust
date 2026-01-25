using System.ComponentModel;

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Wasteland Survival skill specializations that provide unique bonuses and abilities.
/// </summary>
/// <remarks>
/// <para>
/// Each specialization represents a distinct approach to survival in the wasteland,
/// granting passive bonuses and active abilities that enhance specific types of
/// Wasteland Survival checks.
/// </para>
/// <para>
/// Specializations are typically tied to character backgrounds or class choices:
/// <list type="bullet">
///   <item><description><see cref="Veioimaor"/>: Hunter archetype focused on tracking living creatures</description></item>
///   <item><description><see cref="MyrStalker"/>: Swamp dweller adapted to toxic environments</description></item>
///   <item><description><see cref="GantryRunner"/>: Urban survivor skilled in navigating ruins</description></item>
/// </list>
/// </para>
/// <para>
/// A character can only have one Wasteland Survival specialization at a time.
/// The <see cref="None"/> value indicates no specialization has been selected.
/// </para>
/// </remarks>
/// <seealso cref="WastelandSurvivalCheckType"/>
public enum WastelandSurvivalSpecializationType
{
    /// <summary>
    /// No specialization selected.
    /// </summary>
    /// <remarks>
    /// Characters without a specialization receive no passive bonuses or
    /// special abilities for Wasteland Survival checks.
    /// </remarks>
    [Description("No Specialization")]
    None = 0,

    /// <summary>
    /// Hunter specialization focused on tracking and hunting living creatures.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Veioimaor are legendary hunters who have honed their tracking skills
    /// to supernatural levels. They can read the signs of prey passage that others
    /// would miss entirely.
    /// </para>
    /// <para>
    /// <strong>Abilities:</strong>
    /// <list type="bullet">
    ///   <item><description><strong>Beast Tracker (Passive):</strong> +2d10 to tracking checks when pursuing living creatures</description></item>
    ///   <item><description><strong>Predator's Eye (Active):</strong> After successful tracking, reveal creature weakness and behavior patterns</description></item>
    ///   <item><description><strong>Hunting Grounds (Active):</strong> Mark an area as hunting grounds for +2d10 to all Wasteland Survival checks in that zone until rest</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    [Description("Veioimaor (Hunter)")]
    Veioimaor = 1,

    /// <summary>
    /// Swamp dweller specialization adapted to toxic and marshy environments.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Myr-Stalkers have spent their lives in the poisonous bogs and marshlands
    /// of the wasteland, developing an innate resistance to toxins and an
    /// instinctive knowledge of safe paths through treacherous terrain.
    /// </para>
    /// <para>
    /// <strong>Abilities:</strong>
    /// <list type="bullet">
    ///   <item><description><strong>Swamp Navigator (Passive):</strong> +1d10 to all Wasteland Survival checks in swamp or marsh terrain</description></item>
    ///   <item><description><strong>Toxin Resistance (Passive):</strong> Advantage on saves against poison gas and toxic hazards</description></item>
    ///   <item><description><strong>Mire Knowledge (Active):</strong> When navigating a bog, reveal a safe path through the hazards</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    [Description("Myr-Stalker")]
    MyrStalker = 2,

    /// <summary>
    /// Urban survivor specialization skilled in navigating ruined cityscapes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Gantry-Runners are survivors of the great ruined cities, experts at
    /// navigating the treacherous urban landscape via rooftops, gantries,
    /// and elevated pathways that others would never consider.
    /// </para>
    /// <para>
    /// <strong>Abilities:</strong>
    /// <list type="bullet">
    ///   <item><description><strong>Urban Navigator (Passive):</strong> +1d10 to all Wasteland Survival checks in ruins terrain</description></item>
    ///   <item><description><strong>Rooftop Routes (Active):</strong> When seeking an elevated path, reveal a safe route across rooftops and gantries</description></item>
    ///   <item><description><strong>Scrap Familiar (Passive):</strong> +1d10 to foraging checks when in ruins terrain</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    [Description("Gantry-Runner")]
    GantryRunner = 3
}
