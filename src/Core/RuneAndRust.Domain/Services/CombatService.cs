using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Domain.Services;

/// <summary>
/// Represents the outcome of a single round of combat.
/// </summary>
/// <param name="DamageDealt">The amount of damage the player dealt to the monster.</param>
/// <param name="DamageReceived">The amount of damage the player received from the monster.</param>
/// <param name="MonsterDefeated">True if the monster was defeated this round.</param>
/// <param name="PlayerDefeated">True if the player was defeated this round.</param>
public record CombatResult(
    int DamageDealt,
    int DamageReceived,
    bool MonsterDefeated,
    bool PlayerDefeated
);

/// <summary>
/// Represents the outcome of a monster's attack on a player.
/// </summary>
/// <param name="Damage">The amount of damage dealt to the player.</param>
/// <param name="PlayerDefeated">True if the player was defeated by this attack.</param>
public record MonsterAttackResult(
    int Damage,
    bool PlayerDefeated
);

/// <summary>
/// Handles combat resolution between players and monsters.
/// </summary>
/// <remarks>
/// The CombatService implements a turn-based combat system where the player
/// always attacks first. Damage is calculated based on attacker's attack stat
/// versus defender's defense stat, with some random variance applied.
/// </remarks>
public class CombatService
{
    /// <summary>
    /// Random number generator for damage variance calculations.
    /// </summary>
    private readonly Random _random = new();

    /// <summary>
    /// Logger instance for combat diagnostics.
    /// </summary>
    private readonly ILogger<CombatService> _logger;

    /// <summary>
    /// Creates a new combat service instance.
    /// </summary>
    /// <param name="logger">Optional logger for combat diagnostics. If null, a no-op logger is used.</param>
    public CombatService(ILogger<CombatService>? logger = null)
    {
        _logger = logger ?? NullLogger<CombatService>.Instance;
        _logger.LogDebug("CombatService initialized");
    }

    /// <summary>
    /// Resolves a single round of combat between a player and a monster.
    /// </summary>
    /// <param name="player">The player engaging in combat.</param>
    /// <param name="monster">The monster being fought.</param>
    /// <returns>A <see cref="CombatResult"/> containing the outcome of the combat round.</returns>
    /// <exception cref="ArgumentNullException">Thrown when player or monster is null.</exception>
    /// <remarks>
    /// Combat proceeds as follows:
    /// <list type="number">
    /// <item>Player attacks the monster, dealing damage based on attack vs defense.</item>
    /// <item>If the monster survives, it counterattacks the player.</item>
    /// <item>The result indicates damage dealt/received and whether either combatant was defeated.</item>
    /// </list>
    /// </remarks>
    public CombatResult ResolveCombatRound(Player player, Monster monster)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(monster);

        _logger.LogDebug(
            "Resolving combat round - Player: {PlayerName} (HP:{PlayerHP}, ATK:{PlayerATK}, DEF:{PlayerDEF}) vs " +
            "Monster: {MonsterName} (HP:{MonsterHP}, ATK:{MonsterATK}, DEF:{MonsterDEF})",
            player.Name,
            player.Health,
            player.Stats.Attack,
            player.Stats.Defense,
            monster.Name,
            monster.Health,
            monster.Stats.Attack,
            monster.Stats.Defense);

        // Player attacks first
        var playerDamage = CalculateDamage(player.Stats.Attack, monster.Stats.Defense);
        var actualPlayerDamage = monster.TakeDamage(playerDamage);
        _logger.LogDebug(
            "Player attack: Calculated {CalculatedDamage}, Actual {ActualDamage} (Monster DEF: {MonsterDef})",
            playerDamage,
            actualPlayerDamage,
            monster.Stats.Defense);

        var monsterDamage = 0;
        var actualMonsterDamage = 0;

        // Monster attacks if still alive
        if (monster.IsAlive)
        {
            monsterDamage = CalculateDamage(monster.Stats.Attack, player.Stats.Defense);
            actualMonsterDamage = player.TakeDamage(monsterDamage);
            _logger.LogDebug(
                "Monster counterattack: Calculated {CalculatedDamage}, Actual {ActualDamage} (Player DEF: {PlayerDef})",
                monsterDamage,
                actualMonsterDamage,
                player.Stats.Defense);
        }
        else
        {
            _logger.LogDebug("Monster defeated before counterattack");
        }

