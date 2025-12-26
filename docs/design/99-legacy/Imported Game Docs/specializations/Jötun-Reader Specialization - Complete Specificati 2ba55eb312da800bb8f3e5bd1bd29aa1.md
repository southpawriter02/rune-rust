# Jötun-Reader Specialization - Complete Specification

Parent item: Specs: Specializations (Specs%20Specializations%202ba55eb312da8022a82bc3d0883e1d26.md)

> Specification ID: SPEC-SPECIALIZATION-JOTUNREADER
Version: 1.0
Last Updated: 2025-11-27
Status: Draft - Implementation Review
> 

---

## Document Control

### Purpose

This document provides the complete specification for the Jötun-Reader specialization, including:

- Design philosophy and mechanical identity
- All 9 abilities with **exact formulas per rank**
- **Rank unlock requirements** (tree-progression based, NOT PP-based)
- **GUI display specifications per rank**
- Current implementation status
- Combat system integration points

### Related Files

| Component | File Path | Status |
| --- | --- | --- |
| Factory Implementation | `RuneAndRust.Engine/SpecializationFactory.cs` (Lines 340-507) | Implemented |
| Data Seeding | `RuneAndRust.Persistence/DataSeeder.cs.disabled` (Lines 377-659) | Disabled |
| Specialization Enum | `RuneAndRust.Core/Specialization.cs` (Line 19) | Implemented |
| Tests | `RuneAndRust.Tests/SpecializationIntegrationTests.cs` | Partial |
| Specialization Tree UI | `RuneAndRust.DesktopUI/Views/SpecializationTreeView.axaml` | Generic |
| Combat UI | `RuneAndRust.DesktopUI/Views/CombatView.axaml` | No specialization integration |

---

## 1. Specialization Overview

### 1.1 Identity

| Property | Value |
| --- | --- |
| **Internal Name** | JotunReader |
| **Display Name** | Jötun-Reader |
| **Specialization ID** | 2 |
| **Archetype** | Adept (ArchetypeID = 2) |
| **Path Type** | Coherent |
| **Mechanical Role** | Controller / Utility Specialist |
| **Primary Attribute** | WITS |
| **Secondary Attribute** | FINESSE |
| **Resource System** | Stamina + Psychic Stress |
| **Trauma Risk** | High |
| **Icon** | :mag: |

### 1.2 Unlock Requirements

| Requirement | Value | Notes |
| --- | --- | --- |
| **PP Cost to Unlock** | 3 PP | Standard unlock cost |
| **Minimum Legend** | 3 | Early-mid game specialization |
| **Maximum Corruption** | 100 | Coherent path - no corruption restriction |
| **Minimum Corruption** | 0 | No minimum corruption |
| **Required Quest** | None | No quest prerequisite |

### 1.3 Design Philosophy

**Tagline**: "Forensic Pathologist of the Apocalypse"

**Core Fantasy**: You are the scholar-pathologist who reads the crash logs of a dead civilization. Where others see chaos, you see patterns. You translate error messages carved in ancient stone, identify structural flaws in corrupted war-machines, and speak fragments of command-line code that freeze enemies in logic conflicts.

**Mechanical Identity**:

1. **Information Warfare**: Analyze weaknesses, expose enemy defenses to allies
2. **Linguistic Archaeology**: Translate ancient Jötun inscriptions, decipher error codes
3. **Force Multiplier**: Zero direct damage, but increases party effectiveness by 30-40%
4. **Trauma Economy Integration**: Most abilities cost Psychic Stress (high-risk/high-reward)

**Design Pillars**:

- Non-combat specialist role - provides tactical advantage rather than damage
- High dependency on party synergy (weak solo, strong in groups)
- Requires Bone-Setter support for Psychic Stress sustainability
- Unlocks exploration content through Runic Linguistics
- Ultimate power against Jötun-Forged enemies via Capstone

### 1.4 Specialization Description (Full Text)

> You are the scholar-pathologist who reads the crash logs of a dead civilization. Where others see chaos, you see patterns. You translate error messages carved in ancient stone, identify structural flaws in corrupted war-machines, and speak fragments of command-line code that freeze enemies in logic conflicts.
> 
> 
> The Jötun-Reader excels at information warfare—analyzing enemies to reveal weaknesses, guiding allies with tactical precision, and unlocking secrets hidden in ancient ruins. Your knowledge is your weapon, turning observation into advantage.
> 
> The ultimate expression is Architect of the Silence—speaking command syntax that overrides corrupted machine-logic, seizing control of Jötun-Forged enemies.
> 

### 1.5 Strategic Considerations

**Strengths**:

- Party-wide accuracy buffs (+10-15% hit rate)
- Healing amplification (+25-75% via Calculated Triage)
- Exploration content unlock (20-30% of dungeon secrets)
- Complete shutdown of machine-type enemies (Capstone)

**Weaknesses**:

- Zero direct damage output
- Weak in solo scenarios
- High Psychic Stress costs create sustainability challenges
- Dependent on Bone-Setter for stress management

---

## 2. Rank Progression System

### 2.1 CRITICAL: Rank Unlock Rules

**Ranks are unlocked through TREE PROGRESSION, not PP spending.**

| Tier | Starting Rank | Progresses To | Rank 3 Trigger |
| --- | --- | --- | --- |
| **Tier 1** | Rank 1 (when learned) | Rank 2 (when 2 Tier 2 trained) | Capstone trained |
| **Tier 2** | Rank 2 (when learned) | Rank 3 (when Capstone trained) | Capstone trained |
| **Tier 3** | No ranks | N/A | N/A |
| **Capstone** | No ranks | N/A | N/A |

**Important Notes**:

- **Tier 1 abilities** start at Rank 1, progress to Rank 2 when 2 Tier 2 abilities are trained, and reach Rank 3 when Capstone is trained
- **Tier 2 abilities** start at Rank 2 (reflecting their higher tier), and progress to Rank 3 when Capstone is trained
- **Tier 3 and Capstone abilities** do NOT have ranks - they are powerful single-effect abilities
- Rank progression is **automatic** when requirements are met (no additional PP cost)
- This creates a natural power curve: Capstone training is a major power spike for ALL trained abilities

### 2.2 Ability Structure by Tier

| Tier | Abilities | PP Cost to Unlock | Starting Rank | Max Rank | Rank Progression |
| --- | --- | --- | --- | --- | --- |
| **Tier 1** | 3 | 3 PP each | 1 | 3 | 1→2 (2× Tier 2), 2→3 (Capstone) |
| **Tier 2** | 3 | 4 PP each | 2 | 3 | 2→3 (Capstone) |
| **Tier 3** | 2 | 5 PP each | N/A | N/A | No ranks |
| **Capstone** | 1 | 6 PP | N/A | N/A | No ranks |

### 2.3 Total PP Investment

