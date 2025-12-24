using FluentAssertions;
using RuneAndRust.Core.Models.Analysis;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Unit tests for the CombatStatistics accumulator model.
/// </summary>
public class CombatStatisticsTests
{
    #region RecordEncounterResult Tests

    [Fact]
    public void RecordEncounterResult_WithPlayerWin_IncrementsPlayerWins()
    {
        // Arrange
        var stats = new CombatStatistics();
        var result = new CombatMatchResult(
            PlayerWon: true,
            RoundsElapsed: 5,
            PlayerHPRemaining: 50,
            EnemyHPRemaining: 0,
            StaminaSpent: 30);

        // Act
        stats.RecordEncounterResult(result);

        // Assert
        stats.TotalEncounters.Should().Be(1);
        stats.PlayerWins.Should().Be(1);
        stats.EnemyWins.Should().Be(0);
        stats.TotalPlayerHPRemaining.Should().Be(50);
    }

    [Fact]
    public void RecordEncounterResult_WithEnemyWin_IncrementsEnemyWins()
    {
        // Arrange
        var stats = new CombatStatistics();
        var result = new CombatMatchResult(
            PlayerWon: false,
            RoundsElapsed: 3,
            PlayerHPRemaining: 0,
            EnemyHPRemaining: 20,
            StaminaSpent: 40);

        // Act
        stats.RecordEncounterResult(result);

        // Assert
        stats.TotalEncounters.Should().Be(1);
        stats.PlayerWins.Should().Be(0);
        stats.EnemyWins.Should().Be(1);
        stats.TotalPlayerHPRemaining.Should().Be(0); // HP not added for losses
    }

    [Fact]
    public void RecordEncounterResult_TracksRoundsAndStamina()
    {
        // Arrange
        var stats = new CombatStatistics();
        var result1 = new CombatMatchResult(true, 4, 60, 0, 25);
        var result2 = new CombatMatchResult(true, 6, 40, 0, 35);

        // Act
        stats.RecordEncounterResult(result1);
        stats.RecordEncounterResult(result2);

        // Assert
        stats.TotalRounds.Should().Be(10);
        stats.TotalPlayerStaminaSpent.Should().Be(60);
    }

    [Fact]
    public void RecordEncounterResult_AccumulatesMultipleResults()
    {
        // Arrange
        var stats = new CombatStatistics();

        // 3 wins, 2 losses
        stats.RecordEncounterResult(new CombatMatchResult(true, 5, 50, 0, 30));
        stats.RecordEncounterResult(new CombatMatchResult(false, 3, 0, 25, 20));
        stats.RecordEncounterResult(new CombatMatchResult(true, 4, 60, 0, 25));
        stats.RecordEncounterResult(new CombatMatchResult(false, 6, 0, 10, 45));
        stats.RecordEncounterResult(new CombatMatchResult(true, 7, 30, 0, 40));

        // Assert
        stats.TotalEncounters.Should().Be(5);
        stats.PlayerWins.Should().Be(3);
        stats.EnemyWins.Should().Be(2);
        stats.TotalRounds.Should().Be(25);
        stats.TotalPlayerStaminaSpent.Should().Be(160);
        stats.TotalPlayerHPRemaining.Should().Be(140); // 50 + 60 + 30 from wins only
    }

    #endregion

    #region RecordPlayerAttack Tests

    [Fact]
    public void RecordPlayerAttack_Hit_TracksDamageAndHits()
    {
        // Arrange
        var stats = new CombatStatistics();

        // Act
        stats.RecordPlayerAttack(true, 15);
        stats.RecordPlayerAttack(true, 20);

        // Assert
        stats.TotalPlayerHits.Should().Be(2);
        stats.TotalPlayerMisses.Should().Be(0);
        stats.TotalPlayerDamageDealt.Should().Be(35);
        stats.TotalPlayerTurns.Should().Be(2);
    }

    [Fact]
    public void RecordPlayerAttack_Miss_TracksOnlyMisses()
    {
        // Arrange
        var stats = new CombatStatistics();

        // Act
        stats.RecordPlayerAttack(false, 0);
        stats.RecordPlayerAttack(false, 0);

        // Assert
        stats.TotalPlayerHits.Should().Be(0);
        stats.TotalPlayerMisses.Should().Be(2);
        stats.TotalPlayerDamageDealt.Should().Be(0);
        stats.TotalPlayerTurns.Should().Be(2);
    }

    [Fact]
    public void RecordPlayerAttack_MixedHitsAndMisses_TracksCorrectly()
    {
        // Arrange
        var stats = new CombatStatistics();

        // Act - 3 hits (10, 15, 20), 2 misses
        stats.RecordPlayerAttack(true, 10);
        stats.RecordPlayerAttack(false, 0);
        stats.RecordPlayerAttack(true, 15);
        stats.RecordPlayerAttack(false, 0);
        stats.RecordPlayerAttack(true, 20);

        // Assert
        stats.TotalPlayerHits.Should().Be(3);
        stats.TotalPlayerMisses.Should().Be(2);
        stats.TotalPlayerDamageDealt.Should().Be(45);
        stats.TotalPlayerTurns.Should().Be(5);
    }

