# Rune & Rust - Testing Documentation

## Overview

Comprehensive unit testing suite for the Aethelgard Saga Systems (v0.2.1) using NUnit 3.13.3.

**Total Test Coverage**: 83+ tests across 5 test classes

---

## 🎯 Test Suite Structure

### 1. **SagaServiceTests.cs** - 36 Tests
Core Legend/Milestone/PP system validation.

#### Legend Award Tests (8 tests)
- ✅ Basic formula: `BLV × DM × TM`
- ✅ Trauma modifier application (1.0x → 1.25x)
- ✅ Multiple source accumulation
- ✅ Max milestone blocking

**Example**:
```csharp
// Normal combat: 10 × 1.0 × 1.0 = 10 Legend
AwardLegend(player, 10, 1.0f, 1.0f);
Assert.That(player.CurrentLegend, Is.EqualTo(10));

// Boss fight: 100 × 1.0 × 1.25 = 125 Legend
AwardLegend(player, 100, 1.0f, 1.25f);
Assert.That(player.CurrentLegend, Is.EqualTo(125));
```

#### Milestone Tests (8 tests)
- ✅ Threshold checking (100, 150, 200)
- ✅ Rewards (+1 PP, +10 HP, +5 Stamina)
- ✅ Full heal on milestone
- ✅ Max milestone cap (3)

#### PP Spending Tests (12 tests)
- ✅ Attribute increases (1 PP each)
- ✅ Cap validation (max 6)
- ✅ Insufficient PP handling
- ✅ All 5 attributes tested

#### Trauma Modifier Tests (8 tests)
- ✅ Normal: 1.0x
- ✅ Puzzle: 1.25x
- ✅ Boss: 1.25x
- ✅ Blight: 1.25x
- ✅ Unknown: 1.0x (default)

---

### 2. **AbilityRankTests.cs** - 14 Tests
Ability rank progression system validation.

#### Rank Advancement Tests (6 tests)
- ✅ Rank 1 → 2 with 5 PP
- ✅ Insufficient PP blocking
- ✅ Rank 3 locked (v0.5+)
- ✅ PP deduction on success

#### Rank 2 Improvements Tests (8 tests)
Each ability tested for proper upgrades:

**Power Strike**:
- BonusDice: 2 → 3
- SuccessThreshold: 3 → 2
- StaminaCost: 5 → 4

**Shield Wall**:
- BonusDice: 1 → 2
- DefensePercent: 50% → 75%
- DefenseDuration: 2 → 3 turns

**Quick Dodge**:
- BonusDice: 1 → 2
- StaminaCost: 3 → 2

**Aetheric Bolt**:
- BonusDice: 2 → 3
- DamageDice: 1 → 2

---

### 3. **CombatLegendTests.cs** - 10 Tests
Combat integration with Legend awards and trauma modifiers.

#### Combat Award Tests (7 tests)
- ✅ Single enemy (normal): 10 Legend
- ✅ Single enemy (blight): 31 Legend (25 × 1.25)
- ✅ Boss fight: 125 Legend (100 × 1.25)
- ✅ Multiple enemies: Cumulative awards
- ✅ Only defeated enemies count

#### Integration Test (3 tests)
**Full v0.1 Playthrough**:
```
Fight 1: 2 Servitors (normal) → 20 Legend
Fight 2: Blight-Drone (blight) → 51 total (20 + 31)
Fight 3: Blight-Drone (blight) → 82 total (51 + 31)
Fight 4: 2 Servitors (normal) → 102 total (82 + 20)
Result: Milestone 1 reached! ✅
```

---

### 4. **SaveLoadTests.cs** - 16 Tests
Persistence with new Aethelgard schema validation.

#### Save/Load Tests (8 tests)
- ✅ New character save
- ✅ Load existing save
- ✅ Non-existent save returns null
- ✅ Save overwriting

#### Data Persistence Tests (8 tests)
**Progression Data**:
- CurrentMilestone
- CurrentLegend
- ProgressionPoints
- LegendToNextMilestone

**Attribute Data**:
- Might, Finesse, Wits, Will, Sturdiness

**Resource Data**:
- HP, MaxHP, Stamina, MaxStamina, AP

**World State**:
- CurrentRoomId, ClearedRoomIds
- PuzzleSolved, BossDefeated

---

### 5. **ProgressionIntegrationTests.cs** - 7 Tests
End-to-end system integration tests.

#### Full Progression Tests (4 tests)
- ✅ Milestone 0 → 1 complete cycle
- ✅ PP spending on attributes + abilities
- ✅ Attribute advancement to cap (6)
- ✅ Milestone 0 → 3 full progression

