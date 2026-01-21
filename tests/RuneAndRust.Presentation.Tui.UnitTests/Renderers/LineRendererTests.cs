// ═══════════════════════════════════════════════════════════════════════════════
// LineRendererTests.cs
// Unit tests for the LineRenderer class.
// Version: 0.13.2c
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Enums;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UnitTests.Renderers;

/// <summary>
/// Unit tests for <see cref="LineRenderer"/>.
/// </summary>
[TestFixture]
public class LineRendererTests
{
    private LineRenderer _renderer = null!;
    private PrerequisiteLinesConfig _config = null!;

    // ═══════════════════════════════════════════════════════════════
    // SETUP
    // ═══════════════════════════════════════════════════════════════

    [SetUp]
    public void Setup()
    {
        _config = PrerequisiteLinesConfig.CreateDefault();
        var options = Options.Create(_config);
        _renderer = new LineRenderer(options);
    }

    // ═══════════════════════════════════════════════════════════════
    // GetLineColor TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(LineState.Satisfied, ConsoleColor.Green)]
    [TestCase(LineState.Unsatisfied, ConsoleColor.DarkGray)]
    public void GetLineColor_WithState_ReturnsCorrectColor(
        LineState state, ConsoleColor expectedColor)
    {
        // Arrange & Act
        var result = _renderer.GetLineColor(state);

        // Assert
        result.Should().Be(expectedColor);
    }

    // ═══════════════════════════════════════════════════════════════
    // GetLineCharacter TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(LineDirection.Horizontal, '─')]
    [TestCase(LineDirection.Vertical, '│')]
    [TestCase(LineDirection.CornerTopRight, '┐')]
    [TestCase(LineDirection.CornerBottomRight, '┘')]
    [TestCase(LineDirection.CornerTopLeft, '┌')]
    [TestCase(LineDirection.CornerBottomLeft, '└')]
    public void GetLineCharacter_WithDirection_ReturnsCorrectCharacter(
        LineDirection direction, char expectedCharacter)
    {
        // Arrange & Act
        var result = _renderer.GetLineCharacter(direction);

        // Assert
        result.Should().Be(expectedCharacter);
    }

    // ═══════════════════════════════════════════════════════════════
    // CalculatePath TESTS - HORIZONTAL
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void CalculatePath_WithSameYPosition_ReturnsStraightHorizontalLine()
    {
        // Arrange
        var start = new LinePoint(10, 5);
        var end = new LinePoint(20, 5);

        // Act
        var result = _renderer.CalculatePath(start, end).ToList();

        // Assert
        result.Should().HaveCount(10);
        result.Should().AllSatisfy(s => s.Y.Should().Be(5));
        result.Should().AllSatisfy(s => s.Direction.Should().Be(LineDirection.Horizontal));
    }

    [Test]
    public void CalculatePath_WithSameYPosition_SegmentsHaveSequentialXPositions()
    {
        // Arrange
        var start = new LinePoint(5, 10);
        var end = new LinePoint(15, 10);

        // Act
        var result = _renderer.CalculatePath(start, end).ToList();

        // Assert
        for (var i = 0; i < result.Count; i++)
        {
            result[i].X.Should().Be(start.X + i);
            result[i].Index.Should().Be(i);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // CalculatePath TESTS - VERTICAL
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void CalculatePath_WithDifferentYPositions_ReturnsPathWithVerticalSegment()
    {
        // Arrange
        var start = new LinePoint(10, 5);
        var end = new LinePoint(20, 10);

        // Act
        var result = _renderer.CalculatePath(start, end).ToList();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain(s => s.Direction == LineDirection.Vertical);
    }

    [Test]
    public void CalculatePath_WithDifferentYPositions_IncludesCornerSegments()
    {
        // Arrange
        var start = new LinePoint(10, 5);
        var end = new LinePoint(20, 10);

        // Act
        var result = _renderer.CalculatePath(start, end).ToList();

        // Assert - Should have corner segments
        result.Should().Contain(s => 
            s.Direction == LineDirection.CornerBottomRight ||
            s.Direction == LineDirection.CornerTopRight ||
            s.Direction == LineDirection.CornerBottomLeft ||
            s.Direction == LineDirection.CornerTopLeft);
    }

    // ═══════════════════════════════════════════════════════════════
    // CONFIGURATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithNullConfig_UsesDefaultConfig()
    {
        // Arrange & Act
        var renderer = new LineRenderer();

        // Assert
        renderer.GetConfig().Should().NotBeNull();
        renderer.GetConfig().SatisfiedLineColor.Should().Be(ConsoleColor.Green);
    }

    [Test]
    public void GetConfig_ReturnsCurrentConfiguration()
    {
        // Arrange & Act
        var config = _renderer.GetConfig();

        // Assert
        config.Should().BeSameAs(_config);
    }

    // ═══════════════════════════════════════════════════════════════
    // LINE CHARACTER ACCESS TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetSatisfiedLineCharacter_ReturnsConfiguredCharacter()
    {
        // Arrange & Act
        var result = _renderer.GetSatisfiedLineCharacter();

        // Assert
        result.Should().Be('─');
    }

    [Test]
    public void GetUnsatisfiedLineCharacter_ReturnsConfiguredCharacter()
    {
        // Arrange & Act
        var result = _renderer.GetUnsatisfiedLineCharacter();

        // Assert
        result.Should().Be('─');
    }

    // ═══════════════════════════════════════════════════════════════
    // FormatConnectionPoint TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatConnectionPoint_ReturnsConfiguredCharacter()
    {
        // Arrange & Act
        var result = _renderer.FormatConnectionPoint(LineDirection.Horizontal);

        // Assert
        result.Should().Be("─");
    }
}
