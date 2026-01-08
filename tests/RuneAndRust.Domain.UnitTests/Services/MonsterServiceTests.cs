using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Services;

namespace RuneAndRust.Domain.UnitTests.Services;

/// <summary>
/// Unit tests for MonsterService (v0.0.9a).
/// </summary>
[TestFixture]
public class MonsterServiceTests
{
    private Mock<ILogger<MonsterService>> _loggerMock = null!;
    private List<MonsterDefinition> _testDefinitions = null!;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<MonsterService>>();

        _testDefinitions =
        [
            MonsterDefinition.Create("goblin", "Goblin", "A small creature.", 30, 8, 2, 25, AIBehavior.Cowardly, ["humanoid"], spawnWeight: 100),
            MonsterDefinition.Create("skeleton", "Skeleton", "Animated bones.", 25, 6, 3, 20, AIBehavior.Aggressive, ["undead"], spawnWeight: 80),
            MonsterDefinition.Create("orc", "Orc", "A large brute.", 45, 12, 4, 40, AIBehavior.Aggressive, ["humanoid"], spawnWeight: 50),
            MonsterDefinition.Create("goblin_shaman", "Goblin Shaman", "A healer.", 25, 6, 1, 30, AIBehavior.Support, ["humanoid", "magic"], true, 10, 30),
            MonsterDefinition.Create("slime", "Slime", "A blob.", 40, 5, 5, 15, AIBehavior.Chaotic, ["ooze"], spawnWeight: 90)
        ];
    }

    private MonsterService CreateService(List<MonsterDefinition>? definitions = null)
    {
        var defs = definitions ?? _testDefinitions;
        return new MonsterService(
            () => defs,
            id => defs.FirstOrDefault(d => d.Id.Equals(id, StringComparison.OrdinalIgnoreCase)),
            _loggerMock.Object);
    }

    // ===== SpawnMonster by ID Tests =====

    [Test]
    public void SpawnMonster_WithValidId_ReturnsMonster()
    {
        var service = CreateService();

        var monster = service.SpawnMonster("goblin");

        monster.Should().NotBeNull();
        monster.Name.Should().Be("Goblin");
        monster.MonsterDefinitionId.Should().Be("goblin");
    }

    [Test]
    public void SpawnMonster_IsCaseInsensitive()
    {
        var service = CreateService();

        var monster = service.SpawnMonster("GOBLIN");

        monster.Should().NotBeNull();
        monster.Name.Should().Be("Goblin");
    }

    [Test]
    public void SpawnMonster_WithInvalidId_ThrowsArgumentException()
    {
        var service = CreateService();

        var act = () => service.SpawnMonster("nonexistent");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("definitionId");
    }

    [Test]
    public void SpawnMonster_WithNullId_ThrowsArgumentException()
    {
        var service = CreateService();

        var act = () => service.SpawnMonster(null!);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("definitionId");
    }

    [Test]
    public void SpawnMonster_WithEmptyId_ThrowsArgumentException()
    {
        var service = CreateService();

        var act = () => service.SpawnMonster("");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("definitionId");
    }

    // ===== SpawnRandomMonster Tests =====

    [Test]
    public void SpawnRandomMonster_WithDefinitions_ReturnsMonster()
    {
        var service = CreateService();

        var monster = service.SpawnRandomMonster();

        monster.Should().NotBeNull();
        _testDefinitions.Select(d => d.Name).Should().Contain(monster.Name);
    }

    [Test]
    public void SpawnRandomMonster_WithNoDefinitions_ReturnsFallbackGoblin()
    {
        var service = CreateService([]);

        var monster = service.SpawnRandomMonster();

        monster.Should().NotBeNull();
        monster.Name.Should().Be("Goblin");
    }

    [Test]
    public void SpawnRandomMonster_RespectsWeights()
    {
        // Create definitions with extreme weights
        var definitions = new List<MonsterDefinition>
        {
            MonsterDefinition.Create("common", "Common", "Common.", 30, 8, 2, spawnWeight: 1000),
            MonsterDefinition.Create("rare", "Rare", "Rare.", 50, 12, 4, spawnWeight: 1)
        };
        var service = CreateService(definitions);

        // Spawn many monsters and count occurrences
        var counts = new Dictionary<string, int> { ["Common"] = 0, ["Rare"] = 0 };
        for (int i = 0; i < 100; i++)
        {
            var monster = service.SpawnRandomMonster();
            counts[monster.Name]++;
        }

        // Common should appear significantly more often
        counts["Common"].Should().BeGreaterThan(counts["Rare"]);
    }

    // ===== SpawnRandomMonster with Tags Tests =====

    [Test]
    public void SpawnRandomMonster_WithTags_ReturnsMatchingMonster()
    {
        var service = CreateService();

        var monster = service.SpawnRandomMonster(["undead"]);

        monster.Should().NotBeNull();
        monster.Name.Should().Be("Skeleton");
    }

    [Test]
    public void SpawnRandomMonster_WithMultipleTags_ReturnsMonsterWithAllTags()
    {
        var service = CreateService();

        var monster = service.SpawnRandomMonster(["humanoid", "magic"]);

        monster.Should().NotBeNull();
        monster.Name.Should().Be("Goblin Shaman");
    }

    [Test]
    public void SpawnRandomMonster_WithNoMatchingTags_ThrowsInvalidOperationException()
    {
        var service = CreateService();

        var act = () => service.SpawnRandomMonster(["dragon"]);

        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void SpawnRandomMonster_WithNullTags_ReturnsAnyMonster()
    {
        var service = CreateService();

        var monster = service.SpawnRandomMonster(null!);

        monster.Should().NotBeNull();
    }

    [Test]
    public void SpawnRandomMonster_WithEmptyTags_ReturnsAnyMonster()
    {
        var service = CreateService();

        var monster = service.SpawnRandomMonster([]);

        monster.Should().NotBeNull();
    }

    // ===== GetAllDefinitions Tests =====

    [Test]
    public void GetAllDefinitions_ReturnsAllDefinitions()
    {
        var service = CreateService();

        var definitions = service.GetAllDefinitions();

        definitions.Should().HaveCount(5);
        definitions.Select(d => d.Id).Should().BeEquivalentTo(
            ["goblin", "skeleton", "orc", "goblin_shaman", "slime"]);
    }

    [Test]
    public void GetAllDefinitions_WithNoDefinitions_ReturnsEmptyList()
    {
        var service = CreateService([]);

        var definitions = service.GetAllDefinitions();

        definitions.Should().BeEmpty();
    }

    // ===== GetDefinition Tests =====

    [Test]
    public void GetDefinition_WithValidId_ReturnsDefinition()
    {
        var service = CreateService();

        var definition = service.GetDefinition("orc");

        definition.Should().NotBeNull();
        definition!.Name.Should().Be("Orc");
    }

    [Test]
    public void GetDefinition_WithInvalidId_ReturnsNull()
    {
        var service = CreateService();

        var definition = service.GetDefinition("nonexistent");

        definition.Should().BeNull();
    }

    [Test]
    public void GetDefinition_WithNullId_ReturnsNull()
    {
        var service = CreateService();

        var definition = service.GetDefinition(null!);

        definition.Should().BeNull();
    }

    [Test]
    public void GetDefinition_IsCaseInsensitive()
    {
        var service = CreateService();

        var definition = service.GetDefinition("ORC");

        definition.Should().NotBeNull();
        definition!.Name.Should().Be("Orc");
    }

    // ===== Monster Properties Tests =====

    [Test]
    public void SpawnMonster_SetsCorrectBehavior()
    {
        var service = CreateService();

        var goblin = service.SpawnMonster("goblin");
        var orc = service.SpawnMonster("orc");
        var shaman = service.SpawnMonster("goblin_shaman");

        goblin.Behavior.Should().Be(AIBehavior.Cowardly);
        orc.Behavior.Should().Be(AIBehavior.Aggressive);
        shaman.Behavior.Should().Be(AIBehavior.Support);
    }

    [Test]
    public void SpawnMonster_SetsCorrectHealing()
    {
        var service = CreateService();

        var shaman = service.SpawnMonster("goblin_shaman");
        var goblin = service.SpawnMonster("goblin");

        shaman.CanHeal.Should().BeTrue();
        shaman.HealAmount.Should().Be(10);
        goblin.CanHeal.Should().BeFalse();
    }

    [Test]
    public void SpawnMonster_SetsCorrectExperienceValue()
    {
        var service = CreateService();

        var goblin = service.SpawnMonster("goblin");
        var orc = service.SpawnMonster("orc");

        goblin.ExperienceValue.Should().Be(25);
        orc.ExperienceValue.Should().Be(40);
    }
}
