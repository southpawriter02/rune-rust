using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Core.Models.Combat;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Manages the lifecycle of combat encounters including starting, advancing turns, and ending combat.
/// </summary>
public class CombatService : ICombatService
{
    private readonly GameState _gameState;
    private readonly IInitiativeService _initiative;
    private readonly ILogger<CombatService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CombatService"/> class.
    /// </summary>
    /// <param name="gameState">The shared game state singleton.</param>
    /// <param name="initiative">The initiative service for turn order calculations.</param>
    /// <param name="logger">The logger for traceability.</param>
    public CombatService(
        GameState gameState,
        IInitiativeService initiative,
        ILogger<CombatService> logger)
    {
        _gameState = gameState;
        _initiative = initiative;
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
    public void EndCombat()
    {
        _logger.LogInformation("Combat Ended");
        _gameState.Phase = GamePhase.Exploration;
        _gameState.CombatState = null;
    }
}
