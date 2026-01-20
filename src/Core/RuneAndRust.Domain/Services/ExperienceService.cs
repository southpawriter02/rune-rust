using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Services;

/// <summary>
/// Service for managing experience point operations.
/// </summary>
/// <remarks>
/// <para>Handles the logic for awarding experience points to players.
/// Level-up detection and stat increases are handled by ProgressionService in v0.0.8b.</para>
/// <para>Key responsibilities:</para>
/// <list type="bullet">
/// <item>Award XP from monster defeats</item>
/// <item>Track XP gains for logging/display</item>
/// <item>Validate XP amounts</item>
/// </list>
/// </remarks>
public class ExperienceService
{
    private readonly ILogger<ExperienceService> _logger;

    /// <summary>
    /// Creates a new ExperienceService instance.
    /// </summary>
    /// <param name="logger">The logger for experience operations.</param>
    public ExperienceService(ILogger<ExperienceService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Awards experience points to a player from defeating a monster.
    /// </summary>
    /// <param name="player">The player receiving experience.</param>
    /// <param name="monster">The defeated monster.</param>
    /// <returns>The result of the experience gain.</returns>
    /// <exception cref="ArgumentNullException">Thrown when player or monster is null.</exception>
    /// <remarks>
    /// Only awards XP if the monster is actually defeated (IsDefeated == true).
    /// Returns ExperienceGainResult.None if the monster is still alive.
    /// </remarks>
    public ExperienceGainResult AwardExperienceFromMonster(Player player, Monster monster)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(monster);

        if (!monster.IsDefeated)
        {
            _logger.LogWarning(
                "Attempted to award XP from non-defeated monster {MonsterName}",
                monster.Name);
            return ExperienceGainResult.None(player.Experience);
        }

        if (monster.ExperienceValue <= 0)
        {
            _logger.LogDebug(
                "Monster {MonsterName} has no XP value",
                monster.Name);
            return ExperienceGainResult.None(player.Experience);
        }

        return AwardExperience(player, monster.ExperienceValue, $"Defeated {monster.Name}");
    }

    /// <summary>
    /// Awards a specific amount of experience points to a player.
    /// </summary>
    /// <param name="player">The player to award XP to.</param>
    /// <param name="amount">The amount of XP to award.</param>
    /// <param name="source">A description of where the XP came from.</param>
    /// <returns>The result of the XP award operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when player is null.</exception>
    /// <remarks>
    /// For zero or negative amounts, returns ExperienceGainResult.None.
    /// </remarks>
    public ExperienceGainResult AwardExperience(Player player, int amount, string source)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (amount <= 0)
        {
            _logger.LogDebug("No XP to award (amount: {Amount})", amount);
            return ExperienceGainResult.None(player.Experience);
        }

        var previousXp = player.Experience;
        var newTotal = player.AddExperience(amount);

        _logger.LogInformation(
            "Player {PlayerName} gained {Amount} XP from {Source} (Total: {PreviousXp} -> {NewTotal})",
            player.Name, amount, source, previousXp, newTotal);

        return new ExperienceGainResult(amount, newTotal, previousXp, source);
    }

    /// <summary>
    /// Gets the default XP value for a monster by name.
    /// </summary>
    /// <param name="monsterName">The name of the monster.</param>
    /// <returns>The default XP value.</returns>
    /// <remarks>
    /// This is a placeholder method for looking up XP values.
    /// Will be replaced with configuration-based lookup in v0.0.8c.
    /// </remarks>
    public static int GetDefaultMonsterXp(string monsterName)
    {
        return monsterName.ToLowerInvariant() switch
        {
            "goblin" => 25,
            "skeleton" => 20,
            "orc" => 40,
            "goblin shaman" => 30,
            "slime" => 15,
            _ => 10 // Default for unknown monsters
        };
    }
}
