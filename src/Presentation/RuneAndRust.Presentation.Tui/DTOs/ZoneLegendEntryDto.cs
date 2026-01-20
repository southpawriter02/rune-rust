// ═══════════════════════════════════════════════════════════════════════════════
// ZoneLegendEntryDto.cs
// Data transfer object for zone legend entries in the combat UI.
// Version: 0.13.0b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// Data transfer object for zone legend entries.
/// </summary>
/// <remarks>
/// <para>
/// Used to display the "ACTIVE ZONES:" legend below the combat grid.
/// Each entry shows a zone's symbol, name, duration, and effect summary.
/// </para>
/// <para>
/// Format: <c>[F] Fire Zone - 2 turns - 5 fire damage/turn</c>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var entry = new ZoneLegendEntryDto
/// {
///     Symbol = 'F',
///     Name = "Fire Zone",
///     Duration = "2 turns",
///     Effect = "5 fire damage/turn",
///     Color = ConsoleColor.Red
/// };
/// </code>
/// </example>
public record ZoneLegendEntryDto
{
    /// <summary>
    /// Zone display symbol.
    /// </summary>
    /// <remarks>
    /// Single character shown in brackets in the legend.
    /// Examples: 'F' for fire, 'I' for ice, '!' for control.
    /// </remarks>
    public required char Symbol { get; init; }

    /// <summary>
    /// Zone display name.
    /// </summary>
    /// <remarks>
    /// Human-readable name for the zone.
    /// Examples: "Fire Zone", "Poison Cloud", "Healing Circle".
    /// </remarks>
    public required string Name { get; init; }

    /// <summary>
    /// Formatted duration string.
    /// </summary>
    /// <remarks>
    /// Shows remaining turns or "permanent" for permanent zones.
    /// Examples: "3 turns", "1 turn", "permanent", "expiring".
    /// </remarks>
    public required string Duration { get; init; }

    /// <summary>
    /// Effect summary string.
    /// </summary>
    /// <remarks>
    /// Brief description of what the zone does.
    /// Examples: "5 fire damage/turn", "Slowed movement", "Healing aura".
    /// </remarks>
    public required string Effect { get; init; }

    /// <summary>
    /// Zone color for rendering.
    /// </summary>
    /// <remarks>
    /// Used to colorize the symbol in the legend.
    /// Matches the zone's grid overlay color.
    /// </remarks>
    public ConsoleColor Color { get; init; }
}
