# v0.4 Playtest Guide

**Version:** v0.4 Content Expansion
**Date:** 2025-11-11
**Purpose:** Systematic playtesting of new content and balance verification

---

## Quick Start

### Prerequisites

1. .NET 8.0 SDK installed
2. Terminal/Command Prompt access
3. 45-60 minutes per full playthrough
4. Notepad for recording observations

### Build & Run

```bash
cd /path/to/rune-rust
dotnet build
dotnet run --project RuneAndRust.ConsoleApp
```

---

## Playtest Structure

Complete **6 full playthroughs** to test all major combinations:

| # | Class | Path | Boss | Talk/Fight | Priority |
|---|-------|------|------|------------|----------|
| 1 | Warrior | East | Ruin-Warden | — | **HIGH** |
| 2 | Scavenger | West | Ruin-Warden | Talk | **HIGH** |
| 3 | Mystic | West | Aetheric Aberration | Talk | **HIGH** |
| 4 | Warrior | East | Aetheric Aberration | — | MEDIUM |
| 5 | Scavenger | East | Ruin-Warden | — | MEDIUM |
| 6 | Mystic | West | Ruin-Warden | Fight | LOW |

**Why these combinations?**
- Playtest #1: Tests combat-heavy path with physical boss
- Playtest #2: Tests exploration path with negotiation mechanic
- Playtest #3: Tests magic-focused build vs magic boss
- Playtest #4-6: Coverage for remaining combinations

---

## Playtest #1: Warrior → East Wing → Ruin-Warden

**Goal:** Test combat-heavy path with traditional boss

### Setup
- Create new character: Warrior class
- Name: "TestWarrior1" (for easy identification)
- Starting attributes: High MIGHT, moderate STURDINESS

### Route
1. Entrance (Room 1) - Safe zone
2. Corridor (Room 2) - 2x Corrupted Servitor
3. Salvage Bay (Room 3) - 1x Servitor + 1x Scrap-Hound
4. **Operations Center (Room 4)** - Pick up 2x Clan-Forged loot, choose **EAST**
5. Arsenal (Room 5) - 3x Blight-Drone
6. Training Chamber (Room 6) - 1x War-Frame (mini-boss)
7. **Ammunition Forge (Room 7)** - 2x Blight-Drone + environmental hazard
8. Vault Antechamber (Room 11) - 3x Blight-Drone + 1x Scrap-Hound
9. **Vault Corridor (Room 12)** - Attempt secret room puzzle (optional)
10. **Arsenal Vault (Room 14)** - Ruin-Warden boss

### Critical Tests

**Room 4 - Operations Center:**
- [ ] 2x Clan-Forged items appear on ground
- [ ] Items are class-appropriate (warrior weapons)
- [ ] Exits clearly show: south (back), east (arsenal), west (archives)

**Room 7 - Ammunition Forge:**
- [ ] Environmental hazard warning displays at combat start
- [ ] Hazard deals 1d6 damage per turn
- [ ] Puzzle option available (solve to disable hazard)
- [ ] Puzzle success disables hazard immediately
- [ ] Puzzle rewards Optimized equipment

**Room 6 - War-Frame Mini-Boss:**
- [ ] War-Frame uses PrecisionStrike (high damage)
- [ ] War-Frame uses TacticalReposition (defense boost)
- [ ] War-Frame uses EmergencyRepairs (heal 10 HP)
- [ ] Fight feels challenging but fair
- [ ] Estimated time to defeat: _____ minutes

**Room 14 - Ruin-Warden Boss:**
- [ ] Phase transition at 50% HP
- [ ] Boss difficulty appropriate for end-game
- [ ] Estimated time to defeat: _____ minutes
- [ ] Victory screen displays correctly

### Data Collection

**Playtime:**
- Start time: _____
- End time: _____
- Total duration: _____ minutes

**Combat Statistics:**
- Total combats: _____
- Deaths: _____
- Close calls (HP < 10): _____
- Final HP: _____ / _____

**Progression:**
- Starting Legend: 0
- Final Legend: _____
- Final Level: _____
- Milestones earned: _____

**Loot Obtained:**
- Operations Center: _____________
- Ammunition Forge puzzle: _____________
- Combat drops: _____________
- Secret room (if found): _____________

**Difficulty Feedback:**
- Easiest encounter: _____________
- Hardest encounter: _____________
- Most fun encounter: _____________
- Most frustrating moment: _____________

### Notes

Record any bugs, balance issues, or suggestions:

```
[Your notes here]
```

---

## Playtest #2: Scavenger → West Wing → Ruin-Warden (Talk)

**Goal:** Test exploration path with Forlorn Scholar negotiation

### Setup
- Create new character: Scavenger class
- Name: "TestScavenger1"
- Starting attributes: High FINESSE, moderate WITS

