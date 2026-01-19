using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Tracking;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for detecting and tracking ability combos during combat.
/// </summary>
/// <remarks>
/// <para>IComboService manages the combo detection system, which rewards players for executing
/// specific sequences of abilities within a time window. Key responsibilities:</para>
/// <list type="bullet">
///   <item><description><see cref="OnAbilityUsed"/> - Process ability usage and update combo tracking</description></item>
///   <item><description><see cref="GetActiveProgress"/> - Retrieve in-progress combos for a combatant</description></item>
///   <item><description><see cref="GetComboHints"/> - Get UI hints for completing active combos</description></item>
///   <item><description><see cref="TickCombos"/> - Decrement combo windows at end of turn</description></item>
///   <item><description><see cref="ResetProgress"/> - Clear all combo progress (e.g., at combat end)</description></item>
/// </list>
/// <para>
/// The service maintains scoped state per combat session, tracking multiple in-progress combos
/// per combatant. Combos are automatically started when the first ability in a sequence is used,
/// and progress is tracked until completion or window expiration.
/// </para>
/// <para>
/// Combo definitions are loaded from <see cref="IComboProvider"/> (v0.10.3a) and include
/// class restrictions, target requirements, and bonus effects.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Process an ability use
/// var result = comboService.OnAbilityUsed(player, "fire-bolt", target);
///
/// // Check if any combos completed
/// if (result.HasCompletions)
/// {
///     foreach (var combo in result.CompletedCombos)
///     {
///         combatLog.Add($"Completed {combo.Name}!");
///     }
/// }
///
/// // Get hints for the UI
/// var hints = comboService.GetComboHints(player);
/// foreach (var hint in hints)
/// {
///     ui.ShowComboHint(hint);
/// }
///
/// // At end of turn
/// comboService.TickCombos(player);
/// </code>
/// </example>
public interface IComboService
{
    /// <summary>
    /// Processes an ability use and updates combo tracking.
    /// </summary>
    /// <param name="user">The combatant who used the ability.</param>
    /// <param name="abilityId">The identifier of the ability used.</param>
    /// <param name="target">The target of the ability, or null for self-targeted abilities.</param>
    /// <returns>A result containing all combo actions that occurred.</returns>
    /// <exception cref="ArgumentNullException">Thrown when user or abilityId is null.</exception>
    /// <remarks>
    /// <para>This method performs the following operations:</para>
    /// <list type="number">
    ///   <item><description>Checks if the ability starts any new combos (using <see cref="IComboProvider.GetCombosStartingWith"/>)</description></item>
    ///   <item><description>Advances any active combos that match the ability and target requirements</description></item>
    ///   <item><description>Applies bonus effects for completed combos</description></item>
    ///   <item><description>Returns a result containing all actions taken</description></item>
    /// </list>
    /// <para>
    /// A single ability use can trigger multiple actions. For example, using "fire-bolt"
    /// might start a new "Elemental Burst" combo while advancing an existing combo.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = comboService.OnAbilityUsed(player, "fire-bolt", target);
    ///
    /// foreach (var action in result.Actions)
    /// {
    ///     Console.WriteLine(action.GetDescription());
    /// }
    /// </code>
    /// </example>
    ComboResult OnAbilityUsed(Combatant user, string abilityId, Combatant? target);

    /// <summary>
    /// Gets all active combo progress for a combatant.
    /// </summary>
    /// <param name="combatant">The combatant to query.</param>
    /// <returns>A read-only list of in-progress combos.</returns>
    /// <exception cref="ArgumentNullException">Thrown when combatant is null.</exception>
    /// <remarks>
    /// <para>Returns an empty list if the combatant has no active combos.</para>
    /// <para>Useful for debugging and detailed UI displays.</para>
    /// </remarks>
    IReadOnlyList<ComboProgress> GetActiveProgress(Combatant combatant);

    /// <summary>
    /// Gets all combo definitions available to a combatant.
    /// </summary>
    /// <param name="combatant">The combatant to query.</param>
    /// <returns>A read-only list of available combo definitions.</returns>
    /// <exception cref="ArgumentNullException">Thrown when combatant is null.</exception>
    /// <remarks>
    /// <para>Filters combos by the combatant's class (if applicable).</para>
    /// <para>Monsters without a class may have no available combos.</para>
    /// </remarks>
    IReadOnlyList<ComboDefinition> GetAvailableCombos(Combatant combatant);

    /// <summary>
    /// Gets UI hints for completing active combos.
    /// </summary>
    /// <param name="combatant">The combatant to query.</param>
    /// <returns>A list of hints showing the next ability needed for each active combo.</returns>
    /// <exception cref="ArgumentNullException">Thrown when combatant is null.</exception>
    /// <remarks>
    /// <para>Returns one hint per active combo.</para>
    /// <para>Hints include the next ability ID, progress percentage, and urgency.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var hints = comboService.GetComboHints(player);
    /// foreach (var hint in hints)
    /// {
    ///     if (hint.IsUrgent)
    ///     {
    ///         ui.HighlightAbility(hint.NextAbilityId, urgent: true);
    ///     }
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<ComboHint> GetComboHints(Combatant combatant);

    /// <summary>
    /// Decrements combo windows at the end of a turn.
    /// </summary>
    /// <param name="combatant">The combatant whose turn ended.</param>
    /// <exception cref="ArgumentNullException">Thrown when combatant is null.</exception>
    /// <remarks>
    /// <para>Call this at the end of each combatant's turn.</para>
    /// <para>Combos whose window reaches zero are automatically failed and removed.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // At end of turn
    /// comboService.TickCombos(currentCombatant);
    /// </code>
    /// </example>
    void TickCombos(Combatant combatant);

    /// <summary>
    /// Clears all combo progress for a combatant.
    /// </summary>
    /// <param name="combatant">The combatant to reset.</param>
    /// <exception cref="ArgumentNullException">Thrown when combatant is null.</exception>
    /// <remarks>
    /// <para>Call this when combat ends or the combatant is removed from combat.</para>
    /// <para>All in-progress combos are discarded without triggering failure events.</para>
    /// </remarks>
    void ResetProgress(Combatant combatant);

    /// <summary>
    /// Checks if a combatant has any active combo progress.
    /// </summary>
    /// <param name="combatant">The combatant to query.</param>
    /// <returns>True if the combatant has at least one in-progress combo; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when combatant is null.</exception>
    /// <remarks>
    /// <para>More efficient than calling <see cref="GetActiveProgress"/> and checking the count.</para>
    /// </remarks>
    bool HasActiveProgress(Combatant combatant);

    /// <summary>
    /// Gets the total number of combos completed by a combatant this combat session.
    /// </summary>
    /// <param name="combatant">The combatant to query.</param>
    /// <returns>The number of completed combos.</returns>
    /// <exception cref="ArgumentNullException">Thrown when combatant is null.</exception>
    /// <remarks>
    /// <para>This counter is reset when <see cref="ResetProgress"/> is called.</para>
    /// <para>Useful for achievements or combat statistics.</para>
    /// </remarks>
    int GetCompletedComboCount(Combatant combatant);
}
