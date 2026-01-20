namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// A hint that can be revealed for a puzzle.
/// </summary>
/// <remarks>
/// <para>
/// Hints provide guidance to players stuck on puzzles. They can be:
/// </para>
/// <list type="bullet">
///   <item><description>Free hints - revealed immediately on request</description></item>
///   <item><description>Check hints - require a dice check to reveal</description></item>
/// </list>
/// <para>
/// Hints are revealed in order (1 = first, 2 = second, etc.).
/// </para>
/// </remarks>
public class PuzzleHint
{
    // ===== Core Properties =====

    /// <summary>
    /// Gets the hint text.
    /// </summary>
    public string Text { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the order in which this hint is revealed (1 = first).
    /// </summary>
    public int Order { get; private set; }

    /// <summary>
    /// Gets the DC for a dice check to reveal this hint (0 = free).
    /// </summary>
    public int RevealDC { get; private set; }

    /// <summary>
    /// Gets the attribute used for the reveal check.
    /// </summary>
    public string? RevealAttribute { get; private set; }

    // ===== Computed Properties =====

    /// <summary>
    /// Gets whether revealing this hint has a cost.
    /// </summary>
    public bool HasCost => RevealDC > 0;

    /// <summary>
    /// Gets whether this hint is free to reveal.
    /// </summary>
    public bool IsFree => RevealDC == 0;

    // ===== Constructors =====

    /// <summary>
    /// Private parameterless constructor for EF Core.
    /// </summary>
    private PuzzleHint() { }

    // ===== Factory Methods =====

    /// <summary>
    /// Creates a free hint (no dice check required).
    /// </summary>
    /// <param name="text">The hint text.</param>
    /// <param name="order">The reveal order (1 = first).</param>
    /// <returns>A free PuzzleHint instance.</returns>
    public static PuzzleHint Free(string text, int order)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(order);

        return new PuzzleHint
        {
            Text = text,
            Order = order,
            RevealDC = 0
        };
    }

    /// <summary>
    /// Creates a hint requiring a dice check to reveal.
    /// </summary>
    /// <param name="text">The hint text.</param>
    /// <param name="order">The reveal order (1 = first).</param>
    /// <param name="dc">The difficulty class for the check.</param>
    /// <param name="attribute">The attribute used for the check.</param>
    /// <returns>A PuzzleHint with a reveal check.</returns>
    public static PuzzleHint WithCheck(string text, int order, int dc, string attribute)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(order);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dc);
        ArgumentException.ThrowIfNullOrWhiteSpace(attribute);

        return new PuzzleHint
        {
            Text = text,
            Order = order,
            RevealDC = dc,
            RevealAttribute = attribute
        };
    }

    /// <summary>
    /// Returns a string representation of this hint.
    /// </summary>
    public override string ToString() =>
        HasCost
            ? $"PuzzleHint(Order={Order}, DC={RevealDC} {RevealAttribute})"
            : $"PuzzleHint(Order={Order}, Free)";
}
