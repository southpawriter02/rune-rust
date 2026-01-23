namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service for managing stealth checks and [Hidden] status.
/// </summary>
/// <remarks>
/// <para>
/// The stealth service handles:
/// <list type="bullet">
///   <item><description>Individual stealth checks against surface DCs</description></item>
///   <item><description>Party stealth using weakest-link rule</description></item>
///   <item><description>[Hidden] status tracking and break conditions</description></item>
///   <item><description>[System-Wide Alert] fumble consequences</description></item>
/// </list>
/// </para>
/// </remarks>
public interface IStealthService
{
    /// <summary>
    /// Attempts a stealth check for an individual character.
    /// </summary>
    /// <param name="characterId">The ID of the character attempting stealth.</param>
    /// <param name="context">The stealth context with modifiers.</param>
    /// <param name="dicePool">The dice pool for the stealth check.</param>
    /// <returns>A StealthCheckResult with the attempt outcome.</returns>
    StealthCheckResult AttemptStealth(
        string characterId,
        StealthContext context,
        int dicePool);

    /// <summary>
    /// Attempts a party stealth check using the weakest-link rule.
    /// </summary>
    /// <param name="partyMemberPools">Dictionary of member IDs to their dice pools.</param>
    /// <param name="context">The stealth context with modifiers.</param>
    /// <returns>A PartyStealthResult with the party attempt outcome.</returns>
    PartyStealthResult AttemptPartyStealth(
        IReadOnlyDictionary<string, int> partyMemberPools,
        StealthContext context);

    /// <summary>
    /// Gets the current [Hidden] status for a character.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <returns>The HiddenStatus if character is hidden, null otherwise.</returns>
    HiddenStatus? GetHiddenStatus(string characterId);

    /// <summary>
    /// Checks if a character is currently [Hidden].
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <returns>True if the character has active [Hidden] status.</returns>
    bool IsHidden(string characterId);

    /// <summary>
    /// Breaks the [Hidden] status for a character due to a condition.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <param name="condition">The break condition that occurred.</param>
    /// <returns>True if [Hidden] was broken, false if not hidden.</returns>
    bool BreakHidden(string characterId, HiddenBreakCondition condition);

    /// <summary>
    /// Manually applies [Hidden] status via ability (e.g., [Slip into Shadow]).
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <param name="source">The source of the [Hidden] status.</param>
    /// <param name="detectionModifier">Modifier to enemy detection DCs.</param>
    /// <returns>The created HiddenStatus.</returns>
    HiddenStatus ApplyHiddenStatus(
        string characterId,
        string source,
        int detectionModifier = 0);

    /// <summary>
    /// Gets the stealth DC for a surface type with modifiers.
    /// </summary>
    /// <param name="surface">The surface type.</param>
    /// <param name="isDimLight">Whether in dim lighting.</param>
    /// <param name="isIlluminated">Whether in bright light.</param>
    /// <param name="enemiesAlerted">Whether enemies are alerted.</param>
    /// <returns>The effective DC in successes needed.</returns>
    int GetStealthDc(
        StealthSurface surface,
        bool isDimLight = false,
        bool isIlluminated = false,
        bool enemiesAlerted = false);

    /// <summary>
    /// Finds the party member with the lowest dice pool (weakest link).
    /// </summary>
    /// <param name="partyMemberPools">Dictionary of member IDs to their dice pools.</param>
    /// <returns>Tuple of (weakest member ID, dice pool size).</returns>
    (string MemberId, int DicePool) FindWeakestMember(IReadOnlyDictionary<string, int> partyMemberPools);

    /// <summary>
    /// Clears all [Hidden] statuses (e.g., at end of encounter).
    /// </summary>
    void ClearAllHiddenStatuses();
}
