using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for the <see cref="FumbleConsequence"/> entity.
/// </summary>
[TestFixture]
public class FumbleConsequenceTests
{
    private const string ValidConsequenceId = "fc-001";
    private const string ValidCharacterId = "char-001";
    private const string ValidSkillId = "persuasion";
    private const string ValidDescription = "Trust has been shattered.";

    [Test]
    public void Constructor_WithValidParameters_CreatesActiveConsequence()
    {
        // Arrange & Act
        var consequence = CreateValidConsequence();

        // Assert
        consequence.ConsequenceId.Should().Be(ValidConsequenceId);
        consequence.CharacterId.Should().Be(ValidCharacterId);
        consequence.SkillId.Should().Be(ValidSkillId);
        consequence.IsActive.Should().BeTrue();
        consequence.DeactivatedAt.Should().BeNull();
    }

    [Test]
    [TestCase(null, "Consequence ID is required.")]
    [TestCase("", "Consequence ID is required.")]
    [TestCase("   ", "Consequence ID is required.")]
    public void Constructor_WithInvalidConsequenceId_ThrowsArgumentException(
        string? consequenceId,
        string expectedMessage)
    {
        // Arrange & Act
        var act = () => new FumbleConsequence(
            consequenceId!,
            ValidCharacterId,
            ValidSkillId,
            FumbleType.TrustShattered,
            null,
            DateTime.UtcNow,
            null,
            ValidDescription,
            null);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage($"*{expectedMessage}*");
    }

    [Test]
    public void IsExpired_BeforeExpiresAt_ReturnsFalse()
    {
        // Arrange
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var consequence = CreateValidConsequence(expiresAt: expiresAt);

        // Act
        var isExpired = consequence.IsExpired(DateTime.UtcNow);

        // Assert
        isExpired.Should().BeFalse();
    }

    [Test]
    public void IsExpired_AfterExpiresAt_ReturnsTrue()
    {
        // Arrange
        var expiresAt = DateTime.UtcNow.AddHours(-1);
        var consequence = CreateValidConsequence(expiresAt: expiresAt);

        // Act
        var isExpired = consequence.IsExpired(DateTime.UtcNow);

        // Assert
        isExpired.Should().BeTrue();
    }

    [Test]
    public void IsExpired_WhenNoExpiresAt_ReturnsFalse()
    {
        // Arrange
        var consequence = CreateValidConsequence(expiresAt: null);

        // Act
        var isExpired = consequence.IsExpired(DateTime.UtcNow.AddYears(100));

        // Assert
        isExpired.Should().BeFalse();
    }

    [Test]
    public void Deactivate_WhenActive_SetsIsActiveFalse()
    {
        // Arrange
        var consequence = CreateValidConsequence();
        var deactivatedAt = DateTime.UtcNow;

        // Act
        consequence.Deactivate("Recovery condition met", deactivatedAt);

        // Assert
        consequence.IsActive.Should().BeFalse();
        consequence.DeactivatedAt.Should().Be(deactivatedAt);
        consequence.DeactivationReason.Should().Be("Recovery condition met");
    }

    [Test]
    public void Deactivate_WhenAlreadyInactive_DoesNothing()
    {
        // Arrange
        var consequence = CreateValidConsequence();
        var firstDeactivation = DateTime.UtcNow;
        consequence.Deactivate("First reason", firstDeactivation);

        // Act
        consequence.Deactivate("Second reason", DateTime.UtcNow.AddHours(1));

        // Assert
        consequence.DeactivatedAt.Should().Be(firstDeactivation);
        consequence.DeactivationReason.Should().Be("First reason");
    }

    [Test]
    public void CanRecover_WhenConditionMet_ReturnsTrue()
    {
        // Arrange
        var consequence = CreateValidConsequence(recoveryCondition: "complete-quest");
        var completedConditions = new[] { "other-task", "complete-quest" };

        // Act
        var canRecover = consequence.CanRecover(completedConditions);

        // Assert
        canRecover.Should().BeTrue();
    }

    [Test]
    public void CanRecover_WhenConditionNotMet_ReturnsFalse()
    {
        // Arrange
        var consequence = CreateValidConsequence(recoveryCondition: "complete-quest");
        var completedConditions = new[] { "other-task" };

        // Act
        var canRecover = consequence.CanRecover(completedConditions);

        // Assert
        canRecover.Should().BeFalse();
    }

    [Test]
    public void BlocksCheck_WhenActiveAndMatchesSkillAndTarget_ReturnsTrue()
    {
        // Arrange
        var consequence = new FumbleConsequence(
            ValidConsequenceId,
            ValidCharacterId,
            ValidSkillId,
            FumbleType.TrustShattered,
            targetId: "npc-001",
            DateTime.UtcNow,
            null,
            ValidDescription,
            null);

        // Act
        var blocks = consequence.BlocksCheck(ValidSkillId, "npc-001");

        // Assert
        blocks.Should().BeTrue();
    }

    [Test]
    public void BlocksCheck_WhenInactive_ReturnsFalse()
    {
        // Arrange
        var consequence = CreateValidConsequence();
        consequence.Deactivate("Test", DateTime.UtcNow);

        // Act
        var blocks = consequence.BlocksCheck(ValidSkillId, null);

        // Assert
        blocks.Should().BeFalse();
    }

    [Test]
    public void GetDicePenalty_ForLieExposed_ReturnsNegativeTwo()
    {
        // Arrange
        var consequence = new FumbleConsequence(
            ValidConsequenceId,
            ValidCharacterId,
            "deception",
            FumbleType.LieExposed,
            null,
            DateTime.UtcNow,
            null,
            "Your lie was exposed.",
            null);

        // Act
        var penalty = consequence.GetDicePenalty();

        // Assert
        penalty.Should().Be(-2);
    }

    [Test]
    public void GetDcModifier_ForMechanismJammed_ReturnsPlusTwo()
    {
        // Arrange
        var consequence = new FumbleConsequence(
            ValidConsequenceId,
            ValidCharacterId,
            "lockpicking",
            FumbleType.MechanismJammed,
            null,
            DateTime.UtcNow,
            null,
            "The lock is jammed.",
            null);

        // Act
        var modifier = consequence.GetDcModifier();

        // Assert
        modifier.Should().Be(2);
    }

    private static FumbleConsequence CreateValidConsequence(
        DateTime? expiresAt = null,
        string? recoveryCondition = null)
    {
        return new FumbleConsequence(
            ValidConsequenceId,
            ValidCharacterId,
            ValidSkillId,
            FumbleType.TrustShattered,
            targetId: null,
            appliedAt: DateTime.UtcNow,
            expiresAt: expiresAt,
            description: ValidDescription,
            recoveryCondition: recoveryCondition);
    }
}
