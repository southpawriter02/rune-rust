namespace RuneAndRust.Core.Models.Analysis;

/// <summary>
/// Accumulates statistics from multiple combat encounters for Monte Carlo analysis.
/// Used by the CombatAuditService to track win rates, damage, and resource consumption.
/// </summary>
public class CombatStatistics
{
    #region Aggregate Counts

    /// <summary>
    /// Gets the total number of combat encounters simulated.
    /// </summary>
    public int TotalEncounters { get; private set; }

    /// <summary>
    /// Gets the number of encounters won by the player.
    /// </summary>
    public int PlayerWins { get; private set; }

    /// <summary>
    /// Gets the number of encounters won by the enemy.
    /// </summary>
    public int EnemyWins { get; private set; }

    #endregion

    #region Round/Turn Tracking

    /// <summary>
    /// Gets the total number of combat rounds across all encounters.
    /// </summary>
    public int TotalRounds { get; private set; }

    /// <summary>
    /// Gets the total number of player turns taken.
    /// </summary>
    public int TotalPlayerTurns { get; private set; }

    /// <summary>
    /// Gets the total number of enemy turns taken.
    /// </summary>
    public int TotalEnemyTurns { get; private set; }

    #endregion

    #region Damage Tracking

    /// <summary>
    /// Gets the cumulative damage dealt by the player across all encounters.
    /// </summary>
    public long TotalPlayerDamageDealt { get; private set; }

    /// <summary>
    /// Gets the cumulative damage received by the player across all encounters.
    /// </summary>
    public long TotalPlayerDamageReceived { get; private set; }

    /// <summary>
    /// Gets the total number of successful player attacks (hits).
    /// </summary>
    public int TotalPlayerHits { get; private set; }

    /// <summary>
    /// Gets the total number of missed player attacks.
    /// </summary>
    public int TotalPlayerMisses { get; private set; }

    /// <summary>
    /// Gets the cumulative damage dealt by enemies across all encounters.
    /// </summary>
    public long TotalEnemyDamageDealt { get; private set; }

    /// <summary>
    /// Gets the total number of successful enemy attacks (hits).
    /// </summary>
    public int TotalEnemyHits { get; private set; }

    /// <summary>
    /// Gets the total number of missed enemy attacks.
    /// </summary>
    public int TotalEnemyMisses { get; private set; }

    #endregion

    #region Resource Tracking

    /// <summary>
    /// Gets the total stamina spent by the player across all encounters.
    /// </summary>
    public long TotalPlayerStaminaSpent { get; private set; }

    /// <summary>
    /// Gets the cumulative player HP remaining at end of winning encounters.
    /// </summary>
    public long TotalPlayerHPRemaining { get; private set; }

    #endregion

    #region Derived Metrics

    /// <summary>
    /// Gets the player win rate as a percentage (0-100).
    /// </summary>
    public double WinRate =>
        TotalEncounters > 0 ? (double)PlayerWins / TotalEncounters * 100 : 0;

    /// <summary>
    /// Gets the average number of rounds per encounter.
    /// </summary>
    public double AvgRoundsPerEncounter =>
        TotalEncounters > 0 ? (double)TotalRounds / TotalEncounters : 0;

    /// <summary>
    /// Gets the player hit rate as a percentage (0-100).
    /// </summary>
    public double PlayerHitRate =>
        (TotalPlayerHits + TotalPlayerMisses) > 0
            ? (double)TotalPlayerHits / (TotalPlayerHits + TotalPlayerMisses) * 100
            : 0;

    /// <summary>
    /// Gets the enemy hit rate as a percentage (0-100).
    /// </summary>
    public double EnemyHitRate =>
        (TotalEnemyHits + TotalEnemyMisses) > 0
            ? (double)TotalEnemyHits / (TotalEnemyHits + TotalEnemyMisses) * 100
            : 0;

    /// <summary>
    /// Gets the average damage dealt per successful player hit.
    /// </summary>
    public double AvgPlayerDamagePerHit =>
        TotalPlayerHits > 0 ? (double)TotalPlayerDamageDealt / TotalPlayerHits : 0;

    /// <summary>
    /// Gets the average damage dealt per successful enemy hit.
    /// </summary>
    public double AvgEnemyDamagePerHit =>
        TotalEnemyHits > 0 ? (double)TotalEnemyDamageDealt / TotalEnemyHits : 0;

    /// <summary>
    /// Gets the average stamina spent per encounter.
    /// </summary>
    public double AvgStaminaPerEncounter =>
        TotalEncounters > 0 ? (double)TotalPlayerStaminaSpent / TotalEncounters : 0;

    /// <summary>
    /// Gets the average HP remaining when player wins (survivability metric).
    /// </summary>
    public double AvgHPRemainingOnWin =>
        PlayerWins > 0 ? (double)TotalPlayerHPRemaining / PlayerWins : 0;

    #endregion

    #region Recording Methods

    /// <summary>
    /// Records the result of a single combat encounter.
    /// </summary>
    /// <param name="result">The match result to record.</param>
    public void RecordEncounterResult(CombatMatchResult result)
    {
        TotalEncounters++;
        TotalRounds += result.RoundsElapsed;
        TotalPlayerStaminaSpent += result.StaminaSpent;

        if (result.PlayerWon)
        {
            PlayerWins++;
            TotalPlayerHPRemaining += result.PlayerHPRemaining;
        }
        else
        {
            EnemyWins++;
        }
    }

    /// <summary>
    /// Records a player attack attempt.
    /// </summary>
    /// <param name="hit">Whether the attack hit.</param>
    /// <param name="damage">The damage dealt (0 if miss).</param>
    public void RecordPlayerAttack(bool hit, int damage)
    {
        TotalPlayerTurns++;

        if (hit)
        {
            TotalPlayerHits++;
            TotalPlayerDamageDealt += damage;
        }
        else
        {
            TotalPlayerMisses++;
        }
    }

    /// <summary>
    /// Records an enemy attack attempt.
    /// </summary>
    /// <param name="hit">Whether the attack hit.</param>
    /// <param name="damage">The damage dealt (0 if miss).</param>
    public void RecordEnemyAttack(bool hit, int damage)
    {
        TotalEnemyTurns++;

        if (hit)
        {
            TotalEnemyHits++;
            TotalEnemyDamageDealt += damage;
            TotalPlayerDamageReceived += damage;
        }
        else
        {
            TotalEnemyMisses++;
        }
    }

    #endregion
}

/// <summary>
/// Result of a single simulated combat match.
/// </summary>
/// <param name="PlayerWon">True if the player won the encounter.</param>
/// <param name="RoundsElapsed">Number of combat rounds.</param>
/// <param name="PlayerHPRemaining">Player HP at end of combat.</param>
/// <param name="EnemyHPRemaining">Enemy HP at end of combat.</param>
/// <param name="StaminaSpent">Total stamina consumed by player.</param>
public record CombatMatchResult(
    bool PlayerWon,
    int RoundsElapsed,
    int PlayerHPRemaining,
    int EnemyHPRemaining,
    int StaminaSpent);
