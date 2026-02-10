// ------------------------------------------------------------------------------
// <copyright file="SpecializationIdExtensions.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Extension methods for SpecializationId enum providing path type lookups,
// parent archetype mappings, display names with Norse diacritics, Heretical
// checks, and Corruption risk descriptions.
// Part of v0.17.4a Specialization Enum & Path Types implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Extensions;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Extension methods for <see cref="SpecializationId"/>.
/// </summary>
/// <remarks>
/// <para>
/// Provides utility methods for the <see cref="SpecializationId"/> enum,
/// including path type classification, parent archetype mapping, human-readable
/// display names with Old Norse diacritics, Heretical status checks, and
/// Corruption risk descriptions.
/// </para>
/// <para>
/// All methods use switch expressions for O(1) lookup time — no dictionary
/// or configuration file access is required at runtime for these operations.
/// </para>
/// <para>
/// Key classification data:
/// </para>
/// <list type="bullet">
///   <item><description>18 total specializations across 4 archetypes</description></item>
///   <item><description>5 Heretical (Berserkr, GorgeMaw, MyrkGengr, Seidkona, EchoCaller)</description></item>
///   <item><description>13 Coherent (all others)</description></item>
///   <item><description>Warrior: 6, Skirmisher: 4, Mystic: 2, Adept: 6</description></item>
/// </list>
/// </remarks>
/// <seealso cref="SpecializationId"/>
/// <seealso cref="SpecializationPathType"/>
/// <seealso cref="Archetype"/>
/// <seealso cref="SpecializationPathTypeExtensions"/>
public static class SpecializationIdExtensions
{
    // -------------------------------------------------------------------------
    // Path Type Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the path type (Coherent or Heretical) for this specialization.
    /// </summary>
    /// <param name="specializationId">The specialization identifier.</param>
    /// <returns>
    /// The <see cref="SpecializationPathType"/> for this specialization.
    /// Returns <see cref="SpecializationPathType.Heretical"/> for the 5
    /// Heretical specializations, and <see cref="SpecializationPathType.Coherent"/>
    /// for all 12 others.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Heretical specializations (5 total):
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Berserkr (Warrior) — rage risks Corruption</description></item>
    ///   <item><description>GorgeMaw (Warrior) — consumption risks Corruption</description></item>
    ///   <item><description>MyrkGengr (Skirmisher) — shadow manipulation risks Corruption</description></item>
    ///   <item><description>Seidkona (Mystic) — Aether spellcasting risks Corruption</description></item>
    ///   <item><description>EchoCaller (Mystic) — communing with dead risks Corruption</description></item>
    /// </list>
    /// <para>
    /// All other specializations default to Coherent.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var pathType = SpecializationId.Berserkr.GetPathType();
    /// // SpecializationPathType.Heretical
    ///
    /// var pathType2 = SpecializationId.Skjaldmaer.GetPathType();
    /// // SpecializationPathType.Coherent
    /// </code>
    /// </example>
    public static SpecializationPathType GetPathType(this SpecializationId specializationId)
    {
        return specializationId switch
        {
            // Heretical paths (5 total)
            SpecializationId.Berserkr => SpecializationPathType.Heretical,
            SpecializationId.GorgeMaw => SpecializationPathType.Heretical,
            SpecializationId.MyrkGengr => SpecializationPathType.Heretical,
            SpecializationId.Seidkona => SpecializationPathType.Heretical,
            SpecializationId.EchoCaller => SpecializationPathType.Heretical,

            // All others are Coherent (13 total)
            _ => SpecializationPathType.Coherent
        };
    }

    /// <summary>
    /// Gets whether this specialization is Heretical (risks Corruption).
    /// </summary>
    /// <param name="specializationId">The specialization identifier.</param>
    /// <returns>
    /// <c>true</c> if the specialization is classified as Heretical and its
    /// abilities may trigger Corruption gain; <c>false</c> for Coherent
    /// specializations.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Convenience method that delegates to <see cref="GetPathType"/> and
    /// compares against <see cref="SpecializationPathType.Heretical"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// bool heretical = SpecializationId.Seidkona.IsHeretical(); // true
    /// bool coherent = SpecializationId.BoneSetter.IsHeretical(); // false
    /// </code>
    /// </example>
    public static bool IsHeretical(this SpecializationId specializationId)
    {
        return specializationId.GetPathType() == SpecializationPathType.Heretical;
    }

