# v0.34.4: Service Implementation & Testing

Type: Technical
Description: CompanionService orchestration, command verb integration, combat turn processing, System Crash & recovery mechanics, complete unit test suite (15+ tests, 85%+ coverage). 7-11 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.34.1 (Database), v0.34.2 (AI), v0.34.3 (Recruitment/Progression)
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.34: NPC Companion System (v0%2034%20NPC%20Companion%20System%208c14db5e07a84f2fbab85e9179c6dd9f.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.34.4-COMPANION-SERVICES

**Status:** Implementation Complete — Ready for Integration

**Timeline:** 7-11 hours

**Prerequisites:** v0.34.1 (Database), v0.34.2 (AI), v0.34.3 (Recruitment/Progression)

**Parent Specification:** v0.34 NPC Companion System[[1]](v0%2034%20NPC%20Companion%20System%208c14db5e07a84f2fbab85e9179c6dd9f.md)

---

## I. Executive Summary

This specification defines the **orchestration layer** that integrates all companion systems including combat integration, command verb parsing, System Crash mechanics, and comprehensive testing.

---

## II. In Scope vs. Out of Scope

### ✅ In Scope

- CompanionService orchestration layer
- Command verb integration (`command` and `stance`)
- Combat turn processing
- System Crash & Recovery mechanics
- Complete unit test suite (15+ tests, 85%+ coverage)

### ❌ Out of Scope

- New AI behaviors (covered in v0.34.2)
- New companion definitions (covered in v0.34.1)
- UI/rendering (handled by existing display layer)
- Advanced mechanics (defer to v2.0+)

---

## III. CompanionService (Primary Orchestration)

**File:** `RuneAndRust.Engine/Services/CompanionService.cs` (~300 lines)

**Key Methods:**

**Combat Processing:**

- `ProcessCompanionTurn()` — Execute companion action in combat (calls CompanionAIService)
- `ExecuteCompanionAction()` — Perform attack/ability/movement
- `ApplyCompanionDamage()` — Handle incoming damage

**System Crash & Recovery:**

- `HandleSystemCrash()` — Companion reaches 0 HP, apply Psychic Stress to player
- `RecoverCompanion()` — After-combat recovery to 50% HP
- `ReviveCompanion()` — Mid-dungeon revival (Bone-Setter abilities)

**Command Integration:**

- `CommandCompanion()` — Parse and execute direct commands
- `ChangeStance()` — Switch AI behavior (Aggressive/Defensive/Passive)

**Party Management:**

- `GetPartyCompanions()` — List active party members
- `GetCompanionStats()` — Current HP/Stamina/Aether/position

---

## IV. Command Verb Integration

### Command Syntax (v2.0 Canonical)

**Direct Control:**

```
command [companion_name] [ability] [target]
```

**Examples:**

```
command Kara shield_bash warden
command Finnr aetheric_bolt cultist_2
command Bjorn repair Kara
```

**Stance Control:**

```
stance [companion_name] aggressive|defensive|passive
```

**Examples:**

```
stance Runa defensive
stance Einar aggressive
stance Valdis passive
```

### CommandParser Extension

**File:** `RuneAndRust.Engine/Commands/CompanionCommands.cs` (~150 lines)

**Parsing Logic:**

1. Extract companion name (fuzzy match: "Kara" matches "Kara Ironbreaker")
2. Verify companion in active party
3. Extract ability name (fuzzy match against unlocked abilities)
4. Parse target (enemy name or companion name for support abilities)
5. Validate target in range
6. Execute via CompanionService.CommandCompanion()

---

## V. Combat Integration

### Turn Order Integration

**Companions use existing initiative system:**

```csharp
// In CombatEngine.ProcessTurn()
if (currentTurn.ActorType == "Companion")
{
    var companion = GetCompanion(currentTurn.ActorId);
    _companionService.ProcessCompanionTurn(companion, combatState);
}
```

### Companion Turn Processing

**Flow:**

1. Check if companion incapacitated (System Crash)
2. If Passive stance and no command: Skip turn
3. Call CompanionAIService.SelectAction()
4. Execute action (attack/ability/move)
5. Consume resources (Stamina/Aether)
6. Update combat state
7. Log action to combat log

---

## VI. System Crash & Recovery

### System Crash Mechanics (v2.0 Canonical)

**When Companion Reaches 0 HP:**

1. **System Crash:** Companion removed from combat
2. **Psychic Feedback:** Player suffers +10 Psychic Stress (Trauma Economy integration)
3. **Incapacitation:** Cannot act for remainder of encounter
4. **No Permadeath:** All companions recoverable

**Code Implementation:**

```csharp
public void HandleSystemCrash(Companion companion, Character player)
{
    companion.IsIncapacitated = true;
    companion.CurrentHitPoints = 0;
    
    // Apply Psychic Stress to player
    player.PsychicStress += 10;
    
    _logger.Warning(
        "Companion {Name} incapacitated. Player +10 Psychic Stress",
        companion.CompanionName);
    
    // Remove from combat grid
    _tacticalGridService.RemoveActor(companion.CurrentPosition);
}
```

### Recovery Methods

**After-Combat Recovery (Automatic):**

- Companion HP restored to 50%
- Full Stamina/Aether recovery
- Ready for next encounter

**Field Recovery (Bone-Setter Abilities):**

- Abilities like "Field Repair" can revive mid-dungeon
- Restores 4d6 HP
- Companion returns to combat grid

**Sanctuary Recovery (Full):**

- Full HP/Stamina/Aether restore
- Clears incapacitation status

---

## VII. Testing Strategy

### Unit Tests (15+ Tests)

**File:** `RuneAndRust.Tests/Services/CompanionServiceTests.cs`

**Test Coverage:**

**Command Parsing (4 tests):**

- Parse direct command with valid target
- Parse stance change command
- Invalid companion name error
- Invalid ability name error

**Combat Processing (5 tests):**

- Process companion turn (Aggressive stance)
- Process companion turn (Defensive stance)
- Passive stance with no command skips turn
- Execute companion attack
- Execute companion ability with resource cost

**System Crash & Recovery (4 tests):**

- Companion reaches 0 HP triggers System Crash
- Player receives +10 Psychic Stress on crash
- After-combat recovery restores 50% HP
- Field revival ability works mid-dungeon

**Resource Management (2 tests):**

- Stamina consumed on ability use
- Cannot use ability without sufficient resources

**Target:** 85%+ coverage

---

## VIII. Integration with Existing Systems

### Tactical Grid (v0.20)

```csharp
// Companions use same grid as player/enemies
_tacticalGridService.MoveActor(companion, newPosition);
_tacticalGridService.GetValidTargets(companion.CurrentPosition, range);
```

### Trauma Economy (v0.15)

```csharp
// System Crash applies Psychic Stress
player.PsychicStress += 10; // Per companion crash
```

### Combat Engine (v0.10)

```csharp
// Companions in turn order
combatState.TurnOrder.Add(new TurnEntry
{
    ActorType = "Companion",
    ActorId = companion.CompanionId,
    Initiative = companion.Initiative
});
```

---

## IX. Deployment Steps

### Step 1: Implement CompanionService

1. Create `RuneAndRust.Engine/Services/CompanionService.cs`
2. Implement orchestration methods
3. Register in DI container

### Step 2: Implement Command Parser

1. Create `RuneAndRust.Engine/Commands/CompanionCommands.cs`
2. Add command/stance verb handlers
3. Integrate with CommandParser service

### Step 3: Combat Integration

1. Update CombatEngine.ProcessTurn() to handle companion turns
2. Update TurnOrderService to include companions
3. Test companion initiative and turn execution

### Step 4: Run Unit Tests

```bash
dotnet test --filter CompanionServiceTests
```

**Expected:** 15+ tests pass, 85%+ coverage

### Step 5: Integration Testing

**Test Scenarios:**

1. Recruit companion → Add to party → Enter combat → Companion takes turn
2. Command companion to use ability → Verify execution
3. Companion reaches 0 HP → System Crash → Player gains Psychic Stress
4. After combat → Companion recovers to 50% HP
5. Change stance → Verify AI behavior changes

---

## X. Success Criteria

### Functional Requirements

- [ ]  Companions process turns in combat
- [ ]  Command verb parses correctly
- [ ]  Stance verb changes AI behavior
- [ ]  System Crash triggers at 0 HP
- [ ]  Player receives +10 Psychic Stress on companion crash
- [ ]  After-combat recovery functional
- [ ]  Resources (Stamina/Aether) consumed correctly
- [ ]  Tactical grid integration works

### Quality Gates

- [ ]  15+ unit tests pass
- [ ]  85%+ test coverage
- [ ]  Serilog logging throughout
- [ ]  No memory leaks in combat loop
- [ ]  Command parsing handles typos gracefully

### Balance Validation

- [ ]  Companion power scales with player
- [ ]  System Crash Psychic Stress meaningful (+10 is ~10% of Breaking threshold)
- [ ]  Recovery times feel fair
- [ ]  Commands responsive in combat

---

## XI. Remaining Integration Work

**Post-v0.34.4 Tasks:**

**Database Execution (~30 min):**

- Execute v0.34.1 SQL scripts
- Seed 6 companion definitions
- Verify foreign key constraints

**Combat Engine Integration (~2-3 hours):**

- Add companion turn processing
- Update initiative calculation
- Test with 3-companion party

**Manual Testing (~2-3 hours):**

- Full recruitment flow
- Combat with all 3 stances
- System Crash and recovery
- All 6 companions recruited and tested
- Personal quests triggered

**Total Integration Time:** ~5-7 hours

---

## XII. Related Documents

**Prerequisites:**

- v0.34.1: Database Schema & Companion Definitions[[2]](v0%2034%201%20Database%20Schema%20&%20Companion%20Definitions%200d9bf4c187e94d2dbebf7d73e81ded97.md)
- v0.34.2: Companion AI & Tactical Behavior[[3]](v0%2034%202%20Companion%20AI%20&%20Tactical%20Behavior%20b844723698524ce4939ece492a91bb75.md)
- v0.34.3: Recruitment & Progression Systems[[4]](v0%2034%203%20Recruitment%20&%20Progression%20Systems%20c234daa5f1074be8b7323d6137cf70b3.md)

**Parent Specification:**

- v0.34: NPC Companion System[[1]](v0%2034%20NPC%20Companion%20System%208c14db5e07a84f2fbab85e9179c6dd9f.md)

---

**Implementation-ready orchestration service with command verb integration, System Crash mechanics, and complete testing suite. Total implementation: ~450 lines across CompanionService and CompanionCommands.**