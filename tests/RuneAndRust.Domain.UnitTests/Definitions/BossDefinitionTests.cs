using FluentAssertions;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Definitions;

/// <summary>
/// Unit tests for BossDefinition (v0.10.4a).
/// </summary>
[TestFixture]
public class BossDefinitionTests
{
    // ═══════════════════════════════════════════════════════════════
    // CREATE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Create_WithValidParameters_ReturnsBossDefinition()
    {
        // Arrange & Act
        var boss = BossDefinition.Create(
            bossId: "skeleton-king",
            name: "The Skeleton King",
            description: "An ancient ruler risen from death",
            baseMonsterDefinitionId: "skeleton-elite");

        // Assert
        boss.Should().NotBeNull();
        boss.BossId.Should().Be("skeleton-king");
        boss.Name.Should().Be("The Skeleton King");
        boss.Description.Should().Be("An ancient ruler risen from death");
        boss.BaseMonsterDefinitionId.Should().Be("skeleton-elite");
        boss.Phases.Should().BeEmpty();
        boss.Loot.Should().BeEmpty();
    }

    [Test]
    public void Create_NormalizesIdToLowercase()
    {
        // Arrange & Act
        var boss = BossDefinition.Create(
            bossId: "SKELETON-KING",
            name: "The Skeleton King",
            description: "Test",
            baseMonsterDefinitionId: "SKELETON-ELITE");

        // Assert
        boss.BossId.Should().Be("skeleton-king");
        boss.BaseMonsterDefinitionId.Should().Be("skeleton-elite");
    }

