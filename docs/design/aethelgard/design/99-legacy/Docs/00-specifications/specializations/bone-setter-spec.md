# Bone-Setter Specialization - Complete Specification

> **Specification ID**: SPEC-SPECIALIZATION-BONE-SETTER
> **Version**: 1.0
> **Last Updated**: 2025-11-27
> **Status**: Draft - Implementation Review

---

## Document Control

### Purpose
This document provides the complete specification for the Bone-Setter specialization.

### Related Files
| Component | File Path | Status |
|-----------|-----------|--------|
| Factory Implementation | `RuneAndRust.Engine/SpecializationFactory.cs` | Implemented (Legacy) |
| Data Seeding | `RuneAndRust.Persistence/BoneSetterSeeder.cs` | **Not Yet Migrated** |

### Change Log
| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-11-27 | Initial specification from SpecializationFactory |

---

## 1. Specialization Overview

### 1.1 Identity

| Property | Value |
|----------|-------|
| **Internal Name** | BoneSetter |
| **Display Name** | Bone-Setter |
| **Specialization ID** | **TBD** (not yet assigned) |
| **Archetype** | Adept (ArchetypeID = 3) |
| **Path Type** | Coherent |
| **Mechanical Role** | Healer / Sanity Anchor |
| **Primary Attribute** | WITS |
| **Secondary Attribute** | FINESSE |
| **Resource System** | Stamina |
| **Trauma Risk** | None |
| **Icon** | :adhesive_bandage: |

### 1.2 Unlock Requirements

| Requirement | Value | Notes |
|-------------|-------|-------|
| **PP Cost to Unlock** | 3 PP | Standard cost |
| **Minimum Legend** | 5 | Mid-game specialization |

### 1.3 Design Philosophy

**Tagline**: "Pragmatic Restoration"

**Core Fantasy**: You are the non-magical combat medic who fights entropy with practiced skill and steady hands. While others rely on Aether or corruption, you trust in anatomy, antiseptics, and sheer determination. Your kit is always ready, your hands always steady. You keep the band alive when the world wants them dead.

**Mechanical Identity**:
1. **HP and Stress Healing**: Specializes in restoring both physical health and mental stability
2. **Consumable Crafting**: Creates healing items between combats
3. **Debuff Cleansing**: Removes poisons, diseases, and mental effects
4. **Risk Enabler**: Allows party to pursue high-risk strategies with healing backup

### 1.4 Starting Equipment

Upon unlocking Bone-Setter, the character receives:
- **3x Healing Poultice** (Standard quality, restores 15 HP)
- **8x Common Herb** (crafting component)
- **4x Clean Cloth** (crafting component)
- **2x Antiseptic** (crafting component)
- **2x Suture** (crafting component)

---

## 2. Ability Summary Table

| ID | Ability Name | Tier | Type | Ranks | Key Effect |
|----|--------------|------|------|-------|------------|
| TBD | Field Medic | 1 | Passive | 1→2→3 | +dice to medical checks, crafting bonus |
| TBD | Mend Wound | 1 | Active | 1→2→3 | Heal ally HP |
| TBD | Apply Tourniquet | 1 | Active | 1→2→3 | Stop [Bleeding], prevent bleedout |
| TBD | Anatomical Insight | 2 | Active | 1→2→3 | Apply [Vulnerable] to enemy |
| TBD | Administer Antidote | 2 | Active | 1→2→3 | Remove poison/disease |
| TBD | Triage | 2 | Passive | 1→2→3 | Passive healing bonus to critical allies |
| TBD | Cognitive Realignment | 3 | Active | — | Remove mental debuffs + restore Stress |
| TBD | Defensive Focus | 3 | Passive | — | Defense bonus after healing |
| TBD | Miracle Worker | 4 | Active | — | Massive heal + death protection |

---

## 3. Tier 1 Abilities

### 3.1 Field Medic (ID: TBD)

**Type**: Passive | **Target**: Self

#### Description
You are an expert at preparing medical supplies. Your kit is always ready, your hands always steady.

#### Rank Details

**Rank 1**: +1d10 to WITS checks for medical procedures, crafting consumables, and identifying ailments. Crafted healing items gain +5 HP restored.

**Rank 2**: +2d10 bonus. Crafted items gain +10 HP restored. Can craft consumables 20% faster.

**Rank 3**: +3d10 bonus. Crafted items gain +15 HP restored. 10% chance to not consume materials when crafting.

---

### 3.2 Mend Wound (ID: TBD)

**Type**: Active | **Cost**: 35 Stamina | **Target**: Single Ally

#### Description
You quickly dress the wound, applying poultice with practiced efficiency. The healing begins.

#### Rank Details

