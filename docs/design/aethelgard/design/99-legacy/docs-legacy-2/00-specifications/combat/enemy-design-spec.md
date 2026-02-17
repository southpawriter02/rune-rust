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

### Practical Workflow

**For Enemy Designers**: The complete enemy design workflow follows an 8-step process from concept to implementation:

**Phase 1: Proposal** (Pre-Design)
1. **Enemy Design Proposal** (`/docs/templates/enemy-design-proposal.md`) - Pitch concept for team review
   - Define enemy concept, thematic justification, proposed tier/archetype
   - Validate roster diversity (avoid redundancy)
   - Get approval before committing to full design

**Phase 2: Design** (Worksheet)
2. **Enemy Design Worksheet** (`/docs/templates/enemy-design-worksheet.md`) - Full stat allocation and balance validation
   - Select threat tier and archetype (Step 1)
   - Allocate stats within budget formulas (Step 2)
   - Configure special mechanics (IsForlorn, IsBoss, Soak) (Step 3)
   - Design AI behavior probabilities (Step 4)
   - Validate TTK and damage output targets (Step 5)
   - Plan implementation across 4-5 code files (Step 6)

**Phase 3: Review** (Pre-Implementation)
3. **Design Review Checklist** (`/docs/templates/design-review-checklist.md`) - Reviewer validation before coding
   - Verify thematic consistency, stat budget compliance, balance targets
   - Prevent v0.18 pitfalls (one-shots, bullet sponges, damage variance)
   - Approve or request revisions

**Phase 4: Implementation** (Code + Documentation)
4. Implement in code files (Enemy.cs, EnemyFactory.cs, EnemyAI.cs, LootService.cs)
5. Create bestiary entry (`/docs/templates/enemy-bestiary-entry.md`)

The templates distill this 1,494-line specification into practical checklists that enforce all design guidelines across the full lifecycle.

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

**Templates** (ordered by workflow phase):

**Phase 1: Proposal**
- `/docs/templates/enemy-design-proposal.md` - **Enemy Design Proposal** (153 lines) - **START HERE** when proposing new enemies
  - Lightweight 1-page concept pitch for team review
  - Thematic justification, archetype/tier selection, design intent
  - Roster diversity validation (prevent redundancy)
  - Approval checklist for reviewers before worksheet phase

**Phase 2: Design**
- `/docs/templates/enemy-design-worksheet.md` - **Enemy Design Worksheet** (333 lines) - Full stat allocation and balance validation
  - Fill-in-the-blank checklist enforcing SPEC-COMBAT-012 guidelines
  - Validates stat budgets, archetype patterns, AI probabilities, TTK targets
  - Prevents v0.18 balance mistakes (one-shots, bullet sponges, damage variance)
  - References all FR sections with automatic ✓/✗ validation checks
- `/docs/templates/ai-behavior-pattern-template.md` - **AI Behavior Pattern Template** (658 lines) - Use after Step 4 of worksheet for implementation
  - Code templates for 5 archetype patterns (Aggressive, Defensive, Tactical, Phase-Based, Conditional)
  - Copy-paste starter code for EnemyAI.cs with probability-based decision making
  - Examples for 8 enemy types with full implementation (Tank, DPS, Swarm, Caster, Support, Mini-Boss, Boss)
  - Testing checklist and frequency analysis methodology

**Phase 3: Review**
- `/docs/templates/design-review-checklist.md` - **Design Review Checklist** (307 lines) - Reviewer validation before implementation
  - 8-section validation checklist for completed worksheets
  - Verifies thematic consistency, stat budget compliance, balance targets
  - Critical v0.18 pitfall checks (one-shot prevention, Soak caps, damage variance)
  - Approval/revision/rejection decision framework

**Phase 4: Multi-Enemy Encounters**
- `/docs/templates/encounter-composition-calculator.md` - **Encounter Composition Calculator** (650 lines) - Use when designing multi-enemy encounters
  - ETS (Encounter Threat Score) formula calculator with automatic validation
  - Validates encounter difficulty vs. player Legend level (Easy/Medium/Hard/Boss ranges)
  - Balance warnings for enemy count, damage output, TTK estimates
  - 7 encounter archetype patterns (Balanced, Tank+DPS, Swarm, Caster+Shield, Mixed Threat, Boss+Reinforcements, Elite Solo)
  - 4 fully-worked example encounters with validation and adjustments
  - Adjustment recommendations for encounters that are too easy/hard/unbalanced

**Phase 5: Post-Implementation Documentation**
- `/docs/templates/enemy-bestiary-entry.md` - **Enemy Bestiary Entry** (284 lines) - Use after implementation for documentation

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

## Balance & Tuning

### Time-to-Kill (TTK) Analysis

**TTK Targets by Threat Tier**:

| Threat Tier | Solo Player | 2-Player Party | 4-Player Party | Design Intent |
|-------------|-------------|----------------|----------------|---------------|
| **Low** | 2-3 turns | 1-2 turns | 1 turn | Quick clears, fodder, swarm threats |
| **Medium** | 4-6 turns | 2-3 turns | 1-2 turns | Standard combat duration, tactical decisions matter |
| **High** | 7-10 turns | 4-5 turns | 2-3 turns | Challenging combat, resource management required |
| **Lethal** | 10-15 turns | 6-8 turns | 3-5 turns | Boss-adjacent difficulty, party-wiping potential |
| **Boss** | 12-20 turns | 8-12 turns | 5-8 turns | Multi-phase encounters, narrative setpieces |

**TTK Calculation Methodology**:

