using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Enums;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UnitTests.Renderers;

/// <summary>
/// Unit tests for <see cref="ComboRenderer"/>.
/// </summary>
[TestFixture]
public class ComboRendererTests
{
    private Mock<ITerminalService> _mockTerminal = null!;
    private ComboRenderer _renderer = null!;

    [SetUp]
    public void SetUp()
    {
        _mockTerminal = new Mock<ITerminalService>();
        _mockTerminal.Setup(t => t.SupportsUnicode).Returns(true);
        _mockTerminal.Setup(t => t.SupportsColor).Returns(true);

        _renderer = new ComboRenderer(
            _mockTerminal.Object,
            null,
            NullLogger<ComboRenderer>.Instance);
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT CHAIN STEP TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatChainStep_CompletedState_ReturnsCheckmark()
    {
        // Arrange & Act
        var result = _renderer.FormatChainStep("Strike", ComboStepState.Completed);

        // Assert
        result.Should().Be("[✓Strike]");
    }

    [Test]
    public void FormatChainStep_CurrentState_ReturnsArrow()
    {
        // Arrange & Act
        var result = _renderer.FormatChainStep("Slash", ComboStepState.Current);

        // Assert
        result.Should().Be("[>Slash]");
    }

    [Test]
    public void FormatChainStep_PendingState_ReturnsQuestionMark()
    {
        // Arrange & Act
        var result = _renderer.FormatChainStep("Thrust", ComboStepState.Pending);

        // Assert
        result.Should().Be("[?Thrust]");
    }

    // ═══════════════════════════════════════════════════════════════
    // GET STEP COLOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetStepColor_AllStates_ReturnsCorrectColors()
    {
        // Arrange & Act & Assert
        _renderer.GetStepColor(ComboStepState.Completed).Should().Be(ConsoleColor.Green);
        _renderer.GetStepColor(ComboStepState.Current).Should().Be(ConsoleColor.Yellow);
        _renderer.GetStepColor(ComboStepState.Pending).Should().Be(ConsoleColor.DarkGray);
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT BONUS TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatBonus_ZeroProgress_ReturnsBuildingMessage()
    {
        // Arrange & Act
        var result = _renderer.FormatBonus(0);

        // Assert
        result.Should().Be("Building combo...");
    }

    [Test]
    public void FormatBonus_FiftyPercent_ReturnsCorrectBonus()
    {
        // Arrange & Act
        var result = _renderer.FormatBonus(50);

        // Assert - 50% progress with 10% multiplier per step = 5% bonus
        result.Should().Be("+5% damage bonus");
    }

    [Test]
    public void FormatBonus_FullProgress_ReturnsCorrectBonus()
    {
        // Arrange & Act
        var result = _renderer.FormatBonus(100);

        // Assert - 100% progress with 10% multiplier per step = 10% bonus
        result.Should().Be("+10% damage bonus");
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT WINDOW REMAINING TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatWindowRemaining_ZeroTurns_ReturnsExpired()
    {
        // Arrange & Act
        var result = _renderer.FormatWindowRemaining(0);

        // Assert
        result.Should().Be("EXPIRED");
    }

    [Test]
    public void FormatWindowRemaining_OneTurn_ReturnsUrgent()
    {
        // Arrange & Act
        var result = _renderer.FormatWindowRemaining(1);

        // Assert
        result.Should().Be("1 turn (!)");
    }

    [Test]
    public void FormatWindowRemaining_MultipleTurns_ReturnsPlural()
    {
        // Arrange & Act
        var result = _renderer.FormatWindowRemaining(3);

        // Assert
        result.Should().Be("3 turns");
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT BREAK/COMPLETION MESSAGE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatBreakMessage_WithInfo_FormatsCorrectly()
    {
        // Arrange
        var breakInfo = new ComboBreakDisplayDto("Warrior's Fury", 2, "Window expired");

        // Act
        var result = _renderer.FormatBreakMessage(breakInfo);

        // Assert
        result.Should().Contain("COMBO BROKEN!");
        result.Should().Contain("Warrior's Fury");
        result.Should().Contain("step 2");
    }

    [Test]
    public void FormatCompletionMessage_WithInfo_FormatsCorrectly()
    {
        // Arrange
        var completion = new ComboCompletionDisplayDto(
            "Warrior's Fury",
            4,
            new[] { "+25% damage" });

        // Act
        var result = _renderer.FormatCompletionMessage(completion);

        // Assert
        result.Should().Contain("COMBO COMPLETE!");
        result.Should().Contain("Warrior's Fury");
        result.Should().Contain("4 steps");
    }

    // ═══════════════════════════════════════════════════════════════
    // GET WINDOW COLOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetWindowColor_UrgentTurns_ReturnsRed()
    {
        // Arrange & Act & Assert
        _renderer.GetWindowColor(1).Should().Be(ConsoleColor.Red);
        _renderer.GetWindowColor(2).Should().Be(ConsoleColor.Yellow);
        _renderer.GetWindowColor(3).Should().Be(ConsoleColor.White);
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT CONTINUATION OPTION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatContinuationOption_WithBonus_FormatsCorrectly()
    {
        // Arrange
        var continuation = new ComboContinuationDto(
            OptionNumber: 1,
            AbilityId: "thrust",
            AbilityName: "Thrust",
            ComboName: "Warrior's Fury",
            StepProgress: "3/4",
            WindowRemaining: 2,
            BonusPreview: "+25% damage");

        // Act
        var result = _renderer.FormatContinuationOption(continuation);

        // Assert
        result.Should().Contain("[1]");
        result.Should().Contain("Thrust");
        result.Should().Contain("+25% damage");
    }
}
