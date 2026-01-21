// ═══════════════════════════════════════════════════════════════════════════════
// DiceCheckRenderer.cs
// Renders dice check displays with roll breakdowns and results.
// Version: 0.13.3d
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Presentation.Tui.Configuration;

namespace RuneAndRust.Presentation.Tui.Renderers;

/// <summary>
/// Renders dice check displays with roll breakdowns and results.
/// </summary>
/// <remarks>
/// <para>Provides consistent formatting for dice rolls including raw values,
/// modifiers, totals, and success/failure indicators.</para>
/// <para>Format examples:</para>
/// <code>
/// Roll: [14] + 3 = 17
/// Result: [x] SUCCESS!
/// </code>
/// </remarks>
public class DiceCheckRenderer
{
    private readonly GatheringDisplayConfig _config;

    // ═══════════════════════════════════════════════════════════════════════════
    // Constructor
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new instance of the DiceCheckRenderer.
    /// </summary>
    /// <param name="config">Configuration for display settings.</param>
    public DiceCheckRenderer(GatheringDisplayConfig? config = null)
    {
        _config = config ?? GatheringDisplayConfig.CreateDefault();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Public Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats a dice roll with breakdown.
    /// </summary>
    /// <param name="rawRoll">The raw dice roll value.</param>
    /// <param name="modifier">The skill modifier.</param>
    /// <param name="total">The total result.</param>
    /// <returns>The formatted roll string.</returns>
    /// <example>
    /// <code>
    /// renderer.FormatRoll(14, 3, 17);  // "[14] + 3 = 17"
    /// renderer.FormatRoll(8, -2, 6);   // "[8] - 2 = 6"
    /// </code>
    /// </example>
    public string FormatRoll(int rawRoll, int modifier, int total)
    {
        // Format: [14] + 3 = 17
        // Determine the sign based on modifier value
        var modifierSign = modifier >= 0 ? "+" : "-";
        var modifierValue = Math.Abs(modifier);

        return $"[{rawRoll}] {modifierSign} {modifierValue} = {total}";
    }

    /// <summary>
    /// Formats modifiers for display.
    /// </summary>
    /// <param name="modifiers">The list of modifiers.</param>
    /// <returns>The formatted modifiers string.</returns>
    /// <example>
    /// <code>
    /// renderer.FormatModifiers(new[] { 3, -1, 2 }); // "+ 3 - 1 + 2"
    /// </code>
    /// </example>
    public string FormatModifiers(IEnumerable<int> modifiers)
    {
        var modifierList = modifiers.ToList();
        if (modifierList.Count == 0)
        {
            return string.Empty;
        }

        var parts = new List<string>();
        foreach (var mod in modifierList)
        {
            var sign = mod >= 0 ? "+" : "-";
            parts.Add($"{sign} {Math.Abs(mod)}");
        }

        return string.Join(" ", parts);
    }

    /// <summary>
    /// Formats the difficulty class display.
    /// </summary>
    /// <param name="skillName">The name of the skill being checked.</param>
    /// <param name="dc">The difficulty class.</param>
    /// <returns>The formatted DC string.</returns>
    /// <example>
    /// <code>
    /// renderer.FormatDC("Herbalism", 10); // "Skill Check: Herbalism (DC 10)"
    /// </code>
    /// </example>
    public string FormatDC(string skillName, int dc)
    {
        return $"Skill Check: {skillName} (DC {dc})";
    }

    /// <summary>
    /// Formats the result indicator.
    /// </summary>
    /// <param name="success">Whether the check succeeded.</param>
    /// <returns>The formatted result string.</returns>
    /// <example>
    /// <code>
    /// renderer.FormatResult(true);  // "[x] SUCCESS!"
    /// renderer.FormatResult(false); // "[ ] FAILED"
    /// </code>
    /// </example>
    public string FormatResult(bool success)
    {
        return success ? "[x] SUCCESS!" : "[ ] FAILED";
    }

    /// <summary>
    /// Gets the color for a result indicator.
    /// </summary>
    /// <param name="success">Whether the check succeeded.</param>
    /// <returns>The appropriate console color.</returns>
    public ConsoleColor GetResultColor(bool success)
    {
        return success ? _config.SuccessColor : _config.FailureColor;
    }

    /// <summary>
    /// Formats a complete dice check display.
    /// </summary>
    /// <param name="skillName">The skill name.</param>
    /// <param name="dc">The difficulty class.</param>
    /// <param name="rawRoll">The raw dice roll.</param>
    /// <param name="modifier">The skill modifier.</param>
    /// <param name="total">The total result.</param>
    /// <param name="success">Whether the check succeeded.</param>
    /// <returns>Multi-line formatted check display.</returns>
    /// <example>
    /// <code>
    /// var display = renderer.FormatCompleteCheck("Herbalism", 10, 14, 3, 17, true);
    /// // Returns:
    /// // "Skill Check: Herbalism (DC 10)
    /// //
    /// // Rolling: 1d20 + 3 (Herbalism)
    /// // Result: [14] + 3 = 17  [x] SUCCESS!"
    /// </code>
    /// </example>
    public string FormatCompleteCheck(
        string skillName,
        int dc,
        int rawRoll,
        int modifier,
        int total,
        bool success)
    {
        var lines = new[]
        {
            FormatDC(skillName, dc),
            "",
            $"Rolling: 1d20 + {modifier} ({skillName})",
            $"Result: {FormatRoll(rawRoll, modifier, total)}  {FormatResult(success)}"
        };

        return string.Join(Environment.NewLine, lines);
    }
}
