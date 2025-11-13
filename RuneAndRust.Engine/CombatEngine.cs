using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

public class CombatEngine
{
    private static readonly ILogger _log = Log.ForContext<CombatEngine>();
    private readonly DiceService _diceService;
    private readonly SagaService _sagaService;
    private readonly LootService _lootService;
    private readonly EquipmentService _equipmentService;
    private readonly HazardService _hazardService; // [v0.6]
    private readonly PerformanceService _performanceService; // [v0.7]
    private readonly CurrencyService _currencyService; // [v0.9]

    public CombatEngine(DiceService diceService, SagaService sagaService, LootService lootService, EquipmentService equipmentService, HazardService hazardService, CurrencyService currencyService)
    {
        _diceService = diceService;
        _sagaService = sagaService;
        _lootService = lootService;
        _equipmentService = equipmentService;
        _hazardService = hazardService; // [v0.6]
        _performanceService = new PerformanceService(); // [v0.7]
        _currencyService = currencyService; // [v0.9]
    }

    /// <summary>
    /// Initialize combat with the player and enemies, roll initiative
    /// </summary>
    public CombatState InitializeCombat(PlayerCharacter player, List<Enemy> enemies, Room? currentRoom = null, bool canFlee = true)
    {
        _log.Information("Combat initiated: Player={PlayerName}, Enemies={EnemyCount}, CanFlee={CanFlee}, Room={RoomId}",
            player.Name, enemies.Count, canFlee, currentRoom?.Id);

        var combatState = new CombatState
        {
            Player = player,
            Enemies = new List<Enemy>(enemies),
            IsActive = true,
            CanFlee = canFlee,
            CurrentRoom = currentRoom
        };

        // Roll initiative for all participants
        RollInitiative(combatState);

        combatState.AddLogEntry("=== COMBAT BEGINS ===");
        combatState.AddLogEntry("Initiative order determined:");
        foreach (var participant in combatState.InitiativeOrder)
        {
            var name = participant.IsPlayer ? combatState.Player.Name : ((Enemy)participant.Character!).Name;
            combatState.AddLogEntry($"  {name} (Initiative: {participant.InitiativeRoll})");
        }
        combatState.AddLogEntry("");

        // [v0.6] Environmental hazard warning
        if (currentRoom != null && _hazardService.RoomHasActiveHazard(currentRoom))
        {
            combatState.AddLogEntry($"[WARNING] {currentRoom.HazardDescription}");

            // Display hazard details based on type
            if (currentRoom.HazardDamageDice > 0)
            {
                combatState.AddLogEntry($"  You will take {currentRoom.HazardDamageDice}d{currentRoom.HazardDamageDieSize} damage per turn!");
            }
            else if (currentRoom.HazardDamagePerTurn > 0)
            {
                combatState.AddLogEntry($"  You will take {currentRoom.HazardDamagePerTurn} damage per turn!");
            }

            if (currentRoom.HazardStressPerTurn > 0)
            {
                combatState.AddLogEntry($"  +{currentRoom.HazardStressPerTurn} Psychic Stress per turn!");
            }

            if (currentRoom.HazardRequiresCheck)
            {
                combatState.AddLogEntry($"  {currentRoom.HazardCheckAttribute} Check (DC {currentRoom.HazardCheckDC}) required to avoid damage!");
            }

            combatState.AddLogEntry("");
        }

        return combatState;
    }

    /// <summary>
    /// Roll initiative for all combatants and sort by initiative order
    /// </summary>
    private void RollInitiative(CombatState combatState)
    {
        var participants = new List<CombatParticipant>();

        // Player initiative
        var playerInitiativeRoll = _diceService.Roll(combatState.Player.Attributes.Finesse);
        participants.Add(new CombatParticipant
        {
            Name = combatState.Player.Name,
            IsPlayer = true,
            InitiativeRoll = playerInitiativeRoll.Successes,
            InitiativeAttribute = combatState.Player.Attributes.Finesse,
            Character = combatState.Player
        });

        // Enemy initiative
        foreach (var enemy in combatState.Enemies)
        {
            var enemyInitiativeRoll = _diceService.Roll(enemy.Attributes.Finesse);
            participants.Add(new CombatParticipant
            {
                Name = enemy.Name,
                IsPlayer = false,
                InitiativeRoll = enemyInitiativeRoll.Successes,
                InitiativeAttribute = enemy.Attributes.Finesse,
                Character = enemy
            });
        }

        // Sort by initiative (high to low), ties broken by attribute value
        combatState.InitiativeOrder = participants
            .OrderByDescending(p => p.InitiativeRoll)
            .ThenByDescending(p => p.InitiativeAttribute)
            .ToList();
    }

    /// <summary>
    /// Process player attack action
    /// </summary>
    public void PlayerAttack(CombatState combatState, Enemy target)
    {
        var player = combatState.Player;

        // Get weapon stats from equipped weapon (v0.3 Equipment System)
        string weaponAttribute;
        int weaponDice;
        int weaponDamageBonus;
        int accuracyBonus;

        if (player.EquippedWeapon != null)
        {
            var weapon = player.EquippedWeapon;
            weaponAttribute = weapon.WeaponAttribute;
            weaponDice = weapon.DamageDice;
            weaponDamageBonus = weapon.DamageBonus;
            accuracyBonus = weapon.AccuracyBonus;
        }
        else
        {
            // Fallback to legacy system (v0.1/v0.2) or unarmed
            weaponAttribute = !string.IsNullOrEmpty(player.WeaponAttribute) ? player.WeaponAttribute : "MIGHT";
            weaponDice = player.BaseDamage > 0 ? player.BaseDamage : 1;
            weaponDamageBonus = -2; // Unarmed penalty
            accuracyBonus = -1; // Unarmed penalty
        }

        var attributeValue = _equipmentService.GetEffectiveAttributeValue(player, weaponAttribute);
        var bonusDice = combatState.PlayerNextAttackBonusDice;

        // Reset bonus dice after use
        combatState.PlayerNextAttackBonusDice = 0;

        // Apply Battle Rage bonus if active
        if (player.BattleRageTurnsRemaining > 0)
        {
            bonusDice += 2;
        }

        // Apply accuracy bonus from equipment
        bonusDice += accuracyBonus;

        // v0.7: Apply [Analyzed] status bonus (+2 Accuracy)
        if (target.AnalyzedTurnsRemaining > 0)
        {
            bonusDice += 2;
            combatState.AddLogEntry($"  [Analyzed] grants +2 Accuracy against {target.Name}!");
        }

        // v0.7: Apply Saga of Courage performance bonus (+2 Accuracy)
        if (player.IsPerforming && player.CurrentPerformance == "Saga of Courage")
        {
            bonusDice += 2;
            combatState.AddLogEntry($"  [Saga of Courage] inspires you! +2 Accuracy!");
        }

        var totalDice = attributeValue + bonusDice;
        var attackRoll = _diceService.Roll(totalDice);

        combatState.AddLogEntry($"{player.Name} attacks {target.Name}!");
        combatState.AddLogEntry($"  Rolled {totalDice}d6: {FormatRolls(attackRoll)} = {attackRoll.Successes} successes");

        // Opponent defends
        var defendRoll = _diceService.Roll(target.Attributes.Sturdiness);
        combatState.AddLogEntry($"{target.Name} defends!");
        combatState.AddLogEntry($"  Rolled {target.Attributes.Sturdiness}d6: {FormatRolls(defendRoll)} = {defendRoll.Successes} successes");

        // Calculate damage using weapon damage dice
        var netSuccesses = Math.Max(0, attackRoll.Successes - defendRoll.Successes);

        // Roll weapon damage based on successes
        int damage = 0;
        if (netSuccesses > 0)
        {
            // v0.7: Apply [Inspired] damage bonus (+3 damage dice)
            int totalDamageDice = weaponDice;
            if (player.InspiredTurnsRemaining > 0)
            {
                totalDamageDice += 3;
                combatState.AddLogEntry($"  [Inspired] grants +3 damage dice!");
            }

            // Roll weapon damage dice
            damage = _diceService.RollDamage(totalDamageDice) + weaponDamageBonus;
            damage = Math.Max(1, damage); // Minimum 1 damage on hit
        }

        // Apply defense bonus if active
        if (target.DefenseTurnsRemaining > 0)
        {
            var reducedDamage = (int)(damage * (1 - target.DefenseBonus / 100.0));
            combatState.AddLogEntry($"{target.Name}'s defense reduces damage from {damage} to {reducedDamage}");
            damage = reducedDamage;
            target.DefenseTurnsRemaining--;
        }

        if (damage > 0)
        {
            target.HP -= damage;
            combatState.AddLogEntry($"  {target.Name} takes {damage} damage! (HP: {Math.Max(0, target.HP)}/{target.MaxHP})");

            _log.Information("Damage dealt: Attacker={Attacker}, Target={Target}, Damage={Damage}, AttackSuccesses={AttackSuccesses}, DefendSuccesses={DefendSuccesses}, RemainingHP={RemainingHP}",
                player.Name, target.Name, damage, attackRoll.Successes, defendRoll.Successes, target.HP);
        }
        else
        {
            combatState.AddLogEntry($"  The attack is deflected!");

            _log.Debug("Attack deflected: Attacker={Attacker}, Target={Target}, AttackSuccesses={AttackSuccesses}, DefendSuccesses={DefendSuccesses}",
                player.Name, target.Name, attackRoll.Successes, defendRoll.Successes);
        }

        // Check if target is defeated
        if (!target.IsAlive)
        {
            combatState.AddLogEntry($"  {target.Name} is destroyed!");

            _log.Information("Enemy defeated: Enemy={EnemyName}, Killer={PlayerName}", target.Name, player.Name);
        }

        combatState.AddLogEntry("");
    }

