# Balance Reference & Analysis

This directory contains **balance analysis materials** for tuning game systems. Use this to understand current balance state, identify issues, and make data-driven tuning decisions.

---

## Purpose

The Balance Reference provides:
- **Damage analysis** - DPS calculations, TTK metrics
- **Survivability analysis** - HP pools, effective HP, mitigation
- **Resource economy** - Stamina/Stress/Corruption rates and balance
- **Build viability** - Effectiveness ratings for different builds
- **Difficulty curves** - Progression difficulty analysis

---

## Documentation Index

### ⚔️ Damage Analysis

#### [Damage Output Analysis](./damage-output-analysis.md)
Comprehensive damage calculations across all sources.

**Contents:**
- Average damage by weapon type
- Average damage by specialization (Legend 1, 5, 10, 15)
- DPS calculations (burst vs sustained)
- Damage per Stamina efficiency
- Damage scaling curves

**Analysis Tables:**
```
| Weapon Type | Avg Damage | DPS | Stamina Cost | Efficiency |
|-------------|------------|-----|--------------|------------|
| Dagger      | 2d10+X     | YY  | 10           | Y.Y        |
| Longsword   | 3d10+X     | YY  | 15           | Y.Y        |
```

---

#### [Time-to-Kill (TTK) Analysis](./ttk-analysis.md)
How long it takes to defeat enemies.

**Contents:**
- TTK by player build vs enemy type
- TTK by Legend level
- TTK for boss encounters
- Comparison across specializations

**Target TTK Ranges:**
- Trivial enemies: 1-2 turns
- Low threat: 2-4 turns
- Medium threat: 4-6 turns
- High threat: 6-10 turns
- Lethal enemies: 10-15 turns
- Boss encounters: 15-25 turns

---

#### [Critical Hit Analysis](./critical-hit-analysis.md)
Critical hit rates and impact.

**Contents:**
- Crit chance by specialization
- Crit damage multipliers
- Expected value with crits
- Builds that maximize crits

---

### 🛡️ Survivability Analysis

#### [HP Pool Analysis](./hp-pool-analysis.md)
Character HP across builds and levels.

**Contents:**
- Base HP by archetype
- HP scaling with STURDINESS
- HP at Legend 1, 5, 10, 15
- Tank vs glass cannon comparison

**HP Ranges:**
```
Legend 1:  Tank: 80-100 HP  |  Glass Cannon: 40-60 HP
Legend 5:  Tank: 120-150 HP |  Glass Cannon: 60-80 HP
Legend 10: Tank: 180-220 HP |  Glass Cannon: 90-120 HP
```

---

#### [Damage Mitigation Analysis](./damage-mitigation-analysis.md)
How damage reduction affects survivability.

**Contents:**
- Soak values by armor type and tier
- Effective HP calculations (HP + mitigation)
- Damage reduction percentages
- Armor efficiency analysis

**Effective HP Formula:**
```
Effective_HP = Actual_HP × (1 + (Soak / Average_Damage))
```

---

#### [Deaths-per-Encounter Analysis](./deaths-per-encounter-analysis.md)
Player death rates by scenario.

**Contents:**
- Death rate by Legend level
- Death rate by specialization
- Death rate by encounter type
- Difficulty spikes (where deaths cluster)

*Note: Requires playtest data*

---

### 💰 Resource Economy Analysis

#### [Stamina Economy](./stamina-economy.md)
Stamina costs, regeneration, and balance.

**Contents:**
- Stamina costs per action type
- Regeneration rates
- Stamina pool by build
- Stamina bottlenecks
- Actions per rest

**Balance Questions:**
- Can players sustain DPS without running out?
- Are high-cost abilities worth it?
- Is regeneration too fast/slow?

---

#### [Stress Economy](./stress-economy.md)
Stress accumulation and mitigation.

**Contents:**
- All Stress gain sources (20+ documented)
- Stress gain rates by scenario
- Stress mitigation options
- Stress accumulation curves
- Breaking Point frequencies

**Stress Sources:**
```
| Source           | Stress Gain | Frequency      |
|------------------|-------------|----------------|
| Enemy proximity  | +2/turn     | Per enemy      |
| Taking damage    | +1/hit      | Per attack     |
| Ally defeat      | +10         | Per occurrence |
```

---

#### [Corruption Economy](./corruption-economy.md)
Corruption accumulation and costs.

**Contents:**
- All Corruption gain sources (15+ documented)
- Heretical ability costs
- Corruption accumulation curves
- Corruption mitigation options
- Corruption threshold impacts

**Corruption Sources:**
```
| Source                | Corruption | Voluntary? |
|-----------------------|------------|------------|
| Heretical abilities   | +2 to +5   | Yes        |
| Symbiotic Plate hit   | +1         | No         |
| Jötun-Reader exposure | +3 to +10  | No         |
```

---

#### [Resource Bottleneck Analysis](./resource-bottleneck-analysis.md)
Which resources limit player performance.

**Contents:**
- Bottleneck identification by build
- Most common limiting resource
- Resource balance recommendations

---

### 🏆 Build Viability Analysis

#### [Specialization Effectiveness Matrix](./specialization-effectiveness.md)
How effective each specialization is by role.

**Matrix Format:**
```
| Specialization | Tank | DPS | Support | Control | Utility | Overall |
|----------------|------|-----|---------|---------|---------|---------|
| Warrior Spec 1 | 9/10 | 7/10| 4/10    | 3/10    | 5/10    | 7/10    |
| Warrior Spec 2 | 7/10 | 9/10| 3/10    | 5/10    | 6/10    | 8/10    |
```

**Ratings:**
- 1-3: Weak (needs buffs)
- 4-6: Viable (playable but not optimal)
- 7-8: Strong (competitive)
- 9-10: Dominant (may need nerfs)

