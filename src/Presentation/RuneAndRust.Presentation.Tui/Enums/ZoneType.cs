// ═══════════════════════════════════════════════════════════════════════════════
// ZoneType.cs
// Enum for categorizing AoE zone effect types for UI rendering.
// Version: 0.13.0b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.Enums;

/// <summary>
/// Zone effect types for UI categorization and rendering.
/// </summary>
/// <remarks>
/// <para>
/// This enum is used by the presentation layer to determine visual styling
/// for AoE zone overlays on the combat grid. Each zone type has distinct
/// symbols and colors for easy identification.
/// </para>
/// <para>
/// Priority order (highest to lowest):
/// </para>
/// <list type="bullet">
///   <item><description><see cref="Damage"/> - Highest priority (immediate threat)</description></item>
///   <item><description><see cref="Control"/> - High priority (prevents actions)</description></item>
///   <item><description><see cref="Debuff"/> - Medium priority (negative effects)</description></item>
///   <item><description><see cref="Buff"/> - Low priority (beneficial effects)</description></item>
///   <item><description><see cref="Preview"/> - Lowest priority (targeting only)</description></item>
/// </list>
/// </remarks>
public enum ZoneType
{
    /// <summary>
    /// Zones that deal damage per turn.
    /// </summary>
    /// <remarks>
    /// Examples: Fire zones, poison clouds, lightning fields.
    /// Displayed with damage type-specific symbols (F, P, L, etc.).
    /// </remarks>
    Damage,

    /// <summary>
    /// Zones that apply control effects.
    /// </summary>
    /// <remarks>
    /// Examples: Stun zones, root zones, freeze zones.
    /// Displayed with '!' symbol to indicate danger.
    /// </remarks>
    Control,

    /// <summary>
    /// Zones that apply debuffs.
    /// </summary>
    /// <remarks>
    /// Examples: Slow zones, weaken zones, curse zones.
    /// Displayed with '-' symbol to indicate negative effect.
    /// </remarks>
    Debuff,

    /// <summary>
    /// Zones that apply buffs or healing.
    /// </summary>
    /// <remarks>
    /// Examples: Healing circles, buff auras, sanctuary zones.
    /// Displayed with '+' symbol to indicate positive effect.
    /// </remarks>
    Buff,

    /// <summary>
    /// Targeting preview (not a real zone).
    /// </summary>
    /// <remarks>
    /// Used during ability targeting to show affected cells.
    /// Displayed with '.' symbol at reduced opacity.
    /// </remarks>
    Preview
}
