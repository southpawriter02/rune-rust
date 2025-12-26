---
id: SPEC-CORE-RES-CORRUPTION
title: "Runic Blight Corruption — Resource Specification"
version: 1.0
status: approved
last-updated: 2025-12-14
related-files:
  - path: "RuneAndRust.Engine/Services/CorruptionService.cs"
    status: Planned
  - path: "RuneAndRust.Engine/Models/CorruptionState.cs"
    status: Planned
---

# Runic Blight Corruption

> *"The Blight doesn't just poison the world—it poisons those who touch it. Every act of forbidden power leaves a stain that cannot be washed away."*

---

## Document Control

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-14 | Initial specification (extracted from trauma-economy.md) |

---

## 1. Overview

### 1.1 Identity Table

| Property | Value |
|----------|-------|
| **Resource ID** | `CORRUPTION` |
| **Type** | Trauma Currency (Permanent) |
| **Range** | 0–100 |
| **Recovery** | Near-impossible (by design) |
| **Availability** | Universal (all characters can be corrupted) |
| **Primary Source** | Heretical abilities, Mystic magic, Blighted artifacts |

### 1.2 Core Philosophy

Runic Blight Corruption represents the insidious, near-permanent spiritual contamination from exposure to the Great Silence's reality-warping influence. Unlike Psychic Stress (which recovers with rest), Corruption accumulates over a character's lifetime and serves as the ultimate limiter on heretical power usage.

**Design Intent:**
- Corruption is the **permanent** trauma currency
- Recovery is intentionally punishing—barely worth pursuing
- Creates long-term strategic decisions, not tactical ones
- Heretical builds trade Corruption for power, knowing the cost
- Terminal Error (100 Corruption) = character permanently lost

### 1.3 Thematic Identity

| Layer | Interpretation |
|-------|----------------|
| L1 (Mythic) | The Blight's mark upon the soul; the price of touching forbidden things |
| L2 (Diagnostic) | Bio-Aetheric contamination; reality-pattern degradation in biological systems |
| L3 (Technical) | Accumulated glitch-state corruption in character data; permanent debuff stack |

---

## 2. Corruption Range & States

### 2.1 Corruption Thresholds

| Value | State | Effects |
|-------|-------|---------|
| 0–20 | **Clean** | No effects; character is uncorrupted |
| 21–40 | **Tainted** | Occasional reality glitches; +1 Tech checks, −1 Social checks |
| 41–60 | **Infected** | Frequent glitches; +2 Tech, −2 Social; cannot gain human faction reputation |
| 61–80 | **Corrupted** | Constant glitches; Wild Magic (Mystics); acquire [MACHINE AFFINITY] trauma |
| 81–99 | **Terminal** | Extreme transformation; NPCs fear character; countdown to loss |
| 100 | **Terminal Error** | Character lost to Blight (unplayable) |

### 2.2 Threshold Triggers

Threshold effects trigger **once** when first crossed:

| Threshold | One-Time Effect |
|-----------|-----------------|
| **25** | Display warning: "You feel the Blight's touch..." |
| **50** | Lock out human faction reputation gains |
| **75** | Acquire `[MACHINE AFFINITY]` permanent trauma |
| **100** | Trigger Terminal Error sequence |

### 2.3 Resource Pool Penalties

Corruption reduces maximum HP and AP:

```
Max HP Penalty = floor(Corruption / 10) × 5%
Max AP Penalty = floor(Corruption / 10) × 5%
```

| Corruption | Max HP/AP Reduction |
|------------|---------------------|
| 0–9 | 0% |
| 10–19 | 5% |
| 20–29 | 10% |
| 30–39 | 15% |
| 40–49 | 20% |
| 50–59 | 25% |
| 60–69 | 30% |
| 70–79 | 35% |
| 80–89 | 40% |
| 90–99 | 45% |
| 100 | Character Lost |

> [!IMPORTANT]
> Stamina is **NOT** penalized by Corruption to prevent unwinnable combat states.

---

## 3. Corruption Sources

### 3.1 Mystic Magic

