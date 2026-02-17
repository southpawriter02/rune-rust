# Testing & QA Documentation

This directory contains **test specifications, coverage reports, and QA procedures** for Rune & Rust. Use this to understand what is tested, how to test, and what gaps remain.

---

## Purpose

The Testing & QA Documentation provides:

- **Test coverage reports** - What is tested and what isn't
- **Test specifications** - How each system is tested
- **QA checklists** - Manual testing procedures
- **Bug templates** - How to report and track issues

---

## Documentation Index

### 📊 Coverage Reports

### [Test Coverage Report](https://www.notion.so/test-coverage-report.md)

Overall test coverage across all systems.

**Contents:**

- Coverage percentage by project
- Coverage percentage by system
- Uncovered code sections
- Gap analysis
- Coverage trends over versions

**Target Coverage:** 80% overall

**Current Status:**

```
RuneAndRust.Core:       XX%
RuneAndRust.Engine:     XX%
RuneAndRust.Persistence: XX%
Overall:                XX%

```

---

### [Coverage Gaps](https://www.notion.so/coverage-gaps.md)

Detailed analysis of untested code.

**Contents:**

- High-priority gaps (critical paths)
- Medium-priority gaps (important features)
- Low-priority gaps (edge cases)
- Planned tests to fill gaps

---

### 🧪 Test Specifications

### [Combat Tests](https://www.notion.so/combat-tests.md)

Test specifications for combat systems.

**Test Suites:**

- `CombatEngineTests` - Turn resolution, initiative
- `DamageServiceTests` - Damage calculation, Soak
- `AccuracyServiceTests` - Hit/miss, criticals
- `StatusEffectServiceTests` - Status application/removal
- `EnemyAITests` - AI decision-making

**Total Tests:** XX
**Coverage:** XX%

---

### [Progression Tests](https://www.notion.so/progression-tests.md)

Test specifications for progression systems.

**Test Suites:**

- `ProgressionServiceTests` - XP, leveling, PP
- `AttributeServiceTests` - Attribute increases
- `AbilityServiceTests` - Ability unlocks
- `SpecializationServiceTests` - Specialization selection

**Total Tests:** XX
**Coverage:** XX%

---

### [Equipment Tests](https://www.notion.so/equipment-tests.md)

Test specifications for equipment systems.

**Test Suites:**

- `EquipmentServiceTests` - Equip/unequip logic
- `EquipmentDatabaseTests` - Item definitions
- `LootServiceTests` - Loot generation

**Total Tests:** XX
**Coverage:** XX%

---

### [Procedural Generation Tests](https://www.notion.so/procedural-generation-tests.md)

Test specifications for generation systems.

**Test Suites:**

- `WaveFunctionCollapseTests` - Graph generation
- `SectorGeneratorTests` - Sector creation
- `RoomPopulatorTests` - Enemy/loot spawning
- `BiomeLibraryTests` - Biome definitions

**Total Tests:** XX
**Coverage:** XX%

---

### [Persistence Tests](https://www.notion.so/persistence-tests.md)

Test specifications for save/load systems.

**Test Suites:**

- `SaveRepositoryTests` - Save/load operations
- `WorldStateRepositoryTests` - State change persistence
- `StateSerializerTests` - Serialization/deserialization

**Total Tests:** XX
**Coverage:** XX%

---

### [Quest & Dialogue Tests](https://www.notion.so/quest-dialogue-tests.md)

Test specifications for narrative systems.

**Test Suites:**

- `QuestServiceTests` - Quest generation, tracking
- `DialogueServiceTests` - Dialogue trees, choices
- `SkillCheckTests` - Attribute checks

**Total Tests:** XX
**Coverage:** XX%

---

### ✅ QA Checklists

### [Combat System QA](https://www.notion.so/qa-combat-system.md)

Manual testing procedures for combat.

**Checklist (30+ checks):**

- [ ]  Turn order respects initiative
- [ ]  Damage calculation matches formulas
- [ ]  Status effects apply correctly
- [ ]  Enemy AI makes valid decisions
- [ ]  Combat log displays all actions
- [Full checklist in document]

---

### [Progression System QA](https://www.notion.so/qa-progression-system.md)

Manual testing procedures for progression.

**Checklist (20+ checks):**

- [ ]  XP gain matches enemy values
- [ ]  Level-up rewards applied correctly
- [ ]  PP spending enforces costs
- [ ]  Attribute increases capped at 10
- [Full checklist in document]

---

### [Equipment System QA](https://www.notion.so/qa-equipment-system.md)

Manual testing procedures for equipment.

**Checklist (15+ checks):**

- [ ]  Equipment stats apply when equipped
- [ ]  Loot generation respects class
- [ ]  Quality tiers modify stats correctly
- [ ]  Inventory limits enforced
- [Full checklist in document]

---

### [Procedural Generation QA](https://www.notion.so/qa-procedural-generation.md)

Manual testing procedures for generation.

**Checklist (25+ checks):**

