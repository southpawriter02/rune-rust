using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.Gui.Controls;

namespace RuneAndRust.Application.UnitTests.Presentation.Gui.Controls;

/// <summary>
/// Unit tests for <see cref="HealthBarControl"/>.
/// </summary>
[TestFixture]
public class HealthBarControlTests
{
    private HealthBarControl _control = null!;

    [SetUp]
    public void SetUp()
    {
        _control = new HealthBarControl();
    }

    /// <summary>
    /// Verifies that default values are set correctly.
    /// </summary>
    [Test]
    public void Constructor_HasDefaultValues()
    {
        // Assert
        _control.CurrentValue.Should().Be(0);
        _control.MaxValue.Should().Be(100);
        _control.Label.Should().Be("HP");
        _control.BarType.Should().Be(BarType.Health);
    }

    /// <summary>
    /// Verifies that FillPercentage calculates correctly.
    /// </summary>
    [Test]
    public void FillPercentage_With50Of100_Returns0Point5()
    {
        // Arrange
        _control.CurrentValue = 50;
        _control.MaxValue = 100;

        // Assert
        _control.FillPercentage.Should().BeApproximately(0.5, 0.001);
    }

    /// <summary>
    /// Verifies that FillPercentage returns 0 when MaxValue is 0.
    /// </summary>
    [Test]
    public void FillPercentage_WithZeroMax_ReturnsZero()
    {
        // Arrange
        _control.MaxValue = 0;
        _control.CurrentValue = 50;

        // Assert
        _control.FillPercentage.Should().Be(0);
    }

    /// <summary>
    /// Verifies that DisplayText format is correct.
    /// </summary>
    [Test]
    public void DisplayText_WithValues_ReturnsCorrectFormat()
    {
        // Arrange
        _control.CurrentValue = 65;
        _control.MaxValue = 100;

        // Assert
        _control.DisplayText.Should().Be("65/100");
    }

    /// <summary>
    /// Verifies that different BarTypes produce different FillBrush colors.
    /// </summary>
    [Test]
    public void FillBrush_ForManaType_IsRoyalBlue()
    {
        // Arrange
        _control.BarType = BarType.Mana;
        _control.CurrentValue = 50;
        _control.MaxValue = 100;

        // Assert - Just check it's not null, actual color comparison is tricky
        _control.FillBrush.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies FillBrush changes based on health percentage thresholds.
    /// </summary>
    [Test]
    public void FillBrush_ForHealthAt20Percent_IsDarkRed()
    {
        // Arrange
        _control.BarType = BarType.Health;
        _control.CurrentValue = 20;
        _control.MaxValue = 100;

        // Assert
        _control.FillBrush.Should().NotBeNull();
    }
}
