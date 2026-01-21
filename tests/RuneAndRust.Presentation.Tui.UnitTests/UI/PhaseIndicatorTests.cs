using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;
using RuneAndRust.Presentation.Tui.UI;

namespace RuneAndRust.Presentation.Tui.UnitTests.UI;

/// <summary>
/// Unit tests for <see cref="PhaseIndicator"/>.
/// </summary>
[TestFixture]
public class PhaseIndicatorTests
{
    private Mock<ITerminalService> _mockTerminal = null!;
    private BossStatusRenderer _renderer = null!;
    private PhaseIndicator _indicator = null!;

    [SetUp]
    public void SetUp()
    {
        _mockTerminal = new Mock<ITerminalService>();
        _mockTerminal.Setup(t => t.SupportsUnicode).Returns(true);
        _mockTerminal.Setup(t => t.SupportsColor).Returns(true);

        _renderer = new BossStatusRenderer(
            null,
            NullLogger<BossStatusRenderer>.Instance);

        _indicator = new PhaseIndicator(
            _renderer,
            _mockTerminal.Object,
            null,
            NullLogger<PhaseIndicator>.Instance);
    }

    // ═══════════════════════════════════════════════════════════════
    // RENDER PHASE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void RenderPhase_WithValidDto_CallsWriteColoredAt()
    {
        // Arrange
        var dto = new PhaseDisplayDto(
            PhaseNumber: 2,
            PhaseName: "Empowered",
            Behavior: BossBehavior.Aggressive,
            AbilityHints: new List<string> { "Watch for AoE" },
            StatModifiers: new Dictionary<string, float>());

        // Act
        _indicator.RenderPhase(dto);

        // Assert
        _mockTerminal.Verify(t => t.WriteColoredAt(
            It.IsAny<int>(), It.IsAny<int>(),
            It.Is<string>(s => s.Contains("EMPOWERED")),
            It.IsAny<ConsoleColor>()), Times.Once);
    }

    [Test]
    public void RenderPhase_SetsCurrentPhaseNumber()
    {
        // Arrange
        var dto = new PhaseDisplayDto(
            PhaseNumber: 3,
            PhaseName: "Fury",
            Behavior: BossBehavior.Enraged,
            AbilityHints: Array.Empty<string>(),
            StatModifiers: new Dictionary<string, float>());

        // Act
        _indicator.RenderPhase(dto);

        // Assert
        _indicator.CurrentPhaseNumber.Should().Be(3);
    }

    [Test]
    public void RenderPhase_WithAbilityHints_RendersHints()
    {
        // Arrange
        var dto = new PhaseDisplayDto(
            PhaseNumber: 2,
            PhaseName: "Empowered",
            Behavior: BossBehavior.Aggressive,
            AbilityHints: new List<string> { "Increased damage", "Summons minions" },
            StatModifiers: new Dictionary<string, float>());

        // Act
        _indicator.RenderPhase(dto);

        // Assert - WriteAt called for each hint
        _mockTerminal.Verify(t => t.WriteAt(
            It.IsAny<int>(), It.IsAny<int>(),
            It.Is<string>(s => s.Contains("Increased damage"))), Times.Once);
    }

    [Test]
    public void RenderPhase_WithNullDto_ThrowsArgumentNullException()
    {
        // Arrange
        PhaseDisplayDto dto = null!;

        // Act
        var act = () => _indicator.RenderPhase(dto);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // SHOW TRANSITION EFFECT TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ShowTransitionEffect_WithValidDto_RendersTransitionBox()
    {
        // Arrange
        var dto = new PhaseTransitionDto(
            OldPhaseNumber: 1,
            NewPhaseNumber: 2,
            PhaseName: "Empowered",
            TransitionText: "Rise!",
            BossName: "Skeleton King");

        // Act
        _indicator.ShowTransitionEffect(dto);

        // Assert - Multiple lines rendered
        _mockTerminal.Verify(t => t.WriteColoredAt(
            It.IsAny<int>(), It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<ConsoleColor>()), Times.AtLeast(5));
    }

    [Test]
    public void ShowTransitionEffect_CallsFlashDelay()
    {
        // Arrange
        var dto = new PhaseTransitionDto(
            OldPhaseNumber: 1,
            NewPhaseNumber: 2,
            PhaseName: "Empowered",
            TransitionText: null,
            BossName: "Boss");

        // Act
        _indicator.ShowTransitionEffect(dto);

        // Assert
        _mockTerminal.Verify(t => t.FlashDelay(It.IsAny<int>()), Times.AtLeastOnce);
    }

    // ═══════════════════════════════════════════════════════════════
    // CLEAR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Clear_ResetsCurrentPhaseNumber()
    {
        // Arrange
        var dto = new PhaseDisplayDto(
            PhaseNumber: 2,
            PhaseName: "Test",
            Behavior: BossBehavior.Aggressive,
            AbilityHints: Array.Empty<string>(),
            StatModifiers: new Dictionary<string, float>());
        _indicator.RenderPhase(dto);

        // Act
        _indicator.Clear();

        // Assert
        _indicator.CurrentPhaseNumber.Should().BeNull();
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
        var act = () => new PhaseIndicator(renderer, _mockTerminal.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Constructor_WithNullTerminalService_ThrowsArgumentNullException()
    {
        // Arrange
        ITerminalService terminal = null!;

        // Act
        var act = () => new PhaseIndicator(_renderer, terminal);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
