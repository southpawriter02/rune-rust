namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the result of a party stealth check using the weakest-link rule.
/// </summary>
/// <remarks>
/// <para>
/// Party stealth uses the weakest-link rule: the party member with the lowest
/// Acrobatics dice pool makes the check for the entire party. If they fail,
/// the entire party is detected.
/// </para>
/// <para>
/// "A chain is only as strong as its weakest link."
/// </para>
/// </remarks>
/// <param name="ParticipantIds">IDs of all party members in the check.</param>
/// <param name="WeakestMemberId">ID of the member with the lowest dice pool.</param>
/// <param name="WeakestPool">The dice pool size of the weakest member.</param>
/// <param name="Context">The stealth context for the check.</param>
/// <param name="Outcome">The skill outcome of the weakest member's roll.</param>
/// <param name="NetSuccesses">Net successes from the weakest member's roll.</param>
/// <param name="Margin">Margin of success or failure.</param>
/// <param name="PartyHidden">Whether the entire party is now hidden.</param>
/// <param name="DetectedBy">IDs of enemies who detected the party.</param>
/// <param name="FumbleTriggered">Whether a fumble triggered system-wide alert.</param>
public readonly record struct PartyStealthResult(
    IReadOnlyList<string> ParticipantIds,
    string WeakestMemberId,
    int WeakestPool,
    StealthContext Context,
    SkillOutcome Outcome,
    int NetSuccesses,
    int Margin,
    bool PartyHidden,
    IReadOnlyList<string>? DetectedBy = null,
    bool FumbleTriggered = false)
{
    /// <summary>
    /// Gets the number of party members in the check.
    /// </summary>
    public int PartySize => ParticipantIds.Count;

    /// <summary>
    /// Gets a value indicating whether the party was detected.
    /// </summary>
    public bool WasDetected => !PartyHidden;

    /// <summary>
    /// Gets a value indicating whether this was a critical party success.
    /// </summary>
    public bool IsCriticalSuccess => Outcome == SkillOutcome.CriticalSuccess;

    /// <summary>
    /// Gets a value indicating whether the party is on high alert.
    /// </summary>
    /// <remarks>
    /// Marginal success means the party is hidden but enemies are suspicious.
    /// </remarks>
    public bool EnemiesSuspicious => PartyHidden && Margin == 0;

    /// <summary>
    /// Creates a successful party stealth result.
    /// </summary>
    /// <param name="participantIds">All party member IDs.</param>
    /// <param name="weakestMemberId">ID of the weakest member.</param>
    /// <param name="weakestPool">Dice pool of the weakest member.</param>
    /// <param name="context">The stealth context.</param>
    /// <param name="netSuccesses">Net successes rolled.</param>
    /// <param name="outcome">The skill outcome.</param>
    /// <returns>A new PartyStealthResult for success.</returns>
    public static PartyStealthResult Success(
        IReadOnlyList<string> participantIds,
        string weakestMemberId,
        int weakestPool,
        StealthContext context,
        int netSuccesses,
        SkillOutcome outcome)
    {
        var margin = netSuccesses - context.EffectiveDc;

        return new PartyStealthResult(
            ParticipantIds: participantIds,
            WeakestMemberId: weakestMemberId,
            WeakestPool: weakestPool,
            Context: context,
            Outcome: outcome,
            NetSuccesses: netSuccesses,
            Margin: margin,
            PartyHidden: true);
    }

    /// <summary>
    /// Creates a failed party stealth result.
    /// </summary>
    /// <param name="participantIds">All party member IDs.</param>
    /// <param name="weakestMemberId">ID of the weakest member.</param>
    /// <param name="weakestPool">Dice pool of the weakest member.</param>
    /// <param name="context">The stealth context.</param>
    /// <param name="netSuccesses">Net successes rolled.</param>
    /// <param name="detectedBy">IDs of enemies who detected the party.</param>
    /// <returns>A new PartyStealthResult for failure.</returns>
    public static PartyStealthResult Failure(
        IReadOnlyList<string> participantIds,
        string weakestMemberId,
        int weakestPool,
        StealthContext context,
        int netSuccesses,
        IReadOnlyList<string>? detectedBy = null)
    {
        var margin = netSuccesses - context.EffectiveDc;

        return new PartyStealthResult(
            ParticipantIds: participantIds,
            WeakestMemberId: weakestMemberId,
            WeakestPool: weakestPool,
            Context: context,
            Outcome: SkillOutcome.Failure,
            NetSuccesses: netSuccesses,
            Margin: margin,
            PartyHidden: false,
            DetectedBy: detectedBy);
    }

    /// <summary>
    /// Creates a fumbled party stealth result.
    /// </summary>
    /// <param name="participantIds">All party member IDs.</param>
    /// <param name="weakestMemberId">ID of the weakest member.</param>
    /// <param name="weakestPool">Dice pool of the weakest member.</param>
    /// <param name="context">The stealth context.</param>
    /// <param name="detectedBy">IDs of enemies who detected the party.</param>
    /// <returns>A new PartyStealthResult for fumble.</returns>
    public static PartyStealthResult Fumble(
        IReadOnlyList<string> participantIds,
        string weakestMemberId,
        int weakestPool,
        StealthContext context,
        IReadOnlyList<string>? detectedBy = null)
    {
        return new PartyStealthResult(
            ParticipantIds: participantIds,
            WeakestMemberId: weakestMemberId,
            WeakestPool: weakestPool,
            Context: context,
            Outcome: SkillOutcome.CriticalFailure,
            NetSuccesses: 0,
            Margin: -context.EffectiveDc,
            PartyHidden: false,
            DetectedBy: detectedBy,
            FumbleTriggered: true);
    }

    /// <summary>
    /// Gets a description of the party stealth result.
    /// </summary>
    /// <returns>A narrative description of the outcome.</returns>
    public string ToDescription()
    {
        var weakestName = WeakestMemberId; // Would be resolved to character name in practice

        if (FumbleTriggered)
        {
            return $"{weakestName}'s catastrophic fumble triggers a [SYSTEM-WIDE ALERT]! " +
                   "All adjacent rooms have been alerted to your presence!";
        }

        if (WasDetected)
        {
            var detectCount = DetectedBy?.Count ?? 0;
            return $"{weakestName}'s attempt at stealth fails, alerting {detectCount} " +
                   "enemies to the party's presence!";
        }

        if (EnemiesSuspicious)
        {
            return $"The party barely manages to stay hidden, but {weakestName}'s " +
                   "movement has made the enemies suspicious...";
        }

        if (IsCriticalSuccess)
        {
            return "The party moves as one with the shadows, completely undetected!";
        }

        return "The party successfully sneaks past, guided by their weakest member's caution.";
    }

    /// <summary>
    /// Gets a formatted summary of the party stealth result.
    /// </summary>
    public string ToSummary()
    {
        var outcomeStr = Outcome switch
        {
            SkillOutcome.CriticalSuccess => "CRITICAL",
            SkillOutcome.Failure => "FAILED",
            SkillOutcome.CriticalFailure => "FUMBLE",
            _ => PartyHidden ? "SUCCESS" : "FAILED"
        };

        return $"Party Stealth ({PartySize} members): {outcomeStr} " +
               $"[{WeakestMemberId} rolled {NetSuccesses} vs DC {Context.EffectiveDc}]";
    }

    /// <inheritdoc/>
    public override string ToString() => ToSummary();
}
