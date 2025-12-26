# v0.4 Balance Review & Analysis

**Date:** 2025-11-11
**Version:** v0.4 Content Expansion
**Status:** Pre-Playtest Analysis

---

## Overview

This document provides a mathematical analysis of v0.4 balance, comparing the East Wing (combat-focused) and West Wing (exploration-focused) paths to ensure both are viable, rewarding, and roughly equivalent in difficulty and playtime.

---

## Path Comparison

### East Wing - Combat Path

**Route:** Entrance (1) → Corridor (2) → Salvage Bay (3) → Operations Center (4) → Arsenal (5) → Training Chamber (6) → Ammunition Forge (7) → Vault Antechamber (11) → Vault Corridor (12) → Boss (14 or 15)

**Combat Encounters:**
1. **Corridor (Room 2):** 2x Corrupted Servitor (20 HP each)
2. **Salvage Bay (Room 3):** 1x Corrupted Servitor + 1x Scrap-Hound (20 + 10 HP)
3. **Arsenal (Room 5):** 3x Blight-Drone (75 HP total)
4. **Training Chamber (Room 6):** 1x War-Frame (50 HP, mini-boss)
5. **Ammunition Forge (Room 7):** 2x Blight-Drone + environmental hazard (50 HP + 1d6/turn)
6. **Vault Antechamber (Room 11):** 3x Blight-Drone + 1x Scrap-Hound (85 HP)
7. **Boss Room (14 or 15):** Ruin-Warden (80 HP) OR Aetheric Aberration (60 HP)

**Total Enemy HP (excluding boss):** 310 HP
**Total Combats (excluding boss):** 6 encounters
**Environmental Hazards:** 1 (Ammunition Forge, 1d6 damage/turn)

**Puzzles:**
- Ammunition Forge: WITS DC 3 (disables hazard, rewards Optimized gear)

**Loot:**
- Operations Center: 2x Clan-Forged (weapon + armor)
- Ammunition Forge puzzle: 1x Optimized equipment
- Combat drops: Random from enemies (Jury-Rigged to Optimized)
- Secret Room (optional): 3x Myth-Forged

**Legend Gain (excluding boss):**
- 2x Corrupted Servitor: 20 Legend
- 1x Corrupted Servitor: 10 Legend
- 1x Scrap-Hound: 10 Legend
- 3x Blight-Drone (Arsenal): 75 Legend
- 1x War-Frame: 50 Legend
- 2x Blight-Drone (Forge): 50 Legend
- 3x Blight-Drone + 1x Scrap-Hound (Vault): 85 Legend
- **Total: 300 Legend** (+ 100 from boss = 400 total)

---

### West Wing - Exploration Path

**Route:** Entrance (1) → Corridor (2) → Salvage Bay (3) → Operations Center (4) → Research Archives (8) → Specimen Containment (9) → Observation Deck (10) → Vault Antechamber (11) → Vault Corridor (12) → Boss (14 or 15)

**Combat Encounters:**
1. **Corridor (Room 2):** 2x Corrupted Servitor (40 HP)
2. **Salvage Bay (Room 3):** 1x Corrupted Servitor + 1x Scrap-Hound (30 HP)
3. **Specimen Containment (Room 9):** 2x Test Subject (30 HP)
4. **Observation Deck (Room 10):** 1x Forlorn Scholar (30 HP) - **Can be talked to!**
5. **Vault Antechamber (Room 11):** 3x Blight-Drone + 1x Scrap-Hound (85 HP)
6. **Boss Room (14 or 15):** Ruin-Warden (80 HP) OR Aetheric Aberration (60 HP)

**Total Enemy HP (excluding boss, if all combat):** 215 HP
**Total Combats (excluding boss):** 5 encounters (4 if talk succeeds)
**Environmental Hazards:** 0

