using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Represents a quest objective.
/// </summary>
public class QuestObjective
{
    /// <summary>Gets the objective ID.</summary>
    public Guid Id { get; private set; }

    /// <summary>Gets the objective description.</summary>
    public string Description { get; private set; }

    /// <summary>Gets the required count to complete.</summary>
    public int RequiredCount { get; private set; }

    /// <summary>Gets the current progress count.</summary>
    public int CurrentProgress { get; private set; }

    /// <summary>Gets whether this objective is completed.</summary>
    public bool IsCompleted => CurrentProgress >= RequiredCount;

    /// <summary>Private constructor for EF Core.</summary>
    private QuestObjective()
    {
        Description = null!;
    }

    /// <summary>Creates a new quest objective.</summary>
    public QuestObjective(string description, int requiredCount = 1)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentOutOfRangeException.ThrowIfLessThan(requiredCount, 1);

        Id = Guid.NewGuid();
        Description = description;
        RequiredCount = requiredCount;
        CurrentProgress = 0;
    }

    /// <summary>Advances progress by the specified amount.</summary>
    public void AdvanceProgress(int amount = 1)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        CurrentProgress = Math.Min(CurrentProgress + amount, RequiredCount);
    }

    /// <summary>Resets progress to zero.</summary>
    public void ResetProgress()
    {
        CurrentProgress = 0;
    }
}

/// <summary>
/// Represents a quest in the game.
/// </summary>
/// <remarks>
/// Quests have objectives, categories, and support abandonment and repeatable mechanics.
/// </remarks>
public class Quest : IEntity
{
    private readonly List<QuestObjective> _objectives = [];

    /// <summary>Gets the quest ID.</summary>
    public Guid Id { get; private set; }

    /// <summary>Gets the quest definition ID.</summary>
    public string DefinitionId { get; private set; }

    /// <summary>Gets the quest name.</summary>
    public string Name { get; private set; }

    /// <summary>Gets the quest description.</summary>
    public string Description { get; private set; }

    /// <summary>Gets the quest objectives.</summary>
    public IReadOnlyList<QuestObjective> Objectives => _objectives;

    /// <summary>Gets the quest status.</summary>
    public QuestStatus Status { get; internal set; } = QuestStatus.Available;

    /// <summary>Gets the failure reason if failed.</summary>
    public string? FailureReason { get; internal set; }

    // ===== v0.3.2b TIMED QUEST PROPERTIES =====

    /// <summary>Gets whether this quest is timed.</summary>
    public bool IsTimed => TimeLimit.HasValue;

    /// <summary>Gets the time limit in turns.</summary>
    public int? TimeLimit { get; private set; }

    /// <summary>Gets the remaining turns for timed quests.</summary>
    public int? TurnsRemaining { get; internal set; }

    // ===== v0.3.2a CHAIN PROPERTIES =====

    /// <summary>Gets whether this quest is part of a chain.</summary>
    public bool IsInChain => !string.IsNullOrEmpty(ChainId);

    /// <summary>Gets the chain ID if part of a chain.</summary>
    public string? ChainId { get; private set; }

    // ===== v0.3.2c CATEGORY PROPERTIES =====

    /// <summary>Gets the quest category. Defaults to Side.</summary>
    public QuestCategory Category { get; private set; } = QuestCategory.Side;

    /// <summary>Gets whether this quest is repeatable.</summary>
    public bool IsRepeatable { get; private set; }

    /// <summary>Gets whether this is a daily quest.</summary>
    public bool IsDaily => Category == QuestCategory.Daily;

    /// <summary>Gets the cooldown for repeatable quests (in hours).</summary>
    public int? RepeatCooldown { get; private set; }

    /// <summary>Gets when this quest can be repeated.</summary>
    public DateTime? RepeatableAfter { get; private set; }

    /// <summary>Gets whether the quest is on cooldown.</summary>
    public bool IsOnCooldown => RepeatableAfter.HasValue && RepeatableAfter.Value > DateTime.UtcNow;

    /// <summary>Gets the time remaining until the quest can be repeated.</summary>
    public TimeSpan? CooldownRemaining => IsOnCooldown
        ? RepeatableAfter!.Value - DateTime.UtcNow
        : null;

    /// <summary>Private constructor for EF Core.</summary>
    private Quest()
    {
        DefinitionId = null!;
        Name = null!;
        Description = null!;
    }

