# Rust-Witch Specialization Specification

Parent item: Specs: Specializations (Specs%20Specializations%202ba55eb312da8022a82bc3d0883e1d26.md)

> Version: 1.0
Last Updated: 2025-11-27
Status: Draft
Specialization ID: RustWitch (enum value)
Archetype: Mystic (ArchetypeID: 5)
> 

---

## Document Control

### Version History

| Version | Date | Author | Changes |
| --- | --- | --- | --- |
| 1.0 | 2025-11-27 | AI | Initial comprehensive specification |

---

## Executive Summary

### Specialization Identity

**Name**: Rust-Witch
**Tagline**: "Corrosion & Entropy Magic"
**Role**: Heretical Debuffer / DoT Specialist
**Path Type**: Heretical
**Icon**: (Suggested: Rust/decay symbol)

**Description**:

> You have learned to embrace the world's decay as a weapon. Where others see corruption as a threat, you see it as a tool—a fundamental force that can be directed against your enemies. Your curses rot metal, corrode flesh, and accelerate entropy itself.
> 
> 
> **WARNING**: All active abilities inflict self-Corruption. Using this specialization is a bargain with decay—you trade your own purity for devastating power over your enemies.
> 

### Core Mechanics

| Attribute | Value |
| --- | --- |
| **Primary Attribute** | WILL |
| **Secondary Attribute** | WITS |
| **Resource System** | Aether Pool (AP) |
| **Trauma Risk** | Very High (self-Corruption) |
| **Unlock Cost** | 3 PP |
| **Unlock Requirements** | Mystic Archetype |

### Unique Systems

1. **[Corroded] Stacking**: Core damage-over-time mechanic that stacks up to 5 times
2. **Self-Corruption**: ALL active abilities inflict Corruption on the user
3. **Entropy Synergies**: Multiple abilities amplify [Corroded] effectiveness
4. **Execution Threshold**: Capstone can instantly kill targets with high Corruption/stacks
5. **Cascade Spreading**: Death of corroded enemies spreads the effect

---

## Rank Progression System

### How Ranks Work (Tree-Based Progression)

Ranks are **NOT** purchased with PP. Instead, ranks advance automatically based on tree investment:

| Trigger | Effect |
| --- | --- |
| **Specialization Unlocked** | All Tier 1 abilities start at Rank 1 |
| **2 Tier 2 Abilities Trained** | All Tier 1 abilities advance to Rank 2 |
| **Capstone Trained** | All Tier 1 and Tier 2 abilities advance to Rank 3 |

### Rank Availability by Tier

| Tier | Starting Rank | Maximum Rank | Rank Progression |
| --- | --- | --- | --- |
| **Tier 1** | Rank 1 | Rank 3 | 1 → 2 (2 Tier 2s) → 3 (Capstone) |
| **Tier 2** | Rank 2 | Rank 3 | 2 → 3 (Capstone) |
| **Tier 3** | No Ranks | No Ranks | Single power level |
| **Capstone** | No Ranks | No Ranks | Single power level (triggers Rank 3) |

### Visual Rank Indicators (GUI)

| Rank | Border Color | Hex Code | Glow Effect |
| --- | --- | --- | --- |
| **Rank 1** | Bronze | #CD7F32 | None |
| **Rank 2** | Silver | #C0C0C0 | Subtle shimmer |
| **Rank 3** | Gold | #FFD700 | Prominent glow |

---

## Ability Summary

### Quick Reference Table

| ID | Name | Tier | Type | AP Cost | Self-Corruption | Key Effect |
| --- | --- | --- | --- | --- | --- | --- |
| - | Philosopher of Dust | 1 | Passive | - | - | +1d analysis vs corrupted |
| - | Corrosive Curse | 1 | Active | 20 | +2 | Apply 1 [Corroded] stack |
| - | Entropic Field | 1 | Passive | - | - | Enemies in row: -1 Armor |
| - | System Shock | 2 | Active | 25 | +3 | 2 [Corroded] + [Stunned] on Mechanical |
| - | Flash Rust | 2 | Active | 35 | +4 | 2 [Corroded] to ALL enemies |
| - | Accelerated Entropy | 2 | Passive | - | - | [Corroded] damage: 2d6/stack |
| - | Unmaking Word | 3 | Active | 30 | +4 | DOUBLE [Corroded] stacks (max 5) |
| - | Cascade Reaction | 3 | Passive | - | - | Spread [Corroded] on death |
| - | Entropic Cascade | 4 | Active | 50 | +6 | Execute OR 6d6 Arcane damage |

