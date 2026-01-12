using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Tests for QuestJournalFilter value object.
/// </summary>
[TestFixture]
public class QuestJournalFilterTests
{
    [Test]
    public void Default_ReturnsFilterWithNoConstraints()
    {
        // Act
        var filter = QuestJournalFilter.Default;

        // Assert
        filter.Category.Should().BeNull();
        filter.Status.Should().BeNull();
        filter.TimedOnly.Should().BeFalse();
        filter.ChainsOnly.Should().BeFalse();
        filter.SearchText.Should().BeNull();
        filter.HasActiveFilter.Should().BeFalse();
    }

    [Test]
    public void ForCategory_CreatesCategoryFilter()
    {
        // Act
        var filter = QuestJournalFilter.ForCategory(QuestCategory.Daily);

        // Assert
        filter.Category.Should().Be(QuestCategory.Daily);
        filter.HasActiveFilter.Should().BeTrue();
    }

    [Test]
    public void ActiveOnly_CreatesActiveStatusFilter()
    {
        // Act
        var filter = QuestJournalFilter.ActiveOnly;

        // Assert
        filter.Status.Should().Be(QuestStatus.Active);
        filter.HasActiveFilter.Should().BeTrue();
    }

    [Test]
    public void Apply_FiltersByCategory()
    {
        // Arrange
        var quests = CreateTestQuests();
        var filter = QuestJournalFilter.ForCategory(QuestCategory.Main);

        // Act
        var result = filter.Apply(quests).ToList();

        // Assert
        result.Should().AllSatisfy(q => q.Category.Should().Be(QuestCategory.Main));
        result.Should().HaveCount(1);
    }

    [Test]
    public void Apply_FiltersByMultipleCriteria()
    {
        // Arrange
        var quests = CreateTestQuests();
        var filter = new QuestJournalFilter
        {
            Category = QuestCategory.Side,
            Status = QuestStatus.Active
        };

        // Act
        var result = filter.Apply(quests).ToList();

        // Assert
        result.Should().AllSatisfy(q =>
        {
            q.Category.Should().Be(QuestCategory.Side);
            q.Status.Should().Be(QuestStatus.Active);
        });
    }

    [Test]
    public void Apply_FiltersBySearchText()
    {
        // Arrange
        var quests = CreateTestQuests();
        var filter = QuestJournalFilter.WithSearch("goblin");

        // Act
        var result = filter.Apply(quests).ToList();

        // Assert
        result.Should().AllSatisfy(q =>
            (q.Name.Contains("goblin", StringComparison.OrdinalIgnoreCase) ||
             q.Description.Contains("goblin", StringComparison.OrdinalIgnoreCase))
            .Should().BeTrue());
    }

    [Test]
    public void GetDisplayName_ReturnsFormattedDescription()
    {
        // Arrange
        var filter = new QuestJournalFilter
        {
            Category = QuestCategory.Daily,
            Status = QuestStatus.Active
        };

        // Act
        var displayName = filter.GetDisplayName();

        // Assert
        displayName.Should().Be("Daily, Active");
    }

    private static List<Quest> CreateTestQuests()
    {
        var mainQuest = new Quest("main-quest", "Main Story", "The main story quest");
        mainQuest.SetCategory(QuestCategory.Main);
        mainQuest.Activate();

        var sideQuest = new Quest("side-quest", "Goblin Hunt", "Hunt the goblins");
        sideQuest.SetCategory(QuestCategory.Side);
        sideQuest.Activate();

        var dailyQuest = new Quest("daily-quest", "Daily Training", "Train with dummies");
        dailyQuest.SetCategory(QuestCategory.Daily);

        return [mainQuest, sideQuest, dailyQuest];
    }
}
