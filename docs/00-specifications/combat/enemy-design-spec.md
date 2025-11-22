# Enemy Design & Bestiary System Specification

> **Specification ID**: SPEC-COMBAT-012
> **Status**: Draft
> **Domain**: Combat
> **Related Systems**: Combat Resolution, Damage Calculation, Loot System, Trauma Economy

---

## Document Control

| Property | Value |
|----------|-------|
| **Specification ID** | SPEC-COMBAT-012 |
| **Title** | Enemy Design & Bestiary System |
| **Version** | 1.0.0 |
| **Status** | Draft |
| **Last Updated** | 2025-11-21 |
| **Author** | AI Specification Agent |
| **Reviewers** | Game Design, Balance Team, Implementation Team |
| **Stakeholders** | Economy Designer (loot integration), Narrative Designer (trauma/lore), Combat Designer (balance) |

---

## Executive Summary

### Purpose

The **Enemy Design & Bestiary System** provides the design framework for creating balanced, thematically appropriate enemies across all threat levels in Rune & Rust. This specification formalizes:

1. **Stat Budget Formulas** - How HP, attributes, damage, and defense scale across 5 threat tiers
2. **Enemy Archetypes** - 8 tactical roles (Tank, DPS, Glass Cannon, Support, Swarm, Caster, Mini-Boss, Boss)
3. **AI Behavior Patterns** - Probability-based decision-making for 60+ enemy actions
4. **Trauma Integration** - Stress and Corruption infliction guidelines for horror atmosphere
5. **Scaling Systems** - How enemies adapt to player Legend/progression

### Scope

**In Scope**:
- Stat budget formulas for 5 threat tiers (Low, Medium, High, Lethal, Boss)
- 8 enemy archetype definitions with tactical roles
- AI behavior taxonomy and decision-making patterns
- Special mechanics (Soak, IsForlorn, IsBoss, IsChampion flags)
- Loot table integration (quality tier mapping)
- Trauma Economy integration (Stress/Corruption infliction)
- Enemy scaling formulas vs. player Legend
- Balance targets (time-to-kill, damage output, survivability)
- v0.18 balance adjustments and design rationale

**Out of Scope**:
- Individual enemy bestiary entries (see `/docs/templates/enemy-bestiary-entry.md`)
- Procedural enemy spawning algorithms → SPEC-WORLD-001
- Boss encounter phase mechanics → SPEC-COMBAT-005
- AI pathfinding and tactical positioning → SPEC-COMBAT-009
- Specific ability implementations (documented per enemy in bestiary)

### Success Criteria

This specification is successful if:

1. **Designers can create balanced enemies** - New enemies fit threat tier budgets without playtesting
2. **Players perceive threat correctly** - Low enemies feel weak, Bosses feel epic
3. **Damage output is balanced** - Time-to-kill targets met (Low: 2-3 turns, Boss: 8-12 turns)
4. **Enemies feel distinct** - Each archetype has clear tactical identity
5. **Trauma atmosphere maintained** - Forlorn enemies create horror tension without frustration

---

## Related Documentation

### Dependencies

**Completed Specifications**:
- **[SPEC-COMBAT-001](../combat/combat-resolution-spec.md)**: Combat Resolution System → Turn sequence, initiative
- **[SPEC-COMBAT-002](../combat/damage-calculation-spec.md)**: Damage Calculation System → Damage formulas, mitigation
- **[SPEC-COMBAT-003](../combat/status-effects-spec.md)**: Status Effects System → Debuff application
- **[SPEC-ECONOMY-001](../economy/loot-equipment-spec.md)**: Loot & Equipment System → Drop quality tiers
- **[SPEC-ECONOMY-003](../economy/trauma-economy-spec.md)**: Trauma Economy System → Stress/Corruption infliction
- **[SPEC-PROGRESSION-001](../progression/character-progression-spec.md)**: Character Progression → Legend scaling

**Planned Specifications**:
- SPEC-COMBAT-005 (Boss Encounter System) - Phase mechanics, enrage timers
- SPEC-COMBAT-009 (Movement & Positioning) - Tactical AI positioning
- SPEC-AI-001 (Enemy AI System) - Comprehensive AI behavior documentation

