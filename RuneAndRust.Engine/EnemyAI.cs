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
    DesperateSummon,    // Aberration Phase 2: spawn add

    // v0.6 new actions (The Lower Depths)
    WeldingTorch,       // Maintenance Construct: basic attack
    RepairProtocol,     // Maintenance Construct: self-heal
    EmergencyLockdown,  // Maintenance Construct: defense buff
    ToxicBite,          // Sludge-Crawler: poison attack
    Lunge,              // Sludge-Crawler: gap closer
    Submerge,           // Sludge-Crawler: evasion boost (in flooded areas)
    OverchargeAlly,     // Corrupted Engineer: buff ally damage
    ArcDischarge,       // Corrupted Engineer: ranged electrical attack
    EmergencyRepairAlly,// Corrupted Engineer: heal ally
    SystemShock,        // Corrupted Engineer: stun attack
    HalberdSweep,       // Vault Custodian: single target attack
    DefensiveStance,    // Vault Custodian: defense mode
    GuardianProtocol,   // Vault Custodian: self-heal
    WhirlwindStrike,    // Vault Custodian Phase 2: AOE attack
    LastStand,          // Vault Custodian Phase 2: buff all attacks
    MindSpike,          // Forlorn Archivist: psychic attack
    SummonRevenants,    // Forlorn Archivist: spawn adds
    PsychicScream,      // Forlorn Archivist: stress AOE
    MassHysteria,       // Forlorn Archivist Phase 2: fear debuff
    PsychicStorm,       // Forlorn Archivist Phase 3: massive AOE
    MaulStrike,         // Omega Sentinel: single target slam
    SeismicSlam,        // Omega Sentinel: AOE knockback
    PowerDraw,          // Omega Sentinel: self-heal from power core
    OverchargedMaul,    // Omega Sentinel Phase 2: enhanced attack
    DefensiveProtocols, // Omega Sentinel Phase 2: defense buff
    OmegaProtocol       // Omega Sentinel Phase 3: ultimate attack
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

            // v0.6 new enemies
            EnemyType.MaintenanceConstruct => DetermineMaintenanceConstructAction(enemy),
            EnemyType.SludgeCrawler => DetermineSludgeCrawlerAction(enemy),
            EnemyType.CorruptedEngineer => DetermineCorruptedEngineerAction(enemy),
            EnemyType.VaultCustodian => DetermineVaultCustodianAction(enemy),
            EnemyType.ForlornArchivist => DetermineForlornArchivistAction(enemy),
            EnemyType.OmegaSentinel => DetermineOmegaSentinelAction(enemy),

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

    // ====== v0.6 NEW ENEMY AI PATTERNS (THE LOWER DEPTHS) ======

    private EnemyAction DetermineMaintenanceConstructAction(Enemy construct)
    {
        var roll = _random.Next(100);
        var hpPercent = (double)construct.HP / construct.MaxHP;

        // First time reduced to 50% HP, use Repair Protocol
        if (hpPercent <= 0.5 && hpPercent > 0.4 && !construct.HasUsedSpecialAbility)
        {
            construct.HasUsedSpecialAbility = true;
            return EnemyAction.RepairProtocol;
        }

        if (roll < 60) // 60% chance
            return EnemyAction.WeldingTorch;
        else if (roll < 90) // 30% chance
            return EnemyAction.RepairProtocol;
        else // 10% chance
            return EnemyAction.EmergencyLockdown;
    }

    private EnemyAction DetermineSludgeCrawlerAction(Enemy crawler)
    {
        var roll = _random.Next(100);

        if (roll < 70) // 70% chance
            return EnemyAction.ToxicBite;
        else if (roll < 90) // 20% chance
            return EnemyAction.Lunge;
        else // 10% chance
            return EnemyAction.Submerge;
    }

    private EnemyAction DetermineCorruptedEngineerAction(Enemy engineer)
    {
        var roll = _random.Next(100);

        // Priority: Buff/heal allies over attacking
        if (roll < 40) // 40% chance
            return EnemyAction.OverchargeAlly;
        else if (roll < 60) // 20% chance
            return EnemyAction.EmergencyRepairAlly;
        else if (roll < 90) // 30% chance
            return EnemyAction.ArcDischarge;
        else // 10% chance
            return EnemyAction.SystemShock;
    }

    private EnemyAction DetermineVaultCustodianAction(Enemy custodian)
    {
        var hpPercent = (double)custodian.HP / custodian.MaxHP;

        // Phase 1 (100%-50% HP)
        if (hpPercent > 0.5)
        {
            var roll = _random.Next(100);

            if (roll < 50) // 50% chance
                return EnemyAction.HalberdSweep;
            else if (roll < 80) // 30% chance
                return EnemyAction.DefensiveStance;
            else // 20% chance
                return EnemyAction.GuardianProtocol;
        }
        // Phase 2 (50%-0% HP)
        else
        {
            // First time reduced to 25% HP, use Last Stand
            if (hpPercent <= 0.25 && !custodian.HasUsedSpecialAbility)
            {
                custodian.HasUsedSpecialAbility = true;
                return EnemyAction.LastStand;
            }

            var roll = _random.Next(100);

            if (roll < 40) // 40% chance
                return EnemyAction.WhirlwindStrike;
            else if (roll < 80) // 40% chance
                return EnemyAction.HalberdSweep;
            else // 20% chance
                return EnemyAction.LastStand; // Can try again if already used
        }
    }

    private EnemyAction DetermineForlornArchivistAction(Enemy archivist)
    {
        var hpPercent = (double)archivist.HP / archivist.MaxHP;

        // Phase 1 (100%-60% HP)
        if (hpPercent > 0.6)
        {
            var roll = _random.Next(100);

            if (roll < 40) // 40% chance
                return EnemyAction.MindSpike;
            else if (roll < 70) // 30% chance
                return EnemyAction.SummonRevenants;
            else if (roll < 90) // 20% chance
                return EnemyAction.PsychicScream;
            else // 10% chance
                return EnemyAction.PhaseShift;
        }
        // Phase 2 (60%-30% HP)
        else if (hpPercent > 0.3)
        {
            var roll = _random.Next(100);

            if (roll < 40) // 40% chance
                return EnemyAction.MassHysteria;
            else if (roll < 70) // 30% chance
                return EnemyAction.MindSpike;
            else if (roll < 90) // 20% chance
                return EnemyAction.SummonRevenants;
            else // 10% chance
                return EnemyAction.PhaseShift;
        }
        // Phase 3 (30%-0% HP)
        else
        {
            var roll = _random.Next(100);

            if (roll < 50) // 50% chance
                return EnemyAction.PsychicStorm;
            else if (roll < 80) // 30% chance
                return EnemyAction.MindSpike;
            else // 20% chance
                return EnemyAction.SummonRevenants;
        }
    }

    private EnemyAction DetermineOmegaSentinelAction(Enemy sentinel)
    {
        var hpPercent = (double)sentinel.HP / sentinel.MaxHP;

        // Phase 1 (100%-60% HP)
        if (hpPercent > 0.6)
        {
            var roll = _random.Next(100);

            if (roll < 50) // 50% chance
                return EnemyAction.MaulStrike;
            else if (roll < 80) // 30% chance
                return EnemyAction.SeismicSlam;
            else // 20% chance
                return EnemyAction.PowerDraw;
        }
        // Phase 2 (60%-30% HP)
        else if (hpPercent > 0.3)
        {
            var roll = _random.Next(100);

            if (roll < 40) // 40% chance
                return EnemyAction.OverchargedMaul;
            else if (roll < 80) // 40% chance
                return EnemyAction.SeismicSlam;
            else // 20% chance
                return EnemyAction.DefensiveProtocols;
        }
        // Phase 3 (30%-0% HP)
        else
        {
            var roll = _random.Next(100);

            if (roll < 50) // 50% chance
                return EnemyAction.OmegaProtocol;
            else if (roll < 80) // 30% chance
                return EnemyAction.OverchargedMaul;
            else // 20% chance
                return EnemyAction.PowerDraw; // Emergency healing
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

            // v0.6 new actions (The Lower Depths)
            case EnemyAction.WeldingTorch:
                ExecuteWeldingTorch(enemy, player, combatState);
                break;

            case EnemyAction.RepairProtocol:
                ExecuteRepairProtocol(enemy, combatState);
                break;

            case EnemyAction.EmergencyLockdown:
                ExecuteEmergencyLockdown(enemy, combatState);
                break;

            case EnemyAction.ToxicBite:
                ExecuteToxicBite(enemy, player, combatState);
                break;

            case EnemyAction.Lunge:
                ExecuteLunge(enemy, player, combatState);
                break;

            case EnemyAction.Submerge:
                ExecuteSubmerge(enemy, combatState);
                break;

            case EnemyAction.OverchargeAlly:
                ExecuteOverchargeAlly(enemy, combatState);
                break;

            case EnemyAction.ArcDischarge:
                ExecuteArcDischarge(enemy, player, combatState);
                break;

            case EnemyAction.EmergencyRepairAlly:
                ExecuteEmergencyRepairAlly(enemy, combatState);
                break;

            case EnemyAction.SystemShock:
                ExecuteSystemShock(enemy, player, combatState);
                break;

            case EnemyAction.HalberdSweep:
                ExecuteHalberdSweep(enemy, player, combatState);
                break;

            case EnemyAction.DefensiveStance:
                ExecuteDefensiveStance(enemy, combatState);
                break;

            case EnemyAction.GuardianProtocol:
                ExecuteGuardianProtocol(enemy, combatState);
                break;

            case EnemyAction.WhirlwindStrike:
                ExecuteWhirlwindStrike(enemy, player, combatState);
                break;

            case EnemyAction.LastStand:
                ExecuteLastStand(enemy, combatState);
                break;

            case EnemyAction.MindSpike:
                ExecuteMindSpike(enemy, player, combatState);
                break;

            case EnemyAction.SummonRevenants:
                ExecuteSummonRevenants(enemy, combatState);
                break;

            case EnemyAction.PsychicScream:
                ExecutePsychicScream(enemy, player, combatState);
                break;

            case EnemyAction.MassHysteria:
                ExecuteMassHysteria(enemy, player, combatState);
                break;

            case EnemyAction.PsychicStorm:
                ExecutePsychicStorm(enemy, player, combatState);
                break;

            case EnemyAction.MaulStrike:
                ExecuteMaulStrike(enemy, player, combatState);
                break;

            case EnemyAction.SeismicSlam:
                ExecuteSeismicSlam(enemy, player, combatState);
                break;

            case EnemyAction.PowerDraw:
                ExecutePowerDraw(enemy, combatState);
                break;

            case EnemyAction.OverchargedMaul:
                ExecuteOverchargedMaul(enemy, player, combatState);
                break;

            case EnemyAction.DefensiveProtocols:
                ExecuteDefensiveProtocols(enemy, combatState);
                break;

            case EnemyAction.OmegaProtocol:
                ExecuteOmegaProtocol(enemy, player, combatState);
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

    // ====== v0.6 NEW ENEMY ACTION EXECUTIONS (THE LOWER DEPTHS) ======

    // MAINTENANCE CONSTRUCT ACTIONS

    private void ExecuteWeldingTorch(Enemy enemy, PlayerCharacter player, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} attacks with its welding torch!");

        var attackRoll = _diceService.Roll(enemy.Attributes.Might + 2);
        combatState.AddLogEntry($"  Rolled {enemy.Attributes.Might + 2}d6: {FormatRolls(attackRoll)} = {attackRoll.Successes} successes");

        if (combatState.PlayerNegateNextAttack)
        {
            combatState.AddLogEntry($"  {player.Name} dodges the torch!");
            combatState.PlayerNegateNextAttack = false;
            combatState.AddLogEntry("");
            return;
        }

        var defendRoll = _diceService.Roll(player.DefenseDice);
        combatState.AddLogEntry($"{player.Name} defends!");
        combatState.AddLogEntry($"  Rolled {player.DefenseDice}d6: {FormatRolls(defendRoll)} = {defendRoll.Successes} successes");

        var netSuccesses = Math.Max(0, attackRoll.Successes - defendRoll.Successes);
        var damage = netSuccesses + _diceService.RollDamage(enemy.BaseDamageDice) + enemy.DamageBonus;

        ApplyDamageToPlayer(player, damage, combatState);
        combatState.AddLogEntry("");
    }

    private void ExecuteRepairProtocol(Enemy enemy, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} initiates repair protocols!");

        var healAmount = 8;
        enemy.HP = Math.Min(enemy.MaxHP, enemy.HP + healAmount);

        combatState.AddLogEntry($"  {enemy.Name} repairs {healAmount} HP! (Now at {enemy.HP}/{enemy.MaxHP} HP)");
        combatState.AddLogEntry("");
    }

    private void ExecuteEmergencyLockdown(Enemy enemy, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} activates emergency lockdown!");

        enemy.DefenseBonus = 3;
        enemy.DefenseTurnsRemaining = 1;

        combatState.AddLogEntry($"  Defense increased by +3 until next turn!");
        combatState.AddLogEntry("");
    }

    // SLUDGE-CRAWLER ACTIONS

    private void ExecuteToxicBite(Enemy enemy, PlayerCharacter player, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} bites with toxic fangs!");

        var attackRoll = _diceService.Roll(enemy.Attributes.Might);
        combatState.AddLogEntry($"  Rolled {enemy.Attributes.Might}d6: {FormatRolls(attackRoll)} = {attackRoll.Successes} successes");

        if (combatState.PlayerNegateNextAttack)
        {
            combatState.AddLogEntry($"  {player.Name} dodges the bite!");
            combatState.PlayerNegateNextAttack = false;
            combatState.AddLogEntry("");
            return;
        }

        var defendRoll = _diceService.Roll(player.DefenseDice);
        combatState.AddLogEntry($"{player.Name} defends!");
        combatState.AddLogEntry($"  Rolled {player.DefenseDice}d6: {FormatRolls(defendRoll)} = {defendRoll.Successes} successes");

        var netSuccesses = Math.Max(0, attackRoll.Successes - defendRoll.Successes);
        var damage = netSuccesses + _diceService.RollDamage(enemy.BaseDamageDice);

        ApplyDamageToPlayer(player, damage, combatState);

        // Apply poison DoT
        enemy.PoisonDamagePerTurn = 2; // 1d4 approximated as 2
        enemy.PoisonTurnsRemaining = 2;

        combatState.AddLogEntry($"  Venom courses through {player.Name}'s veins! (Poison for 2 turns)");
        combatState.AddLogEntry("");
    }

    private void ExecuteLunge(Enemy enemy, PlayerCharacter player, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} lunges forward!");

        var attackRoll = _diceService.Roll(enemy.Attributes.Might);
        combatState.AddLogEntry($"  Rolled {enemy.Attributes.Might}d6: {FormatRolls(attackRoll)} = {attackRoll.Successes} successes");

        if (combatState.PlayerNegateNextAttack)
        {
            combatState.AddLogEntry($"  {player.Name} sidesteps the lunge!");
            combatState.PlayerNegateNextAttack = false;
            combatState.AddLogEntry("");
            return;
        }

        var defendRoll = _diceService.Roll(player.DefenseDice);
        combatState.AddLogEntry($"{player.Name} defends!");
        combatState.AddLogEntry($"  Rolled {player.DefenseDice}d6: {FormatRolls(defendRoll)} = {defendRoll.Successes} successes");

        var netSuccesses = Math.Max(0, attackRoll.Successes - defendRoll.Successes);
        var damage = netSuccesses + _diceService.RollDamage(enemy.BaseDamageDice);

        ApplyDamageToPlayer(player, damage, combatState);
        combatState.AddLogEntry($"  {enemy.Name} closes the distance!");
        combatState.AddLogEntry("");
    }

    private void ExecuteSubmerge(Enemy enemy, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} submerges into the toxic sludge!");

        enemy.DefenseBonus = 4;
        enemy.DefenseTurnsRemaining = 1;

        combatState.AddLogEntry($"  Evasion increased dramatically until next turn!");
        combatState.AddLogEntry("");
    }

    // CORRUPTED ENGINEER ACTIONS

    private void ExecuteOverchargeAlly(Enemy enemy, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} overcharges an ally's systems!");
        combatState.AddLogEntry($"  [Note: Ally buff mechanics - target ally gains +2 dice and +1d6 damage on next attack]");
        combatState.AddLogEntry($"  (Implementation pending multi-enemy combat system)");
        combatState.AddLogEntry("");
    }

    private void ExecuteArcDischarge(Enemy enemy, PlayerCharacter player, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} fires an arc discharge!");

        var attackRoll = _diceService.Roll(enemy.Attributes.Wits + 2);
        combatState.AddLogEntry($"  Rolled {enemy.Attributes.Wits + 2}d6: {FormatRolls(attackRoll)} = {attackRoll.Successes} successes");

        if (combatState.PlayerNegateNextAttack)
        {
            combatState.AddLogEntry($"  {player.Name} dodges the electrical blast!");
            combatState.PlayerNegateNextAttack = false;
            combatState.AddLogEntry("");
            return;
        }

        var defendRoll = _diceService.Roll(player.DefenseDice);
        combatState.AddLogEntry($"{player.Name} defends!");
        combatState.AddLogEntry($"  Rolled {player.DefenseDice}d6: {FormatRolls(defendRoll)} = {defendRoll.Successes} successes");

        var netSuccesses = Math.Max(0, attackRoll.Successes - defendRoll.Successes);
        var damage = netSuccesses + _diceService.RollDamage(enemy.BaseDamageDice);

        ApplyDamageToPlayer(player, damage, combatState);
        combatState.AddLogEntry($"  Electrical energy crackles across {player.Name}!");
        combatState.AddLogEntry("");
    }

    private void ExecuteEmergencyRepairAlly(Enemy enemy, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} initiates emergency repairs on an ally!");
        combatState.AddLogEntry($"  [Note: Ally heal mechanics - target ally recovers 15 HP]");
        combatState.AddLogEntry($"  (Implementation pending multi-enemy combat system)");
        combatState.AddLogEntry("");
    }

    private void ExecuteSystemShock(Enemy enemy, PlayerCharacter player, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} unleashes a system shock!");

        var attackRoll = _diceService.Roll(enemy.Attributes.Wits);
        combatState.AddLogEntry($"  Rolled {enemy.Attributes.Wits}d6: {FormatRolls(attackRoll)} = {attackRoll.Successes} successes");

        var defendRoll = _diceService.Roll(Math.Max(1, player.Attributes.Will));
        combatState.AddLogEntry($"{player.Name} resists with WILL!");
        combatState.AddLogEntry($"  Rolled {Math.Max(1, player.Attributes.Will)}d6: {FormatRolls(defendRoll)} = {defendRoll.Successes} successes");

        if (attackRoll.Successes > defendRoll.Successes)
        {
            combatState.AddLogEntry($"  {player.Name} loses their next turn!");
            combatState.AddLogEntry($"  (Note: Stun mechanic placeholder - would skip player turn)");
        }
        else
        {
            combatState.AddLogEntry($"  {player.Name} resists the shock!");
        }
        combatState.AddLogEntry("");
    }

    // VAULT CUSTODIAN ACTIONS

    private void ExecuteHalberdSweep(Enemy enemy, PlayerCharacter player, CombatState combatState)
    {
        var attackDice = enemy.Phase == 1 ? enemy.Attributes.Might + 3 : enemy.Attributes.Might + 4;
        combatState.AddLogEntry($"{enemy.Name} swings its massive halberd!");

        var attackRoll = _diceService.Roll(attackDice);
        combatState.AddLogEntry($"  Rolled {attackDice}d6: {FormatRolls(attackRoll)} = {attackRoll.Successes} successes");

        if (combatState.PlayerNegateNextAttack)
        {
            combatState.AddLogEntry($"  {player.Name} narrowly dodges!");
            combatState.PlayerNegateNextAttack = false;
            combatState.AddLogEntry("");
            return;
        }

        var defendRoll = _diceService.Roll(player.DefenseDice);
        combatState.AddLogEntry($"{player.Name} defends!");
        combatState.AddLogEntry($"  Rolled {player.DefenseDice}d6: {FormatRolls(defendRoll)} = {defendRoll.Successes} successes");

        var netSuccesses = Math.Max(0, attackRoll.Successes - defendRoll.Successes);
        var damage = Math.Max(0, netSuccesses + _diceService.RollDamage(enemy.BaseDamageDice) + enemy.DamageBonus - enemy.Soak);

        ApplyDamageToPlayer(player, damage, combatState);
        combatState.AddLogEntry("");
    }

    private void ExecuteDefensiveStance(Enemy enemy, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} assumes a defensive stance!");

        enemy.DefenseBonus = 4;
        enemy.DefenseTurnsRemaining = 1;

        combatState.AddLogEntry($"  Defense greatly increased, but cannot attack next turn!");
        combatState.AddLogEntry("");
    }

    private void ExecuteGuardianProtocol(Enemy enemy, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} activates guardian repair systems!");

        var healAmount = 12;
        enemy.HP = Math.Min(enemy.MaxHP, enemy.HP + healAmount);

        combatState.AddLogEntry($"  {enemy.Name} repairs {healAmount} HP! (Now at {enemy.HP}/{enemy.MaxHP} HP)");
        combatState.AddLogEntry("");
    }

    private void ExecuteWhirlwindStrike(Enemy enemy, PlayerCharacter player, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} spins its halberd in a devastating arc!");

        var attackRoll = _diceService.Roll(enemy.Attributes.Might + 4);
        combatState.AddLogEntry($"  Rolled {enemy.Attributes.Might + 4}d6: {FormatRolls(attackRoll)} = {attackRoll.Successes} successes");

        if (combatState.PlayerNegateNextAttack)
        {
            combatState.AddLogEntry($"  {player.Name} dives clear of the whirlwind!");
            combatState.PlayerNegateNextAttack = false;
            combatState.AddLogEntry("");
            return;
        }

        var defendRoll = _diceService.Roll(player.DefenseDice);
        combatState.AddLogEntry($"{player.Name} defends!");
        combatState.AddLogEntry($"  Rolled {player.DefenseDice}d6: {FormatRolls(defendRoll)} = {defendRoll.Successes} successes");

        var netSuccesses = Math.Max(0, attackRoll.Successes - defendRoll.Successes);
        var damage = Math.Max(0, netSuccesses + _diceService.RollDamage(2) + 1 - enemy.Soak);

        ApplyDamageToPlayer(player, damage, combatState);
        combatState.AddLogEntry($"  The whirlwind strike hits all enemies in the area!");
        combatState.AddLogEntry("");
    }

    private void ExecuteLastStand(Enemy enemy, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} enters Last Stand mode!");

        // Gain +2 dice to all attacks for 3 turns (track via DefenseBonus temporarily)
        combatState.AddLogEntry($"  Combat protocols overclocked! +2 dice to all attacks for 3 turns!");
        combatState.AddLogEntry($"  [Note: Attack bonus tracked - implementation requires turn counter]");
        combatState.AddLogEntry("");
    }

    // FORLORN ARCHIVIST ACTIONS

    private void ExecuteMindSpike(Enemy enemy, PlayerCharacter player, CombatState combatState)
    {
        var attackDice = enemy.Attributes.Will + 3;
        if (enemy.Phase == 2) attackDice += 1;
        if (enemy.Phase == 3) attackDice += 2;

        combatState.AddLogEntry($"{enemy.Name} lances psychic energy through your mind!");

        var attackRoll = _diceService.Roll(attackDice);
        combatState.AddLogEntry($"  Rolled {attackDice}d6: {FormatRolls(attackRoll)} = {attackRoll.Successes} successes");

        if (combatState.PlayerNegateNextAttack)
        {
            combatState.AddLogEntry($"  {player.Name} blocks the mental assault!");
            combatState.PlayerNegateNextAttack = false;
            combatState.AddLogEntry("");
            return;
        }

        var defendRoll = _diceService.Roll(Math.Max(1, player.Attributes.Will));
        combatState.AddLogEntry($"{player.Name} resists with WILL!");
        combatState.AddLogEntry($"  Rolled {Math.Max(1, player.Attributes.Will)}d6: {FormatRolls(defendRoll)} = {defendRoll.Successes} successes");

        var netSuccesses = Math.Max(0, attackRoll.Successes - defendRoll.Successes);
        var damage = netSuccesses + _diceService.RollDamage(enemy.BaseDamageDice);

        ApplyDamageToPlayer(player, damage, combatState);
        combatState.AddLogEntry($"  Psychic damage ignores physical armor!");
        combatState.AddLogEntry("");
    }

    private void ExecuteSummonRevenants(Enemy enemy, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} summons spectral echoes!");
        combatState.AddLogEntry($"  2x Scrap-Hound revenants materialize!");
        combatState.AddLogEntry($"  [Note: Summon mechanics require combat system updates]");
        combatState.AddLogEntry("");
    }

    private void ExecutePsychicScream(Enemy enemy, PlayerCharacter player, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} releases a psychic scream!");

        var attackRoll = _diceService.Roll(enemy.Attributes.Will + 3);
        combatState.AddLogEntry($"  Rolled {enemy.Attributes.Will + 3}d6: {FormatRolls(attackRoll)} = {attackRoll.Successes} successes");

        var defendRoll = _diceService.Roll(Math.Max(1, player.Attributes.Will));
        combatState.AddLogEntry($"{player.Name} resists with WILL!");
        combatState.AddLogEntry($"  Rolled {Math.Max(1, player.Attributes.Will)}d6: {FormatRolls(defendRoll)} = {defendRoll.Successes} successes");

        if (attackRoll.Successes > defendRoll.Successes)
        {
            combatState.AddLogEntry($"  {player.Name} gains +15 Psychic Stress!");
            combatState.AddLogEntry($"  (Note: Would call trauma service to add stress)");
        }
        else
        {
            combatState.AddLogEntry($"  {player.Name} resists the psychic assault!");
        }
        combatState.AddLogEntry("");
    }

    private void ExecuteMassHysteria(Enemy enemy, PlayerCharacter player, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} spreads mass hysteria!");

        var attackRoll = _diceService.Roll(enemy.Attributes.Will + 4);
        combatState.AddLogEntry($"  Rolled {enemy.Attributes.Will + 4}d6: {FormatRolls(attackRoll)} = {attackRoll.Successes} successes");

        var defendRoll = _diceService.Roll(Math.Max(1, player.Attributes.Will));
        combatState.AddLogEntry($"{player.Name} resists with WILL!");
        combatState.AddLogEntry($"  Rolled {Math.Max(1, player.Attributes.Will)}d6: {FormatRolls(defendRoll)} = {defendRoll.Successes} successes");

        if (attackRoll.Successes > defendRoll.Successes)
        {
            combatState.AddLogEntry($"  {player.Name} is [Feared] for 2 turns!");
            combatState.AddLogEntry($"  (Note: Fear status effect - implementation pending)");
        }
        else
        {
            combatState.AddLogEntry($"  {player.Name} steels their mind against the fear!");
        }
        combatState.AddLogEntry("");
    }

    private void ExecutePsychicStorm(Enemy enemy, PlayerCharacter player, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} unleashes a psychic storm!");

        var damage = _diceService.RollDamage(2) + 5; // 2d10 approx as 2d6+5

        combatState.AddLogEntry($"  The storm tears through all minds in range!");
        ApplyDamageToPlayer(player, damage, combatState);
        combatState.AddLogEntry($"  Psychic damage ignores physical armor!");
        combatState.AddLogEntry("");
    }

    // OMEGA SENTINEL ACTIONS

    private void ExecuteMaulStrike(Enemy enemy, PlayerCharacter player, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} slams down with its massive maul!");

        var attackRoll = _diceService.Roll(enemy.Attributes.Might + 4);
        combatState.AddLogEntry($"  Rolled {enemy.Attributes.Might + 4}d6: {FormatRolls(attackRoll)} = {attackRoll.Successes} successes");

        if (combatState.PlayerNegateNextAttack)
        {
            combatState.AddLogEntry($"  {player.Name} rolls clear of the massive blow!");
            combatState.PlayerNegateNextAttack = false;
            combatState.AddLogEntry("");
            return;
        }

        var defendRoll = _diceService.Roll(player.DefenseDice);
        combatState.AddLogEntry($"{player.Name} defends!");
        combatState.AddLogEntry($"  Rolled {player.DefenseDice}d6: {FormatRolls(defendRoll)} = {defendRoll.Successes} successes");

        var netSuccesses = Math.Max(0, attackRoll.Successes - defendRoll.Successes);
        var damage = netSuccesses + _diceService.RollDamage(enemy.BaseDamageDice) + enemy.DamageBonus;

        ApplyDamageToPlayer(player, damage, combatState);
        combatState.AddLogEntry("");
    }

    private void ExecuteSeismicSlam(Enemy enemy, PlayerCharacter player, CombatState combatState)
    {
        var attackDice = enemy.Phase == 1 ? enemy.Attributes.Might + 3 : enemy.Attributes.Might + 4;
        combatState.AddLogEntry($"{enemy.Name} slams the ground with earth-shaking force!");

        var attackRoll = _diceService.Roll(attackDice);
        combatState.AddLogEntry($"  Rolled {attackDice}d6: {FormatRolls(attackRoll)} = {attackRoll.Successes} successes");

        if (combatState.PlayerNegateNextAttack)
        {
            combatState.AddLogEntry($"  {player.Name} leaps above the shockwave!");
            combatState.PlayerNegateNextAttack = false;
            combatState.AddLogEntry("");
            return;
        }

        var defendRoll = _diceService.Roll(player.DefenseDice);
        combatState.AddLogEntry($"{player.Name} braces for impact!");
        combatState.AddLogEntry($"  Rolled {player.DefenseDice}d6: {FormatRolls(defendRoll)} = {defendRoll.Successes} successes");

        var netSuccesses = Math.Max(0, attackRoll.Successes - defendRoll.Successes);
        var baseDamage = enemy.Phase == 1 ? 2 : 3;
        var damage = netSuccesses + _diceService.RollDamage(baseDamage);

        ApplyDamageToPlayer(player, damage, combatState);
        combatState.AddLogEntry($"  The shockwave hits all enemies, knocking them back!");
        combatState.AddLogEntry($"  (Note: May knock into hazards - takes 2d8 electrical if knocked into conduits)");
        combatState.AddLogEntry("");
    }

    private void ExecutePowerDraw(Enemy enemy, CombatState combatState)
    {
        var healAmount = enemy.Phase == 3 ? 25 : 15;
        combatState.AddLogEntry($"{enemy.Name} drains energy from the power core!");

        enemy.HP = Math.Min(enemy.MaxHP, enemy.HP + healAmount);

        combatState.AddLogEntry($"  {enemy.Name} absorbs {healAmount} HP! (Now at {enemy.HP}/{enemy.MaxHP} HP)");
        combatState.AddLogEntry("");
    }

    private void ExecuteOverchargedMaul(Enemy enemy, PlayerCharacter player, CombatState combatState)
    {
        var attackDice = enemy.Phase == 2 ? enemy.Attributes.Might + 5 : enemy.Attributes.Might + 6;
        combatState.AddLogEntry($"{enemy.Name} charges its maul with crackling energy!");

        var attackRoll = _diceService.Roll(attackDice);
        combatState.AddLogEntry($"  Rolled {attackDice}d6: {FormatRolls(attackRoll)} = {attackRoll.Successes} successes");

        if (combatState.PlayerNegateNextAttack)
        {
            combatState.AddLogEntry($"  {player.Name} narrowly avoids the electrified strike!");
            combatState.PlayerNegateNextAttack = false;
            combatState.AddLogEntry("");
            return;
        }

        var defendRoll = _diceService.Roll(player.DefenseDice);
        combatState.AddLogEntry($"{player.Name} defends!");
        combatState.AddLogEntry($"  Rolled {player.DefenseDice}d6: {FormatRolls(defendRoll)} = {defendRoll.Successes} successes");

        var netSuccesses = Math.Max(0, attackRoll.Successes - defendRoll.Successes);
        var physicalDamage = netSuccesses + _diceService.RollDamage(enemy.BaseDamageDice) + enemy.DamageBonus;
        var electricalDamage = enemy.Phase == 2 ? _diceService.RollDamage(2) + 1 : _diceService.RollDamage(3) + 1;
        var totalDamage = physicalDamage + electricalDamage;

        ApplyDamageToPlayer(player, totalDamage, combatState);
        combatState.AddLogEntry($"  Physical + electrical damage combined!");
        combatState.AddLogEntry("");
    }

    private void ExecuteDefensiveProtocols(Enemy enemy, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} activates defensive protocols!");

        enemy.DefenseBonus = 5;
        enemy.DefenseTurnsRemaining = 2;

        combatState.AddLogEntry($"  Defense massively increased for 2 turns!");
        combatState.AddLogEntry("");
    }

    private void ExecuteOmegaProtocol(Enemy enemy, PlayerCharacter player, CombatState combatState)
    {
        combatState.AddLogEntry($"{enemy.Name} initiates OMEGA PROTOCOL!");
        combatState.AddLogEntry($"  Reality-warping energy surges through the arena!");

        var attackRoll = _diceService.Roll(enemy.Attributes.Might + 6);
        combatState.AddLogEntry($"  Rolled {enemy.Attributes.Might + 6}d6: {FormatRolls(attackRoll)} = {attackRoll.Successes} successes");

        if (combatState.PlayerNegateNextAttack)
        {
            combatState.AddLogEntry($"  {player.Name} desperately shields themselves!");
            combatState.PlayerNegateNextAttack = false;
            combatState.AddLogEntry("");
            return;
        }

        var defendRoll = _diceService.Roll(player.DefenseDice);
        combatState.AddLogEntry($"{player.Name} desperately defends!");
        combatState.AddLogEntry($"  Rolled {player.DefenseDice}d6: {FormatRolls(defendRoll)} = {defendRoll.Successes} successes");

        var netSuccesses = Math.Max(0, attackRoll.Successes - defendRoll.Successes);
        var damage = netSuccesses + _diceService.RollDamage(4) + 6; // 4d10 approx as 4d6+6

        ApplyDamageToPlayer(player, damage, combatState);
        combatState.AddLogEntry($"  Devastating AOE attack hits ALL enemies!");
        combatState.AddLogEntry($"  Everyone is knocked back into hazards!");
        combatState.AddLogEntry("");
    }

    private string FormatRolls(DiceResult result)
    {
        return string.Join(", ", result.Rolls);
    }
}
