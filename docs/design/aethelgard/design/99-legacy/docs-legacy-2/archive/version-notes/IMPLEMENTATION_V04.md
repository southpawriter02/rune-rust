# v0.4 Content Expansion - Implementation Log

**Started:** 2025-11-11
**Target:** Expand from 5 rooms to 15 rooms with branching paths
**Goal:** Add variety and replayability through multiple paths and boss choices

## Overview

v0.4 expands the game from a linear 5-room dungeon to a 15-room experience with:
- **Branching paths**: East (combat-focused) vs West (exploration-focused)
- **Boss choice**: Two different final bosses
- **5 new enemy types**: Scrap-Hound, Test Subject, War-Frame, Forlorn Scholar, Aetheric Aberration
- **Environmental hazards**: Unstable reactors, puzzles with rewards
- **Secret room**: Hidden supply cache with legendary loot

---

## Phase 1: Room Framework ✅ COMPLETE

### What Was Added

**New Room Count:** 15 total rooms (up from 5)

**Room Structure:**
- **Entrance Zone (Rooms 1-3):** Linear tutorial path
  - Room 1: Entrance (safe zone)
  - Room 2: Corridor (2x Corrupted Servitor)
  - Room 3: Salvage Bay (NEW - 1x Servitor + 1x Scrap-Hound)

- **Central Hub (Room 4):** Operations Center (NEW - safe zone, branching choice)
  - Exits: East to Arsenal, West to Research Archives

- **East Wing - Combat Path (Rooms 5-7):**
  - Room 5: Arsenal (3x Blight-Drone)
  - Room 6: Training Chamber (1x War-Frame mini-boss)
  - Room 7: Ammunition Forge (2x Blight-Drone + hazard)

- **West Wing - Exploration Path (Rooms 8-10):**
  - Room 8: Research Archives (puzzle, no combat)
  - Room 9: Specimen Containment (2x Test Subject)
  - Room 10: Observation Deck (1x Forlorn Scholar, talk/fight)

- **Deep Vault - Convergence (Rooms 11-12):**
  - Room 11: Vault Antechamber (paths merge, 3x Blight-Drone + 1x Scrap-Hound)
  - Room 12: Vault Corridor (boss choice, no combat)

- **Secret Room (Room 13):**
  - Room 13: Supply Cache (hidden, myth-forged loot)

- **Boss Sanctums (Rooms 14-15):**
  - Room 14: Arsenal Vault (Ruin-Warden)
  - Room 15: Energy Core (NEW - Aetheric Aberration)

### Files Modified

**GameWorld.cs** (RuneAndRust.Engine)
- ✅ Completely rewrote `InitializeRooms()` method
- ✅ Added all 15 rooms with descriptions and connections
- ✅ Implemented branching at Operations Center (Room 4)
- ✅ Implemented boss choice at Vault Corridor (Room 12)
- ✅ Added `UnlockSecretRoom()` method for secret room discovery
- ✅ Updated `AddPuzzleReward()` to handle multiple puzzle rooms

**Program.cs** (RuneAndRust.ConsoleApp)
- ✅ Updated victory condition to check for either boss room
- ✅ Fixed references to old "Puzzle Chamber" and "Boss Sanctum" names
- ✅ Added backwards compatibility notes for v0.3 saves

**GameState.cs** (RuneAndRust.Engine)
- ✅ Updated `SolvePuzzle()` method to handle new puzzle system
- ✅ Only Vault Corridor puzzle unlocks secret room

### Branching Logic

**Central Hub (Room 4):**
- Player can choose East (combat) or West (exploration)
- Both paths eventually converge at Vault Antechamber (Room 11)

**Boss Choice (Room 12):**
- Player can choose West to Arsenal Vault (Ruin-Warden)
- Or East to Energy Core (Aetheric Aberration)

**Secret Room (Room 13):**
- Discovered via WITS check in Vault Corridor (Room 12)
- Unlocks south exit from Vault Corridor to Supply Cache

---

## Phase 2: New Enemies ✅ COMPLETE

### Enemy Types Added

#### 1. Scrap-Hound (Tier 0 - Fast Harasser)
- **HP:** 10
- **Attributes:** MIGHT 2, FINESSE 4, STURDINESS 1
- **Damage:** 1d4 (weak but fast)
- **AI Pattern:**
  - 70% Quick Bite (attacks twice, -1 die each)
  - 30% Dart Away (gains +2 evasion)
- **Threat:** Low HP but hard to hit, tests player accuracy

#### 2. Test Subject (Tier 1 - Glass Cannon)
- **HP:** 15
- **Attributes:** MIGHT 3, FINESSE 5, STURDINESS 2
- **Damage:** 1d8 (high for tier)
- **AI Pattern:**
  - 60% Feral Strike (MIGHT + 2 dice)
  - 30% Berserker Rush (immediate attack, skips next turn)
  - 10% Shriek (buff allies)
