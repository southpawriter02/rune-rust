using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.Events;

/// <summary>
/// Combat-specific event.
/// </summary>
public record CombatEvent : GameEvent
{
    /// <summary>Gets the attacker ID.</summary>
    public Guid? AttackerId { get; init; }

    /// <summary>Gets the target ID.</summary>
    public Guid? TargetId { get; init; }

    /// <summary>Gets the damage dealt.</summary>
    public int? Damage { get; init; }

    /// <summary>Gets the ability used.</summary>
    public string? AbilityId { get; init; }

    /// <summary>Gets whether it was a critical hit.</summary>
    public bool IsCritical { get; init; }

    public CombatEvent() => Category = EventCategory.Combat;

    public static CombatEvent Attack(Guid attackerId, Guid targetId, int damage, bool isCritical = false, string? abilityId = null) =>
        new()
        {
            EventType = "Attack",
            Message = $"Attack dealt {damage} damage{(isCritical ? " (critical)" : "")}",
            AttackerId = attackerId,
            TargetId = targetId,
            Damage = damage,
            IsCritical = isCritical,
            AbilityId = abilityId
        };

    public static CombatEvent Death(Guid targetId, Guid? killerId = null) =>
        new()
        {
            EventType = "Death",
            Message = "Target was defeated",
            TargetId = targetId,
            AttackerId = killerId
        };

    public static CombatEvent CombatStarted(Guid correlationId) =>
        new() { EventType = "CombatStarted", Message = "Combat initiated", CorrelationId = correlationId };

    public static CombatEvent CombatEnded(Guid correlationId, bool playerVictory) =>
        new()
        {
            EventType = "CombatEnded",
            Message = playerVictory ? "Combat won" : "Combat lost",
            CorrelationId = correlationId
        };
}

/// <summary>
/// Exploration-specific event.
/// </summary>
public record ExplorationEvent : GameEvent
{
    /// <summary>Gets the source room ID.</summary>
    public Guid? FromRoomId { get; init; }

    /// <summary>Gets the destination room ID.</summary>
    public Guid? ToRoomId { get; init; }

    /// <summary>Gets the direction moved.</summary>
    public Direction? Direction { get; init; }

    public ExplorationEvent() => Category = EventCategory.Exploration;

    public static ExplorationEvent Moved(Guid fromRoom, Guid toRoom, Direction direction) =>
        new()
        {
            EventType = "Moved",
            Message = $"Moved {direction}",
            FromRoomId = fromRoom,
            ToRoomId = toRoom,
            RoomId = toRoom,
            Direction = direction
        };

    public static ExplorationEvent RoomEntered(Guid roomId, string roomName) =>
        new() { EventType = "RoomEntered", Message = $"Entered {roomName}", RoomId = roomId };

    public static ExplorationEvent RoomDiscovered(Guid roomId, string roomName) =>
        new() { EventType = "RoomDiscovered", Message = $"Discovered {roomName}", RoomId = roomId };
}

/// <summary>
/// Interaction-specific event.
/// </summary>
public record InteractionEvent : GameEvent
{
    /// <summary>Gets the object ID.</summary>
    public Guid? ObjectId { get; init; }

    /// <summary>Gets the object name.</summary>
    public string? ObjectName { get; init; }

    /// <summary>Gets the interaction type.</summary>
    public InteractionType? InteractionPerformed { get; init; }

    /// <summary>Gets the result state.</summary>
    public ObjectState? ResultState { get; init; }

    public InteractionEvent() => Category = EventCategory.Interaction;

    public static InteractionEvent Interacted(Guid objectId, string objectName, InteractionType type, ObjectState? newState = null) =>
        new()
        {
            EventType = type.ToString(),
            Message = $"{type} {objectName}",
            ObjectId = objectId,
            ObjectName = objectName,
            InteractionPerformed = type,
            ResultState = newState
        };
}

/// <summary>
/// Quest-specific event.
/// </summary>
public record QuestEvent : GameEvent
{
    /// <summary>Gets the quest ID.</summary>
    public string? QuestId { get; init; }

    /// <summary>Gets the quest name.</summary>
    public string? QuestName { get; init; }

    public QuestEvent() => Category = EventCategory.Quest;

    public static QuestEvent Started(string questId, string questName, Guid? playerId = null) =>
        new() { EventType = "QuestStarted", Message = $"Started quest: {questName}", QuestId = questId, QuestName = questName, PlayerId = playerId };

    public static QuestEvent Completed(string questId, string questName, Guid? playerId = null) =>
        new() { EventType = "QuestCompleted", Message = $"Completed quest: {questName}", QuestId = questId, QuestName = questName, PlayerId = playerId };

    public static QuestEvent Failed(string questId, string questName, string reason, Guid? playerId = null) =>
        new() { EventType = "QuestFailed", Message = $"Failed quest: {questName} - {reason}", QuestId = questId, QuestName = questName, PlayerId = playerId };

    public static QuestEvent ObjectiveCompleted(string questId, string objectiveDescription, Guid? playerId = null) =>
        new() { EventType = "ObjectiveCompleted", Message = $"Objective completed: {objectiveDescription}", QuestId = questId, PlayerId = playerId };
}

/// <summary>
/// Character-specific event.
/// </summary>
public record CharacterEvent : GameEvent
{
    /// <summary>Gets the stat or attribute affected.</summary>
    public string? StatName { get; init; }

    /// <summary>Gets the old value.</summary>
    public int? OldValue { get; init; }

    /// <summary>Gets the new value.</summary>
    public int? NewValue { get; init; }

    public CharacterEvent() => Category = EventCategory.Character;

    public static CharacterEvent LevelUp(Guid playerId, int newLevel) =>
        new() { EventType = "LevelUp", Message = $"Reached level {newLevel}", PlayerId = playerId, NewValue = newLevel };

    public static CharacterEvent HealthChanged(Guid playerId, int oldHealth, int newHealth) =>
        new() { EventType = "HealthChanged", Message = $"Health: {oldHealth} → {newHealth}", PlayerId = playerId, StatName = "Health", OldValue = oldHealth, NewValue = newHealth };

    public static CharacterEvent Created(Guid playerId, string name) =>
        new() { EventType = "CharacterCreated", Message = $"Character {name} created", PlayerId = playerId };
}

/// <summary>
/// Dice roll event.
/// </summary>
public record DiceEvent : GameEvent
{
    /// <summary>Gets the dice notation.</summary>
    public string? Notation { get; init; }

    /// <summary>Gets the individual die results.</summary>
    public IReadOnlyList<int>? Results { get; init; }

    /// <summary>Gets the total.</summary>
    public int? Total { get; init; }

    public DiceEvent() => Category = EventCategory.Dice;

    public static DiceEvent Rolled(string notation, IReadOnlyList<int> results, int total) =>
        new() { EventType = "DiceRolled", Message = $"Rolled {notation}: {total}", Notation = notation, Results = results, Total = total };
}
