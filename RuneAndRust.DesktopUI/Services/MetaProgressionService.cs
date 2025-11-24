using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.DesktopUI.Services;

/// <summary>
/// v0.43.15: Service implementation for accessing meta-progression data.
/// Currently provides sample data for UI demonstration.
/// Will be updated to use persistence layer in future integration.
/// </summary>
public class MetaProgressionService : IMetaProgressionService
{
    private readonly ILogger _logger;

    public MetaProgressionService()
    {
        _logger = Log.ForContext<MetaProgressionService>();
        _logger.Information("MetaProgressionService initialized");
    }

    /// <inheritdoc/>
    public int CurrentAccountId => 1;

    /// <inheritdoc/>
    public MetaProgressionState LoadMetaProgression()
    {
        _logger.Debug("Loading meta-progression state");

        var state = new MetaProgressionState
        {
            Account = GenerateSampleAccount(),
            Achievements = GenerateSampleAchievements(),
            Unlocks = GenerateSampleUnlocks(),
            UnlockedCosmetics = GenerateSampleCosmetics(),
            CurrentTier = GenerateSampleMilestoneTier()
        };

        _logger.Information("Meta-progression loaded: {AchievementCount} achievements, {UnlockCount} unlocks",
            state.Achievements.Count, state.Unlocks.Count);

        return state;
    }

    /// <inheritdoc/>
    public List<AchievementWithProgress> GetAchievementsByCategory(string category)
    {
        var achievements = GenerateSampleAchievements();

        if (category == "All")
            return achievements;

        return achievements
            .Where(a => a.Category.ToString() == category)
            .ToList();
    }

    /// <inheritdoc/>
    public List<Cosmetic> GetUnlockedCosmetics()
    {
        return GenerateSampleCosmetics();
    }

    /// <inheritdoc/>
    public List<AccountUnlock> GetAccountUnlocks()
    {
        return GenerateSampleUnlocks();
    }

    #region Sample Data Generation

    private AccountProgression GenerateSampleAccount()
    {
        return new AccountProgression
        {
            AccountId = 1,
            TotalAchievementPoints = 145,
            CurrentMilestoneTier = 3,
            TotalCharactersCreated = 5,
            TotalCampaignsCompleted = 3,
            TotalBossesDefeated = 12,
            TotalAchievementsUnlocked = 8,
            HighestNewGamePlusTier = 1,
            HighestEndlessWave = 15,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow
        };
    }

    private List<AchievementWithProgress> GenerateSampleAchievements()
    {
        var achievements = new List<AchievementWithProgress>
        {
            // Milestone achievements
            CreateAchievement("first_steps", "First Steps", AchievementCategory.Milestone,
                "Complete the tutorial", "Every journey begins with a single step into the wastes",
                5, false, 1, 1, true),

            CreateAchievement("survivor", "Survivor", AchievementCategory.Milestone,
                "Complete the campaign", "You've survived the impossible. But this is only the beginning.",
                10, false, 1, 1, true),

            CreateAchievement("legend", "Legend", AchievementCategory.Milestone,
                "Reach level 20", "Your name will echo through the halls of the Forsaken",
                10, false, 1, 0, false),

            CreateAchievement("master_of_all", "Master of All", AchievementCategory.Milestone,
                "Unlock all 4 archetype specializations", "Versatility is the mark of a true master",
                15, false, 4, 2, false),

            CreateAchievement("transcendent", "Transcendent", AchievementCategory.Milestone,
                "Beat New Game+5", "You have transcended the limits of mortality",
                20, false, 1, 0, false),

            // Combat achievements
            CreateAchievement("untouchable", "Untouchable", AchievementCategory.Combat,
                "Complete a sector without taking damage", "Grace under pressure, perfection in motion",
                15, false, 1, 0, false),

            CreateAchievement("boss_slayer", "Boss Slayer", AchievementCategory.Combat,
                "Defeat your first boss", "The hunt begins",
                5, false, 1, 1, true),

            CreateAchievement("flawless_victory", "Flawless Victory", AchievementCategory.Combat,
                "Defeat a boss without using healing", "Perfection requires no second chances",
                20, false, 1, 0, false),

            CreateAchievement("combo_master", "Combo Master", AchievementCategory.Combat,
                "Execute a 20-hit combo", "Relentless assault, perfect timing",
                15, false, 1, 1, true),

            // Exploration achievements
            CreateAchievement("lorekeeper", "Lorekeeper", AchievementCategory.Exploration,
                "Unlock all codex entries for a biome", "Knowledge is power, history is wisdom",
                10, false, 1, 0, false),

            CreateAchievement("bestiary_complete", "Bestiary Complete", AchievementCategory.Exploration,
                "Examine all enemy types", "Know thy enemy",
                15, false, 50, 23, false),

            CreateAchievement("cartographer", "Cartographer", AchievementCategory.Exploration,
                "Visit all 5 biome types", "The wastes have no more secrets from you",
                10, false, 5, 3, false),

            CreateAchievement("treasure_hunter", "Treasure Hunter", AchievementCategory.Exploration,
                "Discover 20 hidden rooms", "Fortune favors the curious",
                10, false, 20, 8, false),

            // Challenge achievements
            CreateAchievement("iron_will", "Iron Will", AchievementCategory.Challenge,
                "Complete a campaign without acquiring Trauma", "Unbroken, unbowed, unconquered",
                50, false, 1, 0, false),

            CreateAchievement("the_purist", "The Purist", AchievementCategory.Challenge,
                "Beat campaign using only Tier 1 equipment", "True strength comes from within",
                30, false, 1, 0, false),

            CreateAchievement("speed_demon", "Speed Demon", AchievementCategory.Challenge,
                "Complete campaign in under 5 hours", "Time waits for no one",
                25, false, 1, 0, false),

            CreateAchievement("endless_legend", "Endless Legend", AchievementCategory.Challenge,
                "Reach wave 50 in Endless Mode", "Legends never die",
                35, false, 1, 0, false),

            // Narrative achievements
            CreateAchievement("truth_revealed", "The Truth Revealed", AchievementCategory.Narrative,
                "Discover the cause of the Glitch", "Some truths are best left buried",
                15, false, 1, 1, true),

            CreateAchievement("mercy", "Mercy", AchievementCategory.Narrative,
                "Spare a boss encounter", "Not all stories need to end in blood",
                10, true, 1, 0, false),

            CreateAchievement("all_roads", "All Roads", AchievementCategory.Narrative,
                "Complete all faction quest chains", "Every path walked, every story told",
                25, false, 4, 1, false)
        };

        return achievements;
    }

