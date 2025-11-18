using RuneAndRust.Core;
using RuneAndRust.Core.Population;
using Serilog;
using System.Text;

namespace RuneAndRust.Engine.Commands;

/// <summary>
/// v0.37.1: Investigate Command
/// Perform WITS-based skill check to discover hidden content.
/// Syntax: investigate [target]
/// Aliases: inv
/// </summary>
public class InvestigateCommand : ICommand
{
    private static readonly ILogger _log = Log.ForContext<InvestigateCommand>();
    private readonly DiceService _diceService;
    private readonly ExaminationFlavorTextService? _flavorTextService;

    public InvestigateCommand(DiceService diceService, ExaminationFlavorTextService? flavorTextService = null)
    {
        _diceService = diceService ?? throw new ArgumentNullException(nameof(diceService));
        _flavorTextService = flavorTextService;
    }

    public CommandResult Execute(GameState state, string[] args)
    {
        if (args.Length == 0)
        {
            _log.Debug("Investigate command called with no target");
            return CommandResult.Failure("Investigate what? Specify a target (e.g., 'investigate corpse').");
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
                return CommandResult.Failure("You are nowhere. This should not happen.");
            }

            // Find investigatable target in room
            var investigationResult = FindAndInvestigate(state, room, target);

            return investigationResult;
        }
        catch (Exception ex)
        {
            _log.Error(ex,
                "Investigate command failed: CharacterID={CharacterID}, Error={ErrorType}",
                state.Player?.CharacterID ?? 0,
                ex.GetType().Name);
            return CommandResult.Failure("An error occurred while investigating.");
        }
    }

    /// <summary>
    /// Find target and perform investigation with WITS check
    /// </summary>
    private CommandResult FindAndInvestigate(GameState state, Room room, string target)
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

        // Not found or not investigatable
        _log.Debug("Investigate target not found or not investigatable: {Target}", target);
        return CommandResult.Failure($"You cannot investigate '{target}' here.");
    }

    /// <summary>
    /// Investigate static terrain with WITS check
    /// </summary>
    private CommandResult InvestigateTerrain(GameState state, StaticTerrain terrain)
    {
        // Check if already investigated
        if (terrain.HasBeenInvestigated)
        {
            return CommandResult.Failure($"You have already thoroughly investigated the {terrain.TerrainName}.");
        }

        // Perform WITS check
        var dc = terrain.InvestigationDC > 0 ? terrain.InvestigationDC : 2; // Default DC 2
        var witsValue = state.Player?.Attributes?.Wits ?? 2;

        var rollResult = _diceService.Roll(witsValue);
        var success = rollResult.Successes >= dc;

        _log.Information(
            "Investigation check: Target={Target}, DC={DC}, Wits={Wits}, Successes={Successes}, Result={Result}",
            terrain.TerrainName,
            dc,
            witsValue,
            rollResult.Successes,
            success ? "SUCCESS" : "FAILURE");

        var sb = new StringBuilder();
        sb.AppendLine($"[WITS Check: {rollResult.Successes} successes vs DC {dc}] {(success ? "SUCCESS" : "FAILURE")}");
        sb.AppendLine();

        if (success)
        {
            terrain.HasBeenInvestigated = true;

            // [v0.38.9] Use examination flavor text if available
            if (_flavorTextService != null)
            {
                var flavorText = _flavorTextService.GenerateExaminationText(
                    objectCategory: "Terrain",
                    objectType: terrain.TerrainName,
                    witsCheck: rollResult.Successes,
                    objectState: null,
                    biomeName: state.CurrentRoom?.BiomeName);

                sb.AppendLine(flavorText);

                // Add lore fragment for expert examination (DC 18+)
                if (rollResult.Successes >= 18)
                {
                    var loreFragment = _flavorTextService.GetLoreFragment(
                        objectType: terrain.TerrainName,
                        detailLevel: "Expert",
                        biomeName: state.CurrentRoom?.BiomeName);

                    if (loreFragment != null)
                    {
                        sb.AppendLine();
                        sb.AppendLine($"[Expert Discovery: {loreFragment.LoreTitle}]");
                        sb.AppendLine(loreFragment.LoreText);
                    }
                }
            }
            else
            {
                sb.AppendLine(terrain.InvestigationSuccessText ?? $"You discover something interesting about the {terrain.TerrainName}.");
            }

            // Grant rewards if any (this would require a rewards system)
            // TODO: Implement reward granting in future version
        }
        else
        {
            sb.AppendLine(terrain.InvestigationFailureText ?? $"You find nothing of interest in the {terrain.TerrainName}.");
        }

        return CommandResult.Success(sb.ToString(), redrawRoom: success);
    }

    /// <summary>
    /// Investigate loot node (usually auto-succeeds, but may have hidden compartments)
    /// </summary>
    private CommandResult InvestigateLootNode(GameState state, LootNode lootNode)
    {
        // If already looted, no point investigating
        if (lootNode.HasBeenLooted)
        {
            return CommandResult.Failure($"The {lootNode.NodeType} has already been thoroughly searched.");
        }

        // For basic loot nodes, suggest using 'search' instead
        // Investigation is for WITS-based hidden content
        var hasHiddenContent = lootNode.RequiresInvestigation;

        if (!hasHiddenContent)
        {
            return CommandResult.Failure($"Use 'search {lootNode.NodeType}' to look for items.");
        }

        // Perform WITS check for hidden compartment/content
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

            // [v0.38.9] Use examination flavor text if available
            if (_flavorTextService != null)
            {
                var flavorText = _flavorTextService.GeneratePerceptionCheckText(
                    detectionType: "HiddenCache",
                    checkResult: rollResult.Successes,
                    biomeName: state.CurrentRoom?.BiomeName);

                sb.AppendLine(flavorText);
                sb.AppendLine("Use 'search' to collect the contents.");
            }
            else
            {
                sb.AppendLine($"You discover a hidden compartment in the {lootNode.NodeType}!");
                sb.AppendLine("Use 'search' to collect the contents.");
            }
        }
        else
        {
            sb.AppendLine($"You don't find anything hidden in the {lootNode.NodeType}.");
        }

        return CommandResult.Success(sb.ToString(), redrawRoom: success);
    }

    /// <summary>
    /// Investigate hazard to learn about disabling mechanism
    /// </summary>
    private CommandResult InvestigateHazard(GameState state, DynamicHazard hazard)
    {
        if (!hazard.IsActive)
        {
            return CommandResult.Failure($"The {hazard.HazardName} is already disabled.");
        }

        // Check if hazard can be disabled
        if (!hazard.CanBeDisabled)
        {
            return CommandResult.Failure($"The {hazard.HazardName} cannot be disabled through investigation.");
        }

        // If already investigated, provide hint
        if (hazard.HasBeenInvestigated)
        {
            return CommandResult.Success($"You recall: {hazard.DisableHint ?? "There must be a way to disable this."}");
        }

        // Perform WITS check
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

        return CommandResult.Success(sb.ToString());
    }
}