    /// <summary>
    /// Process player defend action
    /// </summary>
    public void PlayerDefend(CombatState combatState)
    {
        var player = combatState.Player;
        var defendRoll = _diceService.Roll(player.Attributes.Sturdiness);

        combatState.AddLogEntry($"{player.Name} takes a defensive stance!");
        combatState.AddLogEntry($"  Rolled {player.Attributes.Sturdiness}d6: {FormatRolls(defendRoll)} = {defendRoll.Successes} successes");

        // Each success = 25% damage reduction, max 75%
        var defensePercent = Math.Min(75, defendRoll.Successes * 25);
        player.DefenseBonus = defensePercent;
        player.DefenseTurnsRemaining = 1; // Lasts until next turn

        combatState.AddLogEntry($"  Defense raised by {defensePercent}% for next attack");
        combatState.AddLogEntry("");
    }

    /// <summary>
    /// v0.7: Use a consumable item in combat
    /// </summary>
    public bool PlayerUseConsumable(CombatState combatState, Consumable consumable)
    {
        var player = combatState.Player;

        combatState.AddLogEntry($"{player.Name} uses {consumable.GetDisplayName()}!");
        combatState.AddLogEntry("");

        // Apply HP restoration
        int totalHPRestore = consumable.GetTotalHPRestore();
        if (totalHPRestore > 0)
        {
            int hpBefore = player.HP;
            player.HP = Math.Min(player.MaxHP, player.HP + totalHPRestore);
            int actualHealed = player.HP - hpBefore;
            combatState.AddLogEntry($"  Restored {actualHealed} HP (now at {player.HP}/{player.MaxHP})");
        }

        // Apply Stamina restoration
        int totalStaminaRestore = consumable.GetTotalStaminaRestore();
        if (totalStaminaRestore > 0)
        {
            int staminaBefore = player.Stamina;
            player.Stamina = Math.Min(player.MaxStamina, player.Stamina + totalStaminaRestore);
            int actualRestored = player.Stamina - staminaBefore;
            combatState.AddLogEntry($"  Restored {actualRestored} Stamina (now at {player.Stamina}/{player.MaxStamina})");
        }

        // Reduce Stress
        if (consumable.StressRestore > 0)
        {
            int stressBefore = player.PsychicStress;
            player.PsychicStress = Math.Max(0, player.PsychicStress - consumable.StressRestore);
            int stressReduced = stressBefore - player.PsychicStress;
            combatState.AddLogEntry($"  Reduced Stress by {stressReduced} (now at {player.PsychicStress}/100)");
        }

        // Grant Temp HP
        if (consumable.TempHPGrant > 0)
        {
            player.TempHP += consumable.TempHPGrant;
            combatState.AddLogEntry($"  Granted {consumable.TempHPGrant} Temporary HP (total: {player.TempHP})");
        }

        // Clear status effects
        if (consumable.ClearsBleeding)
        {
            // Note: Bleeding status not yet implemented in player character
            combatState.AddLogEntry($"  Stopped bleeding");
        }

        if (consumable.ClearsPoison)
        {
            // Note: Poison status not yet implemented in player character
            combatState.AddLogEntry($"  Cured poison");
        }

        if (consumable.ClearsDisease)
        {
            // Note: Disease status not yet implemented in player character
            combatState.AddLogEntry($"  Cured disease");
        }

        combatState.AddLogEntry("");

        // Remove consumable from inventory
        player.Consumables.Remove(consumable);

        return true;
    }

    /// <summary>
    /// Process player ability use
    /// </summary>
    public bool PlayerUseAbility(CombatState combatState, Ability ability, Enemy? target = null)
    {
        var player = combatState.Player;

        _log.Information("Ability use attempt: Character={CharacterName}, Ability={AbilityName}, Target={Target}, StaminaCost={StaminaCost}, APCost={APCost}, CurrentStamina={CurrentStamina}, CurrentAP={CurrentAP}",
            player.Name, ability.Name, target?.Name ?? "None", ability.StaminaCost, ability.APCost, player.Stamina, player.AP);

        // v0.19.8: Check AP cost for Mystic abilities
        if (ability.APCost > 0)
        {
            if (player.AP < ability.APCost)
            {
                combatState.AddLogEntry($"Not enough Aether Pool! ({player.AP}/{ability.APCost} AP required)");
                _log.Debug("Ability failed: Insufficient AP for {AbilityName}", ability.Name);
                return false;
            }

            // Pay AP cost (Mystic abilities)
            player.AP -= ability.APCost;

            combatState.AddLogEntry($"{player.Name} uses {ability.Name}!");
            combatState.AddLogEntry($"  Cost: {ability.APCost} AP (Remaining: {player.AP}/{player.MaxAP})");
        }
        else
        {
            // Check stamina cost (Warrior/Adept abilities)
            if (player.Stamina < ability.StaminaCost)
            {
                combatState.AddLogEntry($"Not enough stamina! ({player.Stamina}/{ability.StaminaCost} required)");
                _log.Debug("Ability failed: Insufficient stamina for {AbilityName}", ability.Name);
                return false;
            }

            // Pay stamina cost
            player.Stamina -= ability.StaminaCost;

            combatState.AddLogEntry($"{player.Name} uses {ability.Name}!");
            combatState.AddLogEntry($"  Cost: {ability.StaminaCost} Stamina (Remaining: {player.Stamina}/{player.MaxStamina})");
        }

        _log.Information("Ability used: Character={CharacterName}, Ability={AbilityName}, Target={Target}, RemainingStamina={RemainingStamina}",
            player.Name, ability.Name, target?.Name ?? "None", player.Stamina);

        // [v0.5] Apply trauma costs for heretical abilities
        var traumaService = new TraumaEconomyService();
        int stressBefore = player.PsychicStress;
        int corruptionBefore = player.Corruption;

        switch (ability.Name)
        {
            case "Void Strike":
                traumaService.AddCorruption(player, 3);
                combatState.AddLogEntry("  ⚠️ You channel the Blight's power...");
                combatState.AddLogEntry($"  Corruption: {corruptionBefore} → {player.Corruption}/100");
                break;

            case "Psychic Lash":
                traumaService.AddStress(player, 10);
                combatState.AddLogEntry("  ⚠️ You project your trauma outward as a weapon...");
                combatState.AddLogEntry($"  Psychic Stress: {stressBefore} → {player.PsychicStress}/100");
                break;

            case "Desperate Gambit":
                traumaService.AddStress(player, 15);
                traumaService.AddCorruption(player, 5);
                combatState.AddLogEntry("  ‼️ You draw deeply from the Blight's well of power...");
                combatState.AddLogEntry($"  Psychic Stress: {stressBefore} → {player.PsychicStress}/100");
                combatState.AddLogEntry($"  Corruption: {corruptionBefore} → {player.Corruption}/100");
                break;

            // [v0.6] NEW HERETICAL ABILITIES

            case "Blight Surge":
                traumaService.AddStress(player, 8);
                traumaService.AddCorruption(player, 2);
                combatState.AddLogEntry("  ⚠️ Blight energy surges through you...");
                combatState.AddLogEntry($"  Psychic Stress: {stressBefore} → {player.PsychicStress}/100");
                combatState.AddLogEntry($"  Corruption: {corruptionBefore} → {player.Corruption}/100");
                break;

            case "Blood Sacrifice":
                // Special: Costs HP instead of Stamina
                if (player.HP >= 30)
                {
                    player.HP = Math.Max(1, player.HP - 20); // Lose 20 HP, minimum 1
                    traumaService.AddCorruption(player, 3);
                    combatState.AddLogEntry("  ⚠️ You sacrifice your vitality for power...");
                    combatState.AddLogEntry($"  HP: {player.HP + 20} → {player.HP}/{player.MaxHP}");
                    combatState.AddLogEntry($"  Corruption: {corruptionBefore} → {player.Corruption}/100");
                }
                else
                {
                    combatState.AddLogEntry("  ⚠️ FAILED: HP too low (minimum 30 HP required)!");
                    return false; // Cancel ability
                }
                break;

            case "Mass Psychic Lash":
                traumaService.AddStress(player, 20);
                combatState.AddLogEntry("  ⚠️ You project your trauma to all enemies...");
                combatState.AddLogEntry($"  Psychic Stress: {stressBefore} → {player.PsychicStress}/100");
                break;

            case "Corruption Nova":
                traumaService.AddCorruption(player, 10);
                combatState.AddLogEntry("  ⚠️ You release accumulated Corruption as a destructive wave...");
                combatState.AddLogEntry($"  Corruption: {corruptionBefore} → {player.Corruption}/100");
                break;

            case "Siphon Sanity":
                combatState.AddLogEntry("  You drain enemy mental coherence...");
                // Stress recovery handled after damage calculation
                break;

            case "Glitch Reality":
                traumaService.AddStress(player, 5);
                traumaService.AddCorruption(player, 4);
                combatState.AddLogEntry("  ⚠️ You tear at the fabric of reality...");
                combatState.AddLogEntry($"  Psychic Stress: {stressBefore} → {player.PsychicStress}/100");
                combatState.AddLogEntry($"  Corruption: {corruptionBefore} → {player.Corruption}/100");
                break;

            // v0.19.8: Rust-Witch heretical abilities (EXTREME Corruption costs)
            case "Corrosive Curse":
                traumaService.AddCorruption(player, 2);
                combatState.AddLogEntry("  ⚠️ You channel corrosive entropy...");
                combatState.AddLogEntry($"  Corruption: {corruptionBefore} → {player.Corruption}/100");
                break;

            case "System Shock":
                traumaService.AddCorruption(player, 3);
                combatState.AddLogEntry("  ⚠️ You disrupt reality's code with heretical power...");
                combatState.AddLogEntry($"  Corruption: {corruptionBefore} → {player.Corruption}/100");
                break;

            case "Flash Rust":
                traumaService.AddCorruption(player, 4);
                combatState.AddLogEntry("  ⚠️ You accelerate entropy across the battlefield...");
                combatState.AddLogEntry($"  Corruption: {corruptionBefore} → {player.Corruption}/100");
                break;

            case "Unmaking Word":
                traumaService.AddCorruption(player, 4);
                combatState.AddLogEntry("  ⚠️ You speak the word of dissolution...");
                combatState.AddLogEntry($"  Corruption: {corruptionBefore} → {player.Corruption}/100");
                break;

            case "Entropic Cascade":
                traumaService.AddCorruption(player, 6);
                combatState.AddLogEntry("  ‼️ You invoke ultimate entropy at terrible cost...");
                combatState.AddLogEntry($"  Corruption: {corruptionBefore} → {player.Corruption}/100");
                break;
        }

        // Check for threshold warnings after adding trauma
        var (shouldWarn, warningMessage) = traumaService.CheckForThresholdWarning(player);
        if (shouldWarn)
        {
            combatState.AddLogEntry("");
            combatState.AddLogEntry(warningMessage);
        }

        // Roll for ability
        var attributeValue = player.GetAttributeValue(ability.AttributeUsed);
        var totalDice = attributeValue + ability.BonusDice;
        var abilityRoll = _diceService.Roll(totalDice);

        combatState.AddLogEntry($"  Rolled {totalDice}d6: {FormatRolls(abilityRoll)} = {abilityRoll.Successes} successes");

        // Check if ability succeeds
        if (abilityRoll.Successes < ability.SuccessThreshold)
        {
            combatState.AddLogEntry($"  Ability fails! (Needed {ability.SuccessThreshold} successes)");
            combatState.AddLogEntry("");
            return true; // Still consumed turn and stamina
        }

        combatState.AddLogEntry($"  Ability succeeds!");

        // v0.7: Check if ability is a performance (Skald channeling)
        if (PerformanceService.IsPerformanceAbility(ability.Name))
        {
            var performanceResult = _performanceService.StartPerformance(player, ability.Name, abilityRoll.Successes);
            if (performanceResult.Success)
            {
                combatState.AddLogEntry($"  {performanceResult.Message}");

                // v0.7: Special performance effects
                switch (ability.Name)
                {
                    case "Saga of the Einherjar":
                        // Grant temporary HP (2d6)
                        int tempHP = _diceService.RollDamage(ability.DamageDice);
                        player.TempHP += tempHP;
                        combatState.AddLogEntry($"  All allies gain {tempHP} Temporary HP!");

                        // Apply [Inspired] status (+3 damage dice)
                        player.InspiredTurnsRemaining = performanceResult.Duration;
                        combatState.AddLogEntry($"  All allies gain [Inspired] (+3 damage dice) for {performanceResult.Duration} rounds!");
                        break;

                    case "Saga of Courage":
                        combatState.AddLogEntry($"  Your inspiring battle hymn fills allies with courage!");
                        combatState.AddLogEntry($"  All allies gain +2 Accuracy while the saga continues!");
                        break;

                    case "Dirge of Defeat":
                        combatState.AddLogEntry($"  Your mournful dirge saps the will of your enemies!");
                        combatState.AddLogEntry($"  All enemies suffer -2 to their rolls while the dirge plays!");
                        break;

                    case "Lay of the Iron Wall":
                        combatState.AddLogEntry($"  Your protective chant hardens the resolve of your allies!");
                        combatState.AddLogEntry($"  All allies gain +2 Soak (damage reduction) while the lay continues!");
                        break;
                }
            }
            else
            {
                combatState.AddLogEntry($"  {performanceResult.Message}");
            }

            combatState.AddLogEntry("");
            return true; // Performance started, turn complete
        }

        // Apply ability effects
        switch (ability.Type)
        {
            case AbilityType.Attack:
                ProcessAttackAbility(combatState, ability, target, abilityRoll.Successes);
                break;

            case AbilityType.Defense:
                ProcessDefenseAbility(combatState, ability);
                break;

            case AbilityType.Utility:
                ProcessUtilityAbility(combatState, ability);
                break;

            case AbilityType.Control:
                ProcessControlAbility(combatState, ability, target);
                break;
        }

        combatState.AddLogEntry("");
        return true;
    }

