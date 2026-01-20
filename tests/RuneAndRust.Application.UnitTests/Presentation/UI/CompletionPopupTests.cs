using FluentAssertions;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.UI;

namespace RuneAndRust.Application.UnitTests.Presentation.UI;

/// <summary>
/// Unit tests for <see cref="CompletionPopup"/>.
/// </summary>
[TestFixture]
public class CompletionPopupTests
{
    private Mock<ITerminalService> _mockTerminal = null!;
    private CompletionPopup _popup = null!;

    [SetUp]
    public void Setup()
    {
        _mockTerminal = new Mock<ITerminalService>();
        _popup = new CompletionPopup(_mockTerminal.Object);
    }

    #region Show Tests

    [Test]
    public void Show_MultipleSuggestions_BecomesVisible()
    {
        // Arrange
        var suggestions = new[] { "attack", "armor", "abilities" };

        // Act
        _popup.Show(suggestions, 0, 0);

        // Assert
        _popup.IsVisible.Should().BeTrue();
    }

    [Test]
    public void Show_SingleSuggestion_DoesNotShow()
    {
        // Arrange
        var suggestions = new[] { "attack" };

        // Act
        _popup.Show(suggestions, 0, 0);

        // Assert
        _popup.IsVisible.Should().BeFalse();
    }

    #endregion

    #region Hide Tests

    [Test]
    public void Hide_WhenVisible_BecomesInvisible()
    {
        // Arrange
        _popup.Show(new[] { "a", "b" }, 0, 0);

        // Act
        _popup.Hide();

        // Assert
        _popup.IsVisible.Should().BeFalse();
    }

    #endregion

    #region Navigation Tests

    [Test]
    public void SelectNext_IncrementsIndex()
    {
        // Arrange
        _popup.Show(new[] { "a", "b", "c" }, 0, 0);

        // Act
        _popup.SelectNext();

        // Assert
        _popup.SelectedIndex.Should().Be(1);
    }

    [Test]
    public void SelectNext_AtEnd_WrapsToStart()
    {
        // Arrange
        _popup.Show(new[] { "a", "b" }, 0, 0);
        _popup.SelectNext();

        // Act
        _popup.SelectNext();

        // Assert
        _popup.SelectedIndex.Should().Be(0);
    }

    [Test]
    public void SelectPrevious_AtStart_WrapsToEnd()
    {
        // Arrange
        _popup.Show(new[] { "a", "b", "c" }, 0, 0);

        // Act
        _popup.SelectPrevious();

        // Assert
        _popup.SelectedIndex.Should().Be(2);
    }

    #endregion

    #region GetSelected Tests

    [Test]
    public void GetSelected_ReturnsCurrentSuggestion()
    {
        // Arrange
        _popup.Show(new[] { "attack", "armor" }, 0, 0);
        _popup.SelectNext();

        // Act
        var result = _popup.GetSelected();

        // Assert
        result.Should().Be("armor");
    }

    [Test]
    public void GetSelected_NotVisible_ReturnsNull()
    {
        // Act
        var result = _popup.GetSelected();

        // Assert
        result.Should().BeNull();
    }

    #endregion
}
