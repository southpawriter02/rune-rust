# Status Effect System — Mechanic Specification v5.0

Status: Proposed
Balance Validated: No
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

## I. Core Philosophy: The State of the System

Status Effects are the mechanical representation of a combatant's **current system state**.

**The Conceptual Model:**

In Aethelgard, status effects are not just arbitrary icons—they are tangible, temporary conditions reflecting:

- **Hardware alterations:** Physical trauma, chemical contamination, mechanical damage
- **Software corruption:** Mental manipulation, cognitive glitches, behavioral overrides
- **System state changes:** Operational mode shifts, process interruptions, resource drain

**Examples:**

- `[Poisoned]`: Running a malicious "decay.exe" program
- `[Stunned]`: Temporary critical system crash
- `[Feared]`: Logic overridden by primal, irrational subroutine
- `[Hasted]`: Overclocked processing speed

**Why This System Exists:**

**Thematic Justification:**

- Reflects the "broken system" nature of Aethelgard
- External forces can alter a character's operational state
- Status effects are the battlefield equivalent of system errors, malware, or patches

**Gameplay Benefits:**

- **Tactical depth beyond damage:** Controlling enemy/ally states is as important as managing HP
- **Specialist identity:** Different builds excel at applying/removing specific effects
- **Dynamic battlefield:** Constantly shifting states create evolving tactical situations
- **Synergy potential:** Status effects enable powerful ability combos

---

## II. Parent System

**Parent:** [Combat System — Core System Specification v5.0](Combat%20System%20%E2%80%94%20Core%20System%20Specification%20v5%200%204fdd9ec9ec974a75b45746d33e32a7d1.md)

**Relationship:**

The Status Effect System is a **child mechanic** of the Combat System. It is invoked throughout combat:

**Combat System Integration Points:**

1. **After damage application:** Many attacks apply status effects on hit
2. **Start of turn:** Status effects tick down, DoT effects trigger
3. **Before actions:** Status effects may block actions (Stunned, Silenced)
4. **During actions:** Status effects modify dice pools (Disoriented, Inspired)

**Integration Flow:**

```
Combat Turn Start
→ Status Effect System: Tick down durations
→ Status Effect System: Apply DoT damage
→ Character Action Phase
→ Status Effect System: Check action restrictions
→ Status Effect System: Apply stat modifiers
→ Action Resolution
→ Status Effect System: Apply new effects (if attack hits)
```

---

*Migration in progress. Next: Section III - Anatomy of a Status Effect.*

```jsx

---

## III. Anatomy of a Status Effect

Every status effect is a distinct data entity with consistent properties.

### Core Properties

**Name:**
- Unique identifier (e.g., `[Bleeding]`, `[Feared]`)
- Always displayed in brackets: `[EffectName]`
- Player-facing and immutable

**Effect Type:**
- **Buff:** Beneficial effect (increases capabilities)
- **Debuff:** Harmful effect (decreases capabilities)
- **Neutral:** State change without clear benefit/harm (rare)

**Duration:**
- Number of combat rounds the effect lasts
- **Tick-down timing:** Decrements at **start** of affected character's turn
- Range: Typically 1-3 rounds (some effects may be longer)

**Source:**
- The ability, item, or hazard that applied the effect
- Used for tracking (who applied this?) and removal (some cleanses require specific sources)

**Stacking Rules:**
- Defines how multiple applications interact
- Three types: **Refresh**, **Intensify**, **No Stack**
- See Section V for detailed stacking mechanics

**Effect Script:**
- The actual mechanical modifier(s) applied
- Examples: "-2 dice to Accuracy", "Take 1d6 damage per turn", "Cannot move"
- Defined in individual status effect specs

**Cleanse Type:**
- Category of ability/action required to remove prematurely
- Types: **Physical**, **Alchemical**, **Mental**, **Arcane**
- Some effects cannot be cleansed during combat

---

*Migration in progress. Next: Section IV - Status Effect Categories.*
- Some effects cannot be cleansed during combat

---

## IV. Status Effect Categories & Reference

Status effects are organized by function: **Debuffs** (hostile) and **Buffs** (beneficial).

### Debuff Categories

**Damage Over Time (DoT):**
- Deal damage at start of affected character's turn
- Examples: [Bleeding], [Poisoned], [Burning]
- Stack type: Intensify (more stacks = more damage)

**Stat Penalties:**
- Reduce attributes, dice pools, or derived stats
- Examples: [Disoriented], [Corroded], [Weakened]
- Stack type: Varies by effect

**Control:**
- Restrict or prevent actions
- Examples: [Stunned], [Feared], [Rooted], [Silenced]
- Stack type: Typically Refresh (duration resets)

**Resource Drain:**
- Reduce or prevent resource regeneration
- Examples: [Exhausted], [Drained]
- Stack type: Intensify

### Buff Categories

**Stat Bonuses:**
- Increase attributes, dice pools, or derived stats
- Examples: [Inspired], [Fortified], [Empowered]
- Stack type: Refresh

**Defensive:**
- Increase survivability
- Examples: [Protected], [Warded], [Regeneration]
- Stack type: Varies by effect

**Offensive:**
- Increase damage output or action economy
- Examples: [Hasted], [Enraged], [Blessed]
- Stack type: Refresh

---

*Migration in progress. Next: Section V - Application & Resistance.*
- Stack type: Refresh

---

## V. Application & Resistance Mechanics

### Application Pipeline

**Step 1: Attack Hits (if required)**
- Most status effects require a successful attack first
- Accuracy Check must succeed (unless ability has `[Guaranteed Hit]`)
- Some effects (AOE, environmental) bypass this step

**Step 2: Resistance Check**
- Target makes a **Resolve Check** to resist the effect
- **Physical Resolve** (STURDINESS-based): For physical effects (Bleeding, Poisoned, Rooted)
- **Mental Resolve** (WILL-based): For mental effects (Feared, Disoriented, Silenced)

**Resolve Check Formula:**
```

