// ═══════════════════════════════════════════════════════════════════════════════
// NameValidationConfig.cs
// Configuration for character name validation rules. Loaded from the
// creation-workflow.json configuration file's "nameValidation" section.
// Defines minimum/maximum length constraints, allowed character pattern (regex),
// and a profanity filter word list. Consumed by the NameValidator application
// service to enforce configurable name validation during character creation.
// Version: 0.17.5c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Configuration for character name validation rules.
/// Loaded from the <c>creation-workflow.json</c> configuration file.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="NameValidationConfig"/> is an immutable value object that holds the
/// validation parameters used by the <c>NameValidator</c> service. All properties
/// are configurable via the <c>nameValidation</c> section of <c>creation-workflow.json</c>,
/// allowing game designers to adjust name rules without code changes.
/// </para>
/// <para>
/// <strong>Configuration Source:</strong> The <c>nameValidation</c> section in
/// <c>config/creation-workflow.json</c> provides:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <strong>minLength:</strong> Minimum name length (default: 2). Names shorter
///       than this are rejected with a "too short" error.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>maxLength:</strong> Maximum name length (default: 20). Names longer
///       than this are rejected with a "too long" error and a truncation suggestion.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>allowedPattern:</strong> Regex pattern for allowed characters. The
///       default pattern permits ASCII letters, spaces, and hyphens, requiring the
///       name to start and end with a letter.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>profanityFilter:</strong> Array of words to block from character names.
///       Matching is case-insensitive and checks both substrings and individual words.
///     </description>
///   </item>
/// </list>
/// <para>
/// <strong>Default Configuration:</strong> Use <see cref="Default"/> for standard
/// validation rules when configuration loading is not available or for testing.
/// </para>
/// </remarks>
/// <seealso cref="NameValidationResult"/>
public readonly record struct NameValidationConfig
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the minimum allowed name length.
    /// </summary>
    /// <value>
    /// The minimum number of characters required for a valid name.
    /// Default: 2 characters. Names shorter than this value are rejected.
    /// </value>
    /// <remarks>
    /// Single-character names (e.g., "A") are not allowed by default.
    /// The minimum of 2 allows short names like "Al" or "Jo" while preventing
    /// meaningless single-letter entries.
    /// </remarks>
    public int MinLength { get; init; }

    /// <summary>
    /// Gets the maximum allowed name length.
    /// </summary>
    /// <value>
    /// The maximum number of characters allowed for a valid name.
    /// Default: 20 characters. Names longer than this value are rejected
    /// with a truncation suggestion.
    /// </value>
    /// <remarks>
    /// The 20-character limit accommodates names like "Bjorn the Swift" (15 chars)
    /// while preventing excessively long entries that would disrupt TUI layout.
    /// </remarks>
    public int MaxLength { get; init; }

    /// <summary>
    /// Gets the regex pattern for allowed characters.
    /// </summary>
    /// <value>
    /// A regular expression pattern that valid names must match. The default
    /// pattern <c>^[a-zA-Z][a-zA-Z\s\-]*[a-zA-Z]$|^[a-zA-Z]{2}$</c> requires:
    /// names start and end with an ASCII letter, with optional ASCII letters,
    /// spaces, and hyphens in between. The alternation handles the 2-character
    /// edge case.
    /// </value>
    /// <remarks>
    /// <para>
    /// The pattern is compiled into a <see cref="System.Text.RegularExpressions.Regex"/>
    /// at <c>NameValidator</c> construction time with <c>RegexOptions.Compiled</c> for
    /// performance and a 100ms timeout for safety.
    /// </para>
    /// <para>
    /// <strong>Note:</strong> The <c>NameValidator</c> currently uses a character-by-character
    /// check (<c>IsOnlyAllowedCharacters</c>) rather than this regex for the actual validation,
    /// as the character iteration is more efficient for simple ASCII checks and enables
    /// explicit non-ASCII rejection (<c>c &gt; 127</c>). The pattern is retained for
    /// configuration completeness and potential future use.
    /// </para>
    /// </remarks>
    public string AllowedPattern { get; init; }

    /// <summary>
    /// Gets the list of words to filter from names.
    /// </summary>
    /// <value>
    /// An immutable list of words that are blocked from appearing in character names.
    /// Matching is case-insensitive and checks both substrings and individual words
    /// split on spaces and hyphens.
    /// </value>
    /// <remarks>
    /// <para>
    /// The profanity filter prevents inappropriate names from being created. The default
    /// configuration includes system-reserved terms ("admin", "moderator", "system") to
    /// prevent player impersonation. Game-specific offensive terms can be added to the
    /// <c>profanityFilter</c> array in <c>creation-workflow.json</c>.
    /// </para>
    /// <para>
    /// <strong>Security Note:</strong> When a profanity match is detected, the actual
    /// name is NOT logged to prevent sensitive content from appearing in log files.
    /// Only a generic "Name contains filtered word" message is logged.
    /// </para>
    /// </remarks>
    public IReadOnlyList<string> ProfanityFilter { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // STATIC PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates the default configuration matching the standard creation-workflow.json values.
    /// </summary>
    /// <value>
    /// A <see cref="NameValidationConfig"/> with MinLength=2, MaxLength=20,
    /// the standard ASCII letter/space/hyphen pattern, and an empty profanity filter.
    /// </value>
    /// <remarks>
    /// Use this property for unit testing or as a fallback when configuration
    /// loading is not available. The production <c>NameValidator</c> should
    /// receive its configuration from the JSON configuration loader.
    /// </remarks>
    public static NameValidationConfig Default => new()
    {
        MinLength = 2,
        MaxLength = 20,
        AllowedPattern = @"^[a-zA-Z][a-zA-Z\s\-]*[a-zA-Z]$|^[a-zA-Z]{2}$",
        ProfanityFilter = Array.Empty<string>()
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a formatted string representation for debugging.
    /// </summary>
    /// <returns>
    /// A string showing the configuration parameters: length range, pattern,
    /// and profanity filter count.
    /// </returns>
    public override string ToString() =>
        $"NameValidationConfig [Length:{MinLength}-{MaxLength}, " +
        $"Pattern:{AllowedPattern}, ProfanityFilter:{ProfanityFilter?.Count ?? 0} terms]";
}
