                                                                                                                                 using RuneAndRust.Domain.Entities;
using RuneAndRust.Presentation.Shared.DTOs;

namespace RuneAndRust.Presentation.Shared.Interfaces;

/// <summary>
/// Interface for rendering the combat grid as ASCII art.
/// </summary>
public interface IGridRenderer
{
    /// <summary>
    /// Renders the full combat grid with optional box drawing.
    /// </summary>
    /// <param name="grid">The combat grid to render.</param>
    /// <param name="options">Rendering options.</param>
    /// <returns>ASCII representation of the grid.</returns>
    string RenderGrid(CombatGrid grid, GridRenderOptions? options = null);

    /// <summary>
    /// Renders a compact version of the grid without grid lines.
    /// </summary>
    /// <param name="grid">The combat grid to render.</param>
    /// <param name="options">Rendering options.</param>
    /// <returns>Compact ASCII representation.</returns>
    string RenderCompactGrid(CombatGrid grid, GridRenderOptions? options = null);

    /// <summary>
    /// Renders the grid legend explaining symbols.
    /// </summary>
    /// <param name="grid">The combat grid.</param>
    /// <returns>Legend text.</returns>
    string RenderLegend(CombatGrid grid);

    /// <summary>
    /// Renders a list of combatants with positions and stats.
    /// </summary>
    /// <param name="grid">The combat grid.</param>
    /// <param name="player">The player.</param>
    /// <param name="monsters">The monsters in combat.</param>
    /// <returns>Combatant list text.</returns>
    string RenderCombatantList(CombatGrid grid, Player player, IEnumerable<Monster> monsters);
}
