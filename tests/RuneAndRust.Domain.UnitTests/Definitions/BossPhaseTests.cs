using FluentAssertions;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Definitions;

/// <summary>
/// Unit tests for BossPhase (v0.10.4a).
/// </summary>
[TestFixture]
public class BossPhaseTests
{
    // ═══════════════════════════════════════════════════════════════
    // CREATE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Create_WithValidParameters_ReturnsBossPhase()
    {
        // Arrange & Act
        var phase = BossPhase.Create(
            phaseNumber: 1,
            name: "Awakened",
            healthThreshold: 100,
            behavior: BossBehavior.Tactical);

        // Assert
        phase.Should().NotBeNull();
        phase.PhaseNumber.Should().Be(1);
        phase.Name.Should().Be("Awakened");
        phase.HealthThreshold.Should().Be(100);
        phase.Behavior.Should().Be(BossBehavior.Tactical);
        phase.AbilityIds.Should().BeEmpty();
        phase.StatModifiers.Should().BeEmpty();
        phase.TransitionText.Should().BeNull();
        phase.TransitionEffectId.Should().BeNull();
        phase.HasSummoning.Should().BeFalse();
    }

    [Test]
    public void Create_WithPhaseNumberZero_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => BossPhase.Create(
            phaseNumber: 0,
            name: "Test",
            healthThreshold: 100,
            behavior: BossBehavior.Tactical);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("phaseNumber");
    }

    [Test]
    public void Create_WithNullName_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => BossPhase.Create(
            phaseNumber: 1,
            name: null!,
            healthThreshold: 100,
            behavior: BossBehavior.Tactical);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Test]
    public void Create_WithInvalidHealthThreshold_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var actTooHigh = () => BossPhase.Create(
            phaseNumber: 1,
            name: "Test",
            healthThreshold: 101,
            behavior: BossBehavior.Tactical);

        var actTooLow = () => BossPhase.Create(
            phaseNumber: 1,
            name: "Test",
            healthThreshold: -1,
            behavior: BossBehavior.Tactical);

        // Assert
        actTooHigh.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("healthThreshold");
        actTooLow.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("healthThreshold");
    }

    // ═══════════════════════════════════════════════════════════════
    // FLUENT CONFIGURATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void WithAbilities_AddsAbilityIds()
    {
        // Arrange
        var phase = BossPhase.Create(1, "Test", 100, BossBehavior.Tactical);

        // Act
        phase.WithAbilities("bone-strike", "raise-dead", "death-command");

        // Assert
        phase.AbilityIds.Should().HaveCount(3);
        phase.AbilityIds.Should().Contain("bone-strike");
        phase.AbilityIds.Should().Contain("raise-dead");
        phase.AbilityIds.Should().Contain("death-command");
    }

    [Test]
    public void WithAbilities_FiltersEmptyStrings()
    {
        // Arrange
        var phase = BossPhase.Create(1, "Test", 100, BossBehavior.Tactical);

        // Act
        phase.WithAbilities("bone-strike", "", "raise-dead", null!, "  ");

        // Assert
        phase.AbilityIds.Should().HaveCount(2);
        phase.AbilityIds.Should().Contain("bone-strike");
        phase.AbilityIds.Should().Contain("raise-dead");
    }

    [Test]
    public void WithStatModifier_AddsModifier()
    {
        // Arrange
        var phase = BossPhase.Create(1, "Test", 100, BossBehavior.Tactical);
        var modifier = StatModifier.Percentage("attack", 0.5f);

        // Act
        phase.WithStatModifier(modifier);

        // Assert
        phase.StatModifiers.Should().HaveCount(1);
        phase.StatModifiers[0].StatId.Should().Be("attack");
        phase.StatModifiers[0].Value.Should().Be(0.5f);
    }

    [Test]
    public void WithTransitionText_SetsText()
    {
        // Arrange
        var phase = BossPhase.Create(1, "Test", 100, BossBehavior.Tactical);

        // Act
        phase.WithTransitionText("The boss awakens!");

        // Assert
        phase.TransitionText.Should().Be("The boss awakens!");
    }

    [Test]
    public void WithTransitionEffect_SetsEffectId()
    {
        // Arrange
        var phase = BossPhase.Create(1, "Test", 100, BossBehavior.Tactical);

        // Act
        phase.WithTransitionEffect("boss-enrage-aura");

        // Assert
        phase.TransitionEffectId.Should().Be("boss-enrage-aura");
    }

    [Test]
    public void WithSummonConfig_SetsSummonConfiguration()
    {
        // Arrange
        var phase = BossPhase.Create(1, "Test", 100, BossBehavior.Summoner);
        var summonConfig = SummonConfiguration.Create("skeleton-minion", count: 2)
            .WithIntervalTurns(3)
            .WithMaxActive(6);

        // Act
        phase.WithSummonConfig(summonConfig);

        // Assert
        phase.SummonConfig.Should().Be(summonConfig);
        phase.HasSummoning.Should().BeTrue();
        phase.SummonConfig.MonsterDefinitionId.Should().Be("skeleton-minion");
        phase.SummonConfig.Count.Should().Be(2);
        phase.SummonConfig.IntervalTurns.Should().Be(3);
        phase.SummonConfig.MaxActive.Should().Be(6);
    }

    // ═══════════════════════════════════════════════════════════════
    // VALIDATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void IsValid_ReturnsTrue_ForValidPhase()
    {
        // Arrange
        var phase = BossPhase.Create(1, "Awakened", 100, BossBehavior.Tactical)
            .WithAbilities("bone-strike")
            .WithTransitionText("The boss awakens!");

        // Assert
        phase.IsValid.Should().BeTrue();
    }

    [Test]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var phase = BossPhase.Create(2, "Commanding", 60, BossBehavior.Summoner);

        // Act
        var result = phase.ToString();

        // Assert
        result.Should().Contain("Phase 2");
        result.Should().Contain("Commanding");
        result.Should().Contain("60%");
    }

    // ═══════════════════════════════════════════════════════════════
    // BEHAVIOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(BossBehavior.Aggressive)]
    [TestCase(BossBehavior.Tactical)]
    [TestCase(BossBehavior.Defensive)]
    [TestCase(BossBehavior.Enraged)]
    [TestCase(BossBehavior.Summoner)]
    public void Create_WithAllBehaviorTypes_Succeeds(BossBehavior behavior)
    {
        // Arrange & Act
        var phase = BossPhase.Create(1, "Test", 100, behavior);

        // Assert
        phase.Behavior.Should().Be(behavior);
    }
}
