# Myrk-gengr (Shadow-Walker) Specialization Specification

Parent item: Specs: Specializations (Specs%20Specializations%202ba55eb312da8022a82bc3d0883e1d26.md)

> Version: 1.0
Last Updated: 2025-11-27
Status: Draft
Specialization ID: 24002
Archetype: Skirmisher (ArchetypeID: 4)
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

**Name**: Myrk-gengr (Shadow-Walker)
**Tagline**: "The Ghost in the Machine"
**Role**: Stealth Assassin / Alpha Strike Specialist
**Path Type**: Heretical
**Icon**: 🌑

**Description**:

> You are the ghost in the machine. You've learned to wrap yourself in the world's psychic static, becoming a blind spot in enemy perception. Your attacks from stealth don't just deal physical damage—they inflict psychological terror, shattering minds alongside bodies.
> 
> 
> Your capstone ability lets you become a living glitch, a reality-warping violation of causality. You are the predator who strikes from places the mind insists are empty. The ultimate alpha strike specialist who deletes high-priority targets before they can act.
> 

### Core Mechanics

| Attribute | Value |
| --- | --- |
| **Primary Attribute** | FINESSE |
| **Secondary Attribute** | WILL |
| **Resource System** | Stamina + Focus |
| **Trauma Risk** | High |
| **Unlock Cost** | 3 PP |
| **Unlock Requirements** | Legend 5+, Any Corruption (0-100) |

### Unique Systems

1. **[Hidden] State**: Core stealth mechanic - enemies cannot target you directly while Hidden
2. **Psychic Resonance Zones**: Areas of psychic static that enhance stealth capabilities
3. **Terror Strikes**: First attack from stealth inflicts massive Psychic Stress and Fear
4. **Stealth Persistence**: Chance to remain Hidden after attacking (via Ghostly Form)
5. **Self-Corruption Risk**: Capstone ability inflicts significant self-Corruption

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

| ID | Name | Tier | Type | Stamina | Key Effect |
| --- | --- | --- | --- | --- | --- |
| 24010 | One with the Static I | 1 | Passive | - | +Stealth dice, enhanced in Resonance zones |
| 24011 | Enter the Void | 1 | Active | 40/35 | Enter [Hidden] state via stealth check |
| 24012 | Shadow Strike | 1 | Active | 35 | Guaranteed crit from [Hidden] |
| 24013 | Throat-Cutter | 2 | Active | 45 | Damage + [Silenced] from flank/Hidden |
| 24014 | Sensory Scramble | 2 | Active | 30 | Create [Psychic Resonance] zone |
| 24015 | Mind of Stillness | 2 | Passive | - | Regen Stamina/remove Stress while Hidden |
| 24016 | Terror from the Void | 3 | Passive | - | First strike: +Stress, +[Feared] |
| 24017 | Ghostly Form | 3 | Passive | - | +Defense, stealth persistence chance |
| 24018 | Living Glitch | 4 | Active | 60 | Ultimate assassination, +18 self-Corruption |

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

### Ability 24010: One with the Static I

**Type**: Passive
**Action**: Free Action
**Target**: Self
**Resource Cost**: None

**Description**:

> You find comfort in the world's background noise. The hum of the Blight is not a threat—it is camouflage.
> 

### Rank Progression

| Rank | Stealth Bonus | Resonance Zone Bonus | Special |
| --- | --- | --- | --- |
| **Rank 1** | +1d10 | +2d10 additional | - |
| **Rank 2** | +2d10 | +2d10 additional | Ignore -1d10 Resonance penalty |
| **Rank 3** | +3d10 | +2d10 additional | Enemies -2d10 to detect you in zones |

### Detailed Mechanics

**Rank 1** (Unlock → 2 Tier 2s trained):

- Passive bonus of +1d10 to all FINESSE-based Stealth checks
- When in [Psychic Resonance] zones, gain additional +2d10 (total +3d10)

**Rank 2** (2 Tier 2s trained → Capstone):

- Base bonus increases to +2d10 to Stealth checks
- Partial immunity to [Psychic Resonance] penalties (ignore -1d10 to other checks)
- In Resonance zones: +4d10 total (+2d10 base + 2d10 zone)

**Rank 3** (Capstone trained):

