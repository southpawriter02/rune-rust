namespace RuneAndRust.Core;

/// <summary>
/// v0.21.4: Parry outcome enum (v2.0 canonical)
/// </summary>
public enum ParryOutcome
{
    Failed,   // Parry < Accuracy
    Standard, // Parry = Accuracy
    Superior, // Parry > Accuracy (by 1-4)
    Critical  // Parry > Accuracy (by 5+)
}

/// <summary>
/// v0.21.4: Result of a parry attempt
/// </summary>
public class ParryResult
{
    public bool Success { get; set; }
    public ParryOutcome Outcome { get; set; }
    public int ParryRoll { get; set; }
    public int AccuracyRoll { get; set; }
    public bool RiposteTriggered { get; set; }
    public RiposteResult? Riposte { get; set; }
    public int StressChange { get; set; }
    public int DefenderID { get; set; }
    public int AttackerID { get; set; }
    public string? AttackAbility { get; set; }
}

/// <summary>
/// v0.21.4: Result of a riposte counter-attack
/// </summary>
public class RiposteResult
{
    public bool Hit { get; set; }
    public int DamageDealt { get; set; }
    public bool KilledTarget { get; set; }
    public int AttackRoll { get; set; }
    public int DefenseScore { get; set; }
}

/// <summary>
/// v0.21.4: Parry statistics for a character (persistent)
/// </summary>
public class ParryStatistics
{
    public int CharacterID { get; set; }
    public int TotalParryAttempts { get; set; }
    public int SuccessfulParries { get; set; }
    public int SuperiorParries { get; set; }
    public int CriticalParries { get; set; }
    public int FailedParries { get; set; }
    public int RipostesLanded { get; set; }
    public int RiposteKills { get; set; }

    public float SuccessRate => TotalParryAttempts > 0
        ? (float)SuccessfulParries / TotalParryAttempts * 100f
        : 0f;
}

/// <summary>
/// v0.21.4: Parry bonus from specialization or equipment
/// </summary>
public class ParryBonus
{
    public int BonusID { get; set; }
    public int CharacterID { get; set; }
    public string Source { get; set; } = string.Empty; // 'Reactive Parry', 'Equipment', 'Buff'
    public int BonusDice { get; set; } = 0; // Extra d10s to Parry Pool
    public bool AllowsSuperiorRiposte { get; set; } = false; // Superior triggers Riposte
    public int ParriesPerRound { get; set; } = 1; // Usually 1, Hólmgangr Rank 3 = 2
}

/// <summary>
/// v0.21.4: Parry attempt record (combat log)
/// </summary>
public class ParryAttempt
{
    public int AttemptID { get; set; }
    public int CombatInstanceID { get; set; }
    public int DefenderID { get; set; }
    public int AttackerID { get; set; }
    public string? AttackAbility { get; set; }
    public int ParryPoolRoll { get; set; }
    public int AttackerAccuracyRoll { get; set; }
    public ParryOutcome Outcome { get; set; }
    public bool RiposteTriggered { get; set; }
    public int RiposteDamage { get; set; }
    public DateTime Timestamp { get; set; }
}
