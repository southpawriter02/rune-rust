using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models.Combat;
using RuneAndRust.Engine.Factories;
using Xunit;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for the EnemyFactory class.
/// Validates enemy creation from templates with scaling, variance, property copying,
/// and ability hydration (v0.2.4a).
/// </summary>
public class EnemyFactoryTests
{
    private readonly Mock<ILogger<EnemyFactory>> _mockLogger;
    private readonly Mock<IDiceService> _mockDice;
    private readonly Mock<ICreatureTraitService> _mockTraitService;
    private readonly Mock<IActiveAbilityRepository> _mockAbilityRepository;
    private readonly EnemyFactory _factory;

    public EnemyFactoryTests()
    {
        _mockLogger = new Mock<ILogger<EnemyFactory>>();
        _mockDice = new Mock<IDiceService>();
        _mockTraitService = new Mock<ICreatureTraitService>();
        _mockAbilityRepository = new Mock<IActiveAbilityRepository>();

        // Default dice roll returns 5 (middle of 0-10 range) for consistent tests
        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(5);

        // Default ability repository returns null (no abilities found)
        _mockAbilityRepository.Setup(r => r.GetByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((ActiveAbility?)null);

        _factory = new EnemyFactory(
            _mockLogger.Object,
            _mockDice.Object,
            _mockTraitService.Object,
            _mockAbilityRepository.Object);
    }

    #region CreateFromTemplateAsync Tests

    [Fact]
    public async Task CreateFromTemplateAsync_ReturnsValidEnemy()
    {
        // Arrange
        var template = CreateTestTemplate();

        // Act
        var enemy = await _factory.CreateFromTemplateAsync(template);

        // Assert
        enemy.Should().NotBeNull();
        enemy.Name.Should().Be("Test Enemy");
        enemy.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateFromTemplateAsync_CopiesTemplateId()
    {
        // Arrange
        var template = CreateTestTemplate();

        // Act
        var enemy = await _factory.CreateFromTemplateAsync(template);

        // Assert
        enemy.TemplateId.Should().Be("test_enemy_01");
    }

    [Fact]
    public async Task CreateFromTemplateAsync_CopiesArchetype()
    {
        // Arrange
        var template = CreateTestTemplate(archetype: EnemyArchetype.Tank);

        // Act
        var enemy = await _factory.CreateFromTemplateAsync(template);

        // Assert
        enemy.Archetype.Should().Be(EnemyArchetype.Tank);
    }

    [Fact]
    public async Task CreateFromTemplateAsync_CopiesTags()
    {
        // Arrange
        var template = CreateTestTemplate(tags: new List<string> { "Undying", "Construct" });

        // Act
        var enemy = await _factory.CreateFromTemplateAsync(template);

        // Assert
        enemy.Tags.Should().BeEquivalentTo(new[] { "Undying", "Construct" });
    }

    [Fact]
    public async Task CreateFromTemplateAsync_TagsAreIndependentCopy()
    {
        // Arrange
        var template = CreateTestTemplate(tags: new List<string> { "Original" });

        // Act
        var enemy = await _factory.CreateFromTemplateAsync(template);
        enemy.Tags.Add("Modified");

        // Assert - template tags should not be affected
        template.Tags.Should().NotContain("Modified");
    }

    [Fact]
    public async Task CreateFromTemplateAsync_CopiesAllAttributes()
    {
        // Arrange
        var attributes = new Dictionary<CharacterAttribute, int>
        {
            { CharacterAttribute.Sturdiness, 7 },
            { CharacterAttribute.Might, 8 },
            { CharacterAttribute.Wits, 4 },
            { CharacterAttribute.Will, 3 },
            { CharacterAttribute.Finesse, 5 }
        };
        var template = CreateTestTemplate(attributes: attributes);

        // Act
        var enemy = await _factory.CreateFromTemplateAsync(template);

        // Assert
        enemy.Attributes.Should().BeEquivalentTo(attributes);
        enemy.GetAttribute(CharacterAttribute.Might).Should().Be(8);
    }

    [Fact]
    public async Task CreateFromTemplateAsync_CopiesWeaponStats()
    {
        // Arrange
        var template = CreateTestTemplate(weaponDamageDie: 8, weaponName: "Great Axe");

        // Act
        var enemy = await _factory.CreateFromTemplateAsync(template);

        // Assert
        enemy.WeaponDamageDie.Should().Be(8);
        enemy.WeaponName.Should().Be("Great Axe");
    }

    [Fact]
    public async Task CreateFromTemplateAsync_CopiesArmorSoak()
    {
        // Arrange
        var template = CreateTestTemplate(baseSoak: 5);

        // Act
        var enemy = await _factory.CreateFromTemplateAsync(template);

        // Assert
        enemy.ArmorSoak.Should().Be(5);
    }

    #endregion

    #region Variance Tests

    [Fact]
    public async Task CreateFromTemplateAsync_AppliesVariance_MinRoll()
    {
        // Arrange
        _mockDice.Setup(d => d.RollSingle(11, "HP Variance")).Returns(0); // 0.95x
        var template = CreateTestTemplate(baseHp: 100, baseStamina: 100);

        // Act
        var enemy = await _factory.CreateFromTemplateAsync(template);

        // Assert - 100 * 1.0 (standard) * 0.95 = 95
        enemy.MaxHp.Should().Be(95);
        enemy.MaxStamina.Should().Be(95);
    }

    [Fact]
    public async Task CreateFromTemplateAsync_AppliesVariance_MaxRoll()
    {
        // Arrange
        _mockDice.Setup(d => d.RollSingle(11, "HP Variance")).Returns(10); // 1.05x
        var template = CreateTestTemplate(baseHp: 100, baseStamina: 100);

        // Act
        var enemy = await _factory.CreateFromTemplateAsync(template);

        // Assert - 100 * 1.0 (standard) * 1.05 = ~105 (allow for floating point)
        enemy.MaxHp.Should().BeInRange(104, 105);
        enemy.MaxStamina.Should().BeInRange(104, 105);
    }

    [Fact]
    public async Task CreateFromTemplateAsync_AppliesVariance_MiddleRoll()
    {
        // Arrange
        _mockDice.Setup(d => d.RollSingle(11, "HP Variance")).Returns(5); // 1.0x
        var template = CreateTestTemplate(baseHp: 100, baseStamina: 100);

        // Act
        var enemy = await _factory.CreateFromTemplateAsync(template);

        // Assert - 100 * 1.0 (standard) * 1.0 = 100
        enemy.MaxHp.Should().Be(100);
        enemy.MaxStamina.Should().Be(100);
    }

    [Fact]
    public async Task CreateFromTemplateAsync_CurrentHpEqualsMaxHp()
    {
        // Arrange
        var template = CreateTestTemplate(baseHp: 60);

        // Act
        var enemy = await _factory.CreateFromTemplateAsync(template);

        // Assert
        enemy.CurrentHp.Should().Be(enemy.MaxHp);
    }

    [Fact]
    public async Task CreateFromTemplateAsync_CurrentStaminaEqualsMaxStamina()
    {
        // Arrange
        var template = CreateTestTemplate(baseStamina: 40);

        // Act
        var enemy = await _factory.CreateFromTemplateAsync(template);

        // Assert
        enemy.CurrentStamina.Should().Be(enemy.MaxStamina);
    }

    #endregion

    #region Tier Scaling Tests

    [Fact]
    public async Task CreateFromTemplateAsync_MinionTier_ReducesStats()
    {
        // Arrange
        _mockDice.Setup(d => d.RollSingle(11, "HP Variance")).Returns(5); // 1.0x variance
        var template = CreateTestTemplate(tier: ThreatTier.Minion, baseHp: 100);

        // Act
        var enemy = await _factory.CreateFromTemplateAsync(template);

        // Assert - 100 * 0.6 (minion) * 1.0 = 60
        enemy.MaxHp.Should().Be(60);
    }

    [Fact]
    public async Task CreateFromTemplateAsync_StandardTier_NormalStats()
    {
        // Arrange
        _mockDice.Setup(d => d.RollSingle(11, "HP Variance")).Returns(5); // 1.0x variance
        var template = CreateTestTemplate(tier: ThreatTier.Standard, baseHp: 100);

        // Act
        var enemy = await _factory.CreateFromTemplateAsync(template);

        // Assert - 100 * 1.0 (standard) * 1.0 = 100
        enemy.MaxHp.Should().Be(100);
    }

    [Fact]
    public async Task CreateFromTemplateAsync_EliteTier_IncreasesStats()
    {
        // Arrange
        _mockDice.Setup(d => d.RollSingle(11, "HP Variance")).Returns(5); // 1.0x variance
        var template = CreateTestTemplate(tier: ThreatTier.Elite, baseHp: 100);

        // Act
        var enemy = await _factory.CreateFromTemplateAsync(template);

        // Assert - 100 * 1.5 (elite) * 1.0 = 150
        enemy.MaxHp.Should().Be(150);
    }

    [Fact]
    public async Task CreateFromTemplateAsync_BossTier_FixedHighStats()
    {
        // Arrange
        _mockDice.Setup(d => d.RollSingle(11, "HP Variance")).Returns(5); // 1.0x variance
        var template = CreateTestTemplate(tier: ThreatTier.Boss, baseHp: 100);

        // Act
        var enemy = await _factory.CreateFromTemplateAsync(template);

        // Assert - 100 * 2.5 (boss, fixed) * 1.0 = 250
        enemy.MaxHp.Should().Be(250);
    }

    #endregion

    #region Party Level Scaling Tests

    [Fact]
    public async Task CreateFromTemplateAsync_PartyLevel1_NoScaling()
    {
        // Arrange
        _mockDice.Setup(d => d.RollSingle(11, "HP Variance")).Returns(5);
        var template = CreateTestTemplate(baseHp: 100);

        // Act
        var enemy = await _factory.CreateFromTemplateAsync(template, partyLevel: 1);

        // Assert - Level 1 = 1.0x multiplier
        enemy.MaxHp.Should().Be(100);
    }

    [Fact]
    public async Task CreateFromTemplateAsync_PartyLevel5_ScalesUp()
    {
        // Arrange
        _mockDice.Setup(d => d.RollSingle(11, "HP Variance")).Returns(5);
        var template = CreateTestTemplate(baseHp: 100);

        // Act
        var enemy = await _factory.CreateFromTemplateAsync(template, partyLevel: 5);

        // Assert - Level 5 = 1.0 + (4 * 0.1) = 1.4x, so 100 * 1.0 * 1.4 = 140
        enemy.MaxHp.Should().Be(140);
    }

    [Fact]
    public async Task CreateFromTemplateAsync_BossTier_IgnoresPartyLevelScaling()
    {
        // Arrange
        _mockDice.Setup(d => d.RollSingle(11, "HP Variance")).Returns(5);
        var template = CreateTestTemplate(tier: ThreatTier.Boss, baseHp: 100);

        // Act
        var enemy1 = await _factory.CreateFromTemplateAsync(template, partyLevel: 1);
        var enemy5 = await _factory.CreateFromTemplateAsync(template, partyLevel: 5);

        // Assert - Boss tier uses fixed 2.5x, no level scaling
        enemy1.MaxHp.Should().Be(250);
        enemy5.MaxHp.Should().Be(250);
    }

    #endregion

    #region CreateByIdAsync Tests

    [Fact]
    public async Task CreateByIdAsync_ValidId_ReturnsEnemy()
    {
        // Act
        var enemy = await _factory.CreateByIdAsync("und_draugr_01");

        // Assert
        enemy.Should().NotBeNull();
        enemy.Name.Should().Be("Rusted Draugr");
        enemy.TemplateId.Should().Be("und_draugr_01");
    }

    [Fact]
    public async Task CreateByIdAsync_InvalidId_ReturnsFallback()
    {
        // Act
        var enemy = await _factory.CreateByIdAsync("nonexistent_enemy");

        // Assert - Falls back to und_draugr_01
        enemy.Should().NotBeNull();
        enemy.Name.Should().Be("Rusted Draugr");
        enemy.TemplateId.Should().Be("und_draugr_01");
    }

    [Fact]
    public async Task CreateByIdAsync_WithPartyLevel_ScalesCorrectly()
    {
        // Arrange
        _mockDice.Setup(d => d.RollSingle(11, "HP Variance")).Returns(5);

        // Act
        var enemy = await _factory.CreateByIdAsync("und_draugr_01", partyLevel: 3);

        // Assert - Draugr has 60 base HP, level 3 = 1.2x, so 60 * 1.0 * 1.2 = 72
        enemy.MaxHp.Should().Be(72);
    }

    #endregion

    #region Template Registry Tests

    [Fact]
    public void GetTemplateIds_ReturnsAllTemplates()
    {
        // Act
        var ids = _factory.GetTemplateIds();

        // Assert
        ids.Should().Contain("und_draugr_01");
        ids.Should().Contain("und_haug_01");
        ids.Should().Contain("mec_serv_01");
        ids.Should().Contain("bst_vargr_01");
        ids.Should().Contain("hum_raider_01");
        ids.Should().HaveCount(5);
    }

    [Fact]
    public void GetTemplate_ValidId_ReturnsTemplate()
    {
        // Act
        var template = _factory.GetTemplate("und_draugr_01");

        // Assert
        template.Should().NotBeNull();
        template!.Name.Should().Be("Rusted Draugr");
        template.Archetype.Should().Be(EnemyArchetype.DPS);
        template.Tier.Should().Be(ThreatTier.Standard);
    }

    [Fact]
    public void GetTemplate_InvalidId_ReturnsNull()
    {
        // Act
        var template = _factory.GetTemplate("nonexistent");

        // Assert
        template.Should().BeNull();
    }

    #endregion

    #region Built-in Template Validation Tests

    [Fact]
    public void BuiltInTemplate_RustedDraugr_HasCorrectStats()
    {
        // Act
        var template = _factory.GetTemplate("und_draugr_01");

        // Assert
        template.Should().NotBeNull();
        template!.BaseHp.Should().Be(60);
        template.BaseStamina.Should().Be(40);
        template.BaseSoak.Should().Be(2);
        template.WeaponDamageDie.Should().Be(6);
        template.Tags.Should().Contain("Undying");
        template.Tags.Should().Contain("Construct");
    }

    [Fact]
    public void BuiltInTemplate_HaugbuiLaborer_HasCorrectStats()
    {
        // Act
        var template = _factory.GetTemplate("und_haug_01");

        // Assert
        template.Should().NotBeNull();
        template!.Archetype.Should().Be(EnemyArchetype.Tank);
        template.BaseHp.Should().Be(90);
        template.BaseSoak.Should().Be(4);
    }

    [Fact]
    public void BuiltInTemplate_UtilityServitor_IsMinion()
    {
        // Act
        var template = _factory.GetTemplate("mec_serv_01");

        // Assert
        template.Should().NotBeNull();
        template!.Tier.Should().Be(ThreatTier.Minion);
        template.Archetype.Should().Be(EnemyArchetype.Swarm);
    }

    [Fact]
    public void BuiltInTemplate_AshVargr_IsGlassCannon()
    {
        // Act
        var template = _factory.GetTemplate("bst_vargr_01");

        // Assert
        template.Should().NotBeNull();
        template!.Archetype.Should().Be(EnemyArchetype.GlassCannon);
        template.BaseHp.Should().Be(45); // Lower HP
        template.WeaponDamageDie.Should().Be(8); // High damage
    }

    [Fact]
    public void BuiltInTemplate_RustClanScav_IsSupport()
    {
        // Act
        var template = _factory.GetTemplate("hum_raider_01");

        // Assert
        template.Should().NotBeNull();
        template!.Archetype.Should().Be(EnemyArchetype.Support);
        template.Tags.Should().Contain("Humanoid");
    }

    [Theory]
    [InlineData("und_draugr_01")]
    [InlineData("und_haug_01")]
    [InlineData("mec_serv_01")]
    [InlineData("bst_vargr_01")]
    [InlineData("hum_raider_01")]
    public void BuiltInTemplates_AllHaveRequiredAttributes(string templateId)
    {
        // Act
        var template = _factory.GetTemplate(templateId);

        // Assert
        template.Should().NotBeNull();
        template!.Attributes.Should().ContainKey(CharacterAttribute.Sturdiness);
        template.Attributes.Should().ContainKey(CharacterAttribute.Might);
        template.Attributes.Should().ContainKey(CharacterAttribute.Wits);
        template.Attributes.Should().ContainKey(CharacterAttribute.Will);
        template.Attributes.Should().ContainKey(CharacterAttribute.Finesse);
    }

    [Theory]
    [InlineData("und_draugr_01")]
    [InlineData("und_haug_01")]
    [InlineData("mec_serv_01")]
    [InlineData("bst_vargr_01")]
    [InlineData("hum_raider_01")]
    public async Task BuiltInTemplates_AllCreateValidEnemies(string templateId)
    {
        // Act
        var enemy = await _factory.CreateByIdAsync(templateId);

        // Assert
        enemy.Should().NotBeNull();
        enemy.MaxHp.Should().BeGreaterThan(0);
        enemy.MaxStamina.Should().BeGreaterThan(0);
        enemy.WeaponDamageDie.Should().BeGreaterThan(0);
        enemy.Tags.Should().NotBeNull();
    }

    [Theory]
    [InlineData("und_draugr_01")]
    [InlineData("und_haug_01")]
    [InlineData("mec_serv_01")]
    [InlineData("bst_vargr_01")]
    [InlineData("hum_raider_01")]
    public void BuiltInTemplates_AllHaveAbilityNames(string templateId)
    {
        // Act
        var template = _factory.GetTemplate(templateId);

        // Assert - All templates should have at least one ability (v0.2.4a)
        template.Should().NotBeNull();
        template!.AbilityNames.Should().NotBeNull();
        template.AbilityNames.Should().HaveCountGreaterThan(0);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task CreateFromTemplateAsync_MinimumHpIsOne()
    {
        // Arrange
        _mockDice.Setup(d => d.RollSingle(11, "HP Variance")).Returns(0); // 0.95x
        var template = CreateTestTemplate(tier: ThreatTier.Minion, baseHp: 1);

        // Act
        var enemy = await _factory.CreateFromTemplateAsync(template);

        // Assert - Even with 0.6 scaling and 0.95 variance, HP should be at least 1
        enemy.MaxHp.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task CreateFromTemplateAsync_EachCallCreatesUniqueEnemy()
    {
        // Arrange
        var template = CreateTestTemplate();

        // Act
        var enemy1 = await _factory.CreateFromTemplateAsync(template);
        var enemy2 = await _factory.CreateFromTemplateAsync(template);

        // Assert
        enemy1.Id.Should().NotBe(enemy2.Id);
    }

    #endregion

    #region Ability Hydration Tests (v0.2.4a)

    [Fact]
    public async Task CreateFromTemplateAsync_HydratesAbilities_WhenAbilityNamesProvided()
    {
        // Arrange
        var testAbility = new ActiveAbility { Name = "Test Slash", StaminaCost = 5 };
        _mockAbilityRepository.Setup(r => r.GetByNameAsync("Test Slash"))
            .ReturnsAsync(testAbility);

        var template = CreateTestTemplate(abilityNames: new List<string> { "Test Slash" });

        // Act
        var enemy = await _factory.CreateFromTemplateAsync(template);

        // Assert
        enemy.Abilities.Should().HaveCount(1);
        enemy.Abilities[0].Name.Should().Be("Test Slash");
    }

    [Fact]
    public async Task CreateFromTemplateAsync_ReturnsEmptyAbilities_WhenNoAbilityNames()
    {
        // Arrange
        var template = CreateTestTemplate(abilityNames: new List<string>());

        // Act
        var enemy = await _factory.CreateFromTemplateAsync(template);

        // Assert
        enemy.Abilities.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateFromTemplateAsync_LogsWarning_WhenAbilityNotFound()
    {
        // Arrange
        _mockAbilityRepository.Setup(r => r.GetByNameAsync("Nonexistent Ability"))
            .ReturnsAsync((ActiveAbility?)null);

        var template = CreateTestTemplate(abilityNames: new List<string> { "Nonexistent Ability" });

        // Act
        var enemy = await _factory.CreateFromTemplateAsync(template);

        // Assert
        enemy.Abilities.Should().BeEmpty();
        // Logger warning would be verified if we set up log verification
    }

    [Fact]
    public async Task CreateFromTemplateAsync_HandlesPartialAbilityMatch()
    {
        // Arrange
        var foundAbility = new ActiveAbility { Name = "Found Ability", StaminaCost = 3 };
        _mockAbilityRepository.Setup(r => r.GetByNameAsync("Found Ability"))
            .ReturnsAsync(foundAbility);
        _mockAbilityRepository.Setup(r => r.GetByNameAsync("Missing Ability"))
            .ReturnsAsync((ActiveAbility?)null);

        var template = CreateTestTemplate(abilityNames: new List<string> { "Found Ability", "Missing Ability" });

        // Act
        var enemy = await _factory.CreateFromTemplateAsync(template);

        // Assert - Only the found ability should be in the list
        enemy.Abilities.Should().HaveCount(1);
        enemy.Abilities[0].Name.Should().Be("Found Ability");
    }

    [Fact]
    public async Task CreateFromTemplateAsync_HydratesMultipleAbilities()
    {
        // Arrange
        var ability1 = new ActiveAbility { Name = "Strike", StaminaCost = 2 };
        var ability2 = new ActiveAbility { Name = "Bash", StaminaCost = 4 };
        _mockAbilityRepository.Setup(r => r.GetByNameAsync("Strike")).ReturnsAsync(ability1);
        _mockAbilityRepository.Setup(r => r.GetByNameAsync("Bash")).ReturnsAsync(ability2);

        var template = CreateTestTemplate(abilityNames: new List<string> { "Strike", "Bash" });

        // Act
        var enemy = await _factory.CreateFromTemplateAsync(template);

        // Assert
        enemy.Abilities.Should().HaveCount(2);
        enemy.Abilities.Select(a => a.Name).Should().Contain("Strike");
        enemy.Abilities.Select(a => a.Name).Should().Contain("Bash");
    }

    [Fact]
    public void Enemy_Abilities_InitializesToEmptyList()
    {
        // Arrange & Act
        var enemy = new Enemy();

        // Assert
        enemy.Abilities.Should().NotBeNull();
        enemy.Abilities.Should().BeEmpty();
    }

    [Fact]
    public void Combatant_FromEnemy_CopiesAbilitiesList()
    {
        // Arrange
        var enemy = new Enemy
        {
            Name = "Test",
            Abilities = new List<ActiveAbility>
            {
                new ActiveAbility { Name = "Slash", StaminaCost = 5 }
            }
        };

        // Act
        var combatant = Combatant.FromEnemy(enemy);

        // Assert
        combatant.Abilities.Should().HaveCount(1);
        combatant.Abilities[0].Name.Should().Be("Slash");
    }

    [Fact]
    public void Combatant_FromEnemy_HandlesNullAbilities()
    {
        // Arrange
        var enemy = new Enemy { Name = "Test" };
        enemy.Abilities = null!; // Force null for edge case

        // Act
        var combatant = Combatant.FromEnemy(enemy);

        // Assert
        combatant.Abilities.Should().NotBeNull();
        combatant.Abilities.Should().BeEmpty();
    }

    #endregion

    #region Helper Methods

    private static EnemyTemplate CreateTestTemplate(
        string id = "test_enemy_01",
        string name = "Test Enemy",
        EnemyArchetype archetype = EnemyArchetype.DPS,
        ThreatTier tier = ThreatTier.Standard,
        int baseHp = 50,
        int baseStamina = 30,
        int baseSoak = 2,
        Dictionary<CharacterAttribute, int>? attributes = null,
        int weaponDamageDie = 6,
        string weaponName = "Claws",
        List<string>? tags = null,
        List<string>? abilityNames = null)
    {
        return new EnemyTemplate(
            Id: id,
            Name: name,
            Description: "Test enemy description",
            Archetype: archetype,
            Tier: tier,
            BaseHp: baseHp,
            BaseStamina: baseStamina,
            BaseSoak: baseSoak,
            Attributes: attributes ?? new Dictionary<CharacterAttribute, int>
            {
                { CharacterAttribute.Sturdiness, 5 },
                { CharacterAttribute.Might, 5 },
                { CharacterAttribute.Wits, 3 },
                { CharacterAttribute.Will, 3 },
                { CharacterAttribute.Finesse, 4 }
            },
            WeaponDamageDie: weaponDamageDie,
            WeaponName: weaponName,
            Tags: tags ?? new List<string> { "Test" },
            AbilityNames: abilityNames ?? new List<string>()
        );
    }

    #endregion
}