### PP Cost Summary

| Tier | Abilities | PP Each | Total |
| --- | --- | --- | --- |
| Tier 1 | 3 | 3 PP | 9 PP |
| Tier 2 | 3 | 4 PP | 12 PP |
| Tier 3 | 2 | 5 PP | 10 PP |
| Capstone | 1 | 6 PP | 6 PP |
| **TOTAL** | **9** | - | **37 PP** |

*Note: 3 PP to unlock specialization + 37 PP for all abilities = 40 PP total investment*

---

## Tier 1 Abilities (Ranks 1-3)

### Ability: Philosopher of Dust

**Type**: Passive
**Action**: Free Action
**Target**: Self

**Description**:

> You understand entropy intimately. The decay that others fear, you have studied and embraced.
> 

### Rank Progression

| Rank | Analysis Bonus | Special |
| --- | --- | --- |
| **Rank 1** | +1d10 | vs corrupted targets only |
| **Rank 2** | +2d10 | vs corrupted targets + identify Corruption % |
| **Rank 3** | +3d10 | vs corrupted targets + identify weaknesses |

### Detailed Mechanics

**Rank 1** (Unlock → 2 Tier 2s trained):

- Passive bonus of +1d10 to all analysis/identification checks against targets with Corruption > 0 or [Corroded] status

**Rank 2** (2 Tier 2s trained → Capstone):

- Bonus increases to +2d10
- Analysis reveals target's Corruption percentage

**Rank 3** (Capstone trained):

- Bonus increases to +3d10
- Analysis reveals target's vulnerabilities to entropy effects
- Identifies if target is near execution threshold

### GUI Display Requirements

**Passive Indicator**:

```
┌─────────────────────────────────────┐
│ 📜 PHILOSOPHER OF DUST      [RANK] │
│ ─────────────────────────────────── │
│ Type: Passive                       │
│ Attribute: WILL                     │
│ ─────────────────────────────────── │
│ Analysis Bonus: +Xd10               │
│ (vs corrupted targets)              │
│ [Rank 2+: Shows Corruption %]       │
└─────────────────────────────────────┘

```

---

### Ability: Corrosive Curse

**Type**: Active
**Action**: Standard Action
**Target**: Single Enemy
**Resource Cost**: 20 AP
**Self-Corruption**: +2

**Description**:

> You speak words of dissolution, cursing your target with accelerated decay. Metal rusts, flesh rots, and systems fail.
> 

### Rank Progression

| Rank | [Corroded] Stacks | Duration | Self-Corruption |
| --- | --- | --- | --- |
| **Rank 1** | 1 stack | Permanent | +2 |
| **Rank 2** | 2 stacks | Permanent | +2 |
| **Rank 3** | 2 stacks | Permanent | +1 (reduced) |

### Detailed Mechanics

**Rank 1** (Unlock → 2 Tier 2s trained):

- WILL-based attack vs target's Resolve
- Apply 1 stack of [Corroded] on hit
- [Corroded] is permanent until cleansed
- You gain +2 Corruption

**Rank 2** (2 Tier 2s trained → Capstone):

- Apply 2 stacks instead of 1
- Same self-Corruption cost (+2)

**Rank 3** (Capstone trained):

- Apply 2 stacks
- Self-Corruption reduced to +1

### Formula

