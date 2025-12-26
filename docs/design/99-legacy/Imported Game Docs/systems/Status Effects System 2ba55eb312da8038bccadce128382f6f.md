# Status Effects System

Parent item: System Documentation (System%20Documentation%202ba55eb312da801a9aa4f30d6e439959.md)

**File Path:** `RuneAndRust.Engine/CombatEngine.cs` (lines 1200-1416), `RuneAndRust.Core/PlayerCharacter.cs`, `RuneAndRust.Core/Enemy.cs`**Version:** v0.1-v0.16
**Last Updated:** 2025-11-12
**Status:** ✅ Implemented

---

## Layer 1: Functional Overview (What It Does)

### Purpose

The Status Effects System applies temporary buffs, debuffs, and damage-over-time effects to characters during combat. Effects persist for a specified duration (measured in turns) and automatically expire.

### Player Experience

1. **Effect Applied** - Ability or action applies status effect to target
2. **Effect Active** - Status modifies combat calculations (damage, accuracy, defense)
3. **Duration Tracking** - Combat log shows remaining turns
4. **Effect Expires** - Status removed automatically after duration ends
5. **Visual Feedback** - Status shown in brackets [StatusName] in combat log

### Key Features

- **Duration-Based** - Effects last N turns, then expire automatically
- **Turn Tracking** - Durations decrement at end of each combat round
- **Stackable** - Some effects stack (multiple applications), others refresh
- **Category Types** - Buffs (positive), Debuffs (negative), DoT (damage-over-time), Control (disable actions)
- **Application Conditions** - Some require success threshold (e.g., 3+ successes)
- **Removal Mechanics** - Expire naturally or removed by specific actions

### Edge Cases

- **Effect Expires Mid-Combat** - Calculations revert to base values
- **Multiple Same Effects** - Most don't stack, newest replaces oldest
- **Contradictory Effects** - Can have both offense buff and defense debuff simultaneously
- **Death During DoT** - DoT can kill enemies/player, triggering combat end
- **Effect on Stunned Target** - Can apply effects even if target is stunned

---

## Layer 2: Statistical Reference (The Numbers)

### Status Effect Registry

### 🛡️ Defensive Buffs

**Defense Bonus** (from Defend action)

```
Application: Player uses Defend action
Duration: 1 turn (until next attack hits)
Effect: Reduces incoming damage by X%
  X = STURDINESS successes × 25%, max 75%
Calculation: Final_Damage = Base_Damage × (1 - X/100)
Example: 3 successes = 75% reduction, 20 damage → 5 damage

```

**Shield Absorption** (Mystic ability: Aetheric Shield)

```
Application: Use Aetheric Shield ability
Duration: Until absorbed or combat ends
Effect: Absorbs next N damage
  N = 15 HP (flat absorption)
Calculation: If damage ≤ N, damage = 0 and N reduced by damage
             If damage > N, damage reduced by N, absorption depleted
Example: 20 damage vs 15 shield → 5 damage dealt, shield gone

```

**Temporary HP** (Skald ability: Saga of the Einherjar)

```
Application: Performance grants Temp HP
Duration: Until depleted or combat ends
Effect: Damage targets Temp HP first, then real HP
  Temp HP = 2d6 (average 7)
Calculation: Damage first reduces Temp HP, overflow goes to real HP
Example: 10 damage, 7 Temp HP → 3 real HP damage, Temp HP gone

```

### ⚔️ Offensive Buffs

**Battle Rage** (Warrior ability)

```
Application: Use Battle Rage ability
Duration: 3 turns
Effect: +2 dice to all attack rolls
Trade-off: +25% damage taken while active
Calculation: Attack_Dice = Base + 2
Example: MIGHT 5 → 7d6 attack roll (5+2)

```

**[Inspired]** (Skald ability: Saga of the Einherjar)

```
Application: Performance grants [Inspired]
Duration: Performance duration (varies, typically 3-5 turns)
Effect: +3 damage dice before rolling
Calculation: Damage_Dice = Weapon_Dice + 3
Example: Longsword 3d6 → 6d6 damage (3+3)

```

