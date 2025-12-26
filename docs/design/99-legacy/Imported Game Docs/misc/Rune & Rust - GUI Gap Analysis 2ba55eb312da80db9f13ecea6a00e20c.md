# Rune & Rust - GUI Gap Analysis

## Core Systems vs. GUI Implementation Coverage

**Document Version:** 1.0
**Last Updated:** November 2024
**Purpose:** Identify critical gaps between terminal-side/core game content and current GUI implementation

---

## Table of Contents

1. [Executive Summary](Rune%20&%20Rust%20-%20GUI%20Gap%20Analysis%202ba55eb312da80db9f13ecea6a00e20c.md)
2. [Critical Gaps (No GUI Implementation)](Rune%20&%20Rust%20-%20GUI%20Gap%20Analysis%202ba55eb312da80db9f13ecea6a00e20c.md)
3. [Partial Implementations (Incomplete GUI)](Rune%20&%20Rust%20-%20GUI%20Gap%20Analysis%202ba55eb312da80db9f13ecea6a00e20c.md)
4. [TODO Items in Existing Code](Rune%20&%20Rust%20-%20GUI%20Gap%20Analysis%202ba55eb312da80db9f13ecea6a00e20c.md)
5. [Feature Parity Matrix](Rune%20&%20Rust%20-%20GUI%20Gap%20Analysis%202ba55eb312da80db9f13ecea6a00e20c.md)
6. [Priority Recommendations](Rune%20&%20Rust%20-%20GUI%20Gap%20Analysis%202ba55eb312da80db9f13ecea6a00e20c.md)

---

## 1. Executive Summary

### Coverage Statistics

| Category | Core Systems | GUI Implemented | Coverage |
| --- | --- | --- | --- |
| Character Management | 12 | 10 | 83% |
| Combat Systems | 15 | 11 | 73% |
| Exploration Systems | 14 | 9 | 64% |
| Item & Economy | 8 | 4 | 50% |
| Social & NPC | 6 | 0 | 0% |
| Meta-Progression | 5 | 4 | 80% |
| **Total** | **60** | **38** | **63%** |

### Critical Missing Systems

1. **Crafting System** - Full core implementation, zero GUI
2. **Merchant/Trading System** - Full core implementation, zero GUI
3. **Quest Journal** - Full core implementation, zero GUI
4. **Dialogue System** - Full core implementation, zero GUI
5. **Companion Management** - Partial core support, minimal GUI
6. **Faction Reputation** - Full core implementation, zero GUI

---

## 2. Critical Gaps (No GUI Implementation)

### 2.1 Crafting System

**Core Files:**

- `RuneAndRust.Core/CraftingRecipe.cs`
- `RuneAndRust.Core/CraftingComponent.cs`
- `RuneAndRust.Core/Crafting/CraftingModels.cs`

**Core Features Not Exposed:**

| Feature | Core Support | GUI Status |
| --- | --- | --- |
| Recipe Discovery | ✓ | Missing |
| Component Inventory | ✓ | Missing |
| Crafting Workbench UI | ✓ | Missing |
| Skill Check Integration | ✓ | Missing |
| Masterwork Crafting | ✓ | Missing |
| Bone-Setter Specialization Bonuses | ✓ | Missing |

**Required ViewModel:** `CraftingViewModel`

**Suggested UI Elements:**

- Recipe list with filter by type/rarity
- Component inventory display
- Crafting preview with success chance
- Skill check difficulty indicator
- Masterwork toggle for enhanced crafting
- Result preview with stat comparison

---

### 2.2 Merchant/Trading System

**Core Files:**

- `RuneAndRust.Core/Merchant.cs`

**Core Features Not Exposed:**

| Feature | Core Support | GUI Status |
| --- | --- | --- |
| Shop Inventory Browse | ✓ | Missing |
| Buy/Sell Transactions | ✓ | Missing |
| Price Negotiation (Barter Skill) | ✓ | Missing |
| Reputation Discounts | ✓ | Missing |
| Merchant Type Specializations | ✓ | Missing |
| Shop Restocking | ✓ | Missing |