- Base bonus increases to +3d10 to Stealth checks
- Full immunity to [Psychic Resonance] penalties
- In zones: enemies suffer -2d10 to detect you
- Total in Resonance zone: +5d10 Stealth

### GUI Display Requirements

**Ability Card**:

```
┌─────────────────────────────────────┐
│ 🌑 ONE WITH THE STATIC I    [RANK] │
│ ─────────────────────────────────── │
│ Type: Passive                       │
│ Attribute: FINESSE                  │
│ ─────────────────────────────────── │
│ Stealth Bonus: +Xd10               │
│ Resonance Bonus: +2d10 additional   │
│ [Rank-specific special effect]      │
└─────────────────────────────────────┘

```

**Combat Log Entry**:

- "One with the Static: +{X}d10 to Stealth check"
- "Psychic Resonance detected: +2d10 additional Stealth"

---

### Ability 24011: Enter the Void

**Type**: Active
**Action**: Standard Action (Rank 3: Bonus Action)
**Target**: Self
**Resource Cost**: 40 Stamina (Rank 2+: 35 Stamina)

**Description**:

> You focus your will, synchronizing your presence with the world's psychic static. You vanish from sight—not hidden in shadow, but erased from perception.
> 

### Rank Progression

| Rank | Stamina Cost | DC | Action Type |
| --- | --- | --- | --- |
| **Rank 1** | 40 | 16 | Standard Action |
| **Rank 2** | 35 | 14 | Standard Action |
| **Rank 3** | 35 | 12 | Bonus Action |

### Detailed Mechanics

**Rank 1** (Unlock → 2 Tier 2s trained):

- Cost: 40 Stamina
- Make FINESSE + Acrobatics (Stealth) check vs DC 16
- Success: Enter [Hidden] state
- While Hidden: enemies cannot target you directly
- [Hidden] state persists until you attack or fail stealth check

**Rank 2** (2 Tier 2s trained → Capstone):

- Cost reduced to 35 Stamina
- DC reduced to 14
- All other effects unchanged

**Rank 3** (Capstone trained):

- Cost: 35 Stamina
- DC reduced to 12
- Can use as Bonus Action instead of Standard Action
- Allows entering stealth and attacking in same turn

### Formula

```
Stealth Check = FINESSE + Skill Bonus + One with the Static Bonus + Psychic Resonance Bonus

Success Condition: Stealth Check ≥ DC (16/14/12 by rank)

Example (Rank 2, FINESSE 4, One with Static Rank 2, in Resonance Zone):
  Roll = 4d10 (FINESSE) + 2d10 (One with Static) + 2d10 (Resonance) = 8d10
  DC = 14
  Average roll of 8d10 = 44, very likely success

```

### GUI Display Requirements

**Ability Button State**:

- **Available**: Blue border, Stamina cost displayed
- **Active**: Pulsing glow when [Hidden]
- **Unavailable**: Greyed out if insufficient Stamina

**Stealth Check Dialog**:

```
┌────────────────────────────────────────┐
│         ENTER THE VOID                 │
│ ────────────────────────────────────── │
│ Stealth Check: [Roll] vs DC [X]        │
│                                        │
│ FINESSE:           4d10               │
│ One with Static:   +2d10              │
│ Resonance Zone:    +2d10              │
│ ────────────────────────────────────── │
│ Total Dice:        8d10               │
│                                        │
│ [SUCCESS/FAILURE]                      │
│ You [vanish into/fail to enter] the    │
│ psychic static                         │
└────────────────────────────────────────┘

```

---

### Ability 24012: Shadow Strike

**Type**: Active
**Action**: Standard Action
**Target**: Single Enemy (Melee)
**Resource Cost**: 35 Stamina
**Requirement**: Must be in [Hidden] state

**Description**:

> A precise, brutal attack from a blind spot. The blade finds its mark before their corrupted processors can register the threat.
> 

### Rank Progression

| Rank | Bonus Damage | On Kill | Special |
| --- | --- | --- | --- |
| **Rank 1** | - | - | Guaranteed crit (2× damage) |
| **Rank 2** | +2d6 | Refund 20 Stamina | Guaranteed crit |
| **Rank 3** | +4d6 | Refund 20 Stamina | Guaranteed crit + [Bleeding] 2 turns |

