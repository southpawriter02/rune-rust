using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.ViewModels;
using RuneAndRust.Terminal.Rendering;
using RuneAndRust.Terminal.Services;
using Xunit;

namespace RuneAndRust.Tests.Terminal;

/// <summary>
/// Tests for the JournalScreenRenderer class (v0.3.7c).
/// Note: These tests focus on renderer construction and edge cases.
/// Full visual rendering tests are done via manual integration testing.
/// </summary>
public class JournalScreenRendererTests
{
    private readonly Mock<ILogger<JournalScreenRenderer>> _mockLogger;
    private readonly JournalScreenRenderer _sut;

    public JournalScreenRendererTests()
    {
        _mockLogger = new Mock<ILogger<JournalScreenRenderer>>();
        _sut = new JournalScreenRenderer(_mockLogger.Object);
    }

    #region Render Tests

    [Fact]
    public void Render_WithEmptyEntryList_DoesNotThrow()
    {
        // Arrange
        var vm = CreateEmptyViewModel();

        // Act
        var action = () => _sut.Render(vm);

        // Assert - Should not throw even with empty entry list
        action.Should().NotThrow();
    }

    [Fact]
    public void Render_WithEntries_DoesNotThrow()
    {
        // Arrange
        var vm = CreateViewModelWithEntries();

        // Act
        var action = () => _sut.Render(vm);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Render_WithSelectedEntry_DoesNotThrow()
    {
        // Arrange
        var vm = CreateViewModelWithEntries() with { SelectedEntryIndex = 1 };

        // Act
        var action = () => _sut.Render(vm);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Render_WithEntryDetails_DoesNotThrow()
    {
        // Arrange
        var vm = CreateViewModelWithDetails();

        // Act
        var action = () => _sut.Render(vm);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Render_WithNullDetails_DoesNotThrow()
    {
        // Arrange
        var vm = CreateViewModelWithEntries() with { SelectedDetail = null };

        // Act
        var action = () => _sut.Render(vm);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Render_WithContractsTab_DoesNotThrow()
    {
        // Arrange
        var vm = CreateEmptyViewModel() with { ActiveTab = JournalTab.Contracts };

        // Act
        var action = () => _sut.Render(vm);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Render_WithHighStressLevel_DoesNotThrow()
    {
        // Arrange
        var vm = CreateViewModelWithDetails() with { StressLevel = 75 };

        // Act
        var action = () => _sut.Render(vm);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Render_WithCompleteEntry_DoesNotThrow()
    {
        // Arrange
        var entries = new List<JournalEntryView>
        {
            new JournalEntryView(
                Index: 1,
                EntryId: Guid.NewGuid(),
                Title: "Complete Entry",
                Category: EntryCategory.Bestiary,
                CompletionPercent: 100,
                IsComplete: true
            )
        };

        var vm = new JournalViewModel(
            CharacterName: "Test Scholar",
            StressLevel: 0,
            ActiveTab: JournalTab.Bestiary,
            Entries: entries,
            SelectedEntryIndex: 0,
            SelectedDetail: null
        );

        // Act
        var action = () => _sut.Render(vm);

        // Assert
        action.Should().NotThrow();
    }

    #endregion

    #region JournalViewHelper Tests

    [Theory]
    [InlineData(JournalTab.Codex, "Codex")]
    [InlineData(JournalTab.Bestiary, "Bestiary")]
    [InlineData(JournalTab.FieldGuide, "Field Guide")]
    [InlineData(JournalTab.Contracts, "Contracts")]
    public void GetTabDisplayName_ReturnsCorrectName(JournalTab tab, string expectedName)
    {
        // Act
        var name = JournalViewHelper.GetTabDisplayName(tab);

        // Assert
        name.Should().Be(expectedName);
    }

    [Theory]
    [InlineData(JournalTab.Codex, 'C')]
    [InlineData(JournalTab.Bestiary, 'B')]
    [InlineData(JournalTab.FieldGuide, 'F')]
    [InlineData(JournalTab.Contracts, 'Q')]
    public void GetTabHotkey_ReturnsCorrectHotkey(JournalTab tab, char expectedHotkey)
    {
        // Act
        var hotkey = JournalViewHelper.GetTabHotkey(tab);

        // Assert
        hotkey.Should().Be(expectedHotkey);
    }

    [Fact]
    public void GetTabColor_ReturnsGrey_WhenNotActive()
    {
        // Act
        var color = JournalViewHelper.GetTabColor(JournalTab.Codex, isActive: false);

        // Assert
        color.Should().Be("grey");
    }

    [Theory]
    [InlineData(JournalTab.Codex, "gold1")]
    [InlineData(JournalTab.Bestiary, "red")]
    [InlineData(JournalTab.FieldGuide, "cyan")]
    [InlineData(JournalTab.Contracts, "green")]
    public void GetTabColor_ReturnsCorrectColor_WhenActive(JournalTab tab, string expectedColor)
    {
        // Act
        var color = JournalViewHelper.GetTabColor(tab, isActive: true);

        // Assert
        color.Should().Be(expectedColor);
    }

    [Fact]
    public void GetCompletionIndicator_ReturnsStar_WhenComplete()
    {
        // Act
        var indicator = JournalViewHelper.GetCompletionIndicator(true);

        // Assert
        indicator.Should().Contain("\u2605"); // Star
        indicator.Should().Contain("green");
    }

    [Fact]
    public void GetCompletionIndicator_ReturnsBullet_WhenIncomplete()
    {
        // Act
        var indicator = JournalViewHelper.GetCompletionIndicator(false);

        // Assert
        indicator.Should().Contain("\u25CF"); // Bullet
        indicator.Should().Contain("grey");
    }

    [Theory]
    [InlineData(100, "green")]
    [InlineData(75, "yellow")]
    [InlineData(50, "yellow")]
    [InlineData(49, "grey")]
    [InlineData(0, "grey")]
    public void GetCompletionColor_ReturnsCorrectColor(int percent, string expectedColor)
    {
        // Act
        var color = JournalViewHelper.GetCompletionColor(percent);

        // Assert
        color.Should().Be(expectedColor);
    }

    [Theory]
    [InlineData(EntryCategory.FieldGuide, "\u2139")]  // ℹ
    [InlineData(EntryCategory.BlightOrigin, "\u2623")] // ☣
    [InlineData(EntryCategory.Bestiary, "\u2620")]     // ☠
    [InlineData(EntryCategory.Factions, "\u2694")]     // ⚔
    [InlineData(EntryCategory.Technical, "\u2699")]    // ⚙
    [InlineData(EntryCategory.Geography, "\u2302")]    // ⌂
    public void GetCategoryIcon_ReturnsCorrectIcon(EntryCategory category, string expectedIcon)
    {
        // Act
        var icon = JournalViewHelper.GetCategoryIcon(category);

        // Assert
        icon.Should().Be(expectedIcon);
    }

    [Fact]
    public void FormatThreshold_ConvertsSnakeCaseToTitleCase()
    {
        // Act
        var result = JournalViewHelper.FormatThreshold("WEAKNESS_REVEALED");

        // Assert
        result.Should().Be("Weakness Revealed");
    }

    [Fact]
    public void FormatThreshold_HandlesSingleWord()
    {
        // Act
        var result = JournalViewHelper.FormatThreshold("COMPLETE");

        // Assert
        result.Should().Be("Complete");
    }

    [Fact]
    public void FormatThreshold_HandlesEmptyString()
    {
        // Act
        var result = JournalViewHelper.FormatThreshold("");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ApplyGlitchEffect_ReturnsOriginal_WhenStressBelow50()
    {
        // Arrange
        var text = "Normal text without glitches";

        // Act
        var result = JournalViewHelper.ApplyGlitchEffect(text, 45);

        // Assert
        result.Should().Be(text);
    }

    [Fact]
    public void ApplyGlitchEffect_ModifiesText_WhenStressAbove50()
    {
        // Arrange
        var text = "This text should have some glitched characters";

        // Act
        var result = JournalViewHelper.ApplyGlitchEffect(text, 80);

        // Assert
        result.Should().NotBe(text);
        result.Should().HaveLength(text.Length); // Same length
    }

    [Fact]
    public void ApplyGlitchEffect_PreservesWhitespace()
    {
        // Arrange
        var text = "Word Word Word";

        // Act
        var result = JournalViewHelper.ApplyGlitchEffect(text, 100);

        // Assert
        // Spaces should remain
        result.Should().Contain(" ");
    }

    [Fact]
    public void ApplyGlitchEffect_IsStable_ForSameInput()
    {
        // Arrange
        var text = "Consistent glitch pattern";

        // Act
        var result1 = JournalViewHelper.ApplyGlitchEffect(text, 75);
        var result2 = JournalViewHelper.ApplyGlitchEffect(text, 75);

        // Assert - Same input should produce same output (uses text hash for seed)
        result1.Should().Be(result2);
    }

    [Fact]
    public void FormatFragmentProgress_ShowsCollectedAndRequired()
    {
        // Act
        var result = JournalViewHelper.FormatFragmentProgress(5, 10);

        // Assert
        result.Should().Contain("5");
        result.Should().Contain("10");
        result.Should().Contain("fragments");
    }

    [Fact]
    public void FormatFragmentProgress_UsesGreenWhenComplete()
    {
        // Act
        var result = JournalViewHelper.FormatFragmentProgress(10, 10);

        // Assert
        result.Should().Contain("green");
    }

    [Fact]
    public void FormatFragmentProgress_UsesYellowWhenIncomplete()
    {
        // Act
        var result = JournalViewHelper.FormatFragmentProgress(5, 10);

        // Assert
        result.Should().Contain("yellow");
    }

    #endregion

    #region Helper Methods

    private static JournalViewModel CreateEmptyViewModel()
    {
        return new JournalViewModel(
            CharacterName: "Test Scholar",
            StressLevel: 0,
            ActiveTab: JournalTab.Codex,
            Entries: new List<JournalEntryView>(),
            SelectedEntryIndex: 0,
            SelectedDetail: null
        );
    }

    private static JournalViewModel CreateViewModelWithEntries()
    {
        var entries = new List<JournalEntryView>
        {
            new JournalEntryView(
                Index: 1,
                EntryId: Guid.NewGuid(),
                Title: "Hrimthursar",
                Category: EntryCategory.Bestiary,
                CompletionPercent: 80,
                IsComplete: false
            ),
            new JournalEntryView(
                Index: 2,
                EntryId: Guid.NewGuid(),
                Title: "Iron-Husk",
                Category: EntryCategory.Bestiary,
                CompletionPercent: 45,
                IsComplete: false
            )
        };

        return new JournalViewModel(
            CharacterName: "Test Scholar",
            StressLevel: 0,
            ActiveTab: JournalTab.Bestiary,
            Entries: entries,
            SelectedEntryIndex: 0,
            SelectedDetail: null
        );
    }

    private static JournalViewModel CreateViewModelWithDetails()
    {
        var entries = new List<JournalEntryView>
        {
            new JournalEntryView(
                Index: 1,
                EntryId: Guid.NewGuid(),
                Title: "Hrimthursar",
                Category: EntryCategory.Bestiary,
                CompletionPercent: 80,
                IsComplete: false
            )
        };

        var details = new JournalEntryDetailView(
            EntryId: entries[0].EntryId,
            Title: "Hrimthursar",
            Category: EntryCategory.Bestiary,
            CompletionPercent: 80,
            RedactedContent: "The [grey]████[/] frost giants roam the [grey]████[/] wastes seeking victims.",
            UnlockedThresholds: new List<string> { "WEAKNESS_REVEALED", "HABITAT_REVEALED" },
            FragmentsCollected: 10,
            FragmentsRequired: 12
        );

        return new JournalViewModel(
            CharacterName: "Test Scholar",
            StressLevel: 0,
            ActiveTab: JournalTab.Bestiary,
            Entries: entries,
            SelectedEntryIndex: 0,
            SelectedDetail: details
        );
    }

    #endregion
}
