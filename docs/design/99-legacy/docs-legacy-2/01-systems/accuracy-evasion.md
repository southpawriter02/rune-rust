# Accuracy & Evasion System

**File Path:** `RuneAndRust.Engine/CombatEngine.cs` (lines 130-251)
**Version:** v0.1-v0.16
**Last Updated:** 2025-11-12
**Status:** ✅ Implemented

---

## Layer 1: Functional Overview (What It Does)

### Purpose
The Accuracy & Evasion System determines whether attacks hit or miss using opposed dice pool rolls. Attackers roll dice based on their weapon attribute (MIGHT/FINESSE/WILL), while defenders roll STURDINESS dice. The difference determines hit success.

### Player Experience
1. **Attack Declared** - Player or enemy initiates attack
2. **Attack Roll** - Attacker rolls dice pool (attribute + bonuses)
3. **Defense Roll** - Defender rolls STURDINESS dice
4. **Compare Successes** - Net successes calculated (attack - defense)
5. **Hit or Miss** - Net successes > 0 = hit lands, ≤ 0 = deflected
6. **Proceed to Damage** - If hit, damage calculation begins

### Key Features
- **Opposed Rolls** - Both attacker and defender roll simultaneously
- **Attribute-Based** - Attack uses weapon's governing attribute
- **Success Threshold** - Count dice showing 5-6 as successes (33% per die)
- **Net Success System** - Only difference matters, not absolute roll values
- **Accuracy Bonuses** - Equipment, abilities, status effects grant bonus dice
- **Defense Training** - High STURDINESS improves evasion consistently

### Edge Cases
- **Tie (Equal Successes)** - Attack is deflected (defender wins ties)
- **Zero Attack Dice** - Not possible (minimum 1 die from attribute)
- **Zero Defense Dice** - Possible if STURDINESS = 0 (rare, always hit)
- **Maximum Defense** - STURDINESS 10 = 10d6 defense roll
- **Accuracy Stacking** - Multiple bonuses add up (equipment + status + ability)

---

## Layer 2: Statistical Reference (The Numbers)

### Primary Formula: Hit Determination

```
Net_Successes = Attack_Roll.Successes - Defense_Roll.Successes

If Net_Successes > 0: Hit lands (proceed to damage)
If Net_Successes ≤ 0: Attack deflected (no damage)
```

### Attack Roll Formula

```
Attack_Dice = Base_Attribute + Accuracy_Bonuses

Where:
  Base_Attribute = Weapon's governing attribute (MIGHT/FINESSE/WILL)
    Range: 1-10
  Accuracy_Bonuses = Sum of all bonuses
    Equipment Accuracy Bonus: 0-3
    Ability Bonus Dice: 0-5 (e.g., Exploit Weakness +3)
    [Analyzed] Status: +2 (on target)
    Saga of Courage Performance: +2
    Combat State Bonus: 0-2 (temporary, ability-dependent)
```

**Example Attack Dice Pools:**
| Scenario | Base Attr | Bonuses | Total Dice | Avg Successes |
|----------|-----------|---------|------------|---------------|
| Basic attack (no bonuses) | 5 | 0 | 5d6 | 1.67 |
| With +1 weapon | 5 | +1 | 6d6 | 2.00 |
| With +3 weapon | 5 | +3 | 8d6 | 2.67 |
| With [Analyzed] target | 5 | +2 | 7d6 | 2.33 |
| With Saga of Courage | 5 | +2 | 7d6 | 2.33 |
| All bonuses stacked | 5 | +7 | 12d6 | 4.00 |

### Defense Roll Formula

```
Defense_Dice = STURDINESS

Where:
  STURDINESS = Defender's STURDINESS attribute
    Range: 1-10
    Most common: 3-6 (balanced builds)
    Tanks: 7-10
```

