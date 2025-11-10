namespace RuneAndRust.Core;

public class CombatParticipant
{
    public string Name { get; set; } = string.Empty;
    public bool IsPlayer { get; set; }
    public int InitiativeRoll { get; set; }
    public int InitiativeAttribute { get; set; }

    // Reference to actual character or enemy
    public object? Character { get; set; } // Can be PlayerCharacter or Enemy
}

public class CombatState
{
    public PlayerCharacter Player { get; set; } = new();
    public List<Enemy> Enemies { get; set; } = new();
    public List<CombatParticipant> InitiativeOrder { get; set; } = new();
    public int CurrentTurnIndex { get; set; } = 0;
    public List<string> CombatLog { get; set; } = new();
    public bool IsActive { get; set; } = false;
    public bool CanFlee { get; set; } = true; // False for boss fights

    // Player temporary effects
    public int PlayerNextAttackBonusDice { get; set; } = 0;
    public bool PlayerNegateNextAttack { get; set; } = false;

    public CombatParticipant CurrentParticipant =>
        InitiativeOrder.Count > 0 ? InitiativeOrder[CurrentTurnIndex] : new();

    public void AddLogEntry(string message)
    {
        CombatLog.Add(message);
    }

    public void NextTurn()
    {
        CurrentTurnIndex = (CurrentTurnIndex + 1) % InitiativeOrder.Count;
    }

    public bool IsPlayerTurn()
    {
        return CurrentParticipant.IsPlayer;
    }
}
