// ═══════════════════════════════════════════════════════════════════════════════
// CorruptionSource.cs
// Categorizes the sources of Runic Blight Corruption accumulation in the
// Trauma Economy. Each source type enables contextual corruption tracking,
// history recording for analytics, and descriptive logging for debugging.
// Specific corruption values per source are defined in the
// corruption-sources.json configuration file (v0.18.1e).
// Version: 0.18.1b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Categorizes the sources of Runic Blight Corruption accumulation.
/// </summary>
/// <remarks>
/// <para>
/// Corruption sources are used to:
/// <list type="bullet">
///   <item>
///     <description>
///       Track corruption origin for history records and analytics.
///     </description>
///   </item>
///   <item>
///     <description>
///       Apply contextual corruption modifiers (e.g., lineage resistance to specific sources).
///     </description>
///   </item>
///   <item>
///     <description>
///       Provide descriptive logging for debugging and playtesting.
///     </description>
///   </item>
///   <item>
///     <description>
///       Categorize corruption events for UI display and combat log.
///     </description>
///   </item>
/// </list>
/// </para>
/// <para>
/// Unlike Psychic Stress sources, Corruption sources represent near-permanent
/// contamination. Recovery from Corruption is extremely rare and typically
/// requires special rituals or quests (v0.22.x). This makes source tracking
/// important for narrative context — players should understand <em>why</em>
/// their character is permanently scarred.
/// </para>
/// <para>
/// Specific corruption values and ranges per source are defined in the
/// <c>corruption-sources.json</c> configuration file (v0.18.1e).
/// </para>
/// <para>
/// Enum values are explicitly assigned (0-7) for stable serialization
/// and database persistence. These integer values must not be changed
/// once persisted.
/// </para>
/// </remarks>
/// <seealso cref="RuneAndRust.Domain.ValueObjects.CorruptionState"/>
/// <seealso cref="RuneAndRust.Domain.Enums.CorruptionStage"/>
public enum CorruptionSource
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CORRUPTION SOURCE CATEGORIES (ordered 0-7)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Corruption from standard mystic spellcasting.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Includes: casting standard mystic spells, powerful spell usage, and
    /// overcasting beyond safe limits. Mystic magic channels runic energy
    /// through the caster's body, leaving trace contamination with each use.
    /// </para>
    /// <para>
    /// Corruption ranges:
    /// <list type="bullet">
    ///   <item><description>Standard Spell: 0-2 corruption</description></item>
    ///   <item><description>Powerful Spell: 3-5 corruption</description></item>
    ///   <item><description>Overcasting: 10-15 corruption</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    MysticMagic = 0,

    /// <summary>
    /// Corruption from heretical or forbidden abilities.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Includes: Berserker rage abilities, Blot-Priest sacrificial casting,
    /// Blot-Priest life siphon, Seidkona forbidden divination, and other
    /// abilities that violate the Combine's orthodoxy.
    /// </para>
    /// <para>
    /// Heretical abilities typically generate more corruption than standard
    /// mystic magic, reflecting their transgressive nature. The Blot-Priest
    /// class has unique corruption mechanics tied to HP sacrifice.
    /// </para>
    /// <para>
    /// Corruption ranges:
    /// <list type="bullet">
    ///   <item><description>Berserker Rage Abilities: 2-5 corruption</description></item>
    ///   <item><description>Sacrificial Casting: 1 corruption per HP spent</description></item>
    ///   <item><description>Life Siphon: 1 fixed corruption</description></item>
    ///   <item><description>Forbidden Divination: 3-8 corruption</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    HereticalAbility = 1,

    /// <summary>
    /// Corruption from glitched artifact interaction.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Includes: using glitched artifacts, attuning to corrupted items,
    /// and prolonged contact with Blight-infused equipment. Artifacts
    /// that have been exposed to Runic Blight can transfer contamination
    /// to their wielders.
    /// </para>
    /// <para>
    /// Corruption range: 1-5 corruption per use.
    /// </para>
    /// </remarks>
    Artifact = 2,

    /// <summary>
    /// Corruption from environmental Blight zone exposure.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Includes: entering and remaining in Blight zones, exposure to
    /// Reality Bleed areas, and proximity to corrupted runic infrastructure.
    /// Environmental corruption accumulates per exposure event rather than
    /// continuously, triggered by exploration actions within affected areas.
    /// </para>
    /// <para>
    /// Corruption range: 1-3 corruption per exposure event.
    /// </para>
    /// </remarks>
    Environmental = 3,

    /// <summary>
    /// Corruption from consuming corrupted substances.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Includes: ingesting corrupted food or water, using Blight-tainted
    /// potions, and consuming alchemical substances derived from corrupted
    /// materials. Characters may knowingly use corrupted consumables for
    /// their enhanced effects despite the corruption cost.
    /// </para>
    /// <para>
    /// Corruption range: 2-5 corruption per consumable.
    /// </para>
    /// </remarks>
    Consumable = 4,

    /// <summary>
    /// Corruption from forbidden ritual participation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Includes: participating in forbidden runic rituals, blood
    /// sacrifices, and other ceremonial activities that channel corrupted
    /// energy. Rituals typically involve multiple participants and may
    /// distribute corruption unevenly based on role.
    /// </para>
    /// <para>
    /// Corruption values for rituals are defined per-ritual in the
    /// configuration rather than as a fixed range.
    /// </para>
    /// </remarks>
    Ritual = 5,

    /// <summary>
    /// Corruption from direct Forlorn contact.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Includes: physical contact with Forlorn entities, psychic
    /// contamination from Forlorn proximity, and corruption transferred
    /// during Forlorn attacks. Forlorn are characters who have been
    /// fully consumed by the Blight (Terminal Error survivors who failed).
    /// </para>
    /// <para>
    /// Forlorn contact is among the most dangerous corruption sources,
    /// with higher base corruption values than most other categories.
    /// </para>
    /// <para>
    /// Corruption range: 5-10 corruption per contact event.
    /// </para>
    /// </remarks>
    ForlornContact = 6,

    /// <summary>
    /// Corruption received via Blot-Priest Blight Transfer ability.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Blot-Priest specialization can transfer corruption between
    /// characters using the Blight Transfer ability. This source tracks
    /// corruption received by the <em>target</em> of a transfer.
    /// </para>
    /// <para>
    /// Blight Transfer is a unique mechanic — it does not generate new
    /// corruption but moves existing corruption from one character to
    /// another. The Blot-Priest may use this to "absorb" corruption
    /// from allies at personal cost, or to offload their own corruption
    /// onto enemies or willing recipients.
    /// </para>
    /// </remarks>
    BlightTransfer = 7
}
