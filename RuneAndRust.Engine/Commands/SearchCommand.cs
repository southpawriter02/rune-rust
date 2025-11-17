using RuneAndRust.Core;
using RuneAndRust.Core.Population;
using Serilog;
using System.Text;

namespace RuneAndRust.Engine.Commands;

/// <summary>
/// v0.37.1: Search Command
/// Search containers for loot (no skill check required).
/// Simpler than investigate - finds obvious/visible contents.
/// Syntax: search [container]
/// </summary>
public class SearchCommand : ICommand
{
    private static readonly ILogger _log = Log.ForContext<SearchCommand>();
    private readonly LootService _lootService;

    public SearchCommand(LootService lootService)
    {
        _lootService = lootService ?? throw new ArgumentNullException(nameof(lootService));
    }

    public CommandResult Execute(GameState state, string[] args)
    {
        if (args.Length == 0)
        {
            _log.Debug("Search command called with no target");
            return CommandResult.Failure("Search what? Specify a container (e.g., 'search chest').");
        }

        var target = string.Join(" ", args).ToLower().Trim('[', ']');

        _log.Information(
            "Search command executed: CharacterID={CharacterID}, Target={Target}, RoomId={RoomId}",
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

            // Find and search container
            var searchResult = FindAndSearch(state, room, target);

            return searchResult;
        }
        catch (Exception ex)
        {
            _log.Error(ex,
                "Search command failed: CharacterID={CharacterID}, Error={ErrorType}",
                state.Player?.CharacterID ?? 0,
                ex.GetType().Name);
            return CommandResult.Failure("An error occurred while searching.");
        }
    }

    /// <summary>
    /// Find container and search it
    /// </summary>
    private CommandResult FindAndSearch(GameState state, Room room, string target)
    {
        // Check Loot Nodes (primary searchable containers)
        var lootNode = room.LootNodes?.FirstOrDefault(l =>
            l.NodeType.Contains(target, StringComparison.OrdinalIgnoreCase));

        if (lootNode != null)
        {
            return SearchLootNode(state, lootNode);
        }

        // Check Static Terrain (some terrain might be searchable)
        var terrain = room.StaticTerrain?.FirstOrDefault(t =>
            t.TerrainName.Contains(target, StringComparison.OrdinalIgnoreCase) &&
            t.IsSearchable);

        if (terrain != null)
        {
            return SearchTerrain(state, terrain);
        }

        // Not found or not searchable
        _log.Debug("Search target not found or not searchable: {Target}", target);
        return CommandResult.Failure($"You cannot search '{target}' here.");
    }

    /// <summary>
    /// Search a loot node and collect contents
    /// </summary>
    private CommandResult SearchLootNode(GameState state, LootNode lootNode)
    {
        // Check if already looted
        if (lootNode.HasBeenLooted)
        {
            return CommandResult.Failure($"The {lootNode.NodeType} has already been searched and is empty.");
        }

        // Check if hidden content requires investigation first
        if (lootNode.RequiresInvestigation && !lootNode.HiddenContentRevealed)
        {
            // Still allow basic search, but won't find hidden items
            _log.Debug("Loot node requires investigation: {NodeType}", lootNode.NodeType);
        }

        var sb = new StringBuilder();
        sb.AppendLine($"You search the {lootNode.NodeType}...");
        sb.AppendLine();

        // Generate loot using LootService
        var loot = _lootService.GenerateLootForNode(lootNode);

        if (loot == null || (!loot.Equipment.Any() && !loot.Components.Any() && loot.Currency == 0))
        {
            lootNode.HasBeenLooted = true;
            sb.AppendLine("You find nothing of value.");

            _log.Information(
                "Loot node searched (empty): NodeType={NodeType}, RoomId={RoomId}",
                lootNode.NodeType,
                state.CurrentRoom?.RoomId ?? "unknown");

            return CommandResult.Success(sb.ToString());
        }

        // Add items to player inventory
        sb.AppendLine("You find:");

        // Equipment
        if (loot.Equipment.Any())
        {
            foreach (var item in loot.Equipment)
            {
                state.CurrentRoom.ItemsOnGround.Add(item);
                sb.AppendLine($"- {item.Name}");
            }
        }

        // Crafting components
        if (loot.Components.Any())
        {
            foreach (var component in loot.Components)
            {
                if (state.Player.CraftingComponents.ContainsKey(component.Key))
                {
                    state.Player.CraftingComponents[component.Key] += component.Value;
                }
                else
                {
                    state.Player.CraftingComponents[component.Key] = component.Value;
                }

                sb.AppendLine($"- {component.Key} x{component.Value}");
            }
        }

        // Currency
        if (loot.Currency > 0)
        {
            state.Player.Currency += loot.Currency;
            sb.AppendLine($"- {loot.Currency} Scrap (⚙)");
        }

        // Mark as looted
        lootNode.HasBeenLooted = true;

        _log.Information(
            "Loot node searched: NodeType={NodeType}, ItemCount={ItemCount}, Components={ComponentCount}, Currency={Currency}",
            lootNode.NodeType,
            loot.Equipment.Count,
            loot.Components.Count,
            loot.Currency);

        sb.AppendLine();
        sb.AppendLine("Items have been added to your inventory/resources.");

        return CommandResult.Success(sb.ToString(), redrawRoom: true);
    }

    /// <summary>
    /// Search static terrain (less common, but some terrain is searchable)
    /// </summary>
    private CommandResult SearchTerrain(GameState state, StaticTerrain terrain)
    {
        if (!terrain.IsSearchable)
        {
            return CommandResult.Failure($"The {terrain.TerrainName} cannot be searched.");
        }

        if (terrain.HasBeenSearched)
        {
            return CommandResult.Failure($"You have already searched the {terrain.TerrainName}.");
        }

        var sb = new StringBuilder();
        sb.AppendLine($"You search the {terrain.TerrainName}...");
        sb.AppendLine();

        // Check if terrain has loot
        if (terrain.ContainsLoot)
        {
            // Generate simple loot (this is a simplified version)
            // In a full implementation, this would use LootService with terrain-specific loot tables

            sb.AppendLine("You find:");
            sb.AppendLine("- Small amount of scrap materials");

            // Grant small currency reward
            var scrapAmount = 5 + (state.Player?.Attributes?.Wits ?? 0);
            state.Player.Currency += scrapAmount;

            sb.AppendLine($"- {scrapAmount} Scrap (⚙)");

            terrain.HasBeenSearched = true;

            _log.Information(
                "Terrain searched: TerrainName={TerrainName}, Scrap={Scrap}",
                terrain.TerrainName,
                scrapAmount);
        }
        else
        {
            sb.AppendLine("You find nothing of value.");
            terrain.HasBeenSearched = true;
        }

        return CommandResult.Success(sb.ToString());
    }
}

