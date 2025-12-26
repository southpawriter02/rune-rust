# Vard-Warden Specialization - Complete Specification

Parent item: Specs: Specializations (Specs%20Specializations%202ba55eb312da8022a82bc3d0883e1d26.md)

> Specification ID: SPEC-SPECIALIZATION-VARD-WARDEN
Version: 1.0
Last Updated: 2025-11-27
Status: Draft - Implementation Review
> 

---

## Document Control

### Purpose

This document provides the complete specification for the Vard-Warden (Defensive Caster) specialization.

### Related Files

| Component | File Path | Status |
| --- | --- | --- |
| Factory Implementation | `RuneAndRust.Engine/SpecializationFactory.cs` | Implemented (Legacy) |
| Data Seeding | `RuneAndRust.Persistence/VardWardenSeeder.cs` | **Not Yet Migrated** |

### Change Log

| Version | Date | Changes |
| --- | --- | --- |
| 1.0 | 2025-11-27 | Initial specification from SpecializationFactory |

---

## 1. Specialization Overview

### 1.1 Identity

| Property | Value |
| --- | --- |
| **Internal Name** | VardWarden |
| **Display Name** | Vard-Warden |
| **Specialization ID** | **TBD** (not yet assigned) |
| **Archetype** | Mystic (ArchetypeID = 5) |
| **Path Type** | Coherent |
| **Mechanical Role** | Defensive Caster / Battlefield Controller |
| **Primary Attribute** | WILL |
| **Secondary Attribute** | WITS |
| **Resource System** | Aether Pool (AP) |
| **Trauma Risk** | None |
| **Icon** | :shield: |

### 1.2 Unlock Requirements

| Requirement | Value | Notes |
| --- | --- | --- |
| **PP Cost to Unlock** | 3 PP | Standard cost |
| **Minimum Legend** | 5 | Mid-game specialization |

### 1.3 Design Philosophy

**Tagline**: "Firewall Architect"

**Core Fantasy**: You are the firewall architect who creates pockets of stable reality in a corrupted world. Where chaos spreads, you inscribe barriers of solidified Aether. Where Blight advances, you consecrate ground. You don't just protect—you control the battlefield itself, forcing enemies to fight on your terms.

**Mechanical Identity**:

1. **Physical Barriers**: Create destructible walls that block movement and line-of-sight
2. **Zone Control**: Consecrate areas that heal allies and damage Blighted/Undying
3. **Ally Protection**: Buff allies with shields and stress resistance
4. **Reaction Defense**: Ultimate ability prevents fatal damage as a reaction

---

## 2. Ability Summary Table

| ID | Ability Name | Tier | Type | Ranks | Key Effect |
| --- | --- | --- | --- | --- | --- |
| TBD | Sanctified Resolve I | 1 | Passive | — | +1d WILL vs Push/Pull |
| TBD | Runic Barrier | 1 | Active | 1→2→3 | Create wall (30 HP, blocks movement) |
| TBD | Consecrate Ground | 1 | Active | 1→2→3 | Create healing/damage zone |
| TBD | Rune of Shielding | 2 | Active | 2→3 | Buff ally (+2 Soak, corruption resist) |
| TBD | Reinforce Ward | 2 | Active | 2→3 | Heal barrier or boost zone |
| TBD | Warden's Vigil | 2 | Passive | — | Row-wide Stress resistance |
| TBD | Glyph of Sanctuary | 3 | Active | — | Party temp HP + Stress immunity |
| TBD | Aegis of Sanctity | 3 | Passive | — | Barrier reflection + zone cleanse |
| TBD | Indomitable Bastion | 4 | Reaction | — | Negate fatal damage, create barrier |

---

## 3. Tier 1 Abilities

### 3.1 Sanctified Resolve I (ID: TBD)

**Type**: Passive | **Target**: Self

### Description

Your connection to stable Aether grounds you against displacement effects.

### Mechanical Effect