```
Attack Roll = WILL + Bonus Dice vs Target Resolve
Success Threshold = 2 (need 2+ successes)

On Success:
  - Apply [Corroded] stacks (1 at Rank 1, 2 at Rank 2-3)
  - Caster gains Corruption (2 at Rank 1-2, 1 at Rank 3)

[Corroded] Effect (per stack):
  - 1d4 damage at end of turn (or 2d6 if Accelerated Entropy)
  - -1 Armor per stack
  - Permanent duration (must cleanse)

```

### GUI Display Requirements

**Ability Button**:

```
┌────────────────────────────────────────┐
│ 🦠 CORROSIVE CURSE            [RANK] │
│ ────────────────────────────────────── │
│ AP Cost: 20                            │
│ Self-Corruption: +{2/2/1}              │
│ ────────────────────────────────────── │
│ Apply {1/2/2} [Corroded] stack(s)      │
│ • 1d4 damage/turn (end of turn)        │
│ • -1 Armor per stack                   │
│ • Permanent (requires cleanse)         │
│ ────────────────────────────────────── │
│ ⚠️ You gain Corruption on use         │
└────────────────────────────────────────┘

```

---

### Ability: Entropic Field

**Type**: Passive
**Action**: Free Action (aura)
**Target**: All enemies in your row

**Description**:

> Your presence accelerates decay. Metal weakens, seals fail, and armor crumbles simply by standing near you.
> 

### Rank Progression

| Rank | Armor Reduction | Range | Special |
| --- | --- | --- | --- |
| **Rank 1** | -1 Armor | Same row | - |
| **Rank 2** | -2 Armor | Same row | - |
| **Rank 3** | -2 Armor | Same row + adjacent | -1 additional if [Corroded] |

### Detailed Mechanics

**Rank 1** (Unlock → 2 Tier 2s trained):

- All enemies in the same row as you suffer -1 Armor
- Effect is automatic while you're alive
- Does not stack with multiple Rust-Witches

**Rank 2** (2 Tier 2s trained → Capstone):

- Armor reduction increases to -2

**Rank 3** (Capstone trained):

- Armor reduction remains -2
- Extends to adjacent rows
- Enemies with [Corroded] suffer additional -1 Armor from your presence

### GUI Display Requirements

**Aura Indicator**:

- Visual decay/rust effect around character's row
- Enemy armor values shown with reduction in red

---

## Tier 2 Abilities (Ranks 2-3)

*Note: Tier 2 abilities start at Rank 2 when trained and advance to Rank 3 when Capstone is trained.*

### Ability: System Shock

**Type**: Active
**Action**: Standard Action
**Target**: Single Enemy
**Resource Cost**: 25 AP
**Self-Corruption**: +3

**Description**:

> You overload the target's systems with corruptive energy. Living tissue seizes, mechanical systems fail, and the corrupted mind locks up.
> 

### Rank Progression

| Rank | [Corroded] Stacks | [Stunned] Duration | Special |
| --- | --- | --- | --- |
| **Rank 2** | 2 stacks | 1 turn (Mechanical only) | - |
| **Rank 3** | 3 stacks | 1 turn (any target with 3+ [Corroded]) | Self-Corruption reduced to +2 |

### Detailed Mechanics

**Rank 2** (When trained):

- WILL-based attack (Success Threshold: 2)
- Apply 2 stacks of [Corroded]
- If target has [Mechanical] tag: apply [Stunned] for 1 turn
- You gain +3 Corruption

**Rank 3** (Capstone trained):

- Apply 3 stacks instead of 2
- [Stunned] now triggers on ANY target that has 3+ total [Corroded] stacks (including these new stacks)
- Self-Corruption reduced to +2

### Anti-Mechanical Synergy

```
System Shock is particularly effective against:
- Automatons
- Corrupted Constructs
- Mechanical enemies
- Cyborg-type enemies

These enemies are especially vulnerable to entropy effects,
and System Shock's [Stunned] proc is guaranteed against them.

```

### GUI Display Requirements

**Ability Card**:

