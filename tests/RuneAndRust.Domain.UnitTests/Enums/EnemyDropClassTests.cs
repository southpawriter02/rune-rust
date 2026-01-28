using FluentAssertions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Unit tests for the <see cref="EnemyDropClass"/> enum.
/// </summary>
[TestFixture]
public class EnemyDropClassTests
{
    /// <summary>
    /// Verifies that each class has the expected integer value.
    /// </summary>
    [Test]
    [TestCase(EnemyDropClass.Trash, 0)]
    [TestCase(EnemyDropClass.Standard, 1)]
    [TestCase(EnemyDropClass.Elite, 2)]
    [TestCase(EnemyDropClass.MiniBoss, 3)]
    [TestCase(EnemyDropClass.Boss, 4)]
    public void EnemyDropClass_AllClassesHaveExpectedValues_ValuesMatchSpecification(
        EnemyDropClass dropClass, int expectedValue)
    {
        // Assert
        ((int)dropClass).Should().Be(expectedValue);
    }

    /// <summary>
    /// Verifies that exactly 5 drop classes are defined.
    /// </summary>
    [Test]
    public void EnemyDropClass_AllFiveClassesDefined_CountEquals5()
    {
        // Act
        var values = Enum.GetValues<EnemyDropClass>();

        // Assert
        values.Should().HaveCount(5);
    }

    /// <summary>
    /// Verifies that enum values can be parsed from string names.
    /// </summary>
    [Test]
    [TestCase("Trash", EnemyDropClass.Trash)]
    [TestCase("Standard", EnemyDropClass.Standard)]
    [TestCase("Elite", EnemyDropClass.Elite)]
    [TestCase("MiniBoss", EnemyDropClass.MiniBoss)]
    [TestCase("Boss", EnemyDropClass.Boss)]
    public void EnemyDropClass_CanParseFromString_ReturnsCorrectValue(
        string name, EnemyDropClass expected)
    {
        // Act
        var result = Enum.Parse<EnemyDropClass>(name);

        // Assert
        result.Should().Be(expected);
    }
}
