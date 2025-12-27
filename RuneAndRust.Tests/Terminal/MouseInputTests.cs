using FluentAssertions;
using RuneAndRust.Core.Models.Input;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Terminal;

/// <summary>
/// Unit tests for SGR mouse sequence parsing (v0.3.23c).
/// Tests the TryParseSgrMouseSequence method in InputService.
/// </summary>
public class MouseInputTests
{
    #region Left Click Tests

    [Fact]
    public void TryParseSgrSequence_LeftClick_ReturnsCorrectEvent()
    {
        // Arrange - [<0;10;5M = Left click at column 10, row 5
        var sequence = "[<0;10;5M";

        // Act
        var result = InputService.TryParseSgrMouseSequence(sequence, out var mouseEvent);

        // Assert
        result.Should().BeTrue();
        mouseEvent.EventType.Should().Be(MouseEventType.ButtonDown);
        mouseEvent.Button.Should().Be(MouseButton.Left);
        mouseEvent.ScreenX.Should().Be(10);
        mouseEvent.ScreenY.Should().Be(5);
        mouseEvent.Modifiers.Should().Be(ConsoleModifiers.None);
    }

    [Fact]
    public void TryParseSgrSequence_LeftClick_AtOrigin_ReturnsCorrectEvent()
    {
        // Arrange - [<0;1;1M = Left click at column 1, row 1
        var sequence = "[<0;1;1M";

        // Act
        var result = InputService.TryParseSgrMouseSequence(sequence, out var mouseEvent);

        // Assert
        result.Should().BeTrue();
        mouseEvent.ScreenX.Should().Be(1);
        mouseEvent.ScreenY.Should().Be(1);
    }

    #endregion

    #region Right Click Tests

    [Fact]
    public void TryParseSgrSequence_RightClick_ReturnsCorrectEvent()
    {
        // Arrange - [<2;20;15M = Right click at column 20, row 15
        var sequence = "[<2;20;15M";

        // Act
        var result = InputService.TryParseSgrMouseSequence(sequence, out var mouseEvent);

        // Assert
        result.Should().BeTrue();
        mouseEvent.EventType.Should().Be(MouseEventType.ButtonDown);
        mouseEvent.Button.Should().Be(MouseButton.Right);
        mouseEvent.ScreenX.Should().Be(20);
        mouseEvent.ScreenY.Should().Be(15);
    }

    #endregion

    #region Middle Click Tests

    [Fact]
    public void TryParseSgrSequence_MiddleClick_ReturnsCorrectEvent()
    {
        // Arrange - [<1;5;5M = Middle click at column 5, row 5
        var sequence = "[<1;5;5M";

        // Act
        var result = InputService.TryParseSgrMouseSequence(sequence, out var mouseEvent);

        // Assert
        result.Should().BeTrue();
        mouseEvent.EventType.Should().Be(MouseEventType.ButtonDown);
        mouseEvent.Button.Should().Be(MouseButton.Middle);
        mouseEvent.ScreenX.Should().Be(5);
        mouseEvent.ScreenY.Should().Be(5);
    }

    #endregion

    #region Button Up Tests

    [Fact]
    public void TryParseSgrSequence_ButtonUp_ReturnsReleaseEvent()
    {
        // Arrange - [<0;10;5m = Left button UP at column 10, row 5 (lowercase 'm')
        var sequence = "[<0;10;5m";

        // Act
        var result = InputService.TryParseSgrMouseSequence(sequence, out var mouseEvent);

        // Assert
        result.Should().BeTrue();
        mouseEvent.EventType.Should().Be(MouseEventType.ButtonUp);
        mouseEvent.Button.Should().Be(MouseButton.Left);
    }

    [Fact]
    public void TryParseSgrSequence_RightButtonUp_ReturnsReleaseEvent()
    {
        // Arrange - [<2;15;10m = Right button UP
        var sequence = "[<2;15;10m";

        // Act
        var result = InputService.TryParseSgrMouseSequence(sequence, out var mouseEvent);

        // Assert
        result.Should().BeTrue();
        mouseEvent.EventType.Should().Be(MouseEventType.ButtonUp);
        mouseEvent.Button.Should().Be(MouseButton.Right);
    }

    #endregion

    #region Scroll Tests

    [Fact]
    public void TryParseSgrSequence_ScrollUp_ReturnsScrollEvent()
    {
        // Arrange - [<64;10;5M = Scroll up at column 10, row 5
        var sequence = "[<64;10;5M";

        // Act
        var result = InputService.TryParseSgrMouseSequence(sequence, out var mouseEvent);

        // Assert
        result.Should().BeTrue();
        mouseEvent.EventType.Should().Be(MouseEventType.Scroll);
        mouseEvent.Button.Should().Be(MouseButton.ScrollUp);
        mouseEvent.ScreenX.Should().Be(10);
        mouseEvent.ScreenY.Should().Be(5);
    }