```
┌────────────────────────────────────────┐
│ ⚡ SYSTEM SHOCK               [RANK] │
│ ────────────────────────────────────── │
│ AP Cost: 25                            │
│ Self-Corruption: +{3/2}                │
│ ────────────────────────────────────── │
│ Apply {2/3} [Corroded] stacks          │
│                                        │
│ [Stunned] Trigger:                     │
│ • Rank 2: Mechanical targets (auto)   │
│ • Rank 3: Any target with 3+ stacks   │
│ ────────────────────────────────────── │
│ ⚠️ You gain Corruption on use         │
└────────────────────────────────────────┘

```

---

### Ability: Flash Rust

**Type**: Active
**Action**: Standard Action
**Target**: ALL Enemies
**Resource Cost**: 35 AP
**Self-Corruption**: +4

**Description**:

> You release an instantaneous entropy cascade, causing everything metal or organic in the area to begin decaying simultaneously.
> 

### Rank Progression

| Rank | [Corroded] Stacks | Target Count | Special |
| --- | --- | --- | --- |
| **Rank 2** | 2 stacks | All enemies | - |
| **Rank 3** | 2 stacks | All enemies | +1 stack to [Mechanical], Self-Corruption reduced to +3 |

### Detailed Mechanics

**Rank 2** (When trained):

- No attack roll required (automatic hit)
- Apply 2 stacks of [Corroded] to ALL enemies in combat
- You gain +4 Corruption

**Rank 3** (Capstone trained):

- Apply 2 stacks to all enemies
- [Mechanical] enemies receive +1 additional stack (3 total)
- Self-Corruption reduced to +3

### AoE Efficiency

```
Flash Rust Efficiency Analysis:
- 3 enemies: 6 total [Corroded] stacks applied
- 5 enemies: 10 total [Corroded] stacks applied
- 8 enemies: 16 total [Corroded] stacks applied

Value increases dramatically with enemy count.
Combined with Cascade Reaction, creates chain-reaction potential.

```

### GUI Display Requirements

**AoE Indicator**:

- Show all enemies highlighted as targets
- Display total stack count that will be applied

---

### Ability: Accelerated Entropy

**Type**: Passive
**Action**: Free Action
**Target**: Self (affects all [Corroded] effects)

**Description**:

> Your curses are particularly potent. The corrosion you inflict eats through material faster than normal decay.
> 

### Rank Progression

| Rank | [Corroded] Damage | Special |
| --- | --- | --- |
| **Rank 2** | 2d6 per stack | Doubled from base 1d4 |
| **Rank 3** | 2d6 per stack | Also ignores 1 Soak per stack |

### Detailed Mechanics

**Rank 2** (When trained):

- ALL [Corroded] effects you inflict deal 2d6 damage per stack instead of 1d4
- This is a massive damage increase (avg 7 vs avg 2.5 per stack)

**Rank 3** (Capstone trained):

- [Corroded] damage remains 2d6 per stack
- Additionally, [Corroded] ignores 1 Soak per stack
- At 5 stacks, your [Corroded] effects ignore 5 Soak completely

### Damage Comparison

```
Without Accelerated Entropy (Base [Corroded]):
  - 1 stack: 1d4 damage = avg 2.5
  - 5 stacks: 5d4 damage = avg 12.5

With Accelerated Entropy (Rank 2):
  - 1 stack: 2d6 damage = avg 7
  - 5 stacks: 10d6 damage = avg 35

With Accelerated Entropy (Rank 3):
  - Same damage + ignores 5 Soak at max stacks
  - Effectively +5-10 damage against armored targets

```

### GUI Display Requirements

**Passive Indicator**:

```
┌─────────────────────────────────────┐
│ 💀 ACCELERATED ENTROPY      [RANK] │
│ ─────────────────────────────────── │
│ Type: Passive                       │
│ ─────────────────────────────────── │
│ Your [Corroded] deals 2d6/stack     │
│ (base: 1d4/stack)                   │
│ [Rank 3: Ignores 1 Soak per stack] │
└─────────────────────────────────────┘

```

---

## Tier 3 Abilities (No Ranks)

*Tier 3 abilities do not have ranks. They have a single power level that does not change.*

### Ability: Unmaking Word

