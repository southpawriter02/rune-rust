using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.Gui.Controls;
using RuneAndRust.Presentation.Gui.Enums;

namespace RuneAndRust.Application.UnitTests.Presentation.Gui.Controls;

/// <summary>
/// Unit tests for <see cref="DamagePopupControl"/>.
/// </summary>
[TestFixture]
public class DamagePopupControlTests
{
    private DamagePopupControl _control = null!;

    [SetUp]
    public void SetUp()
    {
        _control = new DamagePopupControl();
    }

    /// <summary>
    /// Verifies that damage displays as negative value.
    /// </summary>
    [Test]
    public void DisplayText_ForDamage_ReturnsNegativeValue()
    {
        // Arrange
        _control.Value = 12;
        _control.PopupType = DamagePopupType.Damage;
        _control.IsCritical = false;

        // Assert
        _control.DisplayText.Should().Be("-12");
    }

    /// <summary>
    /// Verifies that critical hits display with stars.
    /// </summary>
    [Test]
    public void DisplayText_ForCriticalDamage_ReturnsStarFormat()
    {
        // Arrange
        _control.Value = 24;
        _control.PopupType = DamagePopupType.Damage;
        _control.IsCritical = true;

        // Assert
        _control.DisplayText.Should().Be("★ 24 ★");
    }

    /// <summary>
    /// Verifies that healing displays as positive value.
    /// </summary>
    [Test]
    public void DisplayText_ForHealing_ReturnsPositiveValue()
    {
        // Arrange
        _control.Value = 15;
        _control.PopupType = DamagePopupType.Healing;

        // Assert
        _control.DisplayText.Should().Be("+15");
    }

    /// <summary>
    /// Verifies that miss displays correct text.
    /// </summary>
    [Test]
    public void DisplayText_ForMiss_ReturnsMissText()
    {
        // Arrange
        _control.PopupType = DamagePopupType.Miss;

        // Assert
        _control.DisplayText.Should().Be("MISS");
    }

    /// <summary>
    /// Verifies that block displays correct text.
    /// </summary>
    [Test]
    public void DisplayText_ForBlock_ReturnsBlockedText()
    {
        // Arrange
        _control.PopupType = DamagePopupType.Block;

        // Assert
        _control.DisplayText.Should().Be("BLOCKED");
    }

    /// <summary>
    /// Verifies that critical hits have larger font size.
    /// </summary>
    [Test]
    public void FontSize_ForCritical_Returns24()
    {
        // Arrange & Act
        _control.IsCritical = true;

        // Assert
        _control.FontSize.Should().Be(24);
    }

    /// <summary>
    /// Verifies that normal hits have standard font size.
    /// </summary>
    [Test]
    public void FontSize_ForNonCritical_Returns18()
    {
        // Arrange & Act
        _control.IsCritical = false;

        // Assert
        _control.FontSize.Should().Be(18);
    }
}
