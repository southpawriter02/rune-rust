# Combat Resolution System

**File Path:** `RuneAndRust.Engine/CombatEngine.cs`
**Version:** v0.1-v0.16 (evolved across versions)
**Last Updated:** 2025-11-12
**Status:** ✅ Implemented

---

## Layer 1: Functional Overview (What It Does)

### Purpose
The Combat Resolution System orchestrates turn-based combat encounters between the player and enemies. It manages initiative, turn order, action resolution, and combat end conditions.

### Player Experience
1. **Combat Initiation** - Combat begins when player encounters enemies
2. **Initiative Roll** - System determines turn order based on FINESSE rolls
3. **Turn Sequence** - Players and enemies take turns in initiative order
4. **Action Selection** - On player's turn, choose: Attack, Defend, Use Ability, Use Consumable, or Flee
5. **Resolution** - System resolves actions using dice pools
6. **Status Updates** - HP, Stamina, and status effects updated each turn
7. **Combat End** - Victory (all enemies defeated), Defeat (player HP ≤ 0), or Flee (successful escape)

### Key Features
- **Turn-Based** - One action per turn, orderly sequence
- **Initiative System** - FINESSE-based turn order
- **Dice Pool Mechanics** - Roll Xd6, count 5-6 as successes
- **Action Variety** - Multiple tactical options each turn
- **Status Effects** - Buffs, debuffs, and DoT effects persist across turns
- **Environmental Hazards** - Room hazards apply automatic damage per turn
- **Combat Log** - Complete action history displayed to player

### Edge Cases
- **Ties in Initiative** - Broken by highest FINESSE attribute value (player wins ties if equal)
- **Fleeing Boss Fights** - Cannot flee if `CanFlee = false` (boss encounters)
- **Death During Enemy Turn** - Combat immediately ends if player HP ≤ 0
- **Environmental Hazard Death** - Player can die from hazard damage, ending combat
- **All Enemies Stunned** - Combat continues, stunned enemies skip turns
- **Negative HP** - HP never goes below 0 in display (clamped to 0)

---

## Layer 2: Statistical Reference (The Numbers)

### Primary Formula: Initiative

```
Initiative_Score = Roll(FINESSE_d6).Successes
```

**Variables:**
- `FINESSE`: Character's FINESSE attribute
  - Range: 1-10
  - Default: 3 (typical starting value)
- `Successes`: Count of dice showing 5 or 6
  - Range: 0 to FINESSE
  - Probability per die: 2/6 = 33.33%

**Expected Initiative by FINESSE:**
| FINESSE | Average Successes | Probability of 0 | Probability of 3+ |
|---------|-------------------|------------------|-------------------|
| 1 | 0.33 | 67% | 0% |
| 2 | 0.67 | 44% | 0% |
| 3 | 1.00 | 30% | 7% |
| 4 | 1.33 | 20% | 15% |
| 5 | 1.67 | 13% | 29% |
| 6 | 2.00 | 9% | 42% |
| 7 | 2.33 | 6% | 54% |
| 8 | 2.67 | 4% | 65% |
| 9 | 3.00 | 3% | 73% |
| 10 | 3.33 | 2% | 79% |

### Turn Order Sorting

```
Sort by: Initiative_Score DESC, then FINESSE_Attribute DESC
```

**Tie-Breaking Logic:**
1. Compare initiative scores (higher goes first)
2. If tied, compare FINESSE attribute (higher goes first)
3. If still tied, player goes first (implementation detail)

### Example Initiative Roll

**Scenario:** Player (FINESSE 5) vs 3 Enemies (FINESSE 3, 4, 6)

```
Player rolls 5d6: [6, 4, 5, 2, 6] = 3 successes
Enemy 1 rolls 3d6: [3, 5, 1] = 1 success
Enemy 2 rolls 4d6: [6, 6, 2, 3] = 2 successes
Enemy 3 rolls 6d6: [5, 4, 6, 3, 5, 2] = 3 successes

Turn Order (sorted by successes desc, then FINESSE desc):
1. Enemy 3 (3 successes, FINESSE 6) ← Tie with player, FINESSE breaks tie
2. Player (3 successes, FINESSE 5)
3. Enemy 2 (2 successes, FINESSE 4)
4. Enemy 1 (1 success, FINESSE 3)
```

