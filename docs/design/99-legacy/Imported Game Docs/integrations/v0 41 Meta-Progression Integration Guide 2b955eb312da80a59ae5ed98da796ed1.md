# v0.41: Meta-Progression Integration Guide

## Overview

This guide shows how to integrate v0.41 meta-progression with existing gameplay systems. The meta-progression system is fully implemented but requires integration hooks in gameplay code to track achievements and progression.

## Quick Start

### 1. Initialize AccountManager

```csharp
// In your game initialization
var connectionString = "Data Source=runeandrust.db";
var accountManager = new AccountManager(connectionString);

// Create or load account
int accountId = accountManager.CreateAccount(); // Or load existing account ID

```

### 2. Create Character with Account Progress

```csharp
// Standard character creation (v0.41 integrated)
var character = accountManager.CreateCharacterWithAccountProgress(
    accountId: accountId,
    characterClass: CharacterClass.Warrior,
    characterName: "Bjorn",
    alternativeStartId: "standard_start" // Or null for default
);

// Legacy character creation (still supported)
var character = CharacterFactory.CreateCharacter(
    characterClass: CharacterClass.Warrior,
    name: "Bjorn",
    accountId: accountId,
    accountService: accountManager.AccountService,
    alternativeStartId: "veterans_return",
    alternativeStartService: accountManager.AlternativeStartService
);

```

### 3. Track Gameplay Events

```csharp
// After tutorial completion
accountManager.OnTutorialCompleted(accountId);

// After campaign completion
accountManager.OnCampaignCompleted(accountId, traumaFree: false);

// After boss defeat
accountManager.OnBossDefeated(accountId, bossId: "gorge_maw", flawless: true);

// After level up
accountManager.OnLevelReached(accountId, level: 20);

// Custom achievement tracking
accountManager.TrackAchievementProgress(accountId, "combo_master", increment: 1);

```

### 4. Display Account Summary

```csharp
accountManager.DisplayAccountSummary(accountId);

// Get statistics programmatically
var stats = accountManager.GetAccountStatistics(accountId);
Console.WriteLine($"Achievement Points: {stats.TotalAchievementPoints}");
Console.WriteLine($"Current Tier: {stats.CurrentMilestoneTierName}");

```

## Integration Points

### CombatEngine.cs Integration

Add achievement tracking to combat events:

```csharp
// Location: RuneAndRust.Engine/CombatEngine.cs

public class CombatEngine
{
    private readonly AccountManager? _accountManager;
    private int? _accountId;

    // Add AccountManager to constructor
    public CombatEngine(/* existing params */, AccountManager? accountManager = null, int? accountId = null)
    {
        // existing initialization
        _accountManager = accountManager;
        _accountId = accountId;
    }

    // Example: Track perfect sector clear
    public void CompleteSector()
    {
        // Existing sector completion logic...

        // v0.41: Track perfect clear achievement
        if (_accountManager != null && _accountId.HasValue && !_playerTookDamageThisSector)
        {
            _accountManager.OnPerfectSectorClear(_accountId.Value);
        }
    }

    // Example: Track combo achievements
    private void ExecuteAttack(PlayerCharacter player, Enemy enemy, int hitCount)
    {
        // Existing combat logic...

        // v0.41: Track combo achievements
        if (_accountManager != null && _accountId.HasValue && hitCount >= 20)
        {
            _accountManager.TrackAchievementProgress(_accountId.Value, "combo_master");
        }
    }
}

```

### BossEncounterService.cs Integration

Add boss defeat tracking:

```csharp
// Location: RuneAndRust.Engine/BossEncounterService.cs

public class BossEncounterService
{
    private readonly AccountManager? _accountManager;

    // Add AccountManager to constructor
    public BossEncounterService(/* existing params */, AccountManager? accountManager = null)
    {
        _accountManager = accountManager;
    }

    // Track boss defeat
    public void OnBossDefeated(int? accountId, string bossId, bool playerTookDamage)
    {
        // Existing boss defeat logic...

        // v0.41: Track boss defeat achievement
        if (_accountManager != null && accountId.HasValue)
        {
            bool flawless = !playerTookDamage;
            _accountManager.OnBossDefeated(accountId.Value, bossId, flawless);
        }
    }
}

```

