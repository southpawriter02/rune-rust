using RuneAndRust.Core;

namespace RuneAndRust.Engine;

public enum EnemyAction
{
    // Base actions
    BasicAttack,
    Defend,
    RapidStrike,
    HeavyStrike,
    BerserkStrike,
    ChargeDefense,
    EmergencyRepairs,

    // v0.4 new actions
    QuickBite,          // Scrap-Hound: attacks twice, -1 die each
    DartAway,           // Scrap-Hound: move and gain evasion
    FergeralStrike,      // Test Subject: MIGHT + 2 dice
    BerserkerRush,      // Test Subject: immediate attack, skip next turn
    Shriek,             // Test Subject: buff allies
    PrecisionStrike,    // War-Frame: tactical attack with accuracy
    SuppressionFire,    // War-Frame: AOE attack
    TacticalReposition, // War-Frame: move and defense boost
    AethericBolt,       // Forlorn Scholar: ranged magic attack
    RealityDistortion,  // Forlorn Scholar: disable attack
    PhaseShift,         // Forlorn Scholar/Aberration: high evasion
    VoidBlast,          // Aberration: high damage magic
    SummonEchoes,       // Aberration: spawn adds
    RealityTear,        // Aberration: AOE magic damage
    AethericStorm,      // Aberration Phase 2: AOE attack
    DesperateSummon     // Aberration Phase 2: spawn add
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
            // v0.1-v0.3 enemies
            EnemyType.CorruptedServitor => DetermineServitorAction(),
            EnemyType.BlightDrone => DetermineDroneAction(),
            EnemyType.RuinWarden => DetermineWardenAction(enemy),

            // v0.4 new enemies
            EnemyType.ScrapHound => DetermineScrapHoundAction(enemy),
            EnemyType.TestSubject => DetermineTestSubjectAction(enemy),
            EnemyType.WarFrame => DetermineWarFrameAction(enemy),
            EnemyType.ForlornScholar => DetermineForlornScholarAction(enemy),
            EnemyType.AethericAberration => DetermineAberrationAction(enemy),

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

    // ====== v0.4 NEW ENEMY AI PATTERNS ======

    private EnemyAction DetermineScrapHoundAction(Enemy scrapHound)
    {
        var roll = _random.Next(100);
        var hpPercent = (double)scrapHound.HP / scrapHound.MaxHP;

        // Flee if below 50% HP (10% chance)
        if (hpPercent < 0.5 && roll < 10)
            return EnemyAction.Defend; // Placeholder for flee behavior

        if (roll < 70) // 70% chance
            return EnemyAction.QuickBite;
        else // 30% chance (was 20% dart + 10% flee, now just dart)
            return EnemyAction.DartAway;
    }

    private EnemyAction DetermineTestSubjectAction(Enemy testSubject)
    {
        var roll = _random.Next(100);

        if (roll < 60) // 60% chance
            return EnemyAction.FergeralStrike;
        else if (roll < 90) // 30% chance
            return EnemyAction.BerserkerRush;
        else // 10% chance
            return EnemyAction.Shriek;
    }

    private EnemyAction DetermineWarFrameAction(Enemy warFrame)
    {
        var roll = _random.Next(100);
        var hpPercent = (double)warFrame.HP / warFrame.MaxHP;

        // Emergency repair if low HP
        if (hpPercent < 0.3 && roll < 20)
            return EnemyAction.EmergencyRepairs;

        if (roll < 40) // 40% chance
            return EnemyAction.PrecisionStrike;
        else if (roll < 70) // 30% chance
            return EnemyAction.SuppressionFire;
        else // 30% chance (10% repair + 20% reposition)
            return EnemyAction.TacticalReposition;
    }

    private EnemyAction DetermineForlornScholarAction(Enemy scholar)
    {
        var roll = _random.Next(100);

        if (roll < 50) // 50% chance
            return EnemyAction.AethericBolt;
        else if (roll < 80) // 30% chance
            return EnemyAction.RealityDistortion;
        else // 20% chance
            return EnemyAction.PhaseShift;
    }