- +1d10 to WILL Resolve checks against [Push] and [Pull] effects
- Always active, no cost
- No ranks (single-tier passive)

---

### 3.2 Runic Barrier (ID: TBD)

**Type**: Active | **Cost**: 25 AP | **Cooldown**: 3 turns | **Target**: Row (Front/Back)

### Description

Create a physical wall of solidified Aether on target row. Barrier has 30 HP and blocks movement/line-of-sight for 2 turns.

### Rank Details

**Rank 1**: Create barrier with 30 HP. Lasts 2 turns. Blocks all movement and line-of-sight across row.

**Rank 2**: Barrier has 40 HP. Lasts 3 turns. Enemies attacking through barrier have -1d10 accuracy.

**Rank 3**: Barrier has 50 HP. Lasts 4 turns. When barrier is destroyed, deals 2d6 Arcane damage to adjacent enemies.

### Barrier Properties

| Property | Value |
| --- | --- |
| **HP** | 30/40/50 (by rank) |
| **Duration** | 2/3/4 turns (by rank) |
| **Size** | Full row width |
| **Blocks** | Movement, line-of-sight, projectiles |
| **Vulnerable To** | Physical and magical damage |

---

### 3.3 Consecrate Ground (ID: TBD)

**Type**: Active | **Cost**: 30 AP | **Cooldown**: 4 turns | **Target**: Row (Front/Back)

### Description

Target row becomes [Sanctified Ground] for 3 turns. Allies heal 1d6 HP at start of turn. [Blighted]/[Undying] enemies take 1d6 Arcane damage.

### Rank Details

**Rank 1**: Create [Sanctified Ground] for 3 turns. Allies heal 1d6 HP/turn. Blighted/Undying enemies take 1d6 Arcane damage/turn.

**Rank 2**: Duration 4 turns. Healing increased to 2d6. Damage increased to 2d6.

**Rank 3**: Duration 5 turns. Also grants allies +1d10 to Resolve checks while in zone.

---

## 4. Tier 2 Abilities

### 4.1 Rune of Shielding (ID: TBD)

**Type**: Active | **Cost**: 20 AP | **Cooldown**: 3 turns | **Target**: Single Ally

### Description

Inscribe protective rune on ally. Target gains +2 Soak and resistance to Corruption for 3 turns.

### Rank Details

**Rank 2**: Target gains +2 Soak and [Corruption Resistance] (halve corruption gained) for 3 turns.

**Rank 3**: +3 Soak. Duration 4 turns. Target also gains +1d10 to WILL Resolve checks.

---

### 4.2 Reinforce Ward (ID: TBD)

**Type**: Active | **Cost**: 15 AP | **Cooldown**: 2 turns | **Target**: Your Barrier or Zone

### Description

Target your Runic Barrier to heal it for 2d6 HP, OR boost Sanctified Ground to extend duration by 2 turns and increase healing to 2d6.

### Rank Details

**Rank 2**: Heal barrier for 2d6 HP, OR extend zone by 2 turns + boost healing to 2d6.

**Rank 3**: Heal barrier for 3d6 HP, OR extend zone by 3 turns + boost healing to 3d6. Can target any ally's defensive constructs.

---

### 4.3 Warden's Vigil (ID: TBD)

**Type**: Passive | **Target**: Row Allies

### Description

Allies in the same row as you gain +1d10 to Resolve checks against Stress effects. Your presence is calming.

### Mechanical Effect

- Passive aura effect
- Applies to all allies in your row
- +1d10 to Resolve checks vs Psychic Stress
- Always active while you remain in the row
- No ranks (single-tier passive)

---

## 5. Tier 3 Abilities (No Ranks)

### 5.1 Glyph of Sanctuary (ID: TBD)

**Type**: Active | **Cost**: 40 AP | **Cooldown**: 5 turns | **Target**: All Allies

### Description

Party-wide protection: All allies gain 2d6 temporary HP and immunity to Stress for 2 turns. Emergency protection.

### Mechanical Effect

