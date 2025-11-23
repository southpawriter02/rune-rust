using RuneAndRust.Core;
using PopulationLootNode = RuneAndRust.Core.Population.LootNode;
using PopulationDynamicHazard = RuneAndRust.Core.Population.DynamicHazard;
using RuneAndRust.Core.Population;
using Serilog;
using System.Text;

namespace RuneAndRust.Engine.Commands;

/// <summary>
/// v0.37.1: Look Command
/// Primary perception command for displaying room descriptions and examining objects.
/// Syntax: look or look at [target]
/// Aliases: l, examine, x
/// </summary>
public class LookCommand : ICommand
{
    private static readonly ILogger _log = Log.ForContext<LookCommand>();
    private readonly ExaminationFlavorTextService? _flavorTextService;

    public LookCommand(ExaminationFlavorTextService? flavorTextService = null)
    {
        _flavorTextService = flavorTextService;
    }

    public CommandResult Execute(GameState state, string[] args)
    {
        _log.Information(
            "Look command executed: CharacterID={CharacterID}, Target={Target}, RoomId={RoomId}",
            state.Player?.CharacterID ?? 0,
            args.Length > 0 ? string.Join(" ", args) : "(none)",
            state.CurrentRoom?.RoomId ?? "unknown");

        try
        {
            // "look" with no arguments - display full room description
            if (args.Length == 0)
            {
                var description = DescribeRoom(state);
                _log.Debug(
                    "Look command completed: Success=True, OutputLength={Length}",
                    description.Length);
                return CommandResult.Success(description);
            }

            // "look at [target]" - examine specific object
            var target = string.Join(" ", args).Replace("at ", "").Trim();
            var examination = ExamineTarget(state, target);

            _log.Debug(
                "Look command (examine) completed: Target={Target}, Found={Found}",
                target,
                !examination.Contains("don't see"));

            return CommandResult.Success(examination);
        }
        catch (Exception ex)
        {
            _log.Error(ex,
                "Look command failed: CharacterID={CharacterID}, Error={ErrorType}",
                state.Player?.CharacterID ?? 0,
                ex.GetType().Name);
            return CommandResult.Failure("An error occurred while looking around.");
        }
    }

