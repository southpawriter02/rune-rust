# v0.41: Meta-Progression & Unlocks

Type: Feature
Description: Implements account-wide meta-progression transforming individual runs into persistent journey of unlocks and achievements. Delivers account-wide unlocks, alternative starting scenarios, 100+ achievement system, cosmetic customization with 50+ options, and 10-tier milestone system spanning 150-180 hours of content.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.40 (Endgame Content), v0.23 (Boss Encounters), v0.19 (Specialization System), v0.15 (Trauma Economy)
Implementation Difficulty: Hard
Balance Validated: No
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

# SPEC-METAPROGRESSION-001: Meta-Progression & Unlocks

**Version**: 1.0

**Status**: Design Phase

**Last Updated**: 2025-11-23

**Implemented In**: v0.41 (Planned)

**Prerequisites**: v0.40 (Endgame Content), v0.23 (Boss Encounters), v0.19 (Specialization System), v0.15 (Trauma Economy)

**Timeline**: 30-45 hours (4-6 weeks part-time)

**Philosophy**: Reward mastery across multiple runs with account-wide progression that enhances variety without creating power creep

---

## I. Executive Summary

v0.41 implements **account-wide meta-progression**, transforming individual character runs into a persistent journey of unlocks, achievements, and customization that spans the entire account.

**What v0.41 Delivers:**

- **Account-Wide Unlocks** (v0.41.1) - Permanent unlocks across all characters
- **Alternative Starting Scenarios** (v0.41.2) - Skip early game with unlocked starts
- **Achievement System** (v0.41.3) - 100+ achievements tracking mastery
- **Cosmetic Customization** (v0.41.4) - Visual customization with zero gameplay impact

**Strategic Purpose:**

v0.40 provides endgame challenge modes, but each character exists in isolation:

- ✅ New Game+ provides difficulty scaling
- ✅ Challenge Sectors test specific builds
- ✅ Boss Gauntlet and Endless Mode offer competitive play
- ✅ Leaderboards track performance

**But progression is siloed:**

- ❌ No benefits across multiple characters
- ❌ No reason to try different archetypes
- ❌ No visible record of accomplishments
- ❌ No customization options
- ❌ Restarting characters feels like complete reset

**v0.41 Solution:**

Build **account-wide progression** that spans all runs:

- ✅ Account unlocks: +5% starting Legend, extra starting equipment slot
- ✅ Alternative starts: Skip tutorial, start at level 5, unlock advanced specializations early
- ✅ 100+ achievements tracking all aspects of mastery
- ✅ Cosmetic customization: Titles, portraits, UI themes, ability visual effects
- ✅ Milestone system: Clear goals across 10-15 milestone tiers

### Before v0.41 (Current State)

```jsx
Progression:
- Each character run independent
- Completing game = no cross-character benefit
- No record of achievements
- No customization options

Result: Limited incentive for multiple playthroughs with different builds
```

### After v0.41 (Target State)

```jsx
Progression:
- Account-wide unlocks benefit all future characters
- Alternative starts skip repetitive early game
- Achievements visible on profile
- Cosmetic customization shows mastery
- Clear milestone progression across account

Result: Every run contributes to account progression
```

---

## II. Related Documentation

### Dependencies

**Upstream Systems** (must exist before v0.41):

- **v0.40: Endgame Content** ✅ Planned
    - Challenge Sector completion unlocks account rewards
    - Boss Gauntlet victories unlock titles
    - Endless Mode wave milestones unlock cosmetics
    - New Game+ tier completion unlocks alternative starts
- **v0.23: Boss Encounters** ✅ Complete
    - Boss-specific achievements
    - "Defeat X boss without taking damage" challenges
    - Boss kill counts tracked
- **v0.19: Specialization System** ✅ Complete
    - Specialization unlock system
    - Ability progression tracking
    - Archetype completion tracking
- **v0.15: Trauma Economy** ✅ Complete
    - "Complete run without acquiring Trauma" achievement
    - Corruption management achievements
    - Breaking Point avoidance tracking
- **v0.14: Quest System** ✅ Complete
    - Quest completion achievements
    - "Complete all quests in single run" tracking
- **v0.1-v0.12: Core Systems** ✅ Complete
    - Combat achievements (kill counts, perfect runs)
    - Progression achievements (reach level 20)
    - Equipment achievements (collect legendary sets)

**Downstream Systems** (will depend on v0.41):

- **v0.42: Seasonal Events** ⏳ Planned
    - Seasonal achievements with time-limited rewards
    - Seasonal cosmetics
    - Seasonal milestone tracks
- **v0.43: Social Features** ⏳ Planned
    - Display achievements on player profiles
    - Share cosmetic loadouts
    - Achievement comparisons
- **v2.0: Expanded Cosmetics** ⏳ Planned
    - Advanced cosmetic options
    - Cosmetic crafting system
    - Premium cosmetic store

### Code References

**Primary Implementation** (files to be created in v0.41):

- `AccountProgressionService.cs` (~700 lines): Account-wide unlock management
- `AchievementService.cs` (~800 lines): Achievement tracking and rewards
- `AlternativeStartService.cs` (~500 lines): Alternative scenario initialization
- `CosmeticService.cs` (~600 lines): Cosmetic application and validation
- `MilestoneService.cs` (~400 lines): Milestone tier progression

**Integration Points** (existing files to be modified):

- `CharacterCreationService.cs:lines 100-300`: Apply account unlocks to new characters
- `ProgressionService.cs:lines 200-400`: Track achievement progress
- `SaveService.cs:lines 150-350`: Persist account-wide data
- `UIService.cs:lines 100-250`: Display achievements and cosmetics
- `RewardService.cs:lines 50-200`: Distribute account unlock rewards

---

## III. Design Philosophy

### 1. Variety Over Power

**Principle**: Account unlocks enhance variety and convenience, never raw power.

**Design Rationale**:

Meta-progression systems often create power creep—players who grind gain insurmountable advantages, making early-game trivial and creating barriers for new players. v0.41 avoids this trap.

**Unlocks are Convenience, Not Power:**

❌ **Never Unlock:**

- Direct stat boosts (+10% damage for all characters)
- Starting level advantages (start at level 10)
- Exclusive powerful abilities unavailable to new accounts
- Resource multipliers (2x Legend gain)

✅ **Always Unlock:**

- **Variety**: Alternative starting scenarios (different build paths)
- **Convenience**: Skip tutorial, extra equipment loadout slot
- **Customization**: Cosmetic options (titles, portraits, UI themes)
- **Knowledge**: Unlocked lore entries, bestiary info

**Example Account Unlocks:**

