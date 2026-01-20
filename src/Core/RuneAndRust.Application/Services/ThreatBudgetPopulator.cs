using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Populates rooms with entities using a threat budget system.
/// Budget allocation: Elite chance → Grunts → Swarm fodder for change.
/// </summary>
public class ThreatBudgetPopulator : IEntityPopulator
{
    private const double EliteSpawnChance = 0.20; // 20% chance for elite
    private const double EliteBudgetPercent = 0.50; // Elite takes 50% of budget
    private const int MinConnectionsForPopulation = 2; // Don't populate dead ends

    private readonly IReadOnlyList<EntityTemplate> _templates;
    private readonly Dictionary<Biome, string> _biomeFactions;

    public ThreatBudgetPopulator(IEntityTemplateProvider templateProvider)
    {
        _templates = templateProvider?.GetAllTemplates()
            ?? throw new ArgumentNullException(nameof(templateProvider));

        _biomeFactions = new Dictionary<Biome, string>
        {
            [Biome.Citadel] = "undead_legion",
            [Biome.TheRoots] = "rust_swarm",
            [Biome.Muspelheim] = "fire_cult",
            [Biome.Niflheim] = "frost_horrors",
            [Biome.Jotunheim] = "awakened_titans"
        };
    }

    public void PopulateRoom(
        Room room,
        DungeonNode node,
        int threatBudget,
        string factionId,
        Biome biome,
        Random random)
    {
        // Skip start rooms and rooms with insufficient exits
        if (node.IsStartNode || node.GetConnectionCount() < MinConnectionsForPopulation)
            return;

        // Boss rooms get special handling
        if (node.IsBossArena)
        {
            PopulateBossRoom(room, factionId, biome, random);
            return;
        }

        var remainingBudget = threatBudget;

        // 20% chance for an elite unit (takes 50% of budget)
        if (random.NextDouble() < EliteSpawnChance)
        {
            var eliteBudget = (int)(threatBudget * EliteBudgetPercent);
            var elite = SelectEntity(factionId, biome, EntityRole.Elite, eliteBudget);

            if (elite != null && elite.CanAfford(eliteBudget))
            {
                room.AddMonster(elite.CreateMonster());
                remainingBudget -= elite.Cost;
            }
        }

        // Fill remaining budget with grunts
        SpawnGrunts(room, factionId, biome, remainingBudget, random);
    }

    public void PopulateSector(
        Sector sector,
        IReadOnlyDictionary<Guid, Room> rooms,
        Random random)
    {
        var factionId = GetFactionForBiome(sector.Biome);
        var roomCount = rooms.Count;

        // Distribute threat budget across rooms (excluding start and boss)
        var normalRoomCount = sector.GetAllNodes()
            .Count(n => !n.IsStartNode && !n.IsBossArena && n.GetConnectionCount() >= MinConnectionsForPopulation);

        var budgetPerRoom = normalRoomCount > 0
            ? sector.ThreatBudget / normalRoomCount
            : 0;

        foreach (var node in sector.GetAllNodes())
        {
            if (!rooms.TryGetValue(node.Id, out var room))
                continue;

            // Calculate room-specific budget with some variance
            var variance = random.Next(-budgetPerRoom / 4, budgetPerRoom / 4 + 1);
            var roomBudget = Math.Max(0, budgetPerRoom + variance);

            PopulateRoom(room, node, roomBudget, factionId, sector.Biome, random);
        }
    }

    public string GetFactionForBiome(Biome biome)
    {
        return _biomeFactions.TryGetValue(biome, out var faction)
            ? faction
            : "undead_legion"; // Default fallback
    }

    private void PopulateBossRoom(Room room, string factionId, Biome biome, Random random)
    {
        // Spawn the boss
        var boss = SelectEntity(factionId, biome, EntityRole.Boss, int.MaxValue);
        if (boss != null)
        {
            room.AddMonster(boss.CreateMonster());
        }

        // Add some supporting units (20-40% of boss cost as adds)
        if (boss != null)
        {
            var addsBudget = (int)(boss.Cost * (0.2 + random.NextDouble() * 0.2));
            SpawnGrunts(room, factionId, biome, addsBudget, random);
        }
    }

    private void SpawnGrunts(Room room, string factionId, Biome biome, int budget, Random random)
    {
        var remainingBudget = budget;
        var attempts = 0;
        const int maxAttempts = 20;

        while (remainingBudget > 0 && attempts < maxAttempts)
        {
            attempts++;

            // Get affordable grunts
            var candidates = GetAffordableEntities(factionId, biome, remainingBudget,
                [EntityRole.Melee, EntityRole.Ranged, EntityRole.Tank]);

            if (candidates.Count == 0)
            {
                // Try swarm units for remaining "change"
                var swarm = SelectEntity(factionId, biome, EntityRole.Swarm, remainingBudget);
                if (swarm != null && swarm.CanAfford(remainingBudget))
                {
                    room.AddMonster(swarm.CreateMonster());
                    remainingBudget -= swarm.Cost;
                }
                else
                {
                    break; // Can't afford anything
                }
            }
            else
            {
                // Select weighted random grunt
                var grunt = SelectWeighted(candidates, random);
                room.AddMonster(grunt.CreateMonster());
                remainingBudget -= grunt.Cost;
            }
        }

        // Spend remaining change on swarm if possible
        while (remainingBudget > 0)
        {
            var swarm = SelectEntity(factionId, biome, EntityRole.Swarm, remainingBudget);
            if (swarm == null || !swarm.CanAfford(remainingBudget))
                break;

            room.AddMonster(swarm.CreateMonster());
            remainingBudget -= swarm.Cost;
        }
    }

    private EntityTemplate? SelectEntity(string factionId, Biome biome, EntityRole role, int maxCost)
    {
        return _templates
            .Where(t => t.BelongsToFaction(factionId))
            .Where(t => t.IsCompatibleWithBiome(biome))
            .Where(t => t.Role == role)
            .Where(t => t.CanAfford(maxCost))
            .OrderByDescending(t => t.Cost)
            .FirstOrDefault();
    }

    private List<EntityTemplate> GetAffordableEntities(string factionId, Biome biome, int maxCost, EntityRole[] roles)
    {
        return _templates
            .Where(t => t.BelongsToFaction(factionId))
            .Where(t => t.IsCompatibleWithBiome(biome))
            .Where(t => roles.Contains(t.Role))
            .Where(t => t.CanAfford(maxCost))
            .ToList();
    }

    private static EntityTemplate SelectWeighted(IReadOnlyList<EntityTemplate> templates, Random random)
    {
        // Inverse weight by cost - cheaper units are more common
        var maxCost = templates.Max(t => t.Cost);
        var weights = templates.Select(t => maxCost - t.Cost + 1).ToList();
        var totalWeight = weights.Sum();

        var roll = random.Next(totalWeight);
        var cumulative = 0;

        for (var i = 0; i < templates.Count; i++)
        {
            cumulative += weights[i];
            if (roll < cumulative)
                return templates[i];
        }

        return templates[^1];
    }
}
