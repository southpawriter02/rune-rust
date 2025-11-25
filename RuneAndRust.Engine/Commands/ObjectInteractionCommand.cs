using RuneAndRust.Core;
using RuneAndRust.Core.Descriptors;
using Serilog;
using System.Text;
using DescriptorInteractionType = RuneAndRust.Core.Descriptors.InteractionType;
using DescriptorInteractionResult = RuneAndRust.Core.Descriptors.InteractionResult;

namespace RuneAndRust.Engine.Commands;

/// <summary>
/// v0.38.3 Integration: Object Interaction Command
/// Handles all object interactions: pull, search, read, hack, open
/// </summary>
public class ObjectInteractionCommand : ICommand
{
    private static readonly ILogger _log = Log.ForContext<ObjectInteractionCommand>();
    private readonly ObjectInteractionService _objectService;
    private readonly DescriptorInteractionType _interactionType;
    private readonly string _commandName;

    public ObjectInteractionCommand(
        ObjectInteractionService objectService,
        DescriptorInteractionType interactionType,
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
            return CommandResult.CreateFailure($"{_commandName.CapitalizeFirst()} what? Specify a target.");
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
                return CommandResult.CreateFailure("You are nowhere. This should not happen.");
            }

            // Find object
            var obj = FindInteractiveObject(room, target);
            if (obj == null)
            {
                return CommandResult.CreateFailure($"You don't see '{target}' here.");
            }

            // Check if interaction type matches
            if (obj.InteractionType != _interactionType)
            {
                var correctCommand = GetCorrectCommand(obj.InteractionType);
                return CommandResult.CreateFailure(
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
            return CommandResult.CreateFailure($"An error occurred while attempting to {_commandName} the target.");
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

    private CommandResult FormatInteractionResult(DescriptorInteractionResult result, InteractiveObject obj)
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
            ? CommandResult.CreateSuccess(sb.ToString(), redrawRoom: redrawRoom)
            : CommandResult.CreateFailure(sb.ToString());
    }

    private string GetCorrectCommand(DescriptorInteractionType interactionType)
    {
        return interactionType switch
        {
            DescriptorInteractionType.Pull => "pull",
            DescriptorInteractionType.Open => "open",
            DescriptorInteractionType.Search => "search",
            DescriptorInteractionType.Read => "read",
            DescriptorInteractionType.Hack => "hack",
            DescriptorInteractionType.Examine => "investigate",
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