**Rank 1**: Heal target for 3d6 HP. Consumes 1 Healing Poultice. Auto-success.

**Rank 2**: Heal for 4d6 HP. Cost reduced to 30 Stamina.

**Rank 3**: Heal for 5d6 HP. If target is below 25% HP, heal for additional 2d6 HP.

---

### 3.3 Apply Tourniquet (ID: TBD)

**Type**: Active | **Cost**: 30 Stamina | **Cooldown**: 2 turns | **Target**: Single Ally

#### Description
With speed and precision, you stop the life-threatening blood loss. They'll live.

#### Rank Details

**Rank 1**: Remove [Bleeding] status from target. Target cannot gain [Bleeding] for 2 turns.

**Rank 2**: Also restore 1d6 HP when removing [Bleeding]. Protection lasts 3 turns.

**Rank 3**: If target would be reduced to 0 HP by [Bleeding] damage, prevent death and leave at 1 HP (once per combat).

---

## 4. Tier 2 Abilities

### 4.1 Anatomical Insight (ID: TBD)

**Type**: Active | **Cost**: 40 Stamina | **Cooldown**: 3 turns | **Target**: Single Enemy

#### Description
You observe their anatomy and recognize the weak points. There—that's where to strike.

#### Rank Details

**Rank 2**: Make WITS check vs enemy. On success, apply [Vulnerable] for 2 turns. [Vulnerable]: +2 damage from all sources.

**Rank 3**: [Vulnerable] lasts 3 turns. Allies gain +1d10 to attack rolls vs [Vulnerable] targets.

---

### 4.2 Administer Antidote (ID: TBD)

**Type**: Active | **Cost**: 30 Stamina | **Cooldown**: 2 turns | **Target**: Single Ally

#### Description
You administer the carefully prepared antidote. The toxins are neutralized.

#### Rank Details

**Rank 2**: Remove [Poisoned] or [Diseased] status from target. Consumes 1 Antidote. Auto-success.

**Rank 3**: Also grants immunity to poison/disease for 2 turns. If no debuff present, restore 2d6 HP instead.

---

### 4.3 Triage (ID: TBD)

**Type**: Passive | **Target**: Self

#### Description
You understand battlefield medicine: treat the most grievous wounds first. Maximum efficiency.

#### Rank Details

**Rank 2**: Allies below 25% HP receive +25% healing from your abilities.

**Rank 3**: Allies below 25% HP receive +50% healing. You automatically stabilize dying allies within your row.

---

## 5. Tier 3 Abilities (No Ranks)

### 5.1 Cognitive Realignment (ID: TBD)

**Type**: Active | **Cost**: 45 Stamina | **Cooldown**: 4 turns | **Target**: Single Ally

#### Description
Calming techniques, pressure points, smelling salts—you reboot their panicked mind.

#### Mechanical Effect
- Remove all mental debuffs ([Feared], [Disoriented], [Stunned], [Confused]) from target
- Restore 15 Psychic Stress
- Consumes 1 Calming Draught
- Auto-success

---

### 5.2 Defensive Focus (ID: TBD)

**Type**: Passive | **Target**: Self

#### Mechanical Effect
- After using any healing ability, gain [Focused Defense] for 1 turn
- [Focused Defense]: +2 Soak, +2d10 to Resolve checks
- Does not stack; refreshes duration

---

## 6. Capstone Ability

### 6.1 Miracle Worker (ID: TBD)

**Type**: Active | **Cost**: 50 Stamina | **Once Per Expedition**

#### Description
A complex procedure—stimulants, field surgery, sheer will. You bring them back from the brink.

#### Mechanical Effect
- Heal target for 8d6 HP
- Remove ALL debuffs (physical and mental)
- Target gains [Death Ward] for 3 turns
- [Death Ward]: If reduced to 0 HP, instead set to 1 HP and remove Death Ward
- Consumes 1 Miracle Tincture (rare crafted item)
- Once per expedition

#### GUI Display - CAPSTONE

```
┌─────────────────────────────────────────────┐
│           MIRACLE WORKER!                   │
├─────────────────────────────────────────────┤
│                                             │
│  Through skill and will, you deny death!    │
│                                             │
│  • Heal for 8d6 HP                          │
│  • Remove ALL debuffs                       │
│  • Grant [Death Ward] (3 turns)             │
│                                             │
│  Cost: 50 Stamina + Miracle Tincture        │
│                                             │
│  "Not today. Not on my watch."              │
│                                             │
└─────────────────────────────────────────────┘
```

---

## 7. Status Effect Definitions

### 7.1 [Vulnerable]

