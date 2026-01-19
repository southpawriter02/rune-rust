using RuneAndRust.Domain.Definitions;

namespace RuneAndRust.Application.Tracking;

/// <summary>
/// Tracks the progress of a combo during combat.
/// </summary>
/// <remarks>
/// <para>ComboProgress maintains the state of an in-progress combo attempt:</para>
/// <list type="bullet">
///   <item><description><see cref="ComboId"/> - The combo being tracked</description></item>
///   <item><description><see cref="CurrentStep"/> - The last completed step (1-indexed)</description></item>
///   <item><description><see cref="WindowRemaining"/> - Turns remaining to complete the combo</description></item>
///   <item><description><see cref="LastTargetId"/> - The last target hit (for SameTarget/DifferentTarget checks)</description></item>
///   <item><description><see cref="AllTargetsHit"/> - All targets hit during the combo (for AllHitTargets bonus)</description></item>
/// </list>
/// <para>
/// Instances are created via <see cref="Start"/> when a combo begins, and updated
/// via <see cref="AdvanceStep"/> as abilities are used. The combo fails if
/// <see cref="WindowRemaining"/> reaches zero before completion.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Start tracking a combo
/// var progress = ComboProgress.Start(combo, targetId);
///
/// // Advance after successful step
/// progress.AdvanceStep(targetId);
///
/// // Decrement window at end of turn
/// progress.DecrementWindow();
///
/// // Check progress status
/// var display = progress.GetProgressString(); // "2/3 steps"
/// </code>
/// </example>
public class ComboProgress
{
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique identifier of the combo being tracked.
    /// </summary>
    /// <remarks>
    /// <para>Matches <see cref="ComboDefinition.ComboId"/> of the tracked combo.</para>
    /// <para>Used to look up the combo definition during validation.</para>
    /// </remarks>
    public string ComboId { get; private set; } = null!;

    /// <summary>
    /// Gets the display name of the combo being tracked.
    /// </summary>
    /// <remarks>
    /// <para>Cached for display purposes without requiring a definition lookup.</para>
    /// </remarks>
    public string ComboName { get; private set; } = null!;

    /// <summary>
    /// Gets the total number of steps in the combo.
    /// </summary>
    /// <remarks>
    /// <para>Cached from <see cref="ComboDefinition.StepCount"/> for progress display.</para>
    /// </remarks>
    public int TotalSteps { get; private set; }

    /// <summary>
    /// Gets the most recently completed step number (1-indexed).
    /// </summary>
    /// <remarks>
    /// <para>After starting a combo, this is 1 (first step complete).</para>
    /// <para>When this equals <see cref="TotalSteps"/>, the combo is complete.</para>
    /// </remarks>
    public int CurrentStep { get; private set; }

    /// <summary>
    /// Gets the number of turns remaining to complete the combo.
    /// </summary>
    /// <remarks>
    /// <para>Initialized from <see cref="ComboDefinition.WindowTurns"/>.</para>
    /// <para>Decremented each turn by <see cref="DecrementWindow"/>.</para>
    /// <para>When this reaches zero, the combo fails.</para>
    /// </remarks>
    public int WindowRemaining { get; private set; }

    /// <summary>
    /// Gets the identifier of the last target hit during this combo.
    /// </summary>
    /// <remarks>
    /// <para>Used to validate <see cref="Domain.Enums.ComboTargetRequirement.SameTarget"/>
    /// and <see cref="Domain.Enums.ComboTargetRequirement.DifferentTarget"/> requirements.</para>
    /// <para>May be null for self-only abilities.</para>
    /// </remarks>
    public Guid? LastTargetId { get; private set; }

    /// <summary>
    /// Gets the timestamp when this combo was started.
    /// </summary>
    /// <remarks>
    /// <para>Useful for logging and diagnostics.</para>
    /// </remarks>
    public DateTime StartedAt { get; private set; }

