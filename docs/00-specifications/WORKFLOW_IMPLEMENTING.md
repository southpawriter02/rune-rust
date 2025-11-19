# Workflow: Implementing a Specification

**Purpose**: Step-by-step workflow for implementing an approved specification in code.

**Target Audience**: AI assistants and human developers implementing features from specifications

**Last Updated**: 2025-11-19

**Prerequisites**: Approved specification exists (status: "Approved" or "Active")

---

## Table of Contents

1. [When to Use This Workflow](#when-to-use-this-workflow)
2. [Implementation Workflow Overview](#implementation-workflow-overview)
3. [Step 1: Preparation & Setup](#step-1-preparation--setup)
4. [Step 2: Create Data Models](#step-2-create-data-models)
5. [Step 3: Implement Services](#step-3-implement-services)
6. [Step 4: Implement Factories & Databases](#step-4-implement-factories--databases)
7. [Step 5: Wire Up Dependencies](#step-5-wire-up-dependencies)
8. [Step 6: Implement Integration Points](#step-6-implement-integration-points)
9. [Step 7: Write Unit Tests](#step-7-write-unit-tests)
10. [Step 8: Write Integration Tests](#step-8-write-integration-tests)
11. [Step 9: Balance Testing](#step-9-balance-testing)
12. [Step 10: Code Review & Finalization](#step-10-code-review--finalization)
13. [Implementation Checklist](#implementation-checklist)

---

## When to Use This Workflow

Use this workflow when:
- ✅ Specification exists and is approved (status: "Approved" or "Active")
- ✅ Ready to write code to implement specification
- ✅ Functional requirements are clear and complete

**Do NOT use this workflow if**:
- ❌ Specification doesn't exist (use `WORKFLOW_PLANNING.md` and `WORKFLOW_DRAFTING.md` first)
- ❌ Specification is still in "Draft" or "Review" status (get approval first)
- ❌ Requirements are ambiguous (clarify spec before coding)

---

## Implementation Workflow Overview

**Estimated Time**: Varies by complexity (simple system: 4-8 hours, complex system: 16-40 hours)

**Process**:
```
1. Preparation & Setup (30 min)
   ↓
2. Create Data Models (1-2 hours)
   ↓
3. Implement Services (3-8 hours)
   ↓
4. Implement Factories & Databases (1-3 hours)
   ↓
5. Wire Up Dependencies (1 hour)
   ↓
6. Implement Integration Points (2-4 hours)
   ↓
7. Write Unit Tests (2-4 hours)
   ↓
8. Write Integration Tests (2-3 hours)
   ↓
9. Balance Testing (2-4 hours)
   ↓
10. Code Review & Finalization (1 hour)
```

**Goal**: Produce fully functional, tested code that implements all functional requirements.

---

## Step 1: Preparation & Setup

**Time**: 30 minutes

**Objective**: Read specification thoroughly and set up development environment.

### Read the Specification

1. **Complete Read**: Read entire specification start to finish
2. **FR Focus**: Pay special attention to Functional Requirements section (FR-XXX)
3. **Mechanics Focus**: Understand all formulas and mechanics
4. **Integration Focus**: Understand all integration points

### Check Implementation Status

Look at "Implementation Guidance" section:
- [ ] Core models created
- [ ] Service/Engine implemented
- [ ] Factory (if needed) implemented
- [ ] Database/Registry (if needed) populated
- [ ] Integration with dependent systems
- [ ] Unit tests written
- [ ] Integration tests written

**Identify**: What's already done? What needs to be implemented?

### Set Up Git Branch

```bash
# Create feature branch
git checkout -b feature/[system-name]

# Example:
git checkout -b feature/damage-calculation
```

### Gather Resources

Collect all referenced materials:
- [ ] Specification file (SPEC-XXX-YYY)
- [ ] Related specifications (dependencies)
- [ ] Layer 1 implementation docs (if exist)
- [ ] Existing code references (file:line)
- [ ] Setting compliance docs (if applicable)

### Create Implementation Plan

**Task List** (based on FRs):
```
FR-001: [Requirement Name]
  - [ ] Subtask 1
  - [ ] Subtask 2

FR-002: [Requirement Name]
  - [ ] Subtask 1
  - [ ] Subtask 2

[... continue for all FRs ...]
```

### Output

✅ Specification read and understood
✅ Implementation status assessed
✅ Git branch created
✅ Resources gathered
✅ Implementation plan created

---

## Step 2: Create Data Models

**Time**: 1-2 hours

**Objective**: Create POCOs (Plain Old CLR Objects) for all data structures.

### Identify Models from Spec

Look for:
- Data structures mentioned in "Data Requirements" section
- Input/output data in "System Mechanics" section
- State variables in "State Management" section (if exists)

### Create Model Files

**Location**: `RuneAndRust.Core/Models/[Domain]/`

**For Each Model**:

```csharp
namespace RuneAndRust.Core.Models.[Domain]
{
    /// <summary>
    /// [Brief description of what this model represents]
    ///
    /// Specification: SPEC-XXX-YYY ([Spec Name])
    /// </summary>
    public class [ModelName]
    {
        /// <summary>
        /// [Property description]
        /// </summary>
        public [Type] PropertyName { get; set; }

        /// <summary>
        /// [Property description]
        /// </summary>
        public [Type] OtherProperty { get; set; }

        // POCOS only - no business logic methods
    }
}
```

### Model Guidelines (from CODE-RULEs)

- ✅ **POCOs only**: Properties with getters/setters, NO business logic
- ✅ **XML docs**: All public properties documented
- ✅ **Spec reference**: Include `Specification: SPEC-XXX-YYY` in class comment
- ✅ **Canonical terminology**: Use setting-appropriate names
- ❌ **No dependencies**: Models should NOT reference services
- ❌ **No calculations**: Move formulas to services, not models

### Example

```csharp
namespace RuneAndRust.Core.Models.Combat
{
    /// <summary>
    /// Represents the result of a damage calculation including damage amount,
    /// damage type, and whether it was a critical hit.
    ///
    /// Specification: SPEC-COMBAT-002 (Damage Calculation System)
    /// </summary>
    public class DamageResult
    {
        /// <summary>
        /// Final damage amount after all calculations and reductions
        /// </summary>
        public int FinalDamage { get; set; }

        /// <summary>
        /// Type of damage dealt (Physical, Aetheric, Corruption)
        /// </summary>
        public DamageType DamageType { get; set; }

        /// <summary>
        /// Whether this damage was from a critical hit
        /// </summary>
        public bool IsCritical { get; set; }

        /// <summary>
        /// Base damage before armor/resistance application
        /// </summary>
        public int BaseDamage { get; set; }

        /// <summary>
        /// Amount of damage mitigated by armor/resistance
        /// </summary>
        public int DamageMitigated { get; set; }
    }
}
```

### Actions

- [ ] Identify all models from specification
- [ ] Create model files in `RuneAndRust.Core/Models/[Domain]/`
- [ ] Add XML documentation for all properties
- [ ] Reference specification in class comment
- [ ] Ensure POCOs only (no business logic)
- [ ] Use canonical terminology

**Output**: Data models created

---

## Step 3: Implement Services

**Time**: 3-8 hours (THE CORE OF IMPLEMENTATION)

**Objective**: Implement business logic for all functional requirements.

### Identify Services from Spec

Look at "Implementation Guidance" section for recommended architecture:
- Primary service name
- Namespace
- Dependencies

### Create Service File

**Location**: `RuneAndRust.Engine/Services/[Domain]/`

**Service Template**:

```csharp
using RuneAndRust.Core.Models.[Domain];
using System;

namespace RuneAndRust.Engine.Services.[Domain]
{
    /// <summary>
    /// [Brief description of service responsibility]
    ///
    /// Specification: SPEC-XXX-YYY ([Spec Name])
    /// </summary>
    public class [System]Service
    {
        private readonly [Dependency1] _dependency1;
        private readonly [Dependency2] _dependency2;

        /// <summary>
        /// Initializes a new instance of [System]Service
        /// </summary>
        /// <param name="dependency1">[Description]</param>
        /// <param name="dependency2">[Description]</param>
        public [System]Service([Dependency1] dependency1, [Dependency2] dependency2)
        {
            _dependency1 = dependency1 ?? throw new ArgumentNullException(nameof(dependency1));
            _dependency2 = dependency2 ?? throw new ArgumentNullException(nameof(dependency2));
        }

        // Public methods implementing FRs go here
    }
}
```

### Implement Each FR as Method(s)

**For Each Functional Requirement** (FR-XXX):

1. **Create Method**: Public method implementing FR logic
2. **Parameters**: Match "Input Data" from spec
3. **Return Type**: Match "Output Data" from spec
4. **Implementation**: Follow formulas/mechanics from spec
5. **Validation**: Check acceptance criteria
6. **Error Handling**: Handle edge cases from spec

**Method Template**:

```csharp
/// <summary>
/// [Description matching FR description]
///
/// Implements: FR-XXX ([FR Name])
/// </summary>
/// <param name="param1">[Description matching spec inputs]</param>
/// <param name="param2">[Description]</param>
/// <returns>[Description matching spec outputs]</returns>
/// <exception cref="ArgumentNullException">If required param is null</exception>
/// <exception cref="ArgumentException">If param validation fails</exception>
public [ReturnType] MethodName([Type] param1, [Type] param2)
{
    // 1. Validation
    if (param1 == null) throw new ArgumentNullException(nameof(param1));
    if (param2 < 0) throw new ArgumentException("param2 must be >= 0", nameof(param2));

    // 2. Business logic (follow spec formulas)
    var intermediateResult = CalculateStep1(param1);
    var finalResult = CalculateStep2(intermediateResult, param2);

    // 3. Return
    return finalResult;
}
```

### Follow Spec Formulas Exactly

**Copy formulas from spec** and implement in code:

Spec formula:
```
BaseDamage = Weapon.BaseDamage + (SuccessCount × AttributeModifier)
FinalDamage = max(0, BaseDamage - TargetArmor)
```

Code implementation:
```csharp
/// <summary>
/// Calculates Physical damage using weapon base damage, dice successes,
/// and target armor.
///
/// Implements: FR-001 (Physical Damage Calculation)
/// Formula: BaseDamage = Weapon.BaseDamage + (SuccessCount × AttributeModifier)
///          FinalDamage = max(0, BaseDamage - TargetArmor)
/// </summary>
public DamageResult CalculatePhysicalDamage(Weapon weapon, int successCount, int attributeModifier, int targetArmor)
{
    // Validation
    if (weapon == null) throw new ArgumentNullException(nameof(weapon));
    if (successCount < 0) throw new ArgumentException("Success count cannot be negative", nameof(successCount));

    // Calculate base damage
    int baseDamage = weapon.BaseDamage + (successCount * attributeModifier);

    // Apply armor
    int finalDamage = Math.Max(0, baseDamage - targetArmor);

    // Return result
    return new DamageResult
    {
        BaseDamage = baseDamage,
        FinalDamage = finalDamage,
        DamageType = DamageType.Physical,
        IsCritical = false,
        DamageMitigated = baseDamage - finalDamage
    };
}
```

### Use Const/Config for Parameters

**Extract magic numbers to constants** (CODE-RULE-004):

Spec parameter:
```
CritMultiplier: float = 1.5 (Range: 1.0-3.0)
```

Code implementation:
```csharp
/// <summary>
/// Critical hit damage multiplier
/// </summary>
private const float CRIT_MULTIPLIER = 1.5f;

// Usage:
int critDamage = (int)(baseDamage * CRIT_MULTIPLIER);
```

### Handle Edge Cases from Spec

**For each edge case listed in spec**, handle explicitly:

Spec edge case:
```
Edge Case: High armor fully negates low-damage attack
  Condition: BaseDamage < TargetArmor
  Behavior: FinalDamage = 0 (no damage dealt)
```

Code implementation:
```csharp
// Apply armor (handles edge case: armor fully negates damage)
int finalDamage = Math.Max(0, baseDamage - targetArmor);

// If fully negated, could trigger feedback event
if (finalDamage == 0)
{
    OnAttackDeflected?.Invoke(this, new AttackDeflectedEventArgs { Target = target });
}
```

### Service Implementation Checklist

For each FR:
- [ ] Public method created
- [ ] XML documentation with FR reference
- [ ] Input validation (null checks, range checks)
- [ ] Formula from spec implemented exactly
- [ ] Constants used instead of magic numbers
- [ ] Edge cases from spec handled
- [ ] Error handling for failure modes
- [ ] Events published (if specified in integration points)

### Actions

- [ ] Create service class with dependency injection
- [ ] Implement method for each FR (5-12 methods typically)
- [ ] Add XML documentation for all public methods
- [ ] Reference spec ID in class comment
- [ ] Extract magic numbers to constants
- [ ] Handle edge cases from spec
- [ ] Add error handling

**Output**: Service(s) implemented with all FR logic

---

## Step 4: Implement Factories & Databases

**Time**: 1-3 hours

**Objective**: Create factory classes (for object creation) and database classes (for static data).

### Check Spec for Factory Needs

Factories are needed if:
- Complex object initialization
- Multiple creation patterns
- Spec mentions "Factory" in Implementation Guidance

### Factory Template

```csharp
namespace RuneAndRust.Engine.Factories
{
    /// <summary>
    /// Factory for creating [Object] instances
    ///
    /// Specification: SPEC-XXX-YYY ([Spec Name])
    /// </summary>
    public class [Object]Factory
    {
        private readonly [Dependency] _dependency;

        public [Object]Factory([Dependency] dependency)
        {
            _dependency = dependency ?? throw new ArgumentNullException(nameof(dependency));
        }

        /// <summary>
        /// Creates a new [Object] with [specific configuration]
        /// </summary>
        public [Object] Create[Variant]([Parameters])
        {
            // Complex creation logic here
            return new [Object] { /* properties */ };
        }
    }
}
```

### Check Spec for Database Needs

Databases are needed if:
- Static data definitions (enemies, abilities, items, etc.)
- Spec mentions "Database" in Implementation Guidance
- Data is hardcoded (not from JSON/SQL)

### Database Template

```csharp
namespace RuneAndRust.Engine.Databases
{
    /// <summary>
    /// Static database of [Objects]
    ///
    /// Specification: SPEC-XXX-YYY ([Spec Name])
    /// </summary>
    public static class [Object]Database
    {
        /// <summary>
        /// [Description of specific object]
        /// </summary>
        public static [Object] ObjectName => new [Object]
        {
            // Static data here
        };

        // Or collection:
        public static List<[Object]> GetAll() => new List<[Object]>
        {
            ObjectName1,
            ObjectName2,
            // ...
        };
    }
}
```

### Actions

- [ ] Create factory (if needed)
- [ ] Create database (if needed)
- [ ] Add XML documentation
- [ ] Reference specification

**Output**: Factory and/or database created (if needed)

---

## Step 5: Wire Up Dependencies

**Time**: 1 hour

**Objective**: Configure dependency injection for services.

### Identify Dependencies

From spec "Integration Points" section:
- What services does this system consume?
- What services consume this system?

### Register Services

**Location**: `RuneAndRust.ConsoleApp/Program.cs` or DI container configuration

```csharp
// Register service with dependencies
services.AddSingleton<[IDependency], [DependencyImplementation]>();
services.AddSingleton<[IYourService], [YourService]>();
```

### Constructor Injection

Ensure service constructor receives all dependencies:

```csharp
public DamageCalculationService(
    IDiceService diceService,
    ICharacterService characterService,
    IEventBus eventBus)
{
    _diceService = diceService ?? throw new ArgumentNullException(nameof(diceService));
    _characterService = characterService ?? throw new ArgumentNullException(nameof(characterService));
    _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
}
```

### Actions

- [ ] Identify all dependencies from spec
- [ ] Register service in DI container
- [ ] Verify dependencies resolve correctly
- [ ] Test service creation (no missing dependencies)

**Output**: Dependencies wired up via DI

---

## Step 6: Implement Integration Points

**Time**: 2-4 hours

**Objective**: Implement all event subscriptions/publications and cross-system integration.

### Events Published

From spec "Integration Points" → "Events Published" table:

**For each event**:
1. Define event args class (if needed)
2. Declare event in service
3. Invoke event at trigger point

**Example**:

```csharp
// Event args
public class DamageDealtEventArgs : EventArgs
{
    public DamageResult DamageResult { get; set; }
    public Character Source { get; set; }
    public Character Target { get; set; }
}

// Event declaration
public event EventHandler<DamageDealtEventArgs> OnDamageDealt;

// Event invocation (at appropriate point in code)
OnDamageDealt?.Invoke(this, new DamageDealtEventArgs
{
    DamageResult = result,
    Source = attacker,
    Target = defender
});
```

### Events Subscribed

From spec "Integration Points" → "Events Subscribed" table:

**For each subscription**:
1. Subscribe in constructor or initialization
2. Implement handler method
3. Handle event data appropriately

**Example**:

```csharp
// Subscribe in constructor
public DamageCalculationService(IEquipmentService equipmentService)
{
    _equipmentService = equipmentService;
    _equipmentService.OnWeaponEquipped += HandleWeaponEquipped;
}

// Handler method
private void HandleWeaponEquipped(object sender, WeaponEquippedEventArgs e)
{
    // Update cached weapon damage values
    RecalculateBaseDamage(e.Character, e.Weapon);
}
```

### Cross-System Integration

**For each system consumed** (from "This System Consumes"):
1. Call methods/properties as specified
2. Handle failures as specified in "Failure Handling"
3. Validate data contracts

### Actions

- [ ] Implement all events published (from spec table)
- [ ] Subscribe to all events consumed (from spec table)
- [ ] Implement cross-system method calls
- [ ] Handle integration failures gracefully
- [ ] Test event flow

**Output**: Integration points implemented

---

## Step 7: Write Unit Tests

**Time**: 2-4 hours

**Objective**: Test all service methods in isolation.

### Test File Setup

**Location**: `RuneAndRust.Tests/[Domain]/[Service]Tests.cs`

```csharp
using Xunit;
using Moq;
using RuneAndRust.Engine.Services.[Domain];
using RuneAndRust.Core.Models.[Domain];

namespace RuneAndRust.Tests.[Domain]
{
    /// <summary>
    /// Unit tests for [Service]
    ///
    /// Specification: SPEC-XXX-YYY ([Spec Name])
    /// </summary>
    public class [Service]Tests
    {
        // Test methods here
    }
}
```

### Test Each FR

**For Each Functional Requirement**:

1. **Happy Path Test**: Test expected input/output
2. **Edge Case Tests**: Test each edge case from spec
3. **Error Tests**: Test validation and error handling

**Test Template**:

```csharp
[Fact]
public void MethodName_HappyPath_ReturnsExpectedResult()
{
    // Arrange
    var mockDependency = new Mock<IDependency>();
    mockDependency.Setup(d => d.Method()).Returns(expectedValue);
    var service = new [Service](mockDependency.Object);

    var input = /* test input */;

    // Act
    var result = service.MethodName(input);

    // Assert
    Assert.Equal(expectedValue, result.Property);
    mockDependency.Verify(d => d.Method(), Times.Once);
}

[Fact]
public void MethodName_EdgeCase_HandlesCorrectly()
{
    // Arrange
    var service = new [Service](/* dependencies */);
    var edgeCaseInput = /* edge case from spec */;

    // Act
    var result = service.MethodName(edgeCaseInput);

    // Assert
    Assert.[Expected behavior from spec]
}

[Fact]
public void MethodName_InvalidInput_ThrowsArgumentException()
{
    // Arrange
    var service = new [Service](/* dependencies */);
    var invalidInput = null;

    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => service.MethodName(invalidInput));
}
```

### Use Spec Examples as Test Cases

**Copy examples from spec** and turn into tests:

Spec example:
```
Example:
  Weapon.BaseDamage = 10
  SuccessCount = 3
  AttributeModifier = 4
  TargetArmor = 5

  BaseDamage = 10 + (3 × 4) = 22
  FinalDamage = 22 - 5 = 17
```

Test implementation:
```csharp
[Fact]
public void CalculatePhysicalDamage_SpecExample_ReturnsExpectedDamage()
{
    // Arrange (from spec example)
    var service = new DamageCalculationService(/* mocks */);
    var weapon = new Weapon { BaseDamage = 10 };
    int successCount = 3;
    int attributeModifier = 4;
    int targetArmor = 5;

    // Act
    var result = service.CalculatePhysicalDamage(weapon, successCount, attributeModifier, targetArmor);

    // Assert (expected results from spec)
    Assert.Equal(22, result.BaseDamage);
    Assert.Equal(17, result.FinalDamage);
    Assert.Equal(DamageType.Physical, result.DamageType);
}
```

### Test Coverage Target

**Goal**: >80% code coverage

```bash
# Run tests with coverage
dotnet test /p:CollectCoverage=true
```

### Actions

- [ ] Create test file
- [ ] Write tests for each FR (happy path, edge cases, errors)
- [ ] Use spec examples as test cases
- [ ] Achieve >80% code coverage
- [ ] All tests pass

**Output**: Unit tests written with >80% coverage

---

## Step 8: Write Integration Tests

**Time**: 2-3 hours

**Objective**: Test system integration with dependencies (no mocks).

### Integration Test Setup

**Location**: `RuneAndRust.Tests/Integration/[System]IntegrationTests.cs`

```csharp
using Xunit;
using RuneAndRust.Engine.Services.[Domain];
using RuneAndRust.Core.Models.[Domain];

namespace RuneAndRust.Tests.Integration
{
    /// <summary>
    /// Integration tests for [System]
    ///
    /// Tests full integration with dependencies (no mocks)
    ///
    /// Specification: SPEC-XXX-YYY ([Spec Name])
    /// </summary>
    public class [System]IntegrationTests
    {
        // Integration test methods
    }
}
```

### Test Full Workflows

**From spec "Acceptance Testing Scenarios"**:

Spec scenario:
```
Scenario 1: Physical Damage Calculation
1. Create Warrior with MIGHT 4
2. Equip longsword (BaseDamage 12)
3. Attack enemy with 8 armor
4. Roll dice (assume 3 successes)
5. Verify:
   - BaseDamage = 12 + (3×4) = 24
   - FinalDamage = 24 - 8 = 16
   - Enemy HP reduced by 16
```

Integration test:
```csharp
[Fact]
public void PhysicalDamage_FullWorkflow_CalculatesCorrectly()
{
    // Arrange: Create real services (no mocks)
    var diceService = new DiceService();
    var characterService = new CharacterService();
    var damageService = new DamageCalculationService(diceService, characterService);

    var warrior = characterService.CreateWarrior();
    warrior.Attributes.MIGHT = 4;
    var longsword = new Weapon { BaseDamage = 12 };
    var enemy = new Enemy { Armor = 8, CurrentHP = 50 };

    // Act: Full workflow
    var diceResult = diceService.RollDice(poolSize: 4); // MIGHT 4 = 4d6
    // Assume we got 3 successes (could mock dice for determinism)
    int successCount = 3;

    var damageResult = damageService.CalculatePhysicalDamage(
        longsword, successCount, warrior.Attributes.MIGHT, enemy.Armor);

    enemy.CurrentHP -= damageResult.FinalDamage;

    // Assert: Verify full workflow
    Assert.Equal(24, damageResult.BaseDamage);
    Assert.Equal(16, damageResult.FinalDamage);
    Assert.Equal(34, enemy.CurrentHP); // 50 - 16 = 34
}
```

### Test Integration Points

Test event flow:
```csharp
[Fact]
public void DamageDealt_PublishesEvent_StatusEffectServiceReceives()
{
    // Arrange
    var eventBus = new EventBus();
    var damageService = new DamageCalculationService(eventBus, /* other deps */);
    var statusEffectService = new StatusEffectService(eventBus);

    bool eventReceived = false;
    statusEffectService.OnDamageDealtReceived += (s, e) => eventReceived = true;

    // Act
    damageService.CalculateDamage(/* params */);

    // Assert
    Assert.True(eventReceived, "StatusEffectService should receive OnDamageDealt event");
}
```

### Actions

- [ ] Create integration test file
- [ ] Implement acceptance test scenarios from spec
- [ ] Test event flow between systems
- [ ] Test full workflows end-to-end
- [ ] All integration tests pass

**Output**: Integration tests written and passing

---

## Step 9: Balance Testing

**Time**: 2-4 hours

**Objective**: Verify balance targets from spec are met.

### Balance Test Setup

From spec "Balance & Tuning" → "Testing Scenarios"

### Run Balance Tests

Spec test:
```
Balance Test 1: Standard Combat Damage Range
Setup:
  - Level 5 Warrior (MIGHT 4)
  - Standard longsword (BaseDamage 12)
  - Enemy with 8 armor

Procedure:
  1. Run 100 attacks
  2. Record damage dealt per attack
  3. Calculate average, min, max

Expected Results:
  - Average: ~15 damage
  - Min: ~8 damage
  - Max: ~28 damage

Pass Criteria: Average within 13-17 damage range
```

Balance test code:
```csharp
[Fact]
public void BalanceTest_StandardCombatDamage_AverageWithinExpectedRange()
{
    // Setup (from spec)
    var damageService = new DamageCalculationService(/* deps */);
    var weapon = new Weapon { BaseDamage = 12 };
    int might = 4;
    int armor = 8;

    var damageResults = new List<int>();

    // Run 100 attacks
    for (int i = 0; i < 100; i++)
    {
        // Simulate dice roll (1-5 successes typically)
        int successes = Random.Next(1, 6);

        var result = damageService.CalculatePhysicalDamage(weapon, successes, might, armor);
        damageResults.Add(result.FinalDamage);
    }

    // Calculate statistics
    double average = damageResults.Average();
    int min = damageResults.Min();
    int max = damageResults.Max();

    // Assert (from spec pass criteria)
    Assert.InRange(average, 13, 17); // Spec: 13-17 range
    Console.WriteLine($"Average: {average:F2}, Min: {min}, Max: {max}");
}
```

### Verify Balance Targets

From spec "Balance & Tuning" → "Balance Targets":

**For each balance target**:
1. Run balance test
2. Compare result to target
3. If outside target range, adjust tunable parameters
4. Re-run test

### Document Balance Results

Create balance test report:
```markdown
# Balance Test Results: SPEC-XXX-YYY

## Test 1: Standard Combat Damage Range
- **Target**: Average 13-17 damage
- **Actual**: Average 15.3 damage
- **Result**: ✅ PASS

## Test 2: Critical Hit Damage Increase
- **Target**: 1.4x - 1.6x damage increase
- **Actual**: 1.5x damage increase
- **Result**: ✅ PASS

[... continue for all balance tests ...]
```

### Actions

- [ ] Implement all balance tests from spec
- [ ] Run tests and record results
- [ ] Compare to balance targets
- [ ] Adjust tunable parameters if needed
- [ ] Document balance test results

**Output**: Balance tests pass and meet targets

---

## Step 10: Code Review & Finalization

**Time**: 1 hour

**Objective**: Self-review code before submitting for review.

### Code Quality Checklist

From `SPECIFICATION_DEVELOPMENT_RULES.md` → "Review Checklist for Code":

- [ ] **CODE-RULE-001**: Implements specification FRs
- [ ] **CODE-RULE-002**: Spec references in code comments
- [ ] **CODE-RULE-003**: Canonical terminology used
- [ ] **CODE-RULE-004**: No magic numbers
- [ ] **CODE-RULE-005**: XML documentation complete
- [ ] **CODE-RULE-006**: SOLID principles followed
- [ ] **CODE-RULE-007**: Data separated from logic
- [ ] **CODE-RULE-008**: No hardcoded data strings
- [ ] **CODE-RULE-009**: Dependency injection used
- [ ] **CODE-RULE-010**: Unit tests >80% coverage
- [ ] **QUALITY-RULE-001**: No compiler warnings
- [ ] **QUALITY-RULE-002**: Code review completed
- [ ] **QUALITY-RULE-003**: Consistent formatting
- [ ] **QUALITY-RULE-004**: Meaningful commit messages
- [ ] **QUALITY-RULE-005**: All tests pass

### Run Full Test Suite

```bash
# Run all tests
dotnet test

# Verify all pass
# Check coverage report
```

### Format Code

```bash
# Run auto-formatter
dotnet format
```

### Update Specification Status

In specification file:
- [ ] Update "Implementation Status" checkboxes
- [ ] Mark FRs as "Implemented" and "Tested"
- [ ] Update specification status to "Active" (if all FRs complete)

### Create Pull Request

```bash
# Commit changes
git add .
git commit -m "feat: Implement SPEC-XXX-YYY ([System Name])

Implements all functional requirements from specification:
- FR-001: [Requirement name]
- FR-002: [Requirement name]
- [... list all FRs ...]

Includes:
- Service implementation with full business logic
- Data models (POCOs)
- Unit tests (>80% coverage)
- Integration tests
- Balance tests (all targets met)

Refs: SPEC-XXX-YYY"

# Push to remote
git push -u origin feature/[system-name]
```

### Submit for Review

Present implementation summary to user:
```markdown
✅ Implementation Complete: SPEC-XXX-YYY ([System Name])

**Implemented**:
- 8 Functional Requirements (all tested)
- 5 System Mechanics with formulas
- Integration with [System A], [System B], [System C]
- 24 unit tests (85% coverage)
- 6 integration tests (all pass)
- 3 balance tests (all targets met)

**Code Statistics**:
- Service: 350 lines
- Models: 120 lines
- Tests: 580 lines
- Total: 1,050 lines

**Status**: Ready for Code Review

**Next Steps**:
- Please review the code
- Run tests locally to verify
- Approve for merge if acceptable
```

### Actions

- [ ] Run code quality checklist (15 items)
- [ ] Run full test suite (all pass)
- [ ] Format code consistently
- [ ] Update specification status
- [ ] Commit with meaningful message
- [ ] Push to remote branch
- [ ] Submit for review

**Output**: Implementation complete and ready for review

---

## Implementation Checklist

Before submitting for review, verify:

### Preparation
- [ ] Specification read completely
- [ ] Implementation plan created
- [ ] Git branch created
- [ ] Resources gathered

### Data Models
- [ ] All models created as POCOs
- [ ] XML documentation complete
- [ ] Specification referenced in comments
- [ ] Canonical terminology used
- [ ] No business logic in models

### Services
- [ ] Service class created with DI
- [ ] Method for each FR implemented
- [ ] Formulas from spec implemented exactly
- [ ] Edge cases handled
- [ ] Constants used (no magic numbers)
- [ ] XML documentation complete
- [ ] Error handling implemented

### Factories & Databases
- [ ] Factory created (if needed)
- [ ] Database created (if needed)
- [ ] XML documentation complete

### Dependencies
- [ ] All dependencies identified
- [ ] Services registered in DI container
- [ ] Dependencies resolve correctly

### Integration
- [ ] All events published
- [ ] All events subscribed
- [ ] Cross-system integration implemented
- [ ] Event flow tested

### Unit Tests
- [ ] Test file created
- [ ] Tests for each FR (happy path + edge cases + errors)
- [ ] Spec examples used as test cases
- [ ] >80% code coverage achieved
- [ ] All tests pass

### Integration Tests
- [ ] Integration test file created
- [ ] Acceptance test scenarios from spec implemented
- [ ] Event flow tested
- [ ] Full workflows tested
- [ ] All tests pass

### Balance Tests
- [ ] All balance tests from spec implemented
- [ ] Balance targets verified
- [ ] Tunable parameters adjusted (if needed)
- [ ] Results documented

### Code Quality
- [ ] All CODE-RULEs checked (10 rules)
- [ ] All QUALITY-RULEs checked (5 rules)
- [ ] No compiler warnings
- [ ] Code formatted consistently
- [ ] All tests pass

### Finalization
- [ ] Specification status updated
- [ ] Meaningful commit message
- [ ] Pushed to remote branch
- [ ] Submitted for review

**Total Effort**: 20-60 hours depending on complexity

---

**Document Version**: 1.0
**Maintained By**: Specification governance framework
**Feedback**: Update this workflow as implementation patterns emerge
