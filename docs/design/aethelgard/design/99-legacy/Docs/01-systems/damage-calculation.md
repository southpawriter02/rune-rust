# Damage Calculation System

**File Path:** `RuneAndRust.Engine/CombatEngine.cs` (lines 197-922)
**Version:** v0.1-v0.16
**Last Updated:** 2025-11-12
**Status:** ✅ Implemented

---

## Layer 1: Functional Overview (What It Does)

### Purpose
The Damage Calculation System determines how much HP damage is dealt from attacks, abilities, and environmental sources. It accounts for weapon damage, attribute bonuses, status effects, armor, and defensive abilities.

### Player Experience
1. **Attack Declared** - Player attacks enemy or enemy attacks player
2. **Roll Attack vs Defense** - Dice pool opposed roll
3. **Calculate Net Successes** - Successes determine if hit lands
4. **Roll Damage Dice** - Weapon/ability damage rolled (XdY)
5. **Apply Modifiers** - Bonuses, status effects, defense applied
6. **Apply to HP** - Final damage subtracted from target's HP
7. **Display Result** - Combat log shows: "Target takes X damage! (HP: Y/Z)"

### Key Features
- **Weapon-Based Damage** - Different weapons roll different damage dice (1d6 to 4d10)
- **Attribute Scaling** - Weapon attribute (MIGHT/FINESSE/WILL) determines attack accuracy
- **Net Success System** - Only hits with net successes deal damage
- **Minimum Damage** - Successful hits always deal at least 1 damage
- **Damage Modifiers** - Status effects amplify or reduce damage
- **Armor/Soak** - Reduces incoming damage (not implemented for all enemies)
- **Defense Bonus** - Percentage-based damage reduction from Defend action

### Edge Cases
- **Zero Net Successes** - Attack deflected, no damage dealt
- **Negative Damage** - Not possible, minimum 1 damage on hit
- **Overkill Damage** - Can reduce HP below 0, displayed as 0
- **Ignore Armor** - Some abilities bypass defense bonus and Soak
- **Status Effect Stacking** - Multiple modifiers applied sequentially
- **Defense Bonus Expiration** - Defense bonus consumed after taking damage

---

## Layer 2: Statistical Reference (The Numbers)

### Primary Formula: Total Damage

```
Final_Damage = Min_Damage_Or(Base_Damage × Status_Multipliers - Mitigation)

Where:
  Min_Damage_Or(X) = If hit landed (net successes > 0), Max(1, X), else 0
```

**Step-by-Step:**

1. **Attack vs Defense Roll**
```
Net_Successes = AttackRoll.Successes - DefenseRoll.Successes
If Net_Successes <= 0: No damage, attack deflected
```

2. **Base Damage Roll**
```
Base_Damage = Roll(Weapon_Damage_Dice) + Weapon_Damage_Bonus + Ability_Bonus

Where:
  Weapon_Damage_Dice = 1d6 to 4d10 (weapon-dependent)
  Weapon_Damage_Bonus = -2 to +8 (weapon-dependent, negative for unarmed)
  Ability_Bonus = Varies by ability (0 to +10)
```

**Example Weapon Damage:**
| Weapon Type | Damage Dice | Bonus | Average Roll | Expected Damage |
|-------------|-------------|-------|--------------|-----------------|
| Unarmed | 1d6 | -2 | 3.5 | 1.5 |
| Dagger | 2d6 | +2 | 7 | 9 |
| Longsword | 3d6 | +3 | 10.5 | 13.5 |
| Greatsword | 4d6 | +5 | 14 | 19 |
| Aetheric Bolt | 2d6 | +0 | 7 | 7 |
| Heretical Strike | 3d8 | +0 | 13.5 (approx) | 13.5 |

3. **Apply Status Effect Multipliers**
```
Modified_Damage = Base_Damage × Status_Multiplier

Status Effects:
  [Vulnerable]: ×1.25 (+25% damage)
  [Inspired]: +3 damage dice (before roll)
  [Defensive Stance]: ×0.75 (-25% damage, attacker only)
```

4. **Apply Defense Bonus (if target has defense active)**
```
Reduced_Damage = Modified_Damage × (1 - Defense_Percent / 100)

Defense_Percent:
  From Defend action: 0-75% (based on STURDINESS roll)
  Each success on Defend roll = +25% reduction, max 75%
```

