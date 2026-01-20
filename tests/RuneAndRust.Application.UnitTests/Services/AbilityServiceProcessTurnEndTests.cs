using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

[TestFixture]
public class AbilityServiceProcessTurnEndTests
{
    private Mock<IGameConfigurationProvider> _mockConfig = null!;
    private Mock<ILogger<AbilityService>> _mockLogger = null!;
    private Mock<ILogger<ResourceService>> _mockResourceLogger = null!;
    private ResourceService _resourceService = null!;
    private AbilityService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _mockConfig = new Mock<IGameConfigurationProvider>();
        _mockLogger = new Mock<ILogger<AbilityService>>();
        _mockResourceLogger = new Mock<ILogger<ResourceService>>();

        var testAbilities = new List<AbilityDefinition>
        {
            AbilityDefinition.Create(
                "flame-bolt", "Flame Bolt", "Fire damage",
                ["test-class"],
                AbilityCost.Create("mana", 10),
                cooldown: 2,
                [AbilityEffect.Damage(20)],
                Domain.Enums.AbilityTargetType.SingleEnemy)
        };

        _mockConfig.Setup(c => c.GetAbilities()).Returns(testAbilities);
        _mockConfig.Setup(c => c.GetAbilityById("flame-bolt")).Returns(testAbilities[0]);
        _mockConfig.Setup(c => c.GetResourceTypes()).Returns(new List<ResourceTypeDefinition>());

        _resourceService = new ResourceService(_mockConfig.Object, _mockResourceLogger.Object);
        _service = new AbilityService(_mockConfig.Object, _resourceService, _mockLogger.Object);
    }

    [Test]
    public void ProcessTurnEnd_ReturnsEmptyListWhenNoCooldowns()
    {
        // Arrange
        var player = CreateTestPlayer();
        // No abilities added = no cooldowns

        // Act
        var result = _service.ProcessTurnEnd(player);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void ProcessTurnEnd_ReducesCooldowns_ReturnsList()
    {
        // Arrange
        var player = CreateTestPlayer();
        var ability = PlayerAbility.Create("flame-bolt", isUnlocked: true);
        ability.Use(cooldownDuration: 3);
        player.AddAbility(ability);

        // Act
        var result = _service.ProcessTurnEnd(player);

        // Assert
        result.Should().HaveCount(1);
        result[0].AbilityName.Should().Be("Flame Bolt");
        result[0].PreviousCooldown.Should().Be(3);
        result[0].NewCooldown.Should().Be(2);
        result[0].IsNowReady.Should().BeFalse();
    }

    [Test]
    public void ProcessTurnEnd_ReportsAbilityReady()
    {
        // Arrange
        var player = CreateTestPlayer();
        var ability = PlayerAbility.Create("flame-bolt", isUnlocked: true);
        ability.Use(cooldownDuration: 1); // Will become ready after one tick
        player.AddAbility(ability);

        // Act
        var result = _service.ProcessTurnEnd(player);

        // Assert
        result.Should().HaveCount(1);
        result[0].IsNowReady.Should().BeTrue();
        result[0].NewCooldown.Should().Be(0);
    }

    [Test]
    public void ProcessTurnEnd_DoesNotReduceBelowZero()
    {
        // Arrange
        var player = CreateTestPlayer();
        var ability = PlayerAbility.Create("flame-bolt", isUnlocked: true);
        // Don't set cooldown - it's already at 0
        player.AddAbility(ability);

        // Act
        var result = _service.ProcessTurnEnd(player);

        // Assert
        result.Should().BeEmpty(); // Nothing to reduce, so no changes
        player.GetAbility("flame-bolt")!.CurrentCooldown.Should().Be(0);
    }

    [Test]
    public void ProcessTurnEnd_SkipsReadyAbilities()
    {
        // Arrange
        var player = CreateTestPlayer();
        var readyAbility = PlayerAbility.Create("flame-bolt", isUnlocked: true);
        // Don't use it - it's ready with no cooldown
        player.AddAbility(readyAbility);

        // Act
        var result = _service.ProcessTurnEnd(player);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void ProcessTurnEnd_HandlesMultipleAbilities()
    {
        // Arrange
        var player = CreateTestPlayer();

        var ability1 = PlayerAbility.Create("flame-bolt", isUnlocked: true);
        ability1.Use(cooldownDuration: 2);
        player.AddAbility(ability1);

        // Add a second ability that's not on cooldown
        var ability2 = PlayerAbility.Create("frost-nova", isUnlocked: true);
        player.AddAbility(ability2);

        // Act
        var result = _service.ProcessTurnEnd(player);

        // Assert
        result.Should().HaveCount(1); // Only the one on cooldown
        result[0].AbilityName.Should().Be("Flame Bolt");
    }

    private static Player CreateTestPlayer()
    {
        return new Player("TestPlayer", new Stats(100, 10, 5));
    }
}
