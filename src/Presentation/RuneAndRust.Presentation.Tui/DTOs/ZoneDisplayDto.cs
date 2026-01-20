// ═══════════════════════════════════════════════════════════════════════════════
// ZoneDisplayDto.cs
// Data transfer object for AoE zone display rendering.
// Version: 0.13.0b
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Presentation.Tui.Enums;

namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// Data transfer object for zone display rendering.
/// </summary>
/// <remarks>
/// <para>
/// Used by <see cref="UI.AoEZoneOverlay"/> to render AoE zones on the combat grid.
/// Contains all information needed to display zone symbols, colors, and legend entries.
/// </para>
/// <para>
/// Zone priority is determined by <see cref="ZoneType"/> when zones overlap on the same cell.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var fireZone = new ZoneDisplayDto
/// {
///     ZoneId = Guid.NewGuid(),
///     Name = "Fire Zone",
///     ZoneType = ZoneType.Damage,
///     AffectedCells = new List&lt;GridPosition&gt; { new(2, 3), new(2, 4) },
///     RemainingDuration = 3,
///     IsPermanent = false,
///     IsPreview = false,
///     DamageType = "fire",
///     DamagePerTurn = 5
/// };
/// </code>
/// </example>
public record ZoneDisplayDto
{
    /// <summary>
    /// Unique zone identifier.
    /// </summary>
    /// <remarks>
    /// Used for tracking and updating zone state.
    /// For preview zones, this is <see cref="Guid.Empty"/>.
    /// </remarks>
    public required Guid ZoneId { get; init; }

    /// <summary>
    /// Zone display name.
    /// </summary>
    /// <remarks>
    /// Shown in the zone legend. Examples: "Fire Zone", "Poison Cloud", "Healing Circle".
    /// </remarks>
    public required string Name { get; init; }

    /// <summary>
    /// Zone effect type for styling.
    /// </summary>
    /// <remarks>
    /// Determines the base symbol, color, and priority of the zone.
    /// See <see cref="ZoneType"/> for priority ordering.
    /// </remarks>
    public required ZoneType ZoneType { get; init; }

    /// <summary>
    /// Cells affected by this zone.
    /// </summary>
    /// <remarks>
    /// Grid positions that should display the zone overlay.
    /// Multiple zones can affect the same cell; priority determines display.
    /// </remarks>
    public required IReadOnlyList<GridPosition> AffectedCells { get; init; }

    /// <summary>
    /// Remaining duration in turns (null if permanent).
    /// </summary>
    /// <remarks>
    /// Counts down each turn. When reaches 0, zone expires.
    /// Null indicates a permanent zone that never expires.
    /// </remarks>
    public int? RemainingDuration { get; init; }

    /// <summary>
    /// Whether this zone is permanent.
    /// </summary>
    /// <remarks>
    /// Permanent zones have no duration and persist until explicitly removed.
    /// </remarks>
    public bool IsPermanent { get; init; }

    /// <summary>
    /// Whether this is a targeting preview.
    /// </summary>
    /// <remarks>
    /// Preview zones are temporary and have lowest priority.
    /// They are cleared when targeting is confirmed or cancelled.
    /// </remarks>
    public bool IsPreview { get; init; }

    /// <summary>
    /// Damage type for damage zones.
    /// </summary>
    /// <remarks>
    /// Examples: "fire", "ice", "poison", "lightning".
    /// Used to determine the zone symbol (F, I, P, L, etc.).
    /// Null for non-damage zones.
    /// </remarks>
    public string? DamageType { get; init; }

    /// <summary>
    /// Damage per turn for damage zones.
    /// </summary>
    /// <remarks>
    /// The amount of damage dealt to entities in the zone each turn.
    /// Null for non-damage zones.
    /// </remarks>
    public int? DamagePerTurn { get; init; }

    /// <summary>
    /// Status effect applied by this zone.
    /// </summary>
    /// <remarks>
    /// Examples: "Slowed", "Stunned", "Weakened".
    /// Used for control and debuff zones.
    /// Null for pure damage zones.
    /// </remarks>
    public string? StatusEffect { get; init; }
}
