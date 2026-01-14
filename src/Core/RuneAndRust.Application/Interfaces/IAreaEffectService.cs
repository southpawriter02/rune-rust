using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for calculating area effect targeting and cell enumeration.
/// </summary>
public interface IAreaEffectService
{
    /// <summary>
    /// Gets all cells affected by an area effect.
    /// </summary>
    /// <param name="areaEffect">The area effect configuration.</param>
    /// <param name="origin">The caster's position.</param>
    /// <param name="targetPoint">Target point for Circle/Square/Line shapes.</param>
    /// <param name="direction">Direction for Cone shape.</param>
    /// <returns>Enumerable of affected grid positions.</returns>
    IEnumerable<GridPosition> GetAffectedCells(
        AreaEffect areaEffect,
        GridPosition origin,
        GridPosition? targetPoint = null,
        FacingDirection? direction = null);

    /// <summary>
    /// Gets all entity IDs affected by an area effect.
    /// </summary>
    /// <param name="areaEffect">The area effect configuration.</param>
    /// <param name="origin">The caster's position.</param>
    /// <param name="targetPoint">Target point for Circle/Square/Line shapes.</param>
    /// <param name="direction">Direction for Cone shape.</param>
    /// <param name="casterId">The caster's entity ID (for filtering).</param>
    /// <returns>Enumerable of affected entity IDs.</returns>
    IEnumerable<Guid> GetAffectedEntities(
        AreaEffect areaEffect,
        GridPosition origin,
        GridPosition? targetPoint = null,
        FacingDirection? direction = null,
        Guid? casterId = null);

    /// <summary>
    /// Gets a preview of the area effect for UI display.
    /// </summary>
    /// <param name="areaEffect">The area effect configuration.</param>
    /// <param name="origin">The caster's position.</param>
    /// <param name="targetPoint">Target point for Circle/Square/Line shapes.</param>
    /// <param name="direction">Direction for Cone shape.</param>
    /// <param name="casterId">The caster's entity ID.</param>
    /// <returns>Preview information including cells and entities.</returns>
    AreaEffectPreview GetPreview(
        AreaEffect areaEffect,
        GridPosition origin,
        GridPosition? targetPoint = null,
        FacingDirection? direction = null,
        Guid? casterId = null);

    /// <summary>
    /// Validates a target point for an area effect.
    /// </summary>
    /// <param name="areaEffect">The area effect configuration.</param>
    /// <param name="origin">The caster's position.</param>
    /// <param name="targetPoint">The target point to validate.</param>
    /// <param name="range">Maximum range to target.</param>
    /// <returns>Validation result.</returns>
    AreaEffectValidation ValidateTarget(
        AreaEffect areaEffect,
        GridPosition origin,
        GridPosition targetPoint,
        int range);

    /// <summary>
    /// Gets cells in a circle centered on a point.
    /// </summary>
    /// <param name="center">Center of the circle.</param>
    /// <param name="radius">Radius in cells.</param>
    /// <returns>All cells within the circle.</returns>
    IEnumerable<GridPosition> GetCircleCells(GridPosition center, int radius);

    /// <summary>
    /// Gets cells in a cone from origin in a direction.
    /// </summary>
    /// <param name="origin">Starting point of the cone.</param>
    /// <param name="direction">Direction the cone spreads.</param>
    /// <param name="length">Length of the cone.</param>
    /// <param name="angleDegrees">Spread angle in degrees.</param>
    /// <returns>All cells within the cone.</returns>
    IEnumerable<GridPosition> GetConeCells(
        GridPosition origin,
        FacingDirection direction,
        int length,
        int angleDegrees);

    /// <summary>
    /// Gets cells in a line from origin to target.
    /// </summary>
    /// <param name="origin">Starting point of the line.</param>
    /// <param name="target">End point of the line.</param>
    /// <param name="width">Width of the line in cells.</param>
    /// <returns>All cells along the line.</returns>
    IEnumerable<GridPosition> GetLineCells(GridPosition origin, GridPosition target, int width);

    /// <summary>
    /// Gets cells in a square centered on a point.
    /// </summary>
    /// <param name="center">Center of the square.</param>
    /// <param name="size">Size of the square (width and height).</param>
    /// <returns>All cells within the square.</returns>
    IEnumerable<GridPosition> GetSquareCells(GridPosition center, int size);
}

/// <summary>
/// Preview of an area effect for UI display.
/// </summary>
/// <param name="AffectedCells">All cells in the effect area.</param>
/// <param name="AffectedEnemies">Enemies in the effect area.</param>
/// <param name="AffectedAllies">Allies in the effect area (if AffectsAllies).</param>
/// <param name="Description">Human-readable description of the effect.</param>
public readonly record struct AreaEffectPreview(
    IReadOnlyList<GridPosition> AffectedCells,
    IReadOnlyList<AffectedEntityInfo> AffectedEnemies,
    IReadOnlyList<AffectedEntityInfo> AffectedAllies,
    string Description)
{
    /// <summary>Gets the total number of affected entities.</summary>
    public int TotalAffected => AffectedEnemies.Count + AffectedAllies.Count;

    /// <summary>Creates an empty preview.</summary>
    public static AreaEffectPreview Empty => new([], [], [], "No effect");
}

/// <summary>
/// Information about an entity in an area effect.
/// </summary>
/// <param name="Id">Entity's unique ID.</param>
/// <param name="Name">Entity's display name.</param>
/// <param name="Position">Entity's grid position.</param>
/// <param name="IsAlly">Whether the entity is an ally of the caster.</param>
public readonly record struct AffectedEntityInfo(
    Guid Id,
    string Name,
    GridPosition Position,
    bool IsAlly);

/// <summary>
/// Result of validating an area effect target.
/// </summary>
/// <param name="IsValid">Whether the target is valid.</param>
/// <param name="Message">Description of validation result.</param>
/// <param name="InRange">Whether the target is within range.</param>
/// <param name="HasLineOfSight">Whether line of sight exists to target.</param>
public readonly record struct AreaEffectValidation(
    bool IsValid,
    string Message,
    bool InRange,
    bool HasLineOfSight)
{
    /// <summary>Creates a successful validation.</summary>
    public static AreaEffectValidation Success => new(true, "Valid target", true, true);

    /// <summary>Creates a validation failure for out of range.</summary>
    public static AreaEffectValidation OutOfRange => new(false, "Target out of range", false, true);

    /// <summary>Creates a validation failure for no line of sight.</summary>
    public static AreaEffectValidation NoLineOfSight => new(false, "No line of sight to target", true, false);
}
