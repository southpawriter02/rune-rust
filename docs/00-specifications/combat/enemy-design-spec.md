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

### FR-004: Special Mechanics Flags

**Description**: Enemies must support special mechanics flags (IsForlorn, IsBoss, IsChampion, Soak) that modify combat behavior and create tactical variety.

**Rationale**: Special mechanics:
1. **Create memorable encounters** - Forlorn enemies feel distinct through trauma aura
2. **Enable build variety** - Soak/armor requires armor-piercing strategies
3. **Support procedural variation** - Champion variants add surprise to familiar enemies
4. **Integrate horror themes** - IsForlorn connects combat to trauma economy

**Special Mechanics** (from `Enemy.cs`):

#### IsForlorn (Trauma Aura)
**Purpose**: Inflicts passive Psychic Stress/Corruption aura, creating psychological horror tension
**Stat Impact**: +10-20% Legend value bonus (inflicting trauma increases difficulty)
**Examples**:
- **Forlorn Scholar**: IsForlorn = true, inflicts 5 Stress/turn proximity aura
- **Shrieker**: IsForlorn = true, psychic scream AOE + Stress
- **Forlorn Archivist** (Boss): IsForlorn = true, 5 Stress/turn on top of room's Heavy Psychic Resonance

**Design Guidelines**:
- Use for horror-themed enemies (Symbiotic Plate, corrupted humans, psychic entities)
- Balance trauma infliction with combat difficulty (high trauma = avoid if possible, creating player choice)
- Limit IsForlorn to ~30% of enemy roster (overuse dilutes horror atmosphere)

#### IsBoss (Multi-Phase Combat)
**Purpose**: Enables phase-based AI, special abilities, guaranteed high-tier loot
**Stat Impact**: 75-100 HP, 13-20 attr points, 100-150 Legend value
**Examples**:
- **Ruin-Warden**: IsBoss = true, Phase 1 (1-40 HP threshold transitions)
- **Aetheric Aberration**: IsBoss = true, summons echoes, phase transitions at HP thresholds
- **Omega Sentinel**: IsBoss = true, 3-phase combat (PowerDraw → OverchargedMaul → OmegaProtocol)

**Design Guidelines**:
- Bosses have 2-3 distinct phases with different AI priorities
- Phase transitions at HP thresholds (Phase 2 at 50% HP, Phase 3 at 25% HP typical)
- Each phase introduces new mechanic or escalates existing ones

#### IsChampion (Elite Variants)
**Purpose**: Procedurally create elite versions of standard enemies (+50% stats, unique ability)
**Stat Impact**: +50% HP, +1-2 attribute points, +25% damage, +20% Legend value
**Examples** (from `DormantProcess.cs`):
- **Champion Servitor**: 22 HP (15 × 1.5), attributes 3/3/0/0/3, 1d6+1 damage, 15 Legend
- **Champion Drone**: 37 HP (25 × 1.5), attributes 4/4/0/0/4, 1d6+2 damage, 37 Legend

**Design Guidelines**:
- Champion spawn rate: 10-15% for Low/Medium, 20% for High tier enemies
- Champions get unique ability from enemy's ability pool (e.g., Champion Servitor gains RapidStrike)
- Visual distinction required (descriptors, icons, color coding)

#### Soak (Flat Damage Reduction)
**Purpose**: Armor/damage reduction mechanic, forces armor-piercing or DoT strategies
**Stat Impact**: High HP + Soak = significantly longer TTK (requires balance testing)
**Examples**:
- **Vault Custodian**: Soak 4 (reduced from 6 in v0.18 to prevent excessive tankiness)
- **Omega Sentinel**: Soak 6 (reduced from 8 in v0.18 for same reason)
- **Failure Colossus**: Soak 4 (heavy armor)

**v0.18 Balance Lessons**:
- **Vault Custodian Soak reduced 6 → 4**: Original Soak 6 made 70 HP feel like 120+ HP effective (frustrating)
- **Omega Sentinel Soak reduced 8 → 6**: Soak 8 created 5+ minute boss fights (too slow)
- **Soak cap**: Max Soak 6 for bosses, max Soak 4 for non-bosses

**Acceptance Criteria**:
- [ ] IsForlorn enemies inflict documented Stress/Corruption values
- [ ] IsBoss enemies have phase transitions implemented
- [ ] IsChampion variants spawn at configured rates (10-20%)
- [ ] Soak values capped per tier (non-boss max 4, boss max 6)

**Example** (from `EnemyFactory.cs:297-318`):
```csharp
// Vault Custodian: Mini-Boss with Soak and Phase mechanics
MaxHP = 70,
Soak = 4, // v0.18: Reduced from 6 to prevent excessive tankiness
IsBoss = false, // Mini-boss, not full boss
Phase = 1, // Phase-based AI
BaseLegendValue = 75 // Moderate-High Act (mini-boss)
```