### Route
1. Entrance (Room 1) - Safe zone
2. Corridor (Room 2) - 2x Corrupted Servitor
3. Salvage Bay (Room 3) - 1x Servitor + 1x Scrap-Hound
4. **Operations Center (Room 4)** - Pick up 2x Clan-Forged loot, choose **WEST**
5. **Research Archives (Room 8)** - Puzzle (WITS DC 4), no combat
6. Specimen Containment (Room 9) - 2x Test Subject
7. **Observation Deck (Room 10)** - Forlorn Scholar (TALK MECHANIC)
8. Vault Antechamber (Room 11) - 3x Blight-Drone + 1x Scrap-Hound
9. **Vault Corridor (Room 12)** - Attempt secret room puzzle (DC 5)
10. **Arsenal Vault (Room 14)** - Ruin-Warden boss

### Critical Tests

**Room 8 - Research Archives:**
- [ ] No combat encounter (puzzle only)
- [ ] Puzzle description clear
- [ ] WITS check DC 4 feels fair
- [ ] Success rewards Clan-Forged equipment
- [ ] Failure has no penalty (just no loot)

**Room 10 - Observation Deck (Forlorn Scholar):**
- [ ] Special encounter message displays
- [ ] "talk" command available
- [ ] WILL check DC 4 executes
- [ ] Success: Room cleared, loot granted, Legend awarded, boss hints given
- [ ] Failure: Combat starts normally
- [ ] "attack" command allows skipping negotiation

**Test Talk Success Path:**
- [ ] Type "talk" or "speak" or "negotiate"
- [ ] WILL check rolls (note your WILL dice and result)
- [ ] If successful:
  - [ ] Scholar dialogue appears
  - [ ] Boss hints provided (mentions Warden and Aberration)
  - [ ] Optimized loot granted
  - [ ] Legend gain: 35 Legend
  - [ ] Room marked as cleared
- [ ] Can continue to Vault Antechamber

**Test Talk Failure Path (reload save if needed):**
- [ ] Type "talk" with low WILL character
- [ ] WILL check fails
- [ ] Combat automatically starts
- [ ] Forlorn Scholar uses AethericBolt, RealityDistortion, PhaseShift
- [ ] Defeat normally, gain loot

**Room 12 - Vault Corridor Secret Room:**
- [ ] WITS check DC 5 available
- [ ] Success unlocks south exit
- [ ] Message: "You notice a hidden door!"
- [ ] South leads to Supply Cache (Room 13)
- [ ] Supply Cache has 3x Myth-Forged items
- [ ] Items only spawn once (check on re-entry)

### Data Collection

**Playtime:**
- Start time: _____
- End time: _____
- Total duration: _____ minutes

**Talk Mechanic:**
- Forlorn Scholar: [ ] Talked [ ] Fought
- Talk result: [ ] Success [ ] Failure
- WILL dice rolled: _____
- WILL successes: _____
- Felt fair? [ ] Yes [ ] No

**Puzzle Feedback:**
- Research Archives (DC 4): [ ] Success [ ] Failure
- Vault Corridor (DC 5): [ ] Success [ ] Failure [ ] Skipped
- Puzzles felt: [ ] Too Easy [ ] Fair [ ] Too Hard

**Difficulty Comparison:**
- West Wing felt: [ ] Easier [ ] Same [ ] Harder than expected
- Fewer combats compensated by puzzles? [ ] Yes [ ] No

### Notes

```
[Your notes here]
```

---

## Playtest #3: Mystic → West Wing → Aetheric Aberration (Talk)

**Goal:** Test magic build vs magic boss

### Setup
- Create new character: Mystic class
- Name: "TestMystic1"
- Starting attributes: High WILL, moderate WITS

### Route
Same as Playtest #2, but:
- Choose **Energy Core (Room 15)** instead of Arsenal Vault
- Fight **Aetheric Aberration** (magic boss)

### Critical Tests

**Room 15 - Aetheric Aberration Boss:**
- [ ] Phase 1 (100%-50% HP):
  - [ ] VoidBlast (high WILL damage)
  - [ ] SummonEchoes (placeholder message)
  - [ ] RealityTear (AOE magic damage)
  - [ ] PhaseShift (+90% evasion)
- [ ] Phase 2 (50%-0% HP):
  - [ ] AethericStorm (massive AOE)
  - [ ] VoidBlast (increased dice)
  - [ ] DesperateSummon (placeholder message)
- [ ] Boss feels different from Ruin-Warden
- [ ] Magic damage ignores armor (as expected)
- [ ] WILL defense is key (not STURDINESS)

**Mystic Class Performance:**
- [ ] High WILL helps defend against magic attacks
- [ ] Forlorn Scholar talk likely succeeds (high WILL)
- [ ] Mystic abilities effective in combat
- [ ] Class feels viable throughout run

**Boss Comparison:**
- Aetheric Aberration vs Ruin-Warden:
  - Difficulty: [ ] Easier [ ] Same [ ] Harder
  - More fun: [ ] Aberration [ ] Ruin-Warden [ ] Equal
  - Estimated time: _____ minutes (Aberration) vs _____ minutes (Warden)

### Data Collection

**Boss Battle Analysis:**
- Phase 1 duration: _____ turns
- Phase 2 duration: _____ turns
- Total boss HP: 60
- Damage taken from boss: _____
- Closest moment: _____

