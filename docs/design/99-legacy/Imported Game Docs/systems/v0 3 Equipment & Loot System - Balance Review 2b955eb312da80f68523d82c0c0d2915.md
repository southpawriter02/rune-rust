# v0.3 Equipment & Loot System - Balance Review

**Date:** 2025-11-11
**Reviewer:** Claude Code
**Status:** Ready for Playtesting

## Executive Summary

The v0.3 equipment system provides a **well-balanced progression curve** with clear quality tier upgrades and class-specific itemization. The loot drop rates create meaningful scarcity while ensuring players can progress. **Recommended for release** with optional minor adjustments based on playtest feedback.

---

## Weapon Damage Analysis

### Damage Progression by Quality Tier

### Warrior Weapons (MIGHT-based)

| Weapon | Quality | Damage | Stamina | Special |
| --- | --- | --- | --- | --- |
| Rusty Hatchet | Jury-Rigged | 1d6 | 5 | - |
| Scrap Axe | Scavenged | 1d6+2 | 5 | - |
| Clan Greataxe | Clan-Forged | 1d6+3 | 5 | +2 MIGHT |
| Optimized Axe | Optimized | 2d6+2 | 5 | +1 MIGHT, +2 ACC |
| Warden's Greatsword | Myth-Forged | 2d8+4 | 8 | Ignores 50% armor, +Fortified |

**Average Damage per Attack:**

- Jury-Rigged: 3.5 dmg
- Scavenged: 5.5 dmg (+57%)
- Clan-Forged: 6.5 dmg (+18%)
- Optimized: 9 dmg (+38%)
- Myth-Forged: 13 dmg (+44%)

**Balance Assessment:** ✅ **Good**

- Clear upgrades at each tier
- Myth-Forged feels legendary (3.7x starter damage)
- Stamina costs balanced (8 for greatsword is appropriate)

### Scavenger Weapons (FINESSE-based)

| Weapon | Quality | Damage | Stamina | Special |
| --- | --- | --- | --- | --- |
| Makeshift Spear | Jury-Rigged | 1d6 | 4 | - |
| Salvaged Dagger | Scavenged | 1d6+1 | 3 | - |
| Balanced Spear | Clan-Forged | 1d6+3 | 4 | +3 FINESSE |
| Precision Dagger | Optimized | 2d6+1 | 3 | +2 FINESSE, +3 ACC |
| Shadow's Edge | Myth-Forged | 2d6+3 | 3 | Bleed, +4 FINESSE |

**Average Damage per Attack:**

- Jury-Rigged: 3.5 dmg
- Scavenged: 4.5 dmg (+29%)
- Clan-Forged: 6.5 dmg (+44%)
- Optimized: 8 dmg (+23%)
- Myth-Forged: 10 dmg (+25%)

**Balance Assessment:** ✅ **Good**

- Lower stamina costs reward faster attacks
- High accuracy bonuses fit "precision" theme
- Bleed effect adds tactical depth

### Mystic Weapons (WILL-based)

| Weapon | Quality | Damage | Stamina | Special |
| --- | --- | --- | --- | --- |
| Crude Staff | Jury-Rigged | 1d6 | 6 | - |
| Salvaged Focus | Scavenged | 1d6+1 | 5 | - |
| Resonant Staff | Clan-Forged | 1d6+2 | 6 | +2 WILL |
| Aether Conduit | Optimized | 2d6+2 | 5 | +3 WILL, -1 Aether cost |
| Void Lens | Myth-Forged | 2d6+4 | 5 | +4 WILL, -2 Aether, Aether regen |

**Average Damage per Attack:**

- Jury-Rigged: 3.5 dmg
- Scavenged: 4.5 dmg (+29%)
- Clan-Forged: 5.5 dmg (+22%)
- Optimized: 9 dmg (+64%)
- Myth-Forged: 11 dmg (+22%)

**Balance Assessment:** ✅ **Good**

- Aether cost reduction is valuable for ability-focused class
- Void Lens provides strongest utility (Aether regen)
- Damage slightly lower than Warriors but compensated by abilities