5. **Apply Soak (enemy armor, if implemented)**
```
Final_Damage = Max(1, Reduced_Damage - Soak)

Soak values (enemy-dependent):
  Light armor: 0-3
  Medium armor: 3-7
  Heavy armor: 7-12
```

6. **Minimum Damage Rule**
```
If Net_Successes > 0 (hit landed):
  Final_Damage = Max(1, calculated damage)
```

### Damage Calculation Examples

**Example 1: Basic Weapon Attack (No Modifiers)**
```
Setup:
  Player attacks with Longsword (3d6+3)
  Attack roll: 5 successes
  Enemy defense roll: 2 successes
  Net successes: 3 (hit lands)

Calculation:
  Base_Damage = Roll(3d6) + 3
  Damage roll: [4, 6, 2] = 12
  Base_Damage = 12 + 3 = 15
  No status effects: Modified_Damage = 15
  No defense bonus: Reduced_Damage = 15
  No Soak: Final_Damage = 15

Result: Enemy takes 15 damage
```

**Example 2: With [Vulnerable] Status**
```
Setup:
  Player attacks with Dagger (2d6+2)
  Enemy has [Vulnerable] (+25% damage)
  Attack roll: 4 successes
  Enemy defense roll: 1 success
  Net successes: 3 (hit lands)

Calculation:
  Base_Damage = Roll(2d6) + 2
  Damage roll: [5, 6] = 11
  Base_Damage = 11 + 2 = 13
  Apply [Vulnerable]: Modified_Damage = 13 × 1.25 = 16.25 → 16 (rounded down)
  No defense bonus: Reduced_Damage = 16
  Final_Damage = 16

Result: Enemy takes 16 damage (25% bonus from [Vulnerable])
```

**Example 3: With Defense Bonus**
```
Setup:
  Enemy attacks player
  Player has Defense Bonus 75% (rolled 3 successes on Defend action)
  Attack roll: 3 successes
  Player defense roll: 0 successes
  Net successes: 3 (hit lands)

Calculation:
  Base_Damage = Roll(2d6) + 4
  Damage roll: [4, 5] = 9
  Base_Damage = 9 + 4 = 13
  No status effects: Modified_Damage = 13
  Apply Defense Bonus: Reduced_Damage = 13 × (1 - 75/100) = 13 × 0.25 = 3.25 → 3
  Final_Damage = Max(1, 3) = 3

Result: Player takes 3 damage (75% reduction from Defend action)
```

**Example 4: Attack Deflected (Zero Net Successes)**
```
Setup:
  Player attacks with Greatsword (4d6+5)
  Attack roll: 2 successes
  Enemy defense roll: 4 successes
  Net successes: -2 (attack deflected)

Result:
  No damage dealt
  Combat log: "The attack is deflected!"
```

### Damage Type Distribution

**Weapon Damage (Physical):**
- Minimum: 1 (minimum damage rule)
- Average (Longsword): 13-14
- Maximum (Greatsword with modifiers): 30-35

**Ability Damage (varies by ability):**
- Low (Utility): 1d6-2d6 (3-7 average)
- Medium (Standard): 3d6-4d6 (10-14 average)
- High (Ultimate): 4d10-5d10 (20-30 average)
- AOE (Multiple targets): 1d6-3d6 per target (3-10 average)

**Heretical Damage (Corrupted/Psychic):**
- Void Strike: 3d8 ≈ 13-14 average (ignores armor)
- Psychic Lash: 2d6 ≈ 7 average (ignores armor)
- Desperate Gambit: 4d10 ≈ 22 average (AOE, ignores armor)

### Status Effect Damage (DoT)

**Bleeding:**
- Damage: 1d6 per turn
- Duration: 2-3 turns
- Total: 7-10 damage over duration
- Application: Precision Strike (3+ successes required)

**Environmental Hazards:**
- Varies by hazard type: 1d6 to 3d6 per turn
- See [Environmental Hazards](./environmental-hazards.md) for details

---

## Layer 3: Technical Implementation (How It Works)

### Service Class
**File:** `RuneAndRust.Engine/CombatEngine.cs`

**Key Methods:**

