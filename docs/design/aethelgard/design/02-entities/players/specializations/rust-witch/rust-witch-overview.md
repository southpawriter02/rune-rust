---
id: SPEC-RUST-WITCH-25001
title: "Rust-Witch"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Rust-Witch

## Identity

| Property | Value |
|----------|-------|
| **Name** | Rust-Witch |
| **Archetype** | Mystic |
| **Path Type** | Heretical |
| **Role** | Debuffer / DoT Specialist |
| **Primary Attribute** | WILL |
| **Secondary Attribute** | WITS |
| **Resource** | Aether Pool (AP) |
| **Trauma Risk** | Very High (self-Corruption) |

---

## Unlock Requirements

| Requirement | Value |
|-------------|-------|
| **PP Cost** | 3 PP |
| **Minimum Legend** | 3 |
| **Maximum Corruption** | 100 |
| **Required Quest** | None |

---

## Design Philosophy

**Tagline:** "Corrosion & Entropy Magic"

**Core Fantasy:** You have learned to embrace the world's decay as a weapon. Where others see corruption as a threat, you see it as a tool—a fundamental force that can be directed against your enemies. Your curses rot metal, corrode flesh, and accelerate entropy itself.

**WARNING:** All active abilities inflict self-Corruption. Using this specialization is a bargain with decay—you trade your own purity for devastating power over your enemies.

**Mechanical Identity:**
1. **[Corroded] Stacking** - Core damage-over-time mechanic that stacks up to 5 times
2. **Self-Corruption** - ALL active abilities inflict Corruption on the user
3. **Entropy Synergies** - Multiple abilities amplify [Corroded] effectiveness
4. **Execution Threshold** - Capstone can instantly kill targets with high Corruption/stacks
5. **Cascade Spreading** - Death of corroded enemies spreads the effect

**Gameplay Feel:** High-risk entropy mage who trades personal Corruption for devastating debuffs and DoT damage. Masters of the [Corroded] status effect with execution potential.

---

## Core Mechanics

### Self-Corruption System

ALL active Rust-Witch abilities inflict self-Corruption:

| Ability | Base Cost | Rank 3 Cost |
|---------|-----------|-------------|
| Corrosive Curse | +2 | +1 |
| System Shock | +3 | +2 |
| Flash Rust | +4 | +3 |
| Unmaking Word | +4 | +4 |
| Entropic Cascade | +6 | +6 |

### [Corroded] Status Effect

| Property | Value |
|----------|-------|
| **Max Stacks** | 5 |
| **Duration** | Permanent (requires cleanse) |
| **Base Damage** | 1d4/stack/turn |
| **With Accelerated Entropy** | 2d6/stack/turn |
| **Armor Penalty** | -1 per stack |
| **Timing** | End of turn |

---

## Rank Progression

### Tier Unlock Requirements

| Tier | PP Cost | Starting Rank | Max Rank | Progression |
|------|---------|---------------|----------|-------------|
| Tier 1 | 3 PP each | Rank 1 | Rank 3 | 1→2 (2× Tier 2), 2→3 (Capstone) |
| Tier 2 | 4 PP each | Rank 2 | Rank 3 | 2→3 (Capstone) |
| Tier 3 | 5 PP each | — | — | No ranks |
| Capstone | 6 PP | — | — | No ranks (triggers Rank 3) |

### Total Investment: 40 PP (3 unlock + 37 abilities)

---

## Ability Tree

```
                    TIER 1: FOUNDATION
    ┌─────────────────────┼─────────────────────┐
    │                     │                     │
[Philosopher        [Corrosive Curse]    [Entropic Field]
 of Dust]              (Active)            (Passive)
 (Passive)
    │                     │                     │
    └─────────────────────┴─────────────────────┘
                          │
                          ▼
                TIER 2: ADVANCED
    ┌─────────────────────┼─────────────────────┐
    │                     │                     │
[System Shock]      [Flash Rust]      [Accelerated Entropy]
   (Active)           (Active)            (Passive)
    │                     │                     │
    └─────────────────────┴─────────────────────┘
                          │
                          ▼
                TIER 3: MASTERY
          ┌───────────────┴───────────────┐
          │                               │
   [Unmaking Word]              [Cascade Reaction]
      (Active)                     (Passive)
          │                               │
          └───────────────┬───────────────┘
                          │
                          ▼
              TIER 4: CAPSTONE
                          │
               [Entropic Cascade]
                    (Active)
```

### Ability Index

| ID | Name | Tier | Type | PP | Key Effect |
|----|------|------|------|-----|------------|
| 25001 | Philosopher of Dust | 1 | Passive | 3 | +dice to analysis vs corrupted |
| 25002 | Corrosive Curse | 1 | Active | 3 | Apply [Corroded] stacks |
| 25003 | Entropic Field | 1 | Passive | 3 | Aura: enemies -Armor |
| 25004 | System Shock | 2 | Active | 4 | [Corroded] + [Stunned] on Mechanical |
| 25005 | Flash Rust | 2 | Active | 4 | AoE [Corroded] to all enemies |
| 25006 | Accelerated Entropy | 2 | Passive | 4 | [Corroded] deals 2d6/stack |
| 25007 | Unmaking Word | 3 | Active | 5 | Double [Corroded] stacks |
| 25008 | Cascade Reaction | 3 | Passive | 5 | Spread [Corroded] on death |
| 25009 | Entropic Cascade | 4 | Active | 6 | Execute OR 6d6 Arcane damage |

