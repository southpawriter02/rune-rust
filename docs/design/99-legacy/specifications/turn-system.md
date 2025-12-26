# Turn System — Mechanic Specification v5.0

Type: Mechanic
Status: Review
Balance Validated: No
Document ID: AAM-SPEC-MECHANIC-TURN-v5.0
Parent item: Encounter System — Core System Specification v5.0 (Encounter%20System%20%E2%80%94%20Core%20System%20Specification%20v5%200%20c82c42d7129c4843a86f2e69cd72f0d7.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

## Core Philosophy

The Turn System is the **master clock and sequencer of combat**. It imposes a coherent, predictable sequence upon battle chaos, creating the foundation for tactical, deterministic combat.

---

## Time Hierarchy

**Round:** Full cycle where every combatant takes one turn

**Turn:** Individual time slice for a single character

---

## Initiative: Vigilance Score

```
Vigilance = FINESSE + WITS
```

**Turn Order:** Static, calculated once at combat start, sorted by Vigilance (descending)

**Tie Resolution:** Roll 1d10, higher acts first

**Design Rationale:**

- Pure stat sum creates consistent, predictable ordering
- No single archetype dominates initiative
- Enables strategic planning ("I know the boss acts after me")

---

## Turn Structure

### 1. Start of Turn Phase

**Sequence (strict order):**

1. Status effect duration tick-down
2. DoT damage (Bleeding, Poisoned, Corroded)
3. HoT healing (Regeneration)
4. Passive Stamina regeneration (10 base)
5. Special status checks ([Feared], [Stunned])
6. Turn start ability triggers

### 2. Action Phase

- Player: Awaits input, executes action
- NPC: AI determines and executes action instantly

### 3. End of Turn Phase

1. Environmental hazard damage
2. Turn end ability triggers
3. Advance to next combatant

---

## Round Structure

**Round Start:**

- Reset Reactions for all combatants
- Trigger "start of round" effects

**Round End:**

- Trigger "end of round" effects
- Check combat end conditions

---

## Edge Cases

**Combatants Join Mid-Combat:**

- Calculate Vigilance, insert at correct position
- First turn occurs when TurnOrder reaches them

**Combatants Leave Mid-Combat:**

- Remove from TurnOrder
- If current turn, immediately proceed to next

**[Hasted]/[Slowed]:**

- Do NOT modify turn order
- Affect action costs only

---

## Integration Points

**Dependencies:** Attributes (FINESSE, WITS), Combat System, Status Effect System

**Referenced By:** Action System, all Status Effects, Resource Systems (regeneration timing)