```
Assumptions:
- Player base damage: 1d6+MIGHT (avg 5-7 at Legend 1)
- Player attack accuracy: 60-70% hit rate vs. enemy Defense
- Enemy HP regeneration: 0 (most enemies don't heal)
- Critical hits: 10% chance for 2× damage (not yet implemented in v0.24)

TTK Formula:
Effective DPS = (Base Damage × Hit Rate) + (Crit Damage × Crit Rate)
TTK = Enemy HP ÷ Effective DPS

Example: Corrupted Servitor (Low Tier)
- HP: 15
- Player DPS: 5 damage × 0.65 hit rate = 3.25 damage/turn
- TTK: 15 ÷ 3.25 = 4.6 turns ≈ 5 turns (solo player)
- Actual v0.24 playtests: 3-4 turns (confirms formula within 1 turn margin)
```

**TTK Variance Factors**:

1. **Player Build Variance**: ±30% TTK variance based on MIGHT/FINESSE investment
   - Berserker (MIGHT 7 at Legend 3): TTK reduced by ~25%
   - Scholar (MIGHT 2 at Legend 3): TTK increased by ~40%

2. **Enemy Soak Impact**: Each point of Soak increases TTK by ~15-20%
   - Vault Custodian (Soak 4): Expected 7-turn TTK becomes 10-turn TTK
   - Design lesson: Cap Soak at 4 for non-boss, 6 for boss (v0.18 adjustment)

3. **Special Ability Usage**: Boss phase transitions add 2-4 turns to TTK
   - Ruin-Warden Phase 2 (Berserk Strike): +3 turns to TTK due to increased threat forcing defensive play

**TTK Balance Guidelines**:

