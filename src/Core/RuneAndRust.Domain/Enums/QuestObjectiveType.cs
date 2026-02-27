namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Types of quest objectives that can be tracked by the quest event bus.
/// </summary>
/// <remarks>
/// <para>
/// Each objective type maps to one or more game event types from
/// <see cref="Events.GameEvent"/>. The quest event bus uses this enum
/// to route incoming game events to the correct quest objective handlers.
/// </para>
/// <para>Mapping table:</para>
/// <list type="table">
///   <listheader>
///     <term>Objective Type</term>
///     <description>Game Event Type</description>
///   </listheader>
///   <item>
///     <term>KillEnemy</term>
///     <description>MonsterDefeated (Combat category)</description>
///   </item>
///   <item>
///     <term>CollectItem</term>
///     <description>ItemPickedUp (Inventory category)</description>
///   </item>
///   <item>
///     <term>ExploreRoom</term>
///     <description>RoomEntered (Exploration category)</description>
///   </item>
///   <item>
///     <term>InteractWithObject</term>
///     <description>InteractionCompleted (Interaction category)</description>
///   </item>
///   <item>
///     <term>MakeChoice</term>
///     <description>DialogueChoiceMade (Interaction category)</description>
///   </item>
///   <item>
///     <term>SurviveEncounter</term>
///     <description>SurvivedEncounter (Combat category)</description>
///   </item>
///   <item>
///     <term>TalkToNpc</term>
///     <description>DialogueStarted (Interaction category)</description>
///   </item>
///   <item>
///     <term>ReachLevel</term>
///     <description>LevelUp (Character category)</description>
///   </item>
/// </list>
/// </remarks>
public enum QuestObjectiveType
{
    /// <summary>Defeat a specific enemy type. Matched by monster definition ID.</summary>
    KillEnemy = 0,

    /// <summary>Pick up a specific item type. Matched by item definition ID.</summary>
    CollectItem = 1,

    /// <summary>Enter a specific room or area. Matched by room template ID.</summary>
    ExploreRoom = 2,

    /// <summary>Interact with a specific object (terminal, lever, etc.). Matched by object ID.</summary>
    InteractWithObject = 3,

    /// <summary>Make a specific dialogue choice. Matched by choice/node ID.</summary>
    MakeChoice = 4,

    /// <summary>Survive a specific encounter or hazard. Matched by encounter ID.</summary>
    SurviveEncounter = 5,

    /// <summary>Initiate dialogue with a specific NPC. Matched by NPC definition ID.</summary>
    TalkToNpc = 6,

    /// <summary>Reach a certain character or legend level. Matched by level threshold.</summary>
    ReachLevel = 7
}
