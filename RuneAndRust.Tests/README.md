# Rune & Rust - Unit Tests

Comprehensive unit testing suite for the Aethelgard Saga Systems (v0.2.1).

## Test Coverage

### 1. **SagaServiceTests.cs** (36 tests)
Tests for the core Legend/Milestone/PP system:
- ✅ Legend award calculations with BLV × DM × TM formula
- ✅ Trauma modifier application (normal, puzzle, boss, blight)
- ✅ Milestone progression and thresholds
- ✅ Milestone rewards (PP, HP, Stamina, full heal)
- ✅ PP spending on attributes with cap validation
- ✅ Edge cases (max milestone, insufficient PP, invalid attributes)

### 2. **AbilityRankTests.cs** (14 tests)
Tests for the Ability Rank progression system:
- ✅ Rank advancement from 1 → 2 with PP costs
- ✅ Rank 2 improvements for all ability types:
  - Power Strike (dice, threshold, cost)
  - Shield Wall (defense %, duration)
  - Quick Dodge (dice, cost reduction)
  - Aetheric Bolt (dice, damage)
- ✅ Rank 3 locking (v0.5+ Capstones)
- ✅ Multiple ability tracking

### 3. **CombatLegendTests.cs** (10 tests)
Tests for combat Legend awards with trauma modifiers:
- ✅ Single enemy defeats (normal, blight, boss)
- ✅ Multiple enemy cumulative Legend
- ✅ Trauma modifier integration (1.0x normal, 1.25x taxing)
- ✅ Milestone reaching detection
- ✅ Only defeated enemies count toward Legend
- ✅ Full progression scenario through multiple fights

### 4. **SaveLoadTests.cs** (16 tests)
Tests for persistence with new Aethelgard schema:
- ✅ Save game creation and updates
- ✅ Load game data retrieval
- ✅ Progression data persistence (Milestone, Legend, PP)
- ✅ Attribute data persistence
- ✅ Resource data persistence (HP, Stamina, AP)
- ✅ World state persistence (rooms, puzzle, boss)
- ✅ Save overwriting and deletion
- ✅ Save listing and info display

### 5. **ProgressionIntegrationTests.cs** (7 tests)
End-to-end integration tests:
- ✅ Full Milestone 0 → 1 cycle with PP spending
- ✅ Attribute advancement to max cap (6)
- ✅ Milestone 0 → 3 progression with all thresholds
- ✅ Realistic v0.1 gameplay scenario:
  - Starting with 2 PP
  - Fighting through 4 rooms + boss
  - Earning Legend with trauma modifiers
  - Reaching Milestone 2
  - Balanced attribute/ability builds

## Running Tests

### Using dotnet CLI:
```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "FullyQualifiedName~SagaServiceTests"

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"

# Generate code coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Using Visual Studio:
1. Open Test Explorer (Test → Test Explorer)
2. Click "Run All" or select specific tests
3. View results in Test Explorer window

### Using Rider:
1. Right-click on test project → "Run Unit Tests"
2. Or use Ctrl+T, R to run tests
3. View results in Unit Tests window

## Test Organization

Tests are organized by system/feature:
- **Unit Tests**: Test individual components in isolation
- **Integration Tests**: Test system interactions and full workflows
- **Edge Cases**: Validate boundary conditions and error handling

## Key Test Scenarios

### Legend Formula Validation
```csharp
// BLV × DM × TM
10 × 1.0 × 1.0  = 10 Legend  (normal combat)
25 × 1.0 × 1.25 = 31 Legend  (blight area)
100 × 1.0 × 1.25 = 125 Legend (boss fight)
```

### Milestone Thresholds
```csharp
Milestone 0 → 1: 100 Legend
Milestone 1 → 2: 150 Legend
Milestone 2 → 3: 200 Legend
```

### PP Costs
```csharp
Attribute increase: 1 PP (max 6)
Ability Rank 1 → 2: 5 PP
Ability Rank 2 → 3: Locked (v0.5+)
```

## Success Criteria

All tests passing indicates:
- ✅ Legend formula correctly implemented
- ✅ Milestone progression working as designed
- ✅ PP system functioning (attributes + abilities)
- ✅ Trauma modifiers applied correctly
- ✅ Save/Load with new schema working
- ✅ Edge cases handled gracefully
- ✅ Full v0.1 playthrough validated

## Continuous Integration

These tests should be run:
- ✅ Before committing code
- ✅ In CI/CD pipeline
- ✅ Before releasing new versions
- ✅ After any system changes

## Future Test Additions

As new systems are added, tests should cover:
- **v0.3**: Equipment system, loot tables
- **v0.4**: Dynamic scaling (TDR/PPS), Trauma Economy
- **v0.5+**: Specialization trees, Rank 3 Capstones
