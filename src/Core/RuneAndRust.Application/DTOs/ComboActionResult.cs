namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Represents the result of a single combo-related action.
/// </summary>
/// <remarks>
/// <para>ComboActionResult captures individual combo state changes during combat:</para>
/// <list type="bullet">
///   <item><description><see cref="ComboActionType.Started"/> - A new combo has begun</description></item>
///   <item><description><see cref="ComboActionType.Progressed"/> - An existing combo advanced to the next step</description></item>
///   <item><description><see cref="ComboActionType.Completed"/> - A combo finished successfully</description></item>
///   <item><description><see cref="ComboActionType.Failed"/> - A combo failed (window expired or wrong ability)</description></item>
/// </list>
/// <para>
/// Multiple ComboActionResults may be returned from a single ability use if
/// the ability starts one combo while advancing another.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Check what happened after ability use
/// var result = comboService.OnAbilityUsed(user, "fire-bolt", target);
/// foreach (var action in result.Actions)
/// {
///     switch (action.ActionType)
///     {
///         case ComboActionType.Started:
///             Console.WriteLine($"Started combo: {action.ComboName}");
///             break;
///         case ComboActionType.Completed:
///             Console.WriteLine($"Completed combo: {action.ComboName}!");
///             break;
///     }
/// }
/// </code>
/// </example>
public record ComboActionResult
{
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the type of combo action that occurred.
    /// </summary>
    /// <remarks>
    /// <para>Determines how other properties should be interpreted.</para>
    /// </remarks>
    public ComboActionType ActionType { get; init; }

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
    /// <para>Suitable for display in UI or combat log.</para>
    /// </remarks>
    public string ComboName { get; init; } = null!;

    /// <summary>
    /// Gets the current step number (1-indexed).
    /// </summary>
    /// <remarks>
    /// <para>Only relevant for <see cref="ComboActionType.Started"/> and
    /// <see cref="ComboActionType.Progressed"/> actions.</para>
    /// <para>Null for <see cref="ComboActionType.Completed"/> and
    /// <see cref="ComboActionType.Failed"/> actions.</para>
    /// </remarks>
    public int? CurrentStep { get; init; }

    /// <summary>
    /// Gets the total number of steps in the combo.
    /// </summary>
    /// <remarks>
    /// <para>Only relevant for <see cref="ComboActionType.Started"/> and
    /// <see cref="ComboActionType.Progressed"/> actions.</para>
    /// <para>Null for <see cref="ComboActionType.Failed"/> actions.</para>
    /// </remarks>
    public int? TotalSteps { get; init; }

