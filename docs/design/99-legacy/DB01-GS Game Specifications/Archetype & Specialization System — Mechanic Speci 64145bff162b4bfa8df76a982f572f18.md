# Archetype & Specialization System — Mechanic Specification v5.0

Type: Mechanic
Description: Defines the 4 core archetypes (Warrior, Adept, Skirmisher, Mystic), starting attribute distributions, archetype abilities, resource systems, specialization unlock mechanics (3 PP), tier progression thresholds, and ability tree structure.
Priority: Must-Have
Status: Review
Target Version: Alpha
Dependencies: Saga System (PP economy), Dice Pool System (attribute-based rolls), Ability System (specialization abilities)
Implementation Difficulty: Very Complex
Balance Validated: No
Document ID: AAM-SPEC-MECH-ARCHETYPE-v5.0
Parent item: Saga System — Core System Specification v5.0 (Saga%20System%20%E2%80%94%20Core%20System%20Specification%20v5%200%201ff65eb749134fd796b2f2e7ea4bb619.md)
Proof-of-Concept Flag: No
Sub-Type: Core
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Voice Validated: No

## I. Core Philosophy: Character Identity Through Choice

The Archetype & Specialization System provides players with **distinct character identities at creation** (Archetypes) and **advanced build customization during gameplay** (Specializations). This creates diverse playstyles and meaningful build variety.

**Design Pillars:**

- **Distinct Archetype Identity:** Each archetype feels fundamentally different in combat, not just numerically different
- **Specialization as Build Direction:** Focused playstyle refinement, not mandatory upgrades
- **Meaningful Choice Through Scarcity:** 3 PP unlock cost forces careful specialization selection

---

## II. The Four Archetypes

### Archetype Attribute Distributions

| Archetype | MIGHT | FINESSE | WITS | WILL | STURDINESS | Total | Primary Resource |
| --- | --- | --- | --- | --- | --- | --- | --- |
| **Warrior** | 4 | 3 | 2 | 2 | 4 | 15 | Stamina |
| **Adept** | 3 | 3 | 3 | 2 | 3 | 14 | Stamina |
| **Skirmisher** | 3 | 4 | 3 | 2 | 3 | 15 | Stamina |
| **Mystic** | 2 | 3 | 4 | 4 | 2 | 15 | Aether Pool |

### Derived Resource Pool Formulas

```
Maximum HP = 50 + (STURDINESS × 10) + (Milestones × 10)
  Warrior: 50 + (4 × 10) = 90 HP (highest)
  Mystic:  50 + (2 × 10) = 70 HP (lowest)

Maximum Stamina = 20 + (MIGHT + FINESSE) × 5 + (Milestones × 5)
  Warrior:    20 + (4 + 3) × 5 = 55 Stamina
  Skirmisher: 20 + (3 + 4) × 5 = 55 Stamina
  Mystic:     20 + (2 + 3) × 5 = 45 Stamina (lowest)

Maximum Aether Pool (Mystic only) = 20 + (WILL + WITS) × 5
  Mystic: 20 + (4 + 4) × 5 = 60 AP (exclusive resource)
```

---

## III. Archetype Starting Abilities

### Warrior (3 Starting Abilities)

| Ability | Type | Cost | Effect |
| --- | --- | --- | --- |
| **Strike** | Active | 10 Stamina | 2d6+MIGHT damage, standard melee attack |
| **Defensive Stance** | Active | 15 Stamina | +3 Soak, -25% damage dealt, 2 turns |
| **Warrior's Vigor** | Passive | — | +10% Maximum HP |

### Adept (3 Starting Abilities)

| Ability | Type | Cost | Effect |
| --- | --- | --- | --- |
| **Exploit Weakness** | Active | 5 Stamina | Analyze enemy, +2 bonus dice to next attack |
| **Scavenge** | Active | 10 Stamina | Search area for consumable resources |
| **Resourceful** | Passive | — | +20% effectiveness of consumables |

### Skirmisher (3 Starting Abilities)

| Ability | Type | Cost | Effect |
| --- | --- | --- | --- |
| **Quick Strike** | Active | 8 Stamina | 2d6+FINESSE damage, fast attack |
| **Evasive Stance** | Active | 12 Stamina | +3 Evasion, -10% damage dealt, 2 turns |
| **Fleet Footed** | Passive | — | +1 Movement range, +1 Initiative |

### Mystic (3 Starting Abilities)

| Ability | Type | Cost | Effect |
| --- | --- | --- | --- |
| **Aether Dart** | Active | 5 AP | 2d6+WILL Aetheric damage, ranged |
| **Focus Aether** | Active | 10 Stamina | Restore 15 AP, channeling action |
| **Aetheric Attunement** | Passive | — | +10 Maximum AP, +1 AP regen/turn |

---

## IV. Specialization System

### Unlock Requirements

```
CanUnlock = (PP >= 3)
            AND (Legend >= MinLegend)
            AND (Corruption >= MinCorruption)
            AND (Corruption <= MaxCorruption)
            AND (RequiredQuestCompleted OR RequiredQuest == null)
```

**Standard Unlock Cost:** 3 PP

**Unlock Reward:** 3 Tier 1 abilities granted FREE (no PP cost)

### Specialization-Archetype Mapping

**WARRIOR (ArchetypeID = 1):**

- Berserkr (Fury resource, burst DPS)
- Iron-Bane (Righteous Fervor, anti-mechanical)
- Skjaldmaer (Shield tank, defensive)
- Skar-Horde Aspirant (Savagery, berserker)
- Atgeir-wielder (Reach weapon specialist)
- GorgeMawAscetic (Heretical, Corruption-focused)

