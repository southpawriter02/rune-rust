namespace RuneAndRust.Presentation.Gui.Models;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Presentation.Gui.Services;

/// <summary>
/// A highlighted cell with its highlight type.
/// </summary>
/// <param name="Position">The grid position of the highlighted cell.</param>
/// <param name="Type">The type of highlight to display.</param>
public record HighlightedCell(GridPosition Position, HighlightType Type);

/// <summary>
/// Result of a targeting action.
/// </summary>
/// <param name="Mode">The interaction mode that was active.</param>
/// <param name="Target">The targeted position.</param>
/// <param name="Success">Whether the action succeeded.</param>
/// <param name="Message">Optional message (usually for failure reason).</param>
public record TargetingResult(
    GridInteractionMode Mode,
    GridPosition Target,
    bool Success,
    string? Message = null);

/// <summary>
/// Information about the hovered cell.
/// </summary>
/// <param name="Position">The grid position of the hovered cell.</param>
/// <param name="CellLabel">The coordinate label (e.g., "A1").</param>
/// <param name="EntityName">The name of the entity at this position (if any).</param>
/// <param name="EntityHealth">The current health of the entity (if any).</param>
/// <param name="EntityMaxHealth">The max health of the entity (if any).</param>
/// <param name="IsValidTarget">Whether this cell is a valid target in the current mode.</param>
public record HoverInfo(
    GridPosition Position,
    string CellLabel,
    string? EntityName,
    int? EntityHealth,
    int? EntityMaxHealth,
    bool IsValidTarget);
