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
/// Unit tests for <see cref="ZoneRenderer"/>.
/// </summary>
[TestFixture]
public class ZoneRendererTests
{
    private Mock<ITerminalService> _mockTerminal = null!;
    private ZoneRenderer _renderer = null!;

    [SetUp]
    public void SetUp()
    {
        _mockTerminal = new Mock<ITerminalService>();
        _mockTerminal.Setup(t => t.SupportsUnicode).Returns(true);
        _mockTerminal.Setup(t => t.SupportsColor).Returns(true);

        _renderer = new ZoneRenderer(
            _mockTerminal.Object,
            null,
            NullLogger<ZoneRenderer>.Instance);
    }

    // ═══════════════════════════════════════════════════════════════
    // GET ZONE SYMBOL TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetZoneSymbol_AllTypes_ReturnsMappedCharacter()
    {
        // Arrange & Act & Assert
        _renderer.GetZoneSymbol(ZoneType.Damage).Should().Be('X');
        _renderer.GetZoneSymbol(ZoneType.Control).Should().Be('!');
        _renderer.GetZoneSymbol(ZoneType.Debuff).Should().Be('-');
        _renderer.GetZoneSymbol(ZoneType.Buff).Should().Be('+');
        _renderer.GetZoneSymbol(ZoneType.Preview).Should().Be('.');
    }

    // ═══════════════════════════════════════════════════════════════
    // GET DAMAGE TYPE SYMBOL TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase("fire", 'F')]
    [TestCase("ice", 'I')]
    [TestCase("cold", 'I')]
    [TestCase("poison", 'P')]
    [TestCase("lightning", 'L')]
    [TestCase("acid", 'A')]
    [TestCase("necrotic", 'N')]
    [TestCase("holy", 'H')]
    public void GetDamageTypeSymbol_KnownTypes_ReturnsCorrectSymbol(string damageType, char expectedSymbol)
    {
        // Arrange & Act
        var result = _renderer.GetDamageTypeSymbol(damageType);

        // Assert
        result.Should().Be(expectedSymbol);
    }

    [Test]
    public void GetDamageTypeSymbol_UnknownType_ReturnsDefaultD()
    {
        // Arrange & Act
        var result = _renderer.GetDamageTypeSymbol("chaos");

        // Assert
        result.Should().Be('D');
    }

    [Test]
    public void GetDamageTypeSymbol_NullOrEmpty_ReturnsDefaultD()
    {
        // Arrange & Act & Assert
        _renderer.GetDamageTypeSymbol(null).Should().Be('D');
        _renderer.GetDamageTypeSymbol("").Should().Be('D');
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT ZONE DURATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatZoneDuration_NullDuration_ReturnsPermanent()
    {
        // Arrange & Act
        var result = _renderer.FormatZoneDuration(null);

        // Assert
        result.Should().Be("permanent");
    }

    [Test]
    public void FormatZoneDuration_SingleTurn_ReturnsSingular()
    {
        // Arrange & Act
        var result = _renderer.FormatZoneDuration(1);

        // Assert
        result.Should().Be("1 turn");
    }

    [Test]
    public void FormatZoneDuration_MultipleTurns_ReturnsPlural()
    {
        // Arrange & Act
        var result = _renderer.FormatZoneDuration(5);

        // Assert
        result.Should().Be("5 turns");
    }

    [Test]
    public void FormatZoneDuration_ZeroOrNegative_ReturnsExpiring()
    {
        // Arrange & Act & Assert
        _renderer.FormatZoneDuration(0).Should().Be("expiring");
        _renderer.FormatZoneDuration(-1).Should().Be("expiring");
    }

    // ═══════════════════════════════════════════════════════════════
    // GET HAZARD ICON TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetHazardIcon_UnicodeSupported_ReturnsUnicodeIcon()
    {
        // Arrange (already setup with SupportsUnicode = true)

        // Act
        var result = _renderer.GetHazardIcon("fire");

        // Assert
        result.Should().Be("🔥");
    }

    [Test]
    public void GetHazardIcon_UnicodeNotSupported_ReturnsAscii()
    {
        // Arrange
        _mockTerminal.Setup(t => t.SupportsUnicode).Returns(false);
        var asciiRenderer = new ZoneRenderer(_mockTerminal.Object);

        // Act
        var result = asciiRenderer.GetHazardIcon("fire");

        // Assert
        result.Should().Be("F");
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT ZONE CELL TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatZoneCell_DamageZone_UsesDamageTypeSymbol()
    {
        // Arrange
        var zone = new ZoneDisplayDto
        {
            ZoneId = Guid.NewGuid(),
            Name = "Fire Zone",
            ZoneType = ZoneType.Damage,
            DamageType = "fire",
            AffectedCells = Array.Empty<Domain.ValueObjects.GridPosition>()
        };

        // Act
        var result = _renderer.FormatZoneCell(zone);

        // Assert
        result.Should().Be(" F ");
    }

    [Test]
    public void FormatZoneCell_ControlZone_UsesZoneTypeSymbol()
    {
        // Arrange
        var zone = new ZoneDisplayDto
        {
            ZoneId = Guid.NewGuid(),
            Name = "Stun Zone",
            ZoneType = ZoneType.Control,
            AffectedCells = Array.Empty<Domain.ValueObjects.GridPosition>()
        };

        // Act
        var result = _renderer.FormatZoneCell(zone);

        // Assert
        result.Should().Be(" ! ");
    }
}