### Detailed Mechanics

**Rank 1** (Unlock → 2 Tier 2s trained):

- Requires [Hidden] state
- FINESSE-based melee attack
- Guaranteed critical hit (damage doubled)
- Immediately breaks stealth (unless Ghostly Form procs)

**Rank 2** (2 Tier 2s trained → Capstone):

- All Rank 1 effects
- +2d6 bonus damage (before doubling)
- If attack kills target, refund 20 Stamina

**Rank 3** (Capstone trained):

- All Rank 2 effects
- Bonus damage increases to +4d6 (before doubling)
- Apply [Bleeding] for 2 turns (2d6 damage/turn)

### Damage Formula

```
Base Damage = Weapon Damage + FINESSE Modifier + Bonus Dice
Critical Damage = Base Damage × 2

Rank 1: (Weapon + FINESSE) × 2
Rank 2: (Weapon + FINESSE + 2d6) × 2
Rank 3: (Weapon + FINESSE + 4d6) × 2 + [Bleeding]

Example (Rank 3, Dagger 2d6+2, FINESSE +3):
  Base = (2d6+2) + 3 + 4d6 = 6d6 + 5
  Average Base = 21 + 5 = 26
  Critical = 52 damage average
  Plus 2d6 Bleeding/turn for 2 turns = +14 total

```

### Integration with Other Abilities

- **Terror from the Void**: First Shadow Strike per combat triggers additional effects
- **Ghostly Form**: Chance to remain [Hidden] after Shadow Strike
- **One with the Static**: Helps re-enter stealth after strike

### GUI Display Requirements

**Combat Action Panel**:

```
┌────────────────────────────────────────┐
│ ⚔️ SHADOW STRIKE              [RANK] │
│ ────────────────────────────────────── │
│ Stamina: 35                            │
│ Requires: [Hidden]                     │
│ ────────────────────────────────────── │
│ GUARANTEED CRITICAL HIT                │
│ Damage: Weapon × 2 + {bonus}d6 × 2     │
│ [Rank 3: + Bleeding 2 turns]           │
│                                        │
│ ⚠️ Breaks stealth on use              │
└────────────────────────────────────────┘

```

**Combat Log**:

```
[SHADOW STRIKE - CRITICAL HIT]
{Character} strikes from the void!
Damage: {X} (doubled from critical)
[Bleeding applied for 2 turns]
[Stealth broken / Ghostly Form: Stealth maintained!]

```

---

## Tier 2 Abilities (Ranks 2-3)

*Note: Tier 2 abilities start at Rank 2 when trained and advance to Rank 3 when Capstone is trained.*

### Ability 24013: Throat-Cutter

**Type**: Active
**Action**: Standard Action
**Target**: Single Enemy (Melee)
**Resource Cost**: 45 Stamina
**Cooldown**: 3 turns (Per Combat)

**Description**:

> You strike from behind with lethal precision, severing vocal cords. Their screams are silenced before they can escape.
> 

### Rank Progression

| Rank | Bonus Damage | [Silenced] Duration | Special |
| --- | --- | --- | --- |
| **Rank 2** | 2d8 | 1 turn | From flank/Hidden only |
| **Rank 3** | 4d8 | 2 turns | +[Bleeding] if target was [Feared] |

*Note: Rank 1 not applicable - Tier 2 abilities start at Rank 2*

### Detailed Mechanics

**Rank 2** (When trained):

- Melee attack dealing Weapon Damage + 2d8
- If attacking from flanking position OR [Hidden] state: apply [Silenced] for 1 turn
- [Silenced]: Target cannot use vocal abilities, spells requiring incantations, or call for reinforcements

**Rank 3** (Capstone trained):

- Bonus damage increases to 4d8
- [Silenced] duration increases to 2 turns
- If target had [Feared] when hit: also apply [Bleeding] (2d6/turn, 3 turns)

### Damage Formula

```
Total Damage = Weapon Damage + Bonus Dice + FINESSE Modifier

Rank 2: Weapon + 2d8 + FINESSE
Rank 3: Weapon + 4d8 + FINESSE

Example (Rank 3, Dagger 2d6+2, FINESSE +3):
  Damage = (2d6+2) + 4d8 + 3
  Average = 9 + 18 + 3 = 30 damage

```

