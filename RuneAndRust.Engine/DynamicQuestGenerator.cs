using RuneAndRust.Core;
using RuneAndRust.Core.Quests;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.14: Generates dynamic quests based on generated dungeon content
/// Creates adaptive quests like "Clear this sector" or "Collect salvage"
/// </summary>
public class DynamicQuestGenerator
{
    private static readonly ILogger _log = Log.ForContext<DynamicQuestGenerator>();
    private readonly Random _rng;

    public DynamicQuestGenerator(Random? rng = null)
    {
        _rng = rng ?? new Random();
    }

    /// <summary>
    /// Generates a "clear sector" quest - defeat all enemies in a dungeon
    /// </summary>
    public Quest GenerateClearSectorQuest(DungeonGraph dungeon, int dungeonSeed)
    {
        var quest = new Quest
        {
            Id = $"dynamic_clear_{dungeonSeed}",
            Title = $"Clear Sector: Depth {dungeonSeed % 10 + 1}",
            Description = "Eliminate all hostile Dormant Processes in this sector. " +
                         "The sector has been overrun and must be secured for salvage operations.",
            GiverNpcId = "system",
            GiverNpcName = "System Directive",
            Type = QuestType.Dynamic,
            Category = QuestCategory.Combat,
            Status = QuestStatus.Available,
            EstimatedDuration = 20,
            Objectives = new List<QuestObjective>(),
            Reward = CalculateClearRewards(dungeon)
        };

        // Count enemy types (this is simplified - in practice would scan dungeon rooms)
        // For v0.14, we'll create a generic "clear all enemies" objective
        var totalRooms = dungeon.GetNodes().Count();
        var estimatedEnemies = totalRooms * 2; // Rough estimate

        quest.Objectives.Add(new QuestObjective
        {
            Description = "Defeat all hostile processes",
            Type = ObjectiveType.KillEnemy,
            TargetId = "any", // Any enemy type
            Required = estimatedEnemies,
            Current = 0
        });

        _log.Information("Generated dynamic clear quest: QuestId={QuestId}, EstimatedEnemies={Count}",
            quest.Id, estimatedEnemies);

        return quest;
    }

    /// <summary>
    /// Generates a collection quest - gather salvage materials
    /// </summary>
    public Quest GenerateCollectionQuest(DungeonGraph dungeon, int dungeonSeed, string biomeId)
    {
        var quest = new Quest
        {
            Id = $"dynamic_collect_{dungeonSeed}",
            Title = $"Salvage Operation: {GetBiomeName(biomeId)}",
            Description = "Recover valuable salvage from the ruins. " +
                         "Pre-Glitch components are essential for repairs and trade.",
            GiverNpcId = "dvergr_quartermaster",
            GiverNpcName = "Dvergr Quartermaster",
            Type = QuestType.Dynamic,
            Category = QuestCategory.Retrieval,
            Status = QuestStatus.Available,
            EstimatedDuration = 15,
            Objectives = new List<QuestObjective>(),
            Reward = CalculateCollectRewards(dungeon)
        };

        // Collection objectives
        var totalRooms = dungeon.GetNodes().Count();
        var salvageCount = Math.Max(5, totalRooms / 2);

        quest.Objectives.Add(new QuestObjective
        {
            Description = "Collect salvage components",
            Type = ObjectiveType.CollectItem,
            TargetId = "scrap_metal", // Generic salvage item
            Required = salvageCount,
            Current = 0
        });

        _log.Information("Generated dynamic collection quest: QuestId={QuestId}, RequiredItems={Count}",
            quest.Id, salvageCount);

        return quest;
    }