---

## Situational Power Profile

### Optimal Conditions
- Fighting multiple enemies (Flash Rust + Cascade Reaction)
- Enemies with Mechanical tag (System Shock guaranteed stun)
- Extended fights allowing DoT to accumulate
- Paired with Bleeding-focused allies (1.5× Bleeding damage)
- Target-rich environments for cascade chains

### Weakness Conditions
- Short fights (DoT can't accumulate)
- Single tough enemies without Corruption
- Fights requiring sustained output (self-Corruption limits)
- Cleanse-heavy enemy compositions
- High personal Corruption already

---

## Party Synergies

### Positive Synergies
- **Berserkr** - Bleeding + Corroded = 1.5× Bleeding damage
- **Bone-Setter** - Can manage Rust-Witch's Corruption
- **Skjaldmær** - Protects fragile Rust-Witch
- **Skald** - Stress management for high-trauma gameplay

### Negative Synergies
- **Other Heretical paths** - Competing Corruption sources
- **Parties lacking healing** - Self-Corruption unsustainable
- **Speed-focused compositions** - DoT needs time to work

---

## Balance Data

### Power Curve

| Legend | Effectiveness | Notes |
|--------|---------------|-------|
| 3-5 | Medium | Learning [Corroded] management |
| 6-10 | High | Flash Rust + Cascade chains |
| 11-15 | Very High | Execution threshold mastery |
| 16+ | Extreme | Full entropy control |

### Role Effectiveness

| Role | Rating | Notes |
|------|--------|-------|
| Single Target | ★★★☆☆ | Slow buildup, strong finish |
| AoE Damage | ★★★★★ | Flash Rust + Cascade |
| Control | ★★★★☆ | Stuns, armor shred |
| Support | ★☆☆☆☆ | Pure offensive |
| Survivability | ★★☆☆☆ | Self-Corruption risk |

---

## Voice Guidance

### Tone Profile
- Detached, clinical observation of decay
- Dark acceptance of entropy
- Whispered incantations
- Matter-of-fact about self-destruction

### Example Quotes
- "Everything falls apart. I merely... accelerate the process."
- "Your armor rusts. Your flesh rots. This is the natural order."
- "The Corruption in me recognizes the Corruption in you."
- "Entropy always wins. I've simply chosen my side."

---

## Phased Implementation Guide

### Phase 1: Core Framework
- [ ] Implement [Corroded] status effect with stacking
- [ ] Implement self-Corruption on ability use
- [ ] Create Aether Pool resource integration
- [ ] Basic ability damage calculations

### Phase 2: DoT System
- [ ] End-of-turn [Corroded] damage processing
- [ ] Armor reduction per stack
- [ ] Accelerated Entropy damage upgrade
- [ ] Soak bypass at Rank 3

### Phase 3: AoE and Spreading
- [ ] Flash Rust multi-target application
- [ ] Cascade Reaction death trigger
- [ ] Chain reaction processing
- [ ] Adjacency detection for spreading

### Phase 4: Execution System
- [ ] Entropic Cascade execution threshold check
- [ ] Corruption percentage tracking
- [ ] Execute vs damage decision logic
- [ ] Visual execution indicators

### Phase 5: Polish
- [ ] Self-Corruption warning UI
- [ ] [Corroded] stack display on enemies
- [ ] Entropic Field aura visualization
- [ ] Cascade spread animation

---

## Testing Requirements

### Unit Tests
- [Corroded] damage calculation per stack
- Self-Corruption application
- Execution threshold logic
- Cascade spreading mechanics

### Integration Tests
- Combat flow with DoT processing
- Multi-enemy [Corroded] tracking
- Corruption accumulation over combat
- Rank progression triggers

### Manual QA Checklist
- [ ] Self-Corruption warning displays correctly
- [ ] [Corroded] stacks visible on enemy health bars
- [ ] Cascade chains work with multiple deaths
- [ ] Execution triggers at correct thresholds
- [ ] Rank upgrades apply correctly

---

## Logging Requirements

### Combat Events
```
"Corrosive Curse: Applied {stacks} [Corroded] to {target}. Self-Corruption +{amount}"
"Flash Rust: {count} enemies receive {stacks} [Corroded] each. Self-Corruption +{amount}"
"[Corroded] Tick: {target} takes {damage} damage ({stacks} stacks)"
"Cascade Reaction: {target} death spreads [Corroded] to {count} adjacent enemies"
"Entropic Cascade: EXECUTE - {target} reduced to 0 HP (threshold: {condition})"
"Entropic Cascade: {damage} Arcane damage (execution threshold not met)"
```

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Mystic Archetype](../../archetypes/mystic.md) | Parent archetype |
| [Corroded Status](../../../04-systems/status-effects/corroded.md) | Status effect details |
| [Corruption System](../../../01-core/resources/coherence.md) | Corruption mechanics |
| [Trauma Economy](../../../01-core/trauma-economy.md) | Risk/reward framework |

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-14 | Initial gold standard migration |
