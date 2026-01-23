namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Entities;

/// <summary>
/// Result of evaluating master abilities for a skill check.
/// </summary>
/// <remarks>
/// <para>
/// This result tells the skill check service how master abilities affect the check:
/// </para>
/// <list type="bullet">
///   <item><description>If <see cref="ShouldAutoSucceed"/> is true, skip the roll</description></item>
///   <item><description>Otherwise, apply <see cref="TotalDiceBonus"/> to the pool</description></item>
///   <item><description><see cref="ActiveSpecialEffects"/> are passed to skill subsystems</description></item>
/// </list>
/// </remarks>
/// <param name="ShouldAutoSucceed">Whether the check auto-succeeds without rolling.</param>
/// <param name="AutoSucceedAbility">The ability that triggered auto-succeed, if any.</param>
/// <param name="TotalDiceBonus">Sum of dice bonuses from applicable abilities.</param>
/// <param name="DiceBonusAbilities">Abilities contributing dice bonuses.</param>
/// <param name="ActiveSpecialEffects">Special effect strings for skill subsystem handling.</param>
/// <param name="CanReroll">Whether a re-roll ability is available if the check fails.</param>
/// <param name="RerollAbilityId">The ID of the available re-roll ability, if any.</param>
public readonly record struct MasterAbilityEvaluationResult(
    bool ShouldAutoSucceed = false,
    MasterAbility? AutoSucceedAbility = null,
    int TotalDiceBonus = 0,
    IReadOnlyList<MasterAbility>? DiceBonusAbilities = null,
    IReadOnlyList<string>? ActiveSpecialEffects = null,
    bool CanReroll = false,
    string? RerollAbilityId = null)
{
    /// <summary>
    /// A result indicating no master abilities apply.
    /// </summary>
    public static MasterAbilityEvaluationResult None => new();

    /// <summary>
    /// Creates a result indicating the check auto-succeeds.
    /// </summary>
    /// <param name="ability">The ability triggering auto-succeed.</param>
    /// <returns>An auto-succeed evaluation result.</returns>
    public static MasterAbilityEvaluationResult ForAutoSucceed(MasterAbility ability)
        => new(
            ShouldAutoSucceed: true,
            AutoSucceedAbility: ability);

    /// <summary>
    /// Whether any master abilities affect this check.
    /// </summary>
    public bool HasActiveAbilities =>
        ShouldAutoSucceed || TotalDiceBonus > 0 ||
        (ActiveSpecialEffects?.Count ?? 0) > 0 || CanReroll;

    /// <summary>
    /// Gets the trigger message for auto-succeed, if applicable.
    /// </summary>
    public string? AutoSucceedMessage =>
        ShouldAutoSucceed ? AutoSucceedAbility?.TriggerMessage : null;

    /// <summary>
    /// Gets all abilities that contributed to this evaluation.
    /// </summary>
    public IEnumerable<MasterAbility> AllContributingAbilities
    {
        get
        {
            if (AutoSucceedAbility != null)
                yield return AutoSucceedAbility;

            if (DiceBonusAbilities != null)
            {
                foreach (var ability in DiceBonusAbilities)
                    yield return ability;
            }
        }
    }
}