    /// <summary>
    /// Generates an exploration quest - discover all rooms in a sector
    /// </summary>
    public Quest GenerateExplorationQuest(DungeonGraph dungeon, int dungeonSeed, string biomeId)
    {
        var quest = new Quest
        {
            Id = $"dynamic_explore_{dungeonSeed}",
            Title = $"Reconnaissance: {GetBiomeName(biomeId)}",
            Description = "Map the sector and identify key locations. " +
                         "Information is as valuable as salvage in Aethelgard's depths.",
            GiverNpcId = "dvergr_cartographer",
            GiverNpcName = "Dvergr Cartographer",
            Type = QuestType.Dynamic,
            Category = QuestCategory.Exploration,
            Status = QuestStatus.Available,
            EstimatedDuration = 25,
            Objectives = new List<QuestObjective>(),
            Reward = CalculateExploreRewards(dungeon)
        };

        // Create exploration objectives for key rooms
        var nodes = dungeon.GetNodes().ToList();
        var keyRooms = nodes.Where(n => n.Type == NodeType.Boss ||
                                        n.Type == NodeType.Main ||
                                        n.Type == NodeType.Secret).ToList();

        foreach (var room in keyRooms.Take(5)) // Limit to 5 key rooms
        {
            quest.Objectives.Add(new QuestObjective
            {
                Description = $"Explore {room.Name}",
                Type = ObjectiveType.ExploreRoom,
                TargetId = room.Id.ToString(),
                Required = 1,
                Current = 0
            });
        }

        // If no key rooms, require exploring all rooms
        if (quest.Objectives.Count == 0)
        {
            quest.Objectives.Add(new QuestObjective
            {
                Description = "Explore all sector rooms",
                Type = ObjectiveType.ExploreRoom,
                TargetId = "all",
                Required = nodes.Count,
                Current = 0
            });
        }

        _log.Information("Generated dynamic exploration quest: QuestId={QuestId}, Rooms={Count}",
            quest.Id, quest.Objectives.Count);

        return quest;
    }

    /// <summary>
    /// Calculates rewards for clearing a sector
    /// </summary>
    private QuestReward CalculateClearRewards(DungeonGraph dungeon)
    {
        var roomCount = dungeon.GetNodes().Count();
        var baseXP = roomCount * 50;
        var baseCogs = roomCount * 25;

        return new QuestReward
        {
            Experience = baseXP,
            Currency = baseCogs,
            ReputationGains = new Dictionary<string, int>
            {
                { "Dvergr", 10 }
            }
        };
    }

    /// <summary>
    /// Calculates rewards for collection quest
    /// </summary>
    private QuestReward CalculateCollectRewards(DungeonGraph dungeon)
    {
        var roomCount = dungeon.GetNodes().Count();
        var baseXP = roomCount * 30;
        var baseCogs = roomCount * 40; // Better currency reward for salvage

        return new QuestReward
        {
            Experience = baseXP,
            Currency = baseCogs,
            ReputationGains = new Dictionary<string, int>
            {
                { "Dvergr", 15 } // Better reputation for salvage
            }
        };
    }

    /// <summary>
    /// Calculates rewards for exploration quest
    /// </summary>
    private QuestReward CalculateExploreRewards(DungeonGraph dungeon)
    {
        var roomCount = dungeon.GetNodes().Count();
        var baseXP = roomCount * 40;
        var baseCogs = roomCount * 20;

        return new QuestReward
        {
            Experience = baseXP,
            Currency = baseCogs,
            ReputationGains = new Dictionary<string, int>
            {
                { "Dvergr", 12 }
            }
        };
    }

    /// <summary>
    /// Gets a friendly name for a biome
    /// </summary>
    private string GetBiomeName(string biomeId)
    {
        return biomeId switch
        {
            "the_roots" => "The Roots",
            "iron_catacombs" => "Iron Catacombs",
            "forgotten_archives" => "Forgotten Archives",
            "glitched_depths" => "Glitched Depths",
            _ => "Unknown Sector"
        };
    }

    /// <summary>
    /// Generates a random appropriate quest for a dungeon
    /// </summary>
    public Quest GenerateRandomQuest(DungeonGraph dungeon, int dungeonSeed, string biomeId)
    {
        var questType = _rng.Next(3);
        return questType switch
        {
            0 => GenerateClearSectorQuest(dungeon, dungeonSeed),
            1 => GenerateCollectionQuest(dungeon, dungeonSeed, biomeId),
            2 => GenerateExplorationQuest(dungeon, dungeonSeed, biomeId),
            _ => GenerateClearSectorQuest(dungeon, dungeonSeed)
        };
    }
}