**Core Merchant Types:**

- General
- Apothecary
- ScrapTrader
- Specialist

**Required ViewModel:** `MerchantViewModel` or `ShopViewModel`

**Suggested UI Elements:**

- Shop inventory grid with quality-colored borders
- Player inventory for selling
- Price display with markup/discount indicators
- Barter skill impact preview
- Category filters (Weapons, Armor, Consumables, Materials)
- "Sell All Junk" quick action
- Reputation standing indicator

---

### 2.3 Quest Journal System

**Core Files:**

- `RuneAndRust.Core/Quests/Quest.cs`
- `RuneAndRust.Core/Quests/QuestObjective.cs`
- `RuneAndRust.Core/Quests/QuestReward.cs`
- `RuneAndRust.Core/Quests/QuestGenerationRequirements.cs`
- `RuneAndRust.Core/QuestAnchor.cs`

**Core Features Not Exposed:**

| Feature | Core Support | GUI Status |
| --- | --- | --- |
| Active Quest List | ✓ | Missing |
| Quest Tracking | ✓ | Missing |
| Objective Progress | ✓ | Missing |
| Quest Categories | ✓ | Missing |
| Reward Preview | ✓ | Missing |
| Quest Log History | ✓ | Missing |
| Quest Abandonment | ✓ | Missing |

**Quest Types Defined:**

- Main
- Side
- Dynamic
- Repeatable

**Quest Categories:**

- Combat
- Exploration
- Retrieval
- Delivery
- Investigation
- Dialogue

**Quest Statuses:**

- NotStarted, Available, Active, Complete, Completed, TurnedIn, Failed, Abandoned

**Required ViewModel:** `QuestJournalViewModel`

**Suggested UI Elements:**

- Tabbed quest list (Active/Available/Completed/Failed)
- Quest detail panel with objectives checklist
- Objective progress bars
- Reward preview section
- Map waypoint integration
- "Track Quest" toggle
- Quest giver location hint

---

### 2.4 Dialogue System

**Core Files:**

- `RuneAndRust.Core/Dialogue/DialogueNode.cs`
- `RuneAndRust.Core/Dialogue/DialogueOption.cs`
- `RuneAndRust.Core/Dialogue/DialogueOutcome.cs`
- `RuneAndRust.Core/Dialogue/SkillCheckRequirement.cs`

**Core Features Not Exposed:**

| Feature | Core Support | GUI Status |
| --- | --- | --- |
| NPC Conversation UI | ✓ | Missing |
| Dialogue Choices | ✓ | Missing |
| Skill Check Options | ✓ | Missing |
| Dialogue Outcomes | ✓ | Missing |
| Topic Tracking | ✓ | Missing |
| Disposition Display | ✓ | Missing |

**Required ViewModel:** `DialogueViewModel`

**Suggested UI Elements:**

- NPC portrait and name header
- Dialogue text area with typewriter effect option
- Response options list with:
    - Skill check indicators (DC and attribute)
    - Success chance percentage
    - Locked options with requirement display
- Disposition meter
- Topic history sidebar
- Exit conversation button

---

### 2.5 Companion Management

**Core Files:**

- `RuneAndRust.Core/Companion.cs`
- `RuneAndRust.Core/CompanionAbility.cs`
- `RuneAndRust.Core/CompanionAction.cs`

**Core Features Not Exposed:**

| Feature | Core Support | GUI Status |
| --- | --- | --- |
| Companion Roster View | ✓ | Missing |
| Companion Stats Display | ✓ | Missing |
| Companion Equipment | ✓ | Missing |
| Companion Abilities | ✓ | Missing |
| Companion Recruitment | ✓ | Missing |
| Personal Quest Tracking | ✓ | Missing |
| Combat Role Assignment | ✓ | Missing |
| Direct Command Queue | ✓ | Missing |
| Party Formation | ✓ | Missing |

**Companion Properties Defined:**