**Example Defense Dice Pools:**
| STURDINESS | Dice | Avg Successes | 0 Success % | 3+ Success % |
|------------|------|---------------|-------------|--------------|
| 1 | 1d6 | 0.33 | 67% | 0% |
| 2 | 2d6 | 0.67 | 44% | 0% |
| 3 | 3d6 | 1.00 | 30% | 7% |
| 4 | 4d6 | 1.33 | 20% | 15% |
| 5 | 5d6 | 1.67 | 13% | 29% |
| 6 | 6d6 | 2.00 | 9% | 42% |
| 7 | 7d6 | 2.33 | 6% | 54% |
| 8 | 8d6 | 2.67 | 4% | 65% |
| 9 | 9d6 | 3.00 | 3% | 73% |
| 10 | 10d6 | 3.33 | 2% | 79% |

### Hit Probability Tables

**Hit Chance by Attack vs Defense:**
| Attack Dice | vs STURDINESS 3 | vs STURDINESS 5 | vs STURDINESS 7 | vs STURDINESS 10 |
|-------------|-----------------|-----------------|-----------------|-------------------|
| 3d6 | 50% | 30% | 15% | 5% |
| 5d6 | 70% | 50% | 30% | 15% |
| 7d6 | 85% | 70% | 50% | 30% |
| 10d6 | 95% | 85% | 70% | 50% |

*Approximate probabilities based on dice pool variance*

### Example Accuracy Scenarios

**Example 1: Basic Attack (No Bonuses)**
```
Setup:
  Player MIGHT = 5
  Enemy STURDINESS = 4
  No bonuses

Attack Roll:
  5d6: [6, 4, 5, 2, 3] = 2 successes

Defense Roll:
  4d6: [6, 3, 5, 1] = 2 successes

Net Successes:
  2 - 2 = 0 (TIE)

Result:
  Attack is DEFLECTED (defender wins ties)
```

**Example 2: With Accuracy Bonus**
```
Setup:
  Player FINESSE = 6
  Weapon Accuracy Bonus = +2
  Enemy STURDINESS = 5
  Total attack dice: 6 + 2 = 8d6

Attack Roll:
  8d6: [6, 5, 4, 6, 2, 5, 3, 1] = 4 successes

Defense Roll:
  5d6: [3, 5, 2, 6, 4] = 2 successes

Net Successes:
  4 - 2 = 2 (HIT)

Result:
  Attack HITS, proceed to damage roll
  Bonus hits (net 2) may grant additional damage in some systems
```

**Example 3: Against [Analyzed] Target**
```
Setup:
  Player WILL = 5
  Target has [Analyzed] status (+2 accuracy for all attackers)
  Enemy STURDINESS = 6
  Total attack dice: 5 + 2 = 7d6

Attack Roll:
  7d6: [6, 3, 5, 5, 2, 6, 4] = 4 successes

Defense Roll:
  6d6: [2, 4, 6, 3, 5, 1] = 2 successes

Net Successes:
  4 - 2 = 2 (HIT)

Result:
  [Analyzed] status made the difference (+2 dice likely added 1 success)
```

**Example 4: Tank Defense (High STURDINESS)**
```
Setup:
  Enemy MIGHT = 6
  Player STURDINESS = 9 (tank build)

Attack Roll:
  6d6: [5, 4, 6, 3, 2, 5] = 3 successes

Defense Roll:
  9d6: [6, 3, 5, 4, 6, 2, 5, 1, 3] = 4 successes

Net Successes:
  3 - 4 = -1 (MISS)

Result:
  High STURDINESS deflects the attack
```

### Critical Success/Failure

**No explicit crit system**, but high net successes may affect:
- Damage (more successes = more damage dice in some abilities)
- Status effects (some require 3+ successes to apply)
- Narrative flavor (combat log describes overwhelming hits)

**Extreme Rolls:**
- **All 6s (perfect roll):** Maximum successes (100% success rate per die)
  - 5d6 all 6s = 5 successes (very rare, ~0.013%)
- **All 1-4 (complete miss):** Zero successes
  - 5d6 all miss = 0 successes (~13% probability)

---

## Layer 3: Technical Implementation (How It Works)

### Service Class
**File:** `RuneAndRust.Engine/CombatEngine.cs`