**Puzzles:**
- Research Archives: WITS DC 4 (rewards Clan-Forged gear)
- Vault Corridor: WITS DC 5 (unlocks Secret Room)

**Loot:**
- Operations Center: 2x Clan-Forged (weapon + armor)
- Research Archives puzzle: 1x Clan-Forged equipment
- Forlorn Scholar (talk success): 1x Optimized equipment + boss hints
- Combat drops: Random from enemies
- Secret Room (optional): 3x Myth-Forged

**Legend Gain (excluding boss):**
- 2x Corrupted Servitor: 20 Legend
- 1x Corrupted Servitor + 1x Scrap-Hound: 20 Legend
- 2x Test Subject: 40 Legend
- 1x Forlorn Scholar: 35 Legend (talk or fight)
- 3x Blight-Drone + 1x Scrap-Hound (Vault): 85 Legend
- **Total: 200 Legend** (+ 100 from boss = 300 total)

---

## Balance Analysis

### Combat Load

| Metric | East Wing | West Wing | Difference |
|--------|-----------|-----------|------------|
| Total Enemy HP | 310 HP | 215 HP | **+95 HP (44% more)** |
| Combat Encounters | 6 | 5 (4 if talk) | **+1-2 encounters** |
| Environmental Hazard | Yes (1d6/turn) | No | **+3-6 damage/turn** |
| Puzzles | 1 (DC 3) | 2 (DC 4, DC 5) | **-1 puzzle, lower DC** |
| Legend Gain | 300 | 200 | **+100 Legend (50% more)** |

**Analysis:**
- East Wing has **44% more combat HP** and **50% more Legend gain**
- West Wing compensates with **fewer combats** and **more puzzles**
- West Wing has **peaceful resolution option** (Forlorn Scholar)
- Both paths converge at Vault Antechamber (same difficulty spike)

**Verdict:** East Wing is **more combat-heavy** but **more rewarding** in terms of Legend gain. West Wing is **more strategic** (puzzles, negotiation) but grants **less Legend**. This imbalance may need adjustment.

---

### Loot Distribution

| Loot Source | East Wing | West Wing |
|-------------|-----------|-----------|
| Operations Center | 2x Clan-Forged | 2x Clan-Forged |
| Puzzle Rewards | 1x Optimized (Forge) | 1x Clan-Forged (Archives) |
| NPC Interaction | — | 1x Optimized (Scholar talk) |
| Combat Drops | ~6 encounters | ~4-5 encounters |
| Secret Room | 3x Myth-Forged | 3x Myth-Forged |

**Analysis:**
- East Wing gets **more combat loot** due to more encounters
- West Wing gets **similar guaranteed loot** (Clan-Forged + Optimized)
- Secret Room equalizes end-game loot (both paths can access)
- **Puzzle reward disparity:** East gets Optimized, West gets Clan-Forged
  - This seems backwards - West should get better puzzle rewards

**Verdict:** Loot distribution is **mostly fair**, but puzzle rewards could be adjusted. West Wing's higher puzzle DCs should reward Optimized gear.

---

### Difficulty Curve

#### East Wing Difficulty Progression

1. **Corridor (Easy):** 2x Corrupted Servitor - Tutorial combat
2. **Salvage Bay (Easy):** 1x Servitor + 1x Scrap-Hound - Introduces fast enemy
3. **Arsenal (Moderate):** 3x Blight-Drone - First real challenge
4. **Training Chamber (Hard):** 1x War-Frame - Mini-boss, difficulty spike
5. **Ammunition Forge (Hard):** 2x Blight-Drone + hazard - Tactical challenge
6. **Vault Antechamber (Very Hard):** 3x Blight-Drone + 1x Scrap-Hound - Pre-boss gauntlet
7. **Boss (Extreme):** Ruin-Warden or Aetheric Aberration

**Difficulty Progression:** Smooth curve with mini-boss spike at War-Frame.

#### West Wing Difficulty Progression