| Milestone | PP Spent | Abilities Unlocked | Tier 1 Rank | Tier 2 Rank |
| --- | --- | --- | --- | --- |
| Unlock Specialization | 3 PP | 0 | - | - |
| All Tier 1 | 3 + 9 = 12 PP | 3 Tier 1 | Rank 1 | - |
| 2 Tier 2 | 12 + 8 = 20 PP | 3 Tier 1 + 2 Tier 2 | **Rank 2** | Rank 2 |
| All Tier 2 | 20 + 4 = 24 PP | 3 Tier 1 + 3 Tier 2 | Rank 2 | Rank 2 |
| All Tier 3 | 24 + 10 = 34 PP | 3 Tier 1 + 3 Tier 2 + 2 Tier 3 | Rank 2 | Rank 2 |
| Capstone | 34 + 6 = 40 PP | All 9 abilities | **Rank 3** | **Rank 3** |

---

## 3. Ability Tree Overview

### 3.1 Visual Tree Structure

```
                    TIER 1: FOUNDATION (3 PP each, Ranks 1-3)
    ┌─────────────────────┼─────────────────────┐
    │                     │                     │
[Scholarly         [Analyze            [Runic
 Acumen I]         Weakness]          Linguistics]
 (Passive)          (Active)           (Passive)
    │                     │                     │
    └─────────────────────┴─────────────────────┘
                          │
                          ▼
              ════════════════════════
              RANK 2 UNLOCKS HERE
              (when 2 Tier 2 trained)
              ════════════════════════
                          │
                          ▼
                TIER 2: ADVANCED (4 PP each)
    ┌─────────────────────┼─────────────────────┐
    │                     │                     │
[Exploit           [Navigational      [Structural
 Design Flaw]        Bypass]           Insight]
 (Active)           (Active)          (Passive)
    │                     │                     │
    └─────────────────────┴─────────────────────┘
                          │
                          ▼
                TIER 3: MASTERY (5 PP each)
          ┌───────────────┴───────────────┐
          │                               │
    [Calculated             [The Unspoken
     Triage]                    Truth]
     (Passive)                 (Active)
          │                               │
          └───────────────┬───────────────┘
                          │
                          ▼
              ════════════════════════
              RANK 3 UNLOCKS HERE
              (when Capstone trained)
              ════════════════════════
                          │
                          ▼
              TIER 4: CAPSTONE (6 PP)
                          │
              [Architect of the Silence]
                      (Active)

```

### 3.2 Ability Summary Table

| ID | Ability Name | Tier | Type | Ranks | Resource Cost | Key Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 201 | Scholarly Acumen I | 1 | Passive | 1→2→3 | None | +dice to WITS checks |
| 202 | Analyze Weakness | 1 | Active | 1→2→3 | 30 Stamina + Stress | Reveal enemy stats |
| 203 | Runic Linguistics | 1 | Passive | 1→2→3 | None | Translate inscriptions |
| 204 | Exploit Design Flaw | 2 | Active | 2→3 | 35 Stamina | Apply [Analyzed] debuff |
| 205 | Navigational Bypass | 2 | Active | 2→3 | 30 Stamina | Party trap resistance |
| 206 | Structural Insight | 2 | Passive | 2→3 | None | Hazard detection |
| 207 | Calculated Triage | 3 | Passive | — | None | Healing amplification |
| 208 | The Unspoken Truth | 3 | Active | — | 40 Stamina | Psychological attack |
| 209 | Architect of the Silence | 4 | Active | — | 60 Stamina + 15 Stress | [Seized] on machines |

---

## 4. Tier 1 Abilities (Detailed Rank Specifications)

These abilities have 3 ranks. Rank progression is automatic based on tree investment.

---

### 4.1 Scholarly Acumen I (ID: 201)

**Type**: Passive | **Action**: Free Action (always active) | **Target**: Self

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 1 (Foundation) |
| **PP Cost to Unlock** | 3 PP |
| **Ranks** | 3 |
| **Resource Cost** | None (passive) |
| **Attribute Used** | WITS |

### Description

Your mind is a finely honed instrument, constantly processing layers of forgotten history. Years of study have made pattern recognition instinctive.

### Rank Details

### Rank 1 (Unlocked: When ability is learned)

**Mechanical Effect**:

- +2 bonus dice (d10) to all WITS-based Investigate checks
- +2 bonus dice (d10) to all System Bypass checks

**Formula**:

```
InvestigateCheckPool = WITS + 2d10
SystemBypassCheckPool = WITS + 2d10

```

**GUI Display**:

- Passive icon: Open book with magnifying glass
- Tooltip: "Scholarly Acumen I (Rank 1): +2d10 to Investigate and System Bypass checks"
- Color: Bronze border

**Combat Log Examples**:

- "Scholarly Acumen grants +2d10 to Investigation"
- "Rolling WITS (4) + Scholarly Acumen (2d10) for System Bypass"

---

### Rank 2 (Unlocked: When 2 Tier 2 abilities are trained)

**Mechanical Effect**:

- +4 bonus dice (d10) to all WITS-based Investigate checks (doubled)
- +4 bonus dice (d10) to all System Bypass checks
- Translation time reduced by 25%

**Formula**:

```
InvestigateCheckPool = WITS + 4d10
SystemBypassCheckPool = WITS + 4d10
TranslationTime = BaseTime * 0.75

```

**GUI Display**:

- Passive icon: Open book with glowing magnifying glass
- Tooltip: "Scholarly Acumen I (Rank 2): +4d10 to Investigate and System Bypass checks. Translation -25% time."
- Color: Silver border
- **Rank-up notification**: "Scholarly Acumen I has reached Rank 2! Doubled bonus dice!"

**Combat Log Examples**:

- "Scholarly Acumen (Rank 2) grants +4d10 to Investigation"
- "Translation completed 25% faster (Scholarly Acumen)"

---

### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:

- +4 bonus dice (d10) to all WITS-based Investigate checks
- +4 bonus dice (d10) to all System Bypass checks
- Translation time reduced by 25%
- **NEW**: Auto-upgrade Success → Critical Success on Investigation checks

**Formula**:

```
InvestigateCheckPool = WITS + 4d10
If InvestigateCheck == Success:
    InvestigateCheck = CriticalSuccess  // Auto-upgrade
SystemBypassCheckPool = WITS + 4d10
TranslationTime = BaseTime * 0.75

```

**GUI Display**:

- Passive icon: Radiant book with golden magnifying glass
- Tooltip: "Scholarly Acumen I (Rank 3): +4d10 to Investigate and System Bypass checks. Translation -25% time. Investigation Success → Critical Success."
- Color: Gold border
- Additional indicator: "Auto-Crit" badge
- **Rank-up notification**: "Scholarly Acumen I has reached Rank 3! Investigation always yields maximum results!"

