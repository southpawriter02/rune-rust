using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

[TestFixture]
public class ResourceServiceTests
{
    private ResourceService _service = null!;
    private Mock<IGameConfigurationProvider> _mockConfig = null!;
    private Mock<ILogger<ResourceService>> _mockLogger = null!;

    [SetUp]
    public void SetUp()
    {
        _mockConfig = new Mock<IGameConfigurationProvider>();
        _mockLogger = new Mock<ILogger<ResourceService>>();

        var resourceTypes = new List<ResourceTypeDefinition>
        {
            ResourceTypeDefinition.Create("health", "Vitality", "HP", "Life force", "#FF0000", 100, isUniversal: true),
            ResourceTypeDefinition.Create("mana", "Arcane Power", "MP", "Magic", "#0066FF", 100, regenPerTurn: 10),
            ResourceTypeDefinition.Create("rage", "Fury", "RG", "Combat", "#FF6600", 100,
                decayPerTurn: 10, buildOnDamageDealt: 10, buildOnDamageTaken: 15, startsAtZero: true),
            ResourceTypeDefinition.Create("faith", "Divine Favor", "FTH", "Support", "#FFDD88", 100,
                regenPerTurn: 5, buildOnHeal: 15)
        };

        _mockConfig.Setup(c => c.GetResourceTypes()).Returns(resourceTypes);
        _mockConfig.Setup(c => c.GetResourceTypeById(It.IsAny<string>()))
            .Returns((string id) => resourceTypes.FirstOrDefault(r =>
                r.Id.Equals(id, StringComparison.OrdinalIgnoreCase)));

        _service = new ResourceService(_mockConfig.Object, _mockLogger.Object);
    }

    [Test]
    public void GetAllResourceTypes_ReturnsAllTypes()
    {
        var result = _service.GetAllResourceTypes();

        Assert.That(result.Count, Is.EqualTo(4));
    }

    [Test]
    public void InitializePlayerResources_InitializesUniversalResources()
    {
        var player = Player.Create("Hero", "human", "soldier", new PlayerAttributes(), new Stats(100, 10, 5));
        var classDef = ClassDefinition.Create("mage", "Mage", "Desc", "mystic",
            StatModifiers.None, StatModifiers.None, "mana");

        _service.InitializePlayerResources(player, classDef);

        Assert.That(player.HasResource("health"), Is.True);
        Assert.That(player.HasResource("mana"), Is.True);
    }

    [Test]
    public void SpendResource_ReturnsTrueWhenSufficient()
    {
        var player = Player.Create("Hero", "human", "soldier", new PlayerAttributes(), new Stats(100, 10, 5));
        player.InitializeResource("mana", 100);

        var result = _service.SpendResource(player, "mana", 30);

        Assert.That(result, Is.True);
        Assert.That(player.GetResource("mana")!.Current, Is.EqualTo(70));
    }

    [Test]
    public void SpendResource_ReturnsFalseWhenInsufficient()
    {
        var player = Player.Create("Hero", "human", "soldier", new PlayerAttributes(), new Stats(100, 10, 5));
        player.InitializeResource("mana", 100);
        player.GetResource("mana")!.Spend(80);

        var result = _service.SpendResource(player, "mana", 30);

        Assert.That(result, Is.False);
    }

    [Test]
    public void GainResource_IncreasesResourceCurrent()
    {
        var player = Player.Create("Hero", "human", "soldier", new PlayerAttributes(), new Stats(100, 10, 5));
        player.InitializeResource("mana", 100);
        player.GetResource("mana")!.Spend(50);

        var gained = _service.GainResource(player, "mana", 30);

        Assert.That(gained, Is.EqualTo(30));
        Assert.That(player.GetResource("mana")!.Current, Is.EqualTo(80));
    }

    [Test]
    public void ProcessTurnEnd_AppliesRegeneration()
    {
        var player = Player.Create("Hero", "human", "soldier", new PlayerAttributes(), new Stats(100, 10, 5));
        player.InitializeResource("mana", 100);
        player.GetResource("mana")!.Spend(30);

        var result = _service.ProcessTurnEnd(player);

        Assert.That(result.HasChanges, Is.True);
        Assert.That(player.GetResource("mana")!.Current, Is.EqualTo(80)); // +10 regen
    }

    [Test]
    public void ProcessTurnEnd_AppliesDecayOutOfCombat()
    {
        var player = Player.Create("Hero", "human", "soldier", new PlayerAttributes(), new Stats(100, 10, 5));
        player.InitializeResource("rage", 100, startAtZero: false);
        player.GetResource("rage")!.SetCurrent(50);

        var result = _service.ProcessTurnEnd(player, inCombat: false);

        Assert.That(result.HasChanges, Is.True);
        Assert.That(player.GetResource("rage")!.Current, Is.EqualTo(40)); // -10 decay
    }

    [Test]
    public void ProcessTurnEnd_SkipsDecayInCombat()
    {
        var player = Player.Create("Hero", "human", "soldier", new PlayerAttributes(), new Stats(100, 10, 5));
        player.InitializeResource("rage", 100, startAtZero: false);
        player.GetResource("rage")!.SetCurrent(50);

        var result = _service.ProcessTurnEnd(player, inCombat: true);

        Assert.That(player.GetResource("rage")!.Current, Is.EqualTo(50)); // No decay
    }

    [Test]
    public void ProcessCombatHit_BuildsResourceOnDamage()
    {
        var player = Player.Create("Hero", "human", "soldier", new PlayerAttributes(), new Stats(100, 10, 5));
        player.InitializeResource("rage", 100, startAtZero: true);

        var result = _service.ProcessCombatHit(player, damageDealt: 20, damageTaken: 10);

        Assert.That(result.HasChanges, Is.True);
        Assert.That(player.GetResource("rage")!.Current, Is.EqualTo(25)); // +10 dealt + 15 taken
    }

    [Test]
    public void ProcessSupportAction_BuildsResourceOnHeal()
    {
        var player = Player.Create("Hero", "human", "soldier", new PlayerAttributes(), new Stats(100, 10, 5));
        player.InitializeResource("faith", 100);
        player.GetResource("faith")!.Spend(50);

        var result = _service.ProcessSupportAction(player, healAmount: 25);

        Assert.That(result.HasChanges, Is.True);
        Assert.That(player.GetResource("faith")!.Current, Is.EqualTo(65)); // +15 from heal
    }
}
