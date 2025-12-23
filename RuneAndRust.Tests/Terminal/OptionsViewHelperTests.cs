using FluentAssertions;
using RuneAndRust.Core.Enums;
using RuneAndRust.Terminal.Rendering;
using Xunit;

namespace RuneAndRust.Tests.Terminal;

/// <summary>
/// Unit tests for OptionsViewHelper static methods (v0.3.10b, extended v0.3.10c).
/// Validates slider rendering, toggle formatting, theme name resolution, and key binding helpers.
/// </summary>
public class OptionsViewHelperTests
{
    #region RenderSlider Tests

    [Fact]
    public void RenderSlider_ReturnsAllEmpty_AtMinimum()
    {
        // Arrange
        var value = 0;
        var min = 0;
        var max = 100;
        var width = 10;

        // Act
        var result = OptionsViewHelper.RenderSlider(value, min, max, width);

        // Assert
        result.Should().Contain("[green]");
        result.Should().Contain("[grey]");
        // At minimum, all blocks should be empty (grey)
        result.Should().Contain("░░░░░░░░░░");
    }

    [Fact]
    public void RenderSlider_ReturnsAllFilled_AtMaximum()
    {
        // Arrange
        var value = 100;
        var min = 0;
        var max = 100;
        var width = 10;

        // Act
        var result = OptionsViewHelper.RenderSlider(value, min, max, width);

        // Assert
        result.Should().Contain("[green]");
        // At maximum, all blocks should be filled (green)
        result.Should().Contain("██████████");
    }

    [Fact]
    public void RenderSlider_ReturnsHalfFilled_AtMidpoint()
    {
        // Arrange
        var value = 50;
        var min = 0;
        var max = 100;
        var width = 10;

        // Act
        var result = OptionsViewHelper.RenderSlider(value, min, max, width);

        // Assert
        result.Should().Contain("[green]█████[/]");
        result.Should().Contain("[grey]░░░░░[/]");
    }

    [Fact]
    public void RenderSlider_ClampsValueToMin_WhenBelowRange()
    {
        // Arrange
        var value = -10;
        var min = 0;
        var max = 100;

        // Act
        var result = OptionsViewHelper.RenderSlider(value, min, max, 10);

        // Assert - should behave as if value is at minimum
        result.Should().Contain("░░░░░░░░░░");
    }

    [Fact]
    public void RenderSlider_ClampsValueToMax_WhenAboveRange()
    {
        // Arrange
        var value = 150;
        var min = 0;
        var max = 100;

        // Act
        var result = OptionsViewHelper.RenderSlider(value, min, max, 10);

        // Assert - should behave as if value is at maximum
        result.Should().Contain("██████████");
    }

    #endregion

    #region FormatToggle Tests

    [Fact]
    public void FormatToggle_ReturnsGreenOn_WhenTrue()
    {
        // Act
        var result = OptionsViewHelper.FormatToggle(true);

        // Assert
        result.Should().Be("[green]ON[/]");
    }

    [Fact]
    public void FormatToggle_ReturnsGreyOff_WhenFalse()
    {
        // Act
        var result = OptionsViewHelper.FormatToggle(false);

        // Assert
        result.Should().Be("[grey]OFF[/]");
    }

    #endregion

    #region GetThemeName Tests

    [Theory]
    [InlineData(0, "Standard")]
    [InlineData(1, "High Contrast")]
    [InlineData(2, "Protanopia")]
    [InlineData(3, "Deuteranopia")]
    [InlineData(4, "Tritanopia")]
    public void GetThemeName_ReturnsCorrectName_ForValidTheme(int themeValue, string expectedName)
    {
        // Act
        var result = OptionsViewHelper.GetThemeName(themeValue);

        // Assert
        result.Should().Be(expectedName);
    }

    [Fact]
    public void GetThemeName_ReturnsUnknown_ForInvalidTheme()
    {
        // Act
        var result = OptionsViewHelper.GetThemeName(99);

        // Assert
        result.Should().Be("Unknown");
    }

    #endregion

    #region GetTabDisplayName Tests

    [Theory]
    [InlineData(OptionsTab.General, "General")]
    [InlineData(OptionsTab.Display, "Display")]
    [InlineData(OptionsTab.Audio, "Audio")]
    [InlineData(OptionsTab.Controls, "Controls")]
    public void GetTabDisplayName_ReturnsCorrectName_ForValidTab(OptionsTab tab, string expectedName)
    {
        // Act
        var result = OptionsViewHelper.GetTabDisplayName(tab);

        // Assert
        result.Should().Be(expectedName);
    }

    #endregion

    #region FormatSliderValue Tests