**Saga of Courage** (Skald performance, ongoing)

```
Application: Start performance
Duration: Until performance ends or interrupted
Effect: +2 accuracy dice while active
Calculation: Attack_Dice = Base + 2
Example: Continuous +2 accuracy for entire performance

```

### 🎯 Debuffs (on Enemies)

**[Vulnerable]** (Bone-Setter ability: Anatomical Insight)

```
Application: Ability hits target
Duration: 3 turns
Effect: +25% damage taken from all sources
Calculation: Final_Damage = Base_Damage × 1.25
Example: 20 damage → 25 damage

```

**[Analyzed]** (Jötun-Reader ability: Exploit Design Flaw)

```
Application: Ability hits target
Duration: 4 turns
Effect: All attackers gain +2 accuracy dice against this target
Calculation: Attack_Dice = Base + 2 (for any attacker)
Example: Party-wide accuracy buff vs single target

```

**[Corrupted]** (Heretical ability: Blight Surge)

```
Application: Blight Surge hits target
Duration: 3 turns
Effect: +20% Stress from all sources
Calculation: Stress_Gain = Base_Stress × 1.20
Example: Not fully implemented in v0.16

```

### 🚫 Control Effects (Disable Actions)

**[Stunned]** (Disrupt ability, other sources)

```
Application: Disrupt ability, special enemy attacks
Duration: 1 turn (skips next turn)
Effect: Target cannot take actions on their turn
Turn skip: Enemy's turn is skipped in initiative order
Calculation: N/A (binary: stunned or not)
Example: Enemy stunned, player gets 2 consecutive turns

```

**[Silenced]** (Skald ability: Song of Silence)

```
Application: Song of Silence hits target
Duration: 3 turns
Effect: Cannot cast spells or use performance abilities
Restriction: Blocks abilities with "magic" or "performance" tags
Example: Enemy caster cannot use special abilities

```

**[Seized]** (Jötun-Reader ability: Architect of the Silence)

```
Application: Architect of the Silence ability
Duration: 2 turns
Effect: Target cannot move or take actions
Status: Partially implemented (placeholder for AI system)
Example: Enemy frozen in place by architecture manipulation

```

### 🩸 Damage-Over-Time Effects

**[Bleeding]** (Scavenger ability: Precision Strike)

```
Application: Precision Strike with 3+ successes
Duration: 2-3 turns
Effect: 1d6 damage at start of target's turn
Damage calculation: Roll 1d6, apply as unmitigated damage
Total damage: Average 7-10 over duration (3.5 × 2-3 turns)
Can kill: Yes, bleeding damage can reduce HP to 0

```