### GUI Display Requirements

**Ability Button**:

- Show cooldown timer when on cooldown
- Highlight when in flanking position or [Hidden]

**Combat Log**:

```
[THROAT-CUTTER]
{Character} silences {Target}!
Damage: {X}
[Silenced] applied for {Y} turns
[Bleeding applied - target was Feared]

```

---

### Ability 24014: Sensory Scramble

**Type**: Active
**Action**: Standard Action
**Target**: Target Row
**Resource Cost**: 30 Stamina + 1 Alchemical Component
**Cooldown**: 4 turns (Per Combat)

**Description**:

> You shatter a dart of Blighted reagents, releasing powder that overloads senses with corrupted data. A temporary zone of pure psychic noise.
> 

### Rank Progression

| Rank | Duration | Enemy Penalty | Your Bonus | Special |
| --- | --- | --- | --- | --- |
| **Rank 2** | 2 turns | -1d10 Perception | +2d10 Enter the Void | - |
| **Rank 3** | 4 turns | -1d10 + 1d6 Stress/turn | +2d10 Enter the Void | Move without stealth check |

### Detailed Mechanics

**Rank 2** (When trained):

- Create [Psychic Resonance] zone in target row for 2 turns
- Enemies in zone: -1d10 to Perception checks
- You: +2d10 to Enter the Void checks while in zone
- Requires 1 Alchemical Component (consumed)

**Rank 3** (Capstone trained):

- Duration increases to 4 turns
- Zone inflicts 1d6 Psychic Stress/turn to enemies within
- You can move through zone without requiring new stealth check
- All Rank 2 effects apply

### Zone Mechanics

```
[Psychic Resonance] Zone Effects:
- Radius: Entire target row
- Duration: 2/4 turns by rank
- Enemy debuff: -1d10 Perception
- Ally buff (Myrk-gengr only): +2d10 Stealth
- Synergy: One with the Static provides additional +2d10

```

### GUI Display Requirements

**Zone Indicator**:

- Purple/static visual effect on affected row
- Duration counter visible
- Tooltip showing all active effects

**Targeting Interface**:

```
┌────────────────────────────────────────┐
│ 💨 SENSORY SCRAMBLE           [RANK] │
│ ────────────────────────────────────── │
│ Cost: 30 Stamina + 1 Alchemical Comp   │
│ Target: Select Row                     │
│ Duration: {X} turns                    │
│ ────────────────────────────────────── │
│ Creates [Psychic Resonance] zone:      │
│ • Enemies: -1d10 Perception           │
│ • You: +2d10 to Enter the Void        │
│ [Rank 3: Enemies take 1d6 Stress/turn] │
└────────────────────────────────────────┘

```

---

### Ability 24015: Mind of Stillness

**Type**: Passive
**Action**: Free Action
**Target**: Self
**Trigger**: While in [Hidden] state

**Description**:

> To manipulate the static, your mind must be a fortress of perfect calm. You meditate within chaos, becoming the quiet center of the psychic storm.
> 

### Rank Progression

| Rank | Stress Removed/Turn | Stamina Regen/Turn | Special |
| --- | --- | --- | --- |
| **Rank 2** | 3 | 5 | - |
| **Rank 3** | 7 | 10 | +1d10 Resolve while Hidden |

### Detailed Mechanics

**Rank 2** (When trained):

- While in [Hidden] state, at start of your turn:
    - Remove 3 Psychic Stress
    - Regenerate 5 Stamina
- Does nothing if not Hidden

**Rank 3** (Capstone trained):

- Stress removal increases to 7/turn
- Stamina regeneration increases to 10/turn
- Gain +1d10 to all Resolve checks while Hidden
- Encourages tactical hiding to recover between strikes

### Implementation Logic

```csharp
// Called at start of turn
public void ApplyMindOfStillness(PlayerCharacter shadowWalker, int abilityRank)
{
    if (!HasStatusEffect(shadowWalker, "Hidden"))
        return;

    int stressReduction = abilityRank switch
    {
        3 => 7,
        2 => 5,
        _ => 3  // Rank 1 not applicable for Tier 2
    };

    int staminaRegen = abilityRank switch
    {
        3 => 10,
        2 => 8,
        _ => 5
    };

    shadowWalker.PsychicStress = Math.Max(0, shadowWalker.PsychicStress - stressReduction);
    shadowWalker.Stamina = Math.Min(shadowWalker.MaxStamina, shadowWalker.Stamina + staminaRegen);
}

```

