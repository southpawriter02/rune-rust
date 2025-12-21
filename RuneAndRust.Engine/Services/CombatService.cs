using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Core.Models.Combat;
using RuneAndRust.Core.ViewModels;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Manages the lifecycle of combat encounters including starting, advancing turns, and ending combat.
/// </summary>
public class CombatService : ICombatService
{
    private readonly GameState _gameState;
    private readonly IInitiativeService _initiative;
    private readonly IAttackResolutionService _attackResolution;
    private readonly ILootService _lootService;
    private readonly IStatusEffectService _statusEffects;
    private readonly IEnemyAIService _aiService;
    private readonly ICreatureTraitService _traitService;
    private readonly IResourceService _resourceService;
    private readonly IAbilityService _abilityService;
    private readonly IActiveAbilityRepository _abilityRepository;
    private readonly ITraumaService _traumaService;
    private readonly IHazardService _hazardService;
    private readonly IConditionService _conditionService;
    private readonly IRoomRepository _roomRepository;
    private readonly ILogger<CombatService> _logger;

    /// <summary>
    /// Rolling buffer of combat events for player-visible UI display.
    /// </summary>
    private readonly Queue<string> _combatLog = new();

    /// <summary>
    /// Maximum number of entries to retain in the combat log.
    /// </summary>
    private const int MaxLogHistory = 10;

    /// <summary>
    /// Delay in milliseconds before enemy actions for UX pacing.
    /// </summary>
    private const int EnemyActionDelayMs = 750;