| Source | Corruption | Notes |
|--------|------------|-------|
| Standard spell | 0–2 | Low risk |
| Powerful spell | 3–5 | Moderate risk |
| Overcasting | 10+ | Emergency only |
| Failed Focus ability | 5 | Random risk |

### 3.2 Heretical Abilities

| Source | Corruption | Notes |
|--------|------------|-------|
| Berserkr rage abilities | 2–5 | Per use |
| Blót-Priest HP-casting | 1 per HP spent | Sacrificial Casting |
| Blót-Priest life siphon | 1 per siphon | Corrupted harvest |
| Blót-Priest Blight Transfer | Transfers to ally | Spreads corruption |
| Seiðkona divination | 3–8 | Forbidden knowledge |
| Rust-Witch entropy | 2–6 | Decay magic |

### 3.3 Environmental & Item Sources

| Source | Corruption | Notes |
|--------|------------|-------|
| Glitched artifact use | 1–5 | Per use |
| Blight zone exposure | 1–3 | Per extended exposure |
| Corrupted consumable | 2–5 | Per consumption |
| Forbidden ritual | 5–15 | Story-driven |
| Contact with Forlorn | 5–10 | Direct exposure |

### 3.4 Blót-Priest Specific Sources

The Blót-Priest is the most Corruption-intensive specialization:

| Action | Corruption Cost |
|--------|-----------------|
| Every HP-cast (Sacrificial Casting) | +1 per HP spent |
| Every Life Siphon | +1 |
| Gift of Vitae | Transfers 1 to ally |
| Exsanguinate (full duration) | +3 total |
| Heartstopper: Crimson Deluge | +10 self + 5 to each ally |
| Heartstopper: Final Anathema | +15 after transfer |

---

## 4. Corruption Recovery

> [!WARNING]
> Corruption recovery is **intentionally near-impossible** by design.

### 4.1 Recovery Methods

| Method | Recovery | Availability |
|--------|----------|--------------|
| Purification Ritual | 1–3 points | Once per character lifetime |
| Legendary Item | 5 points | Saga Quest reward only |
| Story Event | Variable | GM discretion |
| Divine Intervention | Variable | Exceptional circumstances |

### 4.2 Design Rationale

Recovery is intentionally punishing because:
1. Corruption is the **long-term cost** of heretical power
2. Players must make permanent strategic choices
3. The threat of Terminal Error creates meaningful stakes
4. "Cleansing" should feel miraculous, not routine

---

## 5. Corruption → Stress Interaction

### 5.1 Resolve Check Penalty

High Corruption weakens Stress resistance:

```
Resolve Check Penalty = −1 die per 20 Corruption
```

| Corruption | Resolve Dice Penalty |
|------------|----------------------|
| 0–19 | 0 |
| 20–39 | −1 die |
| 40–59 | −2 dice |
| 60–79 | −3 dice |
| 80–99 | −4 dice |

### 5.2 The Compounding Death Spiral

```
Heretical Power → Corruption → Weaker Resolve → More Stress → Breaking Point → Trauma → More Stress
```

This feedback loop is **intentional design**—it punishes overreliance on forbidden power.

---

## 6. Terminal Error

### 6.1 Trigger Condition

When Corruption reaches **exactly 100**:
1. Terminal Error triggers immediately
2. Transformation Check determines final outcome
3. Character is permanently unplayable

### 6.2 Transformation Check

```
Transformation Check: WILL vs DC 8 - (Corruption over 100 / 5)
```

**On Failure:** Character becomes **Forlorn**
- Permanently unplayable
- May become enemy in future runs (roguelike mechanic)
- Character data archived, not deleted

**On Success:** Character survives at Corruption 99
- Cannot use any Corruption-generating abilities
- One more Corruption point = automatic Terminal Error
- Effectively "on borrowed time"

---

## 7. Blight Transference (Blót-Priest Unique)

### 7.1 Mechanic Overview

The Blót-Priest can **transfer** Corruption to allies through healing:

