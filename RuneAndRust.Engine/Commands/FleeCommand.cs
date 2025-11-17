using RuneAndRust.Core;
using Serilog;
using System.Text;

namespace RuneAndRust.Engine.Commands;

/// <summary>
/// v0.37.2: Flee Command
/// Attempt to escape from combat.
/// Syntax: flee
/// Aliases: run, escape
/// Action Cost: Full turn (Standard + Free)
/// </summary>
public class FleeCommand : ICommand
{
    private static readonly ILogger _log = Log.ForContext<FleeCommand>();
    private readonly CombatEngine _combatEngine;

    public FleeCommand(CombatEngine combatEngine)
    {
        _combatEngine = combatEngine ?? throw new ArgumentNullException(nameof(combatEngine));
    }

    public CommandResult Execute(GameState state, string[] args)
    {
        _log.Information(
            "Flee command executed: CharacterID={CharacterID}",
            state.Player?.CharacterID ?? 0);

        try
        {
            // Validation 1: Must be in combat
            if (state.CurrentPhase != GamePhase.Combat || state.Combat == null)
            {
                _log.Debug("Flee failed: Not in combat");
                return CommandResult.Failure("You are not in combat.");
            }

            var combat = state.Combat;

            // Validation 2: Combat must be active
            if (!combat.IsActive)
            {
                _log.Debug("Flee failed: Combat is not active");
                return CommandResult.Failure("Combat has already ended.");
            }

            // Validation 3: Can flee from this combat
            if (!combat.CanFlee)
            {
                _log.Debug("Flee failed: Cannot flee from this combat");
                return CommandResult.Failure("You cannot flee from this encounter!");
            }

            // Validation 4: Player must have a turn
            if (!IsPlayerTurn(combat))
            {
                _log.Debug("Flee failed: Not player's turn");
                return CommandResult.Failure("Wait for your turn!");
            }

            // Clear combat log for this action
            combat.ClearLogForNewAction();

            // Attempt flee via CombatEngine
            var success = _combatEngine.PlayerFlee(combat);

            // Get combat log output
            var result = new StringBuilder();
            foreach (var logEntry in combat.CombatLog)
            {
                result.AppendLine(logEntry);
            }

            if (success)
            {
                _log.Information("Flee successful: Player={Player}", state.Player.Name);

                // End combat phase
                state.CurrentPhase = GamePhase.Exploration;

                // Note: The actual room navigation should be handled by the game loop
                result.AppendLine();
                result.AppendLine("You have escaped from combat!");
                result.AppendLine();

                // Trigger look command to show new room
                var lookCommand = new LookCommand();
                var lookResult = lookCommand.Execute(state, Array.Empty<string>());
                if (lookResult.Success)
                {
                    result.Append(lookResult.Message);
                }
            }
            else
            {
                _log.Information("Flee failed: Player={Player}", state.Player.Name);
                result.AppendLine();
                result.AppendLine("You remain in combat!");
            }

            return CommandResult.Success(result.ToString(), redrawRoom: success);
        }
        catch (Exception ex)
        {
            _log.Error(ex,
                "Flee command failed: CharacterID={CharacterID}, Error={ErrorType}",
                state.Player?.CharacterID ?? 0,
                ex.GetType().Name);
            return CommandResult.Failure($"An error occurred while attempting to flee: {ex.Message}");
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
}