### Code References

**Core Implementation Files**:
- `RuneAndRust.Core/Enemy.cs` (lines 1-123) - Enemy data model with 29+ enemy types
- `RuneAndRust.Engine/EnemyFactory.cs` (lines 1-599) - Enemy creation with hardcoded stats
- `RuneAndRust.Engine/EnemyAI.cs` (lines 1-150+) - AI decision-making logic
- `RuneAndRust.Core/Population/DormantProcess.cs` (lines 27-34) - ThreatLevel enum

**Related Services**:
- `RuneAndRust.Engine/LootService.cs` - Enemy loot table generation
- `RuneAndRust.Engine/BossEncounterService.cs` - Boss-specific mechanics

### Layer 1 Documentation

- `/docs/templates/enemy-bestiary-entry.md` - Bestiary entry template (not yet used)

---

## Design Philosophy

The Enemy Design & Bestiary System is built on 4 core design principles:

### 1. **Readable Threat Levels**

**Principle**: Players should instantly recognize enemy threat through stat patterns, not trial-and-error deaths.

**Implementation**:
- **HP ranges** create visual threat cues (10-15 HP = Low, 80-100 HP = Boss)
- **Consistent damage scaling** (1d6 → 2d6 → 3d6 → 4d6) telegraphs lethality
- **ThreatLevel classification** (Minimal/Low/Medium/High/Boss) informs spawning
- **Archetype visual design** (Tank = high STURDINESS, Glass Cannon = high damage/low HP)

**Example**:
- **Corrupted Servitor** (15 HP, 1d6 damage) feels like a minion
- **Ruin-Warden** (80 HP, 2d6 damage, IsBoss) feels like a climactic threat

### 2. **Tactical Diversity Through Archetypes**

**Principle**: Enemies should force different tactical responses, not just "hit until dead."

**Implementation**:
- **8 Archetypes** with distinct roles (Tank, DPS, Glass Cannon, Support, Swarm, Caster, Mini-Boss, Boss)
- **Special mechanics** (Soak for Tanks, Forlorn aura for Casters, self-healing for Support)
- **AI behavior patterns** (Aggressive DPS vs. Defensive Tanks vs. Tactical Support)
- **Attribute specialization** (MIGHT for melee, WILL for casters, FINESSE for evasion)

**Example**:
- **Vault Custodian** (Soak 4, self-healing) requires armor-piercing or DoT strategies
- **Forlorn Scholar** (WILL 5, psychic damage) punishes low-WILL builds

### 3. **Balanced Power Budget**

**Principle**: Enemy power should scale predictably with threat tier, avoiding surprise one-shots.

**Implementation**:
- **Stat point budgets** (Low: 5-10 points, Boss: 13-20 points) constrain attribute allocation
- **HP-to-Legend ratio** (0.5-1.0 Legend per HP) ensures XP rewards match difficulty
- **Damage caps per tier** (v0.18 balance: max 4d6 for Lethal, prevent 5d6 one-shots)
- **Time-to-kill targets** (Low: 2-3 turns, Boss: 8-12 turns solo)

**Balance Example** (v0.18 adjustments):
- **Failure Colossus**: Reduced from 4d6+3 (avg 17) to 3d6+4 (avg 14.5) to prevent one-shot deaths
- **Sentinel Prime**: Reduced from 5d6 (avg 17.5) to 4d6 (avg 14) for same reason

### 4. **Horror Atmosphere Through Trauma**

**Principle**: Enemies should evoke dread through psychological mechanics, not just difficulty.

**Implementation**:
- **IsForlorn flag** - Enemies inflict passive Stress/Corruption auras (Forlorn Scholar, Shrieker, Bone-Keeper)
- **Psychic damage** - Ignores armor, targets WILL (Forlorn Archivist, Aetheric Aberration)
- **Body horror** - Symbiotic Plate enemies (Husk Enforcer, Bone-Keeper) trigger visceral reactions
- **Corruption sources** - Jötun-Reader Fragment inflicts Corruption, creating long-term consequences

**Atmospheric Design**:
- **Forlorn enemies** create tension even when not engaged (aura effects)
- **Boss encounters** have elevated Stress infliction (room auras + enemy actions)
- **Trauma integration** makes combat psychologically costly, not just HP/resource management