    /// <summary>
    /// Initializes a new instance of the <see cref="CombatService"/> class.
    /// </summary>
    /// <param name="gameState">The shared game state singleton.</param>
    /// <param name="initiative">The initiative service for turn order calculations.</param>
    /// <param name="attackResolution">The attack resolution service for combat mechanics.</param>
    /// <param name="lootService">The loot service for generating combat rewards.</param>
    /// <param name="statusEffects">The status effect service for managing combat conditions.</param>
    /// <param name="aiService">The enemy AI service for turn decisions.</param>
    /// <param name="traitService">The creature trait service for Elite enemy effects.</param>
    /// <param name="resourceService">The resource service for stamina/aether management.</param>
    /// <param name="abilityService">The ability service for cooldown and ability management.</param>
    /// <param name="abilityRepository">The ability repository for loading archetype abilities.</param>
    /// <param name="traumaService">The trauma service for stress and trauma mechanics (v0.3.0).</param>
    /// <param name="hazardService">The hazard service for environmental triggers (v0.3.3a).</param>
    /// <param name="conditionService">The condition service for ambient effects (v0.3.3b).</param>
    /// <param name="roomRepository">The room repository for current room lookup.</param>
    /// <param name="logger">The logger for traceability.</param>
    public CombatService(
        GameState gameState,
        IInitiativeService initiative,
        IAttackResolutionService attackResolution,
        ILootService lootService,
        IStatusEffectService statusEffects,
        IEnemyAIService aiService,
        ICreatureTraitService traitService,
        IResourceService resourceService,
        IAbilityService abilityService,
        IActiveAbilityRepository abilityRepository,
        ITraumaService traumaService,
        IHazardService hazardService,
        IConditionService conditionService,
        IRoomRepository roomRepository,
        ILogger<CombatService> logger)
    {
        _gameState = gameState;
        _initiative = initiative;
        _attackResolution = attackResolution;
        _lootService = lootService;
        _statusEffects = statusEffects;
        _aiService = aiService;
        _traitService = traitService;
        _resourceService = resourceService;
        _abilityService = abilityService;
        _abilityRepository = abilityRepository;
        _traumaService = traumaService;
        _hazardService = hazardService;
        _conditionService = conditionService;
        _roomRepository = roomRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public void StartCombat(List<Enemy> enemies)
    {
        _logger.LogInformation("Initializing Combat. Enemies: {Count}", enemies.Count);

        if (_gameState.CurrentCharacter == null)
        {
            _logger.LogWarning("Cannot start combat without active character");
            return;
        }

        var state = new CombatState();

        // Load abilities for the player's archetype (v0.2.3c)
        var character = _gameState.CurrentCharacter;
        var abilities = _abilityRepository.GetByArchetypeAsync(character.Archetype, maxTier: 1)
            .GetAwaiter().GetResult().ToList();

        _logger.LogInformation(
            "Loaded {Count} abilities for archetype {Archetype}",
            abilities.Count, character.Archetype);

        // Add player with abilities
        var player = Combatant.FromCharacter(character, abilities);
        _initiative.RollInitiative(player);
        state.TurnOrder.Add(player);
        _logger.LogDebug("Added player {Name} to combat with {AbilityCount} abilities",
            player.Name, player.Abilities.Count);

        // Add enemies
        foreach (var enemy in enemies)
        {
            var combatant = Combatant.FromEnemy(enemy);
            _initiative.RollInitiative(combatant);
            state.TurnOrder.Add(combatant);
            _logger.LogDebug("Added enemy {Name} to combat", combatant.Name);
        }

        // Apply ambient condition modifiers to all combatants (v0.3.3b)
        if (_gameState.CurrentRoomId.HasValue)
        {
            var condition = _conditionService.GetRoomConditionAsync(_gameState.CurrentRoomId.Value)
                .GetAwaiter().GetResult();

            if (condition != null)
            {
                foreach (var combatant in state.TurnOrder)
                {
                    _conditionService.ApplyPassiveModifiers(combatant, condition.Type);
                }

                LogCombatEvent($"[orange1][AMBIENT][/] {condition.Name}: {condition.Description}");
                _logger.LogInformation(
                    "[Condition] Combat starting in [{ConditionName}] zone. {Count} combatants affected.",
                    condition.Name, state.TurnOrder.Count);
            }
        }

        // Sort by initiative
        state.TurnOrder = _initiative.SortTurnOrder(state.TurnOrder);

        // Update global state
        _gameState.CombatState = state;
        _gameState.Phase = GamePhase.Combat;

        // Clear previous combat log and add initial entry
        _combatLog.Clear();
        LogCombatEvent("[bold]Combat begins![/]");
        LogCombatEvent($"[cyan]{player.Name}[/] faces {string.Join(", ", enemies.Select(e => $"[red]{e.Name}[/]"))}.");

        _logger.LogInformation("Combat Started. Round {Round}. Active: {Name}",
            state.RoundNumber, state.ActiveCombatant?.Name ?? "None");
    }

    /// <inheritdoc/>
    public void NextTurn()
    {
        if (_gameState.CombatState == null)
        {
            _logger.LogWarning("NextTurn called but no combat is active");
            return;
        }

        var state = _gameState.CombatState;
        state.TurnIndex++;

        if (state.TurnIndex >= state.TurnOrder.Count)
        {
            state.TurnIndex = 0;
            state.RoundNumber++;
            _logger.LogInformation("Round {Round} begins", state.RoundNumber);
        }

        var active = state.ActiveCombatant;
        if (active != null)
        {
            // Process turn start: apply DoT damage from status effects
            var dotDamage = _statusEffects.ProcessTurnStart(active);
            if (dotDamage > 0)
            {
                active.CurrentHp -= dotDamage;
                LogCombatEvent($"[yellow]{active.Name}[/] takes [red]{dotDamage}[/] damage from status effects!");

                // Check for death from DoT
                if (active.CurrentHp <= 0)
                {
                    LogCombatEvent($"[red]{active.Name}[/] has fallen to their wounds!");
                    _logger.LogWarning("{Name} killed by DoT damage", active.Name);
                    RemoveDefeatedCombatant(active);

                    // Check victory and potentially advance to next turn
                    if (CheckVictoryCondition())
                    {
                        _logger.LogInformation("Combat Victory! All enemies defeated by DoT.");
                        return;
                    }

                    // Recursively advance to next combatant
                    NextTurn();
                    return;
                }
            }

            // Process trait regeneration at turn start (v0.2.2c)
            var regenHeal = _traitService.ProcessTraitTurnStart(active);
            if (regenHeal > 0)
            {
                LogCombatEvent($"[green]{active.Name}[/] regenerates [green]{regenHeal}[/] HP!");
            }

            // Process stamina regeneration at turn start (v0.2.3a)
            var staminaRegen = _resourceService.RegenerateStamina(active);
            if (staminaRegen > 0)
            {
                LogCombatEvent($"[cyan]{active.Name}[/] recovers [cyan]{staminaRegen}[/] stamina.");
            }

            // Process ability cooldowns at turn start (v0.2.3b)
            _abilityService.ProcessCooldowns(active);

            // Process trauma triggers at turn start (v0.3.0c)
            // Only for player combatants with active traumas
            if (active.IsPlayer && active.CharacterSource != null)
            {
                ProcessTraumaTriggers(active);
            }

            // Process ambient condition tick at turn start (v0.3.3b)
            if (active.ActiveCondition != null && _gameState.CurrentRoomId.HasValue)
            {
                ProcessConditionTick(active);
            }

            // Reset defending stance at start of turn
            if (active.IsDefending)
            {
                active.IsDefending = false;
                _logger.LogDebug("{Name} is no longer defending", active.Name);
            }

            // Check if combatant is stunned and cannot act
            if (!_statusEffects.CanAct(active))
            {
                LogCombatEvent($"[yellow]{active.Name}[/] is [purple]stunned[/] and loses their turn!");

                // Process turn end to decrement durations before skipping
                _statusEffects.ProcessTurnEnd(active);

                // Recursively advance to next combatant
                NextTurn();
                return;
            }
        }

        _logger.LogInformation("Turn: {Name} ({Type})",
            active?.Name ?? "None",
            active?.IsPlayer == true ? "Player" : "Enemy");
    }

    /// <inheritdoc/>
    public CombatResult? EndCombat()
    {
        if (_gameState.CombatState == null)
        {
            _logger.LogWarning("EndCombat called but no combat is active");
            return null;
        }

        var victory = CheckVictoryCondition();
        var loot = new List<Item>();
        var xp = 0;

        if (victory)
        {
            // Calculate XP based on Wits attribute for future scaling
            var character = _gameState.CurrentCharacter;
            var witsBonus = character?.GetEffectiveAttribute(CharacterAttribute.Wits) ?? 0;

            // Placeholder XP calculation (will be expanded with enemy XP values)
            xp = 50;

            // Generate loot using the loot service
            var lootContext = new LootGenerationContext(
                BiomeType: BiomeType.Industrial, // Default biome for combat
                DangerLevel: DangerLevel.Unstable, // Combat loot defaults to Unstable
                LootTier: null,
                WitsBonus: witsBonus
            );

            var lootResult = _lootService.GenerateLoot(lootContext);
            loot = lootResult.Items.ToList();

            _logger.LogInformation("Combat Victory! XP: {Xp}, Loot: {Count} items", xp, loot.Count);
        }
        else
        {
            _logger.LogInformation("Combat Ended (non-victory)");
        }

        _gameState.Phase = GamePhase.Exploration;
        _gameState.CombatState = null;

        return new CombatResult(
            Victory: victory,
            XpEarned: xp,
            LootFound: loot,
            Summary: victory ? "Victory! All enemies defeated." : "Combat ended."
        );
    }

    /// <inheritdoc/>
    public string ExecutePlayerAttack(string targetName, AttackType attackType)
    {
        var state = _gameState.CombatState;

        // Validate combat state
        if (state == null)
        {
            _logger.LogWarning("ExecutePlayerAttack called but no combat is active");
            return "You are not in combat.";
        }

        // Validate it's the player's turn
        if (!state.IsPlayerTurn)
        {
            _logger.LogDebug("Attack attempted but it is not the player's turn");
            return "It is not your turn.";
        }

        var attacker = state.ActiveCombatant!;

        // Find target by name (partial match, case-insensitive)
        var target = state.TurnOrder.FirstOrDefault(c =>
            !c.IsPlayer && c.Name.Contains(targetName, StringComparison.OrdinalIgnoreCase));

        if (target == null)
        {
            _logger.LogDebug("Target '{TargetName}' not found in combat", targetName);
            return $"Target '{targetName}' not found.";
        }

        // Check stamina affordability
        var staminaCost = _attackResolution.GetStaminaCost(attackType);
        if (!_attackResolution.CanAffordAttack(attacker, attackType))
        {
            _logger.LogDebug(
                "{Attacker} cannot afford {AttackType} attack. Stamina: {Current}/{Cost}",
                attacker.Name, attackType, attacker.CurrentStamina, staminaCost);
            return $"Not enough stamina for {attackType.ToString().ToLower()} attack. Need {staminaCost}, have {attacker.CurrentStamina}.";
        }

        // Deduct stamina cost
        attacker.CurrentStamina -= staminaCost;
        _logger.LogDebug(
            "{Attacker} spent {Cost} stamina. Remaining: {Current}/{Max}",
            attacker.Name, staminaCost, attacker.CurrentStamina, attacker.MaxStamina);

        // Resolve the attack
        var result = _attackResolution.ResolveMeleeAttack(attacker, target, attackType);

        _logger.LogInformation(
            "{Attacker} attacks {Target} ({AttackType}): {Outcome}",
            attacker.Name, target.Name, attackType, result.Outcome);

        // Apply damage to target
        if (result.IsHit)
        {
            target.CurrentHp -= result.FinalDamage;
            _logger.LogDebug(
                "{Target} took {Damage} damage. HP: {Current}/{Max}",
                target.Name, result.FinalDamage, target.CurrentHp, target.MaxHp);

            // Process thorns damage (v0.2.2c)
            var thornsDamage = _traitService.ProcessTraitOnDamageReceived(target, attacker, result.FinalDamage);
            if (thornsDamage > 0)
            {
                attacker.CurrentHp -= thornsDamage;
                LogCombatEvent($"[orange1]{target.Name}[/]'s thorns deal [red]{thornsDamage}[/] damage to you!");

                // Sync player HP
                if (attacker.CharacterSource != null)
                {
                    attacker.CharacterSource.CurrentHP = attacker.CurrentHp;
                    _logger.LogDebug("Synced thorns damage to player source. HP: {Current}", attacker.CharacterSource.CurrentHP);
                }
            }

            // Check for death
            if (target.CurrentHp <= 0)
            {
                _logger.LogWarning("{Target} was slain! HP: 0/{Max}", target.Name, target.MaxHp);
                RemoveDefeatedCombatant(target);

                // Check victory condition
                if (CheckVictoryCondition())
                {
                    _logger.LogInformation("Combat Victory! All enemies defeated.");
                    _statusEffects.ProcessTurnEnd(attacker);
                    var victoryMsg = BuildVictoryMessage(result, target);
                    LogCombatEvent(BuildLogMessage(result, target, isVictory: true));
                    return victoryMsg;
                }

                _statusEffects.ProcessTurnEnd(attacker);
                var deathMsg = BuildDeathMessage(result, target);
                LogCombatEvent(BuildLogMessage(result, target, isDeath: true));
                return deathMsg;
            }

            _statusEffects.ProcessTurnEnd(attacker);
            var hitMsg = BuildHitMessage(result, target);
            LogCombatEvent(BuildLogMessage(result, target));
            return hitMsg;
        }

        // Process turn end for the attacker (decrement status effect durations)
        _statusEffects.ProcessTurnEnd(attacker);

        var missMsg = BuildMissMessage(result, target);
        LogCombatEvent(BuildMissLogMessage(result, target));
        return missMsg;
    }

    /// <inheritdoc/>
    public void RemoveDefeatedCombatant(Combatant combatant)
    {
        var state = _gameState.CombatState;
        if (state == null) return;

        // Process on-death traits BEFORE removal (v0.2.2c Explosive)
        var explosionDamage = _traitService.ProcessTraitOnDeath(combatant, state.TurnOrder);
        foreach (var (target, damage) in explosionDamage)
        {
            target.CurrentHp -= damage;
            LogCombatEvent($"[orange1]{combatant.Name}[/] EXPLODES! [cyan]{target.Name}[/] takes [red]{damage}[/] damage!");

            // Sync player HP if affected
            if (target.CharacterSource != null)
            {
                target.CharacterSource.CurrentHP = target.CurrentHp;
                _logger.LogDebug("Synced explosion damage to player source. HP: {Current}", target.CharacterSource.CurrentHP);
            }
        }

        var index = state.TurnOrder.IndexOf(combatant);
        if (index < 0) return;

        state.TurnOrder.Remove(combatant);
        _logger.LogDebug("Removed {Name} from turn order", combatant.Name);

        // Adjust turn index if necessary
        if (index < state.TurnIndex)
        {
            state.TurnIndex--;
        }
        else if (index == state.TurnIndex && state.TurnIndex >= state.TurnOrder.Count)
        {
            state.TurnIndex = 0;
        }
    }

    /// <inheritdoc/>
    public bool CheckVictoryCondition()
    {
        var state = _gameState.CombatState;
        if (state == null) return false;

        // Victory if no enemies remain
        return !state.TurnOrder.Any(c => !c.IsPlayer);
    }

    /// <inheritdoc/>
    public string GetCombatStatus()
    {
        var state = _gameState.CombatState;
        if (state == null)
        {
            return "No active combat.";
        }

        var lines = new List<string>
        {
            $"=== COMBAT STATUS (Round {state.RoundNumber}) ==="
        };

        foreach (var combatant in state.TurnOrder)
        {
            var turnMarker = combatant == state.ActiveCombatant ? " <<" : "";
            var typeLabel = combatant.IsPlayer ? "[PLAYER]" : "[ENEMY]";

            lines.Add($"  {typeLabel} {combatant.Name}: HP {combatant.CurrentHp}/{combatant.MaxHp}, Stamina {combatant.CurrentStamina}/{combatant.MaxStamina}{turnMarker}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    #region Message Builders

    private static string BuildHitMessage(AttackResult result, Combatant target)
    {
        var outcomeText = result.Outcome switch
        {
            AttackOutcome.Glancing => "Glancing blow!",
            AttackOutcome.Solid => "Solid hit!",
            AttackOutcome.Critical => "CRITICAL HIT!",
            _ => "Hit!"
        };

        return $"{outcomeText} You dealt {result.FinalDamage} damage to {target.Name}. (HP: {target.CurrentHp}/{target.MaxHp})";
    }

    private static string BuildMissMessage(AttackResult result, Combatant target)
    {
        return result.Outcome == AttackOutcome.Fumble
            ? $"FUMBLE! Your attack goes wildly astray, missing {target.Name} entirely."
            : $"Miss! Your attack was evaded by {target.Name}.";
    }

    private static string BuildDeathMessage(AttackResult result, Combatant target)
    {
        var outcomeText = result.Outcome switch
        {
            AttackOutcome.Critical => "CRITICAL STRIKE!",
            _ => "FATAL BLOW!"
        };

        return $"{outcomeText} You struck {target.Name} for {result.FinalDamage} damage. They fall dead!";
    }

    private static string BuildVictoryMessage(AttackResult result, Combatant target)
    {
        return $"VICTORY! You struck down {target.Name} with {result.FinalDamage} damage. All enemies defeated!";
    }

    /// <summary>
    /// Builds a Spectre-markup formatted log message for hits.
    /// </summary>
    private static string BuildLogMessage(AttackResult result, Combatant target, bool isDeath = false, bool isVictory = false)
    {
        var outcomeMarkup = result.Outcome switch
        {
            AttackOutcome.Glancing => "[grey]Glancing blow![/]",
            AttackOutcome.Solid => "[white]Solid hit![/]",
            AttackOutcome.Critical => "[bold yellow]CRITICAL HIT![/]",
            _ => "[white]Hit![/]"
        };

        var damageText = $"[cyan]{result.FinalDamage}[/] damage";

        if (isVictory)
        {
            return $"{outcomeMarkup} You struck [red]{target.Name}[/] for {damageText}. [bold green]VICTORY![/]";
        }

        if (isDeath)
        {
            return $"{outcomeMarkup} [red]{target.Name}[/] falls! ({damageText})";
        }

        return $"{outcomeMarkup} You hit [red]{target.Name}[/] for {damageText}.";
    }

    /// <summary>
    /// Builds a Spectre-markup formatted log message for misses.
    /// </summary>
    private static string BuildMissLogMessage(AttackResult result, Combatant target)
    {
        return result.Outcome == AttackOutcome.Fumble
            ? $"[bold red]FUMBLE![/] Your attack goes wildly astray."
            : $"[grey]Miss![/] [red]{target.Name}[/] evades your attack.";
    }

    #endregion

    #region Combat Log and ViewModel

    /// <inheritdoc/>
    public void LogCombatEvent(string message)
    {
        if (_combatLog.Count >= MaxLogHistory)
        {
            _combatLog.Dequeue();
        }

        _combatLog.Enqueue(message);
        _logger.LogTrace("Combat log: {Message}", message);
    }

    /// <inheritdoc/>
    public CombatViewModel? GetViewModel()
    {
        var state = _gameState.CombatState;
        if (state == null)
        {
            return null;
        }

        var player = state.TurnOrder.FirstOrDefault(c => c.IsPlayer);
        if (player == null)
        {
            return null;
        }

        // Map abilities to AbilityView with hotkeys (v0.2.3c)
        var abilityViews = player.Abilities
            .Select((ability, index) => new AbilityView(
                Hotkey: index + 1,
                Name: ability.Name,
                CostDisplay: FormatAbilityCost(ability),
                CooldownRemaining: _abilityService.GetCooldownRemaining(player, ability.Id),
                IsUsable: _abilityService.CanUse(player, ability)
            ))
            .ToList();

        _logger.LogTrace("Generated CombatViewModel for Round {Round}", state.RoundNumber);

        return new CombatViewModel(
            state.RoundNumber,
            state.ActiveCombatant?.Name ?? "Unknown",
            state.TurnOrder.Select(c => MapToView(c, state.ActiveCombatant)).ToList(),
            _combatLog.ToList(),
            new PlayerStatsView(
                player.CurrentHp, player.MaxHp,
                player.CurrentStamina, player.MaxStamina,
                player.CurrentStress, player.MaxStress,
                player.CurrentCorruption, player.MaxCorruption),
            abilityViews
        );
    }

    /// <summary>
    /// Formats the cost of an ability for display.
    /// </summary>
    private static string FormatAbilityCost(ActiveAbility ability)
    {
        var costs = new List<string>();

        if (ability.StaminaCost > 0)
        {
            costs.Add($"{ability.StaminaCost} STA");
        }

        if (ability.AetherCost > 0)
        {
            costs.Add($"{ability.AetherCost} AP");
        }

        return costs.Count > 0 ? string.Join(", ", costs) : "Free";
    }

    /// <summary>
    /// Maps a Combatant to a display-ready CombatantView.
    /// Hides enemy HP as narrative text per design pillar.
    /// </summary>
    /// <param name="combatant">The combatant to map.</param>
    /// <param name="activeCombatant">The currently active combatant for turn marker.</param>
    /// <returns>A CombatantView for UI rendering.</returns>
    private static CombatantView MapToView(Combatant combatant, Combatant? activeCombatant)
    {
        // Design Pillar: Hide Enemy Numbers - use narrative health
        string healthStatus = combatant.IsPlayer
            ? $"{combatant.CurrentHp}/{combatant.MaxHp}"
            : GetNarrativeHealth(combatant);

        // Generate status effect display with icons
        var effectIcons = string.Join(" ", combatant.StatusEffects.Select(e => e.Type switch
        {
            StatusEffectType.Bleeding => $"[red]BLD×{e.Stacks}[/]",
            StatusEffectType.Poisoned => $"[green]PSN×{e.Stacks}[/]",
            StatusEffectType.Stunned => "[purple]STUN[/]",
            StatusEffectType.Vulnerable => "[orange1]VULN[/]",
            StatusEffectType.Fortified => $"[blue]FRT×{e.Stacks}[/]",
            StatusEffectType.Hasted => "[cyan]HAST[/]",
            StatusEffectType.Inspired => "[yellow]INSP[/]",
            _ => ""
        }));

        return new CombatantView(
            combatant.Id,
            combatant.Name,
            combatant.IsPlayer,
            combatant.Id == activeCombatant?.Id,
            healthStatus,
            string.IsNullOrWhiteSpace(effectIcons) ? "[ ]" : effectIcons,
            combatant.Initiative.ToString()
        );
    }

    /// <summary>
    /// Converts combatant HP percentage to narrative health description.
    /// Applies to enemies only - players see exact numbers.
    /// </summary>
    /// <param name="combatant">The combatant to evaluate.</param>
    /// <returns>Narrative health: "Healthy", "Wounded", "Critical", or "Dead".</returns>
    private static string GetNarrativeHealth(Combatant combatant)
    {
        if (combatant.MaxHp <= 0) return "Unknown";

        var percentage = (double)combatant.CurrentHp / combatant.MaxHp * 100;

        return percentage switch
        {
            <= 0 => "[grey]Dead[/]",
            <= 25 => "[red]Critical[/]",
            <= 75 => "[yellow]Wounded[/]",
            _ => "[green]Healthy[/]"
        };
    }

    #endregion

    #region Enemy AI Methods

    /// <inheritdoc/>
    public async Task ProcessEnemyTurnAsync(Combatant enemy)
    {
        _logger.LogDebug("Processing enemy turn for {Name}", enemy.Name);

        // UX pacing delay
        await Task.Delay(EnemyActionDelayMs);

        // Reset defending state from previous turn
        enemy.IsDefending = false;

        // Get AI decision
        var action = _aiService.DetermineAction(enemy, _gameState.CombatState!);

        _logger.LogInformation(
            "[AI] {Name} decided: {ActionType}. Target: {Target}",
            enemy.Name, action.Type, action.TargetId?.ToString() ?? "None");

        // Execute the action
        switch (action.Type)
        {
            case ActionType.Attack when action.TargetId.HasValue && action.AttackType.HasValue:
                var target = _gameState.CombatState!.TurnOrder.FirstOrDefault(c => c.Id == action.TargetId);
                if (target != null)
                {
                    var result = ExecuteEnemyAttack(enemy, target, action.AttackType.Value);
                    LogCombatEvent(result);
                }
                break;

            case ActionType.Defend:
                ProcessDefend(enemy);
                LogCombatEvent($"[yellow]{enemy.Name}[/] {action.FlavorText ?? "takes a defensive stance."}");
                break;

            case ActionType.Flee:
                ProcessFlee(enemy);
                LogCombatEvent($"[grey]{enemy.Name}[/] {action.FlavorText ?? "flees the battle!"}");
                break;

            case ActionType.Pass:
            default:
                LogCombatEvent($"[grey]{enemy.Name}[/] {action.FlavorText ?? "hesitates."}");
                break;
        }

        // Process turn end
        _statusEffects.ProcessTurnEnd(enemy);

        // Check for player death
        if (!CheckPlayerAlive())
        {
            _logger.LogWarning("Player has been defeated!");
            return;
        }

        // Check for victory (enemy fled might have been the last one)
        if (CheckVictoryCondition())
        {
            _logger.LogInformation("Combat Victory! All enemies defeated or fled.");
            return;
        }

        // Advance to next turn
        NextTurn();
    }

    /// <inheritdoc/>
    public string ExecuteEnemyAttack(Combatant attacker, Combatant target, AttackType attackType)
    {
        // Deduct stamina
        var staminaCost = _attackResolution.GetStaminaCost(attackType);
        attacker.CurrentStamina -= staminaCost;

        _logger.LogDebug(
            "{Attacker} spent {Cost} stamina. Remaining: {Current}/{Max}",
            attacker.Name, staminaCost, attacker.CurrentStamina, attacker.MaxStamina);

        // Resolve attack
        var result = _attackResolution.ResolveMeleeAttack(attacker, target, attackType);

        _logger.LogInformation(
            "{Attacker} attacks {Target} ({AttackType}): {Outcome}",
            attacker.Name, target.Name, attackType, result.Outcome);

        // Apply damage
        if (result.IsHit)
        {
            target.CurrentHp -= result.FinalDamage;

            _logger.LogDebug(
                "{Target} took {Damage} damage. HP: {Current}/{Max}",
                target.Name, result.FinalDamage, target.CurrentHp, target.MaxHp);

            // Process vampiric healing (v0.2.2c)
            var vampiricHeal = _traitService.ProcessTraitOnDamageDealt(attacker, result.FinalDamage);
            if (vampiricHeal > 0)
            {
                LogCombatEvent($"[purple]{attacker.Name}[/] drains [green]{vampiricHeal}[/] HP!");
            }

            // Sync damage back to source Character
            if (target.CharacterSource != null)
            {
                target.CharacterSource.CurrentHP = target.CurrentHp;
            }

            // Build narrative message
            return BuildEnemyHitMessage(attacker, result, target);
        }

        return BuildEnemyMissMessage(attacker, result, target);
    }

    /// <inheritdoc/>
    public void ProcessDefend(Combatant combatant)
    {
        combatant.IsDefending = true;
        _logger.LogDebug("{Name} is defending (IsDefending = true)", combatant.Name);
    }

    /// <inheritdoc/>
    public void ProcessFlee(Combatant combatant)
    {
        _logger.LogInformation("{Name} fled from combat", combatant.Name);
        RemoveDefeatedCombatant(combatant);
    }

    /// <summary>
    /// Checks if the player is still alive.
    /// </summary>
    /// <returns>True if player exists and has HP > 0, false otherwise.</returns>
    private bool CheckPlayerAlive()
    {
        var player = _gameState.CombatState?.TurnOrder.FirstOrDefault(c => c.IsPlayer);
        return player != null && player.CurrentHp > 0;
    }

    /// <summary>
    /// Builds a Spectre-markup formatted message for enemy hit.
    /// </summary>
    private static string BuildEnemyHitMessage(Combatant attacker, AttackResult result, Combatant target)
    {
        var outcomeMarkup = result.Outcome switch
        {
            AttackOutcome.Glancing => "[grey]grazes[/]",
            AttackOutcome.Solid => "[white]strikes[/]",
            AttackOutcome.Critical => "[bold red]CRITICALLY HITS[/]",
            _ => "[white]hits[/]"
        };

        return $"[red]{attacker.Name}[/] {outcomeMarkup} [cyan]{target.Name}[/] for [red]{result.FinalDamage}[/] damage!";
    }

    /// <summary>
    /// Builds a Spectre-markup formatted message for enemy miss.
    /// </summary>
    private static string BuildEnemyMissMessage(Combatant attacker, AttackResult result, Combatant target)
    {
        return result.Outcome == AttackOutcome.Fumble
            ? $"[red]{attacker.Name}[/] fumbles their attack!"
            : $"[red]{attacker.Name}[/] misses [cyan]{target.Name}[/]!";
    }

    #endregion

    #region Ability Methods (v0.2.3c)

    /// <inheritdoc/>
    public List<ActiveAbility> GetPlayerAbilities()
    {
        var state = _gameState.CombatState;
        var player = state?.TurnOrder.FirstOrDefault(c => c.IsPlayer);
        return player?.Abilities ?? new List<ActiveAbility>();
    }

    /// <inheritdoc/>
    public string ExecutePlayerAbility(int hotkey, string? targetName = null)
    {
        var abilities = GetPlayerAbilities();

        // Validate hotkey (1-based index)
        if (hotkey < 1 || hotkey > abilities.Count)
        {
            _logger.LogDebug("Invalid ability hotkey: {Hotkey}. Player has {Count} abilities.",
                hotkey, abilities.Count);
            return $"Invalid ability. Use 1-{abilities.Count}.";
        }

        var ability = abilities[hotkey - 1];
        return ExecuteAbilityInternal(ability, targetName);
    }

    /// <inheritdoc/>
    public string ExecutePlayerAbility(string abilityName, string? targetName = null)
    {
        var abilities = GetPlayerAbilities();

        // Find ability by name (partial match, case-insensitive)
        var ability = abilities.FirstOrDefault(a =>
            a.Name.Contains(abilityName, StringComparison.OrdinalIgnoreCase));

        if (ability == null)
        {
            _logger.LogDebug("Ability '{AbilityName}' not found in player's kit", abilityName);
            return $"Ability '{abilityName}' not found.";
        }

        return ExecuteAbilityInternal(ability, targetName);
    }

    /// <summary>
    /// Internal method to execute an ability with auto-targeting logic.
    /// </summary>
    /// <param name="ability">The ability to execute.</param>
    /// <param name="targetName">Optional explicit target name.</param>
    /// <returns>A narrative message describing the result.</returns>
    private string ExecuteAbilityInternal(ActiveAbility ability, string? targetName)
    {
        var state = _gameState.CombatState;

        // Validate combat state
        if (state == null)
        {
            _logger.LogWarning("ExecutePlayerAbility called but no combat is active");
            return "You are not in combat.";
        }

        // Validate it's the player's turn
        if (!state.IsPlayerTurn)
        {
            _logger.LogDebug("Ability attempted but it is not the player's turn");
            return "It is not your turn.";
        }

        var user = state.ActiveCombatant!;

        // Check if ability can be used (cooldown, resources)
        if (!_abilityService.CanUse(user, ability))
        {
            var cooldown = _abilityService.GetCooldownRemaining(user, ability.Id);
            if (cooldown > 0)
            {
                return $"{ability.Name} is on cooldown ({cooldown} turn{(cooldown > 1 ? "s" : "")} remaining).";
            }

            if (ability.StaminaCost > 0 && user.CurrentStamina < ability.StaminaCost)
            {
                return $"Not enough stamina for {ability.Name}. Need {ability.StaminaCost}, have {user.CurrentStamina}.";
            }

            if (ability.AetherCost > 0 && user.CurrentAp < ability.AetherCost)
            {
                return $"Not enough Aether for {ability.Name}. Need {ability.AetherCost}, have {user.CurrentAp}.";
            }

            return $"Cannot use {ability.Name}.";
        }

        // Resolve target
        var target = ResolveAbilityTarget(ability, targetName, state);
        if (target == null)
        {
            // Multiple enemies, target required
            var enemies = state.TurnOrder.Where(c => !c.IsPlayer && c.CurrentHp > 0).ToList();
            var enemyNames = string.Join(", ", enemies.Select(e => e.Name));
            _logger.LogWarning("Multiple enemies present, target required for {Ability}", ability.Name);
            return $"Multiple targets available: {enemyNames}. Specify a target with 'use {ability.Name} on <target>'.";
        }

        // Execute the ability
        var result = _abilityService.Execute(user, target, ability);

        // Log to combat log
        var logMessage = BuildAbilityLogMessage(ability, result, target, user);
        LogCombatEvent(logMessage);

        // Check for death
        if (target.CurrentHp <= 0 && target != user)
        {
            _logger.LogWarning("{Target} was slain by ability!", target.Name);
            RemoveDefeatedCombatant(target);

            // Check victory condition
            if (CheckVictoryCondition())
            {
                _logger.LogInformation("Combat Victory! All enemies defeated by ability.");
                _statusEffects.ProcessTurnEnd(user);
                return $"{result.Message} VICTORY! All enemies defeated!";
            }
        }

        // Process turn end
        _statusEffects.ProcessTurnEnd(user);

        return result.Message;
    }

    /// <summary>
    /// Resolves the target for an ability based on range and auto-targeting logic.
    /// </summary>
    /// <param name="ability">The ability being used.</param>
    /// <param name="targetName">Optional explicit target name.</param>
    /// <param name="state">The current combat state.</param>
    /// <returns>The resolved target, or null if multiple targets exist and none was specified.</returns>
    private Combatant? ResolveAbilityTarget(ActiveAbility ability, string? targetName, CombatState state)
    {
        var user = state.ActiveCombatant!;

        // Self-targeting abilities
        if (ability.Range == 0)
        {
            _logger.LogDebug("{Ability} is self-targeting", ability.Name);
            return user;
        }

        // Get living enemies
        var enemies = state.TurnOrder.Where(c => !c.IsPlayer && c.CurrentHp > 0).ToList();

        // Explicit target specified
        if (!string.IsNullOrWhiteSpace(targetName))
        {
            var explicitTarget = enemies.FirstOrDefault(e =>
                e.Name.Contains(targetName, StringComparison.OrdinalIgnoreCase));

            if (explicitTarget != null)
            {
                _logger.LogDebug("Explicit target: {Target}", explicitTarget.Name);
                return explicitTarget;
            }

            _logger.LogDebug("Specified target '{Target}' not found", targetName);
            return null;
        }

        // Auto-target single enemy
        if (enemies.Count == 1)
        {
            _logger.LogDebug("Auto-targeting single enemy: {Target}", enemies[0].Name);
            return enemies[0];
        }

        // Multiple enemies, no target specified
        _logger.LogDebug("Multiple enemies present ({Count}), no auto-target", enemies.Count);
        return null;
    }

    /// <summary>
    /// Builds a Spectre-markup formatted log message for ability use.
    /// </summary>
    private static string BuildAbilityLogMessage(
        ActiveAbility ability,
        AbilityResult result,
        Combatant target,
        Combatant user)
    {
        var targetText = target == user
            ? "[cyan]self[/]"
            : $"[red]{target.Name}[/]";

        var effectText = new List<string>();

        if (result.TotalDamage > 0)
        {
            effectText.Add($"[red]{result.TotalDamage}[/] damage");
        }

        if (result.TotalHealing > 0)
        {
            effectText.Add($"[green]{result.TotalHealing}[/] HP healed");
        }

        if (result.StatusesApplied?.Count > 0)
        {
            effectText.Add($"[purple]{string.Join(", ", result.StatusesApplied)}[/] applied");
        }

        var effects = effectText.Count > 0
            ? $" ({string.Join(", ", effectText)})"
            : "";

        return $"[cyan]{user.Name}[/] uses [yellow]{ability.Name}[/] on {targetText}!{effects}";
    }

    #endregion

    #region Hazard System (v0.3.3a)

    /// <summary>
    /// Triggers hazards in the current room when damage is dealt.
    /// Called after each damage application to check for DamageTaken triggers.
    /// </summary>
    /// <param name="damageType">The type of damage dealt.</param>
    /// <param name="amount">The amount of damage dealt.</param>
    /// <param name="target">The combatant who may be affected by hazard effects.</param>
    private async Task ProcessDamageHazardsAsync(DamageType damageType, int amount, Combatant target)
    {
        if (!_gameState.CurrentRoomId.HasValue) return;

        var room = await _roomRepository.GetByIdAsync(_gameState.CurrentRoomId.Value);
        if (room == null) return;

        var results = await _hazardService.TriggerOnDamageAsync(room, damageType, amount, target);

        foreach (var result in results.Where(r => r.WasTriggered))
        {
            LogCombatEvent($"[orange1][HAZARD][/] {result.Message}");

            // Sync player HP if affected
            if (target.IsPlayer && target.CharacterSource != null)
            {
                target.CharacterSource.CurrentHP = target.CurrentHp;
            }

            _logger.LogInformation(
                "[Hazard] {Hazard} triggered: {Damage} damage, State: {State}",
                result.HazardName, result.TotalDamage, result.NewState);
        }
    }

    /// <summary>
    /// Ticks hazard cooldowns at the end of a combat round.
    /// </summary>
    private async Task TickHazardCooldownsAsync()
    {
        if (!_gameState.CurrentRoomId.HasValue) return;

        var room = await _roomRepository.GetByIdAsync(_gameState.CurrentRoomId.Value);
        if (room == null) return;

        await _hazardService.TickCooldownsAsync(room);
    }

    #endregion

    #region Trauma System (v0.3.0c)

    /// <summary>
    /// Processes trauma triggers at the start of a player's turn.
    /// Applies passive stress from traumas with always-active or met conditions.
    /// </summary>
    /// <param name="combatant">The player combatant to process traumas for.</param>
    private void ProcessTraumaTriggers(Combatant combatant)
    {
        if (combatant.CharacterSource == null) return;

        var character = combatant.CharacterSource;
        var activeTraumas = character.ActiveTraumas.Where(t => t.IsActive).ToList();

        if (activeTraumas.Count == 0) return;

        _logger.LogDebug(
            "Processing {Count} active traumas for {Name}",
            activeTraumas.Count, combatant.Name);

        foreach (var trauma in activeTraumas)
        {
            var definition = TraumaRegistry.GetById(trauma.DefinitionId);
            if (definition == null || definition.StressPerTurnInCondition <= 0)
            {
                continue;
            }

            // Check if trauma condition is met
            // For v0.3.0c, only process "Always active" traumas automatically
            // Other conditions (environmental, combat-specific) require future hook integration
            if (definition.TriggerCondition.Contains("Always active", StringComparison.OrdinalIgnoreCase))
            {
                var stressResult = _traumaService.InflictStress(
                    combatant,
                    definition.StressPerTurnInCondition,
                    $"Trauma: {trauma.Name}");

                if (stressResult.NetStressApplied > 0)
                {
                    LogCombatEvent(
                        $"[magenta]{combatant.Name}[/] suffers from [grey]{trauma.Name}[/] (+{stressResult.NetStressApplied} stress)");
                }

                _logger.LogDebug(
                    "Trauma trigger: {Trauma} inflicted {Net} stress (Raw: {Raw}, Mitigated: {Mit})",
                    trauma.Name, stressResult.NetStressApplied, stressResult.RawStress, stressResult.MitigatedAmount);
            }
        }
    }

    #endregion

    #region Ambient Condition System (v0.3.3b)

    /// <summary>
    /// Processes ambient condition tick effects at the start of a combatant's turn.
    /// Applies damage, stress, or corruption based on the room's active condition.
    /// </summary>
    /// <param name="combatant">The combatant whose turn is starting.</param>
    private void ProcessConditionTick(Combatant combatant)
    {
        if (!_gameState.CurrentRoomId.HasValue || combatant.ActiveCondition == null)
        {
            return;
        }

        var condition = _conditionService.GetRoomConditionAsync(_gameState.CurrentRoomId.Value)
            .GetAwaiter().GetResult();

        if (condition == null)
        {
            return;
        }

        var tickResult = _conditionService.ProcessTurnTickAsync(combatant, condition)
            .GetAwaiter().GetResult();

        if (tickResult.WasApplied)
        {
            LogCombatEvent($"[orange1][AMBIENT][/] {tickResult.Message}");

            // Sync player state if applicable
            if (combatant.IsPlayer && combatant.CharacterSource != null)
            {
                combatant.CharacterSource.CurrentHP = combatant.CurrentHp;
                combatant.CharacterSource.PsychicStress = combatant.CurrentStress;
                combatant.CharacterSource.Corruption = combatant.CurrentCorruption;

                _logger.LogDebug(
                    "[Condition] Synced condition effects to player source. HP: {HP}, Stress: {Stress}, Corruption: {Corruption}",
                    combatant.CharacterSource.CurrentHP, combatant.CharacterSource.PsychicStress, combatant.CharacterSource.Corruption);
            }

            // Check for death from condition damage
            if (combatant.CurrentHp <= 0)
            {
                LogCombatEvent($"[red]{combatant.Name}[/] succumbs to the {condition.Name}!");
                _logger.LogWarning("{Name} killed by condition damage", combatant.Name);
            }
        }
    }

    #endregion
}