    private EnemyAction DetermineAberrationAction(Enemy aberration)
    {
        var hpPercent = (double)aberration.HP / aberration.MaxHP;

        if (hpPercent > 0.5)
        {
            // Phase 1 (100%-50% HP)
            var roll = _random.Next(100);

            if (roll < 40) // 40% chance
                return EnemyAction.VoidBlast;
            else if (roll < 70) // 30% chance
                return EnemyAction.SummonEchoes;
            else if (roll < 90) // 20% chance
                return EnemyAction.RealityTear;
            else // 10% chance
                return EnemyAction.PhaseShift;
        }
        else
        {
            // Phase 2 (50%-0% HP)
            var roll = _random.Next(100);

            if (roll < 40) // 40% chance
                return EnemyAction.AethericStorm;
            else if (roll < 80) // 40% chance
                return EnemyAction.VoidBlast;
            else // 20% chance
                return EnemyAction.DesperateSummon;
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
            // Base actions
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

            // v0.4 new actions
            case EnemyAction.QuickBite:
                ExecuteQuickBite(enemy, player, combatState);
                break;

            case EnemyAction.DartAway:
                ExecuteDartAway(enemy, combatState);
                break;

            case EnemyAction.FergeralStrike:
                ExecuteFergeralStrike(enemy, player, combatState);
                break;

            case EnemyAction.BerserkerRush:
                ExecuteBerserkerRush(enemy, player, combatState);
                break;

            case EnemyAction.Shriek:
                ExecuteShriek(enemy, combatState);
                break;

            case EnemyAction.PrecisionStrike:
                ExecutePrecisionStrike(enemy, player, combatState);
                break;

            case EnemyAction.SuppressionFire:
                ExecuteSuppressionFire(enemy, player, combatState);
                break;

            case EnemyAction.TacticalReposition:
                ExecuteTacticalReposition(enemy, combatState);
                break;

            case EnemyAction.AethericBolt:
                ExecuteAethericBolt(enemy, player, combatState);
                break;

            case EnemyAction.RealityDistortion:
                ExecuteRealityDistortion(enemy, player, combatState);
                break;

            case EnemyAction.PhaseShift:
                ExecutePhaseShift(enemy, combatState);
                break;

            case EnemyAction.VoidBlast:
                ExecuteVoidBlast(enemy, player, combatState);
                break;

            case EnemyAction.SummonEchoes:
                ExecuteSummonEchoes(enemy, combatState);
                break;

            case EnemyAction.RealityTear:
                ExecuteRealityTear(enemy, player, combatState);
                break;

            case EnemyAction.AethericStorm:
                ExecuteAethericStorm(enemy, player, combatState);
                break;

            case EnemyAction.DesperateSummon:
                ExecuteDesperateSummon(enemy, combatState);
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

    // ====== v0.4 NEW ENEMY ACTION EXECUTIONS ======

    private void ExecuteQuickBite(Enemy enemy, PlayerCharacter player, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} unleashes a quick double bite!");

        // Attack twice, each with -1 die
        for (int i = 1; i <= 2; i++)
        {
            combatState.AddLogEntry($"  Bite {i}:");

            var attackDice = Math.Max(1, enemy.Attributes.Might - 1);
            var attackRoll = _diceService.Roll(attackDice);
            combatState.AddLogEntry($"    Rolled {attackDice}d6: {FormatRolls(attackRoll)} = {attackRoll.Successes} successes");

            if (combatState.PlayerNegateNextAttack)
            {
                combatState.AddLogEntry($"    {player.Name} dodges the attack!");
                combatState.PlayerNegateNextAttack = false;
                continue;
            }

            var defendRoll = _diceService.Roll(player.Attributes.Sturdiness);
            combatState.AddLogEntry($"    {player.Name} defends: {defendRoll.Successes} successes");

            var netSuccesses = Math.Max(0, attackRoll.Successes - defendRoll.Successes);
            var baseDamage = netSuccesses + _diceService.RollDamage(enemy.BaseDamageDice) + enemy.DamageBonus;
            var damage = baseDamage;

            if (player.DefenseTurnsRemaining > 0)
            {
                damage = (int)(damage * (1 - player.DefenseBonus / 100.0));
            }

            ApplyDamageToPlayer(player, damage, combatState, "    ");
        }

        combatState.AddLogEntry("");
    }

    private void ExecuteDartAway(Enemy enemy, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} darts away with incredible speed!");

        // Gain +2 Evasion (represented as defense bonus for 1 turn)
        enemy.DefenseBonus = 75; // High evasion
        enemy.DefenseTurnsRemaining = 1;

        combatState.AddLogEntry($"  {enemy.Name}'s evasion increased dramatically until next turn!");
        combatState.AddLogEntry("");
    }