    private void ProcessAttackAbility(CombatState combatState, Ability ability, Enemy? target, int successes)
    {
        if (target == null || !target.IsAlive)
        {
            combatState.AddLogEntry("  No valid target!");
            return;
        }

        int damage = 0;

        // v0.7.1: Check for Strike (standard weapon attack)
        if (ability.Name == "Strike")
        {
            var player = combatState.Player;

            // Base weapon damage
            int baseDamage = 1; // Default 1d6
            if (player.EquippedWeapon != null)
            {
                baseDamage = player.EquippedWeapon.DamageDice;
            }
            else if (player.BaseDamage > 0)
            {
                baseDamage = player.BaseDamage; // Legacy support
            }

            // Roll base damage + successes
            int totalDice = baseDamage + successes;
            damage = _diceService.RollDamage(totalDice);

            combatState.AddLogEntry($"  Weapon damage: {baseDamage}d6 + {successes} successes = {totalDice}d6");
            combatState.AddLogEntry($"  Rolled {totalDice}d6 for damage: {damage}");

            // v0.7.1: Apply stance modifier (Defensive Stance = -25% damage)
            if (player.ActiveStance?.Type == StanceType.Defensive)
            {
                int originalDamage = damage;
                damage = (int)(damage * player.ActiveStance.DamageMultiplier);
                combatState.AddLogEntry($"  Defensive Stance penalty: {originalDamage} → {damage} damage (-25%)");
            }

            ApplyDamageToEnemy(combatState, target, damage, ability.IgnoresArmor);
            return; // Early return
        }
        // Check for Power Strike (double weapon damage)
        else if (ability.Name == "Power Strike")
        {
            var weaponDamage = _diceService.RollDamage(combatState.Player.BaseDamage);
            damage = weaponDamage * 2;
            combatState.AddLogEntry($"  Weapon damage doubled: {damage}");
        }
        // Check for Cleaving Strike (AOE damage)
        else if (ability.Name == "Cleaving Strike")
        {
            damage = _diceService.RollDamage(ability.DamageDice);
            combatState.AddLogEntry($"  Rolled {ability.DamageDice}d6 for damage: {damage}");

            // Apply damage to primary target
            ApplyDamageToEnemy(combatState, target, damage, ability.IgnoresArmor);

            // If 3+ successes, also damage another enemy
            if (successes >= 3)
            {
                var adjacentEnemy = combatState.Enemies.FirstOrDefault(e => e.IsAlive && e != target);
                if (adjacentEnemy != null)
                {
                    var splashDamage = damage / 2;
                    combatState.AddLogEntry($"  Cleaving attack strikes {adjacentEnemy.Name}!");
                    ApplyDamageToEnemy(combatState, adjacentEnemy, splashDamage, ability.IgnoresArmor);
                }
            }
            return; // Early return to avoid applying damage twice
        }
        // Check for Precision Strike (applies bleeding)
        else if (ability.Name == "Precision Strike")
        {
            damage = _diceService.RollDamage(ability.DamageDice);
            combatState.AddLogEntry($"  Rolled {ability.DamageDice}d6 for damage: {damage}");

            ApplyDamageToEnemy(combatState, target, damage, ability.IgnoresArmor);

            // Apply bleeding if 3+ successes
            if (successes >= 3)
            {
                target.BleedingTurnsRemaining = 2;
                combatState.AddLogEntry($"  {target.Name} is bleeding! (1d6 damage for 2 turns)");
                _log.Information("Status effect applied: Enemy={EnemyName}, Effect=Bleeding, Duration={Duration}, Successes={Successes}",
                    target.Name, 2, successes);
            }
            return; // Early return
        }
        // Check for Chain Lightning (AOE to all enemies)
        else if (ability.Name == "Chain Lightning")
        {
            var damageDice = successes >= 4 ? 2 : 1;
            combatState.AddLogEntry($"  Lightning chains across all enemies!");

            foreach (var enemy in combatState.Enemies.Where(e => e.IsAlive))
            {
                var lightningDamage = _diceService.RollDamage(damageDice);
                combatState.AddLogEntry($"  {enemy.Name} struck for {lightningDamage} damage!");
                enemy.HP -= lightningDamage;

                if (!enemy.IsAlive)
                {
                    combatState.AddLogEntry($"  {enemy.Name} is destroyed!");
                }
            }
            return; // Early return
        }
        // [v0.5] Void Strike - Corrupted strike (3d8 damage, ignores armor)
        else if (ability.Name == "Void Strike")
        {
            var baseDamage = _diceService.RollDamage(3);
            damage = (int)(baseDamage * 1.33); // Approximate 3d8 with 3d6 * 1.33
            combatState.AddLogEntry($"  Corrupted energy tears through {target.Name}!");
            combatState.AddLogEntry($"  Rolled 3d8 (approx) for damage: {damage}");
            ApplyDamageToEnemy(combatState, target, damage, ability.IgnoresArmor);
            return; // Early return
        }
        // [v0.5] Psychic Lash - Mental assault (2d6 psychic damage, ignores armor)
        else if (ability.Name == "Psychic Lash")
        {
            damage = _diceService.RollDamage(2);
            combatState.AddLogEntry($"  Psychic assault strikes {target.Name}'s mind!");
            combatState.AddLogEntry($"  Rolled 2d6 psychic damage: {damage}");
            ApplyDamageToEnemy(combatState, target, damage, ability.IgnoresArmor);
            return; // Early return
        }
        // [v0.5] Desperate Gambit - Ultimate AOE heretical ability (4d10 to all enemies)
        else if (ability.Name == "Desperate Gambit")
        {
            combatState.AddLogEntry($"  ‼️ Aetheric energy explodes outward!");

            foreach (var enemy in combatState.Enemies.Where(e => e.IsAlive))
            {
                // Roll 4d10 for each enemy (using d6 approximation: 1.67 * 4d6)
                var baseDamage = _diceService.RollDamage(4);
                var desperateDamage = (int)(baseDamage * 1.67); // Approximate d10 distribution
                combatState.AddLogEntry($"  {enemy.Name} takes {desperateDamage} damage!");
                enemy.HP -= desperateDamage;

                if (!enemy.IsAlive)
                {
                    combatState.AddLogEntry($"  {enemy.Name} is obliterated!");
                }
            }
            return; // Early return
        }
        // [v0.6] Blight Surge - Corruption debuff attack (3d6 damage)
        else if (ability.Name == "Blight Surge")
        {
            damage = _diceService.RollDamage(3);
            combatState.AddLogEntry($"  Blight energy surges into {target.Name}!");
            combatState.AddLogEntry($"  Rolled 3d6 damage: {damage}");
            ApplyDamageToEnemy(combatState, target, damage, ability.IgnoresArmor);
            combatState.AddLogEntry($"  {target.Name} gains [Corrupted] (+20% Stress from all sources for 3 turns)");
            return; // Early return
        }
        // [v0.6] Blood Sacrifice - Already handled in trauma cost section (HP check)
        else if (ability.Name == "Blood Sacrifice")
        {
            var baseDamage = _diceService.RollDamage(4);
            damage = (int)(baseDamage * 1.33); // Approximate 4d8 with 4d6 * 1.33
            combatState.AddLogEntry($"  Your blood fuels devastating power!");
            combatState.AddLogEntry($"  Rolled 4d8 (approx) damage: {damage}");
            ApplyDamageToEnemy(combatState, target, damage, ability.IgnoresArmor);
            return; // Early return
        }
        // [v0.6] Mass Psychic Lash - AOE psychic damage (2d6 to all enemies)
        else if (ability.Name == "Mass Psychic Lash")
        {
            combatState.AddLogEntry($"  ⚠️ Your trauma cascades outward to all minds!");

            foreach (var enemy in combatState.Enemies.Where(e => e.IsAlive))
            {
                var psychicDamage = _diceService.RollDamage(2);
                combatState.AddLogEntry($"  {enemy.Name} takes {psychicDamage} psychic damage!");
                enemy.HP -= psychicDamage;

                if (!enemy.IsAlive)
                {
                    combatState.AddLogEntry($"  {enemy.Name}'s mind shatters!");
                }
            }
            return; // Early return
        }
        // [v0.6] Corruption Nova - Scaling AOE damage (5d6 base + 1d6 per 20 Corruption)
        else if (ability.Name == "Corruption Nova")
        {
            var player = combatState.Player;
            var baseDice = 5;
            var bonusDice = player.Corruption / 20; // +1d6 per 20 Corruption
            var totalDice = baseDice + bonusDice;

            combatState.AddLogEntry($"  ⚠️ Corruption explodes outward in a devastating wave!");
            combatState.AddLogEntry($"  Damage scales with your Corruption: {baseDice}d6 + {bonusDice}d6 = {totalDice}d6 total");

            foreach (var enemy in combatState.Enemies.Where(e => e.IsAlive))
            {
                var novaDamage = _diceService.RollDamage(totalDice);
                combatState.AddLogEntry($"  {enemy.Name} takes {novaDamage} corrupted damage!");
                enemy.HP -= novaDamage;

                if (!enemy.IsAlive)
                {
                    combatState.AddLogEntry($"  {enemy.Name} is consumed by the Blight!");
                }
            }
            return; // Early return
        }
        // [v0.6] Siphon Sanity - Drains enemy's sanity, recovers your Stress
        else if (ability.Name == "Siphon Sanity")
        {
            var player = combatState.Player;
            damage = _diceService.RollDamage(2);
            combatState.AddLogEntry($"  You drain {target.Name}'s mental coherence!");
            combatState.AddLogEntry($"  Rolled 2d6 psychic damage: {damage}");
            ApplyDamageToEnemy(combatState, target, damage, ability.IgnoresArmor);

            // Recover Stress equal to damage dealt
            var stressBefore = player.PsychicStress;
            player.PsychicStress = Math.Max(0, player.PsychicStress - damage);
            var stressRecovered = stressBefore - player.PsychicStress;
            combatState.AddLogEntry($"  You recover {stressRecovered} Psychic Stress! ({stressBefore} → {player.PsychicStress})");
            return; // Early return
        }
        // [v0.6] Glitch Reality - Random chaos effect (1d6 roll)
        else if (ability.Name == "Glitch Reality")
        {
            var player = combatState.Player;
            var effectRoll = _diceService.Roll(1).Rolls[0]; // 1d6

            combatState.AddLogEntry($"  ⚠️ Reality warps unpredictably... (Rolled {effectRoll})");

            switch (effectRoll)
            {
                case 1: // All enemies take 2d10 damage
                    combatState.AddLogEntry($"  Effect: Reality Tear - All enemies take massive damage!");
                    foreach (var enemy in combatState.Enemies.Where(e => e.IsAlive))
                    {
                        var tearDamage = _diceService.RollDamage(2) + 5; // 2d10 approx as 2d6+5
                        combatState.AddLogEntry($"  {enemy.Name} takes {tearDamage} damage!");
                        enemy.HP -= tearDamage;
                        if (!enemy.IsAlive) combatState.AddLogEntry($"  {enemy.Name} is torn apart!");
                    }
                    break;

                case 2: // All allies heal 2d10 HP
                    combatState.AddLogEntry($"  Effect: Temporal Flux - You heal!");
                    var healAmount = _diceService.RollDamage(2) + 5; // 2d10 approx
                    player.HP = Math.Min(player.MaxHP, player.HP + healAmount);
                    combatState.AddLogEntry($"  You heal {healAmount} HP! (Now at {player.HP}/{player.MaxHP})");
                    break;

                case 3: // All enemies gain [Disoriented] for 2 turns
                    combatState.AddLogEntry($"  Effect: Mass Confusion - All enemies become disoriented!");
                    foreach (var enemy in combatState.Enemies.Where(e => e.IsAlive))
                    {
                        combatState.AddLogEntry($"  {enemy.Name} is [Disoriented] for 2 turns (-2 dice to all actions)");
                    }
                    break;

                case 4: // You teleport to random location in room
                    combatState.AddLogEntry($"  Effect: Spatial Rift - You blink across the battlefield!");
                    combatState.AddLogEntry($"  (Position reset, enemies lose targeting)");
                    break;

                case 5: // Reality tears: Both allies and enemies take 1d10 damage
                    combatState.AddLogEntry($"  Effect: Reality Collapse - EVERYONE takes damage!");
                    player.HP -= _diceService.RollDamage(1) + 3; // 1d10 approx
                    combatState.AddLogEntry($"  You take damage! (Now at {player.HP}/{player.MaxHP})");
                    foreach (var enemy in combatState.Enemies.Where(e => e.IsAlive))
                    {
                        var collapseDamage = _diceService.RollDamage(1) + 3;
                        enemy.HP -= collapseDamage;
                        combatState.AddLogEntry($"  {enemy.Name} takes {collapseDamage} damage!");
                        if (!enemy.IsAlive) combatState.AddLogEntry($"  {enemy.Name} is destroyed!");
                    }
                    break;

                case 6: // Time stutters: You gain extra turn immediately
                    combatState.AddLogEntry($"  Effect: Time Stutter - You gain an EXTRA TURN!");
                    combatState.AddLogEntry($"  (Note: Extra turn mechanic - would trigger immediately)");
                    break;
            }
            return; // Early return
        }
        // v0.7: Anatomical Insight - Apply [Vulnerable] status (Architect ability)
        else if (ability.Name == "Anatomical Insight")
        {
            damage = _diceService.RollDamage(ability.DamageDice);
            combatState.AddLogEntry($"  You identify structural weaknesses in {target.Name}!");
            combatState.AddLogEntry($"  Rolled {ability.DamageDice}d6 for damage: {damage}");

            ApplyDamageToEnemy(combatState, target, damage, ability.IgnoresArmor);

            // Apply [Vulnerable] status
            target.VulnerableTurnsRemaining = 3;
            combatState.AddLogEntry($"  {target.Name} is [Vulnerable] for 3 turns! (+25% damage taken)");
            _log.Information("Status effect applied: Enemy={EnemyName}, Effect=Vulnerable, Duration={Duration}, Ability={AbilityName}",
                target.Name, 3, ability.Name);
            return; // Early return
        }
        // v0.7: Exploit Design Flaw - Apply [Analyzed] status (Architect ability)
        else if (ability.Name == "Exploit Design Flaw")
        {
            damage = _diceService.RollDamage(ability.DamageDice);
            combatState.AddLogEntry($"  You analyze {target.Name}'s design patterns and expose critical flaws!");
            combatState.AddLogEntry($"  Rolled {ability.DamageDice}d6 for damage: {damage}");

            ApplyDamageToEnemy(combatState, target, damage, ability.IgnoresArmor);

            // Apply [Analyzed] status
            target.AnalyzedTurnsRemaining = 4;
            combatState.AddLogEntry($"  {target.Name} is [Analyzed] for 4 turns! (All attackers gain +2 Accuracy)");
            _log.Information("Status effect applied: Enemy={EnemyName}, Effect=Analyzed, Duration={Duration}, Ability={AbilityName}",
                target.Name, 4, ability.Name);
            return; // Early return
        }
        // v0.19.8: Aether Dart - Basic Mystic attack (2d6 Arcane damage)
        else if (ability.Name == "Aether Dart")
        {
            var player = combatState.Player;
            var willBonus = player.Attributes.Will / 2; // Spells scale with WILL

            damage = _diceService.RollDamage(ability.DamageDice);
            damage += willBonus; // Add WILL bonus to damage

            combatState.AddLogEntry($"  A compressed bolt of corrupted Aether strikes {target.Name}!");
            combatState.AddLogEntry($"  Rolled {ability.DamageDice}d6 + {willBonus} WILL bonus = {damage} Arcane damage");

            ApplyDamageToEnemy(combatState, target, damage, false); // Respects armor
            return; // Early return
        }
        // v0.19.8: Corrosive Curse - Apply [Corroded] (Rust-Witch Tier 1)
        else if (ability.Name == "Corrosive Curse")
        {
            combatState.AddLogEntry($"  You whisper words of entropy at {target.Name}...");

            // Apply 1 stack of [Corroded]
            target.CorrodedStacks = Math.Min(5, target.CorrodedStacks + 1);
            target.CorrodedStackDurations.Add(3); // 3 turns per stack

            combatState.AddLogEntry($"  {target.Name} gains [Corroded] x{target.CorrodedStacks}!");
            combatState.AddLogEntry($"  (1d6 damage/turn per stack, -2 Armor per stack)");

            _log.Information("Status effect applied: Enemy={EnemyName}, Effect=Corroded, Stacks={Stacks}",
                target.Name, target.CorrodedStacks);
            return; // Early return
        }
        // v0.19.8: System Shock - 2 stacks [Corroded] + [Stunned] vs Mechanical (Rust-Witch Tier 2)
        else if (ability.Name == "System Shock")
        {
            combatState.AddLogEntry($"  You disrupt {target.Name}'s fundamental code!");

            // Apply 2 stacks of [Corroded]
            int stacksToAdd = 2;
            target.CorrodedStacks = Math.Min(5, target.CorrodedStacks + stacksToAdd);
            for (int i = 0; i < stacksToAdd; i++)
            {
                target.CorrodedStackDurations.Add(3);
            }

            combatState.AddLogEntry($"  {target.Name} gains [Corroded] x{target.CorrodedStacks}!");

            // Check if target is Mechanical (placeholder - would need enemy tags)
            if (target.Type.ToString().Contains("Construct") || target.Type.ToString().Contains("Sentinel"))
            {
                target.StunTurnsRemaining = 1;
                combatState.AddLogEntry($"  {target.Name} is [Stunned] for 1 turn! (Mechanical vulnerability)");
            }

            return; // Early return
        }
        // v0.19.8: Flash Rust - 2 stacks [Corroded] to ALL enemies (Rust-Witch Tier 2 AoE)
        else if (ability.Name == "Flash Rust")
        {
            combatState.AddLogEntry($"  ⚠️ Entropy cascades across the battlefield!");

            foreach (var enemy in combatState.Enemies.Where(e => e.IsAlive))
            {
                int stacksToAdd = 2;
                enemy.CorrodedStacks = Math.Min(5, enemy.CorrodedStacks + stacksToAdd);
                for (int i = 0; i < stacksToAdd; i++)
                {
                    enemy.CorrodedStackDurations.Add(3);
                }

                combatState.AddLogEntry($"  {enemy.Name} gains [Corroded] x{enemy.CorrodedStacks}!");
            }

            return; // Early return
        }
        // v0.19.8: Unmaking Word - DOUBLE [Corroded] stacks (Rust-Witch Tier 3)
        else if (ability.Name == "Unmaking Word")
        {
            combatState.AddLogEntry($"  You speak the word of dissolution...");

            int currentStacks = target.CorrodedStacks;
            int newStacks = Math.Min(5, currentStacks * 2);
            int stacksAdded = newStacks - currentStacks;

            if (stacksAdded > 0)
            {
                target.CorrodedStacks = newStacks;
                for (int i = 0; i < stacksAdded; i++)
                {
                    target.CorrodedStackDurations.Add(3);
                }
                combatState.AddLogEntry($"  {target.Name}'s [Corroded] stacks DOUBLE! ({currentStacks} → {newStacks})");
            }
            else
            {
                combatState.AddLogEntry($"  {target.Name} is already at maximum [Corroded] (5 stacks)!");
            }

            return; // Early return
        }
        // v0.19.8: Entropic Cascade - Execute or massive damage (Rust-Witch Capstone)
        else if (ability.Name == "Entropic Cascade")
        {
            var player = combatState.Player;

            // Check execute conditions: >50% Corruption OR 5 [Corroded] stacks
            bool canExecute = (target.CorrodedStacks >= 5) || (player.Corruption >= 50);

            if (canExecute)
            {
                combatState.AddLogEntry($"  ‼️ ENTROPIC CASCADE: {target.Name} dissolves into nothingness!");
                target.HP = 0;
                combatState.AddLogEntry($"  {target.Name} is EXECUTED!");
            }
            else
            {
                damage = _diceService.RollDamage(ability.DamageDice);
                combatState.AddLogEntry($"  Massive entropy wave strikes {target.Name}!");
                combatState.AddLogEntry($"  Rolled {ability.DamageDice}d6 Arcane damage: {damage}");
                ApplyDamageToEnemy(combatState, target, damage, false);
            }

            return; // Early return
        }
        // Check if ability has special damage dice (like Aetheric Bolt)
        else if (ability.DamageDice > 0)
        {
            damage = _diceService.RollDamage(ability.DamageDice);
            combatState.AddLogEntry($"  Rolled {ability.DamageDice}d6 for damage: {damage}");
        }
        else
        {
            damage = successes;
        }

        // Apply damage to target (standard path)
        ApplyDamageToEnemy(combatState, target, damage, ability.IgnoresArmor);
    }

