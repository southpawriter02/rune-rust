# Combat Resolution System — Core System Specification v5.0

Type: Core System
Description: Core turn-based tactical combat loop orchestrating initiative, turn sequencing, action resolution, victory/defeat conditions, and integration with damage, accuracy, status effect, and counter-attack subsystems.
Priority: Must-Have
Status: Review
Target Version: Alpha
Dependencies: Dice Pool System, Attribute System (MIGHT/FINESSE/WILL/STURDINESS), Room/Hazard System, Character System
Implementation Difficulty: Very Complex
Balance Validated: No
Document ID: AAM-SPEC-CORE-COMBAT-v5.0
Proof-of-Concept Flag: No
Related Projects: (PROJECT) Game Specification Consolidation & Standardization (https://www.notion.so/PROJECT-Game-Specification-Consolidation-Standardization-e1d0c8b2ea2042f9b9c08471c6077c92?pvs=21)
Session Handoffs: Consolidation Work — Phase 1A Core Systems Audit Complete (https://www.notion.so/Consolidation-Work-Phase-1A-Core-Systems-Audit-Complete-0c51f0058f3a478fb7bc6a2c192cac2a?pvs=21)
Sub-Type: Core
Sub-item: Accuracy & Evasion — Mechanic Specification v5.0 (Accuracy%20&%20Evasion%20%E2%80%94%20Mechanic%20Specification%20v5%200%20114392e33e9c489f8266ffbaa3775b99.md), Counter-Attack & Parry — Mechanic Specification v5.0 (Counter-Attack%20&%20Parry%20%E2%80%94%20Mechanic%20Specification%20v5%20b811af65e68846eea6edd086e520c890.md), Damage Calculation — Mechanic Specification v5.0 (Damage%20Calculation%20%E2%80%94%20Mechanic%20Specification%20v5%200%200cd87ac6028d44e98fca58e733c36631.md)
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: Medium
Voice Layer: Layer 4 (Design)
Voice Validated: No

## Layer 1 — Mythic/Narrative Voice

> *"Combat in Rune & Rust is not merely an exchange of blows—it is a crucible where legends are forged. Each clash of steel, each desperate parry, each calculated strike carries the weight of choice. The wheel of initiative turns, granting moments of action to all who stand upon the field. In these moments, warriors rise or fall, not by fate alone, but by the decisions they make when death's breath touches their neck."*
> 

### Design Intent (Combat Philosophy)

The Combat Resolution System embodies the core tension of Rune & Rust: **tactical depth within accessible mechanics**. Combat should feel dangerous but fair—a puzzle where positioning, resource management, and timing matter as much as raw power.

**Core Pillars:**

1. **Meaningful Choices** — Every action carries opportunity cost; attacking forgoes defense, parrying forgoes offense
2. **Tactical Positioning** — Grid-based combat rewards flanking, cover usage, and elevation advantages
3. **Resource Tension** — Stamina, AP, and HP create difficult tradeoffs between aggression and survival
4. **Dramatic Reversals** — Critical hits, ripostes, and fumbles create memorable combat moments
5. **Stress Integration** — Combat outcomes ripple into the Trauma Economy, connecting physical and psychological states

---

## Layer 2 — Diagnostic/Functional Overview

### System Purpose

The Combat Resolution System is the **central orchestrator** for all combat interactions. It manages:

- Turn-based initiative sequencing
- Player action resolution (attack, defend, ability, item, flee, parry)
- Enemy AI coordination and companion participation
- Victory/defeat condition detection
- Integration with tactical subsystems (flanking, cover, stances, status effects)

### Upstream Dependencies

| System | Dependency Type | Integration Point |
| --- | --- | --- |
| **Dice Pool System** | Core | All attack/defense rolls use d6 dice pools |
| **Attribute System** | Core | MIGHT/FINESSE/WILL/STURDINESS drive combat calculations |
| **Equipment System** | Required | Weapon stats, armor soak, attribute bonuses |
| **Room/Hazard System** | Optional | Environmental hazard damage during combat |
| **Character System** | Core | Player/companion stats, HP, resources |

### Downstream Consumers

| System | Consumption Type | Data Flow |
| --- | --- | --- |
| **Damage Calculation** | Sub-system | Attack success → damage resolution |
| **Accuracy & Evasion** | Sub-system | Hit/miss determination before damage |
| **Counter-Attack/Parry** | Sub-system | Reactive defense mechanics |
| **Status Effect System** | Integration | Effect application, tick processing |
| **Trauma Economy** | Integration | Stress changes from combat outcomes |
| **Loot System** | Post-combat | Enemy defeat triggers loot generation |
| **Legend/XP System** | Post-combat | Combat completion awards progression |

### Combat State Machine

```
┌─────────────────────────────────────────────────────────────┐
│                    COMBAT LIFECYCLE                         │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│ INITIALIZATION                                              │
│ • Load player, enemies, companions                          │
│ • Initialize tactical grid                                  │
│ • Apply room environmental features                         │
│ • Roll initiative for all participants                      │
│ • Sort initiative order (ties → FINESSE)                    │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│ TURN LOOP                                                   │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ Turn Start                                              │ │
│ │ • Process environmental hazards                         │ │
│ │ • Tick status effect durations                          │ │
│ │ • Regenerate resources (stamina)                        │ │
│ │ • Reset per-turn limits (parry attempts)                │ │
│ └─────────────────────────────────────────────────────────┘ │
│                         │                                   │
│                         ▼                                   │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ Action Phase                                            │ │
│ │ • Player: Choose action (attack/defend/ability/etc.)    │ │
│ │ • Enemy: AI selects action based on threat assessment   │ │
│ │ • Companion: AI executes role-appropriate action        │ │
│ └─────────────────────────────────────────────────────────┘ │
│                         │                                   │
│                         ▼                                   │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ Resolution Phase                                        │ │
│ │ • Execute action (damage, effects, movement)            │ │
│ │ • Check reactive triggers (parry, riposte)              │ │
│ │ • Apply stress changes                                  │ │
│ │ • Update combat log                                     │ │
│ └─────────────────────────────────────────────────────────┘ │
│                         │                                   │
│                         ▼                                   │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ End Check                                               │ │
│ │ • All enemies defeated? → VICTORY                       │ │
│ │ • Player HP ≤ 0? → DEFEAT                               │ │
│ │ • Player fled? → ESCAPE                                 │ │
│ │ • Otherwise → Next participant in initiative            │ │
│ └─────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│ POST-COMBAT                                                 │
│ • Generate loot (LootService)                               │
│ • Award Legend XP                                           │
│ • Award currency                                            │
│ • Recover companions                                        │
│ • Update territory/faction standings                        │
│ • Process trauma outcomes                                   │
└─────────────────────────────────────────────────────────────┘
```

### Player Action Types

| Action | Cost | Effect | Reactive |
| --- | --- | --- | --- |
| **Attack** | Standard action | Roll attack vs defense, deal damage on success | No |
| **Defend** | Standard action | Roll STURDINESS, reduce incoming damage 25-75% | No |
| **Parry** | Standard action (prepare) | Prepare reactive defense, triggers on enemy attack | Yes |
| **Ability** | Standard action + Resource | Execute special ability, costs vary (Stamina/AP/etc.) | Varies |
| **Consumable** | Standard action | Use item (heal, buff, cure status) | No |
| **Flee** | Standard action | Attempt escape (if allowed) | No |
| **Move** | Movement action | Reposition on tactical grid | No |

### Initiative System

**Initiative Roll Formula:**

```
Initiative = FINESSE + 1d10
```

**Tie Resolution:** Higher base FINESSE wins; if still tied, player acts before enemies, enemies act before companions.

**Initiative Order Processing:**

1. All participants roll initiative at combat start
2. Order sorted descending (highest acts first)
3. Turn advances through order sequentially
4. Order persists for entire combat (no re-rolling)

### Victory & Defeat Conditions

**Victory Triggers:**

- All enemy HP reduced to 0
- Special encounter objectives completed (rare)

**Defeat Triggers:**

- Player HP reduced to 0 → Death & Resurrection System activates
- All companions incapacitated AND player HP = 0

**Escape Triggers:**

- Player successfully executes Flee action
- `CombatState.CanFlee` must be `true` (disabled for boss fights)

---

## Layer 3 — Technical Implementation

### Primary Implementation

**File:** `/RuneAndRust.Engine/CombatEngine.cs` + `CombatEngine_GaldrFlavorExtensions.cs`

**Version:** v0.38.12

**Lines:** ~1,200 (partial class split)

### Service Dependencies (17 Total)

```csharp
public partial class CombatEngine
{
    // Core Services
    private readonly DiceService _diceService;              // v0.1
    private readonly EquipmentService _equipmentService;    // v0.3
    private readonly SagaService _sagaService;              // v0.1
    private readonly HazardService _hazardService;          // v0.6
    
    // Tactical Services
    private readonly GridInitializationService _gridService;         // v0.20
    private readonly FlankingService _flankingService;               // v0.20.1
    private readonly CoverService _coverService;                     // v0.20.2
    private readonly StanceService _stanceService;                   // v0.21.1
    private readonly AdvancedStatusEffectService _statusService;     // v0.21.3
    private readonly CounterAttackService _counterAttackService;     // v0.21.4
    
    // Support Services
    private readonly LootService _lootService;              // v0.1
    private readonly CurrencyService _currencyService;      // v0.9
    private readonly PerformanceService _performanceService; // v0.7
    private readonly CompanionService _companionService;    // v0.34.4
    private readonly TerritoryService _territoryService;    // v0.35
    
    // Flavor Services
    private readonly CombatFlavorTextService _flavorService;     // v0.38.6
    private readonly GaldrFlavorTextService _galdrFlavorService; // v0.38.7
}
```

### Core Data Models

**CombatState** (Central state container):

```csharp
public class CombatState
{
    public PlayerCharacter Player { get; set; }
    public List<Enemy> Enemies { get; set; }
    public List<Companion> Companions { get; set; }           // v0.34.4
    public List<CombatParticipant> InitiativeOrder { get; set; }
    public CombatGrid? Grid { get; set; }                     // v0.20
    public Room? CurrentRoom { get; set; }
    public bool IsActive { get; set; }
    public bool CanFlee { get; set; }
    public int CharacterId { get; set; }
    public int PlayerNextAttackBonusDice { get; set; }
    public List<string> CombatLog { get; }
    
    // Turn tracking
    public int CurrentTurnIndex { get; set; }
    public int RoundNumber { get; set; }
}
```

**CombatParticipant** (Initiative entry):

```csharp
public class CombatParticipant
{
    public string Name { get; set; }
    public bool IsPlayer { get; set; }
    public bool IsCompanion { get; set; }
    public int InitiativeRoll { get; set; }
    public int InitiativeAttribute { get; set; }  // FINESSE for tiebreaker
    public object? Character { get; set; }        // Player, Enemy, or Companion
}
```

### Public API Methods

**Combat Lifecycle:**

```csharp
// Initialize combat encounter
public CombatState InitializeCombat(
    PlayerCharacter player,
    List<Enemy> enemies,
    Room? currentRoom = null,
    bool canFlee = true,
    int characterId = 0);

// Advance to next turn
public void NextTurn(CombatState combatState);

// Check combat end conditions
public bool IsCombatOver(CombatState combatState);
```

**Player Actions:**

```csharp
// Standard attack
public void PlayerAttack(CombatState combatState, Enemy target);

// Defensive stance
public void PlayerDefend(CombatState combatState);

// Prepare parry reaction
public void PrepareParry(CombatState combatState);

// Use special ability
public bool PlayerUseAbility(CombatState combatState, Ability ability, Enemy? target = null);

// Use consumable item
public bool PlayerUseConsumable(CombatState combatState, Consumable consumable);

// Attempt escape
public bool PlayerFlee(CombatState combatState);
```

**Post-Combat:**

```csharp
// Generate loot from defeated enemies
public void GenerateLoot(CombatState combatState, Room room);

// Award Legend XP
public bool AwardCombatLegend(CombatState combatState, float traumaMod = 1.0f);

// Recover companions
public void RecoverCompanionsAfterCombat(CombatState combatState);
```

### Attack Resolution Algorithm

```csharp
public void PlayerAttack(CombatState combatState, Enemy target)
{
    // 1. GET WEAPON STATS
    var weapon = _equipmentService.GetEquippedWeapon(combatState.Player);
    int attackAttribute = GetWeaponAttribute(weapon);  // MIGHT or FINESSE
    int weaponAccuracyBonus = weapon?.AccuracyBonus ?? 0;
    
    // 2. CALCULATE ATTACK BONUS DICE
    int bonusDice = 0;
    bonusDice += combatState.Player.BattleRageActive ? 2 : 0;     // Battle Rage
    bonusDice += weaponAccuracyBonus;                              // Equipment
    bonusDice += target.IsAnalyzed ? 2 : 0;                        // [Analyzed] status
    bonusDice += _sagaService.HasCourageBuff(combatState.Player) ? 2 : 0;  // Saga
    bonusDice += _flankingService.CalculateFlankingBonus(combatState, target);
    bonusDice += GetElevationBonus(combatState.Player, target);    // High ground
    bonusDice += combatState.PlayerNextAttackBonusDice;            // Temporary bonus
    combatState.PlayerNextAttackBonusDice = 0;                     // Clear temp bonus
    
    // 3. ROLL ATTACK (Dice Pool)
    int attackDice = attackAttribute + bonusDice;
    var attackResult = _diceService.RollPool(attackDice);
    int attackSuccesses = attackResult.Successes;  // 5-6 on d6 = success
    
    // 4. CHECK FUMBLE
    if (attackSuccesses == 0 && attackResult.HasOnes)
    {
        // Fumble! Apply fumble consequence
        ApplyFumble(combatState, combatState.Player);
        return;
    }
    
    // 5. CALCULATE DEFENSE
    int baseDefense = target.Attributes.Sturdiness;
    int flankingPenalty = _flankingService.CalculateFlankingPenalty(combatState, target);
    int coverBonus = _coverService.CalculateCoverBonus(combatState, target);
    int elevationBonus = GetElevationBonus(target, combatState.Player);
    int defensePool = Math.Max(1, baseDefense - flankingPenalty + coverBonus + elevationBonus);
    
    // 6. ROLL DEFENSE
    var defenseResult = _diceService.RollPool(defensePool);
    int defenseSuccesses = defenseResult.Successes;
    
    // 7. DETERMINE HIT
    int netSuccesses = attackSuccesses - defenseSuccesses;
    if (netSuccesses <= 0)
    {
        combatState.AddLogEntry($"Your attack misses {[target.Name](http://target.Name)}!");
        return;
    }
    
    // 8. CALCULATE DAMAGE (delegated to Damage Calculation sub-system)
    int baseDamage = _diceService.RollDamage(weapon?.DamageDice ?? "1d6");
    int mightBonus = combatState.Player.Attributes.Might;
    int stanceBonus = _stanceService.GetFlatDamageBonus(combatState.Player);
    
    // 9. CHECK CRITICAL HIT (5% base + flanking bonus)
    float critChance = 0.05f + (_flankingService.GetCritBonus(combatState, target) * 0.01f);
    bool isCritical = _diceService.RollPercentile() <= critChance;
    
    int totalDamage = baseDamage + mightBonus + stanceBonus;
    if (isCritical) totalDamage *= 2;  // Double damage dice
    
    // 10. APPLY DAMAGE
    int soak = Math.Max(0, target.Soak - (target.CorrodedStacks * 2));
    int effectiveDamage = Math.Max(0, totalDamage - soak);
    target.HP -= effectiveDamage;
    
    // 11. LOG AND CHECK DEFEAT
    combatState.AddLogEntry($"You hit {[target.Name](http://target.Name)} for {effectiveDamage} damage!");
    if (target.HP <= 0)
    {
        HandleEnemyDefeat(combatState, target);
    }
}
```

### Tactical Subsystem Integration

**Flanking (FlankingService v0.20.1):**

- +2 attack dice when flanking (adjacent ally on opposite side)
- -2 defense dice for flanked target
- +5% critical hit chance when flanking

**Cover (CoverService v0.20.2):**

- Half cover: +2 defense dice
- Full cover: +4 defense dice
- Cover can be destroyed by attacks (HP-based)

**Elevation (Grid-based):**

- High ground: +2 attack dice, +2 defense dice
- Low ground: -2 attack dice, -2 defense dice

**Stances (StanceService v0.21.1):**

- Aggressive: +2 damage, -2 defense
- Defensive: +2 defense, -2 damage
- Balanced: No modifiers

### Environmental Hazard Processing

```csharp
private void ProcessEnvironmentalHazards(CombatState combatState)
{
    if (combatState.CurrentRoom == null) return;
    if (!_hazardService.RoomHasActiveHazard(combatState.CurrentRoom)) return;
    
    var hazard = combatState.CurrentRoom.ActiveHazard;
    
    // Apply hazard to all participants
    foreach (var participant in combatState.InitiativeOrder)
    {
        if (participant.IsPlayer)
        {
            int hazardDamage = _hazardService.CalculateHazardDamage(hazard, combatState.Player);
            combatState.Player.HP -= hazardDamage;
            combatState.AddLogEntry($"Environmental hazard ({[hazard.Name](http://hazard.Name)}) deals {hazardDamage} damage!");
        }
        else if (!participant.IsCompanion)
        {
            var enemy = participant.Character as Enemy;
            int hazardDamage = _hazardService.CalculateHazardDamage(hazard, enemy);
            enemy.HP -= hazardDamage;
        }
    }
}
```

### Combat Log System

Combat events are logged to `CombatState.CombatLog` for UI display and post-combat analysis:

```csharp
public class CombatState
{
    private readonly List<string> _combatLog = new();
    public IReadOnlyList<string> CombatLog => _combatLog;
    
    public void AddLogEntry(string entry)
    {
        _combatLog.Add($"[R{RoundNumber}] {entry}");
    }
    
    public void ClearLogForNewAction()
    {
        // Optional: Clear log between actions for cleaner UI
    }
}
```

### Performance Metrics Tracking

```csharp
// Tracked via PerformanceService (v0.7)
public class CombatMetrics
{
    public int TotalDamageDealt { get; set; }
    public int TotalDamageTaken { get; set; }
    public int EnemiesDefeated { get; set; }
    public int RoundsElapsed { get; set; }
    public int CriticalHits { get; set; }
    public int Fumbles { get; set; }
    public int AbilitiesUsed { get; set; }
    public int ParriesAttempted { get; set; }
    public int ParriesSuccessful { get; set; }
}
```

---

## Sub-Systems Reference

This Core System integrates with the following Mechanic-type specifications:

| Sub-System | Document ID | Purpose |
| --- | --- | --- |
| **Damage Calculation** | AAM-SPEC-MECH-DAMAGE-v5.0 | Damage formulas, soak, critical hits |
| **Accuracy & Evasion** | AAM-SPEC-MECH-ACCURACY-v5.0 | Hit/miss determination, dice pools |
| **Counter-Attack & Parry** | AAM-SPEC-MECH-COUNTERATTACK-v5.0 | Reactive defense, riposte system |

---

## Version History

| Version | Date | Changes |
| --- | --- | --- |
| v0.1 | 2024-01 | Initial combat system |
| v0.3 | 2024-02 | Equipment system integration |
| v0.6 | 2024-03 | Environmental hazard support |
| v0.7 | 2024-03 | Consumables, performance tracking, Saga integration |
| v0.9 | 2024-04 | Currency rewards |
| v0.20 | 2024-06 | Tactical grid system, flanking |
| v0.20.2 | 2024-06 | Cover mechanics |
| v0.21.1 | 2024-07 | Stance system |
| v0.21.3 | 2024-07 | Advanced status effects |
| v0.21.4 | 2024-07 | Parry/counter-attack system |
| v0.34.4 | 2024-10 | Companion system integration |
| v0.35 | 2024-10 | Territory/faction tracking |
| v0.38.6 | 2024-11 | Combat flavor text |
| v0.38.7 | 2024-11 | Partial class, Galdr flavor extensions |
| v0.38.12 | 2024-11 | Fumble system, critical hit flavor text |

---

## Balance Targets

| Metric | Target | Notes |
| --- | --- | --- |
| Average combat duration | 3-6 rounds | Longer for bosses |
| Player victory rate | 70-85% | Adjustable via enemy scaling |
| Average damage per round | 8-15 | Varies by build |
| Critical hit rate | 5-15% | Higher with flanking/abilities |
| Fumble rate | ~5% | Only on 0 successes with 1s |
| Parry success rate | 40-60% | Higher for specialists |

---

## Cross-References

### Source Documentation

- SPEC-COMBAT-001: Combat Resolution (Imported Game Docs)
- CombatEngine Service (Code Documentation)

### Related Core Systems

- Death & Resurrection System
- Encounter System
- Trauma Economy System

### Related Mechanics

- Status Effect System
- Dice Pool System
- Attribute System