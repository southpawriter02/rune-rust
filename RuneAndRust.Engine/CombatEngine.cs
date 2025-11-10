using RuneAndRust.Core;

namespace RuneAndRust.Engine;

public class CombatEngine
{
    private readonly DiceService _diceService;

    public CombatEngine(DiceService diceService)
    {
        _diceService = diceService;
    }

    /// <summary>
    /// Initialize combat with the player and enemies, roll initiative
    /// </summary>
    public CombatState InitializeCombat(PlayerCharacter player, List<Enemy> enemies, bool canFlee = true)
    {
        var combatState = new CombatState
        {
            Player = player,
            Enemies = new List<Enemy>(enemies),
            IsActive = true,
            CanFlee = canFlee
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
        var weaponAttribute = player.GetAttributeValue(player.WeaponAttribute);
        var bonusDice = combatState.PlayerNextAttackBonusDice;

        // Reset bonus dice after use
        combatState.PlayerNextAttackBonusDice = 0;

        var totalDice = weaponAttribute + bonusDice;
        var attackRoll = _diceService.Roll(totalDice);

        combatState.AddLogEntry($"{player.Name} attacks {target.Name}!");
        combatState.AddLogEntry($"  Rolled {totalDice}d6: {FormatRolls(attackRoll)} = {attackRoll.Successes} successes");

        // Opponent defends
        var defendRoll = _diceService.Roll(target.Attributes.Sturdiness);
        combatState.AddLogEntry($"{target.Name} defends!");
        combatState.AddLogEntry($"  Rolled {target.Attributes.Sturdiness}d6: {FormatRolls(defendRoll)} = {defendRoll.Successes} successes");

        // Calculate damage
        var netSuccesses = Math.Max(0, attackRoll.Successes - defendRoll.Successes);
        var damage = netSuccesses;

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

        // Check if ability has special damage dice (like Aetheric Bolt)
        if (ability.DamageDice > 0)
        {
            damage = _diceService.RollDamage(ability.DamageDice);
            combatState.AddLogEntry($"  Rolled {ability.DamageDice}d6 for damage: {damage}");
        }
        // Check for Power Strike (double weapon damage)
        else if (ability.Name == "Power Strike")
        {
            var weaponDamage = _diceService.RollDamage(combatState.Player.BaseDamage);
            damage = weaponDamage * 2;
            combatState.AddLogEntry($"  Weapon damage doubled: {damage}");
        }
        else
        {
            damage = successes;
        }

        // Check if ability ignores armor
        if (!ability.IgnoresArmor && target.DefenseTurnsRemaining > 0)
        {
            var reducedDamage = (int)(damage * (1 - target.DefenseBonus / 100.0));
            combatState.AddLogEntry($"  {target.Name}'s defense reduces damage from {damage} to {reducedDamage}");
            damage = reducedDamage;
        }
        else if (ability.IgnoresArmor)
        {
            combatState.AddLogEntry($"  Ignores armor!");
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
        // Exploit Weakness - grants bonus dice to next attack
        combatState.PlayerNextAttackBonusDice = ability.NextAttackBonusDice;
        combatState.AddLogEntry($"  Next attack gains +{ability.NextAttackBonusDice} bonus dice!");
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
            return true;
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

        // Decrement enemy defense and stun turns
        foreach (var enemy in combatState.Enemies.Where(e => e.IsAlive))
        {
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
