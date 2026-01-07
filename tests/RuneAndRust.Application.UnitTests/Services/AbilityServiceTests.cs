using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

[TestFixture]
public class AbilityServiceTests
{
    private Mock<IGameConfigurationProvider> _mockConfig = null!;
    private Mock<ILogger<AbilityService>> _mockLogger = null!;
    private Mock<ILogger<ResourceService>> _mockResourceLogger = null!;
    private ResourceService _resourceService = null!;
    private AbilityService _service = null!;
    private List<AbilityDefinition> _testAbilities = null!;

    [SetUp]
    public void SetUp()
    {
        _mockConfig = new Mock<IGameConfigurationProvider>();
        _mockLogger = new Mock<ILogger<AbilityService>>();
        _mockResourceLogger = new Mock<ILogger<ResourceService>>();

        _testAbilities =
        [
            AbilityDefinition.Create(
                "flame-bolt", "Flame Bolt", "Hurl a bolt of fire.",
                ["galdr-caster"],
                AbilityCost.Create("mana", 15),
                cooldown: 0,
                [AbilityEffect.Damage(20, "will", 0.6f)],
                AbilityTargetType.SingleEnemy
            ),
            AbilityDefinition.Create(
                "frost-nova", "Frost Nova", "Unleash a wave of frost.",
                ["galdr-caster"],
                AbilityCost.Create("mana", 25),
                cooldown: 4,
                [AbilityEffect.Damage(12)],
                AbilityTargetType.AllEnemies
            ),
            AbilityDefinition.Create(
                "healing-word", "Healing Word", "Restore health.",
                ["blood-priest"],
                AbilityCost.Create("faith", 20),
                cooldown: 2,
                [AbilityEffect.Heal(25, "will", 0.6f)],
                AbilityTargetType.Self
            )
        ];

        _mockConfig.Setup(c => c.GetAbilities()).Returns(_testAbilities);
        _mockConfig.Setup(c => c.GetAbilityById("flame-bolt")).Returns(_testAbilities[0]);
        _mockConfig.Setup(c => c.GetAbilityById("frost-nova")).Returns(_testAbilities[1]);
        _mockConfig.Setup(c => c.GetAbilityById("healing-word")).Returns(_testAbilities[2]);
        _mockConfig.Setup(c => c.GetAbilitiesForClass("galdr-caster"))
            .Returns(_testAbilities.Where(a => a.IsAvailableToClass("galdr-caster")).ToList());

        var resourceTypes = new List<ResourceTypeDefinition>
        {
            ResourceTypeDefinition.Create("mana", "Mana", "MP", "#0000FF", "Magic points", 100),
            ResourceTypeDefinition.Create("faith", "Faith", "FP", "#FFD700", "Holy power", 50)
        };
        _mockConfig.Setup(c => c.GetResourceTypes()).Returns(resourceTypes);
        _mockConfig.Setup(c => c.GetResourceTypeById("mana")).Returns(resourceTypes[0]);
        _mockConfig.Setup(c => c.GetResourceTypeById("faith")).Returns(resourceTypes[1]);

        _resourceService = new ResourceService(_mockConfig.Object, _mockResourceLogger.Object);
        _service = new AbilityService(_mockConfig.Object, _resourceService, _mockLogger.Object);
    }

    [Test]
    public void GetAbilityDefinition_WithValidId_ReturnsDefinition()
    {
        // Act
        var result = _service.GetAbilityDefinition("flame-bolt");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Flame Bolt");
    }

    [Test]
    public void GetAbilityDefinition_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = _service.GetAbilityDefinition("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void GetAbilitiesForClass_ReturnsMatchingAbilities()
    {
        // Act
        var result = _service.GetAbilitiesForClass("galdr-caster");

        // Assert
        result.Should().HaveCount(2);
        result.Select(a => a.Id).Should().Contain("flame-bolt");
        result.Select(a => a.Id).Should().Contain("frost-nova");
    }

    [Test]
    public void CanUseAbility_WithValidState_ReturnsSuccess()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.InitializeResource("mana", 100);
        var playerAbility = PlayerAbility.Create("flame-bolt", isUnlocked: true);
        player.AddAbility(playerAbility);

        // Act
        var result = _service.CanUseAbility(player, "flame-bolt");

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void CanUseAbility_WithMissingAbility_ReturnsNotLearned()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        var result = _service.CanUseAbility(player, "flame-bolt");

        // Assert
        result.IsValid.Should().BeFalse();
        result.FailureReason.Should().Contain("haven't learned");
    }