### GUI Display Requirements

**Status Effect Indicator** (when Hidden):

```
┌─────────────────────────────────┐
│ 🧘 MIND OF STILLNESS     Active │
│ ─────────────────────────────── │
│ Per Turn (while Hidden):        │
│ • Stress: -{X}                  │
│ • Stamina: +{Y}                 │
│ [Rank 3: +1d10 Resolve]         │
└─────────────────────────────────┘

```

**Turn Start Notification**:
"Mind of Stillness: -{X} Stress, +{Y} Stamina"

---

## Tier 3 Abilities (No Ranks)

*Tier 3 abilities do not have ranks. They have a single power level that does not change.*

### Ability 24016: Terror from the Void

**Type**: Passive
**Action**: Free Action (triggered)
**Target**: Self (affects first Shadow Strike target)
**Trigger**: First Shadow Strike per combat

**Description**:

> You have mastered the art of the psychological alpha strike. The sheer shock and terror of your initial assault shatters minds.
> 

### Effects

| Effect | Value |
| --- | --- |
| **Psychic Stress Inflicted** | 15 |
| **[Feared] Chance** | 85% |
| **[Feared] Duration** | 3 turns |
| **Witness Fear** | No (Rank 3 only - not applicable) |

### Detailed Mechanics

- Triggers automatically on your first Shadow Strike each combat
- Target suffers 15 Psychic Stress (in addition to Shadow Strike damage)
- 85% chance to apply [Feared] for 3 turns
- [Feared]: Target suffers -2d10 to all checks, cannot willingly approach you
- Combat flag tracks "first strike used" to prevent multiple triggers

### Integration

```
First Shadow Strike Flow:
1. Shadow Strike executes (guaranteed crit)
2. Terror from the Void triggers:
   - +15 Psychic Stress to target
   - 85% chance [Feared] for 3 turns
3. Ghostly Form persistence check (if applicable)
4. Stealth state resolved

```

### GUI Display Requirements

**First Strike Indicator**:

- Visual "READY" indicator when Terror from the Void hasn't triggered yet
- Changes to "EXPENDED" after first Shadow Strike

**Combat Log**:

```
[TERROR FROM THE VOID]
{Target} is struck by existential dread!
Psychic Stress: +15
[Feared] applied for 3 turns!

```

---

### Ability 24017: Ghostly Form

**Type**: Passive
**Action**: Free Action
**Target**: Self
**Trigger**: While [Hidden] and after Shadow Strike

**Description**:

> Your connection to the world's static becomes so profound you are no longer just hiding—you are a part of it. Your form flickers and desynchronizes.
> 

### Effects

| Effect | Value |
| --- | --- |
| **Defense Bonus (while Hidden)** | +3d10 |
| **Stealth Persistence Chance** | 65% |
| **On Persistence** | Continue as normal |

### Detailed Mechanics

**While [Hidden]**:

- Gain +3d10 Defense bonus to all incoming attacks
- This makes the Shadow-Walker difficult to hit even if detected

**After Shadow Strike**:

- 65% chance to remain [Hidden] instead of breaking stealth
- Roll d100; if ≤ 65, maintain [Hidden] state
- If stealth maintained, can continue operating from stealth

### Defense Bonus Calculation

```
Defense Pool (while Hidden) = Base Defense + Ghostly Form Bonus

Example (FINESSE 4, Hidden):
  Defense = 4d10 (FINESSE) + 3d10 (Ghostly Form) = 7d10

Note: Defense bonus ONLY applies while Hidden

```

### Stealth Persistence Flow

```
After Shadow Strike:
1. Check if Ghostly Form is trained
2. If trained, roll d100
3. If roll ≤ 65: Remain [Hidden]
4. If roll > 65: [Hidden] state ends
5. Continue combat

```

### GUI Display Requirements

**Persistence Roll Display**:

```
┌────────────────────────────────────────┐
│ 👻 GHOSTLY FORM - PERSISTENCE CHECK   │
│ ────────────────────────────────────── │
│ Roll: [X] vs 65%                       │
│                                        │
│ [SUCCESS - Stealth Maintained!]        │
│ or                                     │
│ [FAILURE - Stealth Broken]             │
└────────────────────────────────────────┘

```

---

## Capstone Ability (No Ranks)

### Ability 24018: Living Glitch

**Type**: Active
**Action**: Standard Action
**Target**: Single Enemy (Melee)
**Resource Cost**: 60 Stamina + 75 Focus
**Self-Corruption**: +18
**Cooldown**: Once per combat
**Requirement**: Must be in [Hidden] state
**Prerequisites**: Terror from the Void (24016) OR Ghostly Form (24017)

**Description**:

> For a single, horrifying moment, you do not hide in a glitch—you become one. You de-compile your own physical presence, stepping outside the world's logical grid to deliver a blow that is a fundamental violation of causality.
> 

### Effects

| Effect | Value |
| --- | --- |
| **Damage** | 10d10 + Weapon + FINESSE × 2 |
| **Hit Type** | Guaranteed (cannot be parried, dodged, or blocked) |
| **Psychic Stress to Target** | 30 |
| **Self-Corruption** | +15 |
| **Stealth** | Breaks (unless kills target at Rank 3*) |

*Note: Rank 3 mechanic exists in code but as Capstone has no ranks, this represents the base power level*

### Detailed Mechanics

**Requirements**:

- Must be in [Hidden] state
- Must have 60+ Stamina
- Must have 75+ Focus
- Once per combat limitation

**Attack Resolution**:

- Guaranteed hit - cannot be parried, dodged, or blocked
- Ignores all defensive abilities and reactions
- Deals 10d10 + Weapon Damage + (FINESSE modifier × 2)

**Psychic Effects**:

- Target suffers 30 Psychic Stress
- This is catastrophic - can trigger Breaking Points
- Separate from Terror from the Void (stacks if both apply)

**Self-Corruption**:

- You gain 15 Corruption (forcing your own reality-glitch)
- This is the cost of violating causality
- High Corruption can lead to negative effects over time

**Stealth Resolution**:

- Normally breaks [Hidden] state after use
- If the attack kills the target, you may maintain stealth

### Damage Formula

```
Total Damage = 10d10 + Weapon Damage + (FINESSE Modifier × 2)

Example (Dagger 2d6+2, FINESSE +4):
  Damage = 10d10 + (2d6+2) + (4 × 2)
  Average = 55 + 9 + 8 = 72 damage

Note: This is single-target burst damage, not AoE

```

### GUI Display Requirements

**Ability Card**:

```
┌────────────────────────────────────────────┐
│ ⚡ LIVING GLITCH                 [CAPSTONE] │
│ ────────────────────────────────────────── │
│ Cost: 60 Stamina + 75 Focus                │
│ Self-Corruption: +15                       │
│ ────────────────────────────────────────── │
│ GUARANTEED HIT (Unblockable)               │
│ Damage: 10d10 + Weapon + FINESSE × 2       │
│ Target Stress: +30                         │
│ ────────────────────────────────────────── │
│ ⚠️ Requires [Hidden]                       │
│ ⚠️ Once per combat                         │
│ ⚠️ +15 Corruption to self                  │
└────────────────────────────────────────────┘

```

**Execution Animation** (suggested):

1. Screen briefly distorts/glitches
2. Character silhouette flickers
3. Massive damage number appears
4. Static/corruption visual on character

**Combat Log**:

```
═══════════════════════════════════════════
        ⚡ LIVING GLITCH ⚡
═══════════════════════════════════════════
{Character} becomes a violation of causality!
GUARANTEED HIT - UNBLOCKABLE
Damage: {X} ({breakdown})
{Target} suffers 30 Psychic Stress!

⚠️ {Character} gains 15 Corruption
[Stealth Broken / Target Eliminated - Stealth Maintained]
═══════════════════════════════════════════

```

---

## Status Effect Definitions

### [Hidden]

| Property | Value |
| --- | --- |
| **Category** | Buff |
| **Duration** | Until broken |
| **Effect** | Enemies cannot target character directly |
| **Breaks When** | Character attacks (unless Ghostly Form procs) or fails stealth check |

