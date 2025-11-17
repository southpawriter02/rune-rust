using RuneAndRust.Core.Territory;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.35.3: Generates quest templates from territorial events
/// Creates faction-specific quests that respond to world events
/// </summary>
public class TerritorialQuestGenerator
{
    private static readonly ILogger _log = Log.ForContext<TerritorialQuestGenerator>();

    /// <summary>
    /// Generate a quest template from an event
    /// </summary>
    public TerritorialQuestTemplate? GenerateEventQuest(int sectorId, string eventType, string? affectedFaction)
    {
        _log.Debug("Generating quest template for event type {EventType} in sector {SectorId}",
            eventType, sectorId);

        var template = eventType switch
        {
            "Awakening_Ritual" => CreateAwakeningRitualQuest(sectorId, affectedFaction),
            "Excavation_Discovery" => CreateExcavationDiscoveryQuest(sectorId, affectedFaction),
            "Purge_Campaign" => CreatePurgeCampaignQuest(sectorId, affectedFaction),
            "Incursion" => CreateIncursionQuest(sectorId, affectedFaction),
            "Supply_Raid" => CreateSupplyRaidQuest(sectorId, affectedFaction),
            "Catastrophe" => CreateCatastropheQuest(sectorId, affectedFaction),
            "Scavenger_Caravan" => CreateScavengerCaravanQuest(sectorId, affectedFaction),
            _ => null
        };

        if (template != null)
        {
            _log.Information("Quest template generated: {QuestName} for event {EventType}",
                template.QuestName, eventType);
        }
        else
        {
            _log.Debug("No quest template for event type {EventType}", eventType);
        }

        return template;
    }

    /// <summary>
    /// Quest: Disrupt the Awakening Ritual
    /// Objective: Kill God-Sleeper cultists before ritual completes
    /// </summary>
    private TerritorialQuestTemplate CreateAwakeningRitualQuest(int sectorId, string? faction)
    {
        return new TerritorialQuestTemplate
        {
            SectorId = sectorId,
            QuestName = "Disrupt the Awakening Ritual",
            Description = "God-Sleeper cultists are performing a ritual to awaken dormant Jötun-Forged constructs. Stop them before the ritual completes, or the sector will be overrun with awakened machines.",
            ObjectiveType = "KillEnemies",
            ObjectiveCount = 5,
            TargetFaction = "GodSleeperCultists",
            QuestGiver = "Local Settlement Elder",
            RewardGold = 150,
            RewardReputation = 20,
            RewardFaction = "IronBanes", // Opposing faction
            FactionPenalty = "GodSleeperCultists",
            PenaltyAmount = -15,
            TimeLimit = 7
        };
    }

    /// <summary>
    /// Quest: Claim the Pre-Glitch Cache
    /// Objective: Reach excavation site and secure artifacts
    /// </summary>
    private TerritorialQuestTemplate CreateExcavationDiscoveryQuest(int sectorId, string? faction)
    {
        return new TerritorialQuestTemplate
        {
            SectorId = sectorId,
            QuestName = "Claim the Pre-Glitch Cache",
            Description = "Jötun-Readers have discovered a major artifact cache from the Pre-Glitch era. Reach the excavation site and secure valuable technology before they extract everything.",
            ObjectiveType = "ReachLocation",
            ObjectiveCount = 1,
            TargetLocation = "Excavation Site",
            QuestGiver = "Rust-Clan Trader",
            RewardGold = 200,
            RewardReputation = 15,
            RewardFaction = "RustClans",
            RewardItems = new List<string> { "Ancient Data Core", "Pre-Glitch Schematic" },
            FactionPenalty = "JotunReaders",
            PenaltyAmount = -10,
            TimeLimit = 5
        };
    }

    /// <summary>
    /// Quest: Join the Purge
    /// Objective: Hunt Undying alongside Iron-Banes
    /// </summary>
    private TerritorialQuestTemplate CreatePurgeCampaignQuest(int sectorId, string? faction)
    {
        return new TerritorialQuestTemplate
        {
            SectorId = sectorId,
            QuestName = "Join the Purge",
            Description = "Iron-Banes are launching a coordinated hunt against Undying corruption. Join the effort and prove your worth to the Purity Oath.",
            ObjectiveType = "KillEnemies",
            ObjectiveCount = 15,
            TargetFaction = "Undying",
            QuestGiver = "Iron-Bane Commander",
            RewardGold = 250,
            RewardReputation = 25,
            RewardFaction = "IronBanes",
            RewardItems = new List<string> { "Blessed Weapon Oil", "Salt Grenades" },
            TimeLimit = 10
        };
    }

