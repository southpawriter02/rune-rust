using FluentAssertions;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.TestUtilities.Mocks;

namespace RuneAndRust.Application.UnitTests.TestUtilities;

/// <summary>
/// Tests for the mock configuration provider and repository.
/// </summary>
[TestFixture]
public class MockProviderTests
{
    [Test]
    public void MockConfigurationProvider_WithClass_ReturnsClass()
    {
        // Arrange
        var classDef = ClassDefinition.Create(
            id: "warrior",
            name: "Warrior",
            description: "A skilled fighter",
            archetypeId: "martial",
            statModifiers: new StatModifiers { MaxHealth = 20 },
            growthRates: StatModifiers.None,
            primaryResourceId: "rage",
            startingAbilityIds: ["power_strike"]);

        var provider = new MockConfigurationProvider()
            .WithClass(classDef);

        // Act
        var result = provider.GetClassById("warrior");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("warrior");
        result.Name.Should().Be("Warrior");
    }

    [Test]
    public void MockConfigurationProvider_WithAbility_ReturnsAbility()
    {
        // Arrange
        var ability = AbilityDefinition.Create(
            id: "power_strike",
            name: "Power Strike",
            description: "A powerful melee attack",
            classIds: ["warrior"],
            cost: AbilityCost.Create("rage", 25),
            cooldown: 3,
            effects: [AbilityEffect.Damage(15)],
            targetType: AbilityTargetType.SingleEnemy);

        var provider = new MockConfigurationProvider()
            .WithAbility(ability);

        // Act
        var result = provider.GetAbilityById("power_strike");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("power_strike");
        result.Name.Should().Be("Power Strike");
    }

    [Test]
    public void MockConfigurationProvider_GetClassById_FindsClass()
    {
        // Arrange
        var warrior = ClassDefinition.Create(
            "warrior", "Warrior", "Fighter", "martial",
            StatModifiers.None, StatModifiers.None, "rage");
        var mage = ClassDefinition.Create(
            "mage", "Mage", "Spellcaster", "arcane",
            StatModifiers.None, StatModifiers.None, "mana");

        var provider = new MockConfigurationProvider()
            .WithClass(warrior)
            .WithClass(mage);

        // Act
        var result = provider.GetClassById("mage");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("mage");
        result.Name.Should().Be("Mage");
    }

    [Test]
    public void MockConfigurationProvider_WithDefaultResourceTypes_AddsAll()
    {
        // Arrange
        var provider = new MockConfigurationProvider()
            .WithDefaultResourceTypes();

        // Act
        var resources = provider.GetResourceTypes();

        // Assert
        resources.Should().HaveCount(4);
        provider.GetResourceTypeById("mana").Should().NotBeNull();
        provider.GetResourceTypeById("rage").Should().NotBeNull();
        provider.GetResourceTypeById("faith").Should().NotBeNull();
        provider.GetResourceTypeById("stamina").Should().NotBeNull();
    }

    [Test]
    public void MockConfigurationProvider_GetAbilitiesForClass_FiltersCorrectly()
    {
        // Arrange
        var warriorAbility = AbilityDefinition.Create(
            "power_strike", "Power Strike", "Attack",
            ["warrior"],
            AbilityCost.Create("rage", 25),
            3,
            [AbilityEffect.Damage(15)],
            AbilityTargetType.SingleEnemy);
        var mageAbility = AbilityDefinition.Create(
            "fireball", "Fireball", "Fire spell",
            ["mage"],
            AbilityCost.Create("mana", 30),
            5,
            [AbilityEffect.Damage(20)],
            AbilityTargetType.SingleEnemy);
        var sharedAbility = AbilityDefinition.Create(
            "sprint", "Sprint", "Move fast",
            ["warrior", "mage"],
            AbilityCost.Create("stamina", 10),
            10,
            [AbilityEffect.Buff(new StatModifiers { Finesse = 2 }, 3)],
            AbilityTargetType.Self);

        var provider = new MockConfigurationProvider()
            .WithAbility(warriorAbility)
            .WithAbility(mageAbility)
            .WithAbility(sharedAbility);

        // Act
        var warriorAbilities = provider.GetAbilitiesForClass("warrior");
        var mageAbilities = provider.GetAbilitiesForClass("mage");

        // Assert
        warriorAbilities.Should().HaveCount(2);
        warriorAbilities.Select(a => a.Id).Should().Contain("power_strike");
        warriorAbilities.Select(a => a.Id).Should().Contain("sprint");

        mageAbilities.Should().HaveCount(2);
        mageAbilities.Select(a => a.Id).Should().Contain("fireball");
        mageAbilities.Select(a => a.Id).Should().Contain("sprint");
    }

    [Test]
    public async Task MockRepository_SaveAsync_StoresSession()
    {
        // Arrange
        var repository = new MockRepository();
        var session = GameSession.CreateNew("TestPlayer");

        // Act
        var id = await repository.SaveAsync(session);

        // Assert
        repository.Count.Should().Be(1);
        var loaded = await repository.GetByIdAsync(id);
        loaded.Should().NotBeNull();
        loaded!.Player.Name.Should().Be("TestPlayer");
    }

    [Test]
    public async Task MockRepository_GetByIdAsync_ReturnsNullForMissing()
    {
        // Arrange
        var repository = new MockRepository();

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task MockRepository_DeleteAsync_RemovesSession()
    {
        // Arrange
        var repository = new MockRepository();
        var session = GameSession.CreateNew("TestPlayer");
        await repository.SaveAsync(session);

        // Act
        await repository.DeleteAsync(session.Id);

        // Assert
        repository.Count.Should().Be(0);
        var loaded = await repository.GetByIdAsync(session.Id);
        loaded.Should().BeNull();
    }

    [Test]
    public async Task MockRepository_GetSavedGamesAsync_ReturnsSummaries()
    {
        // Arrange
        var repository = new MockRepository();
        var session1 = GameSession.CreateNew("Player1");
        var session2 = GameSession.CreateNew("Player2");
        await repository.SaveAsync(session1);
        await repository.SaveAsync(session2);

        // Act
        var summaries = await repository.GetSavedGamesAsync();

        // Assert
        summaries.Should().HaveCount(2);
        summaries.Select(s => s.PlayerName).Should().Contain("Player1");
        summaries.Select(s => s.PlayerName).Should().Contain("Player2");
    }
}
