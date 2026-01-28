using FluentAssertions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Unit tests for the <see cref="WeaponProficiencyLevel"/> enum.
/// </summary>
[TestFixture]
public class WeaponProficiencyLevelTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Enum Value Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that all four expected proficiency levels are defined.
    /// </summary>
    [Test]
    public void WeaponProficiencyLevel_ContainsAllFourLevels()
    {
        // Assert
        Enum.GetValues<WeaponProficiencyLevel>().Should().HaveCount(4);
        Enum.IsDefined(WeaponProficiencyLevel.NonProficient).Should().BeTrue();
        Enum.IsDefined(WeaponProficiencyLevel.Proficient).Should().BeTrue();
        Enum.IsDefined(WeaponProficiencyLevel.Expert).Should().BeTrue();
        Enum.IsDefined(WeaponProficiencyLevel.Master).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that each level has the expected integer value per specification.
    /// </summary>
    [Test]
    [TestCase(WeaponProficiencyLevel.NonProficient, 0)]
    [TestCase(WeaponProficiencyLevel.Proficient, 1)]
    [TestCase(WeaponProficiencyLevel.Expert, 2)]
    [TestCase(WeaponProficiencyLevel.Master, 3)]
    public void WeaponProficiencyLevel_HasExpectedIntegerValue(
        WeaponProficiencyLevel level, int expectedValue)
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
    public void WeaponProficiencyLevel_HigherLevelsAreGreaterThanLowerLevels()
    {
        // Assert - using integer comparison for FluentAssertions compatibility
        ((int)WeaponProficiencyLevel.Proficient)
            .Should().BeGreaterThan((int)WeaponProficiencyLevel.NonProficient);
        ((int)WeaponProficiencyLevel.Expert)
            .Should().BeGreaterThan((int)WeaponProficiencyLevel.Proficient);
        ((int)WeaponProficiencyLevel.Master)
            .Should().BeGreaterThan((int)WeaponProficiencyLevel.Expert);
    }

    /// <summary>
    /// Verifies that proficiency levels can be compared using comparison operators.
    /// </summary>
    [Test]
    public void WeaponProficiencyLevel_CanCompareUsingOperators()
    {
        // Arrange
        var nonProficient = WeaponProficiencyLevel.NonProficient;
        var proficient = WeaponProficiencyLevel.Proficient;
        var expert = WeaponProficiencyLevel.Expert;
        var master = WeaponProficiencyLevel.Master;

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
    [TestCase("NonProficient", WeaponProficiencyLevel.NonProficient)]
    [TestCase("nonproficient", WeaponProficiencyLevel.NonProficient)]
    [TestCase("NONPROFICIENT", WeaponProficiencyLevel.NonProficient)]
    [TestCase("Proficient", WeaponProficiencyLevel.Proficient)]
    [TestCase("Expert", WeaponProficiencyLevel.Expert)]
    [TestCase("Master", WeaponProficiencyLevel.Master)]
    public void TryParse_ValidLevelName_ReturnsTrue(
        string input, WeaponProficiencyLevel expected)
    {
        // Act
        var success = Enum.TryParse<WeaponProficiencyLevel>(input, ignoreCase: true, out var result);

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
        var success = Enum.TryParse<WeaponProficiencyLevel>(input, ignoreCase: true, out _);

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
    [TestCase(0, WeaponProficiencyLevel.NonProficient)]
    [TestCase(1, WeaponProficiencyLevel.Proficient)]
    [TestCase(2, WeaponProficiencyLevel.Expert)]
    [TestCase(3, WeaponProficiencyLevel.Master)]
    public void WeaponProficiencyLevel_CanCastFromInteger(
        int value, WeaponProficiencyLevel expected)
    {
        // Act
        var level = (WeaponProficiencyLevel)value;

        // Assert
        level.Should().Be(expected);
    }
}
