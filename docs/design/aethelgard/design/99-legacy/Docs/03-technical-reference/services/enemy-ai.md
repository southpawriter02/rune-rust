# EnemyAI Service

**File Path:** `RuneAndRust.Engine/EnemyAI.cs`
**Version:** v0.42.1
**Last Updated:** 2025-11-27
**Status:** ✅ Implemented

---

## Overview

The `EnemyAI` service handles decision-making for all enemy types during combat. It determines which action each enemy takes based on their type, current HP percentage (phase), and weighted random selection. The system supports 20+ enemy types with unique behavior patterns.

---

## Architecture

### Decision Model

```
┌─────────────────────────────────────────────────────────────┐
│                      EnemyAI                                │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│   Enemy Type ──► Behavior Pattern ──► Weighted Action       │
│                                                             │
│   ┌─────────────────────────────────────────────────────┐   │
│   │ Phase Detection (HP %)                              │   │
│   │  • Phase 1: >50% HP (cautious/standard)             │   │
│   │  • Phase 2: ≤50% HP (aggressive/desperate)          │   │
│   │  • Phase 3: ≤30% HP (some bosses)                   │   │
│   └─────────────────────────────────────────────────────┘   │
│                                                             │
│   ┌─────────────────────────────────────────────────────┐   │
│   │ Weighted Random Selection                           │   │
│   │  • Each action has percentage chance                │   │
│   │  • Phase changes modify probabilities               │   │
│   │  • Special abilities may be one-time use            │   │
│   └─────────────────────────────────────────────────────┘   │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### Dependencies

| Service | Version | Purpose |
|---------|---------|---------|
| `DiceService` | v0.1 | Random number generation |
| `FlankingService` | v0.20.1 | Flanking calculations |
| `CoverService` | v0.20.2 | Cover mechanics |
| `StanceService` | v0.21.1 | Stance modifications |
| `AdvancedStatusEffectService` | v0.21.3 | Status effect application |
| `CounterAttackService` | v0.21.4 | Parry/riposte handling |
| `CombatFlavorTextService` | v0.38.12 | Action flavor text |

---

## Public API

### `DetermineAction`

Determines what action an enemy should take this turn.

```csharp
public EnemyAction DetermineAction(Enemy enemy)
```

**Parameters:**
- `enemy` - The enemy making a decision

**Returns:** `EnemyAction` enum value

**Special Cases:**
- If enemy is stunned, returns `BasicAttack` (handled specially in execution)
- Unknown enemy types default to `BasicAttack`

---

## Enemy Action Enum

### Base Actions (All Enemies)

| Action | Description |
|--------|-------------|
| `BasicAttack` | Standard melee/ranged attack |
| `Defend` | Defensive stance |
| `RapidStrike` | Fast attack, reduced damage |
| `HeavyStrike` | Slow attack, increased damage |
| `BerserkStrike` | Reckless attack, self-damage |
| `ChargeDefense` | Build up defensive buff |
| `EmergencyRepairs` | Self-heal (mechanical enemies) |

### Enemy-Specific Actions

#### Scrap-Hound (v0.4)
| Action | Chance | Description |
|--------|--------|-------------|
| `QuickBite` | 70% | Attack twice, -1 die each |
| `DartAway` | 30% | Move and gain evasion |

#### Test Subject (v0.4)
| Action | Chance | Description |
|--------|--------|-------------|
| `FergeralStrike` | 60% | MIGHT + 2 dice attack |
| `BerserkerRush` | 30% | Immediate attack, skip next turn |
| `Shriek` | 10% | Buff allies |

#### War-Frame (v0.4)
| Action | Chance | Description |
|--------|--------|-------------|
| `PrecisionStrike` | 40% | Tactical attack with accuracy |
| `SuppressionFire` | 30% | AOE attack |
| `TacticalReposition` | 30% | Move and defense boost |

#### Forlorn Scholar (v0.4)
| Action | Chance | Description |
|--------|--------|-------------|
| `AethericBolt` | 50% | Ranged magic attack |
| `RealityDistortion` | 30% | Disable attack |
| `PhaseShift` | 20% | High evasion |

#### Aetheric Aberration (v0.4) - Boss
**Phase 1 (>50% HP):**
| Action | Chance | Description |
|--------|--------|-------------|
| `VoidBlast` | 40% | High damage magic |
| `SummonEchoes` | 30% | Spawn adds |
| `RealityTear` | 20% | AOE magic damage |
| `PhaseShift` | 10% | High evasion |

**Phase 2 (≤50% HP):**
| Action | Chance | Description |
|--------|--------|-------------|
| `AethericStorm` | 40% | AOE attack |
| `VoidBlast` | 40% | High damage magic |
| `DesperateSummon` | 20% | Spawn add |

#### Corrupted Engineer (v0.6)
| Action | Chance | Description |
|--------|--------|-------------|
| `OverchargeAlly` | 40% | Buff ally damage |
| `EmergencyRepairAlly` | 20% | Heal ally |
| `ArcDischarge` | 30% | Ranged electrical attack |
| `SystemShock` | 10% | Stun attack |

#### Vault Custodian (v0.6) - Mini-Boss
**Phase 1 (>50% HP):**
| Action | Chance | Description |
|--------|--------|-------------|
| `HalberdSweep` | 50% | Single target attack |
| `DefensiveStance` | 30% | Defense mode |
| `GuardianProtocol` | 20% | Self-heal |

**Phase 2 (≤50% HP):**
| Action | Chance | Description |
|--------|--------|-------------|
| `WhirlwindStrike` | 40% | AOE attack |
| `HalberdSweep` | 40% | Single target attack |
| `LastStand` | 20% | Buff all attacks (one-time at 25% HP) |

#### Omega Sentinel (v0.6) - Boss
| Action | Description |
|--------|-------------|
| `MaulStrike` | Single target slam |
| `SeismicSlam` | AOE knockback |
| `PowerDraw` | Self-heal from power core |
| `OverchargedMaul` | Enhanced attack (Phase 2) |
| `DefensiveProtocols` | Defense buff (Phase 2) |
| `OmegaProtocol` | Ultimate attack (Phase 3) |

---

## Behavior Patterns

### Simple Enemies

Basic enemies use fixed probability tables:

```csharp
private EnemyAction DetermineServitorAction()
{
    var roll = _random.Next(100);

    if (roll < 80)      // 80% chance
        return EnemyAction.BasicAttack;
    else                // 20% chance
        return EnemyAction.Defend;
}
```

### Phase-Based Enemies

Complex enemies change behavior based on HP:

```csharp
private EnemyAction DetermineWardenAction(Enemy warden)
{
    var hpPercent = (double)warden.HP / warden.MaxHP;

    if (hpPercent > 0.5)
    {
        // Phase 1: Cautious
        // 50% HeavyStrike, 30% BasicAttack, 20% ChargeDefense
    }
    else
    {
        // Phase 2: Desperate
        // 40% BerserkStrike, 40% HeavyStrike, 20% EmergencyRepairs
    }
}
```

### Special Ability Tracking

Some enemies have one-time abilities:

```csharp
if (hpPercent <= 0.5 && !construct.HasUsedSpecialAbility)
{
    construct.HasUsedSpecialAbility = true;
    return EnemyAction.RepairProtocol;  // One-time emergency heal
}
```

---

## Enemy Types by Version

### v0.1-v0.3 (Base Game)
- `CorruptedServitor` - Basic melee
- `BlightDrone` - Fast ranged
- `RuinWarden` - Mini-boss, phase-based

### v0.4 (New Enemies)
- `ScrapHound` - Fast, flees when hurt
- `TestSubject` - Berserker
- `WarFrame` - Tactical ranged
- `ForlornScholar` - Magic user
- `AethericAberration` - Boss with adds

### v0.6 (The Lower Depths)
- `MaintenanceConstruct` - Self-healing
- `SludgeCrawler` - Poison, terrain-aware
- `CorruptedEngineer` - Support/healer
- `VaultCustodian` - Mini-boss guardian
- `ForlornArchivist` - Psychic boss
- `OmegaSentinel` - Major boss

---

## Integration Points

### Called By

| Caller | Context |
|--------|---------|
| `CombatEngine.NextTurn()` | Determines enemy action during their turn |

### Works With

| Service | Interaction |
|---------|-------------|
| `CombatEngine` | Executes chosen actions |
| `FlankingService` | Tactical positioning decisions |
| `CoverService` | Cover-seeking behavior |
| `StatusEffectService` | Applies status effects from actions |

---

## Data Flow

### Action Selection Flow

```
CombatEngine.NextTurn()
        │
        ▼
