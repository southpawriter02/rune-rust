using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Tests for Quest entity category functionality.
/// </summary>
[TestFixture]
public class QuestCategoryEntityTests
{
    [Test]
    public void Quest_DefaultCategory_IsSide()
    {
        // Arrange & Act
        var quest = new Quest("test-quest", "Test Quest", "A test quest description");

        // Assert
        quest.Category.Should().Be(QuestCategory.Side);
    }

    [Test]
    public void SetCategory_UpdatesCategory()
    {
        // Arrange
        var quest = new Quest("test-quest", "Test Quest", "A test quest description");

        // Act
        quest.SetCategory(QuestCategory.Main);

        // Assert
        quest.Category.Should().Be(QuestCategory.Main);
    }

    [Test]
    public void IsDaily_TrueWhenCategoryIsDaily()
    {
        // Arrange
        var quest = new Quest("test-quest", "Test Quest", "A test quest description");

        // Act
        quest.SetCategory(QuestCategory.Daily);

        // Assert
        quest.IsDaily.Should().BeTrue();
    }

    [Test]
    public void CanBeAbandoned_FalseForMainQuests()
    {
        // Arrange
        var quest = new Quest("test-quest", "Test Quest", "A test quest description");
        quest.SetCategory(QuestCategory.Main);

        // Act & Assert
        quest.CanBeAbandoned().Should().BeFalse();
    }

    [Test]
    public void CanBeAbandoned_TrueForSideQuests()
    {
        // Arrange
        var quest = new Quest("test-quest", "Test Quest", "A test quest description");
        quest.SetCategory(QuestCategory.Side);

        // Act & Assert
        quest.CanBeAbandoned().Should().BeTrue();
    }
}
