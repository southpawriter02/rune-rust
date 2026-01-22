# Encounter Composition Calculator

Parent item: Template (Template%202ba55eb312da80a8b0f3fbfdcd27220c.md)

**Purpose**: Validate encounter difficulty and balance using the **Encounter Threat Score (ETS)** formula from SPEC-COMBAT-012.

**When to Use**: When designing multi-enemy encounters for dungeons, boss fights, or procedural spawning to ensure difficulty matches player Legend level.

**Reference**: See SPEC-COMBAT-012 FR-001 (Threat Level Classification) and "Encounter Composition Guidelines" section for design rules.

---

## How to Use This Calculator

**Step 1**: Fill in enemy composition below
**Step 2**: Calculate total ETS (sum of all enemy Legend values)
**Step 3**: Compare ETS to player Legend-appropriate ranges
**Step 4**: Check balance warnings (enemy count, damage output, TTK estimates)
**Step 5**: Adjust composition if needed using recommendations

---

## Encounter Composition Input

**Encounter Name**: _______________________________________________

**Intended Player Legend Level**: _____ (1-10)

**Intended Difficulty** (select one):

- [ ]  **Easy** (warm-up, rest area ambush, tutorial)
- [ ]  **Medium** (standard combat, exploration encounter)
- [ ]  **Hard** (challenging setpiece, mini-boss fight)
- [ ]  **Boss** (end-of-act climactic encounter)

---

## Enemy Composition

**Fill in the enemies in your encounter below:**

| Enemy Type | Threat Tier | Legend Value (per enemy) | Count | Total Legend (Value × Count) |
| --- | --- | --- | --- | --- |
| ___________ | _____ | _____ | _____ | _____ |
| ___________ | _____ | _____ | _____ | _____ |
| ___________ | _____ | _____ | _____ | _____ |
| ___________ | _____ | _____ | _____ | _____ |
| ___________ | _____ | _____ | _____ | _____ |
| ___________ | _____ | _____ | _____ | _____ |

**Total Enemy Count**: _____ enemies
**Total Encounter Threat Score (ETS)**: _____ (sum of Total Legend column)

---

## ETS Validation

### Target ETS Ranges (from SPEC-COMBAT-012)

**For Player Legend _____:**

| Difficulty | Target ETS Range | Your ETS | Within Range? |
| --- | --- | --- | --- |
| **Easy** | _____ - _____ | _____ | ✓ / ✗ |
| **Medium** | _____ - _____ | _____ | ✓ / ✗ |
| **Hard** | _____ - _____ | _____ | ✓ / ✗ |
| **Boss** | _____ - _____ | _____ | ✓ / ✗ |

**Reference Table** (copy appropriate row based on player Legend):

| Player Legend | Easy ETS | Medium ETS | Hard ETS | Boss ETS |
| --- | --- | --- | --- | --- |
| Legend 1 | 15-30 | 30-50 | 50-80 | 100-150 |
| Legend 2 | 20-40 | 40-65 | 65-100 | 100-150 |
| Legend 3 | 30-60 | 60-100 | 100-150 | 150-200 |
| Legend 4 | 40-80 | 80-120 | 120-180 | 180-230 |
| Legend 5 | 50-100 | 100-150 | 150-250 | 200-300 |

**ETS Result**:

- [ ]  **Too Easy** (ETS below target range): Encounter may feel trivial, consider adding 1-2 Low tier enemies
- [ ]  **Balanced** (ETS within target range): Encounter difficulty matches intent ✓
- [ ]  **Too Hard** (ETS above target range): Encounter may cause player wipes, consider reducing enemy count or tier

---

## Balance Warnings

### Enemy Count Check

**Total Enemy Count**: _____ enemies

- [ ]  **1 Enemy** (Solo Encounter):
    - ✓ Acceptable if Boss/Lethal tier (Ruin-Warden, Omega Sentinel)
    - ✓ Acceptable if High tier with interesting abilities (Vault Custodian, Rust-Witch)
    - ✗ Warning: Low/Medium tier solo feels unrewarding unless narrative encounter
- [ ]  **2-3 Enemies** (Standard Combat):
    - ✓ Ideal encounter size for tactical variety
    - ✓ Allows focus-fire priorities, AoE value, positioning tactics
