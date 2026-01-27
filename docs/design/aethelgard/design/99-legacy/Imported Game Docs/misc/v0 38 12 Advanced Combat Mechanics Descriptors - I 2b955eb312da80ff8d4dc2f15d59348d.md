# v0.38.12: Advanced Combat Mechanics Descriptors - Integration Guide

**Status:** ✅ Implementation Complete - Ready for Integration
**Date:** 2025-11-18
**Branch:** claude/combat-mechanics-descriptors-01Qv5UQwSwCCvfuonBEDejCw

---

## Overview

v0.38.12 integration connects the Advanced Combat Mechanics Descriptor System (140 descriptors, 5 categories) with the core combat engine. This guide covers integration points for defensive actions, critical hits, fumbles, stances, and special maneuvers.

---

## Database Setup

### 1. Load Schema

```bash
sqlite3 game.db < Data/v0.38.12_combat_mechanics_descriptors_schema.sql

```

Creates 5 tables:

- `Combat_Defensive_Action_Descriptors`
- `Combat_Stance_Descriptors`
- `Combat_Critical_Hit_Descriptors`
- `Combat_Fumble_Descriptors`
- `Combat_Maneuver_Descriptors`

### 2. Load Data

```bash
sqlite3 game.db < Data/v0.38.12_combat_mechanics_descriptors_data.sql

```

Populates 140 descriptors:

- 40 Defensive Actions
- 30 Combat Stances
- 25 Critical Hits
- 25 Fumbles
- 20 Combat Maneuvers

### 3. Verify

```bash
sqlite3 game.db "SELECT COUNT(*) FROM Combat_Defensive_Action_Descriptors;"
sqlite3 game.db "SELECT COUNT(*) FROM Combat_Stance_Descriptors;"
sqlite3 game.db "SELECT COUNT(*) FROM Combat_Critical_Hit_Descriptors;"
sqlite3 game.db "SELECT COUNT(*) FROM Combat_Fumble_Descriptors;"
sqlite3 game.db "SELECT COUNT(*) FROM Combat_Maneuver_Descriptors;"

```

Expected output:

```
40
30
25
25
20

```

---

## Integration Points

### 1. Defensive Actions (CombatEngine)

**Goal:** Add flavor text to blocking, parrying, and dodging

**Implementation:**

```csharp
// In CombatEngine when player defends
public void PlayerDefend(string actionType, bool successful, int damageBlocked = 0)
{
    var outcomeType = successful ? "Success" : "Failure";

    // Check for critical success (perfect defense)
    if (successful && IsDefenseCritical())
    {
        outcomeType = "CriticalSuccess";
    }

    var defenseText = _combatFlavorService.GenerateDefensiveActionText(
        actionType,
        outcomeType,
        weaponType: player.EquippedShield?.Type ?? player.EquippedWeapon?.Type,
        attackIntensity: DetermineAttackIntensity(incomingDamage),
        variables: new Dictionary<string, string>
        {
            {"ActorName", player.Name},
            {"ShieldName", player.EquippedShield?.Name ?? "shield"},
            {"WeaponName", player.EquippedWeapon?.Name ?? "weapon"},
            {"DamageBlocked", damageBlocked.ToString()}
        });

    gameState.AddMessage(defenseText);
}

```

**Example Output:**

```
> defend

You time it perfectly! The attack glances off your shield at just the right angle, leaving your opponent off-balance!
[Counter-attack opportunity available]

```

---

### 2. Critical Hits (CombatEngine)

**Goal:** Make critical hits feel devastating

**Implementation:**

```csharp
// In CombatEngine when critical hit occurs
public void ProcessCriticalHit(PlayerCharacter player, Enemy enemy, Weapon weapon, int damage)
{
    var attackCategory = weapon.IsRanged ? "Ranged" : "Melee";
    var damageType = weapon.DamageType; // Slashing, Crushing, Piercing
    var specialEffect = DetermineSpecialEffect(damage, enemy);

    var critText = _combatFlavorService.GenerateCriticalHitText(
        attackCategory,
        damageType,
        weaponOrSpellType: weapon.Type,
        targetType: enemy.Type,
        specialEffect: specialEffect,
        variables: new Dictionary<string, string>
        {
            {"AttackerName", player.Name},
            {"WeaponName", weapon.Name},
            {"TargetName", enemy.Name},
            {"DamageAmount", damage.ToString()}
        });

    gameState.AddMessage(critText);

    // Apply special effect
    if (specialEffect == "Bleeding")
    {
        enemy.ApplyStatusEffect(new StatusEffect { Type = "Bleeding", Duration = 3 });
    }
}

```

**Example Output:**

```
> attack Raider

Your blade finds the perfect angle—it carves through armor and flesh like butter!
[Maximum damage + bleeding]

You hit the Raider for 45 damage! CRITICAL HIT!
The Raider is bleeding heavily.

```

---

### 3. Fumbles (CombatEngine)

**Goal:** Add consequences and flavor to critical failures

**Implementation:**

