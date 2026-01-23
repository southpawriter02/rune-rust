// ------------------------------------------------------------------------------
// <copyright file="DeceptionResult.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Captures the outcome of a deception attempt.
// Part of v0.15.3c Deception System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Entities;

/// <summary>
/// Captures the outcome of a deception attempt.
/// </summary>
/// <remarks>
/// <para>
/// The deception result includes whether the lie was believed, any changes
/// to the NPC's suspicion state, the psychic stress cost from the Liar's
/// Burden mechanic, and any fumble consequences such as [Lie Exposed].
/// </para>
/// <para>
/// Unlike persuasion, deception always incurs a stress cost regardless of
/// outcome, reflecting the psychological toll of maintaining falsehoods.
/// </para>
/// </remarks>
/// <param name="Outcome">The skill outcome classification.</param>
/// <param name="LieBelieved">Whether the NPC believes the lie.</param>
/// <param name="PlayerSuccesses">Number of successes the player rolled.</param>
/// <param name="NpcSuccesses">Number of successes the NPC rolled.</param>
/// <param name="Margin">The difference between player and NPC successes.</param>
/// <param name="NpcSuspicionChange">Change to NPC suspicion (0 = no change, +1 = suspicious, +2 = very suspicious).</param>
/// <param name="StressCost">Psychic stress incurred from Liar's Burden.</param>
/// <param name="DispositionChange">Change to NPC disposition.</param>
/// <param name="UnlockedOptions">Dialogue options unlocked by successful deception.</param>
/// <param name="LockedOptions">Dialogue options locked by failed deception.</param>
/// <param name="FumbleConsequence">The [Lie Exposed] fumble consequence if applicable.</param>
/// <param name="CombatInitiated">Whether the fumble resulted in combat.</param>
/// <param name="SettlementBan">Whether the fumble resulted in settlement ban.</param>
/// <param name="SettlementBanDuration">Duration of ban in in-game days (0 = permanent).</param>
/// <param name="QuestFailed">Whether the fumble caused a quest to fail.</param>
/// <param name="FailedQuestId">ID of the failed quest if applicable.</param>
/// <param name="UntrustworthyApplied">Whether [Untrustworthy] flag was applied.</param>
/// <param name="NarrativeText">Flavor text describing the outcome.</param>
public readonly record struct DeceptionResult(
    SkillOutcome Outcome,
    bool LieBelieved,
    int PlayerSuccesses,
    int NpcSuccesses,
    int Margin,
    int NpcSuspicionChange,
    int StressCost,
    int DispositionChange,
    IReadOnlyList<string> UnlockedOptions,
    IReadOnlyList<string> LockedOptions,
    FumbleConsequence? FumbleConsequence,
    bool CombatInitiated,
    bool SettlementBan,
    int SettlementBanDuration,
    bool QuestFailed,
    string? FailedQuestId,
    bool UntrustworthyApplied,
    string? NarrativeText)
{
    /// <summary>
    /// Gets whether this was a successful deception.
    /// </summary>
    public bool IsSuccess => Outcome >= SkillOutcome.MarginalSuccess;

    /// <summary>
    /// Gets whether this was a fumble.
    /// </summary>
    public bool IsFumble => Outcome == SkillOutcome.CriticalFailure;

    /// <summary>
    /// Gets whether the NPC now suspects the player.
    /// </summary>
    public bool NpcNowSuspicious => NpcSuspicionChange > 0;

    /// <summary>
    /// Gets whether the deception had severe consequences.
    /// </summary>
    public bool HasSevereConsequences =>
        CombatInitiated || SettlementBan || QuestFailed || UntrustworthyApplied;

    /// <summary>
    /// Creates a successful deception result.
    /// </summary>
    /// <param name="outcome">The success outcome tier.</param>
    /// <param name="playerSuccesses">Player's success count.</param>
    /// <param name="npcSuccesses">NPC's success count.</param>
    /// <param name="stressCost">Liar's Burden stress cost.</param>
    /// <param name="unlockedOptions">Dialogue options unlocked.</param>
    /// <param name="narrativeText">Flavor text for the outcome.</param>
    /// <returns>A successful DeceptionResult.</returns>
    public static DeceptionResult Success(
        SkillOutcome outcome,
        int playerSuccesses,
        int npcSuccesses,
        int stressCost,
        IReadOnlyList<string>? unlockedOptions = null,
        string? narrativeText = null)
    {
        return new DeceptionResult(
            Outcome: outcome,
            LieBelieved: true,
            PlayerSuccesses: playerSuccesses,
            NpcSuccesses: npcSuccesses,
            Margin: playerSuccesses - npcSuccesses,
            NpcSuspicionChange: 0,
            StressCost: stressCost,
            DispositionChange: 0,
            UnlockedOptions: unlockedOptions ?? Array.Empty<string>(),
            LockedOptions: Array.Empty<string>(),
            FumbleConsequence: null,
            CombatInitiated: false,
            SettlementBan: false,
            SettlementBanDuration: 0,
            QuestFailed: false,
            FailedQuestId: null,
            UntrustworthyApplied: false,
            NarrativeText: narrativeText ?? GetDefaultSuccessNarrative(outcome));
    }

    /// <summary>
    /// Creates a failed deception result (not fumble).
    /// </summary>
    /// <param name="playerSuccesses">Player's success count.</param>
    /// <param name="npcSuccesses">NPC's success count.</param>
    /// <param name="stressCost">Liar's Burden stress cost.</param>
    /// <param name="dispositionChange">Disposition penalty.</param>
    /// <param name="lockedOptions">Dialogue options locked.</param>
    /// <param name="narrativeText">Flavor text for the outcome.</param>
    /// <returns>A failed DeceptionResult.</returns>
    public static DeceptionResult Failure(
        int playerSuccesses,
        int npcSuccesses,
        int stressCost,
        int dispositionChange = -10,
        IReadOnlyList<string>? lockedOptions = null,
        string? narrativeText = null)
    {
        return new DeceptionResult(
            Outcome: SkillOutcome.Failure,
            LieBelieved: false,
            PlayerSuccesses: playerSuccesses,
            NpcSuccesses: npcSuccesses,
            Margin: playerSuccesses - npcSuccesses,
            NpcSuspicionChange: 1, // NPC becomes suspicious
            StressCost: stressCost,
            DispositionChange: dispositionChange,
            UnlockedOptions: Array.Empty<string>(),
            LockedOptions: lockedOptions ?? Array.Empty<string>(),
            FumbleConsequence: null,
            CombatInitiated: false,
            SettlementBan: false,
            SettlementBanDuration: 0,
            QuestFailed: false,
            FailedQuestId: null,
            UntrustworthyApplied: false,
            NarrativeText: narrativeText ?? "The NPC doesn't buy it. Something in your story doesn't add up.");
    }

    /// <summary>
    /// Creates a fumble result with [Lie Exposed] consequences.
    /// </summary>
    /// <param name="playerSuccesses">Player's success count.</param>
    /// <param name="npcSuccesses">NPC's success count (0 for fumble).</param>
    /// <param name="stressCost">Liar's Burden stress cost (should be 13).</param>
    /// <param name="fumbleConsequence">The created fumble consequence entity.</param>
    /// <param name="combatInitiated">Whether combat was triggered.</param>
    /// <param name="settlementBan">Whether a settlement ban was imposed.</param>
    /// <param name="banDuration">Ban duration in days (0 = permanent).</param>
    /// <param name="questFailed">Whether a quest failed.</param>
    /// <param name="failedQuestId">ID of the failed quest.</param>
    /// <param name="narrativeText">Flavor text for the fumble.</param>
    /// <returns>A fumble DeceptionResult with [Lie Exposed] consequences.</returns>
    public static DeceptionResult Fumble(
        int playerSuccesses,
        int npcSuccesses,
        int stressCost,
        FumbleConsequence fumbleConsequence,
        bool combatInitiated,
        bool settlementBan,
        int banDuration,
        bool questFailed,
        string? failedQuestId,
        string? narrativeText = null)
    {
        return new DeceptionResult(
            Outcome: SkillOutcome.CriticalFailure,
            LieBelieved: false,
            PlayerSuccesses: playerSuccesses,
            NpcSuccesses: npcSuccesses,
            Margin: playerSuccesses - npcSuccesses,
            NpcSuspicionChange: 2, // NPC is now very suspicious
            StressCost: stressCost,
            DispositionChange: -30,
            UnlockedOptions: Array.Empty<string>(),
            LockedOptions: new[] { "*" }, // All deception locked
            FumbleConsequence: fumbleConsequence,
            CombatInitiated: combatInitiated,
            SettlementBan: settlementBan,
            SettlementBanDuration: banDuration,
            QuestFailed: questFailed,
            FailedQuestId: failedQuestId,
            UntrustworthyApplied: true,
            NarrativeText: narrativeText ?? GetDefaultFumbleNarrative(combatInitiated, settlementBan, banDuration));
    }

    private static string GetDefaultSuccessNarrative(SkillOutcome outcome)
    {
        return outcome switch
        {
            SkillOutcome.CriticalSuccess =>
                "Your lie is delivered flawlessly. The NPC believes you completely and may even embellish your story to others.",
            SkillOutcome.ExceptionalSuccess =>
                "Your deception is utterly convincing. The NPC has no reason to doubt you.",
            SkillOutcome.FullSuccess =>
                "The NPC accepts your story without question.",
            SkillOutcome.MarginalSuccess =>
                "Your lie lands, but perhaps a bit awkwardly. The NPC seems to believe you... for now.",
            _ => "The NPC believes your story."
        };
    }

    private static string GetDefaultFumbleNarrative(bool combatInitiated, bool settlementBan, int banDuration)
    {
        var parts = new List<string>
        {
            "Your lie falls apart spectacularly. The NPC sees through you completely."
        };

        if (combatInitiated)
            parts.Add("Enraged, they attack!");
        else if (settlementBan)
            parts.Add($"You are banished from the settlement{(banDuration > 0 ? $" for {banDuration} days" : " permanently")}.");

        parts.Add("Word of your dishonesty will spread.");

        return string.Join(" ", parts);
    }

    /// <summary>
    /// Gets a summary of the result for display.
    /// </summary>
    /// <returns>A multi-line string summarizing the deception outcome.</returns>
    public string ToSummary()
    {
        var lines = new List<string>
        {
            $"Deception: {Outcome}"
        };

        if (LieBelieved)
        {
            lines.Add("Lie BELIEVED");
        }
        else
        {
            lines.Add("Lie DETECTED");
        }

        lines.Add($"Roll: {PlayerSuccesses} vs {NpcSuccesses} (Margin: {Margin:+#;-#;0})");
        lines.Add($"Liar's Burden: +{StressCost} Psychic Stress");

        if (DispositionChange != 0)
        {
            var sign = DispositionChange > 0 ? "+" : "";
            lines.Add($"Disposition: {sign}{DispositionChange}");
        }

        if (NpcNowSuspicious)
        {
            lines.Add("NPC is now [Suspicious]");
        }

        if (CombatInitiated)
        {
            lines.Add("COMBAT INITIATED!");
        }

        if (SettlementBan)
        {
            var duration = SettlementBanDuration > 0 ? $"{SettlementBanDuration} days" : "PERMANENT";
            lines.Add($"Settlement Ban: {duration}");
        }

        if (QuestFailed)
        {
            lines.Add($"Quest Failed: {FailedQuestId}");
        }

        if (UntrustworthyApplied)
        {
            lines.Add("[Untrustworthy] flag applied");
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <inheritdoc/>
    public override string ToString() => ToSummary();
}
