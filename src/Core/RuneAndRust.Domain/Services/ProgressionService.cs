using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Services;

/// <summary>
/// Service responsible for detecting and applying player level-ups.
/// </summary>
/// <remarks>
/// The ProgressionService checks if a player has accumulated enough experience
/// to level up, calculates stat increases, and applies them to the player.
/// It supports multi-level gains (gaining multiple levels from one XP award)
/// and uses configurable progression settings from <see cref="ProgressionDefinition"/>.
/// </remarks>
public class ProgressionService
{
    private readonly ILogger<ProgressionService> _logger;
    private readonly ProgressionDefinition _progression;

    /// <summary>
    /// Creates a new ProgressionService instance with default progression settings.
    /// </summary>
    /// <param name="logger">Logger for diagnostics.</param>
    public ProgressionService(ILogger<ProgressionService> logger)
        : this(logger, null)
    {
    }

    /// <summary>
    /// Creates a new ProgressionService instance with custom progression settings.
    /// </summary>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <param name="progression">The progression configuration to use, or null for defaults.</param>
    public ProgressionService(ILogger<ProgressionService> logger, ProgressionDefinition? progression)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _progression = progression ?? ProgressionDefinition.Default;

        _logger.LogDebug(
            "ProgressionService initialized - MaxLevel: {MaxLevel}, Curve: {Curve}, " +
            "BaseXP: {BaseXP}, {XPTerm}/{LevelTerm} terminology",
            _progression.MaxLevel,
            _progression.CurveType,
            _progression.BaseXpRequirement,
            _progression.ExperienceTerminology,
            _progression.LevelTerminology);
    }

    /// <summary>
    /// Gets the progression configuration.
    /// </summary>
    public ProgressionDefinition Progression => _progression;

    /// <summary>
    /// Checks if the player should level up based on their current experience.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="classGrowthRates">Optional class-specific growth rates.</param>
    /// <returns>A result indicating whether and how many levels were gained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when player is null.</exception>
    public LevelUpResult CheckForLevelUp(Player player, LevelStatModifiers? classGrowthRates = null)
    {
        ArgumentNullException.ThrowIfNull(player);

        var currentLevel = player.Level;

        // Check max level cap
        if (_progression.MaxLevel > 0 && currentLevel >= _progression.MaxLevel)
        {
            _logger.LogDebug(
                "Player {Name} at max {LevelTerm} {MaxLevel}",
                player.Name, _progression.LevelTerminology, _progression.MaxLevel);
            return LevelUpResult.None(currentLevel);
        }

        var expectedLevel = _progression.GetLevelForExperience(player.Experience);

        // Cap at max level
        if (_progression.MaxLevel > 0 && expectedLevel > _progression.MaxLevel)
        {
            expectedLevel = _progression.MaxLevel;
        }

        if (expectedLevel <= currentLevel)
        {
            _logger.LogDebug(
                "Player {Name} at {LevelTerm} {Level} with {XP} {XPTerm} - no level up pending",
                player.Name, _progression.LevelTerminology, currentLevel,
                player.Experience, _progression.ExperienceTerminology);
            return LevelUpResult.None(currentLevel);
        }

        var levelsToGain = expectedLevel - currentLevel;
        var statIncreases = GetStatIncreasesForLevels(currentLevel, expectedLevel, classGrowthRates);

        _logger.LogInformation(
            "{LevelTerm} up detected - Player {Name}: {LevelTerm} {OldLevel} -> {NewLevel} " +
            "(+{HP} HP, +{ATK} ATK, +{DEF} DEF)",
            _progression.LevelTerminology, player.Name, _progression.LevelTerminology,
            currentLevel, expectedLevel,
            statIncreases.MaxHealth, statIncreases.Attack, statIncreases.Defense);

        return new LevelUpResult(currentLevel, expectedLevel, statIncreases);
    }

    /// <summary>
    /// Applies a level-up result to the player, updating their level and stats.
    /// </summary>
    /// <param name="player">The player to level up.</param>
    /// <param name="result">The level-up result to apply.</param>
    /// <param name="getAbilitiesAtLevel">Optional function to get unlocked abilities for a level.</param>
    /// <returns>The updated result with any unlocked abilities.</returns>
    /// <exception cref="ArgumentNullException">Thrown when player is null.</exception>
    public LevelUpResult ApplyLevelUp(
        Player player,
        LevelUpResult result,
        Func<int, IReadOnlyList<string>>? getAbilitiesAtLevel = null)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (!result.DidLevelUp)
        {
            _logger.LogDebug("No level-up to apply for player {Name}", player.Name);
            return result;
        }

        var oldStats = player.Stats;

        // Update player level
        player.SetLevel(result.NewLevel);

        // Apply stat modifiers, using configured HealOnLevelUp setting
        player.ApplyLevelStatModifiers(result.StatIncreases, healToNewMax: _progression.HealOnLevelUp);

        _logger.LogInformation(
            "{LevelTerm} up applied - Player {Name} is now {LevelTerm} {Level}. " +
            "Stats: HP={HP}, ATK={ATK}, DEF={DEF}",
            _progression.LevelTerminology, player.Name, _progression.LevelTerminology,
            player.Level,
            player.Stats.MaxHealth, player.Stats.Attack, player.Stats.Defense);

        // Collect unlocked abilities across all gained levels
        var unlockedAbilities = new List<string>();
        if (getAbilitiesAtLevel != null)
        {
            for (var level = result.OldLevel + 1; level <= result.NewLevel; level++)
            {
                var abilities = getAbilitiesAtLevel(level);
                if (abilities.Count > 0)
                {
                    unlockedAbilities.AddRange(abilities);
                    _logger.LogInformation(
                        "Player {Name} unlocked abilities at {LevelTerm} {Level}: {Abilities}",
                        player.Name, _progression.LevelTerminology, level, string.Join(", ", abilities));
                }
            }
        }

        // Return updated result with abilities if any were unlocked
        if (unlockedAbilities.Count > 0)
        {
            return new LevelUpResult(
                result.OldLevel,
                result.NewLevel,
                result.StatIncreases,
                unlockedAbilities);
        }

        return result;
    }

    /// <summary>
    /// Checks for and applies any pending level-ups in a single operation.
    /// </summary>
    /// <param name="player">The player to check and update.</param>
    /// <param name="classGrowthRates">Optional class-specific growth rates.</param>
    /// <param name="getAbilitiesAtLevel">Optional function to get unlocked abilities for a level.</param>
    /// <returns>The level-up result (may indicate no level-up occurred).</returns>
    /// <exception cref="ArgumentNullException">Thrown when player is null.</exception>
    public LevelUpResult CheckAndApplyLevelUp(
        Player player,
        LevelStatModifiers? classGrowthRates = null,
        Func<int, IReadOnlyList<string>>? getAbilitiesAtLevel = null)
    {
        var result = CheckForLevelUp(player, classGrowthRates);

        if (!result.DidLevelUp)
        {
            return result;
        }

        return ApplyLevelUp(player, result, getAbilitiesAtLevel);
    }

    /// <summary>
    /// Gets the cumulative stat increases for gaining multiple levels.
    /// </summary>
    /// <param name="fromLevel">The starting level.</param>
    /// <param name="toLevel">The ending level.</param>
    /// <param name="classGrowthRates">Optional class-specific growth rates.</param>
    /// <returns>The total stat increases.</returns>
    public LevelStatModifiers GetStatIncreasesForLevels(
        int fromLevel,
        int toLevel,
        LevelStatModifiers? classGrowthRates = null)
    {
        if (toLevel <= fromLevel)
        {
            return LevelStatModifiers.Zero;
        }

        var total = LevelStatModifiers.Zero;

        for (var level = fromLevel + 1; level <= toLevel; level++)
        {
            var bonuses = _progression.GetStatBonusesForLevel(level, classGrowthRates);
            total = total.Add(bonuses);
        }

        return total;
    }

    /// <summary>
    /// Gets the stat increases for a given number of levels using default progression.
    /// </summary>
    /// <param name="levelsGained">The number of levels gained.</param>
    /// <returns>The combined stat increases.</returns>
    public static LevelStatModifiers GetStatIncreasesForLevels(int levelsGained) =>
        LevelStatModifiers.ForLevels(levelsGained);

    /// <summary>
    /// Gets the experience points required to reach the next level.
    /// </summary>
    /// <param name="currentLevel">The current level.</param>
    /// <returns>The XP threshold for the next level.</returns>
    public int GetExperienceForNextLevel(int currentLevel)
    {
        if (_progression.MaxLevel > 0 && currentLevel >= _progression.MaxLevel)
        {
            return 0;
        }

        return _progression.GetExperienceForLevel(currentLevel + 1);
    }

    /// <summary>
    /// Gets how much more experience is needed to reach the next level.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>The remaining XP needed, or 0 if at max level.</returns>
    /// <exception cref="ArgumentNullException">Thrown when player is null.</exception>
    public int GetExperienceUntilNextLevel(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (_progression.MaxLevel > 0 && player.Level >= _progression.MaxLevel)
        {
            return 0;
        }

        var xpForNext = _progression.GetExperienceForLevel(player.Level + 1);
        var remaining = xpForNext - player.Experience;
        return Math.Max(0, remaining);
    }

    /// <summary>
    /// Gets custom rewards for a specific level, if any.
    /// </summary>
    /// <param name="level">The level to check.</param>
    /// <returns>List of reward descriptions, or empty if none.</returns>
    public IReadOnlyList<string> GetCustomRewardsForLevel(int level)
    {
        if (_progression.LevelOverrides.TryGetValue(level, out var levelDef))
        {
            return levelDef.CustomRewards;
        }

        return [];
    }

    /// <summary>
    /// Gets the milestone title for a level, if any.
    /// </summary>
    /// <param name="level">The level to check.</param>
    /// <returns>The title or null if none defined.</returns>
    public string? GetTitleForLevel(int level)
    {
        if (_progression.LevelOverrides.TryGetValue(level, out var levelDef))
        {
            return levelDef.Title;
        }

        return null;
    }
}