1. **Corridor (Easy):** 2x Corrupted Servitor - Tutorial combat
2. **Salvage Bay (Easy):** 1x Servitor + 1x Scrap-Hound - Introduces fast enemy
3. **Research Archives (Easy):** Puzzle only (DC 4) - Break from combat
4. **Specimen Containment (Moderate):** 2x Test Subject - Glass cannons, tactical
5. **Observation Deck (Moderate/Hard):** 1x Forlorn Scholar - Magic enemy or negotiation
6. **Vault Antechamber (Very Hard):** 3x Blight-Drone + 1x Scrap-Hound - Pre-boss gauntlet
7. **Boss (Extreme):** Ruin-Warden or Aetheric Aberration

**Difficulty Progression:** Gentler curve, easier mid-game, same end-game spike.

**Verdict:** East Wing has **steeper difficulty curve** due to War-Frame mini-boss. West Wing is **more forgiving** in mid-game. Both paths converge at same difficulty for final rooms.

---

## Enemy Balance Review

### Tier 0 - Tutorial Enemies

**Corrupted Servitor:**
- HP: 20
- Damage: 1d6
- AI: 80% attack, 20% defend
- **Verdict:** Balanced, good tutorial enemy

**Scrap-Hound:**
- HP: 10 (very low)
- Damage: 1d4 per hit (QuickBite = 2 attacks)
- AI: 70% QuickBite, 30% DartAway (+75% evasion)
- **Analysis:** Low HP but hard to hit. Tests player accuracy.
- **Concern:** May feel "annoying" rather than threatening
- **Verdict:** Balanced for harassment role

---

### Tier 1 - Standard Enemies

**Blight-Drone:**
- HP: 25
- Damage: 1d8
- AI: 60% RapidStrike, 30% BasicAttack, 10% ChargeDefense
- **Verdict:** Balanced, core enemy type

**Test Subject:**
- HP: 15 (low)
- Damage: 1d8 + 2 (high)
- AI: 60% FergeralStrike (+2 dice), 30% BerserkerRush (high damage, skip turn), 10% Shriek
- **Analysis:** Glass cannon design. High risk, high reward for player.
- **Concern:** BerserkerRush stun mechanic may feel punishing
- **Verdict:** Balanced but may need playtesting

---

### Tier 2 - Elite Enemies

**War-Frame (Mini-Boss):**
- HP: 50
- Damage: 2d6
- AI: 40% PrecisionStrike (+3 dice, +2 accuracy), 30% SuppressionFire (AOE), 20% TacticalReposition (+75% defense), 10% EmergencyRepairs (heal 10)
- **Analysis:** Well-rounded mini-boss with healing and defense
- **Concern:** May be too defensive with TacticalReposition + EmergencyRepairs combo
- **Verdict:** Needs playtesting, may need slight HP reduction (50 → 45)

**Forlorn Scholar:**
- HP: 30
- Damage: 2d6 (ignores armor)
- AI: 50% AethericBolt (WILL attack), 30% RealityDistortion (stun), 20% PhaseShift (+90% evasion)
- Special: Can be talked to (WILL DC 4)
- **Analysis:** First magic-focused enemy. Rewards WILL builds.
- **Concern:** RealityDistortion stun may be frustrating (30% chance)
- **Verdict:** Balanced, interesting tactical encounter

---

### Bosses

**Ruin-Warden:**
- HP: 80
- Damage: 2d8
- AI: Phase-based (aggressive → defensive)
- **Analysis:** Existing boss, proven balanced in v0.3
- **Verdict:** Balanced

**Aetheric Aberration:**
- HP: 60 (lower than Ruin-Warden)
- Damage: 3d6 (ignores armor)
- AI: Phase 1 (>50% HP): 40% VoidBlast, 30% SummonEchoes, 20% RealityTear, 10% PhaseShift
- AI: Phase 2 (<50% HP): 40% AethericStorm, 40% VoidBlast, 20% DesperateSummon
- **Analysis:** Lower HP but higher damage output. Magic-focused alternative to Ruin-Warden.
- **Concern:** Summon mechanics are placeholders (not functional)
- **Verdict:** Needs playtesting, but mathematical balance looks good

