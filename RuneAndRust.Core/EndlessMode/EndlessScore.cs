namespace RuneAndRust.Core.EndlessMode;

/// <summary>
/// v0.40.4: Endless Mode Score Calculation
/// Breaks down scoring into components
/// </summary>
public class EndlessScore
{
    // ═══════════════════════════════════════════════════════════
    // SCORE COMPONENTS
    // ═══════════════════════════════════════════════════════════

    /// <summary>Wave score: WavesCompleted × 1000</summary>
    public int WaveScore { get; set; } = 0;

    /// <summary>Kill score: EnemiesKilled × 50</summary>
    public int KillScore { get; set; } = 0;

    /// <summary>Boss score: BossesKilled × 500</summary>
    public int BossScore { get; set; } = 0;

    /// <summary>Time bonus: Max(0, 10000 - TotalTimeSeconds)</summary>
    public int TimeBonus { get; set; } = 0;

    /// <summary>Survival bonus: Max(0, 5000 - TotalDamageTaken)</summary>
    public int SurvivalBonus { get; set; } = 0;

    /// <summary>Total score: Sum of all components</summary>
    public int TotalScore { get; set; } = 0;

    // ═══════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════

    /// <summary>Base score (wave + kill + boss)</summary>
    public int BaseScore => WaveScore + KillScore + BossScore;

    /// <summary>Bonus score (time + survival)</summary>
    public int BonusScore => TimeBonus + SurvivalBonus;

    /// <summary>Percentage of score from bonuses</summary>
    public float BonusPercentage => TotalScore > 0
        ? (float)BonusScore / TotalScore * 100
        : 0;

    /// <summary>Score breakdown display</summary>
    public string BreakdownDisplay => $@"
Total Score: {TotalScore:N0}

Base Scores:
  Wave Score:     {WaveScore:N0}
  Kill Score:     {KillScore:N0}
  Boss Score:     {BossScore:N0}

Bonuses:
  Time Bonus:     {TimeBonus:N0}
  Survival Bonus: {SurvivalBonus:N0}

Base: {BaseScore:N0} | Bonus: {BonusScore:N0} ({BonusPercentage:F1}%)
".Trim();

    /// <summary>
    /// Calculate score from run metrics
    /// </summary>
    public static EndlessScore Calculate(int wavesCompleted, int enemiesKilled,
        int bossesKilled, int totalTimeSeconds, int totalDamageTaken)
    {
        var score = new EndlessScore
        {
            WaveScore = wavesCompleted * 1000,
            KillScore = enemiesKilled * 50,
            BossScore = bossesKilled * 500,
            TimeBonus = Math.Max(0, 10000 - totalTimeSeconds),
            SurvivalBonus = Math.Max(0, 5000 - totalDamageTaken)
        };

        score.TotalScore = score.WaveScore + score.KillScore + score.BossScore +
                          score.TimeBonus + score.SurvivalBonus;

        return score;
    }

    /// <summary>
    /// Calculate score from EndlessRun
    /// </summary>
    public static EndlessScore Calculate(EndlessRun run)
    {
        return Calculate(
            run.HighestWaveReached,
            run.TotalEnemiesKilled,
            run.TotalBossesKilled,
            run.TotalTimeSeconds,
            run.TotalDamageTaken
        );
    }
}