    /// <summary>
    /// Quest: Defend Against Incursion
    /// Objective: Repel faction forces attempting territorial expansion
    /// </summary>
    private TerritorialQuestTemplate CreateIncursionQuest(int sectorId, string? faction)
    {
        return new TerritorialQuestTemplate
        {
            SectorId = sectorId,
            QuestName = $"Defend Against {faction} Incursion",
            Description = $"{faction} forces are pushing to expand their territorial control. Help defend the sector from their incursion.",
            ObjectiveType = "KillEnemies",
            ObjectiveCount = 10,
            TargetFaction = faction,
            QuestGiver = "Settlement Defense Coordinator",
            RewardGold = 180,
            RewardReputation = 20,
            FactionPenalty = faction,
            PenaltyAmount = -20,
            TimeLimit = 3
        };
    }

    /// <summary>
    /// Quest: Recover Raided Supplies
    /// Objective: Track down raiders and recover merchant goods
    /// </summary>
    private TerritorialQuestTemplate CreateSupplyRaidQuest(int sectorId, string? faction)
    {
        return new TerritorialQuestTemplate
        {
            SectorId = sectorId,
            QuestName = "Recover Raided Supplies",
            Description = "Enemy raiders have struck merchant supply lines. Track them down and recover what you can before the goods are lost.",
            ObjectiveType = "KillEnemies",
            ObjectiveCount = 8,
            TargetFaction = "Raiders",
            QuestGiver = "Merchant Guild Representative",
            RewardGold = 120,
            RewardReputation = 15,
            RewardFaction = "RustClans",
            RewardItems = new List<string> { "Scrap Bundle", "Salvaged Components" },
            TimeLimit = 1
        };
    }

    /// <summary>
    /// Quest: Stabilize Reality Corruption
    /// Objective: Destroy corrupted hazards to reduce surge
    /// </summary>
    private TerritorialQuestTemplate CreateCatastropheQuest(int sectorId, string? faction)
    {
        return new TerritorialQuestTemplate
        {
            SectorId = sectorId,
            QuestName = "Stabilize Reality Corruption",
            Description = "A surge in reality corruption has warped the environment. Destroy corrupted hazards to help stabilize the sector before the damage becomes permanent.",
            ObjectiveType = "DestroyHazards",
            ObjectiveCount = 12,
            QuestGiver = "Sector Safety Officer",
            RewardGold = 160,
            RewardReputation = 18,
            RewardItems = new List<string> { "Aether Flask", "Stabilization Kit" },
            TimeLimit = 2
        };
    }

    /// <summary>
    /// Quest: Escort the Scavenger Caravan
    /// Objective: Protect caravan from threats
    /// </summary>
    private TerritorialQuestTemplate CreateScavengerCaravanQuest(int sectorId, string? faction)
    {
        return new TerritorialQuestTemplate
        {
            SectorId = sectorId,
            QuestName = "Escort the Scavenger Caravan",
            Description = "Rust-Clans are establishing a trade route through the sector. Escort their caravan safely to the destination for generous compensation.",
            ObjectiveType = "Defend",
            ObjectiveCount = 1,
            TargetLocation = "Trade Outpost",
            QuestGiver = "Caravan Master",
            RewardGold = 140,
            RewardReputation = 20,
            RewardFaction = "RustClans",
            RewardItems = new List<string> { "Repair Kit", "Mystery Box" },
            TimeLimit = 2
        };
    }

    /// <summary>
    /// Get all available quest templates for a faction
    /// Returns pre-defined templates that can be offered based on sector state
    /// </summary>
    public List<TerritorialQuestTemplate> GetFactionQuestTemplates(string factionName, int sectorId)
    {
        var templates = new List<TerritorialQuestTemplate>();

        switch (factionName)
        {
            case "IronBanes":
                templates.Add(CreateIronBanesPatrolQuest(sectorId));
                templates.Add(CreateIronBanesInvestigationQuest(sectorId));
                break;

            case "JotunReaders":
                templates.Add(CreateJotunReadersResearchQuest(sectorId));
                templates.Add(CreateJotunReadersRecoveryQuest(sectorId));
                break;

            case "GodSleeperCultists":
                templates.Add(CreateGodSleepersRitualQuest(sectorId));
                break;

            case "RustClans":
                templates.Add(CreateRustClansSalvageQuest(sectorId));
                templates.Add(CreateRustClansTradeQuest(sectorId));
                break;
        }

        _log.Debug("Generated {Count} quest templates for faction {FactionName}",
            templates.Count, factionName);

        return templates;
    }

    // Faction-specific quest templates