**Environmental Hazards** (various, see [Environmental Hazards](https://www.notion.so/environmental-hazards.md))

```
Application: Present in room
Duration: Entire combat
Effect: Xd6 damage per turn (hazard-dependent)
Example: Steam Vent = 2d6 per turn, Psychic Resonance = stress per turn

```

### Duration Tracking Table

| Status Effect | Duration | Tick Timing | Expires When |
| --- | --- | --- | --- |
| Defense Bonus | 1 turn | After taking damage | First hit or 1 turn |
| Battle Rage | 3 turns | End of round | 3 turns complete |
| Shield Absorption | Until depleted | On taking damage | HP absorbed = 0 |
| [Stunned] | 1 turn | End of round | 1 turn skipped |
| [Bleeding] | 2-3 turns | Start of target turn | Duration = 0 |
| [Vulnerable] | 3 turns | End of round | 3 turns complete |
| [Analyzed] | 4 turns | End of round | 4 turns complete |
| [Inspired] | Performance length | End of round | Performance ends |
| [Silenced] | 3 turns | End of round | 3 turns complete |
| Temp HP | Until depleted | On taking damage | Temp HP = 0 |

### Status Effect Power Comparison

**Buff Value Analysis:**

| Buff | Duration | Value | Total Benefit |
| --- | --- | --- | --- |
| Defense 75% | 1 turn | Negate 15 dmg | 15 dmg prevented |
| Battle Rage | 3 turns | +0.67 successes/turn | 2 extra successes total |
| [Inspired] | 3-5 turns | +10 dmg/turn | 30-50 extra damage |
| Shield Absorption | Until depleted | 15 HP | 15 HP absorbed |

**Debuff Value Analysis:**

| Debuff | Duration | Value | Total Impact |
| --- | --- | --- | --- |
| [Vulnerable] | 3 turns | +25% dmg | ~15-20 extra damage |
| [Analyzed] | 4 turns | +0.67 acc/attacker | 2-3 extra hits |
| [Bleeding] | 2-3 turns | 1d6/turn | 7-10 damage |
| [Stunned] | 1 turn | Skip turn | 1 free turn |

---

## Layer 3: Technical Implementation (How It Works)

### Service Class

**File:** `RuneAndRust.Engine/CombatEngine.cs`

**Status Application:**

```csharp
// Defense Bonus (Defend action)
public void PlayerDefend(CombatState combatState)
{
    var defendRoll = _diceService.Roll(player.Attributes.Sturdiness);
    var defensePercent = Math.Min(75, defendRoll.Successes * 25);

    player.DefenseBonus = defensePercent;
    player.DefenseTurnsRemaining = 1;

    combatState.AddLogEntry($"  Defense raised by {defensePercent}% for next attack");
}

// [Vulnerable] Status (Anatomical Insight ability)
target.VulnerableTurnsRemaining = 3;
combatState.AddLogEntry($"  {target.Name} is [Vulnerable] for 3 turns! (+25% damage taken)");

// [Bleeding] Status (Precision Strike ability)
if (successes >= 3)
{
    target.BleedingTurnsRemaining = 2;
    combatState.AddLogEntry($"  {target.Name} is bleeding! (1d6 damage for 2 turns)");
}

// [Stunned] Status (Disrupt ability)
target.IsStunned = true;
target.StunTurnsRemaining = 1;
combatState.AddLogEntry($"  {target.Name} is disrupted and will skip their next turn!");

```

**Status Effect Processing (Turn Advancement):**

```csharp
public void NextTurn(CombatState combatState)
{
    // Decrement player status effects
    if (combatState.Player.DefenseTurnsRemaining > 0)
    {
        combatState.Player.DefenseTurnsRemaining--;
        if (combatState.Player.DefenseTurnsRemaining == 0)
        {
            combatState.Player.DefenseBonus = 0;
        }
    }

    if (combatState.Player.BattleRageTurnsRemaining > 0)
    {
        combatState.Player.BattleRageTurnsRemaining--;
        if (combatState.Player.BattleRageTurnsRemaining == 0)
        {
            combatState.AddLogEntry($"{combatState.Player.Name}'s battle rage ends.");
        }
    }

    // Decrement enemy status effects
    foreach (var enemy in combatState.Enemies.Where(e => e.IsAlive))
    {
        // Apply bleeding damage at start of enemy turn
        if (enemy.BleedingTurnsRemaining > 0)
        {
            var bleedDamage = _diceService.RollDamage(1);
            enemy.HP -= bleedDamage;
            combatState.AddLogEntry($"{enemy.Name} takes {bleedDamage} bleeding damage! (HP: {enemy.HP}/{enemy.MaxHP})");

            enemy.BleedingTurnsRemaining--;
            if (enemy.BleedingTurnsRemaining == 0)
            {
                combatState.AddLogEntry($"{enemy.Name} is no longer bleeding.");
            }

            // Check if bleeding killed the enemy
            if (!enemy.IsAlive)
            {
                combatState.AddLogEntry($"{enemy.Name} is destroyed!");
            }
        }

        // Decrement stun
        if (enemy.StunTurnsRemaining > 0)
        {
            enemy.StunTurnsRemaining--;
            if (enemy.StunTurnsRemaining == 0)
            {
                enemy.IsStunned = false;
            }
        }

        // Decrement [Analyzed]
        if (enemy.AnalyzedTurnsRemaining > 0)
        {
            enemy.AnalyzedTurnsRemaining--;
        }

        // Decrement [Vulnerable]
        if (enemy.VulnerableTurnsRemaining > 0)
        {
            enemy.VulnerableTurnsRemaining--;
        }

        // Decrement [Silenced]
        if (enemy.SilencedTurnsRemaining > 0)
        {
            enemy.SilencedTurnsRemaining--;
        }
    }
}

```

**Status Effect Usage in Calculations:**

```csharp
// Defense Bonus reduces damage
if (target.DefenseTurnsRemaining > 0)
{
    var reducedDamage = (int)(damage * (1 - target.DefenseBonus / 100.0));
    combatState.AddLogEntry($"{target.Name}'s defense reduces damage from {damage} to {reducedDamage}");
    damage = reducedDamage;
    target.DefenseTurnsRemaining--; // Consumed by first hit
}

// [Vulnerable] increases damage
if (target.VulnerableTurnsRemaining > 0)
{
    var vulnerableDamage = (int)(damage * 1.25);
    combatState.AddLogEntry($"  [Vulnerable] increases damage from {damage} to {vulnerableDamage}!");
    damage = vulnerableDamage;
}

// [Inspired] adds damage dice
if (player.InspiredTurnsRemaining > 0)
{
    totalDamageDice += 3;
    combatState.AddLogEntry($"  [Inspired] grants +3 damage dice!");
}

// [Analyzed] adds accuracy dice
if (target.AnalyzedTurnsRemaining > 0)
{
    bonusDice += 2;
    combatState.AddLogEntry($"  [Analyzed] grants +2 Accuracy against {target.Name}!");
}

// Battle Rage adds attack dice
if (player.BattleRageTurnsRemaining > 0)
{
    bonusDice += 2;
}

```

### Core Models

**Player Character Status Tracking:**

```csharp
// File: RuneAndRust.Core/PlayerCharacter.cs
public class PlayerCharacter
{
    // Defense
    public int DefenseBonus { get; set; } = 0;
    public int DefenseTurnsRemaining { get; set; } = 0;

    // Offensive Buffs
    public int BattleRageTurnsRemaining { get; set; } = 0;
    public int ShieldAbsorptionRemaining { get; set; } = 0;
    public int TempHP { get; set; } = 0;

    // Adept Specialization Status Effects
    public int VulnerableTurnsRemaining { get; set; } = 0;
    public int AnalyzedTurnsRemaining { get; set; } = 0;
    public int SeizedTurnsRemaining { get; set; } = 0;
    public int InspiredTurnsRemaining { get; set; } = 0;
    public int SilencedTurnsRemaining { get; set; } = 0;

    // Performance System
    public bool IsPerforming { get; set; } = false;
    public int PerformingTurnsRemaining { get; set; } = 0;
    public string? CurrentPerformance { get; set; } = null;
}

```

**Enemy Status Tracking:**

```csharp
// File: RuneAndRust.Core/Enemy.cs
public class Enemy
{
    // Defense
    public int DefenseBonus { get; set; } = 0;
    public int DefenseTurnsRemaining { get; set; } = 0;

    // Control Effects
    public bool IsStunned { get; set; } = false;
    public int StunTurnsRemaining { get; set; } = 0;

    // Damage-Over-Time
    public int BleedingTurnsRemaining { get; set; } = 0;

    // Debuffs
    public int AnalyzedTurnsRemaining { get; set; } = 0;
    public int VulnerableTurnsRemaining { get; set; } = 0;
    public int SilencedTurnsRemaining { get; set; } = 0;
}

```

### Database Schema

**No persistent status effect data.** Status effects are session-based (combat only) and do not persist across saves.

### Integration Points

**Integrates with:**

- **CombatEngine** - Applies, tracks, and processes all status effects
- **DiceService** - Rolls damage for DoT effects (Bleeding)
- **AbilitySystem** - Abilities apply status effects
- **PerformanceService** - Performance abilities manage durations

**Called by:**

- **PlayerUseAbility()** - Applies status effects from abilities
- **NextTurn()** - Decrements durations, applies DoT damage
- **Damage Calculation** - Modifies damage based on active effects

### Data Flow

```
[Ability/Action Applied]
        ↓
[Status Effect Created]
  • Set effect value (damage %, dice bonus, etc.)
  • Set duration (N turns)
  • Log application message
        ↓
[Status Active in Combat]
  • Modify calculations (damage, accuracy, etc.)
  • Display in combat log
  • Persist across turns
        ↓
[Each Turn End: NextTurn()]
  • Decrement duration counters
  • Apply DoT damage (if applicable)
  • Check for expiration (duration = 0)
        ↓
[Status Expires]
  • Remove effect from calculations
  • Log expiration message
  • Reset modifier values to 0

```

---

## Layer 4: Testing Coverage (How We Verify)

### Unit Tests

**File:** `RuneAndRust.Tests/StatusEffectTests.cs` *(to be created)*

**Expected Key Tests:**

```csharp
[TestMethod]
public void DefenseBonus_ReducesDamage_ByPercentage()
{
    // Arrange: Target with DefenseBonus = 50%
    // Act: Apply 20 damage
    // Assert: Final damage = 10 (50% reduction)
}

[TestMethod]
public void DefenseBonus_Expires_AfterFirstHit()
{
    // Arrange: Target with DefenseBonus, DefenseTurnsRemaining = 1
    // Act: Take damage
    // Assert: DefenseTurnsRemaining = 0, DefenseBonus = 0
}

[TestMethod]
public void Bleeding_Deals1d6PerTurn()
{
    // Arrange: Enemy with BleedingTurnsRemaining = 2
    // Act: NextTurn() twice
    // Assert: Damage dealt both turns, BleedingTurnsRemaining = 0 after
}

[TestMethod]
public void Bleeding_CanKillEnemy()
{
    // Arrange: Enemy with 3 HP, BleedingTurnsRemaining = 1
    // Act: NextTurn(), roll 4 damage
    // Assert: Enemy.IsAlive = false
}

[TestMethod]
public void Vulnerable_Increases DamageBy25Percent()
{
    // Arrange: Enemy with VulnerableTurnsRemaining = 3
    // Act: Deal 20 damage
    // Assert: Final damage = 25 (20 × 1.25)
}

[TestMethod]
public void Stunned_SkipsTurn()
{
    // Arrange: Enemy with IsStunned = true, StunTurnsRemaining = 1
    // Act: Enemy turn
    // Assert: No action taken, turn skipped
}

[TestMethod]
public void Inspired_Adds3DamageDice()
{
    // Arrange: Player with InspiredTurnsRemaining = 3, weapon 3d6
    // Act: Attack
    // Assert: Damage rolled with 6d6 (3+3)
}

[TestMethod]
public void Analyzed_Adds2AccuracyForAllAttackers()
{
    // Arrange: Enemy with AnalyzedTurnsRemaining = 4
    // Act: Two players attack
    // Assert: Both get +2 accuracy dice
}

[TestMethod]
public void StatusEffects_Decrement_AtEndOfRound()
{
    // Arrange: Various statuses with duration 3
    // Act: NextTurn()
    // Assert: All durations = 2
}

[TestMethod]
public void StatusEffects_Remove_WhenDurationZero()
{
    // Arrange: Status with duration 1
    // Act: NextTurn()
    // Assert: Status removed, no longer active
}

```

**Missing Coverage:**

- [ ]  Multiple status effects stacking (order of application)
- [ ]  Status effect conflicts (contradictory effects)
- [ ]  Performance interruption ([Silenced] ends performance)
- [ ]  DoT killing during status tick phase

### QA Checklist

### Status Application

- [ ]  Status effect applied when ability/action used
- [ ]  Duration set correctly
- [ ]  Effect value set correctly (%, dice, etc.)
- [ ]  Application message displayed in combat log

### Status Active (During Combat)

- [ ]  Status modifies calculations correctly
- [ ]  Status displays in combat log ([StatusName])
- [ ]  Multiple statuses can be active simultaneously
- [ ]  Status persists across turns until expiration

### Status Expiration

- [ ]  Duration decrements at end of each round
- [ ]  Status expires when duration = 0
- [ ]  Expiration message displayed
- [ ]  Calculations revert to base values after expiration

### Defensive Buffs

- [ ]  Defense Bonus reduces damage correctly (0-75%)
- [ ]  Shield Absorption blocks damage, then depletes
- [ ]  Temp HP damaged before real HP
- [ ]  Defense Bonus expires after first hit

### Offensive Buffs

- [ ]  Battle Rage adds +2 attack dice
- [ ]  [Inspired] adds +3 damage dice
- [ ]  Saga of Courage adds +2 accuracy dice
- [ ]  Buffs expire after duration

### Debuffs

- [ ]  [Vulnerable] increases damage by 25%
- [ ]  [Analyzed] grants +2 accuracy to all attackers
- [ ]  Debuffs expire after duration
- [ ]  Debuffs display correctly in combat log

### Control Effects

- [ ]  [Stunned] skips enemy turn
- [ ]  [Silenced] prevents spellcasting/performance
- [ ]  [Seized] prevents movement/actions
- [ ]  Control effects expire correctly

### Damage-Over-Time

- [ ]  [Bleeding] deals 1d6 damage per turn
- [ ]  DoT applied at start of target's turn
- [ ]  DoT can kill target (reduces HP to 0)
- [ ]  DoT expires after duration

### Known Issues

- **Issue 1:** Defense Bonus consumed on first hit, even if damage = 0 (fully blocked)
    - **Impact:** Slight waste if defense blocks all damage
    - **Priority:** Low
- **Issue 2:** Multiple [Vulnerable] applications don't stack (new replaces old)
    - **Impact:** No additional benefit from multiple applications
    - **Priority:** Low (design decision)
- **Issue 3:** Status effects on dead targets not cleaned up immediately
    - **Impact:** Minimal (dead enemies don't take turns)
    - **Priority:** Trivial

---

## Layer 5: Balance Considerations (Why These Numbers)

### Design Intent

The Status Effects System is designed to:

1. **Create Tactical Depth** - Timing and choice of buffs/debuffs matters
2. **Support Team Play** - Debuffs benefit all attackers ([Analyzed])
3. **Risk/Reward Tradeoffs** - Battle Rage grants offense but increases damage taken
4. **Enable Build Diversity** - Different specializations access different effects

### Power Budget

**Buff Power Ranking:**

1. **[Inspired]** (+3 damage dice, 3-5 turns) = ~30-50 extra damage = Extremely Powerful
2. **Defense 75%** (75% reduction, 1 turn) = ~15 damage prevented = Very Powerful
3. **Battle Rage** (+2 attack dice, 3 turns) = ~2 extra hits = Powerful
4. **Shield Absorption** (15 HP block) = 15 HP saved = Moderate
5. **Saga of Courage** (+2 accuracy, ongoing) = Moderate

**Debuff Power Ranking:**

1. **[Stunned]** (skip turn) = 1 free turn = Extremely Powerful (but short)
2. **[Vulnerable]** (+25% damage, 3 turns) = ~15-20 extra damage = Very Powerful
3. **[Analyzed]** (+2 accuracy all, 4 turns) = ~2-3 extra hits party-wide = Powerful
4. **[Bleeding]** (1d6 × 2-3 turns) = 7-10 damage = Moderate
5. **[Silenced]** (no spells, 3 turns) = Situational (only vs casters)

### Tuning Rationale

**Why These Duration Values?**

- **Short (1 turn):** High-power effects (Stun, Defense Bonus) to prevent overpowered stacking
- **Medium (2-3 turns):** Standard debuffs (Bleeding, Vulnerable) for sustained impact
- **Long (4 turns):** Team-wide effects ([Analyzed]) to reward coordination
- **Ongoing (until ended):** Performances (Saga of Courage) for strategic resource management

**Why 25% Damage Modifiers?**

- **Lower (10-15%):** Too weak, not worth ability cost
- **Current (25%):** Noticeable impact, worth tactical consideration
- **Higher (50%+):** Too swingy, makes fights too unpredictable

**Why 1d6 for Bleeding?**

- Average 3.5 damage per turn × 2-3 turns = 7-10 total
- Comparable to one weapon attack
- Low enough to not instantly kill, high enough to matter

### Known Balance Issues

### Issue 1: [Inspired] Dominance

**Problem:** [Inspired] (+3 damage dice) is significantly more powerful than other buffs.

**Data:**

- Longsword 3d6 (10.5 avg) → 6d6 (21 avg) = +10.5 damage per hit
- Over 3-5 turns: +30 to +50 total damage
- Compare to: [Vulnerable] on enemy = +25% of ~13 = ~3 damage per hit, +9-12 over 3 turns

**Proposed Tuning:**

- Reduce to +2 damage dice (still powerful, less overwhelming)
- Reduce duration to 2-3 turns (from 3-5)
- Add Stamina/Stress cost to maintain performance

### Issue 2: Control Effect Power

**Problem:** [Stunned] (skip turn) is extremely powerful but only lasts 1 turn.

**Current:**

- Stun = guaranteed free turn (player acts twice in a row)
- Extremely high value vs bosses or dangerous enemies

**Proposed Tuning:**

- Add Stun resistance (bosses immune or 50% chance to resist)
- Change to "Dazed" (-50% effectiveness) instead of full skip
- Increase duration to 2 turns if weakened

### Issue 3: [Analyzed] Party-Wide Benefit

**Problem:** In future multiplayer, [Analyzed] becomes exponentially more powerful.

**Current:**

- Solo: +2 accuracy for 1 attacker
- 4-player party: +2 accuracy for 4 attackers = 4× value

**Proposed Tuning:**

- Reduce duration to 2-3 turns (from 4) for multiplayer
- Cap number of benefiting attackers to 2-3
- Change to "first 3 attacks gain +2 accuracy" (consumable bonus)

---

## Cross-References

### Related Systems

- [Combat Resolution System](https://www.notion.so/combat-resolution.md) - Status effects integrated into combat flow
- [Damage Calculation System](https://www.notion.so/damage-calculation.md) - Status modifiers affect damage
- [Accuracy & Evasion System](https://www.notion.so/accuracy-evasion.md) - [Analyzed], Battle Rage affect accuracy
- [Ability System](https://www.notion.so/ability-system.md) - Abilities apply status effects

### Registry Entries

- See [Abilities Registry](https://www.notion.so/02-statistical-registry/abilities-registry.md) for abilities that apply status effects
- See [Status Effects Registry](https://www.notion.so/02-statistical-registry/status-effects-registry.md) for complete status list

### Technical References

- [CombatEngine API](https://www.notion.so/03-technical-reference/combat-service-api.md#status-effects)
- [PlayerCharacter Model](https://www.notion.so/03-technical-reference/database-schema.md#playercharacter)
- [Enemy Model](https://www.notion.so/03-technical-reference/database-schema.md#enemy)

---

## Changelog

### v0.1 - Basic Status Effects

- Defense Bonus (Defend action)
- Basic status tracking

### v0.2 - Warrior/Mystic Status Effects

- Battle Rage (Warrior)
- Shield Absorption (Mystic)
- [Stunned] (Disrupt ability)

### v0.3 - Scavenger Status Effects

- [Bleeding] (Precision Strike)

### v0.5 - Heretical Status Effects

- [Corrupted] (Blight Surge) - partially implemented

### v0.7 - Adept Specialization Status Effects

- [Vulnerable] (Bone-Setter: Anatomical Insight)
- [Analyzed] (Jötun-Reader: Exploit Design Flaw)
- [Inspired] (Skald: Saga of the Einherjar)
- [Silenced] (Skald: Song of Silence)
- [Seized] (Jötun-Reader: Architect of the Silence)
- Temp HP (Skald performance)
- Performance system (ongoing status effects)

### v0.16 - Content Expansion

- Additional status effect applications
- Expanded ability pool with status effects

---

**Documentation Status:** ✅ Complete
**Last Reviewed:** 2025-11-12
**Reviewer:** Claude (AI Documentation Assistant)
**Coverage:** Comprehensive (all 5 layers documented)
**Total Status Effects Documented:** 12+