    private void ApplyDamageToEnemy(CombatState combatState, Enemy target, int damage, bool ignoresArmor)
    {
        // v0.7: Apply [Vulnerable] status (+25% damage)
        if (target.VulnerableTurnsRemaining > 0)
        {
            var vulnerableDamage = (int)(damage * 1.25);
            if (vulnerableDamage > damage)
            {
                combatState.AddLogEntry($"  [Vulnerable] increases damage from {damage} to {vulnerableDamage}!");
                damage = vulnerableDamage;
            }
        }

        // v0.19.8: Apply Soak (flat damage reduction) unless ignored
        if (!ignoresArmor && target.Soak > 0)
        {
            // Calculate effective Soak (reduced by [Corroded] stacks: -2 per stack)
            int corrodedArmorReduction = target.CorrodedStacks * 2;
            int effectiveSoak = Math.Max(0, target.Soak - corrodedArmorReduction);

            if (effectiveSoak > 0)
            {
                int soakedDamage = Math.Max(0, damage - effectiveSoak);
                if (corrodedArmorReduction > 0)
                {
                    combatState.AddLogEntry($"  {target.Name}'s armor reduced by [Corroded]: {target.Soak} → {effectiveSoak} Soak");
                }
                combatState.AddLogEntry($"  Armor absorbs {effectiveSoak} damage ({damage} → {soakedDamage})");
                damage = soakedDamage;
            }
            else if (corrodedArmorReduction > 0)
            {
                combatState.AddLogEntry($"  {target.Name}'s armor completely destroyed by [Corroded]!");
            }
        }

        // Check if ability ignores armor
        if (!ignoresArmor && target.DefenseTurnsRemaining > 0)
        {
            var reducedDamage = (int)(damage * (1 - target.DefenseBonus / 100.0));
            combatState.AddLogEntry($"  {target.Name}'s defense reduces damage from {damage} to {reducedDamage}");
            damage = reducedDamage;
        }
        else if (ignoresArmor)
        {
            combatState.AddLogEntry($"  Ignores armor!");
        }

        // Apply shield absorption if player has one
        if (combatState.Player.ShieldAbsorptionRemaining > 0 && target == null)
        {
            // This is for player damage, not enemy damage
        }

        if (target != null)
        {
            target.HP -= damage;
            combatState.AddLogEntry($"  {target.Name} takes {damage} damage! (HP: {Math.Max(0, target.HP)}/{target.MaxHP})");

            if (!target.IsAlive)
            {
                combatState.AddLogEntry($"  {target.Name} is destroyed!");
            }
        }
    }