    #endregion

    #region RecordEnemyAttack Tests

    [Fact]
    public void RecordEnemyAttack_Hit_TracksDamageAndPlayerDamageReceived()
    {
        // Arrange
        var stats = new CombatStatistics();

        // Act
        stats.RecordEnemyAttack(true, 12);
        stats.RecordEnemyAttack(true, 8);

        // Assert
        stats.TotalEnemyHits.Should().Be(2);
        stats.TotalEnemyMisses.Should().Be(0);
        stats.TotalEnemyDamageDealt.Should().Be(20);
        stats.TotalPlayerDamageReceived.Should().Be(20);
        stats.TotalEnemyTurns.Should().Be(2);
    }

    [Fact]
    public void RecordEnemyAttack_Miss_TracksOnlyMisses()
    {
        // Arrange
        var stats = new CombatStatistics();

        // Act
        stats.RecordEnemyAttack(false, 0);

        // Assert
        stats.TotalEnemyHits.Should().Be(0);
        stats.TotalEnemyMisses.Should().Be(1);
        stats.TotalEnemyDamageDealt.Should().Be(0);
        stats.TotalPlayerDamageReceived.Should().Be(0);
    }

    #endregion

    #region Derived Metrics Tests

    [Fact]
    public void WinRate_CalculatesCorrectly()
    {
        // Arrange
        var stats = new CombatStatistics();

        // 8 wins, 2 losses = 80% win rate
        for (int i = 0; i < 8; i++)
            stats.RecordEncounterResult(new CombatMatchResult(true, 5, 50, 0, 30));
        for (int i = 0; i < 2; i++)
            stats.RecordEncounterResult(new CombatMatchResult(false, 3, 0, 20, 25));

        // Assert
        stats.WinRate.Should().BeApproximately(80.0, 0.01);
    }

    [Fact]
    public void WinRate_ReturnsZeroWhenNoEncounters()
    {
        // Arrange
        var stats = new CombatStatistics();

        // Assert
        stats.WinRate.Should().Be(0);
    }

    [Fact]
    public void AvgRoundsPerEncounter_CalculatesCorrectly()
    {
        // Arrange
        var stats = new CombatStatistics();

        // 4 encounters with 3, 5, 4, 8 rounds = 20 total / 4 = 5.0 avg
        stats.RecordEncounterResult(new CombatMatchResult(true, 3, 50, 0, 20));
        stats.RecordEncounterResult(new CombatMatchResult(true, 5, 40, 0, 25));
        stats.RecordEncounterResult(new CombatMatchResult(false, 4, 0, 10, 30));
        stats.RecordEncounterResult(new CombatMatchResult(true, 8, 30, 0, 40));

        // Assert
        stats.AvgRoundsPerEncounter.Should().BeApproximately(5.0, 0.01);
    }

    [Fact]
    public void PlayerHitRate_CalculatesCorrectly()
    {
        // Arrange
        var stats = new CombatStatistics();

        // 7 hits, 3 misses = 70% hit rate
        for (int i = 0; i < 7; i++)
            stats.RecordPlayerAttack(true, 10);
        for (int i = 0; i < 3; i++)
            stats.RecordPlayerAttack(false, 0);

        // Assert
        stats.PlayerHitRate.Should().BeApproximately(70.0, 0.01);
    }

    [Fact]
    public void PlayerHitRate_ReturnsZeroWhenNoAttacks()
    {
        // Arrange
        var stats = new CombatStatistics();

        // Assert
        stats.PlayerHitRate.Should().Be(0);
    }

    [Fact]
    public void EnemyHitRate_CalculatesCorrectly()
    {
        // Arrange
        var stats = new CombatStatistics();

        // 6 hits, 4 misses = 60% hit rate
        for (int i = 0; i < 6; i++)
            stats.RecordEnemyAttack(true, 8);
        for (int i = 0; i < 4; i++)
            stats.RecordEnemyAttack(false, 0);

        // Assert
        stats.EnemyHitRate.Should().BeApproximately(60.0, 0.01);
    }

    [Fact]
    public void AvgPlayerDamagePerHit_CalculatesCorrectly()
    {
        // Arrange
        var stats = new CombatStatistics();

        // Hits with damage: 10, 15, 20, 15 = 60 total / 4 hits = 15 avg
        stats.RecordPlayerAttack(true, 10);
        stats.RecordPlayerAttack(true, 15);
        stats.RecordPlayerAttack(false, 0); // Miss, doesn't count
        stats.RecordPlayerAttack(true, 20);
        stats.RecordPlayerAttack(true, 15);

        // Assert
        stats.AvgPlayerDamagePerHit.Should().BeApproximately(15.0, 0.01);
    }

