using RuneAndRust.Core;

namespace RuneAndRust.Engine;

public enum EnemyAction
{
    BasicAttack,
    Defend,
    RapidStrike,
    HeavyStrike,
    BerserkStrike,
    ChargeDefense,
    EmergencyRepairs
}

public class EnemyAI
{
    private readonly Random _random;
    private readonly DiceService _diceService;

    public EnemyAI(DiceService diceService)
    {
        _diceService = diceService;
        _random = new Random();
    }

    public EnemyAI(DiceService diceService, int seed)
    {
        _diceService = diceService;
        _random = new Random(seed);
    }

    /// <summary>
    /// Determine what action an enemy should take
    /// </summary>
    public EnemyAction DetermineAction(Enemy enemy)
    {
        // Skip turn if stunned
        if (enemy.IsStunned)
        {
            return EnemyAction.BasicAttack; // Will be handled specially in execution
        }

        return enemy.Type switch
        {
            EnemyType.CorruptedServitor => DetermineServitorAction(),
            EnemyType.BlightDrone => DetermineDroneAction(),
            EnemyType.RuinWarden => DetermineWardenAction(enemy),
            _ => EnemyAction.BasicAttack
        };
    }

    private EnemyAction DetermineServitorAction()
    {
        var roll = _random.Next(100);

        if (roll < 80) // 80% chance
            return EnemyAction.BasicAttack;
        else // 20% chance
            return EnemyAction.Defend;
    }

    private EnemyAction DetermineDroneAction()
    {
        var roll = _random.Next(100);

        if (roll < 60) // 60% chance
            return EnemyAction.BasicAttack;
        else if (roll < 90) // 30% chance
            return EnemyAction.RapidStrike;
        else // 10% chance
            return EnemyAction.Defend;
    }

    private EnemyAction DetermineWardenAction(Enemy warden)
    {
        // Determine phase based on HP percentage
        var hpPercent = (double)warden.HP / warden.MaxHP;

        if (hpPercent > 0.5)
        {
            // Phase 1 (100%-50% HP)
            var roll = _random.Next(100);

            if (roll < 50) // 50% chance
                return EnemyAction.HeavyStrike;
            else if (roll < 80) // 30% chance
                return EnemyAction.BasicAttack;
            else // 20% chance
                return EnemyAction.ChargeDefense;
        }
        else
        {
            // Phase 2 (50%-0% HP)
            var roll = _random.Next(100);

            if (roll < 40) // 40% chance
                return EnemyAction.BerserkStrike;
            else if (roll < 80) // 40% chance
                return EnemyAction.HeavyStrike;
            else // 20% chance
                return EnemyAction.EmergencyRepairs;
        }
    }

    /// <summary>
    /// Execute an enemy's action
    /// </summary>
    public void ExecuteAction(Enemy enemy, EnemyAction action, PlayerCharacter player, CombatState combatState)
    {
        // Check if stunned
        if (enemy.IsStunned)
        {
            combatState.AddLogEntry($"{enemy.Name} is stunned and cannot act!");
            combatState.AddLogEntry("");
            return;
        }

        switch (action)
        {
            case EnemyAction.BasicAttack:
                ExecuteBasicAttack(enemy, player, combatState);
                break;

            case EnemyAction.Defend:
                ExecuteDefend(enemy, combatState);
                break;

            case EnemyAction.RapidStrike:
                ExecuteRapidStrike(enemy, player, combatState);
                break;

            case EnemyAction.HeavyStrike:
                ExecuteHeavyStrike(enemy, player, combatState);
                break;

            case EnemyAction.BerserkStrike:
                ExecuteBerserkStrike(enemy, player, combatState);
                break;

            case EnemyAction.ChargeDefense:
                ExecuteChargeDefense(enemy, combatState);
                break;

            case EnemyAction.EmergencyRepairs:
                ExecuteEmergencyRepairs(enemy, combatState);
                break;
        }
    }

    private void ExecuteBasicAttack(Enemy enemy, PlayerCharacter player, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} attacks {player.Name}!");

        var attackRoll = _diceService.Roll(enemy.Attributes.Might);
        combatState.AddLogEntry($"  Rolled {enemy.Attributes.Might}d6: {FormatRolls(attackRoll)} = {attackRoll.Successes} successes");

        // Check if player has dodge active
        if (combatState.PlayerNegateNextAttack)
        {
            combatState.AddLogEntry($"  {player.Name} dodges the attack completely!");
            combatState.PlayerNegateNextAttack = false;
            combatState.AddLogEntry("");
            return;
        }

