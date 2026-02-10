// ═══════════════════════════════════════════════════════════════════════════════
// LightSaveResult.cs
// Enumerates possible outcomes when a magical light source saves against
// the Eclipse zone's extinguishing effect.
// Version: 0.20.4c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the possible outcomes when a magical light source attempts to
/// resist the <see cref="MyrkgengrAbilityId.Eclipse"/> zone.
/// </summary>
/// <remarks>
/// <para>
/// Non-magical light sources are automatically extinguished by Eclipse.
/// Magical light sources receive a DC 16 save to resist:
/// </para>
/// <list type="bullet">
///   <item><description><b>Extinguished:</b> Save failed — light is fully extinguished</description></item>
///   <item><description><b>Resisted:</b> Save succeeded — light remains at full strength</description></item>
///   <item><description><b>Dimmed:</b> Save partially succeeded — light reduced but not extinguished</description></item>
/// </list>
/// </remarks>
/// <seealso cref="MyrkgengrAbilityId"/>
public enum LightSaveResult
{
    /// <summary>
    /// The light source failed its save and was fully extinguished.
    /// </summary>
    Extinguished = 1,

    /// <summary>
    /// The light source succeeded its save and remains at full strength.
    /// </summary>
    Resisted = 2,

    /// <summary>
    /// The light source partially resisted — reduced in intensity but not extinguished.
    /// Typically occurs on a near-miss save (within 2 of the DC).
    /// </summary>
    Dimmed = 3
}
