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
    /// Initializes a new instance of the <see cref="CombatService"/> class.
    /// </summary>
    /// <param name="gameState">The shared game state singleton.</param>
    /// <param name="initiative">The initiative service for turn order calculations.</param>
    /// <param name="attackResolution">The attack resolution service for combat mechanics.</param>
    /// <param name="lootService">The loot service for generating combat rewards.</param>
    /// <param name="logger">The logger for traceability.</param>
    public CombatService(
        GameState gameState,
        IInitiativeService initiative,
        IAttackResolutionService attackResolution,
        ILootService lootService,
        ILogger<CombatService> logger)
    {
        _gameState = gameState;
        _initiative = initiative;
        _attackResolution = attackResolution;
        _lootService = lootService;
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

        // Add player
        var player = Combatant.FromCharacter(_gameState.CurrentCharacter);
        _initiative.RollInitiative(player);
        state.TurnOrder.Add(player);
        _logger.LogDebug("Added player {Name} to combat", player.Name);

        // Add enemies
        foreach (var enemy in enemies)
        {
            var combatant = Combatant.FromEnemy(enemy);
            _initiative.RollInitiative(combatant);
            state.TurnOrder.Add(combatant);
            _logger.LogDebug("Added enemy {Name} to combat", combatant.Name);
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

        _logger.LogInformation("Turn: {Name} ({Type})",
            state.ActiveCombatant?.Name ?? "None",
            state.ActiveCombatant?.IsPlayer == true ? "Player" : "Enemy");
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

            // Check for death
            if (target.CurrentHp <= 0)
            {
                _logger.LogWarning("{Target} was slain! HP: 0/{Max}", target.Name, target.MaxHp);
                RemoveDefeatedCombatant(target);

                // Check victory condition
                if (CheckVictoryCondition())
                {
                    _logger.LogInformation("Combat Victory! All enemies defeated.");
                    var victoryMsg = BuildVictoryMessage(result, target);
                    LogCombatEvent(BuildLogMessage(result, target, isVictory: true));
                    return victoryMsg;
                }

                var deathMsg = BuildDeathMessage(result, target);
                LogCombatEvent(BuildLogMessage(result, target, isDeath: true));
                return deathMsg;
            }

            var hitMsg = BuildHitMessage(result, target);
            LogCombatEvent(BuildLogMessage(result, target));
            return hitMsg;
        }

        var missMsg = BuildMissMessage(result, target);
        LogCombatEvent(BuildMissLogMessage(result, target));
        return missMsg;
    }

    /// <inheritdoc/>
    public void RemoveDefeatedCombatant(Combatant combatant)
    {
        var state = _gameState.CombatState;
        if (state == null) return;

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

        _logger.LogTrace("Generated CombatViewModel for Round {Round}", state.RoundNumber);

        return new CombatViewModel(
            state.RoundNumber,
            state.ActiveCombatant?.Name ?? "Unknown",
            state.TurnOrder.Select(c => MapToView(c, state.ActiveCombatant)).ToList(),
            _combatLog.ToList(),
            new PlayerStatsView(player.CurrentHp, player.MaxHp, player.CurrentStamina, player.MaxStamina)
        );
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

        return new CombatantView(
            combatant.Id,
            combatant.Name,
            combatant.IsPlayer,
            combatant.Id == activeCombatant?.Id,
            healthStatus,
            "[ ]", // Placeholder for status effects
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
}
