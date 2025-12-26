# Dynamic Scaling System — Mechanic Specification v5.0

Type: Mechanic
Status: Review
Balance Validated: No
Document ID: AAM-SPEC-MECHANIC-DYNAMICSCALING-v5.0
Parent item: Encounter System — Core System Specification v5.0 (Encounter%20System%20%E2%80%94%20Core%20System%20Specification%20v5%200%20c82c42d7129c4843a86f2e69cd72f0d7.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

## Core Philosophy

The Dynamic Scaling System is the **automated difficulty manager**. It ensures every encounter remains meaningful by scaling enemy stats to party power level.

**Design Pillars:**

- Consistent challenge across all areas
- Power-based scaling (PP + Equipment Quality)
- Anti-grinding (prevents trivial encounters)
- Meaningful progression through efficiency gains

---

## Party Power Score (PPS)

```
PPS = Average of Individual Power Scores

Individual Power Score = ProgressionPointsSpent + (ItemQualityScore / 2)
```

### Item Quality Values

| Quality | Value |
| --- | --- |
| Jury-Rigged | 0.5 |
| Scavenged | 1 |
| Clan-Forged | 2 |
| Optimized | 4 |
| Myth-Forged | 6 |

---

## Target Difficulty Rating (TDR)

```
TDR = PPS × Difficulty Multiplier
```

| Encounter Type | Multiplier | Example (PPS 50) |
| --- | --- | --- |
| Trivial | 0.5× | TDR 25 |
| Easy | 0.8× | TDR 40 |
| Standard | 1.0× | TDR 50 |
| Difficult | 1.25× | TDR 63 |
| Deadly (Boss) | 1.5-2.0× | TDR 75-100 |

---

## Scaling Applications

### Enemy HP Scaling (Most Impactful)

```
Final HP = (Base HP + Tier Bonus) × (TDR / 10)
```

**Tier Bonuses:** Minion +0, Standard +10, Elite +30, Boss +100

### Enemy Damage Scaling (Moderate)

```
Final Damage = Base Damage + (TDR / 20)
```

### Loot Quality Scaling

Higher TDR → better quality drops (see Loot System)

### Legend (XP) Scaling

- TDR < PPS - 2: 0× (trivial, no reward)
- TDR = PPS: 1.0×
- TDR = PPS + 2: 2.0×

---

## Scaling Caps

- **Maximum TDR:** 150 (endgame cap)
- **Minimum TDR:** 10 (tutorial areas)

---

## Integration Points

**Dependencies:** Saga System (PP spent), Equipment System (gear quality)

**Referenced By:** Enemy spawning, Loot System, Legend System