---

## Functional Requirements

### FR-001: Threat Level Classification System

**Description**: All enemies must be classified into one of 5 threat tiers that determine stat budgets, loot quality, and spawn frequency.

**Rationale**: Clear threat classification enables:
1. Procedural generation to spawn appropriate difficulty encounters
2. Designers to balance new enemies against existing benchmarks
3. Players to assess risk/reward of engagements
4. Loot system to assign quality tier drops (SPEC-ECONOMY-001)

**Threat Tier Definitions** (from actual implementation):

| Threat Tier | HP Range | Attribute Budget | Damage Range | Legend Value | Example Enemies |
|-------------|----------|------------------|--------------|--------------|-----------------|
| **Low** | 10-15 HP | 5-10 attr points | 1d6 to 1d6+2 | 10-20 Legend | Corrupted Servitor, Scrap-Hound, Corroded Sentry |
| **Medium** | 25-50 HP | 8-16 attr points | 1d6+1 to 2d6 | 15-50 Legend | Blight-Drone, Forlorn Scholar, War-Frame |
| **High** | 60-70 HP | 12-17 attr points | 2d6+2 to 3d6 | 55-75 Legend | Bone-Keeper, Vault Custodian, Rust-Witch |
| **Lethal** | 80-90 HP | 13-20 attr points | 3d6+4 to 4d6 | 60-100 Legend | Failure Colossus, Sentinel Prime |
| **Boss** | 75-100 HP | 13-20 attr points | 2d6 to 3d6+3 | 100-150 Legend | Ruin-Warden, Aetheric Aberration, Omega Sentinel |

**Acceptance Criteria**:
- [ ] Every enemy type has ThreatLevel assigned in DormantProcess
- [ ] HP, damage, and Legend values fall within tier ranges
- [ ] Loot quality mapping documented (Low → T0-T1, Boss → T3-T4)
- [ ] Procedural spawning respects threat tier distribution

**Example** (from `EnemyFactory.cs:47-66`):
```csharp
// Low Tier Enemy: Corrupted Servitor
MaxHP = 15,
Attributes = new Attributes(might: 2, finesse: 2, wits: 0, will: 0, sturdiness: 2), // 6 attr points
BaseDamageDice = 1, DamageBonus = 0, // 1d6 damage
BaseLegendValue = 10 // Low tier reward
```

**Dependencies**:
- SPEC-ECONOMY-001 (Loot & Equipment) - Loot quality tier mapping
- SPEC-WORLD-001 (Procedural Generation) - Spawn difficulty curves

---

### FR-002: Stat Budget Allocation by Threat Tier

**Description**: Enemy stats (HP, attributes, damage) must conform to budget formulas per threat tier to ensure balanced difficulty scaling.

**Rationale**: Stat budgets prevent:
1. **Power creep** - New enemies don't invalidate old balance
2. **One-shot deaths** - Damage caps prevent unfair instant kills (v0.18 lesson)
3. **Bullet sponges** - HP caps ensure reasonable time-to-kill
4. **Attribute bloat** - Point budgets force meaningful specialization

**Stat Budget Formulas** (derived from 20 implemented enemies):

**HP Budget**:
```
Low:    HP = 10-15 (avg 12.5)
Medium: HP = 25-50 (avg 37.5)
High:   HP = 60-70 (avg 65)
Lethal: HP = 80-90 (avg 85)
Boss:   HP = 75-100 (avg 87.5)
```

**Attribute Point Budget**:
```
Low:    Total = 5-10 attr points (avg 7.5)
Medium: Total = 8-16 attr points (avg 12)
High:   Total = 12-17 attr points (avg 14.5)
Lethal: Total = 13-20 attr points (avg 16.5)
Boss:   Total = 13-20 attr points (avg 17)
```

**Damage Budget**:
```
Low:    1d6 to 1d6+2 (avg 3.5-5.5 damage)
Medium: 1d6+1 to 2d6 (avg 4.5-7 damage)
High:   2d6+2 to 3d6 (avg 9-10.5 damage)
Lethal: 3d6+4 to 4d6 (avg 14.5-14 damage, capped to prevent one-shots)
Boss:   2d6 to 3d6+3 (avg 7-13.5 damage, varies by boss role)
```

