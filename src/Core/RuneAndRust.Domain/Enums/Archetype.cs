// ═══════════════════════════════════════════════════════════════════════════════
// Archetype.cs
// Enum defining the four character archetypes that determine combat role,
// primary resource pool, and available specializations.
// Version: 0.17.3a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the four archetypes that determine a character's combat role.
/// </summary>
/// <remarks>
/// <para>
/// Archetypes represent the fundamental combat identity of every character
/// in Aethelgard. They determine the primary resource pool, starting abilities,
/// and available specializations. Archetype is a PERMANENT choice that cannot
/// be changed after character creation.
/// </para>
/// <para>
/// The four archetypes cover all major combat roles:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <see cref="Warrior"/>: Tank / Melee DPS focused on damage absorption (Stamina-based)
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="Skirmisher"/>: Mobile DPS focused on hit-and-run tactics (Stamina-based)
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="Mystic"/>: Caster / Control focused on Aetheric abilities (Aether Pool-based)
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="Adept"/>: Support / Utility focused on resource efficiency (Stamina-based)
///     </description>
///   </item>
/// </list>
/// <para>
/// Archetype values are explicitly assigned (0-3) to ensure stable serialization
/// and database storage. The order reflects typical selection frequency.
/// New archetypes should be added at the end if needed.
/// </para>
/// </remarks>
/// <seealso cref="Entities.ArchetypeDefinition"/>
/// <seealso cref="ResourceType"/>
public enum Archetype
{
    /// <summary>
    /// Warrior - Tank / Melee DPS focused on damage absorption.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Warriors are the frontline fighters who absorb punishment and deal
    /// devastating melee damage. They use Stamina as their primary resource
    /// and have the highest HP bonus (+49). Key characteristics:
    /// </para>
    /// <para>
    /// Combat Role: Tank / Melee DPS
    /// </para>
    /// <para>
    /// Primary Resource: Stamina
    /// </para>
    /// <para>
    /// Key Strengths: Highest HP pool in the game, defensive abilities and stances,
    /// melee-focused combat style
    /// </para>
    /// <para>
    /// Available Specializations: 6 (Berserkr, Iron-Bane, Skjaldmaer,
    /// Skar-Horde, Atgeir-Wielder, Gorge-Maw)
    /// </para>
    /// </remarks>
    Warrior = 0,

    /// <summary>
    /// Skirmisher - Mobile DPS focused on hit-and-run tactics.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Skirmishers excel at mobility and precision strikes, weaving in and
    /// out of combat. They use Stamina as their primary resource and have
    /// +1 Movement bonus. Key characteristics:
    /// </para>
    /// <para>
    /// Combat Role: Mobile DPS
    /// </para>
    /// <para>
    /// Primary Resource: Stamina
    /// </para>
    /// <para>
    /// Key Strengths: Enhanced movement speed, evasion-focused defense,
    /// quick and precise attacks
    /// </para>
    /// <para>
    /// Available Specializations: 4 (Veiðimaðr, Myrk-gengr, Strandhögg,
    /// Hlekkr-master)
    /// </para>
    /// </remarks>
    Skirmisher = 1,

    /// <summary>
    /// Mystic - Caster / Control focused on Aetheric abilities.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Mystics channel the corrupted Aether to unleash devastating magical
    /// attacks from range. They are the ONLY archetype using Aether Pool
    /// as their primary resource. Key characteristics:
    /// </para>
    /// <para>
    /// Combat Role: Caster / Control
    /// </para>
    /// <para>
    /// Primary Resource: Aether Pool
    /// </para>
    /// <para>
    /// Key Strengths: Ranged magical damage, crowd control abilities,
    /// highest Aether Pool bonus (+20)
    /// </para>
    /// <para>
    /// Available Specializations: 2 (Seiðkona, Echo-Caller)
    /// </para>
    /// </remarks>
    Mystic = 2,

    /// <summary>
    /// Adept - Support / Utility focused on resource efficiency.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Adepts master mundane arts and make the most of limited resources.
    /// They use Stamina as their primary resource and have unique bonuses
    /// to consumable effectiveness. Key characteristics:
    /// </para>
    /// <para>
    /// Combat Role: Support / Utility
    /// </para>
    /// <para>
    /// Primary Resource: Stamina
    /// </para>
    /// <para>
    /// Key Strengths: +20% consumable effectiveness, support and utility
    /// abilities, resource optimization focus
    /// </para>
    /// <para>
    /// Available Specializations: 5 (Bone-Setter, Jötun-Reader, Skald,
    /// Scrap-Tinker, Einbúi)
    /// </para>
    /// <para>
    /// Note: Adept receives 14 starting attribute points instead of the
    /// standard 15, balancing their broader utility access.
    /// </para>
    /// </remarks>
    Adept = 3
}
