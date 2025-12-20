using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models.Combat;
using RuneAndRust.Engine.Services;
using Xunit;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;
using CharacterEntity = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for the ResourceService class.
/// Validates resource management: spending, regeneration, and Overcast mechanic.
/// </summary>
public class ResourceServiceTests
{
    private readonly Mock<ILogger<ResourceService>> _mockLogger;
    private readonly ResourceService _sut;

    public ResourceServiceTests()
    {
        _mockLogger = new Mock<ILogger<ResourceService>>();
        _sut = new ResourceService(_mockLogger.Object);
    }

    #region CanAfford Tests

    [Fact]
    public void CanAfford_ZeroCost_ReturnsTrue()
    {
        // Arrange
        var combatant = CreatePlayerCombatant();

        // Act & Assert
        _sut.CanAfford(combatant, ResourceType.Stamina, 0).Should().BeTrue();
        _sut.CanAfford(combatant, ResourceType.Aether, 0).Should().BeTrue();
        _sut.CanAfford(combatant, ResourceType.Health, 0).Should().BeTrue();
    }

    [Fact]
    public void CanAfford_NegativeCost_ReturnsTrue()
    {
        // Arrange
        var combatant = CreatePlayerCombatant();

        // Act & Assert
        _sut.CanAfford(combatant, ResourceType.Stamina, -5).Should().BeTrue();
    }