### QuestService.cs Integration

Add quest completion tracking:

```csharp
// Location: RuneAndRust.Engine/QuestService.cs

public class QuestService
{
    private readonly AccountManager? _accountManager;

    // Add AccountManager to constructor
    public QuestService(/* existing params */, AccountManager? accountManager = null)
    {
        _accountManager = accountManager;
    }

    // Track quest completion
    public void CompleteQuest(int? accountId, Quest quest)
    {
        // Existing quest completion logic...
        character.CompletedQuests.Add(quest);

        // v0.41: Check for "All Roads" achievement (complete all faction quests)
        if (_accountManager != null && accountId.HasValue)
        {
            var completedFactionQuests = character.CompletedQuests
                .Count(q => q.QuestType == QuestType.Faction);

            if (completedFactionQuests >= 10) // Adjust threshold as needed
            {
                _accountManager.TrackAchievementProgress(accountId.Value, "all_roads");
            }
        }
    }
}

```

### BossGauntletService.cs Integration

Add gauntlet completion tracking:

```csharp
// Location: RuneAndRust.Engine/BossGauntlet/BossGauntletService.cs

public class BossGauntletService
{
    private readonly AccountManager? _accountManager;

    // Add AccountManager to constructor
    public BossGauntletService(/* existing params */, AccountManager? accountManager = null)
    {
        _accountManager = accountManager;
    }

    // Track gauntlet completion
    public void OnGauntletCompleted(int? accountId, bool usedHealing)
    {
        // Existing gauntlet completion logic...

        // v0.41: Track gauntlet achievements
        if (_accountManager != null && accountId.HasValue)
        {
            // Basic completion
            _accountManager.TrackAchievementProgress(accountId.Value, "survivor");

            // Gauntlet-specific achievement
            if (!usedHealing)
            {
                _accountManager.TrackAchievementProgress(accountId.Value, "gauntlet_master");
            }
        }
    }
}

```

### Character Progression Integration

Track level-ups and specialization unlocks:

```csharp
// Location: Wherever character levels up

public void OnCharacterLevelUp(PlayerCharacter character, int newLevel, int? accountId)
{
    // Existing level-up logic...

    // v0.41: Track level achievements
    if (_accountManager != null && accountId.HasValue)
    {
        _accountManager.OnLevelReached(accountId.Value, newLevel);

        // Track specialization unlocks
        if (character.Specialization != Specialization.None)
        {
            var totalSpecsUnlocked = GetTotalSpecializationsUnlocked(character);
            if (totalSpecsUnlocked >= 4)
            {
                _accountManager.OnAllSpecializationsUnlocked(accountId.Value);
            }
        }
    }
}

```

### Campaign Completion Integration

Track campaign completion:

```csharp
// Location: Where campaign completes (e.g., after final boss)

public void OnCampaignComplete(PlayerCharacter character, int? accountId)
{
    // Existing campaign completion logic...

    // v0.41: Track campaign completion
    if (_accountManager != null && accountId.HasValue)
    {
        bool traumaFree = character.Traumas.Count == 0;
        _accountManager.OnCampaignCompleted(accountId.Value, traumaFree);
    }
}

```

### New Game+ Integration

Track NG+ tier completion:

```csharp
// Location: NewGamePlus service or completion handler

public void OnNewGamePlusCompleted(int? accountId, int tier)
{
    // Existing NG+ logic...

    // v0.41: Track NG+ achievements
    if (_accountManager != null && accountId.HasValue)
    {
        _accountManager.OnNewGamePlusCompleted(accountId.Value, tier);
    }
}

```

### Endless Mode Integration

Track wave milestones:

```csharp
// Location: EndlessMode service

public void OnWaveCompleted(int? accountId, int wave)
{
    // Existing wave completion logic...

    // v0.41: Track wave achievements
    if (_accountManager != null && accountId.HasValue)
    {
        _accountManager.OnEndlessWaveReached(accountId.Value, wave);
    }
}

```

