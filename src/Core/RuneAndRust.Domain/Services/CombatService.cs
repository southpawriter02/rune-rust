using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Services;

/// <summary>
/// Represents the outcome of a single round of combat (legacy simple result).
/// </summary>
/// <remarks>
/// Maintained for backwards compatibility. Consider using <see cref="CombatRoundResult"/>
/// for detailed dice breakdown.
/// </remarks>
public record CombatResult(
    int DamageDealt,
    int DamageReceived,
    bool MonsterDefeated,
    bool PlayerDefeated
);

/// <summary>
/// Represents the outcome of a monster's attack on a player (legacy simple result).
/// </summary>
public record MonsterAttackResult(
    int Damage,
    bool PlayerDefeated
);

/// <summary>
/// Handles combat resolution between players and monsters using dice-based mechanics.
/// </summary>
/// <remarks>
/// <para>The CombatService implements a turn-based combat system where the player
/// always attacks first. Attack and damage are determined by dice rolls:</para>
/// <list type="bullet">
///   <item><description>Attack: 1d10 + Finesse vs target Defense</description></item>
///   <item><description>Damage: weapon dice (default 1d6) + Might - armor</description></item>
///   <item><description>Critical hit on natural 10: always hits, double damage dice</description></item>
///   <item><description>Critical miss on natural 1: always misses</description></item>
/// </list>
/// </remarks>
public class CombatService
{
    /// <summary>
    /// Default damage dice for unarmed combat.
    /// </summary>
    public static readonly DicePool UnarmedDamageDice = DicePool.D4();

    /// <summary>
    /// Default damage dice when no weapon system is active (fallback).
    /// </summary>
    public static readonly DicePool DefaultWeaponDice = DicePool.D6();

    private readonly Random _random = new();
    private readonly ILogger<CombatService> _logger;

    private static readonly DicePool DefaultPlayerDamagePool = DicePool.D6();
    private static readonly DicePool DefaultMonsterDamagePool = DicePool.D6();

    /// <summary>
    /// Creates a new combat service instance.
    /// </summary>
    public CombatService(ILogger<CombatService>? logger = null)
    {
        _logger = logger ?? NullLogger<CombatService>.Instance;
        _logger.LogDebug("CombatService initialized");
    }

    // ===== DICE-BASED COMBAT (NEW) =====

    /// <summary>
    /// Resolves a single round of combat using dice-based mechanics.
    /// </summary>
    /// <param name="player">The player engaging in combat.</param>
    /// <param name="monster">The monster being fought.</param>
    /// <param name="diceService">The dice rolling service.</param>
    /// <returns>A <see cref="CombatRoundResult"/> with detailed dice breakdown.</returns>
    public CombatRoundResult ResolveCombatRound(Player player, Monster monster, IDiceService diceService)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(monster);
        ArgumentNullException.ThrowIfNull(diceService);

        _logger.LogDebug(
            "Resolving dice-based combat - Player: {PlayerName} (HP:{HP}, Finesse:{Fin}, Might:{Might}) vs " +
            "Monster: {MonsterName} (HP:{MHP}, ATK:{ATK}, DEF:{DEF})",
            player.Name, player.Health, player.Attributes.Finesse, player.Attributes.Might,
            monster.Name, monster.Health, monster.Stats.Attack, monster.Stats.Defense);

        // Player Attack Phase
        var playerAttack = ResolvePlayerAttack(player, monster, diceService);

        // Apply player damage
        if (playerAttack.IsHit && playerAttack.DamageDealt > 0)
        {
            monster.TakeDamage(playerAttack.DamageDealt);
            _logger.LogDebug("Player dealt {Damage} to {Monster} (HP: {HP})",
                playerAttack.DamageDealt, monster.Name, monster.Health);
        }

