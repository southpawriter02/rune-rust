using RuneAndRust.Domain.Validation;

namespace RuneAndRust.Domain.UnitTests.Validation;

[TestFixture]
public class PlayerNameValidatorTests
{
    [Test]
    public void Validate_ValidName_ReturnsSuccess()
    {
        var result = PlayerNameValidator.Validate("Thorin");
        Assert.That(result.IsValid, Is.True);
        Assert.That(result.ErrorMessage, Is.Null);
    }

    [Test]
    public void Validate_MinimumLength_ReturnsSuccess()
    {
        var result = PlayerNameValidator.Validate("Al");
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_MaximumLength_ReturnsSuccess()
    {
        var name = new string('A', 30);
        var result = PlayerNameValidator.Validate(name);
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_NameWithSpaces_ReturnsSuccess()
    {
        var result = PlayerNameValidator.Validate("John Smith");
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_NameWithHyphen_ReturnsSuccess()
    {
        var result = PlayerNameValidator.Validate("Mary-Jane");
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_NameWithApostrophe_ReturnsSuccess()
    {
        var result = PlayerNameValidator.Validate("O'Brien");
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_NullName_ReturnsFalse()
    {
        var result = PlayerNameValidator.Validate(null);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.ErrorMessage, Is.EqualTo("Name cannot be empty."));
    }

    [Test]
    public void Validate_EmptyString_ReturnsFalse()
    {
        var result = PlayerNameValidator.Validate("");
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.ErrorMessage, Contains.Substring("empty"));
    }

    [Test]
    public void Validate_WhitespaceOnly_ReturnsFalse()
    {
        var result = PlayerNameValidator.Validate("   ");
        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void Validate_TooShort_ReturnsFalse()
    {
        var result = PlayerNameValidator.Validate("A");
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.ErrorMessage, Contains.Substring("at least 2"));
    }

    [Test]
    public void Validate_TooLong_ReturnsFalse()
    {
        var name = new string('A', 31);
        var result = PlayerNameValidator.Validate(name);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.ErrorMessage, Contains.Substring("exceed 30"));
    }

    [Test]
    public void Validate_StartsWithNumber_ReturnsFalse()
    {
        var result = PlayerNameValidator.Validate("1Thorin");
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.ErrorMessage, Contains.Substring("start with a letter"));
    }

    [Test]
    public void Validate_ContainsNumbers_ReturnsFalse()
    {
        var result = PlayerNameValidator.Validate("Th0r1n");
        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void Validate_ConsecutiveSpaces_ReturnsFalse()
    {
        var result = PlayerNameValidator.Validate("John  Smith");
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.ErrorMessage, Contains.Substring("consecutive spaces"));
    }

    [Test]
    public void Normalize_TrimsWhitespace()
    {
        var result = PlayerNameValidator.Normalize("  Thorin  ");
        Assert.That(result, Is.EqualTo("Thorin"));
    }

    [Test]
    public void Normalize_CollapsesMultipleSpaces()
    {
        var result = PlayerNameValidator.Normalize("John   Smith");
        Assert.That(result, Is.EqualTo("John Smith"));
    }
}