    private void ProcessDefenseAbility(CombatState combatState, Ability ability)
    {
        var player = combatState.Player;

        // v0.7.1: Check for Defensive Stance (enter stance)
        if (ability.Name == "Defensive Stance")
        {
            player.ActiveStance = Stance.CreateDefensiveStance();
            combatState.AddLogEntry($"  Entered Defensive Stance!");
            combatState.AddLogEntry($"  Effects: +3 Soak, -25% damage dealt, +1d to defensive Resolve Checks");
            combatState.AddLogEntry($"  Exit as a Free Action at any time.");
        }
        // Check for Quick Dodge (negates next attack)
        else if (ability.NegateNextAttack)
        {
            combatState.PlayerNegateNextAttack = true;
            combatState.AddLogEntry($"  {player.Name} will dodge the next attack completely!");
        }
        // Check for Aetheric Shield (damage absorption)
        else if (ability.Name == "Aetheric Shield")
        {
            player.ShieldAbsorptionRemaining = 15;
            combatState.AddLogEntry($"  Aetheric shield created! (Absorbs next 15 damage)");
        }
        // v0.19.8: Runic Barrier - Create defensive wall (Vard-Warden)
        else if (ability.Name == "Runic Barrier")
        {
            combatState.AddLogEntry($"  You manifest a Runic Barrier of solidified Aether!");
            combatState.AddLogEntry($"  Barrier has 30 HP, blocks movement and line-of-sight for 2 turns");
            combatState.AddLogEntry($"  (Note: Full barrier mechanics require RunicConstruct system integration)");

            _log.Information("Runic Barrier created: Character={Name}, Rank={Rank}",
                player.Name, ability.CurrentRank);
        }
        // v0.19.8: Rune of Shielding - Buff ally (Vard-Warden)
        else if (ability.Name == "Rune of Shielding")
        {
            player.RuneOfShieldingTurnsRemaining = 3;
            combatState.AddLogEntry($"  Protective rune inscribed!");
            combatState.AddLogEntry($"  +2 Soak and Corruption resistance for 3 turns");

            _log.Information("Rune of Shielding applied: Character={Name}",
                player.Name);
        }
        // v0.19.8: Glyph of Sanctuary - Party-wide protection (Vard-Warden)
        else if (ability.Name == "Glyph of Sanctuary")
        {
            int tempHP = _diceService.RollDamage(2); // 2d6
            player.GlyphOfSanctuaryTempHP += tempHP;
            player.GlyphOfSanctuaryStressImmunity = 2;

            combatState.AddLogEntry($"  Glyph of Sanctuary radiates protective energy!");
            combatState.AddLogEntry($"  Gained {tempHP} temporary HP and Stress immunity for 2 turns!");

            _log.Information("Glyph of Sanctuary used: Character={Name}, TempHP={TempHP}",
                player.Name, tempHP);
        }
        // Shield Wall or similar
        else
        {
            player.DefenseBonus = ability.DefensePercent;
            player.DefenseTurnsRemaining = ability.DefenseDuration;
            combatState.AddLogEntry($"  Defense raised by {ability.DefensePercent}% for {ability.DefenseDuration} turns");
        }
    }

