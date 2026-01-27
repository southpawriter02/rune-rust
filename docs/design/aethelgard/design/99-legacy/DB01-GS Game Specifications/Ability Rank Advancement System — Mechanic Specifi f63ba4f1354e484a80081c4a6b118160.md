# Ability Rank Advancement System — Mechanic Specification v5.0

Type: Mechanic
Description: Automatic ability rank progression system where Tier 1 abilities advance to Rank 2 after learning 2 Tier 2 abilities, and all abilities advance to Rank 3 upon Capstone acquisition. Includes rank scaling formulas (+1d6 per rank).
Priority: Must-Have
Status: Review
Target Version: Alpha
Dependencies: Archetype & Specialization System, Saga System (PP economy), Combat Resolution System
Implementation Difficulty: Hard
Balance Validated: No
Document ID: AAM-SPEC-MECH-ABILITYRANK-v5.0
Parent item: Saga System — Core System Specification v5.0 (Saga%20System%20%E2%80%94%20Core%20System%20Specification%20v5%200%201ff65eb749134fd796b2f2e7ea4bb619.md)
Proof-of-Concept Flag: No
Sub-Type: Core
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Voice Validated: No

## I. Core Philosophy: Automatic Mastery Through Investment

The Ability Rank Advancement System provides **automatic incremental power scaling** for abilities as players progress through specialization trees. Ranks increase automatically based on tree progression milestones—no PP is spent on rank-ups.

**Design Pillars:**

- **Automatic Mastery:** Ranks improve as players invest in specialization; mastery comes from dedication
- **Clear Power Scaling:** Each rank provides visible, measurable improvements (+1d6 per rank)
- **Milestone-Driven Progression:** Rank-ups tied to meaningful tree progression, not arbitrary spending

---

## II. Rank Progression Model

### Tier-Based Starting Ranks

| Tier | PP Cost to Learn | Starting Rank | Can Rank Up To |
| --- | --- | --- | --- |
| **Tier 1** | 0 PP (free) | Rank 1 | Rank 2 → Rank 3 |
| **Tier 2** | 4 PP each | Rank 2 | Rank 3 |
| **Tier 3** | 5 PP each | Rank 3 | — |
| **Capstone** | 6 PP | Rank 3 | — |

### Automatic Rank-Up Triggers

**Trigger 1: Rank 2 Advancement**

- **Condition:** Player learns their **2nd Tier 2 ability** in a specialization
- **Effect:** ALL learned Tier 1 abilities in that specialization automatically advance to Rank 2
- **Cost:** FREE (no PP spent)

**Trigger 2: Rank 3 Advancement**

- **Condition:** Player learns the **Capstone ability** in a specialization
- **Effect:** ALL learned Tier 1 AND Tier 2 abilities automatically advance to Rank 3
- **Cost:** FREE (no PP spent)

---

## III. Rank Scaling Formulas

### Damage Abilities

```
Rank 1: Base dice (e.g., 2d6)
Rank 2: Base + 1d6 (e.g., 3d6) → +50% average damage
Rank 3: Base + 2d6 (e.g., 4d6) → +100% average damage
```

### Healing Abilities

```
Rank 1: 2d6 HP healed
Rank 2: 3d6 HP healed
Rank 3: 4d6 HP healed + [Remove Bleeding]
```

### Duration-Based Abilities

```
Rank 1: D turns duration
Rank 2: D + 1 turns duration
Rank 3: D + 2 turns duration
```

### Status Effect Abilities

```
Rank 1: Single target, base duration
Rank 2: Single target, extended duration
Rank 3: AoE upgrade OR enhanced effect
```

---

## IV. Worked Examples

### Example 1: Warrior Berserkr Progression

```
MILESTONE 3: Unlock Berserkr (3 PP)
  → Grants: Furious Strike, Battle Shout, Reckless Charge (Tier 1, Rank 1)
  → Furious Strike deals 2d6 damage
  → PP Remaining: 0

MILESTONE 6-8: Learn Tier 2 abilities
  → Learn Berserk Rage (4 PP, starts at Rank 2)
  → Learn Fury Unleashed (4 PP, starts at Rank 2)
  
  [TRIGGER: 2nd Tier 2 learned]
  → Furious Strike: Rank 1 → Rank 2 (now deals 3d6!)
  → Battle Shout: Rank 1 → Rank 2
  → Reckless Charge: Rank 1 → Rank 2
  → ALL Tier 1 abilities upgraded FREE

MILESTONE 25-30: Learn Capstone
  → Learn "Unstoppable Fury" (Capstone, 6 PP)
  
  [TRIGGER: Capstone learned]
  → ALL Tier 1: Rank 2 → Rank 3 (Furious Strike now deals 4d6!)
  → ALL Tier 2: Rank 2 → Rank 3
  → Massive power spike, specialization mastery achieved
```

### Example 2: Multi-Specialization Independence

```
Player has:
  Berserkr (2 Tier 2 abilities learned) → Tier 1s at Rank 2
  Skjaldmaer (1 Tier 2 ability learned) → Tier 1s still at Rank 1

Rank-ups are PER-SPECIALIZATION. Berserkr progress does NOT affect Skjaldmaer.
```

---

## V. Resolution Pipeline

### On Ability Learning

