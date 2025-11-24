using RuneAndRust.Core;
using RuneAndRust.Core.Descriptors;
using Serilog;
using System.Text;
using InteractionType = RuneAndRust.Core.Descriptors.InteractionType;
using InteractionResult = RuneAndRust.Core.Descriptors.InteractionResult;

namespace RuneAndRust.Engine.Commands;

/// <summary>
/// v0.38.3 Integration: Object Interaction Command
/// Handles all object interactions: pull, search, read, hack, open
/// </summary>
public class ObjectInteractionCommand : ICommand
{
    private static readonly ILogger _log = Log.ForContext<ObjectInteractionCommand>();
    private readonly ObjectInteractionService _objectService;
    private readonly InteractionType _interactionType;
    private readonly string _commandName;

    public ObjectInteractionCommand(
        ObjectInteractionService objectService,
        InteractionType interactionType,
        string commandName)
    {
        _objectService = objectService ?? throw new ArgumentNullException(nameof(objectService));
        _interactionType = interactionType;
        _commandName = commandName;
    }

    public CommandResult Execute(GameState state, string[] args)
    {
        if (args.Length == 0)
        {
            return CommandResult.Failure($"{_commandName.CapitalizeFirst()} what? Specify a target.");
        }

        var target = string.Join(" ", args).ToLower().Trim('[', ']');

        _log.Information(
            "{Command} command executed: CharacterID={CharacterID}, Target={Target}, RoomId={RoomId}",
            _commandName,
            state.Player?.CharacterID ?? 0,
            target,
            state.CurrentRoom?.RoomId ?? "unknown");

        try
        {
            var room = state.CurrentRoom;
            if (room == null)
            {
                return CommandResult.Failure("You are nowhere. This should not happen.");
            }

            // Find object
            var obj = FindInteractiveObject(room, target);
            if (obj == null)
            {
                return CommandResult.Failure($"You don't see '{target}' here.");
            }

            // Check if interaction type matches
            if (obj.InteractionType != _interactionType)
            {
                var correctCommand = GetCorrectCommand(obj.InteractionType);
                return CommandResult.Failure(
                    $"You cannot {_commandName} the {obj.Name}. Try '{correctCommand} {target}' instead.");
            }

            // Attempt interaction
            var result = _objectService.AttemptInteraction(obj, state.Player?.CharacterID ?? 0);

            return FormatInteractionResult(result, obj);
        }
        catch (Exception ex)
        {
            _log.Error(ex,
                "{Command} command failed: CharacterID={CharacterID}, Error={ErrorType}",
                _commandName,
                state.Player?.CharacterID ?? 0,
                ex.GetType().Name);
            return CommandResult.Failure($"An error occurred while attempting to {_commandName} the target.");
        }
    }

    #region Helper Methods

    private InteractiveObject? FindInteractiveObject(Room room, string target)
    {
        return room.InteractiveObjects.FirstOrDefault(obj =>
            obj.Name.Contains(target, StringComparison.OrdinalIgnoreCase) ||
            obj.ObjectType.ToString().Contains(target, StringComparison.OrdinalIgnoreCase) ||
            (obj.BaseTemplateName != null && obj.BaseTemplateName.Contains(target, StringComparison.OrdinalIgnoreCase)));
    }

    private CommandResult FormatInteractionResult(InteractionResult result, InteractiveObject obj)
    {
        var sb = new StringBuilder();

        // Display interaction result
        sb.AppendLine(result.Description);

        // Display consequence details
        if (result.Consequence != null && result.Consequence.Executed)
        {
            sb.AppendLine();

            switch (result.Consequence.Type)
            {
                case ConsequenceType.Loot:
                    if (result.Consequence.LootGranted != null && result.Consequence.LootGranted.Count > 0)
                    {
                        sb.AppendLine("[Loot Granted]:");
                        foreach (var item in result.Consequence.LootGranted)
                        {
                            sb.AppendLine($"  • {item}");
                        }
                    }
                    break;

                case ConsequenceType.Reveal:
                    if (!string.IsNullOrEmpty(result.Consequence.NarrativeClue))
                    {
                        sb.AppendLine($"[Clue]: {result.Consequence.NarrativeClue}");
                    }
                    if (!string.IsNullOrEmpty(result.Consequence.QuestHook))
                    {
                        sb.AppendLine($"[Quest Hook]: {result.Consequence.QuestHook}");
                    }
                    break;

                case ConsequenceType.Trap:
                    if (!string.IsNullOrEmpty(result.Consequence.Damage))
                    {
                        sb.AppendLine($"[Trap Damage]: {result.Consequence.Damage}");
                    }
                    if (result.Consequence.StatusEffect != null && result.Consequence.StatusEffect.Count > 0)
                    {
                        var effectName = result.Consequence.StatusEffect[0]?.ToString() ?? "Unknown";
                        sb.AppendLine($"[Status Effect]: {effectName}");
                    }
                    break;
            }
        }

        // Display state change if reversible
        if (result.StateChanged && !string.IsNullOrEmpty(result.NewState))
        {
            sb.AppendLine();
            sb.AppendLine($"[State]: {obj.Name} is now {result.NewState}.");
        }

        var redrawRoom = result.Success && (
            result.Consequence?.Type == ConsequenceType.Unlock ||
            result.Consequence?.Type == ConsequenceType.Trigger ||
            result.Consequence?.Type == ConsequenceType.Spawn);

        return result.Success
            ? CommandResult.Success(sb.ToString(), redrawRoom: redrawRoom)
            : CommandResult.Failure(sb.ToString());
    }

    private string GetCorrectCommand(InteractionType interactionType)
    {
        return interactionType switch
        {
            InteractionType.Pull => "pull",
            InteractionType.Open => "open",
            InteractionType.Search => "search",
            InteractionType.Read => "read",
            InteractionType.Hack => "hack",
            InteractionType.Examine => "investigate",
            _ => "interact"
        };
    }

    #endregion
}

public static class StringExtensions
{
    public static string CapitalizeFirst(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        return char.ToUpper(str[0]) + str.Substring(1);
    }
}