| Unlock | Requirement | Benefit | Power Impact |
| --- | --- | --- | --- |
| Veteran's Start | Complete campaign once | Skip tutorial, start with basic equipment | Zero (tutorial optional) |
| Extra Loadout Slot | Complete NG+3 | +1 equipment loadout slot (convenience) | Zero (same gear, faster swaps) |
| Starting Legend +5% | Beat Boss Gauntlet | Slightly faster early levels | Minimal (~30 min saved) |
| Advanced Spec Unlock | Defeat 10 bosses | Unlock advanced specs at level 1 | Zero (specs balanced) |

**Why This Works:**

- New players not disadvantaged
- Veterans get convenience and variety
- No mandatory grind for competitive play
- Account progression feels rewarding without creating imbalance

### 2. Achievement as Narrative, Not Checklist

**Principle**: Achievements tell the story of player mastery, not arbitrary grinds.

**Design Rationale**:

Many achievement systems devolve into meaningless checklists—"kill 10,000 enemies," "collect 500 items." These don't celebrate mastery or memorable moments. v0.41 achievements tell stories.

**Achievement Categories:**

**1. Milestone Achievements (Progression Markers)**

- "First Steps" - Complete tutorial
- "Survivor" - Complete first campaign
- "Legend" - Reach level 20
- "Master" - Beat NG+5

**2. Mastery Achievements (Skill Expression)**

- "Untouchable" - Complete sector without taking damage
- "Flawless Victory" - Defeat boss without using healing
- "One Shot, One Kill" - Kill elite enemy in single attack
- "The Long Road" - Reach wave 50 in Endless Mode

**3. Discovery Achievements (World Exploration)**

- "Lorekeeper" - Unlock all codex entries for a biome
- "Bestiary Complete" - Examine all enemy types
- "Treasure Hunter" - Discover all hidden rooms
- "Cartographer" - Visit all biome types

**4. Challenge Achievements (Extreme Difficulty)**

- "Iron Will" - Complete run without acquiring Trauma
- "The Purist" - Complete campaign using only Tier 1 equipment
- "Solo Artist" - Beat Boss Gauntlet solo, no companions
- "Speed Demon" - Complete campaign in under 5 hours

**5. Narrative Achievements (Story Moments)**

- "The Truth Revealed" - Discover the cause of the Glitch
- "Mercy" - Spare a boss encounter
- "Vengeance" - Defeat the Iron-Bane Commander
- "Silence Broken" - Commune with a Forlorn

**Anti-Grind Design:**

- No "kill X enemies" achievements (X > 100)
- No "collect X items" achievements requiring hundreds
- No luck-based RNG achievements
- No achievements requiring months of daily play
- All achievements completable in 100-150 hours

### 3. Cosmetics as Expression, Not Status

**Principle**: Cosmetic customization enables personal expression without signaling elitism.

**Design Rationale**:

Many games use cosmetics to signal status—"look how much I grinded." This creates toxicity and FOMO. v0.41 cosmetics are expressive, not exclusive.

**Cosmetic Philosophy:**

**Accessibility:**

- Free cosmetics outnumber premium 10:1
- Every archetype has 5+ free cosmetic options
- Basic customization available from account creation
- Premium cosmetics never "better," just different

**Variety Over Rarity:**

- 50+ free titles unlockable through achievements
- 20+ free portraits from completing content
- 10+ UI themes from milestones
- 30+ ability visual effects from achievements

**No Artificial Scarcity:**

- No time-limited cosmetics (except seasonal)
- No random loot boxes
- No "only 100 players will ever have this"
- All cosmetics clearly explained how to unlock

**Example Cosmetic Options:**

| Cosmetic Type | Free Options | Example Unlocks |
| --- | --- | --- |
| Titles | 50+ | "Gauntlet Champion," "Survivor," "The Forsaken," "Blight-Walker" |
| Character Portraits | 20+ | Archetype variants, specialization-themed, achievement-based |
| UI Themes | 10+ | Muspelheim (fire), Niflheim (ice), Alfheim (light), Dark Mode |
| Ability Visual Effects | 30+ | Color variants, particle effects, sound variants |
| Combat Log Styles | 5+ | Minimal, Verbose, Cinematic, Technical, Lore-Heavy |

### 4. Milestones Provide Clear Goals

**Principle**: Milestone system creates clear, achievable goals across account progression.

**Design Rationale**:

Players need clear sense of progress. v0.41 uses milestone tiers to create progression ladder from "new player" to "master."

**Milestone Tier Structure (10 Tiers):**

```jsx
Milestone Tier Progression:

Tier 1: "Initiate" - Complete tutorial (unlock: Skip Tutorial)
Tier 2: "Survivor" - Complete campaign (unlock: Veteran's Start)
Tier 3: "Veteran" - Beat NG+1 (unlock: +5% starting Legend)
Tier 4: "Challenger" - Complete 5 Challenge Sectors (unlock: UI Theme)
Tier 5: "Champion" - Beat Boss Gauntlet (unlock: Extra Loadout Slot)
Tier 6: "Legend" - Reach level 20 (unlock: Portrait Pack 1)
Tier 7: "Master" - Beat NG+3 (unlock: Alternative Start: Advanced)
Tier 8: "Conqueror" - Complete 15 Challenge Sectors (unlock: Ability VFX Pack)
Tier 9: "Immortal" - Reach wave 30 Endless (unlock: Title Pack 2)
Tier 10: "Transcendent" - Beat NG+5 (unlock: Legendary Cosmetic Set)
```

**Each Tier Unlocks:**

- 1 major account unlock (convenience or variety)
- 3-5 cosmetic options
- 1 alternative starting scenario
- Achievement point milestone reward

**Why Milestones Work:**

- Clear progression path
- Achievable goals (not "kill 10,000 enemies")
- Rewards feel substantial
- Motivates diverse playstyles (requires trying all modes)

### 5. Account Progression Respects Time

**Principle**: Account progression completable in finite time without daily grind.

**Design Rationale**:

Many meta-progression systems trap players in infinite treadmills—daily quests, battle passes, seasonal FOMO. v0.41 respects player time.

**Time Budget for Full Completion:**

| Milestone Tier | Cumulative Hours | Unlock |
| --- | --- | --- |
| Tier 1-3 | 30-40 hours | Core unlocks (skip tutorial, veteran start) |
| Tier 4-6 | 60-80 hours | Convenience unlocks (extra loadout, Legend boost) |
| Tier 7-9 | 100-130 hours | Advanced unlocks (alternative starts, cosmetics) |
| Tier 10 | 150-180 hours | Mastery unlocks (legendary cosmetics) |

**150-180 hours to unlock everything = reasonable commitment**

**No Daily Grind:**

- No daily quests
- No "log in every day for 30 days"
- No time-gated unlocks (except seasonal events)
- Play at your own pace

**No FOMO:**

- Core unlocks always available
- Seasonal content returns annually
- No "limited time only" pressure
- Can take breaks without penalty

---