    [Fact]
    public void TryParseSgrSequence_ScrollDown_ReturnsScrollEvent()
    {
        // Arrange - [<65;10;5M = Scroll down at column 10, row 5
        var sequence = "[<65;10;5M";

        // Act
        var result = InputService.TryParseSgrMouseSequence(sequence, out var mouseEvent);

        // Assert
        result.Should().BeTrue();
        mouseEvent.EventType.Should().Be(MouseEventType.Scroll);
        mouseEvent.Button.Should().Be(MouseButton.ScrollDown);
    }

    #endregion

    #region Modifier Tests

    [Fact]
    public void TryParseSgrSequence_WithShift_IncludesModifier()
    {
        // Arrange - [<4;10;5M = Left click + Shift (bit 2 set)
        var sequence = "[<4;10;5M";

        // Act
        var result = InputService.TryParseSgrMouseSequence(sequence, out var mouseEvent);

        // Assert
        result.Should().BeTrue();
        mouseEvent.Modifiers.Should().HaveFlag(ConsoleModifiers.Shift);
    }

    [Fact]
    public void TryParseSgrSequence_WithAlt_IncludesModifier()
    {
        // Arrange - [<8;10;5M = Left click + Alt (bit 3 set)
        var sequence = "[<8;10;5M";

        // Act
        var result = InputService.TryParseSgrMouseSequence(sequence, out var mouseEvent);

        // Assert
        result.Should().BeTrue();
        mouseEvent.Modifiers.Should().HaveFlag(ConsoleModifiers.Alt);
    }

    [Fact]
    public void TryParseSgrSequence_WithCtrl_IncludesModifier()
    {
        // Arrange - [<16;10;5M = Left click + Control (bit 4 set)
        var sequence = "[<16;10;5M";

        // Act
        var result = InputService.TryParseSgrMouseSequence(sequence, out var mouseEvent);

        // Assert
        result.Should().BeTrue();
        mouseEvent.Modifiers.Should().HaveFlag(ConsoleModifiers.Control);
    }

    [Fact]
    public void TryParseSgrSequence_WithMultipleModifiers_IncludesAllModifiers()
    {
        // Arrange - [<28;10;5M = Left click + Shift + Alt + Ctrl (4 + 8 + 16 = 28)
        var sequence = "[<28;10;5M";

        // Act
        var result = InputService.TryParseSgrMouseSequence(sequence, out var mouseEvent);

        // Assert
        result.Should().BeTrue();
        mouseEvent.Modifiers.Should().HaveFlag(ConsoleModifiers.Shift);
        mouseEvent.Modifiers.Should().HaveFlag(ConsoleModifiers.Alt);
        mouseEvent.Modifiers.Should().HaveFlag(ConsoleModifiers.Control);
    }

    #endregion

    #region Large Coordinate Tests

    [Fact]
    public void TryParseSgrSequence_LargeCoordinates_Works()
    {
        // Arrange - [<0;250;100M = Left click at column 250, row 100
        var sequence = "[<0;250;100M";

        // Act
        var result = InputService.TryParseSgrMouseSequence(sequence, out var mouseEvent);

        // Assert
        result.Should().BeTrue();
        mouseEvent.ScreenX.Should().Be(250);
        mouseEvent.ScreenY.Should().Be(100);
    }

    [Fact]
    public void TryParseSgrSequence_VeryLargeCoordinates_Works()
    {
        // Arrange - SGR can handle coordinates > 223 (unlike legacy mode)
        var sequence = "[<0;1000;500M";

        // Act
        var result = InputService.TryParseSgrMouseSequence(sequence, out var mouseEvent);

        // Assert
        result.Should().BeTrue();
        mouseEvent.ScreenX.Should().Be(1000);
        mouseEvent.ScreenY.Should().Be(500);
    }

    #endregion

    #region Invalid Sequence Tests

