// ═══════════════════════════════════════════════════════════════════════════════
// TechniqueAccess.cs
// Enum defining the tiers of combat techniques accessible at different
// proficiency levels.
// Version: 0.16.1a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the tiers of combat techniques accessible at different proficiency levels.
/// </summary>
/// <remarks>
/// <para>
/// Technique access is cumulative - having <see cref="Advanced"/> access
/// includes <see cref="Basic"/>, and <see cref="Signature"/> access
/// includes both <see cref="Basic"/> and <see cref="Advanced"/>.
/// </para>
/// <para>
/// The mapping between proficiency levels and technique access:
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Proficiency Level</term>
///     <description>Technique Access</description>
///   </listheader>
///   <item>
///     <term>NonProficient</term>
///     <description><see cref="None"/> - No techniques available</description>
///   </item>
///   <item>
///     <term>Proficient</term>
///     <description><see cref="Basic"/> - Standard combat maneuvers</description>
///   </item>
///   <item>
///     <term>Expert</term>
///     <description><see cref="Advanced"/> - Complex maneuvers</description>
///   </item>
///   <item>
///     <term>Master</term>
///     <description><see cref="Signature"/> - Master-level techniques</description>
///   </item>
/// </list>
/// <para>
/// Tier values are explicitly assigned (0-3) to ensure stable serialization
/// and to allow comparison operations (e.g., <c>access >= TechniqueAccess.Basic</c>).
/// </para>
/// </remarks>
/// <seealso cref="WeaponProficiencyLevel"/>
/// <seealso cref="ValueObjects.ProficiencyEffect"/>
public enum TechniqueAccess
{
    /// <summary>
    /// No techniques available.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Non-proficient characters cannot execute any weapon-specific combat
    /// techniques. They are limited to basic attack and defend actions
    /// without the specialized maneuvers that trained combatants can perform.
    /// </para>
    /// <para>
    /// This tier is associated with <see cref="WeaponProficiencyLevel.NonProficient"/>.
    /// </para>
    /// </remarks>
    None = 0,

    /// <summary>
    /// Basic techniques available.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Basic techniques represent standard combat maneuvers taught during
    /// initial weapon training. These include fundamental offensive and
    /// defensive moves appropriate for each weapon category.
    /// </para>
    /// <para>
    /// Examples of basic techniques:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Power Attack - Trade accuracy for damage</description></item>
    ///   <item><description>Defensive Stance - Trade offense for defense</description></item>
    ///   <item><description>Quick Strike - Fast, light attack</description></item>
    /// </list>
    /// <para>
    /// This tier is associated with <see cref="WeaponProficiencyLevel.Proficient"/>.
    /// </para>
    /// </remarks>
    Basic = 1,

    /// <summary>
    /// Advanced techniques available (includes Basic).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Advanced techniques represent complex maneuvers that require
    /// significant practice to execute properly. These techniques offer
    /// more powerful effects than basic ones but may have higher costs
    /// or stricter requirements.
    /// </para>
    /// <para>
    /// Examples of advanced techniques:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Riposte - Counter-attack on successful parry</description></item>
    ///   <item><description>Feint - Trick enemy into exposing weakness</description></item>
    ///   <item><description>Cleaving Strike - Hit multiple adjacent enemies</description></item>
    /// </list>
    /// <para>
    /// This tier is associated with <see cref="WeaponProficiencyLevel.Expert"/>
    /// and includes all <see cref="Basic"/> techniques.
    /// </para>
    /// </remarks>
    Advanced = 2,

    /// <summary>
    /// Signature techniques available (includes Basic and Advanced).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Signature techniques are master-level maneuvers that define a
    /// weapon expert's combat style. These are the pinnacle of weapon
    /// mastery and offer the most powerful effects available through
    /// pure weapon skill.
    /// </para>
    /// <para>
    /// Examples of signature techniques:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Whirlwind Strike - Attack all adjacent enemies simultaneously</description></item>
    ///   <item><description>Perfect Parry - Negate all damage and riposte with bonus</description></item>
    ///   <item><description>Execution - Instantly defeat critically wounded enemies</description></item>
    /// </list>
    /// <para>
    /// This tier is associated with <see cref="WeaponProficiencyLevel.Master"/>
    /// and includes all <see cref="Basic"/> and <see cref="Advanced"/> techniques.
    /// </para>
    /// </remarks>
    Signature = 3
}