Resolve Pool = STURDINESS or WILL + bonuses

Target Number = Effect Potency (set by ability)

If Successes >= Potency: Resist (no effect applied)

If Successes < Potency: Effect applied

```jsx

**Step 3: Apply Effect**
- If resistance fails, add effect to target's active status effect list
- Set initial duration (defined by ability)
- Apply initial stacks (typically 1 stack)

**Bypass:** Some abilities apply effects **automatically** (no resistance check):
- Critical hits with certain weapons
- High-tier abilities
- Environmental hazards

---

*Migration in progress. Next: Section VI - Stacking Rules.*
- Environmental hazards

---

## VI. Stacking Rules

When a status effect is applied to a target that already has the same effect, the **stacking rule** determines the outcome.

### Stacking Type 1: Refresh

**Behavior:** New application resets duration to maximum, does not increase intensity.

**Use Case:** Control effects and most buffs.

**Example:**
```

Target has [Rooted] with 1 round remaining

New [Rooted] application (2 rounds duration)

Result: [Rooted] duration resets to 2 rounds

```

**Effects Using Refresh:**
- Most control effects: [Rooted], [Slowed], [Feared], [Disoriented]
- Most buffs: [Hasted], [Inspired], [Fortified]

**Design Rationale:** Prevents overly long CC chains, keeps effects predictable.

---

### Stacking Type 2: Intensify

**Behavior:** New application adds a **stack**, increasing effect potency. Duration may also refresh.

**Use Case:** Damage-over-time effects and Soak-reduction effects.

**Example:**
```

Target has [Bleeding] (1 stack, 3 rounds remaining)

New [Bleeding] application (3 rounds duration)

Result: [Bleeding] (2 stacks, 3 rounds remaining)

Effect: Now deals 2d6 damage per turn instead of 1d6

```

**Max Stacks:** Each effect defines a maximum (typically 3-5 stacks).

**Effects Using Intensify:**
- DoT effects: [Bleeding], [Poisoned], [Burning]
- Debuff stacking: [Corroded] (reduces Soak per stack)

**Design Rationale:** Rewards focused attacks, creates synergy, enables burst windows.

---

### Stacking Type 3: No Stack

**Behavior:** New application fails if effect is already active. Effect is immune to itself until it expires.

**Use Case:** Most powerful "hard control" effects.

**Example:**
```

Target has [Stunned] (1 round remaining)

New [Stunned] application attempted

Result: Application fails (target already stunned)

