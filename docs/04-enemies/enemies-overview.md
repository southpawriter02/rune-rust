# Enemies & Encounters System

**Version**: v0.17 (Based on v0.16 implementation)
**Status**: Core System (Tier 3)
**Dependencies**: Combat Engine, Attributes, Trauma Economy
**Integration Points**: Combat Resolution, Legend Awards, AI System

---

## Table of Contents

1. [Functional Overview](#1-functional-overview)
2. [Enemy Categories](#2-enemy-categories)
3. [Enemy Mechanics](#3-enemy-mechanics)
4. [Enemy Types Summary](#4-enemy-types-summary)
5. [Balance Considerations](#5-balance-considerations)

---

## 1. Functional Overview

### 1.1 Purpose

**Enemies** are the primary combat opponents in Aethelgard. The enemy system provides:
- **Combat Challenges**: Varied stat blocks and abilities
- **Progression Rewards**: Legend (XP) awards upon defeat
- **Narrative Flavor**: Machine types reflect the world's lore
- **Trauma Triggers**: Forlorn enemies inflict Psychic Stress

### 1.2 Enemy Structure

```csharp
public class Enemy
{
    string Name;              // "Corrupted Servitor"
    EnemyType Type;          // Enum identifier
    int HP, MaxHP;           // Health pool
    Attributes Attributes;   // MIGHT, FINESSE, STURDINESS
    int BaseDamageDice;      // 1-4d6
    int BaseLegendValue;     // XP reward (20-150)
    bool IsForlorn;          // Inflicts Psychic Stress aura
    bool IsBoss;             // Elite enemy flag
}
```

---

## 2. Enemy Categories

### 2.1 Difficulty Tiers

| Tier | HP Range | Damage | Legend Value | Examples |
|------|----------|--------|--------------|----------|
| **Trivial** | 10-20 | 1d6 | 10-20 | Corroded Sentry |
| **Low** | 20-40 | 1d6 to 1d6+2 | 20-40 | Scrap Hound, Husk Enforcer |
| **Medium** | 40-60 | 2d6 | 40-60 | Corrupted Servitor, Shrieker |
| **High** | 60-90 | 2d6+2 | 60-90 | War Frame, Bone Keeper |
| **Lethal** | 90-120 | 3d6 | 80-120 | Rust Witch, Sentinel Prime |
| **Boss** | 100-200 | 3d6+ | 100-150 | Aetheric Aberration, Omega Sentinel |

### 2.2 Enemy Archetypes

**Swarm Enemies** (Low HP, appears in groups):
- Scrap Hound, Maintenance Construct, Servitor Swarm
- Strategy: Low HP but high numbers; AOE abilities effective

**Tank Enemies** (High HP, high defense):
- War Frame, Vault Custodian, Omega Sentinel
- Strategy: Long fights; chip damage and debuffs effective

**Caster Enemies** (Medium HP, special abilities):
- Forlorn Scholar, Corrupted Engineer, Jötun-Reader Fragment
- Strategy: Burst damage or silence effects

**Boss Enemies** (High HP, phase mechanics):
- Aetheric Aberration, Forlorn Archivist, Omega Sentinel
- Strategy: Multi-phase fights with changing tactics

---

## 3. Enemy Mechanics

### 3.1 Forlorn Enemies

**Forlorn**: Psychic Stress aura (WILL Resolve Check)

**Forlorn Enemies**:
- Forlorn Scholar
- Forlorn Archivist
- TestSubject (sometimes)

**Mechanic**: At start of combat, player rolls WILL Resolve Check (DC 2). If fail, gain 5-10 Psychic Stress.

**Design**: Trauma Economy integration—some enemies threaten mental health, not just physical.

### 3.2 Status Effects

**Enemies can have**:
- **Stunned**: Skip turn
- **Bleeding**: 1d6 damage per turn
- **Analyzed**: Allies get +2 accuracy
- **Vulnerable**: +25% damage taken
- **Silenced**: Cannot cast spells

**Applied by**:
- Player abilities (Disrupt, Precision Strike, Anatomical Insight)
- Certain equipment effects

### 3.3 Special Mechanics

**Soak** (Damage Reduction):
- Some enemies have armor that reduces incoming damage
- Examples:
  - Vault Custodian: **4 Soak** **(v0.18: reduced from 6)**
  - Omega Sentinel: **6 Soak** **(v0.18: reduced from 8)**

**Poison**:
- Sludge Crawler inflicts poison (1d6 damage per turn)
- Lasts 2-3 turns

**Phases** (Bosses):
- Aetheric Aberration: Phase 1 (ranged), Phase 2 (melee enrage)
- Omega Sentinel: Phase 1 (defensive), Phase 2 (offensive)

---

## 4. Enemy Types Summary

### 4.1 Complete Enemy List (36 Total)

**v0.1-v0.3 Enemies** (3):
1. Corrupted Servitor (Medium)
2. Blight Drone (Medium)
3. Ruin Warden (High)

**v0.4 Enemies** (5):
4. Scrap Hound (Low)
5. Test Subject (Low)
6. War Frame (High)
7. Forlorn Scholar (Medium, Forlorn)
8. Aetheric Aberration (Boss) **[75 HP, v0.18: increased from 60]**

**v0.6 Enemies** (6):
9. Maintenance Construct (Low)
10. Sludge Crawler (Low, Poison)
11. Corrupted Engineer (Medium, Support)
12. Vault Custodian (High) **[Soak 4, v0.18: reduced from 6]**
13. Forlorn Archivist (Boss, Forlorn)
14. Omega Sentinel (Boss, Tank) **[Soak 6, v0.18: reduced from 8]**

**v0.16 Enemies** (10):
15. Corroded Sentry (Trivial) **[10 Legend, v0.18: increased from 5]**
16. Husk Enforcer (Low) **[18 Legend, v0.18: increased from 15]**
17. Arc Welder Unit (Low) **[25 Legend, v0.18: increased from 20]**
18. Shrieker (Medium, Psychic)
19. Jötun-Reader Fragment (Medium, Tech)
20. Servitor Swarm (Medium, Swarm) **[40 Legend, v0.18: increased from 30]**
21. Bone Keeper (High, Undead) **[55 Legend, v0.18: increased from 50]**
22. Failure Colossus (High, Construct) **[3d6+4 damage, v0.18: reduced from 4d6+3]**
23. Rust Witch (Lethal, Corruption)
24. Sentinel Prime (Lethal, Military) **[4d6 damage, v0.18: reduced from 5d6]**

**Total**: 36 enemy types across 5 difficulty tiers

---

## 5. Balance Considerations

### 5.1 Enemy Scaling

**HP Scaling**: Linear growth (~10 HP per tier jump)
**Damage Scaling**: +1d6 or +2 damage bonus per 2 tiers

**Ratio**:
- Trivial: 15 HP, 1d6 (3.5 avg) = ~4 hits to kill
- Medium: 50 HP, 2d6 (7 avg) = ~7 hits to kill
- Lethal: 100 HP, 3d6 (10.5 avg) = ~10 hits to kill

**Design**: Combat duration increases with enemy tier, but not exponentially.

### 5.2 Legend Value Scaling

**Legend Awards**: Target ratio of 0.8-1.2 Legend per HP (normal), 1.2-1.5 (bosses)

**Examples (v0.18 adjusted)**:
- Corroded Sentry: 15 HP, **10 Legend** (0.67 ratio - early enemy, intentionally lower)
- Corrupted Servitor: 50 HP, 40 Legend (0.80 ratio)
- Aetheric Aberration: **75 HP**, 60 Legend (0.80 ratio - boss)
- Omega Sentinel: 150 HP, 120 Legend (0.80 ratio - boss)

**Design**: Higher-tier enemies provide proportionally more XP, incentivizing risk. v0.18 adjusted several enemies to improve Legend/HP ratios for fairer progression.

### 5.3 Forlorn Enemy Risk/Reward

**Forlorn Enemies**:
- Risk: +5-10 Psychic Stress (WILL check to resist)
- Reward: 25% bonus Legend (Trauma Modifier = 1.25)

**Trade-off**: Mental health vs faster progression

**Example**:
- Forlorn Scholar: 50 HP, 40 BLV × 1.25 TM = **50 Legend**
- Standard Scholar: 50 HP, 40 BLV × 1.0 TM = **40 Legend**

**Balance**: +10 Legend reward (+25%) justifies +5-10 Stress risk.

---

**End of Document**
*For detailed enemy stat blocks*: See enemy bestiary (pending)
*For AI behavior*: See enemy AI documentation (pending)
