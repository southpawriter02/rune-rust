// ═══════════════════════════════════════════════════════════════════════════════
// AttributeModificationResult.cs
// Value object encapsulating the outcome of an attribute modification attempt
// during character creation, including the new state on success or an error
// message on failure.
// Version: 0.17.2f
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using Microsoft.Extensions.Logging;

/// <summary>
/// The result of an attribute modification attempt during character creation.
/// </summary>
/// <remarks>
/// <para>
/// AttributeModificationResult captures the outcome of
/// <c>IAttributeAllocationService.TryModifyAttribute()</c>, including the new
/// allocation state on success or a descriptive error message on failure.
/// This pattern enables callers to check the result without exception handling,
/// following the Try-pattern convention.
/// </para>
/// <para>
/// Use the static factory methods <see cref="Succeeded"/> and <see cref="Failed"/>
/// to create instances rather than using the constructor directly.
/// </para>
/// <para>
/// <strong>Usage Pattern:</strong>
/// <code>
/// var result = allocationService.TryModifyAttribute(state, CoreAttribute.Might, 5);
/// if (result.Success)
/// {
///     currentState = result.NewState!.Value;
///     UpdateUI(result.PointsSpent, result.PointsRemaining);
/// }
/// else
/// {
///     ShowError(result.ErrorMessage);
/// }
/// </code>
/// </para>
/// <para>
/// On success, <see cref="NewState"/> contains the updated allocation state with
/// the modified attribute value and recalculated point totals. On failure,
/// <see cref="NewState"/> is <c>null</c> and <see cref="ErrorMessage"/> describes
/// the reason (e.g., insufficient points, value out of range, Simple mode restriction).
/// </para>
/// </remarks>
/// <seealso cref="AttributeAllocationState"/>
public readonly record struct AttributeModificationResult
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Logger for detailed diagnostic output during result creation.
    /// </summary>
    private static ILogger<AttributeModificationResult>? _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether the modification succeeded.
    /// </summary>
    /// <value>
    /// <c>true</c> if the attribute was modified successfully and
    /// <see cref="NewState"/> contains the updated allocation state;
    /// <c>false</c> if the modification was rejected.
    /// </value>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the new allocation state after modification.
    /// </summary>
    /// <value>
    /// The updated <see cref="AttributeAllocationState"/> when <see cref="Success"/>
    /// is <c>true</c>, or <c>null</c> when the modification failed.
    /// </value>
    /// <remarks>
    /// When successful, the new state reflects the modified attribute value,
    /// updated points spent, and updated points remaining. The original state
    /// is never modified (immutable state transitions).
    /// </remarks>
    public AttributeAllocationState? NewState { get; init; }

    /// <summary>
    /// Gets the total points spent after modification.
    /// </summary>
    /// <value>
    /// The cumulative points spent on attribute allocation. On success, this
    /// reflects the new state's spent total. On failure, this reflects the
    /// current (unchanged) state's spent total.
    /// </value>
    public int PointsSpent { get; init; }

    /// <summary>
    /// Gets the points remaining after modification.
    /// </summary>
    /// <value>
    /// The unspent points available for further allocation. On success, this
    /// reflects the new state's remaining total. On failure, this reflects the
    /// current (unchanged) state's remaining total.
    /// </value>
    public int PointsRemaining { get; init; }

    /// <summary>
    /// Gets the error message if modification failed.
    /// </summary>
    /// <value>
    /// A human-readable description of why the modification was rejected,
    /// or <c>null</c> when <see cref="Success"/> is <c>true</c>.
    /// </value>
    /// <remarks>
    /// Common failure reasons include:
    /// <list type="bullet">
    ///   <item><description>"Cannot modify attributes in Simple mode"</description></item>
    ///   <item><description>"Value must be between 1 and 10"</description></item>
    ///   <item><description>"Insufficient points: need X, have Y"</description></item>
    /// </list>
    /// </remarks>
    public string? ErrorMessage { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether the modification was rejected.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="Success"/> is <c>false</c>;
    /// otherwise, <c>false</c>.
    /// </value>
    public bool IsFailure => !Success;

    /// <summary>
    /// Gets whether the result contains an updated state.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="NewState"/> is not <c>null</c>;
    /// otherwise, <c>false</c>.
    /// </value>
    public bool HasNewState => NewState.HasValue;

    /// <summary>
    /// Gets whether the result contains an error message.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="ErrorMessage"/> is not null or whitespace;
    /// otherwise, <c>false</c>.
    /// </value>
    public bool HasErrorMessage => !string.IsNullOrWhiteSpace(ErrorMessage);

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a successful modification result with the updated state.
    /// </summary>
    /// <param name="newState">
    /// The allocation state after the modification was applied.
    /// Must contain the updated attribute value and recalculated point totals.
    /// </param>
    /// <param name="logger">Optional logger for diagnostic output during creation.</param>
    /// <returns>
    /// An <see cref="AttributeModificationResult"/> with <see cref="Success"/> set
    /// to <c>true</c>, <see cref="NewState"/> set to the provided state, and
    /// point totals extracted from the new state.
    /// </returns>
    /// <example>
    /// <code>
    /// var updatedState = currentState.WithAttributeValue(CoreAttribute.Might, 5, 4);
    /// var result = AttributeModificationResult.Succeeded(updatedState);
    /// // result.Success == true
    /// // result.NewState == updatedState
    /// // result.PointsSpent == updatedState.PointsSpent
    /// // result.PointsRemaining == updatedState.PointsRemaining
    /// </code>
    /// </example>
    public static AttributeModificationResult Succeeded(
        AttributeAllocationState newState,
        ILogger<AttributeModificationResult>? logger = null)
    {
        _logger = logger;

        _logger?.LogDebug(
            "Creating successful AttributeModificationResult. " +
            "NewState: M:{Might} F:{Finesse} Wi:{Wits} Wl:{Will} S:{Sturdiness}, " +
            "PointsSpent={PointsSpent}, PointsRemaining={PointsRemaining}",
            newState.CurrentMight,
            newState.CurrentFinesse,
            newState.CurrentWits,
            newState.CurrentWill,
            newState.CurrentSturdiness,
            newState.PointsSpent,
            newState.PointsRemaining);

        var result = new AttributeModificationResult
        {
            Success = true,
            NewState = newState,
            PointsSpent = newState.PointsSpent,
            PointsRemaining = newState.PointsRemaining,
            ErrorMessage = null
        };

        _logger?.LogDebug(
            "Created successful modification result. " +
            "PointsSpent={PointsSpent}, PointsRemaining={PointsRemaining}",
            result.PointsSpent,
            result.PointsRemaining);

        return result;
    }

    /// <summary>
    /// Creates a failed modification result with an error message.
    /// </summary>
    /// <param name="errorMessage">
    /// A human-readable description of why the modification was rejected.
    /// Must not be null or whitespace.
    /// </param>
    /// <param name="currentState">
    /// The unchanged allocation state (before the failed modification attempt).
    /// Point totals are extracted from this state to reflect the current status.
    /// </param>
    /// <param name="logger">Optional logger for diagnostic output during creation.</param>
    /// <returns>
    /// An <see cref="AttributeModificationResult"/> with <see cref="Success"/> set
    /// to <c>false</c>, <see cref="NewState"/> set to <c>null</c>, and
    /// <see cref="ErrorMessage"/> set to the provided reason.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="errorMessage"/> is null or whitespace.
    /// </exception>
    /// <example>
    /// <code>
    /// var result = AttributeModificationResult.Failed(
    ///     "Insufficient points: need 4, have 2", currentState);
    /// // result.Success == false
    /// // result.NewState == null
    /// // result.ErrorMessage == "Insufficient points: need 4, have 2"
    /// // result.PointsSpent == currentState.PointsSpent
    /// </code>
    /// </example>
    public static AttributeModificationResult Failed(
        string errorMessage,
        AttributeAllocationState currentState,
        ILogger<AttributeModificationResult>? logger = null)
    {
        _logger = logger;

        _logger?.LogDebug(
            "Creating failed AttributeModificationResult. " +
            "ErrorMessage='{ErrorMessage}', " +
            "CurrentPointsSpent={PointsSpent}, CurrentPointsRemaining={PointsRemaining}",
            errorMessage,
            currentState.PointsSpent,
            currentState.PointsRemaining);

        // Validate the error message is not null or whitespace
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage, nameof(errorMessage));

        var result = new AttributeModificationResult
        {
            Success = false,
            NewState = null,
            PointsSpent = currentState.PointsSpent,
            PointsRemaining = currentState.PointsRemaining,
            ErrorMessage = errorMessage
        };

        _logger?.LogDebug(
            "Created failed modification result. " +
            "ErrorMessage='{ErrorMessage}', PointsSpent={PointsSpent}, " +
            "PointsRemaining={PointsRemaining}",
            result.ErrorMessage,
            result.PointsSpent,
            result.PointsRemaining);

        return result;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a formatted string representation of this modification result.
    /// </summary>
    /// <returns>
    /// A string in the format "Success: X spent, Y remaining" or
    /// "Failed: error message".
    /// </returns>
    public override string ToString() =>
        Success
            ? $"Success: {PointsSpent} spent, {PointsRemaining} remaining"
            : $"Failed: {ErrorMessage}";
}