## IV. System Overview

### Current State Analysis (v0.1-v0.40)

The game currently delivers complete gameplay with endgame modes:

**Character Progression (v0.1-v0.9)**:

- Individual character leveling
- Specialization unlocks per character
- Equipment progression per character
- No cross-character benefits

**Endgame Modes (v0.40)**:

- New Game+ with 5 difficulty tiers
- Challenge Sectors with unique rewards
- Boss Gauntlet with leaderboards
- Endless Mode with competitive scoring

**Strengths**:

- ✅ Complete character progression per run
- ✅ Multiple endgame challenge modes
- ✅ Competitive leaderboards
- ✅ Replayability through difficulty scaling

**Limitations (Why v0.41 is Needed)**:

- ❌ **No Cross-Character Benefits**: Each character isolated
- ❌ **No Visible Accomplishments**: No achievement display
- ❌ **No Customization**: All characters look identical
- ❌ **Repetitive Early Game**: Must replay tutorial every character
- ❌ **No Account-Level Goals**: Progression resets per character

### Scope Definition

**✅ In Scope (v0.41):**

- Account-wide unlock system
- 15-20 major account unlocks
- Alternative starting scenarios (5-7 scenarios)
- Achievement system (100+ achievements)
- Achievement point tracking and rewards
- Cosmetic customization system
- 50+ cosmetic options (titles, portraits, UI themes)
- Milestone tier system (10 tiers)
- Database schema for account progression
- Service architecture for all systems
- Integration with character creation
- Integration with v0.40 endgame modes
- 80%+ unit test coverage

**❌ Out of Scope:**

- Premium cosmetic store (defer to v2.0+)
- Social features (profile sharing, comparisons) (defer to v0.43)
- Seasonal events (defer to v0.42)
- Advanced cosmetic crafting (defer to v2.0+)
- Battle pass system (not planned)
- Daily quest system (not planned)
- Account reputation/prestige system (defer to v2.0+)
- New gameplay content (use existing systems)

**Why These Limits**: v0.41 is account progression architecture only. Social features are v0.43. Premium systems are v2.0+. No live service mechanics.

### System Lifecycle

```jsx
ACCOUNT CREATION
  ↓
┌─────────────────────────────────────────────────────────────┐
│ INITIALIZE ACCOUNT PROGRESSION                              │
│   - Create account progression record                       │
│   - Set milestone tier to 1 (Initiate)                      │
│   - Unlock basic cosmetics                                  │
│   - Initialize achievement tracking                         │
└─────────────────────────────────────────────────────────────┘
  ↓
┌─────────────────────────────────────────────────────────────┐
│ CHARACTER CREATION                                          │
│   - Apply account unlocks to new character                  │
│   - Offer alternative starting scenarios (if unlocked)      │
│   - Apply cosmetic selections                               │
└─────────────────────────────────────────────────────────────┘
  ↓
┌─────────────────────────────────────────────────────────────┐
│ GAMEPLAY LOOP                                               │
│   - Track achievement progress across all activities        │
│   - Award achievement points on completion                  │
│   - Check milestone tier progression                        │
└─────────────────────────────────────────────────────────────┘
  ↓
┌─────────────────────────────────────────────────────────────┐
│ ACHIEVEMENT UNLOCKED                                        │
│   - Display achievement notification                        │
│   - Award achievement points                                │
│   - Unlock associated cosmetics/rewards                     │
│   - Check if milestone tier threshold reached               │
└─────────────────────────────────────────────────────────────┘
  ↓
┌─────────────────────────────────────────────────────────────┐
│ MILESTONE TIER REACHED                                      │
│   - Display milestone tier notification                     │
│   - Unlock tier rewards (account unlocks, cosmetics)        │
│   - Persist to account progression                          │
└─────────────────────────────────────────────────────────────┘
  ↓
RETURN TO CHARACTER CREATION (benefits all future characters)
```

---

## V. Functional Requirements

### FR-001: Account-Wide Unlock System

**Requirement**: Players shall earn permanent account-wide unlocks that benefit all future characters.

**Account Unlock Categories:**

```csharp
public enum AccountUnlockType
{
    Convenience,           // Quality of life improvements
    Variety,               // Alternative options
    Progression,           // Minor progression boosts
    Cosmetic,              // Visual customization
    Knowledge              // Lore and information
}

public class AccountUnlock
{
    public string UnlockId { get; set; }
    public string Name { get; set; }
    public AccountUnlockType Type { get; set; }
    public string Description { get; set; }
    public string RequirementDescription { get; set; }
    public bool IsUnlocked { get; set; }
    public DateTime? UnlockedAt { get; set; }
}
```

**Major Account Unlocks:**

| Unlock | Type | Requirement | Benefit |
| --- | --- | --- | --- |
| Skip Tutorial | Convenience | Complete tutorial once | New characters skip tutorial |
| Veteran's Start | Variety | Complete campaign | Start with basic equipment set |
| Legend Boost +5% | Progression | Beat Boss Gauntlet | +5% Legend gain (faster early levels) |
| Extra Loadout Slot | Convenience | Complete NG+3 | +1 equipment loadout for quick swaps |
| Advanced Spec Unlock | Variety | Defeat 10 bosses | Unlock advanced specs at level 1 |
| Starting Resources +50% | Progression | Reach wave 30 Endless | Start with 50% more consumables |
| Bestiary Auto-Complete | Knowledge | Examine 50 enemy types | New characters have partial bestiary |
| Codex Persistence | Knowledge | Unlock 30 codex entries | Codex entries carry across characters |
| Fast Travel Unlock | Convenience | Complete NG+5 | Unlock fast travel between sectors |
| Crafting Mastery | Variety | Craft 50 items | -10% crafting material costs |

**Service Implementation:**

```csharp
public class AccountProgressionService
{
    private readonly ILogger<AccountProgressionService> _logger;
    private readonly IAccountRepository _accountRepo;
    private readonly ICharacterRepository _characterRepo;
    
    public async Task<List<AccountUnlock>> GetUnlockedBenefitsAsync(int accountId)
    {
        _logger.Information(
            "Retrieving account unlocks: AccountID={AccountID}",
            accountId);
        
        var account = await _accountRepo.GetByIdAsync(accountId);
        var unlocks = await _accountRepo.GetUnlocksAsync(accountId);
        
        return unlocks.Where(u => u.IsUnlocked).ToList();
    }
    
    public async Task ApplyAccountUnlocksToCharacter(
        int accountId, 
        int characterId)
    {
        _logger.Information(
            "Applying account unlocks: AccountID={AccountID}, CharacterID={CharacterID}",
            accountId, characterId);
        
        var unlocks = await GetUnlockedBenefitsAsync(accountId);
        var character = await _characterRepo.GetByIdAsync(characterId);
        
        foreach (var unlock in unlocks)
        {
            ApplyUnlockToCharacter(character, unlock);
        }
        
        await _characterRepo.UpdateAsync(character);
        
        _logger.Information(
            "Applied {UnlockCount} account unlocks to character",
            unlocks.Count);
    }
    
    private void ApplyUnlockToCharacter(Character character, AccountUnlock unlock)
    {
        switch (unlock.UnlockId)
        {
            case "legend_boost_5":
                character.LegendMultiplier *= 1.05f;
                break;
            case "starting_resources_50":
                character.StartingResources *= 1.5f;
                break;
            case "extra_loadout_slot":
                character.MaxEquipmentLoadouts += 1;
                break;
            // ... additional unlock applications
        }
    }
}
```