**Dependencies**:
- SPEC-ECONOMY-003 (Trauma Economy) - Stress/Corruption infliction mechanics
- SPEC-COMBAT-005 (Boss Encounters) - Phase transition triggers

---

### FR-005: AI Behavior Decision-Making

**Description**: Enemy AI must use probability-based decision trees that vary by archetype and create predictable but tactically interesting combat patterns.

**Rationale**: Probability-based AI:
1. **Creates consistency** - Players learn enemy patterns through repeated encounters
2. **Prevents exploitability** - Randomness prevents perfect player counter-strategies
3. **Supports archetype identity** - Aggressive enemies attack 80%+, Defensive enemies use utility 50%
4. **Enables phase-based progression** - Bosses change probabilities per phase

**AI Decision Pattern** (from `EnemyAI.cs`):

```csharp
public EnemyAction DetermineAction(Enemy enemy)
{
    // Skip turn if stunned
    if (enemy.IsStunned) return EnemyAction.BasicAttack; // Will be handled specially

    // Route to enemy-specific AI logic
    return enemy.Type switch
    {
        EnemyType.CorruptedServitor => DetermineServitorAction(),
        EnemyType.BlightDrone => DetermineDroneAction(),
        EnemyType.RuinWarden => DetermineWardenAction(enemy), // HP-based phase logic
        // ... 20+ enemy types
    };
}
```

**AI Archetype Patterns** (probability distributions):

#### Aggressive (Glass Cannon, DPS, Swarm)
```csharp
private EnemyAction DetermineServitorAction()
{
    var roll = _random.Next(100);

    if (roll < 80) return EnemyAction.BasicAttack; // 80% attack
    else return EnemyAction.Defend; // 20% defend
}
```
**Pattern**: 70-90% offensive actions, 10-30% defensive/utility

#### Defensive (Tank)
```csharp
private EnemyAction DetermineVaultCustodianAction(Enemy enemy)
{
    if (enemy.HP < enemy.MaxHP * 0.3) // Low HP
        return EnemyAction.GuardianProtocol; // Self-heal priority

    var roll = _random.Next(100);
    if (roll < 40) return EnemyAction.HalberdSweep; // 40% attack
    else if (roll < 70) return EnemyAction.DefensiveStance; // 30% defense buff
    else return EnemyAction.GuardianProtocol; // 30% self-heal
}
```
**Pattern**: 40-50% attacks, 30-40% defense, 20-30% self-heal

#### Tactical (Support, Caster)
```csharp
private EnemyAction DetermineCorruptedEngineerAction(Enemy enemy)
{
    var roll = _random.Next(100);

    if (roll < 40) return EnemyAction.ArcDischarge; // 40% attack
    else if (roll < 70) return EnemyAction.OverchargeAlly; // 30% buff ally
    else return EnemyAction.EmergencyRepairAlly; // 30% heal ally
}
```
**Pattern**: 30-50% attacks, 30-40% buffs/debuffs, 20-30% heals/summons

#### Phase-Based (Boss, Mini-Boss)
```csharp
private EnemyAction DetermineWardenAction(Enemy enemy)
{
    // Phase 1 (HP > 50%): Defensive testing
    if (enemy.HP > enemy.MaxHP * 0.5)
    {
        var roll = _random.Next(100);
        if (roll < 60) return EnemyAction.BasicAttack; // 60% basic
        else return EnemyAction.HeavyStrike; // 40% heavy
    }
    // Phase 2 (HP ≤ 50%): Aggressive escalation
    else
    {
        var roll = _random.Next(100);
        if (roll < 40) return EnemyAction.BerserkStrike; // 40% berserk (high damage)
        else if (roll < 70) return EnemyAction.HeavyStrike; // 30% heavy
        else return EnemyAction.ChargeDefense; // 30% charge (telegraphed attack)
    }
}
```
**Pattern**: Phase transitions at HP thresholds change probability distributions

**Acceptance Criteria**:
- [ ] AI decision logic uses Random.Next(100) probability rolls
- [ ] Each enemy type has documented probability distribution
- [ ] Archetype patterns consistent across similar enemies
- [ ] Phase-based enemies modify distributions at HP thresholds

**Dependencies**:
- SPEC-AI-001 (Enemy AI System) - Comprehensive AI behavior documentation
- SPEC-COMBAT-005 (Boss Encounters) - Phase transition mechanics

---

### FR-006: Enemy Scaling vs. Player Progression

**Description**: Enemy stats must scale with player Legend/level to maintain challenge across progression, without creating bullet sponges or one-shot deaths.

