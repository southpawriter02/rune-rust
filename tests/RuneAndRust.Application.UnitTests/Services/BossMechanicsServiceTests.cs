using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Application.Tracking;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for BossMechanicsService (v0.10.4b).
/// </summary>
/// <remarks>
/// <para>
/// Tests cover the complete boss encounter lifecycle:
/// <list type="bullet">
///   <item><description>Boss spawning from definitions</description></item>
///   <item><description>Phase transition detection and execution</description></item>
///   <item><description>Vulnerability window management</description></item>
///   <item><description>Per-turn state updates</description></item>
///   <item><description>Boss query methods</description></item>
///   <item><description>Minion tracking</description></item>
/// </list>
/// </para>
/// </remarks>
[TestFixture]
public class BossMechanicsServiceTests
{
    // ═══════════════════════════════════════════════════════════════
    // TEST FIXTURES
    // ═══════════════════════════════════════════════════════════════

    private Mock<IBossProvider> _mockBossProvider = null!;
    private Mock<IGameEventLogger> _mockEventLogger = null!;
    private Mock<ILogger<BossMechanicsService>> _mockLogger = null!;
    private BossMechanicsService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _mockBossProvider = new Mock<IBossProvider>();
        _mockEventLogger = new Mock<IGameEventLogger>();
        _mockLogger = new Mock<ILogger<BossMechanicsService>>();

