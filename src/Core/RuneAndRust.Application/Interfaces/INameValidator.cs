// ═══════════════════════════════════════════════════════════════════════════════
// INameValidator.cs
// Interface defining the contract for character name validation during the
// character creation workflow. Validates names against configurable rules
// including length constraints, allowed character patterns, boundary requirements,
// and profanity filtering. Provides full validation, quick validity checking,
// and name sanitization with diacritics normalization.
// Version: 0.17.5c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Validates character names against configurable rules including length,
/// allowed characters, boundaries, and profanity filtering.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="INameValidator"/> is the application-layer service contract for
/// character name validation used during Step 6 (Summary) of the character
/// creation wizard. Unlike the domain-level <c>PlayerNameValidator</c> (which
/// provides basic static validation), this service offers configurable rules
/// loaded from <c>creation-workflow.json</c>, structured logging, profanity
/// filtering, and name sanitization with diacritics normalization.
/// </para>
/// <para>
/// <strong>Validation Rules (in order):</strong>
/// </para>
/// <list type="number">
///   <item><description>Required — name must not be null or whitespace</description></item>
///   <item><description>Minimum length — must be at least 2 characters (configurable)</description></item>
///   <item><description>Maximum length — must be at most 20 characters (configurable)</description></item>
///   <item><description>Boundary — must start and end with an ASCII letter</description></item>
///   <item><description>Allowed characters — only ASCII letters (a-z, A-Z), spaces, and hyphens</description></item>
///   <item><description>Profanity filter — must not contain any blocked words (case-insensitive)</description></item>
/// </list>
/// <para>
/// <strong>Usage Pattern:</strong> The CharacterCreationController (v0.17.5d) calls
/// <see cref="Validate"/> when the player enters or confirms a name in Step 6. The
/// TUI (v0.17.5f) may call <see cref="IsValid"/> for real-time keystroke feedback.
/// The CharacterFactory (v0.17.5e) calls <see cref="Validate"/> as a final guard
/// before character creation.
/// </para>
/// <para>
/// <strong>Configuration:</strong> Implementations receive validation parameters
/// via <see cref="NameValidationConfig"/>, which is loaded from the
/// <c>nameValidation</c> section of <c>creation-workflow.json</c>.
/// </para>
/// </remarks>
/// <seealso cref="NameValidationResult"/>
/// <seealso cref="NameValidationConfig"/>
public interface INameValidator
{
    /// <summary>
    /// Validates the provided character name against all configured rules.
    /// </summary>
    /// <param name="name">
    /// The character name to validate. May be <c>null</c> or empty, in which
    /// case the result will indicate the name is required.
    /// </param>
    /// <returns>
    /// A <see cref="NameValidationResult"/> containing the validation status,
    /// error message (if invalid), and optional suggested correction.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Rules are evaluated in priority order (required → length → boundary →
    /// characters → profanity). Validation stops at the first failure and returns
    /// the corresponding error message. When a name fails due to invalid characters
    /// or boundary issues, the validator attempts to sanitize the name and includes
    /// the result as <see cref="NameValidationResult.SuggestedName"/>.
    /// </para>
    /// <para>
    /// The input name is trimmed before validation. Leading/trailing whitespace
    /// is not counted toward the length check.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = nameValidator.Validate("Björn the Swift");
    /// // result.IsValid == false
    /// // result.ErrorMessage == "Name can only contain letters, spaces, and hyphens."
    /// // result.SuggestedName == "Bjorn the Swift"
    /// </code>
    /// </example>
    NameValidationResult Validate(string? name);

    /// <summary>
    /// Checks if the provided name is valid without returning detailed errors.
    /// Useful for quick validation in UI scenarios.
    /// </summary>
    /// <param name="name">The character name to check. May be <c>null</c>.</param>
    /// <returns>
    /// <c>true</c> if the name passes all validation rules; <c>false</c> otherwise.
    /// </returns>
    /// <remarks>
    /// This is a convenience method that delegates to <see cref="Validate"/> and
    /// returns only the <see cref="NameValidationResult.IsValid"/> flag. Use this
    /// for real-time keystroke validation in the TUI where only a boolean indicator
    /// is needed (e.g., green/red border on the name input field).
    /// </remarks>
    /// <example>
    /// <code>
    /// if (nameValidator.IsValid(currentText))
    ///     ShowValidIndicator();
    /// else
    ///     ShowInvalidIndicator();
    /// </code>
    /// </example>
    bool IsValid(string? name);

    /// <summary>
    /// Attempts to sanitize an invalid name by replacing or removing
    /// invalid characters while preserving the intended name.
    /// </summary>
    /// <param name="name">The name to sanitize. Must not be <c>null</c>.</param>
    /// <returns>
    /// A sanitized version of the name that passes character and boundary validation,
    /// or <c>null</c> if the name cannot be meaningfully corrected (e.g., all digits,
    /// result too short after removing invalid characters).
    /// </returns>
    /// <remarks>
    /// <para>
    /// Sanitization performs the following transformations:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Replaces accented characters with ASCII equivalents (e.g., ö → o, é → e, ñ → n)</description></item>
    ///   <item><description>Removes non-letter, non-space, non-hyphen characters</description></item>
    ///   <item><description>Collapses consecutive spaces</description></item>
    ///   <item><description>Trims non-letter characters from boundaries</description></item>
    ///   <item><description>Truncates to maximum length if needed</description></item>
    /// </list>
    /// <para>
    /// <strong>Note:</strong> Sanitization does NOT check profanity. A sanitized name
    /// may still fail the profanity filter and should be re-validated after sanitization.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var sanitized = nameValidator.Sanitize("Björn the Swift");
    /// // sanitized == "Bjorn the Swift"
    ///
    /// var noSanitize = nameValidator.Sanitize("12345");
    /// // noSanitize == null (cannot preserve intent)
    /// </code>
    /// </example>
    string? Sanitize(string name);
}