## Achievement Event Reference

### Complete Event Handler List

| Event Method | When to Call | Example Location |
| --- | --- | --- |
| `OnTutorialCompleted` | After tutorial finishes | Tutorial system |
| `OnCampaignCompleted` | After campaign completion | Final boss defeat |
| `OnBossDefeated` | After any boss defeat | BossEncounterService |
| `OnLevelReached` | After character levels up | Progression service |
| `OnNewGamePlusCompleted` | After NG+ tier completion | NewGamePlus service |
| `OnEndlessWaveReached` | After each wave in Endless | EndlessMode service |
| `OnPerfectSectorClear` | After sector with no damage | CombatEngine |
| `OnAllSpecializationsUnlocked` | When all specs unlocked | Specialization service |
| `OnCodexEntryUnlocked` | When codex entry unlocked | Codex system |
| `OnEnemyExamined` | When enemy is examined | Examine action |
| `OnTraumaFreeCampaignCompleted` | Campaign with zero Traumas | Campaign completion |
| `TrackAchievementProgress` | For custom achievements | Anywhere |

### Achievement IDs Reference

Use these achievement IDs when calling `TrackAchievementProgress`:

**Milestone Achievements:**

- `first_steps` - Complete tutorial
- `survivor` - Complete campaign
- `legend` - Reach level 20
- `master_of_all` - Unlock all specializations
- `transcendent` - Beat NG+5

**Combat Achievements:**

- `untouchable` - Perfect sector clear
- `boss_slayer` - Defeat first boss
- `flawless_victory` - Boss defeat without healing
- `combo_master` - 20-hit combo

**Challenge Achievements:**

- `iron_will` - Campaign without Trauma
- `the_purist` - Campaign with Tier 1 equipment only
- `speed_demon` - Campaign in <5 hours
- `gauntlet_master` - Boss Gauntlet without healing
- `endless_legend` - Wave 50 in Endless

**Exploration Achievements:**

- `lorekeeper` - Complete biome codex
- `bestiary_complete` - Examine all enemies
- `cartographer` - Visit all biomes
- `treasure_hunter` - Discover 20 hidden rooms

**Narrative Achievements:**

- `truth_revealed` - Discover Glitch's cause
- `mercy` - Spare a boss (SECRET)
- `all_roads` - Complete all faction quests

## Alternative Start Integration

### Character Creation UI

Add alternative start selection to character creation:

```csharp
// Example: Character creation screen

public void CharacterCreationScreen(int accountId)
{
    // Get unlocked alternative starts
    var unlockedStarts = accountManager.GetUnlockedAlternativeStarts(accountId);

    Console.WriteLine("\\n=== SELECT STARTING SCENARIO ===");
    for (int i = 0; i < unlockedStarts.Count; i++)
    {
        var start = unlockedStarts[i];
        Console.WriteLine($"  [{i + 1}] {start.Name}");
        Console.WriteLine($"      {start.Description}");
        Console.WriteLine($"      Level {start.StartingLevel} | {start.FlavorText}");
        Console.WriteLine();
    }

    // Get player choice
    Console.Write("Select scenario: ");
    int choice = int.Parse(Console.ReadLine() ?? "1");
    var selectedStart = unlockedStarts[choice - 1];

    // Create character with selected start
    var character = accountManager.CreateCharacterWithAccountProgress(
        accountId: accountId,
        characterClass: CharacterClass.Warrior,
        characterName: "Hero Name",
        alternativeStartId: selectedStart.StartId
    );
}

```

## Cosmetic System Integration

### Apply Cosmetic Loadout

```csharp
// Get active cosmetic loadout
var loadout = accountManager.GetActiveLoadout(accountId);
if (loadout != null)
{
    // Apply title
    if (!string.IsNullOrEmpty(loadout.SelectedTitle))
    {
        character.DisplayTitle = loadout.SelectedTitle;
    }

    // Apply UI theme
    if (!string.IsNullOrEmpty(loadout.SelectedUITheme))
    {
        ApplyUITheme(loadout.SelectedUITheme);
    }

    // Apply portrait
    if (!string.IsNullOrEmpty(loadout.SelectedPortrait))
    {
        character.PortraitId = loadout.SelectedPortrait;
    }
}

```

