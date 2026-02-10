// ═══════════════════════════════════════════════════════════════════════════════
// SpecializationId.cs
// Enum defining the unique identifier for each of the 18 specializations in
// Aethelgard. Specializations are grouped by parent archetype: Warrior (6),
// Skirmisher (4), Mystic (2), and Adept (6). Each specialization is classified
// as either Coherent or Heretical via its path type.
// Version: 0.17.4a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Unique identifier for each specialization in the game.
/// </summary>
/// <remarks>
/// <para>
/// Each specialization belongs to a single parent archetype and provides a
/// distinct tactical identity through unique ability trees, special resources,
/// and path type classification. Specialization is a permanent choice that
/// cannot be changed after selection. The first specialization is free at
/// character creation (Step 5); additional specializations cost 3 Progression
/// Points.
/// </para>
/// <para>
/// Specializations are grouped by parent archetype:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <b>Warrior</b>: 6 specializations (Berserkr, Iron-Bane, Skjaldmaer,
///       Skar-Horde, Atgeir-Wielder, Gorge-Maw) — values 0-5
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>Skirmisher</b>: 4 specializations (Veiðimaðr, Myrk-gengr,
///       Strandhögg, Hlekkr-master) — values 6-9
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>Mystic</b>: 2 specializations (Seiðkona, Echo-Caller) — values 10-11
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>Adept</b>: 6 specializations (Bone-Setter, Jötun-Reader, Skald,
///       Scrap-Tinker, Einbúi, Rúnasmiðr) — values 12-17
///     </description>
///   </item>
/// </list>
/// <para>
/// Of 18 total specializations, 5 are Heretical (risk Corruption) and 13 are
/// Coherent (no Corruption risk). Path type is determined per specialization
/// via the <c>GetPathType()</c> extension method.
/// </para>
/// <para>
/// Enum values are explicitly assigned (0-17) to ensure stable serialization
/// and database storage. The order groups specializations by parent archetype,
/// with Warrior specializations first (0-5), then Skirmisher (6-9), Mystic
/// (10-11), and Adept (12-17).
/// </para>
/// </remarks>
/// <seealso cref="SpecializationPathType"/>
/// <seealso cref="Archetype"/>
/// <seealso cref="RuneAndRust.Domain.Extensions.SpecializationIdExtensions"/>
/// <seealso cref="Entities.ArchetypeDefinition"/>
public enum SpecializationId
{
    // ═══════════════════════════════════════════════════════════════════════════
    // WARRIOR SPECIALIZATIONS (6)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Fury-powered damage dealer. Heretical path — rage risks Corruption.
    /// </summary>
    /// <remarks>
    /// <para>Parent: Warrior</para>
    /// <para>Path: Heretical</para>
    /// <para>Special Resource: Rage (0-100)</para>
    /// <para>
    /// The Berserkr channels primal rage into devastating attacks. Rage
    /// accumulates when dealing or taking damage, and is spent to activate
    /// powerful Berserkr abilities. The Heretical nature of uncontrolled rage
    /// means some abilities may trigger Corruption gain.
    /// </para>
    /// </remarks>
    Berserkr = 0,

    /// <summary>
    /// Monster hunter specializing in fighting Blighted creatures.
    /// </summary>
    /// <remarks>
    /// <para>Parent: Warrior</para>
    /// <para>Path: Coherent</para>
    /// <para>Special Resource: Righteous Fervor (0-50)</para>
    /// <para>
    /// The Iron-Bane is a righteous monster hunter who gains Fervor from
    /// slaying Blighted enemies. Their Coherent path reflects disciplined,
    /// purposeful combat against the corruption of the Runic Blight.
    /// </para>
    /// </remarks>
    IronBane = 1,

    /// <summary>
    /// Living shield, protector of allies.
    /// </summary>
    /// <remarks>
    /// <para>Parent: Warrior</para>
    /// <para>Path: Coherent</para>
    /// <para>Special Resource: Block Charges (0-3)</para>
    /// <para>
    /// The Skjaldmaer stands between allies and danger, absorbing blows
    /// with shield and body. Block Charges fuel powerful defensive abilities
    /// and restore on rest. Their Coherent path reflects protective discipline.
    /// </para>
    /// </remarks>
    Skjaldmaer = 2,

    /// <summary>
    /// Leader commanding allies in battle.
    /// </summary>
    /// <remarks>
    /// <para>Parent: Warrior</para>
    /// <para>Path: Coherent</para>
    /// <para>
    /// The Skar-Horde commands allies through leadership and tactical unity.
    /// Their Coherent path reflects structured command and cooperative warfare
    /// rather than chaotic individual power.
    /// </para>
    /// </remarks>
    SkarHorde = 3,

    /// <summary>
    /// Master of reach weapons and battlefield control.
    /// </summary>
    /// <remarks>
    /// <para>Parent: Warrior</para>
    /// <para>Path: Coherent</para>
    /// <para>
    /// The Atgeir-Wielder masters the polearm to control space on the
    /// battlefield, keeping enemies at bay with disciplined reach combat.
    /// Their Coherent path reflects martial training and weapon mastery.
    /// </para>
    /// </remarks>
    AtgeirWielder = 4,

    /// <summary>
    /// Consumes enemies to fuel power. Heretical path — consumption risks Corruption.
    /// </summary>
    /// <remarks>
    /// <para>Parent: Warrior</para>
    /// <para>Path: Heretical</para>
    /// <para>
    /// The Gorge-Maw devours fallen enemies to fuel terrifying abilities.
    /// This consumption of corrupted flesh and essence risks Corruption gain,
    /// making it a Heretical path that trades safety for raw power.
    /// </para>
    /// </remarks>
    GorgeMaw = 5,

    // ═══════════════════════════════════════════════════════════════════════════
    // SKIRMISHER SPECIALIZATIONS (4)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Traditional hunter and tracker.
    /// </summary>
    /// <remarks>
    /// <para>Parent: Skirmisher</para>
    /// <para>Path: Coherent</para>
    /// <para>
    /// The Veiðimaðr is a traditional hunter who tracks and stalks prey
    /// through the wilds of Aethelgard. Their Coherent path reflects
    /// natural hunting skills honed through experience and patience.
    /// </para>
    /// </remarks>
    Veidimadr = 6,

    /// <summary>
    /// Shadow manipulator. Heretical path — shadow abilities risk Corruption.
    /// </summary>
    /// <remarks>
    /// <para>Parent: Skirmisher</para>
    /// <para>Path: Heretical</para>
    /// <para>
    /// The Myrk-gengr manipulates shadows and darkness to strike unseen.
    /// Shadow manipulation taps into corrupted Aether, making this a
    /// Heretical path where stealth abilities may trigger Corruption gain.
    /// </para>
    /// </remarks>
    MyrkGengr = 7,

    /// <summary>
    /// Tactical raider specializing in hit-and-run.
    /// </summary>
    /// <remarks>
    /// <para>Parent: Skirmisher</para>
    /// <para>Path: Coherent</para>
    /// <para>
    /// The Strandhögg employs naval raider tactics adapted for land combat,
    /// specializing in lightning strikes and quick withdrawals. Their Coherent
    /// path reflects tactical cunning rather than supernatural power.
    /// </para>
    /// </remarks>
    Strandhogg = 8,

    /// <summary>
    /// Master of chains and restraint.
    /// </summary>
    /// <remarks>
    /// <para>Parent: Skirmisher</para>
    /// <para>Path: Coherent</para>
    /// <para>
    /// The Hlekkr-master wields chains and bindings to control and restrain
    /// enemies on the battlefield. Their Coherent path reflects physical
    /// skill with specialized weapons rather than arcane manipulation.
    /// </para>
    /// </remarks>
    HlekkrMaster = 9,

    // ═══════════════════════════════════════════════════════════════════════════
    // MYSTIC SPECIALIZATIONS (2)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Traditional Aether manipulation. Heretical path — spellcasting risks Corruption.
    /// </summary>
    /// <remarks>
    /// <para>Parent: Mystic</para>
    /// <para>Path: Heretical</para>
    /// <para>Special Resource: Aether Resonance (0-10)</para>
    /// <para>
    /// The Seiðkona practices traditional Aether manipulation, channeling
    /// raw magical energy. Aether Resonance accumulates with spellcasting
    /// and amplifies subsequent spells. All Aether manipulation carries
    /// inherent Corruption risk, making this a Heretical path.
    /// </para>
    /// </remarks>
    Seidkona = 10,

    /// <summary>
    /// Communes with the dead. Heretical path — communing risks Corruption.
    /// </summary>
    /// <remarks>
    /// <para>Parent: Mystic</para>
    /// <para>Path: Heretical</para>
    /// <para>Special Resource: Echoes (0-5)</para>
    /// <para>
    /// The Echo-Caller communes with spirits of the dead, gaining Echoes
    /// from fallen enemies. Bridging the gap between living and dead taps
    /// into deeply corrupted Aether streams, making this a Heretical path
    /// with significant Corruption risk.
    /// </para>
    /// </remarks>
    EchoCaller = 11,

    // ═══════════════════════════════════════════════════════════════════════════
    // ADEPT SPECIALIZATIONS (5)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Healer and traditional medicine practitioner.
    /// </summary>
    /// <remarks>
    /// <para>Parent: Adept</para>
    /// <para>Path: Coherent</para>
    /// <para>
    /// The Bone-Setter practices traditional medicine and healing arts,
    /// mending wounds and treating ailments through mundane knowledge.
    /// Their Coherent path reflects medical skill rather than magical healing.
    /// </para>
    /// </remarks>
    BoneSetter = 12,

    /// <summary>
    /// Scholar of lore and ancient knowledge.
    /// </summary>
    /// <remarks>
    /// <para>Parent: Adept</para>
    /// <para>Path: Coherent</para>
    /// <para>
    /// The Jötun-Reader studies ancient texts, runes, and lost knowledge.
    /// Their scholarly pursuit is Coherent — reading and understanding
    /// lore does not require interfacing with corrupted Aether.
    /// </para>
    /// </remarks>
    JotunReader = 13,

    /// <summary>
    /// Performer inspiring allies through song and story.
    /// </summary>
    /// <remarks>
    /// <para>Parent: Adept</para>
    /// <para>Path: Coherent</para>
    /// <para>
    /// The Skald uses performance, song, and storytelling to inspire allies
    /// and demoralize enemies. Their Coherent path reflects artistic talent
    /// and oral tradition rather than supernatural influence.
    /// </para>
    /// </remarks>
    Skald = 14,

    /// <summary>
    /// Crafter of gadgets and practical inventions.
    /// </summary>
    /// <remarks>
    /// <para>Parent: Adept</para>
    /// <para>Path: Coherent</para>
    /// <para>
    /// The Scrap-Tinker builds gadgets, traps, and practical inventions
    /// from salvaged materials. Their Coherent path reflects engineering
    /// ingenuity and craftsmanship rather than arcane construction.
    /// </para>
    /// </remarks>
    ScrapTinker = 15,

    /// <summary>
    /// Self-reliant survivalist.
    /// </summary>
    /// <remarks>
    /// <para>Parent: Adept</para>
    /// <para>Path: Coherent</para>
    /// <para>
    /// The Einbúi is a solitary survivalist who thrives in the harsh
    /// wilderness of Aethelgard through self-reliance and resourcefulness.
    /// Their Coherent path reflects practical survival skills and
    /// independence rather than supernatural endurance.
    /// </para>
    /// </remarks>
    Einbui = 16,

    /// <summary>
    /// Crafting-focused inscription specialist.
    /// </summary>
    /// <remarks>
    /// <para>Parent: Adept</para>
    /// <para>Path: Coherent</para>
    /// <para>Special Resource: Rune Charges (0-5)</para>
    /// <para>
    /// The Rúnasmiðr inscribes ancient runes onto equipment and surfaces,
    /// providing enhancement bonuses, protective wards, and identification
    /// of Jötun technology. Their Coherent path reflects scholarly craftsmanship
    /// and disciplined inscription techniques.
    /// </para>
    /// </remarks>
    Runasmidr = 17
}
