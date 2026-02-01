namespace RuneAndRust.Domain.UnitTests.Records;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Records;

/// <summary>
/// Unit tests for <see cref="RetirementCheckResult"/> record.
/// Verifies factory methods and property values.
/// </summary>
[TestFixture]
public class RetirementCheckResultTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Test Data
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly Guid _characterId = Guid.NewGuid();

    // ═══════════════════════════════════════════════════════════════════════════
    // CreateMustRetire
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void CreateMustRetire_SetsMandatoryRetirement()
    {
        // Arrange & Act
        var result = RetirementCheckResult.CreateMustRetire(
            characterId: _characterId,
            retirementReason: "Machine Affinity forces immediate retirement",
            traumasCausingRetirement: new[] { "machine-affinity" }
        );

        // Assert
        result.CharacterId.Should().Be(_characterId);
        result.MustRetire.Should().BeTrue();
        result.RetirementReason.Should().Be("Machine Affinity forces immediate retirement");
        result.TraumasCausingRetirement.Should().ContainSingle("machine-affinity");
        result.TotalRetirementTraumas.Should().Be(1);
        result.CanContinueWithPermission.Should().BeFalse();
    }

    [Test]
    public void CreateMustRetire_WithMultipleTraumas_IncludesAllTraumas()
    {
        // Arrange
        var traumas = new[] { "reality-doubt", "hollow-resonance" };

        // Act
        var result = RetirementCheckResult.CreateMustRetire(
            characterId: _characterId,
            retirementReason: "Critical trauma accumulation",
            traumasCausingRetirement: traumas
        );

        // Assert
        result.TraumasCausingRetirement.Should().HaveCount(2);
        result.TraumasCausingRetirement.Should().Contain("reality-doubt");
        result.TraumasCausingRetirement.Should().Contain("hollow-resonance");
        result.TotalRetirementTraumas.Should().Be(2);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CreateOptional
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void CreateOptional_AllowsContinuation()
    {
        // Arrange
        var traumas = new[] { "combat-flashbacks", "night-terrors", "paranoid-ideation" };

        // Act
        var result = RetirementCheckResult.CreateOptional(
            characterId: _characterId,
            traumasCausingRetirement: traumas
        );

        // Assert
        result.CharacterId.Should().Be(_characterId);
        result.MustRetire.Should().BeFalse();
        result.RetirementReason.Should().BeNull();
        result.CanContinueWithPermission.Should().BeTrue();
        result.TotalRetirementTraumas.Should().Be(3);
    }

    [Test]
    public void CreateOptional_TraumasListPopulated()
    {
        // Arrange
        var traumas = new[] { "trauma-a", "trauma-b", "trauma-c", "trauma-d" };

        // Act
        var result = RetirementCheckResult.CreateOptional(_characterId, traumas);

        // Assert
        result.TraumasCausingRetirement.Should().HaveCount(4);
        result.TraumasCausingRetirement.Should().ContainInOrder("trauma-a", "trauma-b", "trauma-c", "trauma-d");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CreateNoRetirement
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void CreateNoRetirement_ReturnsNoRetirementState()
    {
        // Arrange & Act
        var result = RetirementCheckResult.CreateNoRetirement(_characterId);

        // Assert
        result.CharacterId.Should().Be(_characterId);
        result.MustRetire.Should().BeFalse();
        result.RetirementReason.Should().BeNull();
        result.TraumasCausingRetirement.Should().BeEmpty();
        result.TotalRetirementTraumas.Should().Be(0);
        result.CanContinueWithPermission.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Record Equality
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void CreateNoRetirement_WithSameCharacterId_AreEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var result1 = RetirementCheckResult.CreateNoRetirement(id);
        var result2 = RetirementCheckResult.CreateNoRetirement(id);

        // Assert
        result1.CharacterId.Should().Be(result2.CharacterId);
        result1.MustRetire.Should().Be(result2.MustRetire);
        result1.TotalRetirementTraumas.Should().Be(result2.TotalRetirementTraumas);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Edge Cases
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void CreateMustRetire_WithEmptyTraumaList_HasZeroTotal()
    {
        // Arrange & Act
        var result = RetirementCheckResult.CreateMustRetire(
            characterId: _characterId,
            retirementReason: "Test reason",
            traumasCausingRetirement: new List<string>()
        );

        // Assert
        result.TotalRetirementTraumas.Should().Be(0);
        result.TraumasCausingRetirement.Should().BeEmpty();
    }
}
