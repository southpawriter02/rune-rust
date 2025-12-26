---
id: SPEC-COMBAT-012
title: "Enemy Design & Bestiary System"
version: 1.0.0
status: Draft
last-updated: 2025-11-21
related-files:
  - path: "RuneAndRust.Core/Enemy.cs"
    status: Exists
  - path: "RuneAndRust.Engine/EnemyFactory.cs"
    status: Exists
  - path: "RuneAndRust.Engine/EnemyAI.cs"
    status: Exists
---

# Enemy Design & Bestiary System

> "The horrors of Aethelgard are not random; they are the calculated remnants of a broken world, refined by corruption and time."

---

## 1. Overview

### 1.1 Identity Table

| Property | Value |
|----------|-------|
| Spec ID | `SPEC-COMBAT-012` |
| Category | Combat |
| Priority | Must-Have |
| Status | Draft |
| Domain | Combat, Enemy Design |

### 1.2 Core Philosophy

The **Enemy Design & Bestiary System** provides the design framework for creating balanced, thematically appropriate enemies across all threat levels in Rune & Rust. It formalizes stat budgets, archetypes, AI behaviors, and trauma integration to ensure a cohesive and challenging combat experience.

**Design Pillars:**
1.  **Readable Threat Levels**: Players should instantly recognize enemy threat through stat patterns (HP ranges, visual cues) rather than trial-and-error deaths.
2.  **Tactical Diversity Through Archetypes**: Enemies should force different tactical responses (e.g., Tank vs. Glass Cannon) rather than just "hit until dead."
3.  **Balanced Power Budget**: Enemy power scales predictably with threat tier, avoiding surprise one-shots and bullet sponges.
4.  **Horror Atmosphere Through Trauma**: Enemies evoke dread through psychological mechanics (Stress/Corruption) via the "IsForlorn" flag and specific abilities.

---

## 2. Player Experience

### 2.1 How Players Interact

Players encounter enemies as the primary obstacles in the dungeon. They must assess threat levels based on enemy types and descriptions, choose appropriate tactics based on archetypes, and manage both physical health and psychological trauma (Stress/Corruption).

### 2.2 Key Features

-   **5 Threat Tiers**: Low, Medium, High, Lethal, Boss.
-   **8 Enemy Archetypes**: Tank, DPS, Glass Cannon, Support, Swarm, Caster, Mini-Boss, Boss.
-   **Special Mechanics**: 'IsForlorn' (Trauma Aura), 'IsBoss' (Phases), 'IsChampion' (Elite), 'Soak' (Damage Reduction).
-   **Loot Integration**: Risk-reward balance where harder enemies drop better gear.

---

## 3. Mechanics

### 3.1 Threat Level Classification

All enemies are classified into one of 5 threat tiers.

| Threat Tier | HP Range | Attribute Budget | Damage Range | Legend Value | Example Enemies |
|-------------|----------|------------------|--------------|--------------|-----------------|
| **Low** | 10-15 HP | 5-10 attr points | 1d6 to 1d6+2 | 10-20 Legend | Corrupted Servitor, Scrap-Hound |
| **Medium** | 25-50 HP | 8-16 attr points | 1d6+1 to 2d6 | 15-50 Legend | Blight-Drone, Forlorn Scholar |
| **High** | 60-70 HP | 12-17 attr points | 2d6+2 to 3d6 | 55-75 Legend | Bone-Keeper, Vault Custodian |
| **Lethal** | 80-90 HP | 13-20 attr points | 3d6+4 to 4d6 | 60-100 Legend | Failure Colossus, Sentinel Prime |
| **Boss** | 75-100 HP | 13-20 attr points | 2d6 to 3d6+3 | 100-150 Legend | Ruin-Warden, Omega Sentinel |

### 3.2 Enemy Archetypes

Taxonomy defining tactical role, stat distribution, and AI behavior.

