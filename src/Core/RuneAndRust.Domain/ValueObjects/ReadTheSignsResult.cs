using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the outcome of a Read the Signs investigation ability execution
/// by a Veiðimaðr (Hunter). Contains discovered information about creature tracks,
/// remains, or environmental signs.
/// </summary>
/// <remarks>
/// <para>Read the Signs is the Veiðimaðr's Tier 1 investigation ability:</para>
/// <list type="bullet">
/// <item>Cost: 1 AP, no resource cost</item>
/// <item>Range: Touch (adjacent to tracks/remains)</item>
/// <item>Skill check: 1d20 + Read the Signs (+4) + Keen Senses (+1 if unlocked) vs DC</item>
/// <item>DC determined by <see cref="CreatureTrackType"/>: Fresh=10, Recent=12, Worn=15, Ancient=18, Unclear=20</item>
/// </list>
/// <para>On success, creature information fields are populated with data from the encounter.
/// On failure, creature information fields are null and only vague impressions are provided
/// in <see cref="InformationRevealed"/>.</para>
/// <para>Introduced in v0.20.7a as part of the Veiðimaðr specialization framework.</para>
/// </remarks>
public sealed record ReadTheSignsResult
{
    /// <summary>
    /// Unique identifier of the character performing the investigation.
    /// </summary>
    public Guid InvestigatorId { get; init; }

    /// <summary>
    /// Description of the location being investigated (e.g., "muddy trail near the river crossing").
    /// </summary>
    public string LocationDescription { get; init; } = string.Empty;

    /// <summary>
    /// Type of creature identified from the tracks (e.g., "Corrupted Wolf", "Draugr Scout").
    /// Null on failure or if creature type is indeterminate.
    /// </summary>
    public string? CreatureType { get; init; }

    /// <summary>
    /// Estimated number of creatures that left the tracks.
    /// Null on failure or if count is indeterminate.
    /// </summary>
    public int? CreatureCount { get; init; }

    /// <summary>
    /// Estimated time since the creature was in this location (e.g., "1-2 hours ago").
    /// Null on failure.
    /// </summary>
    public string? TimePassedEstimate { get; init; }

    /// <summary>
    /// Direction the creature was heading (e.g., "northeast, toward the abandoned hold").
    /// Null on failure.
    /// </summary>
    public string? DirectionOfTravel { get; init; }

    /// <summary>
    /// Observable condition of the creature (e.g., "bleeding", "healthy, large specimen").
    /// Null on failure.
    /// </summary>
    public string? CreatureCondition { get; init; }

    /// <summary>
    /// Quality/freshness classification of the tracks being investigated.
    /// Determines the DC of the skill check.
    /// </summary>
    public CreatureTrackType TrackQuality { get; init; }

    /// <summary>
    /// Total bonus applied to the skill check from Veiðimaðr abilities
    /// (Read the Signs +4, Keen Senses +1 if unlocked).
    /// </summary>
    public int BonusApplied { get; init; }

    /// <summary>
    /// The total skill check result (1d20 roll + all bonuses).
    /// </summary>
    public int SkillCheckRoll { get; init; }

    /// <summary>
    /// The Difficulty Class that was compared against, determined by <see cref="TrackQuality"/>.
    /// </summary>
    public int SkillCheckDc { get; init; }

    /// <summary>
    /// Whether the investigation succeeded (skill check roll >= DC).
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// List of specific information revealed by the investigation.
    /// On success: detailed creature facts. On failure: vague impressions only.
    /// </summary>
    public IReadOnlyList<string> InformationRevealed { get; init; } = [];

    /// <summary>
    /// Returns a narrative description of the investigation findings.
    /// </summary>
    /// <returns>A human-readable summary of what was discovered.</returns>
    public string GetDescription()
    {
        if (!Success)
        {
            return $"You examine the {TrackQuality.ToString().ToLowerInvariant()} tracks at {LocationDescription}, " +
                   "but cannot determine what made them.";
        }

        var description = $"You study the {TrackQuality.ToString().ToLowerInvariant()} tracks at {LocationDescription}.";

        if (CreatureType != null)
            description += $" Creature: {CreatureType}.";
        if (CreatureCount.HasValue)
            description += $" Count: approximately {CreatureCount.Value}.";
        if (TimePassedEstimate != null)
            description += $" Time: {TimePassedEstimate}.";
        if (DirectionOfTravel != null)
            description += $" Direction: {DirectionOfTravel}.";
        if (CreatureCondition != null)
            description += $" Condition: {CreatureCondition}.";

        return description;
    }

    /// <summary>
    /// Formats the complete investigation results as a multi-line display string for combat output.
    /// Includes the skill check roll, DC, and all discovered information.
    /// </summary>
    /// <returns>A formatted multi-line investigation report.</returns>
    public string GetFormattedOutput()
    {
        var lines = new List<string>
        {
            $"=== Read the Signs: {LocationDescription} ===",
            $"Tracks: {TrackQuality} | Check: {SkillCheckRoll} vs DC {SkillCheckDc} | " +
                $"{(Success ? "SUCCESS" : "FAILURE")}",
            $"Bonus Applied: +{BonusApplied} (Read the Signs + Keen Senses)"
        };

        if (Success)
        {
            if (CreatureType != null)
                lines.Add($"  Creature: {CreatureType}");
            if (CreatureCount.HasValue)
                lines.Add($"  Count: ~{CreatureCount.Value}");
            if (TimePassedEstimate != null)
                lines.Add($"  Time Passed: {TimePassedEstimate}");
            if (DirectionOfTravel != null)
                lines.Add($"  Direction: {DirectionOfTravel}");
            if (CreatureCondition != null)
                lines.Add($"  Condition: {CreatureCondition}");
        }

        if (InformationRevealed.Count > 0)
        {
            lines.Add("  Discovered:");
            foreach (var info in InformationRevealed)
            {
                lines.Add($"    - {info}");
            }
        }

        lines.Add("===============================");

        return string.Join("\n", lines);
    }
}
