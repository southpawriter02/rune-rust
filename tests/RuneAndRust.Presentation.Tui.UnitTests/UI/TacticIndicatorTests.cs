using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;
using RuneAndRust.Presentation.Tui.UI;

namespace RuneAndRust.Presentation.Tui.UnitTests.UI;

/// <summary>
/// Unit tests for the <see cref="TacticIndicator"/> class.
/// </summary>
[TestFixture]
public class TacticIndicatorTests
{
    private MonsterGroupRenderer _renderer = null!;
    private Mock<ITerminalService> _mockTerminal = null!;
    private MonsterGroupDisplayConfig _config = null!;
    private Mock<ILogger<TacticIndicator>> _mockLogger = null!;
    private TacticIndicator _tacticIndicator = null!;

    [SetUp]
    public void Setup()
    {
        _config = MonsterGroupDisplayConfig.CreateDefault();
        _mockTerminal = new Mock<ITerminalService>();
        _renderer = new MonsterGroupRenderer(_config);
        _mockLogger = new Mock<ILogger<TacticIndicator>>();

        _tacticIndicator = new TacticIndicator(
            _renderer,
            _mockTerminal.Object,
            _config,
            _mockLogger.Object);
    }

    // ═══════════════════════════════════════════════════════════════
    // RENDER TACTIC TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void RenderTactic_WithValidDto_RendersTacticTitle()
    {
        // Arrange
        var tactic = new TacticDisplayDto(
            "Flank",
            "Flanking Assault",
            "Surround target for flanking bonuses",
            new List<RoleAssignmentDto>
            {
                new("Archer", "Attack from range"),
                new("Chief", "Engage in melee")
            });

        // Act
        _tacticIndicator.RenderTactic(tactic);

        // Assert
        _mockTerminal.Verify(t => t.WriteColoredAt(
            It.IsAny<int>(), It.IsAny<int>(),
            It.Is<string>(s => s.Contains("TACTIC: Flanking Assault")),
            _config.Colors.TacticColor), Times.Once);
    }

    [Test]
    public void RenderTactic_WithRoleAssignments_RendersRolesWithPrefixes()
    {
        // Arrange
        var tactic = new TacticDisplayDto(
            "Flank",
            "Flanking Assault",
            "Surround target",
            new List<RoleAssignmentDto>
            {
                new("Archer", "Attack from range"),
                new("Shaman", "Provide support")
            });

        // Act
        _tacticIndicator.RenderTactic(tactic);

        // Assert - Role assignments are rendered with tree prefixes
        _mockTerminal.Verify(t => t.WriteAt(
            It.IsAny<int>(), It.IsAny<int>(),
            It.Is<string>(s => s.Contains("|-- Archer: Attack from range"))), Times.Once);

        _mockTerminal.Verify(t => t.WriteAt(
            It.IsAny<int>(), It.IsAny<int>(),
            It.Is<string>(s => s.Contains("+-- Shaman: Provide support"))), Times.Once);
    }

    [Test]
    public void RenderTactic_WithNullDto_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _tacticIndicator.RenderTactic(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // HIGHLIGHT TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void HighlightActiveTactic_AfterRender_UsesHighlightColor()
    {
        // Arrange
        var tactic = new TacticDisplayDto(
            "Flank",
            "Flanking Assault",
            "Surround target",
            new List<RoleAssignmentDto>());

        _tacticIndicator.RenderTactic(tactic);

        // Act
        _tacticIndicator.HighlightActiveTactic();

        // Assert
        _tacticIndicator.IsHighlighted.Should().BeTrue();
        _mockTerminal.Verify(t => t.WriteColoredAt(
            It.IsAny<int>(), It.IsAny<int>(),
            It.Is<string>(s => s.Contains("TACTIC: Flanking Assault")),
            _config.Colors.TacticHighlightColor), Times.Once);
    }

    [Test]
    public void HighlightActiveTactic_WithNoTactic_DoesNotThrow()
    {
        // Act
        var act = () => _tacticIndicator.HighlightActiveTactic();

        // Assert
        act.Should().NotThrow();
        _tacticIndicator.IsHighlighted.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // CLEAR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Clear_ResetsState()
    {
        // Arrange
        var tactic = new TacticDisplayDto(
            "Flank",
            "Flanking Assault",
            "Surround target",
            new List<RoleAssignmentDto>());
        _tacticIndicator.RenderTactic(tactic);

        // Act
        _tacticIndicator.Clear();

        // Assert
        _tacticIndicator.CurrentTacticName.Should().BeNull();
        _tacticIndicator.IsHighlighted.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithNullRenderer_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new TacticIndicator(
            null!,
            _mockTerminal.Object,
            _config,
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("renderer");
    }

    [Test]
    public void Constructor_WithNullTerminal_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new TacticIndicator(
            _renderer,
            null!,
            _config,
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("terminalService");
    }
}