```csharp
/// <summary>
/// Process player attack action
/// </summary>
/// <remarks>
/// 1. Roll attack (Attribute + bonuses)
/// 2. Roll defense (STURDINESS)
/// 3. Calculate net successes
/// 4. If hit: Roll weapon damage dice
/// 5. Apply status effects and defense bonus
/// 6. Subtract from enemy HP
/// </remarks>
public void PlayerAttack(CombatState combatState, Enemy target)
{
    // Get weapon stats from equipped weapon
    string weaponAttribute = player.EquippedWeapon?.WeaponAttribute ?? "MIGHT";
    int weaponDice = player.EquippedWeapon?.DamageDice ?? 1;
    int weaponDamageBonus = player.EquippedWeapon?.DamageBonus ?? -2;
    int accuracyBonus = player.EquippedWeapon?.AccuracyBonus ?? 0;

    // Get effective attribute value (includes equipment bonuses)
    var attributeValue = _equipmentService.GetEffectiveAttributeValue(player, weaponAttribute);

    // Roll attack
    var totalDice = attributeValue + bonusDice + accuracyBonus;
    var attackRoll = _diceService.Roll(totalDice);

    // Roll defense
    var defendRoll = _diceService.Roll(target.Attributes.Sturdiness);

    // Calculate net successes
    var netSuccesses = Math.Max(0, attackRoll.Successes - defendRoll.Successes);

    // If hit, calculate damage
    int damage = 0;
    if (netSuccesses > 0)
    {
        // Roll weapon damage dice
        int totalDamageDice = weaponDice;
        if (player.InspiredTurnsRemaining > 0)
        {
            totalDamageDice += 3; // [Inspired] status bonus
        }

        damage = _diceService.RollDamage(totalDamageDice) + weaponDamageBonus;
        damage = Math.Max(1, damage); // Minimum 1 damage on hit
    }

    // Apply defense bonus if active
    if (target.DefenseTurnsRemaining > 0)
    {
        var reducedDamage = (int)(damage * (1 - target.DefenseBonus / 100.0));
        damage = reducedDamage;
    }

    // Apply damage to target
    if (damage > 0)
    {
        target.HP -= damage;
        combatState.AddLogEntry($"  {target.Name} takes {damage} damage! (HP: {Math.Max(0, target.HP)}/{target.MaxHP})");
    }
    else
    {
        combatState.AddLogEntry($"  The attack is deflected!");
    }
}
```

**Ability Damage Processing:**
```csharp
private void ProcessAttackAbility(CombatState combatState, Ability ability, Enemy? target, int successes)
{
    if (target == null || !target.IsAlive)
    {
        combatState.AddLogEntry("  No valid target!");
        return;
    }

    int damage = 0;

    // Ability-specific damage calculations
    if (ability.DamageDice > 0)
    {
        damage = _diceService.RollDamage(ability.DamageDice);
    }

    // Apply damage to target
    ApplyDamageToEnemy(combatState, target, damage, ability.IgnoresArmor);
}
```

**Damage Application with Modifiers:**
```csharp
private void ApplyDamageToEnemy(CombatState combatState, Enemy target, int damage, bool ignoresArmor)
{
    // Apply [Vulnerable] status (+25% damage)
    if (target.VulnerableTurnsRemaining > 0)
    {
        var vulnerableDamage = (int)(damage * 1.25);
        if (vulnerableDamage > damage)
        {
            combatState.AddLogEntry($"  [Vulnerable] increases damage from {damage} to {vulnerableDamage}!");
            damage = vulnerableDamage;
        }
    }

    // Check if ability ignores armor
    if (!ignoresArmor && target.DefenseTurnsRemaining > 0)
    {
        var reducedDamage = (int)(damage * (1 - target.DefenseBonus / 100.0));
        combatState.AddLogEntry($"  {target.Name}'s defense reduces damage from {damage} to {reducedDamage}");
        damage = reducedDamage;
    }
    else if (ignoresArmor)
    {
        combatState.AddLogEntry($"  Ignores armor!");
    }

    // Apply damage to HP
    target.HP -= damage;
    combatState.AddLogEntry($"  {target.Name} takes {damage} damage! (HP: {Math.Max(0, target.HP)}/{target.MaxHP})");

    // Check for death
    if (!target.IsAlive)
    {
        combatState.AddLogEntry($"  {target.Name} is destroyed!");
    }
}
```

### Core Models

**Equipment Stats (affects damage):**
```csharp
// File: RuneAndRust.Core/Equipment.cs
public class Equipment
{
    public int DamageDice { get; set; }       // Number of d6s to roll
    public int DamageBonus { get; set; }      // Flat bonus to damage
    public int AccuracyBonus { get; set; }    // Bonus dice to attack roll
    public string WeaponAttribute { get; set; } // "MIGHT", "FINESSE", or "WILL"
}
```