        var result = new CombatResult(
            DamageDealt: actualPlayerDamage,
            DamageReceived: actualMonsterDamage,
            MonsterDefeated: monster.IsDefeated,
            PlayerDefeated: player.IsDead
        );

        _logger.LogInformation(
            "Combat round complete - Dealt: {DamageDealt}, Received: {DamageReceived}, " +
            "MonsterDefeated: {MonsterDefeated}, PlayerDefeated: {PlayerDefeated}",
            result.DamageDealt,
            result.DamageReceived,
            result.MonsterDefeated,
            result.PlayerDefeated);

        return result;
    }

    /// <summary>
    /// Calculates the damage dealt based on attack and defense values.
    /// </summary>
    /// <param name="attack">The attacker's attack stat.</param>
    /// <param name="defense">The defender's defense stat.</param>
    /// <returns>The final damage amount (minimum 1).</returns>
    /// <remarks>
    /// Damage formula: max(1, attack - defense) + random(-2 to +2).
    /// The final result is always at least 1 damage.
    /// </remarks>
    private int CalculateDamage(int attack, int defense)
    {
        // Simple damage calculation with some randomness
        var baseDamage = Math.Max(1, attack - defense);
        var variance = _random.Next(-2, 3); // -2 to +2 variance
        var finalDamage = Math.Max(1, baseDamage + variance);

        _logger.LogDebug(
            "Damage calculation: ATK {Attack} - DEF {Defense} = Base {BaseDamage}, Variance {Variance}, Final {FinalDamage}",
            attack,
            defense,
            baseDamage,
            variance,
            finalDamage);

        return finalDamage;
    }

    /// <summary>
    /// Resolves a monster's attack against a player.
    /// </summary>
    /// <param name="monster">The attacking monster.</param>
    /// <param name="player">The player being attacked.</param>
    /// <returns>A <see cref="MonsterAttackResult"/> containing the damage dealt.</returns>
    public MonsterAttackResult MonsterAttack(Monster monster, Player player)
    {
        ArgumentNullException.ThrowIfNull(monster);
        ArgumentNullException.ThrowIfNull(player);

        if (!monster.IsAlive)
        {
            _logger.LogDebug("Monster is dead, cannot attack");
            return new MonsterAttackResult(0, false);
        }

        var damage = CalculateDamage(monster.Stats.Attack, player.Stats.Defense);
        var actualDamage = player.TakeDamage(damage);

        _logger.LogDebug(
            "Monster attack: {MonsterName} dealt {Damage} damage to {PlayerName}",
            monster.Name, actualDamage, player.Name);

        return new MonsterAttackResult(actualDamage, player.IsDead);
    }

    /// <summary>
    /// Generates a description of a monster's attack.
    /// </summary>
    /// <param name="result">The attack result.</param>
    /// <param name="monsterName">The name of the monster.</param>
    /// <param name="playerName">The name of the player.</param>
    /// <returns>A description of the attack.</returns>
    public string GetMonsterAttackDescription(MonsterAttackResult result, string monsterName, string playerName)
    {
        if (result.Damage == 0)
            return $"The {monsterName} misses!";

        var description = $"The {monsterName} strikes back for {result.Damage} damage!";

        if (result.PlayerDefeated)
            description += $" {playerName} has fallen in battle...";

        return description;
    }

    /// <summary>
    /// Generates a human-readable description of a combat round's outcome.
    /// </summary>
    /// <param name="result">The combat result to describe.</param>
    /// <param name="playerName">The name of the player for the description.</param>
    /// <param name="monsterName">The name of the monster for the description.</param>
    /// <returns>A multi-line string describing what happened during combat.</returns>
    public string GetCombatDescription(CombatResult result, string playerName, string monsterName)
    {
        _logger.LogDebug(
            "Generating combat description for {PlayerName} vs {MonsterName}",
            playerName,
            monsterName);

        var lines = new List<string>();

        if (result.DamageDealt > 0)
        {
            lines.Add($"{playerName} attacks the {monsterName} for {result.DamageDealt} damage!");
        }

        if (result.MonsterDefeated)
        {
            lines.Add($"The {monsterName} has been defeated!");
        }
        else if (result.DamageReceived > 0)
        {
            lines.Add($"The {monsterName} strikes back for {result.DamageReceived} damage!");
        }

        if (result.PlayerDefeated)
        {
            lines.Add($"{playerName} has fallen in battle...");
        }

        return string.Join(Environment.NewLine, lines);
    }
}
