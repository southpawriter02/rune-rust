using FluentAssertions;
using RuneAndRust.Core.Enums;
using Xunit;

namespace RuneAndRust.Tests.Core;

/// <summary>
/// Tests for the NodeStatus enum.
/// Validates the node status values and structure.
/// </summary>
/// <remarks>See: v0.4.1c (The Tree of Runes) for implementation.</remarks>
public class NodeStatusTests
{
    [Fact]
    public void NodeStatus_ShouldHaveExactlyFourValues()
    {
        // Arrange
        var values = Enum.GetValues<NodeStatus>();

        // Assert
        values.Should().HaveCount(4, "NodeStatus should have exactly 4 values: Locked, InsufficientPP, Available, Unlocked");
    }

    [Fact]
    public void NodeStatus_ShouldContain_Locked()
    {
        // Assert
        Enum.IsDefined(typeof(NodeStatus), NodeStatus.Locked).Should().BeTrue();
    }

    [Fact]
    public void NodeStatus_ShouldContain_InsufficientPP()
    {
        // Assert
        Enum.IsDefined(typeof(NodeStatus), NodeStatus.InsufficientPP).Should().BeTrue();
    }

    [Fact]
    public void NodeStatus_ShouldContain_Available()
    {
        // Assert
        Enum.IsDefined(typeof(NodeStatus), NodeStatus.Available).Should().BeTrue();
    }

    [Fact]
    public void NodeStatus_ShouldContain_Unlocked()
    {
        // Assert
        Enum.IsDefined(typeof(NodeStatus), NodeStatus.Unlocked).Should().BeTrue();
    }

    [Fact]
    public void NodeStatus_EnumValues_ShouldBeSequential()
    {
        // Assert
        ((int)NodeStatus.Locked).Should().Be(0);
        ((int)NodeStatus.InsufficientPP).Should().Be(1);
        ((int)NodeStatus.Available).Should().Be(2);
        ((int)NodeStatus.Unlocked).Should().Be(3);
    }

    [Theory]
    [InlineData(NodeStatus.Locked, "Locked")]
    [InlineData(NodeStatus.InsufficientPP, "InsufficientPP")]
    [InlineData(NodeStatus.Available, "Available")]
    [InlineData(NodeStatus.Unlocked, "Unlocked")]
    public void NodeStatus_ToString_ReturnsExpectedName(NodeStatus status, string expectedName)
    {
        // Assert
        status.ToString().Should().Be(expectedName);
    }

    [Theory]
    [InlineData(0, NodeStatus.Locked)]
    [InlineData(1, NodeStatus.InsufficientPP)]
    [InlineData(2, NodeStatus.Available)]
    [InlineData(3, NodeStatus.Unlocked)]
    public void NodeStatus_FromInt_ReturnsCorrectStatus(int value, NodeStatus expected)
    {
        // Act
        var status = (NodeStatus)value;

        // Assert
        status.Should().Be(expected);
    }

    [Fact]
    public void NodeStatus_DefaultValue_ShouldBeLocked()
    {
        // Arrange & Act
        var defaultStatus = default(NodeStatus);

        // Assert
        defaultStatus.Should().Be(NodeStatus.Locked);
    }
}
