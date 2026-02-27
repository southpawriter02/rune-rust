using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Immutable result record from the Apex Predator passive ability evaluation.
/// Records whether a marked quarry's concealment was denied by the hunter.
/// </summary>
/// <remarks>
/// <para>Introduced in v0.20.7c as the Veiðimaðr Tier 3 passive evaluation result.</para>
/// <para>Apex Predator denies concealment benefits for targets with active Quarry Marks.
/// Does NOT affect cover (physical defense) — only concealment (perception-based).
/// Cover bypass is handled separately by Hunter's Eye (Tier 2).</para>
/// <para>Concealment is denied when both conditions are met:</para>
/// <list type="bullet">
/// <item><description>Target has an active Quarry Mark (designated via Mark Quarry)</description></item>
/// <item><description>Target has any concealment type other than <see cref="ConcealmentType.None"/></description></item>
/// </list>
/// <para>Combined with Hunter's Eye (T2), a marked quarry has no hiding place:
/// Hunter's Eye bypasses cover, Apex Predator bypasses concealment.</para>
/// <para>No Corruption risk — Apex Predator follows the Coherent path.</para>
/// </remarks>
public sealed record ApexPredatorResult
{
    /// <summary>
    /// Unique identifier of the Veiðimaðr evaluating the target.
    /// </summary>
    public Guid HunterId { get; init; }

    /// <summary>
    /// Unique identifier of the target being evaluated for concealment denial.
    /// </summary>
    public Guid TargetId { get; init; }

    /// <summary>
    /// Display name of the target (for UI and logging).
    /// </summary>
    public string TargetName { get; init; } = string.Empty;

    /// <summary>
    /// Whether the target had any active concealment before Apex Predator evaluation.
    /// True if <see cref="ConcealmentType"/> is not <see cref="Enums.ConcealmentType.None"/>.
    /// </summary>
    public bool WasConcealed { get; init; }

    /// <summary>
    /// The type of concealment the target had (or <see cref="Enums.ConcealmentType.None"/>).
    /// </summary>
    public ConcealmentType ConcealmentType { get; init; }

    /// <summary>
    /// Whether the concealment was successfully denied by Apex Predator.
    /// True only when the target is both marked as quarry AND had active concealment.
    /// </summary>
    public bool ConcealmentDenied { get; init; }

    /// <summary>
    /// Whether the target had an active Quarry Mark at the time of evaluation.
    /// Apex Predator only applies to marked targets.
    /// </summary>
    public bool TargetWasMarked { get; init; }

    /// <summary>
    /// Determines whether concealment should be denied for a target based on
    /// their mark status and concealment type.
    /// </summary>
    /// <param name="concealmentType">The target's current concealment type.</param>
    /// <param name="isMarked">Whether the target has an active Quarry Mark.</param>
    /// <returns>
    /// True if concealment should be denied (target is marked AND has concealment);
    /// false otherwise.
    /// </returns>
    /// <remarks>
    /// Pure static function matching the <see cref="HuntersEyeResult.ShouldIgnoreCover"/> pattern.
    /// Concealment is only denied when both conditions are true — unmarked targets
    /// retain their concealment, and marked targets with no concealment are unaffected.
    /// </remarks>
    public static bool ShouldDenyConcealment(ConcealmentType concealmentType, bool isMarked)
    {
        return isMarked && concealmentType != ConcealmentType.None;
    }

    /// <summary>
    /// Returns whether concealment was lost as a result of this evaluation.
    /// Equivalent to <see cref="ConcealmentDenied"/>.
    /// </summary>
    /// <returns>True if concealment was denied by Apex Predator.</returns>
    public bool IsConcealmentLost() => ConcealmentDenied;

    /// <summary>
    /// Returns a human-readable description of the Apex Predator evaluation outcome.
    /// Differentiates between: concealment denied, target not marked, and no concealment present.
    /// </summary>
    /// <returns>A narrative description of the evaluation result.</returns>
    public string GetDescription()
    {
        if (ConcealmentDenied)
        {
            return $"Apex Predator: {TargetName}'s {FormatConcealmentType(ConcealmentType)} " +
                   "concealment denied — the hunter's mark reveals their position.";
        }

        if (!TargetWasMarked)
        {
            return $"Apex Predator: {TargetName} is not marked — concealment unaffected.";
        }

        return $"Apex Predator: {TargetName} has no concealment to deny.";
    }

    /// <summary>
    /// Formats a <see cref="ConcealmentType"/> value as a lowercase display string.
    /// </summary>
    /// <param name="type">The concealment type to format.</param>
    /// <returns>A lowercase string representation of the concealment type.</returns>
    private static string FormatConcealmentType(ConcealmentType type)
    {
        return type switch
        {
            ConcealmentType.LightObscurement => "light obscurement",
            ConcealmentType.Invisibility => "invisibility",
            ConcealmentType.MagicalCamo => "magical camouflage",
            ConcealmentType.Hidden => "stealth",
            _ => "unknown"
        };
    }
}
