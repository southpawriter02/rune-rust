using RuneAndRust.Core;
using RuneAndRust.Core.Descriptors;
using PopulationLootNode = RuneAndRust.Core.Population.LootNode;
using PopulationDynamicHazard = RuneAndRust.Core.Population.DynamicHazard;
using RuneAndRust.Core.Population;
using Serilog;
using System.Text;

namespace RuneAndRust.Engine.Commands;

/// <summary>
/// v0.38 Integration: Enhanced Investigate Command
/// Integrates v0.38.3 ObjectInteractionService with existing investigation system
/// Syntax: investigate [target]
/// Aliases: inv, examine
/// </summary>
public class EnhancedInvestigateCommand : ICommand
{
    private static readonly ILogger _log = Log.ForContext<EnhancedInvestigateCommand>();
    private readonly DiceService _diceService;
    private readonly ObjectInteractionService? _objectService;

    public EnhancedInvestigateCommand(
        DiceService diceService,
        ObjectInteractionService? objectService = null)
    {
        _diceService = diceService ?? throw new ArgumentNullException(nameof(diceService));
        _objectService = objectService;
    }

    public CommandResult Execute(GameState state, string[] args)
    {
        if (args.Length == 0)
        {
            return ListInvestigatableTargets(state.CurrentRoom);
        }

        var target = string.Join(" ", args).ToLower().Trim('[', ']');

        _log.Information(
            "Investigate command executed: CharacterID={CharacterID}, Target={Target}, RoomId={RoomId}",
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

            // Step 1: Try v0.38.3 Interactive Objects first (highest priority)
            if (_objectService != null && room.InteractiveObjects.Count > 0)
            {
                var obj = FindInteractiveObject(room, target);
                if (obj != null)
                {
                    return InvestigateInteractiveObject(obj, state);
                }
            }

            // Step 2: Fall back to legacy investigation system
            var legacyResult = FindAndInvestigateLegacy(state, room, target);
            if (legacyResult != null)
            {
                return legacyResult;
            }

            // Not found
            _log.Debug("Investigate target not found: {Target}", target);
            return CommandResult.CreateFailure($"You cannot investigate '{target}' here.");
        }
        catch (Exception ex)
        {
            _log.Error(ex,
                "Investigate command failed: CharacterID={CharacterID}, Error={ErrorType}",
                state.Player?.CharacterID ?? 0,
                ex.GetType().Name);
            return CommandResult.CreateFailure("An error occurred while investigating.");
        }
    }

    #region v0.38.3 Interactive Object Investigation

    private InteractiveObject? FindInteractiveObject(Room room, string target)
    {
        return room.InteractiveObjects.FirstOrDefault(obj =>
            obj.Name.Contains(target, StringComparison.OrdinalIgnoreCase) ||
            obj.ObjectType.ToString().Contains(target, StringComparison.OrdinalIgnoreCase));
    }

    private CommandResult InvestigateInteractiveObject(InteractiveObject obj, GameState state)
    {
        if (_objectService == null)
        {
            return CommandResult.CreateFailure("Object interaction service not available.");
        }

        _log.Information(
            "Investigating interactive object: {ObjName} (Type: {Type})",
            obj.Name,
            obj.ObjectType);

        var sb = new StringBuilder();

        // Display object description
        sb.AppendLine($"[{obj.ObjectType}: {obj.Name}]");
        sb.AppendLine(obj.Description);
        sb.AppendLine();

        // Display tactical information
        var summary = obj.GetDisplaySummary();
        sb.AppendLine($"Summary: {summary}");
        sb.AppendLine();

        // For investigatable objects (corpses, data slates), auto-interact
        if (obj.ObjectType == InteractiveObjectType.Investigatable)
        {
            var result = _objectService.AttemptInteraction(obj);
            sb.AppendLine(result.Description);

            if (result.Consequence != null && result.Consequence.Executed)
            {
                if (!string.IsNullOrEmpty(result.Consequence.NarrativeClue))
                {
                    sb.AppendLine();
                    sb.AppendLine($"[Clue]: {result.Consequence.NarrativeClue}");
                }

                if (!string.IsNullOrEmpty(result.Consequence.QuestHook))
                {
                    sb.AppendLine();
                    sb.AppendLine($"[Quest Hook]: {result.Consequence.QuestHook}");
                }
            }

            return CommandResult.CreateSuccess(sb.ToString(), redrawRoom: true);
        }

        // For other objects, provide interaction hint
        var hint = obj.InteractionType switch
        {
            InteractionType.Pull => $"Use 'pull {obj.Name.ToLower()}' to activate it.",
            InteractionType.Open => $"Use 'open {obj.Name.ToLower()}' to interact with it.",
            InteractionType.Search => $"Use 'search {obj.Name.ToLower()}' to look for items.",
            InteractionType.Read => $"Use 'read {obj.Name.ToLower()}' to view its contents.",
            InteractionType.Hack => $"Use 'hack {obj.Name.ToLower()}' to interface with it.",
            _ => "You may be able to interact with this object."
        };

        sb.AppendLine($"Hint: {hint}");

        return CommandResult.CreateSuccess(sb.ToString());
    }

