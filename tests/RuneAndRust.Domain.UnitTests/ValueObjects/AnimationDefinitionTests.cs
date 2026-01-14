using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="AnimationDefinition"/> and <see cref="AnimationFrame"/>.
/// </summary>
[TestFixture]
public class AnimationDefinitionTests
{
    #region AnimationFrame Tests

    [Test]
    public void AnimationFrame_WithDefaults_HasExpectedValues()
    {
        // Arrange & Act
        var frame = new AnimationFrame("Test text", 100);

        // Assert
        frame.TextTemplate.Should().Be("Test text");
        frame.DurationMs.Should().Be(100);
        frame.Color.Should().BeNull();
        frame.Center.Should().BeFalse();
        frame.ClearPrevious.Should().BeTrue();
    }

    [Test]
    public void AnimationFrame_WithAllOptions_StoresValues()
    {
        // Arrange & Act
        var frame = new AnimationFrame(
            "Centered text",
            200,
            ConsoleColor.Red,
            PositionX: 5,
            PositionY: 10,
            Center: true,
            ClearPrevious: false);

        // Assert
        frame.TextTemplate.Should().Be("Centered text");
        frame.DurationMs.Should().Be(200);
        frame.Color.Should().Be(ConsoleColor.Red);
        frame.PositionX.Should().Be(5);
        frame.PositionY.Should().Be(10);
        frame.Center.Should().BeTrue();
        frame.ClearPrevious.Should().BeFalse();
    }

    #endregion

    #region AnimationDefinition Tests

    [Test]
    public void AnimationDefinition_WithFrames_CalculatesTotalDuration()
    {
        // Arrange
        var frames = new List<AnimationFrame>
        {
            new("Frame 1", 100),
            new("Frame 2", 200),
            new("Frame 3", 150)
        };

        // Act
        var definition = new AnimationDefinition(AnimationType.AttackHit, frames);

        // Assert
        definition.TotalDurationMs.Should().Be(450);
        definition.FrameCount.Should().Be(3);
    }

    [Test]
    public void AnimationDefinition_WithVerbs_StoresVerbs()
    {
        // Arrange
        var frames = new List<AnimationFrame> { new("Test", 100) };
        var verbs = new[] { "SLASH", "STRIKE", "HIT" };

        // Act
        var definition = new AnimationDefinition(
            AnimationType.AttackHit, 
            frames, 
            Verbs: verbs);

        // Assert
        definition.Verbs.Should().HaveCount(3);
        definition.Verbs.Should().Contain("SLASH");
    }

    [Test]
    public void AnimationDefinition_DefaultDuration_HasDefaultValue()
    {
        // Arrange
        var frames = new List<AnimationFrame> { new("Test", 100) };

        // Act
        var definition = new AnimationDefinition(AnimationType.AttackMiss, frames);

        // Assert
        definition.DefaultDurationMs.Should().Be(150);
    }

    [Test]
    public void AnimationDefinition_EmptyFrames_HasZeroDuration()
    {
        // Arrange & Act
        var definition = new AnimationDefinition(AnimationType.Death, new List<AnimationFrame>());

        // Assert
        definition.TotalDurationMs.Should().Be(0);
        definition.FrameCount.Should().Be(0);
    }

    #endregion
}