        _service = new BossMechanicsService(
            _mockBossProvider.Object,
            _mockEventLogger.Object,
            _mockLogger.Object);
    }

    // ═══════════════════════════════════════════════════════════════
    // SPAWN BOSS TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void SpawnBoss_ValidBossId_CreatesBossAndState()
    {
        // Arrange
        var bossDef = CreateTestBossDefinition();
        _mockBossProvider.Setup(p => p.GetBoss("skeleton-king")).Returns(bossDef);

        // Act
        var boss = _service.SpawnBoss("skeleton-king", new GridPosition(5, 5));

        // Assert
        boss.Should().NotBeNull();
        boss.Name.Should().Be("The Skeleton King");
        boss.IsAlive.Should().BeTrue();

        var state = _service.GetBossState(boss);
        state.Should().NotBeNull();
        state!.BossId.Should().Be("skeleton-king");
        state.CurrentPhaseNumber.Should().Be(1);

        _mockEventLogger.Verify(
            e => e.LogCombat(
                "BossSpawned",
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<Dictionary<string, object>?>()),
            Times.Once);
    }

    [Test]
    public void SpawnBoss_InvalidBossId_ThrowsArgumentException()
    {
        // Arrange
        _mockBossProvider.Setup(p => p.GetBoss("invalid-boss")).Returns((BossDefinition?)null);

        // Act
        var act = () => _service.SpawnBoss("invalid-boss", new GridPosition(0, 0));

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*invalid-boss*")
            .WithParameterName("bossId");
    }

    // ═══════════════════════════════════════════════════════════════
    // PHASE TRANSITION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void OnBossDamaged_TriggersPhaseTransition_WhenHealthBelowThreshold()
    {
        // Arrange
        var bossDef = CreateTestBossDefinition();
        _mockBossProvider.Setup(p => p.GetBoss("skeleton-king")).Returns(bossDef);

        var boss = _service.SpawnBoss("skeleton-king", new GridPosition(5, 5));

        // Simulate damage bringing health to 55% (below 60% threshold for phase 2)
        var damageAmount = (int)(boss.MaxHealth * 0.45);
        boss.TakeDamage(damageAmount);

        // Act
        _service.OnBossDamaged(boss, damageAmount);

        // Assert
        var state = _service.GetBossState(boss);
        state!.CurrentPhaseNumber.Should().Be(2);

        _mockEventLogger.Verify(
            e => e.LogCombat(
                "BossPhaseChanged",
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<Dictionary<string, object>?>()),
            Times.Once);
    }

    [Test]
    public void OnBossDamaged_NoTransition_WhenSamePhase()
    {
        // Arrange
        var bossDef = CreateTestBossDefinition();
        _mockBossProvider.Setup(p => p.GetBoss("skeleton-king")).Returns(bossDef);

        var boss = _service.SpawnBoss("skeleton-king", new GridPosition(5, 5));

        // Simulate small damage that keeps health above 60% (phase 1 threshold)
        var smallDamage = (int)(boss.MaxHealth * 0.10);
        boss.TakeDamage(smallDamage);

        // Act
        _service.OnBossDamaged(boss, smallDamage);

        // Assert
        var state = _service.GetBossState(boss);
        state!.CurrentPhaseNumber.Should().Be(1);

        // BossPhaseChanged should not be logged (only BossSpawned from spawn)
        _mockEventLogger.Verify(
            e => e.LogCombat(
                "BossPhaseChanged",
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<Dictionary<string, object>?>()),
            Times.Never);
    }

    // ═══════════════════════════════════════════════════════════════
    // PHASE QUERY TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetCurrentPhase_ReturnsCorrectPhase()
    {
        // Arrange
        var bossDef = CreateTestBossDefinition();
        _mockBossProvider.Setup(p => p.GetBoss("skeleton-king")).Returns(bossDef);

        var boss = _service.SpawnBoss("skeleton-king", new GridPosition(5, 5));

        // Act
        var phase = _service.GetCurrentPhase(boss);

        // Assert
        phase.Should().NotBeNull();
        phase!.PhaseNumber.Should().Be(1);
        phase.Name.Should().Be("Awakened");
    }

    // ═══════════════════════════════════════════════════════════════
    // VULNERABILITY TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void IsVulnerable_ReturnsTrue_WhenVulnerableTurnsGreaterThanZero()
    {
        // Arrange
        var bossDef = CreateTestBossDefinition();
        _mockBossProvider.Setup(p => p.GetBoss("skeleton-king")).Returns(bossDef);

        var boss = _service.SpawnBoss("skeleton-king", new GridPosition(5, 5));
        _service.SetVulnerable(boss, 3);

        // Act
        var result = _service.IsVulnerable(boss);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void GetVulnerabilityMultiplier_Returns1_5_WhenVulnerable()
    {
        // Arrange
        var bossDef = CreateTestBossDefinition();
        _mockBossProvider.Setup(p => p.GetBoss("skeleton-king")).Returns(bossDef);

        var boss = _service.SpawnBoss("skeleton-king", new GridPosition(5, 5));
        _service.SetVulnerable(boss, 2);

        // Act
        var multiplier = _service.GetVulnerabilityMultiplier(boss);

        // Assert
        multiplier.Should().Be(1.5f);
    }

    [Test]
    public void SetVulnerable_SetsVulnerableTurns()
    {
        // Arrange
        var bossDef = CreateTestBossDefinition();
        _mockBossProvider.Setup(p => p.GetBoss("skeleton-king")).Returns(bossDef);

        var boss = _service.SpawnBoss("skeleton-king", new GridPosition(5, 5));

        // Act
        _service.SetVulnerable(boss, 5);

        // Assert
        var state = _service.GetBossState(boss);
        state!.VulnerableTurns.Should().Be(5);

        _mockEventLogger.Verify(
            e => e.LogCombat(
                "BossVulnerable",
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<Dictionary<string, object>?>()),
            Times.Once);
    }

    // ═══════════════════════════════════════════════════════════════
    // TICK BOSS TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void TickBoss_DecrementsVulnerability()
    {
        // Arrange
        var bossDef = CreateTestBossDefinition();
        _mockBossProvider.Setup(p => p.GetBoss("skeleton-king")).Returns(bossDef);

        var boss = _service.SpawnBoss("skeleton-king", new GridPosition(5, 5));
        _service.SetVulnerable(boss, 3);

        // Act
        _service.TickBoss(boss);

        // Assert
        var state = _service.GetBossState(boss);
        state!.VulnerableTurns.Should().Be(2);
    }

    [Test]
    public void TickBoss_FiresVulnerabilityEndedEvent_WhenCountdownReachesZero()
    {
        // Arrange
        var bossDef = CreateTestBossDefinition();
        _mockBossProvider.Setup(p => p.GetBoss("skeleton-king")).Returns(bossDef);

        var boss = _service.SpawnBoss("skeleton-king", new GridPosition(5, 5));
        _service.SetVulnerable(boss, 1);

        // Act
        _service.TickBoss(boss);

        // Assert
        var state = _service.GetBossState(boss);
        state!.VulnerableTurns.Should().Be(0);
        state.IsVulnerable.Should().BeFalse();

        _mockEventLogger.Verify(
            e => e.LogCombat(
                "BossVulnerabilityEnded",
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<Dictionary<string, object>?>()),
            Times.Once);
    }

    // ═══════════════════════════════════════════════════════════════
    // BOSS QUERY TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void IsBoss_ReturnsTrue_ForTrackedBoss()
    {
        // Arrange
        var bossDef = CreateTestBossDefinition();
        _mockBossProvider.Setup(p => p.GetBoss("skeleton-king")).Returns(bossDef);

        var boss = _service.SpawnBoss("skeleton-king", new GridPosition(5, 5));

        // Act
        var result = _service.IsBoss(boss);

        // Assert
        result.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // MINION TRACKING TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void OnMinionDeath_RemovesFromTracking()
    {
        // Arrange
        var bossDef = CreateTestBossDefinition();
        _mockBossProvider.Setup(p => p.GetBoss("skeleton-king")).Returns(bossDef);

        var boss = _service.SpawnBoss("skeleton-king", new GridPosition(5, 5));
        var minionId = Guid.NewGuid();

        // Manually add a minion to tracking
        var state = _service.GetBossState(boss);
        state!.AddSummonedMinion(minionId);
        state.ActiveSummonCount.Should().Be(1);

        // Act
        _service.OnMinionDeath(boss, minionId);

        // Assert
        state.ActiveSummonCount.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a standard three-phase boss definition for testing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The test boss has three phases:
    /// <list type="bullet">
    ///   <item><description>Phase 1 (Awakened): 100% threshold, Tactical behavior</description></item>
    ///   <item><description>Phase 2 (Commanding): 60% threshold, Summoner behavior</description></item>
    ///   <item><description>Phase 3 (Enraged): 25% threshold, Enraged behavior</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <returns>A test boss definition with three phases.</returns>
    private static BossDefinition CreateTestBossDefinition()
    {
        return BossDefinition.Create(
                "skeleton-king",
                "The Skeleton King",
                "An ancient ruler risen from death",
                "skeleton-elite")
            .WithTitleText("Lord of the Undead Crypt")
            .WithPhase(BossPhase.Create(1, "Awakened", 100, BossBehavior.Tactical)
                .WithAbilities("bone-sweep", "summon-skeleton"))
            .WithPhase(BossPhase.Create(2, "Commanding", 60, BossBehavior.Summoner)
                .WithAbilities("mass-summon", "soul-drain")
                .WithTransitionText("The Skeleton King raises his arms!"))
            .WithPhase(BossPhase.Create(3, "Enraged", 25, BossBehavior.Enraged)
                .WithAbilities("devastating-slam", "death-nova")
                .WithTransitionEffect("boss-enrage-aura")
                .WithTransitionText("The Skeleton King enters a furious rage!"))
            .WithLoot(BossLootEntry.Guaranteed("gold", 500))
            .WithLoot(BossLootEntry.Create("crown-of-bones", 0.25));
    }
}