- [ ]  **4-5 Enemies** (Large Battle):
    - ✓ Tests resource management, AoE abilities, target prioritization
    - ⚠️ Warning: Ensure not all Medium/High tier (overwhelming damage output)
- [ ]  **6+ Enemies** (Swarm/Boss Reinforcements):
    - ⚠️ Warning: UI clutter, turn resolution time exceeds 2 minutes
    - ✓ Only use for swarm encounters (all Low tier) or boss reinforcement waves
    - ✗ AVOID: 6+ Medium/High tier enemies (unfun difficulty spike)

### Threat Tier Distribution Check

**Enemy Breakdown by Tier**:

- Low Tier (10-20 Legend): _____ enemies
- Medium Tier (15-50 Legend): _____ enemies
- High Tier (55-75 Legend): _____ enemies
- Lethal Tier (60-100 Legend): _____ enemies
- Boss Tier (100-150 Legend): _____ enemies

**Balance Warnings**:

- [ ]  **4+ Medium tier enemies**: ⚠️ Warning - May create overwhelming damage output (check total DPS below)
- [ ]  **2+ High tier enemies**: ⚠️ Warning - High threat, ensure player has defensive options
- [ ]  **2+ Lethal tier enemies**: ✗ Avoid - Likely party wipe unless Legend 5+ players
- [ ]  **2+ Boss tier enemies**: ✗ Never - Violates boss encounter design (1 boss per encounter)

### Damage Output Check

**Calculate Total Enemy DPS** (damage per turn across all enemies):

| Enemy Type | Avg Damage (per enemy) | Count | Total Damage (Avg × Count) |
| --- | --- | --- | --- |
| ___________ | _____ | _____ | _____ |
| ___________ | _____ | _____ | _____ |
| ___________ | _____ | _____ | _____ |
| ___________ | _____ | _____ | _____ |
| ___________ | _____ | _____ | _____ |

**Total Encounter DPS**: _____ damage/turn (sum of Total Damage column)

**Damage Reference Table** (avg damage by tier):

- Low Tier: 3-5 damage/enemy
- Medium Tier: 6-10 damage/enemy
- High Tier: 11-16 damage/enemy
- Lethal Tier: 17-24 damage/enemy
- Boss Tier: 12-20 damage/enemy

**Player HP Pool** (Legend _____): _____ HP (use 30 HP for Legend 1, 45 HP for Legend 3, 60 HP for Legend 5)

**Lethality Check**:

- Total Encounter DPS: _____ damage/turn
- % of Player HP: _____ % (Total DPS ÷ Player HP × 100)
- **Turns to Kill Player**: _____ turns (Player HP ÷ Total DPS)

**Lethality Warnings**:

- [ ]  **Turns to Kill < 2 turns**: ✗ Critical - Instant death encounter, reduce damage or enemy count
- [ ]  **Turns to Kill 2-3 turns**: ⚠️ Warning - Very high lethality, only for Hard/Boss difficulty
- [ ]  **Turns to Kill 4-6 turns**: ✓ Balanced - Meaningful threat, requires healing/defense
- [ ]  **Turns to Kill 7-10 turns**: ✓ Moderate - Sustained combat, attrition threat
- [ ]  **Turns to Kill 10+ turns**: ✓ Low threat - Safe for Easy difficulty, exploration encounters

### Time-to-Kill (TTK) Estimate

**Estimate how long it takes player to kill all enemies:**

**Player Assumptions** (Legend _____):

- Player Avg Damage: _____ (6-7 for Legend 1, 10-12 for Legend 3, 15-18 for Legend 5)
- Player Hit Rate: 65% (default)
- Player Effective DPS: _____ × 0.65 = _____ damage/turn

**Enemy HP Totals**:

| Enemy Type | HP (per enemy) | Count | Total HP (HP × Count) |
| --- | --- | --- | --- |
| ___________ | _____ | _____ | _____ |
| ___________ | _____ | _____ | _____ |
| ___________ | _____ | _____ | _____ |
| ___________ | _____ | _____ | _____ |
| ___________ | _____ | _____ | _____ |

**Total Encounter HP**: _____ HP (sum of Total HP column)

