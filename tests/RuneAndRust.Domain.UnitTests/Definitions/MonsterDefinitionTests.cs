using FluentAssertions;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Definitions;

/// <summary>
/// Unit tests for MonsterDefinition (v0.0.9a).
/// </summary>
[TestFixture]
public class MonsterDefinitionTests
{
    // ===== Factory Method Validation Tests =====

    [Test]
    public void Create_WithValidParameters_ReturnsDefinition()
    {
        var definition = MonsterDefinition.Create(
            id: "test_monster",
            name: "Test Monster",
            description: "A test monster.",
            baseHealth: 50,
            baseAttack: 10,
            baseDefense: 5,
            experienceValue: 25);

        definition.Should().NotBeNull();
        definition.Id.Should().Be("test_monster");
        definition.Name.Should().Be("Test Monster");
        definition.Description.Should().Be("A test monster.");
        definition.BaseHealth.Should().Be(50);
        definition.BaseAttack.Should().Be(10);
        definition.BaseDefense.Should().Be(5);
        definition.ExperienceValue.Should().Be(25);
    }

    [Test]
    public void Create_WithNullId_ThrowsArgumentException()
    {
        var act = () => MonsterDefinition.Create(
            id: null!,
            name: "Test",
            description: "Test",
            baseHealth: 50,
            baseAttack: 10,
            baseDefense: 5);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("id");
    }

    [Test]
    public void Create_WithEmptyId_ThrowsArgumentException()
    {
        var act = () => MonsterDefinition.Create(
            id: "",
            name: "Test",
            description: "Test",
            baseHealth: 50,
            baseAttack: 10,
            baseDefense: 5);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("id");
    }

    [Test]
    public void Create_WithNullName_ThrowsArgumentException()
    {
        var act = () => MonsterDefinition.Create(
            id: "test",
            name: null!,
            description: "Test",
            baseHealth: 50,
            baseAttack: 10,
            baseDefense: 5);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Test]
    public void Create_WithNullDescription_ThrowsArgumentException()
    {
        var act = () => MonsterDefinition.Create(
            id: "test",
            name: "Test",
            description: null!,
            baseHealth: 50,
            baseAttack: 10,
            baseDefense: 5);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("description");
    }

    [Test]
    public void Create_WithZeroHealth_ThrowsArgumentOutOfRangeException()
    {
        var act = () => MonsterDefinition.Create(
            id: "test",
            name: "Test",
            description: "Test",
            baseHealth: 0,
            baseAttack: 10,
            baseDefense: 5);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("baseHealth");
    }

    [Test]
    public void Create_WithNegativeHealth_ThrowsArgumentOutOfRangeException()
    {
        var act = () => MonsterDefinition.Create(
            id: "test",
            name: "Test",
            description: "Test",
            baseHealth: -10,
            baseAttack: 10,
            baseDefense: 5);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("baseHealth");
    }

    // ===== Default Value Tests =====

    [Test]
    public void Create_WithDefaults_AppliesDefaultValues()
    {
        var definition = MonsterDefinition.Create(
            id: "test",
            name: "Test",
            description: "Test",
            baseHealth: 50,
            baseAttack: 10,
            baseDefense: 5);

        definition.Behavior.Should().Be(AIBehavior.Aggressive);
        definition.Tags.Should().BeEmpty();
        definition.CanHeal.Should().BeFalse();
        definition.HealAmount.Should().BeNull();
        definition.SpawnWeight.Should().Be(100);
        definition.InitiativeModifier.Should().Be(0);
        definition.ExperienceValue.Should().Be(0);
    }

    [Test]
    public void Create_WithNegativeAttack_ClampsToZero()
    {
        var definition = MonsterDefinition.Create(
            id: "test",
            name: "Test",
            description: "Test",
            baseHealth: 50,
            baseAttack: -5,
            baseDefense: 5);

        definition.BaseAttack.Should().Be(0);
    }

    [Test]
    public void Create_WithNegativeDefense_ClampsToZero()
    {
        var definition = MonsterDefinition.Create(
            id: "test",
            name: "Test",
            description: "Test",
            baseHealth: 50,
            baseAttack: 10,
            baseDefense: -5);

        definition.BaseDefense.Should().Be(0);
    }

    [Test]
    public void Create_WithNegativeExperience_ClampsToZero()
    {
        var definition = MonsterDefinition.Create(
            id: "test",
            name: "Test",
            description: "Test",
            baseHealth: 50,
            baseAttack: 10,
            baseDefense: 5,
            experienceValue: -100);

        definition.ExperienceValue.Should().Be(0);
    }

    [Test]
    public void Create_WithZeroSpawnWeight_ClampsToOne()
    {
        var definition = MonsterDefinition.Create(
            id: "test",
            name: "Test",
            description: "Test",
            baseHealth: 50,
            baseAttack: 10,
            baseDefense: 5,
            spawnWeight: 0);

        definition.SpawnWeight.Should().Be(1);
    }

    // ===== CreateMonster Tests =====

    [Test]
    public void CreateMonster_ReturnsMonsterWithCorrectStats()
    {
        var definition = MonsterDefinition.Create(
            id: "goblin",
            name: "Goblin",
            description: "A small creature.",
            baseHealth: 30,
            baseAttack: 8,
            baseDefense: 2,
            experienceValue: 25,
            behavior: AIBehavior.Cowardly,
            initiativeModifier: 1);

        var monster = definition.CreateMonster();

        monster.Name.Should().Be("Goblin");
        monster.Description.Should().Be("A small creature.");
        monster.MaxHealth.Should().Be(30);
        monster.Health.Should().Be(30);
        monster.Stats.Attack.Should().Be(8);
        monster.Stats.Defense.Should().Be(2);
        monster.ExperienceValue.Should().Be(25);
        monster.Behavior.Should().Be(AIBehavior.Cowardly);
        monster.MonsterDefinitionId.Should().Be("goblin");
        monster.InitiativeModifier.Should().Be(1);
    }

