# CombatEngine Service

Parent item: Service Architecture Overview (Service%20Architecture%20Overview%202ba55eb312da80a18965d6f5e87a15ec.md)

**File Path:** `RuneAndRust.Engine/CombatEngine.cs`, `CombatEngine_GaldrFlavorExtensions.cs`**Version:** v0.38.12
**Last Updated:** 2025-11-27
**Status:** ✅ Implemented

---

## Overview

The `CombatEngine` is the central orchestrator for all combat interactions in Rune & Rust. It manages turn-based combat flow, player actions, enemy AI coordination, companion participation, and integrates with numerous supporting services for tactical gameplay features.

---

## Architecture

### Class Structure

```csharp
// Partial class split across two files for maintainability
public partial class CombatEngine  // Main file: CombatEngine.cs
public partial class CombatEngine  // Extension: CombatEngine_GaldrFlavorExtensions.cs

```

### Dependencies

The CombatEngine integrates with 17 services:

| Service | Version | Purpose |
| --- | --- | --- |
| `DiceService` | v0.1 | Core dice rolling mechanics |
| `SagaService` | v0.1 | Performance/saga system integration |
| `LootService` | v0.1 | Post-combat loot generation |
| `EquipmentService` | v0.3 | Weapon stats, attribute bonuses |
| `HazardService` | v0.6 | Environmental hazard damage |
| `PerformanceService` | v0.7 | Combat metrics tracking |
| `CurrencyService` | v0.9 | Currency rewards |
| `GridInitializationService` | v0.20 | Tactical grid setup |
| `FlankingService` | v0.20.1 | Flanking bonus calculations |
| `CoverService` | v0.20.2 | Cover mechanics |
| `StanceService` | v0.21.1 | Combat stance modifiers |
| `AdvancedStatusEffectService` | v0.21.3 | Status effect management |
| `CounterAttackService` | v0.21.4 | Parry/riposte mechanics |
| `CompanionService` | v0.34.4 | Party companion management |
| `TerritoryService` | v0.35 | Territorial faction tracking |
| `CombatFlavorTextService` | v0.38.6 | Dynamic combat descriptions |
| `GaldrFlavorTextService` | v0.38.7 | Magic ability flavor text |

### Dependency Diagram

```
                    ┌─────────────────────┐
                    │    CombatEngine     │
                    └──────────┬──────────┘
                               │
    ┌──────────────────────────┼──────────────────────────┐
    │                          │                          │
    ▼                          ▼                          ▼
┌─────────┐            ┌───────────────┐          ┌──────────────┐
│  Core   │            │   Tactical    │          │   Support    │
│Services │            │   Services    │          │   Services   │
└────┬────┘            └───────┬───────┘          └──────┬───────┘
     │                         │                         │
     ├─ DiceService            ├─ GridInitService        ├─ LootService
     ├─ EquipmentService       ├─ FlankingService        ├─ CurrencyService
     ├─ SagaService            ├─ CoverService           ├─ CompanionService
     └─ HazardService          ├─ StanceService          ├─ TerritoryService
                               └─ StatusEffectService    └─ FlavorTextServices

```

---

## Public API

### Combat Lifecycle

### `InitializeCombat`

Initializes a new combat encounter with the player, enemies, and optional context.

```csharp
public CombatState InitializeCombat(
    PlayerCharacter player,
    List<Enemy> enemies,
    Room? currentRoom = null,
    bool canFlee = true,
    int characterId = 0)

```

**Parameters:**

- `player` - The player character entering combat
- `enemies` - List of enemies to fight
- `currentRoom` - Optional room context for environmental hazards
- `canFlee` - Whether fleeing is allowed (false for boss fights)
- `characterId` - Database ID for loading companions

**Returns:** Initialized `CombatState` with initiative order determined

**Behavior:**

1. Creates `CombatState` with player/enemies
2. Loads party companions via `CompanionService` (if characterId > 0)
3. Initializes tactical grid via `GridInitializationService`
4. Applies environmental features from room
5. Rolls initiative for all participants (player, companions, enemies)
6. Sorts by initiative score (ties broken by FINESSE attribute)
7. Logs environmental hazard warnings if present

---

### `NextTurn`

Advances combat to the next turn in initiative order.

```csharp
public void NextTurn(CombatState combatState)

```

**Behavior:**

1. Processes environmental hazard damage (if active room hazard)
2. Ticks down status effects (defense bonuses, analyzed, etc.)
3. Advances to next participant in initiative order
4. If enemy turn: triggers `EnemyAI` decision-making
5. If companion turn: triggers companion AI
6. Regenerates player stamina (base + stance bonus)

