// ═══════════════════════════════════════════════════════════════════════════════
// LightLevelType.cs
// Enum representing environmental light conditions that affect Myrk-gengr
// abilities, Shadow Essence generation, and Corruption risk.
// Version: 0.20.4a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the light level at a given position, determining Shadow Essence
/// generation rates, ability effectiveness, and Corruption risk for the
/// Myrk-gengr specialization.
/// </summary>
/// <remarks>
/// <para>
/// Light levels directly impact Myrk-gengr gameplay mechanics:
/// </para>
/// <list type="bullet">
///   <item><description>
///     <b>Darkness (0):</b> Maximum Shadow Essence generation (+5/turn),
///     full ability effectiveness, no Corruption risk.
///   </description></item>
///   <item><description>
///     <b>DimLight (1):</b> Moderate Shadow Essence generation (+3/turn),
///     full ability effectiveness, no Corruption risk.
///     Dark-Adapted removes all penalties at this level.
///   </description></item>
///   <item><description>
///     <b>NormalLight (2):</b> No Shadow Essence generation, reduced ability
///     effectiveness (Cloak of Night grants only +1 Stealth), no Corruption risk
///     from most abilities.
///   </description></item>
///   <item><description>
///     <b>BrightLight (3):</b> No Shadow Essence generation, ability penalties
///     (Cloak of Night grants -1 Stealth), Corruption risk triggered.
///   </description></item>
///   <item><description>
///     <b>Sunlight (4):</b> No Shadow Essence generation, maximum penalties,
///     highest Corruption risk.
///   </description></item>
/// </list>
/// <para>
/// Values are explicitly ordered from darkest (0) to brightest (4) to enable
/// comparison operators (e.g., <c>lightLevel &lt;= LightLevelType.DimLight</c>
/// checks for shadow conditions).
/// </para>
/// </remarks>
/// <seealso cref="MyrkgengrAbilityId"/>
public enum LightLevelType
{
    /// <summary>
    /// Complete absence of light. Maximum Shadow Essence generation.
    /// Shadow Step can target, Cloak of Night is fully effective.
    /// </summary>
    Darkness = 0,

    /// <summary>
    /// Partial illumination (torchlight, moonlight, distant fires).
    /// Shadow Essence generates at moderate rate. Dark-Adapted removes
    /// all standard dim light penalties at this level.
    /// </summary>
    DimLight = 1,

    /// <summary>
    /// Standard indoor/outdoor illumination. No Shadow Essence generation.
    /// Cloak of Night provides reduced benefits (+1 Stealth, no silent movement).
    /// </summary>
    NormalLight = 2,

    /// <summary>
    /// Strong illumination (direct sunlight, magical light, bright torchlight).
    /// No Shadow Essence generation. Cloak of Night incurs -1 Stealth penalty.
    /// Shadow abilities trigger Corruption risk.
    /// </summary>
    BrightLight = 3,

    /// <summary>
    /// Maximum illumination (high noon, magical radiance).
    /// All shadow abilities heavily penalized. Highest Corruption risk.
    /// </summary>
    Sunlight = 4
}