**Boss Comparison:**

| Metric | Ruin-Warden | Aetheric Aberration |
|--------|-------------|---------------------|
| HP | 80 | 60 (-25%) |
| Damage Type | Physical (MIGHT) | Magic (WILL) |
| Avg Damage/Turn | ~12-15 | ~15-18 (+20%) |
| Special Mechanics | Phase-based aggression | Summons (placeholder) |
| Difficulty | **Physical challenge** | **Mental/magical challenge** |

**Verdict:** Both bosses offer **different challenges**. Ruin-Warden tests **STURDINESS/defense**, Aberration tests **WILL/resistance**. This is good design for replayability.

---

## Legend Gain Balance

### Legend Gain by Path

**East Wing (Combat Path):**
- Pre-boss: 300 Legend
- Boss: 100 Legend
- **Total: 400 Legend**

**West Wing (Exploration Path):**
- Pre-boss: 200 Legend
- Boss: 100 Legend
- **Total: 300 Legend**

**Analysis:**
- East Wing grants **33% more Legend** than West Wing
- This compensates for **higher difficulty** and **more combat**
- West Wing offers **puzzles, negotiation, and strategic gameplay**

**Concern:** 100 Legend difference may be too large. Players may feel "forced" to take East Wing for optimal progression.

**Recommendation:**
- Option 1: Increase West Wing Legend gain (puzzles grant +25 Legend each)
- Option 2: Add bonus Legend for peaceful resolution (+25 for Scholar talk)
- Option 3: Accept disparity as intentional (combat = more XP)

**Verdict:** **Needs adjustment**. Suggest adding +50 total Legend to West Wing via puzzle/talk bonuses.

---

## Estimated Playtime

### East Wing (Combat Path)

**Combat Time Estimates:**
- Corridor: ~2 minutes
- Salvage Bay: ~2 minutes
- Arsenal: ~4 minutes (3 enemies)
- Training Chamber: ~5 minutes (mini-boss)
- Ammunition Forge: ~4 minutes (hazard adds complexity)
- Vault Antechamber: ~5 minutes (4 enemies)
- Boss: ~8-10 minutes

**Puzzle Time:**
- Ammunition Forge: ~2 minutes

**Total Combat Time:** ~30-35 minutes
**Total Puzzle Time:** ~2 minutes
**Navigation/Loot:** ~8-10 minutes
**Estimated Total:** **40-47 minutes**

---

### West Wing (Exploration Path)

**Combat Time Estimates:**
- Corridor: ~2 minutes
- Salvage Bay: ~2 minutes
- Specimen Containment: ~3 minutes
- Observation Deck: ~4 minutes (or 2 min for talk)
- Vault Antechamber: ~5 minutes
- Boss: ~8-10 minutes

**Puzzle Time:**
- Research Archives: ~3 minutes (DC 4)
- Vault Corridor: ~2 minutes (DC 5, secret room)

**Total Combat Time:** ~24-26 minutes (or ~22 if talk succeeds)
**Total Puzzle Time:** ~5 minutes
**Navigation/Loot:** ~8-10 minutes
**Estimated Total:** **37-41 minutes**

**Verdict:** Both paths are **roughly equivalent in playtime** (38-44 minutes average). Target of 45-60 minutes may be slightly high, but within acceptable range.

---

## Recommendations

### High Priority