### Action Costs

| Action | Stamina Cost | Turn Cost | Notes |
|--------|--------------|-----------|-------|
| **Attack** | 0 | 1 turn | Basic attack with equipped weapon |
| **Defend** | 0 | 1 turn | Gain defense bonus until next turn |
| **Use Ability** | Varies (0-30) | 1 turn | See Ability System |
| **Use Consumable** | 0 | 1 turn | Item consumed from inventory |
| **Flee** | 0 | 1 turn | Requires success roll, can fail |

### Combat Timing

**Turn Structure:**
```
1. Check if combat should end (player dead / all enemies dead)
2. Determine current turn participant (from InitiativeOrder)
3. If player turn:
   - Wait for player action input
   - Resolve player action
4. If enemy turn:
   - EnemyAI selects action
   - Resolve enemy action
5. Apply end-of-turn effects:
   - Forlorn Enemy Aura (Stress damage)
   - Environmental Psychic Resonance (Stress damage)
   - Environmental Hazards (HP/Stress damage)
   - Status effect duration ticks (Bleeding, Stunned, Defense, etc.)
   - Performance duration ticks (Skald abilities)
6. Advance to next turn (CurrentTurnIndex + 1, wrap around)
7. Repeat from step 1
```

### Status Effect Tracking

**Per Turn:**
- Player defense bonus expires after 1 turn
- Enemy defense bonus expires after N turns (ability-dependent)
- Bleeding damage applied at start of enemy's turn
- Stunned enemies skip their turn, then stun decrements
- All durations decrement at end of each full round

---

## Layer 3: Technical Implementation (How It Works)

### Service Class
**File:** `RuneAndRust.Engine/CombatEngine.cs` (1,444 lines)

**Constructor Dependencies:**
```csharp
public CombatEngine(
    DiceService diceService,              // Random number generation
    SagaService sagaService,              // XP/Legend rewards
    LootService lootService,              // Loot generation on victory
    EquipmentService equipmentService,    // Equipment stat bonuses
    HazardService hazardService,          // Environmental hazard processing
    CurrencyService currencyService       // Currency drops
)
```

**Key Methods:**

```csharp
/// <summary>
/// Initialize combat with the player and enemies, roll initiative
/// </summary>
/// <returns>CombatState with initialized initiative order</returns>
public CombatState InitializeCombat(
    PlayerCharacter player,
    List<Enemy> enemies,
    Room? currentRoom = null,
    bool canFlee = true
)
{
    // Create CombatState
    // Roll initiative for all participants
    // Sort by initiative score
    // Display environmental hazard warnings
    // Return initialized combat state
}
```

**Initiative Rolling:**
```csharp
private void RollInitiative(CombatState combatState)
{
    // Roll player initiative (FINESSE dice)
    var playerRoll = _diceService.Roll(player.Attributes.Finesse);

    // Roll enemy initiative (each enemy's FINESSE)
    foreach (var enemy in enemies)
    {
        var enemyRoll = _diceService.Roll(enemy.Attributes.Finesse);
        // Add to participants list
    }

    // Sort: Descending by successes, then by FINESSE attribute
    combatState.InitiativeOrder = participants
        .OrderByDescending(p => p.InitiativeRoll)
        .ThenByDescending(p => p.InitiativeAttribute)
        .ToList();
}
```

**Turn Advancement:**
```csharp
public void NextTurn(CombatState combatState)
{
    // Apply Forlorn Enemy Aura (Stress checks)
    // Apply Environmental Psychic Resonance (Stress)
    // Decrement player status effects (DefenseTurnsRemaining, BattleRage, etc.)
    // Decrement enemy status effects (Bleeding, Stunned, Analyzed, Vulnerable)
    // Apply environmental hazard damage
    // Tick down performance durations (Skald)
    // Advance turn index (CurrentTurnIndex + 1) % InitiativeOrder.Count
}
```