    private void ProcessUtilityAbility(CombatState combatState, Ability ability)
    {
        var player = combatState.Player;

        // Battle Rage - +2 dice to attacks for 3 turns
        if (ability.Name == "Battle Rage")
        {
            player.BattleRageTurnsRemaining = 3;
            combatState.AddLogEntry($"  Entered battle rage! (+2 dice to attacks for 3 turns, +25% damage taken)");
        }
        // Survivalist - heal 2d6 HP
        else if (ability.Name == "Survivalist")
        {
            var healAmount = _diceService.RollDamage(2);
            player.HP = Math.Min(player.MaxHP, player.HP + healAmount);
            combatState.AddLogEntry($"  Restored {healAmount} HP! (HP: {player.HP}/{player.MaxHP})");
        }
        // Exploit Weakness - grants bonus dice to next attack
        else if (ability.NextAttackBonusDice > 0)
        {
            combatState.PlayerNextAttackBonusDice = ability.NextAttackBonusDice;
            combatState.AddLogEntry($"  Next attack gains +{ability.NextAttackBonusDice} bonus dice!");
        }
        // v0.7: Rousing Verse - Restore stamina (Skald ability)
        else if (ability.Name == "Rousing Verse")
        {
            int staminaRestore = 30;
            int staminaBefore = player.Stamina;
            player.Stamina = Math.Min(player.MaxStamina, player.Stamina + staminaRestore);
            int actualRestore = player.Stamina - staminaBefore;
            combatState.AddLogEntry($"  Your rousing verse reinvigorates you!");
            combatState.AddLogEntry($"  Restored {actualRestore} Stamina! ({staminaBefore} → {player.Stamina}/{player.MaxStamina})");
        }
        // v0.19.8: Focus Aether - Restore AP (Mystic core ability)
        else if (ability.Name == "Focus Aether")
        {
            // Restore amount scales with rank: 25/35/50 AP
            int apRestore = ability.CurrentRank switch
            {
                1 => 25,
                2 => 35,
                3 => 50,
                _ => 25
            };

            int apBefore = player.AP;
            player.AP = Math.Min(player.MaxAP, player.AP + apRestore);
            int actualRestore = player.AP - apBefore;

            combatState.AddLogEntry($"  You channel ambient Aether to restore your mental capacity!");
            combatState.AddLogEntry($"  Restored {actualRestore} AP! ({apBefore} → {player.AP}/{player.MaxAP})");
            combatState.AddLogEntry($"  Your turn ends (Focus Aether requires full concentration)");

            _log.Information("Focus Aether used: Character={Name}, Restored={Amount} AP, Rank={Rank}",
                player.Name, actualRestore, ability.CurrentRank);
        }
        // v0.19.8: Consecrate Ground - Create [Sanctified Ground] zone (Vard-Warden)
        else if (ability.Name == "Consecrate Ground")
        {
            combatState.AddLogEntry($"  You consecrate the ground with stable Aether!");
            combatState.AddLogEntry($"  [Sanctified Ground] zone created for 3 turns!");
            combatState.AddLogEntry($"  Allies in zone heal 1d6 HP/turn, Blighted enemies take 1d6 damage/turn");
            combatState.AddLogEntry($"  (Note: Full zone mechanics require RunicConstruct system integration)");

            // Placeholder: Set player status to track being in zone
            player.SanctifiedGroundTurnsRemaining = 3;
        }
        // v0.19.8: Reinforce Ward - Heal barrier or boost zone (Vard-Warden)
        else if (ability.Name == "Reinforce Ward")
        {
            combatState.AddLogEntry($"  You reinforce your magical construct!");
            combatState.AddLogEntry($"  (Note: Full mechanics require RunicConstruct tracking)");

            // Placeholder: Extend Sanctified Ground if active
            if (player.SanctifiedGroundTurnsRemaining > 0)
            {
                player.SanctifiedGroundTurnsRemaining += 2;
                combatState.AddLogEntry($"  [Sanctified Ground] duration extended by 2 turns!");
            }
        }
    }

    private void ProcessControlAbility(CombatState combatState, Ability ability, Enemy? target)
    {
        if (target == null || !target.IsAlive)
        {
            combatState.AddLogEntry("  No valid target!");
            return;
        }

        // Disrupt - skip enemy's next turn
        if (ability.SkipEnemyTurn)
        {
            target.IsStunned = true;
            target.StunTurnsRemaining = 1;
            combatState.AddLogEntry($"  {target.Name} is disrupted and will skip their next turn!");
            _log.Information("Status effect applied: Enemy={EnemyName}, Effect=Stunned, Duration={Duration}, Ability={AbilityName}",
                target.Name, 1, ability.Name);
        }

        // v0.7: Architect of the Silence - Apply [Seized] status (Architect ability)
        if (ability.Name == "Architect of the Silence")
        {
            // NOTE: This applies [Seized] to player (self-lock mechanic for Architect)
            // In actual gameplay, this would apply to enemies, but enemies don't take actions
            // So this is a placeholder for the mechanic
            combatState.AddLogEntry($"  You exploit the labyrinth's patterns to immobilize {target.Name}!");
            combatState.AddLogEntry($"  {target.Name} is temporarily locked in place by the architecture itself.");
            // Future: When enemy AI supports action skipping, add enemy.SeizedTurnsRemaining = 2;
        }

        // v0.7: Song of Silence - Apply [Silenced] status (Skald ability)
        if (ability.Name == "Song of Silence")
        {
            target.SilencedTurnsRemaining = 3;
            combatState.AddLogEntry($"  Your haunting melody strips {target.Name} of its voice!");
            combatState.AddLogEntry($"  {target.Name} is [Silenced] for 3 turns! (Cannot cast spells or perform)");
            _log.Information("Status effect applied: Enemy={EnemyName}, Effect=Silenced, Duration={Duration}, Ability={AbilityName}",
                target.Name, 3, ability.Name);
        }
    }