**Estimated TTK**: _____ turns (Total Encounter HP ÷ Player Effective DPS)

**TTK Targets by Difficulty**:

| Difficulty | Solo Player TTK Target | Your Estimated TTK | Within Range? |
| --- | --- | --- | --- |
| Easy | 3-6 turns | _____ turns | ✓ / ✗ |
| Medium | 6-12 turns | _____ turns | ✓ / ✗ |
| Hard | 12-20 turns | _____ turns | ✓ / ✗ |
| Boss | 15-30 turns | _____ turns | ✓ / ✗ |

**TTK Warnings**:

- [ ]  **TTK < 3 turns**: ⚠️ Too easy - Feels like "clicking through trash," add enemies or increase tier
- [ ]  **TTK 3-6 turns**: ✓ Easy - Quick combat, suitable for exploration
- [ ]  **TTK 6-12 turns**: ✓ Medium - Standard combat duration, tactical decisions matter
- [ ]  **TTK 12-20 turns**: ✓ Hard - Challenging combat, resource management required
- [ ]  **TTK 20-30 turns**: ✓ Boss - Epic encounter, multi-phase mechanics recommended
- [ ]  **TTK > 30 turns**: ✗ Too long - Combat becomes tedious, reduce HP or enemy count

---

## Encounter Archetype Patterns

**Select the encounter pattern that best matches your composition:**

### Pattern 1: Balanced (2× Medium Tier)

**Example**: 2× Blight-Drone (25 Legend each)

- **ETS**: 50 (Medium difficulty for Legend 1-2)
- **Design Pattern**: No focus-fire priority, AoE valuable, sustained combat
- **TTK Estimate**: 6-8 turns
- **Pros**: Simple, clean combat; good for testing player damage output
- **Cons**: Can feel repetitive without varied enemy abilities

### Pattern 2: Tank + DPS (1× High + 1× Medium)

**Example**: 1× Vault Custodian (75 Legend) + 1× Test Subject (20 Legend)

- **ETS**: 95 (Hard difficulty for Legend 1-2)
- **Design Pattern**: Priority kill DPS first, Tank protects/delays
- **TTK Estimate**: 12-15 turns (Tank takes majority of time)
- **Pros**: Forces target prioritization, tests player decision-making
- **Cons**: If DPS dies first, Tank becomes slow cleanup

### Pattern 3: Swarm (3-5× Low Tier)

**Example**: 5× Corrupted Servitor (10 Legend each)

- **ETS**: 50 (Medium difficulty for Legend 1-2)
- **Design Pattern**: AoE/cleave testing, attrition damage, spatial control
- **TTK Estimate**: 8-12 turns (individual kills are fast, but many targets)
- **Pros**: Tests AoE abilities, creates dynamic movement/positioning
- **Cons**: Can feel overwhelming if player lacks AoE

### Pattern 4: Caster + Meat Shield (1× Tank + 1× Caster)

**Example**: 1× Maintenance Construct (15 Legend) + 1× Forlorn Scholar (35 Legend)

- **ETS**: 50 (Medium difficulty for Legend 1-2)
- **Design Pattern**: Protect priority (Caster), tactical positioning
- **TTK Estimate**: 6-8 turns
- **Pros**: Tests tactical thinking, makes Caster feel threatening
- **Cons**: If Tank dies first, Caster becomes trivial

### Pattern 5: Mixed Threat (2× Low + 1× Medium + 1× High)

**Example**: 2× Corrupted Servitor (10 Legend) + 1× Blight-Drone (25 Legend) + 1× Bone-Keeper (55 Legend)

- **ETS**: 100 (Hard difficulty for Legend 2-3)
- **Design Pattern**: Clear target priority (High → Medium → Low), threat assessment
- **TTK Estimate**: 12-18 turns
- **Pros**: Varied combat, multiple decision points, tests prioritization
- **Cons**: Can overwhelm new players unfamiliar with threat assessment

### Pattern 6: Boss + Reinforcements (1× Boss + 2-4× Low/Medium)

**Example**: 1× Ruin-Warden (100 Legend) + 2× Corrupted Servitor (10 Legend each), then +2× Blight-Drone (25 Legend each) at 50% HP

