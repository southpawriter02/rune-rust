namespace RuneAndRust.Domain.UnitTests.Records;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Records;

/// <summary>
/// Unit tests for <see cref="TraumaCheckResult"/> record.
/// Verifies factory methods, properties, and ToString behavior.
/// </summary>
[TestFixture]
public class TraumaCheckResultTests
{
    private Guid _characterId;

    [SetUp]
    public void Setup()
    {
        _characterId = Guid.NewGuid();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CreatePassed Factory Method
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void CreatePassed_ReturnsCorrectValues()
    {
        // Arrange & Act
        var result = TraumaCheckResult.CreatePassed(
            characterId: _characterId,
            trigger: TraumaCheckTrigger.AllyDeath,
            diceRolled: 3,
            successesNeeded: 2,
            successesAchieved: 2,
            modifiers: new[] { "Strong Will" },
            resolvePenalty: 0
        );

        // Assert
        result.CharacterId.Should().Be(_characterId);
        result.Trigger.Should().Be(TraumaCheckTrigger.AllyDeath);
        result.DiceRolled.Should().Be(3);
        result.SuccessesNeeded.Should().Be(2);
        result.SuccessesAchieved.Should().Be(2);
        result.Passed.Should().BeTrue();
        result.TraumaAcquired.Should().BeNull();
        result.Modifiers.Should().ContainSingle().Which.Should().Be("Strong Will");
        result.ResolvePenaltyApplied.Should().Be(0);
    }

    [Test]
    public void CreatePassed_WithNullModifiers_UsesEmptyList()
    {
        // Arrange & Act
        var result = TraumaCheckResult.CreatePassed(
            characterId: _characterId,
            trigger: TraumaCheckTrigger.AllyDeath,
            diceRolled: 3,
            successesNeeded: 2,
            successesAchieved: 2
        );

        // Assert
        result.Modifiers.Should().BeEmpty();
    }

    [Test]
    public void CreatePassed_WithResolvePenalty_IncludesPenalty()
    {
        // Arrange & Act
        var result = TraumaCheckResult.CreatePassed(
            characterId: _characterId,
            trigger: TraumaCheckTrigger.WitnessingHorror,
            diceRolled: 2,
            successesNeeded: 3,
            successesAchieved: 3,
            resolvePenalty: 2
        );

        // Assert
        result.ResolvePenaltyApplied.Should().Be(2);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CreateFailed Factory Method
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void CreateFailed_ReturnsCorrectValues()
    {
        // Arrange & Act
        var result = TraumaCheckResult.CreateFailed(
            characterId: _characterId,
            trigger: TraumaCheckTrigger.AllyDeath,
            diceRolled: 2,
            successesNeeded: 2,
            successesAchieved: 0,
            traumaAcquired: "survivors-guilt",
            modifiers: new[] { "Corrupted Mind" },
            resolvePenalty: 1
        );

        // Assert
        result.CharacterId.Should().Be(_characterId);
        result.Trigger.Should().Be(TraumaCheckTrigger.AllyDeath);
        result.DiceRolled.Should().Be(2);
        result.SuccessesNeeded.Should().Be(2);
        result.SuccessesAchieved.Should().Be(0);
        result.Passed.Should().BeFalse();
        result.TraumaAcquired.Should().Be("survivors-guilt");
        result.Modifiers.Should().ContainSingle().Which.Should().Be("Corrupted Mind");
        result.ResolvePenaltyApplied.Should().Be(1);
    }

    [Test]
    public void CreateFailed_WithNullModifiers_UsesEmptyList()
    {
        // Arrange & Act
        var result = TraumaCheckResult.CreateFailed(
            characterId: _characterId,
            trigger: TraumaCheckTrigger.CorruptionThreshold100,
            diceRolled: 1,
            successesNeeded: 4,
            successesAchieved: 0,
            traumaAcquired: "machine-affinity"
        );

        // Assert
        result.Modifiers.Should().BeEmpty();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ToString Override
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void ToString_PassedResult_ContainsPassedText()
    {
        // Arrange
        var result = TraumaCheckResult.CreatePassed(
            characterId: _characterId,
            trigger: TraumaCheckTrigger.AllyDeath,
            diceRolled: 3,
            successesNeeded: 2,
            successesAchieved: 2
        );

        // Act
        var text = result.ToString();

        // Assert
        text.Should().Contain("AllyDeath");
        text.Should().Contain("3d[RESOLVE]");
        text.Should().Contain("2/2");
        text.Should().Contain("PASSED");
    }

    [Test]
    public void ToString_FailedResult_ContainsTraumaName()
    {
        // Arrange
        var result = TraumaCheckResult.CreateFailed(
            characterId: _characterId,
            trigger: TraumaCheckTrigger.WitnessingHorror,
            diceRolled: 2,
            successesNeeded: 3,
            successesAchieved: 1,
            traumaAcquired: "combat-flashbacks"
        );

        // Act
        var text = result.ToString();

        // Assert
        text.Should().Contain("WitnessingHorror");
        text.Should().Contain("1/3");
        text.Should().Contain("FAILED");
        text.Should().Contain("combat-flashbacks");
    }

    [Test]
    public void ToString_WithCorruptionPenalty_ContainsPenaltyInfo()
    {
        // Arrange
        var result = TraumaCheckResult.CreateFailed(
            characterId: _characterId,
            trigger: TraumaCheckTrigger.CorruptionThreshold100,
            diceRolled: 1,
            successesNeeded: 4,
            successesAchieved: 0,
            traumaAcquired: "hollow-resonance",
            resolvePenalty: 3
        );

        // Act
        var text = result.ToString();

        // Assert
        text.Should().Contain("[-3 corruption]");
    }

    [Test]
    public void ToString_WithoutCorruptionPenalty_DoesNotContainPenaltyInfo()
    {
        // Arrange
        var result = TraumaCheckResult.CreatePassed(
            characterId: _characterId,
            trigger: TraumaCheckTrigger.CriticalFailure,
            diceRolled: 3,
            successesNeeded: 1,
            successesAchieved: 1,
            resolvePenalty: 0
        );

        // Act
        var text = result.ToString();

        // Assert
        text.Should().NotContain("corruption");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Record Equality
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Records_WithSameValues_AreEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var result1 = TraumaCheckResult.CreatePassed(
            characterId: id,
            trigger: TraumaCheckTrigger.AllyDeath,
            diceRolled: 3,
            successesNeeded: 2,
            successesAchieved: 2
        );
        var result2 = TraumaCheckResult.CreatePassed(
            characterId: id,
            trigger: TraumaCheckTrigger.AllyDeath,
            diceRolled: 3,
            successesNeeded: 2,
            successesAchieved: 2
        );

        // Assert - Use BeEquivalentTo for records with collection properties
        // (IReadOnlyList uses reference equality in record comparison)
        result1.Should().BeEquivalentTo(result2);
    }

    [Test]
    public void Records_WithDifferentTriggers_AreNotEqual()
    {
        // Arrange
        var result1 = TraumaCheckResult.CreatePassed(
            characterId: _characterId,
            trigger: TraumaCheckTrigger.AllyDeath,
            diceRolled: 3,
            successesNeeded: 2,
            successesAchieved: 2
        );
        var result2 = TraumaCheckResult.CreatePassed(
            characterId: _characterId,
            trigger: TraumaCheckTrigger.WitnessingHorror,
            diceRolled: 3,
            successesNeeded: 3,
            successesAchieved: 3
        );

        // Assert
        result1.Should().NotBe(result2);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // All Trigger Types
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    [TestCase(TraumaCheckTrigger.StressThreshold100)]
    [TestCase(TraumaCheckTrigger.CorruptionThreshold100)]
    [TestCase(TraumaCheckTrigger.AllyDeath)]
    [TestCase(TraumaCheckTrigger.NearDeathExperience)]
    [TestCase(TraumaCheckTrigger.CriticalFailure)]
    [TestCase(TraumaCheckTrigger.ProlongedExposure)]
    [TestCase(TraumaCheckTrigger.WitnessingHorror)]
    [TestCase(TraumaCheckTrigger.RuinMadnessEscape)]
    public void CreatePassed_AcceptsAllTriggerTypes(TraumaCheckTrigger trigger)
    {
        // Arrange & Act
        var result = TraumaCheckResult.CreatePassed(
            characterId: _characterId,
            trigger: trigger,
            diceRolled: 3,
            successesNeeded: 2,
            successesAchieved: 2
        );

        // Assert
        result.Trigger.Should().Be(trigger);
        result.Passed.Should().BeTrue();
    }

    [Test]
    [TestCase(TraumaCheckTrigger.StressThreshold100)]
    [TestCase(TraumaCheckTrigger.CorruptionThreshold100)]
    [TestCase(TraumaCheckTrigger.AllyDeath)]
    [TestCase(TraumaCheckTrigger.NearDeathExperience)]
    [TestCase(TraumaCheckTrigger.CriticalFailure)]
    [TestCase(TraumaCheckTrigger.ProlongedExposure)]
    [TestCase(TraumaCheckTrigger.WitnessingHorror)]
    [TestCase(TraumaCheckTrigger.RuinMadnessEscape)]
    public void CreateFailed_AcceptsAllTriggerTypes(TraumaCheckTrigger trigger)
    {
        // Arrange & Act
        var result = TraumaCheckResult.CreateFailed(
            characterId: _characterId,
            trigger: trigger,
            diceRolled: 1,
            successesNeeded: 2,
            successesAchieved: 0,
            traumaAcquired: "test-trauma"
        );

        // Assert
        result.Trigger.Should().Be(trigger);
        result.Passed.Should().BeFalse();
    }
}
