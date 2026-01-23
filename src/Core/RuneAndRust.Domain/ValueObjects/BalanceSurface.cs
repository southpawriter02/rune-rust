namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents a surface that requires balance checks to traverse.
/// </summary>
/// <remarks>
/// <para>
/// BalanceSurface encapsulates all physical properties of a surface that
/// affect balance difficulty. It is used to calculate the DC for balance
/// checks and to determine fall consequences on failure.
/// </para>
/// <para>
/// <b>DC Calculation:</b>
/// <list type="bullet">
///   <item><description>Base DC from width (2-5)</description></item>
///   <item><description>+Stability modifier (0-2)</description></item>
///   <item><description>+Condition modifier (0-2)</description></item>
/// </list>
/// </para>
/// <para>
/// <b>v0.15.2f:</b> Initial implementation of balance surface properties.
/// </para>
/// </remarks>
/// <param name="Width">The width category of the surface.</param>
/// <param name="Stability">The stability state of the surface.</param>
/// <param name="LengthFeet">The length of the surface to traverse.</param>
/// <param name="HeightAboveGround">The fall height if balance is lost.</param>
/// <param name="Condition">Surface condition (dry, wet, icy).</param>
/// <param name="Description">Optional descriptive text for the surface.</param>
public readonly record struct BalanceSurface(
    BalanceWidth Width,
    SurfaceStability Stability,
    int LengthFeet,
    int HeightAboveGround,
    SurfaceCondition Condition = SurfaceCondition.Dry,
    string? Description = null)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Fall height threshold above which damage is taken (10+ feet).
    /// </summary>
    public const int DamagingFallThreshold = 10;

    // ═══════════════════════════════════════════════════════════════════════════
    // DC CALCULATION PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the base DC from the width category.
    /// </summary>
    public int BaseDc => Width switch
    {
        BalanceWidth.Wide => 2,
        BalanceWidth.Narrow => 3,
        BalanceWidth.Cable => 4,
        BalanceWidth.RazorEdge => 5,
        _ => 3
    };

    /// <summary>
    /// Gets the DC modifier from stability.
    /// </summary>
    public int StabilityModifier => Stability switch
    {
        SurfaceStability.Stable => 0,
        SurfaceStability.Unstable => 1,
        SurfaceStability.Swaying => 2,
        _ => 0
    };

    /// <summary>
    /// Gets the DC modifier from surface condition.
    /// </summary>
    public int ConditionModifier => Condition switch
    {
        SurfaceCondition.Dry => 0,
        SurfaceCondition.Wet => 1,
        SurfaceCondition.Icy => 2,
        _ => 0
    };

    /// <summary>
    /// Gets the total DC from surface properties alone (before environmental modifiers).
    /// </summary>
    public int SurfaceDc => BaseDc + StabilityModifier + ConditionModifier;

    // ═══════════════════════════════════════════════════════════════════════════
    // QUERY PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Indicates whether falling from this surface would cause damage.
    /// </summary>
    public bool FallCausesDamage => HeightAboveGround >= DamagingFallThreshold;

    /// <summary>
    /// Gets a descriptive name for the surface width.
    /// </summary>
    public string WidthDescription => Width switch
    {
        BalanceWidth.Wide => "wide (2+ ft)",
        BalanceWidth.Narrow => "narrow (1 ft)",
        BalanceWidth.Cable => "cable-thin (6 in)",
        BalanceWidth.RazorEdge => "razor-thin (< 6 in)",
        _ => "unknown"
    };

    /// <summary>
    /// Gets the stability description.
    /// </summary>
    public string StabilityDescription => Stability switch
    {
        SurfaceStability.Stable => "stable",
        SurfaceStability.Unstable => "unstable",
        SurfaceStability.Swaying => "swaying",
        _ => "unknown"
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a display string for the surface.
    /// </summary>
    /// <returns>Formatted surface description for UI display.</returns>
    public string ToDisplayString()
    {
        var stability = Stability != SurfaceStability.Stable
            ? $", {StabilityDescription}"
            : "";

        var condition = Condition != SurfaceCondition.Dry
            ? $", {Condition.ToString().ToLowerInvariant()}"
            : "";

        return $"{WidthDescription} surface ({LengthFeet} ft long, {HeightAboveGround} ft up{stability}{condition})";
    }

    /// <summary>
    /// Creates a short display string for the surface.
    /// </summary>
    /// <returns>Short surface description.</returns>
    public string ToShortString() => $"{WidthDescription} (DC {SurfaceDc})";

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a standard narrow ledge surface.
    /// </summary>
    /// <param name="lengthFeet">Length to traverse.</param>
    /// <param name="heightFeet">Height above ground.</param>
    /// <returns>A new narrow ledge surface.</returns>
    public static BalanceSurface NarrowLedge(int lengthFeet, int heightFeet)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(lengthFeet, nameof(lengthFeet));
        ArgumentOutOfRangeException.ThrowIfNegative(heightFeet, nameof(heightFeet));

        return new BalanceSurface(
            BalanceWidth.Narrow,
            SurfaceStability.Stable,
            lengthFeet,
            heightFeet,
            Description: "A narrow stone ledge.");
    }

    /// <summary>
    /// Creates a rope bridge surface (narrow, swaying).
    /// </summary>
    /// <param name="lengthFeet">Length to traverse.</param>
    /// <param name="heightFeet">Height above ground.</param>
    /// <returns>A new rope bridge surface.</returns>
    public static BalanceSurface RopeBridge(int lengthFeet, int heightFeet)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(lengthFeet, nameof(lengthFeet));
        ArgumentOutOfRangeException.ThrowIfNegative(heightFeet, nameof(heightFeet));

        return new BalanceSurface(
            BalanceWidth.Narrow,
            SurfaceStability.Swaying,
            lengthFeet,
            heightFeet,
            Description: "A rope bridge sways gently in the wind.");
    }

    /// <summary>
    /// Creates a cable or pipe surface.
    /// </summary>
    /// <param name="lengthFeet">Length to traverse.</param>
    /// <param name="heightFeet">Height above ground.</param>
    /// <returns>A new cable surface.</returns>
    public static BalanceSurface Cable(int lengthFeet, int heightFeet)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(lengthFeet, nameof(lengthFeet));
        ArgumentOutOfRangeException.ThrowIfNegative(heightFeet, nameof(heightFeet));

        return new BalanceSurface(
            BalanceWidth.Cable,
            SurfaceStability.Stable,
            lengthFeet,
            heightFeet,
            Description: "A thick cable stretches across the gap.");
    }

    /// <summary>
    /// Creates a crumbling ledge (narrow, unstable).
    /// </summary>
    /// <param name="lengthFeet">Length to traverse.</param>
    /// <param name="heightFeet">Height above ground.</param>
    /// <returns>A new crumbling ledge surface.</returns>
    public static BalanceSurface CrumblingLedge(int lengthFeet, int heightFeet)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(lengthFeet, nameof(lengthFeet));
        ArgumentOutOfRangeException.ThrowIfNegative(heightFeet, nameof(heightFeet));

        return new BalanceSurface(
            BalanceWidth.Narrow,
            SurfaceStability.Unstable,
            lengthFeet,
            heightFeet,
            Description: "Fragments of stone crumble away beneath your feet.");
    }

    /// <summary>
    /// Creates a wide plank surface (easy).
    /// </summary>
    /// <param name="lengthFeet">Length to traverse.</param>
    /// <param name="heightFeet">Height above ground.</param>
    /// <returns>A new wide plank surface.</returns>
    public static BalanceSurface WidePlank(int lengthFeet, int heightFeet)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(lengthFeet, nameof(lengthFeet));
        ArgumentOutOfRangeException.ThrowIfNegative(heightFeet, nameof(heightFeet));

        return new BalanceSurface(
            BalanceWidth.Wide,
            SurfaceStability.Stable,
            lengthFeet,
            heightFeet,
            Description: "A sturdy wooden plank bridges the gap.");
    }

    /// <summary>
    /// Creates a razor edge surface (extreme difficulty).
    /// </summary>
    /// <param name="lengthFeet">Length to traverse.</param>
    /// <param name="heightFeet">Height above ground.</param>
    /// <returns>A new razor edge surface.</returns>
    public static BalanceSurface RazorEdge(int lengthFeet, int heightFeet)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(lengthFeet, nameof(lengthFeet));
        ArgumentOutOfRangeException.ThrowIfNegative(heightFeet, nameof(heightFeet));

        return new BalanceSurface(
            BalanceWidth.RazorEdge,
            SurfaceStability.Stable,
            lengthFeet,
            heightFeet,
            Description: "A knife-thin edge, barely visible against the void.");
    }

    /// <summary>
    /// Creates a custom surface with specified properties.
    /// </summary>
    /// <param name="width">Width category.</param>
    /// <param name="stability">Stability state.</param>
    /// <param name="lengthFeet">Length to traverse.</param>
    /// <param name="heightFeet">Height above ground.</param>
    /// <param name="condition">Surface condition.</param>
    /// <param name="description">Optional description.</param>
    /// <returns>A new custom surface.</returns>
    public static BalanceSurface Create(
        BalanceWidth width,
        SurfaceStability stability,
        int lengthFeet,
        int heightFeet,
        SurfaceCondition condition = SurfaceCondition.Dry,
        string? description = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(lengthFeet, nameof(lengthFeet));
        ArgumentOutOfRangeException.ThrowIfNegative(heightFeet, nameof(heightFeet));

        return new BalanceSurface(width, stability, lengthFeet, heightFeet, condition, description);
    }
}
