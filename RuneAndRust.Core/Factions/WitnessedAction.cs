namespace RuneAndRust.Core.Factions;

/// <summary>
/// v0.33.2: Represents an action that can affect faction reputation when witnessed
/// </summary>
public class WitnessedAction
{
    public string ActionType { get; set; } = string.Empty;
    public int? TargetFactionId { get; set; }
    public string? TargetEnemyType { get; set; }
    public int ReputationImpact { get; set; }
    public string Location { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }

    public WitnessedAction()
    {
        Timestamp = DateTime.Now;
    }
}

/// <summary>
/// v0.33.2: Types of actions that can be witnessed and affect reputation
/// </summary>
public static class WitnessedActionTypes
{
    public const string KillEnemy = "KillEnemy";
    public const string KillUndying = "KillUndying";
    public const string KillJotunForged = "KillJotunForged";
    public const string SpareEnemy = "SpareEnemy";
    public const string CompleteQuest = "CompleteQuest";
    public const string AttackFactionMember = "AttackFactionMember";
    public const string KillFactionMember = "KillFactionMember";
    public const string TradeWithMerchant = "TradeWithMerchant";
    public const string DonateResources = "DonateResources";
    public const string ProtectFactionMember = "ProtectFactionMember";
    public const string DestroyData = "DestroyData";
    public const string RecoverData = "RecoverData";
    public const string ShareKnowledge = "ShareKnowledge";
    public const string HoardKnowledge = "HoardKnowledge";
}