**Type**: Active
**Action**: Standard Action
**Target**: Single Enemy with [Corroded]
**Resource Cost**: 30 AP
**Self-Corruption**: +4

**Description**:

> You speak a word that should not be spoken—a syllable of pure dissolution that accelerates entropy to catastrophic levels.
> 

### Effects

| Effect | Value |
| --- | --- |
| **Stack Multiplication** | Current stacks × 2 |
| **Maximum Stacks** | 5 (hard cap) |
| **Requirement** | Target must have [Corroded] |
| **Self-Corruption** | +4 |

### Detailed Mechanics

- Target must already have at least 1 stack of [Corroded]
- DOUBLE the current [Corroded] stacks (e.g., 2 → 4, 3 → 5)
- Maximum 5 stacks (doubling from 3+ caps at 5)
- You gain +4 Corruption

### Strategic Timing

```
Optimal Unmaking Word Timing:
  - 1 stack → 2 stacks (inefficient, wastes potential)
  - 2 stacks → 4 stacks (good value)
  - 3+ stacks → 5 stacks (maximum value)

Best combo: Corrosive Curse (2) + System Shock (3) = 5 stacks
Then Unmaking Word would be wasted (already at cap).

Better combo: Corrosive Curse (2) → Unmaking Word (4) = 4 stacks
Then System Shock (3 more) → cap at 5 with overflow.

```

### GUI Display Requirements

**Ability Card**:

```
┌────────────────────────────────────────┐
│ 📖 UNMAKING WORD                       │
│ ────────────────────────────────────── │
│ AP Cost: 30                            │
│ Self-Corruption: +4                    │
│ Requires: Target has [Corroded]        │
│ ────────────────────────────────────── │
│ DOUBLE current [Corroded] stacks       │
│ Maximum: 5 stacks                      │
│                                        │
│ Current target stacks: [X] → [2X]      │
│ ────────────────────────────────────── │
│ ⚠️ You gain Corruption on use         │
└────────────────────────────────────────┘

```

---

### Ability: Cascade Reaction

**Type**: Passive
**Action**: Free Action (triggered)
**Target**: Self (affects enemies on death)
**Trigger**: Enemy with [Corroded] dies

**Description**:

> Entropy is contagious. When a corroded target finally succumbs, the decay spreads to those nearby like a virulent plague.
> 

### Effects

| Effect | Value |
| --- | --- |
| **Trigger** | Enemy with [Corroded] dies |
| **Spread Stacks** | 1 stack per adjacent enemy |
| **Range** | Adjacent enemies (same row + adjacent rows) |
| **Stack Limit** | Still caps at 5 per target |

### Detailed Mechanics

- When ANY enemy with [Corroded] dies (regardless of cause)
- All adjacent enemies receive 1 stack of [Corroded]
- "Adjacent" = same row + enemies in adjacent rows
- Can trigger chain reactions if spread kills another corroded enemy
- Does NOT inflict self-Corruption (passive trigger)

### Chain Reaction Potential

```
Chain Reaction Scenario:
1. Enemy A (5 stacks) dies
2. Enemies B, C, D (adjacent) each gain 1 stack
3. Enemy B (was at 4 stacks) now at 5 stacks
4. End of turn: Enemy B dies from [Corroded] damage
5. Enemies C, D, E (adjacent to B) each gain 1 stack
6. Continue until no more deaths

This passive transforms Flash Rust into a potential cascade bomb.

```

### GUI Display Requirements

**Death Trigger Effect**:

- Visual corruption wave spreading from dying enemy
- Stack increase indicators on affected enemies

---

## Capstone Ability (No Ranks)

### Ability: Entropic Cascade

**Type**: Active
**Action**: Standard Action
**Target**: Single Enemy
**Resource Cost**: 50 AP
**Self-Corruption**: +6

**Description**:

> You channel the full force of entropy through your target. If their body or systems are sufficiently compromised, they simply... stop. Otherwise, the wave of dissolution deals catastrophic damage.
> 

### Effects