**Visual Indicator**: Character silhouette becomes translucent/static-effect

### [Feared]

| Property | Value |
| --- | --- |
| **Category** | Control Debuff |
| **Duration** | 2-3 turns (varies by source) |
| **Effect** | -2d10 to all checks, cannot willingly approach fear source |

**Visual Indicator**: Red trembling effect, terror icon

### [Silenced]

| Property | Value |
| --- | --- |
| **Category** | Control Debuff |
| **Duration** | 1-2 turns (varies by rank) |
| **Effect** | Cannot use vocal abilities, spells, or call for help |

**Visual Indicator**: Crossed-out mouth icon, muted audio cue

### [Bleeding]

| Property | Value |
| --- | --- |
| **Category** | Damage Over Time |
| **Duration** | 2-3 turns |
| **Damage** | 2d6 physical damage per turn |

**Visual Indicator**: Red droplet particles, blood trail

### [Psychic Resonance]

| Property | Value |
| --- | --- |
| **Category** | Zone Effect |
| **Duration** | 2-4 turns |
| **Enemy Effect** | -1d10 Perception |
| **Myrk-gengr Effect** | +2d10 to Enter the Void, +2d10 from One with Static |

**Visual Indicator**: Purple static/noise overlay on affected area

---

## Combat Integration

### Turn Flow Integration

```
START OF TURN:
1. Process Mind of Stillness (if Hidden)
   - Remove Psychic Stress
   - Regenerate Stamina
   - Apply Resolve bonus (Rank 3)

MAIN ACTION:
2. Select action:
   a. Enter the Void (enter stealth)
   b. Shadow Strike (attack from stealth)
   c. Throat-Cutter (melee + silence)
   d. Sensory Scramble (create zone)
   e. Living Glitch (capstone assassination)
   f. Standard attack/ability

TRIGGERED EFFECTS:
3. If Shadow Strike used:
   a. Check Terror from the Void (first strike only)
   b. Apply damage (guaranteed crit)
   c. Apply status effects (Bleeding at Rank 3)
   d. Check Ghostly Form persistence
   e. Resolve stealth state

END OF TURN:
4. Process zone durations
5. Process status effect durations

```

### Reaction System

The Myrk-gengr has no explicit reaction abilities, but the following passive triggers occur:

| Trigger | Ability | Response |
| --- | --- | --- |
| Character in [Hidden] | Ghostly Form | +3d10 Defense |
| First Shadow Strike | Terror from the Void | +15 Stress, 85% [Feared] |
| Shadow Strike resolves | Ghostly Form | 65% stay Hidden |
| Turn starts while Hidden | Mind of Stillness | Regen Stamina, remove Stress |

---

## Implementation Status

### Current Implementation (v0.24.2)

| Component | Status | Location |
| --- | --- | --- |
| **MyrkgengrService** | ✅ Implemented | `RuneAndRust.Engine/MyrkgengrService.cs` |
| **MyrkgengrSeeder** | ✅ Implemented | `RuneAndRust.Persistence/MyrkgengrSeeder.cs` |
| **Unit Tests** | ✅ Implemented | `RuneAndRust.Tests/MyrkgengrSpecializationTests.cs` |
| **SpecializationData** | ✅ Seeded | ID: 24002 |
| **AbilityData** | ✅ Seeded | IDs: 24010-24018 |

### Service Methods

| Method | Description | Status |
| --- | --- | --- |
| `GetStealthBonus()` | Calculate One with Static bonus | ✅ |
| `EnterTheVoid()` | Execute stealth entry | ✅ |
| `ExecuteShadowStrike()` | Execute assassination attack | ✅ |
| `ExecuteThroatCutter()` | Execute silencing attack | ✅ |
| `ApplyMindOfStillness()` | Apply passive regen | ✅ |
| `GetGhostlyFormDefenseBonus()` | Calculate defense bonus | ✅ |
| `ExecuteLivingGlitch()` | Execute capstone ability | ✅ |

### GUI Integration Gaps