- **ETS**: 160 (Boss difficulty for Legend 2-3)
- **Design Pattern**: Reinforcement waves at phase transitions, sustained resource management
- **TTK Estimate**: 20-30 turns (multi-phase boss fight)
- **Pros**: Epic encounter, prevents burst strategies, tests resource management
- **Cons**: Can feel frustrating if reinforcements spawn at bad times

### Pattern 7: Elite Solo (1× High/Lethal Tier)

**Example**: 1× Rust-Witch (75 Legend) or 1× Sentinel Prime (100 Legend)

- **ETS**: 75-100 (Hard difficulty for Legend 3-4)
- **Design Pattern**: Boss-adjacent difficulty, focus on single-target damage
- **TTK Estimate**: 10-16 turns
- **Pros**: Clean 1v1 encounter, showcases enemy abilities, feels like duel
- **Cons**: No tactical variety (no target priority), can feel slow if enemy is tanky

---

## Adjustment Recommendations

**If your encounter is too easy (ETS below target):**

1. **Add 1-2 Low tier enemies** (+10-20 ETS, +3-5 damage/turn, +1-2 turns TTK)
    - Safest adjustment, minimal complexity increase
    - Example: 2× Blight-Drone (ETS 50) → 2× Blight-Drone + 2× Corrupted Servitor (ETS 70)
2. **Upgrade 1 enemy to next tier** (+15-30 ETS, +5-10 damage/turn, +3-5 turns TTK)
    - Increases threat without adding UI clutter
    - Example: 2× Blight-Drone + 1× Corrupted Servitor (ETS 60) → 2× Blight-Drone + 1× War-Frame (ETS 100)
3. **Add special mechanic** (+10-20% effective difficulty, no ETS change)
    - IsForlorn: Adds trauma pressure (encourages fast kills)
    - Soak: Increases TTK by 15-20% per Soak point
    - Example: 1× War-Frame (ETS 50) → 1× War-Frame with Soak 2 (feels like ETS 65)

**If your encounter is too hard (ETS above target):**

1. **Remove 1-2 Low tier enemies** (-10-20 ETS, -3-5 damage/turn, -1-2 turns TTK)
    - Safest reduction, preserves core encounter structure
    - Example: 3× Corrupted Servitor + 1× Vault Custodian (ETS 105) → 1× Corrupted Servitor + 1× Vault Custodian (ETS 85)
2. **Downgrade 1 enemy to lower tier** (-20-40 ETS, -5-15 damage/turn, -3-6 turns TTK)
    - Major difficulty reduction without removing enemies
    - Example: 1× Bone-Keeper + 2× Blight-Drone (ETS 105) → 1× War-Frame + 2× Blight-Drone (ETS 100)
3. **Replace High tier with 2× Low tier** (ETS neutral, but lowers lethality)
    - Shifts from burst damage to attrition threat
    - Example: 1× Bone-Keeper (ETS 55, 11 damage) → 3× Corrupted Servitor (ETS 30, 9 total damage)

**If your encounter has balance warnings (too many enemies, too high DPS):**

1. **Split into 2 sequential encounters** (same total ETS, but staged difficulty)
    - Prevents overwhelming first turn damage
    - Example: 6× Corrupted Servitor (ETS 60) → 3× Servitor (ETS 30), rest 10 seconds, then 3× Servitor (ETS 30)
2. **Add reinforcement wave trigger** (same enemies, but staggered spawning)
    - Boss encounter structure, prevents alpha strike strategies
    - Example: 1× Ruin-Warden + 4× Servitor (ETS 140) → 1× Ruin-Warden + 2× Servitor (ETS 120), then +2× Servitor at 50% HP
3. **Replace multiple Medium with 1× High** (lower total damage, higher HP pool)
    - Reduces turn-1 lethality, increases TTK
    - Example: 3× Blight-Drone (ETS 75, 12 damage/turn) → 1× Vault Custodian (ETS 75, 9 damage/turn)

---

## Example Validated Encounters

### Example 1: Early Game Exploration (Legend 1, Easy Difficulty)

**Composition**: 2× Corrupted Servitor (10 Legend each)