---

## Armor Stat Analysis

### HP & Defense Progression

| Armor | Quality | HP Bonus | Defense | Special |
| --- | --- | --- | --- | --- |
| Scrap Plating | Jury-Rigged | 5 | 1 | - |
| Tattered Leathers | Jury-Rigged | 3 | 0 | +1 FINESSE |
| Salvaged Plating | Scavenged | 10 | 2 | - |
| Reinforced Leathers | Scavenged | 8 | 1 | +2 FINESSE |
| Clan Heavy Plate | Clan-Forged | 20 | 4 | +2 STURDINESS, -1 FINESSE |
| Clan Scout Armor | Clan-Forged | 15 | 2 | +3 FINESSE |
| Optimized Combat Rig | Optimized | 30 | 5 | +3 STURDINESS |
| Optimized Stealth Suit | Optimized | 20 | 3 | +4 FINESSE, +1 WITS |
| Warden's Aegis | Myth-Forged | 40 | 6 | +4 STURDINESS, +Block |
| Shadowweave Cloak | Myth-Forged | 25 | 4 | +5 FINESSE, +Evasion |

**Balance Assessment:** ✅ **Excellent**

- Clear trade-offs between HP and mobility
- Heavy armor: More HP/Defense, -FINESSE
- Light armor: Less HP, +FINESSE, +utility
- Myth-Forged offers 8x HP bonus over starting gear

**Armor Philosophy:**

- Warriors: Heavy armor for survivability
- Scavengers: Light armor for mobility
- Mystics: Medium armor for balance

---

## Loot Drop Rate Analysis

### Enemy Tier Drop Tables

### Tier 1: Corrupted Servitor (Entrance/Corridor enemies)

```
60% - Jury-Rigged
30% - Scavenged
10% - Nothing

```

**Expected Loot per 10 Kills:** 9 items (6 Jury-Rigged, 3 Scavenged)

**Balance Assessment:** ✅ **Good**

- Early game grind is reasonable
- Players will accumulate starter gear
- 10% no-drop prevents excessive inventory bloat

### Tier 2: Blight-Drone (Workshop/Junction enemies)

```
40% - Scavenged
40% - Clan-Forged
20% - Optimized

```

**Expected Loot per 10 Kills:** 10 items (4 Scavenged, 4 Clan-Forged, 2 Optimized)

**Balance Assessment:** ✅ **Excellent**

- Mid-game progression feels rewarding
- Clan-Forged drops provide meaningful upgrades
- Optimized drops create excitement

### Tier 3: Ruin-Warden (Boss)

```
30% - Optimized
70% - Myth-Forged

```

**Expected Loot per Kill:** 1 item (70% chance Myth-Forged)

**Balance Assessment:** ✅ **Perfect**

- Boss loot feels legendary
- Myth-Forged weapons are game-changing
- 100% drop rate appropriate for boss difficulty

---

## Class-Appropriate Loot

### Smart Loot System (60% class match)

**Mechanic:** 60% chance for weapon to match player class, 40% universal armor

**Example for Warrior:**

- 60% → Axe/Greatsword
- 40% → Armor (universal)

**Balance Assessment:** ✅ **Good**

- Reduces bad RNG frustration
- Still allows cross-class drops for variety
- Armor is always useful (no class restrictions)

---

## Progression Curve

### Expected Equipment Timeline

| Milestone | Expected Quality | Source |
| --- | --- | --- |
| 0 (Start) | Jury-Rigged | Starting gear |
| 1 (Puzzle) | Clan-Forged | Puzzle reward |
| 1-2 | Scavenged | Servitor farming |
| 2-3 | Clan-Forged | Blight-Drone drops |
| 3 (Pre-Boss) | Optimized | Lucky Blight-Drone drops |
| 4 (Boss Kill) | Myth-Forged | Ruin-Warden guaranteed drop |

**Balance Assessment:** ✅ **Smooth**

- No gear gaps or progression stalls
- Puzzle reward provides guaranteed upgrade
- Boss kill feels rewarding

