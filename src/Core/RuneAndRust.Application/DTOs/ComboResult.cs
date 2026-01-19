using RuneAndRust.Domain.Definitions;

namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Aggregates all combo-related results from processing an ability use.
/// </summary>
/// <remarks>
/// <para>ComboResult is returned from <see cref="Interfaces.IComboService.OnAbilityUsed"/>
/// and contains:</para>
/// <list type="bullet">
///   <item><description><see cref="Actions"/> - Individual combo state changes (started, progressed, completed, failed)</description></item>
///   <item><description><see cref="CompletedCombos"/> - Full combo definitions for completed combos (for applying bonuses)</description></item>
/// </list>
/// <para>
/// A single ability use can trigger multiple actions. For example, using "fire-bolt"
/// might start a new "Elemental Burst" combo while simultaneously failing an expired
/// "Warrior's Onslaught" combo.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var result = comboService.OnAbilityUsed(user, "fire-bolt", target);
///
/// // Log all actions
/// foreach (var action in result.Actions)
/// {
///     combatLog.Add(action.GetDescription());
/// }
///
/// // Apply bonuses for completed combos
/// foreach (var combo in result.CompletedCombos)
/// {
///     ApplyBonusEffects(user, combo, target);
/// }
/// </code>
/// </example>
public record ComboResult
{
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets all combo actions that occurred during ability processing.
    /// </summary>
    /// <remarks>
    /// <para>May contain multiple actions from a single ability use:</para>
    /// <list type="bullet">
    ///   <item><description>New combos started</description></item>
    ///   <item><description>Existing combos progressed</description></item>
    ///   <item><description>Combos completed</description></item>
    ///   <item><description>Combos failed (expired or wrong ability)</description></item>
    /// </list>
    /// <para>Actions are ordered by type: completions first, then progressions, then starts, then failures.</para>
    /// </remarks>
    public IReadOnlyList<ComboActionResult> Actions { get; init; } = [];

    /// <summary>
    /// Gets the combo definitions for all combos that were completed.
    /// </summary>
    /// <remarks>
    /// <para>Contains full <see cref="ComboDefinition"/> objects for applying bonus effects.</para>
    /// <para>Empty if no combos were completed.</para>
    /// </remarks>
    public IReadOnlyList<ComboDefinition> CompletedCombos { get; init; } = [];

    // ═══════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether any combo actions occurred.
    /// </summary>
    /// <remarks>
    /// <para>False when the ability didn't affect any combos.</para>
    /// </remarks>
    public bool HasActions => Actions.Count > 0;

    /// <summary>
    /// Gets whether any combos were completed.
    /// </summary>
    /// <remarks>
    /// <para>True when at least one combo reached its final step.</para>
    /// </remarks>
    public bool HasCompletions => CompletedCombos.Count > 0;

    /// <summary>
    /// Gets the count of combos that were started.
    /// </summary>
    public int StartedCount => Actions.Count(a => a.ActionType == ComboActionType.Started);

    /// <summary>
    /// Gets the count of combos that progressed.
    /// </summary>
    public int ProgressedCount => Actions.Count(a => a.ActionType == ComboActionType.Progressed);

    /// <summary>
    /// Gets the count of combos that were completed.
    /// </summary>
    public int CompletedCount => CompletedCombos.Count;

    /// <summary>
    /// Gets the count of combos that failed.
    /// </summary>
    public int FailedCount => Actions.Count(a => a.ActionType == ComboActionType.Failed);

    // ═══════════════════════════════════════════════════════════════
    // STATIC INSTANCES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets an empty result indicating no combo activity.
    /// </summary>
    /// <remarks>
    /// <para>Use this when an ability doesn't affect any combos.</para>
    /// </remarks>
    public static ComboResult None => new() { Actions = [], CompletedCombos = [] };

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a combo result with the specified actions and completed combos.
    /// </summary>
    /// <param name="actions">The list of combo actions.</param>
    /// <param name="completedCombos">The list of completed combo definitions.</param>
    /// <returns>A new ComboResult instance.</returns>
    /// <remarks>
    /// <para>Actions are automatically ordered by priority.</para>
    /// </remarks>
    public static ComboResult Create(
        IEnumerable<ComboActionResult> actions,
        IEnumerable<ComboDefinition>? completedCombos = null)
    {
        return new ComboResult
        {
            Actions = actions?.ToList() ?? [],
            CompletedCombos = completedCombos?.ToList() ?? []
        };
    }

    /// <summary>
    /// Creates a result with a single started combo.
    /// </summary>
    /// <param name="comboId">The combo identifier.</param>
    /// <param name="name">The combo display name.</param>
    /// <param name="totalSteps">The total number of steps.</param>
    /// <returns>A ComboResult with one started action.</returns>
    public static ComboResult Started(string comboId, string name, int totalSteps)
    {
        return new ComboResult
        {
            Actions = [ComboActionResult.Started(comboId, name, totalSteps)],
            CompletedCombos = []
        };
    }

    /// <summary>
    /// Creates a result with a single completed combo.
    /// </summary>
    /// <param name="combo">The completed combo definition.</param>
    /// <returns>A ComboResult with completion action and the combo definition.</returns>
    public static ComboResult Completed(ComboDefinition combo)
    {
        return new ComboResult
        {
            Actions = [ComboActionResult.Completed(combo.ComboId, combo.Name, combo.StepCount)],
            CompletedCombos = [combo]
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // DISPLAY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a summary string of all combo activity.
    /// </summary>
    /// <returns>A summary suitable for logging.</returns>
    /// <example>
    /// <code>
    /// var summary = result.GetSummary();
    /// // "1 started, 1 progressed, 1 completed, 0 failed"
    /// </code>
    /// </example>
    public string GetSummary()
    {
        if (!HasActions)
        {
            return "No combo activity";
        }

        return $"{StartedCount} started, {ProgressedCount} progressed, {CompletedCount} completed, {FailedCount} failed";
    }

    /// <summary>
    /// Gets all action descriptions as a list.
    /// </summary>
    /// <returns>A list of description strings.</returns>
    public IReadOnlyList<string> GetActionDescriptions()
    {
        return Actions.Select(a => a.GetDescription()).ToList();
    }
}