---

### `IsCombatOver`

Checks if combat should end.

```csharp
public bool IsCombatOver(CombatState combatState)

```

**Returns:** `true` if:

- All enemies defeated (victory)
- Player HP ≤ 0 (defeat)
- Player successfully fled

---

### Player Actions

### `PlayerAttack`

Processes a standard attack against an enemy.

```csharp
public void PlayerAttack(CombatState combatState, Enemy target)

```

**Attack Resolution Flow:**

1. Get weapon stats (attribute, damage dice, accuracy bonus)
2. Calculate bonus dice:
    - Battle Rage (+2 if active)
    - Accuracy from equipment
    - [Analyzed] status on target (+2)
    - Saga of Courage performance (+2)
    - Flanking bonus (from `FlankingService`)
    - High ground bonus (+2 if attacker elevation > target)
3. Roll attack: `(attribute + bonusDice)d6`
4. Check for fumble (0 successes with any 1s)
5. Calculate target defense:
    - Base: target's STURDINESS
    - Flanking penalty (from `FlankingService`)
    - Cover bonus (from `CoverService`)
    - High ground bonus (+2 if defender elevation > attacker)
6. Roll defense: `(adjusted defense)d6`
7. Calculate damage: `netSuccesses > 0` → roll weapon damage dice
8. Apply critical hit (base 5% + flanking bonus) → double damage dice
9. Apply stance damage bonus
10. Apply target's defense reduction (if defending)
11. Generate flavor text (if `CombatFlavorTextService` available)
12. Check for enemy defeat

---

### `PlayerDefend`

Takes a defensive stance to reduce incoming damage.

```csharp
public void PlayerDefend(CombatState combatState)

```

**Mechanics:**

- Roll: `STURDINESS d6`
- Each success = 25% damage reduction (max 75%)
- Lasts until next turn

---

### `PrepareParry`

Prepares a parry reaction for the next incoming attack.

```csharp
public void PrepareParry(CombatState combatState)

```

**Requirements:**

- `CounterAttackService` must be available
- Player must have parry attempts remaining this turn

**Mechanics:**

- Marks `player.ParryReactionPrepared = true`
- Parry attempts limited per round (based on abilities)
- Superior Riposte ability enables counter-attack on successful parry

---

### `PlayerUseAbility`

Uses a special ability in combat.

```csharp
public bool PlayerUseAbility(
    CombatState combatState,
    Ability ability,
    Enemy? target = null)

```

**Cost Types:**

- AP (Aether Pool) - Mystic abilities
- Stamina - Warrior/Adept abilities

**Special Handling:**

- Rúnasmiðr trap abilities → `RunasmidrAbilityHandler`
- Heretical abilities → Trauma/Corruption costs via `TraumaEconomyService`
- Galdr abilities → Flavor text via `GaldrFlavorTextService`

**Returns:** `false` if insufficient resources or ability fails

---

### `PlayerUseConsumable`

Uses a consumable item during combat.

```csharp
public bool PlayerUseConsumable(CombatState combatState, Consumable consumable)

```

**Effects Applied:**

- HP restoration
- Stamina restoration
- Stress reduction
- Temp HP grant
- Status effect clearing (bleeding, poison, disease)

---

### `PlayerFlee`

Attempts to escape from combat.

```csharp
public bool PlayerFlee(CombatState combatState)

```

**Requirements:**

- `combatState.CanFlee` must be `true`

**Returns:** `true` if escape successful

---

### Post-Combat

### `GenerateLoot`

Generates loot from defeated enemies.

```csharp
public void GenerateLoot(CombatState combatState, Room room)

```

**Delegates to:** `LootService`

---

### `AwardCombatLegend`

Awards Legend XP for combat completion.

```csharp
public bool AwardCombatLegend(CombatState combatState, float traumaMod = 1.0f)

```

**Parameters:**

- `traumaMod` - Multiplier for trauma-based bonuses (default 1.0)

---

### Companion Support

### `ProcessCompanionTurn`

Processes a companion's turn using companion AI.

```csharp
public void ProcessCompanionTurn(CombatState combatState, Companion companion)

```

---

### `IsCompanionTurn`

Checks if the current turn belongs to a companion.

```csharp
public bool IsCompanionTurn(CombatState combatState)

```

---

### `DamageCompanion`

Applies damage to a companion.

```csharp
public void DamageCompanion(CombatState combatState, Companion companion, int damage)

```

---

### `RecoverCompanionsAfterCombat`

Recovers companions after combat ends.

```csharp
public void RecoverCompanionsAfterCombat(CombatState combatState)

```