### FR-002: Alternative Starting Scenarios

**Requirement**: Players shall unlock alternative starting scenarios that modify character creation and early game experience.

**Starting Scenario Structure:**

```csharp
public class AlternativeStart
{
    public string StartId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string FlavorText { get; set; }
    
    // Unlock requirements
    public string RequirementDescription { get; set; }
    public bool IsUnlocked { get; set; }
    
    // Starting modifications
    public int StartingLevel { get; set; } = 1;
    public List<string> StartingEquipment { get; set; }
    public List<string> UnlockedSpecializations { get; set; }
    public int StartingLegend { get; set; }
    public Dictionary<string, int> StartingResources { get; set; }
    public string StartingLocation { get; set; }  // Sector ID
    public List<string> CompletedQuests { get; set; }  // Skip early quests
}
```

**Alternative Starting Scenarios:**

| Scenario | Unlock Requirement | Starting Modifications | Benefit |
| --- | --- | --- | --- |
| "Standard Start" | None (default) | Level 1, tutorial, basic equipment | Full campaign experience |
| "Veteran's Return" | Complete campaign once | Skip tutorial, basic equipment set, starting location: Sector 2 | Skip repetitive tutorial |
| "Advanced Explorer" | Beat NG+3 | Level 5, advanced specs unlocked early, quality equipment | Test endgame builds faster |
| "Challenge Seeker" | Complete 10 Challenge Sectors | Level 1, no equipment, hard mode enabled | Maximum difficulty from start |
| "Lorekeeper's Path" | Unlock 50 codex entries | Partial bestiary, full codex, special dialogue options | Narrative-focused playthrough |
| "Speedrunner" | Complete campaign in <5 hours | Timer visible, optimized equipment, fast travel unlocked | Optimized for speed |
| "Ironborn" | Beat Boss Gauntlet solo | Permadeath enabled, +50% rewards, hardcore mode | Ultimate challenge |

**Scenario Selection UI:**

```jsx
=== CHARACTER CREATION ===
[Select Starting Scenario]

> Standard Start
  "Begin your journey from the beginning."
  Level 1 | Tutorial | Full Campaign

  Veteran's Return [UNLOCKED]
  "Skip the basics and get to the action."
  Skip Tutorial | Basic Equipment | Sector 2 Start
  
  Advanced Explorer [LOCKED]
  Requirement: Beat NG+3
  
  Challenge Seeker [LOCKED]
  Requirement: Complete 10 Challenge Sectors
  
[Select to continue]
```

**Alternative Start Service:**

```csharp
public class AlternativeStartService
{
    private readonly ILogger<AlternativeStartService> _logger;
    private readonly IAlternativeStartRepository _startRepo;
    
    public async Task<Character> InitializeCharacterWithScenario(
        int accountId,
        string startScenarioId,
        CharacterCreationData baseData)
    {
        _logger.Information(
            "Initializing character with scenario: AccountID={AccountID}, Scenario={Scenario}",
            accountId, startScenarioId);
        
        var scenario = await _startRepo.GetScenarioAsync(startScenarioId);
        
        if (!scenario.IsUnlocked)
        {
            throw new InvalidOperationException(
                $"Scenario {startScenarioId} not unlocked for account");
        }
        
        // Create base character
        var character = CreateBaseCharacter(baseData);
        
        // Apply scenario modifications
        ApplyScenarioModifications(character, scenario);
        
        _logger.Information(
            "Character initialized with scenario: {Scenario}, Level={Level}",
            [scenario.Name](http://scenario.Name), character.Level);
        
        return character;
    }
}
```

### FR-003: Achievement System

**Requirement**: Players shall earn 100+ achievements tracking mastery across all gameplay aspects.

**Achievement Structure:**

```csharp
public enum AchievementCategory
{
    Milestone,       // Campaign progression
    Combat,          // Combat mastery
    Exploration,     // Discovery
    Challenge,       // Extreme difficulty
    Narrative,       // Story moments
    Collection,      // Bestiary/codex completion
    Social           // Multiplayer/leaderboards (v2.0+)
}

public class Achievement
{
    public string AchievementId { get; set; }
    public string Name { get; set; }
    public AchievementCategory Category { get; set; }
    public string Description { get; set; }
    public string FlavorText { get; set; }
    public int AchievementPoints { get; set; }  // 5-50 points
    public bool IsSecret { get; set; }  // Hidden until unlocked
    public string IconId { get; set; }
    
    // Progress tracking
    public int CurrentProgress { get; set; }
    public int RequiredProgress { get; set; }
    public bool IsUnlocked { get; set; }
    public DateTime? UnlockedAt { get; set; }
    
    // Rewards
    public List<string> RewardIds { get; set; }  // Cosmetics, titles, etc.
}
```

**Achievement Categories & Examples:**

**Milestone Achievements (20 achievements, 5-10 points each):**

| Achievement | Description | Points | Reward |
| --- | --- | --- | --- |
| "First Steps" | Complete the tutorial | 5 | Title: "Initiate" |
| "Survivor" | Complete the campaign | 10 | Title: "Survivor" |
| "Legend" | Reach level 20 | 10 | Portrait: "Legendary Hero" |
| "Master of All" | Unlock all 4 archetype specializations | 15 | UI Theme: "Master" |
| "Transcendent" | Beat NG+5 | 20 | Title: "Transcendent" |

**Combat Achievements (30 achievements, 10-20 points each):**

| Achievement | Description | Points | Reward |
| --- | --- | --- | --- |
| "Untouchable" | Complete sector without taking damage | 15 | Title: "Untouchable" |
| "Flawless Victory" | Defeat boss without using healing | 20 | Ability VFX: "Flawless Glow" |
| "One Shot" | Kill elite enemy in single attack | 10 | Title: "Executioner" |
| "Combo Master" | Execute 20-hit combo | 15 | Combat Log Style: "Combo" |
| "Counter-Striker" | Land 50 counter-attacks | 10 | Ability VFX: "Counter Flash" |

**Challenge Achievements (20 achievements, 20-50 points each):**