```
OnHealAlly(Amount, CorruptionTransfer):
    Ally.HP += Amount
    Ally.Corruption += CorruptionTransfer
    Caster.Corruption -= CorruptionTransfer (if applicable)
```

### 7.2 Moral Implications

Blight Transference creates unique ethical gameplay:
- Save ally now, corrupt them later?
- Spread Corruption evenly across party?
- Let someone die to avoid spreading Corruption?
- Use Heartstopper knowing it corrupts everyone?

No other specialization forces players to harm allies to help them.

---

## 8. UI Display

### 8.1 Corruption Bar

```
┌─────────────────────────────────────────┐
│  RUNIC BLIGHT CORRUPTION                │
│  ⌘: ██████████████████░░  72/100        │
│     [CORRUPTED] Wild Magic Active       │
│     Threshold 75 in 3 points            │
│                                         │
│  Max HP: -35%  |  Max AP: -35%          │
│  Tech: +2      |  Social: -2            │
│  Resolve: -3 dice                       │
└─────────────────────────────────────────┘
```

### 8.2 Color Coding

| Range | Color | Visual Effect |
|-------|-------|---------------|
| 0–20 | Gray | Clean |
| 21–40 | Yellow | Slight glow |
| 41–60 | Orange | Pulsing |
| 61–80 | Red | Intense pulse |
| 81–99 | Dark Red | Flickering, unstable |
| 100 | Black/Void | Terminal Error modal |

### 8.3 Threshold Warning

```
┌─────────────────────────────────────────┐
│  ⚠ CORRUPTION THRESHOLD APPROACHING    │
│                                         │
│  Current: 73/100                        │
│  Next Threshold: 75 (in 2 points)       │
│                                         │
│  At 75: Acquire [MACHINE AFFINITY]      │
│         NPCs will fear you              │
└─────────────────────────────────────────┘
```

### 8.4 Terminal Error Modal

```
╔═══════════════════════════════════════════════════════════╗
║            ▓▓▓ TERMINAL ERROR ▓▓▓                         ║
║                                                           ║
║  The Blight has consumed you completely.                  ║
║  Your form destabilizes. Reality rejects you.             ║
║                                                           ║
║  Rolling WILL (4 dice) vs DC 8...                        ║
║  [████░░░░░░] 1 success — FAILURE                        ║
║                                                           ║
║  ┌─────────────────────────────────────────────────────┐  ║
║  │  [CHARACTER NAME] HAS BECOME FORLORN               │  ║
║  │                                                     │  ║
║  │  "What was once a person is now an echo of the     │  ║
║  │   Great Silence—a glitching horror that wears      │  ║
║  │   a familiar face."                                │  ║
║  │                                                     │  ║
║  │  This character is permanently lost.               │  ║
║  │  They may appear as an enemy in future runs.       │  ║
║  └─────────────────────────────────────────────────────┘  ║
║                                                           ║
║                    [Accept Fate]                          ║
╚═══════════════════════════════════════════════════════════╝
```

---

## 9. Technical Implementation

### 9.1 Data Model

```csharp
public class CorruptionState
{
    public int CurrentCorruption { get; set; } = 0;
    public bool Threshold25Triggered { get; set; } = false;
    public bool Threshold50Triggered { get; set; } = false;
    public bool Threshold75Triggered { get; set; } = false;
    public bool IsTerminal => CurrentCorruption >= 100;

    public string GetCorruptionState()
    {
        return CurrentCorruption switch
        {
            <= 20 => "Clean",
            <= 40 => "Tainted",
            <= 60 => "Infected",
            <= 80 => "Corrupted",
            <= 99 => "Terminal",
            _ => "Terminal Error"
        };
    }

    public int GetMaxHpPenaltyPercent() => (CurrentCorruption / 10) * 5;
    public int GetMaxApPenaltyPercent() => (CurrentCorruption / 10) * 5;
    public int GetResolveDicePenalty() => CurrentCorruption / 20;
}
```

### 9.2 Service Interface

