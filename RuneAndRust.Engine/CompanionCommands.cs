using RuneAndRust.Core;
using RuneAndRust.Engine.Commands;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.34.4: Companion Command Parser
/// Handles "command" and "stance" verbs for direct companion control
/// Integrates with CommandParser service
/// </summary>
public class CompanionCommands
{
    private static readonly ILogger _log = Log.ForContext<CompanionCommands>();
    private readonly CompanionService _companionService;

    public CompanionCommands(CompanionService companionService)
    {
        _companionService = companionService;
    }

    // ============================================
    // COMMAND VERB: Direct ability usage
    // ============================================

    /// <summary>
    /// Parse and execute "command [companion_name] [ability] [target]"
    /// Examples:
    ///   command Kara shield_bash warden
    ///   command Finnr aetheric_bolt cultist_2
    ///   command Bjorn repair Kara
    /// </summary>
    public CommandResult ParseCommandVerb(
        string input,
        int characterId,
        PlayerCharacter player,
        List<Enemy> enemies)
    {
        _log.Debug("Parsing command verb: {Input}", input);

        // Split input into tokens
        var tokens = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (tokens.Length < 3)
        {
            return CommandResult.Failure("Usage: command [companion_name] [ability] [target]");
        }

        // Extract companion name (token 1)
        var companionName = tokens[1];
        var companion = _companionService.GetCompanionByName(characterId, companionName);

        if (companion == null)
        {
            return CommandResult.Failure($"Companion not found in party: {companionName}");
        }

        // Extract ability name (token 2)
        var abilityName = tokens[2];

        // Extract target (token 3, optional for self-targeting abilities)
        Enemy? targetEnemy = null;
        if (tokens.Length > 3)
        {
            var targetName = string.Join(" ", tokens.Skip(3));
            targetEnemy = FindEnemyByName(enemies, targetName);

            if (targetEnemy == null)
            {
                // Check if target is another companion (for support abilities)
                var targetCompanion = _companionService.GetCompanionByName(characterId, targetName);
                if (targetCompanion == null)
                {
                    return CommandResult.Failure($"Target not found: {targetName}");
                }
                // Support abilities on companions handled separately
                _log.Debug("Support ability targeting companion: {TargetName}", targetName);
            }
        }

        // Generate action via CompanionService
        var action = _companionService.CommandCompanion(
            companion,
            abilityName,
            targetEnemy,
            enemies,
            player);

        if (action.ActionType == "Wait" && action.Reason.Contains("Unknown"))
        {
            return CommandResult.Failure($"Ability not found or cannot be used: {abilityName}");
        }

        // Store action for execution during companion's turn
        companion.QueuedAction = action;

        _log.Information("Command queued: {CompanionName} will use {AbilityName} on {Target}",
            companion.DisplayName, abilityName, targetEnemy?.Name ?? "self");

        return CommandResult.Success($"{companion.DisplayName} will use {abilityName} on {targetEnemy?.Name ?? "self"}");
    }

    // ============================================
    // STANCE VERB: Change AI behavior
    // ============================================

    /// <summary>
    /// Parse and execute "stance [companion_name] [aggressive|defensive|passive]"
    /// Examples:
    ///   stance Runa defensive
    ///   stance Einar aggressive
    ///   stance Valdis passive
    /// </summary>
    public CommandResult ParseStanceVerb(string input, int characterId)
    {
        _log.Debug("Parsing stance verb: {Input}", input);

        // Split input into tokens
        var tokens = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (tokens.Length < 3)
        {
            return CommandResult.Failure("Usage: stance [companion_name] [aggressive|defensive|passive]");
        }

        // Extract companion name (token 1)
        var companionName = tokens[1];
        var companion = _companionService.GetCompanionByName(characterId, companionName);

        if (companion == null)
        {
            return CommandResult.Failure($"Companion not found in party: {companionName}");
        }

        // Extract new stance (token 2)
        var newStance = tokens[2].ToLower();
        var validStances = new[] { "aggressive", "defensive", "passive" };

        if (!validStances.Contains(newStance))
        {
            return CommandResult.Failure($"Invalid stance. Must be: aggressive, defensive, or passive");
        }

        // Change stance via CompanionService
        var success = _companionService.ChangeStance(companion, newStance, characterId);

        if (!success)
        {
            return CommandResult.Failure($"Failed to change stance for {companion.DisplayName}");
        }

        _log.Information("Stance changed: {CompanionName} is now {Stance}",
            companion.DisplayName, newStance);

        return CommandResult.Success($"{companion.DisplayName} is now {newStance.ToUpper()}");
    }

    // ============================================
    // HELPER METHODS
    // ============================================

    /// <summary>
    /// Find enemy by name (fuzzy match)
    /// Supports partial names and numeric suffixes (e.g., "cultist_2")
    /// </summary>
    private Enemy? FindEnemyByName(List<Enemy> enemies, string name)
    {
        // Exact match on Name
        var exact = enemies.FirstOrDefault(e =>
            e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (exact != null) return exact;

        // Exact match on Id
        exact = enemies.FirstOrDefault(e =>
            e.Id.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (exact != null) return exact;

        // Partial match on Name
        var partial = enemies.FirstOrDefault(e =>
            e.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
        if (partial != null) return partial;

        // Partial match on Id
        return enemies.FirstOrDefault(e =>
            e.Id.Contains(name, StringComparison.OrdinalIgnoreCase));
    }
}