    private void ExecuteFergeralStrike(Enemy enemy, PlayerCharacter player, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} lunges with a feral strike!");

        var attackDice = enemy.Attributes.Might + 2;
        var attackRoll = _diceService.Roll(attackDice);
        combatState.AddLogEntry($"  Rolled {attackDice}d6: {FormatRolls(attackRoll)} = {attackRoll.Successes} successes");

        if (combatState.PlayerNegateNextAttack)
        {
            combatState.AddLogEntry($"  {player.Name} dodges the feral attack!");
            combatState.PlayerNegateNextAttack = false;
            combatState.AddLogEntry("");
            return;
        }

        var defendRoll = _diceService.Roll(player.Attributes.Sturdiness);
        combatState.AddLogEntry($"{player.Name} defends!");
        combatState.AddLogEntry($"  Rolled {player.Attributes.Sturdiness}d6: {FormatRolls(defendRoll)} = {defendRoll.Successes} successes");

        var netSuccesses = Math.Max(0, attackRoll.Successes - defendRoll.Successes);
        var baseDamage = netSuccesses + _diceService.RollDamage(enemy.BaseDamageDice) + enemy.DamageBonus;
        var damage = baseDamage;

        if (player.DefenseTurnsRemaining > 0)
        {
            var reducedDamage = (int)(damage * (1 - player.DefenseBonus / 100.0));
            combatState.AddLogEntry($"{player.Name}'s defense reduces damage from {damage} to {reducedDamage}");
            damage = reducedDamage;
        }

