namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Application.DTOs;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Service for managing defensive combat actions (Block, Dodge, Parry).
/// </summary>
/// <remarks>
/// <para>IDefenseActionService provides:</para>
/// <list type="bullet">
///   <item><description>Eligibility checking for each defense type</description></item>
///   <item><description>Execution of defense actions with appropriate mechanics</description></item>
///   <item><description>Reaction management (consumption and reset)</description></item>
/// </list>
/// <para>Defense actions allow combatants to react to incoming attacks:</para>
/// <list type="bullet">
///   <item><description><see cref="DefenseActionType.Block"/>: Reduces damage with shield (no reaction cost)</description></item>
///   <item><description><see cref="DefenseActionType.Dodge"/>: Avoids attack entirely (costs reaction)</description></item>
///   <item><description><see cref="DefenseActionType.Parry"/>: Deflects and counter-attacks (costs reaction)</description></item>
/// </list>
/// </remarks>
public interface IDefenseActionService
{
    // ═══════════════════════════════════════════════════════════════
    // ELIGIBILITY CHECKS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if a combatant can use the Block defense action.
    /// </summary>
    /// <param name="combatant">The combatant to check.</param>
    /// <returns>True if the combatant has a shield equipped; otherwise, false.</returns>
    /// <remarks>
    /// Block requires a shield equipped. It does not consume a reaction.
    /// </remarks>
    bool CanBlock(Combatant combatant);

    /// <summary>
    /// Checks if a combatant can use the Dodge defense action.
    /// </summary>
    /// <param name="combatant">The combatant to check.</param>
    /// <returns>True if the combatant has a reaction available and is not wearing heavy armor; otherwise, false.</returns>
    /// <remarks>
    /// Dodge requires:
    /// <list type="bullet">
    ///   <item><description>Reaction available</description></item>
    ///   <item><description>Not wearing heavy armor</description></item>
    /// </list>
    /// </remarks>
    bool CanDodge(Combatant combatant);

    /// <summary>
    /// Checks if a combatant can use the Parry defense action.
    /// </summary>
    /// <param name="combatant">The combatant to check.</param>
    /// <returns>True if the combatant has a reaction available and a melee weapon equipped; otherwise, false.</returns>
    /// <remarks>
    /// Parry requires:
    /// <list type="bullet">
    ///   <item><description>Reaction available</description></item>
    ///   <item><description>Melee weapon equipped</description></item>
    /// </list>
    /// </remarks>
    bool CanParry(Combatant combatant);

    // ═══════════════════════════════════════════════════════════════
    // DEFENSE ACTIONS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Executes a Block defense action against incoming damage.
    /// </summary>
    /// <param name="combatant">The combatant performing the block.</param>
    /// <param name="incomingDamage">The damage amount to block.</param>
    /// <returns>The result of the block action, including reduced damage.</returns>
    /// <remarks>
    /// <para>Block reduces damage by 50% plus the shield's defense bonus.</para>
    /// <para>Block does not consume a reaction and can be used multiple times per round.</para>
    /// <para>If the combatant cannot block, returns a failed result.</para>
    /// </remarks>
    BlockResult UseBlock(Combatant combatant, int incomingDamage);

    /// <summary>
    /// Executes a Dodge defense action against an incoming attack.
    /// </summary>
    /// <param name="combatant">The combatant performing the dodge.</param>
    /// <param name="attackRoll">The attacker's attack roll to dodge against.</param>
    /// <returns>The result of the dodge attempt.</returns>
    /// <remarks>
    /// <para>Dodge rolls 1d20 + DEX modifier against the attack roll.</para>
    /// <para>On success (roll >= attack), the attack is completely avoided.</para>
    /// <para>Dodge consumes the combatant's reaction for the round.</para>
    /// <para>If the combatant cannot dodge, returns a not-allowed result.</para>
    /// </remarks>
    DodgeResult UseDodge(Combatant combatant, int attackRoll);

    /// <summary>
    /// Executes a Parry defense action against an incoming attack.
    /// </summary>
    /// <param name="combatant">The combatant performing the parry.</param>
    /// <param name="attacker">The attacker being parried.</param>
    /// <param name="attackRoll">The attacker's attack roll to parry against.</param>
    /// <returns>The result of the parry attempt, including counter-attack if successful.</returns>
    /// <remarks>
    /// <para>Parry rolls 1d20 + DEX modifier against the attack roll + DC bonus (+2).</para>
    /// <para>On success (roll >= DC), the attack is deflected and a counter-attack is made.</para>
    /// <para>Parry consumes the combatant's reaction for the round.</para>
    /// <para>If the combatant cannot parry, returns a not-allowed result.</para>
    /// </remarks>
    ParryResult UseParry(Combatant combatant, Combatant attacker, int attackRoll);

    // ═══════════════════════════════════════════════════════════════
    // REACTION MANAGEMENT
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if a combatant has their reaction available.
    /// </summary>
    /// <param name="combatant">The combatant to check.</param>
    /// <returns>True if the combatant has a reaction available; otherwise, false.</returns>
    bool HasReaction(Combatant combatant);

    /// <summary>
    /// Resets a combatant's reaction availability.
    /// </summary>
    /// <param name="combatant">The combatant whose reaction to reset.</param>
    /// <remarks>
    /// Called at the start of a combatant's turn to restore their reaction.
    /// </remarks>
    void ResetReaction(Combatant combatant);

    // ═══════════════════════════════════════════════════════════════
    // UTILITY
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the list of defense actions available to a combatant.
    /// </summary>
    /// <param name="combatant">The combatant to check.</param>
    /// <returns>A list of defense action types the combatant can currently use.</returns>
    /// <remarks>
    /// Returns only defense actions for which the combatant meets all requirements.
    /// </remarks>
    IReadOnlyList<DefenseActionType> GetAvailableDefenses(Combatant combatant);
}