| Achievement | Description | Points | Reward |
| --- | --- | --- | --- |
| "Iron Will" | Complete campaign without acquiring Trauma | 50 | Title: "Iron Will" + Portrait |
| "The Purist" | Beat campaign using only Tier 1 equipment | 30 | Title: "The Purist" |
| "Speed Demon" | Complete campaign in under 5 hours | 25 | UI Theme: "Speedrun" |
| "Gauntlet Master" | Beat Boss Gauntlet without using heals | 40 | Title: "Gauntlet Master" |
| "Endless Legend" | Reach wave 50 in Endless Mode | 35 | Portrait: "Endless" |

**Discovery Achievements (15 achievements, 5-15 points each):**

| Achievement | Description | Points | Reward |
| --- | --- | --- | --- |
| "Lorekeeper" | Unlock all codex entries for a biome | 10 | Title: "Lorekeeper" |
| "Bestiary Complete" | Examine all enemy types | 15 | Portrait: "Researcher" |
| "Cartographer" | Visit all 5 biome types | 10 | UI Theme Pack |
| "Treasure Hunter" | Discover 20 hidden rooms | 10 | Title: "Treasure Hunter" |
| "Secret Seeker" | Unlock 10 secret achievements | 20 | Title: "Secret Seeker" |

**Narrative Achievements (15 achievements, 10-25 points each, many SECRET):**

| Achievement | Description | Points | Secret? |
| --- | --- | --- | --- |
| "Truth Revealed" | Discover cause of the Glitch | 15 | No |
| "Mercy" | Spare a boss encounter | 10 | Yes |
| "Vengeance" | Defeat Iron-Bane Commander | 15 | No |
| "Silence Broken" | Successfully commune with Forlorn | 20 | Yes |
| "All Roads" | Complete all faction quest chains | 25 | No |

**Achievement Service Implementation:**

```csharp
public class AchievementService
{
    private readonly ILogger<AchievementService> _logger;
    private readonly IAchievementRepository _achievementRepo;
    private readonly IAccountRepository _accountRepo;
    
    public async Task TrackProgressAsync(
        int accountId,
        string achievementId,
        int progressIncrement = 1)
    {
        var achievement = await _achievementRepo.GetAsync(achievementId);
        var accountProgress = await _accountRepo.GetAchievementProgressAsync(
            accountId, achievementId);
        
        accountProgress.CurrentProgress += progressIncrement;
        
        _logger.Debug(
            "Achievement progress: {Achievement} = {Current}/{Required}",
            [achievement.Name](http://achievement.Name),
            accountProgress.CurrentProgress,
            achievement.RequiredProgress);
        
        if (accountProgress.CurrentProgress >= achievement.RequiredProgress 
            && !achievement.IsUnlocked)
        {
            await UnlockAchievementAsync(accountId, achievement);
        }
        
        await _accountRepo.UpdateAchievementProgressAsync(accountProgress);
    }
    
    private async Task UnlockAchievementAsync(
        int accountId,
        Achievement achievement)
    {
        achievement.IsUnlocked = true;
        achievement.UnlockedAt = DateTime.UtcNow;
        
        _logger.Information(
            "Achievement unlocked: AccountID={AccountID}, Achievement={Achievement}, Points={Points}",
            accountId,
            [achievement.Name](http://achievement.Name),
            achievement.AchievementPoints);
        
        // Award achievement points
        var account = await _accountRepo.GetByIdAsync(accountId);
        account.TotalAchievementPoints += achievement.AchievementPoints;
        await _accountRepo.UpdateAsync(account);
        
        // Unlock rewards (cosmetics, titles, etc.)
        foreach (var rewardId in achievement.RewardIds)
        {
            await UnlockRewardAsync(accountId, rewardId);
        }
        
        // Check milestone tier progression
        await CheckMilestoneTierProgressionAsync(accountId);
    }
}
```

### FR-004: Cosmetic Customization System

**Requirement**: Players shall customize character appearance and UI with cosmetic options earned through achievements.

**Cosmetic Types:**

```csharp
public enum CosmeticType
{
    Title,              // Display title
    Portrait,           // Character portrait
    UITheme,            // Interface color scheme
    AbilityVFX,         // Ability visual effect variant
    CombatLogStyle,     // Combat log formatting
    CharacterFrame,     // Portrait frame decoration
    Emblem              // Account emblem/badge
}

public class Cosmetic
{
    public string CosmeticId { get; set; }
    public string Name { get; set; }
    public CosmeticType Type { get; set; }
    public string Description { get; set; }
    public string PreviewImageUrl { get; set; }
    
    // Unlock requirements
    public string UnlockRequirement { get; set; }
    public bool IsUnlocked { get; set; }
    public DateTime? UnlockedAt { get; set; }
    
    // Application data
    public Dictionary<string, string> Parameters { get; set; }
}
```

**Title Cosmetics (50+ titles):**

| Title | Unlock Requirement | Display |
| --- | --- | --- |
| "Survivor" | Complete campaign | <PlayerName> the Survivor |
| "Gauntlet Champion" | Beat Boss Gauntlet | <PlayerName>, Gauntlet Champion |
| "The Forsaken" | Reach wave 40 Endless | <PlayerName> the Forsaken |
| "Blight-Walker" | Defeat 100 Blighted enemies | <PlayerName>, Blight-Walker |
| "Iron Will" | Complete run without Trauma | <PlayerName> of Iron Will |

**Portrait Cosmetics (20+ portraits):**

- **Archetype Variants**: Warrior (3 variants), Adept (3), Mystic (3), Skirmisher (3)
- **Specialization-Themed**: Berserkr (fury visual), Seidkona (psychic aura), Einbui (survivor)
- **Achievement-Based**: "Legendary Hero," "Gauntlet Master," "Endless Legend"
- **Milestone Rewards**: Tier 6, Tier 8, Tier 10 portraits

**UI Theme Cosmetics (10+ themes):**

| Theme | Unlock Requirement | Color Scheme |
| --- | --- | --- |
| "Default" | None | Gray/Blue (standard) |
| "Muspelheim Forge" | Complete Muspelheim biome | Red/Orange (fire) |
| "Niflheim Frost" | Complete Niflheim biome | Cyan/White (ice) |
| "Alfheim Light" | Complete Alfheim biome | Gold/Purple (light) |
| "Dark Mode" | Milestone Tier 5 | Black/Gray (minimal) |
| "High Contrast" | Unlock accessibility options | Black/Yellow (accessibility) |

**Ability VFX Cosmetics (30+ variants):**

- **Color Variants**: Red flames, blue flames, green flames (for fire abilities)
- **Particle Effects**: Sparks, smoke, lightning, frost shards
- **Sound Variants**: Different impact sounds, casting sounds
- **Unlock via achievements**: Combat mastery, boss victories, challenge sectors

**Cosmetic Loadout System:**

