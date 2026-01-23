using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="ChainedCheckStep"/> value object.
/// </summary>
[TestFixture]
public class ChainedCheckStepTests
{
    [Test]
    public void Create_WithValidParameters_CreatesStepWithCorrectProperties()
    {
        // Act
        var step = ChainedCheckStep.Create("step-1", "Access Terminal", "system-bypass", 3, retries: 1);

        // Assert
        step.StepId.Should().Be("step-1");
        step.Name.Should().Be("Access Terminal");
        step.SkillId.Should().Be("system-bypass");
        step.DifficultyClass.Should().Be(3);
        step.MaxRetries.Should().Be(1);
        step.AllowsRetries.Should().BeTrue();
    }

    [Test]
    public void Create_WithNoRetries_AllowsRetriesIsFalse()
    {
        // Act
        var step = ChainedCheckStep.Create("step-1", "Instant Check", "skill", 2, retries: 0);

        // Assert
        step.MaxRetries.Should().Be(0);
        step.AllowsRetries.Should().BeFalse();
    }

    [Test]
    public void Create_SetsApproprirateDifficultyName()
    {
        // Arrange & Act
        var trivial = ChainedCheckStep.Create("t", "Trivial", "sk", 1);
        var easy = ChainedCheckStep.Create("e", "Easy", "sk", 2);
        var moderate = ChainedCheckStep.Create("m", "Moderate", "sk", 3);
        var challenging = ChainedCheckStep.Create("c", "Challenging", "sk", 4);
        var hard = ChainedCheckStep.Create("h", "Hard", "sk", 5);

        // Assert
        trivial.DifficultyName.Should().Be("Trivial");
        easy.DifficultyName.Should().Be("Easy");
        moderate.DifficultyName.Should().Be("Moderate");
        challenging.DifficultyName.Should().Be("Challenging");
        hard.DifficultyName.Should().Be("Hard");
    }

    [Test]
    public void ToDisplayString_FormatsCorrectly()
    {
        // Arrange
        var step = ChainedCheckStep.Create("step-1", "Bypass Firewall", "system-bypass", 4, retries: 2);

        // Act
        var display = step.ToDisplayString();

        // Assert
        display.Should().Be("Bypass Firewall: system-bypass DC 4 (2 retries)");
    }

    [Test]
    public void ToDisplayString_WithNoRetries_OmitsRetriesSuffix()
    {
        // Arrange
        var step = ChainedCheckStep.Create("step-1", "Final Step", "skill", 3, retries: 0);

        // Act
        var display = step.ToDisplayString();

        // Assert
        display.Should().Be("Final Step: skill DC 3");
        display.Should().NotContain("retries");
    }
}