    /// <summary>
    /// Gets the failure reason if the action type is Failed.
    /// </summary>
    /// <remarks>
    /// <para>Only populated for <see cref="ComboActionType.Failed"/> actions.</para>
    /// <para>Describes why the combo failed (e.g., "Window expired", "Wrong ability").</para>
    /// </remarks>
    public string? FailureReason { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a result indicating a new combo has started.
    /// </summary>
    /// <param name="comboId">The combo identifier.</param>
    /// <param name="name">The combo display name.</param>
    /// <param name="totalSteps">The total number of steps in the combo.</param>
    /// <returns>A ComboActionResult for a started combo.</returns>
    /// <remarks>
    /// <para>CurrentStep is set to 1 since the first step has been completed.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = ComboActionResult.Started("elemental-burst", "Elemental Burst", 3);
    /// // result.CurrentStep == 1, result.TotalSteps == 3
    /// </code>
    /// </example>
    public static ComboActionResult Started(string comboId, string name, int totalSteps)
        => new()
        {
            ActionType = ComboActionType.Started,
            ComboId = comboId,
            ComboName = name,
            CurrentStep = 1,
            TotalSteps = totalSteps
        };

    /// <summary>
    /// Creates a result indicating a combo has progressed to the next step.
    /// </summary>
    /// <param name="comboId">The combo identifier.</param>
    /// <param name="name">The combo display name.</param>
    /// <param name="step">The step just completed (1-indexed).</param>
    /// <param name="total">The total number of steps.</param>
    /// <returns>A ComboActionResult for combo progression.</returns>
    /// <example>
    /// <code>
    /// var result = ComboActionResult.Progressed("elemental-burst", "Elemental Burst", 2, 3);
    /// // result.CurrentStep == 2, result.TotalSteps == 3
    /// </code>
    /// </example>
    public static ComboActionResult Progressed(string comboId, string name, int step, int total)
        => new()
        {
            ActionType = ComboActionType.Progressed,
            ComboId = comboId,
            ComboName = name,
            CurrentStep = step,
            TotalSteps = total
        };

    /// <summary>
    /// Creates a result indicating a combo has completed successfully.
    /// </summary>
    /// <param name="comboId">The combo identifier.</param>
    /// <param name="name">The combo display name.</param>
    /// <param name="totalSteps">The total number of steps completed.</param>
    /// <returns>A ComboActionResult for combo completion.</returns>
    /// <example>
    /// <code>
    /// var result = ComboActionResult.Completed("elemental-burst", "Elemental Burst", 3);
    /// </code>
    /// </example>
    public static ComboActionResult Completed(string comboId, string name, int totalSteps)
        => new()
        {
            ActionType = ComboActionType.Completed,
            ComboId = comboId,
            ComboName = name,
            CurrentStep = totalSteps,
            TotalSteps = totalSteps
        };

    /// <summary>
    /// Creates a result indicating a combo has failed.
    /// </summary>
    /// <param name="comboId">The combo identifier.</param>
    /// <param name="name">The combo display name.</param>
    /// <param name="reason">The reason the combo failed.</param>
    /// <returns>A ComboActionResult for combo failure.</returns>
    /// <example>
    /// <code>
    /// var result = ComboActionResult.Failed("elemental-burst", "Elemental Burst", "Window expired");
    /// </code>
    /// </example>
    public static ComboActionResult Failed(string comboId, string name, string reason)
        => new()
        {
            ActionType = ComboActionType.Failed,
            ComboId = comboId,
            ComboName = name,
            FailureReason = reason
        };

    // ═══════════════════════════════════════════════════════════════
    // DISPLAY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a human-readable description of this action.
    /// </summary>
    /// <returns>A description suitable for combat log display.</returns>
    /// <example>
    /// <code>
    /// var desc = result.GetDescription();
    /// // "Started Elemental Burst (1/3 steps)"
    /// // "Progressed Elemental Burst (2/3 steps)"
    /// // "Completed Elemental Burst!"
    /// // "Failed Elemental Burst: Window expired"
    /// </code>
    /// </example>
    public string GetDescription()
    {
        return ActionType switch
        {
            ComboActionType.Started => $"Started {ComboName} ({CurrentStep}/{TotalSteps} steps)",
            ComboActionType.Progressed => $"Progressed {ComboName} ({CurrentStep}/{TotalSteps} steps)",
            ComboActionType.Completed => $"Completed {ComboName}!",
            ComboActionType.Failed => $"Failed {ComboName}: {FailureReason}",
            _ => $"{ComboName}: {ActionType}"
        };
    }
}

/// <summary>
/// Defines the types of combo actions that can occur.
/// </summary>
/// <remarks>
/// <para>Used by <see cref="ComboActionResult"/> to indicate what happened during combo processing.</para>
/// </remarks>
public enum ComboActionType
{
    /// <summary>
    /// A new combo has begun tracking (first step completed).
    /// </summary>
    Started,

    /// <summary>
    /// An existing combo advanced to the next step.
    /// </summary>
    Progressed,

    /// <summary>
    /// A combo was completed successfully (all steps executed).
    /// </summary>
    Completed,

    /// <summary>
    /// A combo failed (window expired or requirements not met).
    /// </summary>
    Failed
}
