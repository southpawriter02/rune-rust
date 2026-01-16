namespace RuneAndRust.Presentation.Gui.Services;

using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Presentation.Gui.Models;

/// <summary>
/// Handles grid-based interaction for combat targeting.
/// </summary>
public interface IGridInteractionService
{
    /// <summary>
    /// Gets the current interaction mode.
    /// </summary>
    GridInteractionMode CurrentMode { get; }

    /// <summary>
    /// Gets whether targeting is currently active.
    /// </summary>
    bool IsTargeting { get; }

    /// <summary>
    /// Enters movement targeting mode.
    /// </summary>
    /// <param name="moverId">The ID of the entity that will move.</param>
    /// <param name="currentPosition">The entity's current position.</param>
    /// <param name="remainingMovement">The entity's remaining movement points.</param>
    void EnterMovementMode(Guid moverId, GridPosition currentPosition, int remainingMovement);

    /// <summary>
    /// Enters attack targeting mode.
    /// </summary>
    /// <param name="attackerId">The ID of the attacking entity.</param>
    /// <param name="currentPosition">The attacker's current position.</param>
    /// <param name="range">The attack range in cells.</param>
    void EnterAttackMode(Guid attackerId, GridPosition currentPosition, int range);

    /// <summary>
    /// Enters ability targeting mode.
    /// </summary>
    /// <param name="casterId">The ID of the entity using the ability.</param>
    /// <param name="currentPosition">The caster's current position.</param>
    /// <param name="ability">The ability being used.</param>
    void EnterAbilityMode(Guid casterId, GridPosition currentPosition, AbilityDefinition ability);

    /// <summary>
    /// Cancels current targeting mode.
    /// </summary>
    void CancelTargeting();

    /// <summary>
    /// Handles a cell click.
    /// </summary>
    /// <param name="position">The clicked cell position.</param>
    void HandleCellClick(GridPosition position);

    /// <summary>
    /// Handles cell hover.
    /// </summary>
    /// <param name="position">The hovered cell position.</param>
    void HandleCellHover(GridPosition position);

    /// <summary>
    /// Clears the hover state.
    /// </summary>
    void ClearHover();

    /// <summary>
    /// Gets the currently highlighted cells.
    /// </summary>
    IReadOnlyList<HighlightedCell> GetHighlightedCells();

    /// <summary>
    /// Gets info about the currently hovered cell.
    /// </summary>
    HoverInfo? GetHoverInfo();

    /// <summary>
    /// Raised when targeting completes (success or failure).
    /// </summary>
    event Action<TargetingResult>? OnTargetingComplete;

    /// <summary>
    /// Raised when highlighted cells change.
    /// </summary>
    event Action? OnHighlightsChanged;

    /// <summary>
    /// Raised when hover state changes.
    /// </summary>
    event Action<HoverInfo?>? OnHoverChanged;
}