    [Theory]
    [InlineData(50, "MasterVolume", "50%")]
    [InlineData(100, "TextSpeed", "100%")]
    [InlineData(5, "AutosaveIntervalMinutes", "5 min")]
    [InlineData(30, "AutosaveIntervalMinutes", "30 min")]
    public void FormatSliderValue_FormatsCorrectly_ForKnownProperties(int value, string propertyName, string expected)
    {
        // Act
        var result = OptionsViewHelper.FormatSliderValue(value, propertyName);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void FormatSliderValue_ReturnsRawValue_ForUnknownProperty()
    {
        // Act
        var result = OptionsViewHelper.FormatSliderValue(42, "UnknownProperty");

        // Assert
        result.Should().Be("42");
    }

    #endregion

    #region CycleTheme Tests

    [Fact]
    public void CycleTheme_IncrementsToNextTheme()
    {
        // Arrange
        var currentTheme = 0; // Standard

        // Act
        var result = OptionsViewHelper.CycleTheme(currentTheme, 1);

        // Assert
        result.Should().Be(1); // High Contrast
    }

    [Fact]
    public void CycleTheme_DecrementsToThreviousTheme()
    {
        // Arrange
        var currentTheme = 1; // High Contrast

        // Act
        var result = OptionsViewHelper.CycleTheme(currentTheme, -1);

        // Assert
        result.Should().Be(0); // Standard
    }

    [Fact]
    public void CycleTheme_WrapsToFirst_WhenAtLast()
    {
        // Arrange - Tritanopia is the last theme (index 4)
        var currentTheme = 4;

        // Act
        var result = OptionsViewHelper.CycleTheme(currentTheme, 1);

        // Assert
        result.Should().Be(0); // Wraps to Standard
    }

    [Fact]
    public void CycleTheme_WrapsToLast_WhenAtFirst()
    {
        // Arrange - Standard is the first theme (index 0)
        var currentTheme = 0;

        // Act
        var result = OptionsViewHelper.CycleTheme(currentTheme, -1);

        // Assert
        result.Should().Be(4); // Wraps to Tritanopia
    }

    #endregion

    #region GetCommandDisplayName Tests (v0.3.10c)

    [Theory]
    [InlineData("north", "Move North")]
    [InlineData("south", "Move South")]
    [InlineData("east", "Move East")]
    [InlineData("west", "Move West")]
    [InlineData("up", "Move Up")]
    [InlineData("down", "Move Down")]
    [InlineData("confirm", "Confirm")]
    [InlineData("cancel", "Cancel/Back")]
    [InlineData("menu", "Menu")]
    [InlineData("help", "Help")]
    [InlineData("inventory", "Inventory")]
    [InlineData("character", "Character")]
    [InlineData("journal", "Journal")]
    [InlineData("bench", "Crafting")]
    [InlineData("interact", "Interact")]
    [InlineData("look", "Look")]
    [InlineData("search", "Search")]
    [InlineData("wait", "Wait")]
    [InlineData("attack", "Attack")]
    [InlineData("light", "Light Attack")]
    [InlineData("heavy", "Heavy Attack")]
    public void GetCommandDisplayName_ReturnsHumanReadable_ForAllCommands(string command, string expectedName)
    {
        // Act
        var result = OptionsViewHelper.GetCommandDisplayName(command);

        // Assert
        result.Should().Be(expectedName);
    }

    [Fact]
    public void GetCommandDisplayName_ReturnsOriginal_ForUnknownCommand()
    {
        // Act
        var result = OptionsViewHelper.GetCommandDisplayName("unknown_command");

        // Assert
        result.Should().Be("unknown_command");
    }

    #endregion

    #region GetCommandCategory Tests (v0.3.10c)

    [Theory]
    [InlineData("north", "Movement")]
    [InlineData("south", "Movement")]
    [InlineData("east", "Movement")]
    [InlineData("west", "Movement")]
    [InlineData("up", "Movement")]
    [InlineData("down", "Movement")]
    [InlineData("confirm", "Core")]
    [InlineData("cancel", "Core")]
    [InlineData("menu", "Core")]
    [InlineData("help", "Core")]
    [InlineData("inventory", "Screens")]
    [InlineData("character", "Screens")]
    [InlineData("journal", "Screens")]
    [InlineData("bench", "Screens")]
    [InlineData("interact", "Gameplay")]
    [InlineData("look", "Gameplay")]
    [InlineData("search", "Gameplay")]
    [InlineData("wait", "Gameplay")]
    [InlineData("attack", "Combat")]
    [InlineData("light", "Combat")]
    [InlineData("heavy", "Combat")]
    public void GetCommandCategory_ReturnsCorrectCategory_ForAllCommands(string command, string expectedCategory)
    {
        // Act
        var result = OptionsViewHelper.GetCommandCategory(command);

        // Assert
        result.Should().Be(expectedCategory);
    }

    [Fact]
    public void GetCommandCategory_ReturnsOther_ForUnknownCommand()
    {
        // Act
        var result = OptionsViewHelper.GetCommandCategory("unknown_command");

        // Assert
        result.Should().Be("Other");
    }

    #endregion

    #region FormatKeyName Tests (v0.3.10c)

    [Fact]
    public void FormatKeyName_ReturnsRedUnbound_ForNull()
    {
        // Act
        var result = OptionsViewHelper.FormatKeyName(null);

        // Assert
        result.Should().Be("[red][Unbound][/]");
    }

    [Theory]
    [InlineData(ConsoleKey.Spacebar, "[cyan]Space[/]")]
    [InlineData(ConsoleKey.Enter, "[cyan]Enter[/]")]
    [InlineData(ConsoleKey.Escape, "[cyan]Esc[/]")]
    [InlineData(ConsoleKey.Tab, "[cyan]Tab[/]")]
    [InlineData(ConsoleKey.Backspace, "[cyan]Backspace[/]")]
    [InlineData(ConsoleKey.UpArrow, "[cyan]↑[/]")]
    [InlineData(ConsoleKey.DownArrow, "[cyan]↓[/]")]
    [InlineData(ConsoleKey.LeftArrow, "[cyan]←[/]")]
    [InlineData(ConsoleKey.RightArrow, "[cyan]→[/]")]
    public void FormatKeyName_HandlesSpecialKeys(ConsoleKey key, string expected)
    {
        // Act
        var result = OptionsViewHelper.FormatKeyName(key);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(ConsoleKey.A, "[cyan]A[/]")]
    [InlineData(ConsoleKey.N, "[cyan]N[/]")]
    [InlineData(ConsoleKey.I, "[cyan]I[/]")]
    public void FormatKeyName_FormatsRegularKeys(ConsoleKey key, string expected)
    {
        // Act
        var result = OptionsViewHelper.FormatKeyName(key);

        // Assert
        result.Should().Be(expected);
    }

    #endregion
}