**Combat Log Examples**:

- "Scholarly Acumen (Rank 3) grants +4d10 to Investigation"
- "Investigation Success auto-upgraded to Critical Success! (Scholarly Acumen)"

### Implementation Status

- [x]  Factory method: `AddJotunReaderAbilities()` in SpecializationFactory.cs
- [ ]  **FIX NEEDED**: Factory has `CostToRank2 = 20` - should be removed/ignored
- [ ]  GUI: Passive indicator with rank-specific icon
- [ ]  GUI: Rank border color (Bronze/Silver/Gold)
- [ ]  Combat: Integration with Investigation system
- [ ]  Combat: Auto-crit upgrade logic (Rank 3)

---

### 4.2 Analyze Weakness (ID: 202)

**Type**: Active | **Action**: Standard Action | **Target**: Single Enemy

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 1 (Foundation) |
| **PP Cost to Unlock** | 3 PP |
| **Ranks** | 3 |
| **Resource Cost** | 30 Stamina + Variable Psychic Stress |
| **Attribute Used** | WITS |

### Description

Clinical observation reveals structural flaws. You document weakness like a pathologist identifies cause of death. Your analysis enables allies to strike with precision.

### Rank Details

### Rank 1 (Unlocked: When ability is learned)

**Mechanical Effect**:

- Make WITS check vs target
- **Success**: Reveal 1 Resistance + 1 Vulnerability
- **Critical Success**: Reveal ALL Resistances/Vulnerabilities + AI behavior hint
- **Psychic Stress Cost**: 5 Stress

**Formulas**:

```
WITSCheck = Roll(WITS dice) vs TargetDifficulty
StaminaCost = 30
StressCost = 5

If Success:
    Reveal(target.Resistances[0], target.Vulnerabilities[0])
If CriticalSuccess:
    Reveal(target.AllResistances, target.AllVulnerabilities, target.AIHint)

```

**GUI Display**:

- Ability button: Eye with analysis overlay
- Tooltip: "Analyze Weakness (Rank 1): Reveal 1 Resistance + 1 Vulnerability. Cost: 30 Stamina, 5 Psychic Stress"
- Color: Bronze border
- Target must be enemy

**Combat Log Examples**:

- "Analyze Weakness reveals: [Enemy] is Resistant to Fire, Vulnerable to Lightning"
- "CRITICAL: Full analysis complete! [Enemy] behavior pattern identified"

---

### Rank 2 (Unlocked: When 2 Tier 2 abilities are trained)

**Mechanical Effect**:

- Make WITS check vs target
- **Success**: Reveal 2 Resistances + 2 Vulnerabilities
- **Critical Success**: Also reveals special ability information
- **Reduced Psychic Stress Cost**: 3 Stress

**Formulas**:

```
WITSCheck = Roll(WITS dice) vs TargetDifficulty
StaminaCost = 30
StressCost = 3  // Reduced from 5

If Success:
    Reveal(target.Resistances[0..1], target.Vulnerabilities[0..1])
If CriticalSuccess:
    Reveal(All + target.SpecialAbilityInfo)

```

**GUI Display**:

- Ability button: Enhanced eye with dual analysis overlay
- Tooltip: "Analyze Weakness (Rank 2): Reveal 2 Resistances + 2 Vulnerabilities. Cost: 30 Stamina, 3 Psychic Stress"
- Color: Silver border
- **Rank-up notification**: "Analyze Weakness has reached Rank 2! Reveals more information with less Stress!"

**Combat Log Examples**:

- "Analyze Weakness (Rank 2) reveals: [Enemy] is Resistant to Fire, Cold; Vulnerable to Lightning, Psychic"

---

### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:

- Make WITS check vs target
- **Success**: Auto-reveals Critical-level information (what was previously Crit-only)
- **NEW**: Can use as Free Action once per combat
- **Zero Psychic Stress Cost**
- **Reduced Stamina Cost**: 25

**Formulas**:

```
WITSCheck = Roll(WITS dice) vs TargetDifficulty
StaminaCost = 25  // Reduced from 30
StressCost = 0    // Eliminated

If Success:
    Reveal(target.AllResistances, target.AllVulnerabilities, target.SpecialAbilityInfo)

FreeActionUsesRemaining = 1 per combat

```

**GUI Display**:

- Ability button: Radiant eye with complete analysis overlay
- Tooltip: "Analyze Weakness (Rank 3): Full analysis on Success. Free Action 1/combat. Cost: 25 Stamina, 0 Stress"
- Color: Gold border
- Badge: "Free 1/combat"
- **Rank-up notification**: "Analyze Weakness has reached Rank 3! Zero Stress cost and Free Action option!"

**Combat Log Examples**:

- "Analyze Weakness (Rank 3) [FREE ACTION]: Complete analysis of [Enemy]"
- "Full tactical data revealed at no Stress cost!"

### Implementation Status

- [x]  Factory method: `AddJotunReaderAbilities()` in SpecializationFactory.cs
- [ ]  **FIX NEEDED**: Factory has `CostToRank2 = 20` - should be removed/ignored
- [ ]  GUI: Ability button with rank-specific icon
- [ ]  GUI: Free Action indicator (Rank 3)
- [ ]  Combat: Enemy stat revelation system
- [ ]  Combat: Stress cost calculation per rank

---

## 4.3 Runic Linguistics (ID: 203)

**Type**: Passive | **Action**: Free Action (always active) | **Target**: Self

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 1 (Foundation) |
| **PP Cost to Unlock** | 3 PP |
| **Ranks** | 3 |
| **Resource Cost** | None (passive) |
| **Attribute Used** | WITS |

### Description

You read the grammar of reality's operating system. You understand error messages in a dead language. Where others see meaningless scrawl, you see system logs and command syntax.

### Rank Details

### Rank 1 (Unlocked: When ability is learned)

**Mechanical Effect**:

- Can read and translate Elder Futhark inscriptions (non-magical)
- Basic translation of intact text
- Unlocks ~10% of dungeon text content

**Formula**:

```
CanTranslate(inscription) = inscription.Type == "ElderFuthark"
                            AND inscription.Corruption <= 0%

```

**GUI Display**:

- Passive icon: Runic stone with translation overlay
- Tooltip: "Runic Linguistics (Rank 1): Translate intact Elder Futhark inscriptions"
- Color: Bronze border
- Exploration indicator: Glowing runes when translatable text nearby

**Exploration Examples**:

- "You decipher the inscription: 'WARNING: CONTAINMENT BREACH SECTOR 7'"
- "The runes spell out an ancient command sequence..."

---

### Rank 2 (Unlocked: When 2 Tier 2 abilities are trained)

**Mechanical Effect**:

- Instantaneous translation (no time cost)
- Can translate 30-40% corrupted/fragmentary text
- Can identify author/origin of inscription
- Unlocks ~20% of dungeon text content