    private AchievementWithProgress CreateAchievement(
        string id, string name, AchievementCategory category,
        string description, string flavorText,
        int points, bool isSecret, int requiredProgress, int currentProgress, bool isUnlocked)
    {
        return new AchievementWithProgress
        {
            Achievement = new Achievement
            {
                AchievementId = id,
                Name = name,
                Category = category,
                Description = description,
                FlavorText = flavorText,
                AchievementPoints = points,
                IsSecret = isSecret,
                RequiredProgress = requiredProgress,
                IconId = $"icon_{id}"
            },
            Progress = new AchievementProgress
            {
                AccountId = 1,
                AchievementId = id,
                CurrentProgress = currentProgress,
                IsUnlocked = isUnlocked,
                UnlockedAt = isUnlocked ? DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 30)) : null
            }
        };
    }

    private List<AccountUnlock> GenerateSampleUnlocks()
    {
        return new List<AccountUnlock>
        {
            new AccountUnlock
            {
                UnlockId = "skip_tutorial",
                Name = "Skip Tutorial",
                Type = AccountUnlockType.Convenience,
                Description = "New characters can skip the tutorial",
                RequirementDescription = "Complete tutorial once",
                IsUnlocked = true,
                UnlockedAt = DateTime.UtcNow.AddDays(-25)
            },
            new AccountUnlock
            {
                UnlockId = "veterans_start",
                Name = "Veteran's Start",
                Type = AccountUnlockType.Variety,
                Description = "Start new characters with basic equipment set",
                RequirementDescription = "Complete campaign once",
                IsUnlocked = true,
                UnlockedAt = DateTime.UtcNow.AddDays(-15)
            },
            new AccountUnlock
            {
                UnlockId = "legend_boost_5",
                Name = "Legend Boost +5%",
                Type = AccountUnlockType.Progression,
                Description = "All characters gain +5% Legend (faster early levels)",
                RequirementDescription = "Beat Boss Gauntlet",
                IsUnlocked = false
            },
            new AccountUnlock
            {
                UnlockId = "extra_loadout_slot",
                Name = "Extra Loadout Slot",
                Type = AccountUnlockType.Convenience,
                Description = "+1 equipment loadout slot for quick swaps",
                RequirementDescription = "Complete NG+3",
                IsUnlocked = false
            },
            new AccountUnlock
            {
                UnlockId = "bestiary_auto_complete",
                Name = "Bestiary Auto-Complete",
                Type = AccountUnlockType.Knowledge,
                Description = "New characters start with partial bestiary filled",
                RequirementDescription = "Examine 50 enemy types",
                IsUnlocked = false
            }
        };
    }

    private List<Cosmetic> GenerateSampleCosmetics()
    {
        return new List<Cosmetic>
        {
            new Cosmetic
            {
                CosmeticId = "title_initiate",
                Name = "Initiate",
                Type = CosmeticType.Title,
                Description = "Display title: Initiate",
                UnlockRequirement = "Complete tutorial"
            },
            new Cosmetic
            {
                CosmeticId = "title_survivor",
                Name = "Survivor",
                Type = CosmeticType.Title,
                Description = "Display title: Survivor",
                UnlockRequirement = "Complete campaign"
            },
            new Cosmetic
            {
                CosmeticId = "title_boss_slayer",
                Name = "Boss Slayer",
                Type = CosmeticType.Title,
                Description = "Display title: Boss Slayer",
                UnlockRequirement = "Defeat first boss"
            },
            new Cosmetic
            {
                CosmeticId = "portrait_default_warrior",
                Name = "Warrior Portrait",
                Type = CosmeticType.Portrait,
                Description = "Default warrior portrait",
                UnlockRequirement = "Always available"
            },
            new Cosmetic
            {
                CosmeticId = "portrait_survivor",
                Name = "Survivor Portrait",
                Type = CosmeticType.Portrait,
                Description = "Portrait for survivors",
                UnlockRequirement = "Complete campaign"
            },
            new Cosmetic
            {
                CosmeticId = "ui_theme_default",
                Name = "Default Theme",
                Type = CosmeticType.UITheme,
                Description = "Standard gray/blue theme",
                UnlockRequirement = "Always available"
            },
            new Cosmetic
            {
                CosmeticId = "ui_theme_muspelheim",
                Name = "Muspelheim Forge",
                Type = CosmeticType.UITheme,
                Description = "Red/orange fire theme",
                UnlockRequirement = "Complete Muspelheim biome"
            }
        };
    }

    private MilestoneTier GenerateSampleMilestoneTier()
    {
        return new MilestoneTier
        {
            TierNumber = 3,
            TierName = "Veteran",
            Description = "Master the basics and beat New Game+1",
            RequiredAchievementPoints = 150,
            UnlockRewards = new List<string> { "legend_boost_5" },
            CosmeticRewards = new List<string> { "title_veteran", "ability_vfx_fire_blue" }
        };
    }

    #endregion
}