| Property | Value |
|----------|-------|
| **Applied By** | Anatomical Insight |
| **Duration** | 2-3 turns |
| **Effect** | Target takes +2 damage from all sources |

### 7.2 [Death Ward]

| Property | Value |
|----------|-------|
| **Applied By** | Miracle Worker |
| **Duration** | 3 turns |
| **Effect** | If reduced to 0 HP, set to 1 HP instead (consumed on trigger) |

### 7.3 [Focused Defense]

| Property | Value |
|----------|-------|
| **Applied By** | Defensive Focus (passive trigger) |
| **Duration** | 1 turn |
| **Effect** | +2 Soak, +2d10 to Resolve checks |

---

## 8. Consumable Crafting

### 8.1 Craftable Items

| Item | Components | Effect | Craft DC |
|------|------------|--------|----------|
| Healing Poultice | 2 Common Herb + 1 Clean Cloth | Restore 15 HP | DC 10 |
| Antidote | 1 Common Herb + 1 Antiseptic | Remove poison/disease | DC 12 |
| Calming Draught | 2 Common Herb + 1 Antiseptic | Used by Cognitive Realignment | DC 14 |
| Miracle Tincture | 4 Common Herb + 2 Antiseptic + 2 Suture | Used by Miracle Worker | DC 18 |

---

## 9. GUI Requirements

### 9.1 Healing Display

```
┌─────────────────────────────────────────┐
│  MEND WOUND                             │
│  Target: [Ally Name]                    │
│  Healing: 4d6 (+50% Triage bonus)       │
│  Poultice: 2 remaining                  │
└─────────────────────────────────────────┘
```

### 9.2 Inventory Tracking

- Show consumable counts prominently
- Warn when supplies are low
- Track crafting component inventory

---

## 10. Current Implementation Status

### 10.1 What's Implemented

| Component | Status | Notes |
|-----------|--------|-------|
| Ability definitions | ✅ Done | In SpecializationFactory.cs |
| Starting items | ✅ Done | Granted on specialization unlock |
| Basic ability logic | ⚠️ Partial | Some abilities reference CombatEngine |
| Consumable crafting | ⚠️ Partial | Basic framework exists |

### 10.2 Known Gaps

1. No dedicated seeder file (uses legacy factory pattern)
2. No SpecializationID or AbilityID assignments
3. Rank progression uses PP cost (CostToRank2 = 20) instead of tree-based
4. No GUI integration for rank indicators
5. Some abilities marked "handled in CombatEngine" may not be fully implemented

---

## 11. Planned Enhancements

### 11.1 Migration to Seeder Pattern

**Priority**: High

Create `RuneAndRust.Persistence/BoneSetterSeeder.cs` with:
- SpecializationID assignment (suggest: 23001)
- AbilityID assignments (suggest: 23010-23018)
- `SpecializationData` object with complete metadata
- `AbilityData` objects for each ability with tier/prerequisite info

### 11.2 Tree-Based Rank Progression

**Priority**: High

Migrate from PP-based ranking to tree-based:
- **Tier 1 abilities**: Rank 1 (when learned) → Rank 2 (when 2 Tier 2 trained) → Rank 3 (when Capstone trained)
- **Tier 2 abilities**: Rank 2 (when learned) → Rank 3 (when Capstone trained)
- **Tier 3 abilities**: No ranks
- **Capstone**: No ranks

### 11.3 GUI Rank Indicators

**Priority**: Medium

Implement visual rank indicators:
- **Bronze** (#CD7F32): Rank 1
- **Silver** (#C0C0C0): Rank 2
- **Gold** (#FFD700): Rank 3

### 11.4 Ability Refinements

**Priority**: Medium

| Ability | Enhancement |
|---------|-------------|
| Field Medic | Add crafting speed bonus display |
| Mend Wound | Visual feedback for critical heal bonus |
| Triage | Auto-stabilize indicator on dying allies |
| Miracle Worker | Once-per-expedition tracking in UI |

### 11.5 Consumable System Integration

**Priority**: Low

- Integrate with new inventory system
- Add crafting minigame/UI
- Track component sources (loot tables, merchants)

---

## 12. Implementation Priority

### Phase 1: Seeder Migration
1. **Create BoneSetterSeeder.cs** - dedicated seeder file
2. **Assign IDs** - SpecializationID and AbilityIDs
3. **Migrate rank system** - tree-based progression

### Phase 2: Combat Integration
4. **Implement [Vulnerable]** status fully
5. **Implement [Death Ward]** mechanic
6. **Implement Triage** auto-stabilize

### Phase 3: GUI
7. **Add rank indicators** (Bronze/Silver/Gold)
8. **Add consumable tracking** UI
9. **Add crafting interface**

---

**End of Specification**
