using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Events;
using RuneAndRust.Core.Interfaces;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Engine-layer service for managing character progression (Legend, Levels, Milestones).
/// Handles XP accumulation, level-up detection, and Progression Point awards.
/// </summary>
/// <remarks>See: v0.4.0a (The Legend) for system design.</remarks>
public class SagaService : ISagaService
{
    private readonly IEventBus _eventBus;
    private readonly IStatCalculationService _statCalc;
    private readonly ILogger<SagaService> _logger;

    /// <summary>
    /// Milestone table: Level -> (Required Legend, PP Award).
    /// Legend values are cumulative (total XP needed to reach that level).
    /// </summary>
    private static readonly Dictionary<int, (int RequiredLegend, int PpAward)> Milestones = new()
    {
        { 1, (0, 0) },
        { 2, (100, 1) },
        { 3, (300, 1) },
        { 4, (600, 2) },
        { 5, (1000, 2) },
        { 6, (1500, 2) },
        { 7, (2100, 3) },
        { 8, (2800, 3) },
        { 9, (3600, 3) },
        { 10, (4500, 5) },
    };

    /// <summary>
    /// Maximum achievable level.
    /// </summary>
    private const int MaxLevel = 10;

    /// <summary>
    /// Initializes a new instance of the <see cref="SagaService"/> class.
    /// </summary>
    /// <param name="eventBus">The event bus for publishing level-up events.</param>
    /// <param name="statCalc">The stat calculation service for recalculating derived stats.</param>
    /// <param name="logger">The logger instance.</param>
    public SagaService(
        IEventBus eventBus,
        IStatCalculationService statCalc,
        ILogger<SagaService> logger)
    {
        _eventBus = eventBus;
        _statCalc = statCalc;
        _logger = logger;
    }

    /// <inheritdoc/>
    public void AddLegend(Character character, int amount, string reason)
    {
        _logger.LogTrace("[Saga] AddLegend called for {Name} with amount {Amount}, reason: {Reason}",
            character.Name, amount, reason);

        if (amount <= 0)
        {
            _logger.LogWarning("[Saga] AddLegend called with non-positive amount: {Amount}", amount);
            return;
        }

        var oldLegend = character.Legend;
        character.Legend += amount;

        _logger.LogInformation(
            "[Saga] {Name} gained {Amount} Legend ({Reason}). Total: {OldLegend} -> {NewLegend}",
            character.Name, amount, reason, oldLegend, character.Legend);

        CheckMilestones(character);
    }

    /// <inheritdoc/>
    public int GetLegendForNextLevel(int currentLevel)
    {
        int nextLevel = currentLevel + 1;
        return Milestones.TryGetValue(nextLevel, out var milestone) ? milestone.RequiredLegend : -1;
    }

    /// <inheritdoc/>
    public int GetPpAward(int level)
    {
        return Milestones.TryGetValue(level, out var milestone) ? milestone.PpAward : 0;
    }

    /// <summary>
    /// Recursively checks for level-up(s). Handles multi-level jumps when
    /// a character gains enough Legend to skip levels.
    /// </summary>
    /// <param name="character">The character to check for level-ups.</param>
    private void CheckMilestones(Character character)
    {
        if (character.Level >= MaxLevel)
        {
            _logger.LogTrace("[Saga] {Name} is already at max level ({MaxLevel})", character.Name, MaxLevel);
            return;
        }

        int nextLevel = character.Level + 1;
        if (!Milestones.TryGetValue(nextLevel, out var milestone))
        {
            _logger.LogWarning("[Saga] No milestone data found for level {Level}", nextLevel);
            return;
        }

        if (character.Legend >= milestone.RequiredLegend)
        {
            PerformLevelUp(character, nextLevel, milestone.PpAward);
            CheckMilestones(character); // Recursive check for another level-up
        }
    }

    /// <summary>
    /// Performs the actual level-up: increments level, awards PP, heals fully, and fires event.
    /// </summary>
    /// <param name="character">The character leveling up.</param>
    /// <param name="newLevel">The new level being achieved.</param>
    /// <param name="ppAward">The Progression Points to award.</param>
    private void PerformLevelUp(Character character, int newLevel, int ppAward)
    {
        int oldLevel = character.Level;
        character.Level = newLevel;
        character.ProgressionPoints += ppAward;

        // Recalculate derived stats (MaxHP, MaxStamina may change with level if formulas are updated)
        _statCalc.RecalculateDerivedStats(character);

        // Full heal on level-up
        character.CurrentHP = character.MaxHP;
        character.CurrentStamina = character.MaxStamina;

        _logger.LogInformation(
            "[Saga] {Name} LEVELED UP to {Level}! Awarded {PP} PP. HP/Stamina restored.",
            character.Name, newLevel, ppAward);

        // Publish event for UI listeners
        var levelUpEvent = new LevelUpEvent(
            character.Id,
            character.Name,
            newLevel,
            ppAward);

        _eventBus.Publish(levelUpEvent);

        _logger.LogDebug("[Saga] LevelUpEvent published for {Name}: Level {OldLevel} -> {NewLevel}",
            character.Name, oldLevel, newLevel);
    }
}
