namespace RuneAndRust.Engine.Models;

public record SimMatchResult(
    string Winner,
    int Turns,
    int PlayerRemainingHp,
    int PlayerMaxHp,
    int StaminaSpent,
    bool PlayerDied
);

public record SimBatchResult(
    string Archetype,
    string EnemyId,
    int TotalMatches,
    double WinRate,
    double AvgTurns,
    double AvgHpRemaining,
    double AvgStaminaSpent
);
