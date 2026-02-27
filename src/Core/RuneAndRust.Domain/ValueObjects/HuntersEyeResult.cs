using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Encapsulates the result of applying Hunter's Eye passive ability to a ranged attack.
/// Hunter's Eye allows the Veiðimaðr to ignore partial cover penalties when making ranged attacks.
/// </summary>
/// <remarks>
/// <para>Hunter's Eye is a Tier 2 passive ability for the Veiðimaðr (Hunter) specialization.
/// It evaluates whether cover should be ignored for a specific attack context and calculates
/// the effective bonus gained from bypassing the cover penalty.</para>
/// <para>Cover interaction rules:</para>
/// <list type="bullet">
/// <item><description><see cref="CoverType.Partial"/>: Ignored — the +2 AC penalty is negated.</description></item>
/// <item><description><see cref="CoverType.Full"/>: NOT ignored — target cannot be attacked through full cover.</description></item>
/// <item><description><see cref="CoverType.None"/>: No effect — no cover penalty to bypass.</description></item>
/// </list>
/// <para>Introduced in v0.20.7b. Coherent path — zero Corruption risk.</para>
/// </remarks>
public sealed record HuntersEyeResult
{
    /// <summary>
    /// The AC penalty normally applied by partial cover, which Hunter's Eye negates.
    /// </summary>
    private const int PartialCoverPenalty = 2;

    /// <summary>
    /// Gets the ID of the hunter using Hunter's Eye.
    /// </summary>
    public Guid HunterId { get; init; }

    /// <summary>
    /// Gets the ID of the target being attacked.
    /// </summary>
    public Guid TargetId { get; init; }

    /// <summary>
    /// Gets the display name of the target (for UI/logging).
    /// </summary>
    public string TargetName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the original cover type the target had before Hunter's Eye evaluation.
    /// </summary>
    public CoverType OriginalCoverType { get; init; }

    /// <summary>
    /// Gets whether the cover was successfully ignored by Hunter's Eye.
    /// True only when the target had <see cref="CoverType.Partial"/> cover.
    /// </summary>
    public bool CoverIgnored { get; init; }

    /// <summary>
    /// Gets the effective bonus gained by ignoring cover (i.e., the negated penalty).
    /// Equals <see cref="PartialCoverPenalty"/> (2) when partial cover is ignored, 0 otherwise.
    /// </summary>
    public int BonusFromCoverIgnored { get; init; }

    /// <summary>
    /// Gets the distance to the target in spaces.
    /// </summary>
    public int Distance { get; init; }

    /// <summary>
    /// Determines whether a given cover type should be ignored by Hunter's Eye.
    /// </summary>
    /// <param name="coverType">The cover type to evaluate.</param>
    /// <returns>True if the cover type is <see cref="CoverType.Partial"/>; false otherwise.</returns>
    /// <remarks>
    /// Full cover cannot be ignored — the target must still be targetable.
    /// None cover has no penalty to ignore.
    /// </remarks>
    public static bool ShouldIgnoreCover(CoverType coverType)
    {
        return coverType == CoverType.Partial;
    }

    /// <summary>
    /// Gets the AC penalty normally imposed by a given cover type.
    /// </summary>
    /// <param name="coverType">The cover type to evaluate.</param>
    /// <returns>2 for <see cref="CoverType.Partial"/>, 0 for all other cover types.</returns>
    public static int GetCoverPenalty(CoverType coverType)
    {
        return coverType switch
        {
            CoverType.Partial => PartialCoverPenalty,
            _ => 0
        };
    }

    /// <summary>
    /// Returns whether the cover was successfully ignored by Hunter's Eye.
    /// </summary>
    /// <returns>True if the attack bypassed the target's cover.</returns>
    public bool WasCoverIgnored() => CoverIgnored;

    /// <summary>
    /// Gets a narrative description of the Hunter's Eye result for combat logging.
    /// </summary>
    /// <returns>A descriptive string suitable for display in the combat log.</returns>
    public string GetDescription()
    {
        if (CoverIgnored)
        {
            return $"Hunter's Eye: {TargetName}'s {OriginalCoverType.ToString().ToLowerInvariant()} cover ignored " +
                   $"(+{BonusFromCoverIgnored} effective bonus) at {Distance} spaces.";
        }

        if (OriginalCoverType == CoverType.Full)
        {
            return $"Hunter's Eye: Cannot ignore {TargetName}'s full cover — target is not attackable.";
        }

        return $"Hunter's Eye: {TargetName} has no cover to ignore at {Distance} spaces.";
    }
}