    [Fact]
    public void AvgPlayerDamagePerHit_ReturnsZeroWhenNoHits()
    {
        // Arrange
        var stats = new CombatStatistics();
        stats.RecordPlayerAttack(false, 0);
        stats.RecordPlayerAttack(false, 0);

        // Assert
        stats.AvgPlayerDamagePerHit.Should().Be(0);
    }

    [Fact]
    public void AvgEnemyDamagePerHit_CalculatesCorrectly()
    {
        // Arrange
        var stats = new CombatStatistics();

        // Hits with damage: 8, 12, 10 = 30 total / 3 hits = 10 avg
        stats.RecordEnemyAttack(true, 8);
        stats.RecordEnemyAttack(true, 12);
        stats.RecordEnemyAttack(true, 10);

        // Assert
        stats.AvgEnemyDamagePerHit.Should().BeApproximately(10.0, 0.01);
    }

    [Fact]
    public void AvgStaminaPerEncounter_CalculatesCorrectly()
    {
        // Arrange
        var stats = new CombatStatistics();

        // 3 encounters with 20, 30, 40 stamina = 90 total / 3 = 30 avg
        stats.RecordEncounterResult(new CombatMatchResult(true, 4, 50, 0, 20));
        stats.RecordEncounterResult(new CombatMatchResult(true, 5, 45, 0, 30));
        stats.RecordEncounterResult(new CombatMatchResult(false, 3, 0, 15, 40));

        // Assert
        stats.AvgStaminaPerEncounter.Should().BeApproximately(30.0, 0.01);
    }

    [Fact]
    public void AvgHPRemainingOnWin_CalculatesCorrectly()
    {
        // Arrange
        var stats = new CombatStatistics();

        // 3 wins with HP remaining: 60, 40, 50 = 150 total / 3 = 50 avg
        stats.RecordEncounterResult(new CombatMatchResult(true, 4, 60, 0, 20));
        stats.RecordEncounterResult(new CombatMatchResult(false, 3, 0, 20, 25)); // Loss - doesn't count
        stats.RecordEncounterResult(new CombatMatchResult(true, 5, 40, 0, 30));
        stats.RecordEncounterResult(new CombatMatchResult(true, 6, 50, 0, 35));

        // Assert
        stats.AvgHPRemainingOnWin.Should().BeApproximately(50.0, 0.01);
    }

    [Fact]
    public void AvgHPRemainingOnWin_ReturnsZeroWhenNoWins()
    {
        // Arrange
        var stats = new CombatStatistics();
        stats.RecordEncounterResult(new CombatMatchResult(false, 3, 0, 20, 25));
        stats.RecordEncounterResult(new CombatMatchResult(false, 4, 0, 15, 30));

        // Assert
        stats.AvgHPRemainingOnWin.Should().Be(0);
    }

    [Fact]
    public void AllDerivedMetrics_ReturnZeroWhenEmpty()
    {
        // Arrange
        var stats = new CombatStatistics();

        // Assert
        stats.WinRate.Should().Be(0);
        stats.AvgRoundsPerEncounter.Should().Be(0);
        stats.PlayerHitRate.Should().Be(0);
        stats.EnemyHitRate.Should().Be(0);
        stats.AvgPlayerDamagePerHit.Should().Be(0);
        stats.AvgEnemyDamagePerHit.Should().Be(0);
        stats.AvgStaminaPerEncounter.Should().Be(0);
        stats.AvgHPRemainingOnWin.Should().Be(0);
    }

    #endregion

    #region CombatMatchResult Record Tests

    [Fact]
    public void CombatMatchResult_RecordEquality_WorksCorrectly()
    {
        // Arrange
        var result1 = new CombatMatchResult(true, 5, 50, 0, 30);
        var result2 = new CombatMatchResult(true, 5, 50, 0, 30);
        var result3 = new CombatMatchResult(false, 5, 50, 0, 30);

        // Assert
        result1.Should().Be(result2);
        result1.Should().NotBe(result3);
    }

    [Fact]
    public void CombatMatchResult_AllPropertiesAccessible()
    {
        // Arrange
        var result = new CombatMatchResult(
            PlayerWon: true,
            RoundsElapsed: 7,
            PlayerHPRemaining: 45,
            EnemyHPRemaining: 0,
            StaminaSpent: 35);

        // Assert
        result.PlayerWon.Should().BeTrue();
        result.RoundsElapsed.Should().Be(7);
        result.PlayerHPRemaining.Should().Be(45);
        result.EnemyHPRemaining.Should().Be(0);
        result.StaminaSpent.Should().Be(35);
    }

    #endregion
}