    private CommandResult ListInvestigatableTargets(Room? room)
    {
        if (room == null)
        {
            return CommandResult.CreateFailure("You are nowhere.");
        }

        var sb = new StringBuilder();
        sb.AppendLine("You can investigate:");
        sb.AppendLine();

        var count = 0;

        // List v0.38.3 Interactive Objects
        foreach (var obj in room.InteractiveObjects)
        {
            sb.AppendLine($"  • {obj.Name} ({obj.ObjectType})");
            count++;
        }

        // List legacy objects
        if (room.StaticTerrain?.Any(t => t.IsInteractive) == true)
        {
            foreach (var terrain in room.StaticTerrain.Where(t => t.IsInteractive))
            {
                sb.AppendLine($"  • {terrain.TerrainName} (Static Terrain)");
                count++;
            }
        }

        if (room.LootNodes?.Count > 0)
        {
            foreach (var loot in room.LootNodes)
            {
                sb.AppendLine($"  • {loot.NodeType} (Loot Node)");
                count++;
            }
        }

        if (count == 0)
        {
            return CommandResult.CreateFailure("There is nothing here worth investigating.");
        }

        return CommandResult.CreateSuccess(sb.ToString());
    }

    #endregion

    #region Legacy Investigation System

    private CommandResult? FindAndInvestigateLegacy(GameState state, Room room, string target)
    {
        // Check Static Terrain
        var terrain = room.StaticTerrain?.FirstOrDefault(t =>
            t.TerrainName.Contains(target, StringComparison.OrdinalIgnoreCase));

        if (terrain != null && terrain.IsInteractive)
        {
            return InvestigateTerrain(state, terrain);
        }

        // Check Loot Nodes (corpses, chests, etc.)
        var lootNode = room.LootNodes?.FirstOrDefault(l =>
            l.NodeType.Contains(target, StringComparison.OrdinalIgnoreCase));

        if (lootNode != null)
        {
            return InvestigateLootNode(state, lootNode);
        }

        // Check Dynamic Hazards (some may have hidden mechanisms)
        var hazard = room.DynamicHazards?.FirstOrDefault(h =>
            h.HazardName.Contains(target, StringComparison.OrdinalIgnoreCase));

        if (hazard != null)
        {
            return InvestigateHazard(state, hazard);
        }

        return null;
    }

    private CommandResult InvestigateTerrain(GameState state, StaticTerrain terrain)
    {
        if (terrain.HasBeenInvestigated)
        {
            return CommandResult.CreateFailure($"You have already thoroughly investigated the {terrain.TerrainName}.");
        }

        var dc = terrain.InvestigationDC > 0 ? terrain.InvestigationDC : 2;
        var witsValue = state.Player?.Attributes?.Wits ?? 2;

        var rollResult = _diceService.Roll(witsValue);
        var success = rollResult.Successes >= dc;

        _log.Information(
            "Investigation check: Target={Target}, DC={DC}, Successes={Successes}, Result={Result}",
            terrain.TerrainName,
            dc,
            rollResult.Successes,
            success ? "SUCCESS" : "FAILURE");

        var sb = new StringBuilder();
        sb.AppendLine($"[WITS Check: {rollResult.Successes} successes vs DC {dc}] {(success ? "SUCCESS" : "FAILURE")}");
        sb.AppendLine();

        if (success)
        {
            terrain.HasBeenInvestigated = true;
            sb.AppendLine(terrain.InvestigationSuccessText ?? $"You discover something interesting about the {terrain.TerrainName}.");
        }
        else
        {
            sb.AppendLine(terrain.InvestigationFailureText ?? $"You find nothing of interest in the {terrain.TerrainName}.");
        }

        return CommandResult.CreateSuccess(sb.ToString(), redrawRoom: success);
    }

