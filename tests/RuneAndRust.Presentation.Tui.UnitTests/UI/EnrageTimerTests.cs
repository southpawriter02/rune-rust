using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;
using RuneAndRust.Presentation.Tui.UI;

namespace RuneAndRust.Presentation.Tui.UnitTests.UI;

/// <summary>
/// Unit tests for <see cref="EnrageTimer"/>.
/// </summary>
[TestFixture]
public class EnrageTimerTests
{
    private Mock<ITerminalService> _mockTerminal = null!;
    private BossStatusRenderer _renderer = null!;
    private EnrageTimer _timer = null!;

    [SetUp]
    public void SetUp()
    {
        _mockTerminal = new Mock<ITerminalService>();
        _mockTerminal.Setup(t => t.SupportsUnicode).Returns(true);
        _mockTerminal.Setup(t => t.SupportsColor).Returns(true);

        _renderer = new BossStatusRenderer(
            null,
            NullLogger<BossStatusRenderer>.Instance);

        _timer = new EnrageTimer(
            _renderer,
            _mockTerminal.Object,
            null,
            NullLogger<EnrageTimer>.Instance);
    }

    // ═══════════════════════════════════════════════════════════════
    // RENDER TIMER TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void RenderTimer_WhenEnraged_ShowsEnrageActive()
    {
        // Arrange
        var dto = new EnrageStatusDto(
            IsEnraged: true,
            HealthPercentToEnrage: null,
            StatModifiers: new Dictionary<string, float> { { "damage", 1.5f } });

        // Act
        _timer.RenderTimer(dto);

        // Assert
        _mockTerminal.Verify(t => t.WriteColoredAt(
            It.IsAny<int>(), It.IsAny<int>(),
            It.Is<string>(s => s.Contains("ENRAGED")),
            It.IsAny<ConsoleColor>()), Times.Once);
    }

    [Test]
    public void RenderTimer_WhenEnraged_SetsIsEnragedTrue()
    {
        // Arrange
        var dto = new EnrageStatusDto(
            IsEnraged: true,
            HealthPercentToEnrage: null,
            StatModifiers: new Dictionary<string, float>());

        // Act
        _timer.RenderTimer(dto);

        // Assert
        _timer.IsEnraged.Should().BeTrue();
    }

    [Test]
    public void RenderTimer_WhenApproachingEnrage_ShowsWarning()
    {
        // Arrange
        var dto = new EnrageStatusDto(
            IsEnraged: false,
            HealthPercentToEnrage: 8,
            StatModifiers: new Dictionary<string, float>());

        // Act
        _timer.RenderTimer(dto);

        // Assert
        _mockTerminal.Verify(t => t.WriteColoredAt(
            It.IsAny<int>(), It.IsAny<int>(),
            It.Is<string>(s => s.Contains("Enrage in:")),
            It.IsAny<ConsoleColor>()), Times.Once);
    }

    [Test]
    public void RenderTimer_WhenNotEnraged_SetsIsEnragedFalse()
    {
        // Arrange
        var dto = new EnrageStatusDto(
            IsEnraged: false,
            HealthPercentToEnrage: null,
            StatModifiers: new Dictionary<string, float>());

        // Act
        _timer.RenderTimer(dto);

        // Assert
        _timer.IsEnraged.Should().BeFalse();
    }

    [Test]
    public void RenderTimer_WithNullDto_ThrowsArgumentNullException()
    {
        // Arrange
        EnrageStatusDto dto = null!;

        // Act
        var act = () => _timer.RenderTimer(dto);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // SHOW ENRAGE WARNING TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ShowEnrageWarning_RendersWarningText()
    {
        // Arrange
        var healthPercent = 5;

        // Act
        _timer.ShowEnrageWarning(healthPercent);

        // Assert
        _mockTerminal.Verify(t => t.WriteColoredAt(
            It.IsAny<int>(), It.IsAny<int>(),
            It.Is<string>(s => s.Contains("5%")),
            It.IsAny<ConsoleColor>()), Times.Once);
    }

    // ═══════════════════════════════════════════════════════════════
    // SHOW ENRAGE ACTIVE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ShowEnrageActive_WithModifiers_RendersModifiers()
    {
        // Arrange
        var dto = new EnrageStatusDto(
            IsEnraged: true,
            HealthPercentToEnrage: null,
            StatModifiers: new Dictionary<string, float>
            {
                { "damage", 1.5f },
                { "attackSpeed", 1.25f }
            });

        // Act
        _timer.ShowEnrageActive(dto);

        // Assert - Header and modifiers rendered
        _mockTerminal.Verify(t => t.WriteColoredAt(
            It.IsAny<int>(), It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<ConsoleColor>()), Times.AtLeast(2));
    }

    // ═══════════════════════════════════════════════════════════════
    // CLEAR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Clear_ResetsState()
    {
        // Arrange
        var dto = new EnrageStatusDto(
            IsEnraged: true,
            HealthPercentToEnrage: null,
            StatModifiers: new Dictionary<string, float>());
        _timer.RenderTimer(dto);

        // Act
        _timer.Clear();

        // Assert
        _timer.IsEnraged.Should().BeFalse();
        _timer.HealthPercentToEnrage.Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithNullRenderer_ThrowsArgumentNullException()
    {
        // Arrange
        BossStatusRenderer renderer = null!;

        // Act
        var act = () => new EnrageTimer(renderer, _mockTerminal.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