- **Total Enemy Count**: 2
- **Total ETS**: 20
- **Target ETS Range**: 15-30 (Easy for Legend 1) ✓
- **Total Encounter DPS**: 6 damage/turn (2 enemies × 3 avg damage)
- **Turns to Kill Player**: 30 HP ÷ 6 DPS = 5 turns (Safe)
- **Estimated Player TTK**: (15 HP + 15 HP) ÷ 4 DPS = 7.5 turns ✓
- **Balance Warnings**: None
- **Verdict**: Balanced easy encounter, good for Legend 1 exploration

---

### Example 2: Mid-Game Challenge (Legend 3, Medium Difficulty)

**Composition**: 1× Vault Custodian (75 Legend) + 2× Blight-Drone (25 Legend each)

- **Total Enemy Count**: 3
- **Total ETS**: 125
- **Target ETS Range**: 60-100 (Medium for Legend 3) ⚠️ **Too Hard**
- **Total Encounter DPS**: 9 + 8 + 8 = 25 damage/turn
- **Turns to Kill Player**: 45 HP ÷ 25 DPS = 1.8 turns ✗ **Critical**
- **Estimated Player TTK**: (70 HP + 25 HP + 25 HP) ÷ 7 DPS = 17 turns ✓
- **Balance Warnings**:
    - ✗ Turns to Kill < 2 turns (instant death encounter)
    - ⚠️ ETS above target range (too hard)
- **Verdict**: Unbalanced - Reduce to 1× Vault Custodian + 1× Blight-Drone (ETS 100) or downgrade Vault Custodian to War-Frame (ETS 100)

**Adjusted Composition**: 1× War-Frame (50 Legend) + 2× Blight-Drone (25 Legend each)

- **Total ETS**: 100 ✓
- **Total Encounter DPS**: 7 + 8 + 8 = 23 damage/turn
- **Turns to Kill Player**: 45 HP ÷ 23 DPS = 2 turns ⚠️ **High lethality, but acceptable for Medium**
- **Estimated Player TTK**: (50 HP + 25 HP + 25 HP) ÷ 7 DPS = 14 turns ✓
- **Verdict**: Balanced medium encounter after adjustment

---

### Example 3: Boss Encounter (Legend 3, Boss Difficulty)

**Composition**: 1× Forlorn Archivist (150 Legend) + 2× Forlorn Scholar (35 Legend each) at 50% HP transition

- **Total Enemy Count**: 1 (initially), 3 (after reinforcements)
- **Total ETS**: 220 (150 + 70)
- **Target ETS Range**: 150-200 (Boss for Legend 3) ⚠️ **Slightly high**
- **Phase 1 DPS**: 10.5 damage/turn (Archivist only)
- **Phase 2 DPS**: 10.5 + 7 + 7 = 24.5 damage/turn (after reinforcements)
- **Turns to Kill Player (Phase 1)**: 45 HP ÷ 10.5 DPS = 4.3 turns ✓
- **Turns to Kill Player (Phase 2)**: 45 HP ÷ 24.5 DPS = 1.8 turns ⚠️ **High lethality after reinforcements**
- **Estimated Player TTK**: (80 HP + 30 HP + 30 HP) ÷ 7 DPS = 20 turns ✓
- **Balance Warnings**:
    - ⚠️ ETS slightly above target (220 vs. 200 max)
    - ⚠️ Phase 2 lethality very high (1.8 turns to kill)
- **Verdict**: Acceptable boss difficulty - High lethality in Phase 2 creates urgency (kill adds fast), but initial phase is safe. Consider reducing to 1× Forlorn Scholar (ETS 185) if too punishing.

---

### Example 4: Swarm Encounter (Legend 2, Medium Difficulty)

**Composition**: 6× Corrupted Servitor (10 Legend each)

- **Total Enemy Count**: 6
- **Total ETS**: 60
- **Target ETS Range**: 40-65 (Medium for Legend 2) ✓
- **Total Encounter DPS**: 18 damage/turn (6 enemies × 3 avg damage)
- **Turns to Kill Player**: 37 HP ÷ 18 DPS = 2.1 turns ⚠️ **High lethality**
- **Estimated Player TTK**: (15 HP × 6) ÷ 5 DPS = 18 turns ✓
- **Balance Warnings**:
    - ⚠️ 6 enemies (UI clutter warning)
    - ⚠️ High turn-1 lethality (18 damage if all hit)