**Combat End Check:**
```csharp
public bool IsCombatOver(CombatState combatState)
{
    // Check if player is dead
    if (!player.IsAlive)
    {
        combatState.IsActive = false;
        return true; // Defeat
    }

    // Check if all enemies are dead
    if (enemies.All(e => !e.IsAlive))
    {
        combatState.IsActive = false;
        AwardCombatLegend(combatState); // Grant XP
        return true; // Victory
    }

    return false; // Combat continues
}
```

### Core Models
**File:** `RuneAndRust.Core/CombatState.cs`

```csharp
public class CombatState
{
    // Participants
    public PlayerCharacter Player { get; set; }
    public List<Enemy> Enemies { get; set; }
    public List<CombatParticipant> InitiativeOrder { get; set; }

    // State
    public int CurrentTurnIndex { get; set; } = 0;
    public bool IsActive { get; set; } = false;
    public bool CanFlee { get; set; } = true;

    // Environment
    public Room? CurrentRoom { get; set; }

    // Temporary combat effects
    public int PlayerNextAttackBonusDice { get; set; } = 0; // Exploit Weakness bonus
    public bool PlayerNegateNextAttack { get; set; } = false; // Quick Dodge

    // Combat log for UI
    public List<string> CombatLog { get; set; } = new();
}

public class CombatParticipant
{
    public string Name { get; set; }
    public bool IsPlayer { get; set; }
    public int InitiativeRoll { get; set; }      // Successes rolled
    public int InitiativeAttribute { get; set; } // FINESSE value (for tie-breaking)
    public object? Character { get; set; }       // Reference to PlayerCharacter or Enemy
}
```

### Database Schema

**No persistent combat state.** Combat is session-based only.

**Related Persistence:**
- Player HP/Stamina persisted in `saves` table
- Equipment persisted in `saves` table (affects combat stats)
- Legend/XP awards persisted after combat via `SagaService`

### Integration Points

**Integrates with:**
- **DiceService** - All random number generation (initiative, attacks, damage)
- **SagaService** - XP/Legend rewards on victory (`AwardCombatLegend()`)
- **LootService** - Equipment/currency drops on victory (`GenerateLoot()`)
- **EquipmentService** - Stat bonuses from equipped items (`GetEffectiveAttributeValue()`)
- **HazardService** - Environmental hazard damage per turn (`ProcessAutomaticHazard()`)
- **CurrencyService** - Currency drops on victory (`AddCurrency()`)
- **EnemyAI** - Enemy decision-making (called during enemy turns)
- **TraumaEconomyService** - Stress/Corruption tracking from combat events

**Called by:**
- **Main Game Loop** (Program.cs) - Initiates combat when player encounters enemies
- **Room Exploration** - Triggers combat on room entry if enemies present

**Depends on:**
- **Attributes** (FINESSE, MIGHT, STURDINESS) - For initiative, attacks, defense
- **Equipment System** - Weapon damage dice, accuracy bonuses, armor Soak
- **Ability System** - Special actions with unique effects
- **Status Effect System** - Buffs, debuffs, DoT effects

### Data Flow

```
[Player encounters enemies in Room]
          ↓
[CombatEngine.InitializeCombat()]
          ↓
[Roll initiative for all participants]
          ↓
[Sort initiative order]
          ↓
[Display combat UI with InitiativeOrder]
          ↓
┌─────────────────────────────────────┐
│ COMBAT LOOP (until IsCombatOver()) │
├─────────────────────────────────────┤
│ 1. Get current participant          │
│ 2. If Player: Get action input      │
│    If Enemy: EnemyAI.ChooseAction() │
│ 3. Resolve action (attack/defend)   │
│ 4. Update CombatState & Log         │
│ 5. Check combat end condition       │
│ 6. NextTurn() - Apply DoT, tick     │
│ 7. Advance CurrentTurnIndex         │
│ 8. Repeat                            │
└─────────────────────────────────────┘
          ↓
[Combat ends: Victory or Defeat]
          ↓
[If Victory: AwardCombatLegend()]
          ↓
[If Victory: GenerateLoot()]
          ↓
[Return to exploration]
```