    [Fact]
    public void CanAfford_Stamina_SufficientStamina_ReturnsTrue()
    {
        // Arrange
        var combatant = CreatePlayerCombatant();
        combatant.CurrentStamina = 50;

        // Act
        var result = _sut.CanAfford(combatant, ResourceType.Stamina, 25);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanAfford_Stamina_InsufficientStamina_ReturnsFalse()
    {
        // Arrange
        var combatant = CreatePlayerCombatant();
        combatant.CurrentStamina = 10;

        // Act
        var result = _sut.CanAfford(combatant, ResourceType.Stamina, 25);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanAfford_Health_SufficientHp_ReturnsTrue()
    {
        // Arrange
        var combatant = CreatePlayerCombatant();
        combatant.CurrentHp = 50;

        // Act
        var result = _sut.CanAfford(combatant, ResourceType.Health, 25);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanAfford_Health_InsufficientHp_ReturnsFalse()
    {
        // Arrange
        var combatant = CreatePlayerCombatant();
        combatant.CurrentHp = 10;

        // Act
        var result = _sut.CanAfford(combatant, ResourceType.Health, 25);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanAfford_Aether_NonMystic_ReturnsFalse()
    {
        // Arrange
        var combatant = CreatePlayerCombatant(ArchetypeType.Warrior);
        combatant.CurrentAp = 50;
        combatant.MaxAp = 50;

        // Act
        var result = _sut.CanAfford(combatant, ResourceType.Aether, 10);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanAfford_Aether_Mystic_SufficientAp_ReturnsTrue()
    {
        // Arrange
        var combatant = CreatePlayerCombatant(ArchetypeType.Mystic);
        combatant.CurrentAp = 50;
        combatant.MaxAp = 50;

        // Act
        var result = _sut.CanAfford(combatant, ResourceType.Aether, 25);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanAfford_Aether_Mystic_InsufficientAp_CanOvercast_ReturnsTrue()
    {
        // Arrange
        var combatant = CreatePlayerCombatant(ArchetypeType.Mystic);
        combatant.CurrentAp = 10;
        combatant.MaxAp = 50;
        combatant.CurrentHp = 100; // Needs (25-10)*2 = 30 HP to Overcast

        // Act
        var result = _sut.CanAfford(combatant, ResourceType.Aether, 25);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanAfford_Aether_Mystic_InsufficientAp_CannotOvercast_ReturnsFalse()
    {
        // Arrange
        var combatant = CreatePlayerCombatant(ArchetypeType.Mystic);
        combatant.CurrentAp = 10;
        combatant.MaxAp = 50;
        combatant.CurrentHp = 25; // Needs (25-10)*2 = 30 HP to Overcast, only has 25

        // Act
        var result = _sut.CanAfford(combatant, ResourceType.Aether, 25);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanAfford_Aether_Mystic_ZeroAp_CanOvercast_ReturnsTrue()
    {
        // Arrange
        var combatant = CreatePlayerCombatant(ArchetypeType.Mystic);
        combatant.CurrentAp = 0;
        combatant.MaxAp = 50;
        combatant.CurrentHp = 100; // Needs 25*2 = 50 HP to Overcast

        // Act
        var result = _sut.CanAfford(combatant, ResourceType.Aether, 25);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Deduct Tests

    [Fact]
    public void Deduct_ZeroCost_ReturnsTrue()
    {
        // Arrange
        var combatant = CreatePlayerCombatant();
        combatant.CurrentStamina = 50;

        // Act
        var result = _sut.Deduct(combatant, ResourceType.Stamina, 0);

        // Assert
        result.Should().BeTrue();
        combatant.CurrentStamina.Should().Be(50); // Unchanged
    }

    [Fact]
    public void Deduct_Stamina_DeductsCorrectAmount()
    {
        // Arrange
        var combatant = CreatePlayerCombatant();
        combatant.CurrentStamina = 50;

        // Act
        var result = _sut.Deduct(combatant, ResourceType.Stamina, 25);

        // Assert
        result.Should().BeTrue();
        combatant.CurrentStamina.Should().Be(25);
    }

    [Fact]
    public void Deduct_Stamina_InsufficientStamina_ReturnsFalse()
    {
        // Arrange
        var combatant = CreatePlayerCombatant();
        combatant.CurrentStamina = 10;

        // Act
        var result = _sut.Deduct(combatant, ResourceType.Stamina, 25);

        // Assert
        result.Should().BeFalse();
        combatant.CurrentStamina.Should().Be(10); // Unchanged
    }

    [Fact]
    public void Deduct_Health_DeductsCorrectAmount()
    {
        // Arrange
        var combatant = CreatePlayerCombatant();
        combatant.CurrentHp = 100;

        // Act
        var result = _sut.Deduct(combatant, ResourceType.Health, 30);

        // Assert
        result.Should().BeTrue();
        combatant.CurrentHp.Should().Be(70);
    }

    [Fact]
    public void Deduct_Aether_NormalDeduction()
    {
        // Arrange
        var combatant = CreatePlayerCombatant(ArchetypeType.Mystic);
        combatant.CurrentAp = 50;
        combatant.MaxAp = 50;
        combatant.CurrentHp = 100;

        // Act
        var result = _sut.Deduct(combatant, ResourceType.Aether, 20);

        // Assert
        result.Should().BeTrue();
        combatant.CurrentAp.Should().Be(30);
        combatant.CurrentHp.Should().Be(100); // HP unchanged
    }

    [Fact]
    public void Deduct_Aether_Overcast_SpendsHpAt2To1Ratio()
    {
        // Arrange
        var combatant = CreatePlayerCombatant(ArchetypeType.Mystic);
        combatant.CurrentAp = 10;
        combatant.MaxAp = 50;
        combatant.CurrentHp = 100;

        // Act - cost 25, have 10 AP, need 15 more = 30 HP
        var result = _sut.Deduct(combatant, ResourceType.Aether, 25);

        // Assert
        result.Should().BeTrue();
        combatant.CurrentAp.Should().Be(0); // All AP spent
        combatant.CurrentHp.Should().Be(70); // 100 - 30 HP
    }

    [Fact]
    public void Deduct_Aether_Overcast_FullHpCost()
    {
        // Arrange
        var combatant = CreatePlayerCombatant(ArchetypeType.Mystic);
        combatant.CurrentAp = 0;
        combatant.MaxAp = 50;
        combatant.CurrentHp = 100;

        // Act - cost 25, have 0 AP, need 25 = 50 HP
        var result = _sut.Deduct(combatant, ResourceType.Aether, 25);

        // Assert
        result.Should().BeTrue();
        combatant.CurrentAp.Should().Be(0);
        combatant.CurrentHp.Should().Be(50); // 100 - 50 HP
    }

    [Fact]
    public void Deduct_Aether_NonMystic_ReturnsFalse()
    {
        // Arrange
        var combatant = CreatePlayerCombatant(ArchetypeType.Warrior);
        combatant.CurrentAp = 50;

        // Act
        var result = _sut.Deduct(combatant, ResourceType.Aether, 10);

        // Assert
        result.Should().BeFalse();
        combatant.CurrentAp.Should().Be(50); // Unchanged
    }

    [Fact]
    public void Deduct_SyncsToCharacterSource()
    {
        // Arrange
        var character = new CharacterEntity
        {
            Name = "Test",
            Archetype = ArchetypeType.Warrior,
            CurrentStamina = 50,
            MaxStamina = 60
        };
        var combatant = Combatant.FromCharacter(character);

        // Act
        _sut.Deduct(combatant, ResourceType.Stamina, 20);

        // Assert
        character.CurrentStamina.Should().Be(30);
    }

    #endregion

    #region RegenerateStamina Tests

    [Fact]
    public void RegenerateStamina_AppliesBaseRegen()
    {
        // Arrange
        var combatant = CreatePlayerCombatant();
        combatant.CurrentStamina = 30;
        combatant.MaxStamina = 60;

        // Act - Base 5 + (5 Finesse / 2) = 7
        var result = _sut.RegenerateStamina(combatant);

        // Assert
        result.Should().Be(7);
        combatant.CurrentStamina.Should().Be(37);
    }

    [Fact]
    public void RegenerateStamina_ClampsToMaxStamina()
    {
        // Arrange
        var combatant = CreatePlayerCombatant();
        combatant.CurrentStamina = 55;
        combatant.MaxStamina = 60;

        // Act - Would regen 7, but clamped to 5 (55 + 5 = 60)
        var result = _sut.RegenerateStamina(combatant);

        // Assert
        result.Should().Be(5);
        combatant.CurrentStamina.Should().Be(60);
    }

    [Fact]
    public void RegenerateStamina_AtMaxStamina_ReturnsZero()
    {
        // Arrange
        var combatant = CreatePlayerCombatant();
        combatant.CurrentStamina = 60;
        combatant.MaxStamina = 60;

        // Act
        var result = _sut.RegenerateStamina(combatant);

        // Assert
        result.Should().Be(0);
        combatant.CurrentStamina.Should().Be(60);
    }

    [Fact]
    public void RegenerateStamina_Stunned_ReturnsZero()
    {
        // Arrange
        var combatant = CreatePlayerCombatant();
        combatant.CurrentStamina = 30;
        combatant.MaxStamina = 60;
        combatant.StatusEffects.Add(new ActiveStatusEffect
        {
            Type = StatusEffectType.Stunned,
            DurationRemaining = 1
        });

        // Act
        var result = _sut.RegenerateStamina(combatant);

        // Assert
        result.Should().Be(0);
        combatant.CurrentStamina.Should().Be(30); // Unchanged
    }

    [Fact]
    public void RegenerateStamina_HighFinesse_IncreaseRegen()
    {
        // Arrange - Use character source with high Finesse
        var character = new CharacterEntity
        {
            Name = "Test",
            Finesse = 10,
            CurrentStamina = 30,
            MaxStamina = 80
        };
        var combatant = Combatant.FromCharacter(character);

        // Act - Base 5 + (10 / 2) = 10
        var result = _sut.RegenerateStamina(combatant);

        // Assert
        result.Should().Be(10);
        combatant.CurrentStamina.Should().Be(40);
    }

    [Fact]
    public void RegenerateStamina_SyncsToCharacterSource()
    {
        // Arrange
        var character = new CharacterEntity
        {
            Name = "Test",
            Finesse = 6,
            CurrentStamina = 30,
            MaxStamina = 60
        };
        var combatant = Combatant.FromCharacter(character);

        // Act
        _sut.RegenerateStamina(combatant);

        // Assert - Base 5 + (6 / 2) = 8
        character.CurrentStamina.Should().Be(38);
    }

    #endregion

    #region GetCurrent / GetMax Tests

    [Fact]
    public void GetCurrent_ReturnsCorrectValues()
    {
        // Arrange
        var combatant = CreatePlayerCombatant();
        combatant.CurrentHp = 75;
        combatant.CurrentStamina = 40;
        combatant.CurrentAp = 20;

        // Act & Assert
        _sut.GetCurrent(combatant, ResourceType.Health).Should().Be(75);
        _sut.GetCurrent(combatant, ResourceType.Stamina).Should().Be(40);
        _sut.GetCurrent(combatant, ResourceType.Aether).Should().Be(20);
    }

    [Fact]
    public void GetMax_ReturnsCorrectValues()
    {
        // Arrange
        var combatant = CreatePlayerCombatant();
        combatant.MaxHp = 100;
        combatant.MaxStamina = 60;
        combatant.MaxAp = 50;

        // Act & Assert
        _sut.GetMax(combatant, ResourceType.Health).Should().Be(100);
        _sut.GetMax(combatant, ResourceType.Stamina).Should().Be(60);
        _sut.GetMax(combatant, ResourceType.Aether).Should().Be(50);
    }

    #endregion

    #region IsMystic Tests

    [Fact]
    public void IsMystic_MysticArchetype_ReturnsTrue()
    {
        // Arrange
        var combatant = CreatePlayerCombatant(ArchetypeType.Mystic);

        // Act
        var result = _sut.IsMystic(combatant);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsMystic_WarriorArchetype_ReturnsFalse()
    {
        // Arrange
        var combatant = CreatePlayerCombatant(ArchetypeType.Warrior);

        // Act
        var result = _sut.IsMystic(combatant);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsMystic_EnemyCombatant_ReturnsFalse()
    {
        // Arrange
        var enemy = new Enemy
        {
            Name = "Test Enemy",
            MaxHp = 50,
            CurrentHp = 50,
            MaxStamina = 30,
            CurrentStamina = 30
        };
        var combatant = Combatant.FromEnemy(enemy);

        // Act
        var result = _sut.IsMystic(combatant);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Deduct_Aether_CanAffordButJustBarely_Succeeds()
    {
        // Arrange - Exactly enough HP to Overcast
        var combatant = CreatePlayerCombatant(ArchetypeType.Mystic);
        combatant.CurrentAp = 0;
        combatant.MaxAp = 50;
        combatant.CurrentHp = 51; // Need 50 HP, have 51 (must survive)

        // Act
        var result = _sut.Deduct(combatant, ResourceType.Aether, 25);

        // Assert
        result.Should().BeTrue();
        combatant.CurrentHp.Should().Be(1);
    }

    [Fact]
    public void Deduct_Aether_WouldDieFromOvercast_Fails()
    {
        // Arrange - Not enough HP to survive Overcast
        var combatant = CreatePlayerCombatant(ArchetypeType.Mystic);
        combatant.CurrentAp = 0;
        combatant.MaxAp = 50;
        combatant.CurrentHp = 50; // Need 50 HP, have exactly 50 (would die)

        // Act
        var result = _sut.CanAfford(combatant, ResourceType.Aether, 25);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Deduct_Stamina_EnemyCombatant_DoesNotSyncToSource()
    {
        // Arrange
        var enemy = new Enemy
        {
            Name = "Test Enemy",
            MaxStamina = 40,
            CurrentStamina = 40
        };
        var combatant = Combatant.FromEnemy(enemy);

        // Act - Should not throw even without CharacterSource
        var result = _sut.Deduct(combatant, ResourceType.Stamina, 10);

        // Assert
        result.Should().BeTrue();
        combatant.CurrentStamina.Should().Be(30);
    }

    #endregion

    #region Helper Methods

    private static Combatant CreatePlayerCombatant(ArchetypeType archetype = ArchetypeType.Warrior)
    {
        var character = new CharacterEntity
        {
            Name = "Test Player",
            Archetype = archetype,
            Finesse = 5, // Default for regen tests
            CurrentHP = 100,
            MaxHP = 100,
            CurrentStamina = 60,
            MaxStamina = 60,
            CurrentAp = 0,
            MaxAp = archetype == ArchetypeType.Mystic ? 50 : 0
        };

        return Combatant.FromCharacter(character);
    }

    #endregion
}