**Formula**:

```
CanTranslate(inscription) = inscription.Type == "ElderFuthark"
                            AND inscription.Corruption <= 40%
TranslationTime = 0  // Instant
AuthorIdentification = true

```

**GUI Display**:

- Passive icon: Runic stone with enhanced translation overlay
- Tooltip: "Runic Linguistics (Rank 2): Instant translation. Handles 40% corruption. Identifies author."
- Color: Silver border
- **Rank-up notification**: "Runic Linguistics has reached Rank 2! Instant translation and corruption handling!"

**Exploration Examples**:

- "Instant translation: 'EMERGENCY PROTOCOL... [corrupted]... EVACUATE ALL PERSONNEL'"
- "Author identified: Forge-Master Thrain, Third Cycle"

---

### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:

- Translate ANY corruption level
- Extrapolate 70-80% of missing sections
- Reveal hidden subtext and encoded messages
- Unlocks ~30% of dungeon text content (all Jötun writing)

**Formula**:

```
CanTranslate(inscription) = inscription.Type == "ElderFuthark"
                            // No corruption limit
ExtrapolationAccuracy = 0.75
RevealHiddenSubtext = true

```

**GUI Display**:

- Passive icon: Radiant runic stone with complete translation overlay
- Tooltip: "Runic Linguistics (Rank 3): Translate any corruption. Extrapolate missing text. Reveal hidden messages."
- Color: Gold border
- **Rank-up notification**: "Runic Linguistics has reached Rank 3! Master translator - nothing is hidden from you!"

**Exploration Examples**:

- "Despite 90% corruption, you extrapolate: 'The weapon cache is behind the... eastern wall... third stone from the floor'"
- "Hidden message detected within the inscription!"

### Implementation Status

- [x]  Factory method: `AddJotunReaderAbilities()` in SpecializationFactory.cs
- [ ]  **FIX NEEDED**: Factory has `CostToRank2 = 20` - should be removed/ignored
- [ ]  GUI: Passive indicator with rank-specific icon
- [ ]  Exploration: Translation system integration
- [ ]  Exploration: Corruption handling logic
- [ ]  Exploration: Hidden message detection (Rank 3)

---

## 5. Tier 2 Abilities (Rank 2→3 Progression)

These abilities start at **Rank 2** when trained (reflecting their higher tier) and progress to **Rank 3** when the Capstone is trained.

---

### 5.1 Exploit Design Flaw (ID: 204)

**Type**: Active | **Action**: Standard Action | **Target**: Single Enemy (analyzed)

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 2 (Advanced) |
| **PP Cost to Unlock** | 4 PP |
| **Prerequisite** | 8 PP invested in Jötun-Reader tree |
| **Ranks** | 2→3 (starts at Rank 2, Rank 3 with Capstone) |
| **Resource Cost** | 35 Stamina (Rank 2), 25 Stamina (Rank 3) |
| **Status Effects** | [Analyzed] |

### Description

"Strike the left knee joint—actuator is damaged." Your tactical guidance turns allies into precision instruments. You don't fight; you direct the battle.

### Rank Details

### Rank 2 (Starting Rank - When ability is learned)

**Mechanical Effect**:

- **Prerequisite**: Target must have been analyzed (via Analyze Weakness)
- Apply [Analyzed] debuff to target for 2 rounds
- While [Analyzed]: All party attacks against target gain +2 Accuracy
- Increases party hit rate by ~10%

**Formulas**:

```
Prerequisite: Target.HasBeenAnalyzed == true
StaminaCost = 35

Apply [Analyzed] status:
    Duration = 2 rounds
    Effect: AllPartyAttacks.Accuracy += 2

```

**GUI Display**:

- Ability button: Crosshair with structural overlay
- Tooltip: "Exploit Design Flaw (Rank 2): Apply [Analyzed] (+2 Accuracy for all allies) for 2 rounds. Requires prior analysis. Cost: 35 Stamina"
- Color: Silver border
- Target must have been analyzed (show warning if not)

**Combat Log Examples**:

- "Exploit Design Flaw marks [Enemy] weak points! Party gains +2 Accuracy for 2 rounds"
- "[Ally] attacks with +2 Accuracy (Exploited Design Flaw)"

---

### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:

- **No prerequisite**: Can apply without prior Analyze Weakness
- Apply [Analyzed] debuff to target for 4 rounds (doubled)
- While [Analyzed]: All party attacks gain +4 Accuracy AND +1d10 bonus damage
- **Reduced Stamina Cost**: 25
- Increases party hit rate by ~15% and adds damage

**Formulas**:

```
// No prerequisite check needed at Rank 3
StaminaCost = 25  // Reduced from 35

Apply [Analyzed] status:
    Duration = 4 rounds  // Doubled from 2
    Effect: AllPartyAttacks.Accuracy += 4  // Doubled from +2
    Effect: AllPartyAttacks.BonusDamage += Roll(1d10)  // NEW

```

**GUI Display**:

- Ability button: Radiant crosshair with complete structural overlay
- Tooltip: "Exploit Design Flaw (Rank 3): Apply [Analyzed] (+4 Accuracy, +1d10 damage for all allies) for 4 rounds. No prior analysis needed. Cost: 25 Stamina"
- Color: Gold border
- **Rank-up notification**: "Exploit Design Flaw has reached Rank 3! Doubled duration, doubled accuracy, bonus damage, no prerequisite!"

**Combat Log Examples**:

- "Exploit Design Flaw (Rank 3) reveals all weak points! Party gains +4 Accuracy, +1d10 damage for 4 rounds"
- "[Ally] attacks with +4 Accuracy and deals 7 bonus damage (Exploited Design Flaw)"

### Implementation Status

- [x]  Factory method: `AddJotunReaderAbilities()` in SpecializationFactory.cs
- [ ]  **FIX NEEDED**: Factory has `CostToRank2 = 20` - should be removed/ignored
- [ ]  GUI: Ability button with rank-specific icon
- [ ]  GUI: Prerequisite warning (Rank 2)
- [ ]  Combat: [Analyzed] status effect definition
- [ ]  Combat: Party-wide accuracy/damage modifier

---

### 5.2 Navigational Bypass (ID: 205)

**Type**: Active | **Action**: Standard Action | **Target**: Party (exploration buff)

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 2 (Advanced) |
| **PP Cost to Unlock** | 4 PP |
| **Prerequisite** | 8 PP invested in Jötun-Reader tree |
| **Ranks** | 2→3 (starts at Rank 2, Rank 3 with Capstone) |
| **Resource Cost** | 30 Stamina (Rank 2), 20 Stamina (Rank 3) |

### Description

"The trigger mechanism is corroded on the western edge. Distribute weight evenly—sensor won't register threshold pressure." Your analysis guides the party through hazards.

