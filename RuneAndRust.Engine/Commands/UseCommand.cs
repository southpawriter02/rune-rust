using System.Text;
using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine.Commands;

/// <summary>
/// v0.37.3: Use Command
/// Consume items or interact with environmental objects.
/// Syntax: use [item] or use [item] on [target]
/// </summary>
public class UseCommand : ICommand
{
    private static readonly ILogger _log = Log.ForContext<UseCommand>();

    public CommandResult Execute(GameState state, string[] args)
    {
        if (state.Player == null)
        {
            _log.Warning("Use command failed: Player is null");
            return CommandResult.CreateFailure("Player not found.");
        }

        // Check for arguments
        if (args.Length == 0)
        {
            _log.Debug("Use command: No item specified");
            return CommandResult.CreateFailure("Use what? (Usage: use [item name])");
        }

        // Parse arguments - check for "use X on Y" pattern
        string itemName;
        string? targetName = null;
        int onIndex = Array.FindIndex(args, a => a.Equals("on", StringComparison.OrdinalIgnoreCase));

        if (onIndex > 0 && onIndex < args.Length - 1)
        {
            // "use X on Y" pattern
            itemName = string.Join(" ", args.Take(onIndex));
            targetName = string.Join(" ", args.Skip(onIndex + 1));

            _log.Information(
                "Use command (interactive): CharacterId={CharacterId}, Item={Item}, Target={Target}",
                state.Player.CharacterID,
                itemName,
                targetName);

            // Environmental interaction not yet fully implemented
            return CommandResult.CreateFailure(
                "Environmental interactions are not yet implemented. " +
                "You can use consumables like potions and medicines.");
        }
        else
        {
            // Simple "use X" pattern - consumable
            itemName = string.Join(" ", args);

            _log.Information(
                "Use command (consumable): CharacterId={CharacterId}, Item={Item}",
                state.Player.CharacterID,
                itemName);

            return UseConsumable(state, itemName);
        }
    }

    /// <summary>
    /// Use a consumable item from the player's consumables list
    /// </summary>
    private CommandResult UseConsumable(GameState state, string itemName)
    {
        if (state.Player == null)
        {
            return CommandResult.CreateFailure("Player not found.");
        }

        // Find the consumable
        var consumable = state.Player.Consumables.FirstOrDefault(c =>
            c.Name.Contains(itemName, StringComparison.OrdinalIgnoreCase));

        if (consumable == null)
        {
            _log.Warning(
                "Use failed: Consumable not found: Item={Item}, CharacterId={CharacterId}",
                itemName,
                state.Player.CharacterID);

            // Show available consumables
            if (state.Player.Consumables.Any())
            {
                var consumableGroups = state.Player.Consumables
                    .GroupBy(c => c.Name)
                    .Select(g => $"{g.Key} (x{g.Count()})")
                    .ToList();

                var availableConsumables = string.Join(", ", consumableGroups);
                return CommandResult.CreateFailure(
                    $"You don't have a '{itemName}' consumable.\nYou have: {availableConsumables}");
            }

            return CommandResult.CreateFailure($"You don't have any consumables.");
        }

        // Apply consumable effects
        var output = new StringBuilder();
        output.AppendLine($"You use the {consumable.GetDisplayName()}.");

        bool anyEffect = false;

        // Restore HP
        if (consumable.HPRestore > 0)
        {
            int hpToRestore = consumable.GetTotalHPRestore();
            int hpBefore = state.Player.HP;
            state.Player.HP = Math.Min(state.Player.HP + hpToRestore, state.Player.MaxHP);
            int actualRestore = state.Player.HP - hpBefore;

            output.AppendLine($"+{actualRestore} HP");
            output.AppendLine($"HP: {state.Player.HP}/{state.Player.MaxHP}");
            anyEffect = true;

            _log.Information(
                "Consumable restored HP: Item={Item}, Amount={Amount}, CurrentHP={CurrentHP}",
                consumable.Name,
                actualRestore,
                state.Player.HP);
        }

        // Restore Stamina
        if (consumable.StaminaRestore > 0)
        {
            int staminaToRestore = consumable.GetTotalStaminaRestore();
            int staminaBefore = state.Player.Stamina;
            state.Player.Stamina = Math.Min(state.Player.Stamina + staminaToRestore, state.Player.MaxStamina);
            int actualRestore = state.Player.Stamina - staminaBefore;

            output.AppendLine($"+{actualRestore} Stamina");
            output.AppendLine($"Stamina: {state.Player.Stamina}/{state.Player.MaxStamina}");
            anyEffect = true;

            _log.Information(
                "Consumable restored Stamina: Item={Item}, Amount={Amount}, CurrentStamina={CurrentStamina}",
                consumable.Name,
                actualRestore,
                state.Player.Stamina);
        }

        // Restore Psychic Stress
        if (consumable.StressRestore > 0)
        {
            int stressBefore = state.Player.PsychicStress;
            state.Player.PsychicStress = Math.Max(state.Player.PsychicStress - consumable.StressRestore, 0);
            int actualRestore = stressBefore - state.Player.PsychicStress;

            output.AppendLine($"-{actualRestore} Psychic Stress");
            output.AppendLine($"Stress: {state.Player.PsychicStress}/100");
            anyEffect = true;

            _log.Information(
                "Consumable reduced Stress: Item={Item}, Amount={Amount}, CurrentStress={CurrentStress}",
                consumable.Name,
                actualRestore,
                state.Player.PsychicStress);
        }

        // Grant Temporary HP
        if (consumable.TempHPGrant > 0)
        {
            // Note: Temporary HP system not fully implemented in PlayerCharacter
            // This would require a TempHP property
            output.AppendLine($"+{consumable.TempHPGrant} Temporary HP (not yet implemented)");
            anyEffect = true;
        }

        // Status effect removals
        if (consumable.ClearsBleeding)
        {
            output.AppendLine("Bleeding stopped.");
            anyEffect = true;
            // Note: Bleeding status not yet tracked in PlayerCharacter
        }

        if (consumable.ClearsPoison)
        {
            output.AppendLine("Poison cured.");
            anyEffect = true;
            // Note: Poison status not yet tracked in PlayerCharacter
        }

        if (consumable.ClearsDisease)
        {
            output.AppendLine("Disease cured.");
            anyEffect = true;
            // Note: Disease status not yet tracked in PlayerCharacter
        }

        if (!anyEffect)
        {
            output.AppendLine("(No effect)");
        }

        // Remove the consumable from inventory
        state.Player.Consumables.Remove(consumable);

        _log.Information(
            "Consumable used: Item={Item}, RemainingConsumables={Count}",
            consumable.Name,
            state.Player.Consumables.Count);

        return CommandResult.CreateSuccess(output.ToString());
    }
}
