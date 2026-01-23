namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the result of a stealth check.
/// </summary>
/// <remarks>
/// <para>
/// Stealth check outcomes:
/// <list type="bullet">
///   <item><description>Critical Success (margin ≥ 5): [Hidden] + advantage on first attack</description></item>
///   <item><description>Success (margin 0-4): [Hidden] status applied</description></item>
///   <item><description>Failure (margin &lt; 0): Detected, not hidden</description></item>
///   <item><description>Fumble (0 successes + botch): [System-Wide Alert]</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="CharacterId">The ID of the character who made the check.</param>
/// <param name="Context">The stealth context used for the check.</param>
/// <param name="Outcome">The skill outcome classification.</param>
/// <param name="NetSuccesses">Number of net successes from the dice roll.</param>
/// <param name="Margin">Margin of success or failure (NetSuccesses - DC).</param>
/// <param name="BecameHidden">Whether the character is now [Hidden].</param>
/// <param name="HiddenStatus">The [Hidden] status if successful.</param>
/// <param name="FumbleTriggered">Whether a fumble occurred.</param>
/// <param name="DetectionModifier">Detection modifier applied if hidden.</param>
/// <param name="Description">Narrative description of the outcome.</param>
public readonly record struct StealthCheckResult(
    string CharacterId,
    StealthContext Context,
    SkillOutcome Outcome,
    int NetSuccesses,
    int Margin,
    bool BecameHidden,
    HiddenStatus? HiddenStatus,
    bool FumbleTriggered = false,
    int DetectionModifier = 0,
    string? Description = null)
{
    /// <summary>
    /// Gets a value indicating whether this was a critical success.
    /// </summary>
    public bool IsCritical => Outcome == SkillOutcome.CriticalSuccess;

    /// <summary>
    /// Gets a value indicating whether stealth was successful.
    /// </summary>
    public bool Succeeded => Margin >= 0;

    /// <summary>
    /// Gets a value indicating whether the character was detected.
    /// </summary>
    public bool WasDetected => !BecameHidden && !FumbleTriggered;

    /// <summary>
    /// Gets a value indicating whether enemies have advantage detecting this character.
    /// </summary>
    /// <remarks>
    /// Marginal success (margin 0) means enemies are suspicious.
    /// </remarks>
    public bool EnemiesSuspicious => BecameHidden && Margin == 0;

    /// <summary>
    /// Creates a successful stealth check result.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <param name="context">The stealth context.</param>
    /// <param name="netSuccesses">Net successes rolled.</param>
    /// <param name="outcome">The skill outcome.</param>
    /// <returns>A new StealthCheckResult for success.</returns>
    public static StealthCheckResult Success(
        string characterId,
        StealthContext context,
        int netSuccesses,
        SkillOutcome outcome)
    {
        var margin = netSuccesses - context.EffectiveDc;
        var detectionMod = outcome == SkillOutcome.CriticalSuccess ? 2 : 0;
        var hidden = Entities.HiddenStatus.FromStealthCheck(characterId, detectionMod);

        var description = outcome switch
        {
            SkillOutcome.CriticalSuccess =>
                "You blend into the shadows perfectly, becoming all but invisible.",
            SkillOutcome.ExceptionalSuccess =>
                "You move with practiced grace, unnoticed by your enemies.",
            SkillOutcome.FullSuccess =>
                "You slip into the shadows undetected.",
            SkillOutcome.MarginalSuccess =>
                "You manage to hide, but something feels off - they may be watching.",
            _ => "You attempt to hide."
        };

        return new StealthCheckResult(
            CharacterId: characterId,
            Context: context,
            Outcome: outcome,
            NetSuccesses: netSuccesses,
            Margin: margin,
            BecameHidden: true,
            HiddenStatus: hidden,
            DetectionModifier: detectionMod,
            Description: description);
    }

    /// <summary>
    /// Creates a failed stealth check result.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <param name="context">The stealth context.</param>
    /// <param name="netSuccesses">Net successes rolled.</param>
    /// <returns>A new StealthCheckResult for failure.</returns>
    public static StealthCheckResult Failure(
        string characterId,
        StealthContext context,
        int netSuccesses)
    {
        var margin = netSuccesses - context.EffectiveDc;

        return new StealthCheckResult(
            CharacterId: characterId,
            Context: context,
            Outcome: SkillOutcome.Failure,
            NetSuccesses: netSuccesses,
            Margin: margin,
            BecameHidden: false,
            HiddenStatus: null,
            Description: "Your attempt at stealth fails - you've been spotted!");
    }

    /// <summary>
    /// Creates a fumbled stealth check result.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <param name="context">The stealth context.</param>
    /// <returns>A new StealthCheckResult for fumble.</returns>
    public static StealthCheckResult Fumble(
        string characterId,
        StealthContext context)
    {
        return new StealthCheckResult(
            CharacterId: characterId,
            Context: context,
            Outcome: SkillOutcome.CriticalFailure,
            NetSuccesses: 0,
            Margin: -context.EffectiveDc,
            BecameHidden: false,
            HiddenStatus: null,
            FumbleTriggered: true,
            Description: "You stumble catastrophically, triggering a [System-Wide Alert]!");
    }

    /// <summary>
    /// Gets a formatted summary of the stealth check result.
    /// </summary>
    public string ToSummary()
    {
        var outcomeStr = Outcome switch
        {
            SkillOutcome.CriticalSuccess => "CRITICAL SUCCESS",
            SkillOutcome.ExceptionalSuccess => "Exceptional Success",
            SkillOutcome.FullSuccess => "Success",
            SkillOutcome.MarginalSuccess => "Marginal Success",
            SkillOutcome.Failure => "Failure",
            SkillOutcome.CriticalFailure => "FUMBLE",
            _ => Outcome.ToString()
        };

        var statusStr = BecameHidden ? " - [HIDDEN]" : FumbleTriggered ? " - ALERT!" : " - DETECTED";

        return $"Stealth: {outcomeStr} ({NetSuccesses} successes vs DC {Context.EffectiveDc}){statusStr}";
    }

    /// <inheritdoc/>
    public override string ToString() => ToSummary();
}