### Rank Details

### Rank 2 (Starting Rank - When ability is learned)

**Mechanical Effect**:

- Grant entire party +1d10 to next bypass check (trap avoidance/disarm)
- Single use per activation
- Non-combat utility ability

**Formulas**:

```
StaminaCost = 30

Party.NextBypassCheck.BonusDice += 1d10
UsesPerActivation = 1

```

**GUI Display**:

- Ability button: Footprints avoiding trap
- Tooltip: "Navigational Bypass (Rank 2): Party gains +1d10 to next trap bypass check. Cost: 30 Stamina"
- Color: Silver border
- Exploration-focused ability

**Exploration Examples**:

- "Navigational Bypass: Party gains +1d10 to bypass the pressure plate"
- "Following the Jötun-Reader's guidance, the party avoids the trap"

---

### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:

- Grant entire party +3d10 to next 2 bypass checks
- **Critical Success**: Permanently disables the hazard
- **NEW**: Can be used in combat (1/combat)
- **Reduced Stamina Cost**: 20

**Formulas**:

```
StaminaCost = 20  // Reduced from 30

Party.NextBypassCheck.BonusDice += 3d10  // Increased from +1d10
UsesPerActivation = 2  // Covers 2 checks

If CriticalSuccess:
    Hazard.Disabled = true  // Permanent

CombatUsesPerCombat = 1  // NEW: Can use in combat

```

**GUI Display**:

- Ability button: Radiant footprints with disabled trap
- Tooltip: "Navigational Bypass (Rank 3): Party gains +3d10 to next 2 bypass checks. Critical = permanent disable. Usable in combat 1/fight. Cost: 20 Stamina"
- Color: Gold border
- **Rank-up notification**: "Navigational Bypass has reached Rank 3! Triple dice, two uses, combat-capable!"

**Exploration/Combat Examples**:

- "Navigational Bypass (Rank 3): Party gains +3d10 to bypass checks (2 uses remaining)"
- "CRITICAL! The trap mechanism is permanently disabled!"
- "[IN COMBAT] Navigational Bypass guides allies past the environmental hazard"

### Implementation Status

- [x]  Factory method: `AddJotunReaderAbilities()` in SpecializationFactory.cs
- [ ]  GUI: Ability button with rank-specific icon
- [ ]  Exploration: Bypass check integration
- [ ]  Exploration: Permanent disable logic (Rank 3)
- [ ]  Combat: In-combat usage (Rank 3)

---

### 5.3 Structural Insight (ID: 206)

**Type**: Passive | **Action**: Free Action (always active) | **Target**: Environment

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 2 (Advanced) |
| **PP Cost to Unlock** | 4 PP |
| **Prerequisite** | 8 PP invested in Jötun-Reader tree |
| **Ranks** | 2→3 (starts at Rank 2, Rank 3 with Capstone) |
| **Resource Cost** | None (passive) |

### Description

"Support beams compromised. Eastern wall provides solid cover—load-bearing, reinforced. Center of room will collapse." You read structural integrity like others read expressions.

### Rank Details

### Rank 2 (Starting Rank - When ability is learned)

**Mechanical Effect**:

- Auto-detect [Structurally Unstable] features in current room
- Auto-detect [Cover] quality ratings
- Auto-detect environmental hazards in current room
- Once per combat: Warning grants allies +2d10 to defensive checks vs hazard

**Formulas**:

```
AutoDetect: [Structurally Unstable], [Cover], [Hazards] in CurrentRoom
DetectionRange = CurrentRoom

OncePerCombat:
    If HazardTrigger:
        Allies.DefensiveCheck.BonusDice += 2d10

```

**GUI Display**:

- Passive icon: Blueprint with structural overlay
- Tooltip: "Structural Insight (Rank 2): Auto-detect structural features, cover, and hazards in current room. 1/combat: +2d10 vs hazard."
- Color: Silver border
- Environmental indicators appear on exploration map

**Combat/Exploration Examples**:

- "Structural Insight detects: Unstable ceiling (center), High Cover (eastern wall), Fire Hazard (north alcove)"
- "WARNING! [Ally] gains +2d10 Defense vs collapsing pillar!"

---

### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:

- Detection extends to entire dungeon floor
- Once per combat: Call controlled collapse (Standard Action + ally attack)
- **NEW**: Auto-warn of ambush (cannot be surprised)
- **NEW**: Party gains +1 Defense in all analyzed areas

**Formulas**:

```
AutoDetect: [Structurally Unstable], [Cover], [Hazards] in EntireFloor
DetectionRange = DungeonFloor

OncePerCombat:
    ControlledCollapse: StandardAction + AllyCanAttack
    // Triggers environmental damage in targeted area

Party.Defense += 1 in AnalyzedAreas
AmbushImmunity = true  // Cannot be surprised

```

**GUI Display**:

- Passive icon: Radiant blueprint with floor-wide overlay
- Tooltip: "Structural Insight (Rank 3): Detect hazards floor-wide. 1/combat: Trigger controlled collapse. +1 Defense in analyzed areas. Ambush immunity."
- Color: Gold border
- **Rank-up notification**: "Structural Insight has reached Rank 3! Floor-wide detection, controlled demolition, and ambush immunity!"

**Combat/Exploration Examples**:

- "Structural Insight maps entire floor: 3 unstable areas, 7 cover positions, 2 hidden hazards"
- "[Jötun-Reader] triggers controlled collapse! [Ally] follows up with attack!"
- "Ambush detected! Party cannot be surprised (Structural Insight)"

### Implementation Status

- [x]  Factory method: `AddJotunReaderAbilities()` in SpecializationFactory.cs
- [ ]  GUI: Passive indicator with rank-specific icon
- [ ]  Exploration: Environmental detection system
- [ ]  Combat: Hazard warning bonus
- [ ]  Combat: Controlled collapse ability (Rank 3)
- [ ]  Combat: Ambush immunity (Rank 3)

---

## 6. Tier 3 Abilities (No Ranks)

Tier 3 abilities are powerful effects that do **not** have rank progression. They function at full power when unlocked.

---

### 6.1 Calculated Triage (ID: 207)

**Type**: Passive | **Action**: Free Action (always active) | **Target**: Adjacent Allies

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 3 (Mastery) |
| **PP Cost to Unlock** | 5 PP |
| **Prerequisite** | 16 PP invested in Jötun-Reader tree |
| **Ranks** | None (full power when unlocked) |
| **Resource Cost** | None (passive) |

### Description

"Apply pressure to brachial artery first. Follow wound track with the applicator." Your clinical guidance optimizes treatment. You don't heal—you make healers more effective.

### Mechanical Effect