- **Threat:** High damage but dies quickly

#### 3. War-Frame (Tier 2 - Elite/Mini-Boss)
- **HP:** 50
- **Attributes:** MIGHT 4, FINESSE 3, STURDINESS 4
- **Damage:** 2d6
- **AI Pattern:**
  - 40% Precision Strike (MIGHT + 3 dice, +2 accuracy)
  - 30% Suppression Fire (AOE attack)
  - 20% Tactical Reposition (+75% defense)
  - 10% Emergency Repair (heal 10 HP)
- **Threat:** Mini-boss difficulty, requires tactics

#### 4. Forlorn Scholar (Tier 2 - Caster)
- **HP:** 30
- **Attributes:** MIGHT 2, FINESSE 3, WILL 5, STURDINESS 2
- **Damage:** 2d6 (Aetheric, ignores armor)
- **AI Pattern:**
  - 50% Aetheric Bolt (WILL + 2 dice, ignores armor)
  - 30% Reality Distortion (WILL save DC 4 or lose turn)
  - 20% Phase Shift (+90% evasion)
- **Special:** Can be talked to (WILL check DC 4) for peaceful resolution
- **Threat:** First enemy that can disable player

#### 5. Aetheric Aberration (Boss - Magic Focus)
- **HP:** 60 (lower than Ruin-Warden)
- **Attributes:** MIGHT 2, FINESSE 4, WILL 6, STURDINESS 3
- **Damage:** 3d6 (Aetheric, ignores armor)
- **AI Pattern (Phase 1, 100%-50% HP):**
  - 40% Void Blast (WILL + 3 dice, 3d6 damage)
  - 30% Summon Echoes (spawn 2x Scrap-Hound)
  - 20% Reality Tear (AOE 2d6 damage)
  - 10% Phase Shift (+90% evasion for 2 turns)
- **AI Pattern (Phase 2, 50%-0% HP):**
  - 40% Aetheric Storm (AOE 2d8 damage to all)
  - 40% Void Blast (WILL + 4 dice, 4d6 damage)
  - 20% Desperate Summon (spawn 1x Blight-Drone)
- **Threat:** High-risk, high-reward boss for magic-focused builds

### Files Modified

**Enemy.cs** (RuneAndRust.Core)
- ✅ Added 5 new enemy types to `EnemyType` enum

**EnemyFactory.cs** (RuneAndRust.Engine)
- ✅ Added factory methods for all 5 new enemies
- ✅ Balanced HP, attributes, and damage values

**EnemyAI.cs** (RuneAndRust.Engine)
- ✅ Added 12 new enemy actions to `EnemyAction` enum
- ✅ Implemented decision-making logic for all 5 enemies
- ✅ Added execution methods for all new actions
- ✅ Implemented phase-based AI for Aetheric Aberration

### New Enemy Actions

**Scrap-Hound:**
- QuickBite: Attacks twice with -1 die each
- DartAway: Gains +75% evasion for 1 turn

**Test Subject:**
- FergeralStrike: MIGHT + 2 dice attack
- BerserkerRush: High damage attack, then stunned
- Shriek: Buff allies (simplified mechanic)

**War-Frame:**
- PrecisionStrike: MIGHT + 3 dice, +2 accuracy bonus
- SuppressionFire: AOE attack (1d6 to all)
- TacticalReposition: +75% defense for 1 turn

**Forlorn Scholar:**
- AethericBolt: WILL + 2 dice, ignores armor
- RealityDistortion: WILL save or lose turn
- PhaseShift: +90% evasion for 1 turn

**Aetheric Aberration:**
- VoidBlast: High damage magic attack (3d6 or 4d6 in phase 2)
- SummonEchoes: Spawn 2x Scrap-Hound (placeholder)
- RealityTear: AOE 2d6 to all targets
- AethericStorm: Phase 2 AOE 2d8 to all
- DesperateSummon: Phase 2 spawn 1x Blight-Drone (placeholder)
- PhaseShift: +90% evasion

---

## Phase 3: Special Mechanics ✅ COMPLETE

### What's Been Implemented

**Environmental Hazards:** ✅ COMPLETE
- ✅ Unstable Reactors (Room 7)
  - Deal 1d6 damage per turn during combat
  - WITS check DC 3 to disable
  - Hazard automatically disables when puzzle solved
  - Warning displayed at combat start
- ✅ Added hazard properties to Room class
- ✅ Integrated with combat turn system
- ✅ Per-turn damage application at end of each round