```csharp
public interface ICorruptionService
{
    // Modification
    CorruptionResult AddCorruption(Character character, int amount, CorruptionSource source);
    CorruptionResult TransferCorruption(Character from, Character to, int amount);
    CorruptionResult RemoveCorruption(Character character, int amount, string reason);

    // Queries
    int GetMaxHpPenalty(Character character);
    int GetMaxApPenalty(Character character);
    int GetResolvePenalty(Character character);
    string GetCorruptionState(Character character);

    // Checks
    bool CheckThresholdCrossed(Character character, int oldValue);
    TerminalErrorResult TriggerTerminalError(Character character);
}

public record CorruptionResult(
    int CorruptionGained,
    int NewTotal,
    bool ThresholdCrossed,
    string? ThresholdEffect
);

public record TerminalErrorResult(
    bool Survived,
    int FinalCorruption,
    bool BecameForlorn
);
```

### 9.3 Database Schema

```sql
-- Corruption state (extends character_trauma_state)
ALTER TABLE character_trauma_state ADD COLUMN IF NOT EXISTS
    corruption_source_log JSONB DEFAULT '[]';

-- Corruption history for analytics
CREATE TABLE corruption_history (
    id SERIAL PRIMARY KEY,
    character_id UUID NOT NULL REFERENCES characters(id) ON DELETE CASCADE,
    amount INT NOT NULL,
    source VARCHAR(100) NOT NULL,
    new_total INT NOT NULL,
    threshold_crossed INT,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_corruption_history_char ON corruption_history(character_id);
```

---

## 10. Phased Implementation Guide

### Phase 1: Core Logic
- [ ] Implement `CorruptionState` class with computed properties
- [ ] Implement threshold state tracking
- [ ] Implement penalty calculations (HP, AP, Resolve)

### Phase 2: Modification Logic
- [ ] Implement `AddCorruption` with threshold checking
- [ ] Implement `TransferCorruption` for Blót-Priest
- [ ] Implement `RemoveCorruption` with rarity constraints

### Phase 3: Terminal Error
- [ ] Implement Terminal Error trigger
- [ ] Implement Transformation Check
- [ ] Implement Forlorn conversion

### Phase 4: UI Integration
- [ ] Implement Corruption bar with state colors
- [ ] Implement threshold warning system
- [ ] Implement Terminal Error modal

---

## 11. Testing Requirements

### 11.1 Unit Tests
- [ ] Verify Corruption cannot exceed 100
- [ ] Verify threshold triggers fire exactly once
- [ ] Verify HP/AP penalty calculations
- [ ] Verify Resolve dice penalty calculations
- [ ] Verify state string returns correct values

### 11.2 Integration Tests
- [ ] Blót-Priest HP-cast → Corruption gain
- [ ] Blót-Priest Blight Transfer → Ally Corruption gain
- [ ] Corruption 100 → Terminal Error sequence
- [ ] Persistence: Save with Corruption → Load → Verify state

### 11.3 Manual QA
- [ ] Verify Corruption bar visuals at each state
- [ ] Verify threshold warnings display correctly
- [ ] Verify Terminal Error modal flow

---

## 12. Logging Requirements

### 12.1 Log Events

| Event | Level | Message Template |
|-------|-------|------------------|
| Corruption Gain | Information | "{Character} corrupted by {Amount} (Total: {Total}, Source: {Source})" |
| Threshold Crossed | Warning | "{Character} crossed corruption threshold {Threshold}!" |
| Blight Transfer | Information | "{Character} transferred {Amount} corruption to {Target}" |
| Terminal Error | Fatal | "TERMINAL ERROR: {Character} lost to corruption" |

---

## 13. Related Specifications

| Spec ID | Relationship |
|---------|--------------|
| `SPEC-CORE-TRAUMA` | Parent trauma economy system |
| `SPEC-CORE-RES-HP` | HP penalty integration |
| `SPEC-CORE-RES-AETHER` | AP penalty integration |
| `SPEC-CORE-RES-STRESS` | Resolve penalty integration |
| `SPEC-BLOT-PRIEST` | Primary Corruption user |

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-14 | Initial specification (extracted from trauma-economy.md) |
