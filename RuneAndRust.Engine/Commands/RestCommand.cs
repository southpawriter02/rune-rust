using System.Text;
using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine.Commands;

/// <summary>
/// v0.37.4: Rest Command
/// Initiates rest to recover HP, Stamina, and Aether.
/// Syntax: rest
/// </summary>
public class RestCommand : ICommand
{
    private static readonly ILogger _log = Log.ForContext<RestCommand>();

    public CommandResult Execute(GameState state, string[] args)
    {
        if (state.Player == null)
        {
            _log.Warning("Rest command failed: Player is null");
            return CommandResult.Failure("Player not found.");
        }

        _log.Information(
            "Rest initiated: CharacterId={CharacterId}, RoomId={RoomId}",
            state.Player.CharacterID,
            state.CurrentRoom?.RoomId ?? "unknown");

        // Cannot rest during combat
        if (state.CurrentPhase == GamePhase.Combat)
        {
            _log.Warning(
                "Rest failed: Character in combat: CharacterId={CharacterId}",
                state.Player.CharacterID);

            return CommandResult.Failure("You cannot rest during combat. Defeat or flee from enemies first.");
        }

        // Store values before rest
        int hpBefore = state.Player.HP;
        int staminaBefore = state.Player.Stamina;
        int aetherBefore = state.Player.AP;
        int maxHP = state.Player.MaxHP;
        int maxStamina = state.Player.MaxStamina;
        int maxAether = state.Player.MaxAP;

        // Perform rest recovery
        state.Player.HP = state.Player.MaxHP;
        state.Player.Stamina = state.Player.MaxStamina;

        // Restore Aether for Mystics
        if (state.Player.Class == CharacterClass.Mystic)
        {
            state.Player.AP = state.Player.MaxAP;
        }

        // Clear temporary status effects
        state.Player.DefenseBonus = 0;
        state.Player.DefenseTurnsRemaining = 0;
        state.Player.BattleRageTurnsRemaining = 0;

        // Track rooms explored since rest (for rest cooldown)
        state.Player.RoomsExploredSinceRest = 0;

        // Build rest message
        var output = new StringBuilder();
        output.AppendLine("You make camp and rest for 8 hours...");
        output.AppendLine();
        output.AppendLine("[Rest Complete]");

        // HP restoration
        int hpRestored = state.Player.HP - hpBefore;
        if (hpRestored > 0)
        {
            output.AppendLine($"- HP restored: {state.Player.HP}/{maxHP} (+{hpRestored})");
        }
        else
        {
            output.AppendLine($"- HP: {state.Player.HP}/{maxHP} (already at max)");
        }

        // Stamina restoration
        int staminaRestored = state.Player.Stamina - staminaBefore;
        if (staminaRestored > 0)
        {
            output.AppendLine($"- Stamina restored: {state.Player.Stamina}/{maxStamina} (+{staminaRestored})");
        }
        else
        {
            output.AppendLine($"- Stamina: {state.Player.Stamina}/{maxStamina} (already at max)");
        }

        // Aether restoration (Mystics only)
        if (state.Player.Class == CharacterClass.Mystic && maxAether > 0)
        {
            int aetherRestored = state.Player.AP - aetherBefore;
            if (aetherRestored > 0)
            {
                output.AppendLine($"- Aether restored: {state.Player.AP}/{maxAether} (+{aetherRestored})");
            }
            else
            {
                output.AppendLine($"- Aether: {state.Player.AP}/{maxAether} (already at max)");
            }
        }

        // Status effects cleared
        output.AppendLine("- Temporary effects cleared");

        output.AppendLine();

        // Psychic Stress warning (only Sanctuary rest clears stress)
        if (state.Player.PsychicStress > 0)
        {
            output.AppendLine($"Psychic Stress: {state.Player.PsychicStress}/100 (unchanged)");
            output.AppendLine("Warning: Psychic Stress can only be reduced at Sanctuaries.");
        }

        _log.Information(
            "Rest completed: HP {HPBefore}->{HPAfter}, Stamina {StaminaBefore}->{StaminaAfter}",
            hpBefore,
            state.Player.HP,
            staminaBefore,
            state.Player.Stamina);

        return CommandResult.Success(output.ToString());
    }
}
