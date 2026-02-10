// ═══════════════════════════════════════════════════════════════════════════════
// RecoveredData.cs
// Immutable value object representing data fragments recovered from a damaged
// Jötun terminal by the Jötun-Reader's Data Recovery ability.
// Version: 0.20.3b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using System.Text;

/// <summary>
/// Represents data fragments recovered from a Jötun terminal.
/// </summary>
/// <remarks>
/// <para>
/// Data Recovery (Tier 2) allows the Jötun-Reader to extract information from
/// damaged, corrupted, or locked terminals. The amount of data recovered depends
/// on the terminal's condition and the character's check result.
/// </para>
/// <para>
/// Key mechanics:
/// </para>
/// <list type="bullet">
///   <item><description><b>Success:</b> Recovers 1d4 data fragments</description></item>
///   <item><description><b>Critical (DC + 10):</b> Full recovery + 2 bonus fragments + 1 Lore Insight</description></item>
///   <item><description><b>Partial (DC - 5):</b> Recovers 1 fragment (hint only)</description></item>
///   <item><description><b>Failure:</b> No data recovered</description></item>
/// </list>
/// <para>
/// <b>Cost:</b> 3 AP, 2 Lore Insight.
/// <b>Tier:</b> 2 (requires 8 PP invested in Jötun-Reader tree).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var data = RecoveredData.Create(
///     terminalId, "Main Server",
///     new[] { "Override codes v2.14", "Project: SENTINEL" },
///     isComplete: false);
///
/// data.GetFragmentCount(); // 2
/// data.IsComplete;         // false
/// </code>
/// </example>
/// <seealso cref="RuneAndRust.Domain.Enums.TerminalState"/>
/// <seealso cref="RuneAndRust.Domain.Enums.JotunReaderAbilityId"/>
public sealed record RecoveredData
{
    /// <summary>
    /// Maximum completion percentage for incomplete recoveries.
    /// </summary>
    public const int MaxIncompletePercent = 95;

    /// <summary>
    /// Average character length divisor for completion estimation.
    /// </summary>
    public const int QualityDivisor = 10;

    /// <summary>Gets the unique identifier for this recovery.</summary>
    public Guid RecoveryId { get; init; }

    /// <summary>Gets the ID of the terminal the data was recovered from.</summary>
    public Guid SourceTerminalId { get; init; }

    /// <summary>Gets the display name of the source terminal.</summary>
    public string SourceTerminalName { get; init; } = string.Empty;

    /// <summary>Gets the individual data fragments recovered.</summary>
    public IReadOnlyList<string> DataFragments { get; init; } = [];

    /// <summary>Gets whether all data was successfully recovered.</summary>
    public bool IsComplete { get; init; }

    /// <summary>Gets the timestamp when the recovery was performed.</summary>
    public DateTime RecoveredAt { get; init; }

    /// <summary>
    /// Creates a new data recovery record.
    /// </summary>
    /// <param name="terminalId">ID of the source terminal.</param>
    /// <param name="terminalName">Display name of the source terminal.</param>
    /// <param name="fragments">The data fragments recovered.</param>
    /// <param name="isComplete">Whether all data was successfully recovered.</param>
    /// <returns>A new <see cref="RecoveredData"/> instance.</returns>
    public static RecoveredData Create(
        Guid terminalId,
        string terminalName,
        IEnumerable<string> fragments,
        bool isComplete)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(terminalName);
        ArgumentNullException.ThrowIfNull(fragments);

        return new RecoveredData
        {
            RecoveryId = Guid.NewGuid(),
            SourceTerminalId = terminalId,
            SourceTerminalName = terminalName,
            DataFragments = fragments.ToList().AsReadOnly(),
            IsComplete = isComplete,
            RecoveredAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Gets the total number of recovered fragments.
    /// </summary>
    public int GetFragmentCount() => DataFragments.Count;

    /// <summary>
    /// Estimates the recovery completion percentage.
    /// </summary>
    /// <remarks>
    /// Returns 100 for complete recoveries. For incomplete recoveries,
    /// estimates based on average fragment length, capped at 95%.
    /// </remarks>
    /// <returns>Completion percentage (0–100).</returns>
    public int GetCompletionPercentage()
    {
        if (IsComplete)
            return 100;

        if (DataFragments.Count == 0)
            return 0;

        var avgQuality = DataFragments.Average(f => f.Length);
        return (int)Math.Min(avgQuality / QualityDivisor, MaxIncompletePercent);
    }

    /// <summary>
    /// Combines all fragments into a single readable text block.
    /// </summary>
    /// <returns>Fragments separated by "[...]" gap markers.</returns>
    public string GetCombinedText() =>
        string.Join("\n[...]\n", DataFragments);

    /// <summary>
    /// Formats the recovery result for display.
    /// </summary>
    /// <returns>A multi-line formatted result including terminal name,
    /// fragment count, completion status, and recovered data.</returns>
    public string GetFormattedResult()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Terminal: {SourceTerminalName}");
        sb.AppendLine($"Fragments Recovered: {GetFragmentCount()}");
        sb.AppendLine($"Completion: {GetCompletionPercentage()}%");
        sb.AppendLine($"Status: {(IsComplete ? "COMPLETE" : "PARTIAL")}");
        sb.AppendLine();
        sb.AppendLine("Data:");
        sb.Append(GetCombinedText());
        return sb.ToString();
    }

    /// <summary>Returns a human-readable summary of the recovery.</summary>
    public override string ToString()
    {
        var status = IsComplete ? "COMPLETE" : "PARTIAL";
        return $"Recovered Data [{status}] from {SourceTerminalName}: " +
               $"{GetFragmentCount()} fragments ({GetCompletionPercentage()}%)";
    }
}