| Effect | Value |
| --- | --- |
| **Execution Threshold** | Target Corruption > 50% OR 5 [Corroded] stacks |
| **Execution Effect** | Instantly reduce to 0 HP |
| **Non-Execute Damage** | 6d6 Arcane damage |
| **Self-Corruption** | +6 |
| **Cooldown** | Once per combat |

### Detailed Mechanics

**Execution Condition Check**:

1. Check if target has Corruption > 50
2. OR check if target has 5 stacks of [Corroded]
3. If EITHER is true: Execute (0 HP instantly)
4. If NEITHER is true: Deal 6d6 Arcane damage

**Self-Corruption**:

- Regardless of outcome, you gain +6 Corruption
- This is the highest self-Corruption cost in the specialization

### Execution Setup

```
Reliable Execution Setup:
1. Flash Rust: 2 stacks to all enemies
2. Target single enemy with System Shock: +3 stacks = 5 total
3. Entropic Cascade: EXECUTE (target has 5 stacks)

Alternative (Corruption-based):
- Rust-Witch abilities don't directly inflict target Corruption
- Must rely on other sources or naturally high-Corruption enemies
- [Corroded] stack method is more reliable

Damage Comparison:
- 6d6 Arcane = avg 21 damage (non-execute)
- Execute = ~50-100+ HP bypassed (massive value)

```

### GUI Display Requirements

**Capstone Ability Card**:

```
┌────────────────────────────────────────────┐
│ 💀 ENTROPIC CASCADE              [CAPSTONE] │
│ ────────────────────────────────────────── │
│ AP Cost: 50                                │
│ Self-Corruption: +6                        │
│ Cooldown: Once per combat                  │
│ ────────────────────────────────────────── │
│ EXECUTION CHECK:                           │
│ • Target Corruption > 50%    [✓/✗]         │
│ • Target [Corroded] = 5      [✓/✗]         │
│ ────────────────────────────────────────── │
│ If EITHER: Instant kill (0 HP)             │
│ If NEITHER: 6d6 Arcane damage              │
│ ────────────────────────────────────────── │
│ ⚠️ HIGH CORRUPTION COST                   │
└────────────────────────────────────────────┘

```

**Target Indicator**:

- Show execution threshold status on mouseover
- Green checkmark if execution will trigger
- Red X if standard damage will apply

---

## Status Effect Definitions

### [Corroded]

| Property | Value |
| --- | --- |
| **Category** | Damage Over Time |
| **Stackable** | Yes (max 5) |
| **Duration** | Permanent (-1) |
| **Base Damage** | 1d4 per stack (2d6 with Accelerated Entropy) |
| **Armor Penalty** | -1 per stack |
| **Timing** | End of turn |
| **Ignores Soak** | No (unless Accelerated Entropy Rank 3) |

**Detailed [Corroded] Mechanics**:

```
[Corroded] Status Effect:
- Stacks up to 5 times
- Each stack:
  - Deals 1d4 damage at end of turn (2d6 if Accelerated Entropy)
  - Reduces Armor by 1
- Permanent duration (DurationRemaining = -1)
- Must be cleansed to remove
- Damage processed at END of turn (after actions)

Armor Reduction:
- 1 stack: -1 Armor
- 3 stacks: -3 Armor
- 5 stacks: -5 Armor (most targets have 0-5 base Armor)

At 5 stacks with Accelerated Entropy:
- Damage: 10d6 = avg 35 damage per turn
- Armor: -5 (likely 0 effective Armor)
- Soak ignored: 5 (if Rank 3)

```

**Interaction with Bleeding**:

```
StatusInteraction: Bleeding + Corroded → ×1.5 damage

If target has both [Bleeding] and [Corroded]:
- Bleeding damage is amplified by 50%
- Example: 3d6 Bleeding (10.5 avg) → 15.75 damage

This makes Rust-Witch excellent paired with Bleeding-focused allies.

```

**Visual Indicator**:

- Rust/decay particles on affected enemy
- Stack count visible
- Armor reduction shown in red

---

## Self-Corruption System

### Corruption Costs by Ability

