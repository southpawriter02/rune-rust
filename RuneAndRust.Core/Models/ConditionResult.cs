using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Models;

/// <summary>
/// Result of evaluating a single dialogue condition.
/// </summary>
/// <remarks>See: v0.4.2b (The Lexicon) for Dialogue System implementation.</remarks>
public record ConditionResult(
    bool Passed,
    DialogueConditionType ConditionType,
    string DisplayHint,
    string? FailureReason = null,
    IReadOnlyList<int>? DiceRolls = null,
    int? NetSuccesses = null)
{
    /// <summary>
    /// Creates a successful condition result.
    /// </summary>
    public static ConditionResult Success(DialogueConditionType type, string hint) =>
        new(true, type, hint);

    /// <summary>
    /// Creates a successful skill check result with roll details.
    /// </summary>
    public static ConditionResult SkillCheckSuccess(
        string hint,
        IReadOnlyList<int> rolls,
        int netSuccesses) =>
        new(true, DialogueConditionType.SkillCheck, hint, null, rolls, netSuccesses);

    /// <summary>
    /// Creates a failed condition result.
    /// </summary>
    public static ConditionResult Fail(
        DialogueConditionType type,
        string hint,
        string reason) =>
        new(false, type, hint, reason);

    /// <summary>
    /// Creates a failed skill check result with roll details.
    /// </summary>
    public static ConditionResult SkillCheckFail(
        string hint,
        string reason,
        IReadOnlyList<int> rolls,
        int netSuccesses) =>
        new(false, DialogueConditionType.SkillCheck, hint, reason, rolls, netSuccesses);
}