    [Fact]
    public void TryParseSgrSequence_InvalidFormat_ReturnsFalse()
    {
        // Arrange - Missing proper format
        var sequence = "[<invalid";

        // Act
        var result = InputService.TryParseSgrMouseSequence(sequence, out _);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void TryParseSgrSequence_IncompleteSequence_ReturnsFalse()
    {
        // Arrange - No terminator
        var sequence = "[<0;10";

        // Act
        var result = InputService.TryParseSgrMouseSequence(sequence, out _);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void TryParseSgrSequence_TooShort_ReturnsFalse()
    {
        // Arrange - Less than 6 characters
        var sequence = "[<0M";

        // Act
        var result = InputService.TryParseSgrMouseSequence(sequence, out _);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void TryParseSgrSequence_WrongPrefix_ReturnsFalse()
    {
        // Arrange - Not starting with '[<'
        var sequence = "[0;10;5M";

        // Act
        var result = InputService.TryParseSgrMouseSequence(sequence, out _);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void TryParseSgrSequence_WrongTerminator_ReturnsFalse()
    {
        // Arrange - Not ending with 'M' or 'm'
        var sequence = "[<0;10;5X";

        // Act
        var result = InputService.TryParseSgrMouseSequence(sequence, out _);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void TryParseSgrSequence_MissingSemicolons_ReturnsFalse()
    {
        // Arrange - Not exactly 3 parts
        var sequence = "[<0;10M";

        // Act
        var result = InputService.TryParseSgrMouseSequence(sequence, out _);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void TryParseSgrSequence_NonNumeric_ReturnsFalse()
    {
        // Arrange - Non-numeric values
        var sequence = "[<abc;10;5M";

        // Act
        var result = InputService.TryParseSgrMouseSequence(sequence, out _);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void TryParseSgrSequence_EmptyString_ReturnsFalse()
    {
        // Arrange
        var sequence = "";

        // Act
        var result = InputService.TryParseSgrMouseSequence(sequence, out _);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Move Event Tests

    [Fact]
    public void TryParseSgrSequence_MouseMove_ReturnsCorrectEvent()
    {
        // Arrange - [<32;15;20M = Mouse move at column 15, row 20 (bit 5 set for motion)
        var sequence = "[<32;15;20M";

        // Act
        var result = InputService.TryParseSgrMouseSequence(sequence, out var mouseEvent);

        // Assert
        result.Should().BeTrue();
        mouseEvent.EventType.Should().Be(MouseEventType.Move);
        mouseEvent.ScreenX.Should().Be(15);
        mouseEvent.ScreenY.Should().Be(20);
    }

    #endregion

    #region MouseEvent Factory Method Tests

    [Fact]
    public void MouseEvent_Click_CreatesButtonDownEvent()
    {
        // Act
        var mouseEvent = MouseEvent.Click(10, 5);

        // Assert
        mouseEvent.EventType.Should().Be(MouseEventType.ButtonDown);
        mouseEvent.Button.Should().Be(MouseButton.Left);
        mouseEvent.ScreenX.Should().Be(10);
        mouseEvent.ScreenY.Should().Be(5);
    }

    [Fact]
    public void MouseEvent_Click_WithButton_CreatesCorrectEvent()
    {
        // Act
        var mouseEvent = MouseEvent.Click(10, 5, MouseButton.Right);

        // Assert
        mouseEvent.EventType.Should().Be(MouseEventType.ButtonDown);
        mouseEvent.Button.Should().Be(MouseButton.Right);
    }

    [Fact]
    public void MouseEvent_Scroll_Up_CreatesScrollUpEvent()
    {
        // Act
        var mouseEvent = MouseEvent.Scroll(10, 5, up: true);

        // Assert
        mouseEvent.EventType.Should().Be(MouseEventType.Scroll);
        mouseEvent.Button.Should().Be(MouseButton.ScrollUp);
    }

    [Fact]
    public void MouseEvent_Scroll_Down_CreatesScrollDownEvent()
    {
        // Act
        var mouseEvent = MouseEvent.Scroll(10, 5, up: false);

        // Assert
        mouseEvent.EventType.Should().Be(MouseEventType.Scroll);
        mouseEvent.Button.Should().Be(MouseButton.ScrollDown);
    }

    [Fact]
    public void MouseEvent_IsLeftClick_TrueForLeftButtonDown()
    {
        // Arrange
        var mouseEvent = new MouseEvent(MouseEventType.ButtonDown, MouseButton.Left, 10, 5);

        // Assert
        mouseEvent.IsLeftClick.Should().BeTrue();
        mouseEvent.IsRightClick.Should().BeFalse();
    }

    [Fact]
    public void MouseEvent_IsRightClick_TrueForRightButtonDown()
    {
        // Arrange
        var mouseEvent = new MouseEvent(MouseEventType.ButtonDown, MouseButton.Right, 10, 5);

        // Assert
        mouseEvent.IsLeftClick.Should().BeFalse();
        mouseEvent.IsRightClick.Should().BeTrue();
    }

    [Fact]
    public void MouseEvent_XY_AliasesWork()
    {
        // Arrange
        var mouseEvent = new MouseEvent(MouseEventType.ButtonDown, MouseButton.Left, 25, 30);

        // Assert
        mouseEvent.X.Should().Be(25);
        mouseEvent.Y.Should().Be(30);
        mouseEvent.ScreenX.Should().Be(25);
        mouseEvent.ScreenY.Should().Be(30);
    }

    #endregion
}