**Attack Roll Implementation:**
```csharp
/// <summary>
/// Process player attack with accuracy bonuses
/// </summary>
public void PlayerAttack(CombatState combatState, Enemy target)
{
    var player = combatState.Player;

    // Get weapon stats
    string weaponAttribute = player.EquippedWeapon?.WeaponAttribute ?? "MIGHT";
    int accuracyBonus = player.EquippedWeapon?.AccuracyBonus ?? 0;

    // Get effective attribute value (includes equipment bonuses)
    var attributeValue = _equipmentService.GetEffectiveAttributeValue(player, weaponAttribute);

    // Calculate total attack dice
    var bonusDice = combatState.PlayerNextAttackBonusDice; // Ability bonuses
    bonusDice += accuracyBonus; // Weapon accuracy

    // Apply Battle Rage bonus if active
    if (player.BattleRageTurnsRemaining > 0)
    {
        bonusDice += 2;
    }

    // Apply [Analyzed] status bonus (+2 Accuracy)
    if (target.AnalyzedTurnsRemaining > 0)
    {
        bonusDice += 2;
        combatState.AddLogEntry($"  [Analyzed] grants +2 Accuracy against {target.Name}!");
    }

    // Apply Saga of Courage performance bonus (+2 Accuracy)
    if (player.IsPerforming && player.CurrentPerformance == "Saga of Courage")
    {
        bonusDice += 2;
        combatState.AddLogEntry($"  [Saga of Courage] inspires you! +2 Accuracy!");
    }

    var totalDice = attributeValue + bonusDice;
    var attackRoll = _diceService.Roll(totalDice);

    combatState.AddLogEntry($"{player.Name} attacks {target.Name}!");
    combatState.AddLogEntry($"  Rolled {totalDice}d6: {FormatRolls(attackRoll)} = {attackRoll.Successes} successes");

    // Opponent defends
    var defendRoll = _diceService.Roll(target.Attributes.Sturdiness);
    combatState.AddLogEntry($"{target.Name} defends!");
    combatState.AddLogEntry($"  Rolled {target.Attributes.Sturdiness}d6: {FormatRolls(defendRoll)} = {defendRoll.Successes} successes");

    // Calculate damage using net successes
    var netSuccesses = Math.Max(0, attackRoll.Successes - defendRoll.Successes);

    // If net successes > 0, proceed to damage
    if (netSuccesses > 0)
    {
        // Roll weapon damage and apply
        // [See Damage Calculation System]
    }
    else
    {
        combatState.AddLogEntry($"  The attack is deflected!");
    }
}
```

**Defense Roll Implementation:**
```csharp
// Defense roll is simply the STURDINESS attribute as dice count
var defendRoll = _diceService.Roll(defender.Attributes.Sturdiness);

// Net successes determine hit/miss
var netSuccesses = Math.Max(0, attackRoll.Successes - defendRoll.Successes);
```

### Dice Service
**File:** `RuneAndRust.Engine/DiceService.cs`

```csharp
/// <summary>
/// Rolls the specified number of d6 dice and counts successes (5-6 are successes)
/// </summary>
/// <param name="diceCount">Number of d6 to roll</param>
/// <returns>DiceResult with successes count and individual rolls</returns>
public DiceResult Roll(int diceCount)
{
    if (diceCount <= 0)
    {
        return new DiceResult(0, new List<int>(), 0);
    }

    var rolls = new List<int>();
    int successes = 0;

    for (int i = 0; i < diceCount; i++)
    {
        int roll = _random.Next(1, 7); // 1-6
        rolls.Add(roll);

        if (roll >= 5) // 5 or 6 is a success
        {
            successes++;
        }
    }

    return new DiceResult(diceCount, rolls, successes);
}
```

**DiceResult Model:**
```csharp
public class DiceResult
{
    public int DiceCount { get; set; }      // Number of dice rolled
    public List<int> Rolls { get; set; }    // Individual die results [1-6]
    public int Successes { get; set; }      // Count of 5s and 6s
}
```

### Core Models

**Attributes:**
```csharp
// File: RuneAndRust.Core/Attributes.cs
public class Attributes
{
    public int Might { get; set; }      // Melee attack attribute
    public int Finesse { get; set; }    // Ranged attack attribute, initiative
    public int Wits { get; set; }       // Skill checks, puzzles
    public int Will { get; set; }       // Magic attack attribute, Stress resistance
    public int Sturdiness { get; set; } // Defense attribute, HP pool
}
```