| Archetype | Role | Stat Profile | Examples |
|-----------|------|--------------|----------|
| **Tank** | Absorb damage, protect allies | High STURDINESS, High HP, Soak | Vault Custodian, Omega Sentinel |
| **DPS** | Consistent damage, balanced | Balanced Stats, Medium HP | Blight-Drone, War-Frame |
| **Glass Cannon** | High priority threat | High FINESSE/MIGHT, Low HP | Scrap-Hound, Test Subject |
| **Support** | Buff/Debuff, disrupt | High WITS, Medium HP | Corrupted Engineer |
| **Swarm** | Overwhelm through quantity | Low Attributes, High Group HP | Servitor Swarm |
| **Caster** | Magic/Psychic attacks | High WILL, Medium HP | Forlorn Scholar, Rust-Witch |
| **Mini-Boss** | Mid-dungeon challenge | High HP, Phase AI | Vault Custodian |
| **Boss** | Climactic encounter | Very High HP, 3-Phase | Ruin-Warden, Aetheric Aberration |

### 3.3 Special Mechanics Flags

-   **IsForlorn**: Inflicts passive Psychic Stress/Corruption aura. Adds 10-20% Legend value.
-   **IsBoss**: Enables phase-based AI, special abilities, and high-tier loot.
-   **IsChampion**: Procedural elite variants (+50% stats, unique ability).
-   **Soak**: Flat damage reduction. Capped at 4 for non-bosses, 6 for bosses.

---

## 4. Calculations

### 4.1 Stat Budget Formulas

**HP Budget:**
-   Low: 10-15 (avg 12.5)
-   Medium: 25-50 (avg 37.5)
-   High: 60-70 (avg 65)
-   Lethal: 80-90 (avg 85)
-   Boss: 75-100 (avg 87.5)

**Damage Budget:**
-   Low: 1d6 to 1d6+2 (avg 3.5-5.5)
-   Medium: 1d6+1 to 2d6 (avg 4.5-7)
-   High: 2d6+2 to 3d6 (avg 9-10.5)
-   Lethal: 3d6+4 to 4d6 (avg 14.5-14) *Capped to prevent one-shots*
-   Boss: 2d6 to 3d6+3 (avg 7-13.5)

### 4.2 Legend Value Formula

```
Legend = HP × (0.5 to 1.0) + Bonus

Ratios:
- 0.5 ratio: Low tier minions
- 1.0 ratio: High tier elites
- Bonuses: +10-20 (Heal), +10-25 (Support), +20-50 (Boss)
```

### 4.3 Scaling vs. Player Progression (Proposed)

**HP Scaling:**
```
Scaled_HP = Base_HP + (Player_Legend × HP_Scaling_Factor)
Factor: Low (0.5), Medium (1.0), High (1.5), Boss (2.0)
```

**Damage Scaling:**
```
Scaled_Damage = Base_Damage_Dice + (Player_Legend ÷ 3)d6
```

---

## 5. Integration Points

### 5.1 Dependencies

| System | Dependency Type |
|--------|-----------------|
| **Combat Resolution** | Reads turn sequence and initiative |
| **Damage Calculation** | Integrates damage formulas and mitigation |
| **Status Effects** | Applies debuffs (Bleed, Poison, etc.) |
| **Loot System** | Maps threat tiers to drop quality |
| **Trauma Economy** | Integrates Stress/Corruption infliction |
| **Character Progression** | Scales enemies against player Legend |

### 5.2 Loot Table Integration

| Threat Tier | Primary Loot | Secondary Loot | No Drop Rate |
|-------------|--------------|----------------|--------------|
| **Low** | Tier 0 (60%) | Tier 1 (30%) | 10% |
| **Medium** | Tier 1 (40%) | Tier 2 (40%), T3 (20%) | 0% |
| **High** | Tier 2 (40%) | Tier 3 (40%), T4 (20%) | 0% |
| **Lethal** | Tier 3 (50%) | Tier 4 (50%) | 0% |
| **Boss** | Tier 4 (70%) | Tier 3 (30%) | 0% |