```csharp
// In CombatEngine when fumble occurs (natural 1 or critical failure)
public void ProcessFumble(PlayerCharacter player, string fumbleCategory, string fumbleType)
{
    var severity = DetermineSeverity(); // Minor, Moderate, Severe, Catastrophic
    var equipmentType = fumbleCategory switch
    {
        "AttackFumble" => player.EquippedWeapon?.Type,
        "DefensiveFumble" => player.EquippedShield?.Type ?? player.EquippedWeapon?.Type,
        "MagicFumble" => "Staff",
        _ => null
    };

    var fumbleText = _combatFlavorService.GenerateFumbleText(
        fumbleCategory,
        fumbleType,
        equipmentType,
        severity,
        environmentFactor: currentRoom.EnvironmentFactor);

    gameState.AddMessage(fumbleText);

    // Apply mechanical consequences
    switch (fumbleType)
    {
        case "WeaponDrop":
            player.DropEquippedWeapon();
            break;
        case "SelfInjury":
            var selfDamage = new Random().Next(1, 7); // 1d6
            player.TakeDamage(selfDamage);
            break;
        case "Tripped":
            player.ApplyStatusEffect(new StatusEffect { Type = "Prone", Duration = 1 });
            break;
    }
}

```

**Example Output:**

```
> attack Glitch

You fumble your attack and cut yourself! [1d6 self-damage]

You take 4 damage from your own weapon!

```

---

### 4. Combat Stances (CombatEngine or StanceCommand)

**Goal:** Provide tactical feedback when changing stances

**Implementation:**

```csharp
// In StanceCommand or CombatEngine
public void ChangeStance(PlayerCharacter player, string newStance)
{
    var previousStance = player.CurrentStance;
    var situationContext = DetermineSituationContext(); // Winning, Losing, EvenMatch, etc.
    var weaponConfig = DetermineWeaponConfiguration(player);

    var stanceText = _combatFlavorService.GenerateCombatStanceText(
        newStance,
        "Entering",
        previousStance: previousStance,
        situationContext: situationContext,
        weaponConfiguration: weaponConfig);

    gameState.AddMessage(stanceText);

    // Apply mechanical effects
    player.CurrentStance = newStance;
    ApplyStanceModifiers(player, newStance);
}

// For maintaining stance (optional, can be called each turn)
public void MaintainStance(PlayerCharacter player)
{
    if (ShouldShowStanceMaintenance()) // Only occasionally
    {
        var stanceText = _combatFlavorService.GenerateCombatStanceText(
            player.CurrentStance,
            "Maintaining");

        gameState.AddMessage(stanceText);
    }
}

```

**Example Output:**

```
> stance aggressive

You shift into an aggressive stance, weapon raised for maximum offense.
[Attack +2, Defense -2]

```

---

### 5. Special Maneuvers (CombatEngine)

**Goal:** Make special techniques feel distinctive

**Implementation:**

```csharp
// In CombatEngine when player uses special maneuver
public void AttemptManeuver(PlayerCharacter player, Enemy enemy, string maneuverType)
{
    var success = RollManeuverCheck(player, enemy, maneuverType);
    var outcomeType = success ? "Success" : "Failure";

    // Check for critical success
    if (success && IsCriticalSuccess())
    {
        outcomeType = "CriticalSuccess";
    }

    var maneuverText = _combatFlavorService.GenerateCombatManeuverText(
        maneuverType,
        outcomeType,
        weaponType: player.EquippedWeapon?.Type,
        targetType: enemy.Type,
        variables: new Dictionary<string, string>
        {
            {"AttackerName", player.Name},
            {"WeaponName", player.EquippedWeapon?.Name ?? "hands"},
            {"TargetName", enemy.Name},
            {"TargetWeapon", enemy.Weapon?.Name ?? "weapon"}
        });

    gameState.AddMessage(maneuverText);

    // Apply effects
    if (success)
    {
        switch (maneuverType)
        {
            case "Disarm":
                enemy.Disarm();
                break;
            case "Trip":
                enemy.ApplyStatusEffect(new StatusEffect { Type = "Prone", Duration = 1 });
                break;
            case "Grapple":
                enemy.ApplyStatusEffect(new StatusEffect { Type = "Grappled", Duration = 3 });
                break;
        }
    }
}

```

**Example Output:**

```
> disarm Bandit

You strike their weapon hand! Their weapon flies away! [Enemy disarmed]

The Bandit's Rusty Sword clatters to the ground!

```

---

## Helper Methods

### Determine Attack Intensity

```csharp
private string DetermineAttackIntensity(int incomingDamage)
{
    if (incomingDamage < 10) return "Light";
    if (incomingDamage < 20) return "Heavy";
    return "Overwhelming";
}

```

### Determine Special Effect (for critical hits)