    [Test]
    public void CanUseAbility_OnCooldown_ReturnsOnCooldown()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.InitializeResource("mana", 100);
        var playerAbility = PlayerAbility.Create("frost-nova", isUnlocked: true);
        playerAbility.Use(cooldownDuration: 3);
        player.AddAbility(playerAbility);

        // Act
        var result = _service.CanUseAbility(player, "frost-nova");

        // Assert
        result.IsValid.Should().BeFalse();
        result.FailureReason.Should().Contain("cooldown");
    }

    [Test]
    public void CanUseAbility_InsufficientResource_ReturnsInsufficientResource()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.InitializeResource("mana", 10); // Less than required 15
        var playerAbility = PlayerAbility.Create("flame-bolt", isUnlocked: true);
        player.AddAbility(playerAbility);

        // Act
        var result = _service.CanUseAbility(player, "flame-bolt");

        // Assert
        result.IsValid.Should().BeFalse();
        result.FailureReason.Should().Contain("Insufficient");
    }

    [Test]
    public void UseAbility_DeductsResource()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.InitializeResource("mana", 100);
        var playerAbility = PlayerAbility.Create("flame-bolt", isUnlocked: true);
        player.AddAbility(playerAbility);
        var monster = new Monster("Test Goblin", "A weak goblin", 50, new Stats(50, 5, 2));

        // Act
        var result = _service.UseAbility(player, "flame-bolt", monster);

        // Assert
        result.Success.Should().BeTrue();
        player.GetResource("mana")!.Current.Should().Be(85); // 100 - 15
    }

    [Test]
    public void UseAbility_SetsCooldown()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.InitializeResource("mana", 100);
        var playerAbility = PlayerAbility.Create("frost-nova", isUnlocked: true);
        player.AddAbility(playerAbility);

        // Act
        var result = _service.UseAbility(player, "frost-nova");

        // Assert
        result.Success.Should().BeTrue();
        player.GetAbility("frost-nova")!.CurrentCooldown.Should().Be(4);
    }

    [Test]
    public void UseAbility_AppliesDamageEffect()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.InitializeResource("mana", 100);
        var playerAbility = PlayerAbility.Create("flame-bolt", isUnlocked: true);
        player.AddAbility(playerAbility);
        var monster = new Monster("Test Goblin", "A weak goblin", 50, new Stats(50, 5, 2));
        var initialHealth = monster.Health;

        // Act
        var result = _service.UseAbility(player, "flame-bolt", monster);

        // Assert
        result.Success.Should().BeTrue();
        result.HasEffects.Should().BeTrue();
        result.TotalDamageDealt.Should().BeGreaterThan(0);
        monster.Health.Should().BeLessThan(initialHealth);
    }

    [Test]
    public void InitializePlayerAbilities_AddsStartingAbilities()
    {
        // Arrange
        var player = CreateTestPlayer();
        var classDef = ClassDefinition.Create(
            "galdr-caster", "Galdr-Caster", "Mage", "mystic",
            StatModifiers.None, StatModifiers.None, "mana",
            startingAbilityIds: ["flame-bolt", "frost-nova"]
        );

        // Act
        _service.InitializePlayerAbilities(player, classDef);

        // Assert
        player.Abilities.Should().HaveCount(2);
        player.HasAbility("flame-bolt").Should().BeTrue();
        player.HasAbility("frost-nova").Should().BeTrue();
    }

    [Test]
    public void ProcessTurnEnd_ReducesCooldowns()
    {
        // Arrange
        var player = CreateTestPlayer();
        var ability1 = PlayerAbility.Create("frost-nova");
        ability1.Use(cooldownDuration: 3);
        var ability2 = PlayerAbility.Create("flame-bolt");
        ability2.Use(cooldownDuration: 1);
        player.AddAbility(ability1);
        player.AddAbility(ability2);

        // Act
        _service.ProcessTurnEnd(player);

        // Assert
        player.GetAbility("frost-nova")!.CurrentCooldown.Should().Be(2);
        player.GetAbility("flame-bolt")!.CurrentCooldown.Should().Be(0);
    }

    private static Player CreateTestPlayer()
    {
        return new Player("TestPlayer", new Stats(100, 10, 5));
    }
}