**v0.18 Balance Adjustments** (damage caps):
- **Failure Colossus**: Reduced from 4d6+3 (avg 17) → 3d6+4 (avg 14.5) to prevent one-shot deaths
- **Sentinel Prime**: Reduced from 5d6 (avg 17.5) → 4d6 (avg 14) for same reason
- **Max damage cap**: Lethal tier capped at ~14-15 avg damage (enough to 2-shot low-HP characters, not 1-shot)

**Legend Value Formula**:
```
Legend = HP × (0.5 to 1.0) + Bonus for special mechanics

Where:
  0.5 ratio = Low tier minions (Corrupted Servitor: 15 HP × 0.67 = 10 Legend)
  1.0 ratio = High tier elites (Bone-Keeper: 60 HP × 0.92 = 55 Legend)
  Bonus = +10-20 for self-healing, +10-25 for support abilities, +20-50 for boss mechanics
```

**Acceptance Criteria**:
- [ ] All enemies fit within tier HP/attribute/damage budgets
- [ ] Damage caps enforced (max 4d6 for Lethal, 3d6+3 for most Bosses)
- [ ] Legend values correlate with actual difficulty (0.5-1.0 Legend/HP ratio)
- [ ] Balance testing confirms time-to-kill targets met

**Example Stat Allocations**:

**Scrap-Hound (Low Tier, Glass Cannon)**:
```csharp
MaxHP = 10,  // Low HP (glass cannon)
Attributes = new Attributes(might: 2, finesse: 4, wits: 0, will: 0, sturdiness: 1), // 7 points, FINESSE focus
BaseDamageDice = 1, DamageBonus = 0, // 1d6 damage
BaseLegendValue = 10
```

**Rust-Witch (High Tier, Caster)**:
```csharp
MaxHP = 70, // High HP
Attributes = new Attributes(might: 2, finesse: 3, wits: 4, will: 5, sturdiness: 3), // 17 points, WILL/WITS focus
BaseDamageDice = 3, DamageBonus = 0, // 3d6 damage (psychic)
IsForlorn = true, // Special trauma mechanic
BaseLegendValue = 75 // 1.07 Legend/HP ratio (above average for trauma infliction)
```

**Dependencies**:
- SPEC-COMBAT-002 (Damage Calculation) - Damage formula integration
- SPEC-PROGRESSION-001 (Character Progression) - Player power scaling context

---

### FR-003: Enemy Archetype Taxonomy

**Description**: All enemies must belong to one of 8 archetypes that define tactical role, stat distribution, and AI behavior patterns.

**Rationale**: Archetypes ensure:
1. **Tactical diversity** - Players face varied combat challenges
2. **Design consistency** - New enemies fit established patterns
3. **AI predictability** - Players learn archetype behaviors
4. **Encounter composition** - Procedural spawning creates synergistic groups

**8 Enemy Archetypes** (derived from implementation):

#### 1. **Tank** (High HP, High Defense, Low Damage)
**Role**: Absorb damage, protect allies, force player attention
**Stat Profile**: High STURDINESS (4-6), High HP (60-100), Soak 4-6, Low-Medium Damage (2d6-3d6)
**Examples**:
- **Vault Custodian**: 70 HP, STURDINESS 5, Soak 4, 2d6+2 damage
- **Omega Sentinel**: 100 HP, STURDINESS 6, Soak 6, 3d6+3 damage
**AI Pattern**: Defensive (50% BasicAttack, 30% DefensiveStance, 20% self-heal)

#### 2. **DPS** (Medium HP, High Damage, Balanced Stats)
**Role**: Consistent damage output, balanced threat
**Stat Profile**: Balanced attributes (8-12 points), Medium HP (25-50), High Damage (1d6+2 to 2d6)
**Examples**:
- **Blight-Drone**: 25 HP, 3/3/0/0/3 attributes, 1d6+1 damage
- **War-Frame**: 50 HP, 4/3/0/0/4 attributes, 2d6 damage
**AI Pattern**: Aggressive (70% BasicAttack, 20% HeavyStrike, 10% utility)

