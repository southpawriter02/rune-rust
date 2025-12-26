# Counter-Attack System Integration Guide

## v0.21.4: Formalized Counter-Attack System

This guide explains how to integrate the Counter-Attack System (Parry & Riposte) into the combat engine and specialization systems.

## Overview

The counter-attack system implements:

- **Universal Parry System**: All characters can attempt parries
- **Parry Quality Tiers**: Failed/Standard/Superior/Critical outcomes
- **Riposte Triggers**: Free counter-attacks on successful parries
- **Specialization Bonuses**: Hólmgangr and Atgeir-wielder parry bonuses

## Files Created

### Core Domain Models

- `/RuneAndRust.Core/CounterAttack.cs`
    - `ParryOutcome` enum
    - `ParryResult` class
    - `RiposteResult` class
    - `ParryStatistics` class
    - `ParryBonus` class
    - `ParryAttempt` class

### Persistence Layer

- `/RuneAndRust.Persistence/CounterAttackRepository.cs`
    - Database initialization (3 tables)
    - Parry statistics tracking
    - Parry bonus management
    - Combat log persistence

### Business Logic

- `/RuneAndRust.Engine/CounterAttackService.cs`
    - Parry execution logic
    - Riposte execution logic
    - Specialization bonus application
    - Turn management (parry limits)
    - Trauma economy integration

### Tests

- `/RuneAndRust.Tests/CounterAttackServiceTests.cs`
    - Comprehensive unit tests
    - 80%+ coverage target

## Database Schema

The system creates 3 tables automatically on first initialization:

```sql
-- Persistent parry statistics per character
CREATE TABLE ParryStatistics (
    CharacterID INTEGER PRIMARY KEY,
    TotalParryAttempts INTEGER DEFAULT 0,
    SuccessfulParries INTEGER DEFAULT 0,
    SuperiorParries INTEGER DEFAULT 0,
    CriticalParries INTEGER DEFAULT 0,
    FailedParries INTEGER DEFAULT 0,
    RipostesLanded INTEGER DEFAULT 0,
    RiposteKills INTEGER DEFAULT 0
);

-- Active parry bonuses from specializations/equipment
CREATE TABLE ParryBonuses (
    BonusID INTEGER PRIMARY KEY AUTOINCREMENT,
    CharacterID INTEGER NOT NULL,
    Source TEXT NOT NULL,
    BonusDice INTEGER DEFAULT 0,
    AllowsSuperiorRiposte INTEGER DEFAULT 0,
    ParriesPerRound INTEGER DEFAULT 1
);

-- Combat log of parry attempts
CREATE TABLE ParryAttempts (
    AttemptID INTEGER PRIMARY KEY AUTOINCREMENT,
    CombatInstanceID INTEGER NOT NULL,
    DefenderID INTEGER NOT NULL,
    AttackerID INTEGER NOT NULL,
    AttackAbility TEXT,
    ParryPoolRoll INTEGER NOT NULL,
    AttackerAccuracyRoll INTEGER NOT NULL,
    Outcome TEXT NOT NULL,
    RiposteTriggered INTEGER DEFAULT 0,
    RiposteDamage INTEGER DEFAULT 0,
    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
);

```

## PlayerCharacter Changes

Added two new fields to `PlayerCharacter.cs`:

```csharp
// v0.21.4: Counter-Attack System (Parry tracking)
public int ParriesRemainingThisTurn { get; set; } = 1; // Resets each turn
public bool ParryReactionPrepared { get; set; } = false; // Whether parry is prepared

```

## Integration Steps

### 1. Initialize CounterAttackService in CombatEngine

Add to `CombatEngine.cs` constructor:

```csharp
private readonly CounterAttackService _counterAttackService; // [v0.21.4]

public CombatEngine(
    DiceService diceService,
    SagaService sagaService,
    // ... other dependencies
    CounterAttackService? counterAttackService = null)
{
    // ... existing initialization
    _counterAttackService = counterAttackService
        ?? new CounterAttackService(diceService,
            new CounterAttackRepository(),
            new TraumaEconomyService());
}

```

### 2. Add Parry Command to Combat

In `CombatEngine.cs`, add a new method for handling parry commands:

```csharp
/// <summary>
/// v0.21.4: Prepare parry reaction for the next incoming attack
/// </summary>
public void PrepareParry(CombatState combatState)
{
    var player = combatState.Player;

    // Check if can parry this turn
    if (!_counterAttackService.CanParryThisTurn(player))
    {
        int maxParries = _counterAttackService.GetParriesPerRound(player);
        combatState.AddLogEntry($"You have no parry attempts remaining this turn! (Max: {maxParries})");
        return;
    }

    // Mark parry as prepared
    player.ParryReactionPrepared = true;

    combatState.AddLogEntry("You prepare to parry the next attack!");
    combatState.AddLogEntry($"  Parries remaining: {player.ParriesRemainingThisTurn - 1}/{_counterAttackService.GetParriesPerRound(player)}");

    // Get parry pool info for display
    int parryBonusDice = _counterAttackService.GetParryBonusDice(player);
    bool hasSuperiorRiposte = _counterAttackService.HasSuperiorRiposte(player);

    if (parryBonusDice > 0)
    {
        combatState.AddLogEntry($"  Parry bonus: +{parryBonusDice}d10");
    }

    if (hasSuperiorRiposte)
    {
        combatState.AddLogEntry("  [Reactive Parry]: Superior parries trigger Riposte!");
    }
}

```

