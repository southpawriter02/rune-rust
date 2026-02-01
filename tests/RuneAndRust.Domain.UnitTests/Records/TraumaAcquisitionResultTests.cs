namespace RuneAndRust.Domain.UnitTests.Records;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Records;

/// <summary>
/// Unit tests for <see cref="TraumaAcquisitionResult"/> record.
/// Verifies factory methods and property values.
/// </summary>
[TestFixture]
public class TraumaAcquisitionResultTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CreateNew
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void CreateNew_ReturnsCorrectValues()
    {
        // Arrange & Act
        var result = TraumaAcquisitionResult.CreateNew(
            traumaId: "survivors-guilt",
            traumaName: "Survivor's Guilt",
            source: "AllyDeath",
            triggersRetirementCheck: false
        );

        // Assert
        result.Success.Should().BeTrue();
        result.TraumaId.Should().Be("survivors-guilt");
        result.TraumaName.Should().Be("Survivor's Guilt");
        result.Source.Should().Be("AllyDeath");
        result.IsNewTrauma.Should().BeTrue();
        result.NewStackCount.Should().Be(1);
        result.TriggersRetirementCheck.Should().BeFalse();
        result.Message.Should().Contain("You have acquired Survivor's Guilt");
    }

    [Test]
    public void CreateNew_WithRetirementTrigger_IncludesRetirementMessage()
    {
        // Arrange & Act
        var result = TraumaAcquisitionResult.CreateNew(
            traumaId: "machine-affinity",
            traumaName: "Machine Affinity",
            source: "CorruptionThreshold75",
            triggersRetirementCheck: true
        );

        // Assert
        result.TriggersRetirementCheck.Should().BeTrue();
        result.Message.Should().Contain("[RETIREMENT CHECK REQUIRED]");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CreateStacked
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void CreateStacked_IncrementsStackCount()
    {
        // Arrange & Act
        var result = TraumaAcquisitionResult.CreateStacked(
            traumaId: "reality-doubt",
            traumaName: "Reality Doubt",
            source: "WitnessingHorror",
            newStackCount: 3,
            triggersRetirementCheck: false
        );

        // Assert
        result.Success.Should().BeTrue();
        result.IsNewTrauma.Should().BeFalse();
        result.NewStackCount.Should().Be(3);
        result.Message.Should().Contain("has worsened (x3)");
    }

    [Test]
    public void CreateStacked_AtCriticalThreshold_IncludesCriticalMessage()
    {
        // Arrange & Act (Reality Doubt at 5+ triggers retirement)
        var result = TraumaAcquisitionResult.CreateStacked(
            traumaId: "reality-doubt",
            traumaName: "Reality Doubt",
            source: "WitnessingHorror",
            newStackCount: 5,
            triggersRetirementCheck: true
        );

        // Assert
        result.TriggersRetirementCheck.Should().BeTrue();
        result.Message.Should().Contain("[CRITICAL - RETIREMENT CHECK REQUIRED]");
    }

    [Test]
    public void CreateStacked_WithoutRetirementTrigger_DoesNotIncludeCriticalMessage()
    {
        // Arrange & Act
        var result = TraumaAcquisitionResult.CreateStacked(
            traumaId: "combat-flashbacks",
            traumaName: "Combat Flashbacks",
            source: "NearDeathExperience",
            newStackCount: 2,
            triggersRetirementCheck: false
        );

        // Assert
        result.TriggersRetirementCheck.Should().BeFalse();
        result.Message.Should().NotContain("[CRITICAL");
        result.Message.Should().NotContain("RETIREMENT");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CreateFailure
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void CreateFailure_ReturnsFailureState()
    {
        // Arrange & Act
        var result = TraumaAcquisitionResult.CreateFailure(
            traumaId: "machine-affinity",
            traumaName: "Machine Affinity",
            source: "ForlornContact"
        );

        // Assert
        result.Success.Should().BeFalse();
        result.IsNewTrauma.Should().BeFalse();
        result.NewStackCount.Should().Be(0);
        result.TriggersRetirementCheck.Should().BeFalse();
        result.Message.Should().Contain("already has Machine Affinity");
        result.Message.Should().Contain("cannot stack");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Record Equality
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Records_WithSameValues_AreEqual()
    {
        // Arrange
        var result1 = TraumaAcquisitionResult.CreateNew(
            "trauma-id", "Trauma Name", "Source", false);
        var result2 = TraumaAcquisitionResult.CreateNew(
            "trauma-id", "Trauma Name", "Source", false);

        // Assert
        result1.Should().Be(result2);
    }

    [Test]
    public void Records_WithDifferentValues_AreNotEqual()
    {
        // Arrange
        var result1 = TraumaAcquisitionResult.CreateNew(
            "trauma-1", "Trauma One", "Source", false);
        var result2 = TraumaAcquisitionResult.CreateNew(
            "trauma-2", "Trauma Two", "Source", false);

        // Assert
        result1.Should().NotBe(result2);
    }
}