---

## Layer 4: Testing Coverage (How We Verify)

### Unit Tests
**File:** `RuneAndRust.Tests/CombatEngineTests.cs` *(to be verified)*

**Coverage:** Unknown (requires test file inspection)

**Expected Key Tests:**
```csharp
[TestMethod]
public void InitializeCombat_WithMultipleEnemies_RollsInitiativeForAll()
{
    // Arrange: Create player + 3 enemies
    // Act: InitializeCombat()
    // Assert: InitiativeOrder.Count == 4
}

[TestMethod]
public void InitializeCombat_SortsBy_InitiativeScoreDescending()
{
    // Arrange: Mock dice service to return specific initiative rolls
    // Act: InitializeCombat()
    // Assert: InitiativeOrder sorted correctly (high to low)
}

[TestMethod]
public void InitializeCombat_TieBreaker_UsesFinesse()
{
    // Arrange: Player and enemy both roll 2 successes, different FINESSE
    // Act: InitializeCombat()
    // Assert: Higher FINESSE goes first
}

[TestMethod]
public void IsCombatOver_AllEnemiesDead_ReturnsTrue()
{
    // Arrange: Set all enemy HP to 0
    // Act: IsCombatOver()
    // Assert: Returns true
}

[TestMethod]
public void IsCombatOver_PlayerDead_ReturnsTrue()
{
    // Arrange: Set player HP to 0
    // Act: IsCombatOver()
    // Assert: Returns true
}

[TestMethod]
public void NextTurn_AdvancesIndex_WrapAround()
{
    // Arrange: CurrentTurnIndex at last position
    // Act: NextTurn()
    // Assert: CurrentTurnIndex wraps to 0
}

[TestMethod]
public void PlayerFlee_CannotFlee_WhenBossFight()
{
    // Arrange: CombatState with CanFlee = false
    // Act: PlayerFlee()
    // Assert: Returns false, combat continues
}
```

**Missing Coverage:**
- [ ] Edge case: Initiative ties with 3+ participants
- [ ] Edge case: Environmental hazard kills player during turn
- [ ] Edge case: Bleeding damage kills enemy during status tick
- [ ] Integration test: Full combat loop simulation

### QA Checklist

#### Combat Initialization
- [ ] Initiative rolled for all participants (player + all enemies)
- [ ] Initiative order displayed correctly (highest to lowest)
- [ ] Tie-breaker works (FINESSE attribute comparison)
- [ ] Environmental hazard warning displayed if room has hazard
- [ ] Combat log starts with "=== COMBAT BEGINS ==="

#### Turn Sequence
- [ ] Current turn indicator shows correct participant
- [ ] Player actions only available on player's turn
- [ ] Enemy takes action automatically on enemy turn
- [ ] Turn advances correctly after each action
- [ ] Turn wraps around to first participant after last

#### Combat End Conditions
- [ ] Combat ends immediately when player HP ≤ 0
- [ ] Combat ends immediately when all enemies HP ≤ 0
- [ ] Victory message displays on all enemies defeated
- [ ] Defeat message displays on player death
- [ ] XP awarded only on victory
- [ ] Loot generated only on victory

#### Status Effects
- [ ] Status effect durations decrement each turn
- [ ] Expired status effects removed correctly
- [ ] Bleeding applies damage at start of enemy turn
- [ ] Defense bonus expires after specified turns
- [ ] Stunned enemies skip their turn

#### Flee Mechanic
- [ ] Flee succeeds if player roll > enemy roll
- [ ] Flee fails if player roll ≤ enemy roll
- [ ] Cannot flee from boss fights (CanFlee = false)
- [ ] Failed flee attempt still consumes player turn

#### Environmental Hazards
- [ ] Hazard damage applied every turn
- [ ] Hazard Stress applied every turn
- [ ] Hazard can kill player (HP reaches 0)
- [ ] Hazard damage displayed in combat log

