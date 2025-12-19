using FluentAssertions;
using RuneAndRust.Core.Enums;
using Xunit;

namespace RuneAndRust.Tests.Core;

/// <summary>
/// Unit tests for the PendingGameAction enum.
/// </summary>
public class PendingGameActionTests
{
    [Fact]
    public void PendingGameAction_ShouldHaveExactlyThreeValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<PendingGameAction>();

        // Assert
        values.Should().HaveCount(3);
    }

    [Fact]
    public void PendingGameAction_ShouldContain_None()
    {
        // Arrange & Act & Assert
        Enum.IsDefined(typeof(PendingGameAction), PendingGameAction.None).Should().BeTrue();
    }

    [Fact]
    public void PendingGameAction_ShouldContain_Save()
    {
        // Arrange & Act & Assert
        Enum.IsDefined(typeof(PendingGameAction), PendingGameAction.Save).Should().BeTrue();
    }

    [Fact]
    public void PendingGameAction_ShouldContain_Load()
    {
        // Arrange & Act & Assert
        Enum.IsDefined(typeof(PendingGameAction), PendingGameAction.Load).Should().BeTrue();
    }

    [Theory]
    [InlineData(PendingGameAction.None, "None")]
    [InlineData(PendingGameAction.Save, "Save")]
    [InlineData(PendingGameAction.Load, "Load")]
    public void PendingGameAction_ToString_ReturnsExpectedName(PendingGameAction action, string expectedName)
    {
        // Act
        var name = action.ToString();

        // Assert
        name.Should().Be(expectedName);
    }

    [Fact]
    public void PendingGameAction_DefaultValue_ShouldBeNone()
    {
        // Arrange & Act
        var defaultValue = default(PendingGameAction);

        // Assert
        defaultValue.Should().Be(PendingGameAction.None);
    }

    [Fact]
    public void PendingGameAction_EnumValues_ShouldBeSequential()
    {
        // Assert
        ((int)PendingGameAction.None).Should().Be(0);
        ((int)PendingGameAction.Save).Should().Be(1);
        ((int)PendingGameAction.Load).Should().Be(2);
    }
}
