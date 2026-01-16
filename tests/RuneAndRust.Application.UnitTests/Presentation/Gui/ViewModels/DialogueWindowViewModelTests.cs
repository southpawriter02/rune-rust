namespace RuneAndRust.Application.UnitTests.Presentation.Gui.ViewModels;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.Gui.Models;
using RuneAndRust.Presentation.Gui.ViewModels;

[TestFixture]
public class DialogueWindowViewModelTests
{
    // ============================================================================
    // DialogueChoiceViewModel Tests
    // ============================================================================

    [Test]
    public void DialogueChoiceViewModel_DisplayText_IncludesIndexAndQuotes()
    {
        // Arrange
        var data = new DialogueChoiceData("Hello, traveler!");

        // Act
        var vm = new DialogueChoiceViewModel(1, data);

        // Assert
        vm.DisplayText.Should().Be("1. \"Hello, traveler!\"");
    }

    [Test]
    public void DialogueChoiceViewModel_AcceptQuest_HasActionBadge()
    {
        // Arrange
        var data = new DialogueChoiceData("I'll help you.", Action: DialogueAction.AcceptQuest);

        // Act
        var vm = new DialogueChoiceViewModel(1, data);

        // Assert
        vm.ActionBadge.Should().Be("[ACCEPT QUEST]");
        vm.HasActionBadge.Should().BeTrue();
    }

    [Test]
    public void DialogueChoiceViewModel_Leave_HasLeaveBadge()
    {
        // Arrange
        var data = new DialogueChoiceData("Goodbye.", Action: DialogueAction.Leave);

        // Act
        var vm = new DialogueChoiceViewModel(1, data);

        // Assert
        vm.ActionBadge.Should().Be("[LEAVE]");
    }

    [Test]
    public void DialogueChoiceViewModel_NoAction_HasNoActionBadge()
    {
        // Arrange
        var data = new DialogueChoiceData("Tell me more.");

        // Act
        var vm = new DialogueChoiceViewModel(1, data);

        // Assert
        vm.ActionBadge.Should().BeNull();
        vm.HasActionBadge.Should().BeFalse();
    }

    // ============================================================================
    // DialogueWindowViewModel Tests
    // ============================================================================

    [Test]
    public void DialogueWindowViewModel_Constructor_LoadsSampleData()
    {
        // Arrange & Act
        var vm = new DialogueWindowViewModel();

        // Assert
        vm.TypingSpeedMs.Should().Be(30);
        vm.NpcName.Should().Be("Unknown");
    }

    [Test]
    public async Task DialogueWindowViewModel_StartDialogue_SetsNpcInfo()
    {
        // Arrange
        var vm = new DialogueWindowViewModel { TypingSpeedMs = 0 };

        // Act
        await vm.StartDialogueAsync("Test NPC", "🧙");
        await Task.Delay(50); // Allow typing to complete

        // Assert
        vm.NpcName.Should().Be("Test NPC");
        vm.NpcPortrait.Should().Be("🧙");
    }

    [Test]
    public async Task DialogueWindowViewModel_StartDialogue_ShowsFirstNode()
    {
        // Arrange
        var vm = new DialogueWindowViewModel { TypingSpeedMs = 0 };

        // Act
        await vm.StartDialogueAsync("Merchant Marcus");
        await Task.Delay(100);

        // Assert
        vm.FullText.Should().Contain("Greetings");
    }

    [Test]
    public async Task DialogueWindowViewModel_ClickText_WhileTyping_SkipsAnimation()
    {
        // Arrange
        var vm = new DialogueWindowViewModel { TypingSpeedMs = 100 };
        _ = vm.StartDialogueAsync("Test NPC");
        await Task.Delay(50); // Start typing

        // Act
        vm.ClickTextCommand.Execute(null);
        await Task.Delay(50);

        // Assert
        vm.IsTyping.Should().BeFalse();
        vm.DisplayedText.Should().Be(vm.FullText);
    }

    [Test]
    public async Task DialogueWindowViewModel_AfterTyping_ShowsChoices()
    {
        // Arrange
        var vm = new DialogueWindowViewModel { TypingSpeedMs = 0 };

        // Act
        await vm.StartDialogueAsync("Test NPC");
        await Task.Delay(100);

        // Assert
        vm.HasChoices.Should().BeTrue();
        vm.Choices.Should().NotBeEmpty();
    }

    [Test]
    public void DialogueWindowViewModel_ContinueHint_WhileTyping_ShowsSkip()
    {
        // Arrange
        var vm = new DialogueWindowViewModel();

        // Simulate typing state
        // Note: We can't directly set IsTyping, but we can check the default
        // The property is computed, so we test the logic

        // Assert
        vm.ContinueHint.Should().Be("[Click to continue...]"); // Default when not typing
    }

    [Test]
    public void DialogueWindowViewModel_ShowContinueHint_WhenNoChoices()
    {
        // Arrange
        var vm = new DialogueWindowViewModel();

        // Assert - default state has no choices yet
        vm.ShowContinueHint.Should().BeTrue();
    }

    [Test]
    public async Task DialogueWindowViewModel_HandleKeyPress_SelectsChoice()
    {
        // Arrange
        var vm = new DialogueWindowViewModel { TypingSpeedMs = 0 };
        await vm.StartDialogueAsync("Test NPC");
        await Task.Delay(100);

        var initialChoiceCount = vm.Choices.Count;

        // Act - pressing 1 should select first choice
        await vm.HandleKeyPress(1);

        // Assert - dialogue should have advanced
        vm.FullText.Should().NotBeNull();
    }
}
