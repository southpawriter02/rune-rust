using RuneAndRust.Core;
using Serilog;
using System.Text;

namespace RuneAndRust.Engine.Commands;

/// <summary>
/// v0.37.2: Block Command
/// Take defensive action to boost Defense.
/// Syntax: block
/// Aliases: defend, d
/// Action Cost: Standard Action
/// </summary>
public class BlockCommand : ICommand
{
    private static readonly ILogger _log = Log.ForContext<BlockCommand>();
    private readonly CombatEngine _combatEngine;

    public BlockCommand(CombatEngine combatEngine)
    {
        _combatEngine = combatEngine ?? throw new ArgumentNullException(nameof(combatEngine));
    }

    public CommandResult Execute(GameState state, string[] args)
    {
        _log.Information(
            "Block command executed: CharacterID={CharacterID}",
            state.Player?.CharacterID ?? 0);

        try
        {
            // Validation 1: Must be in combat
            if (state.CurrentPhase != GamePhase.Combat || state.Combat == null)
            {
                _log.Debug("Block failed: Not in combat");
                return CommandResult.Failure("You can only block during combat.");
            }

            var combat = state.Combat;

            // Validation 2: Combat must be active
            if (!combat.IsActive)
            {
                _log.Debug("Block failed: Combat is not active");
                return CommandResult.Failure("Combat has ended.");
            }

            // Validation 3: Player must have a turn
            if (!IsPlayerTurn(combat))
            {
                _log.Debug("Block failed: Not player's turn");
                return CommandResult.Failure("Wait for your turn!");
            }

            // Clear combat log for this action
            combat.ClearLogForNewAction();

            // Execute defend via CombatEngine
            _combatEngine.PlayerDefend(combat);

            // Get combat log output
            var result = new StringBuilder();
            foreach (var logEntry in combat.CombatLog)
            {
                result.AppendLine(logEntry);
            }

            _log.Information("Block executed: Player={Player}", state.Player.Name);

            return CommandResult.Success(result.ToString());
        }
        catch (Exception ex)
        {
            _log.Error(ex,
                "Block command failed: CharacterID={CharacterID}, Error={ErrorType}",
                state.Player?.CharacterID ?? 0,
                ex.GetType().Name);
            return CommandResult.Failure($"An error occurred while blocking: {ex.Message}");
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