```
1. Validate tier unlock (PPInTree >= threshold)
2. Validate PP available (PP >= ability cost)
3. Deduct PP, add ability to character
4. Set starting rank based on tier:
   - Tier 1: Rank 1
   - Tier 2: Rank 2
   - Tier 3/Capstone: Rank 3

5. Check rank-up triggers:
   IF ability.Tier == 2:
     countTier2 = COUNT(learned abilities WHERE Tier=2 AND SpecID=current)
     IF countTier2 >= 2:
       TriggerRank2Advancement(character, specializationID)
   
   IF ability.Tier == Capstone:
     TriggerRank3Advancement(character, specializationID)
```

### TriggerRank2Advancement

```
1. Query all learned Tier 1 abilities for this specialization
2. For each ability WHERE CurrentRank < 2:
   - Set CurrentRank = 2
   - Recalculate ability effects
3. Publish event: OnRank2Achieved(specializationID, affectedAbilities)
4. UI notification: "Your foundational abilities have improved!"
```

### TriggerRank3Advancement

```
1. Query all learned Tier 1 AND Tier 2 abilities for this specialization
2. For each ability WHERE CurrentRank < 3:
   - Set CurrentRank = 3
   - Recalculate ability effects
3. Publish event: OnRank3Achieved(specializationID, affectedAbilities)
4. UI notification: "MASTERY ACHIEVED! All abilities now Rank 3!"
```

---

## VI. Database Schema

### CharacterAbility Table

```sql
CREATE TABLE CharacterAbility (
  CharacterID INT NOT NULL,
  AbilityID INT NOT NULL,
  SpecializationID INT NOT NULL,
  CurrentRank INT NOT NULL DEFAULT 1,  -- 1, 2, or 3
  LearnedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (CharacterID, AbilityID),
  
  FOREIGN KEY (CharacterID) REFERENCES Characters(character_id),
  FOREIGN KEY (AbilityID) REFERENCES Abilities(ability_id),
  FOREIGN KEY (SpecializationID) REFERENCES Specialization(SpecializationID)
);
```

### AbilityRankEffects Table

```sql
CREATE TABLE AbilityRankEffects (
  AbilityID INT NOT NULL,
  Rank INT NOT NULL,  -- 1, 2, or 3
  DamageDice TEXT,    -- e.g., "3d6"
  Duration INT,       -- turns
  SpecialEffect TEXT, -- JSON for custom effects
  PRIMARY KEY (AbilityID, Rank)
);
```

---

## VII. Service Architecture

### AbilityRankService

```csharp
public interface IAbilityRankService
{
    // Rank checking
    int GetCurrentRank(int characterID, int abilityID);
    bool CanTriggerRank2(int characterID, int specializationID);
    bool CanTriggerRank3(int characterID, int specializationID);
    
    // Rank advancement (automatic, called internally)
    void TriggerRank2Advancement(int characterID, int specializationID);
    void TriggerRank3Advancement(int characterID, int specializationID);
    
    // Effect calculation
    AbilityEffects GetRankedEffects(int abilityID, int rank);
}
```

---

## VIII. Balance Tuning Parameters

| Parameter | Current | Range | Impact |
| --- | --- | --- | --- |
| Tier2TriggerCount | 2 | 1-3 | How soon Rank 2 occurs |
| DamageScalingPerRank | +1d6 | +1d6 to +2d6 | Power curve steepness |
| DurationScalingPerRank | +1 turn | +1 to +2 turns | Buff/debuff value |

### Progression Targets

- **First Rank 2:** Milestone 6-8 (after learning 2nd Tier 2 ability)
- **Rank 3 Mastery:** Milestone 25-30 (after Capstone)
- **Power Increase Rank 1→2:** ~50% (+1d6 on 2d6 base)
- **Power Increase Rank 1→3:** ~100% (+2d6 on 2d6 base)

---

## IX. Edge Cases

### Skip Rank 2, Rush to Capstone

- **Condition:** Player only has 1 Tier 2 ability learned, acquires Capstone
- **Behavior:** Tier 1 abilities jump directly Rank 1 → Rank 3 (skipping Rank 2)
- **Result:** Mastery still granted; suboptimal path but valid

### Unlearned Abilities

- **Condition:** Player skipped some Tier 1 abilities before rank-up trigger
- **Behavior:** Only LEARNED abilities upgrade; unlearned abilities stay at base rank when eventually learned
- **Note:** Cannot upgrade what isn't learned yet

### Multiple Specializations

- **Condition:** Player has Berserkr (2 Tier 2s) and Skjaldmaer (1 Tier 2)
- **Behavior:** Each specialization tracks independently; Berserkr Tier 1s at Rank 2, Skjaldmaer Tier 1s at Rank 1

---

## X. Integration Points

### Consumes From:

- **Archetype & Specialization System:** Tier structure, ability trees
- **Saga System:** PP economy (for learning, not ranking)
- **Combat Resolution:** Ability execution

### Consumed By:

- **Combat System:** Ranked effects determine damage/duration
- **UI Systems:** Rank display, power-up notifications
- **Balance Validation:** Power curve tracking

---

## XI. PP Progression Table

| Milestone | Cumulative PP | Progression Example | Rank Status |
| --- | --- | --- | --- |
| 3 | 5 | Unlock spec (3 PP) + 2 attributes | Tier 1s at Rank 1 |
| 6 | 8 | Learn 1st Tier 2 (4 PP) | Still Rank 1 |
| 8 | 10 | Learn 2nd Tier 2 (4 PP) | **[RANK 2 TRIGGER]** |
| 15 | 17 | Learn Tier 3 abilities (5 PP each) | Rank 2 maintained |
| 25 | 27 | Learn Capstone (6 PP) | **[RANK 3 TRIGGER]** |

---

*Consolidated from SPEC-PROGRESSION-003: Ability Rank Advancement System Specification*