1. **Legend Gain Balance:**
   - Add +25 Legend bonus for solving puzzles (Research Archives, Ammunition Forge)
   - Add +25 Legend bonus for peaceful resolution (Forlorn Scholar talk)
   - This brings West Wing total to 350 Legend (vs East Wing's 400)

2. **Puzzle Reward Adjustment:**
   - Swap puzzle rewards: West Wing (higher DC) should get Optimized gear
   - Research Archives (DC 4) → Optimized equipment
   - Ammunition Forge (DC 3) → Clan-Forged equipment

3. **War-Frame HP Reduction:**
   - Reduce from 50 HP to 45 HP
   - Still challenging but less of a "brick wall"

### Medium Priority

4. **Vault Corridor Boss Selection UI:**
   - Add special prompt describing both bosses
   - Hint at their strengths/weaknesses
   - Current: Player discovers bosses organically (acceptable but not ideal)

5. **Room Description Polish:**
   - Add more atmospheric flavor text
   - Emphasize branching choice at Operations Center
   - Add environmental storytelling details

6. **Combat Log Refinement:**
   - Shorten some verbose messages
   - Add color coding for different damage types
   - Improve readability during long fights

### Low Priority

7. **Summon Mechanics:**
   - Implement actual summon system (currently placeholders)
   - Aberration would spawn 2x Scrap-Hound (Phase 1) and 1x Blight-Drone (Phase 2)
   - Significant technical lift for moderate gameplay improvement

8. **Boss Lock System:**
   - Lock opposite boss room when one is entered
   - Prevents "boss shopping" (checking both, fleeing from harder one)
   - Currently both bosses accessible (acceptable)

9. **Path Statistics:**
   - Track which path player chose
   - Track which boss player defeated
   - Display in victory screen

---

## Playtesting Checklist

### Pre-Playtest Setup

- [ ] Verify .NET 8.0 SDK installed
- [ ] Build project: `dotnet build`
- [ ] Run project: `dotnet run --project RuneAndRust.ConsoleApp`
- [ ] Create new character for each playtest
- [ ] Test all three classes: Warrior, Scavenger, Mystic

### East Wing Playtest

**Objectives:**
- [ ] Complete full run: Entrance → East Wing → Boss
- [ ] Test all combat encounters
- [ ] Test Ammunition Forge puzzle and hazard
- [ ] Test War-Frame mini-boss difficulty
- [ ] Collect and evaluate loot drops
- [ ] Measure total playtime
- [ ] Note any frustrating or confusing moments

**Specific Tests:**
- [ ] Environmental hazard properly activates in Ammunition Forge
- [ ] Hazard disables when puzzle solved
- [ ] Hazard deals 1d6 damage per turn during combat
- [ ] War-Frame uses all 4 abilities appropriately
- [ ] Operations Center loot spawns correctly (2x Clan-Forged)
- [ ] Secret Room accessible after Vault Corridor puzzle

**Data to Record:**
- Total playtime: _____ minutes
- Deaths: _____
- Final HP: _____ / _____
- Final Legend: _____
- Final Level: _____
- Loot obtained: _____________
- Most difficult encounter: _____________
- Most frustrating moment: _____________

### West Wing Playtest

**Objectives:**
- [ ] Complete full run: Entrance → West Wing → Boss
- [ ] Test Research Archives puzzle (DC 4)
- [ ] Test Forlorn Scholar talk mechanic (WILL check)
- [ ] Test Test Subject combat (glass cannon design)
- [ ] Collect and evaluate loot drops
- [ ] Measure total playtime
- [ ] Compare difficulty to East Wing

**Specific Tests:**
- [ ] Research Archives puzzle grants Clan-Forged loot
- [ ] Forlorn Scholar encounter displays special message
- [ ] Talk command works correctly
- [ ] WILL check (DC 4) feels fair
- [ ] Peaceful resolution grants loot and Legend
- [ ] Failed talk triggers combat normally
- [ ] Attack command skips negotiation

**Data to Record:**
- Total playtime: _____ minutes
- Deaths: _____
- Final HP: _____ / _____
- Final Legend: _____
- Final Level: _____
- Loot obtained: _____________
- Forlorn Scholar: [ ] Talked [ ] Fought [ ] Success [ ] Failure
- Most difficult encounter: _____________
- Felt easier than East Wing? [ ] Yes [ ] No

### Boss Balance Testing

**Objectives:**
- [ ] Test Ruin-Warden (Room 14)
- [ ] Test Aetheric Aberration (Room 15)
- [ ] Compare difficulty between bosses
- [ ] Evaluate phase transitions
- [ ] Test boss mechanics thoroughly

**Ruin-Warden Tests:**
- [ ] Phase transition at 50% HP works
- [ ] All abilities feel fair
- [ ] Boss difficulty appropriate for end-game
- [ ] Estimated time to defeat: _____ minutes

**Aetheric Aberration Tests:**
- [ ] Phase transition at 50% HP works
- [ ] VoidBlast damage feels appropriate
- [ ] PhaseShift (+90% evasion) works correctly
- [ ] Summon placeholders display correctly
- [ ] Boss difficulty compared to Ruin-Warden: [ ] Easier [ ] Same [ ] Harder
- [ ] Estimated time to defeat: _____ minutes

### Legend Gain Testing

**Objectives:**
- [ ] Track Legend gain throughout both paths
- [ ] Verify both paths grant similar total Legend
- [ ] Ensure Legend gain feels rewarding

**East Wing Legend:**
- Pre-boss Legend: _____
- Boss Legend: _____
- Total: _____

**West Wing Legend:**
- Pre-boss Legend: _____
- Boss Legend: _____
- Total: _____

**Difference:** _____ Legend
**Acceptable?** [ ] Yes [ ] No

### Secret Room Testing

**Objectives:**
- [ ] Discover Vault Corridor puzzle (DC 5)
- [ ] Unlock secret room successfully
- [ ] Verify loot placement (3x Myth-Forged)
- [ ] Confirm loot only spawns once

**Tests:**
- [ ] WITS check DC 5 feels appropriately difficult
- [ ] Secret room door unlocks (south exit from Vault Corridor)
- [ ] Message displays when entering secret room
- [ ] 3x Myth-Forged items present
- [ ] Player can choose which items to take

---

## Known Issues & Edge Cases

### Environmental Hazards
- **Issue:** Hazard damage applies even if player defeats all enemies quickly
- **Status:** Working as intended (encourages puzzle solving)
- **Test:** Verify hazard stops when puzzle solved

### Forlorn Scholar
- **Issue:** If player attacks during special encounter, talk becomes unavailable
- **Status:** Working as intended (one choice only)
- **Test:** Verify HasTalkedToNPC flag prevents re-negotiation

### Summon Mechanics
- **Issue:** SummonEchoes and DesperateSummon are placeholders
- **Status:** Known limitation, acceptable for v0.4
- **Test:** Verify placeholder messages display correctly

### Boss Selection
- **Issue:** Both boss rooms accessible, no lock system
- **Status:** Acceptable for v0.4, player can choose freely
- **Test:** Verify player can access both rooms and choose

### Old Saves Compatibility
- **Issue:** v0.3 saves may not work correctly with v0.4
- **Status:** Expected, documented in IMPLEMENTATION_V04.md
- **Test:** Verify new character creation works correctly

---

## Post-Playtest Action Items

After completing playtests, review this document and update with actual data:

1. **Update playtime estimates** with real measurements
2. **Adjust Legend gain** if disparity is too large
3. **Rebalance enemies** based on feedback (especially War-Frame, Test Subject)
4. **Refine puzzle DCs** if too hard/easy
5. **Polish room descriptions** based on player confusion
6. **Fix any bugs** discovered during testing
7. **Update documentation** with final balance numbers

---

## Version History

**v1.0 (2025-11-11):**
- Initial balance review
- Pre-playtest mathematical analysis
- Playtesting checklist created
- Recommendations drafted
