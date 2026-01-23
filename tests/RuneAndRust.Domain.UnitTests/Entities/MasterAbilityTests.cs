namespace RuneAndRust.Domain.UnitTests.Entities;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="MasterAbility"/> entity.
/// </summary>
[TestFixture]
public class MasterAbilityTests
{
    // ===== Factory Method Tests =====

    [Test]
    public void Create_WithValidParameters_CreatesAbility()
    {
        // Arrange & Act
        var ability = MasterAbility.Create(
            abilityId: "spider-climb",
            skillId: "acrobatics",
            name: "Spider Climb",
            description: "Climb like a spider",
            abilityType: MasterAbilityType.AutoSucceed,
            effect: MasterAbilityEffect.ForAutoSucceed(12),
            subTypes: new[] { "climbing" },
            isPassive: true,
            triggerMessage: "You climb effortlessly.");

        // Assert
        ability.AbilityId.Should().Be("spider-climb");
        ability.SkillId.Should().Be("acrobatics");
        ability.Name.Should().Be("Spider Climb");
        ability.Description.Should().Be("Climb like a spider");
        ability.AbilityType.Should().Be(MasterAbilityType.AutoSucceed);
        ability.Effect.AutoSucceedDc.Should().Be(12);
        ability.SubTypes.Should().Contain("climbing");
        ability.IsPassive.Should().BeTrue();
        ability.TriggerMessage.Should().Be("You climb effortlessly.");
    }

    [Test]
    public void Create_WithNullAbilityId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => MasterAbility.Create(
            abilityId: "",
            skillId: "acrobatics",
            name: "Test",
            description: "Test",
            abilityType: MasterAbilityType.AutoSucceed,
            effect: MasterAbilityEffect.ForAutoSucceed(10));

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("abilityId");
    }

    [Test]
    public void Create_WithNullSkillId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => MasterAbility.Create(
            abilityId: "test",
            skillId: "",
            name: "Test",
            description: "Test",
            abilityType: MasterAbilityType.AutoSucceed,
            effect: MasterAbilityEffect.ForAutoSucceed(10));

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("skillId");
    }

    // ===== AppliesToSubType Tests =====

    [Test]
    public void AppliesToSubType_WithNoRestrictions_ReturnsTrue()
    {
        // Arrange
        var ability = MasterAbility.Create(
            abilityId: "test",
            skillId: "acrobatics",
            name: "Test",
            description: "Test",
            abilityType: MasterAbilityType.DiceBonus,
            effect: MasterAbilityEffect.ForDiceBonus(2));

        // Act & Assert
        ability.AppliesToSubType("climbing").Should().BeTrue();
        ability.AppliesToSubType(null).Should().BeTrue();
        ability.AppliesToSubType("").Should().BeTrue();
    }

    [Test]
    public void AppliesToSubType_WithMatchingSubType_ReturnsTrue()
    {
        // Arrange
        var ability = MasterAbility.Create(
            abilityId: "test",
            skillId: "acrobatics",
            name: "Test",
            description: "Test",
            abilityType: MasterAbilityType.AutoSucceed,
            effect: MasterAbilityEffect.ForAutoSucceed(12),
            subTypes: new[] { "climbing", "balance" });

        // Act & Assert
        ability.AppliesToSubType("climbing").Should().BeTrue();
        ability.AppliesToSubType("balance").Should().BeTrue();
        ability.AppliesToSubType("CLIMBING").Should().BeTrue(); // Case-insensitive
    }

    [Test]
    public void AppliesToSubType_WithNonMatchingSubType_ReturnsFalse()
    {
        // Arrange
        var ability = MasterAbility.Create(
            abilityId: "test",
            skillId: "acrobatics",
            name: "Test",
            description: "Test",
            abilityType: MasterAbilityType.AutoSucceed,
            effect: MasterAbilityEffect.ForAutoSucceed(12),
            subTypes: new[] { "climbing" });

        // Act & Assert
        ability.AppliesToSubType("stealth").Should().BeFalse();
        ability.AppliesToSubType(null).Should().BeFalse();
    }

    // ===== WouldAutoSucceed Tests =====

    [Test]
    public void WouldAutoSucceed_WhenDCBelowThreshold_ReturnsTrue()
    {
        // Arrange
        var ability = MasterAbility.Create(
            abilityId: "test",
            skillId: "acrobatics",
            name: "Test",
            description: "Test",
            abilityType: MasterAbilityType.AutoSucceed,
            effect: MasterAbilityEffect.ForAutoSucceed(12));

        // Act & Assert
        ability.WouldAutoSucceed(10).Should().BeTrue();
        ability.WouldAutoSucceed(12).Should().BeTrue(); // At threshold
    }

    [Test]
    public void WouldAutoSucceed_WhenDCAboveThreshold_ReturnsFalse()
    {
        // Arrange
        var ability = MasterAbility.Create(
            abilityId: "test",
            skillId: "acrobatics",
            name: "Test",
            description: "Test",
            abilityType: MasterAbilityType.AutoSucceed,
            effect: MasterAbilityEffect.ForAutoSucceed(12));

        // Act & Assert
        ability.WouldAutoSucceed(13).Should().BeFalse();
        ability.WouldAutoSucceed(20).Should().BeFalse();
    }

    [Test]
    public void WouldAutoSucceed_WhenNotAutoSucceedType_ReturnsFalse()
    {
        // Arrange
        var ability = MasterAbility.Create(
            abilityId: "test",
            skillId: "acrobatics",
            name: "Test",
            description: "Test",
            abilityType: MasterAbilityType.DiceBonus,
            effect: MasterAbilityEffect.ForDiceBonus(2));

        // Act & Assert
        ability.WouldAutoSucceed(5).Should().BeFalse();
    }

    // ===== GetDiceBonus Tests =====

    [Test]
    public void GetDiceBonus_WhenDiceBonusType_ReturnsBonus()
    {
        // Arrange
        var ability = MasterAbility.Create(
            abilityId: "test",
            skillId: "acrobatics",
            name: "Test",
            description: "Test",
            abilityType: MasterAbilityType.DiceBonus,
            effect: MasterAbilityEffect.ForDiceBonus(2));

        // Act & Assert
        ability.GetDiceBonus().Should().Be(2);
    }

    [Test]
    public void GetDiceBonus_WhenNotDiceBonusType_ReturnsZero()
    {
        // Arrange
        var ability = MasterAbility.Create(
            abilityId: "test",
            skillId: "acrobatics",
            name: "Test",
            description: "Test",
            abilityType: MasterAbilityType.AutoSucceed,
            effect: MasterAbilityEffect.ForAutoSucceed(12));

        // Act & Assert
        ability.GetDiceBonus().Should().Be(0);
    }
}