### 5.3 Trauma Economy Integration

-   **IsForlorn**: +5 Stress on sight, +3 Stress/turn proximity.
-   **Psychic Attacks**: +2 Stress per hit.
-   **Corruption**: Jötun-Reader (+2/turn), Symbiotic Plate (+1 on hit).
-   **Resistance**: `Inflicted = Base - (Player_WILL × 0.5)`

---

## 6. UI Requirements

### 6.1 Visual Cues

-   **HP Bars**: Visual length or color coding to indicate Threat Tier (e.g., 10 HP vs 100 HP).
-   **Archetype Icons**: Symbols distinguishing Tank (Shield), DPS (Sword), Caster (Eye/Rune).
-   **Trauma Indicators**: Visual pulsing or aura effect for 'IsForlorn' enemies.

---

## 7. Balance Data

### 7.1 Time-to-Kill (TTK) Targets

| Tier | Solo Player | 2-Player Party | Intent |
|------|-------------|----------------|--------|
| **Low** | 2-3 turns | 1-2 turns | Fodder, swarm threats |
| **Medium** | 4-6 turns | 2-3 turns | Standard combat |
| **High** | 7-10 turns | 4-5 turns | Resource management |
| **Lethal** | 10-15 turns | 6-8 turns | Party-wipe potential |
| **Boss** | 12-20 turns | 8-12 turns | Multi-phase narrative setpieces |

### 7.2 Damage Output

-   **No One-Shots**: Max single-hit damage ≤ 80% of player HP.
-   **Variance**: Keep max/min damage ratio under 4:1.
-   **Sustained Threat**: Bosses threaten 3-6 turn kills, not instant kills.

### 7.3 v0.18 Balance Lessons Learned

**Problem 1: Damage Variance Feels Unfair**
*Sentinel Prime (BEFORE)*: 5d6 (5-30 dmg). Player perception: "RNG lottery."
*Sentinel Prime (AFTER)*: 4d6 (4-24 dmg). Variance reduced to 6:1. Average damage reduced 17.5 -> 14.

**Problem 2: Soak Creates Bullet Sponges**
*Omega Sentinel (BEFORE)*: Soak 8. TTK > 25 turns.
*Omega Sentinel (AFTER)*: Soak 6. TTK ~16 turns.
*Design Principle*: Soak Cap = 6 for Boss, 4 for others. Balance tankiness with HP, not Soak.

**Problem 3: One-Shot Potential**
*Failure Colossus (BEFORE)*: 4d6+3 (17-27 dmg). 70% chance to one-shot Legend 1 player.
*Failure Colossus (AFTER)*: 3d6+4 (7-22 dmg). 0% chance to one-shot.
*Design Principle*: Max hit ≤ 80% of Legend-appropriate player HP.

---

## 8. Voice Guidance

### 8.1 System Tone

**Layer 2 Diagnostic Voice (Cargo Cult / Techno-Mysticism)**
The system speaks as an ancient, corrupted diagnostic tool analyzing "biological" and "machine" components with religious reverence for the "Iron Heart" and disdain or clinical detachment for flesh.

| Context | Tone |
|---------|------|
| **Enemy Analysis** | Clinical, cautionary, reverent of machine complexity. |
| **Threat Warning** | Urgent, fatalistic. |
| **Trauma Detection** | Observing psychic resonance as "data corruption" or "spirit blight". |

### 8.2 Feedback Text Examples

| Event | Text |
|-------|------|
| **Boss Encounter** | "High-Magnitude Essence detected. The Machine Spirit screams in resonance. Proceed with ritual caution." |
| **Forlorn Aura** | "Warning: Local reality stability degrading. Psychic static interfering with cortex functions. Guard your soul-code." |
| **Lethal Threat** | "Entity classification: LETHAL. Survival probability: Minimal. Recommendation: Offer prayers to the Iron Heart." |
| **Victory** | "Threat neutralized. Harvesting salvage data. The cycle continues." |

