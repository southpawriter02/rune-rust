using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Defines an area effect configuration for an ability.
/// </summary>
/// <remarks>
/// Area effects define how abilities affect multiple grid cells.
/// Use factory methods to create specific shape configurations.
/// </remarks>
public readonly record struct AreaEffect
{
    /// <summary>Gets the shape of the area effect.</summary>
    public AreaEffectShape Shape { get; init; }

    /// <summary>Gets the radius for Circle or half-size for Square shapes.</summary>
    public int Radius { get; init; }

    /// <summary>Gets the length for Cone and Line shapes.</summary>
    public int Length { get; init; }

    /// <summary>Gets the width (angle for Cone, cells for Line).</summary>
    public int Width { get; init; }

    /// <summary>Gets whether the caster is included in the effect.</summary>
    public bool IncludesCaster { get; init; }

    /// <summary>Gets whether allies are affected by the effect.</summary>
    public bool AffectsAllies { get; init; }

    /// <summary>Gets whether enemies are affected by the effect.</summary>
    public bool AffectsEnemies { get; init; }

    /// <summary>
    /// Creates a circle area effect centered on target point.
    /// </summary>
    /// <param name="radius">Radius in cells from center.</param>
    /// <param name="affectsAllies">Whether allies are affected.</param>
    /// <param name="includesCaster">Whether the caster is affected if in area.</param>
    /// <returns>A circle area effect configuration.</returns>
    public static AreaEffect Circle(int radius, bool affectsAllies = false, bool includesCaster = false) =>
        new()
        {
            Shape = AreaEffectShape.Circle,
            Radius = radius,
            AffectsAllies = affectsAllies,
            AffectsEnemies = true,
            IncludesCaster = includesCaster
        };

    /// <summary>
    /// Creates a cone area effect spreading from the caster.
    /// </summary>
    /// <param name="length">Length of the cone in cells.</param>
    /// <param name="angleDegrees">Spread angle in degrees (default 90).</param>
    /// <param name="affectsAllies">Whether allies are affected.</param>
    /// <returns>A cone area effect configuration.</returns>
    public static AreaEffect Cone(int length, int angleDegrees = 90, bool affectsAllies = false) =>
        new()
        {
            Shape = AreaEffectShape.Cone,
            Length = length,
            Width = angleDegrees,
            AffectsAllies = affectsAllies,
            AffectsEnemies = true
        };

    /// <summary>
    /// Creates a line area effect from caster to target.
    /// </summary>
    /// <param name="length">Maximum length in cells.</param>
    /// <param name="width">Width in cells (default 1).</param>
    /// <param name="affectsAllies">Whether allies are affected.</param>
    /// <returns>A line area effect configuration.</returns>
    public static AreaEffect Line(int length, int width = 1, bool affectsAllies = false) =>
        new()
        {
            Shape = AreaEffectShape.Line,
            Length = length,
            Width = width,
            AffectsAllies = affectsAllies,
            AffectsEnemies = true
        };

    /// <summary>
    /// Creates a square area effect centered on target point.
    /// </summary>
    /// <param name="size">Size of the square (odd number recommended).</param>
    /// <param name="affectsAllies">Whether allies are affected.</param>
    /// <param name="includesCaster">Whether the caster is affected if in area.</param>
    /// <returns>A square area effect configuration.</returns>
    public static AreaEffect Square(int size, bool affectsAllies = false, bool includesCaster = false) =>
        new()
        {
            Shape = AreaEffectShape.Square,
            Radius = size / 2,
            AffectsAllies = affectsAllies,
            AffectsEnemies = true,
            IncludesCaster = includesCaster
        };

    /// <summary>
    /// Returns a string representation of the area effect.
    /// </summary>
    public override string ToString() => Shape switch
    {
        AreaEffectShape.Circle => $"Circle(radius={Radius})",
        AreaEffectShape.Cone => $"Cone(length={Length}, angle={Width}°)",
        AreaEffectShape.Line => $"Line(length={Length}, width={Width})",
        AreaEffectShape.Square => $"Square(size={Radius * 2 + 1})",
        _ => "None"
    };
}