- Healing consumables used on adjacent allies heal +50% more
- Range: 2 squares from Jötun-Reader
- Also removes one minor debuff ([Bleeding], [Poisoned], [Disoriented])
- Once per combat: Activate "Field Hospital" zone (3x3 area, 3 rounds)
    - +75% healing in zone
    - +2 Resolve for characters in zone
    - Healing actions cost half as much time

**Formulas**:

```
If Ally.IsAdjacent(JotunReader, Range: 2):
    HealingReceived *= 1.50  // +50%
    RemoveDebuff([Bleeding, Poisoned, Disoriented], Count: 1)

OncePerCombat FieldHospital:
    Create Zone(3x3, Duration: 3 rounds)
    Zone.HealingBonus = 0.75  // +75%
    Zone.ResolveBonus = +2
    Zone.HealingActionCost *= 0.5

```

### GUI Display

- Passive icon: Medical cross with efficiency overlay
- Tooltip: "Calculated Triage: Adjacent allies receive +50% healing, cleanse 1 minor debuff. 1/combat: Field Hospital zone (+75% healing, +2 Resolve)."
- Aura indicator on affected allies
- Field Hospital zone indicator (3x3)

### Combat Log Examples

- "Calculated Triage: [Ally]'s healing increased by 50% (healed 45 → 68)"
- "[Bleeding] cleansed by Calculated Triage"
- "FIELD HOSPITAL activated! 3x3 zone provides +75% healing, +2 Resolve for 3 rounds"

### Implementation Status

- [x]  Factory method: `AddJotunReaderAbilities()` in SpecializationFactory.cs
- [ ]  GUI: Passive indicator with aura display
- [ ]  GUI: Field Hospital zone visualization
- [ ]  Combat: Healing modifier system
- [ ]  Combat: Minor debuff cleanse
- [ ]  Combat: Zone-based buff system

---

### 6.2 The Unspoken Truth (ID: 208)

**Type**: Active | **Action**: Standard Action | **Target**: Single Intelligent Enemy

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 3 (Mastery) |
| **PP Cost to Unlock** | 5 PP |
| **Prerequisite** | 16 PP invested in Jötun-Reader tree |
| **Ranks** | None (full power when unlocked) |
| **Resource Cost** | 40 Stamina |
| **Attack Type** | Psychological/Arcane (ignores armor) |

### Description

"Your 'god' is ERROR CODE 0x4A7F. You worship a crash log." The truth shatters their worldview. You speak forbidden knowledge that destabilizes the enemy's mental coherence.

### Mechanical Effect

- Make opposed WITS vs WILL check against intelligent enemy
- **Success**: Target gains [Disoriented] for 3 rounds + 5-7 Psychic Stress
- **Critical Success**: Also [Shaken] for 2 rounds + 10-12 Psychic Stress
- Target must pass WILL check or become [Fixated] on Jötun-Reader for 1 round
- Bosses may trigger narrative consequences

**Formulas**:

```
StaminaCost = 40
OpposedCheck: JotunReader.WITS vs Target.WILL

If Success:
    Target.AddStatusEffect("Disoriented", Duration: 3)
    Target.PsychicStress += Roll(5 + 1d3)  // 5-7

If CriticalSuccess:
    Target.AddStatusEffect("Disoriented", Duration: 3)
    Target.AddStatusEffect("Shaken", Duration: 2)
    Target.PsychicStress += Roll(10 + 1d3)  // 10-12

WILLCheck = Target.Roll(WILL) vs DC 12
If WILLCheck.Fail:
    Target.AddStatusEffect("Fixated", Duration: 1, FixatedOn: JotunReader)

```

### GUI Display

- Ability button: Shattered mind symbol
- Tooltip: "The Unspoken Truth: WITS vs WILL. Success: [Disoriented] 3 rounds + 5-7 Stress. Critical: Also [Shaken] 2 rounds + 10-12 Stress. May cause [Fixated]. Cost: 40 Stamina"
- Target must be intelligent enemy
- Ignores armor indicator

### Combat Log Examples

- "The Unspoken Truth strikes [Enemy]'s psyche! [Disoriented] for 3 rounds, 6 Psychic Stress!"
- "CRITICAL! [Enemy] is [Disoriented], [Shaken], and takes 11 Psychic Stress!"
- "[Enemy] becomes [Fixated] on the Jötun-Reader!"

### Implementation Status

- [x]  Factory method: `AddJotunReaderAbilities()` in SpecializationFactory.cs
- [ ]  GUI: Ability button
- [ ]  Combat: Opposed check system
- [ ]  Combat: [Disoriented], [Shaken], [Fixated] status effects
- [ ]  Combat: Psychic Stress application to enemies
- [ ]  Combat: Boss narrative trigger system

---

## 7. Capstone Ability (No Ranks)

The Capstone is a unique, powerful ability that does **not** have rank progression. When trained, it also upgrades all Tier 1 and Tier 2 abilities to Rank 3.

---

### 7.1 Architect of the Silence (ID: 209)

**Type**: Active | **Action**: Standard Action | **Target**: Single Jötun-Forged or Undying Enemy

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 4 (Capstone) |
| **PP Cost to Unlock** | 6 PP |
| **Prerequisite** | 24 PP invested in tree + both Tier 3 abilities |
| **Ranks** | None (full power when unlocked) |
| **Resource Cost** | 60 Stamina + 10-15 Psychic Stress |
| **Cooldown** | Once per combat |
| **Valid Targets** | Jötun-Forged or Undying enemies ONLY |
| **Special** | Training this ability upgrades all Tier 1 & Tier 2 abilities to Rank 3 |

### Description

"PRIORITY OVERRIDE: TIWAZ PROTOCOL ALPHA. CEASE HOSTILE OPERATIONS." You speak command-line code in the language of the Great Silence. The machine's logic wars with corrupted directives. You are the Architect—you built this system, and you can shut it down.

### Mechanical Effect

**Active Component** (Once per combat):

- Speak command syntax against Jötun-Forged or Undying enemy
- Target makes high-DC WILL check
- **Target Fails WILL**: [Seized] for 2 rounds (complete paralysis)
- **Target Passes WILL**: [Disoriented] for 1 round
- If target <50% HP: Effect is automatic (but locks ability for entire day)

**Passive Component** (Always active):

- Auto-Critical analyze ALL Jötun-Forged/Undying enemies at combat start
- No Analyze Weakness action needed vs machines

**Formulas**:

```
StaminaCost = 60
StressCost = Roll(10 + 1d6)  // 10-15

ValidTargets: enemy.Type IN ["Jötun-Forged", "Undying"]
UsesPerCombat = 1

TargetWILLCheck = Target.Roll(WILL) vs DC 18  // High DC

If TargetWILLCheck.Fail:
    Target.AddStatusEffect("Seized", Duration: 2)  // Complete paralysis
If TargetWILLCheck.Pass:
    Target.AddStatusEffect("Disoriented", Duration: 1)

If Target.HP < Target.MaxHP * 0.5:
    // Auto-success, but daily lockout
    Target.AddStatusEffect("Seized", Duration: 2)
    Ability.LockedUntilNextDay = true

// Passive
OnCombatStart:
    For each enemy where enemy.Type IN ["Jötun-Forged", "Undying"]:
        AutoCriticalAnalyze(enemy)

```