---

## 9. Technical Implementation

### 9.1 Data Model
```csharp
public class Enemy
{
    public string Name { get; set; }
    public EnemyType Type { get; set; }
    public int HP { get; set; }
    public int MaxHP { get; set; }
    public Attributes Attributes { get; set; }
    
    // Combat Stats
    public int BaseDamageDice { get; set; }
    public int DamageBonus { get; set; }
    public int Soak { get; set; }
    
    // Flags & Meta
    public bool IsForlorn { get; set; }
    public bool IsBoss { get; set; }
    public bool IsChampion { get; set; }
    public int BaseLegendValue { get; set; }
    public List<CreatureTraitType> Traits { get; set; }
}
```

### 9.2 Factory Interface
```csharp
public interface IEnemyFactory
{
    Enemy CreateEnemy(string enemyId, int difficultyTier);
    Enemy CreateProceduralChampion(EnemyType baseType, int legendLevel);
    
    // Internal Builders
    void ApplyArchetypeStats(Enemy enemy, EnemyArchetype archetype);
    void ApplyTraitPackage(Enemy enemy, List<CreatureTraitType> traits);
}
```

---

## 10. Phased Implementation Guide

### Phase 1: Core System
- [ ] **Data Model**: Implement `Enemy` class and `EnemyType` enum.
- [ ] **Budget**: Implement static `StatBudget` tables for Tiers 1-5.
- [ ] **Factory**: Create `EnemyFactory` with basic `CreateEnemy(id)` support.

### Phase 2: Traits & Mechanics
- [ ] **Traits**: Integrate `CreatureTraits` list into Enemy object.
- [ ] **Soak**: Implement `Soak` damage mitigation logic.
- [ ] **Forlorn**: Implement `IsForlorn` stress aura in Combat Loop.

### Phase 3: Loot & Scaling
- [ ] **Loot**: Connect `DropTable` generation to `ThreatTier`.
- [ ] **Scaling**: Implement `ScaleToLegend(int level)` formulas.
- [ ] **Bosses**: Implement Multi-Phase State for `IsBoss` enemies.

### Phase 4: UI & Feedback
- [ ] **Visuals**: Distinct HP bars for Boss/Elite.
- [ ] **Icons**: Archetype icons (Shield, Sword, Eye) implementation.
- [ ] **Log**: "Rare Enemy Appeared!" alerts.

---

## 11. Testing Requirements

### 11.1 Unit Tests
- [ ] **Budget**: Factory(Tier 1) -> HP between 10-15.
- [ ] **Cap**: Factory(Lethal) -> Max Dmg < 80% Player HP.
- [ ] **Traits**: IsForlorn -> Has Trait 'ForlornAura'.
- [ ] **Scaling**: Legend 5 -> HP = Base + (5 * ScaleFactor).

### 11.2 Integration Tests
- [ ] **Spawn**: Encounter.Generate -> Creates valid Enemy List.
- [ ] **Combat**: Boss Enemy triggers Phase 2 at 50% HP.
- [ ] **Loot**: Boss Death -> Drops Tier 4 Loot (70% chance).

### 11.3 Manual QA
- [ ] **Visual**: Verify Archetype Icon matches behavior (Tank = Shield).
- [ ] **Audio**: Boss music starts on Boss Encounter trigger.

---

## 12. Logging Requirements

**Reference:** [logging.md](../logging.md)

### 12.1 Log Events

| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Enemy Spawn | Info | "Spawned {Name} (Tier: {Tier}, HP: {HP})." | `Name`, `Tier`, `HP` |
| Boss Phase | Warning | "{Name} entering Phase {Phase}!" | `Name`, `Phase` |
| Loot Drop | Info | "{Name} dropped {Item} (Quality: {Quality})." | `Name`, `Item`, `Quality` |

---

## 13. Known Issues (v0.18 Lessons)

