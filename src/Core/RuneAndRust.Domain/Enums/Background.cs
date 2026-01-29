// ═══════════════════════════════════════════════════════════════════════════════
// Background.cs
// Enum defining the pre-Silence profession options for character creation.
// Version: 0.17.1a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the pre-Silence profession of a character.
/// </summary>
/// <remarks>
/// <para>
/// Background represents what a character did before the Great Silence. Unlike
/// <see cref="Lineage"/> (inherited bloodline traits) or Archetype (combat role),
/// Background reflects learned skills and accumulated possessions from the
/// character's previous profession.
/// </para>
/// <para>
/// Background is chosen at Step 2 of character creation (after Lineage) and
/// determines starting skill bonuses, starting equipment, social standing,
/// and narrative hooks for quest/dialogue integration.
/// </para>
/// <para>
/// The six backgrounds each offer distinct starting advantages:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <see cref="VillageSmith"/>: Craft expertise and physical tools (Craft +2, Might +1)
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="TravelingHealer"/>: Medical knowledge and healing supplies (Medicine +2, Herbalism +1)
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="RuinDelver"/>: Exploration skills and dungeoneering gear (Exploration +2, Traps +1)
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="ClanGuard"/>: Combat training and defensive equipment (Combat +2, Vigilance +1)
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="WanderingSkald"/>: Performance ability and lore items (Performance +2, Lore +1)
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="OutcastScavenger"/>: Survival skills and wilderness gear (Survival +2, Stealth +1)
///     </description>
///   </item>
/// </list>
/// <para>
/// Background values are explicitly assigned (0-5) to ensure stable serialization
/// and database storage. New backgrounds should be added at the end if needed.
/// </para>
/// </remarks>
/// <seealso cref="Entities.BackgroundDefinition"/>
public enum Background
{
    /// <summary>
    /// Village Smith - Blacksmith and metalworker who shaped tools of war and peace.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Village Smiths are respected craftspeople essential to any settlement.
    /// Before the Great Silence, they worked the forge, shaping metal into
    /// tools, weapons, and armor for their communities. Their expertise in
    /// metallurgy and physical labor grants them a strong foundation in
    /// craftsmanship and raw strength.
    /// </para>
    /// <para>
    /// Skill Grants: Craft +2 (primary), Might +1 (secondary)
    /// </para>
    /// <para>
    /// Starting Equipment: Smith's Hammer (MainHand), Leather Apron (Chest)
    /// </para>
    /// <para>
    /// Social Standing: Respected craftsperson, essential to any settlement
    /// </para>
    /// <para>
    /// Narrative Hooks: Recognize craftsmanship in ruins, repair broken equipment
    /// more easily, clan smiths may offer discounts or quests
    /// </para>
    /// </remarks>
    VillageSmith = 0,

    /// <summary>
    /// Traveling Healer - Itinerant medicine practitioner who walked between settlements.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Traveling Healers are welcomed but sometimes viewed with superstition.
    /// Before the Great Silence, they wandered between settlements bringing
    /// medicine and hope. Their knowledge of herbs, remedies, and diseases
    /// grants them unique insight into the healing arts and natural world.
    /// </para>
    /// <para>
    /// Skill Grants: Medicine +2 (primary), Herbalism +1 (secondary)
    /// </para>
    /// <para>
    /// Starting Equipment: Healer's Kit, Bandages ×5
    /// </para>
    /// <para>
    /// Social Standing: Welcomed but sometimes distrusted
    /// </para>
    /// <para>
    /// Narrative Hooks: Recognize diseases and their cures, Bone-Setters may
    /// share knowledge, some view you with superstitious fear
    /// </para>
    /// </remarks>
    TravelingHealer = 1,

    /// <summary>
    /// Ruin Delver - Scavenger and explorer who sought treasures in the old places.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Ruin Delvers are useful but often looked down upon as grave robbers.
    /// Before the Great Silence, they ventured into ancient ruins and
    /// forgotten places, seeking treasures and knowledge. Their familiarity
    /// with traps, hidden passages, and dungeoneering techniques makes them
    /// invaluable explorers.
    /// </para>
    /// <para>
    /// Skill Grants: Exploration +2 (primary), Traps +1 (secondary)
    /// </para>
    /// <para>
    /// Starting Equipment: Lantern, Rope, Lockpicks
    /// </para>
    /// <para>
    /// Social Standing: Useful but looked down upon
    /// </para>
    /// <para>
    /// Narrative Hooks: Know ruin layouts and hazards, Scrap-Tinkers respect
    /// your finds, may have enemies among rival delvers
    /// </para>
    /// </remarks>
    RuinDelver = 2,

    /// <summary>
    /// Clan Guard - Warrior and protector who defended their people from threats.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Clan Guards are honored defenders trusted by clan leadership. Before
    /// the Great Silence, they stood on the walls with shield and spear,
    /// protecting their people from external threats. Their military training
    /// grants combat expertise and situational awareness.
    /// </para>
    /// <para>
    /// Skill Grants: Combat +2 (primary), Vigilance +1 (secondary)
    /// </para>
    /// <para>
    /// Starting Equipment: Shield (OffHand), Spear (MainHand)
    /// </para>
    /// <para>
    /// Social Standing: Honored defender, trusted by clan leadership
    /// </para>
    /// <para>
    /// Narrative Hooks: Recognize military formations and tactics, other guards
    /// trust you more quickly, may have oaths to uphold or avenge
    /// </para>
    /// </remarks>
    ClanGuard = 3,

    /// <summary>
    /// Wandering Skald - Storyteller and historian who carried memories of what was.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Wandering Skalds are entertainers and keepers of lore, welcome at most
    /// hearths. Before the Great Silence, they traveled between settlements
    /// carrying stories, songs, and the memories of a fading world. Their
    /// knowledge of old legends and performance skills make them valuable
    /// companions and sources of information.
    /// </para>
    /// <para>
    /// Skill Grants: Performance +2 (primary), Lore +1 (secondary)
    /// </para>
    /// <para>
    /// Starting Equipment: Instrument, Journal
    /// </para>
    /// <para>
    /// Social Standing: Entertainer, keeper of lore, welcome at most hearths
    /// </para>
    /// <para>
    /// Narrative Hooks: Know old legends that hold clues, earn room and board
    /// through performance, Jötun-Readers value your historical knowledge
    /// </para>
    /// </remarks>
    WanderingSkald = 4,

    /// <summary>
    /// Outcast Scavenger - Exile and lone survivor who learned to survive in the wastes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Outcast Scavengers are pariahs viewed with suspicion but grudging
    /// respect for their survival skills. Cast out from their clan before
    /// the Great Silence, they learned to survive alone in the hostile
    /// wastes. Their expertise in wilderness survival and stealth makes
    /// them resourceful and self-reliant.
    /// </para>
    /// <para>
    /// Skill Grants: Survival +2 (primary), Stealth +1 (secondary)
    /// </para>
    /// <para>
    /// Starting Equipment: Rations ×3, Cloak (Back)
    /// </para>
    /// <para>
    /// Social Standing: Pariah, viewed with suspicion but grudging respect for survival skills
    /// </para>
    /// <para>
    /// Narrative Hooks: Know the wasteland's secrets and dangers, other outcasts
    /// may offer aid, clans may remember why you were cast out
    /// </para>
    /// </remarks>
    OutcastScavenger = 5
}
