using FluentAssertions;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Definitions;

[TestFixture]
public class AbilityDefinitionTests
{
    [Test]
    public void Create_WithValidParameters_ReturnsDefinition()
    {
        // Arrange & Act
        var ability = AbilityDefinition.Create(
            "flame-bolt",
            "Flame Bolt",
            "Hurl a bolt of fire at your enemy.",
            ["galdr-caster"],
            AbilityCost.Create("mana", 15),
            cooldown: 0,
            [AbilityEffect.Damage(20)],
            AbilityTargetType.SingleEnemy
        );

        // Assert
        ability.Should().NotBeNull();
        ability.Id.Should().Be("flame-bolt");
        ability.Name.Should().Be("Flame Bolt");
        ability.Description.Should().Be("Hurl a bolt of fire at your enemy.");
        ability.ClassIds.Should().ContainSingle().Which.Should().Be("galdr-caster");
        ability.Cost.ResourceTypeId.Should().Be("mana");
        ability.Cost.Amount.Should().Be(15);
        ability.Cooldown.Should().Be(0);
        ability.TargetType.Should().Be(AbilityTargetType.SingleEnemy);
    }

    [Test]
    public void Create_NormalizesIdToLowercase()
    {
        // Arrange & Act
        var ability = AbilityDefinition.Create(
            "FLAME-BOLT",
            "Flame Bolt",
            "Description",
            ["GALDR-CASTER"],
            AbilityCost.None,
            cooldown: 0,
            [],
            AbilityTargetType.SingleEnemy
        );

        // Assert
        ability.Id.Should().Be("flame-bolt");
        ability.ClassIds.Should().ContainSingle().Which.Should().Be("galdr-caster");
    }

    [Test]
    public void Create_WithNullId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => AbilityDefinition.Create(
            null!,
            "Name",
            "Description",
            [],
            AbilityCost.None,
            cooldown: 0,
            [],
            AbilityTargetType.Self
        );

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void IsAvailableToClass_WithMatchingClass_ReturnsTrue()
    {
        // Arrange
        var ability = AbilityDefinition.Create(
            "shield-bash",
            "Shield Bash",
            "Description",
            ["shieldmaiden", "berserker"],
            AbilityCost.None,
            cooldown: 0,
            [],
            AbilityTargetType.SingleEnemy
        );

        // Act & Assert
        ability.IsAvailableToClass("shieldmaiden").Should().BeTrue();
        ability.IsAvailableToClass("SHIELDMAIDEN").Should().BeTrue();
        ability.IsAvailableToClass("berserker").Should().BeTrue();
    }

    [Test]
    public void IsAvailableToClass_WithNonMatchingClass_ReturnsFalse()
    {
        // Arrange
        var ability = AbilityDefinition.Create(
            "shield-bash",
            "Shield Bash",
            "Description",
            ["shieldmaiden"],
            AbilityCost.None,
            cooldown: 0,
            [],
            AbilityTargetType.SingleEnemy
        );

        // Act & Assert
        ability.IsAvailableToClass("galdr-caster").Should().BeFalse();
    }

    [Test]
    public void HasCost_WithZeroCost_ReturnsFalse()
    {
        // Arrange
        var ability = AbilityDefinition.Create(
            "auto-attack",
            "Auto Attack",
            "Description",
            [],
            AbilityCost.None,
            cooldown: 0,
            [],
            AbilityTargetType.SingleEnemy
        );

        // Assert
        ability.HasCost.Should().BeFalse();
    }

    [Test]
    public void HasCooldown_WithPositiveCooldown_ReturnsTrue()
    {
        // Arrange
        var ability = AbilityDefinition.Create(
            "frost-nova",
            "Frost Nova",
            "Description",
            [],
            AbilityCost.None,
            cooldown: 4,
            [],
            AbilityTargetType.AllEnemies
        );

        // Assert
        ability.HasCooldown.Should().BeTrue();
        ability.Cooldown.Should().Be(4);
    }

    [Test]
    public void IsInstant_WithZeroCooldown_ReturnsTrue()
    {
        // Arrange
        var ability = AbilityDefinition.Create(
            "flame-bolt",
            "Flame Bolt",
            "Description",
            [],
            AbilityCost.Create("mana", 15),
            cooldown: 0,
            [],
            AbilityTargetType.SingleEnemy
        );

        // Assert
        ability.IsInstant.Should().BeTrue();
        ability.HasCooldown.Should().BeFalse();
    }
}