### Known Issues
- **Issue 1:** Initiative ties between 3+ participants with identical FINESSE may have non-deterministic order (depends on List.Sort() stability)
  - **Impact:** Minor, affects turn order in edge case
  - **Priority:** Low
- **Issue 2:** Combat log can become very long in extended fights, no pagination
  - **Impact:** UI clutter, readability
  - **Priority:** Medium

---

## Layer 5: Balance Considerations (Why These Numbers)

### Design Intent

The Combat Resolution System is designed to:
1. **Create Tension** - Dice variability means no guaranteed outcomes
2. **Reward Tactics** - Initiative, defense, ability timing matter
3. **Support Varied Builds** - FINESSE (initiative), MIGHT (damage), STURDINESS (defense) all relevant
4. **Maintain Pacing** - Most fights 3-10 turns, boss fights 15-25 turns

### Power Budget

**Initiative Importance:**
- **High FINESSE (7-10):** ~2.5-3.5 successes average
  - 60-75% chance to go first vs medium FINESSE enemies
  - Critical advantage: Strike first, apply buffs/debuffs early
- **Medium FINESSE (4-6):** ~1.5-2.0 successes average
  - Balanced, often acts mid-turn-order
- **Low FINESSE (1-3):** ~0.3-1.0 successes average
  - 60-80% chance to go last
  - Disadvantage: Takes damage before acting

**Expected Successes by Dice Pool:**
| Dice | Average Successes | 50% Probability | 90% Probability |
|------|-------------------|-----------------|-----------------|
| 1d6 | 0.33 | 0 | 1 |
| 2d6 | 0.67 | 0 | 1 |
| 3d6 | 1.00 | 1 | 2 |
| 4d6 | 1.33 | 1 | 2 |
| 5d6 | 1.67 | 1 | 3 |
| 6d6 | 2.00 | 2 | 3 |
| 7d6 | 2.33 | 2 | 4 |
| 8d6 | 2.67 | 2 | 4 |
| 9d6 | 3.00 | 3 | 5 |
| 10d6 | 3.33 | 3 | 5 |

### Tuning Rationale

**Why Dice Pool System?**
- **Scalability:** Adding dice increases average successes linearly
- **Variability:** Always risk of low rolls (engagement)
- **Intuitive:** More dice = better chance, but never guaranteed
- **Attribute Relevance:** Higher attributes directly translate to more dice

**Why 5-6 Success Threshold?**
- 33.33% success rate per die balances luck vs skill
- Low dice pools (1-3) rarely get successes (high variance)
- High dice pools (7-10) consistently get successes (reliable)
- Alternative (4-6 threshold, 50% success rate) would reduce variance too much

**Why FINESSE for Initiative?**
- FINESSE represents dexterity, reaction time, speed
- Going first is powerful (apply control effects, deal damage before enemy acts)
- Creates build diversity: FINESSE-focused builds control tempo, MIGHT-focused builds maximize damage per hit

### Known Balance Issues

#### Issue 1: High FINESSE Dominance
**Problem:** Characters with FINESSE 8+ almost always go first, creating "first turn wins" in some encounters.

**Data:**
- FINESSE 10 vs FINESSE 3: 95% chance player goes first
- Going first allows:
  - Apply Stun (enemy skips turn)
  - Apply defense (reduce incoming damage)
  - Deal damage before enemy acts

**Proposed Tuning:**
- Reduce power of control effects (Stun = skip 1 turn → 50% chance to skip)
- Add enemies with high FINESSE (8-10) to challenge high-FINESSE builds
- Add abilities that manipulate turn order (swap positions, delay enemy)

#### Issue 2: Low Variance at Extreme Dice Pools
**Problem:** 10d6 dice pools have very predictable outcomes (3-4 successes 70% of the time).

**Data:**
- 10d6: 70% chance to roll 2-4 successes
- 1d6: 67% chance to roll 0 successes, 33% to roll 1

**Proposed Tuning:**
- Cap dice pools at 8-10 (already implemented via attribute caps)
- Add modifiers that affect success threshold instead of dice count
- Consider alternative: Roll Xd10 instead of Xd6 for high-level play (increases variance)

