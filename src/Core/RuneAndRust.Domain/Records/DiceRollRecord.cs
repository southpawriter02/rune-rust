// ═══════════════════════════════════════════════════════════════════════════════
// DiceRollRecord.cs
// Immutable record capturing details of a single dice roll.
// Version: 0.12.0b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Records;

/// <summary>
/// Immutable record capturing details of a single dice roll.
/// </summary>
/// <remarks>
/// <para>This record provides a complete snapshot of a dice roll including:</para>
/// <list type="bullet">
///   <item><description>The dice expression rolled (e.g., "1d20", "2d6+3")</description></item>
///   <item><description>The final result after modifiers</description></item>
///   <item><description>Individual die results for natural 20/1 detection</description></item>
///   <item><description>Whether the roll included a critical (natural 20 or 1)</description></item>
///   <item><description>The context in which the roll was made</description></item>
///   <item><description>When the roll occurred</description></item>
/// </list>
/// <para>
/// Records are created by the DiceService when rolls are made and stored in
/// the DiceRollHistory entity for statistics tracking and display.
/// </para>
/// </remarks>
/// <param name="DiceExpression">The dice notation rolled (e.g., "1d20", "2d6+3").</param>
/// <param name="Result">The final result including any modifiers.</param>
/// <param name="IndividualRolls">The result of each individual die rolled.</param>
/// <param name="WasCritical">Whether any die showed a natural 20 or natural 1.</param>
/// <param name="Context">The context of the roll (e.g., "attack", "skill check", "damage").</param>
/// <param name="RolledAt">The UTC timestamp when the roll occurred.</param>
public record DiceRollRecord(
    string DiceExpression,
    int Result,
    int[] IndividualRolls,
    bool WasCritical,
    string Context,
    DateTime RolledAt)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new DiceRollRecord with automatic critical detection and timestamp.
    /// </summary>
    /// <param name="expression">The dice notation rolled (e.g., "1d20", "2d6+3").</param>
    /// <param name="result">The final result including modifiers.</param>
    /// <param name="rolls">The individual die results.</param>
    /// <param name="context">The context of the roll (e.g., "attack", "skill check").</param>
    /// <returns>A new DiceRollRecord instance with WasCritical and RolledAt populated.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="expression"/>, <paramref name="rolls"/>,
    /// or <paramref name="context"/> is null.
    /// </exception>
    /// <example>
    /// <code>
    /// // Create a record for an attack roll
    /// var record = DiceRollRecord.Create("1d20", 18, new[] { 18 }, "attack");
    ///
    /// // Create a record for damage roll with multiple dice
    /// var damageRecord = DiceRollRecord.Create("2d6+3", 11, new[] { 4, 4 }, "damage");
    /// </code>
    /// </example>
    public static DiceRollRecord Create(
        string expression,
        int result,
        int[] rolls,
        string context)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));
        ArgumentNullException.ThrowIfNull(rolls, nameof(rolls));
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        // Detect critical rolls (natural 20 or natural 1)
        var wasCritical = rolls.Any(r => r == 20 || r == 1);

        return new DiceRollRecord(
            expression,
            result,
            rolls,
            wasCritical,
            context,
            DateTime.UtcNow);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Computed Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this roll included a natural 20.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A natural 20 occurs when any individual die in the roll shows a 20.
    /// This is tracked separately from WasCritical to enable specific statistics.
    /// </para>
    /// </remarks>
    public bool HasNatural20 => IndividualRolls.Any(r => r == 20);

    /// <summary>
    /// Gets whether this roll included a natural 1.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A natural 1 occurs when any individual die in the roll shows a 1.
    /// This is tracked separately from WasCritical to enable specific statistics.
    /// </para>
    /// </remarks>
    public bool HasNatural1 => IndividualRolls.Any(r => r == 1);

    /// <summary>
    /// Gets the number of dice rolled.
    /// </summary>
    public int DiceCount => IndividualRolls.Length;

    /// <summary>
    /// Gets the sum of all individual die results (before modifiers).
    /// </summary>
    public int DiceSum => IndividualRolls.Sum();
}