    /// <summary>
    /// Generate full room description with exits, objects, enemies, NPCs, etc.
    /// </summary>
    private string DescribeRoom(GameState state)
    {
        var room = state.CurrentRoom;
        if (room == null)
        {
            return "You are nowhere. This should not happen.";
        }

        var sb = new StringBuilder();

        // Room name and description
        sb.AppendLine();
        sb.AppendLine($"=== {room.Name} ===");
        sb.AppendLine(room.Description);
        sb.AppendLine();

        // [v0.38.9] Add flora/fauna atmosphere
        if (_flavorTextService != null && !string.IsNullOrEmpty(room.BiomeName))
        {
            // Randomly show flora or fauna (50% chance each)
            var random = new Random();
            if (random.Next(100) < 30) // 30% chance to show flora
            {
                var floraObservation = _flavorTextService.GenerateFloraObservation(
                    biomeName: room.BiomeName,
                    floraName: null,
                    witsCheck: 0);

                if (!string.IsNullOrEmpty(floraObservation))
                {
                    sb.AppendLine($"[Flora] {floraObservation}");
                    sb.AppendLine();
                }
            }

            if (random.Next(100) < 20) // 20% chance to show fauna
            {
                var faunaObservation = _flavorTextService.GetRandomAmbientCreature(room.BiomeName);

                if (!string.IsNullOrEmpty(faunaObservation))
                {
                    sb.AppendLine($"[Fauna] {faunaObservation}");
                    sb.AppendLine();
                }
            }
        }

        // Exits
        if (room.Exits.Any())
        {
            var exitsList = string.Join(", ", room.Exits.Select(e => $"{e.Key} ({e.Value})"));
            sb.AppendLine($"Exits: {exitsList}");
        }
        else
        {
            sb.AppendLine("Exits: None (you are trapped!)");
        }

        // Interactive Objects (from Population system)
        var interactiveObjects = new List<string>();

        // Static Terrain
        if (room.StaticTerrain?.Any() == true)
        {
            foreach (var terrain in room.StaticTerrain)
            {
                interactiveObjects.Add($"[{terrain.TerrainName}]");
            }
        }

        // Loot Nodes (containers, corpses, etc.)
        if (room.LootNodes?.Any() == true)
        {
            foreach (var loot in room.LootNodes.Where(l => !l.HasBeenLooted))
            {
                interactiveObjects.Add($"[{loot.NodeType}]");
            }
        }

        if (interactiveObjects.Any())
        {
            sb.AppendLine($"Objects: {string.Join(", ", interactiveObjects)}");
        }

        // Items on ground
        if (room.ItemsOnGround?.Any() == true)
        {
            sb.AppendLine($"Items: {string.Join(", ", room.ItemsOnGround.Select(i => i.Name))}");
        }

        // Enemies
        if (room.Enemies?.Any() == true)
        {
            var enemyList = string.Join(", ", room.Enemies.Select(e =>
            {
                var status = e.CurrentHP < e.MaxHP * 0.3 ? " (wounded)" :
                             e.CurrentHP < e.MaxHP * 0.7 ? " (injured)" : "";
                return $"{e.Name}{status}";
            }));
            sb.AppendLine($"Enemies: {enemyList}");
        }

        // NPCs
        if (room.NPCs?.Any() == true)
        {
            var npcList = string.Join(", ", room.NPCs.Select(n => n.Name));
            sb.AppendLine($"NPCs: {npcList}");
        }

        // Dynamic Hazards
        if (room.DynamicHazards?.Any() == true)
        {
            var hazardList = string.Join(", ", room.DynamicHazards
                .Where(h => h.IsActive)
                .Select(h => $"[{h.HazardName}]"));
            if (!string.IsNullOrEmpty(hazardList))
            {
                sb.AppendLine($"Hazards: {hazardList}");
            }
        }

        // Ambient Conditions
        if (room.AmbientConditions?.Any() == true)
        {
            var conditionList = string.Join(", ", room.AmbientConditions
                .Where(c => c.IsActive)
                .Select(c => c.ConditionName));
            if (!string.IsNullOrEmpty(conditionList))
            {
                sb.AppendLine($"Conditions: {conditionList}");
            }
        }

        // Sanctuary status
        if (room.IsSanctuary)
        {
            sb.AppendLine();
            sb.AppendLine("This is a SANCTUARY. You can rest here safely.");
        }

        // Puzzle status
        if (room.HasPuzzle && !room.IsPuzzleSolved)
        {
            sb.AppendLine();
            sb.AppendLine($"Puzzle: {room.PuzzleDescription}");
        }

        // [v0.38.9] Passive perception checks for hidden elements
        if (_flavorTextService != null && state.Player?.Attributes?.Wits != null)
        {
            var random = new Random();
            var passivePerceptionDC = 12; // Standard DC for passive perception

            // Roll passive perception (simplified: Wits attribute determines chance)
            var perceptionRoll = random.Next(1, 21) + state.Player.Attributes.Wits;

            if (perceptionRoll >= passivePerceptionDC)
            {
                // Check for hidden traps
                var hiddenTraps = room.DynamicHazards?
                    .Where(h => h.IsActive && h.IsHidden && !h.HasBeenDiscovered)
                    .ToList();

                if (hiddenTraps?.Any() == true && random.Next(100) < 40) // 40% chance to notice
                {
                    var trap = hiddenTraps.First();
                    trap.HasBeenDiscovered = true;

                    var perceptionText = _flavorTextService.GeneratePerceptionCheckText(
                        detectionType: "HiddenTrap",
                        checkResult: perceptionRoll,
                        biomeName: room.BiomeName);

                    sb.AppendLine();
                    sb.AppendLine($"[Perception Check: {perceptionRoll} vs DC {passivePerceptionDC}] SUCCESS!");
                    sb.AppendLine(perceptionText);
                }
                // Check for secret doors/passages
                else if (room.HasSecretExit && !room.SecretExitRevealed && random.Next(100) < 25) // 25% chance
                {
                    room.SecretExitRevealed = true;

                    var perceptionText = _flavorTextService.GeneratePerceptionCheckText(
                        detectionType: "SecretDoor",
                        checkResult: perceptionRoll,
                        biomeName: room.BiomeName);

                    sb.AppendLine();
                    sb.AppendLine($"[Perception Check: {perceptionRoll} vs DC {passivePerceptionDC}] SUCCESS!");
                    sb.AppendLine(perceptionText);
                }
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Examine a specific target (object, enemy, NPC, etc.)
    /// </summary>
    private string ExamineTarget(GameState state, string target)
    {
        var room = state.CurrentRoom;
        if (room == null)
        {
            return "You cannot examine anything here.";
        }

        var targetLower = target.ToLower().Trim('[', ']');

        // Check enemies
        var enemy = room.Enemies?.FirstOrDefault(e =>
            e.Name.Contains(targetLower, StringComparison.OrdinalIgnoreCase) ||
            e.Id.Contains(targetLower, StringComparison.OrdinalIgnoreCase));

        if (enemy != null)
        {
            return DescribeEnemy(enemy);
        }

        // Check NPCs
        var npc = room.NPCs?.FirstOrDefault(n =>
            n.Name.Contains(targetLower, StringComparison.OrdinalIgnoreCase));

        if (npc != null)
        {
            return DescribeNPC(npc);
        }

        // Check Static Terrain
        var terrain = room.StaticTerrain?.FirstOrDefault(t =>
            t.TerrainName.Contains(targetLower, StringComparison.OrdinalIgnoreCase));

        if (terrain != null)
        {
            return DescribeTerrain(terrain);
        }

        // Check Loot Nodes
        var lootNode = room.LootNodes?.FirstOrDefault(l =>
            l.NodeType.Contains(targetLower, StringComparison.OrdinalIgnoreCase));

        if (lootNode != null)
        {
            return DescribeLootNode(lootNode);
        }

        // Check Dynamic Hazards
        var hazard = room.DynamicHazards?.FirstOrDefault(h =>
            h.HazardName.Contains(targetLower, StringComparison.OrdinalIgnoreCase));

        if (hazard != null)
        {
            return DescribeHazard(hazard);
        }

        // Check items on ground
        var item = room.ItemsOnGround?.FirstOrDefault(i =>
            i.Name.Contains(targetLower, StringComparison.OrdinalIgnoreCase));

        if (item != null)
        {
            return DescribeItem(item);
        }

        // Not found
        _log.Debug("Examine target not found: {Target} in room {RoomId}", target, room.RoomId);
        return $"You don't see '{target}' here.";
    }

    private string DescribeEnemy(Enemy enemy)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"=== {enemy.Name} ===");
        sb.AppendLine($"HP: {enemy.CurrentHP}/{enemy.MaxHP}");
        sb.AppendLine($"Threat Level: {(enemy.IsBoss ? "BOSS" : enemy.Tier)}");

        if (!string.IsNullOrEmpty(enemy.FlavorText))
        {
            sb.AppendLine();
            sb.AppendLine(enemy.FlavorText);
        }

        return sb.ToString();
    }

    private string DescribeNPC(NPC npc)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"=== {npc.Name} ===");
        sb.AppendLine($"Faction: {npc.Faction}");