- All party members gain 2d6 temporary HP
- All party members gain [Stress Immunity] for 2 turns
- [Stress Immunity]: Cannot gain Psychic Stress from any source
- Emergency ability for high-stress encounters

---

### 5.2 Aegis of Sanctity (ID: TBD)

**Type**: Passive | **Target**: Self (affects your constructs)

### Description

Your Runic Barriers reflect 25% damage back to attackers. Your Sanctified Ground zones cleanse 1 debuff per turn.

### Mechanical Effect

- **Barrier Enhancement**: When your Runic Barrier is attacked, attacker takes 25% of damage dealt as Arcane damage
- **Zone Enhancement**: At the start of each ally's turn in your Sanctified Ground, remove 1 random debuff
- Passive, always active

---

## 6. Capstone Ability

### 6.1 Indomitable Bastion (ID: TBD)

**Type**: Reaction | **Cost**: Free | **Once Per Expedition**

### Description

When you or an ally would take fatal damage, negate it and create emergency Runic Barrier on their row (40 HP, 3 turns).

### Mechanical Effect

- **Trigger**: You or an ally would be reduced to 0 HP
- **Effect**: Negate the damage entirely, set HP to 1
- **Bonus**: Create emergency Runic Barrier on target's row (40 HP, 3 turns)
- **Limit**: Once per expedition (resets on return to settlement)
- **Cost**: Free action (reaction)

### GUI Display - CAPSTONE

```
┌─────────────────────────────────────────────┐
│        INDOMITABLE BASTION!                 │
├─────────────────────────────────────────────┤
│                                             │
│  The ward holds! Death is denied!           │
│                                             │
│  REACTION: When ally would die:             │
│  • Negate fatal damage (set to 1 HP)        │
│  • Create 40 HP barrier on their row        │
│  • Barrier lasts 3 turns                    │
│                                             │
│  Once per expedition                        │
│                                             │
│  "Not while I stand."                       │
│                                             │
└─────────────────────────────────────────────┘

```

---

## 7. Status Effect Definitions

### 7.1 [Sanctified Ground]

| Property | Value |
| --- | --- |
| **Applied By** | Consecrate Ground |
| **Duration** | 3-5 turns |
| **Effect** | Allies heal 1d6-2d6/turn; Blighted/Undying take 1d6-2d6 Arcane/turn |
| **Visual** | Glowing runes on ground, pale blue light |

### 7.2 [Corruption Resistance]

| Property | Value |
| --- | --- |
| **Applied By** | Rune of Shielding |
| **Duration** | 3-4 turns |
| **Effect** | Corruption gained is halved |

### 7.3 [Stress Immunity]

| Property | Value |
| --- | --- |
| **Applied By** | Glyph of Sanctuary |
| **Duration** | 2 turns |
| **Effect** | Cannot gain Psychic Stress from any source |

---

## 8. GUI Requirements

### 8.1 Barrier HP Display

```
┌─────────────────────────────────────────┐
│  RUNIC BARRIER [████████░░] 32/40 HP    │
│  Duration: 2 turns remaining            │
│  🛡️ Blocking Front Row                  │
└─────────────────────────────────────────┘

```

- Color: Ethereal blue
- Pulses when taking damage
- Shows remaining duration

### 8.2 Zone Indicator

```
┌─────────────────────────────────────────┐
│  SANCTIFIED GROUND - Back Row           │
│  Healing: 2d6/turn | Damage: 2d6/turn   │
│  Duration: 3 turns | Cleansing: Active  │
└─────────────────────────────────────────┘

```

- Shows zone effects
- Highlights affected tiles on battlefield
- Indicates Aegis of Sanctity bonus if active

### 8.3 Indomitable Bastion Availability

```
┌─────────────────────────────────────────┐
│  ⭐ INDOMITABLE BASTION: READY          │
│  [Reaction: Negate fatal damage]        │
└─────────────────────────────────────────┘

```

- Prominent indicator when available
- Grayed out after use
- Resets indicator on expedition end