```csharp
private string DetermineSpecialEffect(int damage, Enemy enemy)
{
    // Instant kill on overkill
    if (damage >= enemy.CurrentHP * 2)
        return "InstantKill";

    // Dying on near-death
    if (damage >= enemy.CurrentHP)
        return "Dying";

    // Weapon-type specific effects
    if (weaponDamageType == "Slashing")
        return "Bleeding";
    if (weaponDamageType == "Crushing")
        return "Stunned";
    if (weaponDamageType == "Piercing")
        return "Bleeding";

    return null;
}

```

### Determine Situation Context (for stances)

```csharp
private string DetermineSituationContext(PlayerCharacter player, List<Enemy> enemies)
{
    var playerHP = (float)player.CurrentHP / player.MaxHP;
    var enemyCount = enemies.Count;

    if (enemyCount > 3) return "Surrounded";
    if (enemyCount > 1) return "Outnumbered";
    if (playerHP > 0.7) return "Winning";
    if (playerHP < 0.3) return "Losing";
    return "EvenMatch";
}

```

---

## Service Initialization

### In GameEngine or CombatEngine Constructor

```csharp
public CombatEngine(DescriptorRepository descriptorRepository)
{
    _combatFlavorService = new CombatFlavorTextService(descriptorRepository);
    // ... other initialization
}

```

---

## Testing Integration

### Test 1: Defensive Action

```csharp
[Test]
public void TestDefensiveActionIntegration()
{
    // Setup
    var player = CreateTestPlayer();
    var enemy = CreateTestEnemy();

    // Execute
    engine.PlayerDefend("Block", true, damageBlocked: 15);

    // Verify
    var messages = gameState.GetRecentMessages();
    Assert.IsTrue(messages.Any(m => m.Contains("shield")));
}

```

### Test 2: Critical Hit

```csharp
[Test]
public void TestCriticalHitIntegration()
{
    // Setup
    var player = CreateTestPlayer();
    var enemy = CreateTestEnemy();

    // Execute
    engine.ProcessCriticalHit(player, enemy, player.Weapon, 45);

    // Verify
    var messages = gameState.GetRecentMessages();
    Assert.IsTrue(messages.Any(m => m.Contains("critical") || m.Contains("devastating")));
}

```

### Test 3: Fumble

```csharp
[Test]
public void TestFumbleIntegration()
{
    // Setup
    var player = CreateTestPlayer();

    // Execute
    engine.ProcessFumble(player, "AttackFumble", "WeaponDrop");

    // Verify
    Assert.IsNull(player.EquippedWeapon); // Weapon was dropped
    var messages = gameState.GetRecentMessages();
    Assert.IsTrue(messages.Any(m => m.Contains("fumble") || m.Contains("slips")));
}

```

---

## Example Combat Flow with v0.38.12

```
> attack Raider

You shift into an aggressive stance, weapon raised for maximum offense.
[Attack +2, Defense -2]

You swing your Iron Sword at the Raider!
Your blade finds the perfect angle—it carves through armor and flesh like butter!
[Maximum damage + bleeding]

You hit the Raider for 42 damage! CRITICAL HIT!
The Raider is bleeding heavily.

---

Raider's turn:
The Raider retaliates with a heavy swing!

> defend

You raise your shield. The blow cracks against it harmlessly.
[Damage blocked: 8]

---

> riposte

Perfect parry! You not only deflect the attack but throw your opponent off-balance!
[Free riposte attack]

You parry and immediately counter-strike! Your blade finds its mark!

You hit the Raider for 18 damage!
The Raider collapses!

```

---

## Summary

### ✅ Implementation Complete

- 5 descriptor classes created
- 5 database tables with 140 descriptors
- Repository extension methods
- Service extension methods
- Full fallback support

### ⏳ Integration Pending

- CombatEngine defensive action integration
- CombatEngine critical hit integration
- CombatEngine fumble integration
- StanceCommand or CombatEngine stance integration
- CombatEngine special maneuver integration

### 📊 Metrics

- **Files Created:** 7
- **Files Extended:** 1
- **Descriptors Available:** 140
- **Categories Covered:** 5
- **Backward Compatibility:** 100%

---

## Next Steps

1. **Integrate defensive actions:** Add `GenerateDefensiveActionText()` calls to CombatEngine
2. **Integrate critical hits:** Add `GenerateCriticalHitText()` calls when critical hit occurs
3. **Integrate fumbles:** Add `GenerateFumbleText()` calls when fumble occurs
4. **Integrate stances:** Add `GenerateCombatStanceText()` calls to stance system
5. **Integrate maneuvers:** Add `GenerateCombatManeuverText()` calls to special ability system
6. **Test in-game:** Verify all descriptor types generate correctly
7. **Monitor logs:** Check for missing descriptor warnings
8. **Adjust weights:** Fine-tune descriptor selection based on player feedback

For technical details, see:

- **Implementation:** `V0.38.12_IMPLEMENTATION_SUMMARY.md`
- **Service API:** `RuneAndRust.Engine/CombatFlavorTextService.cs` (lines 365-590)
- **Repository API:** `RuneAndRust.Persistence/DescriptorRepository_CombatMechanicsExtensions.cs`