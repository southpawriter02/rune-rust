using Microsoft.Data.Sqlite;
using Serilog;
using System.Text.Json;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.41: Seeder for meta-progression content
/// Populates database with achievements, unlocks, cosmetics, milestone tiers, and alternative starts
/// </summary>
public class MetaProgressionSeeder
{
    private static readonly ILogger _log = Log.ForContext<MetaProgressionSeeder>();
    private readonly string _connectionString;

    public MetaProgressionSeeder(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Seed all meta-progression content
    /// </summary>
    public void SeedAll()
    {
        _log.Information("Seeding meta-progression content");

        SeedMilestoneTiers();
        SeedAccountUnlocks();
        SeedAchievements();
        SeedCosmetics();
        SeedAlternativeStarts();

        _log.Information("Meta-progression content seeded successfully");
    }

    #region Milestone Tiers

    private void SeedMilestoneTiers()
    {
        _log.Debug("Seeding milestone tiers");

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var tiers = new[]
        {
            new
            {
                TierNumber = 1,
                TierName = "Initiate",
                Description = "Complete the tutorial and begin your journey",
                RequiredPoints = 0,
                UnlockRewards = new[] { "skip_tutorial" },
                CosmeticRewards = new[] { "title_initiate", "portrait_basic_1", "portrait_basic_2" },
                AlternativeStart = (string?)null
            },
            new
            {
                TierNumber = 2,
                TierName = "Survivor",
                Description = "Complete your first campaign",
                RequiredPoints = 50,
                UnlockRewards = new[] { "veterans_start" },
                CosmeticRewards = new[] { "title_survivor", "title_wanderer", "portrait_survivor", "portrait_veteran", "ui_theme_muspelheim" },
                AlternativeStart = "veterans_return"
            },
            new
            {
                TierNumber = 3,
                TierName = "Veteran",
                Description = "Master the basics and beat New Game+1",
                RequiredPoints = 150,
                UnlockRewards = new[] { "legend_boost_5" },
                CosmeticRewards = new[] { "title_veteran", "title_battle_tested", "title_scarred", "ability_vfx_fire_blue", "ability_vfx_fire_green", "ability_vfx_lightning_red" },
                AlternativeStart = (string?)null
            },
            new
            {
                TierNumber = 4,
                TierName = "Challenger",
                Description = "Complete 5 Challenge Sectors",
                RequiredPoints = 300,
                UnlockRewards = new string[] { },
                CosmeticRewards = new[] { "ui_theme_niflheim", "title_challenger", "title_sector_master", "title_puzzle_solver", "ability_vfx_frost_shards", "ability_vfx_smoke" },
                AlternativeStart = "challenge_seeker"
            },
            new
            {
                TierNumber = 5,
                TierName = "Champion",
                Description = "Beat the Boss Gauntlet",
                RequiredPoints = 500,
                UnlockRewards = new[] { "extra_loadout_slot" },
                CosmeticRewards = new[] { "ui_theme_dark_mode", "title_champion", "title_gauntlet_champion", "title_boss_slayer", "portrait_champion", "frame_gold_1" },
                AlternativeStart = (string?)null
            },
            new
            {
                TierNumber = 6,
                TierName = "Legend",
                Description = "Reach level 20 and ascend to legendary status",
                RequiredPoints = 750,
                UnlockRewards = new[] { "starting_resources_50" },
                CosmeticRewards = new[] { "title_legend", "title_legendary_hero", "portrait_legendary", "emblem_legend_1", "emblem_legend_2", "frame_gold_2", "ability_vfx_golden_glow" },
                AlternativeStart = (string?)null
            },
            new
            {
                TierNumber = 7,
                TierName = "Master",
                Description = "Beat New Game+3 and prove your mastery",
                RequiredPoints = 1000,
                UnlockRewards = new[] { "advanced_spec_unlock" },
                CosmeticRewards = new[] { "ui_theme_alfheim", "title_master", "title_transcendent", "portrait_master", "ability_vfx_pack_1" },
                AlternativeStart = "advanced_explorer"
            },
            new
            {
                TierNumber = 8,
                TierName = "Conqueror",
                Description = "Complete 15 Challenge Sectors and conquer the wastes",
                RequiredPoints = 1500,
                UnlockRewards = new[] { "crafting_mastery" },
                CosmeticRewards = new[] { "title_conqueror", "title_wasteland_king", "ability_vfx_pack_2", "frame_legendary_1", "emblem_conqueror" },
                AlternativeStart = (string?)null
            },
            new
            {
                TierNumber = 9,
                TierName = "Immortal",
                Description = "Reach wave 30 in Endless Mode",
                RequiredPoints = 2000,
                UnlockRewards = new[] { "bestiary_auto_complete" },
                CosmeticRewards = new[] { "title_immortal", "title_endless_legend", "title_the_forsaken", "portrait_immortal", "ui_theme_endless", "frame_legendary_2" },
                AlternativeStart = (string?)null
            },
            new
            {
                TierNumber = 10,
                TierName = "Transcendent",
                Description = "Beat New Game+5 and transcend mortality",
                RequiredPoints = 3000,
                UnlockRewards = new[] { "fast_travel_unlock" },
                CosmeticRewards = new[] { "title_transcendent", "title_ascendant", "title_godslayer", "portrait_transcendent", "ui_theme_transcendent", "ability_vfx_legendary_pack", "frame_transcendent", "emblem_transcendent", "combat_log_style_legendary" },
                AlternativeStart = "ironborn"
            }
        };

        foreach (var tier in tiers)
        {
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT OR REPLACE INTO Milestone_Tiers
                    (tier_number, tier_name, description, required_achievement_points,
                     unlock_rewards_json, cosmetic_rewards_json, alternative_start_unlock)
                VALUES (@TierNumber, @TierName, @Description, @RequiredPoints,
                        @UnlockRewards, @CosmeticRewards, @AlternativeStart)
            ";
            command.Parameters.AddWithValue("@TierNumber", tier.TierNumber);
            command.Parameters.AddWithValue("@TierName", tier.TierName);
            command.Parameters.AddWithValue("@Description", tier.Description);
            command.Parameters.AddWithValue("@RequiredPoints", tier.RequiredPoints);
            command.Parameters.AddWithValue("@UnlockRewards", JsonSerializer.Serialize(tier.UnlockRewards));
            command.Parameters.AddWithValue("@CosmeticRewards", JsonSerializer.Serialize(tier.CosmeticRewards));
            command.Parameters.AddWithValue("@AlternativeStart", tier.AlternativeStart ?? (object)DBNull.Value);

            command.ExecuteNonQuery();
        }

        _log.Information("Seeded {Count} milestone tiers", tiers.Length);
    }

    #endregion

    #region Account Unlocks

    private void SeedAccountUnlocks()
    {
        _log.Debug("Seeding account unlocks");

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var unlocks = new[]
        {
            new
            {
                UnlockId = "skip_tutorial",
                Name = "Skip Tutorial",
                Type = "Convenience",
                Description = "New characters can skip the tutorial",
                Requirement = "Complete tutorial once"
            },
            new
            {
                UnlockId = "veterans_start",
                Name = "Veteran's Start",
                Type = "Variety",
                Description = "Start new characters with basic equipment set",
                Requirement = "Complete campaign once"
            },
            new
            {
                UnlockId = "legend_boost_5",
                Name = "Legend Boost +5%",
                Type = "Progression",
                Description = "All characters gain +5% Legend (faster early levels)",
                Requirement = "Beat Boss Gauntlet"
            },
            new
            {
                UnlockId = "extra_loadout_slot",
                Name = "Extra Loadout Slot",
                Type = "Convenience",
                Description = "+1 equipment loadout slot for quick swaps",
                Requirement = "Complete NG+3"
            },
            new
            {
                UnlockId = "advanced_spec_unlock",
                Name = "Advanced Specialization Unlock",
                Type = "Variety",
                Description = "Unlock advanced specializations at level 1",
                Requirement = "Defeat 10 bosses"
            },
            new
            {
                UnlockId = "starting_resources_50",
                Name = "Starting Resources +50%",
                Type = "Progression",
                Description = "Start with 50% more currency and consumables",
                Requirement = "Reach wave 30 in Endless Mode"
            },
            new
            {
                UnlockId = "bestiary_auto_complete",
                Name = "Bestiary Auto-Complete",
                Type = "Knowledge",
                Description = "New characters start with partial bestiary filled",
                Requirement = "Examine 50 enemy types"
            },
            new
            {
                UnlockId = "codex_persistence",
                Name = "Codex Persistence",
                Type = "Knowledge",
                Description = "Codex entries carry across all characters",
                Requirement = "Unlock 30 codex entries"
            },
            new
            {
                UnlockId = "fast_travel_unlock",
                Name = "Fast Travel",
                Type = "Convenience",
                Description = "Unlock fast travel between sectors",
                Requirement = "Complete NG+5"
            },
            new
            {
                UnlockId = "crafting_mastery",
                Name = "Crafting Mastery",
                Type = "Variety",
                Description = "Reduce crafting material costs by 10%",
                Requirement = "Craft 50 items"
            }
        };

        foreach (var unlock in unlocks)
        {
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT OR REPLACE INTO Account_Unlocks
                    (unlock_id, name, unlock_type, description, requirement_description, parameters_json)
                VALUES (@UnlockId, @Name, @Type, @Description, @Requirement, '{}')
            ";
            command.Parameters.AddWithValue("@UnlockId", unlock.UnlockId);
            command.Parameters.AddWithValue("@Name", unlock.Name);
            command.Parameters.AddWithValue("@Type", unlock.Type);
            command.Parameters.AddWithValue("@Description", unlock.Description);
            command.Parameters.AddWithValue("@Requirement", unlock.Requirement);

            command.ExecuteNonQuery();
        }

        _log.Information("Seeded {Count} account unlocks", unlocks.Length);
    }

    #endregion

    #region Achievements (Abbreviated - showing key examples)

    private void SeedAchievements()
    {
        _log.Debug("Seeding achievements");

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Milestone Achievements
        SeedMilestoneAchievements(connection);

        // Combat Achievements
        SeedCombatAchievements(connection);

        // Challenge Achievements
        SeedChallengeAchievements(connection);

        // Exploration Achievements
        SeedExplorationAchievements(connection);

        // Narrative Achievements
        SeedNarrativeAchievements(connection);

        _log.Information("Achievements seeded successfully");
    }

    private void SeedMilestoneAchievements(SqliteConnection connection)
    {
        var achievements = new[]
        {
            new
            {
                Id = "first_steps",
                Name = "First Steps",
                Category = "Milestone",
                Description = "Complete the tutorial",
                FlavorText = "Every journey begins with a single step into the wastes",
                Points = 5,
                IsSecret = 0,
                IconId = "icon_tutorial",
                RequiredProgress = 1,
                Rewards = new[] { "title_initiate" }
            },
            new
            {
                Id = "survivor",
                Name = "Survivor",
                Category = "Milestone",
                Description = "Complete the campaign",
                FlavorText = "You've survived the impossible. But this is only the beginning.",
                Points = 10,
                IsSecret = 0,
                IconId = "icon_campaign",
                RequiredProgress = 1,
                Rewards = new[] { "title_survivor" }
            },
            new
            {
                Id = "legend",
                Name = "Legend",
                Category = "Milestone",
                Description = "Reach level 20",
                FlavorText = "Your name will echo through the halls of the Forsaken",
                Points = 10,
                IsSecret = 0,
                IconId = "icon_level_20",
                RequiredProgress = 1,
                Rewards = new[] { "portrait_legendary" }
            },
            new
            {
                Id = "master_of_all",
                Name = "Master of All",
                Category = "Milestone",
                Description = "Unlock all 4 archetype specializations",
                FlavorText = "Versatility is the mark of a true master",
                Points = 15,
                IsSecret = 0,
                IconId = "icon_all_specs",
                RequiredProgress = 1,
                Rewards = new[] { "ui_theme_master" }
            },
            new
            {
                Id = "transcendent",
                Name = "Transcendent",
                Category = "Milestone",
                Description = "Beat New Game+5",
                FlavorText = "You have transcended the limits of mortality",
                Points = 20,
                IsSecret = 0,
                IconId = "icon_ng_plus_5",
                RequiredProgress = 1,
                Rewards = new[] { "title_transcendent" }
            }
        };

        foreach (var ach in achievements)
        {
            InsertAchievement(connection, ach.Id, ach.Name, ach.Category, ach.Description,
                ach.FlavorText, ach.Points, ach.IsSecret, ach.IconId, ach.RequiredProgress);

            foreach (var reward in ach.Rewards)
            {
                InsertAchievementReward(connection, ach.Id, "Cosmetic", reward);
            }
        }

        _log.Debug("Seeded milestone achievements");
    }

    private void SeedCombatAchievements(SqliteConnection connection)
    {
        var achievements = new[]
        {
            new
            {
                Id = "untouchable",
                Name = "Untouchable",
                Category = "Combat",
                Description = "Complete a sector without taking damage",
                FlavorText = "Grace under pressure, perfection in motion",
                Points = 15,
                IsSecret = 0,
                IconId = "icon_perfect",
                RequiredProgress = 1,
                Rewards = new[] { "title_untouchable" }
            },
            new
            {
                Id = "boss_slayer",
                Name = "Boss Slayer",
                Category = "Combat",
                Description = "Defeat your first boss",
                FlavorText = "The hunt begins",
                Points = 5,
                IsSecret = 0,
                IconId = "icon_boss",
                RequiredProgress = 1,
                Rewards = new[] { "title_boss_slayer" }
            },
            new
            {
                Id = "flawless_victory",
                Name = "Flawless Victory",
                Category = "Combat",
                Description = "Defeat a boss without using healing",
                FlavorText = "Perfection requires no second chances",
                Points = 20,
                IsSecret = 0,
                IconId = "icon_flawless",
                RequiredProgress = 1,
                Rewards = new[] { "ability_vfx_flawless_glow" }
            },
            new
            {
                Id = "combo_master",
                Name = "Combo Master",
                Category = "Combat",
                Description = "Execute a 20-hit combo",
                FlavorText = "Relentless assault, perfect timing",
                Points = 15,
                IsSecret = 0,
                IconId = "icon_combo",
                RequiredProgress = 1,
                Rewards = new[] { "combat_log_style_combo" }
            }
        };

        foreach (var ach in achievements)
        {
            InsertAchievement(connection, ach.Id, ach.Name, ach.Category, ach.Description,
                ach.FlavorText, ach.Points, ach.IsSecret, ach.IconId, ach.RequiredProgress);

            foreach (var reward in ach.Rewards)
            {
                InsertAchievementReward(connection, ach.Id, "Cosmetic", reward);
            }
        }

        _log.Debug("Seeded combat achievements");
    }

    private void SeedChallengeAchievements(SqliteConnection connection)
    {
        var achievements = new[]
        {
            new
            {
                Id = "iron_will",
                Name = "Iron Will",
                Category = "Challenge",
                Description = "Complete a campaign without acquiring Trauma",
                FlavorText = "Unbroken, unbowed, unconquered",
                Points = 50,
                IsSecret = 0,
                IconId = "icon_iron_will",
                RequiredProgress = 1,
                Rewards = new[] { "title_iron_will", "portrait_iron_will" }
            },
            new
            {
                Id = "the_purist",
                Name = "The Purist",
                Category = "Challenge",
                Description = "Beat campaign using only Tier 1 equipment",
                FlavorText = "True strength comes from within",
                Points = 30,
                IsSecret = 0,
                IconId = "icon_purist",
                RequiredProgress = 1,
                Rewards = new[] { "title_the_purist" }
            },
            new
            {
                Id = "speed_demon",
                Name = "Speed Demon",
                Category = "Challenge",
                Description = "Complete campaign in under 5 hours",
                FlavorText = "Time waits for no one",
                Points = 25,
                IsSecret = 0,
                IconId = "icon_speedrun",
                RequiredProgress = 1,
                Rewards = new[] { "ui_theme_speedrun" }
            },
            new
            {
                Id = "gauntlet_master",
                Name = "Gauntlet Master",
                Category = "Challenge",
                Description = "Beat Boss Gauntlet without using healing items",
                FlavorText = "The ultimate test of skill and endurance",
                Points = 40,
                IsSecret = 0,
                IconId = "icon_gauntlet_master",
                RequiredProgress = 1,
                Rewards = new[] { "title_gauntlet_master" }
            },
            new
            {
                Id = "endless_legend",
                Name = "Endless Legend",
                Category = "Challenge",
                Description = "Reach wave 50 in Endless Mode",
                FlavorText = "Legends never die",
                Points = 35,
                IsSecret = 0,
                IconId = "icon_endless_50",
                RequiredProgress = 1,
                Rewards = new[] { "portrait_endless" }
            }
        };

        foreach (var ach in achievements)
        {
            InsertAchievement(connection, ach.Id, ach.Name, ach.Category, ach.Description,
                ach.FlavorText, ach.Points, ach.IsSecret, ach.IconId, ach.RequiredProgress);

            foreach (var reward in ach.Rewards)
            {
                InsertAchievementReward(connection, ach.Id, "Cosmetic", reward);
            }
        }

        _log.Debug("Seeded challenge achievements");
    }

    private void SeedExplorationAchievements(SqliteConnection connection)
    {
        var achievements = new[]
        {
            new
            {
                Id = "lorekeeper",
                Name = "Lorekeeper",
                Category = "Exploration",
                Description = "Unlock all codex entries for a biome",
                FlavorText = "Knowledge is power, history is wisdom",
                Points = 10,
                IsSecret = 0,
                IconId = "icon_codex",
                RequiredProgress = 1,
                Rewards = new[] { "title_lorekeeper" }
            },
            new
            {
                Id = "bestiary_complete",
                Name = "Bestiary Complete",
                Category = "Exploration",
                Description = "Examine all enemy types",
                FlavorText = "Know thy enemy",
                Points = 15,
                IsSecret = 0,
                IconId = "icon_bestiary",
                RequiredProgress = 1,
                Rewards = new[] { "portrait_researcher" }
            },
            new
            {
                Id = "cartographer",
                Name = "Cartographer",
                Category = "Exploration",
                Description = "Visit all 5 biome types",
                FlavorText = "The wastes have no more secrets from you",
                Points = 10,
                IsSecret = 0,
                IconId = "icon_map",
                RequiredProgress = 1,
                Rewards = new[] { "ui_theme_pack_biomes" }
            },
            new
            {
                Id = "treasure_hunter",
                Name = "Treasure Hunter",
                Category = "Exploration",
                Description = "Discover 20 hidden rooms",
                FlavorText = "Fortune favors the curious",
                Points = 10,
                IsSecret = 0,
                IconId = "icon_treasure",
                RequiredProgress = 1,
                Rewards = new[] { "title_treasure_hunter" }
            }
        };

        foreach (var ach in achievements)
        {
            InsertAchievement(connection, ach.Id, ach.Name, ach.Category, ach.Description,
                ach.FlavorText, ach.Points, ach.IsSecret, ach.IconId, ach.RequiredProgress);

            foreach (var reward in ach.Rewards)
            {
                InsertAchievementReward(connection, ach.Id, "Cosmetic", reward);
            }
        }

        _log.Debug("Seeded exploration achievements");
    }

    private void SeedNarrativeAchievements(SqliteConnection connection)
    {
        var achievements = new[]
        {
            new
            {
                Id = "truth_revealed",
                Name = "The Truth Revealed",
                Category = "Narrative",
                Description = "Discover the cause of the Glitch",
                FlavorText = "Some truths are best left buried",
                Points = 15,
                IsSecret = 0,
                IconId = "icon_truth",
                RequiredProgress = 1,
                Rewards = new[] { "title_truth_seeker" }
            },
            new
            {
                Id = "mercy",
                Name = "Mercy",
                Category = "Narrative",
                Description = "Spare a boss encounter",
                FlavorText = "Not all stories need to end in blood",
                Points = 10,
                IsSecret = 1, // Secret achievement
                IconId = "icon_mercy",
                RequiredProgress = 1,
                Rewards = new[] { "title_merciful" }
            },
            new
            {
                Id = "all_roads",
                Name = "All Roads",
                Category = "Narrative",
                Description = "Complete all faction quest chains",
                FlavorText = "Every path walked, every story told",
                Points = 25,
                IsSecret = 0,
                IconId = "icon_quests",
                RequiredProgress = 1,
                Rewards = new[] { "title_completionist", "portrait_faction_master" }
            }
        };

        foreach (var ach in achievements)
        {
            InsertAchievement(connection, ach.Id, ach.Name, ach.Category, ach.Description,
                ach.FlavorText, ach.Points, ach.IsSecret, ach.IconId, ach.RequiredProgress);

            foreach (var reward in ach.Rewards)
            {
                InsertAchievementReward(connection, ach.Id, "Cosmetic", reward);
            }
        }

        _log.Debug("Seeded narrative achievements");
    }

    private void InsertAchievement(SqliteConnection connection, string id, string name, string category,
        string description, string flavorText, int points, int isSecret, string iconId, int requiredProgress)
    {
        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT OR REPLACE INTO Achievements
                (achievement_id, name, category, description, flavor_text,
                 achievement_points, is_secret, icon_id, required_progress)
            VALUES (@Id, @Name, @Category, @Description, @FlavorText,
                    @Points, @IsSecret, @IconId, @RequiredProgress)
        ";
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@Name", name);
        command.Parameters.AddWithValue("@Category", category);
        command.Parameters.AddWithValue("@Description", description);
        command.Parameters.AddWithValue("@FlavorText", flavorText);
        command.Parameters.AddWithValue("@Points", points);
        command.Parameters.AddWithValue("@IsSecret", isSecret);
        command.Parameters.AddWithValue("@IconId", iconId);
        command.Parameters.AddWithValue("@RequiredProgress", requiredProgress);

        command.ExecuteNonQuery();
    }

    private void InsertAchievementReward(SqliteConnection connection, string achievementId, string rewardType, string rewardItemId)
    {
        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Achievement_Rewards (achievement_id, reward_type, reward_item_id)
            VALUES (@AchievementId, @RewardType, @RewardItemId)
        ";
        command.Parameters.AddWithValue("@AchievementId", achievementId);
        command.Parameters.AddWithValue("@RewardType", rewardType);
        command.Parameters.AddWithValue("@RewardItemId", rewardItemId);

        command.ExecuteNonQuery();
    }

    #endregion

    #region Cosmetics (Abbreviated - showing key examples)

    private void SeedCosmetics()
    {
        _log.Debug("Seeding cosmetics");

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Seed titles
        SeedTitles(connection);

        // Seed portraits
        SeedPortraits(connection);

        // Seed UI themes
        SeedUIThemes(connection);

        // Seed ability VFX
        SeedAbilityVFX(connection);

        _log.Information("Cosmetics seeded successfully");
    }

    private void SeedTitles(SqliteConnection connection)
    {
        var titles = new[]
        {
            ("title_initiate", "Initiate", "Unlocked by completing tutorial"),
            ("title_survivor", "Survivor", "Unlocked by completing campaign"),
            ("title_veteran", "Veteran", "Unlocked by beating NG+1"),
            ("title_champion", "Champion", "Unlocked by beating Boss Gauntlet"),
            ("title_legend", "Legend", "Unlocked by reaching level 20"),
            ("title_master", "Master", "Unlocked by beating NG+3"),
            ("title_transcendent", "Transcendent", "Unlocked by beating NG+5"),
            ("title_gauntlet_champion", "Gauntlet Champion", "Unlocked by beating Boss Gauntlet"),
            ("title_endless_legend", "Endless Legend", "Unlocked by reaching wave 50 in Endless Mode"),
            ("title_the_forsaken", "The Forsaken", "Unlocked by reaching wave 40 in Endless Mode"),
            ("title_blight_walker", "Blight-Walker", "Unlocked by defeating 100 Blighted enemies"),
            ("title_iron_will", "Iron Will", "Unlocked by completing run without Trauma"),
            ("title_untouchable", "Untouchable", "Unlocked by completing sector without damage"),
            ("title_boss_slayer", "Boss Slayer", "Unlocked by defeating first boss"),
            ("title_lorekeeper", "Lorekeeper", "Unlocked by completing codex for a biome"),
            ("title_treasure_hunter", "Treasure Hunter", "Unlocked by discovering 20 hidden rooms"),
            ("title_the_purist", "The Purist", "Unlocked by beating campaign with Tier 1 equipment only")
        };

        foreach (var (id, name, unlock) in titles)
        {
            InsertCosmetic(connection, id, name, "Title", $"Display title: {name}", unlock);
        }

        _log.Debug("Seeded {Count} titles", titles.Length);
    }

    private void SeedPortraits(SqliteConnection connection)
    {
        var portraits = new[]
        {
            ("portrait_default_warrior", "Warrior Portrait", "Default warrior portrait", "Always available"),
            ("portrait_default_mystic", "Mystic Portrait", "Default mystic portrait", "Always available"),
            ("portrait_default_adept", "Adept Portrait", "Default adept portrait", "Always available"),
            ("portrait_default_skirmisher", "Skirmisher Portrait", "Default skirmisher portrait", "Always available"),
            ("portrait_survivor", "Survivor Portrait", "Portrait for survivors", "Complete campaign"),
            ("portrait_legendary", "Legendary Portrait", "Portrait for legends", "Reach level 20"),
            ("portrait_champion", "Champion Portrait", "Portrait for champions", "Beat Boss Gauntlet"),
            ("portrait_master", "Master Portrait", "Portrait for masters", "Beat NG+3"),
            ("portrait_immortal", "Immortal Portrait", "Portrait for immortals", "Reach wave 30 Endless"),
            ("portrait_transcendent", "Transcendent Portrait", "Portrait for transcendent beings", "Beat NG+5")
        };

        foreach (var (id, name, desc, unlock) in portraits)
        {
            InsertCosmetic(connection, id, name, "Portrait", desc, unlock);
        }

        _log.Debug("Seeded {Count} portraits", portraits.Length);
    }

    private void SeedUIThemes(SqliteConnection connection)
    {
        var themes = new[]
        {
            ("ui_theme_default", "Default Theme", "Standard gray/blue theme", "Always available"),
            ("ui_theme_muspelheim", "Muspelheim Forge", "Red/orange fire theme", "Complete Muspelheim biome"),
            ("ui_theme_niflheim", "Niflheim Frost", "Cyan/white ice theme", "Complete Niflheim biome"),
            ("ui_theme_alfheim", "Alfheim Light", "Gold/purple light theme", "Complete Alfheim biome"),
            ("ui_theme_dark_mode", "Dark Mode", "Black/gray minimal theme", "Milestone Tier 5"),
            ("ui_theme_speedrun", "Speedrun", "High contrast theme for speedrunners", "Complete campaign in under 5 hours")
        };

        foreach (var (id, name, desc, unlock) in themes)
        {
            InsertCosmetic(connection, id, name, "UITheme", desc, unlock);
        }

        _log.Debug("Seeded {Count} UI themes", themes.Length);
    }

    private void SeedAbilityVFX(SqliteConnection connection)
    {
        var vfx = new[]
        {
            ("ability_vfx_fire_blue", "Blue Flames", "Blue fire visual effect", "Milestone Tier 3"),
            ("ability_vfx_fire_green", "Green Flames", "Green fire visual effect", "Milestone Tier 3"),
            ("ability_vfx_lightning_red", "Red Lightning", "Red lightning visual effect", "Milestone Tier 3"),
            ("ability_vfx_frost_shards", "Frost Shards", "Icy particle effect", "Milestone Tier 4"),
            ("ability_vfx_smoke", "Smoke Trail", "Smoke particle effect", "Milestone Tier 4"),
            ("ability_vfx_golden_glow", "Golden Glow", "Golden aura effect", "Milestone Tier 6"),
            ("ability_vfx_flawless_glow", "Flawless Glow", "Perfect execution glow", "Flawless Victory achievement")
        };

        foreach (var (id, name, desc, unlock) in vfx)
        {
            InsertCosmetic(connection, id, name, "AbilityVFX", desc, unlock);
        }

        _log.Debug("Seeded {Count} ability VFX", vfx.Length);
    }

    private void InsertCosmetic(SqliteConnection connection, string id, string name, string type,
        string description, string unlockRequirement)
    {
        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT OR REPLACE INTO Cosmetics
                (cosmetic_id, name, cosmetic_type, description, preview_image_url,
                 unlock_requirement, parameters_json)
            VALUES (@Id, @Name, @Type, @Description, '', @UnlockRequirement, '{}')
        ";
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@Name", name);
        command.Parameters.AddWithValue("@Type", type);
        command.Parameters.AddWithValue("@Description", description);
        command.Parameters.AddWithValue("@UnlockRequirement", unlockRequirement);

        command.ExecuteNonQuery();
    }

    #endregion

    #region Alternative Starts

    private void SeedAlternativeStarts()
    {
        _log.Debug("Seeding alternative starts");

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var starts = new[]
        {
            new
            {
                StartId = "standard_start",
                Name = "Standard Start",
                Description = "Begin your journey from the beginning",
                FlavorText = "Every legend starts somewhere. This is where yours begins.",
                Requirement = "Always available",
                StartingLevel = 1,
                Equipment = new string[] { },
                Specializations = new string[] { },
                Legend = 0,
                Resources = new Dictionary<string, int>(),
                SectorId = (int?)1,
                Quests = new string[] { },
                SkipTutorial = 0,
                HardMode = 0,
                Permadeath = 0,
                RewardMultiplier = 1.0,
                Timer = 0
            },
            new
            {
                StartId = "veterans_return",
                Name = "Veteran's Return",
                Description = "Skip the tutorial and start with basic equipment",
                FlavorText = "You've been here before. You know what to do.",
                Requirement = "Complete campaign once",
                StartingLevel = 1,
                Equipment = new[] { "Rusty Hatchet", "Scrap Plating" },
                Specializations = new string[] { },
                Legend = 0,
                Resources = new Dictionary<string, int> { { "currency", 100 } },
                SectorId = (int?)2,
                Quests = new string[] { },
                SkipTutorial = 1,
                HardMode = 0,
                Permadeath = 0,
                RewardMultiplier = 1.0,
                Timer = 0
            },
            new
            {
                StartId = "advanced_explorer",
                Name = "Advanced Explorer",
                Description = "Start at level 5 with advanced specializations unlocked early",
                FlavorText = "Test your endgame builds from the start.",
                Requirement = "Beat NG+3",
                StartingLevel = 5,
                Equipment = new[] { "Quality Weapon", "Quality Armor" },
                Specializations = new[] { "all" }, // All specs unlocked early
                Legend = 500,
                Resources = new Dictionary<string, int> { { "currency", 500 } },
                SectorId = (int?)1,
                Quests = new string[] { },
                SkipTutorial = 1,
                HardMode = 0,
                Permadeath = 0,
                RewardMultiplier = 1.0,
                Timer = 0
            },
            new
            {
                StartId = "challenge_seeker",
                Name = "Challenge Seeker",
                Description = "Start with no equipment and hard mode enabled",
                FlavorText = "For those who find victory too easy.",
                Requirement = "Complete 10 Challenge Sectors",
                StartingLevel = 1,
                Equipment = new string[] { },
                Specializations = new string[] { },
                Legend = 0,
                Resources = new Dictionary<string, int>(),
                SectorId = (int?)1,
                Quests = new string[] { },
                SkipTutorial = 0,
                HardMode = 1,
                Permadeath = 0,
                RewardMultiplier = 1.5,
                Timer = 0
            },
            new
            {
                StartId = "ironborn",
                Name = "Ironborn",
                Description = "Permadeath enabled with +50% rewards",
                FlavorText = "One life. One chance. Eternal glory.",
                Requirement = "Beat Boss Gauntlet solo",
                StartingLevel = 1,
                Equipment = new string[] { },
                Specializations = new string[] { },
                Legend = 0,
                Resources = new Dictionary<string, int>(),
                SectorId = (int?)1,
                Quests = new string[] { },
                SkipTutorial = 0,
                HardMode = 1,
                Permadeath = 1,
                RewardMultiplier = 1.5,
                Timer = 0
            }
        };

        foreach (var start in starts)
        {
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT OR REPLACE INTO Alternative_Starts
                    (start_id, name, description, flavor_text, requirement_description,
                     starting_level, starting_equipment_json, unlocked_specializations_json,
                     starting_legend, starting_resources_json, starting_sector_id,
                     completed_quests_json, skip_tutorial, hard_mode_enabled,
                     permadeath_enabled, reward_multiplier, timer_visible)
                VALUES (@StartId, @Name, @Description, @FlavorText, @Requirement,
                        @StartingLevel, @Equipment, @Specializations,
                        @Legend, @Resources, @SectorId,
                        @Quests, @SkipTutorial, @HardMode,
                        @Permadeath, @RewardMultiplier, @Timer)
            ";
            command.Parameters.AddWithValue("@StartId", start.StartId);
            command.Parameters.AddWithValue("@Name", start.Name);
            command.Parameters.AddWithValue("@Description", start.Description);
            command.Parameters.AddWithValue("@FlavorText", start.FlavorText);
            command.Parameters.AddWithValue("@Requirement", start.Requirement);
            command.Parameters.AddWithValue("@StartingLevel", start.StartingLevel);
            command.Parameters.AddWithValue("@Equipment", JsonSerializer.Serialize(start.Equipment));
            command.Parameters.AddWithValue("@Specializations", JsonSerializer.Serialize(start.Specializations));
            command.Parameters.AddWithValue("@Legend", start.Legend);
            command.Parameters.AddWithValue("@Resources", JsonSerializer.Serialize(start.Resources));
            command.Parameters.AddWithValue("@SectorId", start.SectorId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Quests", JsonSerializer.Serialize(start.Quests));
            command.Parameters.AddWithValue("@SkipTutorial", start.SkipTutorial);
            command.Parameters.AddWithValue("@HardMode", start.HardMode);
            command.Parameters.AddWithValue("@Permadeath", start.Permadeath);
            command.Parameters.AddWithValue("@RewardMultiplier", start.RewardMultiplier);
            command.Parameters.AddWithValue("@Timer", start.Timer);

            command.ExecuteNonQuery();
        }

        _log.Information("Seeded {Count} alternative starts", starts.Length);
    }

    #endregion
}
