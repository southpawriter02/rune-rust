// ═══════════════════════════════════════════════════════════════════════════════
// SpecializationPathType.cs
// Enum classifying specializations by their relationship with reality and the
// Aether. Coherent paths work within stable reality with no Corruption risk,
// while Heretical paths interface with corrupted Aether and may trigger
// Corruption gain from ability use.
// Version: 0.17.4a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Classifies specializations by their relationship with reality and the Aether.
/// </summary>
/// <remarks>
/// <para>
/// Path type determines how a specialization interacts with the Trauma Economy,
/// particularly Corruption. Heretical paths risk Corruption gain when using
/// certain abilities, while Coherent paths operate within stable reality
/// without Corruption consequences.
/// </para>
/// <para>
/// Path type is permanent and determined by the specialization itself, not
/// player choice. Some archetypes (like Mystic) only have Heretical options,
/// meaning all Mystic specializations carry inherent Corruption risk.
/// </para>
/// <para>
/// The two path types are:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <see cref="Coherent"/>: Works within stable reality — no special
///       Corruption risk from abilities. Uses skills, training, and natural
///       talent. 12 of 17 specializations are Coherent.
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="Heretical"/>: Interfaces with corrupted Aether — abilities
///       may trigger Corruption gain. Taps into unstable or corrupted power
///       sources. 5 of 17 specializations are Heretical.
///     </description>
///   </item>
/// </list>
/// <para>
/// Path type values are explicitly assigned (0-1) to ensure stable serialization
/// and database storage. These integer values must not be changed once persisted.
/// </para>
/// <para>
/// During character creation (Step 5 — Specialization Selection), the UI displays
/// a Corruption warning when a Heretical specialization is selected. This warning
/// is generated via the <c>GetCreationWarning()</c> extension method on this enum.
/// </para>
/// </remarks>
/// <seealso cref="SpecializationId"/>
/// <seealso cref="Archetype"/>
/// <seealso cref="RuneAndRust.Domain.Extensions.SpecializationPathTypeExtensions"/>
/// <seealso cref="RuneAndRust.Domain.Extensions.SpecializationIdExtensions"/>
public enum SpecializationPathType
{
    /// <summary>
    /// Works within stable reality. No special Corruption risk from abilities.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Coherent specializations rely on mundane skills, physical training,
    /// tactical discipline, and natural talent. Their abilities do not
    /// interact with corrupted Aether and carry no inherent Corruption risk.
    /// </para>
    /// <para>
    /// Coherent specializations span all four archetypes:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Warrior: Iron-Bane, Skjaldmaer, Skar-Horde, Atgeir-Wielder (4 of 6)</description></item>
    ///   <item><description>Skirmisher: Veiðimaðr, Strandhögg, Hlekkr-master (3 of 4)</description></item>
    ///   <item><description>Mystic: None (0 of 2) — all Mystic specializations are Heretical</description></item>
    ///   <item><description>Adept: Bone-Setter, Jötun-Reader, Skald, Scrap-Tinker, Einbúi (5 of 5)</description></item>
    /// </list>
    /// <para>
    /// Total Coherent specializations: 12 of 17.
    /// </para>
    /// </remarks>
    Coherent = 0,

    /// <summary>
    /// Interfaces with corrupted Aether. Abilities may trigger Corruption gain.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Heretical specializations tap into unstable or corrupted power sources,
    /// including primal rage, shadow manipulation, Aether channeling, and
    /// communion with the dead. Certain abilities have Corruption costs or
    /// risks on fumble, integrating with the Trauma Economy.
    /// </para>
    /// <para>
    /// The UI displays a warning when selecting a Heretical specialization
    /// during character creation, alerting the player to the Corruption risk.
    /// </para>
    /// <para>
    /// Heretical specializations by archetype:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Warrior: Berserkr (rage), Gorge-Maw (consumption) — 2 of 6</description></item>
    ///   <item><description>Skirmisher: Myrk-gengr (shadow) — 1 of 4</description></item>
    ///   <item><description>Mystic: Seiðkona (Aether), Echo-Caller (dead) — 2 of 2</description></item>
    ///   <item><description>Adept: None — 0 of 5</description></item>
    /// </list>
    /// <para>
    /// Total Heretical specializations: 5 of 17.
    /// </para>
    /// </remarks>
    Heretical = 1
}