- **Low Tier**: Should never survive more than 5 turns solo (risk of player boredom in trash fights)
- **Medium Tier**: 6+ turns is acceptable if enemy has engaging abilities (Test Subject's Berserk Strike)
- **High Tier**: 10+ turns requires multiple tactical abilities to maintain engagement
- **Boss Tier**: 15+ turns requires phase transitions or reinforcement waves (Omega Sentinel v0.6)

---

### Damage Output Balance

**Enemy DPS vs. Player HP Thresholds**:

```
Player HP Pools (v0.24):
- Legend 1: 30 HP (10 + (4×STURDINESS))
- Legend 3: 45 HP (10 + (7×STURDINESS))
- Legend 5: 60 HP (10 + (10×STURDINESS))

Lethality Thresholds:
- 3-Turn Kill: Enemy must deal 33% player HP per turn (10 damage/turn at Legend 1)
- 5-Turn Kill: Enemy must deal 20% player HP per turn (6 damage/turn at Legend 1)
- 10-Turn Kill: Enemy must deal 10% player HP per turn (3 damage/turn at Legend 1)
```

**Damage Output Targets by Tier**:

| Threat Tier | Damage Per Turn (Avg) | % of Player HP | Turns to Kill Player | Design Intent |
|-------------|----------------------|----------------|---------------------|---------------|
| **Low** | 3-5 damage | 10-15% | 8-12 turns | Chip damage, attrition threats |
| **Medium** | 6-10 damage | 15-25% | 5-7 turns | Meaningful threat, requires healing |
| **High** | 11-16 damage | 30-40% | 3-5 turns | Dangerous, forces defensive play |
| **Lethal** | 17-24 damage | 45-60% | 2-3 turns | Two-shot potential, demands priority targeting |
| **Boss** | 12-20 damage | 30-50% | 3-6 turns | Sustained threat, long fights |

**v0.18 Damage Cap Rationale**:

```csharp
// BEFORE v0.18: Failure Colossus (Lethal Tier)
BaseDamageDice = 4, DamageBonus = 3 // 4d6+3 = 17-27 damage (avg 21)
// Problem: 70% chance to one-shot Legend 1 player (30 HP)

// AFTER v0.18: Damage redistribution
BaseDamageDice = 3, DamageBonus = 4 // 3d6+4 = 7-22 damage (avg 14.5)
// Result: 0% one-shot chance, 48% two-shot chance (balanced threat)

// BEFORE v0.18: Sentinel Prime (Lethal Tier)
BaseDamageDice = 5, DamageBonus = 0 // 5d6 = 5-30 damage (avg 17.5)
// Problem: Extreme variance (5 damage vs. 30 damage) feels unfair

// AFTER v0.18: Reduced variance
BaseDamageDice = 4, DamageBonus = 0 // 4d6 = 4-24 damage (avg 14)
// Result: More predictable threat, still dangerous but not RNG-dependent
```

**Design Principles**:

1. **No One-Shots**: Non-boss enemies should never one-shot a full-HP Legend-appropriate player
   - Maximum single-hit damage: 80% of player HP at same Legend level
   - Boss exception: Phase 2 abilities may exceed this (Ruin-Warden's Berserk Strike)

2. **Damage Variance**: Keep max/min damage ratio under 4:1
   - 1d6 (1-6) = 6:1 ratio → acceptable for Low tier
   - 5d6 (5-30) = 6:1 ratio → too swingy for Lethal tier
   - Prefer bonus damage over extra dice for high tiers (3d6+4 vs. 5d6)

3. **Sustained Threat**: Bosses should threaten 3-6 turn kills, not instant kills
   - Forces resource management (healing, defense) over entire encounter
   - Allows dramatic recovery moments (from 10% HP back to 50% via healing)

---

### Encounter Composition Guidelines

**Single-Enemy Encounters**:

```
Suitable for:
- Boss/Lethal tier (Ruin-Warden, Omega Sentinel, Sentinel Prime)
- High tier mini-bosses (Vault Custodian, Rust-Witch)
- Tutorial/narrative encounters (Forlorn Scholar social interaction)

Avoid:
- Low tier solo enemies (too trivial, feels unrewarding)
- Medium tier solo without special abilities (boring combat)
```

**2-3 Enemy Encounters** (Standard Combat):

| Group Type | Composition | Example | Difficulty | Design Pattern |
|------------|-------------|---------|------------|----------------|
| **Balanced** | 2× Medium | 2× Blight-Drone | Medium | No focus-fire priority, AoE valuable |
| **Tank + DPS** | 1× High + 1× Medium | Vault Custodian + Test Subject | High | Priority: Kill DPS first |
| **Swarm** | 3× Low | 3× Corrupted Servitor | Medium | AoE/cleave testing, attrition threat |
| **Caster + Meat Shield** | 1× Medium (Tank) + 1× Medium (Caster) | Maintenance Construct + Forlorn Scholar | High | Protect priority, tactical positioning |

**4-6 Enemy Encounters** (Large Battles):

```
Recommended Compositions:

1. Swarm Wave: 5× Low tier (Scrap-Hound, Corroded Sentry)
   - Tests AoE abilities, resource attrition, spatial control
   - Total HP: 50-75 (equivalent to 1× High tier)
   - Individual TTK: 1-2 turns, Group TTK: 8-12 turns

2. Mixed Threat: 2× Low + 1× Medium + 1× High
   - Example: 2× Corrupted Servitor + 1× Blight-Drone + 1× Bone-Keeper
   - Tests target prioritization, threat assessment
   - Clear order: High → Medium → Low (optimal strategy)

3. Reinforcement Waves: 2× Medium, then +2× Low after 3 turns
   - Boss encounter structure (Omega Sentinel v0.6)
   - Tests sustained resource management, prevents burst strategies

Balance Warning:
- AVOID 4+ Medium tier enemies (overwhelming damage, unfun difficulty spike)
- AVOID 6+ enemies total (UI clutter, turn resolution time exceeds 2 minutes)
```

**Encounter Difficulty Formula**:

```
Encounter Threat Score (ETS) = Σ(Enemy Legend Value × Enemy Count)

Difficulty Tiers:
- Easy: ETS 10-25 (tutorial, rest area ambushes)
- Medium: ETS 25-50 (standard combat)
- Hard: ETS 50-100 (challenging setpiece)
- Boss: ETS 100-200 (end-of-act encounters)

Example:
2× Corrupted Servitor (10 Legend each) + 1× Vault Custodian (75 Legend)
ETS = (10 × 2) + (75 × 1) = 95 (Hard encounter)

Player Legend-Appropriate Encounters:
- Legend 1 players: ETS 15-30 (easy), 30-50 (medium), 50-80 (hard)
- Legend 3 players: ETS 30-60 (easy), 60-100 (medium), 100-150 (hard)
- Legend 5 players: ETS 50-100 (easy), 100-150 (medium), 150-250 (hard)
```

---

### Tunable Parameters

**Safe to Adjust** (Minimal ripple effects):

1. **HP Pools** (±20%):
   - Increases/decreases TTK linearly
   - Preserves tactical role, doesn't break AI logic
   - Example: Corrupted Servitor 15 HP → 18 HP (adds 1 turn TTK)

2. **Damage Bonus** (Flat +X):
   - Affects lethality without changing variance
   - Easier to balance than changing dice count
   - Example: Blight-Drone 1d6+1 → 1d6+2 (adds ~1 damage/turn avg)

3. **Soak** (±1-2 points):
   - Major TTK impact: +1 Soak ≈ +15% effective HP
   - Stay within caps: 0-4 for non-boss, 0-6 for boss
   - Example: Vault Custodian Soak 4 → 5 (10-turn TTK → 12-turn TTK)

4. **BaseLegendValue** (±10 points):
   - Affects loot quality and player XP rewards (SPEC-ECONOMY-001)
   - Negligible combat impact, safe for reward tuning

5. **AI Probability Distributions** (±10%):
   - Changes tactical behavior without breaking roles
   - Example: Corrupted Servitor 80% attack → 70% attack, 30% defend (less aggressive)

**Dangerous to Adjust** (System-wide impact):

1. **Damage Dice Count** (e.g., 2d6 → 3d6):
   - Increases max damage, one-shot risk, damage variance simultaneously
   - Requires TTK recalculation, lethality testing, balance validation
   - Prefer adding +1 flat bonus over +1d6

2. **Attribute Distributions**:
   - Affects Defense Score, Initiative, damage scaling (MIGHT/FINESSE)
   - May invalidate archetype classification (Tank → DPS if MIGHT increased)
   - Requires holistic review of enemy role

3. **Special Ability Flags** (IsBoss, IsForlorn, IsChampion):
   - IsBoss: Triggers phase-based AI, boss-specific UI, loot tables
   - IsForlorn: Adds trauma infliction, changes encounter tone
   - IsChampion: +50% all stats (massive TTK/lethality shift)

4. **ThreatLevel Classification**:
   - Affects Legend-based spawning, population tables, reward scaling
   - Changing Corrupted Servitor from Low → Medium affects 15+ encounters (DormantProcess)

**Recommended Tuning Workflow**:

```
1. Identify Problem: "Corrupted Servitor feels too easy at Legend 3"
2. Measure Baseline: Current TTK 2 turns (target: 3-4 turns)
3. Select Safe Parameter: HP 15 → 18 (+20% HP)
4. Predict Impact: 2-turn TTK → 2.4-turn TTK (still short of target)
5. Iterate: Also adjust Soak 0 → 1 (adds another +15% effective HP)
6. Final Result: 2-turn TTK → 3.2-turn TTK (within target range)
7. Playtest: Validate feels more threatening without becoming tedious
```

---

### v0.18 Balance Lessons Learned

**Problem 1: Damage Variance Feels Unfair**

```
Sentinel Prime (BEFORE v0.18):
BaseDamageDice = 5, DamageBonus = 0 // 5d6 = 5-30 damage

Player Experience:
- Roll 5-8 damage: "This boss is a joke, I'm barely taking damage"
- Roll 28-30 damage: "That's bullshit, I went from full HP to dead in one hit"

Root Cause:
High dice counts create 6:1 variance. Player interprets as "unfair RNG" rather than "exciting risk."

Solution (AFTER v0.18):
BaseDamageDice = 4, DamageBonus = 0 // 4d6 = 4-24 damage
- Reduced variance to 6:1 (same ratio but lower absolute swing)
- Average damage reduced from 17.5 → 14 (no longer one-shot capable)
- Player perception: "Consistent threat that requires healing" vs. "RNG lottery"

Design Principle:
For Lethal/Boss tier, prefer 3d6+X over 5d6+0 to reduce perceived unfairness.
```

**Problem 2: Soak Creates Bullet Sponges**

```
Omega Sentinel (BEFORE v0.18):
MaxHP = 100, Soak = 8

Player Experience:
- Player deals 12 damage → reduced to 4 damage after Soak
- Expected 8-turn kill (100 HP ÷ 12 DPS) becomes 25-turn kill (100 HP ÷ 4 DPS)
- Combat duration: 15+ minutes, feels tedious rather than epic

Root Cause:
Soak scales multiplicatively with HP. High Soak + High HP creates exponential TTK growth.

Solution (AFTER v0.18):
MaxHP = 100, Soak = 6 (reduced by 25%)
- 12 damage → 6 damage after Soak
- TTK: 16-17 turns (still long, but within 10-minute combat window)

Design Principle:
Soak Cap = 6 for Boss tier, 4 for all others. Balance tankiness with HP, not Soak.
If enemy feels too squishy, add +20% HP before adding +1 Soak.
```

**Problem 3: One-Shot Potential Invalidates Player Choices**

```
Failure Colossus (BEFORE v0.18):
BaseDamageDice = 4, DamageBonus = 3 // 4d6+3 = 17-27 damage (avg 21)

Player Experience (Legend 1, 30 HP):
- 70% of attacks deal 21+ damage (one-shot kill from 70% HP)
- Player healing/defense decisions don't matter (killed regardless)
- Encounter becomes "reload simulator" rather than tactical challenge

Root Cause:
Damage ceiling exceeds 80% of player HP pool, negating healing/defensive counterplay.

Solution (AFTER v0.18):
BaseDamageDice = 3, DamageBonus = 4 // 3d6+4 = 7-22 damage (avg 14.5)
- 0% one-shot chance from full HP
- 48% two-shot chance (player has 1 turn to heal/defend)
- Maintains "dangerous threat" while allowing counterplay

Design Principle:
Maximum single-hit damage ≤ 80% of Legend-appropriate player HP.
Lethality comes from sustained pressure (3-4 turn kills), not instant kills.
```

**Problem 4: Low-Tier Enemies Don't Scale Into Late Game**

```
Corrupted Servitor (v0.3 implementation):
MaxHP = 15 (fixed), Damage = 1d6 (fixed)

Player Experience (Legend 5):
- Player HP: 60, Damage: 12-18
- Enemy dies in 1 turn, deals 3 damage (5% of player HP)
- Encounter feels like "clicking through trash" rather than combat

Root Cause:
No enemy scaling vs. player Legend progression. Early-game threats become trivial.

Solution (Proposed in FR-006, not yet implemented):
HP_Scaling = Base_HP + (Player_Legend × 3)
Damage_Scaling = Base_Damage + (Player_Legend ÷ 3)

Corrupted Servitor at Legend 5:
- HP: 15 + (5 × 3) = 30 HP (2-turn kill instead of 1-turn)
- Damage: 1d6 + (5 ÷ 3) = 1d6+1 (4 damage avg, 7% of player HP)

Design Principle:
Low-tier enemies should always require 2-3 turns to kill, regardless of player Legend.
Implement scaling formula before Legend 3 content (when gap becomes noticeable).
```

---

## Appendices

### Appendix A: Complete Enemy Statistics by Threat Tier

#### Low Tier Enemies (Legend 10-15)

| Enemy Name | HP | Damage | Attributes (M/F/Wi/Wl/St) | Soak | Special | Legend | Notes |
|------------|----|----|---------------------------|------|---------|--------|-------|
| **Corrupted Servitor** | 15 | 1d6 | 2/2/0/0/2 (6 total) | 0 | - | 10 | Baseline Low tier, aggressive AI |
| **Scrap-Hound** | 10 | 1d6 | 2/4/0/0/1 (7 total) | 0 | - | 10 | Glass cannon, high FINESSE |
| **Sludge-Crawler** | 12 | 1d6 | 2/3/0/0/1 (6 total) | 0 | Poison | 10 | Swarm enemy, poison DoT |
| **Corroded Sentry** | 15 | 1d6 | 2/1/0/0/2 (5 total) | 0 | -1 Defense | 10 | Rusted, low accuracy |
| **Maintenance Construct** | 25 | 1d6+1 | 3/2/0/0/4 (9 total) | 0 | Self-heal | 15 | Balanced, sustain threat |

**Design Notes**:
- HP Range: 10-25 (avg 15.4)
- Attribute Budget: 5-9 points (avg 6.8)
- Damage Range: 1d6 to 1d6+1 (avg 3.5-5 damage/turn)
- Legend Value: 10-15 (low reward)
- Special Mechanics: 2/5 have unique abilities (poison, self-heal)

---

#### Medium Tier Enemies (Legend 18-50)

| Enemy Name | HP | Damage | Attributes (M/F/Wi/Wl/St) | Soak | Special | Legend | Notes |
|------------|----|----|---------------------------|------|---------|--------|-------|
| **Husk Enforcer** | 25 | 1d6+2 | 3/1/0/0/3 (7 total) | 0 | - | 18 | Reanimated corpse, slow |
| **Test Subject** | 15 | 1d6+2 | 3/5/0/0/2 (10 total) | 0 | Berserk | 20 | Glass cannon, unstable |
| **Blight-Drone** | 25 | 1d6+1 | 3/3/0/0/3 (9 total) | 0 | - | 25 | Balanced DPS, ranged |
| **Arc-Welder Unit** | 30 | 2d6 | 2/3/0/0/3 (8 total) | 0 | Lightning | 25 | Industrial robot, electrical |
| **Corrupted Engineer** | 30 | 2d6 | 2/3/5/0/2 (12 total) | 0 | Buff allies | 30 | Support caster, high WITS |
| **Shrieker** | 35 | 1d6 | 2/2/0/4/2 (10 total) | 0 | IsForlorn | 30 | Psychic scream AoE |
| **Forlorn Scholar** | 30 | 2d6 | 2/3/4/5/2 (16 total) | 0 | IsForlorn | 35 | Caster, can negotiate |
| **Jötun-Reader Fragment** | 40 | 2d6 | 1/4/5/5/3 (18 total) | 0 | IsForlorn | 35 | AI fragment, Corruption |
| **Servitor Swarm** | 50 | 2d6 | 3/3/0/0/2 (8 total) | 0 | -2 Defense | 40 | Collective, low defense |
| **War-Frame** | 50 | 2d6 | 4/3/0/0/4 (11 total) | 0 | - | 50 | Mini-boss tier HP |

**Design Notes**:
- HP Range: 15-50 (avg 31)
- Attribute Budget: 7-18 points (avg 10.3)
- Damage Range: 1d6+1 to 2d6 (avg 5-10 damage/turn)
- Legend Value: 18-50 (moderate reward)
- Special Mechanics: 4/10 are IsForlorn (40% trauma enemies)
- High variance: Test Subject (15 HP glass cannon) vs. War-Frame (50 HP bruiser)

---

#### High Tier Enemies (Legend 55-75)

| Enemy Name | HP | Damage | Attributes (M/F/Wi/Wl/St) | Soak | Special | Legend | Notes |
|------------|----|----|---------------------------|------|---------|--------|-------|
| **Bone-Keeper** | 60 | 3d6 | 4/3/0/3/4 (14 total) | 0 | IsForlorn | 55 | Skeletal horror, armor-piercing |
| **Vault Custodian** | 70 | 2d6+2 | 5/2/0/0/5 (12 total) | 4 | Phase AI | 75 | Mini-boss, heavy armor |

**Design Notes**:
- HP Range: 60-70 (avg 65)
- Attribute Budget: 12-14 points (avg 13)
- Damage Range: 2d6+2 to 3d6 (avg 11-16 damage/turn)
- Legend Value: 55-75 (high reward)
- Special Mechanics: 1/2 is mini-boss with phase-based AI, 1/2 has Soak 4
- Vault Custodian: v0.18 Soak reduced from 6 → 4 to prevent bullet sponge

---

#### Lethal Tier Enemies (Legend 60-100)

| Enemy Name | HP | Damage | Attributes (M/F/Wi/Wl/St) | Soak | Special | Legend | Notes |
|------------|----|----|---------------------------|------|---------|--------|-------|
| **Failure Colossus** | 80 | 3d6+4 | 6/1/0/0/6 (13 total) | 4 | - | 60 | Construction automaton, slow |
| **Rust-Witch** | 70 | 3d6 | 2/3/4/5/3 (17 total) | 0 | IsForlorn | 75 | Symbiotic Plate caster |
| **Sentinel Prime** | 90 | 4d6 | 5/5/4/0/6 (20 total) | 6 | Phase AI | 100 | Elite military unit, tactical |

**Design Notes**:
- HP Range: 70-90 (avg 80)
- Attribute Budget: 13-20 points (avg 16.7)
- Damage Range: 3d6 to 4d6 (avg 14-18 damage/turn)
- Legend Value: 60-100 (very high reward)
- Special Mechanics: All have either Soak (4-6), IsForlorn, or Phase AI
- v0.18 Adjustments:
  - Failure Colossus: 4d6+3 → 3d6+4 (prevent one-shots)
  - Sentinel Prime: 5d6 → 4d6 (reduce variance)

---

#### Boss Tier Enemies (Legend 100-150)

| Enemy Name | HP | Damage | Attributes (M/F/Wi/Wl/St) | Soak | Special | Legend | Notes |
|------------|----|----|---------------------------|------|---------|--------|-------|
| **Ruin-Warden** | 80 | 2d6 | 5/3/0/0/5 (13 total) | 0 | IsBoss | 100 | Act 1 boss, phase-based |
| **Aetheric Aberration** | 75 | 3d6 | 2/4/5/6/3 (20 total) | 0 | IsBoss, IsForlorn | 100 | Act 1 boss, magic focus |
| **Forlorn Archivist** | 80 | 3d6 | 2/4/4/7/3 (20 total) | 0 | IsBoss, IsForlorn | 150 | Act 2 boss, psychic/summoner |
| **Omega Sentinel** | 100 | 3d6+3 | 6/3/0/0/6 (15 total) | 6 | IsBoss | 150 | Act 2 boss, physical tank |

**Design Notes**:
- HP Range: 75-100 (avg 83.75)
- Attribute Budget: 13-20 points (avg 17)
- Damage Range: 2d6 to 3d6+3 (avg 12-18 damage/turn)
- Legend Value: 100-150 (boss reward)
- Special Mechanics: 4/4 are IsBoss (100%), 2/4 are IsForlorn (50%)
- Boss Variance: Ruin-Warden (tank) vs. Aetheric Aberration (caster)
- v0.18 Adjustments:
  - Aetheric Aberration: HP 60 → 75 (match 100 Legend value)
  - Omega Sentinel: Soak 8 → 6 (prevent 25-turn TTK)

---

### Appendix B: Enemy Statistics by Archetype

#### Tank Archetype (High HP, High STURDINESS, Moderate Damage)

| Enemy Name | Tier | HP | Damage | Attributes (M/F/Wi/Wl/St) | Soak | TTK (Solo) | Role |
|------------|------|----|----|---------------------------|------|------------|------|
| **Vault Custodian** | High | 70 | 2d6+2 | 5/2/0/0/5 | 4 | 10-12 turns | Mini-boss guardian |
| **Omega Sentinel** | Boss | 100 | 3d6+3 | 6/3/0/0/6 | 6 | 16-20 turns | Final boss tank |
| **Failure Colossus** | Lethal | 80 | 3d6+4 | 6/1/0/0/6 | 4 | 12-15 turns | Heavy construction unit |

**Archetype Pattern**:
- HP: 70-100 (avg 83)
- STURDINESS: 5-6 (always max or near-max)
- Soak: 4-6 (always present)
- TTK: 10-20 turns (longest fights)
- AI Pattern: Defensive (40-50% attack, 30-40% defend)
- Design Intent: Absorb damage, protect squishier allies, force long encounters

---

#### DPS Archetype (Balanced Stats, Consistent Damage)

| Enemy Name | Tier | HP | Damage | Attributes (M/F/Wi/Wl/St) | Soak | TTK (Solo) | Role |
|------------|------|----|----|---------------------------|------|------------|------|
| **Blight-Drone** | Medium | 25 | 1d6+1 | 3/3/0/0/3 | 0 | 4-6 turns | Ranged harasser |
| **War-Frame** | Medium | 50 | 2d6 | 4/3/0/0/4 | 0 | 6-8 turns | Elite combat unit |
| **Ruin-Warden** | Boss | 80 | 2d6 | 5/3/0/0/5 | 0 | 12-15 turns | Boss DPS hybrid |

**Archetype Pattern**:
- HP: 25-80 (variable by tier)
- Attributes: Balanced (no dump stats below 3)
- Damage: Tier-appropriate (1d6+1 for Medium, 2d6 for Boss)
- TTK: 4-15 turns (tier-dependent)
- AI Pattern: Aggressive (70-80% attack, 20-30% defend)
- Design Intent: Reliable threat, no gimmicks, straightforward combat

---

#### Glass Cannon Archetype (High FINESSE/Damage, Low HP)

| Enemy Name | Tier | HP | Damage | Attributes (M/F/Wi/Wl/St) | Soak | TTK (Solo) | Role |
|------------|------|----|----|---------------------------|------|------------|------|
| **Scrap-Hound** | Low | 10 | 1d6 | 2/4/0/0/1 | 0 | 2-3 turns | Fast harasser |
| **Test Subject** | Medium | 15 | 1d6+2 | 3/5/0/0/2 | 0 | 3-4 turns | Unstable experiment |

**Archetype Pattern**:
- HP: 10-15 (lowest in tier)
- FINESSE: 4-5 (highest in tier)
- STURDINESS: 1-2 (lowest possible)
- Damage: High for tier (1d6+2 at Medium tier)
- TTK: 2-4 turns (dies fast)
- AI Pattern: Hyper-aggressive (80-90% attack, 10-20% flee at low HP)
- Design Intent: High-priority target, burst damage threat, "kill before it kills you"

---

#### Support Archetype (High WITS/WILL, Buff/Debuff Focus)

| Enemy Name | Tier | HP | Damage | Attributes (M/F/Wi/Wl/St) | Soak | TTK (Solo) | Role |
|------------|------|----|----|---------------------------|------|------------|------|
| **Corrupted Engineer** | Medium | 30 | 2d6 | 2/3/5/0/2 | 0 | 5-6 turns | Ally buffer |

**Archetype Pattern**:
- HP: Tier-appropriate (30 for Medium)
- WITS: 5+ (enables tactical abilities)
- Damage: Moderate (2d6)
- TTK: 5-6 turns (standard)
- AI Pattern: Tactical (30-40% attack, 30-40% buff allies, 20-30% debuff player)
- Design Intent: Priority kill target, force tactical decisions, multiplies ally effectiveness

---

#### Swarm Archetype (Low Individual HP, Numbers-Based Threat)

| Enemy Name | Tier | HP | Damage | Attributes (M/F/Wi/Wl/St) | Soak | TTK (Solo) | Role |
|------------|------|----|----|---------------------------|------|------------|------|
| **Corrupted Servitor** | Low | 15 | 1d6 | 2/2/0/0/2 | 0 | 2-3 turns | Baseline minion |
| **Sludge-Crawler** | Low | 12 | 1d6 | 2/3/0/0/1 | 0 | 2-3 turns | Poison swarm |
| **Servitor Swarm** | Medium | 50 | 2d6 | 3/3/0/0/2 | 0 | 6-8 turns | Collective entity |

**Archetype Pattern**:
- HP: 12-50 (variable, Servitor Swarm represents 3-5 units)
- Defense: Often penalty (-2 Defense for Servitor Swarm)
- Damage: Moderate (1d6 to 2d6)
- TTK: 2-8 turns (fast individual kills, high group HP)
- AI Pattern: Aggressive swarm (70-80% attack, 20-30% surround)
- Design Intent: AoE testing, attrition damage, overwhelm player via numbers

---

#### Caster Archetype (High WILL, Psychic/Magic Damage)

| Enemy Name | Tier | HP | Damage | Attributes (M/F/Wi/Wl/St) | Soak | Special | TTK (Solo) | Role |
|------------|------|----|----|---------------------------|------|---------|------------|------|
| **Forlorn Scholar** | Medium | 30 | 2d6 | 2/3/4/5/2 | 0 | IsForlorn | 5-6 turns | Corrupted human caster |
| **Jötun-Reader Fragment** | Medium | 40 | 2d6 | 1/4/5/5/3 | 0 | IsForlorn | 6-7 turns | AI psychic fragment |
| **Rust-Witch** | Lethal | 70 | 3d6 | 2/3/4/5/3 | 0 | IsForlorn | 10-12 turns | Symbiotic Plate caster |
| **Aetheric Aberration** | Boss | 75 | 3d6 | 2/4/5/6/3 | 0 | IsBoss, IsForlorn | 12-15 turns | Magic-focused boss |
| **Forlorn Archivist** | Boss | 80 | 3d6 | 2/4/4/7/3 | 0 | IsBoss, IsForlorn | 14-18 turns | Psychic summoner boss |

**Archetype Pattern**:
- HP: 30-80 (tier-appropriate)
- WILL: 5-7 (highest attribute)
- WITS: 4-5 (second-highest for spell variety)
- Damage: Psychic (ignores armor)
- Special: 5/5 are IsForlorn (100% trauma infliction)
- TTK: 5-18 turns (tier-dependent)
- AI Pattern: Tactical caster (30-50% attack, 30-40% debuff, 20-30% summon/buff)
- Design Intent: Trauma economy integration, psychic damage, high WILL resistance checks

---

#### Mini-Boss Archetype (Phase AI, Elite Stats, Not Full Boss)

| Enemy Name | Tier | HP | Damage | Attributes (M/F/Wi/Wl/St) | Soak | Special | TTK (Solo) | Role |
|------------|------|----|----|---------------------------|------|---------|------------|------|
| **Vault Custodian** | High | 70 | 2d6+2 | 5/2/0/0/5 | 4 | Phase AI | 10-12 turns | Guardian mini-boss |
| **Sentinel Prime** | Lethal | 90 | 4d6 | 5/5/4/0/6 | 6 | Phase AI | 14-16 turns | Elite military unit |

**Archetype Pattern**:
- HP: 70-90 (boss-tier HP)
- Attributes: 12-20 points (high budget)
- Soak: 4-6 (always present)
- Phase AI: HP-based behavior changes
- IsBoss Flag: False (not true boss, but elite)
- TTK: 10-16 turns (long but not boss-length)
- AI Pattern: Phase-based (changes at 50% HP threshold)
- Design Intent: Act-ending challenges, skill checks before bosses, "sub-boss" encounters

---

### Appendix C: Special Mechanics Reference

#### IsForlorn Enemies (Trauma Aura)

| Enemy Name | Tier | Stress/Turn | Corruption/Encounter | Aura Range | Total Trauma Cost |
|------------|------|-------------|----------------------|------------|-------------------|
| **Shrieker** | Medium | +3 | +2 | 3 rows | ~10-15 Stress |
| **Forlorn Scholar** | Medium | +3 | +5 | 3 rows | ~15-20 Stress |
| **Jötun-Reader Fragment** | Medium | +2 | +8 | 2 rows | ~15-25 Corruption |
| **Bone-Keeper** | High | +4 | +3 | 2 rows | ~15-25 Stress |
| **Rust-Witch** | Lethal | +5 | +10 | 4 rows | ~25-40 Corruption |
| **Aetheric Aberration** | Boss | +6 | +8 | 4 rows | ~30-50 Stress |
| **Forlorn Archivist** | Boss | +8 | +12 | 5 rows | ~50-80 Stress |

**Design Pattern**:
- All IsForlorn enemies inflict passive Psychic Stress per turn within aura range
- Corruption is inflicted on encounter start and on special ability hits
- Higher tiers have larger aura ranges (2-5 rows)
- Total trauma cost scales with tier: Medium 10-25, High 15-25, Lethal 25-40, Boss 30-80
- Forces player to balance combat duration vs. trauma accumulation

---

#### IsBoss Enemies (Multi-Phase Combat)

| Enemy Name | HP Phases | Phase 1 AI | Phase 2 AI (< 50% HP) | Phase 3 AI (< 25% HP) | Legend |
|------------|-----------|------------|----------------------|---------------------|--------|
| **Ruin-Warden** | 2-phase | 60% attack, 40% heavy strike | 40% berserk, 30% heavy, 30% charge defense | - | 100 |
| **Aetheric Aberration** | 3-phase | 50% aetheric bolt, 30% psychic scream, 20% phase shift | 40% psychic scream, 30% aetheric bolt, 30% summon | 50% void collapse (AoE), 30% summon, 20% heal | 100 |
| **Forlorn Archivist** | 3-phase | 50% mind spike, 30% memory drain, 20% summon | 40% summon, 30% mind spike, 30% AoE fear | 50% final summoning, 30% AoE, 20% desperation attack | 150 |
| **Omega Sentinel** | 3-phase | 60% railgun, 30% missile barrage, 10% defensive protocols | 40% missile barrage, 40% railgun, 20% shield recharge | 50% overcharge (massive damage), 30% defensive, 20% summon reinforcements | 150 |

**Design Pattern**:
- All bosses have 2-3 phases triggered by HP thresholds (50%, 25%)
- Phase 1: Standard attack rotations (50-60% primary attack)
- Phase 2: Increased special ability usage (30-40% AoE/summons)
- Phase 3 (if present): Desperation moves (50% high-damage abilities)
- IsBoss flag triggers: Phase-based AI, boss UI, special loot tables, narrative beats

---

#### Soak Mechanics (Flat Damage Reduction)

| Enemy Name | Soak | Effective HP Multiplier | v0.18 Adjustment | Design Rationale |
|------------|------|-------------------------|------------------|------------------|
| **Vault Custodian** | 4 | ×1.6 (70 HP → 112 effective HP) | Reduced from 6 → 4 | Prevent bullet sponge |
| **Failure Colossus** | 4 | ×1.6 (80 HP → 128 effective HP) | No change | Heavy armor tank |
| **Sentinel Prime** | 6 | ×2.0 (90 HP → 180 effective HP) | Reduced from 8 → 6 | Military-grade armor, still tough |
| **Omega Sentinel** | 6 | ×1.6 (100 HP → 160 effective HP) | Reduced from 8 → 6 | Boss tank, capped to prevent 25-turn TTK |

**Soak Calculation**:
```
Effective HP = Base HP ÷ (1 - (Soak ÷ Average Player Damage))

Example: Vault Custodian
- Base HP: 70
- Soak: 4
- Player Damage: 10 avg
- Damage Reduction: 4 ÷ 10 = 40%
- Effective HP: 70 ÷ (1 - 0.4) = 70 ÷ 0.6 = 117 effective HP
- TTK: 117 ÷ 10 = 11.7 turns (vs. 7 turns without Soak)
```

**Design Caps**:
- Non-Boss Maximum: Soak 4 (40% reduction vs. 10 damage)
- Boss Maximum: Soak 6 (60% reduction vs. 10 damage)
- v0.18 Lesson: Soak scales multiplicatively with HP, creating exponential TTK growth
- Balance Rule: Add +20% HP before adding +1 Soak

---

#### IsChampion Enemies (Elite Variants)

**Champion Stat Modifiers** (Not yet implemented in v0.24, reserved for future use):
```
Champion Multipliers:
- HP: ×1.5 (Example: Corrupted Servitor 15 HP → 22 HP)
- Damage: ×1.5 (Example: 1d6 → 1d6+3)
- Attributes: ×1.5 rounded down (Example: MIGHT 2 → 3)
- Legend Value: ×1.5 (Example: 10 Legend → 15 Legend)
- Special: +1 Soak, or unique ability

Design Intent:
- Rare spawns (5-10% chance)
- Named variants ("Champion Corrupted Servitor: 'Rust-Eater'")
- Higher reward (loot quality +1 tier)
- Used for dynamic difficulty scaling
```

---

### Appendix D: v0.18 Balance Adjustment Summary

| Enemy Name | Property | Before v0.18 | After v0.18 | Reason |
|------------|----------|--------------|-------------|--------|
| **Aetheric Aberration** | HP | 60 | 75 | Match 100 Legend value, boss durability |
| **Vault Custodian** | Soak | 6 | 4 | Prevent bullet sponge (25-turn TTK → 12-turn TTK) |
| **Omega Sentinel** | Soak | 8 | 6 | Prevent frustrating 25+ turn combat |
| **Failure Colossus** | Damage | 4d6+3 (17-27 avg 21) | 3d6+4 (7-22 avg 14.5) | Prevent one-shots (70% one-shot → 0%) |
| **Sentinel Prime** | Damage | 5d6 (5-30 avg 17.5) | 4d6 (4-24 avg 14) | Reduce variance (6:1 ratio feels unfair) |
| **Corroded Sentry** | Legend | 5 | 10 | Match HP investment (0.67 Legend/HP ratio) |
| **Husk Enforcer** | Legend | 15 | 18 | Match 25 HP + high damage (0.72 Legend/HP) |
| **Arc-Welder Unit** | Legend | 20 | 25 | Match 30 HP pool (0.83 Legend/HP) |
| **Servitor Swarm** | Legend | 30 | 40 | Match 50 HP collective (0.80 Legend/HP) |
| **Bone-Keeper** | Legend | 50 | 55 | Match high damage output (0.92 Legend/HP) |

**Key Lessons**:
1. **Soak Caps Matter**: Soak 8 → 6 reduced Omega Sentinel TTK from 25 turns → 16 turns (playable)
2. **Damage Variance > Average**: 5d6 avg 17.5 feels worse than 4d6 avg 14 due to swing (5-30 vs. 4-24)
3. **One-Shots Kill Fun**: 70% one-shot chance → reload simulator (Failure Colossus 4d6+3)
4. **Legend/HP Ratio**: Target 0.67-1.0 Legend per HP for fair reward scaling
5. **Boss HP Floors**: Bosses should have 75+ HP to match 100+ Legend value (durability expectation)

---

**End of Appendices**

---

