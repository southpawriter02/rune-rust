# v0.4 Testing Guide

## Overview
This guide explains how to run and interpret the v0.4 unit tests that generate statistical data for balance validation and playtest analysis.

## Prerequisites
- .NET 8.0 SDK or later
- xUnit test runner
- All project dependencies installed

## Running the Tests

### Run All v0.4 Tests
```bash
dotnet test --filter "FullyQualifiedName~V04" --logger "console;verbosity=detailed"
```

### Run Specific Test Categories

**Combat Simulation Tests Only:**
```bash
dotnet test --filter "FullyQualifiedName~V04CombatSimulationTests"
```

**Mechanics Validation Tests Only:**
```bash
dotnet test --filter "FullyQualifiedName~V04MechanicsTests"
```

### Run Specific Simulations

**East Wing Path Simulation:**
```bash
dotnet test --filter "FullyQualifiedName~V04_EastWing_FullPathSimulation"
```

**West Wing Path Simulation:**
```bash
dotnet test --filter "FullyQualifiedName~V04_WestWing_FullPathSimulation"
```

## Test Output Interpretation

### Combat Simulation Results

Each combat simulation runs 100 iterations and outputs:

```
=== WARRIOR VS WAR-FRAME (100 simulations) ===
Win Rate: 75.0%
Average Turns to Win: 8.2
Average HP Remaining: 12.5
Average Damage Taken: 17.5
```

**Key Metrics:**
- **Win Rate**: Percentage of successful outcomes (should be 50-85% for balanced encounters)
- **Average Turns**: Combat length (4-10 turns is good pacing)
- **HP Remaining**: How much health player has left (indicates difficulty)
- **Damage Taken**: Total damage sustained (helps calibrate healing needs)

**Balance Indicators:**
- Win Rate < 50%: Enemy too strong, consider reducing HP/damage
- Win Rate > 85%: Enemy too weak, consider increasing HP/damage
- Avg Turns < 4: Combat too quick, lacks engagement
- Avg Turns > 12: Combat too slow, becomes tedious
- HP Remaining < 5: Very risky, player may die to next encounter
- HP Remaining > 20: Too easy, reduce enemy difficulty

### Path Simulation Results

Full path simulations run 50 iterations and output:

```
=== EAST WING (COMBAT PATH) - 50 runs ===
Completion Rate: 42.0% (21/50)
Average Encounters Completed: 4.8/6

Deaths by Room:
  Room 2 (Corridor): 12 deaths (24%)
  Room 3 (Salvage Bay): 8 deaths (16%)
  Room 4 (Operations Center): 0 deaths (0%)
  Room 5 (Arsenal): 6 deaths (12%)
  Room 6 (Training Chamber): 3 deaths (6%)
  Room 7 (Ammunition Forge): 0 deaths (0%)

Total Player Deaths: 29
Players who completed: 21
```

**Key Metrics:**
- **Completion Rate**: Percentage who finish the entire path
- **Average Encounters Completed**: How far players get before dying
- **Deaths by Room**: Which rooms are difficulty spikes

**Balance Indicators:**
- Completion Rate < 20%: Path too difficult
- Completion Rate > 60%: Path too easy
- Death spike in one room: Difficulty imbalance, redistribute challenge
- Early deaths (Room 2-3): Starting equipment insufficient
- Late deaths (Room 6-7): Attrition too high, add healing/rest

## Expected Results Based on Balance Analysis

### Individual Combat Simulations

Based on BALANCE_REVIEW_V04.md predictions:

**Warrior vs War-Frame (East Wing, Boss Room)**
- Expected Win Rate: 55-70%
- Expected Avg Turns: 9-11
- Expected HP Remaining: 8-15
- Notes: Should be challenging but winnable with Clan-Forged equipment

**Mystic vs Aetheric Aberration (West Wing, Boss Room)**
- Expected Win Rate: 60-75%
- Expected Avg Turns: 7-9
- Expected HP Remaining: 10-18
- Notes: Lower enemy HP but higher damage output

**Scavenger vs Forlorn Scholar (Talk Success)**
- Expected Win Rate: 85-95%
- Expected Avg Turns: 5-7
- Expected HP Remaining: 20-25
- Notes: Should be easier if talk succeeds (+2 bonus)

### Path Simulations

**East Wing (Combat Path)**
- Expected Completion Rate: 35-50%
- Expected Deaths: Concentrated in Arsenal (Room 5) and Training Chamber (Room 6)
- Notes: High combat load (6 encounters), attrition is the main challenge

**West Wing (Exploration Path)**
- Expected Completion Rate: 45-60%
- Expected Deaths: Spread across Research Archives and Specimen Containment
- Notes: Fewer combats (4-5), but lower Legend means weaker equipment

## Validating Balance Predictions

After running the tests, compare actual results to predictions:

### 1. Win Rate Validation
- Are individual encounter win rates within expected ranges?
- Which encounters are significantly easier/harder than predicted?

### 2. Path Completion Validation
- Is the East Wing 10-15% harder than West Wing as predicted?
- Are death distributions matching predictions?
- Is the Legend disparity (East 400 vs West 300) creating equipment gaps?

### 3. Combat Pacing Validation
- Are turn counts in the 6-10 range for good pacing?
- Are hazard rooms (Ammunition Forge) significantly harder?
- Does the talk mechanic reduce Observation Deck difficulty as intended?

### 4. Equipment Impact Validation
- Do players with Clan-Forged equipment (East Wing) survive better?
- Does the West Wing suffer from insufficient Legend/loot?
- Are puzzle rewards (Optimized gear) making a measurable difference?

## Recommended Balance Adjustments

Based on test results, consider these adjustments from BALANCE_REVIEW_V04.md:

### High Priority (if tests confirm predictions)