| Issue | Status | Priority | Notes |
|-------|--------|----------|-------|
| **Damage Variance** | Fixed | High | 5d6 reduced to 4d6 to reduce RNG swing. |
| **Soak Sponging** | Fixed | High | Soak capped at 4 (non-boss) / 6 (boss). |
| **One-Shots** | Fixed | High | Damage caps applied to Lethal tier. |
| **Low-Tier Scaling** | Planned | Medium | Need scaling formula for Legend 5+ play. |

---

## 13. Related Specifications

| Spec ID | Relationship |
|---------|--------------|
| `SPEC-COMBAT-001` | Combat Resolution System |
| `SPEC-COMBAT-015` | [Creature Traits System](creature-traits.md) — Composable trait definitions |
| `SPEC-COMBAT-016` | [Encounter Generation System](encounter-generation.md) — Spawn budget, encounter composition |
| `SPEC-COMBAT-017` | [Spawn Scaling & Difficulty](spawn-scaling.md) — TDR/PPS formulas, stat scaling |
| `SPEC-COMBAT-018` | [Boss Encounter Mechanics](boss-mechanics.md) — Multi-phase boss design |
| `SPEC-COMBAT-019` | [Elite & Champion Mechanics](elite-mechanics.md) — Elite/Champion generation |
| `SPEC-COMBAT-020` | [Impossible Encounters](impossible-encounters.md) — Threat assessment thresholds |
| `SPEC-AI-001` | Enemy AI System |
| `SPEC-ECONOMY-003` | Trauma Economy |

---

## Appendix A: Complete Enemy Statistics by Threat Tier

#### Low Tier Enemies (Legend 10-15)

| Enemy Name | HP | Damage | Attributes (M/F/Wi/Wl/St) | Soak | Special | Legend | Notes |
|------------|----|----|---------------------------|------|---------|--------|-------|
| **Corrupted Servitor** | 15 | 1d6 | 2/2/0/0/2 (6 total) | 0 | - | 10 | Baseline Low tier, aggressive AI |
| **Scrap-Hound** | 10 | 1d6 | 2/4/0/0/1 (7 total) | 0 | - | 10 | Glass cannon, high FINESSE |
| **Sludge-Crawler** | 12 | 1d6 | 2/3/0/0/1 (6 total) | 0 | Poison | 10 | Swarm enemy, poison DoT |
| **Corroded Sentry** | 15 | 1d6 | 2/1/0/0/2 (5 total) | 0 | -1 Defense | 10 | Rusted, low accuracy |
| **Maintenance Construct** | 25 | 1d6+1 | 3/2/0/0/4 (9 total) | 0 | Self-heal | 15 | Balanced, sustain threat |

#### Medium Tier Enemies (Legend 18-50)

| Enemy Name | HP | Damage | Attributes (M/F/Wi/Wl/St) | Soak | Special | Legend | Notes |
|------------|----|----|---------------------------|------|---------|--------|-------|
| **Husk Enforcer** | 25 | 1d6+2 | 3/1/0/0/3 (7 total) | 0 | - | 18 | Reanimated corpse, slow |
| **Test Subject** | 15 | 1d6+2 | 3/5/0/0/2 (10 total) | 0 | Berserk | 20 | Glass cannon, unstable |
| **Blight-Drone** | 25 | 1d6+1 | 3/3/0/0/3 (9 total) | 0 | - | 25 | Balanced DPS, ranged |
| **Arc-Welder Unit** | 30 | 2d6 | 2/3/0/0/3 (8 total) | 0 | Spark-Caster | 25 | Industrial walker, spark-blooded |
| **Corrupted Engineer** | 30 | 2d6 | 2/3/5/0/2 (12 total) | 0 | Buff allies | 30 | Support caster, high WITS |
| **Shrieker** | 35 | 1d6 | 2/2/0/4/2 (10 total) | 0 | IsForlorn | 30 | Psychic scream AoE |
| **Forlorn Scholar** | 30 | 2d6 | 2/3/4/5/2 (16 total) | 0 | IsForlorn | 35 | Caster, can negotiate |
| **Jötun-Reader Fragment** | 40 | 2d6 | 1/4/5/5/3 (18 total) | 0 | IsForlorn | 35 | AI fragment, Corruption |
| **Servitor Swarm** | 50 | 2d6 | 3/3/0/0/2 (8 total) | 0 | -2 Defense | 40 | Collective, low defense |
| **War-Frame** | 50 | 2d6 | 4/3/0/0/4 (11 total) | 0 | - | 50 | Mini-boss tier HP |

