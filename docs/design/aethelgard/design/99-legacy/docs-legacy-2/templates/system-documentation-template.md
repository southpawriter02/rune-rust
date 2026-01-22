# [System Name] System

**File Path:** [Primary C# file path]
**Version:** v0.X
**Last Updated:** [Date]
**Status:** ✅ Implemented | ⏳ In Progress | ❌ Not Started

---

## Layer 1: Functional Overview (What It Does)

### Purpose
[Brief description of what this system does from a player perspective]

### Player Experience
[How players interact with this system during gameplay]

### Key Features
- Feature 1
- Feature 2
- Feature 3

### Edge Cases
- Edge case 1: [How the system behaves]
- Edge case 2: [How the system behaves]

---

## Layer 2: Statistical Reference (The Numbers)

### Primary Formula
```
[Formula Name] = [Mathematical expression]
```

**Variables:**
- `Variable1`: Description
  - Range: [min-max]
  - Default: [value]
- `Variable2`: Description
  - Range: [min-max]
  - Default: [value]

### Secondary Formulas
[List all supporting formulas with variable definitions]

### Probability Tables
| Input Range | Output | Probability |
|-------------|--------|-------------|
| X-Y | Z | XX% |

### Example Calculations
**Example 1: [Scenario Name]**
```
Input: A = 5, B = 10
Calculation: Result = (A × 2) + B
Output: Result = 20
```

**Example 2: [Scenario Name]**
```
[Step-by-step calculation with actual values]
```

### Stat Ranges
| Stat | Minimum | Maximum | Average | Notes |
|------|---------|---------|---------|-------|
| Stat1 | X | Y | Z | [Context] |

---

## Layer 3: Technical Implementation (How It Works)

### Service Class
**File:** `RuneAndRust.Engine/[ServiceName].cs`

```csharp
public class [ServiceName]
{
    // Key methods
    public [ReturnType] [MethodName]([Parameters])
    {
        // Purpose: [What this method does]
        // Returns: [What it returns]
        // Throws: [Any exceptions]
    }
}
```

### Core Models
**File:** `RuneAndRust.Core/[ModelName].cs`

```csharp
public class [ModelName]
{
    public int PropertyName { get; set; }  // Purpose: [Description]
}
```

### Database Schema
```sql
CREATE TABLE [TableName] (
    [column_name] [TYPE] [CONSTRAINTS],  -- Purpose: [Description]
    PRIMARY KEY ([column_name]),
    FOREIGN KEY ([column_name]) REFERENCES [OtherTable]([column])
);
```

**Indexes:**
- `idx_[name]` on `[columns]` - Purpose: [Why this index exists]

### Integration Points
- **Integrates with:** [System A] via [method/event]
- **Called by:** [System B] during [scenario]
- **Depends on:** [System C] for [functionality]

### Data Flow
```
[Source] → [Process 1] → [Process 2] → [Destination]
```

---

## Layer 4: Testing Coverage (How We Verify)

### Unit Tests
**File:** `RuneAndRust.Tests/[TestFileName].cs`

**Coverage:** XX% ([Uncovered areas])

**Key Tests:**
```csharp
[TestMethod]
public void [MethodName]_[Scenario]_[ExpectedBehavior]()
{
    // Arrange: [What is set up]
    // Act: [What action is performed]
    // Assert: [What is verified]
}
```

**Test Count:** XX tests

**Missing Coverage:**
- [ ] Edge case 1: [Description]
- [ ] Edge case 2: [Description]

### Integration Tests
[List integration test scenarios that verify this system works with others]

### QA Checklist
- [ ] Test 1: [Manual verification step]
- [ ] Test 2: [Manual verification step]
- [ ] Test 3: [Manual verification step]

### Known Issues
- Issue 1: [Description] - [Status]
- Issue 2: [Description] - [Status]

---

## Layer 5: Balance Considerations (Why These Numbers)

### Design Intent
[What this system is meant to accomplish from a game design perspective]

### Power Budget
[How this system fits into the overall power curve]

**Expected Ranges:**
- Low power: [Range]
- Medium power: [Range]
- High power: [Range]

### Tuning Rationale
[Why the current values were chosen]

**Key Balance Points:**
- Balance point 1: [Explanation]
- Balance point 2: [Explanation]

### Known Issues
- [ ] Issue 1: [What needs tuning and why]
- [ ] Issue 2: [What needs tuning and why]

### Future Tuning Considerations
[What might need adjustment based on playtesting]

---

## Cross-References

### Related Systems
- [System A](../01-systems/system-a.md) - [How they relate]
- [System B](../01-systems/system-b.md) - [How they relate]

### Registry Entries
- [Ability X](../02-statistical-registry/abilities-registry.md#ability-x) - Uses this system
- [Equipment Y](../02-statistical-registry/equipment-registry.md#equipment-y) - Modified by this system

### Technical References
- [Service Architecture](../03-technical-reference/service-architecture.md#[service-name])
- [Database Schema](../03-technical-reference/database-schema.md#[table-name])

---

## Changelog

### v0.X - [Date]
- Initial implementation
- Added feature X
- Modified formula Y

### v0.X+1 - [Date]
- Balance changes: [What changed]
- Bug fixes: [What was fixed]

---

**Documentation Status:** 🚧 Template
**Last Reviewed:** [Date]
**Reviewer:** [Name]
