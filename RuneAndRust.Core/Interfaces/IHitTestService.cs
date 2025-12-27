namespace RuneAndRust.Core.Interfaces;

using RuneAndRust.Core.Enums;

/// <summary>
/// Target types for hit-testing.
/// </summary>
/// <remarks>
/// v0.3.23c: Initial implementation.
/// </remarks>
public enum HitTargetType
{
    /// <summary>No target hit.</summary>
    None,

    /// <summary>A cell in the combat grid.</summary>
    CombatGridCell,

    /// <summary>An entry in the turn order display.</summary>
    TurnOrderEntry,

    /// <summary>An ability button in the combat HUD.</summary>
    AbilityButton,

    /// <summary>A menu option.</summary>
    MenuOption,

    /// <summary>A cell in the minimap.</summary>
    MinimapCell,

    /// <summary>An entry in the combat/event log.</summary>
    LogEntry
}

/// <summary>
/// Result of a hit-test operation.
/// </summary>
/// <remarks>
/// v0.3.23c: Initial implementation.
/// </remarks>
/// <param name="TargetType">The type of UI element hit.</param>
/// <param name="Index">Index within the element (e.g., turn order position).</param>
/// <param name="Row">Grid row if applicable.</param>
/// <param name="Column">Grid column if applicable.</param>
/// <param name="Data">Additional context data (e.g., combatant reference).</param>
public record HitTestResult(
    HitTargetType TargetType,
    int? Index = null,
    int? Row = null,
    int? Column = null,
    object? Data = null
)
{
    /// <summary>
    /// Singleton for "nothing hit" result.
    /// </summary>
    public static readonly HitTestResult None = new(HitTargetType.None);

    /// <summary>
    /// Whether this result represents a hit.
    /// </summary>
    public bool IsHit => TargetType != HitTargetType.None;
}

/// <summary>
/// Service for mapping screen coordinates to game elements.
/// </summary>
/// <remarks>
/// See: SPEC-INPUT-002 for Mouse Support System design.
/// v0.3.23c: Initial implementation.
/// </remarks>
public interface IHitTestService
{
    /// <summary>
    /// Performs a hit-test at the given screen coordinates.
    /// </summary>
    /// <param name="screenX">1-based screen column.</param>
    /// <param name="screenY">1-based screen row.</param>
    /// <param name="currentPhase">Current game phase for context.</param>
    /// <returns>The hit-test result identifying what was clicked.</returns>
    HitTestResult HitTest(int screenX, int screenY, GamePhase currentPhase);

    /// <summary>
    /// Registers a clickable region for the current frame.
    /// Called by renderers to define interactive areas.
    /// </summary>
    /// <param name="type">The type of clickable element.</param>
    /// <param name="left">Left edge (1-based column).</param>
    /// <param name="top">Top edge (1-based row).</param>
    /// <param name="right">Right edge (1-based column, inclusive).</param>
    /// <param name="bottom">Bottom edge (1-based row, inclusive).</param>
    /// <param name="index">Optional index within element type.</param>
    /// <param name="row">Optional grid row.</param>
    /// <param name="column">Optional grid column.</param>
    /// <param name="data">Optional context data.</param>
    void RegisterRegion(
        HitTargetType type,
        int left, int top, int right, int bottom,
        int? index = null, int? row = null, int? column = null,
        object? data = null);

    /// <summary>
    /// Clears all registered regions. Called before each render.
    /// </summary>
    void ClearRegions();
}