| Ability | Base Cost | Rank 3 Cost | Cumulative Risk |
| --- | --- | --- | --- |
| Corrosive Curse | +2 | +1 | Low |
| System Shock | +3 | +2 | Medium |
| Flash Rust | +4 | +3 | Medium-High |
| Unmaking Word | +4 | +4 | Medium-High |
| Entropic Cascade | +6 | +6 | Very High |

### Combat Corruption Accumulation

```
Typical Combat Scenario (5 rounds):
Round 1: Corrosive Curse (+2) = 2 total
Round 2: Flash Rust (+4) = 6 total
Round 3: System Shock (+3) = 9 total
Round 4: Unmaking Word (+4) = 13 total
Round 5: Entropic Cascade (+6) = 19 total

After one combat: +19 Corruption (significant)

Risk Assessment:
- 0-25 Corruption: Safe zone
- 26-50 Corruption: Minor penalties possible
- 51-75 Corruption: Moderate penalties, trauma risk
- 76-100 Corruption: Severe penalties, high trauma risk

```

### GUI Corruption Warning

**Ability Tooltip Warning**:

```
⚠️ CORRUPTION WARNING
This ability inflicts +X Corruption on you.
Current Corruption: [Y]
After Use: [Y+X]
[WARNING: Approaching danger threshold!]

```

---

## Combat Integration

### Turn Flow Integration

```
START OF TURN:
1. Process passive effects (Entropic Field aura)
2. Check Philosopher of Dust analysis bonuses

MAIN ACTION:
3. Select action:
   a. Corrosive Curse (single target stack application)
   b. System Shock (stacks + potential stun)
   c. Flash Rust (AoE stack application)
   d. Unmaking Word (stack doubling)
   e. Entropic Cascade (capstone execution)
   f. Standard attack/ability

END OF TURN:
4. Process [Corroded] damage on all affected enemies
5. Check for enemy deaths
6. If enemy with [Corroded] dies:
   - Trigger Cascade Reaction
   - Spread 1 stack to adjacent enemies
7. Process any chain reaction deaths

```

### Combo Sequences

**Maximum Damage Combo (Single Target)**:

```
Round 1: Corrosive Curse (2 stacks at Rank 2+)
Round 2: System Shock (3 stacks, total 5)
Round 3: Entropic Cascade (EXECUTE - instant kill)
Total AP: 20 + 25 + 50 = 95 AP
Total Self-Corruption: 2 + 3 + 6 = 11

```

**AoE Cascade Combo**:

```
Round 1: Flash Rust (2 stacks to all enemies)
Round 2: Target weakest enemy with System Shock (+3 = 5 stacks)
Round 3: Entropic Cascade on weakest (EXECUTE)
Result: Dead enemy triggers Cascade Reaction
        All adjacent enemies gain +1 stack (now at 3)
Round 4: Another weak enemy may die to [Corroded] damage
        Triggering more cascades

```

---

## Implementation Status

### Current Implementation (v0.19.8)

| Component | Status | Location |
| --- | --- | --- |
| **SpecializationFactory** | ✅ Implemented | `RuneAndRust.Engine/SpecializationFactory.cs:825-982` |
| **Specialization Enum** | ✅ Added | `RuneAndRust.Core/Specialization.cs:23` |
| **[Corroded] Status Effect** | ✅ Implemented | `AdvancedStatusEffectService.cs` |
| **Ability Data** | ✅ Seeded | Via AddRustWitchAbilities() |

### Service Methods

| Method | Description | Status |
| --- | --- | --- |
| `AddRustWitchAbilities()` | Adds all 9 abilities to character | ✅ |
| `ApplyCorrodedEffect()` | Apply [Corroded] stacks | ✅ (in StatusEffectService) |
| `ProcessEndOfTurn()` | Process [Corroded] damage | ✅ |
| `CalculateArmorReduction()` | Calculate stack-based armor penalty | ✅ |

### GUI Integration Gaps

