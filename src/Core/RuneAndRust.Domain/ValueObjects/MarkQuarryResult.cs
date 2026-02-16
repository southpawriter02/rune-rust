namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the outcome of a Mark Quarry ability execution by a Veiðimaðr (Hunter).
/// Contains the created mark, previous/current mark counts, and any replaced mark information.
/// </summary>
/// <remarks>
/// <para>Mark Quarry is the Veiðimaðr's primary Tier 1 active ability, costing 1 AP to designate
/// a visible enemy within 12 spaces as quarry. The result captures:</para>
/// <list type="bullet">
/// <item>The newly created <see cref="QuarryMark"/> with +2 hit bonus</item>
/// <item>Mark count before and after (for UI transition display)</item>
/// <item>Any replaced mark if the hunter was at maximum capacity (3 marks, FIFO replacement)</item>
/// </list>
/// <para>This value object is returned by <c>IVeidimadurAbilityService.ExecuteMarkQuarry()</c>
/// and consumed by the presentation layer for combat log output.</para>
/// <para>Introduced in v0.20.7a as part of the Veiðimaðr specialization framework.</para>
/// </remarks>
public sealed record MarkQuarryResult
{
    /// <summary>
    /// Unique identifier of the hunter who placed the mark.
    /// </summary>
    public Guid HunterId { get; init; }

    /// <summary>
    /// Display name of the hunter (for combat log output).
    /// </summary>
    public string HunterName { get; init; } = string.Empty;

    /// <summary>
    /// Unique identifier of the target that was marked.
    /// </summary>
    public Guid TargetId { get; init; }

    /// <summary>
    /// Display name of the target that was marked (for combat log output).
    /// </summary>
    public string TargetName { get; init; } = string.Empty;

    /// <summary>
    /// The Quarry Mark that was created by this ability execution.
    /// Contains the +2 hit bonus and all mark metadata.
    /// </summary>
    public QuarryMark MarkCreated { get; init; } = null!;

    /// <summary>
    /// Number of active marks before this ability was executed (0–3).
    /// </summary>
    public int PreviousMarksCount { get; init; }

    /// <summary>
    /// Number of active marks after this ability was executed (1–3).
    /// </summary>
    public int CurrentMarksCount { get; init; }

    /// <summary>
    /// The mark that was replaced due to FIFO policy, or null if no replacement occurred.
    /// Non-null only when the hunter was at maximum capacity (3 marks) before marking.
    /// </summary>
    public QuarryMark? ReplacedMark { get; init; }

    /// <summary>
    /// UTC timestamp of when the mark was placed.
    /// </summary>
    public DateTime MarkedAt { get; init; }

    /// <summary>
    /// Checks whether an existing mark was replaced to make room for this new mark.
    /// </summary>
    /// <returns>True if <see cref="ReplacedMark"/> is not null (FIFO replacement occurred).</returns>
    public bool WasReplacement()
    {
        return ReplacedMark != null;
    }

    /// <summary>
    /// Gets the hit bonus of the created mark.
    /// </summary>
    /// <returns>The hit bonus value (typically +2).</returns>
    public int GetHitBonus()
    {
        return MarkCreated.HitBonus;
    }

    /// <summary>
    /// Returns a human-readable description of the mark action for combat log display.
    /// Includes replacement information if applicable.
    /// </summary>
    /// <returns>A formatted combat log message describing the mark action.</returns>
    public string GetDescription()
    {
        if (WasReplacement())
        {
            return $"Your oldest mark on {ReplacedMark!.TargetName} is replaced by your new mark on {TargetName}. " +
                   $"+{GetHitBonus()} to hit against {TargetName}. Quarry Marks: {CurrentMarksCount}/{QuarryMarksResource.DefaultMaxMarks}.";
        }

        return $"You mark {TargetName} as your quarry. +{GetHitBonus()} to hit. " +
               $"Quarry Marks: {CurrentMarksCount}/{QuarryMarksResource.DefaultMaxMarks}.";
    }

    /// <summary>
    /// Returns a formatted status message for UI display, including mark counts
    /// and replacement details.
    /// </summary>
    /// <returns>A formatted status message string.</returns>
    public string GetStatusMessage()
    {
        var message = $"QUARRY MARKED: {TargetName} (+{GetHitBonus()} to hit)";
        if (WasReplacement())
        {
            message += $" [Replaced: {ReplacedMark!.TargetName}]";
        }
        message += $" | Marks: {CurrentMarksCount}/{QuarryMarksResource.DefaultMaxMarks}";
        return message;
    }
}