### Cosmetic Customization Menu

```csharp
public void ShowCosmeticMenu(int accountId)
{
    var unlockedCosmetics = accountManager.GetUnlockedCosmetics(accountId);

    Console.WriteLine("\\n=== COSMETIC CUSTOMIZATION ===");

    // Filter by type
    var titles = unlockedCosmetics.Where(c => c.Type == CosmeticType.Title).ToList();
    var portraits = unlockedCosmetics.Where(c => c.Type == CosmeticType.Portrait).ToList();
    var themes = unlockedCosmetics.Where(c => c.Type == CosmeticType.UITheme).ToList();

    Console.WriteLine($"\\nUnlocked Titles ({titles.Count}):");
    foreach (var title in titles)
    {
        Console.WriteLine($"  - {title.Name}");
    }

    Console.WriteLine($"\\nUnlocked Portraits ({portraits.Count}):");
    foreach (var portrait in portraits)
    {
        Console.WriteLine($"  - {portrait.Name}");
    }

    Console.WriteLine($"\\nUnlocked UI Themes ({themes.Count}):");
    foreach (var theme in themes)
    {
        Console.WriteLine($"  - {theme.Name}");
    }

    // Create/update loadout (implementation details omitted)
}

```

## Achievement Display

### Show Achievements Screen

```csharp
public void ShowAchievementsScreen(int accountId)
{
    var achievementsWithProgress = accountManager.GetAchievementsWithProgress(accountId);

    Console.WriteLine("\\n=== ACHIEVEMENTS ===");

    // Group by category
    var categories = achievementsWithProgress
        .GroupBy(a => a.Achievement.Category)
        .OrderBy(g => g.Key);

    foreach (var category in categories)
    {
        Console.WriteLine($"\\n{category.Key}:");

        foreach (var (achievement, progress) in category)
        {
            if (achievement.IsSecret && (progress == null || !progress.IsUnlocked))
            {
                Console.WriteLine($"  [SECRET] ??? - {achievement.AchievementPoints} pts");
                continue;
            }

            var unlocked = progress?.IsUnlocked ?? false;
            var status = unlocked ? "[✓]" : "[ ]";
            var progressText = unlocked
                ? "UNLOCKED"
                : $"{progress?.CurrentProgress ?? 0}/{achievement.RequiredProgress}";

            Console.WriteLine($"  {status} {achievement.Name} - {achievement.AchievementPoints} pts");
            Console.WriteLine($"      {achievement.Description}");
            Console.WriteLine($"      Progress: {progressText}");

            if (unlocked && !string.IsNullOrEmpty(achievement.FlavorText))
            {
                Console.WriteLine($"      \\"{achievement.FlavorText}\\"");
            }

            Console.WriteLine();
        }
    }
}

```

## Milestone Progress Display

### Show Milestone Progress

```csharp
public void ShowMilestoneProgress(int accountId)
{
    var currentTier = accountManager.GetCurrentMilestoneTier(accountId);
    var allTiers = accountManager.MilestoneService.GetAllTiers();
    var (currentPoints, requiredPoints, progressPercent) = accountManager.GetProgressToNextTier(accountId);

    Console.WriteLine("\\n=== MILESTONE PROGRESSION ===");
    Console.WriteLine($"\\nCurrent Tier: {currentTier?.TierName ?? "Unknown"} (Tier {currentTier?.TierNumber ?? 0}/10)");
    Console.WriteLine($"Achievement Points: {currentPoints}");

    if (currentTier?.TierNumber < 10)
    {
        Console.WriteLine($"Progress to Next Tier: {progressPercent:F1}%");
        Console.WriteLine($"Points Needed: {requiredPoints - currentPoints}");
    }
    else
    {
        Console.WriteLine("MAX TIER REACHED!");
    }

    Console.WriteLine("\\n--- All Tiers ---");
    foreach (var tier in allTiers)
    {
        var achieved = currentTier != null && tier.TierNumber <= currentTier.TierNumber;
        var marker = achieved ? "[✓]" : "[ ]";

        Console.WriteLine($"{marker} Tier {tier.TierNumber}: {tier.TierName} ({tier.RequiredAchievementPoints} pts)");
        if (achieved || tier.TierNumber == (currentTier?.TierNumber ?? 0) + 1)
        {
            Console.WriteLine($"    {tier.Description}");
        }
    }
}

```

