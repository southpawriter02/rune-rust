# Enemy Design & Bestiary System — Mechanic Specification v5.0

Type: Mechanic
Description: Enemy design framework covering stat budget formulas for 5 threat tiers, 8 enemy archetypes with tactical roles, special mechanics (Soak, IsForlorn, IsBoss), trauma integration, and balance targets.
Priority: Must-Have
Status: Review
Target Version: Alpha
Dependencies: Encounter System, Combat Resolution System, Damage Calculation, Trauma Economy System, Loot System
Implementation Difficulty: Hard
Balance Validated: No
Document ID: AAM-SPEC-MECH-ENEMYDESIGN-v5.0
Parent item: Encounter System — Core System Specification v5.0 (Encounter%20System%20%E2%80%94%20Core%20System%20Specification%20v5%200%20c82c42d7129c4843a86f2e69cd72f0d7.md)
Proof-of-Concept Flag: No
Related Projects: (PROJECT) Game Specification Consolidation & Standardization (https://www.notion.so/PROJECT-Game-Specification-Consolidation-Standardization-e1d0c8b2ea2042f9b9c08471c6077c92?pvs=21)
Session Handoffs: Consolidation Work — Phase 1A Core Systems Audit Complete (https://www.notion.so/Consolidation-Work-Phase-1A-Core-Systems-Audit-Complete-0c51f0058f3a478fb7bc6a2c192cac2a?pvs=21)
Sub-Type: Combat
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: Medium
Voice Layer: Layer 4 (Design)
Voice Validated: No

## **I. Core Philosophy: Balanced Opposition**

The Enemy Design & Bestiary System provides the **design framework for creating balanced enemies** across all threat levels. This system formalizes stat budgets, archetypes, AI patterns, and trauma integration.

**Design Pillars:**

- **Readable Threat Levels:** HP/damage patterns telegraph danger (Low: 10-15 HP, Boss: 75-100 HP)
- **Tactical Diversity:** 8 archetypes force different tactical responses
- **Balanced Power Budget:** Stat caps prevent one-shots and bullet sponges
- **Horror Atmosphere:** IsForlorn enemies inflict psychological trauma

---

## **II. Threat Tier Classification**

| Tier | HP Range | Attr Budget | Damage Range | Legend Value | Examples |
| --- | --- | --- | --- | --- | --- |
| **Low** | 10-15 | 5-10 pts | 1d6 to 1d6+2 | 10-20 | Corrupted Servitor, Scrap-Hound |
| **Medium** | 25-50 | 8-16 pts | 1d6+1 to 2d6 | 15-50 | Blight-Drone, Forlorn Scholar |
| **High** | 60-70 | 12-17 pts | 2d6+2 to 3d6 | 55-75 | Bone-Keeper, Vault Custodian |
| **Lethal** | 80-90 | 13-20 pts | 3d6+4 to 4d6 | 60-100 | Failure Colossus, Sentinel Prime |
| **Boss** | 75-100 | 13-20 pts | 2d6 to 3d6+3 | 100-150 | Ruin-Warden, Omega Sentinel |

---

## **III. Enemy Archetypes**

### **1. Tank** (High HP, High Defense, Low Damage)

- **Role:** Absorb damage, protect allies
- **Stats:** STURDINESS 4-6, HP 60-100, Soak 4-6
- **AI:** 40-50% attack, 30-40% defend, 20-30% self-heal
- **Examples:** Vault Custodian (70 HP, Soak 4), Omega Sentinel (100 HP, Soak 6)

### **2. DPS** (Balanced Stats, Consistent Damage)

- **Role:** Reliable threat, straightforward combat
- **Stats:** Balanced attributes, HP 25-80, damage tier-appropriate
- **AI:** 70-80% attack, 20-30% defend
- **Examples:** Blight-Drone (25 HP, 1d6+1), War-Frame (50 HP, 2d6)

### **3. Glass Cannon** (Low HP, High Damage)

- **Role:** High-priority target, burst threat
- **Stats:** FINESSE 4-5, HP 10-20, high damage for tier
- **AI:** 80-90% attack, flee at low HP
- **Examples:** Scrap-Hound (10 HP), Test Subject (15 HP, 1d6+2)

### **4. Support** (Buff/Debuff Focused)

- **Role:** Enhance allies, disrupt players
- **Stats:** WITS 4-5+, HP tier-appropriate, moderate damage
- **AI:** 30-40% attack, 30-40% buff, 20-30% heal
- **Examples:** Corrupted Engineer (30 HP, buff allies)

### **5. Swarm** (Numbers-Based Threat)

- **Role:** Overwhelm through quantity
- **Stats:** Low individual HP, often -2 Defense
- **AI:** 70-80% attack, flanking focus
- **Examples:** Servitor Swarm (50 HP collective)

### **6. Caster** (Psychic/Magic Damage)

- **Role:** Ranged magic, ignore armor, inflict trauma
- **Stats:** WILL 4-7, WITS 4-5, psychic damage
- **AI:** 30-50% attack, 30-40% debuff, 20-30% summon
- **Examples:** Forlorn Scholar (30 HP, IsForlorn), Rust-Witch (70 HP)

### **7. Mini-Boss** (Phase Mechanics, Elite)

- **Role:** Mid-dungeon challenge, skill test
- **Stats:** 50-70 HP, phase-based AI, Soak 4-6
- **AI:** Phase-based (changes at 50% HP)
- **Examples:** Vault Custodian, Sentinel Prime

### **8. Boss** (Multi-Phase, Ultimate Abilities)

- **Role:** Climactic encounter
- **Stats:** 75-100 HP, 3-phase combat, special effects
- **AI:** Phase progression (testing → mechanics → ultimate)
- **Examples:** Ruin-Warden, Forlorn Archivist, Omega Sentinel

---

## **IV. Special Mechanics**

### **IsForlorn (Trauma Aura)**

Inflicts passive Psychic Stress/Corruption aura.

| Enemy | Tier | Stress/Turn | Corruption | Total Cost |
| --- | --- | --- | --- | --- |
| Shrieker | Medium | +3 | +2 | ~10-15 |
| Forlorn Scholar | Medium | +3 | +5 | ~15-20 |
| Bone-Keeper | High | +4 | +3 | ~15-25 |
| Rust-Witch | Lethal | +5 | +10 | ~25-40 |
| Forlorn Archivist | Boss | +8 | +12 | ~50-80 |

### **Soak (Flat Damage Reduction)**

| Enemy | Soak | Effective HP | v0.18 Adjustment |
| --- | --- | --- | --- |
| Vault Custodian | 4 | 112 (from 70) | Reduced 6→4 |
| Failure Colossus | 4 | 128 (from 80) | No change |
| Sentinel Prime | 6 | 180 (from 90) | Reduced 8→6 |
| Omega Sentinel | 6 | 160 (from 100) | Reduced 8→6 |

**Design Caps:** Non-boss max Soak 4, Boss max Soak 6

### **IsChampion (Elite Variants)**

- +50% HP, +1-2 attributes, +25% damage, +20% Legend
- Spawn rate: 10-15% Low/Medium, 20% High tier
- Unique ability from enemy pool

---

## **V. Balance Targets**

### **Time-to-Kill (Solo Player)**

| Tier | TTK Target | Player Death TTK | Design Intent |
| --- | --- | --- | --- |
| Low | 2-3 turns | 8-12 turns | Quick clears, fodder |
| Medium | 4-6 turns | 5-7 turns | Standard combat |
| High | 7-10 turns | 3-5 turns | Challenging, resources needed |
| Lethal | 10-15 turns | 2-3 turns | Boss-adjacent difficulty |
| Boss | 12-20 turns | 3-6 turns | Multi-phase, narrative |

### **v0.18 Balance Lessons**

1. **Damage Variance:** 5d6 (5-30) feels unfair → prefer 3d6+X for consistency
2. **Soak Creates Sponges:** Soak 8 + High HP = 25-turn TTK → cap at 6
3. **One-Shots Kill Fun:** Max single-hit ≤ 80% player HP
4. **Legend/HP Ratio:** Target 0.67-1.0 Legend per HP

---

## **VI. Loot Integration**

| Threat Tier | Primary Quality | Secondary Quality | No Drop |
| --- | --- | --- | --- |
| **Low** | Tier 0 (60%) | Tier 1 (30%) | 10% |
| **Medium** | Tier 1 (40%) | Tier 2 (40%), T3 (20%) | 0% |
| **High** | Tier 2 (40%) | Tier 3 (40%), T4 (20%) | 0% |
| **Lethal** | Tier 3 (50%) | Tier 4 (50%) | 0% |
| **Boss** | Tier 4 (70%) | Tier 3 (30%) | 0% |

---

## **VII. Trauma Economy Integration**

### **Stress Sources**

- **Encounter:** Forlorn +5, Boss +10
- **Proximity Aura:** +3/turn (Forlorn), +5/turn (Forlorn Boss)
- **Psychic Attacks:** +2 per hit
- **Traumatic Abilities:** +5 (PsychicScream, etc.)

### **WILL-Based Resistance**

```
Stress_Inflicted = Base_Stress - (Player_WILL × 0.5)
Corruption_Inflicted = Base_Corruption - (Player_WILL × 0.25)
```

---

## **VIII. Encounter Composition**

### **Encounter Threat Score (ETS)**

```
ETS = Σ(Enemy Legend Value × Count)

Difficulty Tiers:
- Easy: ETS 10-25
- Medium: ETS 25-50
- Hard: ETS 50-100
- Boss: ETS 100-200
```

### **Composition Patterns**

| Pattern | Composition | Difficulty | Design |
| --- | --- | --- | --- |
| Balanced | 2× Medium | Medium | AoE valuable |
| Tank + DPS | 1× High + 1× Medium | High | Kill DPS first |
| Swarm | 3× Low | Medium | Attrition threat |
| Caster + Shield | 1× Tank + 1× Caster | High | Protect priority |

---

## **IX. Implementation Reference**

### **Code Files**

```
RuneAndRust.Core/
  └─ Enemy.cs (29+ enemy types)

RuneAndRust.Engine/
  ├─ EnemyFactory.cs     // Enemy creation, hardcoded stats
  ├─ EnemyAI.cs          // AI decision-making
  └─ LootService.cs      // Loot generation

RuneAndRust.Core/Population/
  └─ DormantProcess.cs   // ThreatLevel enum
```

### **Design Templates**

1. Enemy Design Proposal → concept pitch
2. Enemy Design Worksheet → stat allocation
3. Design Review Checklist → balance validation
4. Encounter Composition Calculator → multi-enemy balancing
5. Enemy Bestiary Entry → documentation

---

## **X. Dependencies**

**Depends On:**

- Combat Resolution System → Turn sequence, initiative
- Damage Calculation → Damage formulas, mitigation
- Loot System → Quality tier mapping
- Trauma Economy → Stress/Corruption mechanics

**Depended Upon By:**

- Encounter System → Enemy composition
- Boss Encounter System → Boss archetype stats
- Enemy AI System → Archetype behavior patterns

---

*This specification follows the v5.0 Three-Tier Template standard. The Enemy Design & Bestiary System provides the framework for creating balanced, diverse enemies with tactical identity and thematic horror atmosphere.*