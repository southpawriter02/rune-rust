// ═══════════════════════════════════════════════════════════════════════════════
// DiceCheckRendererTests.cs
// Unit tests for the DiceCheckRenderer class.
// Version: 0.13.3d
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UnitTests.Renderers;

/// <summary>
/// Unit tests for <see cref="DiceCheckRenderer"/>.
/// </summary>
[TestFixture]
public class DiceCheckRendererTests
{
    private GatheringDisplayConfig _config = null!;
    private DiceCheckRenderer _renderer = null!;

    [SetUp]
    public void Setup()
    {
        _config = new GatheringDisplayConfig();
        _renderer = new DiceCheckRenderer(_config);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FormatRoll Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    [TestCase(14, 3, 17, "[14] + 3 = 17")]
    [TestCase(8, -2, 6, "[8] - 2 = 6")]
    [TestCase(20, 0, 20, "[20] + 0 = 20")]
    [TestCase(1, 5, 6, "[1] + 5 = 6")]
    [TestCase(10, -5, 5, "[10] - 5 = 5")]
    public void FormatRoll_WithValues_ReturnsCorrectFormat(
        int rawRoll, int modifier, int total, string expected)
    {
        // Act
        var result = _renderer.FormatRoll(rawRoll, modifier, total);

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    public void FormatRoll_WithNegativeModifier_ShowsMinusSign()
    {
        // Arrange
        const int rawRoll = 15;
        const int modifier = -3;
        const int total = 12;

        // Act
        var result = _renderer.FormatRoll(rawRoll, modifier, total);

        // Assert
        result.Should().Be("[15] - 3 = 12");
        result.Should().Contain("-");
        result.Should().NotContain("+");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FormatResult Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    [TestCase(true, "[x] SUCCESS!")]
    [TestCase(false, "[ ] FAILED")]
    public void FormatResult_WithSuccess_ReturnsCorrectIndicator(
        bool success, string expected)
    {
        // Act
        var result = _renderer.FormatResult(success);

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    public void FormatResult_Success_ContainsCheckMark()
    {
        // Act
        var result = _renderer.FormatResult(true);

        // Assert
        result.Should().Contain("[x]");
        result.Should().Contain("SUCCESS");
    }

    [Test]
    public void FormatResult_Failure_ContainsEmptyBrackets()
    {
        // Act
        var result = _renderer.FormatResult(false);

        // Assert
        result.Should().Contain("[ ]");
        result.Should().Contain("FAILED");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FormatDC Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void FormatDC_WithSkillAndDC_ReturnsFormattedString()
    {
        // Arrange
        const string skillName = "Herbalism";
        const int dc = 10;

        // Act
        var result = _renderer.FormatDC(skillName, dc);

        // Assert
        result.Should().Be("Skill Check: Herbalism (DC 10)");
    }

    [Test]
    public void FormatDC_WithHighDC_IncludesDCValue()
    {
        // Arrange
        const string skillName = "Mining";
        const int dc = 25;

        // Act
        var result = _renderer.FormatDC(skillName, dc);

        // Assert
        result.Should().Contain("DC 25");
        result.Should().Contain("Mining");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FormatModifiers Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void FormatModifiers_WithMultipleModifiers_FormatsCorrectly()
    {
        // Arrange
        var modifiers = new[] { 3, -1, 2 };

        // Act
        var result = _renderer.FormatModifiers(modifiers);

        // Assert
        result.Should().Contain("+ 3");
        result.Should().Contain("- 1");
        result.Should().Contain("+ 2");
    }

    [Test]
    public void FormatModifiers_WithEmptyList_ReturnsEmptyString()
    {
        // Arrange
        var modifiers = Array.Empty<int>();

        // Act
        var result = _renderer.FormatModifiers(modifiers);

        // Assert
        result.Should().BeEmpty();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FormatCompleteCheck Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void FormatCompleteCheck_WithSuccessfulCheck_ContainsAllElements()
    {
        // Arrange
        const string skillName = "Herbalism";
        const int dc = 10;
        const int rawRoll = 14;
        const int modifier = 3;
        const int total = 17;
        const bool success = true;

        // Act
        var result = _renderer.FormatCompleteCheck(skillName, dc, rawRoll, modifier, total, success);

        // Assert
        result.Should().Contain("Skill Check: Herbalism (DC 10)");
        result.Should().Contain("Rolling: 1d20 + 3 (Herbalism)");
        result.Should().Contain("[14] + 3 = 17");
        result.Should().Contain("[x] SUCCESS!");
    }

    [Test]
    public void FormatCompleteCheck_WithFailedCheck_ContainsFailureIndicator()
    {
        // Arrange
        const string skillName = "Mining";
        const int dc = 15;
        const int rawRoll = 6;
        const int modifier = 2;
        const int total = 8;
        const bool success = false;

        // Act
        var result = _renderer.FormatCompleteCheck(skillName, dc, rawRoll, modifier, total, success);

        // Assert
        result.Should().Contain("[ ] FAILED");
        result.Should().Contain("DC 15");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetResultColor Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void GetResultColor_Success_ReturnsSuccessColor()
    {
        // Act
        var color = _renderer.GetResultColor(true);

        // Assert
        color.Should().Be(_config.SuccessColor);
    }

    [Test]
    public void GetResultColor_Failure_ReturnsFailureColor()
    {
        // Act
        var color = _renderer.GetResultColor(false);

        // Assert
        color.Should().Be(_config.FailureColor);
    }
}