- [ ]  Sectors are always solvable
- [ ]  No unreachable rooms
- [ ]  Enemy spawns respect difficulty
- [ ]  Quest anchors placed correctly
- [Full checklist in document]

---

### [Save/Load System QA](https://www.notion.so/qa-save-load-system.md)

Manual testing procedures for persistence.

**Checklist (20+ checks):**

- [ ]  All character data persists
- [ ]  Equipment survives save/load
- [ ]  World state changes persist
- [ ]  No data corruption
- [Full checklist in document]

---

### 🐛 Bug Tracking

### [Bug Report Template](https://www.notion.so/bug-report-template.md)

Standard format for reporting bugs.

**Template Sections:**

- Bug title and description
- Steps to reproduce
- Expected behavior
- Actual behavior
- System information
- Severity and priority

---

### [Regression Test Template](https://www.notion.so/regression-test-template.md)

How to create regression tests for fixed bugs.

**Template:**

```csharp
[TestMethod]
public void Regression_Bug[Number]_[Description]()
{
    // Arrange: Setup that triggers bug
    // Act: Perform action
    // Assert: Verify bug is fixed
}

```

---

### [Known Issues Log](https://www.notion.so/known-issues-log.md)

Active bugs and their status.

**Format:**

| Bug ID | Description | Severity | Status | Assignee |
| --- | --- | --- | --- | --- |
| #001 | [Issue] | High | Open | [Name] |

---

### 🎯 Test Data

### [Test Fixtures](https://www.notion.so/test-fixtures.md)

Reusable test data and builders.

**Fixtures:**

- Character fixtures (various builds)
- Enemy fixtures (all types)
- Equipment fixtures (all tiers)
- Room fixtures (common layouts)

---

### [Test Scenarios](https://www.notion.so/test-scenarios.md)

Common test scenarios across systems.

**Scenarios:**

- Full playthrough (vertical slice)
- Edge case scenarios (boundary conditions)
- Stress test scenarios (performance)
- Integration scenarios (system interactions)

---

## Testing Standards

### Unit Test Standards

```csharp
[TestClass]
public class ServiceNameTests
{
    [TestMethod]
    public void MethodName_Scenario_ExpectedBehavior()
    {
        // Arrange
        var sut = new ServiceName();
        var input = [test value];

        // Act
        var result = sut.MethodName(input);

        // Assert
        Assert.AreEqual(expected, result);
    }
}

```

### Test Naming Convention

```
[MethodName]_[Scenario]_[ExpectedBehavior]

```

**Examples:**

- `CalculateDamage_WithSoak_ReducesDamage()`
- `ApplyStatus_WhenImmune_DoesNotApply()`
- `GainXP_WhenLevelUp_GrantsRewards()`

---

## Running Tests

### All Tests

```bash
dotnet test

```

### Specific Test Suite

```bash
dotnet test --filter "FullyQualifiedName~CombatEngineTests"

```

### Single Test

```bash
dotnet test --filter "FullyQualifiedName~CombatEngineTests.CalculateDamage_WithSoak_ReducesDamage"

```

### With Coverage

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=html

```

---

## Continuous Integration

### CI Pipeline

1. Build solution
2. Run all unit tests
3. Generate coverage report
4. Fail if coverage < 80%
5. Fail if any test fails

### Pre-Commit Hooks

- Run affected tests
- Run linting
- Verify code compiles

---

## Test-Driven Development

### TDD Workflow

1. **Red:** Write failing test for new feature
2. **Green:** Write minimal code to pass test
3. **Refactor:** Clean up code, keep tests passing

### Example TDD Cycle

```csharp
// 1. RED - Write test first
[TestMethod]
public void NewFeature_Scenario_ExpectedBehavior()
{
    var result = service.NewFeature(input);
    Assert.AreEqual(expected, result);
}

// Test fails because NewFeature() doesn't exist

// 2. GREEN - Implement feature
public int NewFeature(int input)
{
    return input * 2; // Simplest implementation
}

// Test passes

// 3. REFACTOR - Improve implementation
public int NewFeature(int input)
{
    ValidateInput(input);
    return CalculateResult(input);
}
// Tests still pass

```

---

## Documentation for Testers

### Manual Testing Guide

1. Review QA checklist for system
2. Follow step-by-step procedures
3. Document actual results
4. File bugs for failures
5. Update checklist if steps wrong

### Exploratory Testing

- Try unusual inputs
- Combine systems in unexpected ways
- Look for edge cases
- Test performance under load

### Regression Testing

- After bug fix, verify fix works
- Run related tests
- Check for side effects in related systems

---

## Progress Tracking

**Test Documentation Status:**

- Coverage Reports: 0 / 2 complete
- Test Specifications: 0 / 6 complete
- QA Checklists: 0 / 5 complete
- Bug Templates: 0 / 3 complete

**Overall Progress:** 0%

**Test Count:** XX total tests
**Coverage:** XX%

---

**Last Updated:** 2025-11-12
**Documentation Version:** v0.17