---

## Inventory Economy

### Capacity Limits

- **Max Inventory:** 5 items
- **Equipped Slots:** 2 (weapon + armor)
- **Total Carrying:** 7 items max

**Balance Assessment:** ✅ **Good**

- Forces meaningful choices
- Encourages equipment comparison
- Prevents hoarding

**Recommendation:** Consider inventory upgrade as future feature (v0.4+)

---

## Special Effects Balance

### Myth-Forged Abilities

### Warden's Greatsword

- **Effect:** Ignores 50% armor, grants [Fortified] on kill
- **Balance:** ✅ Good - Rewards aggressive play

### Shadow's Edge

- **Effect:** Inflicts [Bleeding] (2 damage/turn)
- **Balance:** ✅ Good - DoT adds tactical depth

### Void Lens

- **Effect:** -2 Aether cost, Aether regen on hit
- **Balance:** ✅ Good - Enables ability spam, fits Mystic fantasy

### Warden's Aegis

- **Effect:** Can block attacks (chance to negate damage)
- **Balance:** ✅ Good - Defensive option for Warriors

### Shadowweave Cloak

- **Effect:** +50% evasion chance
- **Balance:** ⚠️ **Monitor** - Could be too strong if evasion stacks

**Recommendation:** Playtest Shadowweave Cloak for balance (50% evasion + FINESSE bonuses)

---

## Identified Balance Concerns

### 🟡 Minor Concerns

1. **Shadowweave Cloak Evasion Stacking**
    - **Issue:** +5 FINESSE + 50% evasion could be too strong
    - **Recommendation:** Cap evasion at 60% total or reduce cloak bonus to 30%
    - **Priority:** Low (requires playtesting)
2. **Inventory Capacity at Higher Milestones**
    - **Issue:** 5 slots may feel restrictive late game
    - **Recommendation:** Add "Expand Inventory" progression option in v0.4
    - **Priority:** Low (quality of life)
3. **Armor Defense Bonus Implementation**
    - **Issue:** Defense bonus not yet applied to enemy attacks
    - **Recommendation:** Implement in combat calculations
    - **Priority:** Medium (feature gap)

### 🟢 No Major Concerns

- Weapon damage progression: **Balanced**
- Loot drop rates: **Balanced**
- Class itemization: **Balanced**
- Progression curve: **Smooth**

---

## Recommendations

### For Immediate Release

✅ **Ready to Ship** - No critical balance issues identified

### For v0.3.1 Patch (if needed)

1. Implement armor defense bonus in CombatEngine
2. Monitor Shadowweave Cloak evasion in playtests
3. Adjust drop rates if players report too much/too little loot

### For v0.4 Future Features

1. Inventory expansion progression
2. Equipment enchantments/upgrades
3. Set bonuses for matching quality tiers
4. Weapon special attacks tied to equipment

---

## Playtesting Checklist

When players test v0.3, gather feedback on:

- [ ]  Does loot drop frequently enough? Too frequently?
- [ ]  Do upgrades feel meaningful?
- [ ]  Is inventory management fun or frustrating?
- [ ]  Are Myth-Forged items exciting?
- [ ]  Does class-specific loot work well?
- [ ]  Is armor defense bonus noticeable? (once implemented)
- [ ]  Does Shadowweave Cloak evasion feel too strong?
- [ ]  Is there a favorite weapon/armor for each class?

---

## Conclusion

The v0.3 equipment system demonstrates **strong foundational balance** with clear progression curves, meaningful itemization, and rewarding loot mechanics. The system is **recommended for release** with confidence that the core balance is sound.

**Overall Balance Rating:** 9/10

**Key Strengths:**

- Clear quality tier progression
- Balanced damage scaling
- Smart loot system reduces frustration
- Myth-Forged items feel legendary

**Minor Improvements:**

- Implement armor defense bonus
- Monitor evasion stacking
- Future: inventory expansion

---

*Balance review conducted 2025-11-11 by Claude CodeNext review: After 10+ playtests*