    /// <summary>
    /// Process player flee attempt
    /// </summary>
    public bool PlayerFlee(CombatState combatState)
    {
        if (!combatState.CanFlee)
        {
            combatState.AddLogEntry("You cannot flee from this fight!");
            combatState.AddLogEntry("");
            return false;
        }

        var player = combatState.Player;
        var playerRoll = _diceService.Roll(player.Attributes.Finesse);

        // Calculate average enemy finesse
        var avgEnemyFinesse = combatState.Enemies.Where(e => e.IsAlive).Average(e => e.Attributes.Finesse);
        var enemyRoll = _diceService.Roll((int)Math.Ceiling(avgEnemyFinesse));

        combatState.AddLogEntry($"{player.Name} attempts to flee!");
        combatState.AddLogEntry($"  Player rolled {player.Attributes.Finesse}d6: {FormatRolls(playerRoll)} = {playerRoll.Successes} successes");
        combatState.AddLogEntry($"  Enemies rolled {(int)Math.Ceiling(avgEnemyFinesse)}d6: {FormatRolls(enemyRoll)} = {enemyRoll.Successes} successes");

        if (playerRoll.Successes > enemyRoll.Successes)
        {
            combatState.AddLogEntry("  Escape successful!");
            combatState.IsActive = false;
            combatState.AddLogEntry("");
            return true;
        }
        else
        {
            combatState.AddLogEntry("  Escape failed! Enemies block your path.");
            combatState.AddLogEntry("");
            return false;
        }
    }

    /// <summary>
    /// Check if combat should end
    /// </summary>
    public bool IsCombatOver(CombatState combatState)
    {
        // Player defeated
        if (!combatState.Player.IsAlive)
        {
            combatState.IsActive = false;
            _log.Information("Combat ended: Player={PlayerName}, Result=Defeat", combatState.Player.Name);
            return true;
        }

        // All enemies defeated
        if (combatState.Enemies.All(e => !e.IsAlive))
        {
            combatState.IsActive = false;
            combatState.AddLogEntry("=== VICTORY ===");
            combatState.AddLogEntry("All enemies have been defeated!");

            _log.Information("Combat ended: Player={PlayerName}, Result=Victory, EnemiesDefeated={EnemyCount}",
                combatState.Player.Name, combatState.Enemies.Count);

            // Award Legend for defeated enemies (default trauma mod 1.0)
            // Combat context will be set by the caller
            AwardCombatLegend(combatState, 1.0f);

            return true;
        }

        return false;
    }

    /// <summary>
    /// Generate and drop loot from defeated enemies to the room
    /// </summary>
    public void GenerateLoot(CombatState combatState, Room room)
    {
        combatState.AddLogEntry("");
        combatState.AddLogEntry("=== LOOT ===");

        bool anyLoot = false;
        int totalCurrency = 0;

        foreach (var enemy in combatState.Enemies.Where(e => !e.IsAlive))
        {
            // Generate equipment loot
            var loot = _lootService.GenerateLoot(enemy, combatState.Player);
            if (loot != null)
            {
                room.ItemsOnGround.Add(loot);
                combatState.AddLogEntry($"[yellow]{enemy.Name}[/] dropped: [bold]{loot.GetDisplayName()}[/]");
                anyLoot = true;
            }

            // Generate currency drop (v0.9)
            int currencyDrop = _lootService.GenerateCurrencyDrop(enemy);
            if (currencyDrop > 0)
            {
                _currencyService.AddCurrency(combatState.Player, currencyDrop, $"Enemy loot: {enemy.Name}");
                totalCurrency += currencyDrop;
                anyLoot = true;
            }

            // Generate material drops (v0.9)
            var materialDrops = _lootService.GenerateMaterialDrops(enemy);
            foreach (var drop in materialDrops)
            {
                if (combatState.Player.CraftingComponents.ContainsKey(drop.Key))
                {
                    combatState.Player.CraftingComponents[drop.Key] += drop.Value;
                }
                else
                {
                    combatState.Player.CraftingComponents[drop.Key] = drop.Value;
                }

                var materialInfo = CraftingComponent.Create(drop.Key);
                combatState.AddLogEntry($"[cyan]+ {materialInfo.Name} x{drop.Value}[/]");
                anyLoot = true;
            }
        }

        // Display currency gained
        if (totalCurrency > 0)
        {
            combatState.AddLogEntry($"[green]⚙ Gained {_currencyService.GetCurrencyDisplay(totalCurrency)}[/]");
        }

        if (!anyLoot)
        {
            combatState.AddLogEntry("[dim]No loot dropped.[/]");
        }
        else if (combatState.Enemies.Any(e => !e.IsAlive && e.Type != EnemyType.RuinWarden)) // Don't show pickup hint for boss (no items on ground after boss fight)
        {
            combatState.AddLogEntry("[dim]Use 'pickup [item]' to collect equipment loot.[/]");
        }
    }

    /// <summary>
    /// Award Legend for all defeated enemies and check for milestone
    /// </summary>
    public bool AwardCombatLegend(CombatState combatState, float traumaMod = 1.0f)
    {
        int totalLegend = 0;
        foreach (var enemy in combatState.Enemies.Where(e => !e.IsAlive))
        {
            int legendAwarded = (int)(enemy.BaseLegendValue * 1.0f * traumaMod);
            _sagaService.AwardLegend(combatState.Player, enemy.BaseLegendValue, 1.0f, traumaMod);
            totalLegend += legendAwarded;
        }

        if (totalLegend > 0)
        {
            combatState.AddLogEntry($"Earned {totalLegend} Legend! (Total: {combatState.Player.CurrentLegend})");

            // Check if player can reach milestone
            if (_sagaService.CanReachMilestone(combatState.Player))
            {
                combatState.AddLogEntry($"*** MILESTONE REACHED! ***");
                return true; // Signal that milestone is available
            }
        }

        return false;
    }