    /// <summary>Creates a new quest.</summary>
    public Quest(string definitionId, string name, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(definitionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        Id = Guid.NewGuid();
        DefinitionId = definitionId.ToLowerInvariant();
        Name = name;
        Description = description;
    }

    // ===== OBJECTIVE MANAGEMENT =====

    /// <summary>Adds an objective to this quest.</summary>
    public void AddObjective(QuestObjective objective)
    {
        ArgumentNullException.ThrowIfNull(objective);
        _objectives.Add(objective);
    }

    /// <summary>Gets the completion percentage.</summary>
    public int GetCompletionPercentage()
    {
        if (_objectives.Count == 0) return 0;
        var completed = _objectives.Count(o => o.IsCompleted);
        return (int)((double)completed / _objectives.Count * 100);
    }

    /// <summary>Gets whether all objectives are completed.</summary>
    public bool AreAllObjectivesCompleted() => _objectives.All(o => o.IsCompleted);

    // ===== STATUS MANAGEMENT =====

    /// <summary>Activates the quest.</summary>
    public void Activate()
    {
        if (Status != QuestStatus.Available)
            throw new InvalidOperationException("Can only activate available quests.");

        Status = QuestStatus.Active;
        if (TimeLimit.HasValue)
            TurnsRemaining = TimeLimit;
    }

    /// <summary>Completes the quest.</summary>
    public void Complete()
    {
        if (Status != QuestStatus.Active)
            throw new InvalidOperationException("Can only complete active quests.");

        Status = QuestStatus.Completed;

        if (IsRepeatable && RepeatCooldown.HasValue)
            SetRepeatCooldown(RepeatCooldown.Value);
    }

    /// <summary>Fails the quest.</summary>
    public void Fail(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if (Status != QuestStatus.Active)
            throw new InvalidOperationException("Can only fail active quests.");

        Status = QuestStatus.Failed;
        FailureReason = reason;
    }

    // ===== TIMING =====

    /// <summary>Sets the time limit for this quest.</summary>
    public void SetTimeLimit(int turns)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(turns, 1);
        TimeLimit = turns;
        if (Status == QuestStatus.Active)
            TurnsRemaining = turns;
    }

    /// <summary>Advances time and returns true if expired.</summary>
    public bool TickTime(int turns = 1)
    {
        if (!TurnsRemaining.HasValue) return false;

        TurnsRemaining = Math.Max(0, TurnsRemaining.Value - turns);
        return TurnsRemaining.Value <= 0;
    }

    // ===== CHAIN =====

    /// <summary>Sets the chain ID for this quest.</summary>
    public void SetChainId(string chainId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chainId);
        ChainId = chainId.ToLowerInvariant();
    }

    // ===== v0.3.2c CATEGORY METHODS =====

    /// <summary>Sets the quest category.</summary>
    public void SetCategory(QuestCategory category)
    {
        Category = category;
    }

    /// <summary>Marks the quest as repeatable with optional cooldown.</summary>
    public void SetRepeatable(int? cooldownHours = null)
    {
        IsRepeatable = true;
        RepeatCooldown = cooldownHours;
    }

    /// <summary>Resets a repeatable quest for another completion.</summary>
    public void ResetForRepeat()
    {
        if (!IsRepeatable && Category != QuestCategory.Daily)
            throw new InvalidOperationException("Quest is not repeatable.");

        Status = QuestStatus.Available;

        foreach (var objective in _objectives)
        {
            objective.ResetProgress();
        }

        FailureReason = null;
    }

    /// <summary>Sets the cooldown after completing a repeatable quest.</summary>
    public void SetRepeatCooldown(int cooldownHours)
    {
        if (cooldownHours > 0)
            RepeatableAfter = DateTime.UtcNow.AddHours(cooldownHours);
        else
            RepeatableAfter = null;
    }

    /// <summary>Clears the repeat cooldown.</summary>
    public void ClearRepeatCooldown()
    {
        RepeatableAfter = null;
    }

    /// <summary>Gets whether this quest can be abandoned.</summary>
    public bool CanBeAbandoned()
    {
        return Category != QuestCategory.Main;
    }

    /// <summary>Abandons this quest.</summary>
    public void Abandon()
    {
        if (Status != QuestStatus.Active)
            throw new InvalidOperationException("Can only abandon active quests.");

        if (Category == QuestCategory.Main)
            throw new InvalidOperationException("Cannot abandon main quests.");

        Status = QuestStatus.Available;

        foreach (var objective in _objectives)
        {
            objective.ResetProgress();
        }

        FailureReason = null;
        if (IsTimed && TimeLimit.HasValue)
        {
            TurnsRemaining = TimeLimit;
        }
    }
}
