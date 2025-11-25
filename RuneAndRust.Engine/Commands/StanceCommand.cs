using RuneAndRust.Core;
using Serilog;
using System.Text;

namespace RuneAndRust.Engine.Commands;

/// <summary>
/// v0.37.2: Stance Command
/// Change combat stance (Aggressive/Defensive/Calculated/Evasive).
/// Syntax: stance [aggressive|defensive|calculated|evasive]
/// Action Cost: Free Action
/// </summary>
public class StanceCommand : ICommand
{
    private static readonly ILogger _log = Log.ForContext<StanceCommand>();
    private readonly StanceService _stanceService;

    public StanceCommand(StanceService stanceService)
    {
        _stanceService = stanceService ?? throw new ArgumentNullException(nameof(stanceService));
    }

    public CommandResult Execute(GameState state, string[] args)
    {
        _log.Information(
            "Stance command executed: CharacterID={CharacterID}, RequestedStance={Stance}",
            state.Player?.CharacterID ?? 0,
            args.Length > 0 ? args[0] : "(none)");

        try
        {
            // Validation 1: Must have stance argument
            if (args.Length == 0)
            {
                _log.Debug("Stance failed: No stance specified");
                return CommandResult.CreateFailure("Specify a stance: aggressive, defensive, calculated, or evasive.");
            }

            var stanceName = args[0].ToLower();
            var player = state.Player;

            // Parse stance type
            StanceType? stanceType = stanceName switch
            {
                "aggressive" or "agg" => StanceType.Aggressive,
                "defensive" or "def" => StanceType.Defensive,
                "calculated" or "calc" => StanceType.Calculated,
                "evasive" or "eva" => StanceType.Evasive,
                _ => null
            };

            if (!stanceType.HasValue)
            {
                _log.Debug("Stance failed: Invalid stance name: {Stance}", stanceName);
                return CommandResult.CreateFailure($"Unknown stance '{stanceName}'. Valid stances: aggressive, defensive, calculated, evasive.");
            }

            // Capture combat state for logging (if in combat)
            var combat = state.Combat;

            // Clear combat log if in combat
            combat?.ClearLogForNewAction();

            // Execute stance change
            var success = _stanceService.SwitchStance(player, stanceType.Value, combat);

            if (!success)
            {
                // StanceService will have added error message to combat log or we can provide our own
                var currentStance = _stanceService.GetStanceName(player.ActiveStance?.Type ?? StanceType.Calculated);
                return CommandResult.CreateFailure($"Failed to change stance. You are already in {currentStance} stance or have no shifts remaining.");
            }

            // Build output
            var result = new StringBuilder();

            // If in combat, use combat log
            if (combat != null && combat.CombatLog.Count > 0)
            {
                foreach (var logEntry in combat.CombatLog)
                {
                    result.AppendLine(logEntry);
                }
            }
            else
            {
                // Not in combat, provide basic feedback
                var stanceName2 = _stanceService.GetStanceName(stanceType.Value);
                var description = _stanceService.GetStanceDescription(stanceType.Value);

                result.AppendLine($"You shift into {stanceName2} stance!");
                result.AppendLine(description);
            }

            _log.Information(
                "Stance changed: Character={Player}, NewStance={Stance}",
                player.Name,
                stanceType.Value);

            return CommandResult.CreateSuccess(result.ToString());
        }
        catch (Exception ex)
        {
            _log.Error(ex,
                "Stance command failed: CharacterID={CharacterID}, Error={ErrorType}",
                state.Player?.CharacterID ?? 0,
                ex.GetType().Name);
            return CommandResult.CreateFailure($"An error occurred while changing stance: {ex.Message}");
        }
    }
}
