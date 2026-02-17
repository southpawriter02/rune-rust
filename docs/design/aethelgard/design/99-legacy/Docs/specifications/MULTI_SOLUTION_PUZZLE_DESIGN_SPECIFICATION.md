# Multi-Solution Puzzle Design Specification

## Version 1.0 - Alternative Approach System Documentation

**Document Version:** 1.0
**Last Updated:** November 2024
**Specification ID:** SPEC-WORLD-005
**Status:** Draft
**Domain:** World Systems (Design Philosophy)
**Core Dependencies:** `RuneAndRust.Core.Descriptors`, `RuneAndRust.Engine.ObjectInteractionService`

---

## Document Control

### Version History
| Version | Date | Author | Changes | Reviewers |
|---------|------|--------|---------|-----------|
| 1.0 | 2024-11-27 | AI | Initial specification | - |

### Approval Status
- [x] **Draft**: Initial authoring in progress
- [ ] **Review**: Ready for stakeholder review
- [ ] **Approved**: Approved for implementation
- [ ] **Active**: Currently implemented and maintained

---

## Table of Contents

1. [Overview](#1-overview)
2. [Design Philosophy](#2-design-philosophy)
3. [Solution Archetypes](#3-solution-archetypes)
4. [Character Build Integration](#4-character-build-integration)
5. [Puzzle Categories](#5-puzzle-categories)
6. [Implementation Patterns](#6-implementation-patterns)
7. [Content Design Guidelines](#7-content-design-guidelines)
8. [Data Model](#8-data-model)
9. [GUI Integration](#9-gui-integration)
10. [Balance & Validation](#10-balance--validation)

---

## 1. Overview

### 1.1 Purpose

This specification defines the Multi-Solution Puzzle Design philosophy for Rune & Rust. Every physical interaction puzzle should offer multiple valid approaches, ensuring that different character builds can contribute meaningfully to environmental challenges.

The goal is to reward player investment in character specialization while never creating "wrong builds" that cannot progress.

### 1.2 Scope

**In Scope**:
- Solution archetype taxonomy (Force, Finesse, Intellect, Bypass)
- Character build mapping to puzzle solutions
- Puzzle category definitions with required solutions
- Content design guidelines and checklists
- Validation system for puzzle completeness

**Out of Scope**:
- Specific puzzle implementations → Individual level design
- Combat encounter design → SPEC-COMBAT-012
- Narrative puzzle content → Quest specifications
- Logic puzzle mechanics → Separate specification

### 1.3 Success Criteria

- **Build Viability**: Every primary build can solve 80%+ of puzzles
- **Meaningful Choice**: Different solutions have different trade-offs
- **Discovery Reward**: Players feel clever when finding alternative solutions
- **Content Validation**: Automated checks catch single-solution puzzles

---

## 2. Design Philosophy

### 2.1 Core Principle: The Three-Solution Minimum

Every significant environmental puzzle MUST have at least three valid solutions:

1. **Primary Solution**: The "intended" approach, usually physical
2. **Alternative Solution**: Different attribute-based approach
3. **Specialist Solution**: Class/specialization-specific bypass

### 2.2 Design Pillars

1. **Build Validation**
   - **Rationale**: No character build should be "locked out" of content
   - **Examples**: Pure MIGHT warrior can still progress through hacking puzzles

2. **Trade-off Differentiation**
   - **Rationale**: Different solutions should have meaningful differences
   - **Examples**: Force is fast but noisy; Finesse is quiet but risky; Bypass requires resources

3. **Discovery Satisfaction**
   - **Rationale**: Finding an alternative solution should feel rewarding
   - **Examples**: "I realized my Scrap-Tinker could just bypass the whole mechanism!"

4. **Narrative Coherence**
   - **Rationale**: Solutions should make sense in the game world
   - **Examples**: A rusted lever can be forced (MIGHT) or lubricated (Adept tools)

### 2.3 Anti-Patterns to Avoid

| Anti-Pattern | Problem | Solution |
|--------------|---------|----------|
| **Single Solution** | Locks out builds | Add alternatives |
| **Trivial Alternative** | No meaningful choice | Add trade-offs |
| **Impossible Alternative** | DC too high for any build | Cap DC appropriately |
| **Hidden Solution** | Players never find it | Provide discovery hints |
| **Mandatory Specialist** | Requires specific class | Add general alternative |

---

## 3. Solution Archetypes

### 3.1 Force (MIGHT-Based)

**Philosophy**: Apply raw physical power to overcome obstacles.

**Characteristics**:
- High success rate for high-MIGHT characters
- Often the fastest solution
- May create noise (alert enemies)
- Risk of fumble (break object)
- No special resources required

**Applicable To**:
- Stuck mechanisms (levers, valves, wheels)
- Blocked passages (debris, doors)
- Heavy objects (crates, barricades)
- Jammed locks (forced entry)

**Typical DC Range**: 3-6

**Trade-offs**:
| Aspect | Rating |
|--------|--------|
| Speed | Fast |
| Noise | Loud |
| Risk | Moderate (fumble) |
| Resource Cost | None |
| Reversibility | Low (may break) |

### 3.2 Finesse (FINESSE-Based)

**Philosophy**: Apply precision and dexterity to solve problems elegantly.

**Characteristics**:
- High success rate for high-FINESSE characters
- Usually quiet
- May take longer
- Lower fumble consequences
- May require tools (lockpicks)

**Applicable To**:
- Locks and latches
- Delicate mechanisms
- Sequence puzzles (button order)
- Trap disarmament
- Precise adjustments

**Typical DC Range**: 3-5

**Trade-offs**:
| Aspect | Rating |
|--------|--------|
| Speed | Moderate |
| Noise | Quiet |
| Risk | Low |
| Resource Cost | Tools (sometimes) |
| Reversibility | High |

### 3.3 Intellect (WITS-Based)

**Philosophy**: Analyze the problem and find the clever solution.

**Characteristics**:
- High success rate for high-WITS characters
- Often reveals additional information
- May identify alternative solutions
- Rewards investigation
- No physical risk

**Applicable To**:
- Logic puzzles (codes, sequences)
- Hack terminals for information
- Identify weak points
- Discover hidden mechanisms
- Understand ancient tech

**Typical DC Range**: 3-5

**Trade-offs**:
| Aspect | Rating |
|--------|--------|
| Speed | Variable |
| Noise | Silent |
| Risk | None (information only) |
| Resource Cost | None |
| Reversibility | N/A (information) |

### 3.4 Bypass (Specialist)

**Philosophy**: Use specialized knowledge to circumvent the problem entirely.

**Characteristics**:
- Requires specific specialization or equipment
- Often the most efficient solution
- May consume resources
- Feels rewarding when applicable
- Not available to all characters

**Applicable To**:
- Mechanism bypass (Scrap-Tinker)
- Runic override (Rune-Singer)
- Security bypass (Infiltrator)
- Structural analysis (Combat Engineer)

**Typical DC Range**: 2-4 (lower than general solutions)

**Trade-offs**:
| Aspect | Rating |
|--------|--------|
| Speed | Fastest |
| Noise | Variable |
| Risk | Low |
| Resource Cost | Tools/Abilities |
| Reversibility | High |

### 3.5 Destruction (Combat)

**Philosophy**: If you can't solve it, destroy it.

**Characteristics**:
- Always available as last resort
- Time-consuming (combat actions)
- Very noisy
- May have collateral damage
- Cannot destroy everything

**Applicable To**:
- Destructible barriers
- Weak mechanisms
- Some containers
- Obstacles (not puzzles)

**Trade-offs**:
| Aspect | Rating |
|--------|--------|
| Speed | Slow (HP-based) |
| Noise | Very Loud |
| Risk | Collateral damage |
| Resource Cost | HP/Stamina |
| Reversibility | None |

---

## 4. Character Build Integration

### 4.1 Primary Build Archetypes

| Build | Primary Attribute | Secondary | Preferred Solutions |
|-------|-------------------|-----------|---------------------|
| **Warrior** | MIGHT | STURDINESS | Force, Destruction |
| **Duelist** | FINESSE | MIGHT | Finesse, Force |
| **Scholar** | WITS | WILL | Intellect, Analysis |
| **Mystic** | WILL | WITS | Bypass (Runic), Intellect |
| **Ranger** | FINESSE | WITS | Finesse, Intellect |
| **Tank** | STURDINESS | MIGHT | Force, Endurance |

### 4.2 Specialization Bypass Abilities

| Specialization | Bypass Type | Applicable Puzzles |
|----------------|-------------|-------------------|
| **Scrap-Tinker** | System Bypass | Mechanisms, terminals |
| **Rune-Singer** | Runic Override | Ancient tech, wards |
| **Hólmgangr** | Brute Force+ | Reduced MIGHT DCs |
| **Shadow-Walker** | Security Bypass | Locks, alarms |
| **War-Forged** | Structural Analysis | Destructible weak points |
| **Void-Touched** | Aetheric Sense | Hidden mechanisms |

### 4.3 Build Coverage Matrix

Every puzzle should be solvable by multiple builds:

| Puzzle Type | Warrior | Duelist | Scholar | Mystic | Ranger |
|-------------|---------|---------|---------|--------|--------|
| Stuck Lever | Force ✓ | Finesse | Analyze | Bypass? | Finesse |
| Locked Door | Force ✓ | Lockpick ✓ | Analyze | Bypass | Lockpick ✓ |
| Code Panel | Destroy? | Sequence | Hack ✓ | Bypass ✓ | Sequence |
| Heavy Debris | Force ✓ | - | Analyze | - | - |
| Trap | Trigger | Disarm ✓ | Detect ✓ | Sense | Disarm ✓ |

**Legend**:
- ✓ = Preferred solution for this build
- Italics = Available but not optimal
- ? = Possible but risky
- \- = Not directly solvable (needs party help or alternative)

### 4.4 Party Composition Considerations

**Solo Play**: Every puzzle must be solvable by any single character build.

**Party Play**: Optimal solution may require specific party member, but alternatives exist.

**Design Rule**: Never create a puzzle that requires a specific attribute above 6 with no alternatives.

---

## 5. Puzzle Categories

### 5.1 Category: Hardware Malfunction

**Description**: Mechanical devices that have degraded over time.

**Examples**: Rusted levers, stuck valves, jammed doors

**Required Solutions**:
| # | Solution Type | Approach | DC |
|---|---------------|----------|-----|
| 1 | Force | MIGHT to force mechanism | 3-5 |
| 2 | Finesse | FINESSE to carefully work mechanism | 3-4 |
| 3 | Bypass | Scrap-Tinker System Bypass | 2-3 |
| 4 | Destruction | Destroy mechanism (consequences) | HP-based |

**Design Template**:
```
Object: [Adjective] [Mechanism Type]
Description: "A [material] [mechanism] that [problem description]."

Solutions:
- Force: "With enough strength, you could force it."
- Finesse: "Careful manipulation might work it free."
- Bypass: "[Specialist] might find another way."
```

### 5.2 Category: Security System

**Description**: Locks, alarms, and access controls.

**Examples**: Locked doors, security terminals, alarm systems

**Required Solutions**:
| # | Solution Type | Approach | DC |
|---|---------------|----------|-----|
| 1 | Lockpicking | FINESSE + Lockpicks | 3-5 |
| 2 | Hacking | WITS + Terminal access | 3-5 |
| 3 | Force | MIGHT to break (loud, damage) | 5-6 |
| 4 | Bypass | Keycard/code found elsewhere | N/A |

**Design Template**:
```
Object: [Security Level] [Lock/Terminal Type]
Description: "A [technology level] security [mechanism]."

Solutions:
- Lockpick: "The lock mechanism looks [complexity]."
- Hack: "A terminal nearby might control access."
- Force: "The [material] could be broken through."
- Key: "There may be a keycard somewhere."
```

### 5.3 Category: Environmental Hazard

**Description**: Dangerous environmental conditions blocking progress.

**Examples**: Steam vents, electrified floors, toxic gas

**Required Solutions**:
| # | Solution Type | Approach | DC |
|---|---------------|----------|-----|
| 1 | Disable | Turn off source (valve, switch) | 3-4 |
| 2 | Bypass | Find alternate route | N/A |
| 3 | Resist | Endure with STURDINESS/items | Damage |
| 4 | Analyze | Find safe timing/path | WITS 3-4 |

**Design Template**:
```
Hazard: [Type] [Hazard Name]
Description: "[Sensory description of hazard]."

Solutions:
- Disable: "The source might be controllable."
- Bypass: "There may be another way around."
- Resist: "With protection, you might survive passage."
- Analyze: "Study the pattern for safe passage."
```

### 5.4 Category: Logic Puzzle

**Description**: Puzzles requiring deduction or pattern recognition.

**Examples**: Button sequences, combination locks, rune arrangements

**Required Solutions**:
| # | Solution Type | Approach | DC |
|---|---------------|----------|-----|
| 1 | Solve | Deduce correct solution | None (logic) |
| 2 | Investigate | Find clue elsewhere | WITS 2-3 |
| 3 | Trial & Error | Attempt combinations | Time cost |
| 4 | Bypass | Specialist override | Varies |

**Design Template**:
```
Puzzle: [Type] [Puzzle Name]
Description: "[Description of puzzle mechanism]."

Solutions:
- Solve: "The solution involves [hint type]."
- Investigate: "Clues may be found [location hint]."
- Trial: "Systematic attempts might work."
- Bypass: "[Specialist] might circumvent this."
```

### 5.5 Category: Obstruction

**Description**: Physical barriers blocking passage.

**Examples**: Debris piles, collapsed passages, barricades

**Required Solutions**:
| # | Solution Type | Approach | DC |
|---|---------------|----------|-----|
| 1 | Clear | MIGHT to move debris | 4-6 |
| 2 | Navigate | FINESSE to squeeze through | 3-4 |
| 3 | Destroy | Combat to break barrier | HP-based |
| 4 | Alternate | Find different route | Exploration |

**Design Template**:
```
Obstruction: [Material] [Obstruction Type]
Description: "[Description of blockage]."

Solutions:
- Clear: "Heavy lifting might clear a path."
- Navigate: "A narrow gap could be traversed."
- Destroy: "The [material] could be broken."
- Alternate: "There may be another way."
```

---

## 6. Implementation Patterns

### 6.1 Multi-Solution Object Structure

```csharp
public class MultiSolutionObject : InteractiveObject
{
    /// <summary>
    /// All valid solutions for this puzzle
    /// </summary>
    public List<PuzzleSolution> Solutions { get; set; } = new();

    /// <summary>
    /// Whether puzzle has been solved (any solution)
    /// </summary>
    public bool IsSolved { get; set; }

    /// <summary>
    /// Which solution was used (for narrative)
    /// </summary>
    public PuzzleSolution? UsedSolution { get; set; }

    /// <summary>
    /// Get available solutions for a character
    /// </summary>
    public List<PuzzleSolution> GetAvailableSolutions(PlayerCharacter character)
    {
        return Solutions.Where(s => s.IsAvailableTo(character)).ToList();
    }
}
```

### 6.2 PuzzleSolution Class

```csharp
public class PuzzleSolution
{
    /// <summary>Unique identifier</summary>
    public string SolutionId { get; set; } = string.Empty;

    /// <summary>Solution archetype</summary>
    public SolutionArchetype Archetype { get; set; }

    /// <summary>Display name for solution</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Description shown to player</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Hint shown during examination</summary>
    public string DiscoveryHint { get; set; } = string.Empty;

    /// <summary>Required skill check (if any)</summary>
    public SkillCheckType? RequiredCheck { get; set; }

    /// <summary>Difficulty class</summary>
    public int DC { get; set; }

    /// <summary>Required specialization (if specialist solution)</summary>
    public string? RequiredSpecialization { get; set; }

    /// <summary>Required item (if any)</summary>
    public string? RequiredItem { get; set; }

    /// <summary>Resource cost (stamina, items, etc.)</summary>
    public Dictionary<string, int> ResourceCosts { get; set; } = new();

    /// <summary>Time cost in minutes</summary>
    public int TimeCost { get; set; }

    /// <summary>Noise level (0-10)</summary>
    public int NoiseLevel { get; set; }

    /// <summary>Success narrative</summary>
    public string SuccessNarrative { get; set; } = string.Empty;

    /// <summary>Failure narrative</summary>
    public string FailureNarrative { get; set; } = string.Empty;

    /// <summary>Check if solution is available to character</summary>
    public bool IsAvailableTo(PlayerCharacter character)
    {
        // Check specialization requirement
        if (!string.IsNullOrEmpty(RequiredSpecialization))
        {
            if (!character.HasSpecialization(RequiredSpecialization))
                return false;
        }

        // Check item requirement
        if (!string.IsNullOrEmpty(RequiredItem))
        {
            if (!character.HasItem(RequiredItem))
                return false;
        }

        return true;
    }
}

public enum SolutionArchetype
{
    Force,
    Finesse,
    Intellect,
    Bypass,
    Destruction
}
```

### 6.3 Solution Discovery

Players discover solutions through examination:

```csharp
public List<SolutionHint> ExaminePuzzle(
    MultiSolutionObject puzzle,
    PlayerCharacter character)
{
    var hints = new List<SolutionHint>();

    foreach (var solution in puzzle.Solutions)
    {
        var hint = new SolutionHint
        {
            SolutionId = solution.SolutionId,
            Archetype = solution.Archetype
        };

        // Always show general hint
        hint.GeneralHint = solution.DiscoveryHint;

        // Show availability based on character
        hint.IsAvailable = solution.IsAvailableTo(character);

        // Show DC if character has relevant skill
        if (solution.RequiredCheck.HasValue)
        {
            var relevantStat = GetRelevantAttribute(character, solution.RequiredCheck.Value);
            hint.ShowDC = relevantStat >= 3; // Basic competence reveals DC
            hint.EstimatedDifficulty = EstimateDifficulty(relevantStat, solution.DC);
        }

        // Specialist hint
        if (!string.IsNullOrEmpty(solution.RequiredSpecialization))
        {
            hint.SpecialistHint = $"A {solution.RequiredSpecialization} might have another approach.";
        }

        hints.Add(hint);
    }

    return hints;
}
```

---

## 7. Content Design Guidelines

### 7.1 Puzzle Design Checklist

For every significant puzzle, verify:

- [ ] **Three Solutions Minimum**: At least 3 valid approaches exist
- [ ] **Build Coverage**: Any primary build can solve it
- [ ] **DC Appropriateness**: No solution requires attribute > 7
- [ ] **Trade-offs Clear**: Each solution has distinct pros/cons
- [ ] **Hints Present**: Examination reveals solution hints
- [ ] **Bypass Optional**: Specialist bypass is bonus, not required
- [ ] **Failure Recoverable**: Failed attempts don't softlock
- [ ] **Narrative Coherent**: Solutions make sense in world

### 7.2 DC Guidelines

| Difficulty | DC | Who Can Reliably Solve | Design Use |
|------------|-----|----------------------|------------|
| Trivial | 1-2 | Anyone | Tutorial, minor obstacles |
| Easy | 3 | Average characters (4+ dice) | Common puzzles |
| Moderate | 4 | Competent characters (5+ dice) | Standard puzzles |
| Challenging | 5 | Skilled characters (6+ dice) | Significant obstacles |
| Hard | 6 | Expert characters (7+ dice) | Optional challenges |
| Extreme | 7+ | Specialists only | Achievement challenges |

**Rule**: Primary solution DC should never exceed 5. Alternative solutions can be harder.

### 7.3 Trade-off Design

Every solution should have at least one significant trade-off:

| Solution | Primary Benefit | Trade-off |
|----------|----------------|-----------|
| Force | Speed | Noise, fumble risk |
| Finesse | Quiet | Time, tool consumption |
| Intellect | Information | Doesn't directly solve |
| Bypass | Efficiency | Resource cost |
| Destruction | Universal | Very slow, very loud |

### 7.4 Hint Placement

Solutions should be discoverable:

| Hint Type | Placement | Example |
|-----------|-----------|---------|
| **Obvious** | Object description | "The mechanism looks stuck but could be forced." |
| **Examine** | Investigation result | "Closer inspection reveals a maintenance panel." |
| **Environmental** | Room description | "Tool marks suggest regular maintenance." |
| **Specialist** | Class-specific | "Your Scrap-Tinker training reveals an override." |
| **Remote** | Other location | "A note mentions the access code: 4-7-2-1." |

### 7.5 Example: Well-Designed Puzzle

**Name**: Corroded Blast Door

**Description**: "A massive blast door blocks the passage. Its surface is pitted with corrosion, and the control panel beside it is dark and unresponsive."

**Solutions**:

| # | Type | Approach | DC | Time | Noise | Trade-off |
|---|------|----------|-----|------|-------|-----------|
| 1 | Force | Pry open door | MIGHT 5 | 3 min | Loud | May break door permanently |
| 2 | Finesse | Restore panel power | FINESSE 4 | 5 min | Quiet | Requires wire from inventory |
| 3 | Intellect | Find backup entrance | WITS 3 | 2 min | Silent | Reveals alternative route only |
| 4 | Bypass | Override security | Scrap-Tinker | 2 min | Quiet | Requires toolkit charge |
| 5 | Destruction | Cut through door | Combat | 10+ min | Very Loud | Damages equipment |

**Hints**:
- Description: "The door could perhaps be forced open with sufficient strength."
- Examine: "The control panel's power coupling has corroded loose."
- Room: "A maintenance duct in the ceiling might lead somewhere."
- Specialist: "Your training reveals an emergency override circuit."

---

## 8. Data Model

### 8.1 Puzzle Definition Schema

```json
{
  "puzzleId": "blast_door_001",
  "name": "Corroded Blast Door",
  "category": "Security",
  "description": "A massive blast door blocks the passage...",
  "solutions": [
    {
      "solutionId": "force_open",
      "archetype": "Force",
      "name": "Force Open",
      "description": "Pry the door open with brute strength",
      "discoveryHint": "The door could perhaps be forced open.",
      "requiredCheck": "MIGHT",
      "dc": 5,
      "timeCost": 3,
      "noiseLevel": 8,
      "successNarrative": "With a mighty heave, you wrench the door open...",
      "failureNarrative": "The door groans but refuses to budge...",
      "fumbleConsequence": "The door mechanism breaks permanently."
    },
    {
      "solutionId": "restore_power",
      "archetype": "Finesse",
      "name": "Restore Power",
      "description": "Repair the control panel's power connection",
      "discoveryHint": "The power coupling has corroded loose.",
      "requiredCheck": "FINESSE",
      "dc": 4,
      "requiredItem": "wire",
      "timeCost": 5,
      "noiseLevel": 2,
      "successNarrative": "You reconnect the power and the panel hums to life...",
      "failureNarrative": "The connection sparks but doesn't hold..."
    },
    {
      "solutionId": "find_alternate",
      "archetype": "Intellect",
      "name": "Find Alternate Route",
      "description": "Locate another way around",
      "discoveryHint": "A maintenance duct might lead somewhere.",
      "requiredCheck": "WITS",
      "dc": 3,
      "timeCost": 2,
      "noiseLevel": 0,
      "successNarrative": "You spot a maintenance duct that bypasses the door...",
      "consequenceType": "RevealAlternate"
    },
    {
      "solutionId": "security_override",
      "archetype": "Bypass",
      "name": "Security Override",
      "description": "Use Scrap-Tinker knowledge to override security",
      "discoveryHint": "An emergency override circuit is accessible.",
      "requiredSpecialization": "Scrap-Tinker",
      "dc": 3,
      "resourceCosts": { "toolkit_charge": 1 },
      "timeCost": 2,
      "noiseLevel": 1,
      "successNarrative": "You trigger the emergency override and the door slides open..."
    }
  ],
  "validation": {
    "minSolutions": 3,
    "requiresForce": true,
    "requiresFinesse": true,
    "maxPrimaryDC": 5,
    "buildCoverage": ["Warrior", "Duelist", "Scholar", "Mystic", "Ranger"]
  }
}
```

### 8.2 Validation System

```csharp
public class PuzzleValidator
{
    public ValidationResult Validate(MultiSolutionObject puzzle)
    {
        var result = new ValidationResult { IsValid = true };

        // Check minimum solutions
        if (puzzle.Solutions.Count < 3)
        {
            result.AddError("Puzzle must have at least 3 solutions");
        }

        // Check build coverage
        var coveredBuilds = GetCoveredBuilds(puzzle);
        var missingBuilds = RequiredBuilds.Except(coveredBuilds);
        if (missingBuilds.Any())
        {
            result.AddWarning($"Builds without solution: {string.Join(", ", missingBuilds)}");
        }

        // Check DC caps
        var primarySolution = puzzle.Solutions.FirstOrDefault(s => s.Archetype != SolutionArchetype.Bypass);
        if (primarySolution?.DC > 5)
        {
            result.AddError("Primary solution DC exceeds maximum of 5");
        }

        // Check specialist requirement
        var hasNonSpecialist = puzzle.Solutions.Any(s =>
            string.IsNullOrEmpty(s.RequiredSpecialization));
        if (!hasNonSpecialist)
        {
            result.AddError("All solutions require specialization - general solution needed");
        }

        return result;
    }

    private HashSet<string> GetCoveredBuilds(MultiSolutionObject puzzle)
    {
        var covered = new HashSet<string>();

        foreach (var solution in puzzle.Solutions)
        {
            switch (solution.Archetype)
            {
                case SolutionArchetype.Force:
                    covered.Add("Warrior");
                    covered.Add("Tank");
                    break;
                case SolutionArchetype.Finesse:
                    covered.Add("Duelist");
                    covered.Add("Ranger");
                    break;
                case SolutionArchetype.Intellect:
                    covered.Add("Scholar");
                    covered.Add("Mystic");
                    break;
            }

            // Bypass solutions cover their specialization's base class
            if (!string.IsNullOrEmpty(solution.RequiredSpecialization))
            {
                covered.Add(GetBaseClass(solution.RequiredSpecialization));
            }
        }

        return covered;
    }
}
```

---

## 9. GUI Integration

### 9.1 Solution Selection Dialog

When multiple solutions are available:

```
┌────────────────────────────────────────────────────┐
│ CORRODED BLAST DOOR                                │
│ "A massive blast door blocks the passage..."       │
├────────────────────────────────────────────────────┤
│ AVAILABLE APPROACHES:                              │
│                                                    │
│ [1] Force Open                    (MIGHT DC 5) ⚠  │
│     Time: 3 min | Noise: Loud | Risk: May break   │
│                                                    │
│ [2] Restore Power                 (FINESSE DC 4)  │
│     Time: 5 min | Noise: Quiet | Needs: Wire      │
│                                                    │
│ [3] Find Alternate Route          (WITS DC 3)     │
│     Time: 2 min | Noise: Silent | Reveals path    │
│                                                    │
│ [4] Security Override ★           (DC 3)          │
│     Time: 2 min | Noise: Quiet | Uses: Toolkit    │
│     [Scrap-Tinker Specialization]                 │
│                                                    │
│ ★ = Specialist solution available                  │
│ ⚠ = High fumble risk                              │
│                                                    │
│ [Select Approach]                    [Examine More]│
└────────────────────────────────────────────────────┘
```

### 9.2 SolutionSelectionViewModel

```csharp
public class SolutionSelectionViewModel : ViewModelBase
{
    // Puzzle Info
    public string PuzzleName { get; set; } = string.Empty;
    public string PuzzleDescription { get; set; } = string.Empty;

    // Available Solutions
    public ObservableCollection<SolutionOptionViewModel> Solutions { get; }

    // Selection
    public SolutionOptionViewModel? SelectedSolution { get; set; }
    public bool HasSelection => SelectedSolution != null;

    // Commands
    public ICommand SelectSolutionCommand { get; }
    public ICommand ExamineMoreCommand { get; }
    public ICommand CancelCommand { get; }
}

public class SolutionOptionViewModel : ViewModelBase
{
    public int Hotkey { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Requirements
    public string? SkillCheck { get; set; }
    public int? DC { get; set; }
    public string CheckDisplay => SkillCheck != null ? $"({SkillCheck} DC {DC})" : "";

    // Trade-offs
    public string TimeCost { get; set; } = string.Empty;
    public string NoiseLevel { get; set; } = string.Empty;
    public string? ResourceCost { get; set; }
    public string? RiskWarning { get; set; }

    // Specialist
    public bool IsSpecialist { get; set; }
    public string? SpecializationName { get; set; }

    // Risk
    public bool ShowRiskWarning { get; set; }
    public float FumbleChance { get; set; }

    // Visual
    public string BackgroundColor { get; set; } = "#2A2A2A";
    public string BorderColor { get; set; } = "#3A3A3A";
}
```

### 9.3 Hint Display in Examination

When examining a puzzle object:

```
┌────────────────────────────────────────────────────┐
│ EXAMINE: Corroded Blast Door                       │
├────────────────────────────────────────────────────┤
│ A massive blast door blocks the passage. Its       │
│ surface is pitted with corrosion, and the control  │
│ panel beside it is dark and unresponsive.          │
│                                                    │
│ You notice:                                        │
│ • The door could perhaps be forced open with       │
│   sufficient strength.                             │
│ • The control panel's power coupling has           │
│   corroded loose.                                  │
│ • A maintenance duct in the ceiling might lead     │
│   somewhere.                                       │
│                                                    │
│ [Your Scrap-Tinker training reveals an emergency   │
│  override circuit accessible through the panel.]   │
│                                                    │
│                              [Interact] [Close]    │
└────────────────────────────────────────────────────┘
```

---

## 10. Balance & Validation

### 10.1 Automated Validation

Run validation on all puzzle content:

```csharp
public async Task<ValidationReport> ValidateAllPuzzles()
{
    var report = new ValidationReport();
    var validator = new PuzzleValidator();

    foreach (var puzzle in await LoadAllPuzzles())
    {
        var result = validator.Validate(puzzle);
        report.AddResult(puzzle.PuzzleId, result);

        if (!result.IsValid)
        {
            report.FailedPuzzles.Add(puzzle.PuzzleId);
        }
    }

    // Summary statistics
    report.TotalPuzzles = report.Results.Count;
    report.ValidPuzzles = report.TotalPuzzles - report.FailedPuzzles.Count;
    report.Coverage = CalculateBuildCoverage(report);

    return report;
}
```

### 10.2 Build Coverage Report

```
MULTI-SOLUTION PUZZLE VALIDATION REPORT
=======================================

Total Puzzles: 47
Valid Puzzles: 45 (96%)
Failed Puzzles: 2

BUILD COVERAGE:
  Warrior:  47/47 (100%) ✓
  Duelist:  46/47 (98%)  ✓
  Scholar:  45/47 (96%)  ✓
  Mystic:   44/47 (94%)  ⚠
  Ranger:   46/47 (98%)  ✓
  Tank:     43/47 (91%)  ⚠

SOLUTION ARCHETYPE DISTRIBUTION:
  Force:       47 puzzles (100%)
  Finesse:     42 puzzles (89%)
  Intellect:   38 puzzles (81%)
  Bypass:      31 puzzles (66%)
  Destruction: 23 puzzles (49%)

FAILED PUZZLES:
  - security_terminal_003: Only 2 solutions
  - ancient_lock_007: All solutions require specialization

WARNINGS:
  - steam_valve_012: Primary DC is 6 (recommend ≤5)
  - debris_pile_004: No Finesse solution
```

### 10.3 Playtest Validation

Track solution usage in playtesting:

| Metric | Target | Alarm Threshold |
|--------|--------|-----------------|
| Solution variety | >2 solutions used per puzzle | <2 average |
| Build completion rate | 95%+ per build | <90% |
| Time variance | <2x between fastest/slowest | >3x |
| Fumble rate | 5-15% on risky solutions | >25% |
| Hint discovery | 80%+ find alternate solutions | <50% |

---

## Appendix A: Solution Template Library

### A.1 Force Solutions

```json
{
  "archetype": "Force",
  "templates": [
    {
      "verb": "force",
      "applicable": ["doors", "mechanisms", "obstacles"],
      "checkType": "MIGHT",
      "dcRange": [3, 6],
      "noiseRange": [6, 10],
      "timeRange": [2, 5],
      "fumbleConsequence": "BreakObject"
    },
    {
      "verb": "push",
      "applicable": ["obstacles", "debris", "crates"],
      "checkType": "MIGHT",
      "dcRange": [2, 5],
      "noiseRange": [4, 7],
      "timeRange": [1, 3],
      "fumbleConsequence": "Displacement"
    }
  ]
}
```

### A.2 Finesse Solutions

```json
{
  "archetype": "Finesse",
  "templates": [
    {
      "verb": "carefully manipulate",
      "applicable": ["mechanisms", "controls"],
      "checkType": "FINESSE",
      "dcRange": [3, 5],
      "noiseRange": [1, 3],
      "timeRange": [3, 7]
    },
    {
      "verb": "pick",
      "applicable": ["locks"],
      "checkType": "FINESSE",
      "dcRange": [3, 5],
      "requiredItem": "lockpicks",
      "noiseRange": [1, 2],
      "timeRange": [2, 5]
    }
  ]
}
```

---

## Document History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | Nov 2024 | Initial Multi-Solution Puzzle Design specification |

---

*This document is part of the Rune & Rust technical documentation suite.*
*Related: PHYSICAL_INTERACTION_SPECIFICATION.md, Content Design Guidelines*
