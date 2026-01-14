using FluentAssertions;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Definitions;

/// <summary>
/// Unit tests for AbilityDefinition range properties (v0.5.1a).
/// </summary>
[TestFixture]
public class AbilityRangeTests
{
    private static AbilityDefinition CreateAbility(int range = 1, RangeType rangeType = RangeType.Melee) =>
        AbilityDefinition.Create(
            id: "test-ability",
            name: "Test Ability",
            description: "A test ability",
            classIds: ["warrior"],
            cost: AbilityCost.None,
            cooldown: 0,
            effects: [],
            targetType: AbilityTargetType.Enemy,
            unlockLevel: 1);

    [Test]
    public void AbilityDefinition_DefaultRange_Is1()
    {
        // Arrange & Act
        var ability = CreateAbility();

        // Assert
        ability.Range.Should().Be(1);
        ability.RangeType.Should().Be(RangeType.Melee);
    }

    [Test]
    public void GetEffectiveRange_MeleeType_Returns1()
    {
        // Arrange
        var ability = new AbilityDefinition
        {
            Id = "test",
            Name = "Test",
            Range = 10,
            RangeType = RangeType.Melee
        };

        // Act
        var effectiveRange = ability.GetEffectiveRange();

        // Assert
        effectiveRange.Should().Be(1);
    }

    [Test]
    public void GetEffectiveRange_ReachType_Returns2()
    {
        // Arrange
        var ability = new AbilityDefinition
        {
            Id = "test",
            Name = "Test",
            Range = 10,
            RangeType = RangeType.Reach
        };

        // Act
        var effectiveRange = ability.GetEffectiveRange();

        // Assert
        effectiveRange.Should().Be(2);
    }

    [Test]
    public void GetEffectiveRange_RangedType_ReturnsConfiguredRange()
    {
        // Arrange
        var ability = new AbilityDefinition
        {
            Id = "test",
            Name = "Test",
            Range = 8,
            RangeType = RangeType.Ranged
        };

        // Act
        var effectiveRange = ability.GetEffectiveRange();

        // Assert
        effectiveRange.Should().Be(8);
    }

    [TestCase(RangeType.Melee, 1, true)]
    [TestCase(RangeType.Melee, 2, false)]
    [TestCase(RangeType.Reach, 2, true)]
    [TestCase(RangeType.Reach, 3, false)]
    public void IsInRange_VariousDistances_ReturnsExpected(RangeType rangeType, int distance, bool expected)
    {
        // Arrange
        var ability = new AbilityDefinition
        {
            Id = "test",
            Name = "Test",
            Range = 5,
            RangeType = rangeType
        };

        // Act
        var isInRange = ability.IsInRange(distance);

        // Assert
        isInRange.Should().Be(expected);
    }
}
