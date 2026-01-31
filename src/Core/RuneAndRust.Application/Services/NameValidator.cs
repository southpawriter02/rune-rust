// ═══════════════════════════════════════════════════════════════════════════════
// NameValidator.cs
// Application-layer service that validates character names against configurable
// rules loaded from creation-workflow.json. Checks names against length constraints
// (2-20 characters), allowed character patterns (ASCII letters, spaces, hyphens),
// boundary requirements (must start/end with letter), and a profanity filter.
// Provides name sanitization with diacritics normalization for suggesting valid
// alternatives when invalid names are submitted during Step 6 (Summary).
// Version: 0.17.5c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Validates character names against configurable rules.
/// Rules are loaded from the <c>creation-workflow.json</c> configuration.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="NameValidator"/> is the application-layer implementation of
/// <see cref="INameValidator"/> for the character creation workflow. Unlike the
/// domain-level <c>PlayerNameValidator</c> (a static utility with hardcoded rules),
/// this service supports configurable validation parameters via
/// <see cref="NameValidationConfig"/>, structured logging, profanity filtering,
/// and name sanitization with diacritics normalization.
/// </para>
/// <para>
/// <strong>Validation Rules (evaluated in order):</strong>
/// </para>
/// <list type="number">
///   <item><description>Required — name must not be null or whitespace</description></item>
///   <item><description>Minimum length — at least <see cref="NameValidationConfig.MinLength"/> characters (default: 2)</description></item>
///   <item><description>Maximum length — at most <see cref="NameValidationConfig.MaxLength"/> characters (default: 20)</description></item>
///   <item><description>Boundary — must start and end with an ASCII letter (a-z, A-Z)</description></item>
///   <item><description>Allowed characters — only ASCII letters, spaces, and hyphens (non-ASCII letters rejected)</description></item>
///   <item><description>Profanity filter — must not contain any configured blocked words (case-insensitive substring and word matching)</description></item>
/// </list>
/// <para>
/// <strong>Error Handling:</strong> Validation stops at the first failing rule and
/// returns the corresponding error message. For character and boundary failures, the
/// validator attempts to <see cref="Sanitize"/> the name and includes the result as
/// a suggestion in the <see cref="NameValidationResult"/>.
/// </para>
/// <para>
/// <strong>Coexistence with PlayerNameValidator:</strong> This service operates
/// independently of the domain-level <c>PlayerNameValidator</c>. The domain validator
/// has different constraints (MaxLength=30, allows apostrophes, no profanity filter)
/// and serves as a basic entity-level guard. This service provides the full
/// application-layer validation for the character creation workflow.
/// </para>
/// </remarks>
/// <seealso cref="INameValidator"/>
/// <seealso cref="NameValidationResult"/>
/// <seealso cref="NameValidationConfig"/>
public partial class NameValidator : INameValidator
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// The validation configuration containing length constraints, pattern, and profanity filter.
    /// </summary>
    private readonly NameValidationConfig _config;

    /// <summary>
    /// Logger for structured diagnostic output during validation operations.
    /// </summary>
    private readonly ILogger<NameValidator> _logger;

    /// <summary>
    /// Compiled regex from the configuration's allowed pattern.
    /// Constructed at initialization for performance in repeated calls.
    /// </summary>
    private readonly Regex _allowedPatternRegex;

    /// <summary>
    /// Case-insensitive hash set of profanity filter terms for O(1) word lookups.
    /// Built from the configuration's profanity filter list at initialization.
    /// </summary>
    private readonly HashSet<string> _profanitySet;

    // ═══════════════════════════════════════════════════════════════════════════
    // ERROR MESSAGES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Error message when name is null, empty, or whitespace.</summary>
    private const string ErrorNameRequired = "Name is required.";

    /// <summary>Error message when name is shorter than the configured minimum. Format: {0} = MinLength.</summary>
    private const string ErrorNameTooShort = "Name must be at least {0} characters.";

    /// <summary>Error message when name exceeds the configured maximum. Format: {0} = MaxLength.</summary>
    private const string ErrorNameTooLong = "Name must be at most {0} characters.";

    /// <summary>Error message when name contains characters outside the allowed set.</summary>
    private const string ErrorInvalidCharacters = "Name can only contain letters, spaces, and hyphens.";

    /// <summary>Error message when name starts or ends with a non-letter character.</summary>
    private const string ErrorMustStartEndWithLetter = "Name must start and end with a letter.";

    /// <summary>Error message when name matches a profanity filter entry. Intentionally vague to avoid revealing filter contents.</summary>
    private const string ErrorProfanity = "Please choose a different name.";

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="NameValidator"/> class.
    /// </summary>
    /// <param name="config">
    /// The name validation configuration containing length constraints, allowed
    /// character pattern, and profanity filter terms. Loaded from the
    /// <c>nameValidation</c> section of <c>creation-workflow.json</c>.
    /// </param>
    /// <param name="logger">
    /// Logger for structured diagnostic output during validation operations.
    /// Must not be <c>null</c>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="logger"/> is <c>null</c>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// The constructor compiles the allowed pattern regex with
    /// <see cref="RegexOptions.Compiled"/> and a 100ms timeout for safety against
    /// pathological patterns. The profanity filter terms are loaded into a
    /// case-insensitive <see cref="HashSet{T}"/> for O(1) word lookups.
    /// </para>
    /// </remarks>
    public NameValidator(
        NameValidationConfig config,
        ILogger<NameValidator> logger)
    {
        _config = config;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Compile regex from configuration pattern for performance in repeated calls.
        // The 100ms timeout guards against pathological regex patterns from configuration.
        _allowedPatternRegex = new Regex(
            _config.AllowedPattern,
            RegexOptions.Compiled,
            TimeSpan.FromMilliseconds(100));

        // Build case-insensitive hash set from profanity filter for O(1) word lookups.
        // Empty filter list results in an empty set (no profanity blocking).
        _profanitySet = new HashSet<string>(
            _config.ProfanityFilter ?? [],
            StringComparer.OrdinalIgnoreCase);

        _logger.LogDebug(
            "NameValidator initialized. MinLength={MinLength}, MaxLength={MaxLength}, " +
            "PatternLength={PatternLength}, ProfanityCount={ProfanityCount}",
            _config.MinLength,
            _config.MaxLength,
            _config.AllowedPattern?.Length ?? 0,
            _profanitySet.Count);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // INameValidator IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// Validates the name by applying six rules in priority order. The input is
    /// trimmed before validation. Validation stops at the first failure.
    /// </para>
    /// <para>
    /// For rules 4 (boundary) and 5 (allowed characters), the validator attempts
    /// to <see cref="Sanitize"/> the name and includes the sanitized result as
    /// a suggestion if available.
    /// </para>
    /// </remarks>
    public NameValidationResult Validate(string? name)
    {
        _logger.LogDebug("Validating character name: '{Name}'", name ?? "(null)");

        // Rule 1: Required check — name must not be null, empty, or whitespace
        if (string.IsNullOrWhiteSpace(name))
        {
            _logger.LogDebug(
                "Validation failed: Name is null or whitespace. Rule=Required");
            return NameValidationResult.Invalid(ErrorNameRequired);
        }

        var trimmedName = name.Trim();

        // Rule 2: Minimum length check — name must meet configured minimum
        if (trimmedName.Length < _config.MinLength)
        {
            var message = string.Format(ErrorNameTooShort, _config.MinLength);
            _logger.LogDebug(
                "Validation failed: Name too short. Length={Length}, MinLength={MinLength}, Rule=MinLength",
                trimmedName.Length,
                _config.MinLength);
            return NameValidationResult.Invalid(message);
        }

        // Rule 3: Maximum length check — name must not exceed configured maximum
        if (trimmedName.Length > _config.MaxLength)
        {
            var message = string.Format(ErrorNameTooLong, _config.MaxLength);
            var suggested = trimmedName[.._config.MaxLength].TrimEnd();
            _logger.LogDebug(
                "Validation failed: Name too long. Length={Length}, MaxLength={MaxLength}, " +
                "SuggestedLength={SuggestedLength}, Rule=MaxLength",
                trimmedName.Length,
                _config.MaxLength,
                suggested.Length);
            return NameValidationResult.Invalid(message, suggested);
        }

        // Rule 4: Boundary check — name must start and end with an ASCII letter
        if (!char.IsLetter(trimmedName[0]) || !char.IsLetter(trimmedName[^1]))
        {
            _logger.LogDebug(
                "Validation failed: Name does not start/end with letter. " +
                "FirstChar='{FirstChar}', LastChar='{LastChar}', Rule=Boundary",
                trimmedName[0],
                trimmedName[^1]);
            var suggested = Sanitize(trimmedName);
            return NameValidationResult.Invalid(ErrorMustStartEndWithLetter, suggested);
        }

        // Rule 5: Allowed characters check — only ASCII letters, spaces, and hyphens
        if (!IsOnlyAllowedCharacters(trimmedName))
        {
            _logger.LogDebug(
                "Validation failed: Name contains disallowed characters. " +
                "NameLength={NameLength}, Rule=AllowedCharacters",
                trimmedName.Length);
            var suggested = Sanitize(trimmedName);
            return NameValidationResult.Invalid(ErrorInvalidCharacters, suggested);
        }

        // Rule 6: Profanity check — case-insensitive substring and word matching
        if (ContainsProfanity(trimmedName))
        {
            // Security: Do NOT log the actual name when profanity is detected
            _logger.LogDebug(
                "Validation failed: Name contains filtered word. Rule=Profanity");
            return NameValidationResult.Invalid(ErrorProfanity);
        }

        // All rules passed — name is valid
        _logger.LogInformation(
            "Name validation passed: '{ValidName}', Length={Length}",
            trimmedName,
            trimmedName.Length);
        return NameValidationResult.Valid();
    }

    /// <inheritdoc />
    /// <remarks>
    /// Delegates to <see cref="Validate"/> and returns only the
    /// <see cref="NameValidationResult.IsValid"/> flag. All validation rules
    /// are evaluated; this method provides no short-circuit optimization.
    /// </remarks>
    public bool IsValid(string? name) => Validate(name).IsValid;

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// Sanitization processes the name character-by-character:
    /// </para>
    /// <list type="number">
    ///   <item><description>Each character is passed through <c>NormalizeCharacter()</c> to convert diacritics to ASCII equivalents</description></item>
    ///   <item><description>ASCII letters are appended directly</description></item>
    ///   <item><description>Spaces and hyphens are appended if not preceded by another space and not at the start</description></item>
    ///   <item><description>All other characters are silently dropped</description></item>
    /// </list>
    /// <para>
    /// After character processing, the result is trimmed of trailing spaces/hyphens,
    /// truncated to <see cref="NameValidationConfig.MaxLength"/> if needed, and validated
    /// for minimum length and letter boundaries. Returns <c>null</c> if the result
    /// cannot meet these requirements.
    /// </para>
    /// </remarks>
    public string? Sanitize(string name)
    {
        _logger.LogDebug(
            "Sanitizing name. InputLength={InputLength}",
            name?.Length ?? 0);

        if (string.IsNullOrWhiteSpace(name))
        {
            _logger.LogDebug("Sanitization failed: Input is null or whitespace");
            return null;
        }

        var sanitized = new StringBuilder();
        var previousWasSpace = false;

        // Process each character: normalize diacritics, keep allowed chars, drop others
        foreach (var c in name)
        {
            // Attempt to convert non-ASCII characters to their ASCII equivalents
            var normalized = NormalizeCharacter(c);

            if (char.IsLetter(normalized))
            {
                // Only allow ASCII letters (a-z, A-Z) — reject non-ASCII even after normalization
                if (normalized <= 127)
                {
                    sanitized.Append(normalized);
                    previousWasSpace = false;
                }
            }
            else if ((normalized == ' ' || normalized == '-') && !previousWasSpace && sanitized.Length > 0)
            {
                // Allow space/hyphen if not consecutive and not at the start
                sanitized.Append(normalized);
                previousWasSpace = normalized == ' ';
            }
        }

        // Trim trailing non-letter characters (spaces, hyphens)
        var result = sanitized.ToString().TrimEnd(' ', '-');

        // Verify minimum length requirement
        if (result.Length < _config.MinLength)
        {
            _logger.LogDebug(
                "Sanitization failed: Result too short. ResultLength={ResultLength}, " +
                "MinLength={MinLength}",
                result.Length,
                _config.MinLength);
            return null;
        }

        // Truncate to maximum length if needed
        if (result.Length > _config.MaxLength)
        {
            result = result[.._config.MaxLength].TrimEnd(' ', '-');
            _logger.LogDebug(
                "Sanitization truncated result. NewLength={NewLength}, MaxLength={MaxLength}",
                result.Length,
                _config.MaxLength);
        }

        // Final validation: result must meet minimum length and start/end with letter
        if (result.Length < _config.MinLength ||
            !char.IsLetter(result[0]) ||
            !char.IsLetter(result[^1]))
        {
            _logger.LogDebug(
                "Sanitization failed: Result does not meet boundary/length requirements. " +
                "ResultLength={ResultLength}",
                result.Length);
            return null;
        }

        _logger.LogDebug(
            "Sanitization succeeded. InputLength={InputLength}, OutputLength={OutputLength}, " +
            "Output='{SanitizedName}'",
            name.Length,
            result.Length,
            result);

        return result;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE VALIDATION HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if the name contains only allowed characters (ASCII letters, space, hyphen).
    /// Non-ASCII letters (e.g., ö, é, ñ) are explicitly rejected even though
    /// <see cref="char.IsLetter(char)"/> returns <c>true</c> for them.
    /// </summary>
    /// <param name="name">The trimmed name string to check. Must not be null.</param>
    /// <returns>
    /// <c>true</c> if every character in the name is an ASCII letter (a-z, A-Z),
    /// a space, or a hyphen; otherwise, <c>false</c>.
    /// </returns>
    private bool IsOnlyAllowedCharacters(string name)
    {
        foreach (var c in name)
        {
            // Reject non-letter, non-space, non-hyphen characters (digits, symbols, etc.)
            if (!char.IsLetter(c) && c != ' ' && c != '-')
                return false;

            // Reject non-ASCII letters (accented characters, Cyrillic, CJK, etc.)
            // char.IsLetter returns true for these, but we only allow ASCII (0-127)
            if (char.IsLetter(c) && c > 127)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Checks if the name contains any words from the profanity filter.
    /// Performs case-insensitive matching on both substrings and individual words.
    /// </summary>
    /// <param name="name">The trimmed name string to check. Must not be null.</param>
    /// <returns>
    /// <c>true</c> if the name contains any term from the profanity filter as a
    /// substring or as an individual word; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The check operates in two passes:
    /// </para>
    /// <list type="number">
    ///   <item><description>Substring matching — checks if any filter term appears anywhere in the lowercased name</description></item>
    ///   <item><description>Word matching — splits the name on spaces and hyphens, checks each word against the filter set</description></item>
    /// </list>
    /// <para>
    /// The substring check catches evasion attempts like "xBadWordx". The word check
    /// provides an additional semantic layer using the O(1) HashSet lookup.
    /// </para>
    /// </remarks>
    private bool ContainsProfanity(string name)
    {
        var lowerName = name.ToLowerInvariant();

        // Pass 1: Substring matching — catches terms embedded within the name
        foreach (var term in _profanitySet)
        {
            if (lowerName.Contains(term, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        // Pass 2: Word matching — checks each individual word against the filter set
        var words = lowerName.Split([' ', '-'], StringSplitOptions.RemoveEmptyEntries);
        return words.Any(word => _profanitySet.Contains(word));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CHARACTER NORMALIZATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Attempts to normalize non-ASCII characters to their ASCII equivalents.
    /// Used by <see cref="Sanitize"/> to convert diacritics and special characters
    /// in names entered on non-English keyboards.
    /// </summary>
    /// <param name="c">The character to normalize.</param>
    /// <returns>
    /// The ASCII equivalent if a mapping exists (e.g., ö → o, é → e, ñ → n),
    /// or the original character if no mapping is defined.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Coverage includes common Latin-derived diacritics, Nordic characters (ð, þ, æ, ø),
    /// and ligatures (œ, ß). Characters not in the mapping table are returned as-is and
    /// will be dropped by the sanitizer if they are not ASCII letters, spaces, or hyphens.
    /// </para>
    /// <para>
    /// <strong>Limitations:</strong> This is not a comprehensive Unicode normalization.
    /// Full Unicode support (Cyrillic, CJK, etc.) is out of scope for the current version.
    /// </para>
    /// </remarks>
    private static char NormalizeCharacter(char c) => c switch
    {
        // Lowercase vowels with diacritics
        'á' or 'à' or 'â' or 'ä' or 'ã' or 'å' => 'a',
        'é' or 'è' or 'ê' or 'ë' => 'e',
        'í' or 'ì' or 'î' or 'ï' => 'i',
        'ó' or 'ò' or 'ô' or 'ö' or 'õ' or 'ø' => 'o',
        'ú' or 'ù' or 'û' or 'ü' => 'u',
        'ý' or 'ÿ' => 'y',

        // Uppercase vowels with diacritics
        'Á' or 'À' or 'Â' or 'Ä' or 'Ã' or 'Å' => 'A',
        'É' or 'È' or 'Ê' or 'Ë' => 'E',
        'Í' or 'Ì' or 'Î' or 'Ï' => 'I',
        'Ó' or 'Ò' or 'Ô' or 'Ö' or 'Õ' or 'Ø' => 'O',
        'Ú' or 'Ù' or 'Û' or 'Ü' => 'U',
        'Ý' or 'Ÿ' => 'Y',

        // Consonants with diacritics and special Latin characters
        'ñ' => 'n',
        'Ñ' => 'N',
        'ç' => 'c',
        'Ç' => 'C',

        // Germanic and Nordic special characters
        'ß' => 's',    // German sharp s (Eszett)
        'æ' => 'a',    // Old English / Nordic ash
        'Æ' => 'A',
        'œ' => 'o',    // French / Latin ligature
        'Œ' => 'O',
        'ð' => 'd',    // Old English / Icelandic eth
        'Ð' => 'D',
        'þ' => 't',    // Old English / Icelandic thorn
        'Þ' => 'T',

        // No mapping found — return original character
        _ => c
    };
}