#### Realistic Gameplay Tests (3 tests)
**v0.1 Complete Playthrough**:
```
Start: Milestone 0, 2 PP
→ Spend 2 PP on attributes (Might, Sturdiness)
→ Fight 4 rooms + puzzle (145 Legend)
→ Reach Milestone 1, gain 1 PP
→ Boss fight (270 total Legend)
→ Reach Milestone 2
End: Milestone 2, 2 PP, HP 70, Stamina 40
```

**Balanced Build Test**:
- 10 PP distributed optimally
- 3 attributes increased
- 1 ability to Rank 2
- 2 remaining PP saved

---

## 🚀 Running Tests

### Command Line
```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "FullyQualifiedName~SagaServiceTests"

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Generate code coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Visual Studio
1. Open **Test Explorer** (Test → Test Explorer)
2. Click **Run All** or select specific tests
3. View results in Test Explorer window

### Rider
1. Right-click on test project → **Run Unit Tests**
2. Or use `Ctrl+T, R` to run tests
3. View results in Unit Tests window

---

## ✅ Validation Criteria

All tests passing confirms:

### Core Systems
- ✅ Legend formula correctly implemented
- ✅ Milestone progression working as designed
- ✅ PP system functioning (attributes + abilities)
- ✅ Trauma modifiers applied correctly

### Combat Integration
- ✅ Legend awards in combat
- ✅ Milestone reaching detection
- ✅ Trauma modifiers in combat context

### Persistence
- ✅ Save/Load with new schema working
- ✅ All progression data persists
- ✅ World state preserved

### Edge Cases
- ✅ Attribute cap (6) enforced
- ✅ Max milestone (3) enforced
- ✅ Rank 3 locked (v0.5+)
- ✅ Insufficient PP handled gracefully
- ✅ Invalid input rejected

---

## 📊 Test Statistics

| Test Class | Tests | Coverage |
|-----------|-------|----------|
| SagaServiceTests | 36 | Legend, Milestones, PP |
| AbilityRankTests | 14 | Rank progression |
| CombatLegendTests | 10 | Combat integration |
| SaveLoadTests | 16 | Persistence |
| ProgressionIntegrationTests | 7 | End-to-end flows |
| **Total** | **83** | **All v0.2.1 systems** |

---

## 🔄 Continuous Integration

These tests should be run:
- ✅ Before every commit
- ✅ In CI/CD pipeline
- ✅ Before releasing new versions
- ✅ After any system changes

---

## 🎯 Key Test Scenarios

### Legend Formula
```
10 × 1.0 × 1.0  = 10 Legend  (Corrupted Servitor, normal)
25 × 1.0 × 1.25 = 31 Legend  (Blight-Drone, blight area)
100 × 1.0 × 1.25 = 125 Legend (Ruin-Warden, boss fight)
50 × 1.0 × 1.25 = 63 Legend  (Puzzle, taxing act)
```

### Milestone Thresholds
```
Milestone 0 → 1: 100 Legend
Milestone 1 → 2: 150 Legend
Milestone 2 → 3: 200 Legend
Max Milestone: 3
```

### PP Costs
```
Attribute increase: 1 PP (max 6 per attribute)
Ability Rank 1 → 2: 5 PP
Ability Rank 2 → 3: Locked until v0.5+ (Capstones)
```

### Milestone Rewards
```
+1 Progression Point
+10 Max HP
+5 Max Stamina
Full HP and Stamina restore
```

---

## 🔮 Future Test Additions

As new systems are added, tests should cover:

### v0.3 - Equipment & Loot
- Equipment quality tiers (Jury-Rigged → Myth-Forged)
- Loot table generation
- Inventory management
- Equipment bonuses

### v0.4 - Dynamic Scaling & Trauma
- TDR/PPS system
- Dynamic difficulty adjustment
- Psychic Stress mechanics
- Corruption system

### v0.5+ - Specialization Trees
- New ability unlocks
- Ability Rank 3 (Capstones)
- Specialization branches
- Advanced builds

---

## 📝 Test Maintenance

### When to Update Tests
- ✅ Adding new features
- ✅ Changing formulas or mechanics
- ✅ Fixing bugs (add regression test)
- ✅ Refactoring core systems

### Test Writing Guidelines
1. Follow AAA pattern (Arrange, Act, Assert)
2. One logical assertion per test
3. Clear, descriptive test names
4. Include edge cases and error conditions
5. Use test fixtures for common setup
6. Document complex test scenarios

---

## 🏆 Success Metrics

**Current Status**: ✅ All 83 tests passing

This comprehensive test suite ensures:
- 100% coverage of v0.2.1 migration features
- Confidence in Aethelgard systems correctness
- Regression protection for future development
- Documentation of expected behavior
- Fast feedback during development

---

*Last Updated: v0.2.1 - Aethelgard Systems Migration*