        if (!string.IsNullOrEmpty(npc.Description))
        {
            sb.AppendLine();
            sb.AppendLine(npc.Description);
        }

        sb.AppendLine();
        sb.AppendLine("Use 'talk' to speak with this character.");

        return sb.ToString();
    }

    private string DescribeTerrain(StaticTerrain terrain)
    {
        return $"{terrain.TerrainName}: {terrain.FlavorText ?? "A static terrain feature."}";
    }

    private string DescribeLootNode(PopulationLootNode lootNode)
    {
        if (lootNode.HasBeenLooted)
        {
            return $"{lootNode.NodeType}: Already searched.";
        }

        return $"{lootNode.NodeType}: {lootNode.FlavorText ?? "You could search this."}\nUse 'search {lootNode.NodeType}' to investigate.";
    }

    private string DescribeHazard(PopulationDynamicHazard hazard)
    {
        var status = hazard.IsActive ? "ACTIVE" : "Disabled";
        return $"{hazard.HazardName} ({status}): {hazard.FlavorText ?? "A dangerous hazard."}";
    }

    private string DescribeItem(Equipment item)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"=== {item.Name} ===");
        sb.AppendLine($"Type: {item.Slot}");
        sb.AppendLine($"Tier: {item.Tier}");

        if (item.Slot == "Weapon")
        {
            sb.AppendLine($"Damage: {item.DamageDice}d{item.DamageDieSize}");
        }
        else if (item.Slot == "Armor")
        {
            sb.AppendLine($"Defense: {item.DefenseBonus}");
        }

        sb.AppendLine();
        sb.AppendLine("Use 'pickup' to take this item.");

        return sb.ToString();
    }
}