    // -------------------------------------------------------------------------
    // Archetype Mapping Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the parent archetype for this specialization.
    /// </summary>
    /// <param name="specializationId">The specialization identifier.</param>
    /// <returns>The parent <see cref="Archetype"/> for this specialization.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown or undefined <see cref="SpecializationId"/> value
    /// is provided.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Each specialization belongs to exactly one parent archetype:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Warrior (6): Berserkr, IronBane, Skjaldmaer, SkarHorde, AtgeirWielder, GorgeMaw</description></item>
    ///   <item><description>Skirmisher (4): Veidimadr, MyrkGengr, Strandhogg, HlekkrMaster</description></item>
    ///   <item><description>Mystic (2): Seidkona, EchoCaller</description></item>
    ///   <item><description>Adept (6): BoneSetter, JotunReader, Skald, ScrapTinker, Einbui, Runasmidr</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var archetype = SpecializationId.Skjaldmaer.GetParentArchetype();
    /// // Archetype.Warrior
    ///
    /// var archetype2 = SpecializationId.Skald.GetParentArchetype();
    /// // Archetype.Adept
    /// </code>
    /// </example>
    public static Archetype GetParentArchetype(this SpecializationId specializationId)
    {
        return specializationId switch
        {
            // Warrior specializations (6)
            SpecializationId.Berserkr => Archetype.Warrior,
            SpecializationId.IronBane => Archetype.Warrior,
            SpecializationId.Skjaldmaer => Archetype.Warrior,
            SpecializationId.SkarHorde => Archetype.Warrior,
            SpecializationId.AtgeirWielder => Archetype.Warrior,
            SpecializationId.GorgeMaw => Archetype.Warrior,

            // Skirmisher specializations (4)
            SpecializationId.Veidimadr => Archetype.Skirmisher,
            SpecializationId.MyrkGengr => Archetype.Skirmisher,
            SpecializationId.Strandhogg => Archetype.Skirmisher,
            SpecializationId.HlekkrMaster => Archetype.Skirmisher,

            // Mystic specializations (2)
            SpecializationId.Seidkona => Archetype.Mystic,
            SpecializationId.EchoCaller => Archetype.Mystic,

            // Adept specializations (5)
            SpecializationId.BoneSetter => Archetype.Adept,
            SpecializationId.JotunReader => Archetype.Adept,
            SpecializationId.Skald => Archetype.Adept,
            SpecializationId.ScrapTinker => Archetype.Adept,
            SpecializationId.Einbui => Archetype.Adept,
            SpecializationId.Runasmidr => Archetype.Adept,

            _ => throw new ArgumentOutOfRangeException(
                nameof(specializationId),
                specializationId,
                $"Unknown specialization: {specializationId}")
        };
    }

    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets a human-readable display name for this specialization.
    /// </summary>
    /// <param name="specializationId">The specialization identifier.</param>
    /// <returns>
    /// The display name with proper formatting, including Old Norse diacritics
    /// (ð, ö, ú) where appropriate for thematic authenticity.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Display names preserve Old Norse diacritics for authenticity:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Veiðimaðr — contains ð (eth)</description></item>
    ///   <item><description>Strandhögg — contains ö (o with umlaut)</description></item>
    ///   <item><description>Seiðkona — contains ð (eth)</description></item>
    ///   <item><description>Jötun-Reader — contains ö (o with umlaut)</description></item>
    ///   <item><description>Einbúi — contains ú (u with acute)</description></item>
    /// </list>
    /// <para>
    /// Names use hyphenated format for compound names (e.g., "Iron-Bane",
    /// "Skar-Horde", "Atgeir-Wielder") matching the design specification's
    /// display conventions.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var name = SpecializationId.Skjaldmaer.GetDisplayName();
    /// // "Skjaldmaer"
    ///
    /// var name2 = SpecializationId.Veidimadr.GetDisplayName();
    /// // "Veiðimaðr"
    /// </code>
    /// </example>
    public static string GetDisplayName(this SpecializationId specializationId)
    {
        return specializationId switch
        {
            // Warrior specializations
            SpecializationId.Berserkr => "Berserkr",
            SpecializationId.IronBane => "Iron-Bane",
            SpecializationId.Skjaldmaer => "Skjaldmaer",
            SpecializationId.SkarHorde => "Skar-Horde",
            SpecializationId.AtgeirWielder => "Atgeir-Wielder",
            SpecializationId.GorgeMaw => "Gorge-Maw",

            // Skirmisher specializations
            SpecializationId.Veidimadr => "Veiðimaðr",
            SpecializationId.MyrkGengr => "Myrk-gengr",
            SpecializationId.Strandhogg => "Strandhögg",
            SpecializationId.HlekkrMaster => "Hlekkr-master",

            // Mystic specializations
            SpecializationId.Seidkona => "Seiðkona",
            SpecializationId.EchoCaller => "Echo-Caller",

            // Adept specializations
            SpecializationId.BoneSetter => "Bone-Setter",
            SpecializationId.JotunReader => "Jötun-Reader",
            SpecializationId.Skald => "Skald",
            SpecializationId.ScrapTinker => "Scrap-Tinker",
            SpecializationId.Einbui => "Einbúi",
            SpecializationId.Runasmidr => "Rúnasmiðr",

            _ => specializationId.ToString()
        };
    }

    // -------------------------------------------------------------------------
    // Corruption Risk Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the Corruption risk description for this specialization.
    /// </summary>
    /// <param name="specializationId">The specialization identifier.</param>
    /// <returns>
    /// A description of the Corruption risk for Heretical specializations,
    /// or <c>null</c> for Coherent paths that carry no Corruption risk.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Each Heretical specialization has a unique Corruption risk profile
    /// reflecting the nature of its power source:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Berserkr: Rage-triggered Corruption</description></item>
    ///   <item><description>Gorge-Maw: Consumption-triggered Corruption</description></item>
    ///   <item><description>Myrk-gengr: Shadow manipulation Corruption</description></item>
    ///   <item><description>Seiðkona: Aether spellcasting Corruption</description></item>
    ///   <item><description>Echo-Caller: Death communion Corruption</description></item>
    /// </list>
    /// <para>
    /// Coherent specializations return <c>null</c> as they carry no
    /// Corruption risk.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var risk = SpecializationId.Berserkr.GetCorruptionRisk();
    /// // "Rage abilities may trigger Corruption gain"
    ///
    /// var noRisk = SpecializationId.Skjaldmaer.GetCorruptionRisk();
    /// // null
    /// </code>
    /// </example>
    public static string? GetCorruptionRisk(this SpecializationId specializationId)
    {
        return specializationId switch
        {
            SpecializationId.Berserkr => "Rage abilities may trigger Corruption gain",
            SpecializationId.GorgeMaw => "Consuming enemies risks Corruption",
            SpecializationId.MyrkGengr => "Shadow manipulation risks Corruption",
            SpecializationId.Seidkona => "Aether spellcasting risks Corruption",
            SpecializationId.EchoCaller => "Communing with the dead risks Corruption",
            _ => null
        };
    }
}