```csharp
public class CosmeticLoadout
{
    public int LoadoutId { get; set; }
    public int AccountId { get; set; }
    public string LoadoutName { get; set; }
    
    public string SelectedTitle { get; set; }
    public string SelectedPortrait { get; set; }
    public string SelectedUITheme { get; set; }
    public string SelectedCharacterFrame { get; set; }
    public string SelectedEmblem { get; set; }
    
    public Dictionary<string, string> AbilityVFXOverrides { get; set; }
    public string CombatLogStyle { get; set; }
}
```

**Cosmetic Service:**

```csharp
public class CosmeticService
{
    private readonly ILogger<CosmeticService> _logger;
    private readonly ICosmeticRepository _cosmeticRepo;
    
    public async Task ApplyCosmeticLoadoutAsync(
        int accountId,
        int loadoutId)
    {
        var loadout = await _cosmeticRepo.GetLoadoutAsync(loadoutId);
        
        // Validate all cosmetics are unlocked
        await ValidateCosmeticsUnlockedAsync(accountId, loadout);
        
        // Apply loadout to account
        await _cosmeticRepo.SetActiveLoadoutAsync(accountId, loadoutId);
        
        _logger.Information(
            "Applied cosmetic loadout: AccountID={AccountID}, Loadout={Loadout}",
            accountId, loadout.LoadoutName);
    }
    
    public async Task<bool> IsCosmeticUnlockedAsync(
        int accountId,
        string cosmeticId)
    {
        var cosmetic = await _cosmeticRepo.GetCosmeticAsync(cosmeticId);
        var accountProgress = await _cosmeticRepo.GetAccountCosmeticProgressAsync(
            accountId, cosmeticId);
        
        return accountProgress.IsUnlocked;
    }
}
```

### FR-005: Milestone Tier System

**Requirement**: Players shall progress through 10 milestone tiers, each unlocking major account benefits.

**Milestone Tier Structure:**

```csharp
public class MilestoneTier
{
    public int TierNumber { get; set; }  // 1-10
    public string TierName { get; set; }
    public string Description { get; set; }
    public int RequiredAchievementPoints { get; set; }
    
    public List<string> UnlockRewards { get; set; }  // Account unlock IDs
    public List<string> CosmeticRewards { get; set; }  // Cosmetic IDs
    public string AlternativeStartUnlock { get; set; }  // Alternative start ID
}
```

**10 Milestone Tiers:**

| Tier | Name | Points Required | Major Unlock | Additional Rewards |
| --- | --- | --- | --- | --- |
| 1 | Initiate | 0 | Skip Tutorial | 3 basic titles, 1 portrait |
| 2 | Survivor | 50 | Veteran's Start scenario | 5 titles, 2 portraits, UI theme |
| 3 | Veteran | 150 | Legend Boost +5% | 5 titles, 2 portraits, 3 ability VFX |
| 4 | Challenger | 300 | Challenge Seeker scenario | UI theme, 5 titles, 5 ability VFX |
| 5 | Champion | 500 | Extra Loadout Slot | Dark Mode UI, 5 titles, portrait pack |
| 6 | Legend | 750 | Starting Resources +50% | Legendary portrait, 10 titles, emblem |
| 7 | Master | 1000 | Advanced Explorer scenario | Master UI theme, 10 ability VFX |
| 8 | Conqueror | 1500 | Crafting Mastery | Ability VFX pack, 10 titles, frame pack |
| 9 | Immortal | 2000 | Bestiary Auto-Complete | Legendary titles, portrait, emblem |
| 10 | Transcendent | 3000 | Fast Travel + Ironborn scenario | Legendary cosmetic set (all types) |

**Tier Progression Calculation:**

```csharp
public class MilestoneService
{
    private readonly ILogger<MilestoneService> _logger;
    private readonly IMilestoneRepository _milestoneRepo;
    private readonly IAccountRepository _accountRepo;
    
    public async Task CheckMilestoneTierProgressionAsync(int accountId)
    {
        var account = await _accountRepo.GetByIdAsync(accountId);
        var currentTier = await _milestoneRepo.GetCurrentTierAsync(accountId);
        var nextTier = await _milestoneRepo.GetTierAsync(currentTier.TierNumber + 1);
        
        if (nextTier == null)
        {
            // Max tier reached
            return;
        }
        
        if (account.TotalAchievementPoints >= nextTier.RequiredAchievementPoints)
        {
            await AdvanceToNextTierAsync(accountId, nextTier);
        }
    }
    
    private async Task AdvanceToNextTierAsync(
        int accountId,
        MilestoneTier nextTier)
    {
        _logger.Information(
            "Milestone tier advanced: AccountID={AccountID}, Tier={Tier}",
            accountId, nextTier.TierName);
        
        // Update account tier
        var account = await _accountRepo.GetByIdAsync(accountId);
        account.CurrentMilestoneTier = nextTier.TierNumber;
        await _accountRepo.UpdateAsync(account);
        
        // Unlock tier rewards
        foreach (var unlockId in nextTier.UnlockRewards)
        {
            await UnlockAccountBenefitAsync(accountId, unlockId);
        }
        
        foreach (var cosmeticId in nextTier.CosmeticRewards)
        {
            await UnlockCosmeticAsync(accountId, cosmeticId);
        }
        
        if (!string.IsNullOrEmpty(nextTier.AlternativeStartUnlock))
        {
            await UnlockAlternativeStartAsync(accountId, nextTier.AlternativeStartUnlock);
        }
        
        // Display milestone notification to player
        await DisplayMilestoneNotificationAsync(accountId, nextTier);
    }
}
```

### FR-006: Achievement Point Economy

**Requirement**: Achievements shall award points contributing to milestone tier progression and unlock thresholds.

**Point Value Guidelines:**

| Achievement Difficulty | Point Value | Examples |
| --- | --- | --- |
| Trivial | 5 points | Complete tutorial, defeat first boss |
| Easy | 10 points | Complete campaign, reach level 10 |
| Moderate | 15-20 points | Beat NG+1, complete 5 Challenge Sectors |
| Hard | 25-30 points | Beat NG+3, reach wave 30 Endless |
| Very Hard | 35-40 points | Beat Boss Gauntlet no heals, complete run without Trauma |
| Extreme | 50 points | Beat NG+5, reach wave 50 Endless, Ironborn victory |

**Total Achievement Points Available: ~3,500 points**

- Milestone achievements: 200 points
- Combat achievements: 450 points
- Challenge achievements: 650 points
- Discovery achievements: 150 points
- Narrative achievements: 250 points
- Collection achievements: 200 points
- Secret achievements: 300 points
- Misc achievements: 1,300 points

**Milestone Tier 10 requires 3,000 points = 85% of total achievements**

This ensures players must engage with most content types, not just grind one category.

