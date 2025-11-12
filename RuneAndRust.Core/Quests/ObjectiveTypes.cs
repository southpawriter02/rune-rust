namespace RuneAndRust.Core.Quests;

/// <summary>
/// v0.14: Enhanced objective tracking with typed objective classes
/// Provides better type safety and progress tracking than simple ObjectiveType enum
/// </summary>

/// <summary>
/// Base class for all quest objectives
/// </summary>
public abstract class BaseQuestObjective
{
    public string ObjectiveId { get; set; } = Guid.NewGuid().ToString();
    public string Description { get; set; } = string.Empty;
    public bool IsOptional { get; set; } = false;

    public abstract int CurrentProgress { get; }
    public abstract int TargetProgress { get; }
    public abstract string ProgressText { get; }
    public abstract bool IsComplete { get; }

    /// <summary>
    /// Checks if objective is complete based on game state
    /// </summary>
    public abstract bool CheckCompletion(PlayerCharacter player);
}

/// <summary>
/// Objective to kill specific enemies
/// </summary>
public class KillObjective : BaseQuestObjective
{
    public string TargetEnemyType { get; set; } = string.Empty; // Enemy type name
    public int TargetCount { get; set; } = 1;
    public int KilledCount { get; set; } = 0;

    public override int CurrentProgress => KilledCount;
    public override int TargetProgress => TargetCount;
    public override string ProgressText => $"Defeated {KilledCount}/{TargetCount} {TargetEnemyType}";
    public override bool IsComplete => KilledCount >= TargetCount;

    public override bool CheckCompletion(PlayerCharacter player)
    {
        return IsComplete;
    }

    /// <summary>
    /// Records an enemy kill
    /// </summary>
    public void RecordKill()
    {
        if (!IsComplete)
            KilledCount++;
    }
}

/// <summary>
/// Objective to collect specific items
/// </summary>
public class CollectObjective : BaseQuestObjective
{
    public string TargetItemId { get; set; } = string.Empty;
    public int TargetCount { get; set; } = 1;
    public int CollectedCount { get; set; } = 0;

    public override int CurrentProgress => CollectedCount;
    public override int TargetProgress => TargetCount;
    public override string ProgressText => $"Collected {CollectedCount}/{TargetCount} {TargetItemId}";
    public override bool IsComplete => CollectedCount >= TargetCount;

    public override bool CheckCompletion(PlayerCharacter player)
    {
        // Check if player has required items in inventory
        // For now, we'll trust the CollectedCount tracking
        return IsComplete;
    }

    /// <summary>
    /// Records an item collection
    /// </summary>
    public void RecordCollection()
    {
        if (!IsComplete)
            CollectedCount++;
    }
}

/// <summary>
/// Objective to explore specific rooms or locations
/// </summary>
public class ExploreObjective : BaseQuestObjective
{
    public string TargetRoomId { get; set; } = string.Empty; // Room ID or location name
    public bool Discovered { get; set; } = false;

    public override int CurrentProgress => Discovered ? 1 : 0;
    public override int TargetProgress => 1;
    public override string ProgressText => Discovered ? "Location discovered" : $"Find {TargetRoomId}";
    public override bool IsComplete => Discovered;

    public override bool CheckCompletion(PlayerCharacter player)
    {
        return Discovered;
    }

    /// <summary>
    /// Marks the location as discovered
    /// </summary>
    public void MarkDiscovered()
    {
        Discovered = true;
    }
}

/// <summary>
/// Objective to interact with NPCs or objects
/// </summary>
public class InteractObjective : BaseQuestObjective
{
    public string TargetEntityId { get; set; } = string.Empty; // NPC ID or object ID
    public bool Interacted { get; set; } = false;

    public override int CurrentProgress => Interacted ? 1 : 0;
    public override int TargetProgress => 1;
    public override string ProgressText => Interacted ? "Interaction complete" : Description;
    public override bool IsComplete => Interacted;

    public override bool CheckCompletion(PlayerCharacter player)
    {
        return Interacted;
    }

    /// <summary>
    /// Marks the interaction as complete
    /// </summary>
    public void MarkInteracted()
    {
        Interacted = true;
    }
}