    [Test]
    public void CreateMonster_WithHealing_EnablesHealing()
    {
        var definition = MonsterDefinition.Create(
            id: "shaman",
            name: "Shaman",
            description: "A healer.",
            baseHealth: 25,
            baseAttack: 6,
            baseDefense: 1,
            canHeal: true,
            healAmount: 10);

        var monster = definition.CreateMonster();

        monster.CanHeal.Should().BeTrue();
        monster.HealAmount.Should().Be(10);
    }

    [Test]
    public void CreateMonster_WithoutHealing_DoesNotEnableHealing()
    {
        var definition = MonsterDefinition.Create(
            id: "warrior",
            name: "Warrior",
            description: "A fighter.",
            baseHealth: 50,
            baseAttack: 12,
            baseDefense: 4,
            canHeal: false);

        var monster = definition.CreateMonster();

        monster.CanHeal.Should().BeFalse();
        monster.HealAmount.Should().BeNull();
    }

    [Test]
    public void CreateMonster_GeneratesUniqueIds()
    {
        var definition = MonsterDefinition.Create(
            id: "goblin",
            name: "Goblin",
            description: "Test",
            baseHealth: 30,
            baseAttack: 8,
            baseDefense: 2);

        var monster1 = definition.CreateMonster();
        var monster2 = definition.CreateMonster();

        monster1.Id.Should().NotBe(monster2.Id);
    }

    // ===== Tag Tests =====

    [Test]
    public void HasTag_WithMatchingTag_ReturnsTrue()
    {
        var definition = MonsterDefinition.Create(
            id: "goblin",
            name: "Goblin",
            description: "Test",
            baseHealth: 30,
            baseAttack: 8,
            baseDefense: 2,
            tags: ["humanoid", "small"]);

        definition.HasTag("humanoid").Should().BeTrue();
        definition.HasTag("small").Should().BeTrue();
    }

    [Test]
    public void HasTag_WithNonMatchingTag_ReturnsFalse()
    {
        var definition = MonsterDefinition.Create(
            id: "goblin",
            name: "Goblin",
            description: "Test",
            baseHealth: 30,
            baseAttack: 8,
            baseDefense: 2,
            tags: ["humanoid"]);

        definition.HasTag("undead").Should().BeFalse();
    }

    [Test]
    public void HasTag_IsCaseInsensitive()
    {
        var definition = MonsterDefinition.Create(
            id: "goblin",
            name: "Goblin",
            description: "Test",
            baseHealth: 30,
            baseAttack: 8,
            baseDefense: 2,
            tags: ["humanoid"]);

        definition.HasTag("HUMANOID").Should().BeTrue();
        definition.HasTag("Humanoid").Should().BeTrue();
    }

    [Test]
    public void HasTag_WithNullOrEmpty_ReturnsFalse()
    {
        var definition = MonsterDefinition.Create(
            id: "goblin",
            name: "Goblin",
            description: "Test",
            baseHealth: 30,
            baseAttack: 8,
            baseDefense: 2,
            tags: ["humanoid"]);

        definition.HasTag(null!).Should().BeFalse();
        definition.HasTag("").Should().BeFalse();
        definition.HasTag("  ").Should().BeFalse();
    }

    [Test]
    public void HasAllTags_WithAllTagsPresent_ReturnsTrue()
    {
        var definition = MonsterDefinition.Create(
            id: "shaman",
            name: "Shaman",
            description: "Test",
            baseHealth: 25,
            baseAttack: 6,
            baseDefense: 1,
            tags: ["humanoid", "magic", "goblin"]);

        definition.HasAllTags(["humanoid", "magic"]).Should().BeTrue();
    }

    [Test]
    public void HasAllTags_WithMissingTag_ReturnsFalse()
    {
        var definition = MonsterDefinition.Create(
            id: "goblin",
            name: "Goblin",
            description: "Test",
            baseHealth: 30,
            baseAttack: 8,
            baseDefense: 2,
            tags: ["humanoid"]);

        definition.HasAllTags(["humanoid", "magic"]).Should().BeFalse();
    }

    [Test]
    public void HasAllTags_WithNullTags_ReturnsTrue()
    {
        var definition = MonsterDefinition.Create(
            id: "goblin",
            name: "Goblin",
            description: "Test",
            baseHealth: 30,
            baseAttack: 8,
            baseDefense: 2,
            tags: ["humanoid"]);

        definition.HasAllTags(null!).Should().BeTrue();
    }

    [Test]
    public void HasAnyTag_WithOneMatchingTag_ReturnsTrue()
    {
        var definition = MonsterDefinition.Create(
            id: "goblin",
            name: "Goblin",
            description: "Test",
            baseHealth: 30,
            baseAttack: 8,
            baseDefense: 2,
            tags: ["humanoid"]);

        definition.HasAnyTag(["undead", "humanoid"]).Should().BeTrue();
    }

    [Test]
    public void HasAnyTag_WithNoMatchingTags_ReturnsFalse()
    {
        var definition = MonsterDefinition.Create(
            id: "goblin",
            name: "Goblin",
            description: "Test",
            baseHealth: 30,
            baseAttack: 8,
            baseDefense: 2,
            tags: ["humanoid"]);

        definition.HasAnyTag(["undead", "beast"]).Should().BeFalse();
    }
}