        // Player defends
        var defendRoll = _diceService.Roll(player.Attributes.Sturdiness);
        combatState.AddLogEntry($"{player.Name} defends!");
        combatState.AddLogEntry($"  Rolled {player.Attributes.Sturdiness}d6: {FormatRolls(defendRoll)} = {defendRoll.Successes} successes");

        // Calculate damage
        var netSuccesses = Math.Max(0, attackRoll.Successes - defendRoll.Successes);
        var baseDamage = netSuccesses + _diceService.RollDamage(enemy.BaseDamageDice) + enemy.DamageBonus;
        var damage = baseDamage;

        // Apply player defense bonus
        if (player.DefenseTurnsRemaining > 0)
        {
            var reducedDamage = (int)(damage * (1 - player.DefenseBonus / 100.0));
            combatState.AddLogEntry($"{player.Name}'s defense reduces damage from {damage} to {reducedDamage}");
            damage = reducedDamage;
        }

        // Apply damage through shield and battle rage modifiers
        ApplyDamageToPlayer(player, damage, combatState);

        combatState.AddLogEntry("");
    }

    private void ApplyDamageToPlayer(PlayerCharacter player, int damage, CombatState combatState, string indent = "  ")
    {
        if (damage <= 0)
        {
            combatState.AddLogEntry($"{indent}The attack is deflected!");
            return;
        }

        // Apply Battle Rage damage increase (25% more damage)
        if (player.BattleRageTurnsRemaining > 0)
        {
            var increasedDamage = (int)(damage * 1.25);
            if (increasedDamage > damage)
            {
                combatState.AddLogEntry($"{indent}Battle Rage increases damage from {damage} to {increasedDamage}!");
                damage = increasedDamage;
            }
        }

        // Apply shield absorption
        if (player.ShieldAbsorptionRemaining > 0)
        {
            if (damage <= player.ShieldAbsorptionRemaining)
            {
                player.ShieldAbsorptionRemaining -= damage;
                combatState.AddLogEntry($"{indent}Aetheric shield absorbs {damage} damage! (Shield: {player.ShieldAbsorptionRemaining} remaining)");
                return;
            }
            else
            {
                var remainingDamage = damage - player.ShieldAbsorptionRemaining;
                combatState.AddLogEntry($"{indent}Aetheric shield absorbs {player.ShieldAbsorptionRemaining} damage and shatters!");
                player.ShieldAbsorptionRemaining = 0;
                damage = remainingDamage;
            }
        }

        player.HP -= damage;
        combatState.AddLogEntry($"{indent}{player.Name} takes {damage} damage! (HP: {Math.Max(0, player.HP)}/{player.MaxHP})");
    }

    private void ExecuteDefend(Enemy enemy, CombatState combatState)
    {
        var defendRoll = _diceService.Roll(enemy.Attributes.Sturdiness);

        combatState.AddLogEntry($"{enemy.Name} takes a defensive stance!");
        combatState.AddLogEntry($"  Rolled {enemy.Attributes.Sturdiness}d6: {FormatRolls(defendRoll)} = {defendRoll.Successes} successes");

        // 50% damage reduction for this turn
        enemy.DefenseBonus = 50;
        enemy.DefenseTurnsRemaining = 1;

        combatState.AddLogEntry($"  Defense raised by 50% until their next turn");
        combatState.AddLogEntry("");
    }

    private void ExecuteRapidStrike(Enemy enemy, PlayerCharacter player, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} unleashes a rapid strike!");

        // Attack twice, each with -1 die
        for (int i = 1; i <= 2; i++)
        {
            combatState.AddLogEntry($"  Strike {i}:");

            var attackDice = Math.Max(1, enemy.Attributes.Might - 1);
            var attackRoll = _diceService.Roll(attackDice);
            combatState.AddLogEntry($"    Rolled {attackDice}d6: {FormatRolls(attackRoll)} = {attackRoll.Successes} successes");

            // Check dodge
            if (combatState.PlayerNegateNextAttack)
            {
                combatState.AddLogEntry($"    {player.Name} dodges the attack!");
                combatState.PlayerNegateNextAttack = false;
                continue;
            }

            // Player defends
            var defendRoll = _diceService.Roll(player.Attributes.Sturdiness);
            combatState.AddLogEntry($"    {player.Name} defends: {defendRoll.Successes} successes");

            // Calculate damage
            var netSuccesses = Math.Max(0, attackRoll.Successes - defendRoll.Successes);
            var baseDamage = netSuccesses + _diceService.RollDamage(enemy.BaseDamageDice) + enemy.DamageBonus;
            var damage = baseDamage;

            // Apply defense
            if (player.DefenseTurnsRemaining > 0)
            {
                damage = (int)(damage * (1 - player.DefenseBonus / 100.0));
            }

            ApplyDamageToPlayer(player, damage, combatState, "    ");
        }

        combatState.AddLogEntry("");
    }

    private void ExecuteHeavyStrike(Enemy enemy, PlayerCharacter player, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} charges up a heavy strike!");

        var attackDice = enemy.Attributes.Might + 2;
        var attackRoll = _diceService.Roll(attackDice);
        combatState.AddLogEntry($"  Rolled {attackDice}d6: {FormatRolls(attackRoll)} = {attackRoll.Successes} successes");

        // Check dodge
        if (combatState.PlayerNegateNextAttack)
        {
            combatState.AddLogEntry($"  {player.Name} dodges the massive attack!");
            combatState.PlayerNegateNextAttack = false;
            combatState.AddLogEntry("");
            return;
        }

        // Player defends
        var defendRoll = _diceService.Roll(player.Attributes.Sturdiness);
        combatState.AddLogEntry($"{player.Name} defends!");
        combatState.AddLogEntry($"  Rolled {player.Attributes.Sturdiness}d6: {FormatRolls(defendRoll)} = {defendRoll.Successes} successes");

        // 2d6 damage
        var netSuccesses = Math.Max(0, attackRoll.Successes - defendRoll.Successes);
        var baseDamage = netSuccesses + _diceService.RollDamage(2);
        var damage = baseDamage;

        // Apply defense
        if (player.DefenseTurnsRemaining > 0)
        {
            var reducedDamage = (int)(damage * (1 - player.DefenseBonus / 100.0));
            combatState.AddLogEntry($"{player.Name}'s defense reduces damage from {damage} to {reducedDamage}");
            damage = reducedDamage;
        }

        ApplyDamageToPlayer(player, damage, combatState);

        combatState.AddLogEntry("");
    }

    private void ExecuteBerserkStrike(Enemy enemy, PlayerCharacter player, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} enters a berserk rage!");

        var attackDice = enemy.Attributes.Might + 3;
        var attackRoll = _diceService.Roll(attackDice);
        combatState.AddLogEntry($"  Rolled {attackDice}d6: {FormatRolls(attackRoll)} = {attackRoll.Successes} successes");

        // Check dodge
        if (combatState.PlayerNegateNextAttack)
        {
            combatState.AddLogEntry($"  {player.Name} narrowly dodges the devastating attack!");
            combatState.PlayerNegateNextAttack = false;
            combatState.AddLogEntry("");
            return;
        }

        // Player defends
        var defendRoll = _diceService.Roll(player.Attributes.Sturdiness);
        combatState.AddLogEntry($"{player.Name} defends!");
        combatState.AddLogEntry($"  Rolled {player.Attributes.Sturdiness}d6: {FormatRolls(defendRoll)} = {defendRoll.Successes} successes");

        // 3d6 damage
        var netSuccesses = Math.Max(0, attackRoll.Successes - defendRoll.Successes);
        var baseDamage = netSuccesses + _diceService.RollDamage(3);
        var damage = baseDamage;

        // Apply defense
        if (player.DefenseTurnsRemaining > 0)
        {
            var reducedDamage = (int)(damage * (1 - player.DefenseBonus / 100.0));
            combatState.AddLogEntry($"{player.Name}'s defense reduces damage from {damage} to {reducedDamage}");
            damage = reducedDamage;
        }

        ApplyDamageToPlayer(player, damage, combatState);

        combatState.AddLogEntry("");
    }

    private void ExecuteChargeDefense(Enemy enemy, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} charges its defense systems!");

        var defendRoll = _diceService.Roll(enemy.Attributes.Sturdiness);
        combatState.AddLogEntry($"  Rolled {enemy.Attributes.Sturdiness}d6: {FormatRolls(defendRoll)} = {defendRoll.Successes} successes");

        // 50% damage reduction for 2 turns
        enemy.DefenseBonus = 50;
        enemy.DefenseTurnsRemaining = 2;

        combatState.AddLogEntry($"  Defense systems online: 50% damage reduction for 2 turns");
        combatState.AddLogEntry("");
    }

    private void ExecuteEmergencyRepairs(Enemy enemy, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} initiates emergency repair protocols!");

        var healAmount = 10;
        enemy.HP = Math.Min(enemy.MaxHP, enemy.HP + healAmount);

        combatState.AddLogEntry($"  Repaired {healAmount} HP! (HP: {enemy.HP}/{enemy.MaxHP})");
        combatState.AddLogEntry("");
    }

    private string FormatRolls(DiceResult result)
    {
        return string.Join(", ", result.Rolls);
    }
}