**Puzzle System Updates:** ✅ COMPLETE
- ✅ Updated AddPuzzleReward to accept room name
- ✅ Room-specific success messages
- ✅ Puzzles disable environmental hazards
- ✅ Vault Corridor puzzle unlocks secret room
- ✅ Research Archives puzzle rewards Clan-Forged equipment
- ✅ Ammunition Forge puzzle rewards Optimized equipment

**Combat System Integration:** ✅ COMPLETE
- ✅ CombatState now holds CurrentRoom reference
- ✅ InitializeCombat accepts room parameter
- ✅ Environmental damage integrated with turn progression

**Forlorn Scholar Talk Mechanic:** ✅ COMPLETE
- ✅ Added "talk" command (aliases: speak, negotiate, convince)
- ✅ WILL check (DC 4) to negotiate with Forlorn Scholar
- ✅ Peaceful resolution gives Optimized loot, Legend, and boss hints
- ✅ Failure forces combat as normal
- ✅ Attack command allows skipping negotiation
- ✅ Room properties track talkable NPCs
- ✅ Special encounter message displayed in Observation Deck
- ✅ Auto-combat disabled for rooms with talkable NPCs

**Loot Placement:** ✅ COMPLETE
- ✅ Operations Center (Room 4): 2x Clan-Forged equipment cache
  - 1x class-appropriate weapon
  - 1x random armor
  - Placed during character creation
- ✅ Secret Room (Room 13): 3x Myth-Forged equipment with player choice
  - 2x class-appropriate weapons
  - 1x random armor
  - Placed when room first discovered
- ✅ Research Archives puzzle reward (Clan-Forged)
- ✅ Ammunition Forge puzzle reward (Optimized)

### Optional/Future Features

**Boss Selection System (Optional):**
- ❌ Add UI prompt in Vault Corridor
- ❌ Display boss descriptions and difficulty
- ❌ Lock opposite boss room when one is chosen
- Note: Currently both bosses are accessible, no UI prompt

**Summon Mechanics (Future):**
- ❌ Update combat system to handle mid-combat spawns
- ❌ Aetheric Aberration can summon Scrap-Hounds and Blight-Drones
- ❌ Summoned enemies join initiative order
- Note: Currently placeholders with log messages

---

## Phase 4: Puzzle Rewards ✅ COMPLETE

### Puzzle Rooms

**Room 7: Ammunition Forge (East Wing)**
- Puzzle: Disable Unstable Reactors (WITS DC 3)
- Reward: Optimized equipment (class-appropriate)
- ✅ Implemented via AddPuzzleReward()

**Room 8: Research Archives (West Wing)**
- Puzzle: Hack secure terminal (WITS DC 4)
- Reward: Clan-Forged equipment (class-appropriate)
- ✅ Implemented via AddPuzzleReward()

**Room 12: Vault Corridor (Secret Room Discovery)**
- Puzzle: Discover hidden door (WITS DC 5)
- Reward: Access to Room 13 (Supply Cache)
- ✅ Implemented (UnlockSecretRoom)

**Room 13: Supply Cache (Secret Room)**
- No puzzle, just reward
- Reward: 3x Myth-Forged equipment (player choice)
- ✅ Implemented (AddSecretRoomLoot)

**Room 4: Operations Center (Hub)**
- No puzzle
- Reward: 2x Clan-Forged equipment cache
- ✅ Implemented (AddOperationsCenterLoot)

---

## Phase 5: Balance & Polish (TODO)

### Testing Plan

**East Path Playtest:**
- ❌ Full run: Entrance → Hub → Arsenal → Training → Forge → Vault → Boss
- ❌ Verify difficulty curve
- ❌ Ensure loot feels appropriate
- ❌ Balance combat encounters

**West Path Playtest:**
- ❌ Full run: Entrance → Hub → Archives → Containment → Observation → Vault → Boss
- ❌ Verify puzzles are solvable
- ❌ Ensure non-combat path is rewarding
- ❌ Balance loot vs combat path

**Boss Balance:**
- ❌ Test Ruin-Warden in new position (Room 14)
- ❌ Test Aetheric Aberration difficulty
- ❌ Ensure both bosses feel fair and challenging

**Legend Gain:**
- ❌ Verify both paths grant similar XP
- ❌ Ensure playtime is balanced (30-40 min per path)

### Polish Tasks

- ❌ Refine all room descriptions
- ❌ Add atmospheric flavor text
- ❌ Polish combat log messages
- ❌ Add hints about branching paths
- ❌ Add boss selection UI

---

## Phase 6: Replayability (TODO)

### Features to Add

**Path Tracking:**
- ❌ Track which path player chose (East vs West)
- ❌ Track which boss player defeated
- ❌ Save choices to WorldState

