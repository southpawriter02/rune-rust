using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Data transfer object for displaying level-up information.
/// </summary>
/// <remarks>
/// Used by the presentation layer to render level-up notifications
/// with before/after stat comparisons.
/// </remarks>
public record LevelUpDto
{
    /// <summary>
    /// Gets the level before leveling up.
    /// </summary>
    public int OldLevel { get; init; }

    /// <summary>
    /// Gets the level after leveling up.
    /// </summary>
    public int NewLevel { get; init; }

    /// <summary>
    /// Gets the number of levels gained.
    /// </summary>
    public int LevelsGained { get; init; }

    /// <summary>
    /// Gets the max health before leveling up.
    /// </summary>
    public int OldMaxHealth { get; init; }

    /// <summary>
    /// Gets the max health after leveling up.
    /// </summary>
    public int NewMaxHealth { get; init; }

    /// <summary>
    /// Gets the attack before leveling up.
    /// </summary>
    public int OldAttack { get; init; }

    /// <summary>
    /// Gets the attack after leveling up.
    /// </summary>
    public int NewAttack { get; init; }

    /// <summary>
    /// Gets the defense before leveling up.
    /// </summary>
    public int OldDefense { get; init; }

    /// <summary>
    /// Gets the defense after leveling up.
    /// </summary>
    public int NewDefense { get; init; }

    /// <summary>
    /// Gets the names of newly unlocked abilities.
    /// </summary>
    public IReadOnlyList<string> UnlockedAbilityNames { get; init; } = [];

    /// <summary>
    /// Gets the experience points needed to reach the next level.
    /// </summary>
    public int XpToNextLevel { get; init; }

    /// <summary>
    /// Gets custom rewards granted at this level.
    /// </summary>
    public IReadOnlyList<string> CustomRewards { get; init; } = [];

    /// <summary>
    /// Gets the milestone title earned at this level, if any.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Gets the configured term for experience (e.g., "XP", "Glory").
    /// </summary>
    public string ExperienceTerminology { get; init; } = "XP";

    /// <summary>
    /// Gets the configured term for level (e.g., "Level", "Rank").
    /// </summary>
    public string LevelTerminology { get; init; } = "Level";

    /// <summary>
    /// Gets whether multiple levels were gained at once.
    /// </summary>
    public bool IsMultiLevel => LevelsGained > 1;

    /// <summary>
    /// Gets whether any new abilities were unlocked.
    /// </summary>
    public bool HasUnlockedAbilities => UnlockedAbilityNames.Count > 0;

    /// <summary>
    /// Gets whether there are custom rewards for this level.
    /// </summary>
    public bool HasCustomRewards => CustomRewards.Count > 0;

    /// <summary>
    /// Gets whether a title was earned at this level.
    /// </summary>
    public bool HasTitle => !string.IsNullOrEmpty(Title);

    /// <summary>
    /// Gets the health increase from leveling up.
    /// </summary>
    public int HealthIncrease => NewMaxHealth - OldMaxHealth;

    /// <summary>
    /// Gets the attack increase from leveling up.
    /// </summary>
    public int AttackIncrease => NewAttack - OldAttack;

    /// <summary>
    /// Gets the defense increase from leveling up.
    /// </summary>
    public int DefenseIncrease => NewDefense - OldDefense;

    /// <summary>
    /// Creates a LevelUpDto from a domain LevelUpResult and related stats.
    /// </summary>
    /// <param name="result">The level-up result from the domain.</param>
    /// <param name="oldStats">The player's stats before leveling up.</param>
    /// <param name="newStats">The player's stats after leveling up.</param>
    /// <param name="abilityNames">Names of unlocked abilities (from definitions).</param>
    /// <param name="xpToNextLevel">XP needed for the next level.</param>
    /// <param name="customRewards">Custom rewards for this level.</param>
    /// <param name="title">Milestone title for this level.</param>
    /// <param name="experienceTerminology">The term for experience.</param>
    /// <param name="levelTerminology">The term for level.</param>
    /// <returns>A DTO for display.</returns>
    public static LevelUpDto FromResult(
        LevelUpResult result,
        Stats oldStats,
        Stats newStats,
        IReadOnlyList<string>? abilityNames = null,
        int xpToNextLevel = 0,
        IReadOnlyList<string>? customRewards = null,
        string? title = null,
        string experienceTerminology = "XP",
        string levelTerminology = "Level")
    {
        return new LevelUpDto
        {
            OldLevel = result.OldLevel,
            NewLevel = result.NewLevel,
            LevelsGained = result.LevelsGained,
            OldMaxHealth = oldStats.MaxHealth,
            NewMaxHealth = newStats.MaxHealth,
            OldAttack = oldStats.Attack,
            NewAttack = newStats.Attack,
            OldDefense = oldStats.Defense,
            NewDefense = newStats.Defense,
            UnlockedAbilityNames = abilityNames ?? [],
            XpToNextLevel = xpToNextLevel,
            CustomRewards = customRewards ?? [],
            Title = title,
            ExperienceTerminology = experienceTerminology,
            LevelTerminology = levelTerminology
        };
    }
}