**1. Address Legend Disparity**
- Add +50 Legend to West Wing encounters or puzzle rewards
- This should bring West Wing to 350 total (vs East Wing 400)
- Rerun tests to confirm impact

**2. Reduce War-Frame Difficulty (if win rate < 50%)**
- Reduce HP from 50 to 45
- Should improve win rate by ~10-15%
- Rerun East Wing simulation to confirm

**3. Improve Puzzle Rewards**
- Consider swapping: West Wing gets Clan-Forged, East Wing gets Optimized
- This compensates for combat load difference
- Rerun both path simulations to confirm balance

### Medium Priority (if tests show specific issues)

**4. Adjust Environmental Hazards**
- If Ammunition Forge causes death spike, reduce hazard damage (6 → 4)
- If puzzle success rate is too low, reduce DC (3 → 2)

**5. Rebalance Specific Enemies**
- Use individual combat simulation win rates to fine-tune
- Target 60-75% win rate for standard enemies
- Target 50-65% win rate for elite enemies

## Regression Testing

Run the full test suite before and after any balance changes:

```bash
# Before changes
dotnet test --filter "FullyQualifiedName~V04" > results_before.txt

# Make balance adjustments
# (Edit enemy stats, equipment stats, etc.)

# After changes
dotnet test --filter "FullyQualifiedName~V04" > results_after.txt

# Compare results
diff results_before.txt results_after.txt
```

## Data Collection for Manual Playtesting

Use test results to inform PLAYTEST_GUIDE_V04.md:

1. **Identify high-risk rooms** from death distribution
   - Instruct playtesters to pay special attention to these rooms
   - Collect detailed feedback on why deaths occurred

2. **Set difficulty expectations** from win rates
   - If simulation shows 60% win rate, inform playtesters this is expected
   - Helps distinguish "challenging but fair" from "unfair"

3. **Establish completion time baselines** from turn counts
   - Use average turns to estimate playtime
   - Compare manual playtest times to simulation predictions

4. **Track objective vs subjective difficulty**
   - Simulations show objective difficulty (math)
   - Manual playtests capture subjective difficulty (feel)
   - Discrepancies indicate pacing/feedback issues, not balance issues

## Continuous Testing

As you iterate on balance:

1. Run tests after each balance change
2. Document results in a spreadsheet or log file
3. Track trends over iterations
4. Use objective data to validate subjective playtest feedback

## Test Suite Maintenance

### Adding New Combat Simulations
```csharp
[Fact]
public void V04_NewClass_VsNewEnemy_SimulateCombat()
{
    var iterations = 100;
    var results = new CombatSimulationResult("NewClass vs NewEnemy", iterations);

    for (int i = 0; i < iterations; i++)
    {
        var player = CharacterFactory.CreateCharacter(CharacterClass.NewClass, "Test");
        player.MaxHP = 40;
        player.HP = 40;

        var enemy = EnemyFactory.CreateEnemy(EnemyType.NewEnemy);
        var combatResult = SimulateCombat(player, new List<Enemy> { enemy });

        results.RecordResult(combatResult);
    }

    _output.WriteLine($"=== NEWCLASS VS NEWENEMY (100 simulations) ===");
    OutputCombatResults(results);

    Assert.True(results.WinRate > 0.5, "Should have >50% win rate");
}
```

### Adding New Path Simulations
```csharp
[Fact]
public void V04_NewPath_FullPathSimulation()
{
    var iterations = 50;
    var pathResults = new PathSimulationResult("New Path", iterations);

    for (int i = 0; i < iterations; i++)
    {
        var player = CharacterFactory.CreateCharacter(CharacterClass.Warrior, "Test");
        player.MaxHP = 30;
        player.HP = 30;

        // Simulate each room in sequence
        var room1Result = SimulateCombat(player, GetRoomEnemies(1));
        pathResults.AddEncounter(room1Result);
        if (!room1Result.PlayerWon) { pathResults.RecordDeath(1); continue; }

        // Add more rooms...
    }

    OutputPathResults(pathResults);
    Assert.True(pathResults.CompletionRate > 0.3, "Should have >30% completion rate");
}
```

## Troubleshooting

### Tests Fail to Build
- Verify all dependencies are installed: `dotnet restore`
- Check .NET SDK version: `dotnet --version` (need 8.0+)
- Verify project references are correct

### Tests Run but Results Seem Wrong
- Check random seed: Combat uses RNG, results will vary slightly
- Increase iteration count for more stable results
- Verify enemy stats match design docs (BALANCE_REVIEW_V04.md)
- Check equipment is being applied correctly

### Tests Take Too Long
- Reduce iteration count (100 → 50 for combat, 50 → 25 for paths)
- Run specific tests instead of full suite
- Use Release build instead of Debug: `dotnet test -c Release`

### Test Output Not Showing
- Ensure `--logger "console;verbosity=detailed"` is specified
- Check that `ITestOutputHelper` is injected correctly
- Verify `_output.WriteLine()` calls are present

## Summary

The v0.4 unit test suite provides:
- **Objective balance validation** through statistical simulation
- **Regression testing** to catch unintended balance changes
- **Data-driven decision making** for balance adjustments
- **Baseline expectations** for manual playtesting

Use these tests in conjunction with BALANCE_REVIEW_V04.md (mathematical analysis) and PLAYTEST_GUIDE_V04.md (subjective feedback) to achieve well-balanced gameplay.

**Recommended workflow:**
1. Run unit tests to get statistical baseline
2. Compare results to BALANCE_REVIEW_V04.md predictions
3. Conduct manual playtests using PLAYTEST_GUIDE_V04.md
4. Identify discrepancies between objective and subjective difficulty
5. Make targeted balance adjustments
6. Rerun unit tests to validate changes
7. Repeat until balance goals are met