## Testing Integration

### Integration Test Example

```csharp
[Fact]
public void FullAccountProgressionFlow_ShouldWork()
{
    // Create account
    var accountManager = new AccountManager(testConnectionString);
    var accountId = accountManager.CreateAccount();

    // Create character
    var character = accountManager.CreateCharacterWithAccountProgress(
        accountId, CharacterClass.Warrior, "Test Hero");

    // Simulate gameplay
    accountManager.OnTutorialCompleted(accountId);
    accountManager.OnBossDefeated(accountId, "test_boss", flawless: true);
    accountManager.OnCampaignCompleted(accountId, traumaFree: false);
    accountManager.OnLevelReached(accountId, 20);

    // Verify progression
    var stats = accountManager.GetAccountStatistics(accountId);
    Assert.True(stats.TotalAchievementPoints > 0);
    Assert.True(stats.TotalAchievementsUnlocked > 0);

    // Verify tier progression
    accountManager.CheckMilestoneProgression(accountId);
    var tier = accountManager.GetCurrentMilestoneTier(accountId);
    Assert.NotNull(tier);
}

```

## Performance Considerations

### Batching Achievement Checks

For performance, batch achievement checks when possible:

```csharp
// Instead of checking after every enemy kill:
// ❌ BAD
foreach (var enemy in enemiesKilled)
{
    accountManager.TrackAchievementProgress(accountId, "enemy_slayer");
}

// ✅ GOOD
var totalKilled = enemiesKilled.Count;
accountManager.TrackAchievementProgress(accountId, "enemy_slayer", totalKilled);

```

### Async Operations (Future Enhancement)

For production, consider making achievement tracking async:

```csharp
// Future enhancement
public async Task TrackAchievementProgressAsync(int accountId, string achievementId, int increment = 1)
{
    await Task.Run(() => TrackAchievementProgress(accountId, achievementId, increment));
}

```

## Troubleshooting

### Common Issues

**Issue:** Achievements not unlocking

- **Solution:** Verify `CheckMilestoneProgression` is called after achievement unlock
- **Solution:** Check achievement ID matches seeded achievement ID exactly

**Issue:** Account unlocks not applying to character

- **Solution:** Ensure `AccountManager` and services are passed to `CharacterFactory.CreateCharacter`
- **Solution:** Verify account ID is valid and exists in database

**Issue:** Alternative start not working

- **Solution:** Check alternative start is unlocked: `alternativeStartRepo.IsUnlocked(accountId, startId)`
- **Solution:** Verify alternative start ID exists in database

**Issue:** Cosmetics not persisting

- **Solution:** Ensure cosmetic loadout is saved with `CosmeticService.UpdateLoadout`
- **Solution:** Check loadout is set as active

## Summary

To fully integrate v0.41 meta-progression:

1. ✅ **Initialize AccountManager** in game startup
2. ✅ **Create characters** using `CreateCharacterWithAccountProgress`
3. ✅ **Add event tracking** to all gameplay systems:
    - Combat system → perfect clears, combos
    - Boss system → boss defeats, flawless kills
    - Quest system → quest completions
    - Progression → level-ups, specialization unlocks
    - Endgame modes → NG+ tiers, endless waves
4. ✅ **Display progression** in UI:
    - Account summary screen
    - Achievement screen
    - Cosmetic customization menu
    - Milestone progression display
5. ✅ **Test integration** with gameplay flow

The v0.41 system is designed to be **non-intrusive** - games can function without it, but gain rich progression when integrated.

---

**Questions?** See `IMPLEMENTATION_SUMMARY_V0.41.md` for architecture details.