### 3. Handle Parry During Enemy Attacks

Modify the enemy attack method to check for prepared parries:

```csharp
public void EnemyAttack(CombatState combatState, Enemy enemy)
{
    var player = combatState.Player;

    // ... existing attack roll logic ...
    int attackRoll = /* calculate attack roll */;

    // v0.21.4: Check if player has parry prepared
    if (player.ParryReactionPrepared)
    {
        combatState.AddLogEntry($"\\n{enemy.Name} attacks! You attempt to parry!");

        // Execute parry
        var parryResult = _counterAttackService.ExecuteParry(
            player,
            enemy,
            attackAccuracy: attackRoll,
            combatInstanceId: combatState.CombatInstanceID);

        // Consume parry attempt
        _counterAttackService.ConsumeParryAttempt(player);
        player.ParryReactionPrepared = false;

        // Display parry outcome
        switch (parryResult.Outcome)
        {
            case ParryOutcome.Critical:
                combatState.AddLogEntry($"✦ CRITICAL PARRY! ✦");
                combatState.AddLogEntry($"  Parry Roll: {parryResult.ParryRoll} vs Accuracy: {parryResult.AccuracyRoll}");
                combatState.AddLogEntry($"  Attack completely deflected!");
                break;

            case ParryOutcome.Superior:
                combatState.AddLogEntry($"◆ SUPERIOR PARRY! ◆");
                combatState.AddLogEntry($"  Parry Roll: {parryResult.ParryRoll} vs Accuracy: {parryResult.AccuracyRoll}");
                combatState.AddLogEntry($"  Attack deflected!");
                break;

            case ParryOutcome.Standard:
                combatState.AddLogEntry($"▸ PARRY! ▸");
                combatState.AddLogEntry($"  Parry Roll: {parryResult.ParryRoll} vs Accuracy: {parryResult.AccuracyRoll}");
                combatState.AddLogEntry($"  Attack blocked!");
                break;

            case ParryOutcome.Failed:
                combatState.AddLogEntry($"✗ PARRY FAILED! ✗");
                combatState.AddLogEntry($"  Parry Roll: {parryResult.ParryRoll} vs Accuracy: {parryResult.AccuracyRoll}");
                combatState.AddLogEntry($"  Attack hits normally!");
                break;
        }

        // Handle Riposte
        if (parryResult.RiposteTriggered && parryResult.Riposte != null)
        {
            combatState.AddLogEntry("");
            combatState.AddLogEntry("⚔ RIPOSTE! You counter-attack!");

            if (parryResult.Riposte.Hit)
            {
                combatState.AddLogEntry($"  Your riposte strikes {enemy.Name} for {parryResult.Riposte.DamageDealt} damage!");
                combatState.AddLogEntry($"  {enemy.Name} HP: {Math.Max(0, enemy.HP)}/{enemy.MaxHP}");

                if (parryResult.Riposte.KilledTarget)
                {
                    combatState.AddLogEntry($"  ✦ {enemy.Name} is destroyed by your riposte! ✦");
                }
            }
            else
            {
                combatState.AddLogEntry($"  Your riposte misses! ({parryResult.Riposte.AttackRoll} vs Defense {parryResult.Riposte.DefenseScore})");
            }
        }

        // Display stress change
        if (parryResult.StressChange < 0)
        {
            combatState.AddLogEntry($"  Stress relieved: {parryResult.StressChange}");
        }
        else if (parryResult.StressChange > 0)
        {
            combatState.AddLogEntry($"  Stress gained: +{parryResult.StressChange}");
        }

        // If parry succeeded, skip normal damage
        if (parryResult.Success)
        {
            return; // Attack was parried - no damage dealt
        }
    }

    // ... existing damage calculation if parry failed or wasn't used ...
}

```

### 4. Reset Parries Each Turn

Add to the turn start/end logic in `CombatEngine.cs`:

```csharp
/// <summary>
/// v0.21.4: Reset parries at the start of player's turn
/// </summary>
private void StartPlayerTurn(CombatState combatState)
{
    var player = combatState.Player;

    // Reset parries for new turn
    if (_counterAttackService != null)
    {
        _counterAttackService.ResetParriesForNewTurn(player);
    }

    // ... existing turn start logic ...
}

```

