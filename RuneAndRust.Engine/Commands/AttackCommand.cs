using RuneAndRust.Core;
using Serilog;
using System.Text;

namespace RuneAndRust.Engine.Commands;

/// <summary>
/// v0.37.2: Attack Command
/// Basic weapon attack with accuracy and damage resolution.
/// Syntax: attack [target]
/// Aliases: a, hit, kill, fight
/// </summary>
public class AttackCommand : ICommand
{
    private static readonly ILogger _log = Log.ForContext<AttackCommand>();
    private readonly CombatEngine _combatEngine;

    public AttackCommand(CombatEngine combatEngine)
    {
        _combatEngine = combatEngine ?? throw new ArgumentNullException(nameof(combatEngine));
    }

    public CommandResult Execute(GameState state, string[] args)
    {
        _log.Information(
            "Attack command executed: CharacterID={CharacterID}, Target={Target}",
            state.Player?.CharacterID ?? 0,
            args.Length > 0 ? string.Join(" ", args) : "(none)");

        try
        {
            // Validation 1: Must be in combat
            if (state.CurrentPhase != GamePhase.Combat || state.Combat == null)
            {
                _log.Debug("Attack failed: Not in combat");
                return CommandResult.Failure("You are not in combat. Use 'look' to assess your surroundings.");
            }

            var combat = state.Combat;

            // Validation 2: Combat must be active
            if (!combat.IsActive)
            {
                _log.Debug("Attack failed: Combat is not active");
                return CommandResult.Failure("Combat has ended.");
            }

            // Validation 3: Must have target argument
            if (args.Length == 0)
            {
                _log.Debug("Attack failed: No target specified");
                return CommandResult.Failure(GetAvailableTargetsMessage(combat));
            }

            // Validation 4: Player must have a turn
            if (!IsPlayerTurn(combat))
            {
                _log.Debug("Attack failed: Not player's turn");
                return CommandResult.Failure("Wait for your turn!");
            }

            // Parse target
            var targetName = string.Join(" ", args);
            var target = FindEnemy(combat, targetName);

            if (target == null)
            {
                _log.Debug("Attack failed: Target not found: {Target}", targetName);
                return CommandResult.Failure($"Cannot find enemy '{targetName}'. {GetAvailableTargetsMessage(combat)}");
            }

            // Validation 5: Target must be alive
            if (target.CurrentHP <= 0)
            {
                _log.Debug("Attack failed: Target already defeated: {Target}", target.Name);
                return CommandResult.Failure($"{target.Name} has already been defeated.");
            }

            // Clear combat log for this action
            combat.ClearLogForNewAction();

            // Execute attack via CombatEngine
            _combatEngine.PlayerAttack(combat, target);

            // Get combat log output
            var result = new StringBuilder();
            foreach (var logEntry in combat.CombatLog)
            {
                result.AppendLine(logEntry);
            }

            _log.Information(
                "Attack executed: Attacker={Player}, Target={Target}",
                state.Player.Name,
                target.Name);

            return CommandResult.Success(result.ToString());
        }
        catch (Exception ex)
        {
            _log.Error(ex,
                "Attack command failed: CharacterID={CharacterID}, Error={ErrorType}",
                state.Player?.CharacterID ?? 0,
                ex.GetType().Name);
            return CommandResult.Failure($"An error occurred during the attack: {ex.Message}");
        }
    }

    /// <summary>
    /// Check if it's currently the player's turn
    /// </summary>
    private bool IsPlayerTurn(CombatState combat)
    {
        if (combat.InitiativeOrder == null || combat.InitiativeOrder.Count == 0)
        {
            return true; // Default to allowing action if no initiative system
        }

        var currentParticipant = combat.InitiativeOrder[combat.CurrentTurnIndex];
        return currentParticipant.IsPlayer;
    }

    /// <summary>
    /// Find enemy by name (fuzzy matching)
    /// </summary>
    private Enemy? FindEnemy(CombatState combat, string targetName)
    {
        var name = targetName.ToLower();

        // Exact match on Name
        var exact = combat.Enemies.FirstOrDefault(e =>
            e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (exact != null) return exact;

        // Exact match on Id
        exact = combat.Enemies.FirstOrDefault(e =>
            e.Id.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (exact != null) return exact;

        // Partial match on Name
        var partial = combat.Enemies.FirstOrDefault(e =>
            e.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
        if (partial != null) return partial;

        // Partial match on Id
        return combat.Enemies.FirstOrDefault(e =>
            e.Id.Contains(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Generate helpful message showing available targets
    /// </summary>
    private string GetAvailableTargetsMessage(CombatState combat)
    {
        var aliveEnemies = combat.Enemies.Where(e => e.CurrentHP > 0).ToList();

        if (!aliveEnemies.Any())
        {
            return "No enemies remain.";
        }

        var targetList = string.Join(", ", aliveEnemies.Select(e => e.Name));
        return $"Attack who? Available targets: {targetList}";
    }
}
