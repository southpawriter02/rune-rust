using FluentAssertions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Unit tests for the <see cref="ArmorProficiencyLevel"/> enum.
/// </summary>
[TestFixture]
public class ArmorProficiencyLevelTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Enum Value Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that all four expected proficiency levels are defined.
    /// </summary>
    [Test]
    public void ArmorProficiencyLevel_ContainsAllFourLevels()
    {
        // Assert
        Enum.GetValues<ArmorProficiencyLevel>().Should().HaveCount(4);
        Enum.IsDefined(ArmorProficiencyLevel.NonProficient).Should().BeTrue();
        Enum.IsDefined(ArmorProficiencyLevel.Proficient).Should().BeTrue();
        Enum.IsDefined(ArmorProficiencyLevel.Expert).Should().BeTrue();
        Enum.IsDefined(ArmorProficiencyLevel.Master).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that each level has the expected integer value per specification.
    /// </summary>
    [Test]
    [TestCase(ArmorProficiencyLevel.NonProficient, 0)]
    [TestCase(ArmorProficiencyLevel.Proficient, 1)]
    [TestCase(ArmorProficiencyLevel.Expert, 2)]
    [TestCase(ArmorProficiencyLevel.Master, 3)]
    public void ArmorProficiencyLevel_HasExpectedIntegerValue(
        ArmorProficiencyLevel level, int expectedValue)
    {
        // Assert
        ((int)level).Should().Be(expectedValue);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Comparison Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that higher levels compare as greater than lower levels.
    /// </summary>
    [Test]
    public void ArmorProficiencyLevel_HigherLevelsAreGreaterThanLowerLevels()
    {
        // Assert - using integer comparison for FluentAssertions compatibility
        ((int)ArmorProficiencyLevel.Proficient)
            .Should().BeGreaterThan((int)ArmorProficiencyLevel.NonProficient);
        ((int)ArmorProficiencyLevel.Expert)
            .Should().BeGreaterThan((int)ArmorProficiencyLevel.Proficient);
        ((int)ArmorProficiencyLevel.Master)
            .Should().BeGreaterThan((int)ArmorProficiencyLevel.Expert);
    }

    /// <summary>
    /// Verifies that proficiency levels can be compared using comparison operators.
    /// </summary>
    [Test]
    public void ArmorProficiencyLevel_CanCompareUsingOperators()
    {
        // Arrange
        var nonProficient = ArmorProficiencyLevel.NonProficient;
        var proficient = ArmorProficiencyLevel.Proficient;
        var expert = ArmorProficiencyLevel.Expert;
        var master = ArmorProficiencyLevel.Master;

        // Assert - native enum comparison
        (master > expert).Should().BeTrue();
        (expert >= proficient).Should().BeTrue();
        (nonProficient < proficient).Should().BeTrue();
        (proficient <= expert).Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Parsing Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that level strings can be parsed case-insensitively.
    /// </summary>
    [Test]
    [TestCase("NonProficient", ArmorProficiencyLevel.NonProficient)]
    [TestCase("nonproficient", ArmorProficiencyLevel.NonProficient)]
    [TestCase("NONPROFICIENT", ArmorProficiencyLevel.NonProficient)]
    [TestCase("Proficient", ArmorProficiencyLevel.Proficient)]
    [TestCase("Expert", ArmorProficiencyLevel.Expert)]
    [TestCase("Master", ArmorProficiencyLevel.Master)]
    public void TryParse_ValidLevelName_ReturnsTrue(
        string input, ArmorProficiencyLevel expected)
    {
        // Act
        var success = Enum.TryParse<ArmorProficiencyLevel>(input, ignoreCase: true, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that invalid level names fail to parse.
    /// </summary>
    [Test]
    [TestCase("Invalid")]
    [TestCase("Beginner")]
    [TestCase("Novice")]
    [TestCase("")]
    public void TryParse_InvalidLevelName_ReturnsFalse(string input)
    {
        // Act
        var success = Enum.TryParse<ArmorProficiencyLevel>(input, ignoreCase: true, out _);

        // Assert
        success.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Casting Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that integer values can be cast to proficiency levels.
    /// </summary>
    [Test]
    [TestCase(0, ArmorProficiencyLevel.NonProficient)]
    [TestCase(1, ArmorProficiencyLevel.Proficient)]
    [TestCase(2, ArmorProficiencyLevel.Expert)]
    [TestCase(3, ArmorProficiencyLevel.Master)]
    public void ArmorProficiencyLevel_CanCastFromInteger(
        int value, ArmorProficiencyLevel expected)
    {
        // Act
        var level = (ArmorProficiencyLevel)value;

        // Assert
        level.Should().Be(expected);
    }
}