---

## VI. Child Specifications Overview

### v0.41.1: Account-Wide Unlocks & Alternative Starts (8-12 hours)

**Focus:** Implement account unlock system and alternative starting scenarios

**Deliverables:**

- Account unlock system (15-20 unlocks)
- Alternative starting scenarios (5-7 scenarios)
- Unlock application to new characters
- Scenario selection UI
- Database schema for account progression
- Integration with character creation

**Database Changes:**

```sql
CREATE TABLE Account_Progression (
    account_id INTEGER PRIMARY KEY,
    total_achievement_points INTEGER DEFAULT 0,
    current_milestone_tier INTEGER DEFAULT 1,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE Account_Unlocks (
    unlock_id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    unlock_type TEXT,
    description TEXT,
    requirement_description TEXT
);

CREATE TABLE Account_Unlock_Progress (
    progress_id INTEGER PRIMARY KEY,
    account_id INTEGER,
    unlock_id TEXT,
    is_unlocked BOOLEAN DEFAULT 0,
    unlocked_at DATETIME,
    FOREIGN KEY (account_id) REFERENCES Account_Progression(account_id),
    FOREIGN KEY (unlock_id) REFERENCES Account_Unlocks(unlock_id)
);

CREATE TABLE Alternative_Starts (
    start_id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    description TEXT,
    flavor_text TEXT,
    requirement_description TEXT,
    starting_level INTEGER DEFAULT 1,
    starting_equipment TEXT, -- JSON
    starting_location TEXT,
    modifications TEXT -- JSON
);
```

**Key Services:**

- `AccountProgressionService`: Manages account-wide data
- `AlternativeStartService`: Handles scenario selection and application
- `UnlockService`: Tracks and applies unlocks

### v0.41.2: Achievement System (8-12 hours)

**Focus:** Implement comprehensive achievement tracking

**Deliverables:**

- Achievement definitions (100+ achievements)
- Achievement tracking system
- Progress monitoring across all gameplay
- Achievement unlock notifications
- Achievement point awards
- Secret achievement handling
- Integration with all gameplay systems

**Database Changes:**

```sql
CREATE TABLE Achievements (
    achievement_id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    category TEXT,
    description TEXT,
    flavor_text TEXT,
    achievement_points INTEGER,
    is_secret BOOLEAN DEFAULT 0,
    icon_id TEXT,
    required_progress INTEGER DEFAULT 1
);

CREATE TABLE Achievement_Progress (
    progress_id INTEGER PRIMARY KEY,
    account_id INTEGER,
    achievement_id TEXT,
    current_progress INTEGER DEFAULT 0,
    is_unlocked BOOLEAN DEFAULT 0,
    unlocked_at DATETIME,
    FOREIGN KEY (account_id) REFERENCES Account_Progression(account_id),
    FOREIGN KEY (achievement_id) REFERENCES Achievements(achievement_id)
);

CREATE TABLE Achievement_Rewards (
    reward_id INTEGER PRIMARY KEY,
    achievement_id TEXT,
    reward_type TEXT, -- Cosmetic, Unlock, Title
    reward_item_id TEXT,
    FOREIGN KEY (achievement_id) REFERENCES Achievements(achievement_id)
);
```

**Key Services:**

- `AchievementService`: Core achievement tracking
- `AchievementTrackerService`: Monitors gameplay for achievement triggers
- `AchievementNotificationService`: Displays unlock notifications

### v0.41.3: Cosmetic Customization System (7-10 hours)

**Focus:** Implement cosmetic customization with 50+ options

**Deliverables:**

- Cosmetic definitions (50+ cosmetics)
- Cosmetic loadout system
- Title, portrait, UI theme application
- Ability VFX customization
- Cosmetic preview system
- Unlock tracking
- Integration with UI rendering

**Database Changes:**

```sql
CREATE TABLE Cosmetics (
    cosmetic_id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    cosmetic_type TEXT,
    description TEXT,
    preview_image_url TEXT,
    unlock_requirement TEXT,
    parameters TEXT -- JSON
);

CREATE TABLE Cosmetic_Progress (
    progress_id INTEGER PRIMARY KEY,
    account_id INTEGER,
    cosmetic_id TEXT,
    is_unlocked BOOLEAN DEFAULT 0,
    unlocked_at DATETIME,
    FOREIGN KEY (account_id) REFERENCES Account_Progression(account_id),
    FOREIGN KEY (cosmetic_id) REFERENCES Cosmetics(cosmetic_id)
);

CREATE TABLE Cosmetic_Loadouts (
    loadout_id INTEGER PRIMARY KEY,
    account_id INTEGER,
    loadout_name TEXT,
    selected_title TEXT,
    selected_portrait TEXT,
    selected_ui_theme TEXT,
    selected_frame TEXT,
    selected_emblem TEXT,
    ability_vfx_overrides TEXT, -- JSON
    combat_log_style TEXT,
    is_active BOOLEAN DEFAULT 0,
    FOREIGN KEY (account_id) REFERENCES Account_Progression(account_id)
);
```

**Key Services:**

- `CosmeticService`: Manages cosmetic unlocks and application
- `LoadoutService`: Handles cosmetic loadout management
- `UIThemeService`: Applies UI theme changes

### v0.41.4: Milestone Tier System & Integration (7-11 hours)

**Focus:** Implement 10-tier milestone progression

**Deliverables:**

- Milestone tier definitions (10 tiers)
- Tier progression tracking
- Tier unlock rewards
- Tier notification system
- Complete integration with achievements
- Complete integration with account unlocks
- Complete integration with cosmetics
- Final testing and validation

**Database Changes:**

```sql
CREATE TABLE Milestone_Tiers (
    tier_number INTEGER PRIMARY KEY,
    tier_name TEXT NOT NULL,
    description TEXT,
    required_achievement_points INTEGER,
    unlock_rewards TEXT, -- JSON array of unlock IDs
    cosmetic_rewards TEXT, -- JSON array of cosmetic IDs
    alternative_start_unlock TEXT
);

CREATE TABLE Account_Milestone_Progress (
    account_id INTEGER PRIMARY KEY,
    current_tier_number INTEGER DEFAULT 1,
    last_tier_reached_at DATETIME,
    FOREIGN KEY (account_id) REFERENCES Account_Progression(account_id),
    FOREIGN KEY (current_tier_number) REFERENCES Milestone_Tiers(tier_number)
);
```

**Key Services:**

- `MilestoneService`: Manages tier progression
- `MilestoneRewardService`: Distributes tier rewards
- `IntegrationTestingService`: Validates all v0.41 systems

---

## VII. Integration with Existing Systems

### v0.40 Endgame Content Integration

**Account unlocks from v0.40 completion:**