#### High Tier Enemies (Legend 55-75)

| Enemy Name | HP | Damage | Attributes (M/F/Wi/Wl/St) | Soak | Special | Legend | Notes |
|------------|----|----|---------------------------|------|---------|--------|-------|
| **Bone-Keeper** | 60 | 3d6 | 4/3/0/3/4 (14 total) | 0 | IsForlorn | 55 | Skeletal horror, armor-piercing |
| **Vault Custodian** | 70 | 2d6+2 | 5/2/0/0/5 (12 total) | 4 | Phase AI | 75 | Mini-boss, heavy armor |

#### Lethal Tier Enemies (Legend 60-100)

| Enemy Name | HP | Damage | Attributes (M/F/Wi/Wl/St) | Soak | Special | Legend | Notes |
|------------|----|----|---------------------------|------|---------|--------|-------|
| **Failure Colossus** | 80 | 3d6+4 | 6/1/0/0/6 (13 total) | 4 | - | 60 | Iron-Walker, slow |
| **Rust-Witch** | 70 | 3d6 | 2/3/4/5/3 (17 total) | 0 | IsForlorn | 75 | Symbiotic Plate caster |
| **Sentinel Prime** | 90 | 4d6 | 5/5/4/0/6 (20 total) | 6 | Phase AI | 100 | Elite military unit, tactical |

#### Boss Tier Enemies (Legend 100-150)

| Enemy Name | HP | Damage | Attributes (M/F/Wi/Wl/St) | Soak | Special | Legend | Notes |
|------------|----|----|---------------------------|------|---------|--------|-------|
| **Ruin-Warden** | 80 | 2d6 | 5/3/0/0/5 (13 total) | 0 | IsBoss | 100 | Act 1 boss, phase-based |
| **Aetheric Aberration** | 75 | 3d6 | 2/4/5/6/3 (20 total) | 0 | IsBoss, IsForlorn | 100 | Act 1 boss, magic focus |
| **Forlorn Archivist** | 80 | 3d6 | 2/4/4/7/3 (20 total) | 0 | IsBoss, IsForlorn | 150 | Act 2 boss, psychic/summoner |
| **Omega Sentinel** | 100 | 3d6+3 | 6/3/0/0/6 (15 total) | 6 | IsBoss | 150 | Act 2 boss, physical tank |

---

## Appendix B: Enemy Statistics by Archetype

#### Tank Archetype (High HP, High STURDINESS, Moderate Damage)
*Intent: Absorb damage, protect squishier allies, force long encounters.*
- **Vault Custodian**: High, 70 HP, Soak 4, 10-12 turn TTK.
- **Omega Sentinel**: Boss, 100 HP, Soak 6, 16-20 turn TTK.
- **Failure Colossus**: Lethal, 80 HP, Soak 4, 12-15 turn TTK.

#### DPS Archetype (Balanced Stats, Consistent Damage)
*Intent: Reliable threat, no gimmicks, straightforward combat.*
- **Blight-Drone**: Medium, 25 HP, 4-6 turn TTK.
- **War-Frame**: Medium, 50 HP, 6-8 turn TTK.
- **Ruin-Warden**: Boss, 80 HP, 12-15 turn TTK.

#### Glass Cannon Archetype (High FINESSE/Damage, Low HP)
*Intent: High-priority target, burst damage threat, "kill before it kills you".*
- **Scrap-Hound**: Low, 10 HP, 2-3 turn TTK.
- **Test Subject**: Medium, 15 HP, 3-4 turn TTK.