### GUI Display

- Ability button: Command terminal with override symbol, starred (Capstone)
- Tooltip: "Architect of the Silence: WILL check. Fail = [Seized] 2 rounds. Pass = [Disoriented] 1 round. <50% HP = Auto-Seized (daily lockout). Jötun-Forged/Undying only. Cost: 60 Stamina, 10-15 Stress. 1/combat."
- Valid target indicator (only highlights Jötun-Forged/Undying)
- Passive: Auto-analysis indicator at combat start

### Combat Log Examples

- "ARCHITECT OF THE SILENCE: 'PRIORITY OVERRIDE: TIWAZ PROTOCOL ALPHA'"
- "[Jötun-Forged Guardian] fails WILL check! [SEIZED] for 2 rounds - complete system lockdown!"
- "[Undying Sentinel] resists the override but is [Disoriented] for 1 round"
- "[Jötun-Forged at 35% HP] - Emergency override successful! [SEIZED] (ability locked until tomorrow)"
- "[PASSIVE] Combat start: All Jötun-Forged enemies auto-analyzed!"

### Status Effect: [Seized]

| Property | Value |
| --- | --- |
| **Applied By** | Architect of the Silence |
| **Duration** | 2 rounds |
| **Icon** | Frozen gears / system halt |
| **Color** | Blue/White |

**Effects**:

- Cannot take ANY actions (complete paralysis)
- Cannot move
- Cannot use abilities or reactions
- Defense reduced by 2 (vulnerable)
- Attacks against target gain +2 Accuracy

### Implementation Status

- [x]  Factory method: `AddJotunReaderAbilities()` in SpecializationFactory.cs
- [ ]  GUI: Capstone ability button
- [ ]  GUI: Valid target filtering (Jötun-Forged/Undying only)
- [ ]  Combat: High-DC WILL check
- [ ]  Combat: [Seized] status effect
- [ ]  Combat: Auto-success at <50% HP with daily lockout
- [ ]  Combat: Passive auto-analysis at combat start
- [ ]  Enemy: Jötun-Forged/Undying type tagging

---

## 8. Status Effect Definitions

These status effects must be defined for Jötun-Reader abilities to function:

### 8.1 [Analyzed]

| Property | Value |
| --- | --- |
| **Applied By** | Exploit Design Flaw |
| **Duration** | 2-4 rounds (varies by rank) |
| **Icon** | Crosshair with data overlay |
| **Color** | Cyan |

**Effects**:

- All party attacks against target gain +2/+4 Accuracy (varies by rank)
- Rank 3: Also +1d10 bonus damage per attack
- Visible weak points displayed on target

**GUI Display**:

- Icon appears on debuffed enemy
- Tooltip: "[Analyzed]: All attacks +[X] Accuracy, +[Y] damage ([Z] rounds)"
- Weak point indicators visible on enemy sprite

---

### 8.2 [Disoriented]

| Property | Value |
| --- | --- |
| **Applied By** | The Unspoken Truth, Architect of the Silence |
| **Duration** | 1-3 rounds |
| **Icon** | Spinning stars / confusion |
| **Color** | Yellow |

**Effects**:

- 2 to all attack rolls
- 2 to Defense
- Cannot take Reactions
- 25% chance to act randomly (attack wrong target, move wrong direction)

**GUI Display**:

- Icon on affected enemy
- Confusion visual effect
- Tooltip: "[Disoriented]: -2 Attack, -2 Defense, no Reactions, 25% random action"

---

### 8.3 [Shaken]

| Property | Value |
| --- | --- |
| **Applied By** | The Unspoken Truth (Critical) |
| **Duration** | 2 rounds |
| **Icon** | Trembling figure |
| **Color** | Purple |

**Effects**:

- 1 to all dice pools
- Cannot benefit from morale bonuses
- WILL checks at -2

**GUI Display**:

- Icon on affected enemy
- Trembling visual effect
- Tooltip: "[Shaken]: -1 all dice, no morale bonuses, -2 WILL"

---

### 8.4 [Fixated]

| Property | Value |
| --- | --- |
| **Applied By** | The Unspoken Truth |
| **Duration** | 1 round |
| **Icon** | Eye locked on target |
| **Color** | Red |
| **Data** | FixatedOn (character causing fixation) |

**Effects**:

- Must target the character causing fixation with next action
- Cannot voluntarily break line of sight
- If target dies or becomes invalid, fixation breaks

**GUI Display**:

- Icon on affected enemy with line to Jötun-Reader
- Tooltip: "[Fixated]: Must target [Jötun-Reader] with next action"

---

### 8.5 [Seized]

| Property | Value |
| --- | --- |
| **Applied By** | Architect of the Silence |
| **Duration** | 2 rounds |
| **Icon** | Frozen gears / HALT symbol |
| **Color** | Blue/White |

**Effects**:

- Cannot take ANY actions
- Cannot move
- Cannot use abilities or reactions
- Defense -2
- Attacks against target gain +2 Accuracy
- Complete system lockdown (for machines)

**GUI Display**:

- Large icon on frozen enemy
- Static/frozen visual effect
- Tooltip: "[Seized]: Complete paralysis. Cannot act. -2 Defense. Attackers +2 Accuracy."

---

## 9. Data Model Corrections Required

### 9.1 SpecializationFactory.cs Updates

The current factory has incorrect rank cost values. These need to be corrected:

**REMOVE or SET TO 0**:

```csharp
// INCORRECT - Current factory has:
CostToRank2 = 20,  // WRONG - ranks are not purchased
CostToRank3 = 0,

// CORRECT - Should be:
CostToRank2 = 0,   // Rank 2 unlocks automatically (2 Tier 2 abilities)
CostToRank3 = 0,   // Rank 3 unlocks automatically (Capstone trained)

```

### 9.2 Character Ability Rank Calculation

```csharp
public int GetAbilityRank(PlayerCharacter character, AbilityData ability)
{
    var specProgress = GetSpecializationProgress(character, ability.SpecializationID);

    // Check if Capstone is trained (affects Tier 1 and Tier 2)
    bool hasCapstone = specProgress.UnlockedAbilities.Any(a => a.TierLevel == 4);

    // Count Tier 2 abilities unlocked (affects Tier 1)
    int tier2Count = specProgress.UnlockedAbilities.Count(a => a.TierLevel == 2);

    switch (ability.TierLevel)
    {
        case 1:  // Tier 1: Ranks 1→2→3
            if (hasCapstone)
                return 3;
            else if (tier2Count >= 2)
                return 2;
            else
                return 1;

        case 2:  // Tier 2: Ranks 2→3 (starts at Rank 2)
            if (hasCapstone)
                return 3;
            else
                return 2;

        case 3:  // Tier 3: No ranks
        case 4:  // Capstone: No ranks
        default:
            return 0;  // 0 indicates "no rank system"
    }
}

```