```jsx
v0.40 Mode Completion → v0.41 Unlock

New Game+ Tiers:
- NG+1 complete → Achievement "Veteran" (10 pts)
- NG+3 complete → Unlock: Extra Loadout Slot
- NG+5 complete → Achievement "Transcendent" (20 pts)

Challenge Sectors:
- Complete 5 → Achievement "Challenger" (15 pts)
- Complete 15 → Achievement "Conqueror" (25 pts)
- Complete all → Achievement "Master of Challenges" (50 pts)

Boss Gauntlet:
- Complete → Achievement "Gauntlet Champion" (20 pts)
- Complete no heals → Achievement "Gauntlet Master" (40 pts)
- Speedrun record → Leaderboard cosmetic unlock

Endless Mode:
- Wave 20 → Achievement "Endurance" (15 pts)
- Wave 30 → Achievement "Immortal" (25 pts)
- Wave 50 → Achievement "Endless Legend" (50 pts)
```

### v0.19 Specialization System Integration

**Specialization-based achievements:**

```jsx
Specialization Achievements:
- Unlock all 4 archetypes → "Master of All" (15 pts)
- Complete campaign with each archetype → "Versatile" (25 pts)
- Max out specialization tree → "Specialist" (10 pts)
- Unlock all abilities for archetype → "Arsenal" (20 pts)
```

### v0.23 Boss Encounters Integration

**Boss-specific achievements:**

```jsx
Boss Achievements:
- Defeat first boss → "Boss Slayer" (5 pts)
- Defeat 10 bosses → "Hunter" (15 pts)
- Defeat boss flawlessly (no damage) → "Untouchable" (20 pts per boss)
- Defeat all unique bosses → "Apex Predator" (30 pts)
```

### v0.15 Trauma Economy Integration

**Trauma-related achievements:**

```jsx
Trauma Achievements:
- Acquire first Trauma → "Scarred" (5 pts)
- Complete run without acquiring Trauma → "Iron Will" (50 pts)
- Reach max Corruption without corrupting → "Edge Walker" (30 pts)
- Complete run with 5+ active Traumas → "Broken" (25 pts)
```

---

## VIII. Success Criteria

**v0.41 is DONE when:**

### ✅ v0.41.1: Account Unlocks & Alternative Starts

- [ ]  15-20 account unlocks implemented
- [ ]  5-7 alternative starting scenarios defined
- [ ]  Unlocks correctly apply to new characters
- [ ]  Alternative start selection UI functional
- [ ]  Scenario modifications apply correctly
- [ ]  Integration with character creation successful

### ✅ v0.41.2: Achievement System

- [ ]  100+ achievements defined across 6 categories
- [ ]  Achievement tracking monitors all gameplay systems
- [ ]  Achievement unlocks award points correctly
- [ ]  Secret achievements remain hidden until unlocked
- [ ]  Achievement notifications display properly
- [ ]  Rewards distribute on unlock
- [ ]  Integration with v0.40 endgame modes successful

### ✅ v0.41.3: Cosmetic Customization

- [ ]  50+ cosmetic options implemented
- [ ]  Title, portrait, UI theme customization working
- [ ]  Ability VFX customization functional
- [ ]  Cosmetic loadout system operational
- [ ]  Loadout saving and loading works
- [ ]  Cosmetic preview displays correctly
- [ ]  Unlocked cosmetics persist across sessions

### ✅ v0.41.4: Milestone Tier System

- [ ]  10 milestone tiers defined with requirements
- [ ]  Tier progression tracks achievement points
- [ ]  Tier unlock rewards distribute correctly
- [ ]  Milestone notifications display
- [ ]  Complete integration with achievements tested
- [ ]  Complete integration with unlocks tested
- [ ]  Complete integration with cosmetics tested

### ✅ Quality Gates

- [ ]  80%+ unit test coverage achieved
- [ ]  All integration tests passing
- [ ]  Performance: Account data load <200ms
- [ ]  Comprehensive Serilog logging implemented
- [ ]  No critical bugs or crashes
- [ ]  v5.0 setting compliance verified
- [ ]  Complete documentation (architecture, schemas, services)

### ✅ Player Experience Validation

- [ ]  Playtester feedback: "Account progression feels rewarding"
- [ ]  Playtester feedback: "Cosmetics enable personal expression"
- [ ]  Playtester feedback: "Alternative starts reduce repetition"
- [ ]  Achievement completion rate: 30-50% for typical player
- [ ]  Milestone tier progression feels achievable

---

## IX. Timeline & Roadmap

**Phase 1: v0.41.1 - Account Unlocks & Alternative Starts** — 8-12 hours

- Week 1: Account unlock system and database
- Week 2: Alternative starting scenarios and character creation integration

**Phase 2: v0.41.2 - Achievement System** — 8-12 hours

- Week 3: Achievement definitions and tracking system
- Week 4: Achievement integration with gameplay systems

**Phase 3: v0.41.3 - Cosmetic Customization** — 7-10 hours

- Week 5: Cosmetic definitions and loadout system
- Week 6: UI integration and preview system

**Phase 4: v0.41.4 - Milestone Tiers & Integration** — 7-11 hours

- Week 7: Milestone tier system and progression
- Week 8: Complete integration testing

**Total: 30-45 hours (4-6 weeks part-time)**

---

## X. Benefits

### For Development

- ✅ **Player Retention**: Account progression encourages long-term engagement
- ✅ **Alt-Friendly**: Alternative starts reduce repetition
- ✅ **Replayability**: Achievements incentivize trying different builds
- ✅ **Data Insights**: Achievement tracking reveals player behavior

### For Gameplay

- ✅ **Progression Across Runs**: Every character contributes to account
- ✅ **Clear Goals**: Achievements and milestones provide direction
- ✅ **Personal Expression**: Cosmetics enable individuality
- ✅ **Convenience**: Unlocks remove tedious repetition

### For Community

- ✅ **Status Display**: Achievements visible on profiles (v0.43)
- ✅ **Aspirational Goals**: Challenge achievements inspire competition
- ✅ **Content Creators**: Achievement hunting creates streaming content
- ✅ **Social Proof**: Cosmetics signal mastery

---

## XI. After v0.41 Ships

**You'll Have:**

- ✅ Complete account-wide progression (15-20 unlocks)
- ✅ 100+ achievements tracking all aspects of mastery
- ✅ 5-7 alternative starting scenarios
- ✅ 50+ cosmetic customization options
- ✅ 10-tier milestone system with clear goals
- ✅ Foundation for social features (v0.43)

**Next Steps:**

- **v0.42:** Seasonal Events (seasonal achievements, time-limited cosmetics)
- **v0.43:** Social Features (profile sharing, achievement comparisons)
- **v2.0:** Premium Cosmetics (cosmetic store, advanced customization)

**Account progression transforms individual runs into a cohesive journey.**

---

**Ready to build a legacy.**