        // Monster Counterattack Phase
        MonsterCounterAttackResult? monsterCounterAttack = null;
        if (monster.IsAlive)
        {
            monsterCounterAttack = ResolveMonsterCounterAttack(monster, player, diceService);
            if (monsterCounterAttack.IsHit && monsterCounterAttack.DamageDealt > 0)
            {
                player.TakeDamage(monsterCounterAttack.DamageDealt);
                _logger.LogDebug("{Monster} dealt {Damage} to {Player} (HP: {HP})",
                    monster.Name, monsterCounterAttack.DamageDealt, player.Name, player.Health);
            }
        }

        var result = new CombatRoundResult(
            attackRoll: playerAttack.AttackRoll,
            attackTotal: playerAttack.AttackTotal,
            isHit: playerAttack.IsHit,
            isCriticalHit: playerAttack.IsCriticalHit,
            isCriticalMiss: playerAttack.IsCriticalMiss,
            damageRoll: playerAttack.DamageRoll,
            damageDealt: playerAttack.DamageDealt,
            monsterCounterAttack: monsterCounterAttack,
            monsterDefeated: monster.IsDefeated,
            playerDefeated: player.IsDead,
            experienceGained: monster.IsDefeated ? monster.ExperienceValue : 0);

        LogCombatRoundResult(result, player.Name, monster.Name);
        return result;
    }

    private (DiceRollResult AttackRoll, int AttackTotal, bool IsHit, bool IsCriticalHit, bool IsCriticalMiss, DiceRollResult? DamageRoll, int DamageDealt)
        ResolvePlayerAttack(Player player, Monster monster, IDiceService diceService)
    {
        // Get weapon bonuses
        var weaponBonuses = GetWeaponBonuses(player);
        var weaponAttackMod = weaponBonuses.AttackModifier;

        // Calculate effective Finesse (base + weapon bonus)
        var effectiveFinesse = player.Attributes.Finesse + weaponBonuses.Finesse;

        var attackRoll = diceService.Roll(DicePool.D10());
        var attackTotal = attackRoll.Total + effectiveFinesse + weaponAttackMod;

        // Combat uses sum-based mechanics for damage; suppress obsolete warnings
#pragma warning disable CS0618 // IsNaturalMax/IsNaturalOne intentionally used for damage rolls
        var isCriticalHit = attackRoll.IsNaturalMax;
        var isCriticalMiss = attackRoll.IsNaturalOne;
#pragma warning restore CS0618
        var isHit = !isCriticalMiss && (attackTotal >= monster.Stats.Defense || isCriticalHit);

        _logger.LogDebug(
            "Player attack: [{Roll}] + {Finesse} + {WeaponMod} = {Total} vs DEF {Def} -> {Result}",
            attackRoll.Rolls[0], effectiveFinesse, weaponAttackMod, attackTotal, monster.Stats.Defense,
            isCriticalHit ? "CRITICAL HIT!" : isCriticalMiss ? "CRITICAL MISS!" : isHit ? "Hit" : "Miss");

        DiceRollResult? damageRoll = null;
        int damageDealt = 0;

        if (isHit)
        {
            var damagePool = GetPlayerDamagePool(player);
            if (isCriticalHit)
            {
                damagePool = damagePool with { Count = damagePool.Count * 2 };
                _logger.LogDebug("Critical hit! Damage dice doubled to {Pool}", damagePool);
            }

            damageRoll = diceService.Roll(damagePool);

            // Calculate effective Might (base + weapon bonus)
            var effectiveMight = player.Attributes.Might + weaponBonuses.Might;
            var rawDamage = damageRoll.Value.Total + effectiveMight;
            var armorReduction = monster.Stats.Defense / 2;
            damageDealt = Math.Max(1, rawDamage - armorReduction);

            _logger.LogDebug("Player damage: [{Rolls}] + {Might} = {Raw} - {Armor} = {Final}",
                string.Join(",", damageRoll.Value.Rolls), effectiveMight, rawDamage, armorReduction, damageDealt);
        }

        return (attackRoll, attackTotal, isHit, isCriticalHit, isCriticalMiss, damageRoll, damageDealt);
    }

    private MonsterCounterAttackResult ResolveMonsterCounterAttack(Monster monster, Player player, IDiceService diceService)
    {
        var attackRoll = diceService.Roll(DicePool.D10());
        var attackModifier = monster.Stats.Attack;
        var attackTotal = attackRoll.Total + attackModifier;

        // Combat uses sum-based mechanics for damage; suppress obsolete warnings
#pragma warning disable CS0618 // IsNaturalMax/IsNaturalOne intentionally used for damage rolls
        var isCriticalHit = attackRoll.IsNaturalMax;
        var isCriticalMiss = attackRoll.IsNaturalOne;
#pragma warning restore CS0618
        var isHit = !isCriticalMiss && (attackTotal >= player.Stats.Defense || isCriticalHit);

        _logger.LogDebug(
            "Monster attack: [{Roll}] + {Mod} = {Total} vs DEF {Def} -> {Result}",
            attackRoll.Rolls[0], attackModifier, attackTotal, player.Stats.Defense,
            isCriticalHit ? "CRITICAL HIT!" : isCriticalMiss ? "CRITICAL MISS!" : isHit ? "Hit" : "Miss");

        DiceRollResult? damageRoll = null;
        int damageDealt = 0;

        if (isHit)
        {
            var damagePool = GetMonsterDamagePool(monster);
            if (isCriticalHit)
            {
                damagePool = damagePool with { Count = damagePool.Count * 2 };
            }

            damageRoll = diceService.Roll(damagePool);
            var rawDamage = damageRoll.Value.Total;
            var armorReduction = player.Stats.Defense / 2;
            damageDealt = Math.Max(1, rawDamage - armorReduction);

            _logger.LogDebug("Monster damage: [{Rolls}] = {Raw} - {Armor} = {Final}",
                string.Join(",", damageRoll.Value.Rolls), rawDamage, armorReduction, damageDealt);
        }

        return new MonsterCounterAttackResult(
            attackRoll, attackTotal, isHit, isCriticalHit, isCriticalMiss,
            damageRoll, damageDealt, player.IsDead);
    }

    /// <summary>
    /// Gets the damage dice pool for a player based on their equipped weapon.
    /// </summary>
    /// <param name="player">The player to get damage dice for.</param>
    /// <returns>The appropriate DicePool for the player's attack.</returns>
    /// <remarks>
    /// Priority:
    /// 1. Equipped weapon's DamageDice
    /// 2. UnarmedDamageDice (1d4) if no weapon equipped
    /// </remarks>
    public DicePool GetPlayerDamageDice(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        var equippedWeapon = player.GetEquippedItem(EquipmentSlot.Weapon);

        if (equippedWeapon == null)
        {
            _logger.LogDebug("Player {Player} has no weapon equipped, using unarmed dice: {Dice}",
                player.Name, UnarmedDamageDice);
            return UnarmedDamageDice;
        }

        var weaponDice = equippedWeapon.GetDamageDicePool();
        if (weaponDice == null)
        {
            _logger.LogWarning(
                "Equipped weapon {Weapon} has no damage dice, using default: {Dice}",
                equippedWeapon.Name, DefaultWeaponDice);
            return DefaultWeaponDice;
        }

        _logger.LogDebug("Player {Player} using weapon {Weapon} with dice: {Dice}",
            player.Name, equippedWeapon.Name, weaponDice);

        return weaponDice.Value;
    }

    /// <summary>
    /// Gets the equipped weapon's name for combat messages.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>The weapon name, or "fists" if unarmed.</returns>
    public string GetPlayerWeaponName(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        var weapon = player.GetEquippedItem(EquipmentSlot.Weapon);
        return weapon?.Name ?? "fists";
    }

    /// <summary>
    /// Gets the attack roll modifier from the player's equipped weapon.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>The attack modifier from weapon bonuses.</returns>
    public int GetWeaponAttackModifier(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        var weapon = player.GetEquippedItem(EquipmentSlot.Weapon);
        return weapon?.WeaponBonuses.AttackModifier ?? 0;
    }

    /// <summary>
    /// Gets total attribute bonuses from the player's equipped weapon.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>The weapon bonuses, or None if no weapon equipped.</returns>
    public WeaponBonuses GetWeaponBonuses(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        var weapon = player.GetEquippedItem(EquipmentSlot.Weapon);
        return weapon?.WeaponBonuses ?? WeaponBonuses.None;
    }

    private DicePool GetPlayerDamagePool(Player player) => GetPlayerDamageDice(player);
    private DicePool GetMonsterDamagePool(Monster monster) => DefaultMonsterDamagePool;

    private void LogCombatRoundResult(CombatRoundResult result, string playerName, string monsterName)
    {
        var attackResult = result.IsCriticalHit ? "Critical Hit" :
                          result.IsCriticalMiss ? "Critical Miss" :
                          result.IsHit ? "Hit" : "Miss";

        _logger.LogInformation(
            "Combat: {Player} {Result} [{Roll}]+{Mod}={Total}, Dealt:{Dealt}, Received:{Recv}, MonsterDef:{MD}, PlayerDef:{PD}",
            playerName, attackResult, result.AttackRoll.Rolls[0],
            result.AttackTotal - result.AttackRoll.Total, result.AttackTotal,
            result.DamageDealt, result.DamageReceived, result.MonsterDefeated, result.PlayerDefeated);
    }

    /// <summary>
    /// Generates a description of a dice-based combat round.
    /// </summary>
    public string GetCombatDescription(CombatRoundResult result, string playerName, string monsterName)
    {
        var lines = new List<string>();

        if (result.IsCriticalHit)
        {
            lines.Add($"{playerName} lands a CRITICAL HIT on the {monsterName}!");
            if (result.DamageDealt > 0)
                lines.Add($"Rolling double damage dice: {result.DamageDealt} damage!");
        }
        else if (result.IsCriticalMiss)
        {
            lines.Add($"{playerName} fumbles! The attack goes wide.");
        }
        else if (result.IsHit)
        {
            lines.Add($"{playerName} hits the {monsterName} for {result.DamageDealt} damage!");
        }
        else
        {
            lines.Add($"{playerName} swings at the {monsterName} but misses!");
        }

        if (result.MonsterDefeated)
        {
            lines.Add($"The {monsterName} has been defeated!");
        }
        else if (result.MonsterCounterAttack != null)
        {
            var counter = result.MonsterCounterAttack;
            if (counter.IsCriticalHit)
            {
                lines.Add($"The {monsterName} lands a CRITICAL HIT!");
                lines.Add($"Rolling double damage dice: {counter.DamageDealt} damage!");
            }
            else if (counter.IsCriticalMiss)
            {
                lines.Add($"The {monsterName} stumbles and misses!");
            }
            else if (counter.IsHit)
            {
                lines.Add($"The {monsterName} strikes back for {counter.DamageDealt} damage!");
            }
            else
            {
                lines.Add($"The {monsterName} attacks but {playerName} dodges!");
            }
        }

        if (result.PlayerDefeated)
        {
            lines.Add($"{playerName} has fallen in battle...");
        }

        return string.Join(Environment.NewLine, lines);
    }

    // ===== LIGHT PENALTY METHODS (v0.4.3a) =====

    /// <summary>
    /// Gets the accuracy penalty for the current light level.
    /// </summary>
    /// <param name="room">The room where combat is occurring.</param>
    /// <returns>The accuracy penalty (0 or negative).</returns>
    /// <remarks>
    /// In v0.4.3a, this returns the penalty based on room light level.
    /// In v0.4.3b, this will consider attacker's vision type.
    /// </remarks>
    public int GetLightPenalty(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);

        var lightLevel = room.CurrentLightLevel;
        var penalty = Constants.LightPenalties.GetAccuracyPenalty(lightLevel);

        if (penalty < 0)
        {
            _logger.LogDebug(
                "Light penalty of {Penalty} for {LightLevel} conditions in room {Room}",
                penalty, lightLevel, room.Name);
        }

        return penalty;
    }

    /// <summary>
    /// Gets the accuracy penalty for a player attacker based on light and vision.
    /// </summary>
    /// <param name="room">The room where combat is occurring.</param>
    /// <param name="attacker">The attacking player.</param>
    /// <returns>The accuracy penalty after vision modifiers.</returns>
    /// <remarks>
    /// Placeholder signature for v0.4.3b vision type integration.
    /// In v0.4.3a, this ignores the attacker parameter.
    /// </remarks>
    public int GetLightPenalty(Room room, Player attacker)
    {
        // v0.4.3a: Ignore attacker's vision type
        // v0.4.3b: Will check attacker.VisionType to mitigate penalties
        return GetLightPenalty(room);
    }

    /// <summary>
    /// Gets the accuracy penalty for a monster attacker based on light and vision.
    /// </summary>
    /// <param name="room">The room where combat is occurring.</param>
    /// <param name="attacker">The attacking monster.</param>
    /// <returns>The accuracy penalty after vision modifiers.</returns>
    /// <remarks>
    /// Placeholder signature for v0.4.3b vision type integration.
    /// In v0.4.3a, this ignores the attacker parameter.
    /// </remarks>
    public int GetLightPenalty(Room room, Monster attacker)
    {
        // v0.4.3a: Ignore attacker's vision type
        // v0.4.3b: Will check monster.VisionType and LightSensitivity
        return GetLightPenalty(room);
    }

    // ===== LEGACY METHODS (Backwards Compatibility) =====

    /// <summary>
    /// Resolves a single round of combat using legacy static calculations.
    /// </summary>
    [Obsolete("Use ResolveCombatRound(Player, Monster, IDiceService) for dice-based combat")]
    public CombatResult ResolveCombatRound(Player player, Monster monster)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(monster);

        _logger.LogDebug("Resolving legacy combat round - {Player} vs {Monster}", player.Name, monster.Name);

        var playerDamage = CalculateDamage(player.Stats.Attack, monster.Stats.Defense);
        var actualPlayerDamage = monster.TakeDamage(playerDamage);

        var actualMonsterDamage = 0;
        if (monster.IsAlive)
        {
            var monsterDamage = CalculateDamage(monster.Stats.Attack, player.Stats.Defense);
            actualMonsterDamage = player.TakeDamage(monsterDamage);
        }

        return new CombatResult(actualPlayerDamage, actualMonsterDamage, monster.IsDefeated, player.IsDead);
    }

    private int CalculateDamage(int attack, int defense)
    {
        var baseDamage = Math.Max(1, attack - defense);
        var variance = _random.Next(-2, 3);
        return Math.Max(1, baseDamage + variance);
    }

    /// <summary>
    /// Resolves a monster's attack against a player (legacy).
    /// </summary>
    [Obsolete("Use dice-based combat resolution")]
    public MonsterAttackResult MonsterAttack(Monster monster, Player player)
    {
        ArgumentNullException.ThrowIfNull(monster);
        ArgumentNullException.ThrowIfNull(player);

        if (!monster.IsAlive)
            return new MonsterAttackResult(0, false);

        var damage = CalculateDamage(monster.Stats.Attack, player.Stats.Defense);
        var actualDamage = player.TakeDamage(damage);
        return new MonsterAttackResult(actualDamage, player.IsDead);
    }

    /// <summary>
    /// Generates a description of a monster's attack (legacy).
    /// </summary>
    [Obsolete("Use GetCombatDescription with CombatRoundResult")]
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
    /// Generates a description of a combat round (legacy).
    /// </summary>
    [Obsolete("Use GetCombatDescription with CombatRoundResult")]
    public string GetCombatDescription(CombatResult result, string playerName, string monsterName)
    {
        var lines = new List<string>();

        if (result.DamageDealt > 0)
            lines.Add($"{playerName} attacks the {monsterName} for {result.DamageDealt} damage!");

        if (result.MonsterDefeated)
            lines.Add($"The {monsterName} has been defeated!");
        else if (result.DamageReceived > 0)
            lines.Add($"The {monsterName} strikes back for {result.DamageReceived} damage!");

        if (result.PlayerDefeated)
            lines.Add($"{playerName} has fallen in battle...");

        return string.Join(Environment.NewLine, lines);
    }
}