#### Support Archetype (High WITS/WILL, Buff/Debuff Focus)
*Intent: Priority kill target, force tactical decisions, multiplies ally effectiveness.*
- **Corrupted Engineer**: Medium, 30 HP, 5-6 turn TTK.

#### Swarm Archetype (Low Individual HP, Numbers-Based Threat)
*Intent: AoE testing, attrition damage, overwhelm player via numbers.*
- **Corrupted Servitor**: Low, 15 HP, 2-3 turn TTK.
- **Sludge-Crawler**: Low, 12 HP, 2-3 turn TTK.
- **Servitor Swarm**: Medium, 50 HP, 6-8 turn TTK.

#### Caster Archetype (High WILL, Psychic/Magic Damage)
*Intent: Trauma economy integration, psychic damage, high WILL resistance checks.*
- **Forlorn Scholar**: Medium, 30 HP, 5-6 turn TTK.
- **Jötun-Reader Fragment**: Medium, 40 HP, 6-7 turn TTK.
- **Rust-Witch**: Lethal, 70 HP, 10-12 turn TTK.
- **Aetheric Aberration**: Boss, 75 HP, 12-15 turn TTK.
- **Forlorn Archivist**: Boss, 80 HP, 14-18 turn TTK.

#### Mini-Boss Archetype (Phase AI, Elite Stats, Not Full Boss)
*Intent: Act-ending challenges, skill checks before bosses, "sub-boss" encounters.*
- **Vault Custodian**: High, 70 HP, Phase AI, 10-12 turn TTK.
- **Sentinel Prime**: Lethal, 90 HP, Phase AI, 14-16 turn TTK.

---

## Appendix C: Special Mechanics Reference

#### IsForlorn Enemies (Trauma Aura)
*Forces player to balance combat duration vs. trauma accumulation.*
- **Shrieker**: +3 Stress/turn, +2 Corruption on encounter.
- **Forlorn Scholar**: +3 Stress/turn, +5 Corruption on encounter.
- **Rust-Witch**: +5 Stress/turn, +10 Corruption on encounter.
- **Forlorn Archivist**: +8 Stress/turn, +12 Corruption on encounter.

#### IsBoss Enemies (Multi-Phase Combat)
- **Ruin-Warden**: Phase 1 (60% Attack), Phase 2 (Berserk).
- **Aetheric Aberration**: Phase 1 (Aetheric Bolt), Phase 2 (Psychic Scream), Phase 3 (Void Collapse).
- **Forlorn Archivist**: Phase 1 (Mind Spike), Phase 2 (Summon), Phase 3 (Desperation).
- **Omega Sentinel**: Phase 1 (Railgun), Phase 2 (Missile Barrage), Phase 3 (Overcharge).

#### Soak Mechanics (Flat Damage Reduction)
*Cap Rule: Non-Boss Max 4, Boss Max 6.*
- **Vault Custodian**: Soak 4 (40% reduction vs 10 dmg).
- **Sentinel Prime**: Soak 6 (60% reduction vs 10 dmg).

---

## Appendix D: v0.18 Balance Adjustment Summary

| Enemy Name | Property | Before v0.18 | After v0.18 | Reason |
|------------|----------|--------------|-------------|--------|
| **Aetheric Aberration** | HP | 60 | 75 | Match 100 Legend value |
| **Vault Custodian** | Soak | 6 | 4 | Prevent bullet sponge |
| **Omega Sentinel** | Soak | 8 | 6 | Prevent 25+ turn combat |
| **Failure Colossus** | Damage | 4d6+3 | 3d6+4 | Prevent one-shots |
| **Sentinel Prime** | Damage | 5d6 | 4d6 | Reduce variance |
| **Corroded Sentry** | Legend | 5 | 10 | Match HP investment |

---

## 14. Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2025-11-21 | Initial consolidated specification (v0.18 balance adjustments included) |
