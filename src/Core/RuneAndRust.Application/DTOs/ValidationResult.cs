// ═══════════════════════════════════════════════════════════════════════════════
// LineageValidationResult.cs
// Data transfer object representing the result of a lineage validation operation.
// Version: 0.17.0f
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Represents the result of a lineage validation operation.
/// </summary>
/// <remarks>
/// <para>
/// LineageValidationResult is an immutable record that captures whether a lineage
/// validation check passed or failed, along with an optional failure reason. It is
/// used by the LineageApplicationService to validate inputs before performing
/// operations, allowing callers to check validity without triggering side effects.
/// </para>
/// <para>
/// Use the static factory methods <see cref="Valid"/> and <see cref="Fail"/>
/// to create instances rather than using the constructor directly.
/// </para>
/// <para>
/// <strong>Usage Pattern:</strong>
/// <code>
/// var result = service.ValidateLineageSelection(character, lineage);
/// if (!result.IsValid)
/// {
///     ShowError(result.FailureReason);
///     return;
/// }
/// // Proceed with operation...
/// </code>
/// </para>
/// </remarks>
/// <param name="IsValid">Whether the validation passed.</param>
/// <param name="FailureReason">
/// The reason for failure, or <c>null</c> if validation passed.
/// </param>
/// <seealso cref="LineageApplicationResult"/>
public record LineageValidationResult(bool IsValid, string? FailureReason)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <returns>A <see cref="LineageValidationResult"/> with <see cref="IsValid"/> set to <c>true</c>.</returns>
    /// <example>
    /// <code>
    /// return LineageValidationResult.Valid();
    /// </code>
    /// </example>
    public static LineageValidationResult Valid() => new(true, null);

    /// <summary>
    /// Creates a failed validation result with the specified reason.
    /// </summary>
    /// <param name="reason">
    /// A human-readable description of why validation failed.
    /// Must not be null or whitespace.
    /// </param>
    /// <returns>
    /// A <see cref="LineageValidationResult"/> with <see cref="IsValid"/> set to <c>false</c>
    /// and <see cref="FailureReason"/> set to the provided reason.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="reason"/> is null or whitespace.
    /// </exception>
    /// <example>
    /// <code>
    /// return LineageValidationResult.Fail("Character already has a lineage assigned");
    /// </code>
    /// </example>
    public static LineageValidationResult Fail(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason, nameof(reason));
        return new(false, reason);
    }
}