#### 3. **Glass Cannon** (Low HP, Very High Damage)
**Role**: High-priority threat, must be eliminated quickly
**Stat Profile**: High FINESSE or MIGHT (4-5), Low HP (10-20), High Damage (1d6+2 to 2d6)
**Examples**:
- **Scrap-Hound**: 10 HP, FINESSE 4, 1d6 damage (attacks twice with QuickBite)
- **Test Subject**: 15 HP, FINESSE 5, 1d6+2 damage
**AI Pattern**: Aggressive (80% offensive actions, flee at low HP)

#### 4. **Support** (Medium HP, Buff/Debuff Focused)
**Role**: Enhance allies, disrupt players, create tactical complications
**Stat Profile**: High WITS (4-5), Medium HP (25-40), Low Direct Damage (1d6-2d6)
**Examples**:
- **Corrupted Engineer**: 30 HP, WITS 5, 2d6 damage, OverchargeAlly / EmergencyRepairAlly abilities
**AI Pattern**: Tactical (40% BasicAttack, 40% Buff Ally, 20% Heal Ally)

#### 5. **Swarm** (Medium-High HP Pool, Low Individual Threat, Numbers-Based)
**Role**: Overwhelm through quantity, create positioning challenges
**Stat Profile**: Low-Medium attributes (8 points), High collective HP (50), Moderate Damage (2d6), Defense penalty
**Examples**:
- **Servitor Swarm**: 50 HP, 3/3/0/0/2 attributes, 2d6 damage, DefenseBonus -2
**AI Pattern**: Aggressive Swarm (90% BasicAttack, flanking focus)

#### 6. **Caster** (Medium HP, Psychic/Magic Damage, High WILL)
**Role**: Ranged magic attacks, ignore armor, inflict status effects/trauma
**Stat Profile**: High WILL (4-7), High WITS (4-5), Medium HP (30-70), Psychic Damage (2d6-3d6)
**Examples**:
- **Forlorn Scholar**: 30 HP, WILL 5, WITS 4, 2d6 psychic damage, IsForlorn
- **Rust-Witch**: 70 HP, WILL 5, WITS 4, 3d6 damage, IsForlorn, Corruption aura
- **Forlorn Archivist** (Boss): 80 HP, WILL 7, 3d6 psychic damage, summons adds
**AI Pattern**: Ranged Caster (60% magic attacks, 20% debuffs, 20% summons/utility)

#### 7. **Mini-Boss** (High HP, Phase Mechanics, Elite Abilities)
**Role**: Mid-dungeon challenge, test player build, reward significant loot
**Stat Profile**: High HP (50-70), High attributes (11-15 points), Phase-based AI, Special abilities
**Examples**:
- **Vault Custodian**: 70 HP, Phase 1-2, Guardian Protocol self-heal, Whirlwind Strike AOE
**AI Pattern**: Phase-based (Phase 1: defensive, Phase 2: aggressive AOE)

#### 8. **Boss** (Very High HP, Multi-Phase, Ultimate Abilities)
**Role**: Climactic encounter, multiple phases, unique mechanics
**Stat Profile**: 75-100 HP, 13-20 attr points, 3-phase combat, special effects
**Examples**:
- **Ruin-Warden**: 80 HP, MIGHT 5, STURDINESS 5, 2d6 damage, IsBoss
- **Aetheric Aberration**: 75 HP, WILL 6, 3d6 damage, summons echoes, IsForlorn
- **Omega Sentinel**: 100 HP, MIGHT 6, STURDINESS 6, 3d6+3 damage, Soak 6, 3 phases
**AI Pattern**: Phase-based progression (Phase 1: testing, Phase 2: adds/mechanics, Phase 3: desperation ultimate)

**Acceptance Criteria**:
- [ ] Every enemy type tagged with archetype classification
- [ ] Archetype stat profiles documented with ranges
- [ ] AI behavior patterns mapped to archetypes
- [ ] Encounter composition guidelines use archetype taxonomy

**Dependencies**:
- SPEC-COMBAT-005 (Boss Encounters) - Boss phase mechanics
- SPEC-AI-001 (Enemy AI) - AI behavior pattern documentation

---

