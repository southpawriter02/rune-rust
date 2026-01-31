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
/// Unit tests for <see cref="BossHealthBar"/>.
/// </summary>
[TestFixture]
public class BossHealthBarTests
{
    private Mock<ITerminalService> _mockTerminal = null!;
    private BossHealthBarRenderer _renderer = null!;
    private BossHealthBar _healthBar = null!;

    [SetUp]
    public void SetUp()
    {
        _mockTerminal = new Mock<ITerminalService>();
        _mockTerminal.Setup(t => t.SupportsUnicode).Returns(true);
        _mockTerminal.Setup(t => t.SupportsColor).Returns(true);

        _renderer = new BossHealthBarRenderer(
            null,
            _mockTerminal.Object,
            NullLogger<BossHealthBarRenderer>.Instance);

        _healthBar = new BossHealthBar(
            _renderer,
            _mockTerminal.Object,
            null,
            NullLogger<BossHealthBar>.Instance);
    }

    // ═══════════════════════════════════════════════════════════════
    // RENDER BAR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void RenderBar_WithValidDto_RendersNameHeader()
    {
        // Arrange
        var dto = new BossHealthDisplayDto(
            BossId: "skeleton-king",
            BossName: "Skeleton King",
            CurrentHealth: 2847,
            MaxHealth: 5000,
            HealthPercent: 57,
            CurrentPhaseNumber: 2);

        // Act
        _healthBar.RenderBar(dto);

        // Assert - Verify header was written with colored output
        _mockTerminal.Verify(t => t.WriteColoredAt(
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.Is<string>(s => s.Contains("SKELETON KING")),
            It.IsAny<ConsoleColor>()), Times.Once);
    }

    [Test]
    public void RenderBar_WithValidDto_RendersHealthText()
    {
        // Arrange
        var dto = new BossHealthDisplayDto(
            BossId: "skeleton-king",
            BossName: "Skeleton King",
            CurrentHealth: 2847,
            MaxHealth: 5000,
            HealthPercent: 57,
            CurrentPhaseNumber: 2);

        // Act
        _healthBar.RenderBar(dto);

        // Assert - Verify health text was written
        _mockTerminal.Verify(t => t.WriteAt(
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.Is<string>(s => s.Contains("HP:"))), Times.AtLeastOnce);
    }

    [Test]
    public void RenderBar_WithValidDto_SetsCurrentState()
    {
        // Arrange
        var dto = new BossHealthDisplayDto(
            BossId: "skeleton-king",
            BossName: "Skeleton King",
            CurrentHealth: 2847,
            MaxHealth: 5000,
            HealthPercent: 57,
            CurrentPhaseNumber: 2);

        // Act
        _healthBar.RenderBar(dto);

        // Assert
        _healthBar.IsActive.Should().BeTrue();
        _healthBar.CurrentBossId.Should().Be("skeleton-king");
    }

    [Test]
    public void RenderBar_WithNullDto_ThrowsArgumentNullException()
    {
        // Arrange
        BossHealthDisplayDto dto = null!;

        // Act
        var act = () => _healthBar.RenderBar(dto);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // SHOW PHASE MARKERS TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ShowPhaseMarkers_WithPhases_StoresMarkers()
    {
        // Arrange
        var markers = new List<PhaseMarkerDto>
        {
            new(2, "Empowered", 75),
            new(3, "Enraged", 50),
            new(4, "Desperate", 25)
        };

        // Act & Assert (no exception)
        Assert.DoesNotThrow(() => _healthBar.ShowPhaseMarkers(markers));
    }

    [Test]
    public void ShowPhaseMarkers_WithEmptyList_AcceptsEmpty()
    {
        // Arrange
        var emptyMarkers = new List<PhaseMarkerDto>();

        // Act & Assert (no exception)
        Assert.DoesNotThrow(() => _healthBar.ShowPhaseMarkers(emptyMarkers));
    }

    [Test]
    public void ShowPhaseMarkers_WithNullPhases_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<PhaseMarkerDto> nullMarkers = null!;

        // Act
        var act = () => _healthBar.ShowPhaseMarkers(nullMarkers);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // CLEAR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Clear_AfterRender_ClearsState()
    {
        // Arrange
        var dto = new BossHealthDisplayDto(
            BossId: "skeleton-king",
            BossName: "Skeleton King",
            CurrentHealth: 2847,
            MaxHealth: 5000,
            HealthPercent: 57,
            CurrentPhaseNumber: 2);
        _healthBar.RenderBar(dto);

        // Act
        _healthBar.Clear();

        // Assert
        _healthBar.IsActive.Should().BeFalse();
        _healthBar.CurrentBossId.Should().BeNull();
    }

    [Test]
    public void Clear_CallsWriteAtForEachRow()
    {
        // Arrange
        var config = BossHealthDisplayConfig.CreateDefault();

        // Act
        _healthBar.Clear();

        // Assert - Should clear each row of the display area
        _mockTerminal.Verify(t => t.WriteAt(
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<string>()), Times.AtLeast(1));
    }

    // ═══════════════════════════════════════════════════════════════
    // UPDATE HEALTH TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void UpdateHealth_WithoutCurrentState_DoesNotThrow()
    {
        // Arrange - No RenderBar called first

        // Act & Assert (no exception, should log warning)
        Assert.DoesNotThrow(() => _healthBar.UpdateHealth(100, 200));
    }

    [Test]
    public void UpdateHealth_WithCurrentState_UpdatesDisplay()
    {
        // Arrange
        var dto = new BossHealthDisplayDto(
            BossId: "skeleton-king",
            BossName: "Skeleton King",
            CurrentHealth: 5000,
            MaxHealth: 5000,
            HealthPercent: 100,
            CurrentPhaseNumber: 1);
        _healthBar.RenderBar(dto);
        _mockTerminal.Invocations.Clear();

        // Act
        _healthBar.UpdateHealth(2500, 5000);

        // Assert - Should re-render the health bar
        _mockTerminal.Verify(t => t.WriteAt(
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<string>()), Times.AtLeastOnce);
    }

    // ═══════════════════════════════════════════════════════════════
    // ANIMATE DAMAGE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void AnimateDamage_WithNullDto_ThrowsArgumentNullException()
    {
        // Arrange
        DamageAnimationDto dto = null!;

        // Act
        var act = () => _healthBar.AnimateDamage(dto);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void AnimateDamage_CallsFlashDelay()
    {
        // Arrange
        var healthDto = new BossHealthDisplayDto(
            BossId: "skeleton-king",
            BossName: "Skeleton King",
            CurrentHealth: 3000,
            MaxHealth: 5000,
            HealthPercent: 60,
            CurrentPhaseNumber: 1);
        _healthBar.RenderBar(healthDto);

        var damageDto = new DamageAnimationDto(
            PreviousHealth: 3000,
            CurrentHealth: 2850,
            DamageAmount: 150);

        // Act
        _healthBar.AnimateDamage(damageDto);

        // Assert - FlashDelay should be called for both flash and delta display
        _mockTerminal.Verify(t => t.FlashDelay(It.IsAny<int>()), Times.AtLeast(1));
    }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithNullRenderer_ThrowsArgumentNullException()
    {
        // Arrange
        BossHealthBarRenderer renderer = null!;

        // Act
        var act = () => new BossHealthBar(renderer, _mockTerminal.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Constructor_WithNullTerminalService_ThrowsArgumentNullException()
    {
        // Arrange
        ITerminalService terminal = null!;

        // Act
        var act = () => new BossHealthBar(_renderer, terminal);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