**Rationale**: Scaling ensures:
1. **Consistent challenge** - Early enemies stay relevant in late-game encounters
2. **Rewards progression** - Player power growth feels meaningful
3. **Avoids trivialization** - Level 1 enemies don't become irrelevant at Level 10
4. **Maintains TTK targets** - Time-to-kill stays in 2-12 turn range

**Scaling Formula** (proposed, based on existing Legend system):

**HP Scaling**:
```
Scaled_HP = Base_HP + (Player_Legend × HP_Scaling_Factor)

Where:
  HP_Scaling_Factor varies by threat tier:
    Low:    0.5 HP per Legend (Servitor: 15 + 5 Legend × 0.5 = 17.5 HP at Legend 5)
    Medium: 1.0 HP per Legend (Drone: 25 + 5 × 1.0 = 30 HP at Legend 5)
    High:   1.5 HP per Legend (Bone-Keeper: 60 + 5 × 1.5 = 67.5 HP at Legend 5)
    Boss:   2.0 HP per Legend (Ruin-Warden: 80 + 5 × 2.0 = 90 HP at Legend 5)
```

**Damage Scaling**:
```
Scaled_Damage = Base_Damage_Dice + (Player_Legend ÷ 3)d6

Where:
  Damage dice increase every 3 Legend levels:
    Legend 1-2: Base damage (Servitor: 1d6)
    Legend 3-5: +1d6 (Servitor: 2d6)
    Legend 6-8: +2d6 (Servitor: 3d6)
    Legend 9+:  +3d6 (Servitor: 4d6)
```

**Attribute Scaling**:
```
Scaled_Attribute = Base_Attribute + (Player_Legend ÷ 5)

Where:
  Attributes increase every 5 Legend levels:
    Legend 1-4: Base attributes (Servitor: 2/2/0/0/2)
    Legend 5-9: +1 all attributes (Servitor: 3/3/0/0/3)
    Legend 10+: +2 all attributes (Servitor: 4/4/0/0/4)
```

**Example Scaling** (Corrupted Servitor at Legend 1 vs. Legend 10):

| Stat | Legend 1 (Base) | Legend 5 | Legend 10 |
|------|----------------|----------|-----------|
| HP | 15 | 17.5 | 20 |
| Damage | 1d6 (avg 3.5) | 2d6 (avg 7) | 3d6 (avg 10.5) |
| Attributes | 2/2/0/0/2 | 3/3/0/0/3 | 4/4/0/0/4 |
| Legend Value | 10 | 15 | 20 |

**Balance Targets** (time-to-kill):

| Enemy Tier | Solo Player TTK | 2-Player Party TTK |
|------------|-----------------|-------------------|
| Low | 2-3 turns | 1-2 turns |
| Medium | 3-5 turns | 2-3 turns |
| High | 5-8 turns | 3-5 turns |
| Lethal | 7-10 turns | 4-6 turns |
| Boss | 10-15 turns | 6-10 turns |

**Acceptance Criteria**:
- [ ] Scaling formulas implemented per threat tier
- [ ] TTK targets validated through playtesting
- [ ] Damage scaling doesn't create one-shot deaths (max 50% player HP per hit)
- [ ] HP scaling doesn't create bullet sponges (TTK stays within targets)

**Note**: This FR proposes scaling formulas not currently implemented. Current implementation uses static stats from `EnemyFactory.cs`. Implementation would require adding scaling logic to enemy instantiation.

**Dependencies**:
- SPEC-PROGRESSION-001 (Character Progression) - Legend/XP scaling context
- SPEC-COMBAT-002 (Damage Calculation) - Damage formula integration

---

### FR-007: Loot Table Integration

**Description**: Enemy threat tiers must map to equipment quality tiers to ensure loot rewards match difficulty.

**Rationale**: Loot-difficulty correlation ensures:
1. **Risk-reward balance** - Harder enemies drop better loot
2. **Progression pacing** - Players acquire Tier 2-3 gear from appropriate enemies
3. **Boss excitement** - 70% Tier 4 drop rate makes boss kills rewarding
4. **Vendor trash filtering** - Low enemies drop T0-T1, preventing inventory bloat with worthless items

**Enemy-to-Loot Quality Mapping** (from `LootService.cs` and SPEC-ECONOMY-001):

| Enemy Threat Tier | Primary Loot Quality | Secondary Loot Quality | No Drop Rate |
|-------------------|---------------------|----------------------|--------------|
| **Low** | Tier 0 (60%) | Tier 1 (30%) | 10% |
| **Medium** | Tier 1 (40%) | Tier 2 (40%), Tier 3 (20%) | 0% |
| **High** | Tier 2 (40%) | Tier 3 (40%), Tier 4 (20%) | 0% |
| **Lethal** | Tier 3 (50%) | Tier 4 (50%) | 0% |
| **Boss** | Tier 4 (70%) | Tier 3 (30%) | 0% |