    /// <summary>
    /// Gets all targets hit during this combo.
    /// </summary>
    /// <remarks>
    /// <para>Used for <see cref="Domain.Enums.ComboBonusTarget.AllHitTargets"/> bonus effects.</para>
    /// <para>Contains unique target IDs (no duplicates).</para>
    /// </remarks>
    public IReadOnlyList<Guid> AllTargetsHit => _allTargetsHit;

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Backing list for <see cref="AllTargetsHit"/>.
    /// </summary>
    private readonly List<Guid> _allTargetsHit = [];

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Private constructor to enforce factory method usage.
    /// </summary>
    private ComboProgress()
    {
    }

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Starts tracking a new combo attempt.
    /// </summary>
    /// <param name="combo">The combo definition to track.</param>
    /// <param name="targetId">The target of the first ability (null for self-targeted).</param>
    /// <returns>A new ComboProgress instance with step 1 completed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when combo is null.</exception>
    /// <remarks>
    /// <para>Initializes the progress tracker with the first step completed.</para>
    /// <para>The window is set to the combo's <see cref="ComboDefinition.WindowTurns"/>.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var combo = comboProvider.GetCombo("elemental-burst");
    /// var progress = ComboProgress.Start(combo, targetId);
    /// // progress.CurrentStep == 1, progress.WindowRemaining == combo.WindowTurns
    /// </code>
    /// </example>
    public static ComboProgress Start(ComboDefinition combo, Guid? targetId)
    {
        ArgumentNullException.ThrowIfNull(combo);

        var progress = new ComboProgress
        {
            ComboId = combo.ComboId,
            ComboName = combo.Name,
            TotalSteps = combo.StepCount,
            CurrentStep = 1,
            WindowRemaining = combo.WindowTurns,
            LastTargetId = targetId,
            StartedAt = DateTime.UtcNow
        };

        // Track the first target if present
        if (targetId.HasValue)
        {
            progress._allTargetsHit.Add(targetId.Value);
        }

        return progress;
    }

    // ═══════════════════════════════════════════════════════════════
    // PUBLIC METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Advances the combo to the next step.
    /// </summary>
    /// <param name="targetId">The target of the ability (null for self-targeted).</param>
    /// <remarks>
    /// <para>Call this after validating that the ability matches the next step.</para>
    /// <para>Updates <see cref="CurrentStep"/>, <see cref="LastTargetId"/>,
    /// and <see cref="AllTargetsHit"/>.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // After validating step 2 matches
    /// progress.AdvanceStep(targetId);
    /// // progress.CurrentStep is now 2
    /// </code>
    /// </example>
    public void AdvanceStep(Guid? targetId)
    {
        CurrentStep++;
        LastTargetId = targetId;

        // Track unique targets
        if (targetId.HasValue && !_allTargetsHit.Contains(targetId.Value))
        {
            _allTargetsHit.Add(targetId.Value);
        }
    }

    /// <summary>
    /// Decrements the remaining window by one turn.
    /// </summary>
    /// <remarks>
    /// <para>Call this at the end of each turn to track combo expiration.</para>
    /// <para>When <see cref="WindowRemaining"/> reaches zero, the combo has failed.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// progress.DecrementWindow();
    /// if (progress.WindowRemaining == 0)
    /// {
    ///     // Combo expired - remove from tracking
    /// }
    /// </code>
    /// </example>
    public void DecrementWindow()
    {
        if (WindowRemaining > 0)
        {
            WindowRemaining--;
        }
    }

    /// <summary>
    /// Checks if the combo has been completed.
    /// </summary>
    /// <returns>True if all steps have been completed; otherwise, false.</returns>
    /// <remarks>
    /// <para>A combo is complete when <see cref="CurrentStep"/> equals <see cref="TotalSteps"/>.</para>
    /// </remarks>
    public bool IsComplete => CurrentStep >= TotalSteps;

    /// <summary>
    /// Checks if the combo has expired (window depleted without completion).
    /// </summary>
    /// <returns>True if the window has expired; otherwise, false.</returns>
    /// <remarks>
    /// <para>A combo expires when <see cref="WindowRemaining"/> reaches zero
    /// before <see cref="IsComplete"/> becomes true.</para>
    /// </remarks>
    public bool IsExpired => WindowRemaining <= 0 && !IsComplete;

    /// <summary>
    /// Gets the next step number that needs to be completed.
    /// </summary>
    /// <returns>The next step number (1-indexed), or 0 if the combo is complete.</returns>
    /// <remarks>
    /// <para>Returns <see cref="CurrentStep"/> + 1 if the combo is not complete.</para>
    /// <para>Returns 0 if <see cref="IsComplete"/> is true.</para>
    /// </remarks>
    public int NextStep => IsComplete ? 0 : CurrentStep + 1;

    /// <summary>
    /// Gets a human-readable progress string.
    /// </summary>
    /// <returns>A string in the format "X/Y steps".</returns>
    /// <remarks>
    /// <para>Suitable for display in UI or combat log.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var display = progress.GetProgressString(); // "2/3 steps"
    /// </code>
    /// </example>
    public string GetProgressString()
    {
        return $"{CurrentStep}/{TotalSteps} steps";
    }

    /// <summary>
    /// Returns a string representation of this combo progress.
    /// </summary>
    /// <returns>A string showing the combo name and progress.</returns>
    public override string ToString()
    {
        return $"{ComboName}: {GetProgressString()} ({WindowRemaining} turns remaining)";
    }
}
