using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Tests for QuestCategory enum.
/// </summary>
[TestFixture]
public class QuestCategoryTests
{
    [Test]
    public void QuestCategory_HasExpectedValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<QuestCategory>();

        // Assert
        values.Should().Contain(QuestCategory.Main);
        values.Should().Contain(QuestCategory.Side);
        values.Should().Contain(QuestCategory.Daily);
        values.Should().Contain(QuestCategory.Repeatable);
        values.Should().Contain(QuestCategory.Event);
        values.Should().HaveCount(5);
    }

    [Test]
    public void QuestCategory_Main_IsFirst()
    {
        // Assert
        ((int)QuestCategory.Main).Should().Be(0);
    }

    [Test]
    public void QuestCategory_CanBeConvertedToString()
    {
        // Assert
        QuestCategory.Daily.ToString().Should().Be("Daily");
        QuestCategory.Repeatable.ToString().Should().Be("Repeatable");
    }

    [Test]
    public void QuestCategory_CanBeParsedFromString()
    {
        // Assert
        Enum.Parse<QuestCategory>("Repeatable").Should().Be(QuestCategory.Repeatable);
        Enum.Parse<QuestCategory>("Event").Should().Be(QuestCategory.Event);
    }
}