- **Verdict**: Acceptable swarm encounter, but very punishing if player lacks AoE. Recommend 5× Servitor (ETS 50, 15 damage/turn, 2.5 turns to kill) for safer difficulty.

---

## Final Validation Checklist

Before implementing your encounter, verify:

**ETS Validation**:

- [ ]  Total ETS calculated correctly (sum of all enemy Legend values)
- [ ]  ETS within target range for player Legend and intended difficulty
- [ ]  ETS not excessively above/below target (±20% acceptable, ±50% problematic)

**Enemy Count**:

- [ ]  Enemy count within 1-6 range (avoid 7+ enemies)
- [ ]  Solo encounters are Boss/Lethal/High tier (not Low/Medium)
- [ ]  4+ enemy encounters are mostly Low tier or reinforcement waves

**Damage Output**:

- [ ]  Total encounter DPS calculated
- [ ]  Turns to kill player is 2+ turns (no instant death encounters)
- [ ]  High lethality (< 3 turns to kill) only for Hard/Boss difficulty

**Time-to-Kill**:

- [ ]  Estimated TTK within target range for difficulty (Easy 3-6, Medium 6-12, Hard 12-20, Boss 15-30)
- [ ]  TTK not excessively long (< 30 turns)

**Special Considerations**:

- [ ]  IsForlorn enemies present? (Trauma pressure encourages fast kills, may feel harder than ETS suggests)
- [ ]  Soak enemies present? (Increases effective HP by 15-20% per Soak point)
- [ ]  Phase-based AI enemies? (Boss fights, TTK increases with reinforcement waves)

**Design Intent**:

- [ ]  Encounter matches intended archetype pattern (Balanced, Tank+DPS, Swarm, etc.)
- [ ]  Encounter has clear target priority (if multi-enemy)
- [ ]  Encounter tests specific player skill (AoE for swarms, target priority for mixed threat, etc.)

---

## Reference: Quick ETS Formula

```
Encounter Threat Score (ETS) = Σ(Enemy Legend Value × Enemy Count)

Example:
2× Corrupted Servitor (10 Legend each) + 1× Vault Custodian (75 Legend)
ETS = (10 × 2) + (75 × 1) = 20 + 75 = 95

```

**Difficulty Tier Mapping**:

- **Easy**: ETS 10-25 (tutorial, rest encounters)
- **Medium**: ETS 25-50 (standard exploration combat)
- **Hard**: ETS 50-100 (challenging setpieces)
- **Boss**: ETS 100-200 (act-ending encounters)

**Player Legend Scaling** (multiply by Legend ÷ 3 for higher levels):

- Legend 1: Use base ETS ranges (15-30 Easy, 30-50 Medium, 50-80 Hard)
- Legend 3: Multiply by 2× (30-60 Easy, 60-100 Medium, 100-150 Hard)
- Legend 5: Multiply by 3× (50-100 Easy, 100-150 Medium, 150-250 Hard)

---

## Reference Documentation

**Related Templates**:

- **Enemy Design Worksheet**: `/docs/templates/enemy-design-worksheet.md` (create enemies with Legend values)
- **AI Behavior Pattern Template**: `/docs/templates/ai-behavior-pattern-template.md` (implement AI for encounter enemies)
- **Enemy Bestiary Entry**: `/docs/templates/enemy-bestiary-entry.md` (document encounter enemies)

**SPEC-COMBAT-012 Sections**:

- **FR-001**: Threat Level Classification System (Legend value ranges per tier)
- **Balance & Tuning**: Encounter Composition Guidelines (ETS formula, recommended group types)
- **Appendix A**: Complete Enemy Statistics by Threat Tier (Legend values for all 20 enemies)

**Implementation Files**:

- `RuneAndRust.Core/Enemy.cs` (Enemy data model with BaseLegendValue property)
- `RuneAndRust.Core/Population/DormantProcess.cs` (ThreatLevel enum for procedural spawning)

---

**Designed By**: ___________________________ **Date**: ___________

**Reviewed By**: ___________________________ **Date**: ___________

---

**Last Updated**: 2025-11-22
**Status**: ✅ Complete