        ApplyDamageToPlayer(player, damage, combatState);
        combatState.AddLogEntry("");
    }

    private void ExecuteBerserkerRush(Enemy enemy, PlayerCharacter player, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} enters a berserker rush!");

        var attackDice = enemy.Attributes.Might + 3;
        var attackRoll = _diceService.Roll(attackDice);
        combatState.AddLogEntry($"  Rolled {attackDice}d6: {FormatRolls(attackRoll)} = {attackRoll.Successes} successes");

        if (combatState.PlayerNegateNextAttack)
        {
            combatState.AddLogEntry($"  {player.Name} dodges the berserker rush!");
            combatState.PlayerNegateNextAttack = false;
            combatState.AddLogEntry("  ...but the effort leaves {enemy.Name} exhausted!");
            enemy.IsStunned = true; // Skips next turn
            enemy.StunTurnsRemaining = 1;
            combatState.AddLogEntry("");
            return;
        }

        var defendRoll = _diceService.Roll(player.Attributes.Sturdiness);
        combatState.AddLogEntry($"{player.Name} defends!");
        combatState.AddLogEntry($"  Rolled {player.Attributes.Sturdiness}d6: {FormatRolls(defendRoll)} = {defendRoll.Successes} successes");

        var netSuccesses = Math.Max(0, attackRoll.Successes - defendRoll.Successes);
        var baseDamage = netSuccesses + _diceService.RollDamage(2) + enemy.DamageBonus; // Extra damage die
        var damage = baseDamage;

        if (player.DefenseTurnsRemaining > 0)
        {
            damage = (int)(damage * (1 - player.DefenseBonus / 100.0));
        }

        ApplyDamageToPlayer(player, damage, combatState);
        combatState.AddLogEntry($"  The effort exhausts {enemy.Name}! (Skips next turn)");

        // Skip next turn
        enemy.IsStunned = true;
        enemy.StunTurnsRemaining = 1;

        combatState.AddLogEntry("");
    }

    private void ExecuteShriek(Enemy enemy, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} unleashes a terrifying shriek!");
        combatState.AddLogEntry($"  All corrupted allies feel emboldened! (+1 die to next attack)");
        combatState.AddLogEntry($"  [Note: Buff mechanic simplified - enemies gain temporary morale boost]");
        combatState.AddLogEntry("");
    }

    private void ExecutePrecisionStrike(Enemy enemy, PlayerCharacter player, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} targets weak points with precision!");

        var attackDice = enemy.Attributes.Might + 3;
        var attackRoll = _diceService.Roll(attackDice);
        combatState.AddLogEntry($"  Rolled {attackDice}d6 with +2 accuracy: {FormatRolls(attackRoll)} = {attackRoll.Successes} successes");

        if (combatState.PlayerNegateNextAttack)
        {
            combatState.AddLogEntry($"  {player.Name} dodges the precise strike!");
            combatState.PlayerNegateNextAttack = false;
            combatState.AddLogEntry("");
            return;
        }

        var defendRoll = _diceService.Roll(player.Attributes.Sturdiness);
        combatState.AddLogEntry($"{player.Name} defends!");
        combatState.AddLogEntry($"  Rolled {player.Attributes.Sturdiness}d6: {FormatRolls(defendRoll)} = {defendRoll.Successes} successes");

        var netSuccesses = Math.Max(0, attackRoll.Successes - defendRoll.Successes);
        var baseDamage = netSuccesses + _diceService.RollDamage(enemy.BaseDamageDice) + enemy.DamageBonus + 2; // +2 accuracy bonus
        var damage = baseDamage;

        if (player.DefenseTurnsRemaining > 0)
        {
            damage = (int)(damage * (1 - player.DefenseBonus / 100.0));
        }

        ApplyDamageToPlayer(player, damage, combatState);
        combatState.AddLogEntry("");
    }

    private void ExecuteSuppressionFire(Enemy enemy, PlayerCharacter player, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} unleashes suppression fire across the area!");

        var attackDice = enemy.Attributes.Might;
        var attackRoll = _diceService.Roll(attackDice);
        combatState.AddLogEntry($"  Rolled {attackDice}d6: {FormatRolls(attackRoll)} = {attackRoll.Successes} successes");

        if (combatState.PlayerNegateNextAttack)
        {
            combatState.AddLogEntry($"  {player.Name} dodges behind cover!");
            combatState.PlayerNegateNextAttack = false;
            combatState.AddLogEntry("");
            return;
        }

        var defendRoll = _diceService.Roll(player.Attributes.Sturdiness);
        combatState.AddLogEntry($"{player.Name} takes cover!");
        combatState.AddLogEntry($"  Rolled {player.Attributes.Sturdiness}d6: {FormatRolls(defendRoll)} = {defendRoll.Successes} successes");

        var netSuccesses = Math.Max(0, attackRoll.Successes - defendRoll.Successes);
        var baseDamage = netSuccesses + _diceService.RollDamage(1); // 1d6 damage (lower than normal)
        var damage = baseDamage;

        if (player.DefenseTurnsRemaining > 0)
        {
            damage = (int)(damage * (1 - player.DefenseBonus / 100.0));
        }

        ApplyDamageToPlayer(player, damage, combatState);
        combatState.AddLogEntry($"  (AOE attack hits all targets in area)");
        combatState.AddLogEntry("");
    }

    private void ExecuteTacticalReposition(Enemy enemy, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} tactically repositions!");

        enemy.DefenseBonus = 75;
        enemy.DefenseTurnsRemaining = 1;

        combatState.AddLogEntry($"  Defense systems activated: +75% damage reduction until next turn");
        combatState.AddLogEntry("");
    }

    private void ExecuteAethericBolt(Enemy enemy, PlayerCharacter player, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} fires an aetheric bolt!");

        var attackDice = enemy.Attributes.Will + 2;
        var attackRoll = _diceService.Roll(attackDice);
        combatState.AddLogEntry($"  Rolled {attackDice}d6: {FormatRolls(attackRoll)} = {attackRoll.Successes} successes");

        if (combatState.PlayerNegateNextAttack)
        {
            combatState.AddLogEntry($"  {player.Name} dodges the energy blast!");
            combatState.PlayerNegateNextAttack = false;
            combatState.AddLogEntry("");
            return;
        }

        // WILL defense instead of STURDINESS (mental attack)
        var defendRoll = _diceService.Roll(Math.Max(1, player.Attributes.Will));
        combatState.AddLogEntry($"{player.Name} resists with WILL!");
        combatState.AddLogEntry($"  Rolled {Math.Max(1, player.Attributes.Will)}d6: {FormatRolls(defendRoll)} = {defendRoll.Successes} successes");

        var netSuccesses = Math.Max(0, attackRoll.Successes - defendRoll.Successes);
        var damage = netSuccesses + _diceService.RollDamage(enemy.BaseDamageDice);

        combatState.AddLogEntry($"  Aetheric damage ignores armor!");
        ApplyDamageToPlayer(player, damage, combatState);
        combatState.AddLogEntry("");
    }

    private void ExecuteRealityDistortion(Enemy enemy, PlayerCharacter player, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} distorts reality around you!");

        var attackDice = enemy.Attributes.Will;
        var attackRoll = _diceService.Roll(attackDice);
        combatState.AddLogEntry($"  Rolled {attackDice}d6: {FormatRolls(attackRoll)} = {attackRoll.Successes} successes");

        var defendRoll = _diceService.Roll(Math.Max(1, player.Attributes.Will));
        combatState.AddLogEntry($"{player.Name} resists with WILL!");
        combatState.AddLogEntry($"  Rolled {Math.Max(1, player.Attributes.Will)}d6: {FormatRolls(defendRoll)} = {defendRoll.Successes} successes");

        if (attackRoll.Successes > defendRoll.Successes + 1) // Need to beat by 2+
        {
            combatState.AddLogEntry($"  {player.Name} is disoriented and loses their next turn!");
            player.IsStunned = true;
            player.StunTurnsRemaining = 1;
        }
        else
        {
            combatState.AddLogEntry($"  {player.Name} shakes off the effect!");
        }

        combatState.AddLogEntry("");
    }

    private void ExecutePhaseShift(Enemy enemy, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} phase shifts out of reality!");

        enemy.DefenseBonus = 90; // Nearly untouchable
        enemy.DefenseTurnsRemaining = 1;

        combatState.AddLogEntry($"  {enemy.Name} becomes nearly impossible to hit until next turn!");
        combatState.AddLogEntry("");
    }

    private void ExecuteVoidBlast(Enemy enemy, PlayerCharacter player, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} unleashes a void blast!");

        var phase = enemy.HP <= enemy.MaxHP / 2 ? 2 : 1;
        var bonusDice = phase == 2 ? 4 : 3;

        var attackDice = enemy.Attributes.Will + bonusDice;
        var attackRoll = _diceService.Roll(attackDice);
        combatState.AddLogEntry($"  Rolled {attackDice}d6: {FormatRolls(attackRoll)} = {attackRoll.Successes} successes");

        if (combatState.PlayerNegateNextAttack)
        {
            combatState.AddLogEntry($"  {player.Name} dodges the void blast!");
            combatState.PlayerNegateNextAttack = false;
            combatState.AddLogEntry("");
            return;
        }

        var defendRoll = _diceService.Roll(Math.Max(1, player.Attributes.Will));
        combatState.AddLogEntry($"{player.Name} resists with WILL!");
        combatState.AddLogEntry($"  Rolled {Math.Max(1, player.Attributes.Will)}d6: {FormatRolls(defendRoll)} = {defendRoll.Successes} successes");

        var netSuccesses = Math.Max(0, attackRoll.Successes - defendRoll.Successes);
        var damageDice = phase == 2 ? 4 : 3;
        var damage = netSuccesses + _diceService.RollDamage(damageDice);

        combatState.AddLogEntry($"  Void damage ignores armor!");
        ApplyDamageToPlayer(player, damage, combatState);
        combatState.AddLogEntry("");
    }

    private void ExecuteSummonEchoes(Enemy enemy, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} summons echoes from the void!");
        combatState.AddLogEntry($"  Two Scrap-Hound echoes materialize!");
        combatState.AddLogEntry($"  [Note: Summon mechanics require combat system updates]");
        combatState.AddLogEntry($"  (Summoned adds would appear as reinforcements)");
        combatState.AddLogEntry("");
    }

    private void ExecuteRealityTear(Enemy enemy, PlayerCharacter player, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} tears at the fabric of reality!");

        var attackDice = enemy.Attributes.Will;
        var attackRoll = _diceService.Roll(attackDice);
        combatState.AddLogEntry($"  Rolled {attackDice}d6: {FormatRolls(attackRoll)} = {attackRoll.Successes} successes");

        var defendRoll = _diceService.Roll(Math.Max(1, player.Attributes.Will));
        combatState.AddLogEntry($"{player.Name} resists with WILL!");
        combatState.AddLogEntry($"  Rolled {Math.Max(1, player.Attributes.Will)}d6: {FormatRolls(defendRoll)} = {defendRoll.Successes} successes");

        var netSuccesses = Math.Max(0, attackRoll.Successes - defendRoll.Successes);
        var damage = netSuccesses + _diceService.RollDamage(2);

        combatState.AddLogEntry($"  Reality tears hit all targets in the area!");
        ApplyDamageToPlayer(player, damage, combatState);
        combatState.AddLogEntry("");
    }

    private void ExecuteAethericStorm(Enemy enemy, PlayerCharacter player, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} unleashes an aetheric storm!");

        var attackDice = enemy.Attributes.Will + 2;
        var attackRoll = _diceService.Roll(attackDice);
        combatState.AddLogEntry($"  Rolled {attackDice}d6: {FormatRolls(attackRoll)} = {attackRoll.Successes} successes");

        if (combatState.PlayerNegateNextAttack)
        {
            combatState.AddLogEntry($"  {player.Name} dodges through the storm!");
            combatState.PlayerNegateNextAttack = false;
            combatState.AddLogEntry("");
            return;
        }

        var defendRoll = _diceService.Roll(Math.Max(1, player.Attributes.Will));
        combatState.AddLogEntry($"{player.Name} resists with WILL!");
        combatState.AddLogEntry($"  Rolled {Math.Max(1, player.Attributes.Will)}d6: {FormatRolls(defendRoll)} = {defendRoll.Successes} successes");

        var netSuccesses = Math.Max(0, attackRoll.Successes - defendRoll.Successes);
        var damage = netSuccesses + _diceService.RollDamage(2) + 4; // 2d8 approximated as 2d6+4

        combatState.AddLogEntry($"  Aetheric storm hits all characters!");
        ApplyDamageToPlayer(player, damage, combatState);
        combatState.AddLogEntry("");
    }

    private void ExecuteDesperateSummon(Enemy enemy, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} desperately summons reinforcements!");
        combatState.AddLogEntry($"  A Blight-Drone materializes!");
        combatState.AddLogEntry($"  [Note: Summon mechanics require combat system updates]");
        combatState.AddLogEntry($"  (Summoned add would appear as reinforcement)");
        combatState.AddLogEntry("");
    }

    private string FormatRolls(DiceResult result)
    {
        return string.Join(", ", result.Rolls);
    }
}