```jsx

**Effects Using No Stack:**
- Hard control: [Stunned]
- Unique debuffs that would be overpowered if stacked

**Design Rationale:** Prevents degenerate "perma-stun" scenarios.

---

*Migration in progress. Next: Section VII - Cleansing & Removal.*
**Design Rationale:** Prevents degenerate "perma-stun" scenarios.

---

## VII. Cleansing & Removal

Status effects can be removed through **natural expiration** or **active cleansing**.

### Natural Expiration

**Duration Tick-Down:**
- At **start of affected character's turn**, all their status effect durations decrement by 1
- When duration reaches 0, effect is removed
- This is the default removal method

### Active Cleansing

Many effects can be removed prematurely by dedicated cleanse abilities. The **cleanse type** determines what can remove it.

**Cleanse Type: Physical**
- **Targets:** Physical trauma effects ([Bleeding], [Rooted])
- **Sources:** Bone-Setter abilities, field medicine, bandages
- **Example:** Bone-Setter's "Apply Tourniquet" removes [Bleeding]

**Cleanse Type: Alchemical**
- **Targets:** Chemical/toxin effects ([Poisoned])
- **Sources:** Antidotes, alchemical potions, Brewmaster abilities
- **Example:** Antidote Potion removes [Poisoned]

**Cleanse Type: Mental**
- **Targets:** Mental/psychological effects ([Feared], [Disoriented])
- **Sources:** Skald's inspiring words, Thul's logical reassurance
- **Example:** Skald's "Inspiring Word" removes [Feared]

**Cleanse Type: Arcane**
- **Targets:** Magical curses and corruptions ([Corroded], [Silenced])
- **Sources:** Vard-Warden wards, high-tier dispel abilities
- **Example:** Vard-Warden's "Cleansing Ward" removes [Corroded]

**Uncleansable Effects:**
- Some effects cannot be cleansed during combat
- Must expire naturally or wait until out-of-combat rest
- Typically high-tier debuffs with short durations

---

*Migration in progress. Final section: Integration & Systemic Identity.*
- Typically high-tier debuffs with short durations

---

## VIII. Integration & Systemic Identity

### System Dependencies

**Foundation Systems:**
- <mention-page url="[AI Session Handoff — DB10 Specifications Migration](https://www.notion.so/AI-Session-Handoff-DB10-Specifications-Migration-065ef630b24048129cc560a393bd2547?pvs=21) Pool System — Core System Specification v5.0</mention-page>: Used for Resolve Checks
- <mention-page url="[AI Session Handoff — DB10 Specifications Migration](https://www.notion.so/AI-Session-Handoff-DB10-Specifications-Migration-065ef630b24048129cc560a393bd2547?pvs=21) System — Core System Specification v5.0</mention-page>: Provides STURDINESS, WILL

**Parent System:**
- <mention-page url="[https://www.notion.so/4fdd9ec9ec974a75b45746d33e32a7d1">Combat](https://www.notion.so/4fdd9ec9ec974a75b45746d33e32a7d1">Combat) System — Core System Specification v5.0</mention-page>: Invokes Status Effect System throughout combat

**Sibling Systems:**
- <mention-page url="[https://www.notion.so/e0b01bf86d4141c7bc2e7131b3b650bc">Accuracy](https://www.notion.so/e0b01bf86d4141c7bc2e7131b3b650bc">Accuracy) Check System — Mechanic Specification v5.0</mention-page>: Determines if attacks that apply effects hit
- <mention-page url="[https://www.notion.so/65a75f9a127b4a87aa421fc36e8e4340">Damage](https://www.notion.so/65a75f9a127b4a87aa421fc36e8e4340">Damage) System — Mechanic Specification v5.0</mention-page>: DoT effects use Damage System to deal damage

**Referenced Systems:**
- <mention-page url="[https://www.notion.so/2a355eb312da80768e66db52bdd7cf19">Feature](https://www.notion.so/2a355eb312da80768e66db52bdd7cf19">Feature) Specification: The Fury Resource System</mention-page>: Mental effects may trigger Stress
- **Turn System:** Manages duration tick-down timing

---

### Service Architecture

**Primary Service: `StatusEffectService.cs`**

**Key Methods:**
```

ApplyEffect(targetID, effectName, duration, stacks) → bool

ResistEffect(targetID, effectPotency, resolveType) → bool

TickDownEffects(characterID) → void

ProcessDoTEffects(characterID) → void

RemoveEffect(targetID, effectName) → bool

QueryActiveEffects(characterID) → List<ActiveEffect>

```

---

### Specialist Identity

Status effects define specialization roles:

**Rust-Witch:** Master of [Corroded] (Soak reduction)  
**Hlekkr-master:** Master of [Rooted] and [Slowed] (movement control)  
**Bone-Setter:** Master of cleansing physical ailments  
**Skald:** Master of inspiring allies, removing mental debuffs  
**Thul:** Master of applying mental debuffs ([Feared], [Disoriented])

---

### Tactical Core

**Status effects transform combat from damage races into tactical chess matches:**

- **Synergy:** DoT stacking enables burst damage windows
- **Control:** CC effects enable focus-fire strategies
- **Cleansing:** Support roles become crucial (not just healing)
- **Counter-play:** High WILL/STURDINESS counters different effect types

---

## Migration Status: COMPLETE

**Date Migrated:** 2025-11-08  
**Source:** v2.0 Status Effect System Feature Specification  
**Target:** DB10 Status Effect System — Mechanic Specification v5.0  
**Status:** ✅ Draft Complete

**All sections migrated:**
- ✅ I. Core Philosophy
- ✅ II. Parent System
- ✅ III. Anatomy of a Status Effect
- ✅ IV. Status Effect Categories & Reference
- ✅ V. Application & Resistance Mechanics
- ✅ VI. Stacking Rules
- ✅ VII. Cleansing & Removal
- ✅ VIII. Integration & Systemic Identity

**TIER 2 COMPLETE: 4/4 specifications migrated**
- ✅ Combat System (orchestrator) — 8 sections
- ✅ Accuracy Check System — 9 sections
- ✅ Damage System — 6 sections
- ✅ Status Effect System — 8 sections

**Next Phase: TIER 3 (Individual Status Effect Specs)**
- 12 individual status effect specifications to create
- Each will use the Status Effect Specification Template v5.0
- Ready to begin when you are!
```