**Magic Defense:**
- WILL attribute: _____
- Successful WILL saves: _____
- Failed WILL saves: _____
- Magic damage felt manageable? [ ] Yes [ ] No

### Notes

```
[Your notes here]
```

---

## Playtest #4-6: Optional Coverage

Complete remaining combinations as time permits:

### Playtest #4: Warrior → East → Aetheric Aberration
**Focus:** Test physical build vs magic boss (potentially harder)

### Playtest #5: Scavenger → East → Ruin-Warden
**Focus:** Test FINESSE build on combat-heavy path

### Playtest #6: Mystic → West → Ruin-Warden (Fight Scholar)
**Focus:** Test choosing to fight instead of talk

---

## Bug Reporting Template

When you encounter a bug, record it using this format:

```markdown
**Bug #X:** [Short description]

**Steps to Reproduce:**
1. [Step 1]
2. [Step 2]
3. [Expected: X, Actual: Y]

**Impact:** [ ] Critical [ ] Major [ ] Minor [ ] Cosmetic

**Reproducible:** [ ] Always [ ] Sometimes [ ] Once

**Additional Info:**
[Screenshots, logs, or detailed description]
```

---

## Balance Issue Template

When you identify a balance problem:

```markdown
**Balance Issue #X:** [Short description]

**Problem:**
[Describe what feels unbalanced]

**Evidence:**
- [Quantitative data if available]
- [Qualitative feedback]

**Suggested Fix:**
[Your recommendation]

**Priority:** [ ] High [ ] Medium [ ] Low
```

---

## Post-Playtest Summary

After completing all playtests, fill out this summary:

### Overall Impressions

**What worked well:**
1. _____________
2. _____________
3. _____________

**What needs improvement:**
1. _____________
2. _____________
3. _____________

**Most fun moment:**
_____________

**Most frustrating moment:**
_____________

### Path Comparison

| Metric | East Wing | West Wing | Notes |
|--------|-----------|-----------|-------|
| Avg. Playtime | _____ min | _____ min | |
| Difficulty | [1-5] | [1-5] | |
| Fun Factor | [1-5] | [1-5] | |
| Legend Gain | _____ | _____ | |
| Recommended | [ ] Yes [ ] No | [ ] Yes [ ] No | |

### Boss Comparison

| Metric | Ruin-Warden | Aetheric Aberration | Notes |
|--------|-------------|---------------------|-------|
| Avg. Time | _____ min | _____ min | |
| Difficulty | [1-5] | [1-5] | |
| Fun Factor | [1-5] | [1-5] | |
| Recommended | [ ] Yes [ ] No | [ ] Yes [ ] No | |

### Enemy Balance

Rate each enemy on difficulty (1-5 scale, where 3 = balanced):

- Corrupted Servitor: [___]
- Scrap-Hound: [___]
- Blight-Drone: [___]
- Test Subject: [___]
- War-Frame: [___]
- Forlorn Scholar: [___]
- Ruin-Warden: [___]
- Aetheric Aberration: [___]

**Notes on specific enemies:**
```
[Your notes]
```

### New Features Feedback

**Forlorn Scholar Talk Mechanic:**
- Worked as expected: [ ] Yes [ ] No
- Felt rewarding: [ ] Yes [ ] No
- Would use again: [ ] Yes [ ] No
- Notes: _____________

**Environmental Hazards:**
- Noticed immediately: [ ] Yes [ ] No
- Felt impactful: [ ] Yes [ ] No
- Added strategic depth: [ ] Yes [ ] No
- Notes: _____________

**Secret Room:**
- Found naturally: [ ] Yes [ ] No
- Felt rewarding: [ ] Yes [ ] No
- Worth the puzzle difficulty: [ ] Yes [ ] No
- Notes: _____________

### Recommendations for v0.5

**Must Fix:**
1. _____________
2. _____________
3. _____________

**Should Fix:**
1. _____________
2. _____________
3. _____________

**Nice to Have:**
1. _____________
2. _____________
3. _____________

---

## Sharing Feedback

After completing playtests, share your findings:

1. **Create GitHub Issue:**
   - Title: `[Playtest] v0.4 Balance Feedback`
   - Attach this completed guide
   - Tag as: `feedback`, `balance`, `v0.4`

2. **Summary Email/Message:**
   ```
   Completed X/6 playtests for v0.4 Content Expansion.

   Key Findings:
   - [Finding 1]
   - [Finding 2]
   - [Finding 3]

   Bugs Found: X (see attached)
   Balance Issues: X (see attached)

   Overall Verdict: [Ready for release / Needs minor tweaks / Needs major work]
   ```

3. **Attach Supporting Files:**
   - This completed playtest guide
   - Screenshots of bugs
   - Save files (if relevant)

---

## Thank You!

Your playtesting helps make Rune & Rust better. Thank you for your time and detailed feedback!

**Questions or Issues?**
- Check IMPLEMENTATION_V04.md for known issues
- See BALANCE_REVIEW_V04.md for mathematical analysis
- Contact developer with questions

**Version History:**
- v1.0 (2025-11-11): Initial playtest guide for v0.4
