// ═══════════════════════════════════════════════════════════════════════════════
// StatisticsServiceTests.cs
// Unit tests for the StatisticsService.
// Version: 0.12.0a
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Models;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for the <see cref="StatisticsService"/>.
/// </summary>
/// <remarks>
/// <para>Tests cover:</para>
/// <list type="bullet">
///   <item><description>Constructor validation</description></item>
///   <item><description>IncrementStat functionality</description></item>
///   <item><description>Monster kill recording</description></item>
///   <item><description>Damage recording</description></item>
///   <item><description>Room discovery tracking</description></item>
///   <item><description>Category statistics grouping</description></item>
///   <item><description>Metrics calculation including combat rating</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class StatisticsServiceTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Fields
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Mock logger for the service.
    /// </summary>
    private Mock<ILogger<StatisticsService>> _mockLogger = null!;

    /// <summary>
    /// The service under test.
    /// </summary>
    private StatisticsService _service = null!;

    /// <summary>
    /// Test player instance.
    /// </summary>
    private Player _player = null!;

    // ═══════════════════════════════════════════════════════════════════════════
    // Setup
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Sets up the test fixtures before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        // Initialize mocks
        _mockLogger = new Mock<ILogger<StatisticsService>>();

        // Create the service under test
        _service = new StatisticsService(_mockLogger.Object);

        // Create a test player
        _player = new Player("TestPlayer");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Constructor Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the constructor throws when logger is null.
    /// </summary>
    [Test]
    public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new StatisticsService(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // IncrementStat Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that IncrementStat increments the correct statistic.
    /// </summary>
    [Test]
    public void IncrementStat_ValidStat_IncrementsCorrectly()
    {
        // Act
        _service.IncrementStat(_player, "monstersKilled");

        // Assert
        var stats = _service.GetPlayerStatistics(_player);
        stats.MonstersKilled.Should().Be(1);
    }

    /// <summary>
    /// Verifies that IncrementStat correctly handles amount parameter.
    /// </summary>
    [Test]
    public void IncrementStat_WithAmount_IncrementsCorrectly()
    {
        // Act
        _service.IncrementStat(_player, "damageDealt", 100);

        // Assert
        var stats = _service.GetPlayerStatistics(_player);
        stats.TotalDamageDealt.Should().Be(100);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RecordMonsterKill Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that RecordMonsterKill updates kills and tracks type.
    /// </summary>
    [Test]
    public void RecordMonsterKill_UpdatesKillsAndTracksType()
    {
        // Act
        _service.RecordMonsterKill(_player, "Goblin");

        // Assert
        var stats = _service.GetPlayerStatistics(_player);
        stats.MonstersKilled.Should().Be(1);
        stats.MonstersByType.Should().ContainKey("goblin");
        stats.MonstersByType["goblin"].Should().Be(1);
    }

    /// <summary>
    /// Verifies that RecordMonsterKill with boss flag increments BossesKilled.
    /// </summary>
    [Test]
    public void RecordMonsterKill_WithBoss_IncrementsBossesKilled()
    {
        // Act
        _service.RecordMonsterKill(_player, "Dragon Lord", isBoss: true);

        // Assert
        var stats = _service.GetPlayerStatistics(_player);
        stats.MonstersKilled.Should().Be(1);
        stats.BossesKilled.Should().Be(1);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RecordDamageDealt Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that RecordDamageDealt updates damage and total attacks.
    /// </summary>
    [Test]
    public void RecordDamageDealt_UpdatesDamageAndTotalAttacks()
    {
        // Act
        _service.RecordDamageDealt(_player, 50, wasCritical: false);

        // Assert
        var stats = _service.GetPlayerStatistics(_player);
        stats.TotalDamageDealt.Should().Be(50);
        stats.TotalAttacks.Should().Be(1);
    }

    /// <summary>
    /// Verifies that RecordDamageDealt with critical flag increments CriticalHits.
    /// </summary>
    [Test]
    public void RecordDamageDealt_WithCritical_IncrementsCriticalHits()
    {
        // Act
        _service.RecordDamageDealt(_player, 100, wasCritical: true);

        // Assert
        var stats = _service.GetPlayerStatistics(_player);
        stats.CriticalHits.Should().Be(1);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RecordRoomDiscovered Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that RecordRoomDiscovered increments the rooms discovered counter.
    /// </summary>
    [Test]
    public void RecordRoomDiscovered_IncrementsRoomsDiscovered()
    {
        // Act
        _service.RecordRoomDiscovered(_player);

        // Assert
        var stats = _service.GetPlayerStatistics(_player);
        stats.RoomsDiscovered.Should().Be(1);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetCategoryStatistics Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetCategoryStatistics returns correct statistics for combat category.
    /// </summary>
    [Test]
    public void GetCategoryStatistics_Combat_ReturnsCorrectStats()
    {
        // Arrange - Record some combat statistics
        _service.RecordMonsterKill(_player, "Goblin");
        _service.RecordDamageDealt(_player, 50, wasCritical: true);

        // Act
        var combatStats = _service.GetCategoryStatistics(_player, StatisticCategory.Combat);

        // Assert
        combatStats.Should().ContainKey("monstersKilled");
        combatStats["monstersKilled"].Should().Be(1);
        combatStats.Should().ContainKey("damageDealt");
        combatStats["damageDealt"].Should().Be(50);
        combatStats.Should().ContainKey("criticalHits");
        combatStats["criticalHits"].Should().Be(1);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetMetrics Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetMetrics calculates averages correctly.
    /// </summary>
    [Test]
    public void GetMetrics_CalculatesAveragesCorrectly()
    {
        // Arrange - Record some statistics for metric calculations
        _service.RecordDamageDealt(_player, 100, wasCritical: false);
        _service.RecordDamageDealt(_player, 100, wasCritical: true);
        // 2 attacks, 1 critical, 200 total damage, 2 hits (no misses)

        // Act
        var metrics = _service.GetMetrics(_player);

        // Assert
        // Average damage per hit = 200 / 2 = 100
        metrics.AverageDamagePerHit.Should().Be(100);
        // Critical hit rate = 1 / 2 = 0.5
        metrics.CriticalHitRate.Should().Be(0.5);
        // Miss rate = 0 / 2 = 0
        metrics.MissRate.Should().Be(0);
    }

    /// <summary>
    /// Verifies that GetMetrics returns zero rates when no attacks have been made.
    /// </summary>
    [Test]
    public void GetMetrics_WithNoAttacks_ReturnsZeroRates()
    {
        // Act - Fresh player with no combat activity
        var metrics = _service.GetMetrics(_player);

        // Assert
        metrics.AverageDamagePerHit.Should().Be(0);
        metrics.CriticalHitRate.Should().Be(0);
        metrics.MissRate.Should().Be(0);
        metrics.CombatRating.Should().Be(CombatRating.Novice);
    }

    /// <summary>
    /// Verifies that combat rating is calculated correctly for various scenarios.
    /// </summary>
    [Test]
    [TestCase(10, 0, 0, 0, ExpectedResult = CombatRating.Journeyman)]  // 10 kills, 0 deaths = K/D 10, 40 pts
    [TestCase(20, 1, 5, 0, ExpectedResult = CombatRating.Skilled)]     // 20 kills, 1 death = K/D 20 (40 pts) + 5 crit/10 = 100 pts crit (10 pts)
    [TestCase(10, 0, 1, 2, ExpectedResult = CombatRating.Skilled)]     // 10 kills = 40pts + 10% crit = 20pts + 2 bosses = 10pts = ~50 pts
    public CombatRating GetMetrics_CalculateCombatRating_ReturnsCorrectRating(
        int monstersKilled,
        int deaths,
        int criticalHits,
        int bossesKilled)
    {
        // Arrange
        for (int i = 0; i < monstersKilled; i++)
        {
            _service.RecordMonsterKill(_player, "Monster");
        }

        var stats = _service.GetPlayerStatistics(_player);

        // Simulate deaths by directly incrementing
        for (int i = 0; i < deaths; i++)
        {
            stats.IncrementStat("deathCount");
        }

        // Record attacks with criticals
        int totalAttacks = 10;
        for (int i = 0; i < totalAttacks; i++)
        {
            bool isCrit = i < criticalHits;
            _service.RecordDamageDealt(_player, 10, wasCritical: isCrit);
        }

        // Record boss kills
        for (int i = 0; i < bossesKilled; i++)
        {
            _service.RecordMonsterKill(_player, "Boss", isBoss: true);
        }

        // Act
        var metrics = _service.GetMetrics(_player);

        // Assert - Return the rating for comparison
        return metrics.CombatRating;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PlayerStatistics Initialization Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetPlayerStatistics creates statistics if not exists.
    /// </summary>
    [Test]
    public void GetPlayerStatistics_WhenNoStatistics_CreatesNewStatistics()
    {
        // Act
        var stats = _service.GetPlayerStatistics(_player);

        // Assert
        stats.Should().NotBeNull();
        stats.PlayerId.Should().Be(_player.Id);
        stats.MonstersKilled.Should().Be(0);
    }

    /// <summary>
    /// Verifies that GetPlayerStatistics returns same instance on subsequent calls.
    /// </summary>
    [Test]
    public void GetPlayerStatistics_OnSubsequentCalls_ReturnsSameInstance()
    {
        // Act
        var stats1 = _service.GetPlayerStatistics(_player);
        _service.IncrementStat(_player, "monstersKilled");
        var stats2 = _service.GetPlayerStatistics(_player);

        // Assert - Should be same instance
        stats1.Should().BeSameAs(stats2);
        stats2.MonstersKilled.Should().Be(1);
    }
}
