// ═══════════════════════════════════════════════════════════════════════════════
// NameValidationResult.cs
// Represents the result of a character name validation operation. Contains the
// validation status (pass/fail), a user-friendly error message describing why
// validation failed, and an optional suggested correction derived from sanitizing
// the invalid input. Used by the NameValidator service and consumed by the
// CharacterCreationController and TUI for Step 6 (Summary) feedback.
// Version: 0.17.5c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the result of a character name validation operation.
/// Contains validation status, error message, and optional suggested correction.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="NameValidationResult"/> is an immutable value object returned by
/// the <c>INameValidator.Validate()</c> method after checking a character name
/// against all configured validation rules. The result encapsulates three pieces
/// of information:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <strong>IsValid:</strong> Whether the name passed all validation rules.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>ErrorMessage:</strong> A user-friendly description of why the name
///       failed validation. Null when <see cref="IsValid"/> is <c>true</c>.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>SuggestedName:</strong> An automatically corrected version of the
///       name that would pass validation. Only provided when the original name can
///       be meaningfully sanitized (e.g., replacing diacritics, truncating length).
///       Null when no suggestion is available or when the name is valid.
///     </description>
///   </item>
/// </list>
/// <para>
/// <strong>Factory Methods:</strong> Use <see cref="Valid"/> to create a passing
/// result and <see cref="Invalid"/> to create a failing result with an error message
/// and optional suggestion. Direct construction via init properties is also supported
/// but factory methods are preferred for clarity.
/// </para>
/// <para>
/// <strong>Consumers:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>CharacterCreationController (v0.17.5d) — processes result to enable/disable confirm button</description></item>
///   <item><description>TUI Step 6 (v0.17.5f) — displays error message and suggestion to player</description></item>
///   <item><description>CharacterFactory (v0.17.5e) — validates name before character creation</description></item>
/// </list>
/// </remarks>
/// <seealso cref="NameValidationConfig"/>
public readonly record struct NameValidationResult
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a value indicating whether the name passed all validation rules.
    /// </summary>
    /// <value>
    /// <c>true</c> if the name meets all configured validation criteria (length,
    /// character pattern, boundaries, and profanity filter); otherwise, <c>false</c>.
    /// </value>
    public bool IsValid { get; init; }

    /// <summary>
    /// Gets the error message describing why validation failed.
    /// Null when <see cref="IsValid"/> is <c>true</c>.
    /// </summary>
    /// <value>
    /// A user-friendly error message suitable for display in the TUI. Examples:
    /// "Name is required.", "Name must be at least 2 characters.",
    /// "Name can only contain letters, spaces, and hyphens."
    /// Null when validation passes.
    /// </value>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Gets a suggested valid name based on sanitizing the invalid input.
    /// Null when no suggestion is available or name is valid.
    /// </summary>
    /// <value>
    /// A sanitized version of the original name that passes validation, or <c>null</c>
    /// when the name cannot be meaningfully corrected (e.g., all digits, profanity match).
    /// </value>
    /// <remarks>
    /// <para>
    /// Suggestions are provided when the name can be corrected automatically,
    /// such as truncating an over-length name, replacing diacritics with ASCII
    /// equivalents (e.g., "Björn" → "Bjorn"), or removing invalid characters.
    /// </para>
    /// <para>
    /// The TUI can offer the suggestion to the player via a "Use Suggestion"
    /// action (Tab key) on the Step 6 name entry screen.
    /// </para>
    /// </remarks>
    public string? SuggestedName { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <returns>
    /// A <see cref="NameValidationResult"/> with <see cref="IsValid"/> set to <c>true</c>,
    /// and both <see cref="ErrorMessage"/> and <see cref="SuggestedName"/> set to <c>null</c>.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = NameValidationResult.Valid();
    /// // result.IsValid == true
    /// // result.ErrorMessage == null
    /// // result.SuggestedName == null
    /// </code>
    /// </example>
    public static NameValidationResult Valid() => new()
    {
        IsValid = true,
        ErrorMessage = null,
        SuggestedName = null
    };

    /// <summary>
    /// Creates a failed validation result with an error message.
    /// </summary>
    /// <param name="errorMessage">
    /// The user-friendly error message describing why validation failed.
    /// Must not be null or empty.
    /// </param>
    /// <param name="suggestedName">
    /// Optional suggested correction. Pass <c>null</c> when no suggestion is available
    /// (e.g., profanity match, name is all digits).
    /// </param>
    /// <returns>
    /// A <see cref="NameValidationResult"/> with <see cref="IsValid"/> set to <c>false</c>,
    /// the provided error message, and optional suggestion.
    /// </returns>
    /// <example>
    /// <code>
    /// // Without suggestion:
    /// var result = NameValidationResult.Invalid("Name is required.");
    ///
    /// // With suggestion:
    /// var result = NameValidationResult.Invalid(
    ///     "Name can only contain letters, spaces, and hyphens.",
    ///     "Bjorn the Swift");
    /// </code>
    /// </example>
    public static NameValidationResult Invalid(string errorMessage, string? suggestedName = null) => new()
    {
        IsValid = false,
        ErrorMessage = errorMessage,
        SuggestedName = suggestedName
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation for debugging.
    /// </summary>
    /// <returns>
    /// "Valid" when the name passed validation, or "Invalid: {ErrorMessage}" with
    /// an optional "(Suggested: {SuggestedName})" suffix when a suggestion is available.
    /// </returns>
    public override string ToString() =>
        IsValid
            ? "Valid"
            : $"Invalid: {ErrorMessage}" + (SuggestedName != null ? $" (Suggested: {SuggestedName})" : "");
}
