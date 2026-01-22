# Boss Encounter System Specification

Parent item: Specs: Combat (Specs%20Combat%202ba55eb312da80ae8221e819227a61b9.md)

> Specification ID: SPEC-COMBAT-005
Status: Draft
Domain: Combat
Related Systems: Combat Resolution, Enemy Design, Damage Calculation, Loot System
> 

---

## Document Control

| Property | Value |
| --- | --- |
| **Specification ID** | SPEC-COMBAT-005 |
| **Title** | Boss Encounter System |
| **Version** | 1.0.0 |
| **Status** | Draft |
| **Last Updated** | 2025-11-22 |
| **Author** | AI Specification Agent |
| **Reviewers** | Combat Design Team, Balance Team, Implementation Team |
| **Stakeholders** | Enemy Designer (boss design), Loot Designer (boss rewards), Narrative Designer (boss lore) |

---

## Executive Summary

### Purpose

The **Boss Encounter System** defines the mechanics and design framework for epic boss fights in Rune & Rust. This specification formalizes:

1. **Multi-Phase Combat** - HP-based phase transitions with distinct mechanics per phase
2. **Telegraphed Abilities** - Charged attacks with counterplay through interrupts and vulnerability windows
3. **Enrage System** - Time pressure and difficulty escalation through HP/turn-based enrage triggers
4. **Boss Loot Economy** - Artifact drops, guaranteed quality equipment, and unique boss rewards
5. **Integration Architecture** - How boss mechanics hook into the core combat loop

### Scope

**In Scope**:

- Multi-phase boss combat mechanics (2-4 phases per boss)
- HP threshold-based phase transitions (75%, 50%, 25%)
- Telegraphed ability system with charge times and interrupt mechanics
- Enrage triggers (HP-based and turn-based timers)
- Vulnerability windows after ultimate abilities
- Phase-specific stat modifiers (damage, defense, regeneration, bonus actions)
- Add wave spawning during phase transitions
- Invulnerability mechanics during phase transitions
- Boss loot table system (artifacts, guaranteed drops, unique items, set bonuses)
- 4 implemented boss archetypes (Ruin-Warden, Aetheric Aberration, Forlorn Archivist, Omega Sentinel)

**Out of Scope**:

- General enemy design guidelines → SPEC-COMBAT-012
- Standard enemy AI behavior → SPEC-AI-001
- Movement and positioning mechanics → SPEC-COMBAT-009
- Status effect mechanics → SPEC-COMBAT-003
- Individual boss lore and narrative descriptions → Bestiary entries

### Success Criteria

This specification is successful if:

1. **Boss fights feel epic** - Multi-phase mechanics create dramatic escalation and memorable encounters
2. **Counterplay exists** - Players can interrupt telegraphed abilities and exploit vulnerability windows
3. **Difficulty curve is fair** - Enrage prevents stalling while maintaining fairness
4. **Rewards feel meaningful** - Boss loot quality matches difficulty (artifacts, guaranteed Clan-Forged+)
5. **Integration is seamless** - Boss mechanics integrate cleanly into existing combat loop

### Key Metrics

**Boss Encounter Targets** (Solo Player, Legend 5):

| Metric | Low-Tier Boss | Mid-Tier Boss | High-Tier Boss | World Boss |
| --- | --- | --- | --- | --- |
| **HP** | 60-80 | 80-100 | 100-120 | 150-200 |
| **Time-to-Kill** | 8-12 turns | 12-16 turns | 16-20 turns | 20-30 turns |
| **Phases** | 2-3 | 3 | 3-4 | 4 |
| **Telegraphed Abilities** | 1-2 | 2-3 | 3-4 | 4-5 |
| **Add Waves** | 0-1 | 1-2 | 2-3 | 3-4 |
| **Loot Quality** | Clan-Forged+ | Clan-Forged+ | Optimized+ | Optimized+ |
| **Artifact Chance** | 10% | 15% | 18% | 20% |

---

## Related Documentation

### Dependencies

**Completed Specifications**:

- [**SPEC-COMBAT-001**](https://www.notion.so/combat/combat-resolution-spec.md): Combat Resolution System → Turn sequence, initiative, combat state
- [**SPEC-COMBAT-002**](https://www.notion.so/combat/damage-calculation-spec.md): Damage Calculation System → Damage formulas, critical hits
- [**SPEC-COMBAT-003**](https://www.notion.so/combat/status-effects-spec.md): Status Effects System → Buffs, debuffs, control effects
- [**SPEC-COMBAT-012**](https://www.notion.so/combat/enemy-design-spec.md): Enemy Design & Bestiary System → Enemy stat budgets, archetypes
- [**SPEC-ECONOMY-001**](https://www.notion.so/economy/loot-equipment-spec.md): Loot & Equipment System → Quality tiers, drop rates
- [**SPEC-PROGRESSION-001**](https://www.notion.so/progression/character-progression-spec.md): Character Progression → Legend scaling

**Planned Specifications**:

- SPEC-COMBAT-010 (Telegraphed Abilities System) - Detailed telegraph mechanics (currently integrated into this spec)
- SPEC-COMBAT-009 (Movement & Positioning) - Tactical positioning for boss arenas
- SPEC-AI-001 (Enemy AI System) - AI decision-making for boss ability usage

### Code References

**Core Implementation Files**:

- `RuneAndRust.Engine/BossEncounterService.cs` (lines 1-529) - Phase transitions, enrage system, invulnerability
- `RuneAndRust.Engine/BossCombatIntegration.cs` (lines 1-459) - Main integration layer for combat loop
- `RuneAndRust.Engine/TelegraphedAbilityService.cs` - Telegraph mechanics, interrupts, vulnerability windows
- `RuneAndRust.Engine/BossLootService.cs` (lines 1-530) - Boss loot generation, artifacts, set bonuses
- `RuneAndRust.Engine/BossDatabase.cs` (lines 1-711) - Boss configurations (phases, abilities, loot tables)

**Data Models**:

- `RuneAndRust.Core/BossPhaseDefinition.cs` (lines 1-114) - Phase data structure
- `RuneAndRust.Core/BossAbility.cs` (lines 1-184) - Ability data structure
- `RuneAndRust.Core/BossLootTable.cs` - Loot table configuration

**Persistence**:

- `RuneAndRust.Persistence/BossEncounterRepository.cs` - Database persistence for boss state
- `RuneAndRust.Persistence/BossMasterSeeder.cs` - Boss data seeding

### Layer 1 Documentation

**Integration Guides**:

- `/BOSS_COMBAT_INTEGRATION_GUIDE.md` (488 lines) - Complete integration guide for combat loop

**Test Coverage**:

- `RuneAndRust.Tests/BossEncounterTests.cs` - Phase transition tests
- `RuneAndRust.Tests/BossLootServiceTests.cs` - Loot generation tests
- `RuneAndRust.Tests/BossCombatIntegrationDemoTests.cs` - Full combat integration tests
- `RuneAndRust.Tests/BossSystemIntegrationTests.cs` - System integration tests

---

## Design Philosophy

The Boss Encounter System is built on 5 core design principles:

### 1. **Escalating Difficulty Through Phases**

**Principle**: Boss fights should crescendo in difficulty through distinct phases, not just be "bigger HP pools."

**Implementation**:

- **HP-based phase transitions** (Phase 2 at 75%, Phase 3 at 50%, Phase 4 at 25%)
- **Progressive stat scaling** per phase (damage +20-50%, defense +2-5, regeneration +3-5 HP/turn)
- **New abilities unlock** in later phases (Phase 1: basic attacks, Phase 3: ultimate abilities)
- **Add wave reinforcements** during phase transitions
- **Invulnerability windows** (1-2 turns) during phase transitions for dramatic effect

**Example** (Ruin-Warden):

- **Phase 1 (100-75%)**: Basic attacks, 1.0x damage multiplier
- **Phase 2 (75-50%)**: +20% damage, spawns 2 Corrupted Servitors, gains regeneration (3 HP/turn)
- **Phase 3 (50-0%)**: +50% damage, +1 bonus action per turn, regeneration increases to 5 HP/turn

### 2. **Readable Telegraphs Create Counterplay**

**Principle**: Boss special attacks must be telegraphed to create skill-based counterplay, not surprise one-shots.

**Implementation**:

- **Charge time** (1-2 turns) before ability executes
- **Clear telegraph messages** displayed in combat log ("⚡ The Ruin-Warden's core begins to glow with building energy!")
- **Interrupt thresholds** (deal 15-25 damage during charge to cancel ability)
- **Vulnerability windows** (2-3 turns of increased damage taken after ultimate abilities)
- **Visual cues** in combat UI showing active telegraphs and turns remaining

**Example** (Ruin-Warden's System Overload):

```
Turn 5: ⚡ The Ruin-Warden's core begins to glow with building energy!
        [Charging: System Overload | 1 turn remaining | 15 damage needed to interrupt]

Turn 6: Player deals 18 damage
        ✓ INTERRUPTED! System Overload cancelled!

```

### 3. **Enrage Creates Time Pressure Without Frustration**

**Principle**: Enrage mechanics prevent stalling and create urgency, but don't feel like artificial difficulty.

**Implementation**:

- **HP-based enrage** (triggers at 20-30% HP remaining)
- **Turn-based enrage** (optional, triggers at 15-20 turns if implemented)
- **Reasonable enrage buffs** (+30-50% damage, +1 bonus action, control immunity)
- **Clear warnings** before enrage triggers ("⚠️ System integrity critical!")
- **Enrage as dramatic escalation**, not instant-kill punishment

**Example** (Ruin-Warden Enrage at 20% HP):

```
💀 Ruin-Warden enters ENRAGE state!
⚡ Reason: System integrity critical (18%)
⚡ Damage increased by 40%!
⚡ Gains +1 action per turn!
⚡ [Control Immunity] - Cannot be stunned or disabled!

```

### 4. **Boss Loot Matches Difficulty**

**Principle**: Boss rewards must feel proportional to the challenge (guaranteed high-quality loot, artifact chances).

**Implementation**:

- **Guaranteed quality drops** (minimum Clan-Forged, 30-40% chance for Optimized)
- **Artifact drop chances** (10-20% based on boss tier)
- **Unique boss items** (once-per-character drops with special effects)
- **Set item support** (multi-piece sets with synergistic bonuses)
- **Currency scaling** (300-1000 Silver Marks based on boss tier)
- **Crafting material drops** (guaranteed legendary components)

**Example** (Omega Sentinel Loot Table):

- Guaranteed: 1x Optimized quality item (40% for Legendary)
- Artifact Chance: 18%
- Currency: 500-1000 Silver Marks
- Materials: Ancient Power Core (legendary), Structural Scrap x3

### 5. **Seamless Integration Into Combat Loop**

**Principle**: Boss mechanics should integrate cleanly into existing combat without special-case spaghetti code.

**Implementation**:

- **BossCombatIntegration service** provides 7 clean integration points
- **Standard combat flow** works for both bosses and regular enemies
- **Boss-specific checks** isolated in integration layer (`if (enemy.IsBoss)`)
- **Database-driven configuration** (all boss data in database, not hardcoded)
- **Composable mechanics** (telegraphs, phases, enrage work independently)

**Integration Points**:

1. Combat initialization (`InitializeBossEncounters()`)
2. After player damage (`CheckTelegraphInterrupt()`)
3. Boss turn processing (`ShouldBossTelegraph()`, `ProcessBossAction()`)
4. End of turn (`ProcessEndOfTurn()`)
5. Loot generation (`GenerateBossLoot()`)
6. Combat cleanup (`ClearBossCombatState()`)

---

## System Overview

### Boss Encounter Lifecycle

A boss encounter follows this lifecycle:

```
┌─────────────────────────────────────────────────────────────┐
│ 1. COMBAT INITIALIZATION                                    │
│    - Boss encounter initialized (BossEncounterService)      │
│    - Phase 1 activated                                      │
│    - Boss abilities seeded from database                    │
│    - Combat log displays boss intro message                 │
└───────────────┬─────────────────────────────────────────────┘
                │
                ▼
┌─────────────────────────────────────────────────────────────┐
│ 2. PHASE 1 COMBAT LOOP                                      │
│    - Boss uses standard attacks or telegraphed abilities    │
│    - Player can interrupt telegraphed abilities             │
│    - HP threshold monitored for phase transition (75%)      │
└───────────────┬─────────────────────────────────────────────┘
                │
                ▼ HP ≤ 75%
┌─────────────────────────────────────────────────────────────┐
│ 3. PHASE TRANSITION (Phase 1 → Phase 2)                     │
│    - Invulnerability granted (1-2 turns)                    │
│    - Phase transition cinematic message                     │
│    - Stat modifiers applied (+damage, +defense, +regen)     │
│    - Add wave spawned (optional)                            │
│    - New abilities unlocked                                 │
└───────────────┬─────────────────────────────────────────────┘
                │
                ▼
┌─────────────────────────────────────────────────────────────┐
│ 4. PHASE 2+ COMBAT LOOP                                     │
│    - Boss uses phase-appropriate abilities                  │
│    - Enrage check (HP < 20% or Turn > 15)                   │
│    - Repeat phase transition at next threshold (50%, 25%)   │
└───────────────┬─────────────────────────────────────────────┘
                │
                ▼ HP ≤ 20% OR Turn ≥ 15
┌─────────────────────────────────────────────────────────────┐
│ 5. ENRAGE STATE                                             │
│    - Damage multiplier increased (+30-50%)                  │
│    - Bonus actions granted (+1 action/turn)                 │
│    - Control immunity applied                               │
│    - Ultimate abilities used more frequently                │
└───────────────┬─────────────────────────────────────────────┘
                │
                ▼ Boss HP ≤ 0
┌─────────────────────────────────────────────────────────────┐
│ 6. BOSS DEFEATED - LOOT GENERATION                          │
│    - Guaranteed quality drops rolled (Clan-Forged+)         │
│    - Artifact roll (10-20% chance)                          │
│    - Unique item roll (once-per-character tracking)         │
│    - Currency and crafting materials awarded                │
│    - Combat state cleared                                   │
└─────────────────────────────────────────────────────────────┘

```

### Boss Type Classification

**Implementation Note**: The current system defines 4 boss encounters:

| Boss Type | Encounter ID | Tier | Phases | Theme |
| --- | --- | --- | --- | --- |
| **Ruin-Warden** | 1 | Sector Boss | 3 | Electrical/Melee construct |
| **Aetheric Aberration** | 2 | Sector Boss | 3 | Void/Reality-warping entity |
| **Forlorn Archivist** | 3 | Sector Boss | 3 | Psychic/Trauma horror |
| **Omega Sentinel** | 4 | World Boss | 3 | Heavy/Physical juggernaut |

**Design Pattern**: All implemented bosses follow a 3-phase structure (100% → 75% → 50% → 0%).

---

## Functional Requirements

### FR-001: Multi-Phase Combat Mechanics

### FR-001.1: HP-Based Phase Transitions

**Requirement**: Bosses SHALL transition between phases based on HP percentage thresholds, triggering distinct mechanical changes and cinematic events.

**Business Logic** (`BossEncounterService.cs:66-113`):

```csharp
// Phase transition check (executed after boss takes damage)
float hpPercentage = (float)boss.HP / boss.MaxHP;

// Phase 2 transition at 75% HP
if (currentPhase == 1 && !phase2Triggered && hpPercentage <= 0.75) {
    TriggerPhaseTransition(boss, 2, combatState);
}

// Phase 3 transition at 50% HP
if (currentPhase == 2 && !phase3Triggered && hpPercentage <= 0.50) {
    TriggerPhaseTransition(boss, 3, combatState);
}

// Phase 4 transition at 25% HP (if boss has 4 phases)
if (currentPhase == 3 && !phase4Triggered && totalPhases >= 4 && hpPercentage <= 0.25) {
    TriggerPhaseTransition(boss, 4, combatState);
}

```

**Phase Threshold Configuration**:

| Phase Number | HP Threshold | Trigger Condition | Implementation |
| --- | --- | --- | --- |
| Phase 1 | 100% | Combat initialization | Always active at combat start |
| Phase 2 | 75% | `boss.HP / boss.MaxHP <= 0.75` | Triggered once when threshold crossed |
| Phase 3 | 50% | `boss.HP / boss.MaxHP <= 0.50` | Triggered once when threshold crossed |
| Phase 4 | 25% | `boss.HP / boss.MaxHP <= 0.25` | Triggered once (rare, only 4-phase bosses) |

**Phase Transition Flags** (prevents re-triggering):

- `Phase2Triggered` (boolean) - Set to true after Phase 2 transition
- `Phase3Triggered` (boolean) - Set to true after Phase 3 transition
- `Phase4Triggered` (boolean) - Set to true after Phase 4 transition

**Design Constraints**:

- Phase transitions trigger **exactly once** (flags prevent re-entry)
- Phases must be crossed in order (cannot skip Phase 2 to go directly to Phase 3)
- HP percentage check is performed **after damage is dealt** (integration point: `ProcessBossAction()`)
- Minimum 2 phases, maximum 4 phases per boss

### FR-001.2: Phase Transition Execution

**Requirement**: When a phase transition triggers, the system SHALL execute a 4-step sequence: invulnerability → state update → event execution → cinematic log.

**Phase Transition Sequence** (`BossEncounterService.cs:115-172`):

```
Step 1: Apply Invulnerability
   ├─ Set boss invulnerability turns (1-2 turns based on phase config)
   ├─ Boss cannot take damage during invulnerability
   └─ Decrements each turn until expiration

Step 2: Update Phase State
   ├─ Increment boss.CurrentPhase (1 → 2, 2 → 3, etc.)
   ├─ Set phase triggered flag (Phase2Triggered = true)
   └─ Update database state

Step 3: Execute Phase Transition Events
   ├─ Spawn add waves (if configured)
   ├─ Apply phase stat modifiers (damage, defense, regen)
   └─ Unlock new abilities for this phase

Step 4: Build Cinematic Log Message
   ├─ Display phase transition header ("═══ PHASE 2 TRANSITION ═══")
   ├─ Show phase description text
   ├─ List mechanical changes (invulnerability, regen, damage boost)
   └─ Add to combat log

```

**Cinematic Message Format**:

```
╔═══════════════════════════════════════════════════════════════╗
║ PHASE 2 TRANSITION
╚═══════════════════════════════════════════════════════════════╝
⚡ The Ruin-Warden's systems overload! Emergency protocols activated!

⚠️ Ruin-Warden is [INVULNERABLE] for 1 turn(s)!
🔄 Ruin-Warden gains [Regeneration] (3 HP/turn)
⚡ Ruin-Warden's damage increased by 20%!
⚠️ Reinforcements summoned!
  • Corrupted Servitor spawned!
  • Corrupted Servitor spawned!

```

**Implementation Reference**: `BossEncounterService.cs:TriggerPhaseTransition()` (lines 115-172)

### FR-001.3: Phase Stat Modifiers

**Requirement**: Each phase SHALL apply cumulative stat modifiers to the boss, scaling difficulty progressively.

**Stat Modifier Types** (`BossPhaseDefinition.cs:60-86`):

| Modifier | Type | Effect | Example Values | Stacking |
| --- | --- | --- | --- | --- |
| **Damage Multiplier** | Float | Multiply base damage | 1.0, 1.2, 1.5, 1.8 | Replace (not cumulative) |
| **Defense Bonus** | Integer | Add to defense stat | +0, +2, +3, +5 | Cumulative |
| **Regeneration** | Integer | HP healed per turn | 0, 3, 5, 8 | Replace (not cumulative) |
| **Bonus Actions** | Integer | Extra actions/turn | 0, 0, 1, 1 | Cumulative |
| **Soak Bonus** | Integer | Damage reduction | +0, +1, +2, +3 | Cumulative |

**Damage Multiplier Application** (`BossEncounterService.cs:488-492`):

```csharp
if (phaseDefinition.DamageMultiplier > 1.0)
{
    int bonusDamage = (int)((phaseDefinition.DamageMultiplier - 1.0) * boss.BaseDamageDice * 3);
    boss.DamageBonus += bonusDamage;
}

```

**Formula**: `BonusDamage = (Multiplier - 1.0) × BaseDamageDice × 3`

**Example** (Ruin-Warden Phase 2, 2d6 base damage, 1.2x multiplier):

```
BonusDamage = (1.2 - 1.0) × 2 × 3 = 0.2 × 6 = 1.2 ≈ 1
Final Damage = 2d6 + (original bonus) + 1

```

**Phase Stat Progression Pattern** (Standard 3-Phase Boss):

| Phase | HP Range | Damage Mult | Defense | Regen | Bonus Actions |
| --- | --- | --- | --- | --- | --- |
| Phase 1 | 100-75% | 1.0x | +0 | 0 HP/turn | 0 |
| Phase 2 | 75-50% | 1.2-1.3x | +2-3 | 3-4 HP/turn | 0 |
| Phase 3 | 50-0% | 1.5-1.6x | +3-5 | 5-8 HP/turn | +1 |

### FR-001.4: Add Wave Spawning

**Requirement**: Phase transitions MAY spawn add waves (reinforcement enemies) to increase encounter difficulty.

**Add Wave Configuration** (`BossPhaseDefinition.cs:91-113`, `BossDatabase.cs:91-100`):

```csharp
AddWave = new AddWaveConfig
{
    EnemyTypes = new List<EnemyType> { EnemyType.CorruptedServitor, EnemyType.CorruptedServitor },
    SpawnCounts = new Dictionary<EnemyType, int>
    {
        { EnemyType.CorruptedServitor, 2 }
    },
    SpawnDescription = "⚠️ The Ruin-Warden calls for reinforcements!",
    SpawnDelay = 0  // Adds can act immediately (0 turn delay)
}

```

**Add Wave Spawn Logic** (`BossEncounterService.cs:204-247`):

```
1. Parse add wave composition from JSON (EnemyType + Count)
2. For each enemy type in wave:
   a. Create N enemies using EnemyFactory
   b. Generate unique ID per enemy (e.g., "CorruptedServitor_a3f9b821")
   c. Add to combat state enemy list
   d. Log spawn message
3. Update add tracking counters (for future mechanics)
4. Refresh initiative order to include new adds

```

**Add Wave Design Constraints**:

- **Add count**: 1-3 enemies per wave (avoid overwhelming player)
- **Add difficulty**: Low or Medium threat tier (not High/Lethal)
- **Add types**: Typically Swarm or DPS archetypes (not Tanks/Mini-Bosses)
- **Spawn timing**: During phase transition (boss is invulnerable, creates breathing room)
- **Spawn delay**: 0 turns (adds act immediately in initiative order)

**Add Tracking** (future mechanic, currently logged but not enforced):

- `TotalAddsSpawned` - Total reinforcements summoned
- `AddsCurrentlyAlive` - Living adds in combat
- Can be used for enrage triggers ("Boss enrages if all adds are killed")

**Example Add Waves by Boss**:

| Boss | Phase | Add Wave | Tactical Purpose |
| --- | --- | --- | --- |
| Ruin-Warden | Phase 2 | 2x Corrupted Servitor | Distraction, force target switching |
| Aetheric Aberration | Phase 2 | 1x Blight Drone | Ranged pressure, damage over time |
| Forlorn Archivist | Phase 2 | 2x Corrupted Servitor | Meat shields, stress aura stacking |
| Omega Sentinel | None | (No adds) | Pure solo encounter, mechanical difficulty |

### FR-001.5: Invulnerability Mechanics

**Requirement**: Bosses SHALL become invulnerable for 1-2 turns during phase transitions to create cinematic pacing and tactical breaks.

**Invulnerability System** (`BossEncounterService.cs:348-410`):

```csharp
// Check if boss is invulnerable (called before damage application)
public bool IsBossInvulnerable(Enemy boss)
{
    var bossState = repository.GetBossCombatState(boss.Id);
    return bossState != null && bossState.InvulnerabilityTurnsRemaining > 0;
}

// Process end of turn (decrement invulnerability counter)
public string ProcessEndOfTurn(Enemy boss)
{
    if (bossState.InvulnerabilityTurnsRemaining > 0)
    {
        int newTurns = bossState.InvulnerabilityTurnsRemaining - 1;
        repository.UpdateInvulnerability(boss.Id, newTurns);

        if (newTurns == 0)
        {
            return "⚔️ {boss.Name} is no longer invulnerable!";
        }
    }
}

```

**Invulnerability Configuration**:

| Phase | Standard Boss | World Boss | Rationale |
| --- | --- | --- | --- |
| Phase 2 | 1 turn | 1 turn | Brief pause for add spawning |
| Phase 3 | 1-2 turns | 2 turns | Longer cinematic moment |
| Phase 4 | 2 turns | 2 turns | Epic final phase escalation |

**Invulnerability Mechanics**:

- **Damage immunity**: All damage dealt to boss is negated (not just reduced to 0)
- **Duration tracking**: Decrements by 1 each turn (end-of-turn processing)
- **Combat log display**: "⚠️ Boss is [INVULNERABLE] for N turn(s)!"
- **End notification**: "⚔️ Boss is no longer invulnerable!" when expiring

**Design Rationale**:

- **Prevents phase-skip exploits** (player cannot burst through multiple phases)
- **Creates tactical breathing room** (player can reposition, heal, assess add waves)
- **Enhances cinematic feel** (boss transformation feels impactful, not instant)
- **Balances add wave difficulty** (player isn't pressured by both boss + adds simultaneously)

**Integration with Combat Loop**:

```csharp
// In CombatEngine.PlayerAttack()
if (target.IsBoss && bossEncounterService.IsBossInvulnerable(target))
{
    combatState.AddLogEntry($"⚠️ {target.Name} is INVULNERABLE! No damage dealt.");
    return;  // Skip damage calculation entirely
}

```

### FR-001.6: Phase-Specific Ability Availability

**Requirement**: Each phase SHALL unlock specific abilities while optionally disabling others, creating distinct tactical challenges per phase.

**Ability Availability Configuration** (`BossPhaseDefinition.cs:32-39`):

```csharp
public class BossPhaseDefinition
{
    // Abilities available in this phase
    public List<string> AvailableAbilityIds { get; set; } = new();

    // Abilities disabled in this phase (optional)
    public List<string> DisabledAbilityIds { get; set; } = new();
}

```

**Example** (Ruin-Warden Phase Progression - `BossDatabase.cs:57-119`):

| Phase | HP Range | Available Abilities | Design Intent |
| --- | --- | --- | --- |
| Phase 1 | 100-75% | `warden_slash`, `warden_charge` | Basic attacks only, learning phase |
| Phase 2 | 75-50% | `warden_slash`, `warden_charge`, `system_overload` | Adds telegraphed AoE ability |
| Phase 3 | 50-0% | `warden_slash`, `system_overload`, `total_system_failure` | Unlocks ultimate ability, removes basic charge |

**Ability Unlock Patterns**:

1. **Progressive Escalation** (Ruin-Warden, Omega Sentinel):
    - Phase 1: Standard attacks
    - Phase 2: Add telegraphed abilities
    - Phase 3: Replace standard with ultimate abilities
2. **Tactical Shifts** (Aetheric Aberration):
    - Phase 1: Offensive focus (`void_blast`, `phase_shift`)
    - Phase 2: Add summoning (`summon_echoes`) + AoE (`reality_tear`)
    - Phase 3: Pure devastation (`aetheric_storm` ultimate)
3. **Behavioral Changes** (Forlorn Archivist):
    - Phase 1: Single-target psychic attacks
    - Phase 2: Add summoning + AoE fear
    - Phase 3: Madness-themed ultimate spam

**AI Ability Selection** (during boss turn):

```csharp
// Get abilities available for current phase
var phaseDefinition = repository.GetPhaseDefinition(boss.BossEncounterId, boss.CurrentPhase);
var availableAbilities = phaseDefinition.AvailableAbilityIds;

// Filter out disabled abilities
var validAbilities = allAbilities
    .Where(a => availableAbilities.Contains(a.Id))
    .Where(a => !phaseDefinition.DisabledAbilityIds.Contains(a.Id))
    .ToList();

// Select ability based on AI pattern (telegraphed vs standard)

```

**Design Guidelines**:

- **Phase 1**: 2-3 simple abilities (standard attacks, basic mechanics)
- **Phase 2**: 3-4 abilities (add 1 telegraphed ability + optional summon)
- **Phase 3**: 2-3 abilities (focus on ultimate + signature telegraphed ability)
- **Ability retirement**: Remove "weak" abilities in later phases (e.g., Phase 3 drops basic attacks)

---

### FR-002: Regeneration System

### FR-002.1: Phase-Based Regeneration

**Requirement**: Bosses MAY regenerate HP each turn based on current phase configuration, creating sustained pressure and preventing stalling.

**Regeneration Processing** (`BossEncounterService.cs:412-430`):

```csharp
public string ProcessRegeneration(Enemy boss, int regenAmount)
{
    if (regenAmount <= 0 || boss.HP >= boss.MaxHP)
        return "";

    int healedAmount = Math.Min(regenAmount, boss.MaxHP - boss.HP);
    boss.HP += healedAmount;

    return $"🔄 {boss.Name} regenerates {healedAmount} HP ({boss.HP}/{boss.MaxHP})";
}

```

**Regeneration Triggers**:

- Executed at **end of turn** (after all combatants have acted)
- Only applies if `PhaseDefinition.RegenerationPerTurn > 0`
- Cannot exceed `boss.MaxHP` (healing capped at missing HP)

**Regeneration Scaling by Phase**:

| Phase | Ruin-Warden | Aetheric Aberration | Forlorn Archivist | Omega Sentinel |
| --- | --- | --- | --- | --- |
| Phase 1 | 0 HP/turn | 0 HP/turn | 0 HP/turn | 0 HP/turn |
| Phase 2 | 3 HP/turn | 0 HP/turn | 0 HP/turn | 0 HP/turn |
| Phase 3 | 5 HP/turn | 4 HP/turn | 0 HP/turn | 0 HP/turn |

**Design Constraints**:

- **Regeneration cap**: 3-8 HP/turn (prevents excessive healing)
- **Scaling ratio**: ~5-10% of player damage output per turn
- **Phase gating**: Regeneration typically starts in Phase 2 or 3 (not Phase 1)
- **Boss archetype**: Tanky/mechanical bosses regenerate (Ruin-Warden, Omega Sentinel), psychic bosses don't (Forlorn Archivist)

**Balance Rationale**:

- **Prevents stalling**: Forces player to maintain damage pressure
- **Extends encounter**: Adds 2-4 turns to TTK in later phases
- **Thematic fit**: Mechanical bosses "repair systems", organic bosses don't
- **Counterplay exists**: High burst damage can outpace regeneration

**Example Encounter Math** (Ruin-Warden Phase 3):

```
Boss HP: 80 (starting Phase 3 at 50% HP)
Regeneration: 5 HP/turn
Player Average Damage: 12 HP/turn

Effective Player Damage: 12 - 5 = 7 HP/turn net
Turns to Kill (from 50% HP): 80 / 7 ≈ 11.4 turns
Without Regen: 80 / 12 ≈ 6.7 turns

Impact: +4.7 turns (~70% longer Phase 3 duration)

```

---

### FR-003: Telegraphed Abilities System

### FR-003.1: Boss Ability Types

**Requirement**: Boss abilities SHALL be classified into 4 types, each with distinct mechanics and tactical implications.

**Ability Type Taxonomy** (`BossAbility.cs:6-27`):

| Ability Type | Charge Time | Cooldown | Interruptible | Vulnerability Window | Example |
| --- | --- | --- | --- | --- | --- |
| **Standard** | 0 turns | 0-2 turns | No | No | Electro-Blade Slash, Void Blast |
| **Telegraphed** | 1-2 turns | 3-4 turns | Yes | No | System Overload, Reality Tear |
| **Ultimate** | 2 turns | 5-6 turns | No | Yes (2-3 turns) | Total System Failure, Omega Protocol |
| **Passive** | N/A | N/A | N/A | No | Forlorn Aura, Shield Generator |

**Ability Configuration** (`BossAbility.cs:34-131`):

```csharp
public class BossAbility
{
    public string Id { get; set; }                      // Unique identifier
    public string Name { get; set; }                    // Display name
    public BossAbilityType Type { get; set; }           // Standard | Telegraphed | Ultimate | Passive
    public int ChargeTurns { get; set; } = 0;          // Turns to charge (Telegraphed/Ultimate only)
    public string ChargeMessage { get; set; }           // Displayed when charging starts
    public string ExecuteMessage { get; set; }          // Displayed when ability executes
    public int CooldownTurns { get; set; } = 0;        // Turns before reuse
    public bool CanBeInterrupted { get; set; } = true; // Can player interrupt during charge?
    public bool TriggersVulnerability { get; set; }    // Does execution create vulnerability window?
    public int VulnerabilityDuration { get; set; }     // Turns of vulnerability (Ultimate only)
    public int DamageDice { get; set; }                // Base damage dice
    public int DamageBonus { get; set; }               // Flat damage bonus
    public bool IsAoE { get; set; }                    // Targets all enemies?
    public int MinimumHPPercentage { get; set; }       // HP threshold to unlock (0-100)
    public int MaximumHPPercentage { get; set; } = 100; // HP threshold to disable (0-100)
}

```

**Design Guidelines**:

- **Standard abilities**: 60-70% usage frequency, basic attacks
- **Telegraphed abilities**: 20-30% usage frequency, requires counterplay
- **Ultimate abilities**: 5-10% usage frequency, phase 3+ only, dramatic impact
- **Passive abilities**: Always active, no AI decision required

### FR-003.2: Telegraphed Ability Charging

**Requirement**: Telegraphed abilities SHALL require 1-2 turns of charging before execution, creating readable counterplay windows.

**Telegraph Lifecycle** (`TelegraphedAbilityService.cs:32-69`):

```
Turn N: Boss Begins Telegraph
   ├─ Boss AI decides to use telegraphed ability
   ├─ Telegraph state created in database
   ├─ Charge timer set (ChargeCompleteTurn = CurrentTurn + ChargeTurns)
   ├─ Interrupt threshold set (if interruptible)
   └─ Combat log displays charge warning

Turn N+1 to N+(ChargeTurns-1): Charging State
   ├─ Telegraph status displayed in UI
   ├─ Player can deal damage to interrupt
   ├─ Accumulated interrupt damage tracked
   └─ If interrupted, telegraph cancelled + stagger applied

Turn N+ChargeTurns: Execution
   ├─ Telegraph marked as "ready to execute"
   ├─ ProcessActiveTelegraphs() detects ready state
   ├─ ExecuteTelegraphedAbility() called
   ├─ Damage/effects applied
   └─ Telegraph state cleared from database

```

**Charge Warning Message Format**:

```
⚠️  [WARNING] Ruin-Warden begins charging System Overload!
   ⚡ The Ruin-Warden's core begins to glow with building energy!
   ⏰ Executes in 1 turn(s)!
   🛡️  Can be interrupted with 15+ damage!

```

**Implementation** (`TelegraphedAbilityService.cs:BeginTelegraph()`):

- Creates `BossTelegraphState` entry in database
- Stores: Boss ID, Ability ID, Start Turn, Charge Complete Turn, Interrupt Threshold
- Returns formatted combat log message with warning

**Database Persistence** (prevents save-scumming):

- Telegraph state persists across save/load
- Player cannot "reset" telegraphs by reloading
- Ensures fair counterplay window

### FR-003.3: Interrupt Mechanics

**Requirement**: Telegraphed abilities marked as `CanBeInterrupted = true` SHALL be cancellable if player deals sufficient damage during charge window.

**Interrupt System** (`TelegraphedAbilityService.cs:178-207`):

```csharp
public string? CheckTelegraphInterrupt(Enemy boss, int damageDealt)
{
    var activeTelegraphs = GetActiveTelegraphs(boss.Id);

    foreach (var telegraph in activeTelegraphs)
    {
        if (telegraph.InterruptDamageThreshold <= 0)
            continue;  // Not interruptible

        bool interrupted = repository.AddInterruptDamage(telegraph.TelegraphStateId, damageDealt);

        if (interrupted)
        {
            repository.InterruptTelegraph(telegraph.TelegraphStateId);
            ApplyStaggeredStatus(boss, 2);  // Boss is staggered for 2 turns

            return $"\\n⚡ INTERRUPTED! {boss.Name}'s {ability.AbilityName} was disrupted!\\n" +
                   $"   {boss.Name} is [Staggered] for 2 turn(s)!\\n";
        }
    }
}

```

**Interrupt Threshold Calculation**:

| Boss Tier | Charge Turns | Interrupt Threshold | Rationale |
| --- | --- | --- | --- |
| Low-Tier Boss | 1 turn | 10-15 damage | ~50% of player average attack |
| Mid-Tier Boss | 1 turn | 15-20 damage | ~60% of player average attack |
| High-Tier Boss | 2 turns | 20-25 damage | ~75% of player average attack (accumulated) |
| World Boss | 2 turns | 25-30 damage | ~100% of player average attack (accumulated) |

**Interrupt Mechanics**:

- **Cumulative damage**: Multiple attacks can accumulate toward threshold
- **Per-telegraph tracking**: Each active telegraph tracks its own interrupt damage
- **One-time cancellation**: Interrupting telegraph prevents execution entirely
- **Stagger punishment**: Boss receives [Staggered] status (2 turns, -2 to all attributes)
- **Cooldown reset**: Interrupted abilities go on cooldown as normal

**Stagger Status Effects**:

- **Attribute penalty**: -2 to MIGHT, FINESSE, WILL, WITS, STURDINESS
- **Duration**: 2 turns (decrements end-of-turn)
- **Effect**: Reduced hit chance, damage output, and defense

**Design Rationale**:

- **Skill-based counterplay**: Rewards player aggression during charge window
- **Risk-reward**: Player must choose between safety and damage output
- **Balanced thresholds**: Not trivial (auto-interrupt), not impossible (need perfect crits)
- **Punishment for boss**: Stagger creates temporary advantage for player

**Example Interrupt Scenario**:

```
Turn 5: Ruin-Warden begins charging System Overload (15 damage interrupt threshold)

Turn 5 (Player Turn):
  - Player attacks: 8 damage dealt
  - Accumulated: 8 / 15
  - System Overload still charging

Turn 6 (Player Turn):
  - Player attacks: 9 damage dealt
  - Accumulated: 17 / 15
  - ⚡ INTERRUPTED! System Overload cancelled!
  - Ruin-Warden is [Staggered] for 2 turns!

Turn 7-8: Ruin-Warden attacks with reduced effectiveness due to Stagger

```

### FR-003.4: Vulnerability Windows

**Requirement**: Ultimate abilities (Type = Ultimate) SHALL trigger vulnerability windows after execution, creating counterplay opportunities.

**Vulnerability Window System** (`TelegraphedAbilityService.cs:216-256`):

```csharp
// Apply vulnerability after ultimate ability execution
if (ability.IsUltimate && ability.VulnerabilityDurationTurns > 0)
{
    ApplyVulnerabilityWindow(boss, ability.VulnerabilityDurationTurns, ability.VulnerabilityDamageMultiplier);
}

// Vulnerability mechanics
public void ApplyVulnerabilityWindow(Enemy boss, int duration, float damageMultiplier)
{
    boss.VulnerableTurnsRemaining = duration;
    boss.VulnerabilityDamageMultiplier = damageMultiplier;  // 1.25 to 1.5 (25-50% increased damage)
}

// Process end of turn (decrement counter)
public string? ProcessVulnerabilityWindow(Enemy boss)
{
    if (boss.VulnerableTurnsRemaining > 0)
    {
        boss.VulnerableTurnsRemaining--;
        if (boss.VulnerableTurnsRemaining == 0)
        {
            boss.VulnerabilityDamageMultiplier = 1.0f;  // Reset to normal
            return "{boss.Name}'s vulnerability window has ended.";
        }
    }
}

```

**Vulnerability Configuration by Ultimate Ability**:

| Ability | Boss | Duration (Turns) | Damage Multiplier | Total Bonus Damage Window |
| --- | --- | --- | --- | --- |
| Total System Failure | Ruin-Warden | 2 turns | 1.5x (+50%) | 2-3 player attacks at +50% |
| Aetheric Storm | Aetheric Aberration | 3 turns | 1.25x (+25%) | 3-4 player attacks at +25% |
| Psychic Storm | Forlorn Archivist | 2 turns | 1.5x (+50%) | 2-3 player attacks at +50% |
| Omega Protocol | Omega Sentinel | 3 turns | 1.25x (+25%) | 3-4 player attacks at +25% |

**Vulnerability Window Mechanics**:

- **Damage amplification**: All damage dealt to boss is multiplied by `VulnerabilityDamageMultiplier`
- **Duration countdown**: Decrements by 1 each turn (end-of-turn processing)
- **Combat log notification**: "⚔️ Boss is [VULNERABLE] for N turn(s)! (+X% damage taken)"
- **Visual indicator**: UI shows vulnerability status and turns remaining

**Balance Formula**:

```
Vulnerability Bonus Damage = (Multiplier - 1.0) × Average Player Damage × Duration

Example (Ruin-Warden, 12 avg damage, 2 turns, 1.5x multiplier):
Bonus Damage = (1.5 - 1.0) × 12 × 2 = 0.5 × 24 = 12 extra damage total

```

**Design Rationale**:

- **Risk-reward trade-off**: Boss deals massive damage (ultimate), but becomes vulnerable
- **Counterplay window**: Skilled players can capitalize on vulnerability to burst boss
- **TTK balance**: Vulnerability windows reduce effective boss HP by 10-20%
- **Dramatic pacing**: Creates "DPS check" moments where player must maximize output

### FR-003.5: Boss AI Telegraphed Ability Selection

**Requirement**: Boss AI SHALL select telegraphed abilities probabilistically based on phase-specific AI patterns.

**Telegraphed Ability Decision Logic** (`BossCombatIntegration.cs:190-256`):

```csharp
public BossAbilityData? ShouldBossTelegraph(Enemy boss, int currentTurn)
{
    // 1. Check if boss is already charging an ability (max 1 active telegraph)
    if (IsBossChargingAbility(boss.Id))
        return null;

    // 2. Get AI pattern for current phase
    var aiPattern = repository.GetBossAIPattern(boss.BossEncounterId, boss.CurrentPhase);

    // 3. Roll for telegraph based on frequency (20-40% per turn)
    int telegraphRoll = diceService.RollD100();
    int telegraphChance = (int)(aiPattern.TelegraphFrequency * 100);

    if (telegraphRoll > telegraphChance)
        return null;  // Boss uses standard attack instead

    // 4. Get available telegraphed abilities for current phase
    var telegraphedAbilities = abilities
        .Where(a => a.IsTelegraphed && a.PhaseNumber == boss.CurrentPhase)
        .ToList();

    // 5. Check for ultimate ability (HP threshold-based priority)
    var hpPercent = (float)boss.HP / boss.MaxHP;
    var ultimateAbilities = telegraphedAbilities
        .Where(a => a.IsUltimate && hpPercent <= aiPattern.UltimateHpThreshold)
        .ToList();

    if (ultimateAbilities.Any())
    {
        // Prioritize ultimate when below HP threshold (30-50%)
        return ultimateAbilities[diceService.RollBetween(0, ultimateAbilities.Count - 1)];
    }

    // 6. Select random telegraphed ability
    return telegraphedAbilities[diceService.RollBetween(0, telegraphedAbilities.Count - 1)];
}

```

**Boss AI Pattern Configuration**:

| Boss | Phase | Telegraph Frequency | Ultimate HP Threshold | Priority |
| --- | --- | --- | --- | --- |
| Ruin-Warden | Phase 1 | 20% per turn | N/A (no ultimate) | Standard > Telegraphed |
| Ruin-Warden | Phase 2 | 30% per turn | N/A | Standard > Telegraphed |
| Ruin-Warden | Phase 3 | 40% per turn | 50% HP | Ultimate > Telegraphed > Standard |
| Omega Sentinel | Phase 3 | 50% per turn | 50% HP | Ultimate > Telegraphed > Standard |

**Telegraphed Ability Usage Constraints**:

- **Max 1 active telegraph**: Boss cannot start new telegraph while one is charging
- **Cooldown enforcement**: Abilities on cooldown are excluded from selection
- **Phase restriction**: Only abilities marked for current phase are available
- **HP gating**: Ultimate abilities only unlock below HP threshold (30-50%)

---

### FR-004: Enrage System

### FR-004.1: Enrage Triggers

**Requirement**: Bosses SHALL enter enrage state when HP falls below threshold OR combat duration exceeds turn limit, creating time pressure.

**Enrage Trigger Logic** (`BossEncounterService.cs:266-305`):

```csharp
public string? CheckEnrageConditions(Enemy boss, int currentTurn)
{
    var bossState = repository.GetBossCombatState(boss.Id);
    if (bossState == null || bossState.IsEnraged)
        return null;  // Already enraged or not a boss fight

    var bossConfig = repository.GetBossEncounter(bossState.BossEncounterId);
    bool shouldEnrage = false;
    string enrageReason = "";

    // Check HP-based enrage (primary trigger)
    float hpPercentage = (float)boss.HP / boss.MaxHP;
    if (hpPercentage <= bossConfig.EnrageHpThreshold)
    {
        shouldEnrage = true;
        enrageReason = $"System integrity critical ({(hpPercentage * 100):F0}%)";
    }

    // Check turn-based enrage (optional secondary trigger)
    if (bossConfig.EnrageTurnThreshold.HasValue &&
        currentTurn >= bossConfig.EnrageTurnThreshold.Value)
    {
        shouldEnrage = true;
        enrageReason = $"Combat duration exceeded ({currentTurn} turns)";
    }

    if (shouldEnrage)
    {
        return TriggerEnrage(boss, bossState, bossConfig, currentTurn, enrageReason);
    }
}

```

**Enrage Threshold Configuration**:

| Boss Type | HP-Based Enrage | Turn-Based Enrage | Priority |
| --- | --- | --- | --- |
| Ruin-Warden | 20% HP | None | HP only |
| Aetheric Aberration | 25% HP | None | HP only |
| Forlorn Archivist | 20% HP | None | HP only |
| Omega Sentinel | 20% HP | 20 turns | HP or Turn (whichever first) |

**Design Rationale**:

- **HP-based enrage** (primary): Prevents player from safely whittling boss to 1 HP
- **Turn-based enrage** (optional): Prevents stalling/ultra-defensive strategies
- **Threshold range**: 20-30% HP (balanced between fair warning and pressure)
- **Turn threshold**: 15-20 turns (2-3x expected TTK)

### FR-004.2: Enrage Buffs

**Requirement**: When enrage triggers, bosses SHALL receive substantial stat buffs and mechanical advantages.

**Enrage Buff Application** (`BossEncounterService.cs:307-343`):

```csharp
private string TriggerEnrage(Enemy boss, BossEncounterConfig bossConfig, int currentTurn, string reason)
{
    // Update enrage state in database
    repository.UpdateEnrageState(boss.Id, true, currentTurn);

    // Apply damage multiplier buff
    int originalDamageBonus = boss.DamageBonus;
    boss.DamageBonus += (int)(boss.BaseDamageDice * (bossConfig.EnrageDamageMultiplier - 1.0) * 3);

    // Apply bonus actions (if configured)
    if (bossConfig.EnrageSpeedBonus > 0)
    {
        boss.BonusActionsPerTurn += bossConfig.EnrageSpeedBonus;
    }

    // Apply control immunity
    boss.IsImmuneToControl = true;

    // Build enrage message
    return $"\\n💀 {boss.Name} enters ENRAGE state!\\n" +
           $"⚡ Reason: {reason}\\n" +
           $"⚡ Damage increased by {(int)((bossConfig.EnrageDamageMultiplier - 1.0) * 100)}%!\\n" +
           $"⚡ Gains +{bossConfig.EnrageSpeedBonus} action(s) per turn!\\n" +
           $"⚡ [Control Immunity] - Cannot be stunned or disabled!\\n";
}

```

**Enrage Buff Configuration**:

| Boss | Damage Multiplier | Bonus Actions | Control Immunity | Other Effects |
| --- | --- | --- | --- | --- |
| Ruin-Warden | 1.4x (+40%) | +1 action/turn | Yes | None |
| Aetheric Aberration | 1.5x (+50%) | +1 action/turn | Yes | None |
| Forlorn Archivist | 1.3x (+30%) | +1 action/turn | Yes | Stress aura doubled |
| Omega Sentinel | 1.5x (+50%) | +1 action/turn | Yes | Soak +2 |

**Enrage Mechanics**:

- **Damage boost**: Applied to `DamageBonus` stat (+30-50%)
- **Bonus actions**: +1 action per turn (effectively doubles attack frequency)
- **Control immunity**: Immune to Stunned, Feared, Disoriented, Rooted
- **Permanent**: Enrage persists until boss is defeated (no expiration)
- **One-time trigger**: Enrage can only trigger once per encounter

**Balance Impact** (Ruin-Warden example):

```
Pre-Enrage:
- Damage: 2d6 + 2 = 9 average
- Actions: 1 attack/turn
- DPT: 9 damage per turn

Post-Enrage (40% damage boost, +1 action):
- Damage: 2d6 + 2 + 1 (enrage bonus) = 10 average
- Actions: 2 attacks/turn
- DPT: 20 damage per turn

Effective Damage Increase: ~122% (more than double)

```

**Design Constraints**:

- **Damage multiplier cap**: 1.3x to 1.5x (prevents one-shot scenarios)
- **Bonus actions cap**: +1 action maximum (prevents turn spam)
- **Control immunity**: Standard (prevents trivializing enrage phase)
- **No defensive buffs**: Enrage increases offense, not survivability

---

### FR-005: Boss Loot System

### FR-005.1: Guaranteed Quality Drops

**Requirement**: Bosses SHALL drop guaranteed high-quality equipment (minimum Clan-Forged tier) with configurable chances for higher tiers.

**Loot Generation Logic** (`BossLootService.cs:120-157`):

```csharp
// Roll for quality tier based on loot table percentages
int roll = diceService.RollD100();

if (roll <= lootTable.ArtifactChance)
    return "Artifact";
else if (roll <= lootTable.ArtifactChance + lootTable.RuneCarvedChance)
    return "Rune-Carved";
else
    return "Clan-Forged";  // Guaranteed minimum quality

```

**Boss Loot Table Configuration**:

| Boss | Guaranteed Min | Optimized Chance | Legendary Chance | Artifact Chance | Currency |
| --- | --- | --- | --- | --- | --- |
| Ruin-Warden | Clan-Forged | 30% | 15% | 10% | 300-800 Silver |
| Aetheric Aberration | Clan-Forged | 35% | 20% | 12% | 400-900 Silver |
| Forlorn Archivist | Clan-Forged | 30% | 18% | 15% | 350-850 Silver |
| Omega Sentinel | Optimized | 40% | 25% | 18% | 500-1000 Silver |

**Quality Tier Bonus Allocation** (`BossLootService.cs:178-189`):

| Quality Tier | Attribute Bonuses | Example Distribution |
| --- | --- | --- |
| Clan-Forged | +1 to +2 | +1 MIGHT, +1 FINESSE |
| Rune-Carved (Optimized) | +2 to +3 | +2 MIGHT, +1 WILL |
| Artifact | +3 to +5 | +2 MIGHT, +2 FINESSE, +1 WILL |

**Attribute Bonus Distribution** (randomized per item):

- Bonuses distributed across 6 attributes (MIGHT, FINESSE, WITS, WILL, STURDINESS, Defense)
- Random allocation via `RollD6()` for each bonus point
- Weighted toward combat stats (MIGHT/FINESSE/WILL)

### FR-005.2: Artifact Drop System

**Requirement**: Bosses SHALL have a chance to drop unique artifact items with special effects, scaling with boss tier and player TDR (Total Delve Rating).

**Artifact Drop Mechanics** (`BossLootService.cs:222-286`):

```csharp
// Artifact drop chance based on boss tier
int artifactRoll = diceService.RollD100();
int artifactChance = bossTdr switch
{
    >= 80 => 20%,  // World Boss
    >= 40 => 15%,  // Sector Boss
    >= 20 => 10%,  // Elite
    _ => 5%        // Default
};

if (artifactRoll <= artifactChance)
{
    // Get artifacts available for player's TDR
    var availableArtifacts = repository.GetArtifactsByTDR(bossTdr);
    // Randomly select one
    return artifactData;
}

```

**Artifact Configuration**:

| Boss | Unique Artifacts | Drop Weight | Minimum TDR |
| --- | --- | --- | --- |
| Ruin-Warden | Electro-Blade (Blade weapon) | Weight: 3 | TDR 0 |
| Ruin-Warden | Ancient Power Core (Crafting) | Weight: 2 | TDR 5 |
| Aetheric Aberration | Void-Touched Staff (Staff weapon) | Weight: 3 | TDR 0 |
| Omega Sentinel | (TBD Legendary Artifacts) | Weight: 4 | TDR 10 |

**Artifact Properties**:

- **Unique Effects**: Special abilities not found on normal equipment
- **Set Bonuses**: Can belong to multi-piece equipment sets
- **Higher Stat Bonuses**: +2 to +4 to multiple attributes
- **Flavor Text**: Lore-rich descriptions for narrative immersion

**Example Artifact** (Ruin-Warden's Electro-Blade):

```
Name: Ruin-Warden's Electro-Blade
Type: Weapon (Blade category)
Damage: 3d6 + 2
Special Effect: 25% chance to inflict [Stunned] for 1 turn on hit
Bonuses: +2 FINESSE
Description: "A salvaged blade from the Ruin-Warden, still crackling with residual energy."

```

### FR-005.3: Unique Boss Items (Once-Per-Character)

**Requirement**: Bosses MAY drop unique items that can only be obtained once per character, tracked via database persistence.

**Unique Item Tracking** (`BossLootService.cs:295-371`):

```csharp
// Check if player has already received this unique item
if (config.DropsOncePerCharacter)
{
    if (repository.HasReceivedUniqueItem(characterId, config.ArtifactId))
    {
        continue;  // Skip this item, player already has it
    }
}

// Roll for drop
int dropRoll = diceService.RollD100();
if (dropRoll <= config.DropChance)
{
    // Drop item and record in database
    repository.RecordUniqueItemDrop(characterId, config.ArtifactId);
}

```

**Unique Item Design Pattern**:

- **First kill guaranteed**: 100% drop chance on first boss kill
- **Subsequent kills**: 0% drop chance (once-per-character restriction)
- **Database tracking**: `BossUniqueItemDrops` table links CharacterId + ArtifactId
- **No trading**: Unique items are character-bound

**Use Cases**:

- **Boss-specific weapons**: Signature weapon from each boss
- **Quest items**: Story-critical artifacts
- **Achievement rewards**: Special cosmetics or titles

### FR-005.4: Crafting Materials

**Requirement**: Bosses SHALL drop crafting materials based on configurable loot pools, with rare/epic materials at higher boss tiers.

**Crafting Material Generation** (`BossLootService.cs:380-409`):

```csharp
var craftingMaterialPool = JsonSerializer.Deserialize<List<CraftingMaterialDefinition>>(lootTable.CraftingMaterialPool);

foreach (var definition in pool)
{
    int dropRoll = diceService.RollD100();
    if (dropRoll <= definition.DropChance)
    {
        int count = diceService.RollBetween(definition.QuantityMin, definition.QuantityMax);
        materials.Add(new CraftingMaterial
        {
            MaterialName = definition.MaterialName,
            Count = count
        });
    }
}

```

**Crafting Material Drop Rates** (example - Ruin-Warden):

| Material Tier | Material Name | Drop Chance | Quantity |
| --- | --- | --- | --- |
| Guaranteed | Ancient Circuit Board | 100% | 1 |
| Guaranteed | Structural Scrap | 100% | 3 |
| Rare | Dvergar Alloy Ingot | 40% | 1 |
| Rare | Corrupted Crystal | 30% | 1 |
| Epic | Jötun Core Fragment | 15% | 1 |

**Material Quality Tiers**:

- **Common** (100% drop): Basic components for low-tier crafting
- **Rare** (30-50% drop): Mid-tier crafting, enchantments
- **Epic** (10-20% drop): High-tier crafting, artifact upgrades
- **Legendary** (5-10% drop): Endgame crafting, unique items

### FR-005.5: Set Bonus System

**Requirement**: Artifacts MAY belong to equipment sets, granting cumulative bonuses when multiple pieces are equipped.

**Set Bonus Configuration** (`BossLootService.cs:418-430`):

```csharp
// Display set bonuses when artifact drops
if (!string.IsNullOrEmpty(artifact.SetName))
{
    var setBonuses = repository.GetSetBonuses(artifact.SetName);
    foreach (var bonus in setBonuses.OrderBy(b => b.PiecesRequired))
    {
        logMessage += $"({bonus.PiecesRequired}): {bonus.BonusName} - {bonus.BonusDescription}";
    }
}

```

**Set Bonus Example** (Ruin-Warden's Arsenal Set - hypothetical):

| Pieces Equipped | Bonus Name | Bonus Description |
| --- | --- | --- |
| 2 pieces | Electrical Conductivity | +10% damage with electrical abilities |
| 3 pieces | System Override | Ignore 2 Soak when attacking constructs |
| 4 pieces | Emergency Protocols | When HP < 30%, gain +2 Defense for 3 turns (1/combat) |

**Set Design Principles**:

- **2-piece minimum**: Requires 2 artifacts to activate set bonuses
- **4-piece maximum**: Sets capped at 4 pieces (weapon, armor, accessory 1, accessory 2)
- **Incremental bonuses**: Each additional piece grants stronger bonus
- **Thematic synergy**: Set bonuses align with boss theme (electrical, void, psychic, etc.)

---

### FR-006: Balance Targets & Design Guidelines

### FR-006.1: Time-to-Kill (TTK) Targets

**Requirement**: Boss encounters SHALL target specific TTK ranges based on boss tier and player build, ensuring encounters feel epic but not tedious.

**TTK Target Matrix** (Solo Player, Legend 5, Average Damage 12 HP/turn):

| Boss Tier | HP | Expected TTK (Turns) | With Regen | With Vulnerability | Acceptable Range |
| --- | --- | --- | --- | --- | --- |
| Low-Tier | 60-80 | 8-10 turns | +2-3 turns | -2 turns | 6-13 turns |
| Mid-Tier | 80-100 | 12-14 turns | +3-4 turns | -3 turns | 9-17 turns |
| High-Tier | 100-120 | 14-16 turns | +4-5 turns | -3 turns | 11-19 turns |
| World Boss | 150-200 | 20-25 turns | +6-8 turns | -4 turns | 16-29 turns |

**TTK Calculation Formula**:

```
Base TTK = Boss HP / Player Average DPT
Adjusted TTK = Base TTK + Regen Impact - Vulnerability Impact

Example (Ruin-Warden, 80 HP, Phase 3 with 5 HP/turn regen):
Player DPT = 12
Effective DPT = 12 - 5 (regen) = 7
Phase 3 TTK = 40 HP (50% of 80) / 7 = 5.7 turns
Total TTK = Phases 1+2 (8 turns) + Phase 3 (6 turns) = 14 turns

```

**Design Constraints**:

- **Minimum TTK**: 6 turns (prevents trivial bosses)
- **Maximum TTK**: 30 turns (prevents tedium)
- **Variance tolerance**: ±3 turns from target (accounts for crits, misses, interrupts)

### FR-006.2: Damage Output Balance

**Requirement**: Boss damage output SHALL create threat without one-shotting players or trivializing healing.

**Boss Damage Targets** (vs. Player with 40 HP, 2 Defense):

| Boss Tier | Damage Range | Avg Damage | % of Player HP | Turns to Kill Player |
| --- | --- | --- | --- | --- |
| Low-Tier | 1d6 + 2-4 | 6-7 | 15-18% | 6-7 turns |
| Mid-Tier | 2d6 + 2-4 | 9-10 | 22-25% | 4-5 turns |
| High-Tier | 3d6 + 3-5 | 13-15 | 32-37% | 3-4 turns |
| World Boss | 4d6 + 4-6 | 18-20 | 45-50% | 2-3 turns |

**Enrage Damage Scaling** (+40% typical):

| Boss Tier | Pre-Enrage Avg | Post-Enrage Avg | Turns to Kill Player (Enraged) |
| --- | --- | --- | --- |
| Low-Tier | 6-7 | 8-10 | 4-5 turns |
| Mid-Tier | 9-10 | 13-14 | 3 turns |
| High-Tier | 13-15 | 18-21 | 2 turns |
| World Boss | 18-20 | 25-28 | 2 turns (with +1 action = 1 turn) |

**Design Constraints**:

- **No one-shots**: Maximum single-hit damage ≤ 50% of player HP
- **Enrage pressure**: Enraged bosses should threaten kill in 2-3 turns
- **Healing viability**: Player healing (15-20 HP) should mitigate 1-2 attacks
- **Defensive counterplay**: Defensive stance/buffs should reduce damage by 30-40%

### FR-006.3: Phase Transition Timing

**Requirement**: Phase transitions SHALL occur at regular intervals to create predictable rhythm and tactical planning opportunities.

**Phase Timing Targets**:

| Boss Type | Phase 1 Duration | Phase 2 Duration | Phase 3 Duration | Total Fight |
| --- | --- | --- | --- | --- |
| 3-Phase Boss | 3-5 turns | 4-6 turns | 5-8 turns | 12-19 turns |
| 4-Phase Boss | 3-4 turns | 3-5 turns | 4-6 turns (Phase 3) | 15-22 turns |
|  |  |  | 4-6 turns (Phase 4) |  |

**Phase HP Thresholds** (standard):

- Phase 2: 75% HP
- Phase 3: 50% HP
- Phase 4: 25% HP (rare)

**Design Rationale**:

- **Consistent pacing**: 25% HP intervals create predictable transitions
- **Escalation curve**: Later phases are longer (more dangerous, more mechanics)
- **Player adaptation**: 3-5 turn windows allow players to learn phase mechanics

---

## Appendices

### Appendix A: Boss Stat Reference Tables

### Table A.1: Implemented Boss Statistics

| Boss | Encounter ID | HP | Damage | Defense | Soak | Phases | Enrage HP |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Ruin-Warden | 1 | 80 | 2d6+2 | 4 | 3 | 3 | 20% |
| Aetheric Aberration | 2 | 90 | 3d6+2 | 3 | 2 | 3 | 25% |
| Forlorn Archivist | 3 | 85 | 3d6+1 | 3 | 2 | 3 | 20% |
| Omega Sentinel | 4 | 120 | 4d6+3 | 5 | 4 | 3 | 20% |

### Table A.2: Boss Ability Summary

| Boss | Standard Abilities | Telegraphed Abilities | Ultimate Ability | Total Abilities |
| --- | --- | --- | --- | --- |
| Ruin-Warden | 2 (Slash, Charge) | 1 (System Overload) | 1 (Total System Failure) | 4 |
| Aetheric Aberration | 2 (Void Blast, Phase Shift) | 2 (Reality Tear, Summon Echoes) | 1 (Aetheric Storm) | 5 |
| Forlorn Archivist | 2 (Mind Spike, Psychic Scream) | 2 (Mass Hysteria, Summon Revenants) | 1 (Psychic Storm) | 5 |
| Omega Sentinel | 2 (Maul Strike, Seismic Slam) | 2 (Overcharged Maul, Power Draw) | 1 (Omega Protocol) | 5 |

### Appendix B: Boss Encounter Design Checklist

**Use this checklist when designing new boss encounters:**

**Phase Configuration**:

- [ ]  2-4 phases defined with distinct mechanics
- [ ]  HP thresholds set (75%, 50%, 25%)
- [ ]  Phase transition descriptions written
- [ ]  Invulnerability duration configured (1-2 turns)
- [ ]  Stat modifiers defined per phase (damage, defense, regen)
- [ ]  Add waves configured (0-3 enemies per wave)

**Ability Design**:

- [ ]  2-3 standard abilities (60-70% usage)
- [ ]  1-2 telegraphed abilities (20-30% usage)
- [ ]  1 ultimate ability (5-10% usage, Phase 3+)
- [ ]  Charge times set (1-2 turns for telegraphed/ultimate)
- [ ]  Interrupt thresholds balanced (10-25 damage)
- [ ]  Vulnerability windows configured (2-3 turns, 1.25-1.5x)
- [ ]  Cooldowns set (3-6 turns)

**Enrage Mechanics**:

- [ ]  HP-based enrage threshold (20-30%)
- [ ]  Turn-based enrage threshold (optional, 15-20 turns)
- [ ]  Enrage damage multiplier (1.3-1.5x)
- [ ]  Bonus actions configured (+1 action)
- [ ]  Control immunity enabled

**Loot Configuration**:

- [ ]  Guaranteed quality tier set (minimum Clan-Forged)
- [ ]  Artifact chance configured (10-20%)
- [ ]  Unique items defined (once-per-character)
- [ ]  Crafting materials allocated (guaranteed + rare/epic)
- [ ]  Currency range set (300-1000 Silver)
- [ ]  Set bonuses defined (if applicable)

**Balance Validation**:

- [ ]  TTK calculated (target: 8-25 turns based on tier)
- [ ]  Damage output validated (no one-shots, threat in 3-4 turns)
- [ ]  Phase timings validated (3-8 turns per phase)
- [ ]  Regen impact calculated (+2-8 turns to TTK)
- [ ]  Vulnerability impact calculated (-2-4 turns to TTK)
- [ ]  Enrage damage tested (2-3 turn kill window)

### Appendix C: Full Boss Encounter Example (Ruin-Warden)

**Boss Overview**:

- **Name**: Ruin-Warden
- **Type**: Sector Boss (Electrical/Melee Construct)
- **Encounter ID**: 1
- **HP**: 80 | **Damage**: 2d6+2 | **Defense**: 4 | **Soak**: 3

**Phase 1 (100-75% HP)** - "Combat Initialization":

- **HP Range**: 80-60 HP
- **Duration**: 3-5 turns
- **Abilities**: Electro-Blade Slash (standard), Shield Charge (standard)
- **Mechanics**: Basic attacks only, learning phase
- **Stat Modifiers**: 1.0x damage, +0 defense, 0 regen
- **Invulnerability**: None (Phase 1 start)

**Phase 2 (75-50% HP)** - "System Overload":

- **HP Range**: 60-40 HP
- **Duration**: 4-6 turns
- **Phase Transition**: "⚡ The Ruin-Warden's systems overload! Emergency protocols activated!"
- **Invulnerability**: 1 turn
- **Abilities**: Electro-Blade Slash, Shield Charge, System Overload (telegraphed)
- **Add Wave**: 2x Corrupted Servitor
- **Stat Modifiers**: 1.2x damage (+20%), +2 defense, 3 HP/turn regen
- **Mechanics**: Unlocks telegraphed AoE ability

**Phase 3 (50-0% HP)** - "Critical State":

- **HP Range**: 40-0 HP
- **Duration**: 5-8 turns
- **Phase Transition**: "💀 The Ruin-Warden enters CRITICAL STATE! All systems overclocked!"
- **Invulnerability**: 2 turns
- **Abilities**: Electro-Blade Slash, System Overload, Total System Failure (ultimate)
- **Stat Modifiers**: 1.5x damage (+50%), +3 defense, 5 HP/turn regen, +1 bonus action/turn
- **Mechanics**: Unlocks ultimate ability, gains double actions

**Enrage (20% HP or lower)**:

- **Trigger**: HP ≤ 16 HP
- **Message**: "💀 Ruin-Warden enters ENRAGE state! System integrity critical!"
- **Buffs**: 1.4x damage (+40%), +1 action/turn, control immunity
- **Effective DPT**: ~20 damage/turn (doubled actions)

**Loot Table**:

- **Guaranteed**: 1x Clan-Forged quality item
- **Optimized Chance**: 30%
- **Artifact Chance**: 10%
- **Unique Items**: Ruin-Warden's Electro-Blade (once-per-character)
- **Currency**: 300-800 Silver Marks
- **Crafting Materials**: Ancient Circuit Board x1, Structural Scrap x3, Dvergar Alloy Ingot (40%), Corrupted Crystal (30%)

**Encounter Summary**:

- **Total TTK**: ~14 turns (Phase 1: 4 turns, Phase 2: 5 turns, Phase 3: 5 turns)
- **Damage Taken**: ~126 total damage (from 14 boss attacks at 9 avg damage)
- **Healing Required**: 2-3 healing items (15-20 HP each)
- **Difficulty**: Medium (sector boss, Legend 5 player)

---

## Revision History

| Version | Date | Author | Changes |
| --- | --- | --- | --- |
| 1.0.0 | 2025-11-22 | AI Specification Agent | Initial draft based on v0.23 boss system implementation |

---

**End of Specification**