    private TerritorialQuestTemplate CreateIronBanesPatrolQuest(int sectorId)
    {
        return new TerritorialQuestTemplate
        {
            SectorId = sectorId,
            QuestName = "Patrol Duty",
            Description = "Join Iron-Banes patrol to clear Undying threats from sector routes.",
            ObjectiveType = "KillEnemies",
            ObjectiveCount = 10,
            TargetFaction = "Undying",
            QuestGiver = "Iron-Bane Patrol Leader",
            RewardGold = 100,
            RewardReputation = 15,
            RewardFaction = "IronBanes",
            TimeLimit = 5
        };
    }

    private TerritorialQuestTemplate CreateIronBanesInvestigationQuest(int sectorId)
    {
        return new TerritorialQuestTemplate
        {
            SectorId = sectorId,
            QuestName = "Investigate Corruption Source",
            Description = "Iron-Banes need help investigating a potential Runic Blight source.",
            ObjectiveType = "ReachLocation",
            ObjectiveCount = 1,
            TargetLocation = "Corrupted Zone",
            QuestGiver = "Iron-Bane Investigator",
            RewardGold = 120,
            RewardReputation = 18,
            RewardFaction = "IronBanes",
            TimeLimit = 7
        };
    }

    private TerritorialQuestTemplate CreateJotunReadersResearchQuest(int sectorId)
    {
        return new TerritorialQuestTemplate
        {
            SectorId = sectorId,
            QuestName = "Recover Research Data",
            Description = "Jötun-Readers need data cores from abandoned research facilities.",
            ObjectiveType = "CollectItems",
            ObjectiveCount = 5,
            TargetLocation = "Research Facility",
            QuestGiver = "Jötun-Reader Scholar",
            RewardGold = 150,
            RewardReputation = 20,
            RewardFaction = "JotunReaders",
            RewardItems = new List<string> { "Ancient Schematic" },
            TimeLimit = 7
        };
    }

    private TerritorialQuestTemplate CreateJotunReadersRecoveryQuest(int sectorId)
    {
        return new TerritorialQuestTemplate
        {
            SectorId = sectorId,
            QuestName = "Artifact Recovery",
            Description = "Retrieve Pre-Glitch artifacts for Jötun-Reader analysis.",
            ObjectiveType = "ReachLocation",
            ObjectiveCount = 1,
            TargetLocation = "Ancient Vault",
            QuestGiver = "Jötun-Reader Expedition Leader",
            RewardGold = 180,
            RewardReputation = 22,
            RewardFaction = "JotunReaders",
            RewardItems = new List<string> { "Data Core Fragment", "Pre-Glitch Tool Kit" },
            TimeLimit = 5
        };
    }

    private TerritorialQuestTemplate CreateGodSleepersRitualQuest(int sectorId)
    {
        return new TerritorialQuestTemplate
        {
            SectorId = sectorId,
            QuestName = "Ritual Components",
            Description = "Gather components needed for God-Sleeper awakening rituals.",
            ObjectiveType = "CollectItems",
            ObjectiveCount = 8,
            TargetLocation = "Ritual Site",
            QuestGiver = "Cult Ritualist",
            RewardGold = 140,
            RewardReputation = 20,
            RewardFaction = "GodSleeperCultists",
            RewardItems = new List<string> { "Aether Flask", "Corrupted Talisman" },
            FactionPenalty = "IronBanes",
            PenaltyAmount = -15,
            TimeLimit = 7
        };
    }

    private TerritorialQuestTemplate CreateRustClansSalvageQuest(int sectorId)
    {
        return new TerritorialQuestTemplate
        {
            SectorId = sectorId,
            QuestName = "Salvage Operation",
            Description = "Help Rust-Clans salvage components from abandoned sectors.",
            ObjectiveType = "CollectItems",
            ObjectiveCount = 20,
            TargetLocation = "Salvage Zone",
            QuestGiver = "Rust-Clan Foreman",
            RewardGold = 110,
            RewardReputation = 15,
            RewardFaction = "RustClans",
            RewardItems = new List<string> { "Scrap Bundle", "Salvaged Plating" },
            TimeLimit = 5
        };
    }

    private TerritorialQuestTemplate CreateRustClansTradeQuest(int sectorId)
    {
        return new TerritorialQuestTemplate
        {
            SectorId = sectorId,
            QuestName = "Establish Trade Route",
            Description = "Escort Rust-Clan traders to establish new trade connections.",
            ObjectiveType = "Defend",
            ObjectiveCount = 1,
            TargetLocation = "Trade Hub",
            QuestGiver = "Trade Master",
            RewardGold = 130,
            RewardReputation = 18,
            RewardFaction = "RustClans",
            RewardItems = new List<string> { "Repair Kit", "Traveler's Supplies" },
            TimeLimit = 3
        };
    }
}