- Background, Personality, FactionAffiliation
- CombatRole, Archetype
- Personal stats and equipment
- Loyalty/Relationship tracking
- Personal quest system

**Required ViewModel:** `CompanionManagementViewModel`

**Suggested UI Elements:**

- Party roster sidebar
- Companion detail panel with:
    - Portrait and name
    - Stats summary
    - Equipment slots
    - Ability list
    - Relationship meter
    - Personal quest status
- Recruitment interface for available companions
- Combat behavior configuration (Aggressive/Defensive/Support)
- Formation editor

---

### 2.6 Faction Reputation System

**Core Files:**

- `RuneAndRust.Core/Factions/Faction.cs`
- `RuneAndRust.Core/Factions/FactionReputation.cs`
- `RuneAndRust.Core/Factions/WitnessedAction.cs`
- `RuneAndRust.Core/FactionReputationSystem.cs`
- `RuneAndRust.Core/FactionType.cs`

**Core Features Not Exposed:**

| Feature | Core Support | GUI Status |
| --- | --- | --- |
| Faction Standing Display | ✓ | Missing |
| Reputation History | ✓ | Missing |
| Faction Relationships | ✓ | Missing |
| Faction Benefits/Penalties | ✓ | Missing |
| Allied/Enemy Indicators | ✓ | Missing |

**Required ViewModel:** `FactionReputationViewModel`

**Suggested UI Elements:**

- Faction list with reputation bars
- Detailed faction view:
    - Philosophy and description
    - Current standing (Hostile → Neutral → Friendly → Allied)
    - Recent actions affecting reputation
    - Benefits at current tier
    - Allied and enemy factions
- Territory control overlay integration

---

### 2.7 Territory Control System

**Core Files:**

- `RuneAndRust.Core/Territory/World.cs`
- `RuneAndRust.Core/Territory/Sector.cs`
- `RuneAndRust.Core/Territory/SectorControlState.cs`
- `RuneAndRust.Core/Territory/FactionInfluence.cs`
- `RuneAndRust.Core/Territory/FactionWar.cs`
- `RuneAndRust.Core/Territory/PlayerTerritorialAction.cs`
- `RuneAndRust.Core/Territory/NPCBehaviorModifier.cs`
- `RuneAndRust.Core/Territory/TerritorialQuestTemplate.cs`

**Core Features Not Exposed:**

| Feature | Core Support | GUI Status |
| --- | --- | --- |
| World Map View | ✓ | Missing |
| Sector Control Indicators | ✓ | Missing |
| Faction Influence Display | ✓ | Missing |
| Territory Actions | ✓ | Missing |
| Faction War Status | ✓ | Missing |
| NPC Behavior Changes | ✓ | Missing |

**Required ViewModel:** `WorldMapViewModel` or `TerritoryViewModel`

**Suggested UI Elements:**

- Sector-based world map
- Faction control color coding
- Influence percentage overlays
- Active conflicts indicators
- Player territory actions menu
- Sector detail panel with:
    - Controlling faction
    - Contested status
    - Available quests
    - NPC disposition modifiers

---

### 2.8 Leaderboard System

**Core Files:**

- `RuneAndRust.Core/EndlessMode/EndlessLeaderboardEntry.cs`
- `RuneAndRust.Core/BossGauntlet/GauntletLeaderboardEntry.cs`

**Core Features Not Exposed:**

| Feature | Core Support | GUI Status |
| --- | --- | --- |
| Endless Mode Leaderboard | ✓ | Missing |
| Boss Gauntlet Leaderboard | ✓ | Missing |
| Personal Best Tracking | ✓ | Missing |
| Seed Sharing | ✓ | Missing |

**Required ViewModel:** `LeaderboardViewModel`

**Suggested UI Elements:**

- Mode tabs (Endless / Gauntlet)
- Sortable columns (Score, Wave, Time, Character)
- Personal best highlighting
- Seed display and copy button
- Filter by class/specialization

---

## 3. Partial Implementations (Incomplete GUI)

