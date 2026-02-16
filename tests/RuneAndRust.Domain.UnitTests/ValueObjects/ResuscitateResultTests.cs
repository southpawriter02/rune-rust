using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="ResuscitateResult"/>.
/// Validates computed properties, default values, and display methods
/// for the Bone-Setter Tier 3 revival ability result.
/// </summary>
[TestFixture]
public class ResuscitateResultTests
{
    [Test]
    public void HpAfter_AlwaysReturnsOne()
    {
        // Arrange & Act
        var result = new ResuscitateResult { HpBefore = 0 };

        // Assert — Resuscitate always restores to exactly 1 HP
        result.HpAfter.Should().Be(1);
    }

    [Test]
    public void SuppliesUsed_AlwaysReturnsTwo()
    {
        // Arrange & Act
        var result = new ResuscitateResult();

        // Assert — Resuscitate always consumes exactly 2 supplies
        result.SuppliesUsed.Should().Be(2);
    }

    [Test]
    public void GetStatusMessage_ContainsTargetNameAndHpValues()
    {
        // Arrange
        var result = new ResuscitateResult
        {
            TargetName = "Fallen Warrior",
            HpBefore = 0
        };

        // Act
        var message = result.GetStatusMessage();

        // Assert
        message.Should().Contain("Fallen Warrior");
        message.Should().Contain("0");
        message.Should().Contain("1");
    }

    [Test]
    public void GetStatusMessage_ContainsResuscitatedKeyword()
    {
        // Arrange
        var result = new ResuscitateResult
        {
            TargetName = "Scout",
            HpBefore = 0
        };

        // Act
        var message = result.GetStatusMessage();

        // Assert
        message.Should().Contain("RESUSCITATED");
    }

    [Test]
    public void DefaultValues_InitializeCorrectly()
    {
        // Arrange & Act
        var result = new ResuscitateResult();

        // Assert
        result.TargetId.Should().Be(Guid.Empty);
        result.TargetName.Should().Be(string.Empty);
        result.HpBefore.Should().Be(0);
        result.SuppliesRemaining.Should().Be(0);
        result.Method.Should().Be(ResurrectionMethod.SkillBasedResuscitation);
        result.ResurrectionMessage.Should().Be(string.Empty);
    }

    [Test]
    public void Method_PreservesResurrectionMethod()
    {
        // Arrange & Act
        var result = new ResuscitateResult
        {
            Method = ResurrectionMethod.SkillBasedResuscitation
        };

        // Assert
        result.Method.Should().Be(ResurrectionMethod.SkillBasedResuscitation);
    }
}