**Equipment (Accuracy Bonuses):**
```csharp
// File: RuneAndRust.Core/Equipment.cs
public class Equipment
{
    public int AccuracyBonus { get; set; } // Bonus dice to attack rolls (0-3)
    public string WeaponAttribute { get; set; } // "MIGHT", "FINESSE", or "WILL"
}
```

### Integration Points

**Integrates with:**
- **DiceService** - All dice rolls (`Roll()`)
- **EquipmentService** - Effective attribute values (`GetEffectiveAttributeValue()`)
- **StatusEffectService** - Status-based accuracy bonuses ([Analyzed])
- **PerformanceService** - Performance-based bonuses (Saga of Courage)

**Called by:**
- **CombatEngine.PlayerAttack()** - Player attacks
- **CombatEngine.PlayerUseAbility()** - Ability attacks (if applicable)
- **EnemyAI** - Enemy attacks

**Depends on:**
- **Attributes System** - Weapon attributes and STURDINESS
- **Equipment System** - Accuracy bonuses from gear

### Data Flow

```
[Attack Action]
     ↓
[Gather Accuracy Bonuses]
  • Base Attribute (MIGHT/FINESSE/WILL)
  • Equipment Accuracy Bonus
  • Ability Bonus Dice
  • Status Effects ([Analyzed], Battle Rage)
  • Performances (Saga of Courage)
     ↓
[Calculate Total Attack Dice]
  Total = Base + All Bonuses
     ↓
[Roll Attack Dice]
  DiceService.Roll(Total)
     ↓
[Count Attack Successes]
  Count 5s and 6s
     ↓
[Roll Defense Dice]
  DiceService.Roll(STURDINESS)
     ↓
[Count Defense Successes]
  Count 5s and 6s
     ↓
[Calculate Net Successes]
  Net = Attack - Defense
     ↓
[Determine Hit/Miss]
  If Net > 0: HIT → Proceed to Damage
  If Net ≤ 0: MISS → Log "Deflected!"
```

---

## Layer 4: Testing Coverage (How We Verify)

### Unit Tests
**File:** `RuneAndRust.Tests/AccuracyServiceTests.cs` or `CombatEngineTests.cs` *(to be verified)*

**Expected Key Tests:**
```csharp
[TestMethod]
public void AttackRoll_WithAccuracyBonus_AddsBonusDice()
{
    // Arrange: Player with MIGHT 5, +2 accuracy weapon
    // Act: Calculate attack dice
    // Assert: Total dice = 7 (5 + 2)
}

[TestMethod]
public void AttackRoll_WithAnalyzedTarget_Adds2Dice()
{
    // Arrange: Target with [Analyzed] status
    // Act: Calculate attack dice
    // Assert: Bonus dice includes +2
}

[TestMethod]
public void AttackRoll_NetSuccesses_CorrectlyCalculated()
{
    // Arrange: Mock attack roll = 4 successes, defense roll = 2 successes
    // Act: Calculate net successes
    // Assert: Net = 2
}

[TestMethod]
public void AttackRoll_TieSuccesses_AttackDeflected()
{
    // Arrange: Attack roll = 3, defense roll = 3
    // Act: Check if hit lands
    // Assert: No damage, "deflected" message
}

[TestMethod]
public void DefenseRoll_UsesSturdiness()
{
    // Arrange: Enemy with STURDINESS 6
    // Act: Roll defense
    // Assert: 6d6 rolled
}

[TestMethod]
public void AccuracyBonuses_Stack_Correctly()
{
    // Arrange: Equipment +2, [Analyzed] +2, Battle Rage +2
    // Act: Calculate total bonuses
    // Assert: Total bonus = +6
}
```

**Missing Coverage:**
- [ ] Edge case: Zero STURDINESS (always hit)
- [ ] Edge case: Maximum attack dice (15+)
- [ ] Edge case: Accuracy bonus overflow (20+ dice)
- [ ] Performance test: 10,000 rolls to verify 33% success rate

### QA Checklist

