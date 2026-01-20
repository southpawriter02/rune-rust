// ═══════════════════════════════════════════════════════════════════════════════
// AoEZoneOverlay.cs
// UI component for displaying AoE zone overlays on the combat grid.
// Version: 0.13.0b
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Enums;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UI;

/// <summary>
/// UI component for displaying AoE zone overlays on the combat grid.
/// </summary>
/// <remarks>
/// <para>
/// Manages the display of area-of-effect zones including:
/// </para>
/// <list type="bullet">
///   <item><description>Active hazard zones (fire, ice, poison, etc.)</description></item>
///   <item><description>Ability targeting previews</description></item>
///   <item><description>Zone legend with duration and effect information</description></item>
/// </list>
/// <para>
/// Integrates with the CombatGridView to render zone overlays
/// between the terrain and entity layers.
/// </para>
/// <para>
/// Zone overlap is handled via priority ordering:
/// Damage > Control > Debuff > Buff > Preview
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var overlay = new AoEZoneOverlay(renderer, logger);
/// overlay.RenderZones(activeZones);
/// var zoneCells = overlay.GetAllZoneCells();
/// gridView.SetAoEZones(zoneCells);
/// </code>
/// </example>
public class AoEZoneOverlay
{
    // ═══════════════════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly ZoneRenderer _renderer;
    private readonly ILogger<AoEZoneOverlay>? _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // STATE
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Maps grid positions to the zone affecting them.
    /// </summary>
    /// <remarks>
    /// When multiple zones affect the same cell, the highest priority zone is stored.
    /// </remarks>
    private readonly Dictionary<GridPosition, ZoneDisplayDto> _zoneCells = new();

    /// <summary>
    /// List of all active (non-preview) zones.
    /// </summary>
    private readonly List<ZoneDisplayDto> _activeZones = new();

