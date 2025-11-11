using RuneAndRust.Core;

namespace RuneAndRust.Engine;

public class CombatEngine
{
    private readonly DiceService _diceService;
    private readonly SagaService _sagaService;
    private readonly LootService _lootService;
    private readonly EquipmentService _equipmentService;

    public CombatEngine(DiceService diceService, SagaService sagaService, LootService lootService, EquipmentService equipmentService)
    {
        _diceService = diceService;
        _sagaService = sagaService;
        _lootService = lootService;
        _equipmentService = equipmentService;
    }

    /// <summary>
    /// Initialize combat with the player and enemies, roll initiative
    /// </summary>
    public CombatState InitializeCombat(PlayerCharacter player, List<Enemy> enemies, Room? currentRoom = null, bool canFlee = true)
    {
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

        // [v0.4] Environmental hazard warning
        if (currentRoom != null && currentRoom.HasEnvironmentalHazard && currentRoom.IsHazardActive)
        {
            combatState.AddLogEntry($"[WARNING] {currentRoom.HazardDescription}");
            combatState.AddLogEntry($"  You will take {currentRoom.HazardDamagePerTurn} damage per turn!");
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
            // Roll weapon damage dice
            damage = _diceService.RollDamage(weaponDice) + weaponDamageBonus;
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
        }
        else
        {
            combatState.AddLogEntry($"  The attack is deflected!");
        }

        // Check if target is defeated
        if (!target.IsAlive)
        {
            combatState.AddLogEntry($"  {target.Name} is destroyed!");
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
    /// Process player ability use
    /// </summary>
    public bool PlayerUseAbility(CombatState combatState, Ability ability, Enemy? target = null)
    {
        var player = combatState.Player;

        // Check stamina cost
        if (player.Stamina < ability.StaminaCost)
        {
            combatState.AddLogEntry($"Not enough stamina! ({player.Stamina}/{ability.StaminaCost} required)");
            return false;
        }

        // Pay stamina cost
        player.Stamina -= ability.StaminaCost;

        combatState.AddLogEntry($"{player.Name} uses {ability.Name}!");
        combatState.AddLogEntry($"  Cost: {ability.StaminaCost} Stamina (Remaining: {player.Stamina}/{player.MaxStamina})");

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

        // Check for Power Strike (double weapon damage)
        if (ability.Name == "Power Strike")
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

        target.HP -= damage;
        combatState.AddLogEntry($"  {target.Name} takes {damage} damage! (HP: {Math.Max(0, target.HP)}/{target.MaxHP})");

        if (!target.IsAlive)
        {
            combatState.AddLogEntry($"  {target.Name} is destroyed!");
        }
    }

    private void ProcessDefenseAbility(CombatState combatState, Ability ability)
    {
        var player = combatState.Player;

        // Check for Quick Dodge (negates next attack)
        if (ability.NegateNextAttack)
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
            return true;
        }

        // All enemies defeated
        if (combatState.Enemies.All(e => !e.IsAlive))
        {
            combatState.IsActive = false;
            combatState.AddLogEntry("=== VICTORY ===");
            combatState.AddLogEntry("All enemies have been defeated!");

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
        foreach (var enemy in combatState.Enemies.Where(e => !e.IsAlive))
        {
            var loot = _lootService.GenerateLoot(enemy, combatState.Player);
            if (loot != null)
            {
                room.ItemsOnGround.Add(loot);
                combatState.AddLogEntry($"[yellow]{enemy.Name}[/] dropped: [bold]{loot.GetDisplayName()}[/]");
                anyLoot = true;
            }
        }

        if (!anyLoot)
        {
            combatState.AddLogEntry("[dim]No loot dropped.[/]");
        }
        else
        {
            combatState.AddLogEntry("[dim]Use 'pickup [item]' to collect loot.[/]");
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

        // Decrement enemy defense, stun, and bleeding turns
        foreach (var enemy in combatState.Enemies.Where(e => e.IsAlive))
        {
            // Apply bleeding damage at start of enemy turn
            if (enemy.BleedingTurnsRemaining > 0)
            {
                var bleedDamage = _diceService.RollDamage(1);
                enemy.HP -= bleedDamage;
                combatState.AddLogEntry($"{enemy.Name} takes {bleedDamage} bleeding damage! (HP: {Math.Max(0, enemy.HP)}/{enemy.MaxHP})");

                enemy.BleedingTurnsRemaining--;
                if (enemy.BleedingTurnsRemaining == 0)
                {
                    combatState.AddLogEntry($"{enemy.Name} is no longer bleeding.");
                }

                if (!enemy.IsAlive)
                {
                    combatState.AddLogEntry($"{enemy.Name} is destroyed!");
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
                }
            }
        }

        // [v0.4] Apply environmental hazard damage at end of round
        if (combatState.CurrentRoom != null &&
            combatState.CurrentRoom.HasEnvironmentalHazard &&
            combatState.CurrentRoom.IsHazardActive)
        {
            var hazardDamage = _diceService.RollDamage(1); // Roll 1d6 for hazard damage
            if (hazardDamage >= combatState.CurrentRoom.HazardDamagePerTurn)
            {
                hazardDamage = combatState.CurrentRoom.HazardDamagePerTurn;
            }

            combatState.Player.HP -= hazardDamage;
            combatState.AddLogEntry($"[HAZARD] Environmental hazard deals {hazardDamage} damage to {combatState.Player.Name}!");
            combatState.AddLogEntry($"  {combatState.Player.Name} HP: {Math.Max(0, combatState.Player.HP)}/{combatState.Player.MaxHP}");

            if (!combatState.Player.IsAlive)
            {
                combatState.AddLogEntry($"{combatState.Player.Name} has fallen!");
                combatState.IsActive = false;
            }

            combatState.AddLogEntry("");
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