---

## Key Models

### CombatState

Central state object tracking all combat information.

```csharp
public class CombatState
{
    public PlayerCharacter Player { get; set; }
    public List<Enemy> Enemies { get; set; }
    public List<Companion> Companions { get; set; }      // v0.34.4
    public List<CombatParticipant> InitiativeOrder { get; set; }
    public CombatGrid? Grid { get; set; }                // v0.20
    public Room? CurrentRoom { get; set; }
    public bool IsActive { get; set; }
    public bool CanFlee { get; set; }
    public int CharacterId { get; set; }
    public int PlayerNextAttackBonusDice { get; set; }
    public List<string> CombatLog { get; }
}

```

### CombatParticipant

Represents a participant in the initiative order.

```csharp
public class CombatParticipant
{
    public string Name { get; set; }
    public bool IsPlayer { get; set; }
    public bool IsCompanion { get; set; }               // v0.34.4
    public int InitiativeRoll { get; set; }
    public int InitiativeAttribute { get; set; }
    public object? Character { get; set; }              // Player, Enemy, or Companion
}

```

---

## Integration Points

### Called By

| Caller | Context |
| --- | --- |
| `Program.cs` (ConsoleApp) | Main game loop combat handling |
| `CombatController` (DesktopUI) | Avalonia UI combat controller |
| `EnemyAI` | Enemy decision-making triggers CombatEngine methods |

### Calls Into

| Service | Methods Used |
| --- | --- |
| `DiceService` | `Roll()`, `RollDamage()` |
| `EquipmentService` | `GetEffectiveAttributeValue()` |
| `FlankingService` | `CalculateFlankingBonus()` |
| `CoverService` | `CalculateCoverBonus()`, `DamageCover()` |
| `StanceService` | `GetFlatDamageBonus()` |
| `HazardService` | `RoomHasActiveHazard()`, hazard damage |
| `StatusEffectService` | Status application/tick |
| `CompanionService` | `GetPartyCompanions()` |
| `TerritoryService` | Territorial kill tracking |
| `LootService` | Post-combat loot generation |

---

## Data Flow

### Combat Turn Flow

```
Player Turn Start
       │
       ▼
┌──────────────────┐
│ Environmental    │──► Hazard damage applied
│ Hazard Check     │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│ Status Effect    │──► Tick durations, apply DoTs
│ Processing       │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐    ┌─────────────┐
│ Player Action    │───►│ Attack      │──► Damage resolution
│ Selection        │    │ Defend      │──► Defense buff
│                  │    │ Ability     │──► Ability effects
│                  │    │ Consumable  │──► Item effects
│                  │    │ Flee        │──► Escape attempt
└────────┬─────────┘    └─────────────┘
         │
         ▼
┌──────────────────┐
│ Combat End Check │──► Victory/Defeat/Flee
└────────┬─────────┘
         │
         ▼
    Next Turn (Enemy/Companion)

```

---

## Version History

| Version | Changes |
| --- | --- |
| v0.1 | Initial combat system |
| v0.3 | Equipment system integration |
| v0.6 | Environmental hazard support |
| v0.7 | Consumables, performance tracking, Saga integration |
| v0.9 | Currency rewards |
| v0.20 | Tactical grid system, flanking |
| v0.20.2 | Cover mechanics |
| v0.21.1 | Stance system |
| v0.21.3 | Advanced status effects |
| v0.21.4 | Parry/counter-attack system |
| v0.34.4 | Companion system integration |
| v0.35 | Territory/faction tracking |
| v0.38.6 | Combat flavor text |
| v0.38.7 | Made partial class, Galdr flavor extensions |
| v0.38.12 | Fumble system, critical hit flavor text |

---

## Cross-References

### Related Documentation

- [Combat Resolution System](https://www.notion.so/01-systems/combat-resolution.md) - Functional overview
- [Damage Calculation](https://www.notion.so/01-systems/damage-calculation.md) - Damage formulas
- [Status Effects](https://www.notion.so/01-systems/status-effects.md) - Status effect mechanics
- [Accuracy & Evasion](https://www.notion.so/01-systems/accuracy-evasion.md) - Hit/miss calculations

### Related Services

- [DiceService](https://www.notion.so/dice-service.md) - Core dice mechanics
- [EnemyAI](https://www.notion.so/enemy-ai.md) - Enemy decision-making
- [FlankingService](https://www.notion.so/flanking-service.md) - Flanking calculations
- [CoverService](https://www.notion.so/cover-service.md) - Cover mechanics

---

**Documentation Status:** ✅ Complete
**Last Reviewed:** 2025-11-27