    /// <summary>
    /// Current targeting preview zone, if any.
    /// </summary>
    private ZoneDisplayDto? _previewZone;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of <see cref="AoEZoneOverlay"/>.
    /// </summary>
    /// <param name="renderer">Zone renderer for visual formatting.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public AoEZoneOverlay(
        ZoneRenderer renderer,
        ILogger<AoEZoneOverlay>? logger = null)
    {
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _logger = logger;

        _logger?.LogDebug("AoEZoneOverlay initialized");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ZONE RENDERING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Renders all active zones and builds the cell-to-zone mapping.
    /// </summary>
    /// <param name="zones">Active zone data from hazard/ability services.</param>
    /// <remarks>
    /// <para>
    /// Clears existing zone data and rebuilds the cell-to-zone mapping.
    /// When multiple zones affect the same cell, the zone with higher priority
    /// (determined by <see cref="ZoneType"/>) is displayed.
    /// </para>
    /// <para>
    /// Priority order: Damage (4) > Control (3) > Debuff (2) > Buff (1) > Preview (0)
    /// </para>
    /// </remarks>
    public void RenderZones(IReadOnlyList<ZoneDisplayDto> zones)
    {
        ArgumentNullException.ThrowIfNull(zones);

        // Clear existing zone data
        _activeZones.Clear();
        _zoneCells.Clear();

        // Process each zone and build cell mapping
        foreach (var zone in zones)
        {
            _activeZones.Add(zone);

            foreach (var cell in zone.AffectedCells)
            {
                // If cell already has a zone, use priority to determine which shows
                if (_zoneCells.TryGetValue(cell, out var existingZone))
                {
                    if (GetZonePriority(zone.ZoneType) > GetZonePriority(existingZone.ZoneType))
                    {
                        _zoneCells[cell] = zone;
                        _logger?.LogDebug(
                            "Zone overlap at {Position}: {NewZone} takes priority over {ExistingZone}",
                            cell, zone.Name, existingZone.Name);
                    }
                }
                else
                {
                    _zoneCells[cell] = zone;
                }
            }
        }

        _logger?.LogInformation(
            "Rendered {ZoneCount} zones affecting {CellCount} cells",
            _activeZones.Count, _zoneCells.Count);
    }

    /// <summary>
    /// Highlights specific tiles for ability targeting preview.
    /// </summary>
    /// <param name="positions">Grid positions to highlight.</param>
    /// <param name="zoneType">Type of zone for visual styling.</param>
    /// <param name="isPreview">Whether this is a targeting preview.</param>
    /// <remarks>
    /// Preview zones have lowest priority and don't override active zones.
    /// Call <see cref="ClearPreview"/> to remove the preview.
    /// </remarks>
    public void HighlightTiles(
        IReadOnlyList<GridPosition> positions,
        ZoneType zoneType,
        bool isPreview = false)
    {
        ArgumentNullException.ThrowIfNull(positions);

        if (isPreview)
        {
            // Create preview zone DTO
            _previewZone = new ZoneDisplayDto
            {
                ZoneId = Guid.Empty,
                Name = "Target Area",
                ZoneType = zoneType,
                AffectedCells = positions,
                RemainingDuration = null,
                IsPermanent = false,
                IsPreview = true
            };

            // Add preview cells to mapping (only if cell not already occupied)
            foreach (var cell in positions)
            {
                if (!_zoneCells.ContainsKey(cell))
                {
                    _zoneCells[cell] = _previewZone;
                }
            }
        }

        _logger?.LogDebug(
            "Highlighted {CellCount} tiles for {ZoneType} (preview={IsPreview})",
            positions.Count, zoneType, isPreview);
    }

    /// <summary>
    /// Clears the targeting preview zone.
    /// </summary>
    /// <remarks>
    /// Removes preview cells from the mapping without affecting active zones.
    /// </remarks>
    public void ClearPreview()
    {
        if (_previewZone == null)
        {
            return;
        }

        // Remove preview cells from mapping
        foreach (var cell in _previewZone.AffectedCells)
        {
            if (_zoneCells.TryGetValue(cell, out var zone) && zone.IsPreview)
            {
                _zoneCells.Remove(cell);
            }
        }

        _previewZone = null;
        _logger?.LogDebug("Cleared targeting preview");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // QUERY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the zone affecting a specific cell, if any.
    /// </summary>
    /// <param name="position">Grid position to check.</param>
    /// <returns>Zone display data or null if no zone at position.</returns>
    public ZoneDisplayDto? GetZoneAtCell(GridPosition position)
    {
        return _zoneCells.GetValueOrDefault(position);
    }

    /// <summary>
    /// Gets all cells affected by any zone.
    /// </summary>
    /// <returns>Dictionary mapping positions to zone data.</returns>
    /// <remarks>
    /// Used by the CombatGridView to render zone overlays.
    /// </remarks>
    public IReadOnlyDictionary<GridPosition, ZoneDisplayDto> GetAllZoneCells()
    {
        return _zoneCells;
    }

    /// <summary>
    /// Gets all active (non-preview) zones.
    /// </summary>
    /// <returns>List of active zone display data.</returns>
    public IReadOnlyList<ZoneDisplayDto> GetActiveZones()
    {
        return _activeZones.Where(z => !z.IsPreview).ToList();
    }

    /// <summary>
    /// Checks if a cell is affected by any zone.
    /// </summary>
    /// <param name="position">Grid position to check.</param>
    /// <returns>True if the cell has a zone overlay.</returns>
    public bool HasZoneAtCell(GridPosition position)
    {
        return _zoneCells.ContainsKey(position);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DURATION HANDLING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Updates duration display for all tracked zones.
    /// </summary>
    /// <remarks>
    /// Called at the start of each turn to signal that a refresh is needed.
    /// Durations are read fresh from zone data on each render.
    /// </remarks>
    public void UpdateDurations()
    {
        _logger?.LogDebug("Duration update signaled for {ZoneCount} zones", _activeZones.Count);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // LEGEND RENDERING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Renders the zone legend showing active zone information.
    /// </summary>
    /// <returns>List of formatted lines for the legend.</returns>
    /// <remarks>
    /// <para>
    /// Format: <c>[F] Fire Zone - 2 turns - 5 fire damage/turn</c>
    /// </para>
    /// <para>
    /// Returns "No active zones" if no zones are present.
    /// </para>
    /// </remarks>
    public IReadOnlyList<string> RenderLegend()
    {
        var nonPreviewZones = _activeZones.Where(z => !z.IsPreview).ToList();

        if (nonPreviewZones.Count == 0)
        {
            _logger?.LogDebug("Rendered empty zone legend");
            return new List<string> { "No active zones" };
        }

        var lines = new List<string> { "ACTIVE ZONES:" };

        foreach (var zone in nonPreviewZones)
        {
            var symbol = !string.IsNullOrEmpty(zone.DamageType)
                ? _renderer.GetDamageTypeSymbol(zone.DamageType)
                : _renderer.GetZoneSymbol(zone.ZoneType);

            var duration = zone.IsPermanent
                ? "permanent"
                : _renderer.FormatZoneDuration(zone.RemainingDuration);

            var effect = GetZoneEffectSummary(zone);

            lines.Add($"  [{symbol}] {zone.Name} - {duration} - {effect}");
        }

        _logger?.LogDebug("Rendered zone legend with {LineCount} entries", lines.Count - 1);
        return lines;
    }

    /// <summary>
    /// Gets legend entries as structured DTOs.
    /// </summary>
    /// <returns>List of zone legend entry DTOs.</returns>
    public IReadOnlyList<ZoneLegendEntryDto> GetLegendEntries()
    {
        var entries = new List<ZoneLegendEntryDto>();

        foreach (var zone in _activeZones.Where(z => !z.IsPreview))
        {
            var symbol = !string.IsNullOrEmpty(zone.DamageType)
                ? _renderer.GetDamageTypeSymbol(zone.DamageType)
                : _renderer.GetZoneSymbol(zone.ZoneType);

            var color = !string.IsNullOrEmpty(zone.DamageType)
                ? _renderer.GetDamageTypeColor(zone.DamageType)
                : _renderer.GetZoneColor(zone.ZoneType);

            entries.Add(new ZoneLegendEntryDto
            {
                Symbol = symbol,
                Name = zone.Name,
                Duration = zone.IsPermanent
                    ? "permanent"
                    : _renderer.FormatZoneDuration(zone.RemainingDuration),
                Effect = GetZoneEffectSummary(zone),
                Color = color
            });
        }

        return entries;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HAZARD ICONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Shows hazard icons for a specific zone.
    /// </summary>
    /// <param name="zone">Zone to show hazard icons for.</param>
    /// <returns>Hazard icon string (Unicode or ASCII).</returns>
    public string ShowHazardIcons(ZoneDisplayDto zone)
    {
        ArgumentNullException.ThrowIfNull(zone);
        return _renderer.GetHazardIcon(zone.DamageType);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the display priority for a zone type.
    /// </summary>
    /// <param name="type">Zone type to get priority for.</param>
    /// <returns>Priority value (higher = more important).</returns>
    /// <remarks>
    /// Priority order: Damage (4) > Control (3) > Debuff (2) > Buff (1) > Preview (0)
    /// </remarks>
    private static int GetZonePriority(ZoneType type)
    {
        return type switch
        {
            ZoneType.Damage => 4,    // Damage zones highest priority
            ZoneType.Control => 3,   // Control zones (stun, root)
            ZoneType.Debuff => 2,    // Debuff zones
            ZoneType.Buff => 1,      // Buff zones lowest priority
            ZoneType.Preview => 0,   // Preview always lowest
            _ => 0
        };
    }

    /// <summary>
    /// Gets a summary of the zone's effect for the legend.
    /// </summary>
    /// <param name="zone">Zone to summarize.</param>
    /// <returns>Effect summary string.</returns>
    private static string GetZoneEffectSummary(ZoneDisplayDto zone)
    {
        // Damage zones: show damage per turn
        if (zone.DamagePerTurn.HasValue && zone.DamagePerTurn > 0)
        {
            var dmgType = zone.DamageType ?? "damage";
            return $"{zone.DamagePerTurn} {dmgType}/turn";
        }

        // Status effect zones: show effect name
        if (!string.IsNullOrEmpty(zone.StatusEffect))
        {
            return zone.StatusEffect;
        }

        // Fallback based on zone type
        return zone.ZoneType switch
        {
            ZoneType.Damage => "Damage zone",
            ZoneType.Control => "Control effect",
            ZoneType.Debuff => "Debuff zone",
            ZoneType.Buff => "Buff zone",
            _ => "Zone effect"
        };
    }
}
