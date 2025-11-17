using RuneAndRust.Core;
using Serilog;
using System.Text;

namespace RuneAndRust.Engine.Commands;

/// <summary>
/// v0.37.2: Ability Command
/// Execute specialization abilities by name.
/// Syntax: [ability_name] [target]
/// Examples: shield_bash warden, whirlwind_strike, healing_word Kara
/// </summary>
public class AbilityCommand : ICommand
{
    private static readonly ILogger _log = Log.ForContext<AbilityCommand>();
    private readonly CombatEngine _combatEngine;

    public AbilityCommand(CombatEngine combatEngine)
    {
        _combatEngine = combatEngine ?? throw new ArgumentNullException(nameof(combatEngine));
    }

    public CommandResult Execute(GameState state, string[] args)
    {
        // Args format: [ability_name, target (optional)]
        if (args.Length == 0)
        {
            _log.Debug("Ability command called with no ability name");
            return CommandResult.Failure("Specify an ability to use. Type 'skills' to see your abilities.");
        }

        var abilityName = args[0];

        _log.Information(
            "Ability command executed: CharacterID={CharacterID}, Ability={Ability}, Target={Target}",
            state.Player?.CharacterID ?? 0,
            abilityName,
            args.Length > 1 ? string.Join(" ", args.Skip(1)) : "(none)");

        try
        {
            // Validation 1: Must be in combat
            if (state.CurrentPhase != GamePhase.Combat || state.Combat == null)
            {
                _log.Debug("Ability failed: Not in combat");
                return CommandResult.Failure("You can only use combat abilities during combat.");
            }

            var combat = state.Combat;
            var player = state.Player;

            // Validation 2: Combat must be active
            if (!combat.IsActive)
            {
                _log.Debug("Ability failed: Combat is not active");
                return CommandResult.Failure("Combat has ended.");
            }

            // Validation 3: Player must have a turn
            if (!IsPlayerTurn(combat))
            {
                _log.Debug("Ability failed: Not player's turn");
                return CommandResult.Failure("Wait for your turn!");
            }

            // Find ability in player's ability list
            var ability = FindAbility(player, abilityName);

            if (ability == null)
            {
                _log.Debug("Ability not found: {Ability}", abilityName);
                var availableAbilities = string.Join(", ", player.Abilities.Select(a => a.Name));
                return CommandResult.Failure($"You don't know the ability '{abilityName}'. Your abilities: {availableAbilities}");
            }

            // Parse target if provided
            Enemy? target = null;
            if (args.Length > 1)
            {
                var targetName = string.Join(" ", args.Skip(1));
                target = FindEnemy(combat, targetName);

                if (target == null)
                {
                    _log.Debug("Ability target not found: {Target}", targetName);
                    return CommandResult.Failure($"Cannot find target '{targetName}'.");
                }

                if (target.CurrentHP <= 0)
                {
                    _log.Debug("Ability target already defeated: {Target}", target.Name);
                    return CommandResult.Failure($"{target.Name} has already been defeated.");
                }
            }

            // Clear combat log for this action
            combat.ClearLogForNewAction();

            // Execute ability via CombatEngine
            var success = _combatEngine.PlayerUseAbility(combat, ability, target);

            if (!success)
            {
                _log.Debug("Ability execution failed: {Ability}", ability.Name);
                // CombatEngine will have added error message to combat log
            }

            // Get combat log output
            var result = new StringBuilder();
            foreach (var logEntry in combat.CombatLog)
            {
                result.AppendLine(logEntry);
            }

            _log.Information(
                "Ability executed: Ability={Ability}, Target={Target}, Success={Success}",
                ability.Name,
                target?.Name ?? "none",
                success);

            return CommandResult.Success(result.ToString());
        }
        catch (Exception ex)
        {
            _log.Error(ex,
                "Ability command failed: CharacterID={CharacterID}, Ability={Ability}, Error={ErrorType}",
                state.Player?.CharacterID ?? 0,
                abilityName,
                ex.GetType().Name);
            return CommandResult.Failure($"An error occurred while using the ability: {ex.Message}");
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
    /// Find ability by name (fuzzy matching)
    /// </summary>
    private Ability? FindAbility(PlayerCharacter player, string abilityName)
    {
        var name = abilityName.ToLower().Replace("_", " ").Replace("-", " ");

        // Exact match on Name
        var exact = player.Abilities.FirstOrDefault(a =>
            a.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (exact != null) return exact;

        // Match with underscores/spaces normalized
        var normalized = player.Abilities.FirstOrDefault(a =>
            a.Name.Replace("_", " ").Replace("-", " ").Equals(name, StringComparison.OrdinalIgnoreCase));
        if (normalized != null) return normalized;

        // Partial match
        return player.Abilities.FirstOrDefault(a =>
            a.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
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
}
