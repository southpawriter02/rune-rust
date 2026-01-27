# Boss Encounter System — Mechanic Specification v5.0

Type: Mechanic
Description: Multi-phase boss combat mechanics including HP-based phase transitions, telegraphed abilities with interrupt mechanics, vulnerability windows, enrage systems, and boss-specific loot tables.
Priority: Must-Have
Status: Review
Target Version: Alpha
Dependencies: Encounter System, Combat Resolution System, Enemy AI Behavior System, Damage Calculation, Loot System
Implementation Difficulty: Very Complex
Balance Validated: No
Document ID: AAM-SPEC-MECH-BOSSENCOUNTER-v5.0
Parent item: Encounter System — Core System Specification v5.0 (Encounter%20System%20%E2%80%94%20Core%20System%20Specification%20v5%200%20c82c42d7129c4843a86f2e69cd72f0d7.md)
Proof-of-Concept Flag: No
Related Projects: (PROJECT) Game Specification Consolidation & Standardization (https://www.notion.so/PROJECT-Game-Specification-Consolidation-Standardization-e1d0c8b2ea2042f9b9c08471c6077c92?pvs=21)
Session Handoffs: Consolidation Work — Phase 1A Core Systems Audit Complete (https://www.notion.so/Consolidation-Work-Phase-1A-Core-Systems-Audit-Complete-0c51f0058f3a478fb7bc6a2c192cac2a?pvs=21)
Sub-Type: Combat
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: High
Voice Layer: Layer 3 (Technical)
Voice Validated: No

## **I. Core Philosophy: Epic Confrontation**

The Boss Encounter System defines **multi-phase boss combat mechanics** for climactic encounters. Boss fights crescendo through distinct phases with telegraphed abilities, enrage systems, and proportional rewards.

**Design Pillars:**

- **Escalating Difficulty:** HP-based phase transitions (75%/50%/25%) with progressive stat scaling
- **Readable Telegraphs:** Charged attacks with interrupt windows create skill-based counterplay
- **Time Pressure:** Enrage mechanics prevent stalling without feeling artificial
- **Meaningful Rewards:** Boss loot quality matches difficulty (artifacts, guaranteed Clan-Forged+)

---

## **II. Boss Encounter Lifecycle**

```
1. COMBAT INITIALIZATION
   └─ Boss encounter initialized, Phase 1 activated, abilities seeded

2. PHASE 1 COMBAT LOOP (100-75% HP)
   └─ Standard attacks, player learns patterns

3. PHASE TRANSITION → PHASE 2 (75% HP)
   ├─ Invulnerability (1-2 turns)
   ├─ Stat modifiers applied (+damage, +defense, +regen)
   ├─ Add wave spawned (optional)
   └─ New abilities unlocked

4. PHASE 2+ COMBAT LOOP (75-50%, 50-25%)
   └─ Enhanced abilities, enrage check at 20% HP

5. ENRAGE STATE (HP ≤ 20% or Turn ≥ 15)
   ├─ +30-50% damage
   ├─ +1 bonus action/turn
   └─ Control immunity

6. BOSS DEFEATED → LOOT GENERATION
   └─ Guaranteed quality drops, artifact roll, unique items
```

---

## **III. Phase Mechanics**

### **Phase Thresholds**

| Phase | HP Threshold | Typical Duration | Stat Modifiers |
| --- | --- | --- | --- |
| Phase 1 | 100-75% | 3-5 turns | 1.0x damage, +0 defense |
| Phase 2 | 75-50% | 4-6 turns | 1.2x damage, +2-3 defense, 3-4 HP/turn regen |
| Phase 3 | 50-0% | 5-8 turns | 1.5x damage, +3-5 defense, 5-8 HP/turn regen, +1 action |

### **Phase Transition Sequence**

1. Apply invulnerability (1-2 turns)
2. Update phase state + flags
3. Execute events (add waves, stat modifiers, ability unlocks)
4. Display cinematic log message

### **Add Wave Spawning**

| Boss | Phase | Add Wave | Purpose |
| --- | --- | --- | --- |
| Ruin-Warden | Phase 2 | 2× Corrupted Servitor | Distraction, target switching |
| Aetheric Aberration | Phase 2 | 1× Blight Drone | Ranged pressure |
| Forlorn Archivist | Phase 2 | 2× Corrupted Servitor | Meat shields, stress stacking |
| Omega Sentinel | None | (No adds) | Pure solo mechanical difficulty |

---

## **IV. Telegraphed Abilities**

### **Ability Types**

| Type | Charge Time | Cooldown | Interruptible | Vulnerability |
| --- | --- | --- | --- | --- |
| **Standard** | 0 turns | 0-2 turns | No | No |
| **Telegraphed** | 1-2 turns | 3-4 turns | Yes | No |
| **Ultimate** | 2 turns | 5-6 turns | No | Yes (2-3 turns) |
| **Passive** | N/A | N/A | N/A | No |

### **Interrupt Mechanics**

- **Threshold:** 10-25 damage (scales with boss tier)
- **Cumulative:** Multiple attacks accumulate toward threshold
- **Stagger:** Interrupted boss receives [Staggered] for 2 turns (-2 all attributes)

### **Vulnerability Windows**

After ultimate abilities, boss takes +25-50% damage for 2-3 turns.

| Ability | Boss | Duration | Damage Multiplier |
| --- | --- | --- | --- |
| Total System Failure | Ruin-Warden | 2 turns | 1.5x (+50%) |
| Aetheric Storm | Aetheric Aberration | 3 turns | 1.25x (+25%) |
| Psychic Storm | Forlorn Archivist | 2 turns | 1.5x (+50%) |
| Omega Protocol | Omega Sentinel | 3 turns | 1.25x (+25%) |

---

## **V. Enrage System**

### **Enrage Triggers**

| Boss | HP-Based | Turn-Based |
| --- | --- | --- |
| Ruin-Warden | 20% HP | None |
| Aetheric Aberration | 25% HP | None |
| Forlorn Archivist | 20% HP | None |
| Omega Sentinel | 20% HP | 20 turns |

### **Enrage Buffs**

| Boss | Damage Mult | Bonus Actions | Control Immunity | Special |
| --- | --- | --- | --- | --- |
| Ruin-Warden | 1.4x (+40%) | +1/turn | Yes | — |
| Aetheric Aberration | 1.5x (+50%) | +1/turn | Yes | — |
| Forlorn Archivist | 1.3x (+30%) | +1/turn | Yes | Stress aura doubled |
| Omega Sentinel | 1.5x (+50%) | +1/turn | Yes | Soak +2 |

---

## **VI. Boss Loot System**

### **Guaranteed Quality Drops**

| Boss | Min Quality | Optimized % | Artifact % | Currency |
| --- | --- | --- | --- | --- |
| Ruin-Warden | Clan-Forged | 30% | 10% | 300-800 Silver |
| Aetheric Aberration | Clan-Forged | 35% | 12% | 400-900 Silver |
| Forlorn Archivist | Clan-Forged | 30% | 15% | 350-850 Silver |
| Omega Sentinel | Optimized | 40% | 18% | 500-1000 Silver |

### **Artifact Properties**

- Unique effects not found on normal equipment
- Set bonuses (2-4 piece synergies)
- +2 to +4 attribute bonuses
- Once-per-character unique items tracked via database

---

## **VII. Implemented Bosses**

| Boss | HP | Damage | Defense | Soak | Phases | Enrage HP | Legend |
| --- | --- | --- | --- | --- | --- | --- | --- |
| **Ruin-Warden** | 80 | 2d6+2 | 4 | 3 | 3 | 20% | 100 |
| **Aetheric Aberration** | 90 | 3d6+2 | 3 | 2 | 3 | 25% | 100 |
| **Forlorn Archivist** | 85 | 3d6+1 | 3 | 2 | 3 | 20% | 150 |
| **Omega Sentinel** | 120 | 4d6+3 | 5 | 4 | 3 | 20% | 150 |

---

## **VIII. Balance Targets**

### **Time-to-Kill (Solo Player, Legend 5)**

| Boss Tier | HP | Expected TTK | With Regen | Acceptable Range |
| --- | --- | --- | --- | --- |
| Low-Tier | 60-80 | 8-10 turns | +2-3 turns | 6-13 turns |
| Mid-Tier | 80-100 | 12-14 turns | +3-4 turns | 9-17 turns |
| High-Tier | 100-120 | 14-16 turns | +4-5 turns | 11-19 turns |
| World Boss | 150-200 | 20-25 turns | +6-8 turns | 16-29 turns |

### **Design Constraints**

- **No one-shots:** Max single-hit ≤ 50% player HP
- **Enrage pressure:** 2-3 turn kill window when enraged
- **Healing viable:** Player healing (15-20 HP) mitigates 1-2 attacks

---

## **IX. Service Architecture**

```
RuneAndRust.Engine/
  ├─ BossEncounterService.cs      // Phase transitions, enrage, invulnerability
  ├─ BossCombatIntegration.cs     // Combat loop integration (7 hooks)
  ├─ TelegraphedAbilityService.cs // Telegraph mechanics, interrupts
  ├─ BossLootService.cs           // Loot generation, artifacts, sets
  └─ BossDatabase.cs              // Boss configurations

RuneAndRust.Core/
  ├─ BossPhaseDefinition.cs       // Phase data structure
  ├─ BossAbility.cs               // Ability data structure
  └─ BossLootTable.cs             // Loot configuration
```

### **Integration Points**

1. Combat initialization (`InitializeBossEncounters()`)
2. After player damage (`CheckTelegraphInterrupt()`)
3. Boss turn processing (`ShouldBossTelegraph()`, `ProcessBossAction()`)
4. End of turn (`ProcessEndOfTurn()`)
5. Loot generation (`GenerateBossLoot()`)
6. Combat cleanup (`ClearBossCombatState()`)

---

## **X. Dependencies**

**Depends On:**

- Combat Resolution System → Turn sequence, combat state
- Damage Calculation → Damage formulas, critical hits
- Enemy AI Behavior System → Boss AI decision-making
- Loot System → Quality tiers, drop rates

**Depended Upon By:**

- Encounter System → Composes boss encounters
- Enemy Design System → Boss archetype stats

---

*This specification follows the v5.0 Three-Tier Template standard. The Boss Encounter System orchestrates multi-phase epic boss fights with telegraphed abilities, enrage mechanics, and proportional rewards.*