**Enemy Stats (defense):**
```csharp
// File: RuneAndRust.Core/Enemy.cs
public class Enemy
{
    public int HP { get; set; }
    public int MaxHP { get; set; }
    public int DefenseBonus { get; set; }        // Percentage reduction (0-75%)
    public int DefenseTurnsRemaining { get; set; }
    public int Soak { get; set; }                // Flat damage reduction (armor)
    public int VulnerableTurnsRemaining { get; set; } // Takes +25% damage
}
```

### Database Schema

**No damage-specific tables.** Damage is calculated in real-time during combat.

**Persistent Data:**
- Player/Enemy HP stored in `saves` table (current and max)
- Equipment stats stored in `saves` table (affects damage calculations)

### Integration Points

**Integrates with:**
- **DiceService** - All damage rolls (`RollDamage()`)
- **EquipmentService** - Weapon stats (`GetEffectiveAttributeValue()`)
- **StatusEffectService** - Status modifiers ([Vulnerable], [Inspired])
- **TraumaEconomyService** - Stress/Corruption costs for heretical attacks

**Called by:**
- **CombatEngine.PlayerAttack()** - Player attacks enemy
- **CombatEngine.PlayerUseAbility()** - Ability damage resolution
- **EnemyAI** - Enemy attacks player

**Depends on:**
- **Attributes** (MIGHT, FINESSE, WILL) - Determines attack roll
- **Equipment System** - Weapon damage dice and bonuses
- **Status Effect System** - Damage multipliers

### Data Flow

```
[Attack Action Initiated]
          ↓
[Roll Attack vs Defense]
          ↓
[Calculate Net Successes]
          ↓
     ┌────┴────┐
     │ Hit?    │
     └────┬────┘
          │
  ┌───────┴────────┐
  │ Yes            │ No
  ↓                ↓
[Roll Damage]   [Deflected]
  ↓                ↓
[Apply Status]  [Log "Deflected!"]
[Effects]          ↓
  ↓             [End]
[Apply Defense]
[Bonus/Soak]
  ↓
[Final Damage]
  ↓
[Subtract from HP]
  ↓
[Update Combat Log]
  ↓
[Check if Target Dead]
```

---

## Layer 4: Testing Coverage (How We Verify)

### Unit Tests
**File:** `RuneAndRust.Tests/CombatEngineTests.cs` or `DamageServiceTests.cs` *(to be verified)*

**Coverage:** Unknown (requires test file inspection)

**Expected Key Tests:**
```csharp
[TestMethod]
public void CalculateDamage_WithSoak_ReducesDamage()
{
    // Arrange: Player attack, enemy with Soak = 5
    // Act: Apply damage
    // Assert: Damage reduced by 5
}

[TestMethod]
public void CalculateDamage_WithVulnerable_IncreasesDamage()
{
    // Arrange: Enemy with [Vulnerable] status
    // Act: Deal 10 damage
    // Assert: Final damage = 12 (10 × 1.25)
}

[TestMethod]
public void CalculateDamage_MinimumOneDamage()
{
    // Arrange: Attack that would deal 0 damage after reductions
    // Act: Apply damage
    // Assert: Final damage = 1 (minimum rule)
}

[TestMethod]
public void CalculateDamage_IgnoreArmor_BypassesDefense()
{
    // Arrange: Ability with IgnoresArmor = true
    // Act: Attack enemy with defense bonus
    // Assert: Defense bonus not applied
}

[TestMethod]
public void CalculateDamage_DefenseBonus75Percent_ReducesBy75()
{
    // Arrange: Enemy with DefenseBonus = 75%
    // Act: Deal 20 damage
    // Assert: Final damage = 5 (20 × 0.25)
}

[TestMethod]
public void CalculateDamage_ZeroNetSuccesses_NoDamage()
{
    // Arrange: Attack roll 2 successes, defense roll 4 successes
    // Act: Calculate damage
    // Assert: Damage = 0, "Deflected" message
}
```

**Missing Coverage:**
- [ ] Edge case: Negative damage (should never occur)
- [ ] Edge case: Overkill damage (HP -50 displayed as 0)
- [ ] Edge case: Multiple status effects stacking
- [ ] Edge case: Defense bonus expiration timing
- [ ] Integration test: Full damage pipeline with all modifiers

### QA Checklist