### 3.1 Combat Movement System

**Current State:** Basic adjacent-cell movement only

**Core Support:**

- `BattlefieldGrid.cs` - Full pathfinding support
- `AdvancedMovementService` - Referenced but not integrated

**Missing GUI Features:**

| Feature | Priority |
| --- | --- |
| Movement range visualization | High |
| Path preview on hover | High |
| Terrain cost indicators | Medium |
| Zone transition warnings | Medium |
| Difficult terrain highlighting | Low |

---

### 3.2 Combat Item Usage

**Current State:** TODO placeholder in `CombatViewModel`

**Core Support:**

- Full consumable system exists
- Combat context item effects defined

**Missing GUI Features:**

| Feature | Priority |
| --- | --- |
| Item selection dialog | High |
| Target selection for items | High |
| Item effect preview | Medium |
| Cooldown tracking display | Low |

---

### 3.3 Combat Ability Selection

**Current State:** Uses first ability as demo

**Core Support:**

- Full ability system with types, targets, costs

**Missing GUI Features:**

| Feature | Priority |
| --- | --- |
| Ability selection panel | High |
| Target type visualization | High |
| Resource cost display | High |
| Cooldown indicators | Medium |
| Ability tooltips with full details | Medium |

---

### 3.4 Stance System in Combat

**Current State:** Display only in CharacterSheet

**Core Support:**

- `Stance.cs` - Full stance mechanics
- Free switch per turn tracking

**Missing GUI Features:**

| Feature | Priority |
| --- | --- |
| Stance toggle in combat HUD | High |
| Stance modifier preview | High |
| Switches remaining indicator | Medium |
| Stance effect tooltips | Low |

---

### 3.5 Counter-Attack/Parry System

**Core Files:**

- `RuneAndRust.Core/CounterAttack.cs`

**Current State:** No GUI exposure

**Missing GUI Features:**

| Feature | Priority |
| --- | --- |
| Reaction preparation button | High |
| Parries remaining display | High |
| Reaction trigger notification | Medium |
| Counter-attack result display | Medium |

---

### 3.6 Environmental Puzzle Solving

**Current State:** Puzzles detected but no solving UI

**Core Support:**

- Room puzzles exist in exploration
- Skill checks for puzzle solutions

**Missing GUI Features:**

| Feature | Priority |
| --- | --- |
| Puzzle interaction dialog | High |
| Skill check option display | High |
| Puzzle state visualization | Medium |
| Hint system | Low |

---

### 3.7 Resource Gathering

**Core Files:**

- `RuneAndRust.Core/Descriptors/ResourceNode.cs`

**Current State:** Resource nodes defined but no gathering UI

**Missing GUI Features:**

| Feature | Priority |
| --- | --- |
| Resource node interaction | Medium |
| Gathering minigame/check | Medium |
| Yield preview | Low |
| Tool requirements | Low |

---

### 3.8 NPC Interaction (Non-Dialogue)

**Core Files:**

- `RuneAndRust.Core/NPC.cs`
- `RuneAndRust.Core/NPCFlavor/` (3 files)

**Current State:** NPCs visible in rooms, no interaction

**Missing GUI Features:**

| Feature | Priority |
| --- | --- |
| NPC inspection/examine | High |
| NPC context menu | High |
| Disposition display | Medium |
| Quest giver indicators | Medium |
| Merchant indicators | Medium |

---

### 3.9 Trauma Management

**Current State:** Display only in CharacterSheet

**Core Files:**

- `RuneAndRust.Core/Trauma.cs`
- `RuneAndRust.Core/TraumaEffect.cs`

**Core Features:**

- Trauma progression (1-3 levels)
- Management tracking
- Rest restrictions
- Therapy/coping mechanics

**Missing GUI Features:**

| Feature | Priority |
| --- | --- |
| Trauma detail view | Medium |
| Management action interface | Medium |
| Progression warnings | Medium |
| Therapy NPC interaction | Low |

---

