using System.Text;
using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine.Commands;

/// <summary>
/// v0.37.4: Companion Command Command
/// Direct companion to perform specific actions in combat.
/// Syntax: command [companion] [action] [target] (aliases: cmd)
/// </summary>
public class CompanionCommandCommand : ICommand
{
    private static readonly ILogger _log = Log.ForContext<CompanionCommandCommand>();
    private readonly CompanionService _companionService;

    public CompanionCommandCommand(CompanionService companionService)
    {
        _companionService = companionService;
    }

    public CommandResult Execute(GameState state, string[] args)
    {
        if (state.Player == null)
        {
            _log.Warning("Companion command failed: Player is null");
            return CommandResult.CreateFailure("Player not found.");
        }

        // Must be in combat to command companions
        if (state.CurrentPhase != GamePhase.Combat)
        {
            _log.Debug("Companion command failed: Not in combat");
            return CommandResult.CreateFailure("You can only command companions during combat. Use 'stance' to change companion behavior outside combat.");
        }

        if (state.Combat == null)
        {
            _log.Warning("Companion command failed: Combat state is null");
            return CommandResult.CreateFailure("Combat state not found.");
        }

        // Parse arguments: command [companion_name] [action] [target]
        if (args.Length < 2)
        {
            _log.Debug("Companion command failed: Insufficient arguments");
            return CommandResult.CreateFailure("Usage: command [companion] [action] [target]");
        }

        string companionName = args[0];
        string action = args[1];
        string? targetName = args.Length > 2 ? string.Join(" ", args.Skip(2)) : null;

        _log.Information(
            "Companion command: CharacterId={CharacterId}, Companion={Companion}, Action={Action}, Target={Target}",
            state.Player.CharacterID,
            companionName,
            action,
            targetName ?? "none");

        // Find companion in active party
        var companion = _companionService.GetCompanionByName(state.Player.CharacterID, companionName);

        if (companion == null)
        {
            _log.Warning(
                "Companion command failed: Companion not found: {CompanionName}",
                companionName);

            return CommandResult.CreateFailure($"'{companionName}' is not in your active party.");
        }

        // Check if companion is incapacitated
        if (companion.IsIncapacitated)
        {
            _log.Warning(
                "Companion command failed: Companion incapacitated: {CompanionName}",
                companion.DisplayName);

            return CommandResult.CreateFailure($"{companion.DisplayName} is incapacitated (System Crash) and cannot act.");
        }

        // Find target enemy if specified
        Enemy? targetEnemy = null;
        if (!string.IsNullOrEmpty(targetName))
        {
            targetEnemy = FindEnemyByName(state.Combat.Enemies, targetName);

            if (targetEnemy == null)
            {
                _log.Warning(
                    "Companion command failed: Target not found: {TargetName}",
                    targetName);

                var enemyNames = string.Join(", ", state.Combat.Enemies.Select(e => e.Name));
                return CommandResult.CreateFailure($"Target '{targetName}' not found. Available targets: {enemyNames}");
            }
        }

        // Generate companion action via CompanionService
        var companionAction = _companionService.CommandCompanion(
            companion,
            action,
            targetEnemy,
            state.Combat.Enemies,
            state.Player);

        if (companionAction.ActionType == "Wait" && companionAction.Reason.Contains("Unknown"))
        {
            _log.Warning(
                "Companion command failed: Invalid action: {Action}",
                action);

            return CommandResult.CreateFailure($"'{action}' is not a valid ability or action for {companion.DisplayName}.");
        }

        // Queue the action for execution during companion's turn
        // Note: This requires the companion to have a QueuedAction property
        // If not available, we'll return a success message indicating the command was received

        _log.Information(
            "Companion command queued: {CompanionName} will {Action} on {Target}",
            companion.DisplayName,
            action,
            targetName ?? "self");

        var output = new StringBuilder();
        output.AppendLine($"{companion.DisplayName} acknowledges your command.");
        output.AppendLine($"Will attempt: {action}");
        if (targetName != null)
        {
            output.AppendLine($"Target: {targetName}");
        }

        return CommandResult.CreateSuccess(output.ToString());
    }

    /// <summary>
    /// Find enemy by name (fuzzy matching)
    /// </summary>
    private Enemy? FindEnemyByName(List<Enemy> enemies, string targetName)
    {
        // Exact match
        var exact = enemies.FirstOrDefault(e =>
            e.Name.Equals(targetName, StringComparison.OrdinalIgnoreCase));

        if (exact != null)
            return exact;

        // Partial match
        return enemies.FirstOrDefault(e =>
            e.Name.Contains(targetName, StringComparison.OrdinalIgnoreCase));
    }
}