    private CommandResult InvestigateLootNode(GameState state, PopulationLootNode lootNode)
    {
        if (lootNode.HasBeenLooted)
        {
            return CommandResult.CreateFailure($"The {lootNode.NodeType} has already been thoroughly searched.");
        }

        var hasHiddenContent = lootNode.RequiresInvestigation;

        if (!hasHiddenContent)
        {
            return CommandResult.CreateFailure($"Use 'search {lootNode.NodeType}' to look for items.");
        }

        var dc = lootNode.InvestigationDC > 0 ? lootNode.InvestigationDC : 2;
        var witsValue = state.Player?.Attributes?.Wits ?? 2;

        var rollResult = _diceService.Roll(witsValue);
        var success = rollResult.Successes >= dc;

        _log.Information(
            "Investigation check (loot): Target={Target}, DC={DC}, Successes={Successes}, Result={Result}",
            lootNode.NodeType,
            dc,
            rollResult.Successes,
            success ? "SUCCESS" : "FAILURE");

        var sb = new StringBuilder();
        sb.AppendLine($"[WITS Check: {rollResult.Successes} successes vs DC {dc}] {(success ? "SUCCESS" : "FAILURE")}");
        sb.AppendLine();

        if (success)
        {
            lootNode.HiddenContentRevealed = true;
            sb.AppendLine($"You discover a hidden compartment in the {lootNode.NodeType}!");
            sb.AppendLine("Use 'search' to collect the contents.");
        }
        else
        {
            sb.AppendLine($"You don't find anything hidden in the {lootNode.NodeType}.");
        }

        return CommandResult.CreateSuccess(sb.ToString(), redrawRoom: success);
    }

    private CommandResult InvestigateHazard(GameState state, PopulationDynamicHazard hazard)
    {
        if (!hazard.IsActive)
        {
            return CommandResult.CreateFailure($"The {hazard.HazardName} is already disabled.");
        }

        if (!hazard.CanBeDisabled)
        {
            return CommandResult.CreateFailure($"The {hazard.HazardName} cannot be disabled through investigation.");
        }

        if (hazard.HasBeenInvestigated)
        {
            return CommandResult.CreateSuccess($"You recall: {hazard.DisableHint ?? "There must be a way to disable this."}");
        }

        var dc = hazard.InvestigationDC > 0 ? hazard.InvestigationDC : 3;
        var witsValue = state.Player?.Attributes?.Wits ?? 2;

        var rollResult = _diceService.Roll(witsValue);
        var success = rollResult.Successes >= dc;

        _log.Information(
            "Investigation check (hazard): Target={Target}, DC={DC}, Successes={Successes}, Result={Result}",
            hazard.HazardName,
            dc,
            rollResult.Successes,
            success ? "SUCCESS" : "FAILURE");

        var sb = new StringBuilder();
        sb.AppendLine($"[WITS Check: {rollResult.Successes} successes vs DC {dc}] {(success ? "SUCCESS" : "FAILURE")}");
        sb.AppendLine();

        if (success)
        {
            hazard.HasBeenInvestigated = true;
            sb.AppendLine($"You study the {hazard.HazardName} and discover how it works:");
            sb.AppendLine(hazard.DisableHint ?? "You might be able to disable it with the right action.");
        }
        else
        {
            sb.AppendLine($"You cannot discern how the {hazard.HazardName} functions.");
        }

        return CommandResult.CreateSuccess(sb.ToString());
    }

    #endregion
}