### 5. Add Parry to Command Parser

In your combat UI/command parser, add:

```csharp
case "parry":
case "p":
    _combatEngine.PrepareParry(combatState);
    break;

```

## Specialization Integration

### When Hólmgangr Unlocks Reactive Parry

In `AbilityService.cs` or wherever abilities are learned:

```csharp
// When character learns "Reactive Parry" ability (Hólmgangr Tier 2)
if (abilityName == "Reactive Parry")
{
    int rank = ability.CurrentRank;

    var counterAttackService = new CounterAttackService(
        _diceService,
        new CounterAttackRepository(),
        _traumaService);

    counterAttackService.ApplyHolmgangrReactiveParry(character, rank);

    // Log to player
    if (rank == 1)
    {
        Console.WriteLine("✦ REACTIVE PARRY UNLOCKED!");
        Console.WriteLine("  +1d10 to Parry Pool");
        Console.WriteLine("  Superior Parries trigger Riposte!");
    }
    else if (rank == 2)
    {
        Console.WriteLine("✦ REACTIVE PARRY RANK 2!");
        Console.WriteLine("  +2d10 to Parry Pool");
    }
    else if (rank == 3)
    {
        Console.WriteLine("✦ REACTIVE PARRY MASTERY!");
        Console.WriteLine("  Can parry TWICE per round!");
    }
}

```

### When Atgeir-wielder Unlocks Parry Bonus

```csharp
// When character unlocks Atgeir-wielder specialization
if (specialization == Specialization.AtgeirWielder)
{
    var counterAttackService = new CounterAttackService(
        _diceService,
        new CounterAttackRepository(),
        _traumaService);

    counterAttackService.ApplyAtgeirWielderParryBonus(character);

    Console.WriteLine("✦ ATGEIR-WIELDER PARRY BONUS!");
    Console.WriteLine("  +1d10 to Parry Pool (reach advantage)");
}

```

## Testing

Run the unit tests:

```bash
dotnet test --filter "FullyQualifiedName~CounterAttackServiceTests"

```

Expected results:

- All parry outcome calculations work correctly
- Parry pool calculations include bonuses
- Riposte triggers correctly based on outcome and specialization
- Specialization bonuses apply correctly
- Turn management enforces parry limits

## UI Display Examples

### Parry Command Help

```
> help parry

PARRY - Reactive Defense
  Type: Reaction (can be used on enemy turn)
  Frequency: 1/round (Hólmgangr Rank 3: 2/round)

  Prepare to parry the next incoming attack!

  Parry Pool = FINESSE + Weapon Skill + Bonuses

  Outcomes:
    • Failed (Parry < Accuracy): Attack hits normally
    • Standard (Parry = Accuracy): Attack blocked
    • Superior (Parry > Accuracy by 1-4): Attack deflected
    • Critical (Parry > Accuracy by 5+): Perfect deflection!

  Riposte (Free counter-attack):
    • Critical Parries: ALL characters can riposte
    • Superior Parries: ONLY Hólmgangr with Reactive Parry

```

### Statistics Display

```
> stats parry

═══════════════════════════════════════
         PARRY STATISTICS
═══════════════════════════════════════
Total Attempts:     47
Success Rate:       72% (34/47)

Parry Outcomes:
  • Failed:         13
  • Standard:       11
  • Superior:       18
  • Critical:       5

Ripostes:
  • Landed:         21
  • Kills:          7
═══════════════════════════════════════

```

## Troubleshooting

### Issue: "No parry attempts remaining"

**Solution**: Check that `ResetParriesForNewTurn()` is called at the start of each turn.

### Issue: Superior Parries don't trigger Riposte for Hólmgangr

**Solution**: Verify that `ApplyHolmgangrReactiveParry()` was called when the ability was learned.

### Issue: Parry statistics not persisting

**Solution**: Ensure the database connection string is correct and the database file has write permissions.

## Future Extensions

The system is designed to be extensible for future additions:

1. **Equipment Parry Bonuses**: Shields, defensive weapons
2. **Status Effect Interactions**: Slowed/Stunned characters can't parry
3. **Enemy Parrying**: Enemies could also parry player attacks
4. **Skjaldmær Ally Parry**: Interposing Shield (parry for allies)
5. **Parry Training**: Spending PP to increase base parry skill

## Version History

- **v0.21.4**: Initial implementation of universal parry system
    - Core parry mechanics (v2.0 canonical)
    - Riposte triggers and execution
    - Hólmgangr Reactive Parry bonuses
    - Atgeir-wielder reach bonus
    - Trauma economy integration
    - Statistics tracking

## References

- v2.0 Parry System Specification
- v2.0 Hólmgangr Reactive Parry Specification
- v0.15 Trauma Economy Specification
- v0.21 Advanced Combat Mechanics