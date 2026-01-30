// ═══════════════════════════════════════════════════════════════════════════════
// AbilityType.cs
// Enum defining the classification of abilities by activation type. Abilities
// are categorized as Active (player-activated with resource cost), Passive
// (always-on with no activation), or Stance (toggleable modes that modify
// character behavior). This classification affects display, activation, and
// processing of abilities throughout the game.
// Version: 0.17.3c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the classification of abilities by activation type.
/// </summary>
/// <remarks>
/// <para>
/// AbilityType determines how an ability is used and displayed in the game.
/// Each type has distinct characteristics for activation, resource cost,
/// cooldown behavior, and effect duration. The classification is used by
/// the character creation system to categorize starting abilities granted
/// by archetypes, and by the combat system to determine how abilities
/// are processed during encounters.
/// </para>
/// <para>
/// The three ability types are:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <b>Active</b> — Player-activated abilities that cost resources (Stamina
///       or Aether Pool) and typically have cooldowns between uses. Displayed on
///       the action bar for quick access during combat. Examples include Power
///       Strike, Aether Bolt, and Quick Strike.
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>Passive</b> — Always-on abilities that provide constant benefits with
///       no activation required and no resource cost. Displayed in the abilities
///       list but not on the action bar. Examples include Iron Will, Opportunist,
///       and Aether Sense.
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>Stance</b> — Toggleable modes that modify character behavior while
///       active. No resource cost to activate, but only one stance can be active
///       at a time. Displayed on the stance bar. Examples include Defensive Stance.
///     </description>
///   </item>
/// </list>
/// <para>
/// Starting abilities are distributed across types per archetype:
/// </para>
/// <list type="bullet">
///   <item><description>Warrior: 1 Active, 1 Stance, 1 Passive</description></item>
///   <item><description>Skirmisher: 2 Active, 1 Passive</description></item>
///   <item><description>Mystic: 2 Active, 1 Passive</description></item>
///   <item><description>Adept: 2 Active, 1 Passive</description></item>
/// </list>
/// <para>
/// AbilityType values are explicitly assigned (0-2) to ensure stable
/// serialization and database storage. These integer values must not be
/// changed once persisted.
/// </para>
/// </remarks>
/// <seealso cref="RuneAndRust.Domain.ValueObjects.ArchetypeAbilityGrant"/>
/// <seealso cref="Archetype"/>
/// <seealso cref="RuneAndRust.Domain.Entities.ArchetypeDefinition"/>
public enum AbilityType
{
    /// <summary>
    /// Active abilities require player activation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Active abilities must be explicitly activated by the player during
    /// combat or exploration. They typically consume resources from the
    /// character's primary resource pool (Stamina for Warrior, Skirmisher,
    /// and Adept; Aether Pool for Mystic).
    /// </para>
    /// <para>
    /// Characteristics:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Must be activated by player action</description></item>
    ///   <item><description>Typically costs Stamina or Aether Pool</description></item>
    ///   <item><description>Has cooldown between uses</description></item>
    ///   <item><description>Displayed on action bar for quick access</description></item>
    /// </list>
    /// <para>
    /// Starting active abilities by archetype:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Warrior: Power Strike</description></item>
    ///   <item><description>Skirmisher: Quick Strike, Evasive Maneuvers</description></item>
    ///   <item><description>Mystic: Aether Bolt, Aether Shield</description></item>
    ///   <item><description>Adept: Precise Strike, Assess Weakness</description></item>
    /// </list>
    /// </remarks>
    Active = 0,

    /// <summary>
    /// Passive abilities are always active with no activation required.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Passive abilities provide constant benefits without any player
    /// interaction. They have no resource cost, no cooldown, and their
    /// effects are applied automatically when the ability is granted.
    /// </para>
    /// <para>
    /// Characteristics:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>No activation required</description></item>
    ///   <item><description>No resource cost</description></item>
    ///   <item><description>Always provides benefit</description></item>
    ///   <item><description>Displayed in abilities list, not action bar</description></item>
    /// </list>
    /// <para>
    /// Each archetype receives exactly one passive starting ability:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Warrior: Iron Will</description></item>
    ///   <item><description>Skirmisher: Opportunist</description></item>
    ///   <item><description>Mystic: Aether Sense</description></item>
    ///   <item><description>Adept: Resourceful</description></item>
    /// </list>
    /// </remarks>
    Passive = 1,

    /// <summary>
    /// Stance abilities toggle between active and inactive states.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Stance abilities are toggled on or off by the player. While active,
    /// they modify the character's behavior, stats, or combat approach.
    /// Only one stance can be active at a time (typically). Stances have
    /// no resource cost to activate.
    /// </para>
    /// <para>
    /// Characteristics:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Toggled on/off by player</description></item>
    ///   <item><description>No resource cost to activate</description></item>
    ///   <item><description>Modifies character behavior while active</description></item>
    ///   <item><description>Only one stance active at a time (typically)</description></item>
    /// </list>
    /// <para>
    /// Among starting abilities, only the Warrior receives a stance:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Warrior: Defensive Stance — Reduces damage taken but slows movement</description></item>
    /// </list>
    /// </remarks>
    Stance = 2
}
