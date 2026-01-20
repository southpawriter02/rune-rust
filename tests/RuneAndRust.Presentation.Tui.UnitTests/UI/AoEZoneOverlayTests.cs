using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Enums;
using RuneAndRust.Presentation.Tui.Renderers;
using RuneAndRust.Presentation.Tui.UI;

namespace RuneAndRust.Presentation.Tui.UnitTests.UI;

/// <summary>
/// Unit tests for <see cref="AoEZoneOverlay"/>.
/// </summary>
[TestFixture]
public class AoEZoneOverlayTests
{
    private Mock<Application.Interfaces.ITerminalService> _mockTerminal = null!;
    private ZoneRenderer _renderer = null!;
    private AoEZoneOverlay _overlay = null!;

    [SetUp]
    public void SetUp()
    {
        _mockTerminal = new Mock<Application.Interfaces.ITerminalService>();
        _mockTerminal.Setup(t => t.SupportsUnicode).Returns(true);
        _mockTerminal.Setup(t => t.SupportsColor).Returns(true);

        _renderer = new ZoneRenderer(
            _mockTerminal.Object,
            null,
            NullLogger<ZoneRenderer>.Instance);

        _overlay = new AoEZoneOverlay(
            _renderer,
            NullLogger<AoEZoneOverlay>.Instance);
    }

    // ═══════════════════════════════════════════════════════════════
    // RENDER ZONES TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void RenderZones_WithMultipleZones_BuildsCorrectCellMapping()
    {
        // Arrange
        var zones = new List<ZoneDisplayDto>
        {
            CreateFireZone(new[] { new GridPosition(0, 0), new GridPosition(1, 0) }),
            CreateIceZone(new[] { new GridPosition(2, 0), new GridPosition(3, 0) })
        };

        // Act
        _overlay.RenderZones(zones);
        var zoneCells = _overlay.GetAllZoneCells();

        // Assert
        zoneCells.Should().HaveCount(4);
        zoneCells[new GridPosition(0, 0)].DamageType.Should().Be("fire");
        zoneCells[new GridPosition(2, 0)].DamageType.Should().Be("ice");
    }

    [Test]
    public void RenderZones_WithOverlappingZones_UsesPriorityOrder()
    {
        // Arrange
        var overlappingCell = new GridPosition(1, 1);

        var buffZone = new ZoneDisplayDto
        {
            ZoneId = Guid.NewGuid(),
            Name = "Healing Aura",
            ZoneType = ZoneType.Buff,  // Priority 1 (lowest)
            AffectedCells = new[] { overlappingCell },
            IsPermanent = true
        };

        var damageZone = CreateFireZone(new[] { overlappingCell }); // Priority 4 (highest)

        // Act - add buff first, then damage
        _overlay.RenderZones(new List<ZoneDisplayDto> { buffZone, damageZone });
        var zoneCells = _overlay.GetAllZoneCells();

        // Assert - damage should win due to higher priority
        zoneCells[overlappingCell].ZoneType.Should().Be(ZoneType.Damage);
        zoneCells[overlappingCell].DamageType.Should().Be("fire");
    }

    [Test]
    public void HighlightTiles_PreviewMode_AddsToMappingWithLowPriority()
    {
        // Arrange
        var previewCells = new List<GridPosition>
        {
            new(0, 0),
            new(1, 0),
            new(2, 0)
        };

        // Act
        _overlay.HighlightTiles(previewCells, ZoneType.Damage, isPreview: true);
        var zoneCells = _overlay.GetAllZoneCells();

        // Assert
        zoneCells.Should().HaveCount(3);
        zoneCells[new GridPosition(0, 0)].IsPreview.Should().BeTrue();
        zoneCells[new GridPosition(0, 0)].Name.Should().Be("Target Area");
    }

    [Test]
    public void RenderLegend_WithActiveZones_FormatsCorrectly()
    {
        // Arrange
        var zones = new List<ZoneDisplayDto>
        {
            new ZoneDisplayDto
            {
                ZoneId = Guid.NewGuid(),
                Name = "Fire Zone",
                ZoneType = ZoneType.Damage,
                DamageType = "fire",
                DamagePerTurn = 5,
                AffectedCells = new[] { new GridPosition(0, 0) },
                RemainingDuration = 3,
                IsPermanent = false
            }
        };

        _overlay.RenderZones(zones);

        // Act
        var legend = _overlay.RenderLegend();

        // Assert
        legend.Should().HaveCountGreaterThan(1);
        legend[0].Should().Be("ACTIVE ZONES:");
        legend[1].Should().Contain("[F]");
        legend[1].Should().Contain("Fire Zone");
        legend[1].Should().Contain("3 turns");
        legend[1].Should().Contain("5 fire/turn");
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    private static ZoneDisplayDto CreateFireZone(GridPosition[] cells) => new()
    {
        ZoneId = Guid.NewGuid(),
        Name = "Fire Zone",
        ZoneType = ZoneType.Damage,
        DamageType = "fire",
        DamagePerTurn = 5,
        AffectedCells = cells,
        RemainingDuration = 3,
        IsPermanent = false
    };

    private static ZoneDisplayDto CreateIceZone(GridPosition[] cells) => new()
    {
        ZoneId = Guid.NewGuid(),
        Name = "Ice Zone",
        ZoneType = ZoneType.Control,
        DamageType = "ice",
        StatusEffect = "Slowed",
        AffectedCells = cells,
        RemainingDuration = 2,
        IsPermanent = false
    };
}
