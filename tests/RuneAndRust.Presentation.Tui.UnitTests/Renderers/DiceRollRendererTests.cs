// ═══════════════════════════════════════════════════════════════════════════════
// DiceRollRendererTests.cs
// Unit tests for DiceRollRenderer functionality.
// Version: 0.13.4d
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Records;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UnitTests.Renderers;

/// <summary>
/// Unit tests for the DiceRollRenderer class.
/// </summary>
[TestFixture]
public class DiceRollRendererTests
{
    private DiceHistoryPanelConfig _config = null!;
    private DiceRollRenderer _renderer = null!;

    [SetUp]
    public void SetUp()
    {
        _config = new DiceHistoryPanelConfig();
        _renderer = new DiceRollRenderer(_config);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithNullConfig_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new DiceRollRenderer(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("config");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FORMATROLL TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void FormatRoll_NaturalTwenty_IncludesExclamationMark()
    {
        // Act
        var result = _renderer.FormatRoll(20, isCriticalSuccess: true, isCriticalFailure: false);

        // Assert
        result.Should().Be("20!");
    }

    [Test]
    public void FormatRoll_NaturalOne_IncludesExclamationMark()
    {
        // Act
        var result = _renderer.FormatRoll(1, isCriticalSuccess: false, isCriticalFailure: true);

        // Assert
        result.Should().Be("1!");
    }

    [Test]
    public void FormatRoll_NormalRoll_NoExclamationMark()
    {
        // Act
        var result = _renderer.FormatRoll(14, isCriticalSuccess: false, isCriticalFailure: false);

        // Assert
        result.Should().Be("14");
    }

    [Test]
    public void FormatRoll_AutoDetection_NaturalTwenty_IncludesExclamation()
    {
        // Act
        var result = _renderer.FormatRoll(20);

        // Assert
        result.Should().Be("20!");
    }

    [Test]
    public void FormatRoll_AutoDetection_NaturalOne_IncludesExclamation()
    {
        // Act
        var result = _renderer.FormatRoll(1);

        // Assert
        result.Should().Be("1!");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FORMATROLLLIST TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void FormatRollList_WithCriticals_HighlightsCriticalRolls()
    {
        // Arrange
        var rolls = new[]
        {
            DiceRollRecord.Create("1d20", 18, new[] { 18 }, "attack"),
            DiceRollRecord.Create("1d20", 20, new[] { 20 }, "attack"),
            DiceRollRecord.Create("1d20", 1, new[] { 1 }, "attack"),
            DiceRollRecord.Create("1d20", 12, new[] { 12 }, "attack")
        };

        // Act
        var result = _renderer.FormatRollList(rolls);

        // Assert
        result.Should().Contain("20!");
        result.Should().Contain("1!");
        result.Should().Contain("18");
        result.Should().Contain("12");
    }

    [Test]
    public void FormatRollList_EmptyList_ReturnsNoRollsMessage()
    {
        // Arrange
        var rolls = Array.Empty<DiceRollRecord>();

        // Act
        var result = _renderer.FormatRollList(rolls);

        // Assert
        result.Should().Be("No rolls recorded");
    }

    [Test]
    public void FormatRollList_IntValues_FormatsCorrectly()
    {
        // Arrange
        var values = new[] { 20, 15, 1, 8 };

        // Act
        var result = _renderer.FormatRollList(values);

        // Assert
        result.Should().Be("20!, 15, 1!, 8");
    }

    [Test]
    public void FormatRollList_NullList_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => _renderer.FormatRollList((IEnumerable<DiceRollRecord>)null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GETROLLCOLOR TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void GetRollColor_NaturalTwenty_ReturnsYellow()
    {
        // Act
        var result = _renderer.GetRollColor(20);

        // Assert
        result.Should().Be(ConsoleColor.Yellow);
    }

    [Test]
    public void GetRollColor_NaturalOne_ReturnsRed()
    {
        // Act
        var result = _renderer.GetRollColor(1);

        // Assert
        result.Should().Be(ConsoleColor.Red);
    }

    [Test]
    public void GetRollColor_HighRoll_ReturnsGreen()
    {
        // Act
        var result = _renderer.GetRollColor(17);

        // Assert
        result.Should().Be(ConsoleColor.Green);
    }

    [Test]
    public void GetRollColor_AverageRoll_ReturnsWhite()
    {
        // Act
        var result = _renderer.GetRollColor(12);

        // Assert
        result.Should().Be(ConsoleColor.White);
    }

    [Test]
    public void GetRollColor_LowRoll_ReturnsGray()
    {
        // Act
        var result = _renderer.GetRollColor(7);

        // Assert
        result.Should().Be(ConsoleColor.Gray);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STATIC HELPER TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void IsCritical_NaturalTwenty_ReturnsTrue()
    {
        // Assert
        DiceRollRenderer.IsCritical(20).Should().BeTrue();
    }

    [Test]
    public void IsCritical_NaturalOne_ReturnsTrue()
    {
        // Assert
        DiceRollRenderer.IsCritical(1).Should().BeTrue();
    }

    [Test]
    public void IsCritical_NormalRoll_ReturnsFalse()
    {
        // Assert
        DiceRollRenderer.IsCritical(15).Should().BeFalse();
    }
}
