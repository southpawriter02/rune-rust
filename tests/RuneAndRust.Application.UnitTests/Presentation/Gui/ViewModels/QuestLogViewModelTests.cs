namespace RuneAndRust.Application.UnitTests.Presentation.Gui.ViewModels;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.Gui.ViewModels;

[TestFixture]
public class QuestLogViewModelTests
{
    // ============================================================================
    // ObjectiveViewModel Tests
    // ============================================================================

    [Test]
    public void ObjectiveViewModel_CompletedObjective_ShowsCheckbox()
    {
        // Arrange & Act
        var vm = new ObjectiveViewModel("Kill goblins", 5, 5);

        // Assert
        vm.IsCompleted.Should().BeTrue();
        vm.StatusIcon.Should().Be("☑");
    }

    [Test]
    public void ObjectiveViewModel_IncompleteObjective_ShowsEmptyBox()
    {
        // Arrange & Act
        var vm = new ObjectiveViewModel("Find treasure", 0, 1);

        // Assert
        vm.IsCompleted.Should().BeFalse();
        vm.StatusIcon.Should().Be("☐");
    }

    [Test]
    public void ObjectiveViewModel_WithProgress_ShowsProgressText()
    {
        // Arrange & Act
        var vm = new ObjectiveViewModel("Collect herbs", 3, 10);

        // Assert
        vm.HasProgress.Should().BeTrue();
        vm.ProgressText.Should().Be("3/10");
    }

    // ============================================================================
    // RewardViewModel Tests
    // ============================================================================

    [Test]
    public void RewardViewModel_GoldReward_HasGoldIcon()
    {
        // Arrange & Act
        var vm = new RewardViewModel("Gold", "100 Gold");

        // Assert
        vm.Icon.Should().Be("💰");
    }

    [Test]
    public void RewardViewModel_XPReward_HasSparkleIcon()
    {
        // Arrange & Act
        var vm = new RewardViewModel("Experience", "500 XP");

        // Assert
        vm.Icon.Should().Be("✨");
    }

    // ============================================================================
    // QuestEntryViewModel Tests
    // ============================================================================

    [Test]
    public void QuestEntryViewModel_AddObjective_UpdatesProgress()
    {
        // Arrange
        var vm = new QuestEntryViewModel(Guid.NewGuid(), "Test Quest", "Description");

        // Act
        vm.AddObjective("Objective 1", 1, 1);
        vm.AddObjective("Objective 2", 0, 1);

        // Assert
        vm.Objectives.Should().HaveCount(2);
        vm.ProgressPercentage.Should().Be(50);
    }

    [Test]
    public void QuestEntryViewModel_AllObjectivesComplete_ShowsFullProgress()
    {
        // Arrange
        var vm = new QuestEntryViewModel(Guid.NewGuid(), "Test Quest", "Description");

        // Act
        vm.AddObjective("Objective 1", 1, 1);
        vm.AddObjective("Objective 2", 2, 2);

        // Assert
        vm.ProgressPercentage.Should().Be(100);
    }

    [Test]
    public void QuestEntryViewModel_AddReward_UpdatesRewards()
    {
        // Arrange
        var vm = new QuestEntryViewModel(Guid.NewGuid(), "Test Quest", "Description");

        // Act
        vm.AddReward("Gold", "100 Gold");
        vm.AddReward("Experience", "500 XP");

        // Assert
        vm.Rewards.Should().HaveCount(2);
    }

    [Test]
    public void QuestEntryViewModel_ProgressText_ShowsCorrectFormat()
    {
        // Arrange
        var vm = new QuestEntryViewModel(Guid.NewGuid(), "Test Quest", "Description");
        vm.AddObjective("Obj 1", 1, 1);
        vm.AddObjective("Obj 2", 0, 1);
        vm.AddObjective("Obj 3", 0, 1);

        // Assert
        vm.ProgressText.Should().Be("Progress: 1/3 objectives");
    }

    // ============================================================================
    // QuestLogWindowViewModel Tests
    // ============================================================================

    [Test]
    public void QuestLogWindowViewModel_Constructor_LoadsSampleData()
    {
        // Arrange & Act
        var vm = new QuestLogWindowViewModel();

        // Assert
        vm.ActiveQuests.Should().NotBeEmpty();
        vm.CompletedQuests.Should().NotBeEmpty();
    }

    [Test]
    public void QuestLogWindowViewModel_SelectQuest_SetsSelectedQuest()
    {
        // Arrange
        var vm = new QuestLogWindowViewModel();
        var quest = vm.ActiveQuests.First();

        // Act
        vm.SelectQuestCommand.Execute(quest);

        // Assert
        vm.SelectedQuest.Should().Be(quest);
        vm.HasSelectedQuest.Should().BeTrue();
    }

    [Test]
    public void QuestLogWindowViewModel_TrackQuest_UpdatesTrackedStatus()
    {
        // Arrange
        var vm = new QuestLogWindowViewModel();
        var quest = vm.ActiveQuests.Skip(1).First();
        vm.SelectQuestCommand.Execute(quest);

        // Act
        vm.TrackQuestCommand.Execute(null);

        // Assert
        quest.IsTracked.Should().BeTrue();
        vm.TrackedQuestId.Should().Be(quest.QuestId);
    }

    [Test]
    public void QuestLogWindowViewModel_ToggleCompletedQuests_TogglesVisibility()
    {
        // Arrange
        var vm = new QuestLogWindowViewModel();
        vm.ShowCompletedQuests.Should().BeTrue();

        // Act
        vm.ToggleCompletedQuestsCommand.Execute(null);

        // Assert
        vm.ShowCompletedQuests.Should().BeFalse();
        vm.CompletedExpander.Should().Be("►");
    }

    [Test]
    public void QuestLogWindowViewModel_TrackQuest_UntracksOther()
    {
        // Arrange
        var vm = new QuestLogWindowViewModel();
        var firstQuest = vm.ActiveQuests.First();
        var secondQuest = vm.ActiveQuests.Skip(1).First();
        firstQuest.IsTracked = true;

        // Act
        vm.SelectQuestCommand.Execute(secondQuest);
        vm.TrackQuestCommand.Execute(null);

        // Assert
        firstQuest.IsTracked.Should().BeFalse();
        secondQuest.IsTracked.Should().BeTrue();
    }

    [Test]
    public void QuestLogWindowViewModel_CanTrackQuest_FalseForCompletedQuest()
    {
        // Arrange
        var vm = new QuestLogWindowViewModel();
        var completedQuest = vm.CompletedQuests.First();

        // Act
        vm.SelectQuestCommand.Execute(completedQuest);

        // Assert
        vm.CanTrackQuest.Should().BeFalse();
    }
}
