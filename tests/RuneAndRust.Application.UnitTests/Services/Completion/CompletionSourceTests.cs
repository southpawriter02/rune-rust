using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Services.Completion;

namespace RuneAndRust.Application.UnitTests.Services.Completion;

/// <summary>
/// Unit tests for completion sources.
/// </summary>
[TestFixture]
public class CompletionSourceTests
{
    #region CommandCompletionSource Tests

    [Test]
    public void CommandSource_AppliesTo_FirstWord()
    {
        // Arrange
        var source = new CommandCompletionSource(new[] { "attack" });
        var context = CompletionContext.FromInput("a");

        // Assert
        source.AppliesTo(context).Should().BeTrue();
    }

    [Test]
    public void CommandSource_DoesNotApply_SecondWord()
    {
        // Arrange
        var source = new CommandCompletionSource(new[] { "attack" });
        var context = CompletionContext.FromInput("attack gob");

        // Assert
        source.AppliesTo(context).Should().BeFalse();
    }

    [Test]
    public void CommandSource_GetMatches_ReturnsMatchingCommands()
    {
        // Arrange
        var source = new CommandCompletionSource(new[] { "attack", "armor", "help" });
        var context = CompletionContext.FromInput("a");

        // Act
        var result = source.GetMatches("a", context).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain("attack");
        result.Should().Contain("armor");
    }

    #endregion

    #region TargetCompletionSource Tests

    [Test]
    public void TargetSource_AppliesTo_AttackCommand()
    {
        // Arrange
        var source = new TargetCompletionSource();
        var context = CompletionContext.FromInput("attack ");

        // Assert
        source.AppliesTo(context).Should().BeTrue();
    }

    [Test]
    public void TargetSource_GetMatches_ReturnsTargets()
    {
        // Arrange
        var source = new TargetCompletionSource();
        var context = CompletionContext.FromInput(
            "attack sk",
            targets: new[] { "skeleton", "goblin" });

        // Act
        var result = source.GetMatches("sk", context).ToList();

        // Assert
        result.Should().Contain("skeleton");
    }

    #endregion

    #region ItemCompletionSource Tests

    [Test]
    public void ItemSource_AppliesTo_UseCommand()
    {
        // Arrange
        var source = new ItemCompletionSource();
        var context = CompletionContext.FromInput("use ");

        // Assert
        source.AppliesTo(context).Should().BeTrue();
    }

    [Test]
    public void ItemSource_GetMatches_ReturnsItems()
    {
        // Arrange
        var source = new ItemCompletionSource();
        var context = CompletionContext.FromInput(
            "use he",
            items: new[] { "healing-potion", "mana-potion" });

        // Act
        var result = source.GetMatches("he", context).ToList();

        // Assert
        result.Should().Contain("healing-potion");
    }

    #endregion

    #region DirectionCompletionSource Tests

    [Test]
    public void DirectionSource_AppliesTo_GoCommand()
    {
        // Arrange
        var source = new DirectionCompletionSource();
        var context = CompletionContext.FromInput("go ");

        // Assert
        source.AppliesTo(context).Should().BeTrue();
    }

    [Test]
    public void DirectionSource_GetMatches_ReturnsDirections()
    {
        // Arrange
        var source = new DirectionCompletionSource();
        var context = CompletionContext.FromInput("go n");

        // Act
        var result = source.GetMatches("n", context).ToList();

        // Assert
        result.Should().Contain("north");
        result.Should().Contain("n");
    }

    #endregion
}
