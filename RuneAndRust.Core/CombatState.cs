namespace RuneAndRust.Core;

public class CombatParticipant
{
    public string Name { get; set; } = string.Empty;
    public bool IsPlayer { get; set; }
    public bool IsCompanion { get; set; } // v0.34.4: Companion support
    public int InitiativeRoll { get; set; }
    public int InitiativeAttribute { get; set; }

    // Reference to actual character, companion, or enemy
    public object? Character { get; set; } // Can be PlayerCharacter, Companion, or Enemy
}

public class CombatState
{
    public PlayerCharacter Player { get; set; } = new();
    public List<Enemy> Enemies { get; set; } = new();

    // v0.34.4: Companion System integration
    public List<Companion> Companions { get; set; } = new();
    public int CharacterId { get; set; } // For loading companion data

    public List<CombatParticipant> InitiativeOrder { get; set; } = new();
    public int CurrentTurnIndex { get; set; } = 0;
    public List<string> CombatLog { get; set; } = new();
    public bool IsActive { get; set; } = false;
    public bool CanFlee { get; set; } = true; // False for boss fights

    // [v0.4] Current room for environmental hazards
    public Room? CurrentRoom { get; set; } = null;

    // [v0.20] Tactical Combat Grid System
    public BattlefieldGrid? Grid { get; set; } = null;

    // Player temporary effects
    public int PlayerNextAttackBonusDice { get; set; } = 0;
    public bool PlayerNegateNextAttack { get; set; } = false;

    public CombatParticipant CurrentParticipant =>
        InitiativeOrder.Count > 0 ? InitiativeOrder[CurrentTurnIndex] : new();

    public void AddLogEntry(string message)
    {
        CombatLog.Add(message);
    }

    /// <summary>
    /// v0.37.2: Clear combat log for a new action
    /// Used by commands to reset log before executing
    /// </summary>
    public void ClearLogForNewAction()
    {
        CombatLog.Clear();
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
