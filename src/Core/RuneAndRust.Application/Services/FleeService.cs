using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for handling flee attempts during combat.
/// </summary>
/// <remarks>
/// <para>FleeService manages the flee command mechanics including:</para>
/// <list type="bullet">
/// <item>Calculating flee difficulty class based on enemy count</item>
/// <item>Performing the Finesse-based skill check</item>
/// <item>Processing opportunity attacks on failure</item>
/// <item>Handling combat end on success</item>
/// </list>
/// </remarks>
public class FleeService
{
    private readonly IDiceService _diceService;
    private readonly ILogger<FleeService> _logger;

    /// <summary>
    /// Base difficulty class for flee attempts.
    /// </summary>
    public const int BaseFleeDC = 12;

    /// <summary>
    /// Additional DC per active enemy.
    /// </summary>
    public const int DCPerEnemy = 2;

    /// <summary>
    /// Damage multiplier for opportunity attacks (50% of normal damage).
    /// </summary>
    public const float OpportunityDamageMultiplier = 0.5f;

    /// <summary>
    /// Creates a new FleeService instance.
    /// </summary>
    /// <param name="diceService">The dice service for skill checks.</param>
    /// <param name="logger">Logger for flee attempt diagnostics.</param>
    public FleeService(IDiceService diceService, ILogger<FleeService> logger)
    {
        _diceService = diceService ?? throw new ArgumentNullException(nameof(diceService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Calculates the difficulty class for a flee attempt.
    /// </summary>
    /// <param name="encounter">The current combat encounter.</param>
    /// <returns>The DC to beat for a successful flee.</returns>
    public int CalculateFleeDC(CombatEncounter encounter)
    {
        ArgumentNullException.ThrowIfNull(encounter);
        var enemyCount = encounter.ActiveMonsterCount;
        var dc = BaseFleeDC + (DCPerEnemy * enemyCount);

        _logger.LogDebug("Flee DC calculated: {DC} (base {Base} + {Count} enemies x {PerEnemy})",
            dc, BaseFleeDC, enemyCount, DCPerEnemy);

        return dc;
    }

    /// <summary>
    /// Attempts to flee from combat.
    /// </summary>
    /// <param name="player">The player attempting to flee.</param>
    /// <param name="encounter">The current combat encounter.</param>
    /// <returns>The result of the flee attempt.</returns>
    public FleeAttemptResult AttemptFlee(Player player, CombatEncounter encounter)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(encounter);

        var dc = CalculateFleeDC(encounter);
        var modifier = player.Attributes.Finesse;

        // Roll 1d10 + Finesse
        var roll = _diceService.Roll(DicePool.D10());
        var total = roll.Total + modifier;

        // Create skill check result
        var skillCheck = new SkillCheckResult(
            skillId: "flee",
            skillName: "Flee",
            diceResult: roll,
            attributeBonus: modifier,
            otherBonus: 0,
            difficultyClass: dc,
            difficultyName: "Flee DC");

        _logger.LogInformation(
            "Flee attempt: {Roll} + {Mod} = {Total} vs DC {DC}",
            roll.Total, modifier, total, dc);

        if (skillCheck.IsSuccess)
        {
            _logger.LogInformation("Flee successful! Returning to previous room {RoomId}",
                encounter.PreviousRoomId);

            // End combat by flee
            encounter.EndByFlee();

            return FleeAttemptResult.Succeeded(
                skillCheck,
                dc,
                encounter.PreviousRoomId ?? encounter.RoomId);
        }

        // Failure - process opportunity attacks
        var opportunityAttacks = ProcessOpportunityAttacks(player, encounter);
        var totalDamage = opportunityAttacks.Sum(a => a.Damage);

        _logger.LogInformation(
            "Flee failed! Player takes {Damage} damage from {Count} opportunity attacks",
            totalDamage, opportunityAttacks.Count);

        return FleeAttemptResult.Failed(
            skillCheck,
            dc,
            opportunityAttacks,
            totalDamage);
    }

    /// <summary>
    /// Processes opportunity attacks from all active monsters.
    /// </summary>
    private IReadOnlyList<OpportunityAttackResult> ProcessOpportunityAttacks(
        Player player,
        CombatEncounter encounter)
    {
        var attacks = new List<OpportunityAttackResult>();

        foreach (var monster in encounter.GetActiveMonsters())
        {
            if (monster.Monster == null) continue;

            // Opportunity attack deals 50% of monster's attack stat as damage
            var rawDamage = (int)(monster.Monster.Stats.Attack * OpportunityDamageMultiplier);
            var actualDamage = player.TakeDamage(rawDamage);

            attacks.Add(new OpportunityAttackResult(
                monster.DisplayName,
                actualDamage,
                Hit: true));

            _logger.LogDebug(
                "Opportunity attack from {Monster}: {Damage} damage",
                monster.DisplayName, actualDamage);
        }

        return attacks.AsReadOnly();
    }
}