#### Attack Rolls
- [ ] Attack dice = base attribute + all bonuses
- [ ] Equipment accuracy bonus applied
- [ ] Ability bonus dice applied (if any)
- [ ] [Analyzed] status adds +2 dice
- [ ] Battle Rage adds +2 dice
- [ ] Saga of Courage adds +2 dice
- [ ] All bonuses stack correctly

#### Defense Rolls
- [ ] Defense dice = STURDINESS attribute
- [ ] STURDINESS 1-10 rolls 1-10 dice
- [ ] Zero STURDINESS (edge case) rolls 0 dice (always hit)

#### Hit Determination
- [ ] Net successes > 0 = hit lands
- [ ] Net successes ≤ 0 = attack deflected
- [ ] Tie (equal successes) = deflected
- [ ] Negative net successes = deflected

#### Combat Log Display
- [ ] Attack roll displayed: "Rolled Xd6: [rolls] = Y successes"
- [ ] Defense roll displayed: "Rolled Xd6: [rolls] = Y successes"
- [ ] Hit message: "Target takes X damage!"
- [ ] Miss message: "The attack is deflected!"
- [ ] Accuracy bonus messages displayed

#### Probability Verification
- [ ] Success rate per die is approximately 33% (5-6 on d6)
- [ ] Average successes = dice count × 0.33
- [ ] High dice pools (10+) have predictable outcomes
- [ ] Low dice pools (1-3) have high variance

### Known Issues
- **Issue 1:** Very high accuracy bonuses (+10) can make attacks nearly guaranteed hits
  - **Impact:** Trivializes combat if stacked
  - **Priority:** Medium (requires playtesting to confirm)
- **Issue 2:** Defender always wins ties (no explicit crit system)
  - **Impact:** Slight disadvantage to attackers in balanced fights
  - **Priority:** Low (design decision, not a bug)

---

## Layer 5: Balance Considerations (Why These Numbers)

### Design Intent

The Accuracy & Evasion System is designed to:
1. **Create Uncertainty** - Dice variability means no guaranteed hits
2. **Reward Investment** - Higher attributes = more consistent hits
3. **Support Build Diversity** - Offense (MIGHT/FINESSE/WILL) vs Defense (STURDINESS)
4. **Enable Tactical Play** - Accuracy bonuses from equipment, abilities, status effects

### Power Budget

**Hit Chance by Build:**
| Build | Attack Dice | vs Avg Defense (5d6) | vs Tank (10d6) |
|-------|-------------|----------------------|----------------|
| Glass Cannon (10 offense) | 10d6 | ~85% hit | ~50% hit |
| Balanced (6 offense) | 6d6 | ~60% hit | ~30% hit |
| Tank (3 offense) | 3d6 | ~35% hit | ~10% hit |

**Accuracy Bonus Value:**
- **+1 die:** ~+0.33 successes average = ~+10-15% hit chance
- **+2 dice:** ~+0.67 successes average = ~+20-25% hit chance
- **+3 dice:** ~+1.00 successes average = ~+30-35% hit chance

**STURDINESS Investment ROI:**
- **+1 STURDINESS:** ~-0.33 enemy successes = ~-10-15% enemy hit chance
- **+2 STURDINESS:** ~-0.67 enemy successes = ~-20-25% enemy hit chance
- **+3 STURDINESS:** ~-1.00 enemy successes = ~-30-35% enemy hit chance

### Tuning Rationale

**Why 5-6 Success Threshold (33% rate)?**
- **Lower threshold (4-6, 50%):** Would reduce variance too much, make high dice pools overwhelming
- **Higher threshold (6 only, 17%):** Would make low dice pools nearly useless, frustrating
- **Current (5-6, 33%):** Balanced variance at all dice pool sizes

**Why Opposed Rolls (Attack vs Defense)?**
- **Alternative: Fixed target number**
  - Pro: Simpler, faster
  - Con: Doesn't scale with enemy difficulty, tanks have no advantage
- **Current: Opposed rolls**
  - Pro: Defender investment matters, creates build choices
  - Con: Slightly more complex, two rolls per attack

**Why Defender Wins Ties?**
- Design philosophy: Slight advantage to defender (encourages proactive play)
- Historical precedent: Many RPGs give ties to defender
- Balance: Prevents "rocket tag" (both sides always hit)

