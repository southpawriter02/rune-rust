// ═══════════════════════════════════════════════════════════════════════════════
// ScriptType.cs
// Defines the ancient script types that can be comprehended via the
// Ancient Tongues passive ability of the Jötun-Reader specialization.
// Version: 0.20.3a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Types of ancient scripts that the Jötun-Reader can learn to comprehend
/// through the Ancient Tongues passive ability.
/// </summary>
/// <remarks>
/// <para>
/// Ancient Tongues unlocks automatic, instantaneous comprehension of all
/// four script types. No translation check is required.
/// </para>
/// <list type="bullet">
///   <item><description><b>Jötun Formal:</b> Official records, laws, decrees — structured and formulaic</description></item>
///   <item><description><b>Jötun Technical:</b> Machinery labels, system instructions — dense technical jargon</description></item>
///   <item><description><b>Dvergr Standard:</b> General correspondence, daily writing — common patterns</description></item>
///   <item><description><b>Dvergr Runic:</b> Inscriptions and wards — complex symbolic notation</description></item>
/// </list>
/// </remarks>
/// <seealso cref="JotunReaderAbilityId.AncientTongues"/>
public enum ScriptType
{
    /// <summary>
    /// Formal Jötun language, used in official records, laws, and decrees.
    /// Structured and formulaic in nature.
    /// </summary>
    JotunFormal = 0,

    /// <summary>
    /// Technical Jötun notation, used in machinery labels, system instructions,
    /// and engineering documentation. Dense with specialized jargon.
    /// </summary>
    JotunTechnical = 1,

    /// <summary>
    /// Standard Dvergr script, used in general correspondence and daily writing.
    /// Features common, recognizable patterns.
    /// </summary>
    DvergrStandard = 2,

    /// <summary>
    /// Dvergr runic script, used in inscriptions and protective wards.
    /// Complex symbolic notation with layered meanings.
    /// </summary>
    DvergrRunic = 3
}