### 3.10 Galdr/Magic Casting Details

**Core Support:**

- `Ability.cs` has RuneSchool, Element properties
- `GaldrFlavor/` (7 files) for manifestation descriptors

**Current State:** Abilities displayed but no rune school filtering

**Missing GUI Features:**

| Feature | Priority |
| --- | --- |
| Rune school categorization | Low |
| Elemental affinity display | Low |
| Casting visualization options | Low |

---

## 4. TODO Items in Existing Code

### 4.1 CombatViewModel TODOs

| TODO | Line Reference | Priority |
| --- | --- | --- |
| "TODO: Integrate with AdvancedMovementService" | Movement targeting | High |
| "TODO: Implement item selection in v0.43.10" | UseItemCommand | High |
| "TODO: Use proper ability selection dialog" | UseAbilityCommand | High |
| "TODO: Companion-specific sprites" | Sprite loading | Medium |

### 4.2 CharacterSheetViewModel TODOs

| TODO | Line Reference | Priority |
| --- | --- | --- |
| "TODO: Armor bonus hardcoded" | Equipment bonuses | Medium |

### 4.3 DungeonExplorationViewModel Notes

| Issue | Description | Priority |
| --- | --- | --- |
| Movement placeholder | Uses adjacent-cell logic only | Medium |
| Companion sprites | Falls back to warrior sprite | Low |

---

## 5. Feature Parity Matrix

### Legend

- ✓ Full implementation
- ◐ Partial implementation
- ✗ No implementation

### Character Systems

| Feature | Core | GUI | Gap |
| --- | --- | --- | --- |
| Attributes Display | ✓ | ✓ | None |
| Resource Pools (HP/Stamina/AP) | ✓ | ✓ | None |
| Specialization-specific Resources | ✓ | ✓ | None |
| Stance System | ✓ | ◐ | Combat switching |
| Trauma Display | ✓ | ✓ | None |
| Trauma Management | ✓ | ✗ | Full system |
| Equipment Display | ✓ | ✓ | None |
| Ability Tree | ✓ | ✓ | None |

### Combat Systems

| Feature | Core | GUI | Gap |
| --- | --- | --- | --- |
| Turn-based Combat | ✓ | ✓ | None |
| Grid Movement | ✓ | ◐ | Pathfinding |
| Attack Actions | ✓ | ✓ | None |
| Ability Usage | ✓ | ◐ | Selection UI |
| Item Usage | ✓ | ✗ | Full system |
| Defend Action | ✓ | ✓ | None |
| Flee Action | ✓ | ✓ | None |
| Status Effects | ✓ | ✓ | None |
| Environmental Hazards | ✓ | ✓ | None |
| Boss Mechanics | ✓ | ✓ | None |
| Counter-Attack/Parry | ✓ | ✗ | Full system |
| Stance Switching | ✓ | ✗ | Combat UI |
| Companion Commands | ✓ | ✗ | Full system |

### Exploration Systems

| Feature | Core | GUI | Gap |
| --- | --- | --- | --- |
| Room Navigation | ✓ | ✓ | None |
| Room Description | ✓ | ✓ | None |
| Search/Investigate | ✓ | ✓ | None |
| Loot Collection | ✓ | ✓ | None |
| Rest System | ✓ | ✓ | None |
| Minimap | ✓ | ✓ | None |
| Environmental Features | ✓ | ◐ | Interactions |
| Puzzle Solving | ✓ | ✗ | Full system |
| Resource Gathering | ✓ | ✗ | Full system |
| NPC Interaction | ✓ | ✗ | Full system |
| Dialogue System | ✓ | ✗ | Full system |

### Economy Systems

| Feature | Core | GUI | Gap |
| --- | --- | --- | --- |
| Inventory Management | ✓ | ✓ | None |
| Equipment System | ✓ | ✓ | None |
| Consumables | ✓ | ✓ | None |
| Currency Display | ✓ | ✓ | None |
| Crafting | ✓ | ✗ | Full system |
| Merchant Trading | ✓ | ✗ | Full system |