---

## 9. Current Implementation Status

### 9.1 What's Implemented

| Component | Status | Notes |
| --- | --- | --- |
| Ability definitions | ✅ Done | In SpecializationFactory.cs |
| Basic ability logic | ⚠️ Partial | Some abilities reference CombatEngine |
| Barrier mechanics | ⚠️ Partial | HP tracking exists, LoS blocking needs work |
| Zone mechanics | ⚠️ Partial | Basic framework exists |

### 9.2 Known Gaps

1. No dedicated seeder file (uses legacy factory pattern)
2. No SpecializationID or AbilityID assignments
3. Some passives have MaxRank = 1 (correct), others have MaxRank = 3 with PP costs
4. Reaction trigger for Indomitable Bastion not fully implemented
5. Barrier and Zone visual indicators not in GUI
6. Reflection damage (Aegis of Sanctity) not implemented

---

## 10. Planned Enhancements

### 10.1 Migration to Seeder Pattern

**Priority**: High

Create `RuneAndRust.Persistence/VardWardenSeeder.cs` with:

- SpecializationID assignment (suggest: 28001)
- AbilityID assignments (suggest: 28010-28018)
- `SpecializationData` object with complete metadata
- `AbilityData` objects for each ability with tier/prerequisite info

### 10.2 Tree-Based Rank Progression

**Priority**: High

Migrate from mixed ranking to tree-based:

- **Tier 1 abilities**:
    - Sanctified Resolve I: No ranks (passive - correct)
    - Runic Barrier: Rank 1 → Rank 2 (when 2 Tier 2 trained) → Rank 3 (when Capstone trained)
    - Consecrate Ground: Rank 1 → Rank 2 (when 2 Tier 2 trained) → Rank 3 (when Capstone trained)
- **Tier 2 abilities**:
    - Rune of Shielding: Rank 2 → Rank 3 (when Capstone trained)
    - Reinforce Ward: Rank 2 → Rank 3 (when Capstone trained)
    - Warden's Vigil: No ranks (passive - correct)
- **Tier 3 abilities**: No ranks (correct)
- **Capstone**: No ranks (correct)

### 10.3 GUI Rank Indicators

**Priority**: Medium

Implement visual rank indicators for ranked abilities:

- **Bronze** (#CD7F32): Rank 1
- **Silver** (#C0C0C0): Rank 2
- **Gold** (#FFD700): Rank 3

### 10.4 Barrier and Zone Enhancements

**Priority**: Medium

| Feature | Enhancement |
| --- | --- |
| Barrier LoS | Properly block line-of-sight for ranged attacks |
| Barrier Destruction | Visual effect + AoE damage at Rank 3 |
| Zone Overlap | Define behavior when multiple zones overlap |
| Aegis Reflection | Implement 25% damage reflection |

### 10.5 Reaction System

**Priority**: High

Implement proper reaction system for Indomitable Bastion:

- Hook into damage calculation before HP reduction
- Trigger UI prompt when fatal damage would occur
- Allow player to choose whether to use (if multiple reactions available)

---

## 11. Implementation Priority

### Phase 1: Seeder Migration

1. **Create VardWardenSeeder.cs** - dedicated seeder file
2. **Assign IDs** - SpecializationID and AbilityIDs
3. **Migrate rank system** - tree-based progression

### Phase 2: Core Mechanics

1. **Implement Barrier properly** - HP, LoS blocking, destruction effects
2. **Implement Zones properly** - area effects, duration, stacking rules
3. **Implement Reaction trigger** - Indomitable Bastion interrupt

### Phase 3: Passive Enhancements

1. **Implement Aegis reflection** - damage return on barrier attack
2. **Implement zone cleansing** - debuff removal per turn
3. **Add rank indicators** (Bronze/Silver/Gold)

### Phase 4: GUI

1. **Add barrier HP display** with visual feedback
2. **Add zone indicators** on battlefield
3. **Add capstone availability tracker**

---

**End of Specification**