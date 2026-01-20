namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Provides a hint for the next ability needed to continue an in-progress combo.
/// </summary>
/// <remarks>
/// <para>ComboHint is used by the UI to show players which abilities will advance their active combos:</para>
/// <list type="bullet">
///   <item><description><see cref="ComboId"/> and <see cref="ComboName"/> - Identifies the combo</description></item>
///   <item><description><see cref="NextAbilityId"/> - The ability needed for the next step</description></item>
///   <item><description><see cref="CurrentStep"/> and <see cref="TotalSteps"/> - Progress tracking</description></item>
///   <item><description><see cref="WindowRemaining"/> - Turns left to complete</description></item>
/// </list>
/// <para>
/// Hints are generated from active <see cref="Tracking.ComboProgress"/> instances
/// and include computed properties for UI display like <see cref="ProgressPercent"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Get hints for the current combatant
/// var hints = comboService.GetComboHints(combatant);
/// foreach (var hint in hints)
/// {
///     Console.WriteLine($"{hint.ComboName}: Use {hint.NextAbilityId}! ({hint.ProgressPercent}% complete)");
/// }
/// </code>
/// </example>
public record ComboHint
{
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique identifier of the combo.
    /// </summary>
    /// <remarks>
    /// <para>Matches <see cref="Domain.Definitions.ComboDefinition.ComboId"/>.</para>
    /// </remarks>
    public string ComboId { get; init; } = null!;

    /// <summary>
    /// Gets the display name of the combo.
    /// </summary>
    /// <remarks>
    /// <para>Suitable for display in UI.</para>
    /// </remarks>
    public string ComboName { get; init; } = null!;

    /// <summary>
    /// Gets the ability ID needed for the next step.
    /// </summary>
    /// <remarks>
    /// <para>The player should use this ability to advance the combo.</para>
    /// <para>Can be used to highlight or suggest abilities in the UI.</para>
    /// </remarks>
    public string NextAbilityId { get; init; } = null!;

    /// <summary>
    /// Gets the most recently completed step number (1-indexed).
    /// </summary>
    /// <remarks>
    /// <para>This is the step that has been completed, not the step to complete next.</para>
    /// <para>For example, if CurrentStep is 2, the player has completed steps 1 and 2,
    /// and needs to complete step 3.</para>
    /// </remarks>
    public int CurrentStep { get; init; }

    /// <summary>
    /// Gets the total number of steps in the combo.
    /// </summary>
    public int TotalSteps { get; init; }

    /// <summary>
    /// Gets the number of turns remaining to complete the combo.
    /// </summary>
    /// <remarks>
    /// <para>When this reaches 0, the combo will fail.</para>
    /// <para>Used to create urgency indicators in the UI.</para>
    /// </remarks>
    public int WindowRemaining { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the progress percentage (0-100).
    /// </summary>
    /// <remarks>
    /// <para>Calculated as (CurrentStep / TotalSteps) * 100.</para>
    /// <para>Useful for progress bar UI elements.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // With CurrentStep=2, TotalSteps=3
    /// var percent = hint.ProgressPercent; // 66
    /// </code>
    /// </example>
    public int ProgressPercent => TotalSteps > 0
        ? (int)((CurrentStep / (double)TotalSteps) * 100)
        : 0;

    /// <summary>
    /// Gets whether the combo is urgent (1 turn remaining).
    /// </summary>
    /// <remarks>
    /// <para>Can be used to display warning indicators in the UI.</para>
    /// </remarks>
    public bool IsUrgent => WindowRemaining <= 1;

    /// <summary>
    /// Gets the next step number that needs to be completed.
    /// </summary>
    /// <remarks>
    /// <para>Equal to CurrentStep + 1.</para>
    /// </remarks>
    public int NextStep => CurrentStep + 1;

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a combo hint from tracking progress and combo definition.
    /// </summary>
    /// <param name="progress">The active combo progress.</param>
    /// <param name="nextAbilityId">The ability ID for the next step.</param>
    /// <returns>A new ComboHint instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when progress is null.</exception>
    /// <remarks>
    /// <para>This is the primary factory method for creating hints during combo tracking.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var progress = comboService.GetActiveProgress(combatant).First();
    /// var nextAbility = combo.GetAbilityForStep(progress.NextStep);
    /// var hint = ComboHint.FromProgress(progress, nextAbility);
    /// </code>
    /// </example>
    public static ComboHint FromProgress(Tracking.ComboProgress progress, string nextAbilityId)
    {
        ArgumentNullException.ThrowIfNull(progress);

        return new ComboHint
        {
            ComboId = progress.ComboId,
            ComboName = progress.ComboName,
            NextAbilityId = nextAbilityId ?? string.Empty,
            CurrentStep = progress.CurrentStep,
            TotalSteps = progress.TotalSteps,
            WindowRemaining = progress.WindowRemaining
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // DISPLAY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a human-readable display string for this hint.
    /// </summary>
    /// <returns>A string suitable for display in the combat log or UI.</returns>
    /// <example>
    /// <code>
    /// var display = hint.GetDisplayString();
    /// // "Elemental Burst: Use ice-shard (2/3, 2 turns left)"
    /// </code>
    /// </example>
    public string GetDisplayString()
    {
        var urgentMarker = IsUrgent ? " [URGENT]" : "";
        return $"{ComboName}: Use {NextAbilityId} ({CurrentStep}/{TotalSteps}, {WindowRemaining} turn{(WindowRemaining != 1 ? "s" : "")} left){urgentMarker}";
    }

    /// <summary>
    /// Gets a short display string for compact UI elements.
    /// </summary>
    /// <returns>A compact string showing the combo name and next ability.</returns>
    /// <example>
    /// <code>
    /// var short = hint.GetShortDisplayString();
    /// // "Elemental Burst -> ice-shard"
    /// </code>
    /// </example>
    public string GetShortDisplayString()
    {
        return $"{ComboName} -> {NextAbilityId}";
    }
}