| Feature | Status | Priority |
| --- | --- | --- |
| Self-Corruption warning on ability use | ❌ Not implemented | High |
| [Corroded] stack display on enemies | ❌ Not implemented | High |
| Entropic Field aura visual | ❌ Not implemented | Medium |
| Execution threshold indicator | ❌ Not implemented | High |
| Cascade Reaction death spread animation | ❌ Not implemented | Low |
| Corruption accumulation tracker | ❌ Not implemented | Medium |
| Rank visual indicators | ❌ Not implemented | Medium |

### Data Corrections Needed

| Issue | Current | Should Be | Location |
| --- | --- | --- | --- |
| CostToRank2/3 | 5/10 PP | N/A (tree-based) | SpecializationFactory.cs |
| MaxRank on passives | 1 | Correct (passives don't rank) | SpecializationFactory.cs |
| MaxRank on actives | 3 | Correct but progression is tree-based | SpecializationFactory.cs |

---

## Implementation Priority Roadmap

### Phase 1: Core Corruption Display (High Priority)

1. Implement self-Corruption warning on ability use
2. Implement [Corroded] stack display on enemy health bars
3. Implement Corruption meter/tracker for player

### Phase 2: Execution System (High Priority)

1. Implement execution threshold check in Entropic Cascade
2. Implement visual indicator for execution-ready targets
3. Implement execution animation/effect

### Phase 3: Aura and Spreading (Medium Priority)

1. Implement Entropic Field visual aura
2. Implement Cascade Reaction death trigger
3. Implement spreading visual effect

### Phase 4: Polish (Low Priority)

1. Implement rank visual indicators (Bronze/Silver/Gold)
2. Implement [Corroded] damage numbers at end of turn
3. Add atmospheric audio cues for corruption effects

---

## Appendix A: [Corroded] vs Other DoTs

| DoT Type | Damage/Stack | Duration | Soak | Special |
| --- | --- | --- | --- | --- |
| **[Bleeding]** | 1d6 | 5 turns | Ignores | Start of turn |
| **[Poisoned]** | 1d4 | 4 turns | Applies | Start of turn, -50% healing |
| **[Corroded]** | 1d4 (2d6) | Permanent | Applies | End of turn, -1 Armor/stack |

**Key Differentiators**:

- [Corroded] is the only PERMANENT DoT
- [Corroded] stacks provide armor shred
- [Corroded] + [Bleeding] creates amplification combo
- [Corroded] requires cleansing to remove

---

## Appendix B: Rank Calculation Code

```csharp
public int GetAbilityRank(PlayerCharacter character, AbilityData ability)
{
    var specProgress = GetSpecializationProgress(character, ability.SpecializationID);
    bool hasCapstone = specProgress.UnlockedAbilities.Any(a => a.TierLevel == 4);
    int tier2Count = specProgress.UnlockedAbilities.Count(a => a.TierLevel == 2);

    switch (ability.TierLevel)
    {
        case 1:  // Tier 1: Ranks 1→2→3
            if (hasCapstone) return 3;
            else if (tier2Count >= 2) return 2;
            else return 1;

        case 2:  // Tier 2: Ranks 2→3 (starts at Rank 2)
            if (hasCapstone) return 3;
            else return 2;

        case 3:  // Tier 3: No ranks
        case 4:  // Capstone: No ranks
        default:
            return 0;  // Or return 1 to indicate "active but unranked"
    }
}

```

---

## Appendix C: Corruption Threshold Reference

| Corruption Level | Status | Effects |
| --- | --- | --- |
| 0-25 | Safe | No penalties |
| 26-50 | Touched | Minor visual corruption, occasional whispers |
| 51-75 | Tainted | -1d10 to social checks, visible corruption marks |
| 76-99 | Corrupted | -2d10 to social, periodic Psychic Stress |
| 100 | Lost | Character transformation/death (campaign dependent) |

**Rust-Witch Risk Assessment**:

- A full combat using all abilities can add 15-25 Corruption
- Extended dungeon with 3-4 combats could push 50-80 Corruption
- Rest mechanics should allow some Corruption recovery
- Rust-Witch players must balance power vs. Corruption risk

---

**End of Specification**