**ADEPT (ArchetypeID = 2):**

- Bone-Setter (Healer/Support, field medicine)
- Jötun-Reader (Utility/Analyst, system diagnostician)
- Skald (Bard/Buffer, morale and inspiration)
- Scrap-Tinker (Crafting specialist, gadgeteer)
- Einbui (Lone survivor, self-sufficient)

**SKIRMISHER (ArchetypeID = 4):**

- Veiðimaðr (Hunter, tracking and precision)
- Myrk-gengr (Shadow-Walker, stealth and ambush)
- Strandhogg (Glitch-Raider, Jötun system exploiter)
- Hlekkr-master (Chain-Master, crowd control)

**MYSTIC (ArchetypeID = 5):**

- Seidkona (Seer, divination and Aetheric support)
- EchoCaller (Sound manipulation, debuffs and control)

---

## V. Ability Tree Structure (Per Specialization)

### Tier Distribution

| Tier | Abilities | PP Cost Each | PP Required in Tree | Starting Rank |
| --- | --- | --- | --- | --- |
| **Tier 1** | 3 | 0 (free with unlock) | 0 | Rank 1 |
| **Tier 2** | 3 | 4 PP each | 8 PP in tree | Rank 2 |
| **Tier 3** | 2 | 5 PP each | 16 PP in tree | Rank 3 |
| **Capstone** | 1 | 6 PP | 24 PP + both Tier 3 | Rank 3 |

**Total PP for Complete Tree:** 28 PP (3 unlock + 0+12+10+6 learning)

### PP-in-Tree Calculation

```
PP_In_Tree = SUM(All_Abilities_Learned_In_Specialization.PPCost)

Example Progression:
  Unlock Berserkr: 3 PP (not counted in tree) → PPInTree = 0
  Learn Tier 1 #1: 0 PP → PPInTree = 0 (free!)
  Learn Tier 1 #2: 0 PP → PPInTree = 0 (free!)
  Learn Tier 1 #3: 0 PP → PPInTree = 0 (free!)
  Learn Tier 2 #1: 4 PP → PPInTree = 4
  Learn Tier 2 #2: 4 PP → PPInTree = 8 → [RANK 2 TRIGGER]
  Learn Tier 2 #3: 4 PP → PPInTree = 12
  ...
  Learn Capstone: 6 PP → PPInTree = 28 → [RANK 3 TRIGGER]
```

---

## VI. Database Schema

### Specialization Table

```sql
CREATE TABLE Specialization (
  SpecializationID INT PRIMARY KEY,
  Name TEXT NOT NULL,
  ArchetypeID INT NOT NULL,  -- 1=Warrior, 2=Adept, 4=Skirmisher, 5=Mystic
  PathType TEXT NOT NULL,     -- "Coherent" or "Heretical"
  MechanicalRole TEXT,
  PrimaryAttribute TEXT,
  ResourceSystem TEXT DEFAULT 'Stamina',
  TraumaRisk TEXT DEFAULT 'Low',
  PPCostToUnlock INT DEFAULT 3,
  UnlockRequirements_MinLegend INT DEFAULT 0,
  UnlockRequirements_MinCorruption INT DEFAULT 0,
  UnlockRequirements_MaxCorruption INT DEFAULT 100
);
```

### CharacterSpecialization Tracking

```sql
CREATE TABLE CharacterSpecialization (
  CharacterID INT NOT NULL,
  SpecializationID INT NOT NULL,
  UnlockedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
  PPSpentInTree INT DEFAULT 0,
  PRIMARY KEY (CharacterID, SpecializationID)
);
```

---

## VII. Service Architecture

### SpecializationService Methods

```csharp
public interface ISpecializationService
{
    // Browsing
    List<SpecializationData> GetAvailableSpecializations(int archetypeID);
    bool CanUnlockSpecialization(PlayerCharacter character, int specializationID);
    
    // Unlocking
    ServiceResult UnlockSpecialization(PlayerCharacter character, int specializationID);
    
    // Validation
    ValidationResult ValidateSpecialization(int specializationID);
    bool IsArchetypeMatch(PlayerCharacter character, SpecializationData spec);
}
```

### Unlock Flow

1. Validate archetype match (Warrior can't unlock Mystic specs)
2. Check unlock requirements (Legend, Corruption, Quest)
3. Check PP availability (≥ 3)
4. Deduct PP via `SagaService.SpendPP()`
5. Query Tier 1 abilities from `AbilityRepository`
6. Insert `CharacterSpecialization` record
7. Add 3 Tier 1 abilities to `PlayerCharacter.Abilities`
8. Trigger `OnSpecializationUnlocked` event

---

## VIII. Cross-Archetype Validation

```csharp
function CanUnlockSpecialization(character, specialization):
  if character.Archetype.ArchetypeID != specialization.ArchetypeID:
    return Error("This specialization is for {ArchetypeName}, you are {YourArchetype}")
  // ... proceed with other unlock checks
```

**Critical Rule:** Cross-archetype unlocking is ALWAYS blocked. A Warrior cannot unlock Mystic specializations.

---

## IX. Integration Points

### Consumes From:

- **Saga System:** PP economy, Milestone rewards
- **Dice Pool System:** Attribute-based rolls
- **Ability System:** Specialization ability definitions

### Consumed By:

- **Combat Resolution:** Archetype abilities affect combat
- **Ability Rank System:** Specializations provide ability trees
- **Equipment System:** Archetype equipment preferences

---

*Consolidated from SPEC-PROGRESSION-002: Archetype & Specialization System Specification*