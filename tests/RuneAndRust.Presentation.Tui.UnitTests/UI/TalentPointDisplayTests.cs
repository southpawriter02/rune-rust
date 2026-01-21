// ═══════════════════════════════════════════════════════════════════════════════
// TalentPointDisplayTests.cs
// Unit tests for the TalentPointDisplay UI component.
// Version: 0.13.2d
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.UI;

namespace RuneAndRust.Presentation.Tui.UnitTests.UI;

/// <summary>
/// Unit tests for the TalentPointDisplay class.
/// </summary>
[TestFixture]
public class TalentPointDisplayTests
{
    private TalentPointDisplay _sut = null!;
    private Mock<ITerminalService> _terminalServiceMock = null!;

    // ═══════════════════════════════════════════════════════════════
    // SETUP
    // ═══════════════════════════════════════════════════════════════

    [SetUp]
    public void SetUp()
    {
        _terminalServiceMock = new Mock<ITerminalService>();
        var config = NodeTooltipConfig.CreateDefault();
        var configOptions = Options.Create(config);
        _sut = new TalentPointDisplay(_terminalServiceMock.Object, configOptions);
    }

    // ═══════════════════════════════════════════════════════════════
    // INITIAL STATE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void AvailablePoints_InitialState_ReturnsZero()
    {
        // Assert
        _sut.AvailablePoints.Should().Be(0);
    }

    [Test]
    public void SpentPoints_InitialState_ReturnsZero()
    {
        // Assert
        _sut.SpentPoints.Should().Be(0);
    }

    [Test]
    public void TotalPoints_InitialState_ReturnsZero()
    {
        // Assert
        _sut.TotalPoints.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════
    // RENDER POINTS TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void RenderPoints_WithValidValues_UpdatesProperties()
    {
        // Act
        _sut.RenderPoints(5, 10);

        // Assert
        _sut.AvailablePoints.Should().Be(5);
        _sut.SpentPoints.Should().Be(10);
        _sut.TotalPoints.Should().Be(15);
    }

    [Test]
    public void RenderPoints_CallsTerminalService()
    {
        // Act
        _sut.RenderPoints(3, 7);

        // Assert
        _terminalServiceMock.Verify(
            t => t.WriteAt(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()),
            Times.AtLeastOnce);
    }

    // ═══════════════════════════════════════════════════════════════
    // UPDATE AVAILABLE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void UpdateAvailable_WithPointSpent_UpdatesSpentPoints()
    {
        // Arrange
        _sut.RenderPoints(5, 10);

        // Act
        _sut.UpdateAvailable(4);

        // Assert
        _sut.AvailablePoints.Should().Be(4);
        _sut.SpentPoints.Should().Be(11); // One point moved from available to spent
    }

    // ═══════════════════════════════════════════════════════════════
    // POSITION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void SetPosition_WithValues_UpdatesPosition()
    {
        // Act
        _sut.SetPosition(10, 20);

        // Assert
        var position = _sut.GetPosition();
        position.X.Should().Be(10);
        position.Y.Should().Be(20);
    }

    // ═══════════════════════════════════════════════════════════════
    // DTO TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ToDto_AfterRenderPoints_ReturnsDtoWithCorrectValues()
    {
        // Arrange
        _sut.RenderPoints(3, 12);

        // Act
        var dto = _sut.ToDto();

        // Assert
        dto.Available.Should().Be(3);
        dto.Spent.Should().Be(12);
        dto.Total.Should().Be(15);
    }

    // ═══════════════════════════════════════════════════════════════
    // CLEAR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Clear_CallsTerminalServiceToWriteBlank()
    {
        // Arrange
        _sut.SetPosition(5, 5);

        // Act
        _sut.Clear();

        // Assert
        _terminalServiceMock.Verify(
            t => t.WriteAt(5, 5, It.Is<string>(s => s.All(c => c == ' '))),
            Times.AtLeastOnce);
    }
}