**Loot Generation Logic** (from `LootService.cs:22-43`):
```csharp
public Equipment? GenerateLoot(Enemy enemy, PlayerCharacter? player)
{
    return enemy.Type switch
    {
        EnemyType.CorruptedServitor => GenerateServitorLoot(player),  // Low tier
        EnemyType.BlightDrone => GenerateDroneLoot(player),           // Medium tier
        EnemyType.RuinWarden => GenerateBossLoot(player),             // Boss tier
        _ => null
    };
}
```

**Acceptance Criteria**:
- [ ] Enemy threat tier → loot quality mapping documented
- [ ] Boss enemies guarantee 70% Tier 4 drops
- [ ] Low enemies have 10% no-drop rate to prevent inventory spam
- [ ] Class-appropriate filtering applied (60% for standard, 100% for bosses)

**Example** (Boss loot value):
- **Ruin-Warden** (Boss, 100 Legend): 70% Tier 4 weapon/armor (avg value ~1500 scrap equivalent)
- **Risk-Reward**: 10-15 turn boss fight yields best equipment in game

**Dependencies**:
- SPEC-ECONOMY-001 (Loot & Equipment System) - Quality tier definitions, drop rate formulas

---

### FR-008: Trauma Economy Integration

**Description**: Forlorn enemies and specific enemy actions must inflict Psychic Stress and Corruption according to trauma economy rules.

**Rationale**: Trauma integration creates:
1. **Horror atmosphere** - Combat has psychological consequences beyond HP loss
2. **Risk-reward choices** - Players may flee Forlorn enemies despite good loot
3. **Build diversity** - High-WILL builds resist trauma better
4. **Long-term consequences** - Corruption accumulation affects future runs

**Trauma Infliction Rules** (integration with SPEC-ECONOMY-003):

#### Psychic Stress Sources
**On Encounter**:
- Non-Forlorn enemies: +0 Stress (normal combat)
- Forlorn enemies: +5 Stress on first sight (Forlorn Scholar, Shrieker)
- Boss enemies: +10 Stress on encounter (Forlorn Archivist, Aetheric Aberration)

**Proximity Aura** (per turn):
- Forlorn enemies within 2 tiles: +3 Stress/turn
- Forlorn bosses: +5 Stress/turn

**On Attack Received**:
- Physical attacks: +0 Stress
- Psychic attacks: +2 Stress (Forlorn Scholar's AethericBolt)
- Traumatic abilities: +5 Stress (Forlorn Archivist's PsychicScream)

**On Death** (enemy killed by player):
- Normal enemies: +0 Stress
- Forlorn humanoids: +3 Stress (killing corrupted humans)
- Body horror enemies: +5 Stress (Bone-Keeper, Husk Enforcer)

#### Corruption Sources
**Jötun-Reader Enemies**:
- **Jötun-Reader Fragment**: +2 Corruption per turn in combat
- **Proximity**: +1 Corruption/turn within 3 tiles

**Symbiotic Plate Enemies** (body horror):
- **On Attack**: +1 Corruption if hit by Symbiotic Plate melee attack
- **On Death**: +2 Corruption (witnessing transformation)

**Boss Encounters**:
- **Forlorn Archivist**: +5 Corruption on PsychicStorm ability (Phase 3)
- **Aetheric Aberration**: +3 Corruption on RealityTear ability

**WILL-Based Resistance** (from SPEC-ECONOMY-003):
```
Stress_Inflicted = Base_Stress - (Player_WILL × 0.5)
Corruption_Inflicted = Base_Corruption - (Player_WILL × 0.25)

Example:
  Forlorn Scholar encounter (Base +5 Stress)
  Player with WILL 4: 5 - (4 × 0.5) = 3 Stress inflicted
  Player with WILL 6: 5 - (6 × 0.5) = 2 Stress inflicted (min 0)
```

**Acceptance Criteria**:
- [ ] IsForlorn enemies inflict documented Stress/Corruption values
- [ ] WILL-based resistance formula applied
- [ ] Trauma accumulation tracked per combat encounter
- [ ] Breaking point mechanics trigger at 100 Stress (SPEC-ECONOMY-003)

**Example** (Forlorn Scholar encounter):
```csharp
// Forlorn Scholar: Caster with trauma aura
IsForlorn = true, // Inflicts passive Psychic Stress aura
BaseLegendValue = 35 // Moderate Act (can avoid combat to reduce stress)

// Trauma infliction:
// - On encounter: +5 Stress
// - Per turn proximity: +3 Stress/turn
// - On AethericBolt attack: +2 Stress
// - On death: +3 Stress (killed corrupted human)
// Total encounter cost: ~15-20 Stress (10-20% of Breaking Point threshold)
```

**Dependencies**:
- SPEC-ECONOMY-003 (Trauma Economy System) - Stress/Corruption mechanics, Breaking Points

---