| Feature | Status | Priority |
| --- | --- | --- |
| [Hidden] state indicator | ❌ Not implemented | High |
| Stealth check dialog | ❌ Not implemented | High |
| Psychic Resonance zone display | ❌ Not implemented | Medium |
| Terror from the Void ready indicator | ❌ Not implemented | Medium |
| Ghostly Form persistence roll display | ❌ Not implemented | Medium |
| Living Glitch execution animation | ❌ Not implemented | Low |
| Focus resource display | ❌ Not implemented | High |
| Rank visual indicators | ❌ Not implemented | Medium |

### Data Corrections Needed

| Issue | Current | Should Be | Location |
| --- | --- | --- | --- |
| CostToRank2 | 20 | N/A (tree-based) | AbilityData records |
| MaxRank for Tier 2 | 3 | Should start at 2 | MyrkgengrSeeder.cs |
| Focus resource | Not implemented | Needs implementation | Resource system |

---

## Implementation Priority Roadmap

### Phase 1: Core Stealth System (High Priority)

1. Implement [Hidden] state visual indicator
2. Implement stealth check dialog for Enter the Void
3. Implement Focus resource system
4. Integrate MyrkgengrService with CombatEngine

### Phase 2: Combat Effects (High Priority)

1. Implement Ghostly Form defense bonus in combat
2. Implement Terror from the Void trigger system
3. Implement Ghostly Form persistence check display
4. Implement [Silenced], [Feared], [Bleeding] status effect displays

### Phase 3: Zone System (Medium Priority)

1. Implement [Psychic Resonance] zone visual display
2. Implement zone effect processing (damage, debuffs)
3. Implement zone duration tracking

### Phase 4: Polish (Low Priority)

1. Implement Living Glitch execution animation
2. Implement rank visual indicators (Bronze/Silver/Gold)
3. Implement Terror from the Void ready indicator
4. Add atmospheric audio cues for stealth abilities

---

## Appendix A: Ability ID Reference

| Ability ID | Name | Tier |
| --- | --- | --- |
| 24010 | One with the Static I | 1 |
| 24011 | Enter the Void | 1 |
| 24012 | Shadow Strike | 1 |
| 24013 | Throat-Cutter | 2 |
| 24014 | Sensory Scramble | 2 |
| 24015 | Mind of Stillness | 2 |
| 24016 | Terror from the Void | 3 |
| 24017 | Ghostly Form | 3 |
| 24018 | Living Glitch | 4 (Capstone) |

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

## Appendix C: Test Coverage

### Specialization Tests

- ✅ Myrk-gengr is seeded with correct properties
- ✅ Myrk-gengr appears in Skirmisher specializations
- ✅ Myrk-gengr requires minimum Legend 5
- ✅ Myrk-gengr (Heretical) accepts any Corruption level

### Ability Structure Tests

- ✅ Myrk-gengr has 9 abilities
- ✅ Correct tier distribution (3/3/2/1)
- ✅ Correct PP costs (3/4/5/6 by tier)
- ✅ Total PP cost is 37 (33 in code, verify)

### Tier 1 Tests

- ✅ One with the Static provides correct stealth bonuses
- ✅ Enter the Void costs correct Stamina per rank
- ✅ Enter the Void fails with insufficient Stamina
- ✅ Shadow Strike requires [Hidden] state
- ✅ Shadow Strike deals doubled damage (critical)
- ✅ Shadow Strike Rank 2 refunds Stamina on kill
- ✅ Shadow Strike Rank 3 applies [Bleeding]

### Tier 2 Tests

- ✅ Throat-Cutter deals base damage
- ✅ Throat-Cutter applies [Silenced] from flank/Hidden
- ✅ Throat-Cutter Rank 3 applies [Bleeding] to [Feared] target
- ✅ Mind of Stillness regenerates while Hidden
- ✅ Mind of Stillness does nothing when not Hidden

### Tier 3 Tests

- ✅ Terror from the Void inflicts Psychic Stress
- ✅ Ghostly Form provides defense bonus while Hidden

### Capstone Tests

- ✅ Living Glitch requires [Hidden] state
- ✅ Living Glitch deals massive damage
- ✅ Living Glitch applies self-Corruption
- ✅ Living Glitch maintains stealth on kill (Rank 3)
- ✅ Living Glitch fails with insufficient Stamina

---

**End of Specification**