┌───────────────────────┐
│ Is enemy stunned?     │
└───────────┬───────────┘
            │
    Yes     │     No
    │       │
    ▼       ▼
Return   ┌───────────────────────┐
Basic    │ Get enemy type        │
Attack   └───────────┬───────────┘
                     │
                     ▼
         ┌───────────────────────┐
         │ Switch on EnemyType   │
         │ → Specific AI method  │
         └───────────┬───────────┘
                     │
                     ▼
         ┌───────────────────────┐
         │ Calculate HP %        │
         │ (Phase detection)     │
         └───────────┬───────────┘
                     │
                     ▼
         ┌───────────────────────┐
         │ Random roll (0-99)    │
         └───────────┬───────────┘
                     │
                     ▼
         ┌───────────────────────┐
         │ Select action based   │
         │ on probability table  │
         └───────────┬───────────┘
                     │
                     ▼
              Return EnemyAction
```

---

## Design Patterns

### Weighted Random Selection

All AI methods use weighted random for variety:

```csharp
var roll = _random.Next(100);

if (roll < 40)        // 0-39  (40%)
    return Action1;
else if (roll < 70)   // 40-69 (30%)
    return Action2;
else if (roll < 90)   // 70-89 (20%)
    return Action3;
else                  // 90-99 (10%)
    return Action4;
```

### Health-Based Phase Transitions

Enemies become more aggressive as HP decreases:

| HP Range | Behavior Pattern |
|----------|------------------|
| >50% | Standard attacks, occasional defense |
| 25-50% | More special abilities, less defense |
| <25% | Desperate attacks, last stand abilities |

---

## Version History

| Version | Changes |
|---------|---------|
| v0.1 | Basic AI for Servitor, Drone, Warden |
| v0.4 | Added 5 new enemy types with unique behaviors |
| v0.6 | Added Lower Depths enemies, phase-based bosses |
| v0.20.1 | Integrated FlankingService |
| v0.20.2 | Integrated CoverService |
| v0.21.1 | Integrated StanceService |
| v0.38.12 | Added combat flavor text integration |
| v0.42.1 | Enhanced tactical AI services |

---

## Cross-References

### Related Documentation

- [Combat Resolution](../../01-systems/combat-resolution.md) - Combat flow
- [Enemy Bestiary](../../02-statistical-registry/enemies-registry.md) - Enemy stats

### Related Services

- [CombatEngine](./combat-engine.md) - Executes AI decisions
- [DiceService](./dice-service.md) - Random action selection

---

**Documentation Status:** ✅ Complete
**Last Reviewed:** 2025-11-27