---

#### [Build Diversity Analysis](./build-diversity-analysis.md)
Variety of viable builds.

**Contents:**
- Number of viable builds per specialization
- Dominant builds (overused)
- Underperforming builds
- Build diversity index

**Target:** At least 3 viable builds per specialization

---

#### [Synergy Analysis](./synergy-analysis.md)
Best ability/equipment combinations.

**Contents:**
- Top ability combos
- Best equipment sets
- Specialization pairs (2-player)
- Anti-synergies (abilities that conflict)

---

### 📈 Difficulty Curve Analysis

#### [Enemy Scaling Analysis](./enemy-scaling-analysis.md)
How enemies scale with player Legend.

**Contents:**
- Enemy stat scaling formulas
- Enemy damage vs player HP curves
- Enemy HP vs player DPS curves
- Difficulty pacing

**Scaling Formula Example:**
```
Enemy_HP = Base_HP + (Player_Legend × Scaling_Factor)
```

**Balance Check:**
- Does enemy scaling match player scaling?
- Are there difficulty spikes?
- Is scaling too steep/shallow?

---

#### [Room Difficulty Progression](./room-difficulty-progression.md)
Difficulty curve through a Sector.

**Contents:**
- Difficulty by room depth
- Enemy density by location
- Hazard placement patterns
- Loot quality progression

**Target Curve:**
- Rooms 1-3: Easy (learning)
- Rooms 4-6: Medium (challenge)
- Rooms 7-9: Hard (test skills)
- Room 10+: Lethal (boss)

---

#### [Boss Encounter Analysis](./boss-encounter-analysis.md)
Boss fight balance.

**Contents:**
- Boss HP vs player DPS
- Boss damage vs player HP
- Boss mechanic complexity
- Expected fight duration

**Target Boss Duration:** 5-10 minutes (15-25 turns)

---

### 🎯 Balance Tuning Recommendations

#### [Overtuned Content](./overtuned-content.md)
Content that is too strong.

**Format:**
```
## [Ability/Enemy/Item Name]

**Issue:** [What makes it too strong]
**Data:** [Supporting numbers]
**Recommendation:** [How to nerf]
**Impact:** [What this would change]
```

---

#### [Undertuned Content](./undertuned-content.md)
Content that is too weak.

**Format:**
```
## [Ability/Enemy/Item Name]

**Issue:** [What makes it too weak]
**Data:** [Supporting numbers]
**Recommendation:** [How to buff]
**Impact:** [What this would change]
```

---

#### [Balance Priorities](./balance-priorities.md)
What to tune first.

**Priority Levels:**
- **P0 (Critical):** Game-breaking issues
- **P1 (High):** Major balance problems
- **P2 (Medium):** Noticeable but playable
- **P3 (Low):** Minor tweaks

---

## Analysis Methodology

### Data Collection
- **Code Analysis:** Extract formulas and values from codebase
- **Simulation:** Run combat simulations to calculate DPS, TTK
- **Playtest Data:** Collect metrics from real playthroughs (if available)
- **Statistical Analysis:** Identify outliers and trends

### Comparison Methods
- **Horizontal:** Compare within same tier (Tier 1 abilities vs each other)
- **Vertical:** Compare across tiers (Tier 1 vs Tier 2 vs Tier 3)
- **Cross-System:** Compare across systems (DPS vs TTK vs survivability)

### Balance Targets

**Horizontal Balance (Same Tier):**
- Abilities should be within 20% of each other in power
- Equipment should provide similar total stat value
- Enemies should have similar threat levels

**Vertical Balance (Different Tiers):**
- Higher tier should be 30-50% more powerful
- Clear power progression as player levels
- Higher tier comes with higher costs

---

## Using This Documentation

### For Balance Designers
1. Start with [Specialization Effectiveness Matrix](./specialization-effectiveness.md)
2. Identify underperforming specs
3. Drill into [Damage Analysis](./damage-output-analysis.md) or [Survivability Analysis](./hp-pool-analysis.md)
4. Check [Resource Economy](./stamina-economy.md) for bottlenecks
5. Review [Tuning Recommendations](./balance-priorities.md)

### For Developers
1. After implementing balance changes, update relevant analysis docs
2. Re-run simulations to verify changes
3. Update [Statistical Registry](../02-statistical-registry/) with new values
4. Update [System Documentation](../01-systems/) with design rationale

---

## Balance Simulation Tools

### DPS Calculator
```csharp
public double CalculateDPS(Weapon weapon, Character character, int turns)
{
    double totalDamage = 0;
    for (int i = 0; i < turns; i++)
    {
        totalDamage += SimulateAttack(weapon, character);
    }
    return totalDamage / turns;
}
```

### TTK Calculator
```csharp
public int CalculateTTK(Character attacker, Enemy target)
{
    int turns = 0;
    int remainingHP = target.HP;
    while (remainingHP > 0)
    {
        int damage = SimulateAttack(attacker, target);
        remainingHP -= damage;
        turns++;
    }
    return turns;
}
```

### Effective HP Calculator
```csharp
public double CalculateEffectiveHP(int actualHP, int soak, int avgDamage)
{
    double mitigation = (double)soak / avgDamage;
    return actualHP * (1 + mitigation);
}
```

---

## Progress Tracking

**Balance Documentation Status:**
- Damage Analysis: 0 / 3 complete
- Survivability Analysis: 0 / 3 complete
- Resource Economy: 0 / 4 complete
- Build Viability: 0 / 3 complete
- Difficulty Curves: 0 / 3 complete
- Tuning Recommendations: 0 / 3 complete

**Overall Progress:** 0%

---

**Last Updated:** 2025-11-12
**Documentation Version:** v0.17