[[Bleeding] Status Effect — Status Effect Specification v5.0](Status%20Effect%20System%20%E2%80%94%20Mechanic%20Specification%20v5%200/%5BBleeding%5D%20Status%20Effect%20%E2%80%94%20Status%20Effect%20Specifica%2076034413cf0245abbd1fdb958ef7a8c5.md)

[[Poisoned] Status Effect — Status Effect Specification v5.0](Status%20Effect%20System%20%E2%80%94%20Mechanic%20Specification%20v5%200/%5BPoisoned%5D%20Status%20Effect%20%E2%80%94%20Status%20Effect%20Specifica%20db49bd49689e42a5b60371f22ad8358b.md)

[[Stunned] Status Effect — Status Effect Specification v5.0](Status%20Effect%20System%20%E2%80%94%20Mechanic%20Specification%20v5%200/%5BStunned%5D%20Status%20Effect%20%E2%80%94%20Status%20Effect%20Specificat%20ddf77da8d3cc436196c285b86ad0e0cf.md)

[[Feared] Status Effect — Status Effect Specification v5.0](Status%20Effect%20System%20%E2%80%94%20Mechanic%20Specification%20v5%200/%5BFeared%5D%20Status%20Effect%20%E2%80%94%20Status%20Effect%20Specificati%20a34cad67bd1f4af9875eed16b42eab85.md)

[[Corroded] Status Effect — Status Effect Specification v5.0](Status%20Effect%20System%20%E2%80%94%20Mechanic%20Specification%20v5%200/%5BCorroded%5D%20Status%20Effect%20%E2%80%94%20Status%20Effect%20Specifica%20fd3ac8b47e174fecbbaaf4cf84a67448.md)

[[Disoriented] Status Effect — Status Effect Specification v5.0](Status%20Effect%20System%20%E2%80%94%20Mechanic%20Specification%20v5%200/%5BDisoriented%5D%20Status%20Effect%20%E2%80%94%20Status%20Effect%20Specif%2016b2cbb91d93439186378c014d772272.md)

[[Slowed] Status Effect — Status Effect Specification v5.0](Status%20Effect%20System%20%E2%80%94%20Mechanic%20Specification%20v5%200/%5BSlowed%5D%20Status%20Effect%20%E2%80%94%20Status%20Effect%20Specificati%200c64c397af364eda89b93da68c11f175.md)

[[Rooted] Status Effect — Status Effect Specification v5.0](Status%20Effect%20System%20%E2%80%94%20Mechanic%20Specification%20v5%200/%5BRooted%5D%20Status%20Effect%20%E2%80%94%20Status%20Effect%20Specificati%206896d92817804dd69420fce12c978a1b.md)

[[Silenced] Status Effect — Status Effect Specification v5.0](Status%20Effect%20System%20%E2%80%94%20Mechanic%20Specification%20v5%200/%5BSilenced%5D%20Status%20Effect%20%E2%80%94%20Status%20Effect%20Specifica%205e38d80059d949f78ece0c54bfcbb643.md)

[[Fortified] Status Effect — Status Effect Specification v5.0](Status%20Effect%20System%20%E2%80%94%20Mechanic%20Specification%20v5%200/%5BFortified%5D%20Status%20Effect%20%E2%80%94%20Status%20Effect%20Specific%205c9c5032c4e240df92fa96041039cfc4.md)

[[Hasted] Status Effect — Status Effect Specification v5.0](Status%20Effect%20System%20%E2%80%94%20Mechanic%20Specification%20v5%200/%5BHasted%5D%20Status%20Effect%20%E2%80%94%20Status%20Effect%20Specificati%20635817cb391540c0a87a8cf8952cbb90.md)

[[Inspired] Status Effect — Status Effect Specification v5.0](Status%20Effect%20System%20%E2%80%94%20Mechanic%20Specification%20v5%200/%5BInspired%5D%20Status%20Effect%20%E2%80%94%20Status%20Effect%20Specifica%2089578693e068443f82341db9e25f7381.md)