    [Test]
    public void Create_WithNullBossId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => BossDefinition.Create(
            bossId: null!,
            name: "Test",
            description: "Test",
            baseMonsterDefinitionId: "test-base");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("bossId");
    }

    [Test]
    public void Create_WithNullName_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => BossDefinition.Create(
            bossId: "test",
            name: null!,
            description: "Test",
            baseMonsterDefinitionId: "test-base");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Test]
    public void Create_WithNullBaseMonster_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => BossDefinition.Create(
            bossId: "test",
            name: "Test",
            description: "Test",
            baseMonsterDefinitionId: null!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("baseMonsterDefinitionId");
    }

    // ═══════════════════════════════════════════════════════════════
    // FLUENT CONFIGURATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void WithTitleText_SetsTitleText()
    {
        // Arrange
        var boss = BossDefinition.Create(
            "skeleton-king", "The Skeleton King", "Test", "skeleton-elite");

        // Act
        boss.WithTitleText("Lord of the Undead Crypt");

        // Assert
        boss.TitleText.Should().Be("Lord of the Undead Crypt");
    }

    [Test]
    public void WithPhase_AddsPhase()
    {
        // Arrange
        var boss = BossDefinition.Create(
            "skeleton-king", "The Skeleton King", "Test", "skeleton-elite");

        var phase = BossPhase.Create(1, "Awakened", 100, BossBehavior.Tactical);

        // Act
        boss.WithPhase(phase);

        // Assert
        boss.Phases.Should().HaveCount(1);
        boss.Phases[0].Should().Be(phase);
        boss.PhaseCount.Should().Be(1);
    }

    [Test]
    public void WithLoot_AddsLootEntry()
    {
        // Arrange
        var boss = BossDefinition.Create(
            "skeleton-king", "The Skeleton King", "Test", "skeleton-elite");

        var loot = BossLootEntry.Guaranteed("gold", 500);

        // Act
        boss.WithLoot(loot);

        // Assert
        boss.Loot.Should().HaveCount(1);
        boss.Loot[0].ItemId.Should().Be("gold");
        boss.Loot[0].Amount.Should().Be(500);
    }

    [Test]
    public void GuaranteedLoot_ReturnsOnlyGuaranteedDrops()
    {
        // Arrange
        var boss = BossDefinition.Create(
            "skeleton-king", "The Skeleton King", "Test", "skeleton-elite")
            .WithLoot(BossLootEntry.Guaranteed("gold", 500))
            .WithLoot(BossLootEntry.Create("rare-item", 0.25));

        // Act
        var guaranteed = boss.GuaranteedLoot.ToList();

        // Assert
        guaranteed.Should().HaveCount(1);
        guaranteed[0].ItemId.Should().Be("gold");
    }

    [Test]
    public void ChanceLoot_ReturnsOnlyChanceDrops()
    {
        // Arrange
        var boss = BossDefinition.Create(
            "skeleton-king", "The Skeleton King", "Test", "skeleton-elite")
            .WithLoot(BossLootEntry.Guaranteed("gold", 500))
            .WithLoot(BossLootEntry.Create("rare-item", 0.25));

        // Act
        var chance = boss.ChanceLoot.ToList();

        // Assert
        chance.Should().HaveCount(1);
        chance[0].ItemId.Should().Be("rare-item");
    }

    // ═══════════════════════════════════════════════════════════════
    // PHASE MANAGEMENT TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetPhaseForHealth_ReturnsCorrectPhase_ForFullHealth()
    {
        // Arrange
        var boss = CreateThreePhaseBoss();

        // Act
        var phase = boss.GetPhaseForHealth(100);

        // Assert
        phase.Should().NotBeNull();
        phase!.PhaseNumber.Should().Be(1);
        phase.Name.Should().Be("Awakened");
    }

    [Test]
    public void GetPhaseForHealth_ReturnsCorrectPhase_ForMidHealth()
    {
        // Arrange
        var boss = CreateThreePhaseBoss();

        // Act
        var phase = boss.GetPhaseForHealth(45);

        // Assert
        phase.Should().NotBeNull();
        phase!.PhaseNumber.Should().Be(2);
        phase.Name.Should().Be("Commanding");
    }

    [Test]
    public void GetPhaseForHealth_ReturnsCorrectPhase_ForLowHealth()
    {
        // Arrange
        var boss = CreateThreePhaseBoss();

        // Act
        var phase = boss.GetPhaseForHealth(15);

        // Assert
        phase.Should().NotBeNull();
        phase!.PhaseNumber.Should().Be(3);
        phase.Name.Should().Be("Enraged");
    }

    [Test]
    public void GetPhaseForHealth_ReturnsLastPhase_ForZeroHealth()
    {
        // Arrange
        var boss = CreateThreePhaseBoss();

        // Act
        var phase = boss.GetPhaseForHealth(0);

        // Assert
        phase.Should().NotBeNull();
        phase!.PhaseNumber.Should().Be(3);
    }

    [Test]
    public void GetPhaseForHealth_ReturnsNull_WhenNoPhasesConfigured()
    {
        // Arrange
        var boss = BossDefinition.Create(
            "empty-boss", "Empty Boss", "Test", "base-monster");

        // Act
        var phase = boss.GetPhaseForHealth(50);

        // Assert
        phase.Should().BeNull();
    }

    [Test]
    public void GetPhase_ReturnsPhaseByNumber()
    {
        // Arrange
        var boss = CreateThreePhaseBoss();

        // Act
        var phase = boss.GetPhase(2);

        // Assert
        phase.Should().NotBeNull();
        phase!.PhaseNumber.Should().Be(2);
        phase.Name.Should().Be("Commanding");
    }

    [Test]
    public void GetStartingPhase_ReturnsHighestThresholdPhase()
    {
        // Arrange
        var boss = CreateThreePhaseBoss();

        // Act
        var phase = boss.GetStartingPhase();

        // Assert
        phase.Should().NotBeNull();
        phase!.PhaseNumber.Should().Be(1);
        phase.HealthThreshold.Should().Be(100);
    }

    [Test]
    public void GetFinalPhase_ReturnsLowestThresholdPhase()
    {
        // Arrange
        var boss = CreateThreePhaseBoss();

        // Act
        var phase = boss.GetFinalPhase();

        // Assert
        phase.Should().NotBeNull();
        phase!.PhaseNumber.Should().Be(3);
        phase.HealthThreshold.Should().Be(25);
    }

    [Test]
    public void HasMultiplePhases_ReturnsTrue_WhenMultiplePhasesExist()
    {
        // Arrange
        var boss = CreateThreePhaseBoss();

        // Assert
        boss.HasMultiplePhases.Should().BeTrue();
    }

    [Test]
    public void HasMultiplePhases_ReturnsFalse_WhenSinglePhase()
    {
        // Arrange
        var boss = BossDefinition.Create(
            "single-phase-boss", "Single Phase Boss", "Test", "base-monster")
            .WithPhase(BossPhase.Create(1, "Only Phase", 100, BossBehavior.Aggressive));

        // Assert
        boss.HasMultiplePhases.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // VALIDATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void IsValid_ReturnsTrue_ForValidBoss()
    {
        // Arrange
        var boss = CreateThreePhaseBoss();

        // Assert
        boss.IsValid.Should().BeTrue();
    }

    [Test]
    public void IsValid_ReturnsFalse_WhenNoPhases()
    {
        // Arrange
        var boss = BossDefinition.Create(
            "no-phases", "No Phases Boss", "Test", "base-monster");

        // Assert
        boss.IsValid.Should().BeFalse();
    }

    [Test]
    public void GetValidationErrors_ReturnsErrors_ForMissingPhases()
    {
        // Arrange
        var boss = BossDefinition.Create(
            "no-phases", "No Phases Boss", "Test", "base-monster");

        // Act
        var errors = boss.GetValidationErrors();

        // Assert
        errors.Should().Contain(e => e.Contains("At least one phase"));
    }

    [Test]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var boss = CreateThreePhaseBoss();

        // Act
        var result = boss.ToString();

        // Assert
        result.Should().Contain("The Skeleton King");
        result.Should().Contain("skeleton-king");
        result.Should().Contain("3 phase");
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a standard three-phase boss for testing.
    /// </summary>
    private static BossDefinition CreateThreePhaseBoss()
    {
        return BossDefinition.Create(
                "skeleton-king",
                "The Skeleton King",
                "An ancient ruler risen from death",
                "skeleton-elite")
            .WithTitleText("Lord of the Undead Crypt")
            .WithPhase(BossPhase.Create(1, "Awakened", 100, BossBehavior.Tactical))
            .WithPhase(BossPhase.Create(2, "Commanding", 60, BossBehavior.Summoner))
            .WithPhase(BossPhase.Create(3, "Enraged", 25, BossBehavior.Enraged))
            .WithLoot(BossLootEntry.Guaranteed("gold", 500))
            .WithLoot(BossLootEntry.Create("crown-of-bones", 0.25));
    }
}