    /// <summary>
    /// Advance to next turn in combat
    /// </summary>
    public void NextTurn(CombatState combatState)
    {
        // [v0.5] Forlorn Enemy Aura - Apply at start of each turn
        var traumaService = new TraumaEconomyService();
        var resolveService = new ResolveCheckService(_diceService);
        var forlornEnemies = combatState.Enemies.Where(e => e.IsAlive && e.IsForlorn).ToList();

        if (forlornEnemies.Any())
        {
            combatState.AddLogEntry("--- Forlorn Aura ---");

            foreach (var forlornEnemy in forlornEnemies)
            {
                var (success, successes, rollDetails) = resolveService.RollResolveCheck(combatState.Player, dc: 1);

                combatState.AddLogEntry(resolveService.GetForlornAuraFlavorText(forlornEnemy.Name, success));
                combatState.AddLogEntry($"  {rollDetails}");

                if (!success)
                {
                    traumaService.AddStress(combatState.Player, 5);
                    combatState.AddLogEntry($"  You gain 5 Psychic Stress (Current: {combatState.Player.PsychicStress}/100)");
                }
                else
                {
                    combatState.AddLogEntry($"  You resist the mental assault.");
                }
            }

            combatState.AddLogEntry("");
        }

        // [v0.5] Environmental Psychic Resonance - Apply at start of each turn
        if (combatState.CurrentRoom != null && combatState.CurrentRoom.PsychicResonance != PsychicResonanceLevel.None)
        {
            var baseStress = (int)combatState.CurrentRoom.PsychicResonance;
            var (stressToApply, successes, rollDetails) = resolveService.RollEnvironmentalStressResistance(combatState.Player, baseStress);

            combatState.AddLogEntry("--- Psychic Resonance ---");
            combatState.AddLogEntry(resolveService.GetResolveCheckFlavorText(stressToApply == 0, stressToApply));
            combatState.AddLogEntry($"  {rollDetails}");

            traumaService.AddStress(combatState.Player, stressToApply, allowResolveCheck: false, resolveSuccesses: 0);
            combatState.AddLogEntry($"  Psychic Stress: {combatState.Player.PsychicStress - stressToApply} → {combatState.Player.PsychicStress}/100");
            combatState.AddLogEntry("");

            // Check for threshold warnings after environmental stress
            var (shouldWarn, warningMessage) = traumaService.CheckForThresholdWarning(combatState.Player);
            if (shouldWarn)
            {
                combatState.AddLogEntry(warningMessage);
                combatState.AddLogEntry("");
            }
        }

        // Decrement player defense turns
        if (combatState.Player.DefenseTurnsRemaining > 0)
        {
            combatState.Player.DefenseTurnsRemaining--;
            if (combatState.Player.DefenseTurnsRemaining == 0)
            {
                combatState.Player.DefenseBonus = 0;
            }
        }

        // Decrement player Battle Rage turns
        if (combatState.Player.BattleRageTurnsRemaining > 0)
        {
            combatState.Player.BattleRageTurnsRemaining--;
            if (combatState.Player.BattleRageTurnsRemaining == 0)
            {
                combatState.AddLogEntry($"{combatState.Player.Name}'s battle rage ends.");
            }
        }

        // v0.19.8: Apply [Sanctified Ground] healing at start of player turn
        if (combatState.Player.SanctifiedGroundTurnsRemaining > 0)
        {
            int healAmount = _diceService.RollDamage(1); // 1d6 per turn
            int oldHP = combatState.Player.HP;
            combatState.Player.HP = Math.Min(combatState.Player.MaxHP, combatState.Player.HP + healAmount);
            int actualHeal = combatState.Player.HP - oldHP;

            if (actualHeal > 0)
            {
                combatState.AddLogEntry($"[Sanctified Ground] heals you for {actualHeal} HP! (HP: {combatState.Player.HP}/{combatState.Player.MaxHP})");
            }

            combatState.Player.SanctifiedGroundTurnsRemaining--;
            if (combatState.Player.SanctifiedGroundTurnsRemaining == 0)
            {
                combatState.AddLogEntry($"[Sanctified Ground] zone dissipates.");
            }
        }

        // v0.19.8: Decrement Mystic status effects
        if (combatState.Player.RuneOfShieldingTurnsRemaining > 0)
        {
            combatState.Player.RuneOfShieldingTurnsRemaining--;
            if (combatState.Player.RuneOfShieldingTurnsRemaining == 0)
            {
                combatState.AddLogEntry($"Rune of Shielding fades.");
            }
        }

        if (combatState.Player.GlyphOfSanctuaryStressImmunity > 0)
        {
            combatState.Player.GlyphOfSanctuaryStressImmunity--;
        }

        // Decrement enemy defense, stun, and bleeding turns
        foreach (var enemy in combatState.Enemies.Where(e => e.IsAlive))
        {
            // Apply bleeding damage at start of enemy turn
            if (enemy.BleedingTurnsRemaining > 0)
            {
                var bleedDamage = _diceService.RollDamage(1);
                enemy.HP -= bleedDamage;
                combatState.AddLogEntry($"{enemy.Name} takes {bleedDamage} bleeding damage! (HP: {Math.Max(0, enemy.HP)}/{enemy.MaxHP})");
                _log.Debug("Status effect damage: Enemy={EnemyName}, Effect=Bleeding, Damage={Damage}, RemainingHP={HP}, TurnsRemaining={Turns}",
                    enemy.Name, bleedDamage, Math.Max(0, enemy.HP), enemy.BleedingTurnsRemaining - 1);

                enemy.BleedingTurnsRemaining--;
                if (enemy.BleedingTurnsRemaining == 0)
                {
                    combatState.AddLogEntry($"{enemy.Name} is no longer bleeding.");
                    _log.Information("Status effect expired: Enemy={EnemyName}, Effect=Bleeding",
                        enemy.Name);
                }

                if (!enemy.IsAlive)
                {
                    combatState.AddLogEntry($"{enemy.Name} is destroyed!");
                }
            }

            // v0.19.8: Apply [Corroded] DoT damage at start of enemy turn
            if (enemy.CorrodedStacks > 0)
            {
                // Check if Rust-Witch has Accelerated Entropy passive (2d6 per stack)
                var player = combatState.Player;
                bool hasAcceleratedEntropy = player.Abilities.Any(a => a.Name == "Accelerated Entropy");
                int dicePerStack = hasAcceleratedEntropy ? 2 : 1;

                // Roll damage for each stack
                int totalCorrodedDamage = 0;
                for (int i = 0; i < enemy.CorrodedStacks; i++)
                {
                    totalCorrodedDamage += _diceService.RollDamage(dicePerStack);
                }

                enemy.HP -= totalCorrodedDamage;
                combatState.AddLogEntry($"{enemy.Name} takes {totalCorrodedDamage} corrosion damage from {enemy.CorrodedStacks} [Corroded] stacks! (HP: {Math.Max(0, enemy.HP)}/{enemy.MaxHP})");

                _log.Debug("Status effect damage: Enemy={EnemyName}, Effect=Corroded, Stacks={Stacks}, Damage={Damage}, RemainingHP={HP}",
                    enemy.Name, enemy.CorrodedStacks, totalCorrodedDamage, Math.Max(0, enemy.HP));

                // Countdown all stack durations
                for (int i = enemy.CorrodedStackDurations.Count - 1; i >= 0; i--)
                {
                    enemy.CorrodedStackDurations[i]--;
                    if (enemy.CorrodedStackDurations[i] <= 0)
                    {
                        enemy.CorrodedStackDurations.RemoveAt(i);
                        enemy.CorrodedStacks--;
                        combatState.AddLogEntry($"  1 [Corroded] stack expires on {enemy.Name} ({enemy.CorrodedStacks} remaining)");
                    }
                }

                if (enemy.CorrodedStacks == 0)
                {
                    combatState.AddLogEntry($"{enemy.Name} is no longer [Corroded].");
                    _log.Information("Status effect expired: Enemy={EnemyName}, Effect=Corroded", enemy.Name);
                }

                if (!enemy.IsAlive)
                {
                    combatState.AddLogEntry($"{enemy.Name} dissolves from corrosion!");

                    // v0.19.8: Cascade Reaction - Spread [Corroded] on death (Rust-Witch passive)
                    bool hasCascadeReaction = player.Abilities.Any(a => a.Name == "Cascade Reaction");
                    if (hasCascadeReaction)
                    {
                        combatState.AddLogEntry($"  [Cascade Reaction] Entropy spreads from {enemy.Name}!");
                        foreach (var adjacentEnemy in combatState.Enemies.Where(e => e.IsAlive && e != enemy))
                        {
                            adjacentEnemy.CorrodedStacks = Math.Min(5, adjacentEnemy.CorrodedStacks + 1);
                            adjacentEnemy.CorrodedStackDurations.Add(3);
                            combatState.AddLogEntry($"    {adjacentEnemy.Name} gains [Corroded] x{adjacentEnemy.CorrodedStacks}!");
                        }
                    }
                }
            }

            if (enemy.DefenseTurnsRemaining > 0)
            {
                enemy.DefenseTurnsRemaining--;
                if (enemy.DefenseTurnsRemaining == 0)
                {
                    enemy.DefenseBonus = 0;
                }
            }

            if (enemy.StunTurnsRemaining > 0)
            {
                enemy.StunTurnsRemaining--;
                if (enemy.StunTurnsRemaining == 0)
                {
                    enemy.IsStunned = false;
                    _log.Information("Status effect expired: Enemy={EnemyName}, Effect=Stunned",
                        enemy.Name);
                }
            }

            // v0.7: Tick down [Analyzed] status
            if (enemy.AnalyzedTurnsRemaining > 0)
            {
                enemy.AnalyzedTurnsRemaining--;
            }

            // v0.7: Tick down [Vulnerable] status
            if (enemy.VulnerableTurnsRemaining > 0)
            {
                enemy.VulnerableTurnsRemaining--;
            }

            // v0.7: Tick down [Silenced] status
            if (enemy.SilencedTurnsRemaining > 0)
            {
                enemy.SilencedTurnsRemaining--;
            }
        }

        // [v0.6] Apply environmental hazard damage at end of round
        if (combatState.CurrentRoom != null && _hazardService.RoomHasActiveHazard(combatState.CurrentRoom))
        {
            var (damage, stress, logMessage) = _hazardService.ProcessAutomaticHazard(combatState.CurrentRoom, combatState.Player);

            if (damage > 0 || stress > 0)
            {
                combatState.AddLogEntry($"[HAZARD] {logMessage}");
                combatState.AddLogEntry($"  {combatState.Player.Name} HP: {Math.Max(0, combatState.Player.HP)}/{combatState.Player.MaxHP}");

                if (stress > 0)
                {
                    combatState.AddLogEntry($"  {combatState.Player.Name} Psychic Stress: {combatState.Player.PsychicStress}");
                }

                if (!combatState.Player.IsAlive)
                {
                    combatState.AddLogEntry($"{combatState.Player.Name} has fallen to the environmental hazard!");
                    combatState.IsActive = false;
                }

                combatState.AddLogEntry("");
            }
        }

        // v0.7: Tick down performance duration and handle interruptions
        if (combatState.Player.IsPerforming)
        {
            // Check for interruption ([Silenced])
            var interruptMessage = _performanceService.HandleInterruption(combatState.Player);
            if (!string.IsNullOrEmpty(interruptMessage))
            {
                combatState.AddLogEntry($"--- Performance Interrupted ---");
                combatState.AddLogEntry($"  {interruptMessage}");
                combatState.AddLogEntry("");
            }
            else
            {
                // Tick down performance duration
                var tickMessage = _performanceService.TickPerformance(combatState.Player);
                if (!string.IsNullOrEmpty(tickMessage))
                {
                    combatState.AddLogEntry($"--- Performance Status ---");
                    combatState.AddLogEntry($"  {tickMessage}");
                    combatState.AddLogEntry("");
                }
            }
        }

        // v0.7: Tick down v0.7 status effects
        if (combatState.Player.VulnerableTurnsRemaining > 0)
        {
            combatState.Player.VulnerableTurnsRemaining--;
        }

        if (combatState.Player.AnalyzedTurnsRemaining > 0)
        {
            combatState.Player.AnalyzedTurnsRemaining--;
        }

        if (combatState.Player.SeizedTurnsRemaining > 0)
        {
            combatState.Player.SeizedTurnsRemaining--;
        }

        if (combatState.Player.InspiredTurnsRemaining > 0)
        {
            combatState.Player.InspiredTurnsRemaining--;
        }

        if (combatState.Player.SilencedTurnsRemaining > 0)
        {
            combatState.Player.SilencedTurnsRemaining--;
        }

        combatState.NextTurn();
    }

    /// <summary>
    /// Get the next alive enemy for targeting
    /// </summary>
    public Enemy? GetNextAliveEnemy(CombatState combatState)
    {
        return combatState.Enemies.FirstOrDefault(e => e.IsAlive);
    }

    /// <summary>
    /// Get enemy by index (1-based for user display)
    /// </summary>
    public Enemy? GetEnemyByIndex(CombatState combatState, int index)
    {
        var aliveEnemies = combatState.Enemies.Where(e => e.IsAlive).ToList();
        if (index < 1 || index > aliveEnemies.Count)
        {
            return null;
        }
        return aliveEnemies[index - 1];
    }

    private string FormatRolls(DiceResult result)
    {
        return string.Join(", ", result.Rolls);
    }
}