### Social Systems

| Feature | Core | GUI | Gap |
| --- | --- | --- | --- |
| Quest Journal | ✓ | ✗ | Full system |
| Faction Reputation | ✓ | ✗ | Full system |
| Territory Control | ✓ | ✗ | Full system |
| Companion Management | ✓ | ✗ | Full system |

### Meta-Progression

| Feature | Core | GUI | Gap |
| --- | --- | --- | --- |
| Achievements | ✓ | ✓ | None |
| Account Unlocks | ✓ | ✓ | None |
| Cosmetics | ✓ | ✓ | None |
| NG+ Tiers | ✓ | ✓ | None |
| Endless Mode | ✓ | ◐ | Leaderboards |
| Boss Gauntlet | ✓ | ◐ | Leaderboards |
| Challenge Sectors | ✓ | ✓ | None |

---

## 6. Priority Recommendations

### Tier 1: Critical (Blocks Core Gameplay)

| Gap | Effort | Impact | Recommendation |
| --- | --- | --- | --- |
| Combat Item Usage | Medium | High | Implement item selection dialog |
| Combat Ability Selection | Medium | High | Implement ability selection panel |
| Combat Movement | High | High | Integrate AdvancedMovementService |
| Quest Journal | High | High | Core RPG feature, implement full system |

### Tier 2: High Priority (Significant Feature Gaps)

| Gap | Effort | Impact | Recommendation |
| --- | --- | --- | --- |
| Merchant/Trading | Medium | High | Enable economy gameplay |
| Dialogue System | High | High | NPC interaction is core to RPG |
| Companion Management | High | Medium | Party management expected |
| Crafting System | Medium | Medium | Item creation loop |

### Tier 3: Medium Priority (Enhanced Experience)

| Gap | Effort | Impact | Recommendation |
| --- | --- | --- | --- |
| Faction Reputation | Medium | Medium | Political gameplay layer |
| Stance Combat UI | Low | Medium | Quick combat enhancement |
| Counter-Attack UI | Low | Medium | Tactical depth |
| Puzzle Solving | Medium | Medium | Exploration variety |
| Trauma Management | Low | Low | Character depth |

### Tier 4: Low Priority (Polish)

| Gap | Effort | Impact | Recommendation |
| --- | --- | --- | --- |
| Territory Control | High | Low | Strategic layer |
| Leaderboards | Low | Low | Competitive feature |
| Resource Gathering | Low | Low | Crafting prerequisite |
| Galdr Categorization | Low | Low | Mystic flavor |

---

## Appendix: Core File Reference

### Files with No GUI Exposure

```
RuneAndRust.Core/
├── Crafting/
│   └── CraftingModels.cs
├── CraftingRecipe.cs
├── CraftingComponent.cs
├── Merchant.cs
├── Quests/
│   ├── Quest.cs
│   ├── QuestObjective.cs
│   ├── QuestReward.cs
│   └── QuestGenerationRequirements.cs
├── Dialogue/
│   ├── DialogueNode.cs
│   ├── DialogueOption.cs
│   ├── DialogueOutcome.cs
│   └── SkillCheckRequirement.cs
├── Companion.cs
├── CompanionAbility.cs
├── CompanionAction.cs
├── Factions/
│   ├── Faction.cs
│   ├── FactionReputation.cs
│   └── WitnessedAction.cs
├── Territory/
│   ├── World.cs
│   ├── Sector.cs
│   ├── SectorControlState.cs
│   ├── FactionInfluence.cs
│   ├── FactionWar.cs
│   └── ... (6 more files)
├── CounterAttack.cs
├── EndlessMode/
│   └── EndlessLeaderboardEntry.cs
└── BossGauntlet/
    └── GauntletLeaderboardEntry.cs

```

---

## Document History

| Version | Date | Changes |
| --- | --- | --- |
| 1.0 | Nov 2024 | Initial gap analysis |

---

*This document is part of the Rune & Rust technical documentation suite.*