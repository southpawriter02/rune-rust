// ═══════════════════════════════════════════════════════════════════════════════
// AllocationValidationResult.cs
// Value object representing the outcome of an attribute allocation validation
// operation, supporting multiple error messages for comprehensive feedback.
// Version: 0.17.2f
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using Microsoft.Extensions.Logging;

/// <summary>
/// Represents the result of an attribute allocation validation operation
/// with support for multiple errors.
/// </summary>
/// <remarks>
/// <para>
/// AllocationValidationResult is an immutable value object that captures whether
/// an allocation validation check passed or failed, along with zero or more error
/// messages describing the specific issues found. This enables comprehensive
/// validation feedback where multiple problems can be reported simultaneously
/// rather than failing on the first issue encountered.
/// </para>
/// <para>
/// Use the static factory methods <see cref="Valid"/> and <see cref="Invalid"/>
/// to create instances rather than using the constructor directly.
/// </para>
/// <para>
/// <strong>Usage Pattern:</strong>
/// <code>
/// var errors = new List&lt;string&gt;();
/// if (value &lt; min) errors.Add($"Value {value} is below minimum {min}");
/// if (value &gt; max) errors.Add($"Value {value} exceeds maximum {max}");
///
/// return errors.Count == 0
///     ? AllocationValidationResult.Valid()
///     : AllocationValidationResult.Invalid(errors);
/// </code>
/// </para>
/// <para>
/// <strong>Note:</strong> This type is distinct from
/// <c>RuneAndRust.Application.DTOs.LineageValidationResult</c>, which supports only
/// a single failure reason string. This type supports multiple errors for scenarios
/// where comprehensive validation feedback is needed (e.g., attribute allocation
/// validation that checks multiple constraints simultaneously).
/// </para>
/// <para>
/// <strong>Note:</strong> This type is intentionally named <c>AllocationValidationResult</c>
/// rather than <c>ValidationResult</c> to avoid ambiguity with
/// <c>Spectre.Console.ValidationResult</c> used in the TUI presentation layer.
/// </para>
/// </remarks>
/// <seealso cref="AttributeAllocationState"/>
public readonly record struct AllocationValidationResult
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Logger for detailed diagnostic output during validation result creation.
    /// </summary>
    private static ILogger<AllocationValidationResult>? _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether the validation passed successfully.
    /// </summary>
    /// <value>
    /// <c>true</c> if no validation errors were found;
    /// otherwise, <c>false</c>.
    /// </value>
    public bool IsValid { get; init; }

    /// <summary>
    /// Gets the collection of validation error messages.
    /// </summary>
    /// <value>
    /// An empty list when <see cref="IsValid"/> is <c>true</c>,
    /// or a list containing one or more error descriptions when <c>false</c>.
    /// </value>
    /// <remarks>
    /// Each error message should be a human-readable description of a specific
    /// validation failure. Error messages are stored in the order they were
    /// provided and are never null.
    /// </remarks>
    public IReadOnlyList<string> Errors { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether the validation result contains any errors.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="Errors"/> contains at least one entry;
    /// otherwise, <c>false</c>.
    /// </value>
    public bool HasErrors => Errors?.Count > 0;

    /// <summary>
    /// Gets the number of validation errors.
    /// </summary>
    /// <value>
    /// The count of error messages in <see cref="Errors"/>,
    /// or 0 if no errors exist.
    /// </value>
    public int ErrorCount => Errors?.Count ?? 0;

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a successful validation result with no errors.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostic output during creation.</param>
    /// <returns>
    /// An <see cref="AllocationValidationResult"/> with <see cref="IsValid"/> set to <c>true</c>
    /// and an empty <see cref="Errors"/> list.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = AllocationValidationResult.Valid();
    /// // result.IsValid == true
    /// // result.ErrorCount == 0
    /// // result.HasErrors == false
    /// </code>
    /// </example>
    public static AllocationValidationResult Valid(
        ILogger<AllocationValidationResult>? logger = null)
    {
        _logger = logger;

        _logger?.LogDebug("Creating valid AllocationValidationResult with no errors");

        var result = new AllocationValidationResult
        {
            IsValid = true,
            Errors = Array.Empty<string>()
        };

        _logger?.LogDebug(
            "Created valid AllocationValidationResult. IsValid={IsValid}, ErrorCount={ErrorCount}",
            result.IsValid,
            result.ErrorCount);

        return result;
    }

    /// <summary>
    /// Creates a failed validation result with one or more error messages.
    /// </summary>
    /// <param name="errors">
    /// The validation error messages. Must not be null or empty.
    /// Each string should describe a specific validation failure.
    /// </param>
    /// <param name="logger">Optional logger for diagnostic output during creation.</param>
    /// <returns>
    /// An <see cref="AllocationValidationResult"/> with <see cref="IsValid"/> set to <c>false</c>
    /// and <see cref="Errors"/> containing the provided error messages.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="errors"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="errors"/> is empty (contains no elements).
    /// </exception>
    /// <example>
    /// <code>
    /// var errors = new List&lt;string&gt;
    /// {
    ///     "Might value 0 is out of range",
    ///     "Points overspent by 3"
    /// };
    /// var result = AllocationValidationResult.Invalid(errors);
    /// // result.IsValid == false
    /// // result.ErrorCount == 2
    /// // result.Errors[0] == "Might value 0 is out of range"
    /// </code>
    /// </example>
    /// <remarks>
    /// <para>
    /// The error list is copied to prevent external modification after creation.
    /// The original collection is not retained.
    /// </para>
    /// </remarks>
    public static AllocationValidationResult Invalid(
        IEnumerable<string> errors,
        ILogger<AllocationValidationResult>? logger = null)
    {
        _logger = logger;

        _logger?.LogDebug("Creating invalid AllocationValidationResult with error collection");

        // Validate the errors parameter is not null
        ArgumentNullException.ThrowIfNull(errors, nameof(errors));

        // Materialize the errors to a list for count validation and immutable storage
        var errorList = errors.ToList().AsReadOnly();

        // Validate that at least one error is provided
        if (errorList.Count == 0)
        {
            _logger?.LogWarning(
                "Invalid AllocationValidationResult creation attempted with empty error " +
                "collection. At least one error message is required.");

            throw new ArgumentException(
                "At least one error message is required for an invalid validation result.",
                nameof(errors));
        }

        _logger?.LogDebug(
            "Validated error collection. ErrorCount={ErrorCount}",
            errorList.Count);

        var result = new AllocationValidationResult
        {
            IsValid = false,
            Errors = errorList
        };

        _logger?.LogDebug(
            "Created invalid AllocationValidationResult. IsValid={IsValid}, " +
            "ErrorCount={ErrorCount}, FirstError='{FirstError}'",
            result.IsValid,
            result.ErrorCount,
            errorList[0]);

        return result;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a combined summary of all validation errors.
    /// </summary>
    /// <returns>
    /// A semicolon-separated string of all error messages,
    /// or "Valid" if no errors exist.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = AllocationValidationResult.Invalid(new[] { "Error A", "Error B" });
    /// result.GetErrorSummary(); // "Error A; Error B"
    ///
    /// var valid = AllocationValidationResult.Valid();
    /// valid.GetErrorSummary(); // "Valid"
    /// </code>
    /// </example>
    public string GetErrorSummary() =>
        HasErrors
            ? string.Join("; ", Errors)
            : "Valid";

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a formatted string representation of this validation result.
    /// </summary>
    /// <returns>
    /// A string in the format "AllocationValidationResult: Valid" or
    /// "AllocationValidationResult: Invalid (N errors: error1; error2; ...)"
    /// </returns>
    public override string ToString() =>
        IsValid
            ? "AllocationValidationResult: Valid"
            : $"AllocationValidationResult: Invalid ({ErrorCount} error{(ErrorCount != 1 ? "s" : "")}: {GetErrorSummary()})";
}