**Statistics:**
- ❌ Enemies defeated counter
- ❌ Rooms explored counter
- ❌ Time taken tracker
- ❌ Path chosen tracker

**New Game+ (Optional):**
- ❌ Start new run with same character
- ❌ Keep Milestones and Progression Points
- ❌ Reset rooms and loot
- ❌ Add completion statistics screen

---

## Known Issues

### Compatibility
- ✅ Old v0.3 saves will not fully work with v0.4
- ✅ Puzzle states from old saves are ignored
- ✅ Players may need to start fresh in v0.4

### Incomplete Features
- ⚠️ Summon mechanics are placeholders (log messages only)
- ⚠️ Boss selection UI not implemented (both bosses accessible)

---

## Technical Debt

### Future Improvements
- Consider adding dynamic room connections
- Refactor puzzle system for better extensibility
- Add more environmental hazard types
- Implement proper summon/reinforcement system
- Add dialogue tree system for NPCs

---

## Success Criteria

**v0.4 is DONE when:**
- ✅ 15 total rooms exist and are navigable
- ✅ Player can choose East or West path at Central Hub
- ✅ Both paths lead to Deep Vault
- ✅ Player can choose between 2 bosses
- ✅ 8 enemy types total (3 existing + 5 new)
- ✅ All new enemies have functional AI
- ✅ Forlorn Scholar can be talked to or fought
- ✅ Environmental hazards work (Unstable Reactors)
- ✅ Secret room can be found
- ✅ Loot placement complete (Operations Center, Secret Room, puzzles)
- ⏳ War-Frame mini-boss is challenging but fair (needs testing)
- ⏳ Aetheric Aberration boss works (summons are placeholders)
- ⏳ Full playthrough takes 45-60 minutes (needs testing)
- ⏳ Both paths feel distinct and rewarding (needs testing)
- ✅ Game has replay value (2 paths × 2 bosses × talk/fight = multiple combinations)

---

## Change Log

### 2025-11-11

**Phase 1 Complete:**
- Created all 15 rooms with descriptions
- Implemented branching at Central Hub
- Implemented boss choice at Vault Corridor
- Added secret room framework
- Updated victory conditions for both boss rooms
- Fixed references to old room names

**Phase 2 Complete:**
- Added 5 new enemy types to EnemyType enum
- Created factory methods for all new enemies
- Implemented AI decision trees for all enemies
- Added 12 new enemy actions
- Implemented execution methods for all actions
- Balanced enemy stats (HP, attributes, damage)
- Implemented phase-based AI for Aetheric Aberration

**Phase 3 Complete (Environmental Hazards & Puzzles):**
- Implemented environmental hazard system
  - Added hazard properties to Room class
  - Created Unstable Reactors in Ammunition Forge
  - Hazards deal 1d6 damage per turn during combat
  - Puzzles can disable hazards
- Updated puzzle system
  - AddPuzzleReward now accepts room name for context
  - Room-specific success messages
  - Vault Corridor puzzle unlocks secret room
- Combat system integration
  - CombatState holds CurrentRoom reference
  - Environmental damage applied each turn
  - Hazard warnings at combat start

**Compatibility Fixes:**
- Updated Program.cs to handle new boss rooms
- Updated GameState.cs puzzle logic
- Added backwards compatibility notes
- Updated GameWorld helper methods
- Updated combat initialization to pass current room

**Phase 3 & 4 Complete (Loot & NPC Interaction):**
- Implemented loot placement system
  - Operations Center: 2x Clan-Forged equipment
  - Secret Room: 3x Myth-Forged equipment with choice
  - Puzzle rewards for Research Archives and Ammunition Forge
- Implemented Forlorn Scholar talk mechanic
  - Added Talk command type with aliases
  - WILL check (DC 4) for peaceful resolution
  - Success: Loot + Legend + boss hints
  - Failure: Combat as normal
  - Room properties for talkable NPCs
  - Special encounter UI in Observation Deck
- Updated combat auto-trigger logic for talkable NPCs
- Attack command can skip negotiation

---

## Next Steps

1. ✅ Commit Phase 1 & 2 changes
2. ✅ Implement environmental hazards (Phase 3)
3. ✅ Add puzzle reward logic (Phase 4)
4. ✅ Implement talk mechanic for Forlorn Scholar
5. ⏳ Add summon mechanics to combat system (optional, placeholders work)
6. ⏳ Playtest both paths
7. ⏳ Balance encounters and loot
8. ⏳ Polish room descriptions
9. ⏳ Add replayability tracking (optional)
10. ⏳ Final testing and balance pass

**Current Status:** Phases 1-4 COMPLETE. Core v0.4 features fully implemented.
**Remaining:** Phase 5 (Balance & Polish) - playtesting and balancing recommended