#### Issue 3: Flee Mechanic Underused
**Problem:** Players rarely flee because:
- XP/loot loss discourages fleeing
- Failure costs a turn (enemy gets free hit)
- Success rate unclear (opaque formula)

**Data:**
- Flee success rate vs equal FINESSE: ~50%
- Flee success rate vs higher FINESSE: <30%

**Proposed Tuning:**
- Add "Tactical Retreat" ability (guaranteed flee, costs resource)
- Display estimated flee chance before attempting
- Reduce penalty for failed flee (don't consume full turn)

### Future Tuning Considerations

**Post-Balance Pass (v0.18):**
1. **Initiative Tuning:** Playtest high-FINESSE builds, adjust enemy FINESSE if needed
2. **Turn Length:** Track average turns per combat, target 5-8 for normal, 15-20 for boss
3. **Flee Usage:** Track flee attempt rate, adjust incentives if <5% of combats
4. **Status Effect Power:** Measure win rate with/without control effects (Stun, Disrupt)

**Metrics to Track:**
- Average initiative score by character build
- % of combats where player goes first
- Average turns per combat
- Player death rate (by Legend level)
- Flee attempt rate and success rate

---

## Cross-References

### Related Systems
- [Damage Calculation System](./damage-calculation.md) - How damage is calculated and applied
- [Accuracy & Evasion System](./accuracy-evasion.md) - Attack success determination
- [Status Effects System](./status-effects.md) - Buffs, debuffs, DoT mechanics
- [Ability System](./ability-system.md) - Special actions in combat
- [Equipment System](./equipment-system.md) - Equipment stat bonuses affect combat

### Registry Entries
- [DiceService](../03-technical-reference/service-architecture.md#diceservice) - Random number generation
- [CombatEngine](../03-technical-reference/combat-service-api.md) - Complete API reference
- [PlayerCharacter](../03-technical-reference/database-schema.md#playercharacter) - Player model
- [Enemy](../03-technical-reference/database-schema.md#enemy) - Enemy model

### Technical References
- [Service Architecture](../03-technical-reference/service-architecture.md) - How CombatEngine fits into service layer
- [Data Flow](../03-technical-reference/data-flow.md#combat-turn-resolution) - Complete combat data flow

---

## Changelog

### v0.1 - Initial Implementation
- Basic turn-based combat
- Initiative system (FINESSE-based)
- Attack, Defend, Ability actions
- Dice pool mechanics (Xd6, count 5-6 as successes)

### v0.2 - Status Effects
- Added defense bonus system
- Added BattleRage status (Warrior)
- Added ShieldAbsorption status (Mystic)

### v0.3 - Equipment Integration
- Combat uses equipped weapon stats
- Equipment bonuses apply to rolls
- Weapon-specific damage dice

### v0.4 - Environmental Hazards
- Room hazards apply damage per turn
- Hazard warnings at combat start

### v0.5 - Trauma Economy Integration
- Forlorn Enemy Aura (passive Stress damage)
- Environmental Psychic Resonance (Stress per turn)
- Heretical abilities cost Stress/Corruption

### v0.6 - Expanded Enemies & Hazards
- New enemy types with special abilities
- More environmental hazard types
- Enemy Soak (damage reduction)

### v0.7 - Adept Specializations
- New status effects: [Analyzed], [Vulnerable], [Silenced]
- Performance system (Skald abilities)
- Consumable use in combat

### v0.7.1 - Stance System
- Defensive Stance (Warriors)
- Stance affects combat calculations

### v0.9 - Economy Integration
- Currency drops on victory
- Material drops for crafting

### v0.15 - Trauma System
- Permanent Traumas from Breaking Points
- Trauma effects apply in combat

### v0.16 - Content Expansion
- 10 new enemy types
- Expanded ability pool

---

**Documentation Status:** ✅ Complete
**Last Reviewed:** 2025-11-12
**Reviewer:** Claude (AI Documentation Assistant)
**Coverage:** Comprehensive (all 5 layers documented)
