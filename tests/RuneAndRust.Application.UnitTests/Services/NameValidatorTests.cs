// ═══════════════════════════════════════════════════════════════════════════════
// NameValidatorTests.cs
// Unit tests for the NameValidator application service. Verifies character name
// validation against length constraints, allowed character patterns, boundary
// requirements, and profanity filtering. Also tests the Sanitize() method for
// diacritics normalization and invalid character removal, and the IsValid()
// convenience method. Uses Moq for ILogger and FluentAssertions for readable
// assertions.
// Version: 0.17.5c
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.ValueObjects;

#pragma warning disable NUnit2045 // Use Assert.Multiple

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="NameValidator"/>.
/// </summary>
/// <remarks>
/// <para>
/// Tests cover all major validation scenarios organized by category:
/// </para>
/// <list type="bullet">
///   <item><description>Valid name tests — names that should pass all rules</description></item>
///   <item><description>Length boundary tests — null, empty, too short, too long</description></item>
///   <item><description>Character pattern tests — special characters, non-ASCII, boundary violations</description></item>
///   <item><description>Profanity filter tests — exact match, substring match</description></item>
///   <item><description>Sanitization tests — diacritics normalization, character removal, unsalvageable input</description></item>
///   <item><description>IsValid convenience tests — quick boolean validation</description></item>
/// </list>
/// <para>
/// All tests use <see cref="NameValidationConfig.Default"/> unless a custom configuration
/// is needed (profanity filter tests). The logger is mocked via Moq but not verified,
/// as logging is a diagnostic concern.
/// </para>
/// </remarks>
/// <seealso cref="NameValidator"/>
/// <seealso cref="NameValidationResult"/>
/// <seealso cref="NameValidationConfig"/>
[TestFixture]
public class NameValidatorTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Mock logger for the NameValidator. Not verified — logging is diagnostic only.
    /// </summary>
    private Mock<ILogger<NameValidator>> _loggerMock = null!;

    /// <summary>
    /// Default configuration used by most tests. MinLength=2, MaxLength=20, no profanity filter.
    /// </summary>
    private NameValidationConfig _defaultConfig;

    // ═══════════════════════════════════════════════════════════════════════════
    // SETUP
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes mock logger and default configuration before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<NameValidator>>();
        _defaultConfig = NameValidationConfig.Default;
    }

    /// <summary>
    /// Creates a NameValidator instance with the provided or default configuration.
    /// </summary>
    /// <param name="config">
    /// Optional configuration override. When null, uses <see cref="_defaultConfig"/>.
    /// </param>
    /// <returns>A configured <see cref="NameValidator"/> instance.</returns>
    private NameValidator CreateValidator(NameValidationConfig? config = null) =>
        new(config ?? _defaultConfig, _loggerMock.Object);

    // ═══════════════════════════════════════════════════════════════════════════
    // VALID NAME TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Validate() returns a valid result for names that meet all rules:
    /// ASCII letters, spaces, hyphens, within length bounds, starts/ends with letter.
    /// </summary>
    /// <param name="name">A name that should pass all validation rules.</param>
    [TestCase("Bjorn")]
    [TestCase("Dark-Blade")]
    [TestCase("Bjorn the Swift")]
    [TestCase("Al")]
    [TestCase("Jo")]
    [Test]
    public void Validate_WithValidName_ReturnsValid(string name)
    {
        // Arrange
        var validator = CreateValidator();

        // Act
        var result = validator.Validate(name);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
        result.SuggestedName.Should().BeNull();
    }

    /// <summary>
    /// Validate() returns valid for a name at exactly the maximum length (20 characters).
    /// Boundary test: length == MaxLength should pass.
    /// </summary>
    [Test]
    public void Validate_WithMaxLengthName_ReturnsValid()
    {
        // Arrange
        var validator = CreateValidator();
        var name = new string('a', 20); // Exactly 20 characters — at the boundary

        // Act
        var result = validator.Validate(name);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // LENGTH BOUNDARY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Validate() returns invalid with "required" error when name is null.
    /// Rule 1 (Required) triggers before any other validation.
    /// </summary>
    [Test]
    public void Validate_WithNullName_ReturnsInvalid()
    {
        // Arrange
        var validator = CreateValidator();

        // Act
        var result = validator.Validate(null);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("required");
        result.SuggestedName.Should().BeNull();
    }

    /// <summary>
    /// Validate() returns invalid with "required" error when name is empty string.
    /// Empty string is treated as whitespace-only by string.IsNullOrWhiteSpace.
    /// </summary>
    [Test]
    public void Validate_WithEmptyName_ReturnsInvalid()
    {
        // Arrange
        var validator = CreateValidator();

        // Act
        var result = validator.Validate("");

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("required");
    }

    /// <summary>
    /// Validate() returns invalid with "at least 2" error for single-character names.
    /// Rule 2 (MinLength) triggers: length 1 < MinLength 2.
    /// </summary>
    [Test]
    public void Validate_WithSingleCharacter_ReturnsInvalid()
    {
        // Arrange
        var validator = CreateValidator();

        // Act
        var result = validator.Validate("A");

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("at least 2");
    }

    /// <summary>
    /// Validate() returns invalid with "at most 20" error and a truncated suggestion
    /// for names exceeding the maximum length.
    /// Rule 3 (MaxLength) triggers and provides a truncated suggestion.
    /// </summary>
    [Test]
    public void Validate_WithExcessiveLength_ReturnsInvalidWithSuggestion()
    {
        // Arrange
        var validator = CreateValidator();
        var name = new string('a', 25); // 25 characters — 5 over the maximum of 20

        // Act
        var result = validator.Validate(name);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("at most 20");
        result.SuggestedName.Should().NotBeNull();
        result.SuggestedName!.Length.Should().Be(20);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CHARACTER PATTERN TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Validate() returns invalid with "letters, spaces, and hyphens" error for names
    /// containing digits, symbols, or other non-allowed characters.
    /// Rule 5 (AllowedCharacters) triggers for various special character types.
    /// </summary>
    /// <param name="name">A name containing at least one disallowed character.</param>
    [TestCase("Test1Name")]
    [TestCase("Test@Name")]
    [TestCase("Test!Name")]
    [TestCase("Test_Name")]
    [Test]
    public void Validate_WithSpecialCharacters_ReturnsInvalid(string name)
    {
        // Arrange
        var validator = CreateValidator();

        // Act
        var result = validator.Validate(name);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("letters, spaces, and hyphens");
    }

    /// <summary>
    /// Validate() returns invalid with a suggestion for names containing non-ASCII
    /// letters (e.g., ö). The sanitizer converts diacritics to ASCII equivalents.
    /// Rule 5 (AllowedCharacters) triggers because ö has char value > 127.
    /// </summary>
    [Test]
    public void Validate_WithNonAsciiLetters_ReturnsInvalidWithSuggestion()
    {
        // Arrange
        var validator = CreateValidator();

        // Act
        var result = validator.Validate("Björn");

        // Assert
        result.IsValid.Should().BeFalse();
        result.SuggestedName.Should().Be("Bjorn");
    }

    /// <summary>
    /// Validate() returns invalid with "start and end with a letter" error when
    /// the name begins with a hyphen.
    /// Rule 4 (Boundary) triggers: first character is not a letter.
    /// </summary>
    [Test]
    public void Validate_WithLeadingHyphen_ReturnsInvalid()
    {
        // Arrange
        var validator = CreateValidator();

        // Act
        var result = validator.Validate("-Blade");

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("start and end with a letter");
    }

    /// <summary>
    /// Validate() returns invalid with "start and end with a letter" error when
    /// the name ends with a hyphen.
    /// Rule 4 (Boundary) triggers: last character is not a letter.
    /// </summary>
    [Test]
    public void Validate_WithTrailingHyphen_ReturnsInvalid()
    {
        // Arrange
        var validator = CreateValidator();

        // Act
        var result = validator.Validate("Blade-");

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("start and end with a letter");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PROFANITY FILTER TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Validate() returns invalid with "different name" error when the name exactly
    /// matches a profanity filter entry. No suggestion is provided for profanity matches.
    /// Rule 6 (Profanity) triggers with case-insensitive matching.
    /// </summary>
    [Test]
    public void Validate_WithProfanity_ReturnsInvalid()
    {
        // Arrange — configure a custom profanity filter with a test word
        var config = _defaultConfig with
        {
            ProfanityFilter = new[] { "badword" }
        };
        var validator = CreateValidator(config);

        // Act
        var result = validator.Validate("badword");

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("different name");
        result.SuggestedName.Should().BeNull(); // No suggestion for profanity
    }

    /// <summary>
    /// Validate() returns invalid when a profanity filter term appears as a substring
    /// within the name. Prevents evasion by embedding blocked words in longer names.
    /// Rule 6 (Profanity) triggers via substring matching.
    /// </summary>
    [Test]
    public void Validate_WithProfanitySubstring_ReturnsInvalid()
    {
        // Arrange — "bad" appears as substring in "MyBadName"
        var config = _defaultConfig with
        {
            ProfanityFilter = new[] { "bad" }
        };
        var validator = CreateValidator(config);

        // Act
        var result = validator.Validate("MyBadName");

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("different name");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SANITIZATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Sanitize() converts accented characters to their ASCII equivalents.
    /// The diacritics normalization maps ö → o, preserving the intended name.
    /// </summary>
    [Test]
    public void Sanitize_WithDiacritics_ReturnsNormalized()
    {
        // Arrange
        var validator = CreateValidator();

        // Act
        var result = validator.Sanitize("Björn");

        // Assert
        result.Should().Be("Bjorn");
    }

    /// <summary>
    /// Sanitize() removes non-letter, non-space, non-hyphen characters while
    /// preserving the remaining letters.
    /// </summary>
    [Test]
    public void Sanitize_WithSpecialCharacters_RemovesThem()
    {
        // Arrange
        var validator = CreateValidator();

        // Act
        var result = validator.Sanitize("Test!!Name");

        // Assert
        result.Should().Be("TestName");
    }

    /// <summary>
    /// Sanitize() returns null when the input contains only non-letter characters
    /// that cannot be converted to valid letters. The result would be too short
    /// or empty after removing all invalid characters.
    /// </summary>
    [Test]
    public void Sanitize_WithOnlyInvalidCharacters_ReturnsNull()
    {
        // Arrange
        var validator = CreateValidator();

        // Act
        var result = validator.Sanitize("12345");

        // Assert
        result.Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // IsValid CONVENIENCE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// IsValid() returns true for a name that passes all validation rules.
    /// Delegates to Validate() and returns only the IsValid flag.
    /// </summary>
    [Test]
    public void IsValid_WithValidName_ReturnsTrue()
    {
        // Arrange
        var validator = CreateValidator();

        // Act & Assert
        validator.IsValid("ValidName").Should().BeTrue();
    }

    /// <summary>
    /// IsValid() returns false for an empty name that fails validation.
    /// Delegates to Validate() and returns only the IsValid flag.
    /// </summary>
    [Test]
    public void IsValid_WithInvalidName_ReturnsFalse()
    {
        // Arrange
        var validator = CreateValidator();

        // Act & Assert
        validator.IsValid("").Should().BeFalse();
    }
}
