using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Infrastructure.Providers;

#pragma warning disable NUnit2045 // Use Assert.Multiple

namespace RuneAndRust.Application.UnitTests.Providers;

/// <summary>
/// Unit tests for JsonBossProvider and related boss definition entities (v0.10.4a).
/// </summary>
/// <remarks>
/// <para>Tests cover:</para>
/// <list type="bullet">
///   <item><description>BossLootEntry creation and validation</description></item>
///   <item><description>SummonConfiguration creation and validation</description></item>
///   <item><description>Provider loading and querying functionality</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class JsonBossProviderTests
{
    // ═══════════════════════════════════════════════════════════════
    // TEST FIXTURES
    // ═══════════════════════════════════════════════════════════════

    private Mock<ILogger<JsonBossProvider>> _mockLogger = null!;
    private string _testConfigPath = null!;

    /// <summary>
    /// Sets up test dependencies before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<JsonBossProvider>>();

        // Use the actual config path for integration-style tests
        var baseDir = TestContext.CurrentContext.TestDirectory;
        _testConfigPath = Path.Combine(baseDir, "..", "..", "..", "..", "..", "config", "bosses.json");
    }

    // ═══════════════════════════════════════════════════════════════
    // BOSS LOOT ENTRY TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that BossLootEntry.Create creates a valid loot entry.
    /// </summary>
    [Test]
    public void BossLootEntry_Create_ValidParameters_ReturnsLootEntry()
    {
        // Arrange & Act
        var loot = BossLootEntry.Create("gold", chance: 0.5, amount: 100);

        // Assert
        loot.ItemId.Should().Be("gold");
        loot.Chance.Should().Be(0.5);
        loot.Amount.Should().Be(100);
        loot.IsGuaranteed.Should().BeFalse();
        loot.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that BossLootEntry.Guaranteed creates a 100% drop chance entry.
    /// </summary>
    [Test]
    public void BossLootEntry_Guaranteed_CreatesFullChanceEntry()
    {
        // Arrange & Act
        var loot = BossLootEntry.Guaranteed("gold", amount: 500);

        // Assert
        loot.ItemId.Should().Be("gold");
        loot.Chance.Should().Be(1.0);
        loot.Amount.Should().Be(500);
        loot.IsGuaranteed.Should().BeTrue();
        loot.ChancePercent.Should().Be("100%");
    }

    /// <summary>
    /// Verifies that BossLootEntry.Create throws for invalid chance values.
    /// </summary>
    [Test]
    public void BossLootEntry_Create_InvalidChance_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var actTooHigh = () => BossLootEntry.Create("item", chance: 1.5);
        var actTooLow = () => BossLootEntry.Create("item", chance: -0.1);

        // Assert
        actTooHigh.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("chance");
        actTooLow.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("chance");
    }

    /// <summary>
    /// Verifies that BossLootEntry.WithAmount creates a modified copy.
    /// </summary>
    [Test]
    public void BossLootEntry_WithAmount_CreatesModifiedCopy()
    {
        // Arrange
        var original = BossLootEntry.Create("item", chance: 0.5, amount: 1);

        // Act
        var modified = original.WithAmount(10);

        // Assert
        original.Amount.Should().Be(1); // Original unchanged
        modified.Amount.Should().Be(10);
        modified.ItemId.Should().Be("item");
        modified.Chance.Should().Be(0.5);
    }

    // ═══════════════════════════════════════════════════════════════
    // SUMMON CONFIGURATION TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that SummonConfiguration.Create creates a valid config.
    /// </summary>
    [Test]
    public void SummonConfiguration_Create_ValidParameters_ReturnsConfig()
    {
        // Arrange & Act
        var config = SummonConfiguration.Create("skeleton-minion", count: 2);

        // Assert
        config.MonsterDefinitionId.Should().Be("skeleton-minion");
        config.Count.Should().Be(2);
        config.IntervalTurns.Should().Be(2); // Default
        config.MaxActive.Should().Be(4); // Default
        config.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that SummonConfiguration fluent methods work correctly.
    /// </summary>
    [Test]
    public void SummonConfiguration_FluentMethods_ModifyConfig()
    {
        // Arrange
        var config = SummonConfiguration.Create("fire-elemental", count: 1);

        // Act
        var modified = config
            .WithIntervalTurns(5)
            .WithMaxActive(8);

        // Assert
        modified.IntervalTurns.Should().Be(5);
        modified.MaxActive.Should().Be(8);
        modified.MonsterDefinitionId.Should().Be("fire-elemental");
        modified.Count.Should().Be(1);
    }

    /// <summary>
    /// Verifies that SummonConfiguration.Empty returns an invalid config.
    /// </summary>
    [Test]
    public void SummonConfiguration_Empty_ReturnsInvalidConfig()
    {
        // Arrange & Act
        var empty = SummonConfiguration.Empty;

        // Assert
        empty.IsValid.Should().BeFalse();
        empty.MonsterDefinitionId.Should().BeNullOrEmpty();
    }

    // ═══════════════════════════════════════════════════════════════
    // JSON BOSS PROVIDER TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the provider loads all bosses from the config file.
    /// </summary>
    [Test]
    public void GetAllBosses_ReturnsAllLoadedBosses()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonBossProvider(_testConfigPath, _mockLogger.Object);

        // Act
        var bosses = provider.GetAllBosses();

        // Assert
        bosses.Should().NotBeEmpty();
        bosses.Should().HaveCountGreaterOrEqualTo(4); // We defined 4 bosses in config
        bosses.Should().Contain(b => b.BossId == "skeleton-king");
        bosses.Should().Contain(b => b.BossId == "volcanic-wyrm");
        bosses.Should().Contain(b => b.BossId == "shadow-lich");
        bosses.Should().Contain(b => b.BossId == "orc-warlord");
    }

    /// <summary>
    /// Verifies that GetBoss returns the correct boss by ID.
    /// </summary>
    [Test]
    public void GetBoss_ExistingId_ReturnsBoss()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonBossProvider(_testConfigPath, _mockLogger.Object);

        // Act
        var boss = provider.GetBoss("skeleton-king");

        // Assert
        boss.Should().NotBeNull();
        boss!.Name.Should().Be("The Skeleton King");
        boss.TitleText.Should().Be("Lord of the Undead Crypt");
        boss.BaseMonsterDefinitionId.Should().Be("skeleton-elite");
        boss.PhaseCount.Should().Be(3);
        boss.Loot.Should().HaveCountGreaterOrEqualTo(3);
    }

    /// <summary>
    /// Verifies that GetBoss returns null for unknown boss IDs.
    /// </summary>
    [Test]
    public void GetBoss_UnknownId_ReturnsNull()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonBossProvider(_testConfigPath, _mockLogger.Object);

        // Act
        var boss = provider.GetBoss("nonexistent-boss");

        // Assert
        boss.Should().BeNull();
    }

    /// <summary>
    /// Verifies that GetBoss performs case-insensitive lookup.
    /// </summary>
    [Test]
    public void GetBoss_CaseInsensitive_ReturnsBoss()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonBossProvider(_testConfigPath, _mockLogger.Object);

        // Act
        var boss = provider.GetBoss("SKELETON-KING");

        // Assert
        boss.Should().NotBeNull();
        boss!.BossId.Should().Be("skeleton-king");
    }

    /// <summary>
    /// Verifies that BossExists returns correct results.
    /// </summary>
    [Test]
    public void BossExists_ReturnsCorrectResult()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonBossProvider(_testConfigPath, _mockLogger.Object);

        // Act & Assert
        provider.BossExists("skeleton-king").Should().BeTrue();
        provider.BossExists("VOLCANIC-WYRM").Should().BeTrue(); // Case-insensitive
        provider.BossExists("nonexistent").Should().BeFalse();
    }

    /// <summary>
    /// Verifies that GetBossIds returns all boss IDs.
    /// </summary>
    [Test]
    public void GetBossIds_ReturnsAllIds()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonBossProvider(_testConfigPath, _mockLogger.Object);

        // Act
        var ids = provider.GetBossIds();

        // Assert
        ids.Should().HaveCountGreaterOrEqualTo(4);
        ids.Should().Contain("skeleton-king");
        ids.Should().Contain("volcanic-wyrm");
        ids.Should().Contain("shadow-lich");
        ids.Should().Contain("orc-warlord");
    }

    /// <summary>
    /// Verifies that GetBossesByPhaseCount filters correctly.
    /// </summary>
    [Test]
    public void GetBossesByPhaseCount_FiltersCorrectly()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonBossProvider(_testConfigPath, _mockLogger.Object);

        // Act
        var threeOrMore = provider.GetBossesByPhaseCount(3);
        var fourOrMore = provider.GetBossesByPhaseCount(4);

        // Assert
        threeOrMore.Should().Contain(b => b.BossId == "skeleton-king"); // 3 phases
        threeOrMore.Should().Contain(b => b.BossId == "volcanic-wyrm"); // 3 phases
        threeOrMore.Should().Contain(b => b.BossId == "orc-warlord"); // 3 phases

        fourOrMore.Should().Contain(b => b.BossId == "shadow-lich"); // 4 phases
        fourOrMore.Should().NotContain(b => b.BossId == "skeleton-king"); // Only 3 phases
    }

    /// <summary>
    /// Verifies that boss phases are correctly loaded with all properties.
    /// </summary>
    [Test]
    public void GetBoss_LoadsPhasesWithAllProperties()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonBossProvider(_testConfigPath, _mockLogger.Object);

        // Act
        var boss = provider.GetBoss("skeleton-king");

        // Assert
        boss.Should().NotBeNull();

        var phase1 = boss!.GetPhase(1);
        phase1.Should().NotBeNull();
        phase1!.Name.Should().Be("Awakened");
        phase1.HealthThreshold.Should().Be(100);
        phase1.Behavior.Should().Be(BossBehavior.Tactical);
        phase1.TransitionText.Should().NotBeNullOrEmpty();

        var phase2 = boss.GetPhase(2);
        phase2.Should().NotBeNull();
        phase2!.Name.Should().Be("Commanding");
        phase2.Behavior.Should().Be(BossBehavior.Summoner);
        phase2.HasSummoning.Should().BeTrue();
        phase2.SummonConfig.MonsterDefinitionId.Should().Be("skeleton-minion");

        var phase3 = boss.GetPhase(3);
        phase3.Should().NotBeNull();
        phase3!.Name.Should().Be("Enraged");
        phase3.Behavior.Should().Be(BossBehavior.Enraged);
        phase3.StatModifiers.Should().NotBeEmpty();
        phase3.TransitionEffectId.Should().Be("boss-enrage-aura");
    }

    /// <summary>
    /// Verifies that boss loot is correctly loaded.
    /// </summary>
    [Test]
    public void GetBoss_LoadsLootCorrectly()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonBossProvider(_testConfigPath, _mockLogger.Object);

        // Act
        var boss = provider.GetBoss("skeleton-king");

        // Assert
        boss.Should().NotBeNull();
        boss!.Loot.Should().HaveCountGreaterOrEqualTo(3);

        // Check guaranteed gold drop
        var goldDrop = boss.Loot.FirstOrDefault(l => l.ItemId == "gold");
        goldDrop.Should().NotBeNull();
        goldDrop!.Amount.Should().Be(500);
        goldDrop.IsGuaranteed.Should().BeTrue();

        // Check rare drop
        var rareDrop = boss.Loot.FirstOrDefault(l => l.ItemId == "crown-of-bones");
        rareDrop.Should().NotBeNull();
        rareDrop!.Chance.Should().BeLessThan(1.0);
        rareDrop.IsGuaranteed.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that GetPhaseForHealth returns correct phases at boundaries.
    /// </summary>
    [Test]
    public void GetBoss_GetPhaseForHealth_ReturnsCorrectPhaseAtBoundaries()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonBossProvider(_testConfigPath, _mockLogger.Object);
        var boss = provider.GetBoss("skeleton-king");

        // Assert
        boss.Should().NotBeNull();

        // At 100% health - should be phase 1
        boss!.GetPhaseForHealth(100)!.PhaseNumber.Should().Be(1);

        // At 61% health - should still be phase 1
        boss.GetPhaseForHealth(61)!.PhaseNumber.Should().Be(1);

        // At 60% health - should be phase 2
        boss.GetPhaseForHealth(60)!.PhaseNumber.Should().Be(2);

        // At 26% health - should still be phase 2
        boss.GetPhaseForHealth(26)!.PhaseNumber.Should().Be(2);

        // At 25% health - should be phase 3
        boss.GetPhaseForHealth(25)!.PhaseNumber.Should().Be(3);

        // At 1% health - should be phase 3
        boss.GetPhaseForHealth(1)!.PhaseNumber.Should().Be(3);
    }
}
