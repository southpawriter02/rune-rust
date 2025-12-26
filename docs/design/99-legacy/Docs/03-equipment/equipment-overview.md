# Equipment & Items System

**Version**: v0.17 (Based on v0.16 implementation)
**Status**: Core System (Tier 3)
**Dependencies**: Attributes, Combat Engine, Loot System
**Integration Points**: Character Stats, Damage Calculation, Equipment Service

---

## Table of Contents

1. [Functional Overview](#1-functional-overview)
2. [Quality Tiers](#2-quality-tiers)
3. [Weapons](#3-weapons)
4. [Armor](#4-armor)
5. [Balance Considerations](#5-balance-considerations)

---

## 1. Functional Overview

### 1.1 Purpose

**Equipment** provides the primary means of character power scaling beyond level progression:
- **Weapons**: Determine attack damage and attribute scaling
- **Armor**: Increase HP pool and defensive capabilities
- **Bonuses**: Grant attribute increases that can exceed the 6-attribute cap

### 1.2 Equipment Slots

**Active Slots** (v0.3):
- 1× Weapon slot
- 1× Armor slot
- **Total**: 2 equipped items

**Inventory** (v0.3):
- 5 item slots for unequipped gear
- Can swap equipment between encounters

---

## 2. Quality Tiers

### 2.1 Tier System

```
Jury-Rigged (0) → Scavenged (1) → Clan-Forged (2) → Optimized (3) → Myth-Forged (4)
```

### 2.2 Quality Tier Effects

| Tier | Name | Description | Power Level | Availability |
|------|------|-------------|-------------|--------------|
| **0** | Jury-Rigged | Scrap held together with hope | Starter gear | Common |
| **1** | Scavenged | Salvaged from ruins, functional | Early game | Common |
| **2** | Clan-Forged | Crafted by survivors, solid | Mid game | Uncommon |
| **3** | Optimized | Pre-Glitch tech, maintained | Late game | Rare |
| **4** | Myth-Forged | Legendary artifacts | End game | Very Rare |

### 2.3 Quality Scaling

**Weapon Quality Examples** (Axe progression):

| Quality | Name | Damage | Stamina | Bonuses | Total Power |
|---------|------|--------|---------|---------|-------------|
| 0 | Rusty Hatchet | 1d6 | 5 | None | 3.5 avg damage |
| 1 | Scavenged Axe | 1d6+1 | 5 | None | 4.5 avg |
| 2 | Clan-Forged Axe | 1d6+3 | 5 | None | 6.5 avg |
| 3 | Rune-Etched Axe | 2d6 | 5 | +1 MIGHT | 7 avg + attribute |
| 4 | Dvergr Maul | 2d6+4 | 5 | +2 MIGHT | 11 avg + attribute |

**Power Curve**: ~3× damage increase from Tier 0 → Tier 4

---

## 3. Weapons

### 3.1 Weapon Categories

| Category | Attribute | Damage Range | Class | Notes |
|----------|-----------|--------------|-------|-------|
| **Axe** | MIGHT | 1d6 to 2d6 | Warrior | Balanced melee |
| **Greatsword** | MIGHT | 1d6+2 to 4d6+4 | Warrior | Two-handed, highest damage |
| **Spear** | FINESSE | 1d6 to 2d6 | Scavenger | Reach weapon |
| **Dagger** | FINESSE | 1d6-1 to 2d6 | Scavenger | Fast, low damage **(v0.18: improved starter)** |
| **Staff** | WILL | 1d6-1 to 3d6+3 | Mystic | Aether-based **(v0.18: improved starter)** |
| **Focus** | WILL | 0d6 to 0d6 | Mystic | No melee damage, +WILL bonuses |

### 3.2 Weapon Properties

```csharp
public class Weapon : Equipment
{
    string WeaponAttribute;    // "MIGHT", "FINESSE", "WILL"
    int DamageDice;           // 1-4 dice
    int DamageBonus;          // -2 to +6
    int StaminaCost;          // 5-10 Stamina per attack
    int AccuracyBonus;        // -1 to +3 bonus dice
    List<EquipmentBonus> Bonuses;  // Attribute bonuses
    string SpecialEffect;     // Unique effects (Myth-Forged)
}
```

### 3.3 Weapon Examples by Tier

**Tier 0 (Jury-Rigged)**:
- Rusty Hatchet: 1d6, MIGHT
- Makeshift Spear: 1d6, FINESSE
- Crude Staff: **1d6-1**, WILL **(v0.18: improved from 1d6-2)**
- Sharpened Scrap: **1d6-1** **(v0.18: improved from 1d6-2)**

**Tier 2 (Clan-Forged)**:
- Clan-Forged Axe: 1d6+3, MIGHT
- Clan-Forged Greatsword: **1d6+5**, MIGHT **(v0.18: reduced from 1d6+6)**
- Hunter's Spear: 1d6+3, FINESSE
- Iron-Bound Staff: 1d6+1, WILL

**Tier 4 (Myth-Forged)**:
- Dvergr Maul: 2d6+4, MIGHT, +2 MIGHT bonus
- Jötun-Tech Plasma Cutter: 2d6, FINESSE, ignores armor
- Architect's Will: 3d6+3, WILL, +3 WILL bonus

---

## 4. Armor

### 4.1 Armor Categories

| Category | HP Bonus | Defense Bonus | Mobility | Class Synergy |
|----------|----------|---------------|----------|---------------|
| **Light** | +5 to +15 | 0 to +1 | High | Scavenger, Mystic |
| **Medium** | +10 to +20 | +1 to +2 | Balanced | All classes |
| **Heavy** | +15 to +30 | +2 to +3 | Low | Warrior |

### 4.2 Armor Properties

```csharp
public class Armor : Equipment
{
    int HPBonus;              // +5 to +30 MaxHP
    int DefenseBonus;         // 0 to +3 enemy attack penalty
    ArmorCategory Category;   // Light/Medium/Heavy
    List<EquipmentBonus> Bonuses;  // Attribute bonuses
}
```

### 4.3 Armor Trade-offs

**Light Armor**:
- **Pros**: Low weight, +FINESSE bonuses common
- **Cons**: Low HP/defense
- **Best for**: Glass cannons (Mystic, agile Scavenger)

**Heavy Armor**:
- **Pros**: High HP/defense, +STURDINESS bonuses
- **Cons**: **-FINESSE penalties** (Clan-Forged+ tiers) **(v0.18: balanced heavy armor)**
- **Best for**: Tanks (Warrior frontline)

### 4.4 Armor Examples by Tier

**Tier 0-1**:
- Tattered Leathers: +5 HP, 0 defense (Light)
- Scrap Plating: +10 HP, +1 defense (Medium)

**Tier 2**:
- Reinforced Leathers: +15 HP, +1 defense, +1 FINESSE (Light)
- Chain Hauberk: +20 HP, +2 defense (Medium)
- Clan-Forged Full Plate: +25 HP, +3 defense, **-1 FINESSE** (Heavy) **(v0.18: added penalty)**

**Tier 4**:
- Sentinel's Plate: +30 HP, +3 defense, +2 STURDINESS (Heavy)
- Aether-Woven Cloak: +15 HP, +2 WILL, special effect (Light)

---

## 5. Balance Considerations

### 5.1 Quality Tier Pacing

**Design Goal**: Players progress through 2-3 quality tiers per run

**v0.1 Pacing** (15-20 min playtime):
- Start: Tier 0-1 (Jury-Rigged/Scavenged)
- Mid: Tier 1-2 (Scavenged/Clan-Forged)
- End: Tier 2-3 (Clan-Forged/Optimized)

**Tier 4 (Myth-Forged)**: Reserved for rare drops, boss rewards, or long campaigns

**v0.18 Balance Adjustments**:
- **Starter weapons improved**: Jury-rigged weapons (Crude Staff, Sharpened Scrap) reduced penalty from -2 to -1 damage, improving new player experience for support classes
- **Top-tier weapon nerfed**: Clan-Forged Greatsword reduced from +6 to +5 damage to prevent early-game dominance
- **Heavy armor trade-off**: Clan-Forged Full Plate adds -1 FINESSE penalty, creating meaningful choice vs medium armor

### 5.2 Attribute Bonus Scaling

**Design**: Equipment bonuses can exceed 6-attribute cap

**Power Scaling**:
- Tier 2: +1 attribute bonus
- Tier 3: +1 to +2 attribute bonuses
- Tier 4: +2 to +3 attribute bonuses

**Impact**:
- Base attribute cap: 6
- With Tier 4 weapon (+3 MIGHT): Effective 9 MIGHT
- Result: 9d6 attack rolls (~3 successes avg) vs base 6d6 (~2 successes avg)

**Balance**: Myth-Forged items provide ~50% power spike over base cap.

### 5.3 Weapon vs Armor Priority

**Question**: Which equipment slot provides more value?

**Weapon Value**:
- Increases damage output (offense)
- Attribute bonuses improve ALL rolls using that attribute
- **ROI**: High - directly impacts combat effectiveness

**Armor Value**:
- Increases survivability (defense)
- HP bonuses provide buffer against mistakes
- Defense bonuses reduce incoming damage
- **ROI**: Medium - prevents death but doesn't win fights

**Conclusion**: Upgrade weapons first (offense > defense), then armor.

### 5.4 Equipment vs Attribute Progression

**Equipment Scaling**: +1-3 attributes per item (2 slots = +2-6 total)
**PP Scaling**: +1 attribute per 1 PP (5 PP by M3 = +5 total)

**Comparison**:
- Tier 4 equipment: +6 attributes (2 slots × +3 each)
- Milestone 3 PP spending: +5 attributes (5 PP invested)

**Result**: Equipment provides similar power to full PP investment in attributes, but equipment is loot-dependent (RNG) while PP is guaranteed (progression).

---

**End of Document**
*For complete equipment lists*: See equipment registry (pending)
*For crafting system*: See crafting documentation (v0.7+)