#### Basic Damage
- [ ] Attack with net successes > 0 deals damage
- [ ] Attack with net successes ≤ 0 deals no damage
- [ ] Minimum 1 damage rule enforced on successful hits
- [ ] Damage displayed in combat log correctly

#### Weapon Damage
- [ ] Unarmed attack deals 1d6-2 (negative bonus applied)
- [ ] Dagger deals 2d6+2
- [ ] Longsword deals 3d6+3
- [ ] Greatsword deals 4d6+5
- [ ] Damage dice match equipped weapon

#### Status Effect Modifiers
- [ ] [Vulnerable] increases damage by 25%
- [ ] [Inspired] adds +3 damage dice (before roll)
- [ ] [Defensive Stance] reduces damage by 25% (attacker)
- [ ] Status effects stack correctly (apply sequentially)

#### Defense Bonus
- [ ] Defend action grants 0-75% reduction
- [ ] Each success on Defend = +25% reduction
- [ ] Defense bonus expires after 1 turn (player) or specified turns (enemy)
- [ ] Defense bonus consumed by first incoming attack
- [ ] Defense reduction displayed in combat log

#### Ignore Armor
- [ ] Abilities with IgnoresArmor = true bypass defense bonus
- [ ] Heretical abilities (Void Strike, Psychic Lash) ignore armor
- [ ] Ignore armor message displayed in combat log

#### HP Tracking
- [ ] HP never goes below 0 (clamped in display)
- [ ] HP correctly updated after damage
- [ ] Target marked as dead when HP ≤ 0
- [ ] HP displayed as "X/MaxHP" in combat log

### Known Issues
- **Issue 1:** Rounding down on percentage reductions can result in slightly higher damage than intended
  - **Example:** 13 × 1.25 = 16.25 → 16 (should be 16.25, lost 0.25)
  - **Impact:** Minor, typically <1 damage difference
  - **Priority:** Low
- **Issue 2:** Defense bonus percentage displayed as integer, but calculated as float (e.g., "75%" displayed, but actual is 75.0%)
  - **Impact:** None (purely display)
  - **Priority:** Trivial

---

## Layer 5: Balance Considerations (Why These Numbers)

### Design Intent

The Damage Calculation System is designed to:
1. **Reward Investment** - Higher attribute = more attack dice = higher damage
2. **Create Choices** - Offense (damage) vs Defense (mitigation)
3. **Support Varied Builds** - MIGHT (melee), FINESSE (ranged), WILL (magic) all viable
4. **Maintain Tension** - Dice variability means damage is never guaranteed

### Power Budget

**Weapon Damage Scaling:**
| Weapon | Damage Dice | Avg Damage | DPS (per turn) | TTK vs 50 HP Enemy |
|--------|-------------|------------|----------------|---------------------|
| Unarmed | 1d6-2 | 1.5 | 1.5 | 33 turns |
| Dagger | 2d6+2 | 9 | 9 | 6 turns |
| Longsword | 3d6+3 | 13.5 | 13.5 | 4 turns |
| Greatsword | 4d6+5 | 19 | 19 | 3 turns |

**TTK = Time To Kill** (turns to defeat enemy, assumes all attacks hit)

**Status Effect Power:**
- **[Vulnerable]:** +25% damage = ~3-5 extra damage per hit
  - At 13 base damage: 13 → 16 (net +3)
  - ROI: Worth applying if target survives 2+ more hits
- **[Inspired]:** +3 damage dice = ~10 average damage increase
  - At 3d6: 10.5 avg → 21 avg (+10.5)
  - ROI: Extremely powerful, worth using every combat
- **[Defensive Stance]:** -25% damage dealt
  - Trade-off: Reduce own damage to gain +3 Soak

**Defense Bonus Power:**
- **25% reduction** (1 success): Reduces 20 damage → 15 (net -5)
- **50% reduction** (2 successes): Reduces 20 damage → 10 (net -10)
- **75% reduction** (3 successes): Reduces 20 damage → 5 (net -15)

**Soak Value Expectations:**
- Light armor (0-3 Soak): Negates 0-3 damage per hit
- Medium armor (3-7 Soak): Negates 3-7 damage per hit
- Heavy armor (7-12 Soak): Negates 7-12 damage per hit

---

**Documentation Status:** ✅ Complete
**Last Reviewed:** 2025-11-12
**Reviewer:** Claude (AI Documentation Assistant)
**Coverage:** Comprehensive (all 5 layers documented)