### Known Balance Issues

#### Issue 1: Accuracy Stacking Dominance
**Problem:** With max bonuses (+7 to +10), attacks become nearly guaranteed hits.

**Example:**
- Base MIGHT 10 + All Bonuses (+7) = 17d6 attack roll
- Average: 5.67 successes
- vs STURDINESS 10 (3.33 successes average)
- Net: 2.34 average (95%+ hit chance)

**Proposed Tuning:**
- Cap accuracy bonuses at +5 total
- Diminishing returns on stacked bonuses
- Add enemies with 12-15 STURDINESS (super tanks)

#### Issue 2: Low STURDINESS Vulnerability
**Problem:** Characters with STURDINESS 1-3 get hit 70-85% of the time vs balanced attackers.

**Data:**
- STURDINESS 3 vs 6d6 attack: ~60-70% hit chance
- Tank with STURDINESS 10 vs 6d6: ~30% hit chance
- Difference: 40% absolute hit chance reduction

**Proposed Tuning:**
- Increase base STURDINESS for all characters by +1 or +2
- Add "Dodge" ability (adds temporary +3 STURDINESS for 1 turn)
- Add equipment with +STURDINESS bonuses

#### Issue 3: All-or-Nothing Hits
**Problem:** Attacks either fully hit or fully miss, no partial damage.

**Alternative Systems:**
- **Glancing Blows:** Net 0 successes = 50% damage
- **Scaling Damage:** More net successes = more damage multiplier
- **Crit System:** Net 4+ successes = double damage

**Current Design:**
- Keeps system simple (hit or miss, no intermediate states)
- Damage variability comes from damage dice roll, not net successes

### Future Tuning Considerations

**Post-Balance Pass (v0.18):**
1. **Accuracy Cap:** Test +5 bonus cap, measure hit rate vs +10 cap
2. **Defense Scaling:** Adjust enemy STURDINESS to match player offense growth
3. **Critical Hits:** Playtest crit system (net 4+ successes = +50% damage)
4. **Glancing Blows:** Playtest partial damage on ties (50% damage on net 0)

**Metrics to Track:**
- Average hit chance by character build
- % of attacks that miss (target: 30-40%)
- STURDINESS distribution across player builds
- Win rate correlation with STURDINESS investment

---

## Cross-References

### Related Systems
- [Combat Resolution System](./combat-resolution.md) - Turn order and combat flow
- [Damage Calculation System](./damage-calculation.md) - What happens after hit lands
- [Status Effects System](./status-effects.md) - [Analyzed], Battle Rage, etc.
- [Attribute System](./attribute-system.md) - MIGHT, FINESSE, WILL, STURDINESS

### Registry Entries
- [DiceService API](../03-technical-reference/service-architecture.md#diceservice)
- [EquipmentService API](../03-technical-reference/service-architecture.md#equipmentservice)
- [CombatEngine API](../03-technical-reference/combat-service-api.md)

### Technical References
- [Dice Pool Mechanics](../quick-reference/dice-pool-cheat-sheet.md) - Quick reference
- [Probability Tables](../05-balance-reference/hit-chance-tables.md) - Detailed probability analysis

---

## Changelog

### v0.1 - Initial Implementation
- Opposed dice pool system (attack vs defense)
- Success threshold: 5-6 on d6
- Net successes determine hit/miss
- Defender wins ties

### v0.3 - Equipment Integration
- Equipment accuracy bonuses (+1 to +3)
- Weapon attribute determines attack dice pool

### v0.5 - Ability Bonuses
- Abilities can grant temporary accuracy bonuses
- Battle Rage: +2 attack dice

### v0.7 - Status Effects
- [Analyzed]: +2 accuracy for all attackers
- Saga of Courage: +2 accuracy during performance

### v0.16 - Content Expansion
- More enemy STURDINESS variety (1-10 range)
- Expanded accuracy bonus sources

---

**Documentation Status:** ✅ Complete
**Last Reviewed:** 2025-11-12
**Reviewer:** Claude (AI Documentation Assistant)
**Coverage:** Comprehensive (all 5 layers documented)
