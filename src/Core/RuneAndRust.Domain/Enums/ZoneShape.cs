namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Shapes available for zone effects on the combat grid.
/// </summary>
/// <remarks>
/// <para>Zone shapes determine which cells are affected relative to the zone's center point:</para>
/// <list type="bullet">
///   <item><description><see cref="Circle"/> - Circular area using distance formula from center</description></item>
///   <item><description><see cref="Square"/> - Square area centered on the position</description></item>
///   <item><description><see cref="Line"/> - Linear area extending in a direction</description></item>
///   <item><description><see cref="Cone"/> - Cone shape expanding outward in a direction</description></item>
///   <item><description><see cref="Ring"/> - Hollow circle (outer edge only)</description></item>
/// </list>
/// <para>
/// The radius parameter defines the size of each shape. Line and Cone shapes also
/// require a direction parameter to determine their orientation.
/// </para>
/// </remarks>
public enum ZoneShape
{
    /// <summary>
    /// Circular area from center using Euclidean distance.
    /// </summary>
    /// <remarks>
    /// <para>Cells are included if their distance from center ≤ radius.</para>
    /// <para>A radius 2 circle includes approximately 13 cells.</para>
    /// <para>Examples: Healing Circle, Slow Field, Consecration</para>
    /// </remarks>
    Circle,

    /// <summary>
    /// Square area centered on the position.
    /// </summary>
    /// <remarks>
    /// <para>Cells are included within a (2 * radius + 1) square.</para>
    /// <para>A radius 2 square includes 25 cells (5x5).</para>
    /// <para>Examples: Blessing Square, Fortification Zone</para>
    /// </remarks>
    Square,

    /// <summary>
    /// Linear area extending from center in a direction.
    /// </summary>
    /// <remarks>
    /// <para>Cells are included along a line of length equal to radius.</para>
    /// <para>A radius 4 line includes 5 cells (center + 4).</para>
    /// <para>Examples: Wall of Fire, Lightning Bolt Ground Effect</para>
    /// </remarks>
    Line,

    /// <summary>
    /// Cone shape expanding outward from center in a direction.
    /// </summary>
    /// <remarks>
    /// <para>Width increases by 1 cell for each row away from center.</para>
    /// <para>A radius 3 cone includes approximately 18 cells.</para>
    /// <para>Examples: Dragon Breath Ground Fire, Ice Storm Wake</para>
    /// </remarks>
    Cone,

    /// <summary>
    /// Hollow ring around center (outer edge of circle only).
    /// </summary>
    /// <remarks>
    /// <para>Only cells at the edge of the radius are included.</para>
    /// <para>A radius 2 ring includes approximately 8 cells.</para>
    /// <para>Examples: Protective Ward Ring, Barrier Perimeter</para>
    /// </remarks>
    Ring
}