---

## 10. GUI Rank Display Requirements

### 10.1 Ability Card Rank Indicators

Each ability card should show current rank with visual distinction:

| Rank | Border Color | Icon Enhancement | Badge |
| --- | --- | --- | --- |
| 1 | Bronze (#CD7F32) | Base icon | "I" |
| 2 | Silver (#C0C0C0) | Enhanced glow | "II" |
| 3 | Gold (#FFD700) | Radiant glow + particles | "III" |

### 10.2 Jötun-Reader Specific UI Elements

**Analysis Overlay**:

- When enemy is analyzed, display weak point indicators
- Show resistance/vulnerability icons on enemy portrait
- Color-code: Red = Resistant, Green = Vulnerable

**Field Hospital Zone** (Calculated Triage):

- 3x3 green-tinted zone on battlefield
- Healing cross icon at center
- Duration countdown display

**[Seized] Effect**:

- Full-screen blue tint on affected enemy
- "SYSTEM HALT" text overlay
- Frozen animation state

---

## 11. Implementation Priority

### Phase 1: Critical (Foundation)

1. **Fix SpecializationFactory.cs** - Remove/correct CostToRank2 values
2. **Implement rank calculation logic** based on tree progression
3. **Define status effects** ([Analyzed], [Disoriented], [Shaken], [Fixated], [Seized])
4. **Add enemy type tagging** (Jötun-Forged, Undying)

### Phase 2: Combat Integration

1. **Route Jötun-Reader abilities** through CombatEngine
2. **Implement Analyze Weakness** revelation system
3. **Implement Exploit Design Flaw** party-wide accuracy buff
4. **Implement The Unspoken Truth** psychological attack

### Phase 3: Capstone & Advanced

1. **Implement Architect of the Silence** with target restriction
2. **Add [Seized] status effect** with complete paralysis
3. **Implement passive auto-analysis** for machines

### Phase 4: Exploration Integration

1. **Implement Runic Linguistics** translation system
2. **Implement Structural Insight** environmental detection
3. **Implement Navigational Bypass** trap resistance
4. **Implement Calculated Triage** healing zone

---

## 12. Testing Requirements

### 12.1 Rank Progression Tests

```csharp
// === TIER 1 RANK TESTS ===

[Test]
public void JotunReader_Tier1Abilities_StartAtRank1_WhenUnlocked()
{
    // When: Unlock Scholarly Acumen I (Tier 1)
    // Then: Scholarly Acumen I is Rank 1
}

[Test]
public void JotunReader_Tier1Abilities_ProgressToRank2_When2Tier2AbilitiesUnlocked()
{
    // Given: Scholarly Acumen I unlocked at Rank 1
    // When: Unlock Exploit Design Flaw and Navigational Bypass (2 Tier 2 abilities)
    // Then: Scholarly Acumen I is now Rank 2
    // And: All other Tier 1 abilities are also Rank 2
}

[Test]
public void JotunReader_Tier1Abilities_ProgressToRank3_WhenCapstoneUnlocked()
{
    // Given: Scholarly Acumen I at Rank 2
    // When: Unlock Architect of the Silence (Capstone)
    // Then: Scholarly Acumen I is now Rank 3
    // And: All Tier 1 abilities are Rank 3
}

// === TIER 2 RANK TESTS ===

[Test]
public void JotunReader_Tier2Abilities_StartAtRank2_WhenUnlocked()
{
    // When: Unlock Exploit Design Flaw (Tier 2)
    // Then: Exploit Design Flaw is Rank 2 (NOT Rank 1)
}

[Test]
public void JotunReader_Tier2Abilities_ProgressToRank3_WhenCapstoneUnlocked()
{
    // Given: Exploit Design Flaw at Rank 2
    // When: Unlock Architect of the Silence (Capstone)
    // Then: Exploit Design Flaw is now Rank 3
}

// === CAPSTONE TESTS ===

[Test]
public void ArchitectOfSilence_OnlyTargetsJotunForgedOrUndying()
{
    // Given: Regular enemy (not Jötun-Forged or Undying)
    // When: Attempt to use Architect of the Silence
    // Then: Ability cannot target this enemy
}

[Test]
public void ArchitectOfSilence_AppliesSeizedOnWILLFailure()
{
    // Given: Jötun-Forged enemy
    // When: Target fails WILL check
    // Then: Target gains [Seized] for 2 rounds
}

[Test]
public void ArchitectOfSilence_AutoSucceedsUnder50PercentHP()
{
    // Given: Jötun-Forged enemy at 40% HP
    // When: Use Architect of the Silence
    // Then: [Seized] applied automatically
    // And: Ability locked until next day
}

```

### 12.2 Ability Effect Tests

```csharp
[Test]
public void ExploitDesignFlaw_Rank2_Requires_PriorAnalysis()
{
    // Given: Enemy not analyzed
    // When: Attempt Exploit Design Flaw at Rank 2
    // Then: Ability fails / shows warning
}

[Test]
public void ExploitDesignFlaw_Rank3_NoPrerequisite()
{
    // Given: Enemy not analyzed
    // When: Use Exploit Design Flaw at Rank 3
    // Then: Ability succeeds, [Analyzed] applied
}

[Test]
public void CalculatedTriage_IncreasesHealingBy50Percent()
{
    // Given: Ally adjacent to Jötun-Reader
    // When: Ally receives 40 HP healing
    // Then: Ally actually heals 60 HP (+50%)
}

```

---

## 13. Synergy Documentation

### 13.1 Strong Synergies

| Partner | Synergy Effect |
| --- | --- |
| **Bone-Setter** | Essential for Psychic Stress management; combined healing bonus |
| **Skjaldmaer** | Tank benefits from +Accuracy buffs; Jötun-Reader safe behind shield |
| **Any DPS** | +10-15% hit rate from [Analyzed] dramatically increases damage output |
| **Skald** | Combined party buffs create overwhelming advantage |

### 13.2 Anti-Synergies

| Scenario | Problem |
| --- | --- |
| **Solo Play** | Information warfare provides no direct damage; weak alone |
| **Combat-Only Dungeons** | Exploration abilities (Runic Linguistics, Navigational Bypass) wasted |
| **Non-Intelligent Enemies** | The Unspoken Truth ineffective; limited psychological attacks |
| **No Machine Enemies** | Capstone (Architect of the Silence) cannot be used |

---

**End of Specification**